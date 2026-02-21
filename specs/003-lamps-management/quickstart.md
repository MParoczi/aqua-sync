# Quickstart: Lamps Management

**Branch**: `003-lamps-management` | **Date**: 2026-02-21

---

## Build & Run

```bash
# Build (requires runtime identifier for WinUI3)
dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64

# Run
dotnet run --project AquaSync.App -r win-x64
```

Requires .NET SDK 10.0.101+ (pinned in `global.json`).

---

## New Files Summary

### Models (`AquaSync.App/Models/`)
| File | Purpose |
|------|---------|
| `LampConfiguration.cs` | Persisted lamp record (BLE address, mode, brightness, schedule) |
| `LampMode.cs` | `Off \| Manual \| Automatic` enum |
| `ScheduleConfiguration.cs` | Embedded schedule data (times, ramp, channels, days) |

### Service Contract (`AquaSync.App/Contracts/Services/`)
| File | Purpose |
|------|---------|
| `ILampService.cs` | Interface for all lamp operations (persistence + BLE) |

### Service Implementation (`AquaSync.App/Services/`)
| File | Purpose |
|------|---------|
| `LampService.cs` | Singleton; wraps IDataService + IDeviceScanner + IChihirosDevice cache |

### Views (`AquaSync.App/Views/`)
| File | Purpose |
|------|---------|
| `LampsPage.xaml` / `.xaml.cs` | Lamp list with Add/Remove actions (update existing placeholder) |
| `LampDetailPage.xaml` / `.xaml.cs` | Brightness sliders + schedule editor + clock sync (new) |
| `AddLampDialog.xaml` / `.xaml.cs` | ContentDialog: BLE scan + device selection (new) |

### ViewModels (`AquaSync.App/ViewModels/`)
| File | Purpose |
|------|---------|
| `LampsViewModel.cs` | Lamp list state + add/remove commands (update existing placeholder) |
| `LampDetailViewModel.cs` | Detail state: brightness, schedule, clock sync, device connection (new) |

### Controls (`AquaSync.App/Controls/`)
| File | Purpose |
|------|---------|
| `ScheduleEditorControl.xaml` / `.xaml.cs` | Custom Canvas-based 24-hour timeline control (new) |

---

## Modified Files

| File | Change |
|------|--------|
| `App.xaml.cs` | Register `ILampService`, `LampDetailViewModel`, `LampDetailPage` in DI |
| `Services/PageService.cs` | Add `Configure<LampDetailViewModel, LampDetailPage>()` |
| `ViewModels/DashboardViewModel.cs` | Add `ObservableCollection<LampCardViewModel>` + load lamps + mode toggle |
| `Views/DashboardPage.xaml` | Add lamp status cards `ItemsControl` |

---

## DI Registration (App.xaml.cs)

Add to the `ConfigureServices` block:

```csharp
// Device scanner from Chihiros library
services.AddSingleton<IDeviceScanner, DeviceScanner>();

// Lamp service (singleton — manages connection cache)
services.AddSingleton<ILampService, LampService>();

// New ViewModel + Page for lamp detail
services.AddTransient<LampDetailViewModel>();
services.AddTransient<LampDetailPage>();
```

---

## PageService Registration (Services/PageService.cs)

In the `PageService` constructor, add:

```csharp
Configure<LampDetailViewModel, LampDetailPage>();
```

---

## Navigation to Lamp Detail

From `LampsViewModel`, navigate using the lamp's `Id`:

```csharp
_navigationService.NavigateTo(
    typeof(LampDetailViewModel).FullName!,
    lampId);  // Guid passed as parameter
```

`LampDetailViewModel` implements `INavigationAware` and reads the `Guid` in `OnNavigatedTo(object parameter)`.

---

## Data Location

Lamp configurations are stored at:
```
%LOCALAPPDATA%\AquaSync\lamps\{lamp-guid}.json
```

To inspect during development: open `%LOCALAPPDATA%\AquaSync\lamps\` in Explorer.

---

## Key Dependency: AquaSync.Chihiros

The feature uses these library types directly:

| Type | Used In |
|------|---------|
| `IDeviceScanner` / `DeviceScanner` | `LampService.ScanAsync()` |
| `IChihirosDevice` / `ChihirosDevice` | `LampService.ConnectAsync()` + device command calls |
| `DeviceProfiles` | Profile lookup by model name |
| `DiscoveredDevice` | Scan result record passed to `AddLampAsync` |
| `LightSchedule` | Built from `ScheduleConfiguration` before calling `AddScheduleAsync` |
| `ColorChannel` | Channel enum for `SetBrightnessAsync` + schedule channel mapping |
| `Weekday` | Flags enum stored in `ScheduleConfiguration.ActiveDays` |

The library README at `AquaSync.Chihiros/README.md` contains the full API reference.
