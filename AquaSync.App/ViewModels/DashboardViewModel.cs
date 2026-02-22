using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.Chihiros.Exceptions;
using CommunityToolkit.Mvvm.Input;

namespace AquaSync.App.ViewModels;

public sealed class LampCardViewModel : ViewModelBase
{
    private readonly LampConfiguration _lamp;
    private readonly ILampService _lampService;
    private LampMode _currentMode;
    private bool _isConnecting;
    private string? _errorMessage;

    public LampCardViewModel(LampConfiguration lamp, ILampService lampService)
    {
        _lamp = lamp;
        _lampService = lampService;
        _currentMode = lamp.Mode;
        DisplayName = lamp.DeviceName;
        ModelName = lamp.ModelName;
        LampIdString = lamp.Id.ToString("N");
        SetModeCommand = new AsyncRelayCommand<string>(SetModeAsync);
    }

    public string DisplayName { get; }
    public string ModelName { get; }
    public string LampIdString { get; }

    public LampMode CurrentMode
    {
        get => _currentMode;
        private set
        {
            SetProperty(ref _currentMode, value);
            OnPropertyChanged(nameof(IsOn));
            OnPropertyChanged(nameof(IsOffMode));
            OnPropertyChanged(nameof(IsManualMode));
            OnPropertyChanged(nameof(IsAutoMode));
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        private set => SetProperty(ref _isConnecting, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsOn => _currentMode is LampMode.Manual or LampMode.Automatic;
    public bool IsOffMode => _currentMode == LampMode.Off;
    public bool IsManualMode => _currentMode == LampMode.Manual;
    public bool IsAutoMode => _currentMode == LampMode.Automatic;

    public AsyncRelayCommand<string> SetModeCommand { get; }

    private async Task SetModeAsync(string? modeName, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<LampMode>(modeName, out var newMode) || newMode == _currentMode)
            return;

        IsConnecting = true;
        ErrorMessage = null;
        try
        {
            await _lampService.SaveModeAsync(_lamp.Id, newMode, cancellationToken);

            var device = await _lampService.ConnectAsync(_lamp, cancellationToken);
            if (device is not null)
            {
                try
                {
                    switch (newMode)
                    {
                        case LampMode.Off:
                            await device.TurnOffAsync(cancellationToken);
                            break;
                        case LampMode.Manual:
                            if (_lamp.ManualBrightness.Count > 0)
                            {
                                var profile = _lampService.GetProfileForModel(_lamp.ModelName);
                                if (profile is not null)
                                {
                                    foreach (var mapping in profile.Channels)
                                    {
                                        if (_lamp.ManualBrightness.TryGetValue(mapping.Channel.ToString(), out var brightness))
                                            await device.SetBrightnessAsync(mapping.Channel, brightness, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await device.TurnOnAsync(cancellationToken);
                            }
                            break;
                        case LampMode.Automatic:
                            await device.EnableAutoModeAsync(cancellationToken);
                            break;
                    }
                }
                finally
                {
                    await _lampService.DisconnectAsync(_lamp.BluetoothAddress);
                }
            }

            CurrentMode = newMode;
            _lamp.Mode = newMode;
        }
        catch (ChihirosException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsConnecting = false;
        }
    }
}

public sealed class DashboardViewModel : ViewModelBase, INavigationAware
{
    private readonly ILampService _lampService;
    private readonly IAquariumContext _aquariumContext;

    public DashboardViewModel(ILampService lampService, IAquariumContext aquariumContext)
    {
        _lampService = lampService;
        _aquariumContext = aquariumContext;
        LampCards = [];
        LampCards.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasLamps));
    }

    public ObservableCollection<LampCardViewModel> LampCards { get; }

    public bool HasLamps => LampCards.Count > 0;

    public void OnNavigatedTo(object parameter)
    {
        if (_aquariumContext.CurrentAquarium is not { } aquarium) return;
        _ = LoadLampCardsAsync(aquarium.Id);
    }

    public void OnNavigatedFrom() { }

    private async Task LoadLampCardsAsync(Guid aquariumId)
    {
        var lamps = await _lampService.GetLampsForAquariumAsync(aquariumId);
        LampCards.Clear();
        foreach (var lamp in lamps)
            LampCards.Add(new LampCardViewModel(lamp, _lampService));
    }
}
