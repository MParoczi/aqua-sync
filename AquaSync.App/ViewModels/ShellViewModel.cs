using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using CommunityToolkit.Mvvm.Input;

namespace AquaSync.App.ViewModels;

/// <summary>
///     ViewModel for the ShellPage. Tracks the active aquarium context,
///     exposes header properties, and handles back navigation to the selector.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly IAquariumContext _aquariumContext;
    private readonly IAquariumService _aquariumService;
    private string _aquariumName = string.Empty;
    private string _aquariumTypeDisplay = string.Empty;

    private bool _isBackEnabled;
    private bool _isReadOnly;

    public ShellViewModel(IAquariumService aquariumService, IAquariumContext aquariumContext)
    {
        _aquariumService = aquariumService;
        _aquariumContext = aquariumContext;

        GoBackCommand = new RelayCommand(OnGoBack);
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    /// <summary>
    ///     The selected aquarium's name, displayed in the shell header (FR-023).
    /// </summary>
    public string AquariumName
    {
        get => _aquariumName;
        private set => SetProperty(ref _aquariumName, value);
    }

    /// <summary>
    ///     The selected aquarium's type as display text (FR-023).
    /// </summary>
    public string AquariumTypeDisplay
    {
        get => _aquariumTypeDisplay;
        private set => SetProperty(ref _aquariumTypeDisplay, value);
    }

    /// <summary>
    ///     True when the current aquarium is archived (read-only mode) (FR-030).
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set => SetProperty(ref _isReadOnly, value);
    }

    /// <summary>
    ///     Command to navigate back to the selector grid. Clears aquarium context (FR-026).
    ///     Actual RootFrame navigation is handled by ShellPage code-behind.
    /// </summary>
    public IRelayCommand GoBackCommand { get; }

    /// <summary>
    ///     Loads the aquarium by ID and sets the context. Called from ShellPage.OnNavigatedTo.
    /// </summary>
    public async Task InitializeAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var aquarium = await _aquariumService.GetByIdAsync(aquariumId, cancellationToken).ConfigureAwait(false);

        if (aquarium is null) return;

        _aquariumContext.SetCurrentAquarium(aquarium);

        AquariumName = aquarium.Name;
        AquariumTypeDisplay = aquarium.AquariumType.ToString();
        IsReadOnly = _aquariumContext.IsReadOnly;
    }

    /// <summary>
    ///     Restores the current archived aquarium to active status (FR-031).
    ///     Updates the context and hides the read-only banner.
    /// </summary>
    public async Task RestoreCurrentAquariumAsync(CancellationToken cancellationToken = default)
    {
        var aquarium = _aquariumContext.CurrentAquarium;
        if (aquarium is null) return;

        aquarium.Status = AquariumStatus.Active;
        await _aquariumService.SaveAsync(aquarium, cancellationToken).ConfigureAwait(false);

        _aquariumContext.SetCurrentAquarium(aquarium);
        IsReadOnly = _aquariumContext.IsReadOnly;
    }

    private void OnGoBack()
    {
        _aquariumContext.Clear();
    }
}
