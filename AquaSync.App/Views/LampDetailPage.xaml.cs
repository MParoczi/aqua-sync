using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class LampDetailPage : Page
{
    public LampDetailPage()
    {
        ViewModel = App.GetService<LampDetailViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public LampDetailViewModel ViewModel { get; }
}
