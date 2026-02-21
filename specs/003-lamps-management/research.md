# Research: Lamps Management

**Branch**: `003-lamps-management` | **Date**: 2026-02-21

---

## 1. BLE Discovery API Mapping

**Decision**: Use `DeviceScanner.ScanAsync(TimeSpan timeout, IProgress<DiscoveredDevice>? progress, CancellationToken)` for device discovery.

**Rationale**: The integration notes mentioned `ScanByServiceAsync()` — this method does not exist in the library. The correct equivalent is `ScanAsync()`, which internally starts a `BluetoothLEAdvertisementWatcher` filtered by the Nordic UART service UUID (`6E400001-...`). This is a reliable discriminator for all Chihiros devices and is exactly "scan by service". `ScanByNameAsync()` is the alternative, filtering by BLE name prefix, but UART service UUID is the stronger discriminator.

**Alternatives considered**: `ScanByNameAsync()` — filters by Chihiros BLE name prefixes. Rejected as it requires DeviceProfiles to maintain name prefix lists and may miss devices with unexpected names.

---

## 2. Indefinite Scan (Continuous Streaming Until User Stops)

**Decision**: Call `DeviceScanner.ScanAsync(TimeSpan.FromMinutes(10), progress, cancellationToken)` with a `CancellationTokenSource` controlled by the UI. When the user taps Stop, cancel the source.

**Rationale**: Internally `ScanAsync` runs `BluetoothLEAdvertisementWatcher` and awaits `Task.Delay(timeout, cancellationToken)`. Cancelling the token interrupts the delay, stops the watcher, and returns immediately. The `IProgress<DiscoveredDevice>` callback fires on each detection in real-time — this delivers the FR-001 continuous streaming requirement. The 10-minute cap is a safety guard; scanning beyond that is unusual. The aggregate return value of `ScanAsync` is discarded; only `IProgress` is used for UI updates.

**Alternatives considered**: Directly using `BluetoothLEAdvertisementWatcher` — rejected because it duplicates library functionality and violates Constitution IV (use existing libraries).

---

## 3. Connection Lifecycle Management

**Decision**: `LampService` creates `ChihirosDevice` instances on-demand and caches them by BLE address string while connected. `LampDetailViewModel` connects on `OnNavigatedTo` and disconnects on `OnNavigatedFrom`. Dashboard mode toggles follow a connect-send-disconnect pattern.

**Rationale**: Maintaining persistent BLE connections for all lamps would drain device batteries and consume WinRT Bluetooth resources. Connect-on-demand when the user actively views the detail page is the correct balance. `IChihirosDevice` is `IAsyncDisposable`; the cache holds live instances and disposes them on explicit disconnect or app shutdown.

**Threading note**: `IChihirosDevice.Connected` and `Disconnected` events fire on background BLE threads. All ViewModels must marshal to the UI thread via `DispatcherQueue.TryEnqueue()` in event handlers (per Constitution IV and CLAUDE.md).

**Device instantiation**: `ChihirosDevice` is the only implementation of `IChihirosDevice` and has no factory interface in the library. `LampService` instantiates it directly: `new ChihirosDevice(ulong address, string name, DeviceProfile profile)`. This is acceptable per Constitution IV — the library is a project reference, not a NuGet package with a strict abstraction boundary.

**Alternatives considered**: Persistent connections for all lamps — rejected due to battery drain. Abstract factory for device creation — rejected as over-engineering with a single implementation.

---

## 4. BluetoothAddress Serialization

**Decision**: Store `DiscoveredDevice.BluetoothAddress` (`ulong`) as a 12-character uppercase hex string (e.g., `"A1B2C3D4E5F6"`) in `LampConfiguration.BluetoothAddress`.

**Rationale**: `ulong` values near `UInt64.MaxValue` can lose precision in some JSON parsers (JavaScript's `Number` type). Hex string is human-readable in JSON files and unambiguous.

**Conversion**:
- Store: `device.BluetoothAddress.ToString("X12")`
- Restore: `Convert.ToUInt64(lampConfig.BluetoothAddress, 16)`

**Alternatives considered**: Store as `ulong` JSON integer — rejected due to potential precision loss.

---

## 5. ColorChannel Dictionary Key Serialization

**Decision**: Store `Dictionary<ColorChannel, byte>` as `Dictionary<string, byte>` in all model classes (`LampConfiguration.ManualBrightness`, `ScheduleConfiguration.ChannelBrightness`), using `channel.ToString()` (e.g., `"Red"`, `"White"`) as keys.

**Rationale**: `System.Text.Json` with `camelCase` naming policy does not automatically serialize enum dictionary keys to their string names (it writes the integer). String keys are human-readable, avoid custom converter complexity, and round-trip cleanly.

**Conversion**: When building `LightSchedule` for the device, iterate the stored dict and `Enum.Parse<ColorChannel>(key)` to reconstruct the typed dictionary.

**Alternatives considered**: `JsonStringEnumConverter` on dictionary key — available via `JsonStringEnumConverter` on the property, but string keys are simpler and consistent with the rest of the codebase (which uses `JsonStringEnumConverter` only on direct enum values, not dict keys).

---

## 6. Custom Schedule Timeline Control

**Decision**: Implement `ScheduleEditorControl` as a WinUI3 `UserControl` in `AquaSync.App/Controls/`. Use a `Canvas` with drawn `Rectangle` segments for timeline zones and `Ellipse` elements as drag handles. Expose `DependencyProperty` instances for `SunriseTime`, `SunsetTime`, `RampUpMinutes` with `PropertyChangedCallback` to redraw on data change.

**Timeline visual zones** (drawn as `Rectangle` on the `Canvas`):
- **Off-period** (before sunrise, after sunset): muted/dark fill using `SystemControlBackgroundBaseLowBrush`
- **Ramp-up period** (sunrise → sunrise + ramp): accent gradient using `SystemAccentColor` at reduced opacity
- **On-period** (sunrise + ramp → sunset): bright fill using `SystemAccentColorLight2`

**Drag interaction**: `PointerPressed` + `PointerMoved` + `PointerReleased` on handle `Ellipse` elements. Position converted to time via `(pointerX / canvas.ActualWidth) * 24h`. Clamped to valid range (sunrise < sunset, ramp within bounds).

**Rationale**: `Canvas` is the correct WinUI3 primitive for absolute-positioned custom rendering. No third-party charting library needed (Constitution V: minimal dependencies). `DependencyProperty` enables XAML data binding to the host ViewModel.

**Alternatives considered**: Third-party timeline/slider library — rejected (Constitution V). `ItemsControl`-based approach — overly complex for a single visual bar.

---

## 7. Add Lamp Dialog (ContentDialog)

**Decision**: Implement `AddLampDialog` as a `ContentDialog` subclass in `AquaSync.App/Views/`. The dialog manages its own scan lifecycle in code-behind: scan starts via `Opened` event, stops on `PrimaryButtonClick` (Add) or `CloseButtonClick` (Cancel). Each discovered device is immediately checked against `ILampService.IsAddressAssignedAsync()` and shown in a `ListView` as available or greyed-out.

**Result passing**: `AddLampDialog.SelectedDevice` is a `public DiscoveredDevice?` property set when the user selects a device and clicks Add. After `dialog.ShowAsync()` returns `ContentDialogResult.Primary`, `LampsViewModel` reads `dialog.SelectedDevice`.

**Rationale**: Constitution I mandates `ContentDialog` for modal forms. The scan lifecycle is naturally tied to the dialog's lifetime. No separate ViewModel needed — scan state fits cleanly in code-behind for this self-contained modal flow.

**Threading**: `IProgress<DiscoveredDevice>` callbacks from `DeviceScanner` fire on background threads. The `Progress<T>` constructor captures the UI `SynchronizationContext`, so `Report()` calls automatically marshal to the UI thread when `Progress<T>` is constructed on the UI thread.

**Alternatives considered**: Separate `LampScanPage` navigated to from LampsPage — rejected because it adds navigation complexity for a modal, one-shot flow.

---

## 8. Dashboard Lamp Card Integration

**Decision**: `DashboardViewModel` loads lamps for the current aquarium via `ILampService` on `OnNavigatedTo`. Each lamp is wrapped in a `LampCardViewModel` observable item exposing name, model, connection state, current mode, and an `IRelayCommand` for mode switching. The Dashboard XAML renders these in an `ItemsControl` with a card-style `DataTemplate`.

**Mode switch flow**: Connect to device → send appropriate command (`TurnOffAsync`, `TurnOnAsync`, `EnableAutoModeAsync`) → update persisted mode via `ILampService.SaveModeAsync` → disconnect. If the device is unreachable, show an `InfoBar` error without changing the stored mode.

**Rationale**: Dashboard and Lamps page share the same `ILampService` singleton — no data duplication. `LampCardViewModel` is an inner class or nested type of `DashboardViewModel` to keep it scoped without adding a new top-level registration.

---

## Summary of Integration Decisions

| Topic | Decision |
|-------|----------|
| Discovery method | `DeviceScanner.ScanAsync()` (not `ScanByServiceAsync`) |
| Continuous scan | Long timeout + `CancellationToken` stop |
| Connection model | On-demand, cached while detail view is open |
| BLE address in JSON | 12-char uppercase hex string |
| Channel dict keys in JSON | `ColorChannel.ToString()` string keys |
| Schedule timeline control | Custom `UserControl` with `Canvas` |
| Add lamp UI | `ContentDialog` with scan in code-behind |
| Dashboard cards | `LampCardViewModel` items in `DashboardViewModel` |
