namespace AquaSync.Chihiros.Protocol;

/// <summary>
///     16-bit BLE message ID split into high/low bytes.
///     Neither byte may ever equal 0x5A (90) as that is reserved as a command ID.
/// </summary>
internal struct MessageId
{
    private const byte Forbidden = 90;

    public byte High { get; }
    public byte Low { get; }

    public MessageId()
    {
        High = 0;
        Low = 0;
    }

    public MessageId(byte high, byte low)
    {
        High = high;
        Low = low;
    }

    /// <summary>
    ///     Advance to the next message ID, skipping any value where a byte would be 0x5A.
    /// </summary>
    public MessageId Next()
    {
        if (Low == 255)
        {
            if (High == 255)
                // Wrap around to the beginning
                return new MessageId(0, 1);

            if (High == Forbidden - 1)
                // Skip so high byte is never 90
                return new MessageId((byte)(High + 2), Low);

            return new MessageId((byte)(High + 1), 0);
        }

        if (Low == Forbidden - 1)
            // Skip so low byte is never 90
            return new MessageId(0, (byte)(Low + 2));

        return new MessageId(0, (byte)(Low + 1));
    }
}
