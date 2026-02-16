using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using CommunityToolkit.Mvvm.Input;

namespace AquaSync.App.ViewModels;

/// <summary>
/// ViewModel for the aquarium selector launch screen.
/// Loads profiles, exposes sorted collection, and provides CRUD command placeholders.
/// </summary>
public sealed class AquariumSelectorViewModel : ViewModelBase
{
    private readonly IAquariumService _aquariumService;

    private bool _isLoading;
    private bool _hasProfiles;

    public AquariumSelectorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;

        CreateProfileCommand = new RelayCommand(OnCreateProfile);
        ArchiveProfileCommand = new RelayCommand<Aquarium>(OnArchiveProfile);
        RestoreProfileCommand = new RelayCommand<Aquarium>(OnRestoreProfile);
        DeleteProfileCommand = new RelayCommand<Aquarium>(OnDeleteProfile);
    }

    /// <summary>
    /// Sorted collection of aquarium profiles for the grid.
    /// Active first (newest first), then Archived (newest first) per FR-005.
    /// </summary>
    public ObservableCollection<Aquarium> Profiles { get; } = [];

    /// <summary>
    /// True while profiles are being loaded from storage.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// True when at least one profile exists. Used to toggle between grid and empty state.
    /// </summary>
    public bool HasProfiles
    {
        get => _hasProfiles;
        private set => SetProperty(ref _hasProfiles, value);
    }

    /// <summary>
    /// Opens the profile creation form. Placeholder — implemented in US2 (Phase 4).
    /// </summary>
    public IRelayCommand CreateProfileCommand { get; }

    /// <summary>
    /// Archives an active profile. Placeholder — implemented in US5 (Phase 7).
    /// </summary>
    public IRelayCommand<Aquarium> ArchiveProfileCommand { get; }

    /// <summary>
    /// Restores an archived profile. Placeholder — implemented in US5 (Phase 7).
    /// </summary>
    public IRelayCommand<Aquarium> RestoreProfileCommand { get; }

    /// <summary>
    /// Permanently deletes a profile. Placeholder — implemented in US6 (Phase 8).
    /// </summary>
    public IRelayCommand<Aquarium> DeleteProfileCommand { get; }

    /// <summary>
    /// Loads all aquarium profiles and populates the sorted collection.
    /// </summary>
    public async Task LoadProfilesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        try
        {
            var aquariums = await _aquariumService.GetAllAsync(cancellationToken).ConfigureAwait(false);

            // Sort: active first then archived, newest first within each group (FR-005).
            var sorted = aquariums
                .OrderBy(a => a.Status == AquariumStatus.Archived ? 1 : 0)
                .ThenByDescending(a => a.CreatedAt);

            Profiles.Clear();

            foreach (var aquarium in sorted)
            {
                Profiles.Add(aquarium);
            }
        }
        finally
        {
            IsLoading = false;
        }

        HasProfiles = Profiles.Count > 0;
    }

    private void OnCreateProfile()
    {
        // Implemented in US2 (Phase 4).
    }

    private void OnArchiveProfile(Aquarium? aquarium)
    {
        // Implemented in US5 (Phase 7).
    }

    private void OnRestoreProfile(Aquarium? aquarium)
    {
        // Implemented in US5 (Phase 7).
    }

    private void OnDeleteProfile(Aquarium? aquarium)
    {
        // Implemented in US6 (Phase 8).
    }
}
