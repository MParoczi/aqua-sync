# Service Contracts: Aquarium Profile Management

**Branch**: `001-aquarium-profiles` | **Date**: 2026-02-15

## Overview

This feature introduces three new service interfaces and extends one existing interface. All services follow the established DI pattern (registered in `App.xaml.cs`, resolved via `App.GetService<T>()`).

---

## IDataService (EXTENDED)

**Location**: `AquaSync.App/Contracts/Services/IDataService.cs`
**Lifetime**: Singleton (existing)

### New Method

#### ReadAllAsync\<T\>

Enumerates and deserializes all JSON files in a folder.

```
ReadAllAsync<T>(string folderName) → Task<IReadOnlyList<T>>
    where T : class
```

| Parameter | Type | Description |
|-----------|------|-------------|
| folderName | string | Subfolder name under the data root (e.g., "aquariums") |

**Returns**: List of deserialized objects from all `.json` files in the folder. Returns empty list if folder does not exist or contains no JSON files.

**Behavior**:
- Enumerate all `*.json` files in `{rootPath}/{folderName}/`
- Deserialize each file using the same `JsonSerializerOptions` as `ReadAsync`
- Skip files that fail deserialization (log warning, do not throw)
- Thread-safe via existing `SemaphoreSlim` lock

---

## IAquariumService (NEW)

**Location**: `AquaSync.App/Contracts/Services/IAquariumService.cs`
**Implementation**: `AquaSync.App/Services/AquariumService.cs`
**Lifetime**: Singleton
**Dependencies**: `IDataService`

Provides CRUD operations for aquarium profiles and manages associated gallery files.

### Methods

#### GetAllAsync

```
GetAllAsync() → Task<IReadOnlyList<Aquarium>>
```

Returns all aquarium profiles (active and archived), ordered by CreatedAt descending (newest first).

---

#### GetByIdAsync

```
GetByIdAsync(Guid id) → Task<Aquarium?>
```

Returns a single aquarium by its ID, or null if not found.

| Parameter | Type | Description |
|-----------|------|-------------|
| id | Guid | The aquarium's unique identifier |

---

#### SaveAsync

```
SaveAsync(Aquarium aquarium) → Task
```

Creates or updates an aquarium profile. For new aquariums, `Id` and `CreatedAt` should already be set by the caller.

| Parameter | Type | Description |
|-----------|------|-------------|
| aquarium | Aquarium | The aquarium profile to persist |

**Behavior**:
- Serializes the aquarium (including embedded substrates) to `aquariums/{id}.json`
- Creates the `aquariums/` folder if it doesn't exist

---

#### DeleteAsync

```
DeleteAsync(Guid id) → Task
```

Permanently deletes an aquarium profile and its associated gallery folder.

| Parameter | Type | Description |
|-----------|------|-------------|
| id | Guid | The aquarium's unique identifier |

**Behavior**:
- Deletes `aquariums/{id}.json`
- Deletes `gallery/{id}/` folder and all contents (thumbnails)
- No-op if the aquarium doesn't exist

---

#### SaveThumbnailAsync

```
SaveThumbnailAsync(Guid aquariumId, string sourceFilePath) → Task<string>
```

Copies an image file to the aquarium's gallery folder and returns the relative path for storage in the profile JSON.

| Parameter | Type | Description |
|-----------|------|-------------|
| aquariumId | Guid | The aquarium's unique identifier |
| sourceFilePath | string | Full path to the source image file |

**Returns**: Relative path to the stored thumbnail (e.g., `gallery/{id}/thumbnail.jpg`)

**Behavior**:
- Creates `gallery/{aquariumId}/` folder if it doesn't exist
- Deletes any existing thumbnail in the folder before copying
- Copies the source file, preserving the original extension
- Returns the relative path for storage in `Aquarium.ThumbnailPath`

---

#### DeleteThumbnailAsync

```
DeleteThumbnailAsync(Guid aquariumId) → Task
```

Deletes the thumbnail image from the aquarium's gallery folder.

| Parameter | Type | Description |
|-----------|------|-------------|
| aquariumId | Guid | The aquarium's unique identifier |

**Behavior**:
- Removes all files in `gallery/{aquariumId}/` matching thumbnail patterns
- No-op if no thumbnail exists

---

## IAquariumContext (NEW)

**Location**: `AquaSync.App/Contracts/Services/IAquariumContext.cs`
**Implementation**: `AquaSync.App/Services/AquariumContext.cs`
**Lifetime**: Singleton

Holds the currently selected aquarium for the active management shell session. Injected by child ViewModels to access the current aquarium's data without navigation parameter threading.

### Properties

#### CurrentAquarium

```
Aquarium? CurrentAquarium { get; }
```

The currently selected aquarium, or null if no aquarium is selected (user is on the selector page).

---

#### IsReadOnly

```
bool IsReadOnly { get; }
```

True when the current aquarium is archived (read-only mode). ViewModels check this to disable editing controls.

---

### Methods

#### SetCurrentAquarium

```
SetCurrentAquarium(Aquarium aquarium) → void
```

Sets the active aquarium context. Called by ShellPage when entering the management shell. Automatically sets `IsReadOnly` based on the aquarium's status.

| Parameter | Type | Description |
|-----------|------|-------------|
| aquarium | Aquarium | The aquarium to set as current context |

---

#### Clear

```
Clear() → void
```

Clears the current aquarium context. Called when navigating back to the selector page. Sets `CurrentAquarium` to null and `IsReadOnly` to false.

---

## DI Registration Summary

All services registered in `App.xaml.cs` `ConfigureServices`:

```
services.AddSingleton<IDataService, DataService>();          // EXISTING
services.AddSingleton<IAquariumService, AquariumService>();  // NEW
services.AddSingleton<IAquariumContext, AquariumContext>();   // NEW
```

Note: `IPageService` and `INavigationService` remain unchanged.
