namespace AquaSync.Eheim.Devices;

/// <summary>
///     Common interface for all EHEIM Digital devices.
/// </summary>
public interface IEheimDevice
{
    string MacAddress { get; }
    string Name { get; }
    string ModelName { get; }
    string FirmwareVersion { get; }
    string AquariumName { get; }
    IObservable<int> SystemLedBrightness { get; }
    Task SetSystemLedBrightnessAsync(int value, CancellationToken cancellationToken = default);
}
