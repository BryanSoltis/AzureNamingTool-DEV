using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for validating resource names against Azure tenant using Azure Resource Graph
    /// </summary>
    public class AzureValidationService : IAzureValidationService
    {
        private readonly ILogger<AzureValidationService> _logger;
        private readonly IConfiguration _config;
        private ArmClient? _armClient;
        private TokenCredential? _credential;
        private const string SETTINGS_FILE = "azurevalidationsettings.json";
        private const string CACHE_KEY_PREFIX = "azure-validation:";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureValidationService"/> class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="config">Configuration instance</param>
        public AzureValidationService(ILogger<AzureValidationService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Validates a single resource name against Azure tenant
        /// </summary>
        public async Task<AzureValidationMetadata> ValidateNameAsync(string resourceName, string resourceType)
        {
            var metadata = new AzureValidationMetadata
            {
                ValidationPerformed = false,
                ExistsInAzure = false,
                ValidationTimestamp = DateTime.UtcNow
            };

            try
            {
                // Check if validation is enabled
                if (!await IsValidationEnabledAsync())
                {
                    return metadata;
                }

                var settings = await GetSettingsAsync();

                // Check cache first if enabled
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{resourceType}:{resourceName}";
                    var cachedResult = CacheHelper.GetCacheObject(cacheKey);
                    if (cachedResult != null && cachedResult is AzureValidationMetadata cachedMetadata)
                    {
                        _logger.LogInformation("Azure validation cache hit for {ResourceName}", resourceName);
                        return cachedMetadata;
                    }
                }

                // Ensure authenticated
                await EnsureAuthenticatedAsync(settings);

                // Query Azure Resource Graph
                var exists = await CheckResourceExistsAsync(resourceName, resourceType, settings);

                metadata.ValidationPerformed = true;
                metadata.ExistsInAzure = exists.Exists;
                metadata.ConflictingResources = exists.ResourceIds;

                // Cache the result
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{resourceType}:{resourceName}";
                    CacheHelper.SetCacheObject(cacheKey, metadata);
                }

                _logger.LogInformation("Azure validation completed for {ResourceName}: exists={Exists}", 
                    resourceName, exists.Exists);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating resource name {ResourceName} against Azure", resourceName);
                metadata.ValidationWarning = $"Validation error: {ex.Message}";
                return metadata;
            }
        }

        /// <summary>
        /// Validates multiple resource names in a batch
        /// </summary>
        public async Task<Dictionary<string, AzureValidationMetadata>> ValidateBatchAsync(
            List<(string resourceName, string resourceType)> validationRequests)
        {
            var results = new Dictionary<string, AzureValidationMetadata>();

            if (!await IsValidationEnabledAsync())
            {
                foreach (var request in validationRequests)
                {
                    results[request.resourceName] = new AzureValidationMetadata
                    {
                        ValidationPerformed = false,
                        ValidationTimestamp = DateTime.UtcNow
                    };
                }
                return results;
            }

            var settings = await GetSettingsAsync();

            // Check cache for all requests
            var uncachedRequests = new List<(string resourceName, string resourceType)>();
            
            foreach (var request in validationRequests)
            {
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{request.resourceType}:{request.resourceName}";
                    var cachedResult = CacheHelper.GetCacheObject(cacheKey);
                    if (cachedResult != null && cachedResult is AzureValidationMetadata cachedMetadata)
                    {
                        results[request.resourceName] = cachedMetadata;
                        continue;
                    }
                }
                uncachedRequests.Add(request);
            }

            // Query uncached items in batch
            if (uncachedRequests.Any())
            {
                try
                {
                    await EnsureAuthenticatedAsync(settings);

                    foreach (var request in uncachedRequests)
                    {
                        var metadata = await ValidateNameAsync(request.resourceName, request.resourceType);
                        results[request.resourceName] = metadata;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch validation");
                    foreach (var request in uncachedRequests)
                    {
                        if (!results.ContainsKey(request.resourceName))
                        {
                            results[request.resourceName] = new AzureValidationMetadata
                            {
                                ValidationPerformed = false,
                                ValidationWarning = $"Batch validation error: {ex.Message}",
                                ValidationTimestamp = DateTime.UtcNow
                            };
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Tests the Azure connection
        /// </summary>
        public async Task<AzureConnectionTestResult> TestConnectionAsync()
        {
            var result = new AzureConnectionTestResult();

            try
            {
                var settings = await GetSettingsAsync();

                if (!settings.Enabled)
                {
                    result.ErrorMessage = "Azure validation is not enabled";
                    return result;
                }

                result.AuthenticationMode = settings.AuthMode.ToString();
                result.TenantId = settings.TenantId;

                // Test authentication
                await EnsureAuthenticatedAsync(settings);

                if (_armClient == null)
                {
                    result.ErrorMessage = "Failed to create ARM client";
                    return result;
                }

                result.Authenticated = true;

                // Test subscription access
                await foreach (var subscription in _armClient.GetSubscriptions().GetAllAsync())
                {
                    var subAccess = new SubscriptionAccess
                    {
                        SubscriptionId = subscription.Data.SubscriptionId ?? string.Empty,
                        DisplayName = subscription.Data.DisplayName ?? string.Empty,
                        State = subscription.Data.State?.ToString(),
                        HasReadAccess = true
                    };
                    result.AccessibleSubscriptions.Add(subAccess);

                    // Only check configured subscriptions if specified
                    if (settings.SubscriptionIds.Any() && 
                        settings.SubscriptionIds.Contains(subscription.Data.SubscriptionId ?? string.Empty))
                    {
                        // Found a configured subscription
                    }
                }

                // Test Resource Graph query
                try
                {
                    var testQuery = "Resources | where type =~ 'microsoft.resources/subscriptions' | limit 1";
                    var queryResult = await ExecuteResourceGraphQueryAsync(testQuery, settings);
                    result.ResourceGraphAccess = true;
                    result.TestQuerySucceeded = queryResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Resource Graph test query failed");
                    result.ResourceGraphAccess = false;
                    result.ErrorMessage = $"Resource Graph query failed: {ex.Message}";
                }

                result.Message = result.TestQuerySucceeded 
                    ? "Successfully connected to Azure" 
                    : "Authenticated but Resource Graph query failed";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Azure connection");
                result.ErrorMessage = ex.Message;
                result.Message = "Connection test failed";
                return result;
            }
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public async Task<AzureValidationSettings> GetSettingsAsync()
        {
            try
            {
                var settingsPath = Path.Combine("settings", SETTINGS_FILE);
                var settingsContent = await FileSystemHelper.ReadFile(SETTINGS_FILE, "settings/");
                
                if (!string.IsNullOrEmpty(settingsContent))
                {
                    var settings = JsonSerializer.Deserialize<AzureValidationSettings>(settingsContent);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Azure validation settings");
            }

            // Return default settings
            return new AzureValidationSettings();
        }

        /// <summary>
        /// Updates the settings
        /// </summary>
        public async Task<ServiceResponse> UpdateSettingsAsync(AzureValidationSettings settings)
        {
            var response = new ServiceResponse();

            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await FileSystemHelper.WriteFile(SETTINGS_FILE, json, "settings/");

                // Clear ARM client to force re-authentication with new settings
                _armClient = null;
                _credential = null;

                // Clear validation cache
                CacheHelper.InvalidateCacheObject($"{CACHE_KEY_PREFIX}*");

                response.Success = true;
                response.ResponseMessage = "Azure validation settings updated successfully";
                
                _logger.LogInformation("Azure validation settings updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Azure validation settings");
                response.Success = false;
                response.ResponseMessage = $"Error saving settings: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Checks if validation is enabled
        /// </summary>
        public async Task<bool> IsValidationEnabledAsync()
        {
            // Check global toggle first
            var globalEnabled = Convert.ToBoolean(ConfigurationHelper.GetAppSetting("AzureTenantNameValidationEnabled"));
            if (!globalEnabled)
            {
                return false;
            }

            // Check settings
            var settings = await GetSettingsAsync();
            return settings.Enabled;
        }

        #region Private Helper Methods

        /// <summary>
        /// Ensures ARM client is authenticated
        /// </summary>
        private async Task EnsureAuthenticatedAsync(AzureValidationSettings settings)
        {
            if (_armClient != null && _credential != null)
            {
                return; // Already authenticated
            }

            try
            {
                _credential = await GetCredentialAsync(settings);
                _armClient = new ArmClient(_credential);
                
                _logger.LogInformation("Azure ARM client authenticated using {AuthMode}", settings.AuthMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate to Azure");
                throw new InvalidOperationException("Failed to authenticate to Azure. Check your credentials and permissions.", ex);
            }
        }

        /// <summary>
        /// Gets the appropriate credential based on settings
        /// </summary>
        private async Task<TokenCredential> GetCredentialAsync(AzureValidationSettings settings)
        {
            switch (settings.AuthMode)
            {
                case AuthenticationMode.ManagedIdentity:
                    _logger.LogInformation("Using Managed Identity for Azure authentication");
                    return new DefaultAzureCredential();

                case AuthenticationMode.ServicePrincipal:
                    if (settings.ServicePrincipal == null)
                    {
                        throw new InvalidOperationException("Service Principal settings are required");
                    }

                    var clientSecret = await GetClientSecretAsync(settings);
                    
                    _logger.LogInformation("Using Service Principal for Azure authentication");
                    return new ClientSecretCredential(
                        settings.TenantId,
                        settings.ServicePrincipal.ClientId,
                        clientSecret);

                default:
                    throw new InvalidOperationException($"Unsupported authentication mode: {settings.AuthMode}");
            }
        }

        /// <summary>
        /// Gets the client secret from Key Vault or configuration
        /// </summary>
        private async Task<string> GetClientSecretAsync(AzureValidationSettings settings)
        {
            if (settings.ServicePrincipal == null)
            {
                throw new InvalidOperationException("Service Principal settings are required");
            }

            // Try Key Vault first if configured
            if (settings.KeyVault != null && !string.IsNullOrEmpty(settings.KeyVault.KeyVaultUri))
            {
                try
                {
                    _logger.LogInformation("Retrieving client secret from Key Vault");
                    
                    var kvCredential = new DefaultAzureCredential();
                    var kvClient = new SecretClient(new Uri(settings.KeyVault.KeyVaultUri), kvCredential);
                    
                    var secretName = settings.ServicePrincipal.ClientSecretKeyVaultName 
                        ?? settings.KeyVault.ClientSecretName;
                    
                    var secret = await kvClient.GetSecretAsync(secretName);
                    return secret.Value.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve secret from Key Vault");
                    throw new InvalidOperationException("Failed to retrieve client secret from Key Vault", ex);
                }
            }

            // Fallback to configuration (encrypted or plain)
            if (!string.IsNullOrEmpty(settings.ServicePrincipal.ClientSecret))
            {
                // Check if it's encrypted
                if (settings.ServicePrincipal.ClientSecret.StartsWith("encrypted:"))
                {
                    try
                    {
                        var encryptedValue = settings.ServicePrincipal.ClientSecret["encrypted:".Length..];
                        var config = ConfigurationHelper.GetConfigurationData();
                        return GeneralHelper.DecryptString(encryptedValue, config.SALTKey!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to decrypt client secret");
                        throw new InvalidOperationException("Failed to decrypt client secret", ex);
                    }
                }
                
                return settings.ServicePrincipal.ClientSecret;
            }

            throw new InvalidOperationException("Client secret not found in Key Vault or configuration");
        }

        /// <summary>
        /// Checks if a resource exists using Azure Resource Graph
        /// </summary>
        private async Task<(bool Exists, List<string> ResourceIds)> CheckResourceExistsAsync(
            string resourceName, 
            string resourceType, 
            AzureValidationSettings settings)
        {
            try
            {
                // Build Resource Graph query
                var query = $"Resources | where name =~ '{resourceName.Replace("'", "\\'")}' | where type =~ '{resourceType.Replace("'", "\\'")}' | project id, name, type, resourceGroup";

                var result = await ExecuteResourceGraphQueryAsync(query, settings);

                if (result != null && result.Data != null)
                {
                    var dataJson = result.Data.ToString();
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        var dataElement = JsonSerializer.Deserialize<JsonElement>(dataJson);
                        if (dataElement.ValueKind == JsonValueKind.Array)
                        {
                            var resourceIds = new List<string>();
                            
                            foreach (var item in dataElement.EnumerateArray())
                            {
                                if (item.TryGetProperty("id", out var idProp))
                                {
                                    var id = idProp.GetString();
                                    if (!string.IsNullOrEmpty(id))
                                    {
                                        resourceIds.Add(id);
                                    }
                                }
                            }

                            return (resourceIds.Any(), resourceIds);
                        }
                    }
                }

                return (false, new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Azure Resource Graph for {ResourceName}", resourceName);
                throw;
            }
        }

        /// <summary>
        /// Executes a Resource Graph query
        /// </summary>
        private async Task<ResourceQueryResult?> ExecuteResourceGraphQueryAsync(
            string query, 
            AzureValidationSettings settings)
        {
            if (_armClient == null)
            {
                throw new InvalidOperationException("ARM client is not authenticated");
            }

            try
            {
                // Get tenant resource
                string? tenantId = settings.TenantId;
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    // Get first available tenant
                    await foreach (var t in _armClient.GetTenants().GetAllAsync())
                    {
                        tenantId = t.Data.TenantId?.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new InvalidOperationException("Could not determine tenant ID");
                }

                TenantResource? tenant = null;
                await foreach (var t in _armClient.GetTenants().GetAllAsync())
                {
                    if (t.Data.TenantId?.ToString() == tenantId)
                    {
                        tenant = t;
                        break;
                    }
                }

                if (tenant == null)
                {
                    throw new InvalidOperationException($"Could not access tenant {tenantId}");
                }

                // Build query request
                var queryRequest = new ResourceQueryContent(query);

                // Add subscription scope if configured
                if (settings.SubscriptionIds.Any())
                {
                    foreach (var subId in settings.SubscriptionIds)
                    {
                        queryRequest.Subscriptions.Add(subId);
                    }
                }

                // Set query options
                queryRequest.Options = new ResourceQueryRequestOptions
                {
                    ResultFormat = ResultFormat.ObjectArray
                };

                // Execute query with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await tenant.GetResourcesAsync(queryRequest, cts.Token);

                return response.Value;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Resource Graph query timed out after 5 seconds");
                throw new TimeoutException("Azure Resource Graph query timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Resource Graph query");
                throw;
            }
        }

        #endregion
    }
}
