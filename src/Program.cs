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
using Microsoft.Extensions.Diagnostics.HealthChecks;
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

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureNamingTool.HealthChecks.StorageHealthCheck>(
        "storage",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "storage" })
    .AddCheck<AzureNamingTool.HealthChecks.CacheHealthCheck>(
        "cache",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "cache" });

// Always register DbContext (needed by StorageMigrationService even when using FileSystem)
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, storageOptions.DatabasePath);
var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseSqlite(connectionString));

// Always register Migration Service (needed by MainLayout and Admin page)
builder.Services.AddScoped<IStorageMigrationService, StorageMigrationService>();

// Configure Storage Provider based on SiteConfiguration.StorageProvider setting
// This allows users to switch between FileSystem and SQLite via the Admin page
var siteConfig = ConfigurationHelper.GetConfigurationData();
var provider = siteConfig.StorageProvider?.ToLower() ?? "filesystem";

if (provider == "sqlite")
{
    // SQLite Configuration
    
    // Register SQLite Storage Provider
    builder.Services.AddSingleton<IStorageProvider>(sp =>
    {
        var dbContext = sp.GetRequiredService<ConfigurationDbContext>();
        return new SQLiteStorageProvider(dbContext, dbPath);
    });
    
    // Register SQLite Repositories
    builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(SQLiteConfigurationRepository<>));
    
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

// Note: Automatic migration has been removed in favor of user-initiated migration
// Users can migrate via the modal prompt (first run) or Admin page (ongoing management)
// The StorageProvider setting in SiteConfiguration.json determines which provider is active

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

// Map Health Check Endpoints
// Basic ping endpoint (backward compatibility)
app.MapHealthChecks("/healthcheck/ping");

// Liveness probe - checks if application is running
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Don't run any checks, just return 200 if app is running
});

// Readiness probe - checks if application is ready to serve requests
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"), // Run all checks tagged with "ready"
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();


/// <summary>
/// Exists so can be used as reference for WebApplicationFactory in tests project
/// </summary>
public partial class Program
{
}