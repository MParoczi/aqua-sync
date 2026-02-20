namespace AquaSync.App.Models;

/// <summary>
///     Points to a custom data folder location. Stored at the fixed default
///     <c>%LOCALAPPDATA%/AquaSync/data-folder-redirect.json</c> path.
/// </summary>
public sealed record DataFolderRedirect
{
    public string? CustomDataFolderPath { get; init; }
}
