Azure Naming Tool - Release Notes v5.0.0

Overview
--------
v5.0.0 is a major UX and stability release focused on modernizing the Configuration experience and preparing the codebase for future API enhancements (v2). Highlights include a complete drag-and-drop rework for configuration lists, API versioning groundwork (v1/v2), and several data integrity fixes to prevent prior ordering issues.

Highlights (High-level)
-----------------------
- Modern drag-and-drop sorting across all Configuration sections (Components, Environments, Functions, Locations, Orgs, Projects/Apps/Services, Units/Depts, Custom Components)
  - Replaces up/down arrow controls with intuitive drag handles and visual feedback
  - Immediate persistence to configured storage (JSON or SQLite)
  - Robust client/server sync to prevent data corruption
- Data integrity fixes
  - Removed improper ID reassignment during list reorders
  - Fixed sort-order regression where Enabled flag previously affected ordering
  - Added dedicated UpdateSortOrder APIs to perform direct, normalization-free saves
- Blazor rendering stability improvements
  - Forced container render-key strategy to ensure UI reflects data changes instantly
  - Re-initialization of JavaScript handlers after DOM refresh to allow repeated drag operations
- API Versioning groundwork
  - Added support for API versioning (v1 and v2) with separate Swagger docs
  - Introduced v2 controller examples and structured response models to enable future breaking improvements while keeping v1 stable
- Developer & QA improvements
  - Better logging for reorder and persistence operations
  - Transactional SQLite saves and JSON write operations with cache invalidation
  - Unit tests and UI tests scaffolded for drag-and-drop behaviors (where applicable)

Upgrade Notes
-------------
- If you use the FileSystem/JSON storage provider, updated JSON files will be written directly when items are reordered. Ensure your deployment user has write permissions to the `settings/` folder.
- If you use SQLite, saves are transactional. Backups are recommended before upgrading production environments.
- No breaking changes are expected for existing APIs under the default v1 endpoints. v2 endpoints are opt-in.

How to test the key scenarios
----------------------------
1. Start the application and sign in as an admin.
2. Open Configuration → Components (or any sortable section).
3. Drag an item to a new position; observe visual feedback during drag and a success toast after drop.
4. Confirm the new order is persisted by refreshing the page and verifying items remain in the new order.
5. Confirm same behavior for Environments, Functions, Locations, Orgs, Projects/Apps/Services, Units/Depts, and Custom Components.

Files/Areas Changed (developer-focused)
--------------------------------------
- UI/JS/CSS
  - `src/wwwroot/js/drag-drop-sort.js` (new)
  - `src/wwwroot/css/modern-components.css` (updated)
  - `src/Components/Pages/Configuration.razor` (major changes)
- Services
  - `src/Services/ResourceComponentService.cs` (new UpdateSortOrderAsync + bug fixes)
  - `src/Services/ResourceEnvironmentService.cs` (new UpdateSortOrderAsync)
  - Service interfaces updated to expose UpdateSortOrderAsync
- Repositories
  - `src/Repositories/Implementation/FileSystem/JsonFileConfigurationRepository.cs` (SaveAllAsync updates)
  - `src/Repositories/SQLiteConfigurationRepository.cs` (SaveAllAsync transactional behavior)
- API
  - V2 controller examples added under `src/Controllers/V2/` with updated response models

Notes & Known Issues
--------------------
- The render-key re-render strategy is intentionally aggressive to guarantee deterministic UI updates; this may be revisited for optimization in future minor releases.
- Custom components use their own per-parent table; drag-and-drop initialization is applied per custom-component container.

Acknowledgements
----------------
Thanks to the contributors and QA engineers who helped reproduce ordering bugs, validate persistence across storage providers, and polish the UX.

Contact
-------
For issues, open a GitHub issue under the repository and tag with `area/configuration` and `severity:high` if the problem involves data corruption or persistence.
