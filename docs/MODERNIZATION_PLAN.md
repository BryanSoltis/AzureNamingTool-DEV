# Azure Naming Tool - Modernization Plan

**Version:** 1.0  
**Date:** October 15, 2025  
**Status:** Planning  
**Estimated Duration:** 6-8 weeks

---

## 🎯 Executive Summary

This modernization plan updates the **Azure Naming Tool** (a .NET 8 Blazor application for defining and managing Azure Resource Naming Patterns) to follow modern .NET best practices while maintaining complete backward compatibility.

### Non-Negotiable Requirements

| Requirement | Status | Details |
|-------------|--------|---------|
| **API Compatibility** | ✅ Mandatory | All existing API endpoints must work unchanged |
| **File-Based Storage** | ✅ Mandatory | JSON files in `/settings` remain default storage |
| **Data Preservation** | ✅ Mandatory | All existing configuration data must remain intact |
| **Self-Contained** | ✅ Mandatory | No external database dependencies required |
| **Migration Path** | ✅ If Needed | Automatic migration if alternative storage added |

### Key Improvements

- ✅ Dependency Injection throughout application
- ✅ Repository pattern abstracting file storage
- ✅ Comprehensive unit test coverage
- ✅ Modern async patterns (no `async void`)
- ✅ Structured logging with `ILogger<T>`
- ✅ Options pattern for configuration
- ✅ Enhanced testability and maintainability

### What Stays the Same

- ✅ All API routes and endpoints
- ✅ Request/response JSON formats
- ✅ JSON file storage in `/settings` folder
- ✅ API key authentication
- ✅ Application functionality and features
- ✅ User interface and experience

---

## 📋 Table of Contents

- [Executive Summary](#-executive-summary)
- [Overview](#overview)
- [Current Architecture Assessment](#current-architecture-assessment)
- [Modernization Objectives](#modernization-objectives)
- [Phase 1: Foundation & Infrastructure](#phase-1-foundation--infrastructure)
- [Phase 2: Service Layer Refactoring](#phase-2-service-layer-refactoring)
- [Phase 3: Controller Modernization](#phase-3-controller-modernization)
- [Phase 4: Configuration & Logging](#phase-4-configuration--logging)
- [Phase 5: Testing Infrastructure](#phase-5-testing-infrastructure)
- [Phase 6: Enhanced Features](#phase-6-enhanced-features)
- [Storage Architecture & Migration Strategy](#storage-architecture--migration-strategy)
- [API Compatibility Strategy](#api-compatibility-strategy)
- [Testing & Validation Checklist](#testing--validation-checklist)
- [Progress Tracking](#progress-tracking)
- [Notes & Best Practices](#notes--best-practices)

---

## Overview

This modernization plan updates the Azure Naming Tool to follow .NET best practices while **maintaining 100% backward compatibility** with existing API integrations and **preserving the file-based JSON storage system**.

### Application Context

The **Azure Naming Tool** helps organizations define and manage Azure Resource Naming Patterns. It stores configuration data (naming conventions, resource types, components, delimiters, etc.) as JSON files in the `/settings` folder on the file system.

### Key Principles

- ✅ **Zero Breaking Changes** - All existing APIs remain functional
- ✅ **File-Based Storage Preserved** - JSON file storage maintained as primary storage
- ✅ **Data Persistence Guaranteed** - All existing configuration data remains intact
- ✅ **Migration Path Required** - Any alternative storage must auto-migrate existing data
- ✅ **Gradual Migration** - Phase-by-phase implementation
- ✅ **Test Coverage** - Comprehensive testing at each phase
- ✅ **Maintainability** - Clean architecture with dependency injection
- ✅ **Documentation** - Track progress and decisions

---

## Current Architecture Assessment

### Technology Stack
- **Framework:** .NET 8
- **UI:** Blazor Server with Interactive Components
- **API:** ASP.NET Core Web API
- **Storage:** File-based JSON (settings folder) - **MUST BE PRESERVED**
- **Caching:** `MemoryCache.Default` (static)
- **Authentication:** Custom API Key attribute

### Data Storage Architecture

**Current Implementation:**
- All configuration stored as JSON files in `/settings` folder
- Files include: `resourcetypes.json`, `resourcelocations.json`, `resourceenvironments.json`, etc.
- Direct file I/O using `System.IO` and `System.Text.Json`
- No database dependencies - fully self-contained application

**Critical Requirement:**
> ⚠️ **The JSON file-based storage MUST remain the default and primary storage mechanism.** Users rely on the simplicity of file-based storage for easy backup, version control, and portability. Any changes must maintain this functionality.

### Current Issues

| Issue | Impact | Priority |
|-------|--------|----------|
| Static service classes | Hard to test, tight coupling | 🔴 High |
| Direct file I/O (no abstraction) | Cannot swap storage implementations | 🔴 High |
| `async void` methods | Fire-and-forget, error handling issues | 🔴 High |
| `MemoryCache.Default` | Not DI-friendly, disposal issues | 🟡 Medium |
| No service interfaces | Cannot mock for testing | 🔴 High |
| Mixed concerns in helpers | Business logic scattered | 🟡 Medium |
| Static ConfigurationHelper | Anti-pattern, hard to test | 🟡 Medium |

**Note:** While we're adding abstraction for file I/O, the JSON file storage mechanism itself will remain unchanged. The abstraction layer enables better testing and potential future storage options while maintaining file-based storage as the default.

---

## Modernization Objectives

### Primary Goals

1. **Dependency Injection (DI)** - Convert all static services to instance-based with DI
2. **Abstraction Layer** - Introduce interfaces for all services and repositories
3. **Repository Pattern** - Abstract file storage operations (while keeping JSON files)
4. **Modern Caching** - Use `IMemoryCache` instead of static cache
5. **Async Best Practices** - Fix all async anti-patterns
6. **Testability** - Enable comprehensive unit and integration testing
7. **Structured Logging** - Implement modern logging practices
8. **API Compatibility** - Maintain all existing endpoints unchanged
9. **Data Persistence** - Guarantee existing JSON data remains accessible and intact

### Storage Strategy

**Current State:**
- JSON files in `/settings` folder (e.g., `resourcetypes.json`, `resourcelocations.json`)
- Direct file operations using `FileSystemHelper.ReadFile()` and `WriteFile()`

**Modernization Approach:**
- **Phase 1:** Create repository abstraction layer
- **Default Implementation:** JSON file-based repository (maintains current behavior)
- **Future Options:** Enable alternative storage (SQLite, LiteDB) via configuration
- **Migration Path:** Automatic data migration if alternative storage is chosen

**Repository Abstraction Benefits:**
- ✅ Testable (can mock file operations)
- ✅ Maintainable (centralized storage logic)
- ✅ Flexible (can add new storage without changing business logic)
- ✅ Backward Compatible (existing JSON files continue to work)

### Success Metrics

- [ ] 100% of services use dependency injection
- [ ] 80%+ unit test coverage for business logic
- [ ] All `async void` methods replaced with `async Task`
- [ ] Zero breaking changes to API contracts
- [ ] All integration tests passing
- [ ] Performance maintained or improved

---

## Phase 1: Foundation & Infrastructure

**Duration:** 2 weeks  
**Status:** ⬜ Not Started  
**Priority:** 🔴 Critical  

### 1.1 Create Service Interfaces

**Tasks:**
- [ ] Create `src/Services/Interfaces/` folder
- [ ] Define interfaces for all existing services
- [ ] Document interface contracts with XML comments

**Files to Create:**

```
src/Services/Interfaces/
├── IResourceNamingRequestService.cs
├── IResourceTypeService.cs
├── IResourceComponentService.cs
├── IResourceLocationService.cs
├── IResourceEnvironmentService.cs
├── IResourceOrgService.cs
├── IResourceProjAppSvcService.cs
├── IResourceUnitDeptService.cs
├── IResourceFunctionService.cs
├── IResourceDelimiterService.cs
├── ICustomComponentService.cs
├── IGeneratedNamesService.cs
├── IAdminLogService.cs
├── IAdminService.cs
├── IAdminUserService.cs
├── IPolicyService.cs
└── IImportExportService.cs
```

**Example Interface Template:**

```csharp
namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Service for managing resource types
    /// </summary>
    public interface IResourceTypeService
    {
        /// <summary>
        /// Get all resource types
        /// </summary>
        /// <param name="admin">Include admin-only items</param>
        /// <returns>Service response with resource types</returns>
        Task<ServiceResponse> GetItems(bool admin = true);
        
        /// <summary>
        /// Get a specific resource type by ID
        /// </summary>
        /// <param name="id">Resource type ID</param>
        /// <returns>Service response with resource type</returns>
        Task<ServiceResponse> GetItem(int id);
        
        /// <summary>
        /// Create or update a resource type
        /// </summary>
        /// <param name="item">Resource type to save</param>
        /// <returns>Service response</returns>
        Task<ServiceResponse> PostItem(ResourceType item);
        
        /// <summary>
        /// Delete a resource type
        /// </summary>
        /// <param name="id">Resource type ID</param>
        /// <returns>Service response</returns>
        Task<ServiceResponse> DeleteItem(int id);
    }
}
```

**Testing Checklist:**
- [ ] All interfaces compile without errors
- [ ] XML documentation is complete
- [ ] Naming conventions follow .NET standards

---

### 1.2 Create Repository Abstraction Layer

**Tasks:**
- [ ] Create `src/Repositories/` folder structure
- [ ] Define generic repository interface
- [ ] Create site configuration repository interface
- [ ] Implement JSON file-based repositories (default)
- [ ] Add storage provider configuration
- [ ] Create migration utilities for future storage options

**Files to Create:**

```
src/Repositories/
├── Interfaces/
│   ├── IConfigurationRepository.cs
│   ├── ISiteConfigurationRepository.cs
│   ├── IStorageProvider.cs
│   └── IDataMigration.cs
└── Implementation/
    ├── FileSystem/
    │   ├── JsonFileConfigurationRepository.cs
    │   ├── JsonSiteConfigurationRepository.cs
    │   └── FileSystemStorageProvider.cs
    ├── Embedded/  (future: optional SQLite/LiteDB)
    │   └── README.md
    └── Migrations/
        ├── IDataMigrationService.cs
        └── FileToEmbeddedMigration.cs (future use)
```

**Storage Provider Interface:**

```csharp
namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Defines the storage provider abstraction
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Get the storage provider name
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// Check if storage is available and accessible
        /// </summary>
        Task<bool> IsAvailableAsync();
        
        /// <summary>
        /// Initialize storage provider
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Get storage health status
        /// </summary>
        Task<StorageHealthStatus> GetHealthAsync();
    }
    
    public record StorageHealthStatus(bool IsHealthy, string Message, Dictionary<string, object>? Metadata = null);
}
```

**Generic Repository Interface:**

```csharp
namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository for configuration entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IConfigurationRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task SaveAsync(T item);
        Task SaveAllAsync(IEnumerable<T> items);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
```

**File-Based Repository Implementation:**

```csharp
namespace AzureNamingTool.Repositories.Implementation.FileSystem
{
    /// <summary>
    /// JSON file-based repository implementation (DEFAULT)
    /// Maintains existing file storage behavior
    /// </summary>
    public class JsonFileConfigurationRepository<T> : IConfigurationRepository<T> where T : class
    {
        private readonly ILogger<JsonFileConfigurationRepository<T>> _logger;
        private readonly string _fileName;
        private readonly string _settingsPath;

        public JsonFileConfigurationRepository(
            ILogger<JsonFileConfigurationRepository<T>> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            
            // Determine file name based on type
            _fileName = GetFileNameForType();
            
            _logger.LogDebug("Repository initialized for {Type}, File: {FileName}", 
                typeof(T).Name, _fileName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, _fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return Enumerable.Empty<T>();
                }

                var json = await File.ReadAllTextAsync(filePath);
                var items = JsonSerializer.Deserialize<List<T>>(json);
                
                _logger.LogDebug("Loaded {Count} items from {FileName}", items?.Count ?? 0, _fileName);
                return items ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from {FileName}", _fileName);
                throw;
            }
        }

        public async Task SaveAllAsync(IEnumerable<T> items)
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, _fileName);
                
                // Ensure directory exists
                Directory.CreateDirectory(_settingsPath);
                
                var json = JsonSerializer.Serialize(items, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(filePath, json);
                
                _logger.LogInformation("Saved {Count} items to {FileName}", 
                    items.Count(), _fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to {FileName}", _fileName);
                throw;
            }
        }

        private string GetFileNameForType()
        {
            var typeName = typeof(T).Name;
            return typeName switch
            {
                nameof(ResourceType) => "resourcetypes.json",
                nameof(ResourceLocation) => "resourcelocations.json",
                nameof(ResourceEnvironment) => "resourceenvironments.json",
                nameof(ResourceOrg) => "resourceorgs.json",
                nameof(ResourceProjAppSvc) => "resourceprojappsvcs.json",
                nameof(ResourceUnitDept) => "resourceunitdepts.json",
                nameof(ResourceFunction) => "resourcefunctions.json",
                nameof(ResourceDelimiter) => "resourcedelimiters.json",
                nameof(ResourceComponent) => "resourcecomponents.json",
                nameof(CustomComponent) => "customcomponents.json",
                nameof(GeneratedName) => "generatednames.json",
                nameof(AdminLogMessage) => "adminlogmessages.json",
                nameof(AdminUser) => "adminusers.json",
                _ => $"{typeName.ToLowerInvariant()}s.json"
            };
        }

        // ... implement other interface methods ...
    }
}
```

**File System Storage Provider:**

```csharp
namespace AzureNamingTool.Repositories.Implementation.FileSystem
{
    /// <summary>
    /// File system storage provider (DEFAULT IMPLEMENTATION)
    /// </summary>
    public class FileSystemStorageProvider : IStorageProvider
    {
        private readonly ILogger<FileSystemStorageProvider> _logger;
        private readonly string _settingsPath;

        public string ProviderName => "FileSystem (JSON)";

        public FileSystemStorageProvider(
            ILogger<FileSystemStorageProvider> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
        }

        public Task<bool> IsAvailableAsync()
        {
            try
            {
                return Task.FromResult(Directory.Exists(_settingsPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking storage availability");
                return Task.FromResult(false);
            }
        }

        public Task InitializeAsync()
        {
            try
            {
                if (!Directory.Exists(_settingsPath))
                {
                    Directory.CreateDirectory(_settingsPath);
                    _logger.LogInformation("Created settings directory: {Path}", _settingsPath);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing file system storage");
                throw;
            }
        }

        public async Task<StorageHealthStatus> GetHealthAsync()
        {
            try
            {
                var isAvailable = await IsAvailableAsync();
                
                if (!isAvailable)
                {
                    return new StorageHealthStatus(false, "Settings directory not accessible");
                }

                // Check if we can write
                var testFile = Path.Combine(_settingsPath, ".health-check");
                await File.WriteAllTextAsync(testFile, DateTime.UtcNow.ToString());
                File.Delete(testFile);

                var metadata = new Dictionary<string, object>
                {
                    ["SettingsPath"] = _settingsPath,
                    ["DirectoryExists"] = Directory.Exists(_settingsPath),
                    ["FileCount"] = Directory.GetFiles(_settingsPath, "*.json").Length
                };

                return new StorageHealthStatus(true, "File system storage is healthy", metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage health check failed");
                return new StorageHealthStatus(false, $"Health check failed: {ex.Message}");
            }
        }
    }
}
```

**Site Configuration Repository Interface:**

```csharp
namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for site configuration settings
    /// </summary>
    public interface ISiteConfigurationRepository
    {
        Task<ConfigurationData> GetConfigurationAsync();
        Task UpdateConfigurationAsync(ConfigurationData config);
        Task<string> GetSettingAsync(string key);
        Task SetSettingAsync(string key, string value);
        Task<Dictionary<string, string>> GetAllSettingsAsync();
    }
}
```

**Configuration Options for Storage:**

```csharp
// appsettings.json
{
  "StorageOptions": {
    "Provider": "FileSystem",  // Default: FileSystem, Future: SQLite, LiteDB
    "FileSystemPath": "settings",
    "AutoMigrateData": true,  // Automatically migrate if provider changes
    "BackupBeforeMigration": true
  }
}
```

**Future: Data Migration Service Interface:**

```csharp
namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Service for migrating data between storage providers
    /// </summary>
    public interface IDataMigrationService
    {
        /// <summary>
        /// Check if migration is needed
        /// </summary>
        Task<bool> IsMigrationNeededAsync(string fromProvider, string toProvider);
        
        /// <summary>
        /// Migrate all data from one storage provider to another
        /// </summary>
        Task<MigrationResult> MigrateAsync(string fromProvider, string toProvider, bool createBackup = true);
        
        /// <summary>
        /// Create backup of current data
        /// </summary>
        Task<string> CreateBackupAsync();
        
        /// <summary>
        /// Restore from backup
        /// </summary>
        Task RestoreBackupAsync(string backupPath);
    }
    
    public record MigrationResult(
        bool Success, 
        string Message, 
        int ItemsMigrated, 
        string? BackupPath = null,
        List<string>? Errors = null);
}
```

**Important Notes:**

> ⚠️ **The JsonFileConfigurationRepository is the DEFAULT and ONLY implementation in Phase 1.** 
> 
> The repository abstraction enables:
> 1. **Better testing** - Can mock repository in unit tests
> 2. **Centralized logic** - All file I/O in one place
> 3. **Future flexibility** - Can add SQLite/LiteDB later without changing services
> 4. **Data integrity** - Consistent file handling across the application
>
> **All existing JSON files continue to work exactly as before.** The abstraction layer is transparent to the file storage mechanism.

**Testing Checklist:**
- [ ] Repository interfaces compile without errors
- [ ] JSON file repository reads existing data files correctly
- [ ] JSON file repository writes data in same format as before
- [ ] File paths and permissions work correctly
- [ ] Error handling for missing/corrupt files works
- [ ] Storage provider health check works
- [ ] All existing configuration files load successfully

---

### 1.3 Implement Modern Caching Service

**Tasks:**
- [ ] Create `ICacheService` interface
- [ ] Implement `CacheService` using `IMemoryCache`
- [ ] Replace all `MemoryCache.Default` usage
- [ ] Add cache invalidation support

**Files to Create/Modify:**

```
src/Services/Interfaces/ICacheService.cs
src/Services/CacheService.cs
src/Helpers/CacheHelper.cs (modify to use ICacheService)
```

**Cache Service Interface:**

```csharp
namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Service for managing application cache
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get cached object by key
        /// </summary>
        T? GetCacheObject<T>(string cacheKey) where T : class;
        
        /// <summary>
        /// Set cache object with expiration
        /// </summary>
        void SetCacheObject<T>(string cacheKey, T data, int expirationMinutes = 60) where T : class;
        
        /// <summary>
        /// Remove specific cache entry
        /// </summary>
        void InvalidateCacheObject(string cacheKey);
        
        /// <summary>
        /// Clear all cache entries
        /// </summary>
        void ClearAllCache();
        
        /// <summary>
        /// Check if cache key exists
        /// </summary>
        bool Exists(string cacheKey);
    }
}
```

**Implementation Example:**

```csharp
namespace AzureNamingTool.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public T? GetCacheObject<T>(string cacheKey) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(cacheKey, out T? cachedData))
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    return cachedData;
                }
                
                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache object for key: {CacheKey}", cacheKey);
                return null;
            }
        }

        public void SetCacheObject<T>(string cacheKey, T data, int expirationMinutes = 60) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                    Priority = CacheItemPriority.Normal
                };

                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _cacheKeys.TryRemove(key.ToString()!, out _);
                    _logger.LogDebug("Cache evicted: {Key}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(cacheKey, data, cacheOptions);
                _cacheKeys.TryAdd(cacheKey, 0);
                
                _logger.LogDebug("Cache set for key: {CacheKey}, Expiration: {Minutes} minutes", 
                    cacheKey, expirationMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache object for key: {CacheKey}", cacheKey);
            }
        }

        public void InvalidateCacheObject(string cacheKey)
        {
            try
            {
                _memoryCache.Remove(cacheKey);
                _cacheKeys.TryRemove(cacheKey, out _);
                _logger.LogInformation("Cache invalidated for key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
            }
        }

        public void ClearAllCache()
        {
            try
            {
                foreach (var key in _cacheKeys.Keys)
                {
                    _memoryCache.Remove(key);
                }
                _cacheKeys.Clear();
                _logger.LogInformation("All cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache");
            }
        }

        public bool Exists(string cacheKey)
        {
            return _cacheKeys.ContainsKey(cacheKey);
        }
    }
}
```

**Testing Checklist:**
- [ ] Cache service compiles and registers in DI
- [ ] Cache expiration works correctly
- [ ] Cache invalidation works
- [ ] Memory usage is reasonable
- [ ] Concurrent access is thread-safe

---

### Phase 1 Completion Criteria

- [ ] All interfaces created and documented
- [ ] Repository pattern implemented for file-based storage
- [ ] Modern caching service implemented
- [ ] All code compiles without errors
- [ ] **Existing JSON files load correctly through repository layer**
- [ ] **File write operations preserve exact JSON format**
- [ ] **No data loss or corruption in test scenarios**
- [ ] Basic unit tests for repositories and cache service passing
- [ ] Integration tests verify file operations work correctly

**Critical Validation Steps:**

1. **Backup Production Data**: Copy `/settings` folder before testing
2. **Test Read Operations**: Verify all existing JSON files can be read
3. **Test Write Operations**: Verify writes maintain JSON structure
4. **Compare Output**: Ensure written JSON matches original format
5. **Test Edge Cases**: Empty files, malformed JSON, missing files
6. **Performance Test**: Ensure file operations are not slower

**Validation Script:**

```powershell
# PowerShell script to validate repository implementation

# 1. Backup current settings
Copy-Item -Path "settings" -Destination "settings-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# 2. Run application with new repository layer
# Application should start and load all data

# 3. Compare JSON files
$original = Get-ChildItem "settings-backup-*" -Recurse -Filter "*.json"
$current = Get-ChildItem "settings" -Filter "*.json"

foreach ($file in $current) {
    $originalFile = $original | Where-Object { $_.Name -eq $file.Name }
    if ($originalFile) {
        $diff = Compare-Object (Get-Content $originalFile.FullName) (Get-Content $file.FullName)
        if ($diff) {
            Write-Warning "File changed: $($file.Name)"
            $diff | Format-Table
        } else {
            Write-Host "✓ File unchanged: $($file.Name)" -ForegroundColor Green
        }
    }
}
```

**Risks & Mitigation:**
- **Risk:** Breaking existing functionality during refactoring
- **Mitigation:** Keep existing static methods as wrappers initially, comprehensive testing

- **Risk:** Data corruption during file operations
- **Mitigation:** Implement atomic writes with backups, validate after each write

- **Risk:** Performance degradation
- **Mitigation:** Benchmark file operations, optimize if needed

---

## Phase 2: Service Layer Refactoring

**Duration:** 2 weeks  
**Status:** ⬜ Not Started  
**Priority:** 🔴 Critical  

### 2.1 Convert Static Services to Instance Services

**Strategy:** Convert one service at a time, starting with leaf dependencies.

**Service Dependency Order:**

1. **No Dependencies (Start Here):**
   - [ ] ResourceDelimiterService
   - [ ] ResourceLocationService
   - [ ] ResourceEnvironmentService
   - [ ] ResourceOrgService
   - [ ] ResourceProjAppSvcService
   - [ ] ResourceUnitDeptService
   - [ ] ResourceFunctionService

2. **Low Dependencies:**
   - [ ] CustomComponentService
   - [ ] AdminUserService

3. **Medium Dependencies:**
   - [ ] ResourceComponentService
   - [ ] ResourceTypeService
   - [ ] AdminLogService

4. **High Dependencies:**
   - [ ] GeneratedNamesService
   - [ ] ResourceNamingRequestService

5. **Service Orchestrators:**
   - [ ] AdminService
   - [ ] PolicyService
   - [ ] ImportExportService

**Conversion Template:**

```csharp
// BEFORE: Static service
public class ResourceTypeService
{
    public static async Task<ServiceResponse> GetItems(bool admin = true)
    {
        // Implementation
    }
}

// AFTER: Instance service with DI
namespace AzureNamingTool.Services
{
    public class ResourceTypeService : IResourceTypeService
    {
        private readonly IConfigurationRepository<ResourceType> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ResourceTypeService> _logger;

        public ResourceTypeService(
            IConfigurationRepository<ResourceType> repository,
            ICacheService cacheService,
            ILogger<ResourceTypeService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResponse> GetItems(bool admin = true)
        {
            try
            {
                _logger.LogDebug("Getting resource types, admin: {Admin}", admin);
                
                // Check cache first
                var cacheKey = $"ResourceTypes_{admin}";
                var cachedData = _cacheService.GetCacheObject<List<ResourceType>>(cacheKey);
                
                if (cachedData != null)
                {
                    return new ServiceResponse { ResponseObject = cachedData };
                }

                // Load from repository
                var items = await _repository.GetAllAsync();
                var itemsList = items.ToList();
                
                // Cache the result
                _cacheService.SetCacheObject(cacheKey, itemsList);
                
                return new ServiceResponse { ResponseObject = itemsList };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource types");
                return new ServiceResponse 
                { 
                    Success = false, 
                    ResponseMessage = ex.Message 
                };
            }
        }
    }
}
```

**For Each Service, Complete:**
- [ ] Create interface in `Services/Interfaces/`
- [ ] Implement interface with DI constructor
- [ ] Replace direct file I/O with repository calls
- [ ] Replace `MemoryCache.Default` with `ICacheService`
- [ ] Add `ILogger<T>` for structured logging
- [ ] Add null checks and argument validation
- [ ] Improve error handling and logging
- [ ] Add XML documentation comments
- [ ] Create unit tests

**Testing Checklist Per Service:**
- [ ] Service registers in DI container
- [ ] All public methods work correctly
- [ ] Caching works as expected
- [ ] Error handling logs appropriately
- [ ] Unit tests cover happy path and edge cases

---

### 2.2 Fix Async Anti-patterns

**Tasks:**
- [ ] Find all `async void` methods
- [ ] Convert to `async Task`
- [ ] Ensure proper exception handling
- [ ] Update all callers to `await`

**Common Patterns to Fix:**

```csharp
// ❌ BEFORE: Fire-and-forget (dangerous)
public static async void PostItem(AdminLogMessage message)
{
    // If this throws, exception is lost!
}

// ✅ AFTER: Proper async
public async Task PostItem(AdminLogMessage message)
{
    try
    {
        // Implementation
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error posting admin log message");
        // Re-throw or handle appropriately
    }
}

// Caller must now await:
await _adminLogService.PostItem(message);
```

**Files with `async void` to Fix:**
- [ ] `src/Services/AdminLogService.cs`
- [ ] Check all service files for `async void`
- [ ] Check all helper files

**Testing:**
- [ ] No `async void` methods remain (except event handlers)
- [ ] All async calls are properly awaited
- [ ] Exception handling works correctly

---

### 2.3 Register Services in DI Container

**Tasks:**
- [ ] Update `Program.cs` with all service registrations
- [ ] Choose appropriate lifetimes (Scoped/Singleton/Transient)
- [ ] Add configuration options
- [ ] Document service dependencies

**Service Registration in Program.cs:**

```csharp
// Add to src/Program.cs

// ===== Infrastructure Services =====
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// ===== Repositories =====
builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(JsonFileConfigurationRepository<>));
builder.Services.AddScoped<ISiteConfigurationRepository, JsonSiteConfigurationRepository>();

// ===== Core Services =====
// Component Services (no dependencies)
builder.Services.AddScoped<IResourceDelimiterService, ResourceDelimiterService>();
builder.Services.AddScoped<IResourceLocationService, ResourceLocationService>();
builder.Services.AddScoped<IResourceEnvironmentService, ResourceEnvironmentService>();
builder.Services.AddScoped<IResourceOrgService, ResourceOrgService>();
builder.Services.AddScoped<IResourceProjAppSvcService, ResourceProjAppSvcService>();
builder.Services.AddScoped<IResourceUnitDeptService, ResourceUnitDeptService>();
builder.Services.AddScoped<IResourceFunctionService, ResourceFunctionService>();

// Business Services
builder.Services.AddScoped<ICustomComponentService, CustomComponentService>();
builder.Services.AddScoped<IResourceComponentService, ResourceComponentService>();
builder.Services.AddScoped<IResourceTypeService, ResourceTypeService>();

// High-level Services
builder.Services.AddScoped<IGeneratedNamesService, GeneratedNamesService>();
builder.Services.AddScoped<IResourceNamingRequestService, ResourceNamingRequestService>();

// Admin Services
builder.Services.AddScoped<IAdminLogService, AdminLogService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Policy & Import/Export
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();

// ===== Helpers (if converting to services) =====
builder.Services.AddScoped<IValidationHelper, ValidationHelper>();
```

**Service Lifetime Guidelines:**

| Lifetime | Use For | Examples |
|----------|---------|----------|
| **Singleton** | Stateless, thread-safe services | `ICacheService` |
| **Scoped** | Per-request services (API/Blazor) | Most services |
| **Transient** | Lightweight, stateless utilities | Validators |

**Testing Checklist:**
- [ ] Application starts without DI errors
- [ ] All services can be resolved
- [ ] No circular dependencies
- [ ] Service lifetimes are appropriate

---

### Phase 2 Completion Criteria

- [ ] All services converted to instance-based with interfaces
- [ ] No static service methods remain (except temporary wrappers)
- [ ] All services registered in DI container
- [ ] All `async void` converted to `async Task`
- [ ] Unit tests passing for all services (minimum 70% coverage)
- [ ] Integration tests confirm functionality unchanged

---

## Phase 3: Controller Modernization

**Duration:** 1 week  
**Status:** ⬜ Not Started  
**Priority:** 🟡 High  

### 3.1 Update Controllers to Use Dependency Injection

**Controllers to Update:**
- [ ] `AdminController.cs`
- [ ] `CustomComponentsController.cs`
- [ ] `ImportExportController.cs`
- [ ] `PolicyController.cs`
- [ ] `ResourceComponentsController.cs`
- [ ] `ResourceDelimitersController.cs`
- [ ] `ResourceEnvironmentsController.cs`
- [ ] `ResourceFunctionsController.cs`
- [ ] `ResourceLocationsController.cs`
- [ ] `ResourceNamingRequestsController.cs`
- [ ] `ResourceOrgsController.cs`
- [ ] `ResourceProjAppSvcsController.cs`
- [ ] `ResourceTypesController.cs`
- [ ] `ResourceUnitDeptsController.cs`

**Conversion Template:**

```csharp
// BEFORE: Static service calls
[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceTypesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(bool admin = true)
    {
        var response = await ResourceTypeService.GetItems(admin);
        return Ok(response);
    }
}

// AFTER: Dependency injection
namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource types
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceTypesController : ControllerBase
    {
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;
        private readonly ILogger<ResourceTypesController> _logger;

        public ResourceTypesController(
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService,
            ILogger<ResourceTypesController> logger)
        {
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
            _logger = logger;
        }

        /// <summary>
        /// This function will return the resource types data.
        /// </summary>
        /// <param name="admin">bool - All/Only-enabled items</param>
        /// <returns>List of ResourceType</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = true)
        {
            try
            {
                _logger.LogInformation("Getting resource types, admin: {Admin}", admin);
                var response = await _resourceTypeService.GetItems(admin);
                
                if (response.Success)
                {
                    return Ok(response.ResponseObject);
                }
                else
                {
                    _logger.LogWarning("Failed to get resource types: {Message}", response.ResponseMessage);
                    return BadRequest(response.ResponseMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResourceTypesController.Get");
                await _adminLogService.PostItem(new AdminLogMessage 
                { 
                    Title = "ERROR", 
                    Message = ex.Message 
                });
                return StatusCode(500, "An error occurred processing your request.");
            }
        }

        // ... other endpoints maintain same routes/signatures ...
    }
}
```

**Critical Rules:**
- ✅ **Keep all route patterns unchanged** - `[Route("api/[controller]")]`
- ✅ **Keep all action routes unchanged** - `[Route("[action]")]`
- ✅ **Keep all HTTP methods** - `[HttpGet]`, `[HttpPost]`, etc.
- ✅ **Keep all parameter names and types** - Exact same signatures
- ✅ **Keep response formats** - Same JSON structure
- ✅ **Keep authentication attributes** - `[ApiKey]` remains

**For Each Controller:**
- [ ] Add constructor with service dependencies
- [ ] Replace static service calls with injected services
- [ ] Add structured logging
- [ ] Improve error handling
- [ ] Keep XML documentation comments
- [ ] Test all endpoints

**Testing Checklist:**
- [ ] All API endpoints respond correctly
- [ ] Request/response formats unchanged
- [ ] Authentication still works
- [ ] Error responses are appropriate
- [ ] Logging captures request details

---

### 3.2 API Compatibility Testing

**Create API Test Suite:**
- [ ] Test all GET endpoints
- [ ] Test all POST endpoints
- [ ] Test authentication (valid/invalid keys)
- [ ] Test error scenarios
- [ ] Compare responses with previous version

**Integration Test Example:**

```csharp
public class ResourceTypesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ResourceTypesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("APIKey", "test-api-key");
    }

    [Fact]
    public async Task Get_ReturnsResourceTypes_WithValidApiKey()
    {
        // Act
        var response = await _client.GetAsync("/api/ResourceTypes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse>(content);
        
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}
```

**API Contract Tests:**
- [ ] Document all API endpoints
- [ ] Create contract tests for each endpoint
- [ ] Validate JSON schemas match
- [ ] Test backward compatibility

---

### Phase 3 Completion Criteria

- [ ] All controllers use dependency injection
- [ ] No static service calls in controllers
- [ ] All API endpoints tested and working
- [ ] API compatibility verified 100%
- [ ] Integration tests passing
- [ ] Postman/API documentation updated

---

## Phase 4: Configuration & Logging

**Duration:** 1 week  
**Status:** ⬜ Not Started  
**Priority:** 🟡 Medium  

### 4.1 Implement Options Pattern for Configuration

**Tasks:**
- [ ] Create configuration option classes
- [ ] Register options in DI
- [ ] Replace static ConfigurationHelper usage
- [ ] Add configuration validation

**Create Configuration Options:**

```
src/Configuration/
├── SiteConfigurationOptions.cs
├── SecurityOptions.cs
├── CacheOptions.cs
└── StorageOptions.cs
```

**Example Options Class:**

```csharp
namespace AzureNamingTool.Configuration
{
    /// <summary>
    /// Site configuration options
    /// </summary>
    public class SiteConfigurationOptions
    {
        public const string SectionName = "SiteConfiguration";

        /// <summary>
        /// Application theme
        /// </summary>
        public string AppTheme { get; set; } = "default";

        /// <summary>
        /// Enable development mode features
        /// </summary>
        public bool DevMode { get; set; }

        /// <summary>
        /// Allow duplicate resource names
        /// </summary>
        public bool DuplicateNamesAllowed { get; set; }

        /// <summary>
        /// Enable connectivity checks
        /// </summary>
        public bool ConnectivityCheckEnabled { get; set; } = true;

        /// <summary>
        /// Cache expiration in minutes
        /// </summary>
        [Range(1, 1440)]
        public int CacheExpirationMinutes { get; set; } = 60;
    }
}
```

**Register in Program.cs:**

```csharp
// Configure options
builder.Services.Configure<SiteConfigurationOptions>(
    builder.Configuration.GetSection(SiteConfigurationOptions.SectionName));

builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection(SecurityOptions.SectionName));

// Add options validation
builder.Services.AddOptions<SiteConfigurationOptions>()
    .Bind(builder.Configuration.GetSection(SiteConfigurationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Use Options in Services:**

```csharp
public class SomeService
{
    private readonly SiteConfigurationOptions _config;
    private readonly ILogger<SomeService> _logger;

    public SomeService(
        IOptions<SiteConfigurationOptions> config,
        ILogger<SomeService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public void DoSomething()
    {
        if (_config.DevMode)
        {
            _logger.LogDebug("Development mode is enabled");
        }
    }
}
```

**Migration Checklist:**
- [ ] Configuration classes created
- [ ] Options registered in DI
- [ ] ConfigurationHelper static usage replaced
- [ ] Configuration validation added
- [ ] Tests verify configuration loading

---

### 4.2 Enhance Structured Logging

**Tasks:**
- [ ] Define logging standards
- [ ] Add correlation IDs for request tracking
- [ ] Configure log levels per environment
- [ ] Add performance logging
- [ ] Keep AdminLogService for audit trail

**Logging Standards:**

```csharp
// Standard logging patterns

// Method entry (Debug level)
_logger.LogDebug("Starting {Method} with parameters: {@Parameters}", 
    nameof(RequestName), request);

// Business events (Information level)
_logger.LogInformation("Resource name generated: {ResourceName} for type: {ResourceType}", 
    result.ResourceName, request.ResourceType);

// Warnings (Warning level)
_logger.LogWarning("Duplicate name detected: {ResourceName}", name);

// Errors (Error level)
_logger.LogError(ex, "Error generating resource name for request: {@Request}", request);

// Performance tracking
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["UserId"] = userId
}))
{
    var sw = Stopwatch.StartNew();
    // ... operation ...
    sw.Stop();
    _logger.LogInformation("Operation completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
}
```

**Configure Logging in appsettings.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AzureNamingTool": "Debug"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "SingleLine": false,
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss ",
        "UseUtcTimestamp": true,
        "JsonWriterOptions": {
          "Indented": true
        }
      }
    }
  }
}
```

**Separation of Concerns:**

- **ILogger<T>** - Technical/operational logging (errors, performance, debugging)
- **IAdminLogService** - Business audit trail (visible in admin UI)

**Testing:**
- [ ] Logs written at appropriate levels
- [ ] Structured logging includes context
- [ ] No sensitive data in logs
- [ ] Performance acceptable

---

### Phase 4 Completion Criteria

- [ ] Options pattern implemented for all configuration
- [ ] Static configuration helper replaced
- [ ] Structured logging standards documented
- [ ] Logging configured appropriately per environment
- [ ] AdminLogService maintained for audit trail
- [ ] Tests verify configuration and logging

---

## Phase 5: Testing Infrastructure

**Duration:** 1 week  
**Status:** ⬜ Not Started  
**Priority:** 🟡 High  

### 5.1 Set Up Unit Testing Framework

**Tasks:**
- [ ] Add testing packages to test project
- [ ] Create test structure matching source
- [ ] Set up mocking framework
- [ ] Create test helpers and fixtures

**Update Test Project:**

```xml
<!-- tests/AzureNamingTool.UnitTests/AzureNamingTool.UnitTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\AzureNamingTool.csproj" />
  </ItemGroup>
</Project>
```

**Test Structure:**

```
tests/AzureNamingTool.UnitTests/
├── Services/
│   ├── ResourceTypeServiceTests.cs
│   ├── ResourceNamingRequestServiceTests.cs
│   ├── CacheServiceTests.cs
│   └── ... (one test file per service)
├── Repositories/
│   ├── JsonFileConfigurationRepositoryTests.cs
│   └── SiteConfigurationRepositoryTests.cs
├── Controllers/
│   ├── ResourceTypesControllerTests.cs
│   └── ... (one test file per controller)
├── Helpers/
│   └── TestHelper.cs
└── Fixtures/
    └── TestDataFixture.cs
```

---

### 5.2 Write Unit Tests for Services

**Test Template:**

```csharp
namespace AzureNamingTool.UnitTests.Services
{
    public class ResourceTypeServiceTests
    {
        private readonly Mock<IConfigurationRepository<ResourceType>> _mockRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<ResourceTypeService>> _mockLogger;
        private readonly ResourceTypeService _sut; // System Under Test

        public ResourceTypeServiceTests()
        {
            _mockRepository = new Mock<IConfigurationRepository<ResourceType>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<ResourceTypeService>>();
            
            _sut = new ResourceTypeService(
                _mockRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetItems_WhenCacheHit_ReturnsCachedData()
        {
            // Arrange
            var cachedTypes = new List<ResourceType>
            {
                new ResourceType { Id = 1, ShortName = "rg" }
            };
            
            _mockCacheService
                .Setup(x => x.GetCacheObject<List<ResourceType>>(It.IsAny<string>()))
                .Returns(cachedTypes);

            // Act
            var result = await _sut.GetItems(admin: true);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResponseObject.Should().BeEquivalentTo(cachedTypes);
            
            // Verify repository was not called
            _mockRepository.Verify(
                x => x.GetAllAsync(), 
                Times.Never);
        }

        [Fact]
        public async Task GetItems_WhenCacheMiss_LoadsFromRepository()
        {
            // Arrange
            var repositoryTypes = new List<ResourceType>
            {
                new ResourceType { Id = 1, ShortName = "rg" },
                new ResourceType { Id = 2, ShortName = "st" }
            };
            
            _mockCacheService
                .Setup(x => x.GetCacheObject<List<ResourceType>>(It.IsAny<string>()))
                .Returns((List<ResourceType>?)null);
            
            _mockRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(repositoryTypes);

            // Act
            var result = await _sut.GetItems(admin: true);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResponseObject.Should().BeEquivalentTo(repositoryTypes);
            
            // Verify cache was set
            _mockCacheService.Verify(
                x => x.SetCacheObject(It.IsAny<string>(), It.IsAny<List<ResourceType>>(), It.IsAny<int>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetItem_WithValidId_ReturnsItem()
        {
            // Arrange
            var expectedType = new ResourceType { Id = 1, ShortName = "rg" };
            
            _mockRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(expectedType);

            // Act
            var result = await _sut.GetItem(1);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResponseObject.Should().BeEquivalentTo(expectedType);
        }

        [Fact]
        public async Task GetItem_WithInvalidId_ReturnsError()
        {
            // Arrange
            _mockRepository
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((ResourceType?)null);

            // Act
            var result = await _sut.GetItem(999);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ResponseMessage.Should().Contain("not found");
        }

        [Fact]
        public async Task PostItem_WithValidItem_SavesAndInvalidatesCache()
        {
            // Arrange
            var newType = new ResourceType { Id = 1, ShortName = "rg" };
            
            _mockRepository
                .Setup(x => x.SaveAsync(newType))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PostItem(newType);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            
            _mockRepository.Verify(x => x.SaveAsync(newType), Times.Once);
            _mockCacheService.Verify(x => x.InvalidateCacheObject(It.IsAny<string>()), Times.Once);
        }
    }
}
```

**Coverage Goals:**

| Component | Target Coverage |
|-----------|----------------|
| Services | 80%+ |
| Repositories | 80%+ |
| Controllers | 70%+ |
| Helpers/Utilities | 70%+ |
| Models | N/A (data classes) |

**Testing Checklist:**
- [ ] Happy path tests for all services
- [ ] Error condition tests
- [ ] Edge case tests
- [ ] Mock verification tests
- [ ] Async operation tests

---

### 5.3 Integration Testing

**Tasks:**
- [ ] Set up WebApplicationFactory
- [ ] Create test database/storage
- [ ] Test full request pipelines
- [ ] Test API endpoints end-to-end

**Integration Test Example:**

```csharp
namespace AzureNamingTool.IntegrationTests
{
    public class ResourceNamingRequestIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ResourceNamingRequestIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("APIKey", TestHelper.GetTestApiKey());
        }

        [Fact]
        public async Task RequestName_WithValidRequest_ReturnsGeneratedName()
        {
            // Arrange
            var request = new ResourceNameRequest
            {
                ResourceType = "rg",
                ResourceEnvironment = "prod",
                ResourceLocation = "eastus",
                ResourceInstance = "001"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/ResourceNamingRequests/RequestName", content);

            // Assert
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResourceNameResponse>(responseContent);
            
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResourceName.Should().NotBeNullOrEmpty();
        }
    }
}
```

---

### Phase 5 Completion Criteria

- [ ] Unit test framework configured
- [ ] 70%+ test coverage achieved
- [ ] All critical paths tested
- [ ] Integration tests for API endpoints
- [ ] Tests run in CI/CD pipeline
- [ ] Test documentation completed

---

## Phase 6: Enhanced Features

**Duration:** 1 week  
**Status:** ⬜ Not Started  
**Priority:** 🟢 Low  

### 6.1 Enhanced Health Checks

**Tasks:**
- [ ] Create comprehensive health checks
- [ ] Add storage health check
- [ ] Add cache health check
- [ ] Configure health check UI

**Implementation:**

```csharp
// src/HealthChecks/StorageHealthCheck.cs
namespace AzureNamingTool.HealthChecks
{
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly ISiteConfigurationRepository _configRepository;
        private readonly ILogger<StorageHealthCheck> _logger;

        public StorageHealthCheck(
            ISiteConfigurationRepository configRepository,
            ILogger<StorageHealthCheck> logger)
        {
            _configRepository = configRepository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _configRepository.GetConfigurationAsync();
                
                if (config != null)
                {
                    return HealthCheckResult.Healthy("Storage is accessible and configuration can be read.");
                }
                
                return HealthCheckResult.Degraded("Configuration returned null.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage health check failed");
                return HealthCheckResult.Unhealthy("Storage is not accessible.", ex);
            }
        }
    }
}

// src/HealthChecks/CacheHealthCheck.cs
namespace AzureNamingTool.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;

        public CacheHealthCheck(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var testKey = "health_check_test";
                var testData = new { test = "data" };
                
                _cacheService.SetCacheObject(testKey, testData, 1);
                var retrieved = _cacheService.GetCacheObject<object>(testKey);
                _cacheService.InvalidateCacheObject(testKey);
                
                if (retrieved != null)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Cache is working correctly."));
                }
                
                return Task.FromResult(HealthCheckResult.Degraded("Cache retrieval failed."));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Cache is not working.", ex));
            }
        }
    }
}
```

**Register Health Checks in Program.cs:**

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("storage", tags: new[] { "ready" })
    .AddCheck<CacheHealthCheck>("cache", tags: new[] { "ready" })
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"), tags: new[] { "live" });

// Configure endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

**Testing:**
- [ ] Health checks respond correctly
- [ ] Liveness probe works
- [ ] Readiness probe detects issues
- [ ] Health check UI displays status

---

### 6.2 API Versioning (Optional)

**Tasks:**
- [ ] Add API versioning packages
- [ ] Configure versioning strategy
- [ ] Document versioning approach

**Implementation:**

```csharp
// Add package
// <PackageReference Include="Asp.Versioning.Mvc" Version="8.0.0" />

// Configure in Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddMvc();

// Usage in controllers
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ResourceTypesController : ControllerBase
{
    // Endpoints automatically versioned
}

// For future v2
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ResourceTypesV2Controller : ControllerBase
{
    // New functionality while v1 still works
}
```

---

### 6.3 Performance Optimizations

**Tasks:**
- [ ] Add response compression
- [ ] Configure output caching
- [ ] Optimize database queries
- [ ] Add request/response caching headers

**Response Compression:**

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

app.UseResponseCompression();
```

**Output Caching (for GET endpoints):**

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
    options.AddPolicy("ResourceTypes", builder => 
        builder.Expire(TimeSpan.FromMinutes(10)));
});

// In controller
[HttpGet]
[OutputCache(PolicyName = "ResourceTypes")]
public async Task<IActionResult> Get(bool admin = true)
{
    // ...
}
```

---

### Phase 6 Completion Criteria

- [ ] Enhanced health checks implemented
- [ ] API versioning configured (if needed)
- [ ] Performance optimizations applied
- [ ] Metrics and monitoring configured
- [ ] Documentation updated

---

## Storage Architecture & Migration Strategy

### Current File-Based Storage

**Location:** `/settings` folder in application directory

**Configuration Files:**
- `resourcetypes.json` - Azure resource type definitions
- `resourcelocations.json` - Azure region/location mappings
- `resourceenvironments.json` - Environment designations (dev, prod, etc.)
- `resourceorgs.json` - Organization codes
- `resourceprojappsvcs.json` - Project/Application/Service codes
- `resourceunitdepts.json` - Unit/Department codes
- `resourcefunctions.json` - Function/workload identifiers
- `resourcedelimiters.json` - Delimiter characters for name components
- `resourcecomponents.json` - Component configuration
- `customcomponents.json` - User-defined custom components
- `generatednames.json` - History of generated names
- `adminlogmessages.json` - Administrative log entries
- `adminusers.json` - Admin user accounts
- `appsettings.json` - Application settings
- And others...

**Storage Benefits:**
- ✅ **Simple** - No database setup required
- ✅ **Portable** - Entire config can be copied/moved
- ✅ **Version Control** - Files can be tracked in Git
- ✅ **Human Readable** - Easy to inspect and edit
- ✅ **Backup Friendly** - Simple file copy for backup
- ✅ **Self-Contained** - No external dependencies

### Repository Abstraction Layer

**Purpose:** Create abstraction without changing storage mechanism

```
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                         │
│  (ResourceTypeService, ResourceLocationService, etc.)   │
└─────────────────┬───────────────────────────────────────┘
                  │ Uses
                  ▼
┌─────────────────────────────────────────────────────────┐
│              Repository Interfaces                       │
│  IConfigurationRepository<T>, ISiteConfigurationRepo    │
└─────────────────┬───────────────────────────────────────┘
                  │ Implemented by
                  ▼
┌─────────────────────────────────────────────────────────┐
│          Default: JSON File Repository                   │
│     (Reads/Writes to /settings/*.json files)            │
│              ⚡ CURRENT BEHAVIOR ⚡                       │
└─────────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│            File System (/settings/*.json)                │
└─────────────────────────────────────────────────────────┘
```

**Key Point:** The abstraction layer is transparent - JSON files continue to work exactly as before.

### Future Storage Options (Optional)

**Why Consider Alternatives?**
- Improved concurrency for high-traffic scenarios
- Better query performance for large datasets
- Transactional integrity for complex updates
- Still embedded (no external database server)

**Potential Embedded Options:**

| Option | Pros | Cons | Migration Complexity |
|--------|------|------|---------------------|
| **JSON Files (Current)** | Simple, portable, readable | No transactions, file locking issues | N/A - Current |
| **SQLite** | Embedded DB, ACID transactions, SQL queries | Binary format, requires migrations | Medium |
| **LiteDB** | Document DB, .NET native, NoSQL | Less mature than SQLite | Low |

**Migration Decision Tree:**

```
Is current JSON storage working well?
│
├─ YES → Keep JSON files (no change needed)
│
└─ NO (performance/concurrency issues)
   │
   ├─ Need SQL queries? → SQLite
   │
   └─ Prefer document model? → LiteDB
```

### Automatic Migration Path

**If alternative storage is chosen in the future:**

**Phase 1: Detection**
```csharp
// On application startup
var currentProvider = await storageDetectionService.DetectCurrentProviderAsync();
var configuredProvider = configuration["StorageOptions:Provider"];

if (currentProvider != configuredProvider)
{
    // Migration needed
}
```

**Phase 2: Backup**
```csharp
// Create timestamped backup
var backupPath = await dataMigrationService.CreateBackupAsync();
// Result: /backups/settings-backup-20251015-143022.zip
```

**Phase 3: Migration**
```csharp
var result = await dataMigrationService.MigrateAsync(
    fromProvider: "FileSystem",
    toProvider: "SQLite",
    createBackup: true);

if (result.Success)
{
    logger.LogInformation("Migrated {Count} items to {Provider}", 
        result.ItemsMigrated, toProvider);
}
```

**Phase 4: Validation**
```csharp
// Verify all data migrated correctly
var validationResult = await dataMigrationService.ValidateMigrationAsync();
```

**Phase 5: Rollback (if needed)**
```csharp
// If migration fails, automatic rollback to JSON files
await dataMigrationService.RestoreBackupAsync(backupPath);
```

### Migration Implementation Example

```csharp
namespace AzureNamingTool.Services
{
    public class DataMigrationService : IDataMigrationService
    {
        private readonly ILogger<DataMigrationService> _logger;
        private readonly IConfiguration _configuration;

        public async Task<MigrationResult> MigrateAsync(
            string fromProvider, 
            string toProvider, 
            bool createBackup = true)
        {
            var errors = new List<string>();
            var itemsMigrated = 0;
            string? backupPath = null;

            try
            {
                _logger.LogInformation(
                    "Starting migration from {From} to {To}", 
                    fromProvider, toProvider);

                // Step 1: Create backup
                if (createBackup)
                {
                    backupPath = await CreateBackupAsync();
                    _logger.LogInformation("Backup created: {Path}", backupPath);
                }

                // Step 2: Read all data from source
                var sourceData = await ReadAllDataFromProviderAsync(fromProvider);
                
                // Step 3: Write all data to destination
                itemsMigrated = await WriteAllDataToProviderAsync(toProvider, sourceData);

                // Step 4: Validate migration
                var isValid = await ValidateMigrationAsync(fromProvider, toProvider);
                
                if (!isValid)
                {
                    throw new InvalidOperationException("Migration validation failed");
                }

                _logger.LogInformation(
                    "Migration completed successfully. Items migrated: {Count}", 
                    itemsMigrated);

                return new MigrationResult(
                    Success: true,
                    Message: "Migration completed successfully",
                    ItemsMigrated: itemsMigrated,
                    BackupPath: backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed");
                
                // Attempt rollback
                if (backupPath != null)
                {
                    try
                    {
                        await RestoreBackupAsync(backupPath);
                        _logger.LogInformation("Rollback successful");
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Rollback failed");
                        errors.Add($"Rollback failed: {rollbackEx.Message}");
                    }
                }

                errors.Add(ex.Message);
                
                return new MigrationResult(
                    Success: false,
                    Message: "Migration failed - see errors",
                    ItemsMigrated: itemsMigrated,
                    BackupPath: backupPath,
                    Errors: errors);
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            var settingsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "settings");
            
            var backupDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "backups");
            
            Directory.CreateDirectory(backupDir);
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var backupFile = Path.Combine(
                backupDir, 
                $"settings-backup-{timestamp}.zip");

            // Create zip of entire settings folder
            System.IO.Compression.ZipFile.CreateFromDirectory(
                settingsPath, 
                backupFile);

            _logger.LogInformation(
                "Created backup: {BackupFile}, Size: {Size} bytes", 
                backupFile, 
                new FileInfo(backupFile).Length);

            return backupFile;
        }

        public async Task RestoreBackupAsync(string backupPath)
        {
            var settingsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "settings");

            // Clear current settings
            if (Directory.Exists(settingsPath))
            {
                Directory.Delete(settingsPath, recursive: true);
            }

            // Extract backup
            System.IO.Compression.ZipFile.ExtractToDirectory(
                backupPath, 
                settingsPath);

            _logger.LogInformation("Restored backup from: {Path}", backupPath);
        }
    }
}
```

### Storage Configuration

**appsettings.json:**

```json
{
  "StorageOptions": {
    "Provider": "FileSystem",
    "FileSystemPath": "settings",
    "AutoMigrateData": true,
    "BackupBeforeMigration": true,
    "BackupRetentionDays": 30,
    
    "SQLite": {
      "DatabasePath": "data/azurenamingtools.db",
      "ConnectionString": "Data Source=data/azurenamingtool.db;Cache=Shared"
    },
    
    "LiteDB": {
      "DatabasePath": "data/azurenamingtools.litedb",
      "ConnectionString": "Filename=data/azurenamingtool.litedb;Connection=Shared"
    }
  }
}
```

### Migration Checklist

**Before Implementing Alternative Storage:**

- [ ] Confirm JSON file storage is insufficient
- [ ] Choose alternative storage (SQLite or LiteDB)
- [ ] Design database schema
- [ ] Implement alternative repository
- [ ] Create migration service
- [ ] Test migration with sample data
- [ ] Create backup/restore functionality
- [ ] Test rollback scenarios
- [ ] Document migration process
- [ ] Provide user guidance for migration

**User Communication:**

> 📢 **Important:** Any future migration to alternative storage will be:
> - **Optional** - JSON files remain the default
> - **Automatic** - Migration handled by application
> - **Reversible** - Backups created automatically
> - **Validated** - Data integrity checked
> - **Transparent** - No API changes required

### Storage Provider Registration

**Program.cs Configuration:**

```csharp
// Register storage based on configuration
var storageProvider = builder.Configuration["StorageOptions:Provider"] ?? "FileSystem";

switch (storageProvider)
{
    case "FileSystem":
    default:
        // Default: JSON file-based storage
        builder.Services.AddScoped(
            typeof(IConfigurationRepository<>), 
            typeof(JsonFileConfigurationRepository<>));
        builder.Services.AddScoped<ISiteConfigurationRepository, JsonSiteConfigurationRepository>();
        builder.Services.AddSingleton<IStorageProvider, FileSystemStorageProvider>();
        break;
    
    case "SQLite":
        // Future: SQLite storage (after migration)
        builder.Services.AddScoped(
            typeof(IConfigurationRepository<>), 
            typeof(SQLiteConfigurationRepository<>));
        builder.Services.AddScoped<ISiteConfigurationRepository, SQLiteSiteConfigurationRepository>();
        builder.Services.AddSingleton<IStorageProvider, SQLiteStorageProvider>();
        break;
    
    case "LiteDB":
        // Future: LiteDB storage (after migration)
        builder.Services.AddScoped(
            typeof(IConfigurationRepository<>), 
            typeof(LiteDbConfigurationRepository<>));
        builder.Services.AddScoped<ISiteConfigurationRepository, LiteDbSiteConfigurationRepository>();
        builder.Services.AddSingleton<IStorageProvider, LiteDbStorageProvider>();
        break;
}

// Migration service (for future use)
builder.Services.AddScoped<IDataMigrationService, DataMigrationService>();
```

**Startup Migration Check:**

```csharp
// In Program.cs after app is built
var migrationService = app.Services.GetRequiredService<IDataMigrationService>();
var configuredProvider = builder.Configuration["StorageOptions:Provider"];
var autoMigrate = builder.Configuration.GetValue<bool>("StorageOptions:AutoMigrateData");

if (autoMigrate && await migrationService.IsMigrationNeededAsync("FileSystem", configuredProvider))
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Data migration needed from FileSystem to {Provider}", configuredProvider);
    
    // Could prompt user or auto-migrate based on config
    var result = await migrationService.MigrateAsync("FileSystem", configuredProvider);
    
    if (result.Success)
    {
        logger.LogInformation("Migration completed: {Message}", result.Message);
    }
    else
    {
        logger.LogError("Migration failed: {Message}", result.Message);
        // Application could fall back to FileSystem provider
    }
}
```

---

## API Compatibility Strategy

### Critical: Maintaining Backward Compatibility

**Non-Negotiable Rules:**

1. ✅ **All existing routes must remain unchanged**
   - No route pattern changes
   - No parameter renames
   - No HTTP method changes

2. ✅ **All request/response models must remain compatible**
   - No property renames
   - No type changes
   - Can add optional properties
   - Cannot remove properties

3. ✅ **Authentication mechanism unchanged**
   - API Key authentication remains
   - Same header name
   - Same validation logic

4. ✅ **Error responses maintain same format**
   - Same HTTP status codes
   - Same error message structure

### Compatibility Testing

**Before Each Phase:**
- [ ] Export current API schema
- [ ] Document all endpoints
- [ ] Create baseline integration tests

**After Each Phase:**
- [ ] Run full integration test suite
- [ ] Compare API responses with baseline
- [ ] Verify no breaking changes
- [ ] Update documentation

**API Test Checklist:**

```
Endpoint: POST /api/ResourceNamingRequests/RequestName
✅ Route accessible
✅ Accepts same request format
✅ Returns same response format
✅ Authentication works
✅ Error handling unchanged
✅ Performance acceptable

Endpoint: GET /api/ResourceTypes
✅ Route accessible
✅ Query parameters work
✅ Returns same response format
✅ Authentication works
✅ Error handling unchanged
✅ Performance acceptable

... (repeat for all endpoints)
```

---

## Testing & Validation Checklist

### Pre-Deployment Validation

**Unit Tests:**
- [ ] All unit tests passing
- [ ] Code coverage ≥ 70%
- [ ] No skipped tests
- [ ] All critical paths tested

**Integration Tests:**
- [ ] All API endpoints tested
- [ ] Authentication scenarios tested
- [ ] Error scenarios tested
- [ ] Performance benchmarks met

**Manual Testing:**
- [ ] Admin UI fully functional
- [ ] Name generation works correctly
- [ ] All configuration pages work
- [ ] File uploads/downloads work
- [ ] Import/export functionality works

**API Compatibility:**
- [ ] All existing endpoints respond correctly
- [ ] Request/response formats unchanged
- [ ] Authentication unchanged
- [ ] Error responses consistent
- [ ] Performance comparable or better

**Non-Functional:**
- [ ] Application starts without errors
- [ ] No memory leaks detected
- [ ] Logging works correctly
- [ ] Health checks respond
- [ ] Security scan passes

---

## Progress Tracking

### Overall Progress

| Phase | Status | Start Date | End Date | % Complete |
|-------|--------|------------|----------|------------|
| Phase 1: Foundation | ⬜ Not Started | | | 0% |
| Phase 2: Services | ⬜ Not Started | | | 0% |
| Phase 3: Controllers | ⬜ Not Started | | | 0% |
| Phase 4: Configuration | ⬜ Not Started | | | 0% |
| Phase 5: Testing | ⬜ Not Started | | | 0% |
| Phase 6: Enhancements | ⬜ Not Started | | | 0% |

**Legend:**
- ⬜ Not Started
- 🔄 In Progress
- ✅ Complete
- ⚠️ Blocked
- ❌ Cancelled

---

### Phase 1 Progress Detail

| Task | Status | Notes |
|------|--------|-------|
| Create service interfaces | ⬜ | |
| Create repository interfaces | ⬜ | |
| Implement JSON repositories | ⬜ | |
| Create cache service | ⬜ | |
| Unit tests for infrastructure | ⬜ | |

---

### Phase 2 Progress Detail

| Service | Status | Dependencies | Notes |
|---------|--------|--------------|-------|
| ResourceDelimiterService | ⬜ | None | |
| ResourceLocationService | ⬜ | None | |
| ResourceEnvironmentService | ⬜ | None | |
| ResourceOrgService | ⬜ | None | |
| ResourceProjAppSvcService | ⬜ | None | |
| ResourceUnitDeptService | ⬜ | None | |
| ResourceFunctionService | ⬜ | None | |
| CustomComponentService | ⬜ | Low | |
| AdminUserService | ⬜ | Low | |
| ResourceComponentService | ⬜ | Medium | |
| ResourceTypeService | ⬜ | Medium | |
| AdminLogService | ⬜ | Medium | |
| GeneratedNamesService | ⬜ | High | |
| ResourceNamingRequestService | ⬜ | High | |
| AdminService | ⬜ | High | |
| PolicyService | ⬜ | High | |
| ImportExportService | ⬜ | High | |

---

### Phase 3 Progress Detail

| Controller | Status | Endpoints | Notes |
|------------|--------|-----------|-------|
| ResourceTypesController | ⬜ | 5 | |
| ResourceLocationsController | ⬜ | 5 | |
| ResourceEnvironmentsController | ⬜ | 5 | |
| ResourceOrgsController | ⬜ | 5 | |
| ResourceProjAppSvcsController | ⬜ | 5 | |
| ResourceUnitDeptsController | ⬜ | 5 | |
| ResourceFunctionsController | ⬜ | 5 | |
| ResourceDelimitersController | ⬜ | 4 | |
| ResourceComponentsController | ⬜ | 3 | |
| CustomComponentsController | ⬜ | 5 | |
| ResourceNamingRequestsController | ⬜ | 2 | Critical |
| AdminController | ⬜ | 8 | |
| PolicyController | ⬜ | 4 | |
| ImportExportController | ⬜ | 4 | |

---

### Risks & Issues Log

| ID | Risk/Issue | Severity | Status | Mitigation |
|----|------------|----------|--------|------------|
| R1 | Breaking API changes | 🔴 High | Open | Comprehensive testing, gradual migration |
| R2 | Performance degradation | 🟡 Medium | Open | Benchmark each phase |
| R3 | Data loss during migration | 🔴 High | Open | Backup before each phase |
| R4 | Circular dependencies in DI | 🟡 Medium | Open | Careful dependency design |

---

### Decision Log

| Date | Decision | Rationale | Impact |
|------|----------|-----------|--------|
| 2025-10-15 | Keep JSON file storage as default | User requirement, simplicity, portability | High - Maintains core architecture |
| 2025-10-15 | Create repository abstraction layer | Enable testing, future flexibility | Medium - Enables future options |
| 2025-10-15 | Support automatic migration if storage changes | Protect user data, ensure seamless transition | High - User experience |
| 2025-10-15 | Use Scoped lifetime for most services | Per-request isolation for API/Blazor | Medium |
| 2025-10-15 | Keep AdminLogService separate from ILogger | Audit trail vs operational logging | Low |
| 2025-10-15 | Use xUnit for testing | Team familiarity | Low |
| 2025-10-15 | All storage alternatives must be embedded | No external database dependencies | High - Maintains simplicity |

---

## Notes & Best Practices

### Data Storage Guidelines

**Critical Rules:**
- 🔴 **JSON files are sacred** - Never delete or corrupt existing configuration files
- 🔴 **Always backup before changes** - Create backup before any file modifications
- 🔴 **Preserve file format** - Maintain exact JSON structure when writing
- 🔴 **Test with real data** - Use actual configuration files for testing
- 🟡 **Handle file locks** - Implement retry logic for file access
- 🟡 **Validate after write** - Read back and validate after writing files

**File Operation Best Practices:**

```csharp
// ✅ GOOD: Atomic write with backup
public async Task SaveDataAsync<T>(string fileName, T data)
{
    var filePath = Path.Combine(_settingsPath, fileName);
    var backupPath = $"{filePath}.backup";
    
    try
    {
        // Create backup if file exists
        if (File.Exists(filePath))
        {
            File.Copy(filePath, backupPath, overwrite: true);
        }
        
        // Write to temp file first
        var tempPath = $"{filePath}.tmp";
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(tempPath, json);
        
        // Atomic move
        File.Move(tempPath, filePath, overwrite: true);
        
        // Delete backup if successful
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }
    }
    catch (Exception)
    {
        // Restore from backup if write failed
        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, filePath, overwrite: true);
        }
        throw;
    }
}

// ❌ BAD: Direct overwrite (risky)
public async Task SaveDataBad<T>(string fileName, T data)
{
    var filePath = Path.Combine(_settingsPath, fileName);
    var json = JsonSerializer.Serialize(data);
    await File.WriteAllTextAsync(filePath, json); // Could corrupt on failure!
}
```

**Repository Implementation Guidelines:**
- Encapsulate all file I/O in repository layer
- Use async/await for all file operations
- Implement proper error handling and logging
- Cache frequently accessed data
- Validate data before writing
- Use file locks for concurrent access protection

### Service Lifetime Guidelines

- **Singleton**: Stateless, thread-safe, lightweight (e.g., `ICacheService`)
- **Scoped**: Per-request services, most business services
- **Transient**: Very lightweight utilities, avoid for expensive objects

### Testing Best Practices

- Arrange-Act-Assert pattern
- One assertion per test (when possible)
- Meaningful test names: `MethodName_Scenario_ExpectedResult`
- Use FluentAssertions for readability
- Mock only interfaces, not concrete classes

### Migration Tips

1. **Start small**: Begin with services that have no dependencies
2. **Test frequently**: After each service conversion, run tests
3. **Keep wrappers**: Maintain static wrappers temporarily during migration
4. **Document changes**: Update this guide as you progress
5. **Backup regularly**: Before each major change, backup settings folder
6. **Validate data**: After repository changes, verify all JSON files load correctly
7. **Test file operations**: Ensure write operations don't corrupt existing files
8. **Monitor file system**: Check disk space and permissions during testing

### Common Pitfalls to Avoid

- ❌ Changing API contracts
- ❌ Introducing circular dependencies
- ❌ Forgetting to dispose of resources
- ❌ Using wrong service lifetimes
- ❌ Not validating null parameters
- ❌ Catching exceptions without logging

---

## Resources

### Documentation Links

- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

### Tools

- Visual Studio 2022
- ReSharper (optional)
- Postman (API testing)
- dotCover (code coverage)

---

## Appendix

### A. Service Interface Template

```csharp
namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Service for managing [entity name]
    /// </summary>
    public interface I[ServiceName]
    {
        /// <summary>
        /// Get all items
        /// </summary>
        Task<ServiceResponse> GetItems(bool admin = true);
        
        /// <summary>
        /// Get item by ID
        /// </summary>
        Task<ServiceResponse> GetItem(int id);
        
        /// <summary>
        /// Create or update item
        /// </summary>
        Task<ServiceResponse> PostItem([Entity] item);
        
        /// <summary>
        /// Delete item
        /// </summary>
        Task<ServiceResponse> DeleteItem(int id);
    }
}
```

### B. Service Implementation Template

```csharp
namespace AzureNamingTool.Services
{
    public class [ServiceName] : I[ServiceName]
    {
        private readonly IConfigurationRepository<[Entity]> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<[ServiceName]> _logger;

        public [ServiceName](
            IConfigurationRepository<[Entity]> repository,
            ICacheService cacheService,
            ILogger<[ServiceName]> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResponse> GetItems(bool admin = true)
        {
            try
            {
                // Implementation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetItems");
                return new ServiceResponse 
                { 
                    Success = false, 
                    ResponseMessage = ex.Message 
                };
            }
        }
    }
}
```

### C. Unit Test Template

```csharp
namespace AzureNamingTool.UnitTests.Services
{
    public class [ServiceName]Tests
    {
        private readonly Mock<IConfigurationRepository<[Entity]>> _mockRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<[ServiceName]>> _mockLogger;
        private readonly [ServiceName] _sut;

        public [ServiceName]Tests()
        {
            _mockRepository = new Mock<IConfigurationRepository<[Entity]>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<[ServiceName]>>();
            
            _sut = new [ServiceName](
                _mockRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task MethodName_Scenario_ExpectedBehavior()
        {
            // Arrange
            
            // Act
            
            // Assert
        }
    }
}
```

---

## Conclusion

This modernization plan provides a structured approach to updating the Azure Naming Tool to follow .NET best practices while maintaining complete API compatibility. Follow each phase sequentially, test thoroughly, and update this document as you progress.

**Remember:** The goal is gradual, safe improvement—not a complete rewrite. Take your time, test frequently, and maintain backward compatibility at all times.

Good luck! 🚀

---

**Document Version:** 1.0  
**Last Updated:** October 15, 2025  
**Next Review:** After Phase 1 Completion
