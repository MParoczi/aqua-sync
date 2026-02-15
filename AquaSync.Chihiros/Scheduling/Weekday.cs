namespace AquaSync.Chihiros.Scheduling;

/// <summary>
/// Bitmask for selecting weekdays in Chihiros scheduling commands.
/// Bit layout (MSB to LSB): Monday Tuesday Wednesday Thursday Friday Saturday Sunday.
/// </summary>
[Flags]
public enum Weekday : byte
{
    None      = 0,
    Sunday    = 0b_0000001,
    Saturday  = 0b_0000010,
    Friday    = 0b_0000100,
    Thursday  = 0b_0001000,
    Wednesday = 0b_0010000,
    Tuesday   = 0b_0100000,
    Monday    = 0b_1000000,
    Everyday  = 0b_1111111
}
