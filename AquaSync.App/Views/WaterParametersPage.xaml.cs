using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class WaterParametersPage : Page
{
    public WaterParametersPage()
    {
        ViewModel = App.GetService<WaterParametersViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public WaterParametersViewModel ViewModel { get; }
}
