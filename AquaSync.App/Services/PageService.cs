using AquaSync.App.Contracts.Services;
using AquaSync.App.ViewModels;
using AquaSync.App.Views;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Services;

/// <summary>
///     Maintains a bidirectional mapping between ViewModel type names and Page types.
/// </summary>
public sealed class PageService : IPageService
{
    private readonly Dictionary<Type, string> _keys = [];
    private readonly Dictionary<string, Type> _pages = [];

    public PageService()
    {
        Configure<DashboardViewModel, DashboardPage>();
        Configure<LampsViewModel, LampsPage>();
        Configure<FiltersViewModel, FiltersPage>();
        Configure<OtherEquipmentViewModel, OtherEquipmentPage>();
        Configure<WaterParametersViewModel, WaterParametersPage>();
        Configure<MaintenanceViewModel, MaintenancePage>();
        Configure<GalleryViewModel, GalleryPage>();
        Configure<FertilizersViewModel, FertilizersPage>();
        Configure<PlantsViewModel, PlantsPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        if (!_pages.TryGetValue(key, out var pageType)) throw new ArgumentException($"Page not found for key '{key}'. Did you forget to call Configure?");

        return pageType;
    }

    public string GetPageKey(Type pageType)
    {
        if (!_keys.TryGetValue(pageType, out var key)) throw new ArgumentException($"Key not found for page type '{pageType.FullName}'.");

        return key;
    }

    private void Configure<TViewModel, TPage>() where TPage : Page
    {
        var key = typeof(TViewModel).FullName!;
        _pages[key] = typeof(TPage);
        _keys[typeof(TPage)] = key;
    }
}
