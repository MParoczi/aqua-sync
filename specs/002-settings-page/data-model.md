# Data Model: Global Settings Page

**Feature**: 002-settings-page
**Date**: 2026-02-20

## Entities

### AppSettings

Persisted global application settings. Stored as `settings/app-settings.json` via `IDataService`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `DefaultVolumeUnit` | `VolumeUnit` | `Liters` | Default volume UOM for new aquarium profiles |
| `DefaultDimensionUnit` | `DimensionUnit` | `Centimeters` | Default dimension UOM for new aquarium profiles |
| `Theme` | `AppTheme` | `System` | Application theme preference |
| `DataFolderPath` | `string?` | `null` | Custom data folder path; `null` means default `%LOCALAPPDATA%/AquaSync/` |

**Validation rules**:
- `DefaultVolumeUnit` must be a valid `VolumeUnit` enum value
- `DefaultDimensionUnit` must be a valid `DimensionUnit` enum value
- `Theme` must be a valid `AppTheme` enum value
- `DataFolderPath`, when set, must be an existing writable directory

**Serialization**: camelCase JSON via `System.Text.Json`, matching existing `DataService` options.

### AppTheme (New Enum)

| Value | Description |
|-------|-------------|
| `System` | Follow the operating system theme (maps to `ElementTheme.Default`) |
| `Light` | Always use light theme (maps to `ElementTheme.Light`) |
| `Dark` | Always use dark theme (maps to `ElementTheme.Dark`) |

### Existing Enums (Referenced, Not Modified)

- `VolumeUnit`: `Liters`, `Gallons`
- `DimensionUnit`: `Centimeters`, `Inches`

## Relationships

```text
AppSettings (singleton, global)
├── references VolumeUnit enum
├── references DimensionUnit enum
└── references AppTheme enum (new)

AquariumSelectorViewModel
└── reads AppSettings.DefaultVolumeUnit, AppSettings.DefaultDimensionUnit
    via ISettingsService to set creation form defaults

SettingsViewModel
└── reads/writes AppSettings via ISettingsService

MainWindow
└── reads AppSettings.Theme via ISettingsService to set RequestedTheme
```

## State Transitions

`AppSettings` has no lifecycle states — it is a simple value object that is loaded, modified, and persisted. There are no state transitions.

## Data Location Strategy

### Primary Settings File

**Path**: `%LOCALAPPDATA%/AquaSync/settings/app-settings.json`

This file moves with the data folder when the user changes the data folder location.

### Data Folder Redirect File

**Path**: `%LOCALAPPDATA%/AquaSync/data-folder-redirect.json` (fixed location, never moves)

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `CustomDataFolderPath` | `string?` | `null` | Custom data folder path; `null` or file absence means use default |

**Purpose**: On startup, `DataService` checks this fixed-location file to determine where the actual data folder is. This solves the bootstrapping problem: the app always knows where to look for data, even if the data folder has been relocated.

**Lifecycle**:
1. File does not exist → use default `%LOCALAPPDATA%/AquaSync/`
2. User changes data folder → file is created/updated with new path
3. User resets to default → file is deleted

## Sample JSON

### app-settings.json

```json
{
  "defaultVolumeUnit": "Liters",
  "defaultDimensionUnit": "Centimeters",
  "theme": "System",
  "dataFolderPath": null
}
```

### data-folder-redirect.json

```json
{
  "customDataFolderPath": "D:\\MyAquariumData"
}
```
