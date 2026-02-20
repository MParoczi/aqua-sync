# Data Model: Aquarium Profile Management

**Branch**: `001-aquarium-profiles` | **Date**: 2026-02-15

## Entities

### Aquarium

Represents a single aquarium profile. Stored as an individual JSON file at `aquariums/{id}.json`.

| Field | Type | Required | Default | Notes |
|-------|------|----------|---------|-------|
| Id | Guid | Yes | Auto-generated | Primary identifier; also used as filename |
| Name | string | Yes | — | Max 100 characters |
| Volume | double | Yes | — | Must be > 0; display up to 1 decimal place, store input precision |
| VolumeUnit | VolumeUnit | Yes | Liters | Locked after creation |
| Length | double | Yes | — | Must be > 0 |
| Width | double | Yes | — | Must be > 0 |
| Height | double | Yes | — | Must be > 0 |
| DimensionUnit | DimensionUnit | Yes | Centimeters | Locked after creation |
| AquariumType | AquariumType | Yes | — | Locked after creation |
| SetupDate | DateTimeOffset | Yes | — | Date-only input (time stored as midnight UTC); locked after creation |
| Description | string? | No | null | Max 2000 characters |
| ThumbnailPath | string? | No | null | Relative path to image in gallery folder; null = use default graphic |
| Status | AquariumStatus | Yes | Active | Active or Archived |
| CreatedAt | DateTimeOffset | Yes | Auto-set | Set once at creation, never modified |
| Substrates | List\<SubstrateEntry\> | No | Empty list | Embedded collection; ordered by DisplayOrder |

**Editability after creation:**
- Editable: Name, Description, ThumbnailPath, Substrates
- Locked: Volume, VolumeUnit, Length, Width, Height, DimensionUnit, AquariumType, SetupDate
- System-managed: Id, Status (via archive/restore), CreatedAt

### SubstrateEntry

Represents a single substrate or additive layer within an aquarium. Embedded in the parent Aquarium's JSON (not a separate file).

| Field | Type | Required | Default | Notes |
|-------|------|----------|---------|-------|
| Id | Guid | Yes | Auto-generated | Uniquely identifies the entry within the list |
| Brand | string | Yes | — | Manufacturer/brand name |
| ProductName | string | Yes | — | Specific product name |
| Type | SubstrateType | Yes | — | Substrate, Additive, or SoilCap |
| LayerDepth | double | Yes | — | Must be > 0; uses parent aquarium's DimensionUnit |
| DateAdded | DateTimeOffset | Yes | — | Date-only input (time stored as midnight UTC) |
| Notes | string? | No | null | Optional notes |
| DisplayOrder | int | Yes | Auto-assigned | Order in the substrate list (0-based) |

## Enumerations

### VolumeUnit
| Value | Display Name |
|-------|-------------|
| Liters | Liters (L) |
| Gallons | Gallons (gal) |

### DimensionUnit
| Value | Display Name |
|-------|-------------|
| Centimeters | Centimeters (cm) |
| Inches | Inches (in) |

### AquariumType
| Value | Display Name |
|-------|-------------|
| Freshwater | Freshwater |
| Saltwater | Saltwater |
| Brackish | Brackish |

### SubstrateType
| Value | Display Name |
|-------|-------------|
| Substrate | Substrate |
| Additive | Additive |
| SoilCap | Soil Cap |

### AquariumStatus
| Value | Display Name |
|-------|-------------|
| Active | Active |
| Archived | Archived |

## Relationships

```
Aquarium (1) ──contains──> (0..*) SubstrateEntry
```

- SubstrateEntry has no independent lifecycle — it is created, edited, and deleted only through its parent Aquarium.
- Deleting an Aquarium cascades to all its SubstrateEntries (they are embedded in the same JSON).
- The gallery folder `gallery/{aquariumId}/` is associated by convention (same GUID). Deleting an aquarium also deletes its gallery folder.

## State Transitions

```
                    ┌─────────┐
        Create ───> │  Active  │
                    └────┬─────┘
                         │
                 Archive │  ▲ Restore
                         ▼  │
                    ┌─────────┐
                    │ Archived │
                    └────┬─────┘
                         │
                  Delete │         Delete
      (from Active) ─────┼──────────┘
                         ▼
                    ┌─────────┐
                    │ Deleted  │  (file removed, no state persisted)
                    └─────────┘
```

- **Active → Archived**: User confirms archival. Status field updated, all data preserved.
- **Archived → Active**: User restores. Status field updated back.
- **Active → Deleted**: User confirms permanent deletion. JSON file and gallery folder removed.
- **Archived → Deleted**: Same as above.

## Validation Rules

| Rule | Applies To | Condition |
|------|-----------|-----------|
| VR-001 | Aquarium.Name | Required; 1-100 characters; trimmed whitespace |
| VR-002 | Aquarium.Volume | Required; must be > 0 |
| VR-003 | Aquarium.Length | Required; must be > 0 |
| VR-004 | Aquarium.Width | Required; must be > 0 |
| VR-005 | Aquarium.Height | Required; must be > 0 |
| VR-006 | Aquarium.AquariumType | Required; must be a valid enum value |
| VR-007 | Aquarium.SetupDate | Required; must be a valid date (date-only, no time component) |
| VR-008 | Aquarium.Description | Optional; max 2000 characters if provided |
| VR-009 | SubstrateEntry.Brand | Required; non-empty |
| VR-010 | SubstrateEntry.ProductName | Required; non-empty |
| VR-011 | SubstrateEntry.Type | Required; must be a valid enum value |
| VR-012 | SubstrateEntry.LayerDepth | Required; must be > 0 |
| VR-013 | SubstrateEntry.DateAdded | Required; must be a valid date (date-only) |
| VR-014 | ThumbnailPath image file | If provided, must be JPEG, PNG, BMP, GIF, or WebP |

## JSON Serialization

Uses `System.Text.Json` with `JsonNamingPolicy.CamelCase` (matching existing `DataService` configuration).

**Sample JSON** (`aquariums/{guid}.json`):

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Planted 60P",
  "volume": 60.0,
  "volumeUnit": "Liters",
  "length": 60.0,
  "width": 30.0,
  "height": 36.0,
  "dimensionUnit": "Centimeters",
  "aquariumType": "Freshwater",
  "setupDate": "2025-11-15T00:00:00+00:00",
  "description": "ADA 60P iwagumi layout with Seiryu stone",
  "thumbnailPath": "gallery/a1b2c3d4-e5f6-7890-abcd-ef1234567890/thumbnail.jpg",
  "status": "Active",
  "createdAt": "2025-11-15T10:30:00+02:00",
  "substrates": [
    {
      "id": "11111111-2222-3333-4444-555555555555",
      "brand": "ADA",
      "productName": "Power Sand Advance M",
      "type": "Substrate",
      "layerDepth": 2.0,
      "dateAdded": "2025-11-15T00:00:00+00:00",
      "notes": "Base layer under Amazonia",
      "displayOrder": 0
    },
    {
      "id": "66666666-7777-8888-9999-aaaaaaaaaaaa",
      "brand": "ADA",
      "productName": "Aqua Soil Amazonia Ver.2",
      "type": "SoilCap",
      "layerDepth": 5.0,
      "dateAdded": "2025-11-15T00:00:00+00:00",
      "notes": null,
      "displayOrder": 1
    }
  ]
}
```

**Enum serialization**: Uses `JsonStringEnumConverter` to serialize enums as readable strings (e.g., `"Freshwater"` instead of `0`).

## Numeric Precision & Locale

- **Storage**: Volume, dimensions, and layer depth are stored as `double` with full input precision.
- **Display**: Values are displayed with up to 1 decimal place (e.g., 60.5 L, 30.0 cm).
- **Input**: The system accepts the user's system locale decimal separator (comma or period). Parsing uses `CultureInfo.CurrentCulture`.
- **Date fields**: SetupDate and DateAdded are date-only inputs. Stored as `DateTimeOffset` with time set to midnight UTC for serialization compatibility.
