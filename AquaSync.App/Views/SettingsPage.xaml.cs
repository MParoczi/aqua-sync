using System.ComponentModel;
using Windows.Storage.Pickers;
using AquaSync.App.Models;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace AquaSync.App.Views;

/// <summary>
///     Settings page with dual-scope layout: global settings and aquarium-scoped
///     profile editing with substrate management (FR-016, FR-017, FR-027).
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += SettingsPage_Loaded;
    }

    public SettingsViewModel ViewModel { get; }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.LoadFromContext();
        SyncUnitRadioButtons();
        SyncThemeRadioButtons();
        SyncSectionListView();
        SyncAquariumSectionVisibility();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.SelectedVolumeUnit):
            case nameof(ViewModel.SelectedDimensionUnit):
                SyncUnitRadioButtons();
                break;
            case nameof(ViewModel.SelectedTheme):
                SyncThemeRadioButtons();
                break;
            case nameof(ViewModel.IsNavigationBlocked):
                SetNavigationBlocked(ViewModel.IsNavigationBlocked);
                break;
            case nameof(ViewModel.SelectedSection):
                SyncSectionListView();
                break;
            case nameof(ViewModel.HasAquarium):
                SyncAquariumSectionVisibility();
                break;
        }
    }

    private void SetNavigationBlocked(bool blocked)
    {
        BackButton.IsEnabled = !blocked;

        // Walk up the visual tree to find ShellPage and disable its NavigationView.
        DependencyObject? parent = Frame;
        while (parent is not null)
        {
            if (parent is ShellPage shellPage)
            {
                shellPage.SetNavigationEnabled(!blocked);
                break;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }
    }

    private void SyncUnitRadioButtons()
    {
        VolumeUnitRadioButtons.SelectedIndex = (int)ViewModel.SelectedVolumeUnit;
        DimensionUnitRadioButtons.SelectedIndex = (int)ViewModel.SelectedDimensionUnit;
    }

    private void SyncThemeRadioButtons()
    {
        ThemeRadioButtons.SelectedIndex = (int)ViewModel.SelectedTheme;
    }

    private void SyncSectionListView()
    {
        SectionListView.SelectedIndex = (int)ViewModel.SelectedSection;
    }

    private void SyncAquariumSectionVisibility()
    {
        AquariumSectionItem.Visibility = ViewModel.HasAquarium ? Visibility.Visible : Visibility.Collapsed;
    }

    // ========================================================================
    // Section navigation handler
    // ========================================================================

    private void SectionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SectionListView.SelectedItem is ListViewItem { Tag: string tagStr }
            && Enum.TryParse<SettingsSection>(tagStr, out var section))
            ViewModel.SelectedSection = section;
    }

    // ========================================================================
    // Default unit selection handlers (US1)
    // ========================================================================

    private void VolumeUnitRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VolumeUnitRadioButtons.SelectedIndex >= 0)
            ViewModel.SelectedVolumeUnit = (VolumeUnit)VolumeUnitRadioButtons.SelectedIndex;
    }

    private void DimensionUnitRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DimensionUnitRadioButtons.SelectedIndex >= 0)
            ViewModel.SelectedDimensionUnit = (DimensionUnit)DimensionUnitRadioButtons.SelectedIndex;
    }

    // ========================================================================
    // Data export handler (US3)
    // ========================================================================

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("ZIP archive", new List<string> { ".zip" });
        picker.SuggestedFileName = $"AquaSync-Export-{DateTime.Today:yyyy-MM-dd}";

        var mainWindow = App.GetService<MainWindow>();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(mainWindow));

        var file = await picker.PickSaveFileAsync();
        if (file is null) return; // user cancelled

        await ViewModel.ExportDataCommand.ExecuteAsync(file.Path);
    }

    // ========================================================================
    // Theme selection handler (US2)
    // ========================================================================

    private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeRadioButtons.SelectedIndex >= 0)
            ViewModel.SelectedTheme = (AppTheme)ThemeRadioButtons.SelectedIndex;
    }

    // ========================================================================
    // Data folder handlers (US4)
    // ========================================================================

    private async void BrowseDataFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add("*");

        var mainWindow = App.GetService<MainWindow>();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(mainWindow));

        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return; // user cancelled

        var confirmed = await ShowMoveConfirmationDialogAsync(ViewModel.DataFolderPath, folder.Path);
        if (!confirmed) return;

        await ViewModel.BrowseDataFolderCommand.ExecuteAsync(folder.Path);
    }

    private async void ResetDataFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AquaSync");

        var confirmed = await ShowMoveConfirmationDialogAsync(ViewModel.DataFolderPath, defaultPath);
        if (!confirmed) return;

        await ViewModel.ResetDataFolderCommand.ExecuteAsync(null);
    }

    private async Task<bool> ShowMoveConfirmationDialogAsync(string sourcePath, string destinationPath)
    {
        var content = new TextBlock
        {
            Text = $"From:\n{sourcePath}\n\nTo:\n{destinationPath}\n\nThis may take several minutes. Do not close the app during the move.",
            TextWrapping = TextWrapping.Wrap
        };

        var dialog = new ContentDialog
        {
            Title = "Move Data Folder",
            Content = content,
            PrimaryButtonText = "Move",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };
        dialog.Resources["ContentDialogMinWidth"] = 500.0;
        dialog.Resources["ContentDialogMaxWidth"] = 500.0;

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    // ========================================================================
    // Back navigation for standalone mode (FR-001)
    // ========================================================================

    /// <summary>
    ///     Navigates back to AquariumSelectorPage when in standalone mode.
    /// </summary>
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.GetService<MainWindow>();
        mainWindow.ContentFrame.Navigate(typeof(AquariumSelectorPage));
    }

    // ========================================================================
    // Profile editing handlers (FR-016)
    // ========================================================================

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveProfileCommand.Execute(null);
    }

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
        if (file is null) return;

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

    private void ShowSubstrateForm_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowSubstrateFormCommand.Execute(null);
    }

    private void SaveSubstrateEntry_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveSubstrateEntryCommand.Execute(null);
    }

    private void CancelSubstrateEntry_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelSubstrateEntryCommand.Execute(null);
    }

    private void RemoveSubstrate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry }) ViewModel.RemoveSubstrateCommand.Execute(entry);
    }

    private void MoveSubstrateUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry }) ViewModel.MoveSubstrateUpCommand.Execute(entry);
    }

    private void MoveSubstrateDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SubstrateEntry entry }) ViewModel.MoveSubstrateDownCommand.Execute(entry);
    }
}
