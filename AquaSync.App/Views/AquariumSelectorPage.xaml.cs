using AquaSync.App.Models;
using AquaSync.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Views;

/// <summary>
/// Launch screen where the user selects an aquarium profile from a grid of cards.
/// </summary>
public sealed partial class AquariumSelectorPage : Page
{
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

    /// <summary>
    /// Handles the "Add Aquarium" card click and empty state create button.
    /// Creation dialog is implemented in US2 (Phase 4).
    /// </summary>
    private void AddAquariumCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CreateProfileCommand.Execute(null);
    }

    /// <summary>
    /// Toggles Archive/Restore menu items based on the card's aquarium status (FR-007).
    /// </summary>
    private void CardMenuFlyout_Opening(object sender, object e)
    {
        if (sender is MenuFlyout flyout && flyout.Items.Count >= 2
            && flyout.Items[0].Tag is Aquarium aquarium)
        {
            // Archive visible for active; Restore visible for archived.
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
