# Data Model: Lamps Management

**Branch**: `003-lamps-management` | **Date**: 2026-02-21

---

## Storage

Lamp configurations are persisted as JSON files by `IDataService`:

- **Path**: `{dataRoot}/lamps/{LampConfiguration.Id}.json`
- Each file represents one `LampConfiguration` record
- Aquarium association is embedded via the `AquariumId` field
- Global BLE address uniqueness is enforced at add-time by `ILampService.IsAddressAssignedAsync()`
- `ReadAllAsync<LampConfiguration>("lamps")` + filter by `AquariumId` retrieves all lamps for an aquarium

---

## Entities

### LampConfiguration

**Persisted to**: `lamps/{Id}.json`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | Unique | Record identifier; used as JSON filename |
| `AquariumId` | `Guid` | Required | Owner aquarium |
| `BluetoothAddress` | `string` | 12-char hex, unique | BLE address (e.g., `"A1B2C3D4E5F6"`) |
| `DeviceName` | `string` | Non-empty | BLE advertised display name |
| `ModelName` | `string` | May be empty | `DeviceProfile.ModelName`; empty for unmanaged lamps |
| `Mode` | `LampMode` | Default: `Off` | Operational mode |
| `ManualBrightness` | `Dictionary<string, byte>` | Values 0–100 | Channel name → brightness; persisted across mode switches |
| `Schedule` | `ScheduleConfiguration?` | Nullable | `null` when no schedule has been configured (blank initial state) |
| `CreatedAt` | `DateTimeOffset` | Set once | Timestamp of first assignment |

---

### LampMode (enum, JSON string)

| Value | Description |
|-------|-------------|
| `Off` | Lamp is inactive; device receives turn-off command on mode entry |
| `Manual` | Brightness set by `ManualBrightness` values; device runs in manual mode |
| `Automatic` | Device runs its programmed schedule autonomously; set via `EnableAutoModeAsync()` |

**Default**: `Off` — set when a lamp is first added to an aquarium.

---

### ScheduleConfiguration

**Embedded in**: `LampConfiguration.Schedule` (nullable)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Sunrise` | `TimeOnly` | Before `Sunset` | Start of ramp-up period |
| `Sunset` | `TimeOnly` | After `Sunrise` | Time lights turn off (no fade-out) |
| `RampUpMinutes` | `int` | 0–150 | Duration of brightness fade-in from sunrise start |
| `ChannelBrightness` | `Dictionary<string, byte>` | Values 0–100 | Channel name → peak brightness during on-period |
| `ActiveDays` | `Weekday` | Not `Weekday.None` to save | `[Flags]` enum; days schedule is active |

**Initial state**: `LampConfiguration.Schedule = null` — editor opens blank, user must populate all fields.

**On-period**: from `Sunrise + RampUpMinutes` to `Sunset`.

---

### Runtime Types (not persisted)

**`DiscoveredDevice`** (from `AquaSync.Chihiros`):
- `BluetoothAddress: ulong` — raw BLE address
- `Name: string` — advertised BLE name
- `Rssi: short` — signal strength (dBm)
- `MatchedProfile: DeviceProfile?` — matched device profile or `null` for unmanaged

**`LampCardViewModel`** (runtime display model for Dashboard):
- Wraps `LampConfiguration`
- Exposes `DisplayName`, `ModelName`, `ConnectionState`, `CurrentMode`
- Provides `SetModeCommand: IRelayCommand<LampMode>`

---

## State Transitions

### LampMode

```
Off ←→ Manual
Off ←→ Automatic
Manual ←→ Automatic
```

Initiated by:
- Dashboard mode toggle (all transitions)
- LampDetailPage mode control (planned for future detail-view mode selector)

BLE commands per transition:
| Target Mode | BLE Command |
|-------------|-------------|
| `Off` | `TurnOffAsync()` |
| `Manual` | `TurnOnAsync()` (device uses last brightness set via `SetBrightnessAsync`) |
| `Automatic` | `EnableAutoModeAsync()` (also syncs clock) + `AddScheduleAsync()` if schedule exists |

### Connection State (runtime only)

```
Disconnected → Connecting → Connected → Disconnected
```

Managed by `LampService` internal cache. Triggered by:
- `LampDetailViewModel.OnNavigatedTo` → Connect
- `LampDetailViewModel.OnNavigatedFrom` → Disconnect
- Dashboard mode toggle → Connect → send command → Disconnect
- `IChihirosDevice.Disconnected` event → evict from cache

---

## Validation Rules

| Rule | Enforcement Point |
|------|------------------|
| `BluetoothAddress` unique across all lamps | `ILampService.IsAddressAssignedAsync()` before add |
| `Sunrise` < `Sunset` | `ILampService.SaveScheduleAsync()` + UI before save |
| `Sunrise + RampUpMinutes` ≤ `Sunset` (minutes) | `ILampService.SaveScheduleAsync()` + UI before save |
| `RampUpMinutes` in [0, 150] | `ScheduleEditorControl` slider bounds |
| `ActiveDays ≠ Weekday.None` to save | UI save button guard |
| Both `Sunrise` and `Sunset` must be set to save | UI save button guard |
| `ManualBrightness` values in [0, 100] | `IChihirosDevice.SetBrightnessAsync` expects byte 0–100 |

---

## Example JSON

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "aquariumId": "1b3e4a6d-2c91-4e3f-9b01-d7f28c6b0e12",
  "bluetoothAddress": "A1B2C3D4E5F6",
  "deviceName": "DYNC2N_A1B2C3",
  "modelName": "WRGB II Pro",
  "mode": "Automatic",
  "manualBrightness": {
    "Red": 60,
    "Green": 80,
    "Blue": 100,
    "White": 70
  },
  "schedule": {
    "sunrise": "07:00:00",
    "sunset": "21:00:00",
    "rampUpMinutes": 30,
    "channelBrightness": {
      "Red": 50,
      "Green": 75,
      "Blue": 90,
      "White": 65
    },
    "activeDays": 124
  },
  "createdAt": "2026-02-21T10:00:00+00:00"
}
```

**`activeDays` note**: `Weekday` is a `[Flags]` enum. Value `124 = 0b1111100 = Mon+Tue+Wed+Thu+Fri` (weekdays). Stored as integer by `System.Text.Json` (no `JsonStringEnumConverter` applied to flags enums).

**Unmanaged lamp** (unknown model, no profile):
```json
{
  "id": "...",
  "aquariumId": "...",
  "bluetoothAddress": "B2C3D4E5F6A1",
  "deviceName": "UNKNOWN_XYZ",
  "modelName": "",
  "mode": "Off",
  "manualBrightness": {},
  "schedule": null,
  "createdAt": "2026-02-21T11:00:00+00:00"
}
```
