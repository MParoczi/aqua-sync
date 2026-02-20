using AquaSync.App.ViewModels;
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
}
