using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.Chihiros.Discovery;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Bluetooth;

namespace AquaSync.App.Views;

public sealed partial class AddLampDialog : ContentDialog
{
    private readonly ILampService _lampService;
    private readonly Guid _currentAquariumId;
    private CancellationTokenSource? _cts;

    public AddLampDialog(ILampService lampService, Guid currentAquariumId)
    {
        _lampService = lampService;
        _currentAquariumId = currentAquariumId;
        InitializeComponent();
        Opened += OnOpened;
        Closing += OnClosing;
    }

    public DiscoveredDevice? SelectedDevice { get; private set; }

    public ObservableCollection<DiscoveredDeviceItem> Devices { get; } = [];

    private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        try
        {
            var adapter = await BluetoothAdapter.GetDefaultAsync();
            if (adapter is null)
            {
                ShowBluetoothError();
                return;
            }
        }
        catch (Exception)
        {
            ShowBluetoothError();
            return;
        }

        _cts = new CancellationTokenSource();
        var progress = new Progress<DiscoveredDevice>(device => _ = AddDiscoveredDeviceAsync(device));
        try
        {
            await _lampService.ScanAsync(progress, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected: dialog was closed before scan completed.
        }
        catch (Exception)
        {
            ShowBluetoothError();
        }
    }

    private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task AddDiscoveredDeviceAsync(DiscoveredDevice device)
    {
        // Deduplicate: skip if already in list.
        if (Devices.Any(d => d.Device.BluetoothAddress == device.BluetoothAddress))
            return;

        var address = device.BluetoothAddress.ToString("X12");
        var isAssigned = await _lampService.IsAddressAssignedAsync(address);

        // Re-check after await in case another discovery beat us here.
        if (Devices.Any(d => d.Device.BluetoothAddress == device.BluetoothAddress))
            return;

        Devices.Add(new DiscoveredDeviceItem { Device = device, IsAvailable = !isAssigned });
    }

    private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DeviceList.SelectedItem is DiscoveredDeviceItem { IsAvailable: true } item)
        {
            SelectedDevice = item.Device;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            SelectedDevice = null;
            IsPrimaryButtonEnabled = false;
        }
    }

    private void ShowBluetoothError()
    {
        ScanningRing.IsActive = false;
        ScanningText.Visibility = Visibility.Collapsed;
        BluetoothErrorBar.IsOpen = true;
    }
}
