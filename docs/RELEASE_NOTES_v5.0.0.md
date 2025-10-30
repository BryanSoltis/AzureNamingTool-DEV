# Azure Naming Tool - Release Notes v5.0.0

## Overview
Version 5.0.0 is a major release featuring Azure Tenant Name Validation, modernized UI/UX, improved configuration management, and API versioning support.

## 🎯 Major Features

### Azure Tenant Name Validation (NEW)
- **Validate generated names against your Azure tenant** before deployment
- Prevent naming conflicts by checking if resource names already exist
- Support for both **Managed Identity** (recommended) and **Service Principal** authentication
- Flexible conflict resolution strategies:
  - **NotifyOnly** - Warn about conflicts but allow generation (default)
  - **AutoIncrement** - Automatically append incremented suffix (e.g., -001, -002)
  - **Fail** - Block generation if name exists
  - **SuffixRandom** - Add random suffix to resolve conflicts
- **Performance caching** to minimize Azure API calls
- **Scoped validation** - configure specific subscription(s) to check
- Integrated into Site Settings for easy configuration

### Modern UI/UX Improvements
- **Consistent card-based design** across all Admin tabs
- Boxed styling with hover effects for all settings
- Improved visual hierarchy and spacing
- Optimized grouped settings (e.g., Site Navigation toggles)
- Modern, clean interface throughout the application

### Drag-and-Drop Configuration
- **Intuitive drag-and-drop sorting** for all configuration lists
- Replaces up/down arrow controls with drag handles
- Visual feedback during drag operations
- Immediate persistence to storage (JSON or SQLite)
- Supports: Components, Environments, Functions, Locations, Orgs, Projects/Apps/Services, Units/Depts, Custom Components

### API Versioning
- Support for API versioning (v1 and v2)
- Separate Swagger documentation for each version
- v1 endpoints remain stable; v2 enables future enhancements
- No breaking changes to existing v1 APIs

## 🔧 Improvements

### Data Integrity
- Fixed ID reassignment issues during list reorders
- Corrected sort-order behavior when Enabled flag changes
- Added dedicated UpdateSortOrder APIs for reliable persistence
- Transactional SQLite saves with proper cache invalidation

### Rendering Stability
- Improved Blazor component rendering with render-key strategy
- Better JavaScript handler initialization after DOM updates
- More reliable UI updates across all configuration sections

## 📋 Upgrade Notes

### Storage Permissions
- **FileSystem/JSON**: Ensure write permissions to `settings/` folder
- **SQLite**: Backups recommended before upgrading production environments

### Azure Validation (Optional)
- Enable in Admin → Site Settings → "Azure Tenant Name Validation"
- Configure authentication (Managed Identity recommended for Azure deployments)
- Set conflict resolution strategy based on your naming convention
- Test connection before saving configuration

### API Compatibility
- No breaking changes to v1 endpoints
- v2 endpoints are opt-in and experimental

## 🐛 Bug Fixes
- Fixed configuration list ordering persistence issues
- Resolved Enabled flag affecting sort order
- Improved client/server data synchronization
- Fixed spacing inconsistencies in grouped UI elements

## 📚 Documentation
For detailed feature guides, see:
- [Azure Validation Admin Guide](AZURE_VALIDATION_ADMIN_GUIDE.md)
- [Azure Validation API Guide](AZURE_VALIDATION_API_GUIDE.md)
- [Azure Validation Security Guide](AZURE_VALIDATION_SECURITY_GUIDE.md)

## 🙏 Acknowledgements
Thanks to all contributors and testers who helped validate features, identify bugs, and improve the user experience.

---
**For issues or questions**, please open a GitHub issue in the repository.
