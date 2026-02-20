using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AquaSync.App.ViewModels;

/// <summary>
///     ViewModel for the aquarium selector launch screen.
///     Loads profiles, exposes sorted collection, and provides profile creation logic.
/// </summary>
public sealed class AquariumSelectorViewModel : ViewModelBase
{
    private readonly IAquariumService _aquariumService;
    private string _dateError = string.Empty;
    private string _dimensionError = string.Empty;

    // --- Duplicate name warning ---
    private string _duplicateNameWarning = string.Empty;
    private string _entryBrand = string.Empty;
    private DateTimeOffset? _entryDateAdded;
    private double _entryLayerDepth = double.NaN;
    private string _entryNotes = string.Empty;
    private string _entryProductName = string.Empty;
    private int _entryTypeIndex = -1;

    // --- Error notification ---
    private string _errorMessage = string.Empty;
    private bool _hasProfiles;

    // --- Substrate entry form ---
    private bool _isAddingSubstrate;
    private bool _isDimensionCentimeters = true;
    private bool _isErrorOpen;

    // --- Grid state ---
    private bool _isLoading;
    private bool _isNotificationOpen;

    // --- Saving indicator (FR-040) ---
    private bool _isSaving;
    private bool _isVolumeLiters = true;

    // --- Validation errors ---
    private string _nameError = string.Empty;
    private int _newAquariumTypeIndex = -1;
    private double _newHeight = double.NaN;
    private double _newLength = double.NaN;

    // --- Creation form fields ---
    private string _newName = string.Empty;
    private string _newNotes = string.Empty;
    private DateTimeOffset? _newSetupDate;
    private string? _newThumbnailSourcePath;
    private double _newVolume = double.NaN;
    private double _newWidth = double.NaN;

    // --- Notification (FR-039) ---
    private string _notificationMessage = string.Empty;

    // --- Discard confirmation ---
    private bool _showDiscardConfirmation;
    private string _substrateEntryError = string.Empty;
    private BitmapImage? _thumbnailPreview;
    private string _typeError = string.Empty;
    private string _volumeError = string.Empty;

    public AquariumSelectorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;

        CreateProfileCommand = new RelayCommand(OnCreateProfile);
        ArchiveProfileCommand = new RelayCommand<Aquarium>(OnArchiveProfile);
        RestoreProfileCommand = new RelayCommand<Aquarium>(OnRestoreProfile);
        DeleteProfileCommand = new RelayCommand<Aquarium>(OnDeleteProfile);

        ShowSubstrateFormCommand = new RelayCommand(OnShowSubstrateForm);
        SaveSubstrateEntryCommand = new RelayCommand(OnSaveSubstrateEntry);
        CancelSubstrateEntryCommand = new RelayCommand(OnCancelSubstrateEntry);
        RemoveSubstrateCommand = new RelayCommand<SubstrateEntry>(OnRemoveSubstrate);
        MoveSubstrateUpCommand = new RelayCommand<SubstrateEntry>(OnMoveSubstrateUp);
        MoveSubstrateDownCommand = new RelayCommand<SubstrateEntry>(OnMoveSubstrateDown);

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

    // Substrate entry commands (FR-020, FR-021)
    public IRelayCommand ShowSubstrateFormCommand { get; }
    public IRelayCommand SaveSubstrateEntryCommand { get; }
    public IRelayCommand CancelSubstrateEntryCommand { get; }
    public IRelayCommand<SubstrateEntry> RemoveSubstrateCommand { get; }
    public IRelayCommand<SubstrateEntry> MoveSubstrateUpCommand { get; }
    public IRelayCommand<SubstrateEntry> MoveSubstrateDownCommand { get; }

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
            if (SetProperty(ref _newName, value)) CheckDuplicateName();
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
        set
        {
            if (SetProperty(ref _isDimensionCentimeters, value)) OnPropertyChanged(nameof(DimensionUnitLabel));
        }
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
            if (SetProperty(ref _thumbnailPreview, value)) OnPropertyChanged(nameof(HasThumbnailPreview));
        }
    }

    public bool HasThumbnailPreview => ThumbnailPreview is not null;

    // ========================================================================
    // Substrate entry form properties (FR-018, FR-019, FR-020)
    // ========================================================================

    public IReadOnlyList<SubstrateType> SubstrateTypes { get; } =
        Enum.GetValues<SubstrateType>();

    public ObservableCollection<SubstrateEntry> NewSubstrates { get; } = [];

    /// <summary>
    ///     Dimension unit label derived from the creation form toggle (FR-019).
    /// </summary>
    public string DimensionUnitLabel => IsDimensionCentimeters ? "cm" : "in";

    public bool IsAddingSubstrate
    {
        get => _isAddingSubstrate;
        private set => SetProperty(ref _isAddingSubstrate, value);
    }

    public string EntryBrand
    {
        get => _entryBrand;
        set => SetProperty(ref _entryBrand, value);
    }

    public string EntryProductName
    {
        get => _entryProductName;
        set => SetProperty(ref _entryProductName, value);
    }

    public int EntryTypeIndex
    {
        get => _entryTypeIndex;
        set => SetProperty(ref _entryTypeIndex, value);
    }

    public double EntryLayerDepth
    {
        get => _entryLayerDepth;
        set => SetProperty(ref _entryLayerDepth, value);
    }

    public DateTimeOffset? EntryDateAdded
    {
        get => _entryDateAdded;
        set => SetProperty(ref _entryDateAdded, value);
    }

    public string EntryNotes
    {
        get => _entryNotes;
        set => SetProperty(ref _entryNotes, value);
    }

    public string SubstrateEntryError
    {
        get => _substrateEntryError;
        private set => SetProperty(ref _substrateEntryError, value);
    }

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

    // ========================================================================
    // Notification properties (FR-039)
    // ========================================================================

    public string NotificationMessage
    {
        get => _notificationMessage;
        private set => SetProperty(ref _notificationMessage, value);
    }

    public bool IsNotificationOpen
    {
        get => _isNotificationOpen;
        private set => SetProperty(ref _isNotificationOpen, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsErrorOpen
    {
        get => _isErrorOpen;
        private set => SetProperty(ref _isErrorOpen, value);
    }

    // ========================================================================
    // Saving indicator (FR-040)
    // ========================================================================

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    /// <summary>
    ///     True when the user has entered data in the creation form.
    /// </summary>
    public bool HasUnsavedCreationData =>
        !string.IsNullOrWhiteSpace(NewName) ||
        !double.IsNaN(NewVolume) ||
        !double.IsNaN(NewLength) ||
        !double.IsNaN(NewWidth) ||
        !double.IsNaN(NewHeight) ||
        !string.IsNullOrEmpty(NewNotes) ||
        NewThumbnailSourcePath is not null ||
        NewSubstrates.Count > 0;

    /// <summary>
    ///     Shows an error notification that auto-dismisses after 5 seconds.
    /// </summary>
    public void ShowError(string message)
    {
        ErrorMessage = message;
        IsErrorOpen = true;
        _ = AutoDismissErrorAsync();
    }

    private async Task AutoDismissErrorAsync()
    {
        await Task.Delay(5000);
        IsErrorOpen = false;
    }

    /// <summary>
    ///     Shows a success notification that auto-dismisses after 3 seconds (FR-039).
    /// </summary>
    public void ShowNotification(string message)
    {
        NotificationMessage = message;
        IsNotificationOpen = true;
        _ = AutoDismissNotificationAsync();
    }

    private async Task AutoDismissNotificationAsync()
    {
        await Task.Delay(3000);
        IsNotificationOpen = false;
    }

    // ========================================================================
    // Grid methods
    // ========================================================================

    public async Task LoadProfilesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        try
        {
            var aquariums = await _aquariumService.GetAllAsync(cancellationToken);

            var sorted = aquariums
                .OrderBy(a => a.Status == AquariumStatus.Archived ? 1 : 0)
                .ThenByDescending(a => a.CreatedAt);

            Profiles.Clear();

            foreach (var aquarium in sorted) Profiles.Add(aquarium);
        }
        catch (IOException)
        {
            ShowError("Could not load profiles. Please check disk permissions.");
        }
        finally
        {
            IsLoading = false;
        }

        HasProfiles = Profiles.Count > 0;
    }

    // ========================================================================
    // Archive / Restore methods (FR-028, FR-031)
    // ========================================================================

    /// <summary>
    ///     Sets aquarium status to Archived and refreshes the grid (FR-028).
    ///     Called by code-behind after confirmation dialog.
    /// </summary>
    public async Task ArchiveProfileAsync(Aquarium aquarium, CancellationToken cancellationToken = default)
    {
        var previousStatus = aquarium.Status;

        try
        {
            aquarium.Status = AquariumStatus.Archived;
            await _aquariumService.SaveAsync(aquarium, cancellationToken);
            await LoadProfilesAsync(cancellationToken);
        }
        catch (IOException)
        {
            aquarium.Status = previousStatus;
            ShowError("Could not archive profile. Please check disk space and permissions.");
        }
    }

    /// <summary>
    ///     Sets aquarium status to Active and refreshes the grid (FR-031).
    /// </summary>
    public async Task RestoreProfileAsync(Aquarium aquarium, CancellationToken cancellationToken = default)
    {
        var previousStatus = aquarium.Status;

        try
        {
            aquarium.Status = AquariumStatus.Active;
            await _aquariumService.SaveAsync(aquarium, cancellationToken);
            await LoadProfilesAsync(cancellationToken);
        }
        catch (IOException)
        {
            aquarium.Status = previousStatus;
            ShowError("Could not restore profile. Please check disk space and permissions.");
        }
    }

    /// <summary>
    ///     Permanently deletes an aquarium profile and its associated data (FR-032, FR-033).
    ///     Called by code-behind after confirmation dialog.
    /// </summary>
    public async Task DeleteProfileAsync(Aquarium aquarium, CancellationToken cancellationToken = default)
    {
        try
        {
            await _aquariumService.DeleteAsync(aquarium.Id, cancellationToken);
            Profiles.Remove(aquarium);
            HasProfiles = Profiles.Count > 0;
        }
        catch (IOException)
        {
            ShowError("Could not delete profile. Please check disk permissions.");
        }
    }

    // ========================================================================
    // Creation form methods (FR-008, FR-014, FR-034, FR-035)
    // ========================================================================

    /// <summary>
    ///     Validates all creation form fields. Returns true if valid.
    ///     Sets error properties for inline indicators (FR-014).
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
    ///     Creates and saves a new aquarium profile, then refreshes the grid.
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
            Substrates = NewSubstrates.Select((s, i) =>
            {
                s.DisplayOrder = i;
                return s;
            }).ToList()
        };

        try
        {
            if (NewThumbnailSourcePath is not null)
                aquarium.ThumbnailPath = await _aquariumService
                        .SaveThumbnailAsync(aquarium.Id, NewThumbnailSourcePath, cancellationToken)
                    ;

            await _aquariumService.SaveAsync(aquarium, cancellationToken);
            await LoadProfilesAsync(cancellationToken);
        }
        catch (IOException)
        {
            ShowError("Could not save profile. Please check disk space and permissions.");
        }
    }

    /// <summary>
    ///     Sets the thumbnail preview from a picked file path.
    /// </summary>
    public void SetThumbnailPreview(string sourceFilePath)
    {
        NewThumbnailSourcePath = sourceFilePath;
        ThumbnailPreview = new BitmapImage(new Uri(sourceFilePath));
    }

    /// <summary>
    ///     Clears the thumbnail selection.
    /// </summary>
    public void ClearThumbnailPreview()
    {
        NewThumbnailSourcePath = null;
        ThumbnailPreview = null;
    }

    /// <summary>
    ///     Resets all creation form fields to defaults (FR-008).
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
        NewSubstrates.Clear();
        ResetSubstrateEntryForm();

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
        // Confirmation dialog handled by code-behind, which calls ArchiveProfileAsync.
    }

    private void OnRestoreProfile(Aquarium? aquarium)
    {
        // Code-behind calls RestoreProfileAsync directly.
    }

    private void OnDeleteProfile(Aquarium? aquarium)
    {
        // Implemented in US6 (Phase 8).
    }

    // --- Substrate entry form helpers (FR-018, FR-020, FR-021) ---

    private void OnShowSubstrateForm()
    {
        ResetSubstrateEntryForm();
        IsAddingSubstrate = true;
    }

    private void OnSaveSubstrateEntry()
    {
        if (!ValidateSubstrateEntry()) return;

        var entry = new SubstrateEntry
        {
            Id = Guid.NewGuid(),
            Brand = EntryBrand.Trim(),
            ProductName = EntryProductName.Trim(),
            Type = SubstrateTypes[EntryTypeIndex],
            LayerDepth = EntryLayerDepth,
            DateAdded = new DateTimeOffset(EntryDateAdded!.Value.Date, TimeSpan.Zero),
            Notes = string.IsNullOrWhiteSpace(EntryNotes) ? null : EntryNotes.Trim(),
            DisplayOrder = NewSubstrates.Count
        };

        NewSubstrates.Add(entry);
        ResetSubstrateEntryForm();
        IsAddingSubstrate = false;
    }

    private void OnCancelSubstrateEntry()
    {
        ResetSubstrateEntryForm();
        IsAddingSubstrate = false;
    }

    private void OnRemoveSubstrate(SubstrateEntry? entry)
    {
        if (entry is not null) NewSubstrates.Remove(entry);
    }

    private void OnMoveSubstrateUp(SubstrateEntry? entry)
    {
        if (entry is null) return;
        var index = NewSubstrates.IndexOf(entry);
        if (index > 0) NewSubstrates.Move(index, index - 1);
    }

    private void OnMoveSubstrateDown(SubstrateEntry? entry)
    {
        if (entry is null) return;
        var index = NewSubstrates.IndexOf(entry);
        if (index >= 0 && index < NewSubstrates.Count - 1) NewSubstrates.Move(index, index + 1);
    }

    private bool ValidateSubstrateEntry()
    {
        if (string.IsNullOrWhiteSpace(EntryBrand) ||
            string.IsNullOrWhiteSpace(EntryProductName) ||
            EntryTypeIndex < 0 || EntryTypeIndex >= SubstrateTypes.Count ||
            double.IsNaN(EntryLayerDepth) || EntryLayerDepth <= 0 ||
            EntryDateAdded is null)
        {
            SubstrateEntryError = "All fields except notes are required";
            return false;
        }

        SubstrateEntryError = string.Empty;
        return true;
    }

    private void ResetSubstrateEntryForm()
    {
        EntryBrand = string.Empty;
        EntryProductName = string.Empty;
        EntryTypeIndex = -1;
        EntryLayerDepth = double.NaN;
        EntryDateAdded = DateTimeOffset.Now;
        EntryNotes = string.Empty;
        SubstrateEntryError = string.Empty;
    }
}
