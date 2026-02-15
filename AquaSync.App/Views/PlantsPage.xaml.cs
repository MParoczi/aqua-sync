using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class PlantsPage : Page
{
    public PlantsViewModel ViewModel { get; }

    public PlantsPage()
    {
        ViewModel = App.GetService<PlantsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
