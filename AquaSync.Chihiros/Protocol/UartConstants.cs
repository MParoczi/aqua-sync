namespace AquaSync.Chihiros.Protocol;

/// <summary>
///     Nordic UART Service UUIDs used by Chihiros BLE devices.
/// </summary>
internal static class UartConstants
{
    public static readonly Guid ServiceUuid = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
    public static readonly Guid RxCharacteristicUuid = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
    public static readonly Guid TxCharacteristicUuid = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");
}
