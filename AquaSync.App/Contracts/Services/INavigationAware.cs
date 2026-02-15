namespace AquaSync.App.Contracts.Services;

/// <summary>
/// Implemented by ViewModels that need to react to navigation events.
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
