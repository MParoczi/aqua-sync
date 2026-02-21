using AquaSync.App.Contracts.Services;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class LampsPage : Page
{
    public LampsPage()
    {
        ViewModel = App.GetService<LampsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public LampsViewModel ViewModel { get; }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var lampService = App.GetService<ILampService>();
        var dialog = new AddLampDialog(lampService, ViewModel.CurrentAquariumId)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedDevice is { } device)
            await ViewModel.AddLampAsync(device);
    }
}
