using AquaSync.App.Contracts.Services;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AquaSync.App.Views;

/// <summary>
/// The main application shell containing NavigationView sidebar and content Frame.
/// </summary>
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        var navigationService = App.GetService<INavigationService>();
        navigationService.Frame = ContentFrame;
        navigationService.Navigated += OnNavigated;

        navigationService.NavigateTo(typeof(DashboardViewModel).FullName!, clearNavigation: true);
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string pageKey)
        {
            App.GetService<INavigationService>().NavigateTo(pageKey);
        }
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        App.GetService<INavigationService>().GoBack();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        ViewModel.IsBackEnabled = App.GetService<INavigationService>().CanGoBack;

        if (e.SourcePageType is not null)
        {
            var pageKey = App.GetService<IPageService>().GetPageKey(e.SourcePageType);

            var item = NavigationViewControl.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => i.Tag as string == pageKey)
                ?? NavigationViewControl.FooterMenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag as string == pageKey);

            if (item is not null)
            {
                NavigationViewControl.SelectedItem = item;
            }
        }
    }
}
