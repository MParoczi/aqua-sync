using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

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

    private void Slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Slider slider && slider.DataContext is ChannelSlider channel)
            ViewModel.ApplyBrightnessCommand.Execute(channel);
    }
}
