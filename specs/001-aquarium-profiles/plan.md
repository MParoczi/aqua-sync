# Implementation Plan: Aquarium Profile Management

**Branch**: `001-aquarium-profiles` | **Date**: 2026-02-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-aquarium-profiles/spec.md`

## Summary

Implement the aquarium profile management system as the core feature of AquaSync. This includes:
- **Aquarium selector grid** (landing page) with GridView of profile cards and empty state
- **Profile creation** via ContentDialog with all fields, UOM toggles, and substrate management
- **Profile editing** (name, description, thumbnail only) from the Settings page
- **Archive/restore/delete** lifecycle with confirmation dialogs
- **Navigation flow**: AquariumSelectorPage → ShellPage via MainWindow's RootFrame, passing aquarium context

Built on the existing WinUI3 MVVM infrastructure (ViewModelBase, INavigationService, IDataService) with individual JSON files per aquarium and photo storage under `gallery/{aquarium-id}/`.

## Technical Context

**Language/Version**: C# 14 on .NET 10 (`net10.0-windows10.0.19041.0`)
**Primary Dependencies**: Microsoft.WindowsAppSDK 1.7, CommunityToolkit.Mvvm 8.4, Microsoft.Extensions.DependencyInjection + Hosting
**Storage**: JSON files via `System.Text.Json` under `%LOCALAPPDATA%/AquaSync/`; images as raw files under `gallery/{id}/`
**Testing**: Manual testing (no test framework currently configured)
**Target Platform**: Windows 11 desktop (minimum Windows 10 19041)
**Project Type**: WinUI3 desktop application (single solution, 3 projects)
**Performance Goals**: Selector grid loads within 2 seconds; profile creation completes in under 2 minutes of user time
**Constraints**: Local-first (no cloud), single-user, English-only, no third-party UI libraries, no `[ObservableProperty]` attributes
**Scale/Scope**: Unbounded number of profiles (practical limit by disk space); 10 sidebar pages in management shell

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Windows-Native Design | PASS | GridView, ContentDialog, ComboBox, ToggleSwitch, CalendarDatePicker, MenuFlyout, InfoBar — all standard WinUI3 controls. Mica backdrop already configured. Segoe Fluent Icons for iconography. |
| II. Local-First Data | PASS | Individual JSON files per aquarium in `aquariums/` folder. Photos stored locally in `gallery/{id}/`. No cloud, no network for profile data. |
| III. MVVM Architecture | PASS | ViewModels inherit ViewModelBase, manual `SetProperty` calls, DI via `App.GetService<T>()`, services registered in `App.xaml.cs`. |
| IV. Device Integration | N/A | This feature does not interact with Chihiros or Eheim devices directly. Profile metadata (aquarium type, volume) will be available to device features via shared context. |
| V. Minimal Dependencies | PASS | Zero new NuGet packages. All functionality uses built-in .NET and WinUI3 APIs (System.Text.Json, file I/O, Windows.Storage.Pickers). |
| VI. Single-Aquarium Context | PASS | User selects one aquarium from grid, all shell pages scoped to that aquarium. Must return to selector to switch. Settings page is global context only for app-wide settings; profile editing is scoped to current aquarium. |
| VII. English Only | PASS | All strings hardcoded in English. No `x:Uid`, no resource files. |

**Gate result**: ALL PASS — proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/001-aquarium-profiles/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── services.md      # Service interface contracts
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
AquaSync.App/
├── Models/
│   ├── Aquarium.cs                    # NEW — Aquarium profile model
│   ├── SubstrateEntry.cs              # NEW — Substrate/additive entry model
│   ├── AquariumType.cs                # NEW — Enum: Freshwater, Saltwater, Brackish
│   ├── VolumeUnit.cs                  # NEW — Enum: Liters, Gallons
│   ├── DimensionUnit.cs               # NEW — Enum: Centimeters, Inches
│   ├── SubstrateType.cs               # NEW — Enum: Substrate, Additive, SoilCap
│   └── AquariumStatus.cs             # NEW — Enum: Active, Archived
│
├── Contracts/Services/
│   ├── IDataService.cs                # MODIFIED — add ReadAllAsync<T> method
│   ├── IAquariumService.cs            # NEW — aquarium CRUD + image management
│   └── IAquariumContext.cs            # NEW — holds currently selected aquarium
│
├── Services/
│   ├── DataService.cs                 # MODIFIED — implement ReadAllAsync<T>
│   ├── AquariumService.cs             # NEW — IAquariumService implementation
│   └── AquariumContext.cs             # NEW — IAquariumContext implementation
│
├── Converters/
│   ├── AquariumStatusToOpacityConverter.cs   # NEW — archived cards appear muted
│   └── NullToDefaultImageConverter.cs        # NEW — fallback to default thumbnail
│
├── ViewModels/
│   ├── AquariumSelectorViewModel.cs   # MODIFIED — grid data, CRUD commands
│   ├── ShellViewModel.cs              # MODIFIED — aquarium context display, back nav
│   └── SettingsViewModel.cs           # MODIFIED — profile editing (name, notes, photo)
│
├── Views/
│   ├── AquariumSelectorPage.xaml      # MODIFIED — GridView, cards, empty state, add button
│   ├── AquariumSelectorPage.xaml.cs   # MODIFIED — item click handler, navigation
│   ├── ShellPage.xaml                 # MODIFIED — aquarium name header, back-to-selector
│   ├── ShellPage.xaml.cs              # MODIFIED — receive aquarium parameter
│   ├── SettingsPage.xaml              # MODIFIED — profile edit form
│   └── SettingsPage.xaml.cs           # MODIFIED — bind to SettingsViewModel
│
├── Assets/
│   └── default-aquarium.png           # NEW — default thumbnail graphic
│
└── App.xaml.cs                        # MODIFIED — register new services
```

**Structure Decision**: Extends the existing AquaSync.App project structure. No new projects needed. All new files fit naturally into the established Views/ViewModels/Models/Services/Contracts folder hierarchy. The only existing interface modification is adding `ReadAllAsync<T>` to `IDataService` for folder-level enumeration.

## Complexity Tracking

No constitution violations to justify. All implementation uses existing patterns and standard WinUI3 controls.

## Key Design Decisions

### 1. Navigation: Selector → Shell

AquariumSelectorPage navigates the **MainWindow's RootFrame** to ShellPage, passing the aquarium's `Guid` as the navigation parameter. ShellPage receives the ID in its `OnNavigatedTo`, loads the aquarium via `IAquariumService`, and sets it on `IAquariumContext`. The back button in ShellPage's NavigationView navigates the RootFrame back to AquariumSelectorPage.

**Flow:**
```
AquariumSelectorPage → (user clicks card) → MainWindow.ContentFrame.Navigate(ShellPage, aquariumId)
ShellPage.OnNavigatedTo → AquariumContext.SetCurrentAquarium(aquarium)
ShellPage back → MainWindow.ContentFrame.Navigate(AquariumSelectorPage) + AquariumContext.Clear()
```

### 2. Aquarium Context Sharing

`IAquariumContext` is registered as a **singleton** service. It holds the currently selected `Aquarium` object. All child ViewModels in the shell inject `IAquariumContext` to access the current aquarium's data (ID, name, type, units, etc.). This avoids passing parameters through every navigation layer.

### 3. Profile Creation via ContentDialog

Per constitution principle I ("MUST use ContentDialog for modal forms"), profile creation uses a ContentDialog containing:
- Scrollable form with all required/optional fields
- UOM toggle switches (liters/gallons, cm/inches)
- Aquarium type ComboBox
- CalendarDatePicker for setup date
- Image upload button with preview
- Substrate/additive list with add/edit/remove inline controls

The dialog validates all required fields before allowing the primary button (Save) to proceed.

### 4. Substrate Management

Substrates/additives are managed within the profile creation/edit ContentDialog using an inline list:
- Each entry rendered as a compact card or row with brand, product name, type, depth
- "Add Substrate" button opens a secondary ContentDialog or expander for entry fields
- Entries can be reordered, edited, or removed inline
- Data stored as a `List<SubstrateEntry>` within the Aquarium JSON

### 5. Profile Editing

Accessible from the **Settings page** within the management shell (when an aquarium is active). Only editable fields shown: name, description/notes, thumbnail photo. Locked fields displayed as read-only with a lock icon. Uses the same ContentDialog pattern for consistency.

### 6. Archive/Restore/Delete

Triggered from the **aquarium selector grid** via a **MenuFlyout** (right-click or overflow "..." button on each card):
- **Archive**: Confirmation via ContentDialog → sets status to Archived → card becomes visually muted
- **Restore**: Available only on archived cards → sets status back to Active
- **Delete**: Confirmation via ContentDialog (destructive warning) → removes JSON file and gallery folder

Archived cards in the grid use reduced opacity and an "Archived" badge overlay. Clicking an archived card enters the management shell in **read-only mode** (IAquariumContext exposes an `IsReadOnly` flag).

### 7. Data Storage Layout

```
%LOCALAPPDATA%/AquaSync/
├── aquariums/
│   ├── {guid-1}.json          # Aquarium profile + substrates
│   ├── {guid-2}.json
│   └── ...
└── gallery/
    ├── {guid-1}/
    │   └── thumbnail.jpg      # Original resolution, no compression
    └── {guid-2}/
        └── thumbnail.png
```

Each aquarium is a single JSON file named by its GUID. The `SubstrateEntry` list is embedded within the aquarium JSON (not separate files). Thumbnail path stored as a relative reference in the JSON.

### 8. IDataService Extension

Add a single method to `IDataService`:

```
ReadAllAsync<T>(string folderName) → IReadOnlyList<T>
```

Enumerates all `.json` files in the specified folder, deserializes each, and returns the collection. This is a natural extension needed for loading all aquarium profiles and will be reusable for future features.
