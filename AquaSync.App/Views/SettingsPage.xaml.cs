using AquaSync.App.Models;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AquaSync.App.Views;

/// <summary>
/// Settings page with dual-scope layout: global settings and aquarium-scoped
/// profile editing with substrate management (FR-016, FR-017, FR-027).
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadFromContext();
    }

    // ========================================================================
    // Profile editing handlers (FR-016)
    // ========================================================================

    private void SaveProfile_Click(object sender, RoutedEventArgs e) =>
        ViewModel.SaveProfileCommand.Execute(null);

    private async void BrowsePhoto_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".webp");

        var mainWindow = App.GetService<MainWindow>();
        var hwnd = WindowNative.GetWindowHandle(mainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        var properties = await file.GetBasicPropertiesAsync();
        if (properties.Size > 10 * 1024 * 1024)
        {
            PhotoErrorText.Text = "File must be under 10 MB";
            PhotoErrorText.Visibility = Visibility.Visible;
            return;
        }

        PhotoErrorText.Visibility = Visibility.Collapsed;
        ViewModel.SetThumbnailPreview(file.Path);
    }

    private void ClearPhoto_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearThumbnailPreview();
        PhotoErrorText.Visibility = Visibility.Collapsed;
    }

    // ========================================================================
    // Substrate entry handlers (FR-018, FR-020, FR-021)
    // ========================================================================

    private void ShowSubstrateForm_Click(object sender, RoutedEventArgs e) =>
        ViewModel.ShowSubstrateFormCommand.Execute(null);

    private void SaveSubstrateEntry_Click(object sender, RoutedEventArgs e) =>
        ViewModel.SaveSubstrateEntryCommand.Execute(null);

    private void CancelSubstrateEntry_Click(object sender, RoutedEventArgs e) =>
        ViewModel.CancelSubstrateEntryCommand.Execute(null);

    private void RemoveSubstrate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry })
        {
            ViewModel.RemoveSubstrateCommand.Execute(entry);
        }
    }

    private void MoveSubstrateUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry })
        {
            ViewModel.MoveSubstrateUpCommand.Execute(entry);
        }
    }

    private void MoveSubstrateDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry })
        {
            ViewModel.MoveSubstrateDownCommand.Execute(entry);
        }
    }
}
