using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Exceptions;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;

namespace AquaSync.App.ViewModels;

public sealed class ChannelSlider : ViewModelBase
{
    private double _value;

    public ColorChannel Channel { get; init; }

    public string Label { get; init; } = string.Empty;

    public double Value
    {
        get => _value;
        set
        {
            SetProperty(ref _value, value);
            OnPropertyChanged(nameof(ValueLabel));
        }
    }

    public string ValueLabel => $"{(int)Value}%";
}

public sealed class LampDetailViewModel : ViewModelBase, INavigationAware
{
    private readonly ILampService _lampService;
    private readonly INavigationService _navigationService;
    private readonly IAquariumContext _aquariumContext;
    private readonly DispatcherQueue _dispatcherQueue;

    private LampConfiguration? _lamp;
    private IChihirosDevice? _device;
    private bool _isConnected;
    private bool _isConnecting;
    private bool _isBrightnessApplying;
    private string? _errorMessage;
    private bool _isErrorOpen;

    public LampDetailViewModel(
        ILampService lampService,
        INavigationService navigationService,
        IAquariumContext aquariumContext)
    {
        _lampService = lampService;
        _navigationService = navigationService;
        _aquariumContext = aquariumContext;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Channels = [];
        GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
        ApplyBrightnessCommand = new AsyncRelayCommand<ChannelSlider>(ApplyBrightnessAsync);
    }

    public LampConfiguration? Lamp
    {
        get => _lamp;
        private set
        {
            SetProperty(ref _lamp, value);
            OnPropertyChanged(nameof(IsUnmanaged));
            OnPropertyChanged(nameof(IsManaged));
        }
    }

    public IChihirosDevice? Device
    {
        get => _device;
        private set => SetProperty(ref _device, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            SetProperty(ref _isConnected, value);
            OnPropertyChanged(nameof(IsInteractionEnabled));
            OnPropertyChanged(nameof(IsDisconnected));
            OnPropertyChanged(nameof(ConnectionStateText));
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        private set
        {
            SetProperty(ref _isConnecting, value);
            OnPropertyChanged(nameof(IsInteractionEnabled));
            OnPropertyChanged(nameof(IsDisconnected));
        }
    }

    public bool IsUnmanaged => string.IsNullOrEmpty(Lamp?.ModelName);

    public bool IsManaged => !IsUnmanaged;

    public bool IsInteractionEnabled => IsConnected && !IsConnecting;

    public bool IsDisconnected => !IsConnected && !IsConnecting;

    public string ConnectionStateText => IsConnected ? "Connected" : "Disconnected";

    public ObservableCollection<ChannelSlider> Channels { get; }

    public bool IsBrightnessApplying
    {
        get => _isBrightnessApplying;
        private set => SetProperty(ref _isBrightnessApplying, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsErrorOpen
    {
        get => _isErrorOpen;
        set => SetProperty(ref _isErrorOpen, value);
    }

    public RelayCommand GoBackCommand { get; }

    public AsyncRelayCommand<ChannelSlider> ApplyBrightnessCommand { get; }

    public async void OnNavigatedTo(object parameter)
    {
        var lampId = (Guid)parameter;
        var aquariumId = _aquariumContext.CurrentAquarium!.Id;
        var lamps = await _lampService.GetLampsForAquariumAsync(aquariumId);
        Lamp = lamps.FirstOrDefault(l => l.Id == lampId);

        if (Lamp is null || IsUnmanaged)
            return;

        IsConnecting = true;
        try
        {
            var device = await _lampService.ConnectAsync(Lamp);
            Device = device;

            if (Device is not null)
            {
                Device.Disconnected += OnDeviceDisconnected;
                PopulateChannels();
                IsConnected = true;
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to connect to the device.";
            IsErrorOpen = true;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    public void OnNavigatedFrom()
    {
        if (_device is not null)
            _device.Disconnected -= OnDeviceDisconnected;

        if (Lamp is not null)
            _ = _lampService.DisconnectAsync(Lamp.BluetoothAddress);

        Device = null;
        IsConnected = false;
    }

    private void PopulateChannels()
    {
        Channels.Clear();

        var profile = _lampService.GetProfileForModel(Lamp!.ModelName);
        if (profile is null)
            return;

        foreach (var mapping in profile.Channels)
        {
            var label = mapping.Channel.ToString();
            var storedValue = Lamp.ManualBrightness.TryGetValue(label, out var b) ? b : (byte)0;
            Channels.Add(new ChannelSlider
            {
                Channel = mapping.Channel,
                Label = label,
                Value = storedValue,
            });
        }
    }

    private async Task ApplyBrightnessAsync(ChannelSlider? slider, CancellationToken cancellationToken)
    {
        if (slider is null || Lamp is null || Device is null)
            return;

        var brightness = (byte)Math.Clamp(slider.Value, 0, 100);
        Lamp.ManualBrightness[slider.Channel.ToString()] = brightness;
        await _lampService.SaveManualBrightnessAsync(Lamp.Id, Lamp.ManualBrightness, cancellationToken);

        IsBrightnessApplying = true;
        try
        {
            await Device.SetBrightnessAsync(slider.Channel, brightness, cancellationToken);
        }
        catch (ChihirosException ex)
        {
            ErrorMessage = ex.Message;
            IsErrorOpen = true;
        }
        finally
        {
            IsBrightnessApplying = false;
        }
    }

    private void OnDeviceDisconnected(object? sender, string reason)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsConnected = false;
            ErrorMessage = "Device is unreachable.";
            IsErrorOpen = true;
        });
    }
}
