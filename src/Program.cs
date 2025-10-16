using AzureNamingTool.Attributes;
using AzureNamingTool.Components;
using AzureNamingTool.Data;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories;
using AzureNamingTool.Repositories.Implementation.FileSystem;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using BlazorDownloadFile;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Bind storage configuration
var storageOptions = new StorageOptions();
builder.Configuration.GetSection("StorageOptions").Bind(storageOptions);
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("StorageOptions"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.EnableDetailedErrors = false;
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.MaximumParallelInvocationsPerClient = 1;
        options.MaximumReceiveMessageSize = 102400000;
        options.StreamBufferCapacity = 10;
    });


builder.Services.AddHealthChecks();
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CustomHeaderSwaggerAttribute>();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v" + ConfigurationHelper.GetAssemblyVersion(),
        Title = "Azure Naming Tool API",
        Description = "An ASP.NET Core Web API for managing the Azure Naming tool configuration. All API requests require the configured API Keys (found in the site Admin configuration). You can find more details in the <a href=\"https://github.com/mspnp/AzureNamingTool/wiki/Using-the-API\" target=\"_new\">Azure Naming Tool API documentation</a>."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add services to the container
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddMemoryCache();
builder.Services.AddMvcCore().AddApiExplorer();

// Register Cache Service (Singleton since it wraps IMemoryCache)
builder.Services.AddSingleton<ICacheService, CacheService>();

// Configure Storage Provider based on configuration
var provider = storageOptions.Provider?.ToLower() ?? "filesystem";

if (provider == "sqlite")
{
    // SQLite Configuration
    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, storageOptions.DatabasePath);
    var connectionString = $"Data Source={dbPath}";
    
    // Register DbContext
    builder.Services.AddDbContext<ConfigurationDbContext>(options =>
        options.UseSqlite(connectionString));
    
    // Register SQLite Storage Provider
    builder.Services.AddSingleton<IStorageProvider>(sp =>
    {
        var dbContext = sp.GetRequiredService<ConfigurationDbContext>();
        return new SQLiteStorageProvider(dbContext, dbPath);
    });
    
    // Register SQLite Repositories
    builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(SQLiteConfigurationRepository<>));
    
    // Register Migration Service
    builder.Services.AddScoped<IStorageMigrationService, StorageMigrationService>();
    
    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
    Console.WriteLine($"[Storage] Using SQLite provider: {dbPath}");
}
else
{
    // FileSystem (JSON) Configuration - DEFAULT
    builder.Services.AddSingleton<IStorageProvider, FileSystemStorageProvider>();
    
    // Register JSON File Repositories
    builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(JsonFileConfigurationRepository<>));
    builder.Services.AddScoped<IConfigurationRepository<AdminLogMessage>, JsonFileConfigurationRepository<AdminLogMessage>>();
    builder.Services.AddScoped<IConfigurationRepository<ResourceDelimiter>, JsonFileConfigurationRepository<ResourceDelimiter>>();
    
    Console.WriteLine("[Storage] Using FileSystem provider (JSON files)");
}

// Register Storage Provider
// REMOVED - now configured above based on storageOptions.Provider

// Register Repositories (Scoped for per-request lifetime)
// REMOVED - now configured above based on storageOptions.Provider

// Register Cache Service (Singleton since it wraps IMemoryCache)
// MOVED - now configured above

// Register Application Services (Scoped for per-request lifetime)
builder.Services.AddScoped<IAdminLogService, AdminLogService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ICustomComponentService, CustomComponentService>();
builder.Services.AddScoped<IGeneratedNamesService, GeneratedNamesService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IResourceComponentService, ResourceComponentService>();
builder.Services.AddScoped<IResourceDelimiterService, ResourceDelimiterService>();
builder.Services.AddScoped<IResourceEnvironmentService, ResourceEnvironmentService>();
builder.Services.AddScoped<IResourceFunctionService, ResourceFunctionService>();
builder.Services.AddScoped<IResourceLocationService, ResourceLocationService>();
builder.Services.AddScoped<IResourceNamingRequestService, ResourceNamingRequestService>();
builder.Services.AddScoped<IResourceOrgService, ResourceOrgService>();
builder.Services.AddScoped<IResourceProjAppSvcService, ResourceProjAppSvcService>();
builder.Services.AddScoped<IResourceTypeService, ResourceTypeService>();
builder.Services.AddScoped<IResourceUnitDeptService, ResourceUnitDeptService>();

// Register coordinator to break circular dependencies between ResourceComponent and ResourceType
builder.Services.AddScoped<IResourceConfigurationCoordinator, ResourceConfigurationCoordinator>();

// Register Helpers
builder.Services.AddScoped<ServicesHelper>();

var app = builder.Build();

// Perform automatic migration if configured
if (storageOptions?.Provider?.Equals("SQLite", StringComparison.OrdinalIgnoreCase) == true && 
    storageOptions.EnableAutoMigration)
{
    Console.WriteLine("Auto-migration enabled. Checking migration status...");
    
    using (var scope = app.Services.CreateScope())
    {
        var migrationService = scope.ServiceProvider.GetService<IStorageMigrationService>();
        
        if (migrationService != null)
        {
            try
            {
                var isNeeded = await migrationService.IsMigrationNeededAsync();
                
                if (isNeeded)
                {
                    Console.WriteLine("Migration needed. Starting automatic migration from JSON to SQLite...");
                    
                    var result = await migrationService.MigrateToSQLiteAsync();
                    
                    if (result.Success)
                    {
                        Console.WriteLine($"✓ Migration completed successfully in {result.Duration.TotalSeconds:F2} seconds");
                        Console.WriteLine($"  - Entities migrated: {result.EntitiesMigrated}");
                        Console.WriteLine($"  - Backup location: {result.BackupPath}");
                        
                        if (result.EntityCounts != null && result.EntityCounts.Any())
                        {
                            Console.WriteLine("  - Entity counts:");
                            foreach (var kvp in result.EntityCounts)
                            {
                                Console.WriteLine($"    • {kvp.Key}: {kvp.Value}");
                            }
                        }
                        
                        // Log to admin log
                        var adminLogService = scope.ServiceProvider.GetService<IAdminLogService>();
                        if (adminLogService != null)
                        {
                            await adminLogService.PostItemAsync(new AdminLogMessage
                            {
                                Title = "Automatic Migration Completed",
                                Message = $"Successfully migrated {result.EntitiesMigrated} entity types from JSON to SQLite in {result.Duration.TotalSeconds:F2} seconds. Backup saved to: {result.BackupPath}"
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"✗ Migration failed: {result.Message}");
                        
                        if (result.Errors != null && result.Errors.Any())
                        {
                            Console.WriteLine("  - Errors:");
                            foreach (var error in result.Errors)
                            {
                                Console.WriteLine($"    • {error}");
                            }
                        }
                        
                        // Log to admin log
                        var adminLogService = scope.ServiceProvider.GetService<IAdminLogService>();
                        if (adminLogService != null)
                        {
                            await adminLogService.PostItemAsync(new AdminLogMessage
                            {
                                Title = "Automatic Migration Failed",
                                Message = $"Migration from JSON to SQLite failed: {result.Message}. Errors: {string.Join("; ", result.Errors ?? new List<string>())}"
                            });
                        }
                        
                        Console.WriteLine("Application will continue using FileSystem storage provider as fallback.");
                    }
                }
                else
                {
                    Console.WriteLine("Migration not needed. SQLite database already populated or no JSON files found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error during migration check: {ex.Message}");
                Console.WriteLine("Application will continue using configured storage provider.");
            }
        }
    }
}

app.MapHealthChecks("/healthcheck/ping");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AzureNamingToolAPI"));

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseStatusCodePagesWithRedirects("/404");

app.MapControllers();
app.Run();


/// <summary>
/// Exists so can be used as reference for WebApplicationFactory in tests project
/// </summary>
public partial class Program
{
}