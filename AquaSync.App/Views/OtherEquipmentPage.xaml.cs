using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class OtherEquipmentPage : Page
{
    public OtherEquipmentViewModel ViewModel { get; }

    public OtherEquipmentPage()
    {
        ViewModel = App.GetService<OtherEquipmentViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
