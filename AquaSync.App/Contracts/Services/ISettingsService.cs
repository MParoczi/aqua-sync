using AquaSync.App.Models;

namespace AquaSync.App.Contracts.Services;

/// <summary>
///     Manages global application settings including unit preferences,
///     theme, data folder location, and data export.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    ///     Gets the current application settings. Always non-null after initialization.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    ///     Loads settings from disk. Called once during app startup.
    ///     Returns defaults if no settings file exists.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves the current settings to disk.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies the current theme setting to the application window.
    ///     Called at startup and whenever the theme setting changes.
    /// </summary>
    void ApplyTheme();

    /// <summary>
    ///     Exports all application data to a ZIP archive at the specified path.
    /// </summary>
    Task ExportDataAsync(string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves the data folder to a new location. Copies all files, updates
    ///     the redirect file, and removes old data on success.
    /// </summary>
    Task MoveDataFolderAsync(string newFolderPath, CancellationToken cancellationToken = default);
}
