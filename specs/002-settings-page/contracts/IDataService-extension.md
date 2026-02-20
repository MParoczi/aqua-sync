# Contract Extension: IDataService

**Feature**: 002-settings-page
**Date**: 2026-02-20

## New Method Required

`DataService` needs to support updating its internal `_rootPath` at runtime when the data folder is relocated.

```csharp
// Add to IDataService interface:

/// <summary>
///     Updates the root data folder path. Used when the user relocates
///     the data storage directory.
/// </summary>
void SetDataFolderPath(string newPath);
```

## Implementation Notes

```csharp
// In DataService:
public void SetDataFolderPath(string newPath)
{
    _rootPath = newPath;
    Directory.CreateDirectory(_rootPath);
}
```

The `_rootPath` field must be changed from `readonly` to mutable. Thread safety is maintained by the existing `SemaphoreSlim` — all read/write operations already acquire the lock before using `_rootPath`.

## Startup Change

On construction, `DataService` should check for `data-folder-redirect.json` at the default `%LOCALAPPDATA%/AquaSync/` location:

```csharp
public DataService()
{
    var defaultRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AquaSync");

    Directory.CreateDirectory(defaultRoot);

    // Check for custom data folder redirect
    var redirectPath = Path.Combine(defaultRoot, "data-folder-redirect.json");
    if (File.Exists(redirectPath))
    {
        // Read redirect and use custom path if valid
        var json = File.ReadAllText(redirectPath);
        var redirect = JsonSerializer.Deserialize<DataFolderRedirect>(json, s_jsonOptions);
        if (redirect?.CustomDataFolderPath is not null && Directory.Exists(redirect.CustomDataFolderPath))
        {
            _rootPath = redirect.CustomDataFolderPath;
            return;
        }
    }

    _rootPath = defaultRoot;
}
```

## DataFolderRedirect Model

A simple record stored at the fixed default location:

```csharp
namespace AquaSync.App.Models;

public sealed record DataFolderRedirect
{
    public string? CustomDataFolderPath { get; init; }
}
```
