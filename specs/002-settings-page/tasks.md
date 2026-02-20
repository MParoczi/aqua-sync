# Tasks: Global Settings Page

**Input**: Design documents from `/specs/002-settings-page/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not requested. No test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Models + Interfaces)

**Purpose**: Create new model types and service interfaces that all user stories depend on

- [x] T001 [P] Create `AppTheme` enum (`System`, `Light`, `Dark`) in `AquaSync.App/Models/AppTheme.cs`
- [x] T002 [P] Create `DataFolderRedirect` sealed record with `CustomDataFolderPath` property in `AquaSync.App/Models/DataFolderRedirect.cs`
- [x] T003 Create `AppSettings` sealed class with `DefaultVolumeUnit` (default `Liters`), `DefaultDimensionUnit` (default `Centimeters`), `Theme` (default `System`), and `DataFolderPath` (default `null`) properties in `AquaSync.App/Models/AppSettings.cs`
- [x] T004 Create `ISettingsService` interface with `Settings` property, `InitializeAsync`, `SaveAsync`, `ApplyTheme`, `ExportDataAsync`, and `MoveDataFolderAsync` methods in `AquaSync.App/Contracts/Services/ISettingsService.cs` — see `specs/002-settings-page/contracts/ISettingsService.md` for full contract
- [x] T005 [P] Add `SetDataFolderPath(string newPath)` method and `bool HasRedirectFallback` property to `IDataService` interface in `AquaSync.App/Contracts/Services/IDataService.cs` — see `specs/002-settings-page/contracts/IDataService-extension.md` for full contract

---

## Phase 2: Foundational (Service Layer + Entry Points)

**Purpose**: Core service infrastructure and navigation entry points that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 [P] Modify `DataService` in `AquaSync.App/Services/DataService.cs`: make `_rootPath` mutable (remove `readonly`), add redirect file check in constructor (read `data-folder-redirect.json` from default `%LOCALAPPDATA%/AquaSync/`, fall back to default if path invalid), expose `bool HasRedirectFallback` property (true when redirect file existed but pointed to an invalid path), implement `SetDataFolderPath` method — see `specs/002-settings-page/contracts/IDataService-extension.md`
- [ ] T007 [P] Create `SettingsService` sealed class in `AquaSync.App/Services/SettingsService.cs` implementing `ISettingsService`: inject `IDataService` and `MainWindow`, implement `InitializeAsync` (load from `settings/app-settings.json` via `IDataService`, return defaults if missing/corrupt), implement `SaveAsync` (persist via `IDataService`), stub `ApplyTheme`/`ExportDataAsync`/`MoveDataFolderAsync` for later phases. Use `ConfigureAwait(false)` on all awaits.
- [ ] T008 [P] Add `SetTheme(ElementTheme theme)` method to `MainWindow` in `AquaSync.App/Views/MainWindow.xaml.cs` — set `RootFrame.RequestedTheme` to the provided theme value
- [ ] T009 Register `ISettingsService` as singleton in DI container, call `InitializeAsync()` and `ApplyTheme()` in `OnLaunched` (before `mainWindow.Activate()`) in `AquaSync.App/App.xaml.cs`
- [ ] T010 [P] Add gear icon button (glyph `&#xE713;`) to `AquariumSelectorPage` header area in `AquaSync.App/Views/AquariumSelectorPage.xaml`, and add click handler that navigates `MainWindow.ContentFrame` to `SettingsPage` in `AquaSync.App/Views/AquariumSelectorPage.xaml.cs`
- [ ] T011 [P] Add back navigation support to `SettingsPage` for standalone mode (when accessed from `AquariumSelectorPage` without aquarium context): add a back button visible only when `HasAquarium` is false, navigate back to `AquariumSelectorPage` via root frame on click, in `AquaSync.App/Views/SettingsPage.xaml` and `AquaSync.App/Views/SettingsPage.xaml.cs`
- [ ] T012 Build and verify the app compiles and launches with settings infrastructure: `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64`

**Checkpoint**: Settings infrastructure is ready. App launches, loads default settings, gear icon navigates to Settings from AquariumSelectorPage with back navigation.

---

## Phase 3: User Story 1 — Set Default Unit Preferences (Priority: P1) MVP

**Goal**: Users can set default volume and dimension units in Settings, which pre-populate the aquarium creation form

**Independent Test**: Change defaults to Gallons/Inches in Settings, create a new aquarium profile, verify the unit dropdowns default to Gallons/Inches

### Implementation for User Story 1

- [ ] T013 [US1] Add global settings properties to `SettingsViewModel` in `AquaSync.App/ViewModels/SettingsViewModel.cs`: inject `ISettingsService`, add `SelectedVolumeUnit` and `SelectedDimensionUnit` properties (bound to `ISettingsService.Settings`), auto-save on change via `ISettingsService.SaveAsync()`, add `LoadGlobalSettings()` method called from `LoadFromContext()`
- [ ] T014 [US1] Replace the "General" placeholder section in `SettingsPage.xaml` with a "Default Units" section containing volume unit RadioButtons (Liters/Gallons) and dimension unit RadioButtons (Centimeters/Inches) bound to ViewModel properties, add selection changed handlers in `AquaSync.App/Views/SettingsPage.xaml` and `AquaSync.App/Views/SettingsPage.xaml.cs`
- [ ] T015 [P] [US1] Inject `ISettingsService` into `AquariumSelectorViewModel` constructor in `AquaSync.App/ViewModels/AquariumSelectorViewModel.cs`, update `ResetCreationForm()` to use `_settingsService.Settings.DefaultVolumeUnit` and `_settingsService.Settings.DefaultDimensionUnit` instead of hardcoded `true` for `IsVolumeLiters`/`IsDimensionCentimeters`

**Checkpoint**: Default UOM preferences work end-to-end. Settings persist across restarts. New aquarium profiles use the configured defaults.

---

## Phase 4: User Story 2 — Switch Application Theme (Priority: P2)

**Goal**: Users can switch between System/Light/Dark themes with immediate visual effect

**Independent Test**: Select "Always dark" in Settings, verify the entire app switches to dark mode immediately without restart

### Implementation for User Story 2

- [ ] T016 [US2] Implement `ApplyTheme()` in `SettingsService` in `AquaSync.App/Services/SettingsService.cs`: map `AppTheme.System`→`ElementTheme.Default`, `AppTheme.Light`→`ElementTheme.Light`, `AppTheme.Dark`→`ElementTheme.Dark`, call `MainWindow.SetTheme()` on the UI thread via `DispatcherQueue`
- [ ] T017 [US2] Add `SelectedTheme` property to `SettingsViewModel` in `AquaSync.App/ViewModels/SettingsViewModel.cs`: bind to `ISettingsService.Settings.Theme`, on change update settings, call `SaveAsync()` and `ApplyTheme()` immediately
- [ ] T018 [US2] Add "Theme" section UI to `SettingsPage.xaml` with three RadioButtons ("Follow system theme", "Always light", "Always dark") bound to ViewModel's `SelectedTheme` property, add selection changed handler in `AquaSync.App/Views/SettingsPage.xaml` and `AquaSync.App/Views/SettingsPage.xaml.cs`

**Checkpoint**: Theme switching works immediately. Selection persists across app restarts. "Follow system theme" tracks OS changes.

---

## Phase 5: User Story 3 — Export Application Data (Priority: P3)

**Goal**: Users can export all app data as a ZIP archive to a chosen location

**Independent Test**: Click export, choose a save location, verify the resulting ZIP contains all data files and photos from the data folder

### Implementation for User Story 3

- [ ] T019 [US3] Implement `ExportDataAsync` in `SettingsService` in `AquaSync.App/Services/SettingsService.cs`: use `System.IO.Compression.ZipFile.CreateFromDirectory()` to archive the entire data folder, run on background thread via `Task.Run`, check data folder is non-empty before export, throw `IOException` on failure
- [ ] T020 [US3] Add export command and state to `SettingsViewModel` in `AquaSync.App/ViewModels/SettingsViewModel.cs`: add `IsExporting` property, `ExportDataCommand` (AsyncRelayCommand), `OnExportDataAsync` handler that calls `ISettingsService.ExportDataAsync()`, show success notification or error InfoBar, disable export button during data folder move (`IsMovingData`)
- [ ] T021 [US3] Add "Data Export" section UI to `SettingsPage.xaml` with export button, `ProgressRing` bound to `IsExporting`, and add `FileSavePicker` handler in code-behind (`AquaSync.App/Views/SettingsPage.xaml.cs`) with default filename `AquaSync-Export-YYYY-MM-DD.zip` — handle empty data case with info message, handle picker cancellation gracefully

**Checkpoint**: Data export works. ZIP contains all data folder contents. Progress indicator shown during export. Error messages on failure.

---

## Phase 6: User Story 4 — Change Data Folder Location (Priority: P4)

**Goal**: Users can relocate the data folder to a different location with confirmation, progress, rollback on failure, and reset-to-default

**Independent Test**: Browse to a new folder, confirm, verify all data files moved, verify app continues working from new location

### Implementation for User Story 4

- [ ] T022 [US4] Implement `MoveDataFolderAsync` in `SettingsService` in `AquaSync.App/Services/SettingsService.cs`: copy all files/subdirectories from old to new location via `Task.Run`, update `data-folder-redirect.json` at fixed `%LOCALAPPDATA%/AquaSync/` path, call `IDataService.SetDataFolderPath()`, delete old data on success, rollback (delete partial copy, keep original) on failure, throw `InvalidOperationException` for same-folder or non-empty-destination, throw `IOException` for disk/permission failures
- [ ] T023 [US4] Add data folder properties and commands to `SettingsViewModel` in `AquaSync.App/ViewModels/SettingsViewModel.cs`: add `DataFolderPath` display property, `IsMovingData` state, `IsCustomDataFolder` (shows reset button when true), `HasDataFolderWarning` property (true when `IDataService.HasRedirectFallback` — custom path was inaccessible at startup), `BrowseDataFolderCommand`, `ResetDataFolderCommand`, navigation-blocking via `IsNavigationBlocked` property (disables back button and ShellPage sidebar during move), error/success notifications
- [ ] T024 [US4] Add "Data Folder" section UI to `SettingsPage.xaml` with path display TextBlock, "Browse" button, "Reset to default" button (visible when `IsCustomDataFolder` is true), warning `InfoBar` (visible when `HasDataFolderWarning` is true, message: "The previously configured data folder was inaccessible. Data has been loaded from the default location."), `ProgressRing`, confirmation `ContentDialog` (showing source/destination paths and duration warning), and add `FolderPicker` handler + confirmation dialog logic in `AquaSync.App/Views/SettingsPage.xaml.cs`. Wire `IsNavigationBlocked` to disable both the SettingsPage back button and ShellPage NavigationView items during move.

**Checkpoint**: Data folder move works end-to-end with confirmation dialog. Rollback on failure. Reset to default works. Navigation blocked during move. App functions correctly from new location.

---

## Phase 7: User Story 5 — View Application Information (Priority: P5)

**Goal**: Users can see the app name, version, and description in an About section

**Independent Test**: Navigate to Settings, verify About section shows "AquaSync", the correct version number, and a one-sentence description

### Implementation for User Story 5

- [ ] T025 [US5] Add `AppVersion` read-only property to `SettingsViewModel` in `AquaSync.App/ViewModels/SettingsViewModel.cs`: read version from assembly metadata via `Assembly.GetExecutingAssembly().GetName().Version`
- [ ] T026 [US5] Add "About" section UI to `SettingsPage.xaml` with app name "AquaSync" as header, version number from ViewModel binding, and a one-sentence description: "A desktop application for managing home aquariums, controlling LED lights, and monitoring filters." in `AquaSync.App/Views/SettingsPage.xaml`

**Checkpoint**: About section displays correct app name, version, and description. No interactive behavior needed.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and edge case handling

- [ ] T027 Full build verification: `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64` — ensure zero errors and zero warnings
- [ ] T028 Verify edge cases from spec: invalid custom data path on startup falls back to default with warning, same-folder detection, non-empty destination warning, export disabled during folder move, export error messages for disk full/permission denied

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phases 3–7)**: All depend on Foundational phase completion
  - User stories MUST be implemented sequentially (P1 → P2 → P3 → P4 → P5) because they all modify the same files (`SettingsViewModel.cs`, `SettingsPage.xaml`, `SettingsPage.xaml.cs`)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2. No dependencies on other stories.
- **US2 (P2)**: Can start after Phase 2. Independent of US1 but shares files.
- **US3 (P3)**: Can start after Phase 2. Independent of US1/US2 but shares files.
- **US4 (P4)**: Can start after Phase 2. Independent of US1–US3 but shares files.
- **US5 (P5)**: Can start after Phase 2. Independent of US1–US4 but shares files.

### Within Each User Story

- Service implementation before ViewModel (ViewModel calls service)
- ViewModel before View (View binds to ViewModel)
- Consumer integration (e.g., T015) can be parallel with View tasks

### Parallel Opportunities

- **Phase 1**: T001 + T002 in parallel, then T003, then T004 + T005 in parallel
- **Phase 2**: T006 + T007 + T008 in parallel, then T009, then T010 + T011 in parallel
- **Phase 3 (US1)**: T013 then T014, T015 can run parallel with T014
- **Phases 4–7**: Each user story is sequential internally (service → ViewModel → View)

---

## Parallel Example: Phase 2 (Foundational)

```text
# Batch 1 — All independent service/window tasks:
T006: Modify DataService (redirect, SetDataFolderPath) in Services/DataService.cs
T007: Create SettingsService (init, save) in Services/SettingsService.cs
T008: Add SetTheme to MainWindow in Views/MainWindow.xaml.cs

# Batch 2 — DI wiring (depends on T007):
T009: Register ISettingsService in App.xaml.cs

# Batch 3 — UI entry points (depends on T009):
T010: Add gear icon to AquariumSelectorPage
T011: Add back navigation to SettingsPage
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T005)
2. Complete Phase 2: Foundational (T006–T012)
3. Complete Phase 3: User Story 1 (T013–T015)
4. **STOP and VALIDATE**: Test UOM defaults end-to-end
5. Default units work, settings persist, gear icon navigation works

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. Add US1 (Default Units) → Test → **MVP!**
3. Add US2 (Theme) → Test → Immediate visual impact
4. Add US3 (Export) → Test → Backup capability
5. Add US4 (Data Folder) → Test → Advanced data management
6. Add US5 (About) → Test → Informational section
7. Polish → Edge cases verified

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- User stories share `SettingsViewModel.cs`, `SettingsPage.xaml`, and `SettingsPage.xaml.cs` — implement sequentially in priority order
- **No `ConfigureAwait(false)` in ViewModels** — only in `SettingsService` and `DataService`
- **No `[ObservableProperty]`** — use manual `SetProperty` pattern
- **No `[RelayCommand]`** — use manual `RelayCommand` / `AsyncRelayCommand` instantiation
- All concrete classes MUST be `sealed` with file-scoped namespaces
- Commit after each completed user story phase
