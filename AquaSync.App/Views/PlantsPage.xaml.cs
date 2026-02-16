using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class PlantsPage : Page
{
    public PlantsPage()
    {
        ViewModel = App.GetService<PlantsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public PlantsViewModel ViewModel { get; }
}
