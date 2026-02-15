# AquaSync.Eheim

A .NET 10 class library for controlling EHEIM Digital aquarium equipment over a local network. Communicates with the EHEIM hub via WebSocket and exposes a reactive, observable-based API built on `System.Reactive`.

Currently supports the **EHEIM professionel 5e** filter (models 350, 450, 600T, 700).

## Table of Contents

- [Installation](#installation)
- [Architecture Overview](#architecture-overview)
- [Quick Start](#quick-start)
- [Discovery](#discovery)
- [Connecting to the Hub](#connecting-to-the-hub)
- [Working with Devices](#working-with-devices)
- [Filter Properties Reference](#filter-properties-reference)
- [Filter Commands Reference](#filter-commands-reference)
- [Subscribing to Property Changes](#subscribing-to-property-changes)
- [WinUI 3 / XAML Integration](#winui-3--xaml-integration)
- [Error Handling](#error-handling)
- [Connection Lifecycle](#connection-lifecycle)
- [Thread Safety and UI Threading](#thread-safety-and-ui-threading)
- [Testability](#testability)
- [Protocol Details](#protocol-details)

---

## Installation

Add a project reference from your WinUI 3 app to the class library:

```xml
<ProjectReference Include="..\AquaSync.Eheim\AquaSync.Eheim.csproj" />
```

Or via the CLI:

```bash
dotnet add reference ../AquaSync.Eheim/AquaSync.Eheim.csproj
```

The library depends on two NuGet packages (restored automatically):

| Package | Purpose |
|---|---|
| `System.Reactive` | `BehaviorSubject<T>`, `IObservable<T>` infrastructure |
| `Zeroconf` | mDNS/Zeroconf network discovery |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│  Your WinUI 3 Application                       │
│                                                  │
│  Subscribes to IObservable<T> properties         │
│  Calls async command methods                     │
└────────────┬────────────────────────┬────────────┘
             │                        │
     IEheimDiscoveryService       IEheimHub
             │                        │
             │                  IEheimFilter
             │                        │
┌────────────┴────────────────────────┴────────────┐
│  AquaSync.Eheim (this library)                   │
│                                                  │
│  Discovery ──► mDNS scan for EHEIM hubs          │
│  Hub ────────► WebSocket at ws://{host}/ws       │
│  Devices ───► BehaviorSubject-backed observables │
│  Protocol ──► JSON message builder & parser      │
└──────────────────────────────────────────────────┘
             │
     WebSocket (JSON)
             │
┌────────────┴─────────────────────────────────────┐
│  EHEIM Digital Hub (physical device)             │
│  └── professionel 5e filter(s) on mesh network   │
└──────────────────────────────────────────────────┘
```

**Key design choices:**

- Every readable property is an `IObservable<T>` backed by a `BehaviorSubject<T>`, meaning new subscribers immediately receive the current value.
- All values are normalized to metric units (Hz for frequency, L/h for flow rates).
- The library does **not** auto-reconnect or marshal to a UI thread — both are the consumer's responsibility.
- All public types are exposed through interfaces (`IEheimHub`, `IEheimFilter`, `IEheimDevice`, `IEheimDiscoveryService`) for testability.

---

## Quick Start

```csharp
using AquaSync.Eheim;
using AquaSync.Eheim.Devices;
using AquaSync.Eheim.Devices.Enums;

// 1. Connect to the hub
var hub = new EheimHub("192.168.1.50");
await hub.ConnectAsync();

// 2. Listen for filter devices as they are discovered
hub.DeviceDiscovered.Subscribe(device =>
{
    if (device is IEheimFilter filter)
    {
        Console.WriteLine($"Found filter: {filter.Name} ({filter.FilterModel})");

        // 3. Subscribe to live property updates
        filter.CurrentSpeed.Subscribe(hz =>
            Console.WriteLine($"Current speed: {hz} Hz"));

        filter.IsActive.Subscribe(active =>
            Console.WriteLine($"Filter active: {active}"));

        filter.FilterMode.Subscribe(mode =>
            Console.WriteLine($"Mode: {mode}"));
    }
});

// 4. Send commands
// (assuming you have a reference to the filter from the subscription above)
// await filter.SetActiveAsync(true);
// await filter.SetFilterModeAsync(FilterMode.Bio);
// await filter.SetDaySpeedAsync(7);

// 5. Clean up when done
await hub.DisposeAsync();
```

---

## Discovery

Use `EheimDiscoveryService` to find EHEIM hubs on your local network via mDNS. The hub advertises itself as `eheimdigital._http._tcp.local.`.

```csharp
using AquaSync.Eheim.Discovery;

var discovery = new EheimDiscoveryService();

// Scan for 5 seconds — emits each hub as it is found
discovery.Scan(TimeSpan.FromSeconds(5)).Subscribe(
    hub => Console.WriteLine($"Found hub: {hub.Name} at {hub.IpAddress}"),
    () => Console.WriteLine("Scan complete"));
```

The `DiscoveredHub` record contains:

| Property | Type | Description |
|---|---|---|
| `Host` | `string` | mDNS hostname (e.g., `eheimdigital`) |
| `IpAddress` | `string` | Resolved IP address (e.g., `192.168.1.50`) |
| `Name` | `string` | Display name of the hub |

You can then pass `IpAddress` or `Host` to the `EheimHub` constructor.

---

## Connecting to the Hub

```csharp
// Connect using an IP address
var hub = new EheimHub("192.168.1.50");

// Or using the default mDNS hostname
var hub = new EheimHub("eheimdigital.local");

// Or with the default hostname (eheimdigital.local)
var hub = new EheimHub();

// Connect — throws EheimConnectionException on failure
await hub.ConnectAsync();

// Optionally pass a CancellationToken
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
await hub.ConnectAsync(cts.Token);
```

Once connected, the hub automatically:
1. Sends a `GET_USRDTA` broadcast to discover all devices on the mesh.
2. Receives `MESH_NETWORK` and `USRDTA` responses.
3. For each filter device, requests `FILTER_DATA` to determine the model.
4. Creates an `IEheimFilter` instance and emits it via `DeviceDiscovered`.
5. Begins streaming real-time state updates to the filter's observables.

---

## Working with Devices

### Accessing discovered devices

```csharp
// Reactive — get notified as devices appear
hub.DeviceDiscovered.Subscribe(device =>
{
    if (device is IEheimFilter filter)
    {
        // Use the filter
    }
});

// Imperative — access all currently known devices
foreach (var (mac, device) in hub.Devices)
{
    Console.WriteLine($"{mac}: {device.Name} ({device.ModelName})");
}
```

### Common device properties (IEheimDevice)

Every device exposes these properties:

| Property | Type | Description |
|---|---|---|
| `MacAddress` | `string` | Hardware MAC address (unique identifier) |
| `Name` | `string` | User-assigned device name |
| `ModelName` | `string` | Model name (e.g., `Filter350`) |
| `FirmwareVersion` | `string` | Firmware version string |
| `AquariumName` | `string` | User-assigned aquarium name |
| `SystemLedBrightness` | `IObservable<int>` | Status LED brightness (0–100%) |

```csharp
// Read the system LED brightness
filter.SystemLedBrightness.Subscribe(brightness =>
    Console.WriteLine($"LED: {brightness}%"));

// Set it
await filter.SetSystemLedBrightnessAsync(50);
```

---

## Filter Properties Reference

All `IObservable<T>` properties emit the current value immediately upon subscription (BehaviorSubject semantics), then emit again whenever the device reports an update.

### State Properties

| Property | Type | Unit | Description |
|---|---|---|---|
| `IsActive` | `IObservable<bool>` | — | Whether the filter pump is running |
| `CurrentSpeed` | `IObservable<double>` | Hz | Current pump frequency |
| `FilterMode` | `IObservable<FilterMode>` | — | Active operation mode |
| `ServiceHours` | `IObservable<double>` | hours | Hours until next service |
| `OperatingTime` | `IObservable<TimeSpan>` | — | Total operating time |

### Manual Mode Properties

| Property | Type | Unit | Description |
|---|---|---|---|
| `ManualSpeed` | `IObservable<double>` | Hz | Target frequency in manual mode |
| `AvailableManualSpeeds` | `IReadOnlyList<double>` | Hz | Valid frequency values for this filter model |

### Constant Flow Mode Properties

| Property | Type | Unit | Description |
|---|---|---|---|
| `ConstantFlowIndex` | `IObservable<int>` | index 0–14 | Current flow rate index |
| `AvailableFlowRates` | `IReadOnlyList<int>` | L/h | Flow rates in liters/hour for each index |

To display the actual flow rate in L/h:

```csharp
filter.ConstantFlowIndex.Subscribe(index =>
{
    int litersPerHour = filter.AvailableFlowRates[index];
    Console.WriteLine($"Flow rate: {litersPerHour} L/h");
});
```

### Bio Mode Properties

| Property | Type | Unit | Description |
|---|---|---|---|
| `DaySpeed` | `IObservable<int>` | flow index 0–14 | Day flow rate index |
| `NightSpeed` | `IObservable<int>` | flow index 0–14 | Night flow rate index |
| `DayStartTime` | `IObservable<TimeOnly>` | — | When the day cycle starts |
| `NightStartTime` | `IObservable<TimeOnly>` | — | When the night cycle starts |

### Pulse Mode Properties

| Property | Type | Unit | Description |
|---|---|---|---|
| `HighPulseSpeed` | `IObservable<int>` | flow index 0–14 | High pulse flow rate index |
| `LowPulseSpeed` | `IObservable<int>` | flow index 0–14 | Low pulse flow rate index |
| `HighPulseTime` | `IObservable<TimeSpan>` | — | Duration of the high pulse phase |
| `LowPulseTime` | `IObservable<TimeSpan>` | — | Duration of the low pulse phase |

### Filter Model Information

| Property | Type | Description |
|---|---|---|
| `FilterModel` | `EheimFilterModel` | The hardware model variant |

`EheimFilterModel` values: `Filter350`, `Filter450`, `Filter600T`, `Filter700`.

---

## Filter Commands Reference

All commands are async, accept an optional `CancellationToken`, and throw `EheimCommunicationException` on failure.

Commands that modify mode-specific settings (bio, pulse) use a **read-modify-write** pattern internally: they read the current state, change only the specified value, and send the full command to the device. This means you can safely call individual setters without losing other settings.

### General

```csharp
// Turn the filter on or off
await filter.SetActiveAsync(true);
await filter.SetActiveAsync(false);

// Switch operating mode
await filter.SetFilterModeAsync(FilterMode.Manual);
await filter.SetFilterModeAsync(FilterMode.ConstantFlow);
await filter.SetFilterModeAsync(FilterMode.Pulse);
await filter.SetFilterModeAsync(FilterMode.Bio);
```

### Manual Mode

```csharp
// Set pump frequency in Hz (must be one of AvailableManualSpeeds)
await filter.SetManualSpeedAsync(56.0);

// List valid speeds for this filter model
foreach (double hz in filter.AvailableManualSpeeds)
    Console.WriteLine($"{hz} Hz");
```

### Constant Flow Mode

```csharp
// Set by flow rate index (0–14)
await filter.SetConstantFlowAsync(7);

// Translate index to L/h for display
int litersPerHour = filter.AvailableFlowRates[7]; // e.g., 785 L/h for Filter 600T
```

### Bio Mode

```csharp
// Each setter independently modifies one parameter, preserving the others
await filter.SetDaySpeedAsync(10);                              // flow index
await filter.SetNightSpeedAsync(5);                             // flow index
await filter.SetDayStartTimeAsync(new TimeOnly(8, 0));          // 08:00
await filter.SetNightStartTimeAsync(new TimeOnly(20, 0));       // 20:00
```

### Pulse Mode

```csharp
await filter.SetHighPulseSpeedAsync(12);                        // flow index
await filter.SetLowPulseSpeedAsync(4);                          // flow index
await filter.SetHighPulseTimeAsync(TimeSpan.FromSeconds(30));   // high phase duration
await filter.SetLowPulseTimeAsync(TimeSpan.FromSeconds(15));    // low phase duration
```

### System LED

```csharp
// Set the physical status LED brightness (0–100%)
await filter.SetSystemLedBrightnessAsync(75);
```

---

## Subscribing to Property Changes

Since every property is an `IObservable<T>` backed by `BehaviorSubject<T>`, subscribing always delivers the current value first, then subsequent updates.

### Basic subscription

```csharp
IDisposable subscription = filter.CurrentSpeed.Subscribe(
    onNext: hz => Console.WriteLine($"Speed: {hz} Hz"),
    onError: ex => Console.WriteLine($"Error: {ex.Message}"));

// Later: unsubscribe
subscription.Dispose();
```

### Combining multiple properties

```csharp
using System.Reactive.Linq;

// React when both mode AND speed change
filter.FilterMode
    .CombineLatest(filter.CurrentSpeed, (mode, speed) => new { mode, speed })
    .Subscribe(x => Console.WriteLine($"Mode: {x.mode}, Speed: {x.speed} Hz"));
```

### Filtering and throttling

```csharp
using System.Reactive.Linq;

// Only react when the filter stops
filter.IsActive
    .Where(active => !active)
    .Subscribe(_ => Console.WriteLine("Filter stopped!"));

// Throttle rapid updates (e.g., for UI display)
filter.CurrentSpeed
    .Throttle(TimeSpan.FromMilliseconds(500))
    .Subscribe(hz => UpdateSpeedLabel(hz));
```

### Collecting the latest snapshot

```csharp
using System.Reactive.Linq;

// Get the current value once (blocking)
bool isActive = await filter.IsActive.FirstAsync();
double speed = await filter.CurrentSpeed.FirstAsync();

// Get it with a timeout
double speed = await filter.CurrentSpeed
    .Timeout(TimeSpan.FromSeconds(5))
    .FirstAsync();
```

---

## WinUI 3 / XAML Integration

Since observables don't directly bind to XAML, you'll typically bridge them to `INotifyPropertyChanged` in your ViewModel.

### ViewModel pattern

```csharp
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

public class FilterViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IEheimFilter _filter;
    private readonly CompositeDisposable _subscriptions = new();

    private double _currentSpeed;
    public double CurrentSpeed
    {
        get => _currentSpeed;
        private set { _currentSpeed = value; OnPropertyChanged(); }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        private set { _isActive = value; OnPropertyChanged(); }
    }

    private FilterMode _filterMode;
    public FilterMode FilterMode
    {
        get => _filterMode;
        private set { _filterMode = value; OnPropertyChanged(); }
    }

    public FilterViewModel(IEheimFilter filter, SynchronizationContext uiContext)
    {
        _filter = filter;

        // ObserveOn ensures the setter runs on the UI thread
        _subscriptions.Add(
            filter.CurrentSpeed
                .ObserveOn(uiContext)
                .Subscribe(v => CurrentSpeed = v));

        _subscriptions.Add(
            filter.IsActive
                .ObserveOn(uiContext)
                .Subscribe(v => IsActive = v));

        _subscriptions.Add(
            filter.FilterMode
                .ObserveOn(uiContext)
                .Subscribe(v => FilterMode = v));
    }

    // Commands for the UI
    public async Task ToggleFilterAsync()
        => await _filter.SetActiveAsync(!IsActive);

    public async Task SetModeAsync(FilterMode mode)
        => await _filter.SetFilterModeAsync(mode);

    public void Dispose() => _subscriptions.Dispose();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### Capturing the SynchronizationContext

In your WinUI 3 page or App.xaml.cs, capture the context before creating the ViewModel:

```csharp
// In your Page code-behind or App startup (must be called on the UI thread)
var uiContext = SynchronizationContext.Current!;

var vm = new FilterViewModel(filter, uiContext);
this.DataContext = vm;
```

---

## Error Handling

The library throws two exception types:

### EheimConnectionException

Thrown when the WebSocket connection cannot be established.

```csharp
try
{
    await hub.ConnectAsync();
}
catch (EheimConnectionException ex)
{
    // Network unreachable, hub offline, DNS resolution failed, etc.
    Console.WriteLine($"Connection failed: {ex.Message}");
}
```

### EheimCommunicationException

Thrown when a command cannot be sent or the device is in an unexpected state.

```csharp
try
{
    await filter.SetManualSpeedAsync(56.0);
}
catch (EheimCommunicationException ex)
{
    // WebSocket disconnected mid-command, or no filter data received yet
    Console.WriteLine($"Command failed: {ex.Message}");
}
```

A common cause of `EheimCommunicationException` is sending a command before the device has reported its initial state. The read-modify-write setters (bio mode, pulse mode) require at least one `FILTER_DATA` message to have been received. In practice this happens within a second of connection, but if you need to send commands immediately after discovery, subscribe to any property first and wait for the initial value:

```csharp
// Wait until the filter has reported its state
await filter.IsActive.FirstAsync();

// Now commands are safe
await filter.SetFilterModeAsync(FilterMode.Bio);
```

---

## Connection Lifecycle

The library does **not** automatically reconnect if the WebSocket drops. This is by design — reconnection strategy depends on your application (retry with backoff, prompt the user, etc.).

### Detecting disconnection

Check `hub.IsConnected` or wrap commands in try/catch:

```csharp
if (!hub.IsConnected)
{
    // Reconnect
    await hub.ConnectAsync();
}
```

### Manual reconnection pattern

```csharp
async Task MaintainConnectionAsync(EheimHub hub, CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        try
        {
            if (!hub.IsConnected)
            {
                await hub.ConnectAsync(ct);
            }
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
        catch (EheimConnectionException)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }
    }
}
```

### Requesting a full refresh

After reconnection or at any time, you can request all devices to resend their state:

```csharp
await hub.RefreshAsync();
```

This sends `GET_USRDTA` to all devices and requests device-specific data updates. All observables will emit fresh values.

### Cleanup

Always dispose the hub when your application exits:

```csharp
await hub.DisposeAsync();
```

This closes the WebSocket connection and completes all internal subscriptions.

---

## Thread Safety and UI Threading

- **Hub and device methods** are safe to call from any thread.
- **Observable emissions** arrive on a background thread (the WebSocket receive loop).
- **You must marshal to the UI thread** before updating UI-bound properties.

Use `ObserveOn` with a `SynchronizationContext` or `DispatcherQueue`:

```csharp
using System.Reactive.Linq;

// Option 1: SynchronizationContext (recommended)
var uiContext = SynchronizationContext.Current!;
filter.CurrentSpeed
    .ObserveOn(uiContext)
    .Subscribe(hz => SpeedLabel.Text = $"{hz} Hz");

// Option 2: DispatcherQueue (WinUI 3 specific)
filter.CurrentSpeed.Subscribe(hz =>
{
    dispatcherQueue.TryEnqueue(() => SpeedLabel.Text = $"{hz} Hz");
});
```

---

## Testability

All public types are interfaces. The hub also accepts an internal `IEheimTransport` for unit testing without a real WebSocket.

### Mocking IEheimFilter in your ViewModel tests

```csharp
using System.Reactive.Subjects;
using Moq;

var mockFilter = new Mock<IEheimFilter>();

// Set up observable properties with controllable subjects
var speedSubject = new BehaviorSubject<double>(45.0);
mockFilter.Setup(f => f.CurrentSpeed).Returns(speedSubject.AsObservable());

var activeSubject = new BehaviorSubject<bool>(true);
mockFilter.Setup(f => f.IsActive).Returns(activeSubject.AsObservable());

// Use the mock in your ViewModel
var vm = new FilterViewModel(mockFilter.Object, SynchronizationContext.Current!);

// Simulate a device update
speedSubject.OnNext(56.0);
Assert.AreEqual(56.0, vm.CurrentSpeed);
```

### Mocking IEheimHub

```csharp
var mockHub = new Mock<IEheimHub>();
var discoverySubject = new Subject<IEheimDevice>();

mockHub.Setup(h => h.DeviceDiscovered).Returns(discoverySubject.AsObservable());
mockHub.Setup(h => h.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

// Simulate a device being discovered
discoverySubject.OnNext(mockFilter.Object);
```

---

## Protocol Details

For contributors or anyone debugging communication issues, here is a summary of the underlying WebSocket protocol.

### Connection

- **Endpoint:** `ws://{host}/ws`
- **Authentication:** None
- **Message format:** JSON (single object or array of objects)

### Message structure

Every message has three standard fields:

```json
{ "title": "MESSAGE_TYPE", "to": "TARGET_MAC", "from": "USER_OR_DEVICE_MAC" }
```

- Client always sends `"from": "USER"`.
- Devices respond with their MAC address in `"from"`.
- `"to"` is a specific MAC address or `"ALL"` for broadcast.

### Discovery sequence

```
Client → Hub:  {"title":"GET_USRDTA","to":"ALL","from":"USER"}
Hub → Client:  {"title":"MESH_NETWORK","clientList":["AA:BB:CC:DD:EE:FF",...]}
Client → Hub:  {"title":"GET_USRDTA","to":"AA:BB:CC:DD:EE:FF","from":"USER"}
Hub → Client:  {"title":"USRDTA","from":"AA:BB:CC:DD:EE:FF","version":4,"name":"My Filter",...}
Client → Hub:  {"title":"GET_FILTER_DATA","to":"AA:BB:CC:DD:EE:FF","from":"USER"}
Hub → Client:  {"title":"FILTER_DATA","from":"AA:BB:CC:DD:EE:FF","freq":5600,"pumpMode":16,...}
```

### Data encoding conventions

| Data | Encoding |
|---|---|
| Frequency | Integer, divide by 100 for Hz (e.g., `5600` = 56.00 Hz) |
| Flow rate | Index 0–14 mapped to L/h per filter model |
| Time of day | Minutes from midnight (e.g., `480` = 08:00) |
| Booleans | `0` / `1` |
| Filter mode | `pumpMode & 0xFF` → Manual=16, ConstantFlow=1, Pulse=8, Bio=4 |

### Filter model identification

The `"version"` field in `FILTER_DATA` (not `USRDTA`) identifies the filter model:

| Version | Model |
|---|---|
| 74 | professionel 5e 350 |
| 76 | professionel 5e 450 |
| 78 + `tankconfig="WITH_THERMO"` | professionel 5e 600T |
| 78 | professionel 5e 700 |

### Flow rate tables (L/h by index)

| Index | Filter 350 | Filter 450 | Filter 600T/700 |
|---|---|---|---|
| 0 | 400 | 400 | 400 |
| 1 | 440 | 460 | 470 |
| 2 | 480 | 515 | 540 |
| 3 | 515 | 565 | 600 |
| 4 | 550 | 610 | 650 |
| 5 | 585 | 650 | 700 |
| 6 | 620 | 690 | 745 |
| 7 | 650 | 730 | 785 |
| 8 | 680 | 770 | 825 |
| 9 | 710 | 805 | 865 |
| 10 | 740 | 840 | 905 |
| 11 | 770 | 875 | 945 |
| 12 | 800 | 910 | 985 |
| 13 | 830 | 945 | 1025 |
| 14 | 860 | 980 | 1065 |

### Manual speed table (Hz by index)

| Index | Filter 350 | Filter 450 | Filter 600T/700 |
|---|---|---|---|
| 0 | 35.0 | 35.0 | 35.0 |
| 1 | 37.5 | 38.0 | 38.0 |
| 2 | 40.5 | 41.0 | 41.5 |
| 3 | 43.0 | 44.0 | 44.5 |
| 4 | 45.5 | 46.5 | 48.0 |
| 5 | 48.0 | 49.5 | 51.0 |
| 6 | 51.0 | 52.5 | 54.0 |
| 7 | 53.5 | 55.5 | 57.5 |
| 8 | 56.0 | 58.5 | 60.5 |
| 9 | 59.0 | 61.5 | 64.0 |
| 10 | 61.5 | 64.5 | 67.0 |
| 11 | 64.0 | 67.0 | 70.0 |
| 12 | 66.5 | 70.0 | 73.5 |
| 13 | 69.5 | 73.0 | 76.5 |
| 14 | 72.0 | 76.0 | 80.0 |
