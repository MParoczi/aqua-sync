using System.IO.Compression;
using System.Text.Json;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.App.Views;
using Microsoft.UI.Xaml;

namespace AquaSync.App.Services;

/// <summary>
///     Manages global application settings: unit defaults, theme, data export,
///     and data folder location.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private const string SettingsFolder = "settings";
    private const string SettingsFile = "app-settings";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDataService _dataService;

    public SettingsService(IDataService dataService)
    {
        _dataService = dataService;
        Settings = new AppSettings();
    }

    public AppSettings Settings { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var loaded = await _dataService.ReadAsync<AppSettings>(SettingsFolder, SettingsFile)
            .ConfigureAwait(false);

        if (loaded is not null)
            Settings = loaded;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _dataService.SaveAsync(SettingsFolder, SettingsFile, Settings)
            .ConfigureAwait(false);
    }

    public void ApplyTheme()
    {
        var theme = Settings.Theme switch
        {
            AppTheme.Light => ElementTheme.Light,
            AppTheme.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        var mainWindow = App.GetService<MainWindow>();
        mainWindow.DispatcherQueue.TryEnqueue(() => mainWindow.SetTheme(theme));
    }

    public async Task ExportDataAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        var sourceDir = _dataService.GetDataFolderPath();

        if (!Directory.EnumerateFileSystemEntries(sourceDir).Any())
            throw new InvalidOperationException("The data folder is empty. There is nothing to export.");

        await Task.Run(() =>
        {
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            ZipFile.CreateFromDirectory(sourceDir, destinationPath);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveDataFolderAsync(string newFolderPath, CancellationToken cancellationToken = default)
    {
        var sourcePath = _dataService.GetDataFolderPath();
        var defaultRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AquaSync");

        var normalizedSource = Path.GetFullPath(sourcePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedDest = Path.GetFullPath(newFolderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedDefault = Path.GetFullPath(defaultRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(normalizedSource, normalizedDest, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The source and destination folders are the same.");

        var isDefaultDest = string.Equals(normalizedDest, normalizedDefault, StringComparison.OrdinalIgnoreCase);
        var isDefaultSource = string.Equals(normalizedSource, normalizedDefault, StringComparison.OrdinalIgnoreCase);

        if (!isDefaultDest
            && Directory.Exists(newFolderPath)
            && Directory.EnumerateFileSystemEntries(newFolderPath).Any())
            throw new InvalidOperationException("The destination folder is not empty. Please choose an empty folder.");

        await Task.Run(() =>
        {
            var redirectPath = Path.Combine(defaultRoot, "data-folder-redirect.json");
            try
            {
                CopyDirectory(sourcePath, newFolderPath, "data-folder-redirect.json");

                if (isDefaultDest)
                {
                    // Resetting to default: delete the redirect file so the next startup uses default.
                    if (File.Exists(redirectPath))
                        File.Delete(redirectPath);
                }
                else
                {
                    // Moving to custom: write redirect file pointing to new location.
                    var redirect = new DataFolderRedirect { CustomDataFolderPath = newFolderPath };
                    File.WriteAllText(redirectPath, JsonSerializer.Serialize(redirect, s_jsonOptions));
                }

                // Delete old source data.
                if (isDefaultSource)
                {
                    // Keep the default root directory; only delete its data contents
                    // (the redirect file was already handled above).
                    foreach (var f in Directory.GetFiles(sourcePath)
                                 .Where(f => !string.Equals(
                                     Path.GetFileName(f), "data-folder-redirect.json",
                                     StringComparison.OrdinalIgnoreCase)))
                        File.Delete(f);
                    foreach (var d in Directory.GetDirectories(sourcePath))
                        Directory.Delete(d, true);
                }
                else
                {
                    Directory.Delete(sourcePath, true);
                }
            }
            catch
            {
                // Rollback: remove partial copy and keep the original.
                try
                {
                    if (!isDefaultDest && Directory.Exists(newFolderPath))
                        Directory.Delete(newFolderPath, true);
                    // For a failed reset-to-default: the redirect file was not yet modified
                    // when the copy throws, so the original custom folder remains intact
                    // and the app will continue working from it on next startup.
                }
                catch
                {
                    /* best-effort rollback */
                }

                throw;
            }
        }, cancellationToken).ConfigureAwait(false);

        _dataService.SetDataFolderPath(newFolderPath);
    }

    private static void CopyDirectory(string source, string destination, string? excludeFileName = null)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            if (excludeFileName is not null
                && string.Equals(Path.GetFileName(file), excludeFileName, StringComparison.OrdinalIgnoreCase))
                continue;
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
        }

        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)), excludeFileName);
    }
}
