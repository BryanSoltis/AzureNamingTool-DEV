using AzureNamingTool.Attributes;
using AzureNamingTool.Components;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Implementation.FileSystem;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using BlazorDownloadFile;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

// Register Storage Provider
builder.Services.AddSingleton<IStorageProvider, FileSystemStorageProvider>();

// Register Repositories (Scoped for per-request lifetime)
builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(JsonFileConfigurationRepository<>));
builder.Services.AddScoped<IConfigurationRepository<AdminLogMessage>, JsonFileConfigurationRepository<AdminLogMessage>>();
builder.Services.AddScoped<IConfigurationRepository<ResourceDelimiter>, JsonFileConfigurationRepository<ResourceDelimiter>>();

// Register Cache Service (Singleton since it wraps IMemoryCache)
builder.Services.AddSingleton<ICacheService, CacheService>();

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

var app = builder.Build();

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