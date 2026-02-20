# AquaSync.Chihiros

A .NET 10 class library for controlling Chihiros aquarium LED lights over Bluetooth Low Energy. Built on WinRT BLE APIs with no external dependencies.

This library is a clean-room port of the [chihiros-led-control](https://github.com/TheMicDiet/chihiros-led-control) Python project's reverse-engineered BLE
protocol. It provides device discovery, connection management, manual brightness control, and auto-mode scheduling for 15 known Chihiros LED models.

## Requirements

- .NET 10 targeting `net10.0-windows10.0.19041.0`
- Windows 10 (build 19041+) or Windows 11
- A Bluetooth LE adapter

## Project Reference

Add a project reference from your WinUI 3 (or any Windows Desktop) application:

```xml
<ProjectReference Include="..\AquaSync.Chihiros\AquaSync.Chihiros.csproj" />
```

## Namespaces

| Namespace                      | Purpose                                                     |
|--------------------------------|-------------------------------------------------------------|
| `AquaSync.Chihiros.Discovery`  | BLE scanning and device detection                           |
| `AquaSync.Chihiros.Devices`    | Device profiles, connection, and control                    |
| `AquaSync.Chihiros.Scheduling` | Auto-mode lighting schedules                                |
| `AquaSync.Chihiros.Protocol`   | Low-level BLE command encoding (normally not used directly) |
| `AquaSync.Chihiros.Exceptions` | Exception types thrown by the library                       |

---

## Quick Start

```csharp
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Discovery;

// 1. Scan for devices
var scanner = new DeviceScanner();
var devices = await scanner.ScanAsync(TimeSpan.FromSeconds(5));

// 2. Pick a device and connect
var found = devices[0];
var profile = found.MatchedProfile ?? DeviceProfiles.Fallback;

await using var device = new ChihirosDevice(found.BluetoothAddress, found.Name, profile);
await device.ConnectAsync();

// 3. Control the light
await device.SetBrightnessAsync(ColorChannel.Red, 75);
await device.TurnOffAsync();
```

---

## Discovery

`DeviceScanner` finds Chihiros devices over BLE. Two scanning strategies are available.

### Scan by UART Service UUID

The primary method filters BLE advertisements by the Nordic UART Service UUID. This finds any device advertising the service, including unknown models:

```csharp
var scanner = new DeviceScanner();
var devices = await scanner.ScanAsync(TimeSpan.FromSeconds(10));

foreach (var d in devices)
{
    Console.WriteLine($"{d.Name} ({d.BluetoothAddress:X12}) RSSI={d.Rssi}");
    Console.WriteLine($"  Model: {d.MatchedProfile?.ModelName ?? "Unknown"}");
}
```

### Scan by Known Name Prefix

An alternative method skips the UUID filter and instead matches BLE local names against all known Chihiros model codes. This is useful when a device does not
advertise the UART service UUID in its packets:

```csharp
var devices = await scanner.ScanByNameAsync(TimeSpan.FromSeconds(10));
// Only returns devices whose BLE name matches a known Chihiros model code.
```

### Real-Time Progress

Both scan methods accept an `IProgress<DiscoveredDevice>` to report devices as they are found, rather than waiting for the timeout to complete:

```csharp
var progress = new Progress<DiscoveredDevice>(d =>
{
    // Called on the capturing SynchronizationContext (e.g., UI thread in WinUI 3)
    DeviceListView.Items.Add($"{d.Name} - {d.MatchedProfile?.ModelName}");
});

var devices = await scanner.ScanAsync(TimeSpan.FromSeconds(10), progress);
```

### Cancellation

All async operations support `CancellationToken`. To let the user abort a scan:

```csharp
var cts = new CancellationTokenSource();
// Wire cts.Cancel() to a "Stop Scan" button

var devices = await scanner.ScanAsync(TimeSpan.FromSeconds(30), cancellationToken: cts.Token);
```

---

## Device Profiles

Every Chihiros model is described by a `DeviceProfile` that declares its name, BLE name codes, and color channel layout.

### Auto-Detection

When a device is discovered via scanning, `MatchedProfile` is automatically populated if the BLE name matches a known model code. The matching uses *
*longest-prefix-first** comparison, so `DYWPRO30` correctly matches the WRGB II Pro profile rather than a shorter prefix.

### Known Profiles

Access any profile via the `DeviceProfiles` static class:

| Property                          | Model              | Channels                               |
|-----------------------------------|--------------------|----------------------------------------|
| `DeviceProfiles.AII`              | A II               | White                                  |
| `DeviceProfiles.CII`              | C II               | White                                  |
| `DeviceProfiles.ZLightTiny`       | Z Light TINY       | White, Warm                            |
| `DeviceProfiles.TinyTerrariumEgg` | Tiny Terrarium Egg | Red, Green                             |
| `DeviceProfiles.CIIRGB`           | C II RGB           | Red, Green, Blue                       |
| `DeviceProfiles.WRGBII`           | WRGB II            | Red, Green, Blue                       |
| `DeviceProfiles.WRGBIISlim`       | WRGB II Slim       | Red, Green, Blue                       |
| `DeviceProfiles.WRGBIIPro`        | WRGB II Pro        | Red, Green, Blue, White                |
| `DeviceProfiles.UniversalWRGB`    | Universal WRGB     | Red, Green, Blue, White                |
| `DeviceProfiles.Commander1`       | Commander 1        | White (ambiguous — override as needed) |
| `DeviceProfiles.Commander4`       | Commander 4        | White (ambiguous — override as needed) |
| `DeviceProfiles.GenericWhite`     | Generic White      | White                                  |
| `DeviceProfiles.GenericRGB`       | Generic RGB        | Red, Green, Blue                       |
| `DeviceProfiles.GenericWRGB`      | Generic WRGB       | Red, Green, Blue, White                |
| `DeviceProfiles.Fallback`         | Fallback           | White                                  |

### Iterating All Profiles

```csharp
foreach (var profile in DeviceProfiles.All)
{
    Console.WriteLine($"{profile.ModelName}: {string.Join(", ", profile.Channels.Select(c => c.Channel))}");
}
```

### Manual Profile Lookup

```csharp
DeviceProfile? profile = DeviceProfiles.MatchFromName("DYNWRGB_1A2B3C4D5E6F");
// Returns DeviceProfiles.WRGBII
```

Returns `null` if no known profile matches.

### Overriding the Profile

Commander 1, Commander 4, and unknown devices are ambiguous — the hardware could be white-only, RGB, or WRGB. Let the user pick, then pass the chosen profile to
`ChihirosDevice`:

```csharp
// User selected "WRGB" from a dropdown
var profile = userSelection switch
{
    "White" => DeviceProfiles.GenericWhite,
    "RGB"   => DeviceProfiles.GenericRGB,
    "WRGB"  => DeviceProfiles.GenericWRGB,
    _       => DeviceProfiles.Fallback
};

await using var device = new ChihirosDevice(found.BluetoothAddress, found.Name, profile);
```

### Color Channels and Mappings

The `ColorChannel` enum defines the five logical color types:

```csharp
public enum ColorChannel { White, Warm, Red, Green, Blue }
```

Each profile maps these to protocol channel IDs via `ChannelMapping`:

```csharp
// Query a profile's channels
foreach (var mapping in DeviceProfiles.WRGBIIPro.Channels)
{
    Console.WriteLine($"{mapping.Channel} -> protocol channel {mapping.ProtocolChannelId}");
}
// Output:
//   Red -> protocol channel 0
//   Green -> protocol channel 1
//   Blue -> protocol channel 2
//   White -> protocol channel 3
```

You generally do not need to work with `ChannelMapping` directly — `ChihirosDevice` resolves the mapping internally when you pass a `ColorChannel` to
`SetBrightnessAsync`.

---

## Connection Management

Connections are explicit. The application controls when to connect and disconnect — there is no auto-disconnect timer.

### Connecting

```csharp
await using var device = new ChihirosDevice(bluetoothAddress, "My WRGB II", DeviceProfiles.WRGBII);

try
{
    await device.ConnectAsync();
    // device.IsConnected == true
}
catch (DeviceNotFoundException)
{
    // Device not in range or powered off
}
catch (CharacteristicMissingException)
{
    // Connected but UART service/characteristics not found
}
catch (DeviceConnectionException)
{
    // Failed to subscribe to notifications
}
```

### Disconnecting

```csharp
await device.DisconnectAsync();
```

Or rely on `IAsyncDisposable` via `await using`:

```csharp
await using var device = new ChihirosDevice(address, name, profile);
await device.ConnectAsync();
// ... use the device ...
// Automatically disconnects when the scope exits
```

### Connection Events

```csharp
device.Connected += (sender, _) =>
{
    // Raised after ConnectAsync completes successfully
};

device.Disconnected += (sender, reason) =>
{
    // Raised when BLE connection drops unexpectedly
    // 'reason' is a descriptive string, e.g., "BLE connection lost."
    // Use DispatcherQueue to update UI from this event.
};
```

The `Disconnected` event fires on a background thread. In WinUI 3, marshal back to the UI thread:

```csharp
device.Disconnected += (sender, reason) =>
{
    DispatcherQueue.TryEnqueue(() =>
    {
        StatusText.Text = $"Disconnected: {reason}";
    });
};
```

---

## Manual Brightness Control

All brightness values range from **0** (off) to **100** (full).

### Set a Single Channel

```csharp
await device.SetBrightnessAsync(ColorChannel.Red, 80);
await device.SetBrightnessAsync(ColorChannel.Green, 60);
await device.SetBrightnessAsync(ColorChannel.Blue, 40);
```

Passing a `ColorChannel` that the device profile does not support throws `ArgumentException`:

```csharp
// On a white-only device like A II:
await device.SetBrightnessAsync(ColorChannel.Red, 50);
// Throws ArgumentException: "Color channel 'Red' is not supported by device profile 'A II'."
```

### Turn On / Turn Off

These convenience methods set **all** channels on the profile to 100 or 0:

```csharp
await device.TurnOnAsync();   // All channels -> 100
await device.TurnOffAsync();  // All channels -> 0
```

### Set Multiple Channels

There is no batch method. Set channels sequentially:

```csharp
await device.SetBrightnessAsync(ColorChannel.Red, 90);
await device.SetBrightnessAsync(ColorChannel.Green, 70);
await device.SetBrightnessAsync(ColorChannel.Blue, 50);
await device.SetBrightnessAsync(ColorChannel.White, 100);
```

Commands are serialized internally via a `SemaphoreSlim`, so concurrent calls from different threads are safe.

---

## Auto Mode and Scheduling

Auto mode lets the device follow a stored lighting schedule autonomously, even when the app is closed or the phone/PC is out of range.

### Enable Auto Mode

Switches the device to auto mode and syncs the device clock:

```csharp
await device.EnableAutoModeAsync();
```

### Sync the Clock Only

If the device is already in auto mode but the clock drifted:

```csharp
await device.SetTimeAsync(DateTime.Now);
```

### Add a Schedule

Create a `LightSchedule` that describes when the light turns on, its brightness per channel, ramp-up time, and which days it applies to:

```csharp
using AquaSync.Chihiros.Scheduling;

var schedule = new LightSchedule(
    Sunrise: new TimeOnly(8, 0),            // Turn on at 08:00
    Sunset: new TimeOnly(20, 0),            // Turn off at 20:00
    ChannelBrightness: new Dictionary<ColorChannel, byte>
    {
        [ColorChannel.Red] = 80,
        [ColorChannel.Green] = 60,
        [ColorChannel.Blue] = 100
    },
    RampUpMinutes: 30,                      // Fade in over 30 minutes
    Weekdays: Weekday.Everyday              // Active every day
);

await device.AddScheduleAsync(schedule);
```

#### White-Only Devices

For devices with a single white channel, set only that channel:

```csharp
var schedule = new LightSchedule(
    Sunrise: new TimeOnly(7, 0),
    Sunset: new TimeOnly(19, 0),
    ChannelBrightness: new Dictionary<ColorChannel, byte>
    {
        [ColorChannel.White] = 100
    },
    RampUpMinutes: 15,
    Weekdays: Weekday.Monday | Weekday.Wednesday | Weekday.Friday
);

await device.AddScheduleAsync(schedule);
```

#### Weekday Selection

The `Weekday` enum is a `[Flags]` bitmask. Combine days with `|`:

```csharp
Weekday.Everyday                              // All 7 days
Weekday.Monday | Weekday.Friday               // Mon + Fri only
Weekday.Saturday | Weekday.Sunday             // Weekends only
```

Individual values:

```csharp
Weekday.Monday      // 0b_1000000
Weekday.Tuesday     // 0b_0100000
Weekday.Wednesday   // 0b_0010000
Weekday.Thursday    // 0b_0001000
Weekday.Friday      // 0b_0000100
Weekday.Saturday    // 0b_0000010
Weekday.Sunday      // 0b_0000001
Weekday.Everyday    // 0b_1111111
Weekday.None        // 0b_0000000
```

### Remove a Specific Schedule

Pass a `LightSchedule` with the same sunrise, sunset, ramp-up, and weekdays to identify which schedule to remove:

```csharp
await device.RemoveScheduleAsync(schedule);
```

### Reset All Schedules

Remove every stored schedule at once:

```csharp
await device.ResetSchedulesAsync();
```

### Scheduling Limitation: WRGB White Channel

The Chihiros BLE protocol only supports **3 brightness slots** in scheduling commands, mapped to protocol channels 0, 1, and 2. On WRGB devices (like WRGB II
Pro and Universal WRGB), the white channel is protocol channel 3 and **cannot be included in schedules**.

If you include `ColorChannel.White` in a schedule's `ChannelBrightness` for a WRGB device, it will be silently ignored during schedule creation. White
brightness on WRGB devices can only be controlled in manual mode via `SetBrightnessAsync`.

---

## Error Handling

The library throws on first failure with no internal retries. The calling application is responsible for retry logic.

### Exception Types

| Exception                        | When                                                                                        |
|----------------------------------|---------------------------------------------------------------------------------------------|
| `DeviceNotFoundException`        | `ConnectAsync` cannot find a device at the given BLE address                                |
| `CharacteristicMissingException` | The device is found but the UART service or its RX/TX characteristics are missing           |
| `DeviceConnectionException`      | A BLE write fails, notification subscription fails, or a command is sent while disconnected |
| `ArgumentException`              | A `ColorChannel` is passed that the device profile does not support                         |
| `ObjectDisposedException`        | An operation is attempted after the device has been disposed                                |

### Recommended Pattern

```csharp
try
{
    await device.ConnectAsync(cancellationToken);
    await device.SetBrightnessAsync(ColorChannel.White, 80, cancellationToken);
}
catch (DeviceNotFoundException)
{
    // Prompt user to check if the device is powered on and in range
}
catch (CharacteristicMissingException)
{
    // Likely a firmware issue or unsupported device variant
}
catch (DeviceConnectionException ex)
{
    // BLE write failed — offer retry
}
catch (OperationCanceledException)
{
    // User or timeout cancelled the operation
}
```

---

## Threading and Async

- All public methods are `async Task` and accept `CancellationToken`.
- The library uses `ConfigureAwait(false)` throughout. It never captures or posts to a `SynchronizationContext`.
- Events (`Connected`, `Disconnected`) fire on background threads. In WinUI 3, use `DispatcherQueue.TryEnqueue` to update the UI.
- `SendCommandAsync` is serialized internally with a `SemaphoreSlim`, so calling multiple methods concurrently from different threads is safe.

---

## Complete WinUI 3 Example

A realistic example of scanning, connecting, controlling, and scheduling from a WinUI 3 code-behind:

```csharp
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Discovery;
using AquaSync.Chihiros.Exceptions;
using AquaSync.Chihiros.Scheduling;

public sealed partial class MainPage : Page
{
    private ChihirosDevice? _device;
    private CancellationTokenSource? _scanCts;

    // --- Scanning ---

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        _scanCts = new CancellationTokenSource();
        var scanner = new DeviceScanner();

        var progress = new Progress<DiscoveredDevice>(d =>
        {
            DeviceList.Items.Add(new DeviceItem(d));
        });

        try
        {
            await scanner.ScanAsync(TimeSpan.FromSeconds(10), progress, _scanCts.Token);
        }
        catch (OperationCanceledException) { }
    }

    private void StopScanButton_Click(object sender, RoutedEventArgs e)
    {
        _scanCts?.Cancel();
    }

    // --- Connecting ---

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceList.SelectedItem is not DeviceItem item) return;

        var profile = item.Device.MatchedProfile ?? DeviceProfiles.Fallback;
        _device = new ChihirosDevice(item.Device.BluetoothAddress, item.Device.Name, profile);

        _device.Disconnected += (_, reason) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusText.Text = $"Disconnected: {reason}";
            });
        };

        try
        {
            await _device.ConnectAsync();
            StatusText.Text = $"Connected to {_device.Name}";
        }
        catch (Exception ex) when (ex is DeviceNotFoundException or CharacteristicMissingException or DeviceConnectionException)
        {
            StatusText.Text = $"Connection failed: {ex.Message}";
            await _device.DisposeAsync();
            _device = null;
        }
    }

    // --- Brightness Control ---

    private async void BrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_device is not { IsConnected: true }) return;

        try
        {
            await _device.SetBrightnessAsync(ColorChannel.White, (byte)e.NewValue);
        }
        catch (DeviceConnectionException)
        {
            StatusText.Text = "Failed to set brightness.";
        }
    }

    // --- Scheduling ---

    private async void AddScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_device is not { IsConnected: true }) return;

        var schedule = new LightSchedule(
            Sunrise: new TimeOnly(8, 0),
            Sunset: new TimeOnly(20, 0),
            ChannelBrightness: new Dictionary<ColorChannel, byte>
            {
                [ColorChannel.White] = 100
            },
            RampUpMinutes: 30,
            Weekdays: Weekday.Everyday
        );

        await _device.AddScheduleAsync(schedule);
        await _device.EnableAutoModeAsync();
        StatusText.Text = "Schedule added and auto mode enabled.";
    }

    // --- Cleanup ---

    private async void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_device is not null)
        {
            await _device.DisposeAsync();
        }
    }

    private record DeviceItem(DiscoveredDevice Device)
    {
        public override string ToString() =>
            $"{Device.Name} ({Device.MatchedProfile?.ModelName ?? "Unknown"})";
    }
}
```

---

## Architecture

```
AquaSync.Chihiros
│
├── Discovery/
│   ├── DeviceScanner            Scans BLE advertisements, matches device names
│   └── DiscoveredDevice         Scan result record (address, name, RSSI, profile)
│
├── Devices/
│   ├── IChihirosDevice          Interface: connect, control, schedule, events
│   ├── ChihirosDevice           WinRT BLE implementation of IChihirosDevice
│   ├── DeviceProfile            Record: model name, BLE codes, channel mappings
│   ├── DeviceProfiles           Static registry of 15 known models + name matching
│   ├── ColorChannel             Enum: White, Warm, Red, Green, Blue
│   └── ChannelMapping           Record: ColorChannel -> protocol channel ID
│
├── Scheduling/
│   ├── LightSchedule            Record: sunrise/sunset, per-channel brightness, weekdays
│   └── Weekday                  [Flags] enum: Monday..Sunday, Everyday
│
├── Protocol/
│   ├── CommandBuilder           Encodes all BLE commands (internal use by ChihirosDevice)
│   ├── MessageId                16-bit counter with 0x5A avoidance
│   └── UartConstants            Nordic UART Service UUIDs
│
└── Exceptions/
    ├── DeviceNotFoundException
    ├── CharacteristicMissingException
    └── DeviceConnectionException
```

The `Protocol` layer is internal to the library. Consumers interact with `Discovery`, `Devices`, and `Scheduling` only.
