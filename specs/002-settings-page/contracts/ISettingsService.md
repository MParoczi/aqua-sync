# Contract: ISettingsService

**Feature**: 002-settings-page
**Date**: 2026-02-20

## Interface Definition

```csharp
namespace AquaSync.App.Contracts.Services;

public interface ISettingsService
{
    /// <summary>
    ///     Gets the current application settings. Always non-null after initialization.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    ///     Loads settings from disk. Called once during app startup.
    ///     Returns defaults if no settings file exists.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves the current settings to disk.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies the current theme setting to the application window.
    ///     Called at startup and whenever the theme setting changes.
    /// </summary>
    void ApplyTheme();

    /// <summary>
    ///     Exports all application data to a ZIP archive at the specified path.
    /// </summary>
    Task ExportDataAsync(string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves the data folder to a new location. Copies all files, updates
    ///     the redirect file, and removes old data on success.
    /// </summary>
    Task MoveDataFolderAsync(string newFolderPath, CancellationToken cancellationToken = default);
}
```

## Registration

```csharp
// In App.xaml.cs ConfigureServices:
services.AddSingleton<ISettingsService, SettingsService>();
```

## Initialization

```csharp
// In App.OnLaunched, after host build:
var settingsService = GetService<ISettingsService>();
await settingsService.InitializeAsync();
settingsService.ApplyTheme();
```

## Consumer Contracts

### AquariumSelectorViewModel

Injects `ISettingsService` to read default UOM values during `ResetCreationForm()`:

```csharp
// Instead of hardcoded true:
IsVolumeLiters = _settingsService.Settings.DefaultVolumeUnit == VolumeUnit.Liters;
IsDimensionCentimeters = _settingsService.Settings.DefaultDimensionUnit == DimensionUnit.Centimeters;
```

### SettingsViewModel

Injects `ISettingsService` to:
- Read and display current settings values
- Update settings and call `SaveAsync()` on change
- Call `ApplyTheme()` when theme selection changes
- Invoke `ExportDataAsync()` and `MoveDataFolderAsync()` for data operations

### MainWindow

Exposes `SetTheme(ElementTheme theme)` method called by `SettingsService.ApplyTheme()`:

```csharp
public void SetTheme(ElementTheme theme)
{
    if (Content is FrameworkElement rootElement)
    {
        rootElement.RequestedTheme = theme;
    }
}
```

## Error Handling

| Operation | Error Scenario | Behavior |
|-----------|---------------|----------|
| `InitializeAsync` | Settings file corrupt/missing | Return defaults, log nothing (consistent with `DataService` pattern) |
| `SaveAsync` | Disk full / permissions | Throw `IOException`, caller shows error InfoBar |
| `ExportDataAsync` | Disk full / permissions | Throw `IOException`, caller shows error InfoBar |
| `MoveDataFolderAsync` | Copy fails midway | Rollback: keep original, delete partial copy, throw `IOException` |
| `MoveDataFolderAsync` | Destination is same as current | Throw `InvalidOperationException` with descriptive message |
| `MoveDataFolderAsync` | Destination not empty | Throw `InvalidOperationException`, caller shows warning dialog |
