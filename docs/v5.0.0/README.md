## Overview

The Azure Naming Tool documentation is hosted in the GitHub Wiki. Please use the link below to view documentation.

[GitHub Wiki - Documentation](https://github.com/mspnp/AzureNamingTool/wiki)

---

## Technical Documentation

### Release Documentation

| Document | Description | Audience |
|----------|-------------|----------|
| [Release Notes v5.0.0](./RELEASE_NOTES_v5.0.0.md) | Major features and breaking changes | All Users |
| [Migration Guide](./V5.0.0_MIGRATION_GUIDE.md) | Complete v5.0.0 upgrade guide with backup/restart procedures | Administrators, DevOps |

### Azure Tenant Name Validation (v5.0.0+)

Comprehensive documentation for the Azure tenant name validation feature:

#### Development Documentation (`development/`)

| Document | Description | Audience |
|----------|-------------|----------|
| [Implementation Plan](./development/AZURE_NAME_VALIDATION_PLAN.md) | 8-phase roadmap and technical architecture | Developers, Architects |
| [Administrator Guide](./development/AZURE_VALIDATION_ADMIN_GUIDE.md) | Setup, configuration, and maintenance procedures | Administrators, Operations |
| [API Guide](./development/AZURE_VALIDATION_API_GUIDE.md) | V2 API documentation with code examples | Developers, API Consumers |
| [Feature Complete Summary](./development/AZURE_VALIDATION_FEATURE_COMPLETE.md) | Complete feature implementation details | Developers |
| [Phase 5 UI Integration](./development/PHASE5_UI_INTEGRATION_SUMMARY.md) | UI integration implementation details | Developers |

#### Testing Documentation (`testing/`)

| Document | Description | Audience |
|----------|-------------|----------|
| [Testing Guide](./testing/AZURE_VALIDATION_TESTING_GUIDE.md) | Test suites and automated testing scripts | QA Teams, Developers |
| [Security Guide](./testing/AZURE_VALIDATION_SECURITY_GUIDE.md) | Authentication, RBAC, and security best practices | Security Teams, DevOps |
| [Migration Fix](./testing/AZURE_VALIDATION_MIGRATION_FIX.md) | Migration-related fixes and solutions | Developers, Support |
| [Backup & Restore](./testing/BACKUP_RESTORE.md) | Data backup and recovery procedures | Administrators, Operations |

### API Documentation (`wiki/`)

| Document | Description | Audience |
|----------|-------------|----------|
| [API V1 Wiki](./wiki/API_V1_WIKI.md) | Version 1 API documentation | Developers, API Consumers |
| [API V2 Wiki](./wiki/API_V2_WIKI.md) | Version 2 API documentation (recommended) | Developers, API Consumers |
| [Azure Validation Wiki](./wiki/AZURE_VALIDATION_WIKI.md) | Azure validation feature documentation | All Users |

### Other Development Documentation (`development/`)

| Document | Description | Audience |
|----------|-------------|----------|
| [Bulk API Operations](./development/API_BULK_OPERATION_IMPLEMENTATION_PLAN.md) | Bulk name generation implementation | Developers |
| [API Migration Plan](./development/API_MIGRATION_PLAN.md) | V1 to V2 API migration guide | Developers, Architects |
| [Dashboard Implementation](./development/DASHBOARD_IMPLEMENTATION_PLAN.md) | Dashboard features and implementation | Developers |
| [Modernization Plan](./development/MODERNIZATION_PLAN.md) | .NET 8 and Blazor modernization | Developers, Architects |
| [Design Implementation](./development/DESIGN_IMPLEMENTATION_PLAN.md) | UI/UX design patterns | Developers, Designers |
| [Migration Guidance](./development/MIGRATIONGUIDANCE_PLAN.md) | Tool migration procedures | Developers, Administrators |

---

## Document Organization

```
docs/v5.0.0/
├── README.md                          # This file
├── RELEASE_NOTES_v5.0.0.md           # Release notes
├── V5.0.0_MIGRATION_GUIDE.md         # Migration guide
├── development/                       # Development & implementation docs
│   ├── API_BULK_OPERATION_IMPLEMENTATION_PLAN.md
│   ├── API_MIGRATION_PLAN.md
│   ├── AZURE_NAME_VALIDATION_PLAN.md
│   ├── AZURE_VALIDATION_ADMIN_GUIDE.md
│   ├── AZURE_VALIDATION_API_GUIDE.md
│   ├── AZURE_VALIDATION_FEATURE_COMPLETE.md
│   ├── DASHBOARD_IMPLEMENTATION_PLAN.md
│   ├── DESIGN_IMPLEMENTATION_PLAN.md
│   ├── MIGRATIONGUIDANCE_PLAN.md
│   ├── MODERNIZATION_PLAN.md
│   └── PHASE5_UI_INTEGRATION_SUMMARY.md
├── testing/                          # Testing & operational docs
│   ├── AZURE_VALIDATION_MIGRATION_FIX.md
│   ├── AZURE_VALIDATION_SECURITY_GUIDE.md
│   ├── AZURE_VALIDATION_TESTING_GUIDE.md
│   └── BACKUP_RESTORE.md
└── wiki/                             # Wiki-style documentation
    ├── API_V1_WIKI.md
    ├── API_V2_WIKI.md
    └── AZURE_VALIDATION_WIKI.md
```
