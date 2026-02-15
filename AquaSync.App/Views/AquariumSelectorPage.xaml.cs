using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

/// <summary>
/// Launch screen where the user selects an aquarium profile.
/// </summary>
public sealed partial class AquariumSelectorPage : Page
{
    public AquariumSelectorViewModel ViewModel { get; }

    public AquariumSelectorPage()
    {
        ViewModel = App.GetService<AquariumSelectorViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
