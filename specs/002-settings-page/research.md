# Research: Global Settings Page

**Feature**: 002-settings-page
**Date**: 2026-02-20

## R1: WinUI3 Theme Switching at Runtime

**Decision**: Use `FrameworkElement.RequestedTheme` on the root content element of `MainWindow` to switch between `ElementTheme.Default` (system), `ElementTheme.Light`, and `ElementTheme.Dark`.

**Rationale**: WinUI3 does not support `Application.RequestedTheme` changes after app launch — it is read-only after initialization. The supported approach is setting `RequestedTheme` on the root `FrameworkElement` (the `Frame` in our case). This propagates to all child elements and works with the existing `{ThemeResource}` bindings and Mica backdrop. The change is immediate with no restart required.

**Alternatives considered**:
- `Application.Current.RequestedTheme`: Read-only after launch, cannot be changed at runtime.
- Per-page `RequestedTheme`: Would require setting on every page individually, fragile and error-prone.

**Implementation**: `MainWindow` will expose a method `SetTheme(ElementTheme theme)` that sets `RootFrame.RequestedTheme`. The `ISettingsService` calls this at startup and whenever the theme setting changes.

## R2: Settings Persistence Strategy

**Decision**: Store settings in a single JSON file at `settings/app-settings.json` using the existing `IDataService`.

**Rationale**: Follows the established data persistence pattern. The `IDataService` already handles JSON serialization with `System.Text.Json`, camelCase naming, thread-safe `SemaphoreSlim` locking, and folder auto-creation. Reusing it avoids duplication and ensures consistency.

**Alternatives considered**:
- Windows Registry: Not aligned with local-first JSON philosophy; harder to export/backup.
- Separate settings file outside `IDataService`: Unnecessary complexity, breaks consistency.

**Implementation**: `SettingsService.LoadAsync()` reads via `IDataService.ReadAsync<AppSettings>("settings", "app-settings")`. Save via `IDataService.SaveAsync`. Default values returned when file doesn't exist.

## R3: Data Export via ZIP

**Decision**: Use `System.IO.Compression.ZipFile.CreateFromDirectory()` to archive the entire `%LOCALAPPDATA%/AquaSync/` directory.

**Rationale**: Built-in .NET API, no external dependencies. Archives the full data folder including all JSON files (aquariums, settings, future data types) and gallery photos. Preserves folder structure for potential future import support.

**Alternatives considered**:
- Selective file enumeration: More complex, risk of missing new data types added later.
- Third-party ZIP library (SharpZipLib): Violates Principle V (Minimal Dependencies).

**Implementation**: Export runs on a background thread via `Task.Run`. User selects save location via `FileSavePicker`. Progress is shown via an indeterminate `ProgressRing` (ZipFile API doesn't support granular progress). The settings file itself is included in the export.

## R4: Data Folder Migration

**Decision**: Copy all files/folders from old location to new location, verify copy, then update `DataService` root path and delete old files. Use a confirmation `ContentDialog` before starting.

**Rationale**: Copy-then-delete is safer than move — if copying fails midway, original data remains intact. A confirmation dialog prevents accidental data relocation.

**Alternatives considered**:
- `Directory.Move()`: Atomic on same volume but fails across volumes; does not provide rollback safety.
- Symlink/junction: Too complex, platform-specific edge cases.

**Implementation**: `SettingsService` handles the migration. `DataService` needs a new method to update its internal `_rootPath`. The settings file that records the custom path must live in a **fixed location** (not inside the movable data folder) so the app can find it on next launch. Store a `data-folder-redirect.json` at the original `%LOCALAPPDATA%/AquaSync/` path containing only the custom folder path. On startup, `DataService` checks for this redirect file first.

## R5: Settings Navigation from AquariumSelectorPage

**Decision**: Add a gear icon (`&#xE713;`) button in the top-right corner of `AquariumSelectorPage`. Clicking it navigates the root `MainWindow.ContentFrame` to `SettingsPage` directly (not through `ShellPage`).

**Rationale**: The AquariumSelectorPage has no sidebar, so Settings must be a standalone button. Navigating via the root frame keeps the flow simple — the user can press Back to return to the selector. When accessed from AquariumSelectorPage, the SettingsViewModel operates in global-only mode (`HasAquarium = false`), hiding the aquarium-scoped profile section.

**Alternatives considered**:
- Flyout/dialog overlay: Would limit space for settings content and break consistency with ShellPage version.
- Separate SettingsWindow: WinUI3 multi-window is complex and fragile; unnecessary for this use case.

**Implementation**: `AquariumSelectorPage` adds a `Button` with gear icon. Click handler navigates `MainWindow.ContentFrame` to `SettingsPage`. Back navigation returns to `AquariumSelectorPage`. `SettingsViewModel.LoadFromContext()` already handles `HasAquarium = false` to hide aquarium-scoped content.

## R6: ISettingsService Responsibilities

**Decision**: `ISettingsService` is a singleton that:
1. Loads `AppSettings` from disk at app startup
2. Provides current settings values to consumers
3. Saves settings changes immediately
4. Applies theme at startup and on change
5. Provides default UOM values for aquarium profile creation

**Rationale**: Centralizing settings access in a service follows the existing DI pattern (like `IAquariumService`). Singleton ensures all consumers see the same state. Loading at startup avoids async reads during page navigation.

**Alternatives considered**:
- Passing settings directly to ViewModels as parameters: Fragile, doesn't support live updates.
- Static global state: Violates DI pattern established in the codebase.
