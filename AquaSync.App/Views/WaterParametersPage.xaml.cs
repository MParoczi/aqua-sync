using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class WaterParametersPage : Page
{
    public WaterParametersViewModel ViewModel { get; }

    public WaterParametersPage()
    {
        ViewModel = App.GetService<WaterParametersViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
