using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AquaSync.App.ViewModels;

/// <summary>
///     ViewModel for the Settings page. Provides dual-scope settings:
///     global section (placeholder) and aquarium-scoped section for profile
///     editing and substrate management (FR-016, FR-017, FR-027).
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IAquariumContext _aquariumContext;
    private readonly IAquariumService _aquariumService;
    private readonly ISettingsService _settingsService;
    private string _aquariumTypeDisplay = string.Empty;
    private string _dimensionsDisplay = string.Empty;
    private string _dimensionUnitLabel = string.Empty;

    // --- Editable fields ---
    private string _editName = string.Empty;
    private string _editNotes = string.Empty;
    private BitmapImage? _editThumbnailPreview;
    private string? _editThumbnailSourcePath;
    private string _entryBrand = string.Empty;
    private DateTimeOffset? _entryDateAdded;
    private double _entryLayerDepth = double.NaN;
    private string _entryNotes = string.Empty;
    private string _entryProductName = string.Empty;
    private int _entryTypeIndex = -1;
    private string _errorMessage = string.Empty;

    // --- State ---
    private bool _hasAquarium;

    // --- Global settings ---
    private VolumeUnit _selectedVolumeUnit;
    private DimensionUnit _selectedDimensionUnit;

    // --- Substrate entry form ---
    private bool _isAddingSubstrate;
    private bool _isErrorOpen;
    private bool _isNotificationOpen;
    private bool _isReadOnly;
    private bool _isSaving;
    private string _notificationMessage = string.Empty;
    private string _setupDateDisplay = string.Empty;
    private string _substrateEntryError = string.Empty;

    // --- Locked field displays ---
    private string _volumeDisplay = string.Empty;

    public SettingsViewModel(
        IAquariumService aquariumService,
        IAquariumContext aquariumContext,
        ISettingsService settingsService)
    {
        _aquariumService = aquariumService;
        _aquariumContext = aquariumContext;
        _settingsService = settingsService;

        SaveProfileCommand = new RelayCommand(OnSaveProfile);

        ShowSubstrateFormCommand = new RelayCommand(OnShowSubstrateForm);
        SaveSubstrateEntryCommand = new RelayCommand(OnSaveSubstrateEntry);
        CancelSubstrateEntryCommand = new RelayCommand(OnCancelSubstrateEntry);
        RemoveSubstrateCommand = new RelayCommand<SubstrateEntry>(OnRemoveSubstrate);
        MoveSubstrateUpCommand = new RelayCommand<SubstrateEntry>(OnMoveSubstrateUp);
        MoveSubstrateDownCommand = new RelayCommand<SubstrateEntry>(OnMoveSubstrateDown);
    }

    // ========================================================================
    // Commands
    // ========================================================================

    public IRelayCommand SaveProfileCommand { get; }

    public IRelayCommand ShowSubstrateFormCommand { get; }
    public IRelayCommand SaveSubstrateEntryCommand { get; }
    public IRelayCommand CancelSubstrateEntryCommand { get; }
    public IRelayCommand<SubstrateEntry> RemoveSubstrateCommand { get; }
    public IRelayCommand<SubstrateEntry> MoveSubstrateUpCommand { get; }
    public IRelayCommand<SubstrateEntry> MoveSubstrateDownCommand { get; }

    // ========================================================================
    // State properties
    // ========================================================================

    public bool HasAquarium
    {
        get => _hasAquarium;
        private set => SetProperty(ref _hasAquarium, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set => SetProperty(ref _isReadOnly, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

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
    // Global settings properties (US1)
    // ========================================================================

    public VolumeUnit SelectedVolumeUnit
    {
        get => _selectedVolumeUnit;
        set
        {
            if (SetProperty(ref _selectedVolumeUnit, value))
            {
                _settingsService.Settings.DefaultVolumeUnit = value;
                _ = _settingsService.SaveAsync();
            }
        }
    }

    public DimensionUnit SelectedDimensionUnit
    {
        get => _selectedDimensionUnit;
        set
        {
            if (SetProperty(ref _selectedDimensionUnit, value))
            {
                _settingsService.Settings.DefaultDimensionUnit = value;
                _ = _settingsService.SaveAsync();
            }
        }
    }

    // ========================================================================
    // Editable fields (FR-016)
    // ========================================================================

    public string EditName
    {
        get => _editName;
        set => SetProperty(ref _editName, value);
    }

    public string EditNotes
    {
        get => _editNotes;
        set => SetProperty(ref _editNotes, value);
    }

    public string? EditThumbnailSourcePath
    {
        get => _editThumbnailSourcePath;
        private set => SetProperty(ref _editThumbnailSourcePath, value);
    }

    public BitmapImage? EditThumbnailPreview
    {
        get => _editThumbnailPreview;
        private set
        {
            if (SetProperty(ref _editThumbnailPreview, value)) OnPropertyChanged(nameof(HasEditThumbnailPreview));
        }
    }

    public bool HasEditThumbnailPreview => EditThumbnailPreview is not null;

    // ========================================================================
    // Locked field displays (FR-017)
    // ========================================================================

    public string VolumeDisplay
    {
        get => _volumeDisplay;
        private set => SetProperty(ref _volumeDisplay, value);
    }

    public string DimensionsDisplay
    {
        get => _dimensionsDisplay;
        private set => SetProperty(ref _dimensionsDisplay, value);
    }

    public string AquariumTypeDisplay
    {
        get => _aquariumTypeDisplay;
        private set => SetProperty(ref _aquariumTypeDisplay, value);
    }

    public string SetupDateDisplay
    {
        get => _setupDateDisplay;
        private set => SetProperty(ref _setupDateDisplay, value);
    }

    public string DimensionUnitLabel
    {
        get => _dimensionUnitLabel;
        private set => SetProperty(ref _dimensionUnitLabel, value);
    }

    // ========================================================================
    // Substrate properties (FR-018, FR-019, FR-020)
    // ========================================================================

    public IReadOnlyList<SubstrateType> SubstrateTypes { get; } =
        Enum.GetValues<SubstrateType>();

    public ObservableCollection<SubstrateEntry> Substrates { get; } = [];

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

    private void ShowError(string message)
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

    // ========================================================================
    // Initialization
    // ========================================================================

    /// <summary>
    ///     Loads the current aquarium data from context. Called on page Loaded.
    /// </summary>
    public void LoadFromContext()
    {
        LoadGlobalSettings();

        var aquarium = _aquariumContext.CurrentAquarium;
        HasAquarium = aquarium is not null;
        IsReadOnly = _aquariumContext.IsReadOnly;

        if (aquarium is null) return;

        // Editable fields
        EditName = aquarium.Name;
        EditNotes = aquarium.Description ?? string.Empty;
        EditThumbnailSourcePath = null;
        LoadThumbnailPreview(aquarium);

        // Locked fields
        VolumeDisplay = aquarium.VolumeDisplay;
        var dimUnit = aquarium.DimensionUnit == DimensionUnit.Centimeters ? "cm" : "in";
        DimensionsDisplay = $"{aquarium.Length:0.#} × {aquarium.Width:0.#} × {aquarium.Height:0.#} {dimUnit}";
        AquariumTypeDisplay = aquarium.AquariumType.ToString();
        SetupDateDisplay = aquarium.SetupDateDisplay;
        DimensionUnitLabel = dimUnit;

        // Substrates
        Substrates.Clear();
        foreach (var entry in aquarium.Substrates.OrderBy(s => s.DisplayOrder)) Substrates.Add(entry);

        ResetSubstrateEntryForm();
    }

    /// <summary>
    ///     Sets the thumbnail preview from a picked file path.
    /// </summary>
    public void SetThumbnailPreview(string sourceFilePath)
    {
        EditThumbnailSourcePath = sourceFilePath;
        EditThumbnailPreview = new BitmapImage(new Uri(sourceFilePath));
    }

    /// <summary>
    ///     Clears the thumbnail selection (reverts to current saved thumbnail on next load).
    /// </summary>
    public void ClearThumbnailPreview()
    {
        EditThumbnailSourcePath = null;
        EditThumbnailPreview = null;
    }

    // ========================================================================
    // Global settings helpers
    // ========================================================================

    private void LoadGlobalSettings()
    {
        _selectedVolumeUnit = _settingsService.Settings.DefaultVolumeUnit;
        OnPropertyChanged(nameof(SelectedVolumeUnit));

        _selectedDimensionUnit = _settingsService.Settings.DefaultDimensionUnit;
        OnPropertyChanged(nameof(SelectedDimensionUnit));
    }

    // ========================================================================
    // Private helpers
    // ========================================================================

    private void LoadThumbnailPreview(Aquarium aquarium)
    {
        if (aquarium.ThumbnailPath is not null)
        {
            var rootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AquaSync");
            var fullPath = Path.Combine(rootPath, aquarium.ThumbnailPath);

            if (File.Exists(fullPath))
            {
                EditThumbnailPreview = new BitmapImage(new Uri(fullPath));
                return;
            }
        }

        EditThumbnailPreview = null;
    }

    private async void OnSaveProfile()
    {
        var aquarium = _aquariumContext.CurrentAquarium;
        if (aquarium is null || IsReadOnly) return;

        IsSaving = true;

        try
        {
            // Update editable fields (FR-016)
            aquarium.Name = EditName.Trim();
            aquarium.Description = string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim();

            // Update thumbnail if changed
            if (EditThumbnailSourcePath is not null)
            {
                aquarium.ThumbnailPath = await _aquariumService
                        .SaveThumbnailAsync(aquarium.Id, EditThumbnailSourcePath)
                    ;
                EditThumbnailSourcePath = null;
            }

            // Update substrates
            aquarium.Substrates = Substrates.Select((s, i) =>
            {
                s.DisplayOrder = i;
                return s;
            }).ToList();

            await _aquariumService.SaveAsync(aquarium);
            ShowNotification("Profile saved");
        }
        catch (IOException)
        {
            ShowError("Could not save profile. Please check disk space and permissions.");
        }
        finally
        {
            IsSaving = false;
        }
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
            DisplayOrder = Substrates.Count
        };

        Substrates.Add(entry);
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
        if (entry is not null) Substrates.Remove(entry);
    }

    private void OnMoveSubstrateUp(SubstrateEntry? entry)
    {
        if (entry is null) return;
        var index = Substrates.IndexOf(entry);
        if (index > 0) Substrates.Move(index, index - 1);
    }

    private void OnMoveSubstrateDown(SubstrateEntry? entry)
    {
        if (entry is null) return;
        var index = Substrates.IndexOf(entry);
        if (index >= 0 && index < Substrates.Count - 1) Substrates.Move(index, index + 1);
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
        IsAddingSubstrate = false;
    }
}
