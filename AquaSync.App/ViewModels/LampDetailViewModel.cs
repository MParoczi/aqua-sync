using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.Chihiros.Devices;
using AquaSync.Chihiros.Exceptions;
using AquaSync.Chihiros.Scheduling;
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

    // ── Connection state ─────────────────────────────────────────────────────

    private LampConfiguration? _lamp;
    private IChihirosDevice? _device;
    private bool _isConnected;
    private bool _isConnecting;
    private bool _isBrightnessApplying;
    private string? _errorMessage;
    private bool _isErrorOpen;

    // ── Schedule state ───────────────────────────────────────────────────────

    private TimeOnly? _scheduleSunrise;
    private TimeOnly? _scheduleSunset;
    private int _scheduleRampUpMinutes;
    private Weekday _scheduleActiveDays;
    private bool _isSavingSchedule;

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
        ScheduleChannels = [];
        GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
        ApplyBrightnessCommand = new AsyncRelayCommand<ChannelSlider>(ApplyBrightnessAsync);
        SaveScheduleCommand = new AsyncRelayCommand(SaveScheduleAsync);
    }

    // ── Connection properties ────────────────────────────────────────────────

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

    // ── Schedule properties ──────────────────────────────────────────────────

    public TimeOnly? ScheduleSunrise
    {
        get => _scheduleSunrise;
        set
        {
            SetProperty(ref _scheduleSunrise, value);
            OnPropertyChanged(nameof(SunriseTimeSpan));
            OnPropertyChanged(nameof(IsScheduleValid));
            OnPropertyChanged(nameof(ValidationMessage));
        }
    }

    public TimeOnly? ScheduleSunset
    {
        get => _scheduleSunset;
        set
        {
            SetProperty(ref _scheduleSunset, value);
            OnPropertyChanged(nameof(SunsetTimeSpan));
            OnPropertyChanged(nameof(IsScheduleValid));
            OnPropertyChanged(nameof(ValidationMessage));
        }
    }

    /// <summary>
    ///     TimeSpan? wrapper for WinUI3 TimePicker.SelectedTime OneWay binding.
    ///     Changes from the picker go through the SelectedTimeChanged event handler in code-behind
    ///     to avoid a TwoWay DP feedback loop (TimePicker fires change callbacks for same values).
    /// </summary>
    public TimeSpan? SunriseTimeSpan => _scheduleSunrise?.ToTimeSpan();

    /// <summary>
    ///     TimeSpan? wrapper for WinUI3 TimePicker.SelectedTime OneWay binding.
    ///     See <see cref="SunriseTimeSpan"/> for rationale.
    /// </summary>
    public TimeSpan? SunsetTimeSpan => _scheduleSunset?.ToTimeSpan();

    public int ScheduleRampUpMinutes
    {
        get => _scheduleRampUpMinutes;
        set
        {
            SetProperty(ref _scheduleRampUpMinutes, value);
            OnPropertyChanged(nameof(ScheduleRampUpMinutesDouble));
            OnPropertyChanged(nameof(IsScheduleValid));
            OnPropertyChanged(nameof(ValidationMessage));
        }
    }

    /// <summary>double wrapper for WinUI3 NumberBox.Value binding.</summary>
    public double ScheduleRampUpMinutesDouble
    {
        get => _scheduleRampUpMinutes;
        set => ScheduleRampUpMinutes = (int)value;
    }

    // ── Individual weekday bool properties (Mon–Sun) ─────────────────────────

    public bool ScheduleMonday
    {
        get => (_scheduleActiveDays & Weekday.Monday) != 0;
        set { ToggleDay(Weekday.Monday, value); OnPropertyChanged(nameof(ScheduleMonday)); }
    }

    public bool ScheduleTuesday
    {
        get => (_scheduleActiveDays & Weekday.Tuesday) != 0;
        set { ToggleDay(Weekday.Tuesday, value); OnPropertyChanged(nameof(ScheduleTuesday)); }
    }

    public bool ScheduleWednesday
    {
        get => (_scheduleActiveDays & Weekday.Wednesday) != 0;
        set { ToggleDay(Weekday.Wednesday, value); OnPropertyChanged(nameof(ScheduleWednesday)); }
    }

    public bool ScheduleThursday
    {
        get => (_scheduleActiveDays & Weekday.Thursday) != 0;
        set { ToggleDay(Weekday.Thursday, value); OnPropertyChanged(nameof(ScheduleThursday)); }
    }

    public bool ScheduleFriday
    {
        get => (_scheduleActiveDays & Weekday.Friday) != 0;
        set { ToggleDay(Weekday.Friday, value); OnPropertyChanged(nameof(ScheduleFriday)); }
    }

    public bool ScheduleSaturday
    {
        get => (_scheduleActiveDays & Weekday.Saturday) != 0;
        set { ToggleDay(Weekday.Saturday, value); OnPropertyChanged(nameof(ScheduleSaturday)); }
    }

    public bool ScheduleSunday
    {
        get => (_scheduleActiveDays & Weekday.Sunday) != 0;
        set { ToggleDay(Weekday.Sunday, value); OnPropertyChanged(nameof(ScheduleSunday)); }
    }

    public ObservableCollection<ChannelSlider> ScheduleChannels { get; }

    public bool IsSavingSchedule
    {
        get => _isSavingSchedule;
        private set => SetProperty(ref _isSavingSchedule, value);
    }

    public bool IsScheduleValid =>
        _scheduleSunrise is not null &&
        _scheduleSunset is not null &&
        _scheduleSunrise < _scheduleSunset &&
        (_scheduleSunset.Value.ToTimeSpan() - _scheduleSunrise.Value.ToTimeSpan()).TotalMinutes >= _scheduleRampUpMinutes + 1 &&
        _scheduleRampUpMinutes is >= 0 and <= 150 &&
        _scheduleActiveDays != Weekday.None;

    public string? ValidationMessage => IsScheduleValid ? null : GetValidationMessage();

    // ── Commands ─────────────────────────────────────────────────────────────

    public RelayCommand GoBackCommand { get; }

    public AsyncRelayCommand<ChannelSlider> ApplyBrightnessCommand { get; }

    public AsyncRelayCommand SaveScheduleCommand { get; }

    // ── Navigation ───────────────────────────────────────────────────────────

    public async void OnNavigatedTo(object parameter)
    {
        var lampId = (Guid)parameter;
        var aquariumId = _aquariumContext.CurrentAquarium!.Id;
        var lamps = await _lampService.GetLampsForAquariumAsync(aquariumId);
        Lamp = lamps.FirstOrDefault(l => l.Id == lampId);

        if (Lamp is null || IsUnmanaged)
            return;

        LoadScheduleState();

        IsConnecting = true;
        try
        {
            var device = await _lampService.ConnectAsync(Lamp);
            Device = device;

            if (Device is not null)
            {
                Device.Disconnected += OnDeviceDisconnected;
                PopulateChannels();
                PopulateScheduleChannels();
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

    // ── Private helpers ──────────────────────────────────────────────────────

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

    private void PopulateScheduleChannels()
    {
        ScheduleChannels.Clear();

        var profile = _lampService.GetProfileForModel(Lamp!.ModelName);
        if (profile is null)
            return;

        foreach (var mapping in profile.Channels)
        {
            var label = mapping.Channel.ToString();
            byte storedValue = 0;
            if (Lamp.Schedule?.ChannelBrightness.TryGetValue(label, out var sv) == true)
                storedValue = sv;
            ScheduleChannels.Add(new ChannelSlider
            {
                Channel = mapping.Channel,
                Label = label,
                Value = storedValue,
            });
        }
    }

    private void LoadScheduleState()
    {
        if (Lamp?.Schedule is not { } schedule)
            return;

        _scheduleSunrise = schedule.Sunrise;
        _scheduleSunset = schedule.Sunset;
        _scheduleRampUpMinutes = schedule.RampUpMinutes;
        _scheduleActiveDays = schedule.ActiveDays;

        OnPropertyChanged(nameof(ScheduleSunrise));
        OnPropertyChanged(nameof(ScheduleSunset));
        OnPropertyChanged(nameof(SunriseTimeSpan));
        OnPropertyChanged(nameof(SunsetTimeSpan));
        OnPropertyChanged(nameof(ScheduleRampUpMinutes));
        OnPropertyChanged(nameof(ScheduleRampUpMinutesDouble));
        OnPropertyChanged(nameof(ScheduleMonday));
        OnPropertyChanged(nameof(ScheduleTuesday));
        OnPropertyChanged(nameof(ScheduleWednesday));
        OnPropertyChanged(nameof(ScheduleThursday));
        OnPropertyChanged(nameof(ScheduleFriday));
        OnPropertyChanged(nameof(ScheduleSaturday));
        OnPropertyChanged(nameof(ScheduleSunday));
        OnPropertyChanged(nameof(IsScheduleValid));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private void ToggleDay(Weekday flag, bool enabled)
    {
        _scheduleActiveDays = enabled
            ? _scheduleActiveDays | flag
            : (Weekday)((int)_scheduleActiveDays & ~(int)flag);
        OnPropertyChanged(nameof(IsScheduleValid));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private string? GetValidationMessage()
    {
        if (_scheduleSunrise is null || _scheduleSunset is null)
            return "Sunrise and sunset times are required.";
        if (_scheduleSunrise >= _scheduleSunset)
            return "Sunrise must be earlier than sunset.";
        var gap = (_scheduleSunset.Value.ToTimeSpan() - _scheduleSunrise.Value.ToTimeSpan()).TotalMinutes;
        if (gap < _scheduleRampUpMinutes + 1)
            return "The gap between sunrise and sunset must be at least the ramp-up duration plus one minute.";
        if (_scheduleRampUpMinutes is < 0 or > 150)
            return "Ramp-up duration must be between 0 and 150 minutes.";
        if (_scheduleActiveDays == Weekday.None)
            return "At least one day must be selected.";
        return null;
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

    private async Task SaveScheduleAsync(CancellationToken cancellationToken)
    {
        if (Lamp is null || !IsScheduleValid)
            return;

        IsSavingSchedule = true;
        try
        {
            var channelBrightness = ScheduleChannels.ToDictionary(
                s => s.Channel.ToString(),
                s => (byte)Math.Clamp(s.Value, 0, 100));

            var config = new ScheduleConfiguration
            {
                Sunrise = _scheduleSunrise!.Value,
                Sunset = _scheduleSunset!.Value,
                RampUpMinutes = _scheduleRampUpMinutes,
                ChannelBrightness = channelBrightness,
                ActiveDays = _scheduleActiveDays,
            };

            await _lampService.SaveScheduleAsync(Lamp.Id, config, cancellationToken);

            if (Device is not null)
            {
                var bleBrightness = ScheduleChannels.ToDictionary(
                    s => s.Channel,
                    s => (byte)Math.Clamp(s.Value, 0, 100));

                var lightSchedule = new LightSchedule(
                    config.Sunrise,
                    config.Sunset,
                    bleBrightness,
                    config.RampUpMinutes,
                    config.ActiveDays);

                await Device.AddScheduleAsync(lightSchedule, cancellationToken);
            }
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
            IsErrorOpen = true;
        }
        catch (ChihirosException ex)
        {
            ErrorMessage = ex.Message;
            IsErrorOpen = true;
        }
        finally
        {
            IsSavingSchedule = false;
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
