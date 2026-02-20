using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AquaSync.App.Contracts.Services;

/// <summary>
///     Provides Frame-based page navigation within the ShellPage.
/// </summary>
public interface INavigationService
{
    bool CanGoBack { get; }

    Frame? Frame { get; set; }
    event NavigatedEventHandler? Navigated;

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();
}
