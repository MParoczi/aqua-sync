using AquaSync.App.Contracts.Services;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AquaSync.App.Views;

/// <summary>
/// The main application shell containing NavigationView sidebar and content Frame.
/// Receives an aquarium ID parameter and initializes the aquarium context.
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

    /// <summary>
    /// Extracts the aquarium ID parameter and initializes the context (FR-022, FR-025).
    /// </summary>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Guid aquariumId)
        {
            await ViewModel.InitializeAsync(aquariumId);
        }
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string pageKey)
        {
            App.GetService<INavigationService>().NavigateTo(pageKey);
        }
    }

    /// <summary>
    /// Navigates back to the aquarium selector grid (FR-026).
    /// Clears the aquarium context and navigates the MainWindow RootFrame.
    /// </summary>
    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        ViewModel.GoBackCommand.Execute(null);

        var mainWindow = App.GetService<MainWindow>();
        mainWindow.ContentFrame.Navigate(typeof(AquariumSelectorPage));
    }

    /// <summary>
    /// Restores the archived aquarium from the read-only banner (FR-031).
    /// </summary>
    private async void RestoreBanner_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RestoreCurrentAquariumAsync();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
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
