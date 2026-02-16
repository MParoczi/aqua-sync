using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class MaintenancePage : Page
{
    public MaintenancePage()
    {
        ViewModel = App.GetService<MaintenanceViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public MaintenanceViewModel ViewModel { get; }
}
