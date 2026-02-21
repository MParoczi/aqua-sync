using AquaSync.App.Contracts.Services;
using AquaSync.App.Services;
using AquaSync.App.ViewModels;
using AquaSync.App.Views;
using AquaSync.Chihiros.Discovery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace AquaSync.App;

/// <summary>
///     Entry point for the AquaSync application. Configures DI and launches the main window.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Core services
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDataService, DataService>();
                services.AddSingleton<IAquariumService, AquariumService>();
                services.AddSingleton<IAquariumContext, AquariumContext>();
                services.AddSingleton<ISettingsService, SettingsService>();

                // Lamp services
                services.AddSingleton<IDeviceScanner, DeviceScanner>();
                services.AddSingleton<ILampService, LampService>();

                // Main window
                services.AddSingleton<MainWindow>();

                // ViewModels
                services.AddTransient<ShellViewModel>();
                services.AddTransient<AquariumSelectorViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<LampsViewModel>();
                services.AddTransient<LampDetailViewModel>();
                services.AddTransient<FiltersViewModel>();
                services.AddTransient<OtherEquipmentViewModel>();
                services.AddTransient<WaterParametersViewModel>();
                services.AddTransient<MaintenanceViewModel>();
                services.AddTransient<GalleryViewModel>();
                services.AddTransient<FertilizersViewModel>();
                services.AddTransient<PlantsViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddTransient<ShellPage>();
                services.AddTransient<AquariumSelectorPage>();
                services.AddTransient<DashboardPage>();
                services.AddTransient<LampsPage>();
                services.AddTransient<LampDetailPage>();
                services.AddTransient<FiltersPage>();
                services.AddTransient<OtherEquipmentPage>();
                services.AddTransient<WaterParametersPage>();
                services.AddTransient<MaintenancePage>();
                services.AddTransient<GalleryPage>();
                services.AddTransient<FertilizersPage>();
                services.AddTransient<PlantsPage>();
                services.AddTransient<SettingsPage>();
            })
            .Build();
    }

    /// <summary>
    ///     Gets a registered service from the DI container.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        if ((Current as App)!._host.Services.GetService<T>() is not { } service) throw new ArgumentException($"Service of type {typeof(T)} is not registered.");

        return service;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var settingsService = GetService<ISettingsService>();
        await settingsService.InitializeAsync();
        settingsService.ApplyTheme();

        var mainWindow = GetService<MainWindow>();
        mainWindow.Activate();
    }
}
