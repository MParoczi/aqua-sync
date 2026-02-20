using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

public sealed partial class FiltersPage : Page
{
    public FiltersPage()
    {
        ViewModel = App.GetService<FiltersViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    public FiltersViewModel ViewModel { get; }
}
