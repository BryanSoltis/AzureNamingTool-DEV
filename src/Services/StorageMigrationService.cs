using AzureNamingTool.Data;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for migrating configuration data from JSON files to SQLite database
    /// </summary>
    public class StorageMigrationService : IStorageMigrationService
    {
        private readonly ConfigurationDbContext _dbContext;
        private readonly ILogger<StorageMigrationService> _logger;
        private readonly string _settingsPath;
        private readonly string _backupBasePath;

        /// <summary>
        /// Initializes a new instance of the StorageMigrationService
        /// </summary>
        /// <param name="dbContext">Database context for SQLite operations</param>
        /// <param name="logger">Logger for migration operations</param>
        public StorageMigrationService(
            ConfigurationDbContext dbContext,
            ILogger<StorageMigrationService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            _backupBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        }

        /// <summary>
        /// Determines whether a migration from JSON to SQLite is needed
        /// </summary>
        /// <returns>True if migration is needed, false otherwise</returns>
        public async Task<bool> IsMigrationNeededAsync()
        {
            try
            {
                // Check if JSON files exist
                if (!Directory.Exists(_settingsPath))
                {
                    _logger.LogDebug("Settings directory does not exist");
                    return false;
                }

                var jsonFiles = Directory.GetFiles(_settingsPath, "*.json");
                if (jsonFiles.Length == 0)
                {
                    _logger.LogDebug("No JSON files found in settings directory");
                    return false;
                }

                // Check if SQLite database has any data
                var hasData = await _dbContext.ResourceTypes.AnyAsync() ||
                              await _dbContext.ResourceLocations.AnyAsync() ||
                              await _dbContext.ResourceEnvironments.AnyAsync();

                _logger.LogInformation("Migration check: JSON files={JsonFiles}, SQLite has data={HasData}",
                    jsonFiles.Length, hasData);

                // Migration needed if JSON files exist and SQLite is empty
                return jsonFiles.Length > 0 && !hasData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if migration is needed");
                return false;
            }
        }

        /// <summary>
        /// Creates a timestamped backup of current JSON configuration files
        /// </summary>
        /// <returns>Path to the backup directory</returns>
        public Task<string> BackupCurrentDataAsync()
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(_backupBasePath, $"backup_{timestamp}");

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                _logger.LogInformation("Creating backup at {BackupPath}", backupPath);

                // Copy all JSON files
                if (Directory.Exists(_settingsPath))
                {
                    var jsonFiles = Directory.GetFiles(_settingsPath, "*.json");
                    foreach (var file in jsonFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var destFile = Path.Combine(backupPath, fileName);
                        File.Copy(file, destFile, overwrite: true);
                        _logger.LogDebug("Backed up {FileName}", fileName);
                    }

                    _logger.LogInformation("Backup completed: {FileCount} files copied to {BackupPath}",
                        jsonFiles.Length, backupPath);
                }

                return Task.FromResult(backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                throw new InvalidOperationException("Failed to create backup", ex);
            }
        }

        /// <summary>
        /// Migrates all configuration data from JSON files to SQLite
        /// </summary>
        /// <param name="backupPath">Optional backup path; if not provided, creates a new backup</param>
        /// <returns>Migration result with success status and details</returns>
        public async Task<MigrationResult> MigrateToSQLiteAsync(string? backupPath = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MigrationResult();

            try
            {
                _logger.LogInformation("Starting migration from JSON to SQLite");

                // Create backup if not provided
                if (string.IsNullOrEmpty(backupPath))
                {
                    backupPath = await BackupCurrentDataAsync();
                }
                result.BackupPath = backupPath;

                // Ensure database is created
                await _dbContext.Database.EnsureCreatedAsync();

                // Migrate each entity type
                await MigrateEntityAsync<ResourceType>("resourcetype.json", result);
                await MigrateEntityAsync<ResourceLocation>("resourcelocation.json", result);
                await MigrateEntityAsync<ResourceEnvironment>("resourceenvironment.json", result);
                await MigrateEntityAsync<ResourceOrg>("resourceorg.json", result);
                await MigrateEntityAsync<ResourceProjAppSvc>("resourceprojappsvc.json", result);
                await MigrateEntityAsync<ResourceUnitDept>("resourceunitdept.json", result);
                await MigrateEntityAsync<ResourceFunction>("resourcefunction.json", result);
                await MigrateEntityAsync<ResourceDelimiter>("resourcedelimiter.json", result);
                await MigrateEntityAsync<ResourceComponent>("resourcecomponent.json", result);
                await MigrateEntityAsync<CustomComponent>("customcomponent.json", result);
                await MigrateEntityAsync<AdminUser>("adminuser.json", result);
                await MigrateEntityAsync<AdminLogMessage>("adminlogmessage.json", result);
                await MigrateEntityAsync<GeneratedName>("generatedname.json", result);

                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Migration completed successfully. {result.EntitiesMigrated} entities migrated in {result.Duration.TotalSeconds:F2} seconds.";

                _logger.LogInformation(result.Message);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.Duration = stopwatch.Elapsed;
                result.Message = $"Migration failed: {ex.Message}";
                result.Errors.Add(ex.ToString());

                _logger.LogError(ex, "Migration failed after {Duration}ms", stopwatch.ElapsedMilliseconds);

                // Attempt rollback
                if (!string.IsNullOrEmpty(backupPath))
                {
                    _logger.LogWarning("Attempting automatic rollback from {BackupPath}", backupPath);
                    await RollbackMigrationAsync(backupPath);
                }

                return result;
            }
        }

        private async Task MigrateEntityAsync<TEntity>(string fileName, MigrationResult result) where TEntity : class
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogDebug("File {FileName} does not exist, skipping", fileName);
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json) || json == "[]")
                {
                    _logger.LogDebug("File {FileName} is empty, skipping", fileName);
                    result.EntityCounts[typeof(TEntity).Name] = 0;
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var entities = JsonSerializer.Deserialize<List<TEntity>>(json, options);
                if (entities == null || entities.Count == 0)
                {
                    _logger.LogDebug("No entities found in {FileName}", fileName);
                    result.EntityCounts[typeof(TEntity).Name] = 0;
                    return;
                }

                // Add entities to database
                var dbSet = _dbContext.Set<TEntity>();
                await dbSet.AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();

                result.EntitiesMigrated += entities.Count;
                result.EntityCounts[typeof(TEntity).Name] = entities.Count;

                _logger.LogInformation("Migrated {Count} {EntityType} entities from {FileName}",
                    entities.Count, typeof(TEntity).Name, fileName);
            }
            catch (Exception ex)
            {
                var error = $"Failed to migrate {typeof(TEntity).Name} from {fileName}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Error migrating {EntityType}", typeof(TEntity).Name);
                throw new InvalidOperationException(error, ex);
            }
        }

        /// <summary>
        /// Validates that migrated data matches the source JSON files
        /// </summary>
        /// <returns>Validation result with entity count comparisons</returns>
        public async Task<ValidationResult> ValidateMigrationAsync()
        {
            var validation = new ValidationResult { IsValid = true };

            try
            {
                _logger.LogInformation("Validating migration");

                // Validate each entity type
                await ValidateEntityAsync<ResourceType>("resourcetype.json", validation);
                await ValidateEntityAsync<ResourceLocation>("resourcelocation.json", validation);
                await ValidateEntityAsync<ResourceEnvironment>("resourceenvironment.json", validation);
                await ValidateEntityAsync<ResourceOrg>("resourceorg.json", validation);
                await ValidateEntityAsync<ResourceProjAppSvc>("resourceprojappsvc.json", validation);
                await ValidateEntityAsync<ResourceUnitDept>("resourceunitdept.json", validation);
                await ValidateEntityAsync<ResourceFunction>("resourcefunction.json", validation);
                await ValidateEntityAsync<ResourceDelimiter>("resourcedelimiter.json", validation);
                await ValidateEntityAsync<ResourceComponent>("resourcecomponent.json", validation);
                await ValidateEntityAsync<CustomComponent>("customcomponent.json", validation);
                await ValidateEntityAsync<AdminUser>("adminuser.json", validation);
                await ValidateEntityAsync<AdminLogMessage>("adminlogmessage.json", validation);
                await ValidateEntityAsync<GeneratedName>("generatedname.json", validation);

                validation.Message = validation.IsValid
                    ? "Validation successful - all entities match"
                    : "Validation failed - discrepancies found";

                _logger.LogInformation(validation.Message);
                return validation;
            }
            catch (Exception ex)
            {
                validation.IsValid = false;
                validation.Message = $"Validation error: {ex.Message}";
                _logger.LogError(ex, "Error during validation");
                return validation;
            }
        }

        private async Task ValidateEntityAsync<TEntity>(string fileName, ValidationResult validation) where TEntity : class
        {
            var detail = new ValidationDetail();
            var entityName = typeof(TEntity).Name;

            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);

                // Get source count from JSON
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    if (!string.IsNullOrWhiteSpace(json) && json != "[]")
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true
                        };
                        var entities = JsonSerializer.Deserialize<List<TEntity>>(json, options);
                        detail.SourceCount = entities?.Count ?? 0;
                    }
                }

                // Get target count from SQLite
                var dbSet = _dbContext.Set<TEntity>();
                detail.TargetCount = await dbSet.CountAsync();

                detail.Matches = detail.SourceCount == detail.TargetCount;
                
                if (!detail.Matches)
                {
                    detail.Discrepancies.Add($"Count mismatch: JSON={detail.SourceCount}, SQLite={detail.TargetCount}");
                    validation.IsValid = false;
                }

                validation.EntityValidation[entityName] = detail;

                _logger.LogDebug("Validation for {EntityType}: Source={SourceCount}, Target={TargetCount}, Matches={Matches}",
                    entityName, detail.SourceCount, detail.TargetCount, detail.Matches);
            }
            catch (Exception ex)
            {
                detail.Discrepancies.Add($"Validation error: {ex.Message}");
                validation.EntityValidation[entityName] = detail;
                validation.IsValid = false;
                _logger.LogError(ex, "Error validating {EntityType}", entityName);
            }
        }

        /// <summary>
        /// Rolls back a failed migration by deleting the SQLite database and restoring from backup
        /// </summary>
        /// <param name="backupPath">Path to the backup directory to restore from</param>
        /// <returns>True if rollback was successful, false otherwise</returns>
        public async Task<bool> RollbackMigrationAsync(string backupPath)
        {
            try
            {
                _logger.LogWarning("Rolling back migration from {BackupPath}", backupPath);

                if (!Directory.Exists(backupPath))
                {
                    _logger.LogError("Backup directory {BackupPath} does not exist", backupPath);
                    return false;
                }

                // Clear SQLite database
                await _dbContext.Database.EnsureDeletedAsync();
                _logger.LogInformation("SQLite database cleared");

                // Restore JSON files from backup
                var backupFiles = Directory.GetFiles(backupPath, "*.json");
                foreach (var file in backupFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(_settingsPath, fileName);
                    File.Copy(file, destFile, overwrite: true);
                    _logger.LogDebug("Restored {FileName}", fileName);
                }

                _logger.LogInformation("Rollback completed: {FileCount} files restored", backupFiles.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rollback");
                return false;
            }
        }

        /// <summary>
        /// Gets the current migration status including file existence and migration state
        /// </summary>
        /// <returns>Migration status information</returns>
        public async Task<MigrationStatus> GetMigrationStatusAsync()
        {
            var status = new MigrationStatus
            {
                CurrentProvider = "Unknown",
                JsonFilesExist = Directory.Exists(_settingsPath) && Directory.GetFiles(_settingsPath, "*.json").Length > 0,
                SQLiteDatabaseExists = await _dbContext.Database.CanConnectAsync()
            };

            try
            {
                if (status.SQLiteDatabaseExists)
                {
                    var hasData = await _dbContext.ResourceTypes.AnyAsync();
                    status.IsMigrated = hasData;
                    status.CurrentProvider = "SQLite";
                }
                else if (status.JsonFilesExist)
                {
                    status.CurrentProvider = "FileSystem";
                }

                _logger.LogDebug("Migration status: Provider={Provider}, Migrated={Migrated}, JSON={JsonExists}, SQLite={SQLiteExists}",
                    status.CurrentProvider, status.IsMigrated, status.JsonFilesExist, status.SQLiteDatabaseExists);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status");
                return status;
            }
        }
    }
}
