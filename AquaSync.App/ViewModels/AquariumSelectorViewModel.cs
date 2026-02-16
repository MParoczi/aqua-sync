using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AquaSync.App.ViewModels;

/// <summary>
/// ViewModel for the aquarium selector launch screen.
/// Loads profiles, exposes sorted collection, and provides profile creation logic.
/// </summary>
public sealed class AquariumSelectorViewModel : ViewModelBase
{
    private readonly IAquariumService _aquariumService;

    // --- Grid state ---
    private bool _isLoading;
    private bool _hasProfiles;

    // --- Creation form fields ---
    private string _newName = string.Empty;
    private double _newVolume = double.NaN;
    private bool _isVolumeLiters = true;
    private double _newLength = double.NaN;
    private double _newWidth = double.NaN;
    private double _newHeight = double.NaN;
    private bool _isDimensionCentimeters = true;
    private int _newAquariumTypeIndex = -1;
    private DateTimeOffset? _newSetupDate;
    private string _newNotes = string.Empty;
    private string? _newThumbnailSourcePath;
    private BitmapImage? _thumbnailPreview;

    // --- Validation errors ---
    private string _nameError = string.Empty;
    private string _volumeError = string.Empty;
    private string _dimensionError = string.Empty;
    private string _typeError = string.Empty;
    private string _dateError = string.Empty;

    // --- Duplicate name warning ---
    private string _duplicateNameWarning = string.Empty;

    // --- Discard confirmation ---
    private bool _showDiscardConfirmation;

    public AquariumSelectorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;

        CreateProfileCommand = new RelayCommand(OnCreateProfile);
        ArchiveProfileCommand = new RelayCommand<Aquarium>(OnArchiveProfile);
        RestoreProfileCommand = new RelayCommand<Aquarium>(OnRestoreProfile);
        DeleteProfileCommand = new RelayCommand<Aquarium>(OnDeleteProfile);

        ResetCreationForm();
    }

    // ========================================================================
    // Grid properties
    // ========================================================================

    public ObservableCollection<Aquarium> Profiles { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool HasProfiles
    {
        get => _hasProfiles;
        private set => SetProperty(ref _hasProfiles, value);
    }

    // ========================================================================
    // Commands
    // ========================================================================

    public IRelayCommand CreateProfileCommand { get; }
    public IRelayCommand<Aquarium> ArchiveProfileCommand { get; }
    public IRelayCommand<Aquarium> RestoreProfileCommand { get; }
    public IRelayCommand<Aquarium> DeleteProfileCommand { get; }

    // ========================================================================
    // Creation form properties (FR-008)
    // ========================================================================

    public IReadOnlyList<AquariumType> AquariumTypes { get; } =
        Enum.GetValues<AquariumType>();

    public string NewName
    {
        get => _newName;
        set
        {
            if (SetProperty(ref _newName, value))
            {
                CheckDuplicateName();
            }
        }
    }

    public double NewVolume
    {
        get => _newVolume;
        set => SetProperty(ref _newVolume, value);
    }

    public bool IsVolumeLiters
    {
        get => _isVolumeLiters;
        set => SetProperty(ref _isVolumeLiters, value);
    }

    public double NewLength
    {
        get => _newLength;
        set => SetProperty(ref _newLength, value);
    }

    public double NewWidth
    {
        get => _newWidth;
        set => SetProperty(ref _newWidth, value);
    }

    public double NewHeight
    {
        get => _newHeight;
        set => SetProperty(ref _newHeight, value);
    }

    public bool IsDimensionCentimeters
    {
        get => _isDimensionCentimeters;
        set => SetProperty(ref _isDimensionCentimeters, value);
    }

    public int NewAquariumTypeIndex
    {
        get => _newAquariumTypeIndex;
        set => SetProperty(ref _newAquariumTypeIndex, value);
    }

    public DateTimeOffset? NewSetupDate
    {
        get => _newSetupDate;
        set => SetProperty(ref _newSetupDate, value);
    }

    public string NewNotes
    {
        get => _newNotes;
        set => SetProperty(ref _newNotes, value);
    }

    public string? NewThumbnailSourcePath
    {
        get => _newThumbnailSourcePath;
        private set => SetProperty(ref _newThumbnailSourcePath, value);
    }

    public BitmapImage? ThumbnailPreview
    {
        get => _thumbnailPreview;
        private set
        {
            if (SetProperty(ref _thumbnailPreview, value))
            {
                OnPropertyChanged(nameof(HasThumbnailPreview));
            }
        }
    }

    public bool HasThumbnailPreview => ThumbnailPreview is not null;

    // ========================================================================
    // Validation error properties (FR-014)
    // ========================================================================

    public string NameError
    {
        get => _nameError;
        private set => SetProperty(ref _nameError, value);
    }

    public string VolumeError
    {
        get => _volumeError;
        private set => SetProperty(ref _volumeError, value);
    }

    public string DimensionError
    {
        get => _dimensionError;
        private set => SetProperty(ref _dimensionError, value);
    }

    public string TypeError
    {
        get => _typeError;
        private set => SetProperty(ref _typeError, value);
    }

    public string DateError
    {
        get => _dateError;
        private set => SetProperty(ref _dateError, value);
    }

    public string DuplicateNameWarning
    {
        get => _duplicateNameWarning;
        private set => SetProperty(ref _duplicateNameWarning, value);
    }

    public bool ShowDiscardConfirmation
    {
        get => _showDiscardConfirmation;
        set => SetProperty(ref _showDiscardConfirmation, value);
    }

    /// <summary>
    /// True when the user has entered data in the creation form.
    /// </summary>
    public bool HasUnsavedCreationData =>
        !string.IsNullOrWhiteSpace(NewName) ||
        !double.IsNaN(NewVolume) ||
        !double.IsNaN(NewLength) ||
        !double.IsNaN(NewWidth) ||
        !double.IsNaN(NewHeight) ||
        !string.IsNullOrEmpty(NewNotes) ||
        NewThumbnailSourcePath is not null;

    // ========================================================================
    // Grid methods
    // ========================================================================

    public async Task LoadProfilesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        try
        {
            var aquariums = await _aquariumService.GetAllAsync(cancellationToken).ConfigureAwait(false);

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

    // ========================================================================
    // Creation form methods (FR-008, FR-014, FR-034, FR-035)
    // ========================================================================

    /// <summary>
    /// Validates all creation form fields. Returns true if valid.
    /// Sets error properties for inline indicators (FR-014).
    /// </summary>
    public bool ValidateCreationForm()
    {
        var isValid = true;

        // Name: required, max 100 chars (VR-001)
        if (string.IsNullOrWhiteSpace(NewName))
        {
            NameError = "Name is required";
            isValid = false;
        }
        else if (NewName.Trim().Length > 100)
        {
            NameError = "Name must be 100 characters or fewer";
            isValid = false;
        }
        else
        {
            NameError = string.Empty;
        }

        // Volume: required, > 0 (VR-002)
        if (double.IsNaN(NewVolume) || NewVolume <= 0)
        {
            VolumeError = "Volume must be a positive number";
            isValid = false;
        }
        else
        {
            VolumeError = string.Empty;
        }

        // Dimensions: all required, > 0 (VR-003, VR-004, VR-005)
        if (double.IsNaN(NewLength) || NewLength <= 0 ||
            double.IsNaN(NewWidth) || NewWidth <= 0 ||
            double.IsNaN(NewHeight) || NewHeight <= 0)
        {
            DimensionError = "All dimensions must be positive numbers";
            isValid = false;
        }
        else
        {
            DimensionError = string.Empty;
        }

        // Aquarium type: required (VR-006)
        if (NewAquariumTypeIndex < 0 || NewAquariumTypeIndex >= AquariumTypes.Count)
        {
            TypeError = "Please select an aquarium type";
            isValid = false;
        }
        else
        {
            TypeError = string.Empty;
        }

        // Setup date: required (VR-007)
        if (NewSetupDate is null)
        {
            DateError = "Setup date is required";
            isValid = false;
        }
        else
        {
            DateError = string.Empty;
        }

        return isValid;
    }

    /// <summary>
    /// Creates and saves a new aquarium profile, then refreshes the grid.
    /// </summary>
    public async Task SaveNewProfileAsync(CancellationToken cancellationToken = default)
    {
        var setupDate = NewSetupDate!.Value;

        var aquarium = new Aquarium
        {
            Id = Guid.NewGuid(),
            Name = NewName.Trim(),
            Volume = NewVolume,
            VolumeUnit = IsVolumeLiters ? VolumeUnit.Liters : VolumeUnit.Gallons,
            Length = NewLength,
            Width = NewWidth,
            Height = NewHeight,
            DimensionUnit = IsDimensionCentimeters ? DimensionUnit.Centimeters : DimensionUnit.Inches,
            AquariumType = AquariumTypes[NewAquariumTypeIndex],
            SetupDate = new DateTimeOffset(setupDate.Date, TimeSpan.Zero),
            Description = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes.Trim(),
            Status = AquariumStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (NewThumbnailSourcePath is not null)
        {
            aquarium.ThumbnailPath = await _aquariumService
                .SaveThumbnailAsync(aquarium.Id, NewThumbnailSourcePath, cancellationToken)
                .ConfigureAwait(false);
        }

        await _aquariumService.SaveAsync(aquarium, cancellationToken).ConfigureAwait(false);
        await LoadProfilesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the thumbnail preview from a picked file path.
    /// </summary>
    public void SetThumbnailPreview(string sourceFilePath)
    {
        NewThumbnailSourcePath = sourceFilePath;
        ThumbnailPreview = new BitmapImage(new Uri(sourceFilePath));
    }

    /// <summary>
    /// Clears the thumbnail selection.
    /// </summary>
    public void ClearThumbnailPreview()
    {
        NewThumbnailSourcePath = null;
        ThumbnailPreview = null;
    }

    /// <summary>
    /// Resets all creation form fields to defaults (FR-008).
    /// </summary>
    public void ResetCreationForm()
    {
        NewName = string.Empty;
        NewVolume = double.NaN;
        IsVolumeLiters = true;
        NewLength = double.NaN;
        NewWidth = double.NaN;
        NewHeight = double.NaN;
        IsDimensionCentimeters = true;
        NewAquariumTypeIndex = -1;
        NewSetupDate = DateTimeOffset.Now;
        NewNotes = string.Empty;
        ClearThumbnailPreview();

        // Clear validation state
        NameError = string.Empty;
        VolumeError = string.Empty;
        DimensionError = string.Empty;
        TypeError = string.Empty;
        DateError = string.Empty;
        DuplicateNameWarning = string.Empty;
        ShowDiscardConfirmation = false;
    }

    // ========================================================================
    // Private helpers
    // ========================================================================

    private void CheckDuplicateName()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            DuplicateNameWarning = string.Empty;
            return;
        }

        var trimmed = NewName.Trim();
        var isDuplicate = Profiles.Any(p =>
            string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));

        DuplicateNameWarning = isDuplicate
            ? "A profile with this name already exists"
            : string.Empty;
    }

    private void OnCreateProfile()
    {
        // Dialog display handled by code-behind.
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
