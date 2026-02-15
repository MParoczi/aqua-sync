using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class GalleryPage : Page
{
    public GalleryViewModel ViewModel { get; }

    public GalleryPage()
    {
        ViewModel = App.GetService<GalleryViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
