using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

/// <summary>
///     The application's main window. Hosts a root Frame that switches between
///     AquariumSelectorPage (launch screen) and ShellPage (after aquarium selection).
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.Resize(new SizeInt32(1200, 800));
        RootFrame.Navigate(typeof(AquariumSelectorPage));
    }

    /// <summary>
    ///     The root frame used for top-level navigation (selector vs. shell).
    /// </summary>
    public Frame ContentFrame => RootFrame;

    /// <summary>
    ///     Sets the application theme on the root content element.
    /// </summary>
    public void SetTheme(ElementTheme theme)
    {
        if (Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = theme;
    }
}
