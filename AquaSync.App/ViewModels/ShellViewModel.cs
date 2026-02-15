namespace AquaSync.App.ViewModels;

/// <summary>
/// ViewModel for the ShellPage. Tracks navigation state.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private bool _isBackEnabled;

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }
}
