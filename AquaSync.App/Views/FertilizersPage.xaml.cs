using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class FertilizersPage : Page
{
    public FertilizersPage()
    {
        ViewModel = App.GetService<FertilizersViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public FertilizersViewModel ViewModel { get; }
}
