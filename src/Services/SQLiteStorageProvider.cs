using AzureNamingTool.Data;
using AzureNamingTool.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// SQLite storage provider implementation
    /// </summary>
    public class SQLiteStorageProvider : IStorageProvider
    {
        private readonly ConfigurationDbContext _dbContext;
        private readonly string _databasePath;

        /// <summary>
        /// Initializes a new instance of the SQLiteStorageProvider
        /// </summary>
        /// <param name="dbContext">Database context for SQL operations</param>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public SQLiteStorageProvider(ConfigurationDbContext dbContext, string databasePath)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        }

        /// <summary>
        /// Gets the name of this storage provider
        /// </summary>
        public string ProviderName => "SQLite";

        /// <summary>
        /// Checks if storage is available
        /// </summary>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                return await _dbContext.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if SQLite database is accessible and returns health status
        /// </summary>
        public async Task<StorageHealthStatus> GetHealthAsync()
        {
            try
            {
                // Try to open connection and execute a simple query
                var canConnect = await _dbContext.Database.CanConnectAsync();
                var metadata = new Dictionary<string, object>();
                
                if (File.Exists(_databasePath))
                {
                    var fileInfo = new FileInfo(_databasePath);
                    metadata["DatabasePath"] = _databasePath;
                    metadata["SizeBytes"] = fileInfo.Length;
                    metadata["LastModified"] = fileInfo.LastWriteTimeUtc;
                }
                
                return new StorageHealthStatus(
                    IsHealthy: canConnect,
                    ProviderName: ProviderName,
                    Message: canConnect ? "SQLite database is accessible" : "Cannot connect to SQLite database",
                    Metadata: metadata
                );
            }
            catch (Exception ex)
            {
                return new StorageHealthStatus(
                    IsHealthy: false,
                    ProviderName: ProviderName,
                    Message: $"SQLite health check failed: {ex.Message}",
                    Metadata: null
                );
            }
        }

        /// <summary>
        /// Initializes the SQLite database (creates tables if they don't exist)
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database file directory exists
                var directory = Path.GetDirectoryName(_databasePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create or migrate database
                await _dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize SQLite database at '{_databasePath}'", ex);
            }
        }
    }
}
