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

    private readonly IDataService _dataService;
    private readonly MainWindow _mainWindow;

    public SettingsService(IDataService dataService, MainWindow mainWindow)
    {
        _dataService = dataService;
        _mainWindow = mainWindow;
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

        _mainWindow.DispatcherQueue.TryEnqueue(() => _mainWindow.SetTheme(theme));
    }

    public Task ExportDataAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task MoveDataFolderAsync(string newFolderPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
