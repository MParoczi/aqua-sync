# Tasks: Aquarium Profile Management

**Input**: Design documents from `/specs/001-aquarium-profiles/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/services.md, research.md, quickstart.md

**Tests**: Not requested. No test tasks included.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- All paths relative to repository root

---

## Phase 1: Setup (Models & Enums)

**Purpose**: Create all data model types that every subsequent phase depends on

- [x] T001 [P] Create AquariumType enum (Freshwater, Saltwater, Brackish) with JsonStringEnumConverter in AquaSync.App/Models/AquariumType.cs
- [x] T002 [P] Create VolumeUnit enum (Liters, Gallons) with JsonStringEnumConverter in AquaSync.App/Models/VolumeUnit.cs
- [x] T003 [P] Create DimensionUnit enum (Centimeters, Inches) with JsonStringEnumConverter in AquaSync.App/Models/DimensionUnit.cs
- [x] T004 [P] Create SubstrateType enum (Substrate, Additive, SoilCap) with JsonStringEnumConverter in AquaSync.App/Models/SubstrateType.cs
- [x] T005 [P] Create AquariumStatus enum (Active, Archived) with JsonStringEnumConverter in AquaSync.App/Models/AquariumStatus.cs
- [x] T006 [P] Create SubstrateEntry model class with all fields (Id, Brand, ProductName, Type, LayerDepth, DateAdded, Notes, DisplayOrder) per data-model.md in AquaSync.App/Models/SubstrateEntry.cs
- [x] T007 Create Aquarium model class with all fields (Id, Name, Volume, VolumeUnit, Length, Width, Height, DimensionUnit, AquariumType, SetupDate, Description, ThumbnailPath, Status, CreatedAt, Substrates) per data-model.md in AquaSync.App/Models/Aquarium.cs

**Checkpoint**: All model types compile. `dotnet build AquaSync.App/AquaSync.App.csproj` succeeds.

---

## Phase 2: Foundational (Services & Infrastructure)

**Purpose**: Core service infrastructure that MUST be complete before any user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Add `ReadAllAsync<T>(string folderName)` method signature to IDataService interface in AquaSync.App/Contracts/Services/IDataService.cs
- [x] T009 Implement `ReadAllAsync<T>` in DataService — enumerate all .json files in folder, deserialize each with existing JsonSerializerOptions, skip files that fail deserialization, use existing SemaphoreSlim lock, return `IReadOnlyList<T>` in AquaSync.App/Services/DataService.cs
- [x] T010 [P] Create IAquariumService interface with methods: GetAllAsync, GetByIdAsync, SaveAsync, DeleteAsync, SaveThumbnailAsync, DeleteThumbnailAsync per contracts/services.md in AquaSync.App/Contracts/Services/IAquariumService.cs
- [x] T011 [P] Create IAquariumContext interface with properties: CurrentAquarium, IsReadOnly and methods: SetCurrentAquarium, Clear per contracts/services.md in AquaSync.App/Contracts/Services/IAquariumContext.cs
- [x] T012 Implement AquariumService (sealed, singleton) — CRUD operations using IDataService with "aquariums" folder, thumbnail management under gallery/{id}/ folder, ordering by CreatedAt descending in AquaSync.App/Services/AquariumService.cs
- [x] T013 [P] Implement AquariumContext (sealed, singleton) — holds current Aquarium reference, computes IsReadOnly from AquariumStatus, implements SetCurrentAquarium and Clear in AquaSync.App/Services/AquariumContext.cs
- [x] T014 [P] Create NullToDefaultImageConverter (IValueConverter) — returns default aquarium asset path when ThumbnailPath is null, otherwise resolves full path from IDataService.GetDataFolderPath() in AquaSync.App/Converters/NullToDefaultImageConverter.cs
- [x] T015 [P] Create AquariumStatusToOpacityConverter (IValueConverter) — returns 0.5 for Archived status, 1.0 for Active in AquaSync.App/Converters/AquariumStatusToOpacityConverter.cs
- [x] T016 Register IAquariumService/AquariumService and IAquariumContext/AquariumContext as singletons in DI container in AquaSync.App/App.xaml.cs

**Checkpoint**: Foundation ready — all services resolve via DI, `dotnet build` succeeds, `ReadAllAsync` returns empty list from non-existent aquariums folder.

---

## Phase 3: User Story 1 — View Aquarium Selector Grid (Priority: P1) MVP

**Goal**: Display aquarium profile cards in an adaptive grid with empty state, ordered active-first then archived, newest-first within each group. Provide context menu structure on cards and an "Add Aquarium" entry point.

**Independent Test**: Launch the application. With no profiles: verify empty state (illustration + "No aquariums yet" + create button). Manually place a test JSON in the aquariums/ folder and relaunch: verify the card renders with thumbnail, name, type, volume, and date. Verify adaptive grid reflows on window resize.

**FRs**: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007

### Implementation

- [x] T017 [US1] Implement AquariumSelectorViewModel — inject IAquariumService, add ObservableCollection<Aquarium> for profiles, add LoadProfilesAsync method (sort: active first, then archived, newest first within each group per FR-005), add HasProfiles/IsEmpty properties for empty state visibility, add placeholder RelayCommand properties for CreateProfile, ArchiveProfile, RestoreProfile, DeleteProfile in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs
- [x] T018 [US1] Implement AquariumSelectorPage XAML — add GridView with adaptive ItemWidth for responsive columns (FR-006), create card DataTemplate showing thumbnail image (with NullToDefaultImageConverter), name, aquarium type, volume with unit, and setup date (FR-002), add "Add Aquarium" card as first grid item (FR-004), add empty state StackPanel with illustration, "No aquariums yet" text, and create button (FR-003), add MenuFlyout on each card with Archive/Restore/Delete items bound to ViewModel commands with CommandParameter (FR-007), add opacity binding with AquariumStatusToOpacityConverter in AquaSync.App/Views/AquariumSelectorPage.xaml
- [x] T019 [US1] Update AquariumSelectorPage code-behind — resolve AquariumSelectorViewModel from DI, set DataContext, trigger LoadProfilesAsync on page Loaded event, add ItemClick handler stub for card selection in AquaSync.App/Views/AquariumSelectorPage.xaml.cs

**Checkpoint**: App launches showing empty state or profile cards. Cards display all required fields. Grid reflows on resize. Context menu appears on right-click (commands are stubs).

---

## Phase 4: User Story 2 — Create a New Aquarium Profile (Priority: P1)

**Goal**: Users can create a complete aquarium profile via a ContentDialog form with all required/optional fields, UOM toggles, photo upload with preview, and validation. New profiles appear in the selector grid after save.

**Independent Test**: Click "Add Aquarium" card or empty state button. Fill in all fields, toggle UOM, upload a photo, save. Verify profile appears in grid with correct data. Relaunch app to verify persistence. Test validation by leaving required fields empty. Test cancel-with-data confirmation.

**FRs**: FR-008, FR-009, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-034, FR-035, FR-036

### Implementation

- [x] T020 [US2] Add profile creation logic to AquariumSelectorViewModel — form field properties (NewName, NewVolume, NewVolumeUnit, NewLength, NewWidth, NewHeight, NewDimensionUnit, NewAquariumType, NewSetupDate, NewNotes, NewThumbnailPath, NewThumbnailPreview), default values per FR-008 (liters, centimeters, today's date), validation method checking required fields and positive numbers (FR-034, FR-035), validation error properties for inline indicators, SaveProfileAsync command that creates Aquarium object and calls IAquariumService.SaveAsync then refreshes grid, PickThumbnailAsync command using FileOpenPicker with image filters and 10MB limit (FR-012), duplicate name warning check on name change, cancel confirmation flag (HasUnsavedData property), locale-aware numeric parsing (FR-015) in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs
- [x] T021 [US2] Add profile creation ContentDialog to AquariumSelectorPage XAML — ContentDialog with Title "New Aquarium", PrimaryButtonText "Save", CloseButtonText "Cancel", Content is ScrollViewer (FR-013) wrapping StackPanel with: TextBox for name (max 100 chars), NumberBox or TextBox for volume, ToggleSwitch for volume unit (Liters/Gallons default Liters), three NumberBox/TextBox for length/width/height, ToggleSwitch for dimension unit (cm/in default cm), ComboBox for aquarium type, CalendarDatePicker for setup date (default today, date-only), TextBox for notes (max 2000 chars, AcceptsReturn), Button for photo upload with Image preview area, validation TextBlocks for inline error messages (FR-014), inline warning for duplicate name in AquaSync.App/Views/AquariumSelectorPage.xaml
- [x] T022 [US2] Wire creation dialog in AquariumSelectorPage code-behind — handle "Add Aquarium" card click and empty state button click to show creation ContentDialog (set XamlRoot per research.md), handle PrimaryButtonClick to trigger validation and save, handle Closing event to show discard-changes confirmation when HasUnsavedData is true (FR-014), reset form state after successful save or confirmed cancel in AquaSync.App/Views/AquariumSelectorPage.xaml.cs

**Checkpoint**: Full creation flow works end-to-end. All fields save to JSON. UOM toggles set correct units. Photo copies to gallery folder. Validation prevents incomplete saves. Cancel confirmation appears when data entered.

---

## Phase 5: User Story 4 — Select Aquarium and Enter Management Shell (Priority: P1)

**Goal**: Clicking an aquarium card navigates from the selector grid to the management shell scoped to that aquarium. The shell header shows aquarium name and type. Back button returns to selector with grid refresh.

**Independent Test**: Create a profile, click its card. Verify shell loads with correct name and type in header. Click sidebar items to confirm navigation works. Click back button to return to selector grid. Verify grid refreshes on return (FR-026).

**FRs**: FR-022, FR-023, FR-024, FR-025, FR-026, FR-040

### Implementation

- [x] T023 [US4] Add card click navigation in AquariumSelectorPage code-behind — on ItemClick, distinguish "Add Aquarium" card (show creation dialog) from profile card (navigate MainWindow RootFrame to ShellPage passing Aquarium.Id as Guid parameter) in AquaSync.App/Views/AquariumSelectorPage.xaml.cs
- [x] T024 [P] [US4] Update ShellViewModel — inject IAquariumContext, add observable properties for AquariumName, AquariumType, IsReadOnly (from context), add InitializeAsync method to load aquarium by ID via IAquariumService and set IAquariumContext, add GoBackCommand to clear context and navigate RootFrame to AquariumSelectorPage in AquaSync.App/ViewModels/ShellViewModel.cs
- [x] T025 [US4] Update ShellPage XAML — add header area above or within NavigationView displaying aquarium name and type bound to ShellViewModel properties (FR-023), bind NavigationView BackButtonVisible and wire back button to ShellViewModel.GoBackCommand (FR-026) in AquaSync.App/Views/ShellPage.xaml
- [x] T026 [US4] Update ShellPage code-behind — override OnNavigatedTo to extract Guid parameter, call ShellViewModel.InitializeAsync with the ID, handle back navigation by navigating MainWindow RootFrame back to AquariumSelectorPage in AquaSync.App/Views/ShellPage.xaml.cs

**Checkpoint**: Card click navigates to shell with correct aquarium context. Header shows name and type. Sidebar navigation works between pages. Back button returns to selector grid which refreshes its profile list.

---

## Phase 6: User Story 3 — Register Substrates and Additives (Priority: P2)

**Goal**: Users can register substrate/additive entries during profile creation and manage them from the Settings page. Each entry has brand, product name, type, layer depth (in parent's dimension unit), date added, and notes. Entries can be added, edited, removed, and reordered via up/down controls.

**Independent Test**: Create a profile with 2+ substrate entries. Verify entries save and display correctly. Open Settings page in shell, add/edit/remove/reorder substrates. Verify changes persist across app restarts. Verify layer depth uses parent's dimension unit (FR-019).

**FRs**: FR-016, FR-017, FR-018, FR-019, FR-020, FR-021, FR-027

### Implementation

- [ ] T027 [US3] Add substrate management section to creation ContentDialog — ListView or ItemsRepeater within the creation form showing substrate entries with brand, product name, type, depth, date; "Add Substrate" button opening a secondary ContentDialog or inline expander with entry fields; remove button per entry; up/down reorder buttons per entry (FR-020) in AquaSync.App/Views/AquariumSelectorPage.xaml
- [ ] T028 [US3] Add substrate management to AquariumSelectorViewModel — ObservableCollection<SubstrateEntry> for creation form entries, AddSubstrateCommand (opens entry form, validates required fields), RemoveSubstrateCommand, MoveSubstrateUpCommand/MoveSubstrateDownCommand (update DisplayOrder), entry form properties (EntryBrand, EntryProductName, EntryType, EntryLayerDepth, EntryDateAdded, EntryNotes), cancel substrate entry discards partial data (FR-021), layer depth label uses parent dimension unit (FR-019) in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs
- [ ] T029 [US3] Implement SettingsViewModel — inject IAquariumContext and IAquariumService, add properties for editable fields (EditName, EditNotes bound to current aquarium), add SaveProfileCommand for name/notes changes (FR-016), add PickThumbnailCommand for photo replacement, add ObservableCollection<SubstrateEntry> loaded from current aquarium, add substrate CRUD commands (Add/Edit/Remove/MoveUp/MoveDown) matching creation flow, add locked field display properties (volume, dimensions, type, date shown as read-only per FR-017), add IsReadOnly property from IAquariumContext to disable controls for archived profiles in AquaSync.App/ViewModels/SettingsViewModel.cs
- [ ] T030 [US3] Implement SettingsPage XAML — dual-scope layout per FR-027: global settings section (placeholder for future app-wide settings) and aquarium-scoped section (visible when IAquariumContext has a current aquarium) with: read-only fields showing volume, dimensions, type, setup date with lock icon indicator (FR-017), editable TextBox for name, editable TextBox for notes, photo upload button with thumbnail preview, substrate ListView with entry rows (brand, product, type, depth with unit label, date) and add/edit/remove/reorder controls (FR-020) in AquaSync.App/Views/SettingsPage.xaml
- [ ] T031 [US3] Update SettingsPage code-behind — resolve SettingsViewModel from DI, set DataContext, trigger load of current aquarium data from IAquariumContext on page Loaded, handle substrate add/edit ContentDialog display with XamlRoot in AquaSync.App/Views/SettingsPage.xaml.cs

**Checkpoint**: Substrate entries save within aquarium JSON. Entries can be added during creation and from Settings. Reorder (up/down) updates display order. Layer depth shows parent's unit. Settings page shows locked fields with lock indicator and editable fields. Profile editing (name, notes, thumbnail) works from Settings.

---

## Phase 7: User Story 5 — Archive an Aquarium Profile (Priority: P3)

**Goal**: Users can archive an aquarium via card context menu, which marks it inactive with visual distinction. Archived profiles are browsable in read-only mode in the shell. Profiles can be restored to active via context menu or shell banner.

**Independent Test**: Right-click an active profile card → Archive → confirm. Verify card shows 50% opacity + "Archived" badge. Click archived card → verify shell opens in read-only mode with banner. Click Restore on banner → verify shell becomes editable. Right-click archived card → Restore → verify card returns to normal.

**FRs**: FR-028, FR-029, FR-030, FR-031, FR-039

### Implementation

- [ ] T032 [US5] Implement archive and restore commands in AquariumSelectorViewModel — ArchiveProfileCommand shows confirmation ContentDialog ("This aquarium will be archived. You can restore it later. Archive?") then sets Status to Archived via IAquariumService.SaveAsync and refreshes grid (FR-028), RestoreProfileCommand sets Status back to Active and refreshes grid (FR-031) in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs
- [ ] T033 [P] [US5] Add archived visual styling to card DataTemplate — bind card container Opacity to Status via AquariumStatusToOpacityConverter for 50% opacity on archived cards, add "Archived" TextBlock badge overlay with Visibility bound to Status == Archived (FR-029) in AquaSync.App/Views/AquariumSelectorPage.xaml
- [ ] T034 [US5] Add read-only mode to ShellPage XAML — add InfoBar banner at top of content area: "This aquarium is archived (read-only). Restore to make changes." with a Restore action button, bind banner Visibility to ShellViewModel.IsReadOnly (FR-030) in AquaSync.App/Views/ShellPage.xaml
- [ ] T035 [US5] Wire read-only restore action in ShellPage code-behind — handle Restore button click on banner: call IAquariumService to update status, refresh IAquariumContext, update ShellViewModel.IsReadOnly to false, hide banner in AquaSync.App/Views/ShellPage.xaml.cs

**Checkpoint**: Archive/restore cycle works fully. Archived cards show 50% opacity + badge. Archived profile opens in read-only shell with banner. Restore from both context menu and shell banner works. All data preserved through archive/restore cycle (SC-005).

---

## Phase 8: User Story 6 — Delete an Aquarium Profile (Priority: P3)

**Goal**: Users can permanently delete any profile (active or archived) via card context menu with a confirmation dialog. Deletion removes the JSON file, substrates, and gallery folder.

**Independent Test**: Right-click a profile card → Delete → confirm. Verify profile disappears from grid. Verify JSON file and gallery folder are removed from disk. Delete the last profile and verify empty state appears.

**FRs**: FR-032, FR-033

### Implementation

- [ ] T036 [US6] Implement delete command in AquariumSelectorViewModel — DeleteProfileCommand shows confirmation ContentDialog ("Permanently delete [Name]? This will remove all profile data, substrates, and photos. This action cannot be undone.") with destructive styling, on confirm calls IAquariumService.DeleteAsync(id) which removes JSON file and gallery folder (FR-033), then removes profile from ObservableCollection to refresh grid in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs

**Checkpoint**: Delete permanently removes profile data and gallery. Grid updates immediately. Deleting last profile shows empty state. Cancel leaves profile unchanged.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: UX polish, error handling, and edge cases that span multiple user stories

- [ ] T037 [P] Add InfoBar success notifications after create, edit, archive, restore, and delete operations — add InfoBar control to AquariumSelectorPage.xaml bound to ViewModel notification properties, show briefly then auto-dismiss (FR-039) in AquaSync.App/Views/AquariumSelectorPage.xaml and AquaSync.App/ViewModels/AquariumSelectorViewModel.cs
- [ ] T038 [P] Add ProgressRing indicators for async operations — show during profile loading, saving, and photo upload, bind to IsBusy/IsLoading properties on relevant ViewModels (FR-040) in AquaSync.App/Views/AquariumSelectorPage.xaml and AquaSync.App/Views/SettingsPage.xaml
- [ ] T039 Implement error handling for edge cases — corrupted JSON files: show warning InfoBar listing unreadable profiles (FR-037), missing gallery folder: silently fall back to default graphic (FR-038), storage inaccessible: show "Could not save profile. Please check disk space and permissions." and preserve previous state, photo upload failure: preserve previous thumbnail and show error, external JSON deletion while shell is open: show error notification and navigate back to selector in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs and AquaSync.App/ViewModels/ShellViewModel.cs
- [ ] T040 Verify locale-aware decimal input parsing for volume, dimensions, and layer depth — ensure CultureInfo.CurrentCulture is used for parsing, test with comma and period decimal separators (FR-015) in AquaSync.App/ViewModels/AquariumSelectorViewModel.cs and AquaSync.App/ViewModels/SettingsViewModel.cs
- [ ] T041 Run quickstart.md validation checklist — manually verify all 14 items: empty state, profile creation saves correctly, UOM locks, default thumbnail, uploaded photo, active card navigation, archived card read-only, archive/restore, delete, profile editing, substrate CRUD, back navigation, data persistence (SC-001 through SC-008)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (models must exist for services) — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — first user story, provides the grid
- **US2 (Phase 4)**: Depends on US1 (needs grid to show created profiles, "Add Aquarium" card trigger)
- **US4 (Phase 5)**: Depends on US1 (needs grid with cards to click for navigation)
- **US3 (Phase 6)**: Depends on US2 (substrate entry in creation form) AND US4 (Settings page in shell)
- **US5 (Phase 7)**: Depends on US1 (grid) AND US4 (shell for read-only mode)
- **US6 (Phase 8)**: Depends on US1 (grid with cards to delete)
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1: Setup
    ↓
Phase 2: Foundational
    ↓
Phase 3: US1 (Selector Grid) ← MVP starts here
    ├──→ Phase 4: US2 (Create Profile)
    │        ↓
    │    Phase 6: US3 (Substrates) ← also depends on US4
    ├──→ Phase 5: US4 (Shell Navigation) ──→ (feeds into US3, US5)
    ├──→ Phase 7: US5 (Archive/Restore) ← also depends on US4
    └──→ Phase 8: US6 (Delete)
              ↓
         Phase 9: Polish (after all stories complete)
```

### Within Each User Story

- Models/enums already exist (from Setup phase)
- Services already exist (from Foundational phase)
- ViewModel logic before XAML UI
- XAML before code-behind wiring
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1** — All enum tasks (T001–T005) and SubstrateEntry (T006) can run in parallel:
```
Parallel: T001, T002, T003, T004, T005, T006
Then: T007 (depends on all above)
```

**Phase 2** — Interface + converter tasks can run in parallel:
```
Sequential: T008 → T009
Parallel: T010, T011, T014, T015 (all independent)
Then: T012 (depends on T009, T010), T013 (depends on T011)
Then: T016 (depends on T012, T013)
```

**After Phase 3 (US1)** — US2, US4, US5, and US6 can start in parallel if staffed:
```
US2 and US4: can run in parallel (different ViewModel files, different page areas)
US5 and US6: can run in parallel after US1 (US5 also needs US4 for read-only shell)
US3: must wait for both US2 and US4
```

**Phase 9** — T037 and T038 can run in parallel (different concerns).

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup (enums + models)
2. Complete Phase 2: Foundational (services + DI)
3. Complete Phase 3: US1 — Selector Grid renders with cards or empty state
4. Complete Phase 4: US2 — Profile creation works end-to-end
5. Complete Phase 5: US4 — Card click navigates to shell with context
6. **STOP and VALIDATE**: All P1 stories functional. Test SC-001 through SC-004, SC-007.

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Selector grid with cards or empty state (MVP start)
3. Add US2 → Profile creation works (MVP complete!)
4. Add US4 → Full navigation cycle: grid → shell → back
5. Add US3 → Substrate management in creation + Settings
6. Add US5 → Archive/restore with read-only shell
7. Add US6 → Permanent deletion
8. Polish → Notifications, progress indicators, error handling
9. Each story adds value without breaking previous stories

---

## Summary

| Phase | Story | Tasks | Key Files |
|-------|-------|-------|-----------|
| 1 Setup | — | T001–T007 (7) | Models/*.cs |
| 2 Foundational | — | T008–T016 (9) | Services/*.cs, Contracts/*.cs, Converters/*.cs, App.xaml.cs |
| 3 US1 | Selector Grid | T017–T019 (3) | AquariumSelectorViewModel.cs, AquariumSelectorPage.xaml/.cs |
| 4 US2 | Create Profile | T020–T022 (3) | AquariumSelectorViewModel.cs, AquariumSelectorPage.xaml/.cs |
| 5 US4 | Shell Nav | T023–T026 (4) | ShellViewModel.cs, ShellPage.xaml/.cs, AquariumSelectorPage.xaml.cs |
| 6 US3 | Substrates | T027–T031 (5) | AquariumSelectorViewModel.cs, SettingsViewModel.cs, SettingsPage.xaml/.cs |
| 7 US5 | Archive | T032–T035 (4) | AquariumSelectorViewModel.cs, AquariumSelectorPage.xaml, ShellPage.xaml/.cs |
| 8 US6 | Delete | T036 (1) | AquariumSelectorViewModel.cs |
| 9 Polish | — | T037–T041 (5) | Multiple files |
| **Total** | | **41 tasks** | |

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in the same phase
- [Story] label maps tasks to user stories from spec.md for traceability
- Each user story is independently testable at its checkpoint
- Commit after each task or logical group of parallel tasks
- All ViewModels use manual `SetProperty` pattern (NOT `[ObservableProperty]`) per constitution
- All new concrete classes MUST be `sealed` per project conventions
- All async methods take `CancellationToken` parameter per CLAUDE.md conventions
- Existing `aquarium-default.png` in Assets/ serves as the default thumbnail graphic (no new asset needed)
