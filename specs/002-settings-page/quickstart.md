# Quickstart: Global Settings Page

**Feature**: 002-settings-page
**Date**: 2026-02-20

## Prerequisites

- .NET SDK 10.0.101+ (pinned in `global.json`)
- Windows 11 with WinUI3 / Windows App SDK 1.7
- Project builds: `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64`

## New Files to Create

| File | Purpose |
|------|---------|
| `AquaSync.App/Models/AppSettings.cs` | Settings data model with defaults |
| `AquaSync.App/Models/AppTheme.cs` | Theme enum (System, Light, Dark) |
| `AquaSync.App/Models/DataFolderRedirect.cs` | Redirect pointer for custom data folder |
| `AquaSync.App/Contracts/Services/ISettingsService.cs` | Settings service interface |
| `AquaSync.App/Services/SettingsService.cs` | Settings service implementation |

## Existing Files to Modify

| File | Changes |
|------|---------|
| `AquaSync.App/App.xaml.cs` | Register `ISettingsService` as singleton, call `InitializeAsync` + `ApplyTheme` at startup |
| `AquaSync.App/Contracts/Services/IDataService.cs` | Add `SetDataFolderPath(string)` method |
| `AquaSync.App/Services/DataService.cs` | Implement `SetDataFolderPath`, add redirect file check in constructor, make `_rootPath` mutable |
| `AquaSync.App/ViewModels/SettingsViewModel.cs` | Add global settings properties, theme/export/folder commands, inject `ISettingsService` |
| `AquaSync.App/Views/SettingsPage.xaml` | Replace "General" placeholder with UOM, theme, data folder, export, and about sections |
| `AquaSync.App/Views/SettingsPage.xaml.cs` | Add event handlers for theme change, export, browse folder |
| `AquaSync.App/Views/AquariumSelectorPage.xaml` | Add gear icon button in page header |
| `AquaSync.App/Views/AquariumSelectorPage.xaml.cs` | Add click handler to navigate to Settings via root frame |
| `AquaSync.App/Views/MainWindow.xaml.cs` | Add `SetTheme(ElementTheme)` method |
| `AquaSync.App/ViewModels/AquariumSelectorViewModel.cs` | Inject `ISettingsService`, use defaults in `ResetCreationForm()` |

## Implementation Order

1. **Model layer**: Create `AppSettings`, `AppTheme`, `DataFolderRedirect` models
2. **Service contracts**: Create `ISettingsService`, extend `IDataService`
3. **Service implementation**: Create `SettingsService`, modify `DataService`
4. **DI registration**: Register `ISettingsService` in `App.xaml.cs`, call init at startup
5. **Theme support**: Add `SetTheme` to `MainWindow`, wire up in `SettingsService.ApplyTheme()`
6. **ViewModel**: Extend `SettingsViewModel` with global settings properties and commands
7. **View - Settings UI**: Build out `SettingsPage.xaml` with all five sections
8. **View - Selector gear icon**: Add settings button to `AquariumSelectorPage`
9. **Consumer integration**: Wire `AquariumSelectorViewModel` to use settings defaults
10. **Data export**: Implement ZIP export in `SettingsService`
11. **Data folder migration**: Implement folder move with confirmation dialog

## Key Patterns to Follow

### ViewModel Property Pattern (manual SetProperty)
```csharp
private AppTheme _selectedTheme;
public AppTheme SelectedTheme
{
    get => _selectedTheme;
    set => SetProperty(ref _selectedTheme, value);
}
```

### Command Pattern (manual RelayCommand)
```csharp
public IAsyncRelayCommand ExportDataCommand { get; }

// In constructor:
ExportDataCommand = new AsyncRelayCommand(OnExportDataAsync);
```

### Service Registration Pattern
```csharp
services.AddSingleton<ISettingsService, SettingsService>();
```

### Theme Mapping
```csharp
AppTheme.System → ElementTheme.Default
AppTheme.Light  → ElementTheme.Light
AppTheme.Dark   → ElementTheme.Dark
```

## Build & Verify

```bash
dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64
```

## Critical Reminders

- **No `ConfigureAwait(false)` in ViewModels** — only in services (`SettingsService`, `DataService`)
- **No `[ObservableProperty]`** — use manual `SetProperty` pattern
- **No `[RelayCommand]`** — use manual `RelayCommand` / `AsyncRelayCommand` instantiation
- **Sealed classes** for all concrete implementations
- **File-scoped namespaces** everywhere
- **CancellationToken** on all public async service methods
