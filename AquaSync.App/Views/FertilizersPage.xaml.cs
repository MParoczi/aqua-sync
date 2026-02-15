using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class FertilizersPage : Page
{
    public FertilizersViewModel ViewModel { get; }

    public FertilizersPage()
    {
        ViewModel = App.GetService<FertilizersViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
