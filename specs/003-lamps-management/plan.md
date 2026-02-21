# Implementation Plan: Lamps Management

**Branch**: `003-lamps-management` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-lamps-management/spec.md`

---

## Summary

Implement the Lamps Management feature for AquaSync: users can discover Chihiros LED lamps via continuous BLE scanning, assign them to aquariums, control brightness per channel, configure visual daily schedules, sync device clocks, and toggle modes from the Dashboard.

The implementation builds on the existing `AquaSync.Chihiros` library for all BLE communication and follows the WinUI3 MVVM + DI patterns established in the codebase. Key new artifacts: `LampConfiguration` model with embedded `ScheduleConfiguration`, a singleton `LampService` managing persistence and device connections, `LampsPage` / `LampDetailPage` with corresponding ViewModels, an `AddLampDialog` ContentDialog for BLE scanning, and a custom `ScheduleEditorControl` using a Canvas-based 24-hour timeline.

---

## Technical Context

**Language/Version**: C# 14, .NET 10 (`net10.0-windows10.0.19041.0`)
**Primary Dependencies**: WinUI3 (Windows App SDK), CommunityToolkit.Mvvm 8.4, `AquaSync.Chihiros` (project reference — no new NuGet packages)
**Storage**: JSON files via `IDataService`, folder `lamps/`, path `{dataRoot}/lamps/{id}.json`
**Testing**: No test infrastructure present in the project (out of scope)
**Target Platform**: Windows 11 desktop (WinUI3)
**Project Type**: Single WinUI3 app (`AquaSync.App`)
**Performance Goals**: BLE brightness response < 1 s (SC-003), schedule write < 5 s (SC-004), clock sync < 3 s (SC-005), mode change < 2 s (SC-006)
**Constraints**: Local-only (no cloud), standard WinUI3 controls only, `TreatWarningsAsErrors` enabled, no new NuGet packages

---

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Windows-Native Design | ✅ PASS | All UI uses standard WinUI3 controls. `AddLampDialog` uses `ContentDialog`. Schedule timeline uses `Canvas` (WinUI3 primitive). `InfoBar` for errors. Segoe Fluent Icons throughout. |
| II. Local-First Data | ✅ PASS | Lamp configs stored as JSON via `IDataService` at `lamps/`. No cloud, no network storage. |
| III. MVVM Architecture | ✅ PASS | `LampsViewModel`, `LampDetailViewModel`, `DashboardViewModel` all inherit `ViewModelBase`. Manual `SetProperty` + `RelayCommand`. DI registration in `App.xaml.cs`. |
| IV. Device Integration via Existing Libraries | ✅ PASS | Exclusively uses `IDeviceScanner`, `IChihirosDevice`, `DeviceProfiles`, `LightSchedule`, `Weekday` from `AquaSync.Chihiros`. No duplication or rewrite. BLE events marshalled via `DispatcherQueue.TryEnqueue()`. |
| V. Minimal Dependencies | ✅ PASS | Zero new NuGet packages. All functionality available via existing `AquaSync.Chihiros`, `CommunityToolkit.Mvvm`, and WinUI3 framework APIs. |
| VI. Single-Aquarium Context | ✅ PASS | All lamp operations scoped via `IAquariumContext.CurrentAquarium.Id`. No cross-aquarium views. |
| VII. English Only | ✅ PASS | No localization infrastructure. All strings hardcoded in English. |

**No violations. No Complexity Tracking entries required.**

---

## Project Structure

### Documentation (this feature)

```text
specs/003-lamps-management/
├── plan.md              ← this file
├── research.md          ← Phase 0 complete
├── data-model.md        ← Phase 1 complete
├── quickstart.md        ← Phase 1 complete
├── contracts/
│   └── ILampService.md  ← Phase 1 complete
└── tasks.md             ← Phase 2 output (/speckit.tasks — not yet created)
```

### Source Code Changes

```text
AquaSync.App/
├── App.xaml.cs                                  [MODIFY] register IDeviceScanner, ILampService,
│                                                         LampDetailViewModel, LampDetailPage
├── Models/
│   ├── LampConfiguration.cs                     [NEW]
│   ├── LampMode.cs                              [NEW]
│   └── ScheduleConfiguration.cs                [NEW]
├── Contracts/Services/
│   └── ILampService.cs                          [NEW]
├── Services/
│   ├── LampService.cs                           [NEW]
│   └── PageService.cs                           [MODIFY] Configure<LampDetailViewModel, LampDetailPage>()
├── Views/
│   ├── LampsPage.xaml                           [MODIFY] implement from placeholder
│   ├── LampsPage.xaml.cs                        [no change]
│   ├── LampDetailPage.xaml                      [NEW]
│   ├── LampDetailPage.xaml.cs                   [NEW]
│   ├── AddLampDialog.xaml                       [NEW]
│   ├── AddLampDialog.xaml.cs                    [NEW]
│   ├── DashboardPage.xaml                       [MODIFY] add lamp status cards section
│   └── DashboardPage.xaml.cs                    [no change]
├── ViewModels/
│   ├── LampsViewModel.cs                        [MODIFY] implement from placeholder
│   ├── LampDetailViewModel.cs                   [NEW]
│   └── DashboardViewModel.cs                    [MODIFY] add LampCardViewModel + lamp loading
└── Controls/
    ├── ScheduleEditorControl.xaml               [NEW] (Controls/ folder is also new)
    └── ScheduleEditorControl.xaml.cs            [NEW]
```

**Structure Decision**: Single WinUI3 app project. Follows existing folder conventions: `Models/`, `Services/`, `Contracts/Services/`, `Views/`, `ViewModels/`. New `Controls/` folder added for the custom schedule timeline control.

---

## Phase 0: Research (Complete)

See [`research.md`](research.md) for all resolved decisions. Key findings:

1. **Scanner method**: Use `DeviceScanner.ScanAsync()` (not `ScanByServiceAsync` — that method doesn't exist; `ScanAsync` filters by UART service UUID).
2. **Indefinite scan**: `ScanAsync(TimeSpan.FromMinutes(10), progress, cancellationToken)` + cancel token on Stop.
3. **Connection model**: On-demand, cached in `LampService` while detail view is open.
4. **Address serialization**: `ulong` → 12-char hex string via `.ToString("X12")`.
5. **Channel dict keys**: `ColorChannel.ToString()` string keys in JSON.
6. **Timeline control**: `Canvas`-based `UserControl` with `DependencyProperty` and pointer drag events.
7. **Add lamp UI**: `ContentDialog` with scan lifecycle in code-behind.

---

## Phase 1: Design & Contracts (Complete)

See [`data-model.md`](data-model.md), [`contracts/ILampService.md`](contracts/ILampService.md), [`quickstart.md`](quickstart.md).

### Models

#### `LampConfiguration`
```csharp
// AquaSync.App/Models/LampConfiguration.cs
namespace AquaSync.App.Models;

public sealed class LampConfiguration
{
    public Guid Id { get; set; }
    public Guid AquariumId { get; set; }
    public string BluetoothAddress { get; set; } = string.Empty;   // 12-char hex
    public string DeviceName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;          // empty = unmanaged
    public LampMode Mode { get; set; } = LampMode.Off;
    public Dictionary<string, byte> ManualBrightness { get; set; } = [];
    public ScheduleConfiguration? Schedule { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

#### `LampMode`
```csharp
// AquaSync.App/Models/LampMode.cs
namespace AquaSync.App.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LampMode { Off, Manual, Automatic }
```

#### `ScheduleConfiguration`
```csharp
// AquaSync.App/Models/ScheduleConfiguration.cs
namespace AquaSync.App.Models;
using AquaSync.Chihiros.Scheduling;

public sealed class ScheduleConfiguration
{
    public TimeOnly Sunrise { get; set; }
    public TimeOnly Sunset { get; set; }
    public int RampUpMinutes { get; set; }
    public Dictionary<string, byte> ChannelBrightness { get; set; } = [];
    public Weekday ActiveDays { get; set; }
}
```

### Service Interface

#### `ILampService`
Full contract defined in [`contracts/ILampService.md`](contracts/ILampService.md). Signature summary:

```csharp
// AquaSync.App/Contracts/Services/ILampService.cs
public interface ILampService
{
    Task<IReadOnlyList<LampConfiguration>> GetLampsForAquariumAsync(Guid aquariumId, CancellationToken ct = default);
    Task<bool> IsAddressAssignedAsync(string bluetoothAddress, CancellationToken ct = default);
    Task<LampConfiguration> AddLampAsync(Guid aquariumId, DiscoveredDevice device, CancellationToken ct = default);
    Task RemoveLampAsync(Guid lampId, CancellationToken ct = default);
    Task SaveModeAsync(Guid lampId, LampMode mode, CancellationToken ct = default);
    Task SaveManualBrightnessAsync(Guid lampId, Dictionary<string, byte> brightness, CancellationToken ct = default);
    Task SaveScheduleAsync(Guid lampId, ScheduleConfiguration schedule, CancellationToken ct = default);

    Task ScanAsync(IProgress<DiscoveredDevice> progress, CancellationToken ct = default);
    DeviceProfile? GetProfileForModel(string modelName);

    Task<IChihirosDevice?> ConnectAsync(LampConfiguration lamp, CancellationToken ct = default);
    Task DisconnectAsync(string bluetoothAddress);
}
```

### ViewModel Designs

#### `LampsViewModel`
```
Dependencies: ILampService, IAquariumContext, INavigationService

Observable properties:
  ObservableCollection<LampConfiguration> Lamps
  bool IsEmpty                          // Lamps.Count == 0
  bool IsBusy                           // loading indicator

Commands:
  RelayCommand OpenAddLampDialogCommand
  RelayCommand<LampConfiguration> SelectLampCommand    // navigate to detail
  RelayCommand<LampConfiguration> RemoveLampCommand    // confirm + remove

INavigationAware:
  OnNavigatedTo  → LoadLampsAsync(CurrentAquarium.Id)
  OnNavigatedFrom → (no-op)
```

#### `LampDetailViewModel`
```
Dependencies: ILampService, IAquariumContext, INavigationService, DispatcherQueue

Observable properties:
  LampConfiguration? Lamp
  IChihirosDevice? Device               // null = unmanaged or disconnected
  bool IsConnected
  bool IsConnecting
  bool IsUnmanaged                      // ModelName is empty
  ObservableCollection<ChannelSlider> Channels  // per device profile
  bool IsBrightnessApplying
  // Schedule editor bindings (delegated to ScheduleEditorControl via DependencyProperty)
  TimeOnly? ScheduleSunrise
  TimeOnly? ScheduleSunset
  int ScheduleRampUpMinutes
  Dictionary<string, byte> ScheduleChannelBrightness
  Weekday ScheduleActiveDays
  bool IsScheduleValid                  // drives Save button enabled state
  bool IsSavingSchedule
  bool IsClockSyncing
  string? ErrorMessage
  bool IsErrorOpen

Commands:
  RelayCommand<ChannelSlider> ApplyBrightnessCommand
  RelayCommand SaveScheduleCommand
  RelayCommand SyncClockCommand

INavigationAware:
  OnNavigatedTo(object param)  → lampId = (Guid)param → load config + connect
  OnNavigatedFrom()            → DisconnectAsync + clear event subscriptions

Inner type: ChannelSlider { ColorChannel Channel, string Label, byte Value (SetProperty) }
```

#### `DashboardViewModel` (additions)
```
New observable properties:
  ObservableCollection<LampCardViewModel> LampCards

New commands:
  RelayCommand<(Guid lampId, LampMode mode)> SetLampModeCommand

Inner type: LampCardViewModel
  string DisplayName, ModelName
  LampMode CurrentMode
  bool IsConnecting
  RelayCommand<LampMode> SetModeCommand
```

### View Designs

#### `LampsPage.xaml`
```
Layout: Grid (2 rows)
  Row 0: Page header ("Lamps") + Add button (right-aligned, FontIcon E710 plus)
  Row 1: ListView bound to Lamps collection
           Empty state overlay (Visibility bound to IsEmpty)

  ListView ItemTemplate:
    Grid (2 columns):
      Col 0: StackPanel → TextBlock (ModelName, Bold) + TextBlock (DeviceName, Subtle)
      Col 1: StackPanel → TextBlock (Mode string) + TextBlock (connection state)
    SwipeControl or context menu: Remove action

  Footer area: InfoBar for errors
```

#### `LampDetailPage.xaml`
```
Layout: ScrollViewer > StackPanel (Spacing=24, Padding=24)
  1. Page header row: Back button + lamp name + model (or "Unknown Model" badge)
  2. Manual Brightness section (hidden if IsUnmanaged):
       SectionHeader "Manual Brightness"
       ItemsControl bound to Channels:
         DataTemplate: Grid → TextBlock (channel name) + Slider (0-100) + TextBlock (value%)
  3. Schedule section (hidden if IsUnmanaged):
       SectionHeader "Schedule"
       ScheduleEditorControl (custom, bound to schedule properties)
       NumberBox for RampUpMinutes (0-150)
       ItemsControl for weekday checkboxes (7 CheckBox items bound to individual day flags)
       StackPanel for per-channel peak brightness sliders
       Save Schedule Button (disabled if not IsScheduleValid)
  4. Clock Sync section:
       SectionHeader "Device Clock"
       TextBlock "Synchronize the lamp's internal clock with system time"
       Button "Sync Clock" + loading ring
  5. Connecting overlay (Visibility bound to IsConnecting)
  6. Unmanaged notice (Visibility bound to IsUnmanaged): InfoBar warning
  7. Error InfoBar
```

#### `AddLampDialog.xaml`
```
ContentDialog:
  Title: "Add Lamp"
  PrimaryButtonText: "Add" (disabled until a device is selected)
  CloseButtonText: "Cancel"

  Content:
    StackPanel:
      ProgressRing (IsActive=scanning, IsIndeterminate=true)
      TextBlock "Scanning for nearby Chihiros devices..." (visible while scanning)
      ListView of DiscoveredDeviceItem:
        DataTemplate:
          Grid (2 cols):
            Col 0: StackPanel → TextBlock (Name, Bold) + TextBlock (ModelName or "Unknown Model")
            Col 1: StackPanel → signal strength icon (FontIcon) + TextBlock (dBm)
          IsEnabled bound to IsAvailable
          Opacity: 1.0 if available, 0.5 if unavailable
      TextBlock "Already assigned to another aquarium" footnote (below list)
```

#### `ScheduleEditorControl.xaml`
```
UserControl:
  Grid:
    Canvas x:Name="TimelineCanvas" (Height=48, SizeChanged handler)
      Rectangle x:Name="OffSegmentLeft"   (night before sunrise)
      Rectangle x:Name="RampSegment"      (sunrise ramp-up)
      Rectangle x:Name="OnSegment"        (peak on-period)
      Rectangle x:Name="OffSegmentRight"  (night after sunset)
      Ellipse x:Name="SunriseHandle"      (draggable, PointerPressed/Moved/Released)
      Ellipse x:Name="SunsetHandle"       (draggable, PointerPressed/Moved/Released)
    Grid (below canvas): time labels (sunrise time + sunset time TextBlocks)

DependencyProperties:
  SunriseTime: TimeOnly?   (two-way, PropertyChangedCallback → RedrawTimeline)
  SunsetTime: TimeOnly?    (two-way, PropertyChangedCallback → RedrawTimeline)
  RampUpMinutes: int       (one-way-to-source, PropertyChangedCallback → RedrawTimeline)

Code-behind:
  _draggingHandle: Ellipse? (null = not dragging)
  OnPointerPressed  → capture pointer, set _draggingHandle
  OnPointerMoved    → convert pointer X to TimeOnly, update property, RedrawTimeline
  OnPointerReleased → release capture, clear _draggingHandle
  RedrawTimeline()  → compute segment widths from canvas.ActualWidth, update Rectangle.Width/Canvas.SetLeft
```

#### `DashboardPage.xaml` (additions)
```
Add new section before existing content (or as first section):
  SectionHeader "Lamps"
  ItemsControl bound to LampCards:
    DataTemplate: LampCardView
      CardControl (Border with card styling):
        Grid:
          Row 0: TextBlock (DisplayName, Bold) + TextBlock (ModelName, Subtle)
          Row 1: StackPanel (Horizontal):
            RadioButtons or SegmentedControl: Off | Manual | Automatic
            ProgressRing (IsActive=IsConnecting, small)
  Empty state: TextBlock "No lamps assigned" (Visibility=IsEmpty)
```

### DI & Registration Changes

**`App.xaml.cs`** — add to ConfigureServices:
```csharp
services.AddSingleton<IDeviceScanner, DeviceScanner>();
services.AddSingleton<ILampService, LampService>();
services.AddTransient<LampDetailViewModel>();
services.AddTransient<LampDetailPage>();
```

**`Services/PageService.cs`** — add in constructor:
```csharp
Configure<LampDetailViewModel, LampDetailPage>();
```

---

## Implementation Order (for /speckit.tasks)

The following ordering minimizes blocking dependencies:

1. **Models**: `LampMode`, `LampConfiguration`, `ScheduleConfiguration` — no dependencies
2. **ILampService contract** — depends on models
3. **LampService implementation** — depends on interface + `IDataService` + `IDeviceScanner`
4. **DI registration** (`App.xaml.cs` + `PageService.cs`) — depends on service
5. **LampsViewModel** — depends on `ILampService`, `IAquariumContext`, `INavigationService`
6. **LampsPage.xaml** — depends on ViewModel
7. **LampDetailViewModel** — depends on `ILampService`, `IChihirosDevice`
8. **ScheduleEditorControl** — no ViewModel dependency (pure UI control)
9. **LampDetailPage.xaml** — depends on ViewModel + ScheduleEditorControl
10. **AddLampDialog** — depends on `ILampService`
11. **DashboardViewModel additions** — depends on `ILampService`
12. **DashboardPage.xaml additions** — depends on DashboardViewModel

---

## Open Questions / Notes for Implementation

- **`LampDetailPage` back navigation**: Use `INavigationService.GoBack()` from the page header back button; the back button visibility mirrors `INavigationService.CanGoBack`. This is consistent with how `SettingsPage.xaml` handles back navigation.
- **ScheduleEditorControl blank state**: When `SunriseTime == null` and `SunsetTime == null`, render the full bar as off-period (dark). Drag handles are hidden until the user places them (first drag creates the initial position).
- **Weekday checkboxes binding**: `Weekday` is a `[Flags]` enum. Individual day binding requires a `WeekdayFlagConverter : IValueConverter` that tests/sets individual bits. This converter is needed in `LampDetailPage.xaml`.
- **`ScheduleEditorControl` initial drag**: On first drag of sunrise handle, set `SunriseTime` to the dragged position. Before any drag, the control shows the blank (all-off) state.
- **`INavigationAware` on LampDetailViewModel**: `ShellPage` or `NavigationService` must call `OnNavigatedTo`/`OnNavigatedFrom`. Verify how the existing `INavigationAware` contract is invoked — check `NavigationService.cs` and `ShellPage.xaml.cs` to confirm the invocation pattern before implementing.
