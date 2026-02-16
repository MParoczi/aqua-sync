# Quickstart: Aquarium Profile Management

**Branch**: `001-aquarium-profiles` | **Date**: 2026-02-15

## Prerequisites

- .NET SDK 10.0.101+ (verify: `dotnet --version`)
- Windows 11 (or Windows 10 19041+)
- Visual Studio 2022+ or VS Code with C# Dev Kit
- Windows App SDK 1.7 workload installed

## Build & Run

```bash
# Build the solution
dotnet build AquaSync.sln

# Run the app
dotnet run --project AquaSync.App -r win-x64
```

## Implementation Order

Follow this dependency-based order for implementing the feature:

### Layer 1: Data Foundation (no UI dependencies)

1. **Enums** — Create all enum types in `Models/`:
   - `AquariumType.cs`, `VolumeUnit.cs`, `DimensionUnit.cs`, `SubstrateType.cs`, `AquariumStatus.cs`

2. **Models** — Create data model classes in `Models/`:
   - `SubstrateEntry.cs` (no dependencies on other models)
   - `Aquarium.cs` (depends on enums and SubstrateEntry)

3. **IDataService extension** — Add `ReadAllAsync<T>` to the interface and implement in `DataService`

4. **IAquariumService** — Create interface and implementation:
   - Interface in `Contracts/Services/IAquariumService.cs`
   - Implementation in `Services/AquariumService.cs`

5. **IAquariumContext** — Create interface and implementation:
   - Interface in `Contracts/Services/IAquariumContext.cs`
   - Implementation in `Services/AquariumContext.cs`

6. **DI Registration** — Register new services in `App.xaml.cs`

### Layer 2: Selector Grid (entry point)

7. **AquariumSelectorViewModel** — Implement:
   - Observable collection of aquariums
   - Load all profiles on initialization
   - Commands: CreateAquarium, ArchiveAquarium, RestoreAquarium, DeleteAquarium
   - Profile creation dialog logic

8. **AquariumSelectorPage** — Implement XAML:
   - GridView with card DataTemplate (thumbnail, name, type, volume, date)
   - Empty state (no profiles)
   - "Add Aquarium" button
   - MenuFlyout on cards (archive/restore/delete)
   - Converters for archived appearance and default thumbnail

9. **Default thumbnail asset** — Add `default-aquarium.png` to `Assets/`

### Layer 3: Navigation Flow

10. **Root navigation** — Wire AquariumSelectorPage → ShellPage:
    - AquariumSelectorPage item click → navigate MainWindow RootFrame to ShellPage with aquarium ID
    - ShellPage receives aquarium ID, loads via IAquariumService, sets IAquariumContext

11. **ShellPage modifications** — Add:
    - Aquarium name display in header/title area
    - Back-to-selector navigation (override NavigationView back button for root-level back)
    - Read-only visual indicator for archived aquariums

### Layer 4: Profile Editing

12. **SettingsViewModel** — Implement profile editing:
    - Display current aquarium details (locked fields read-only)
    - Edit commands for name, description, thumbnail
    - Substrate management (add/edit/remove)

13. **SettingsPage** — Implement XAML:
    - Profile info display with edit capabilities
    - Substrate list with inline management

## Key Files Quick Reference

| File | Purpose |
|------|---------|
| `Models/Aquarium.cs` | Core data model |
| `Models/SubstrateEntry.cs` | Substrate/additive entry model |
| `Contracts/Services/IAquariumService.cs` | CRUD interface |
| `Contracts/Services/IAquariumContext.cs` | Current aquarium holder |
| `Services/AquariumService.cs` | Persistence implementation |
| `Services/AquariumContext.cs` | Context implementation |
| `ViewModels/AquariumSelectorViewModel.cs` | Grid logic + CRUD commands |
| `Views/AquariumSelectorPage.xaml` | Grid UI + cards |
| `Views/ShellPage.xaml` | Shell modifications |
| `ViewModels/SettingsViewModel.cs` | Profile editing logic |
| `Views/SettingsPage.xaml` | Profile editing UI |

## Patterns to Follow

### ViewModel Property Pattern
```csharp
private string _name = string.Empty;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}
```

### Service Resolution in Code-Behind
```csharp
public AquariumSelectorPage()
{
    ViewModel = App.GetService<AquariumSelectorViewModel>();
    InitializeComponent();
    DataContext = ViewModel;
}
```

### ContentDialog with XamlRoot
```csharp
var dialog = new ContentDialog
{
    XamlRoot = this.XamlRoot,
    Title = "New Aquarium",
    PrimaryButtonText = "Save",
    CloseButtonText = "Cancel",
    Content = dialogContent
};
var result = await dialog.ShowAsync();
```

### Data Access Pattern
```csharp
// Read all aquariums
var aquariums = await _dataService.ReadAllAsync<Aquarium>("aquariums");

// Save an aquarium
await _dataService.SaveAsync("aquariums", aquarium.Id.ToString(), aquarium);

// Delete an aquarium
await _dataService.DeleteAsync("aquariums", aquarium.Id.ToString());
```

## Validation Checklist

After implementation, verify:

- [ ] Empty state shown when no profiles exist
- [ ] Profile creation saves all fields correctly to JSON
- [ ] UOM toggles lock to the profile after creation
- [ ] Default thumbnail used when no photo uploaded
- [ ] Uploaded photo copied to gallery folder
- [ ] Clicking active card navigates to ShellPage with correct context
- [ ] Clicking archived card enters read-only mode
- [ ] Archive/restore toggles status and visual appearance
- [ ] Delete removes JSON file and gallery folder
- [ ] Profile editing only allows name, description, thumbnail changes
- [ ] Substrate add/edit/remove works correctly
- [ ] Back navigation returns to selector grid
- [ ] Data persists across app restarts
