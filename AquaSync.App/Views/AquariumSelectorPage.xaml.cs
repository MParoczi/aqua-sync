using AquaSync.App.Models;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AquaSync.App.Views;

/// <summary>
/// Launch screen where the user selects an aquarium profile from a grid of cards.
/// </summary>
public sealed partial class AquariumSelectorPage : Page
{
    private bool _forceCloseDialog;

    public AquariumSelectorViewModel ViewModel { get; }

    public AquariumSelectorPage()
    {
        ViewModel = App.GetService<AquariumSelectorViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += AquariumSelectorPage_Loaded;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void AquariumSelectorPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadProfilesAsync();
    }

    /// <summary>
    /// Toggle between empty state and grid based on HasProfiles.
    /// </summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.HasProfiles) or nameof(ViewModel.IsLoading))
        {
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (ViewModel.IsLoading)
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            AquariumGrid.Visibility = Visibility.Collapsed;
        }
        else if (ViewModel.HasProfiles)
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            AquariumGrid.Visibility = Visibility.Visible;
        }
        else
        {
            EmptyStatePanel.Visibility = Visibility.Visible;
            AquariumGrid.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Handles clicking a profile card in the grid.
    /// Navigation to shell is implemented in US4 (Phase 5).
    /// </summary>
    private void AquariumGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        // Implemented in US4 (Phase 5) — navigate to ShellPage with aquarium context.
    }

    // ========================================================================
    // Creation dialog (FR-008, FR-012, FR-013, FR-014)
    // ========================================================================

    /// <summary>
    /// Opens the creation dialog from the "Add Aquarium" card or empty state button.
    /// </summary>
    private async void AddAquariumCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetCreationForm();
        _forceCloseDialog = false;

        CreateProfileDialog.XamlRoot = XamlRoot;
        await CreateProfileDialog.ShowAsync();
    }

    /// <summary>
    /// Validates and saves the new profile when the user clicks "Save" (FR-034, FR-035).
    /// </summary>
    private async void CreateProfileDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        if (!ViewModel.ValidateCreationForm())
        {
            args.Cancel = true;
            return;
        }

        var deferral = args.GetDeferral();
        try
        {
            await ViewModel.SaveNewProfileAsync();
            ViewModel.ResetCreationForm();
        }
        finally
        {
            deferral.Complete();
        }
    }

    /// <summary>
    /// Intercepts cancel to show discard confirmation when unsaved data exists (FR-014).
    /// </summary>
    private void CreateProfileDialog_Closing(
        ContentDialog sender,
        ContentDialogClosingEventArgs args)
    {
        if (args.Result == ContentDialogResult.None
            && ViewModel.HasUnsavedCreationData
            && !_forceCloseDialog)
        {
            args.Cancel = true;
            ViewModel.ShowDiscardConfirmation = true;
        }
    }

    /// <summary>
    /// Confirms discarding unsaved changes and closes the dialog.
    /// </summary>
    private void ConfirmDiscard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetCreationForm();
        _forceCloseDialog = true;
        CreateProfileDialog.Hide();
        _forceCloseDialog = false;
    }

    /// <summary>
    /// Opens a file picker for thumbnail photo selection (FR-012).
    /// Validates format and 10 MB size limit.
    /// </summary>
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

    /// <summary>
    /// Clears the selected thumbnail photo.
    /// </summary>
    private void ClearPhoto_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearThumbnailPreview();
        PhotoErrorText.Visibility = Visibility.Collapsed;
    }

    // ========================================================================
    // Context menu handlers (FR-007)
    // ========================================================================

    /// <summary>
    /// Toggles Archive/Restore menu items based on the card's aquarium status.
    /// </summary>
    private void CardMenuFlyout_Opening(object sender, object e)
    {
        if (sender is MenuFlyout flyout && flyout.Items.Count >= 2
            && flyout.Items[0].Tag is Aquarium aquarium)
        {
            flyout.Items[0].Visibility = aquarium.IsArchived ? Visibility.Collapsed : Visibility.Visible;
            flyout.Items[1].Visibility = aquarium.IsArchived ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ArchiveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: Aquarium aquarium })
        {
            ViewModel.ArchiveProfileCommand.Execute(aquarium);
        }
    }

    private void RestoreMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: Aquarium aquarium })
        {
            ViewModel.RestoreProfileCommand.Execute(aquarium);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: Aquarium aquarium })
        {
            ViewModel.DeleteProfileCommand.Execute(aquarium);
        }
    }
}
