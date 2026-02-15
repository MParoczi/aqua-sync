namespace AquaSync.Chihiros.Protocol;

/// <summary>
/// Builds BLE command byte arrays for the Chihiros protocol.
/// Command structure: [cmd_id, 1, param_count+5, msg_hi, msg_lo, mode, ...params, checksum]
/// </summary>
internal static class CommandBuilder
{
    private const byte Forbidden = 90;

    /// <summary>
    /// Encode a raw command. Parameters are sanitized (0x5A replaced with 0x59).
    /// If the checksum equals 0x5A, the message ID is bumped and encoding retried.
    /// </summary>
    internal static byte[] Encode(byte commandId, byte mode, MessageId msgId, ReadOnlySpan<byte> parameters)
    {
        var length = parameters.Length + 5;
        var command = new byte[parameters.Length + 7]; // header(6) + params + checksum(1)

        command[0] = commandId;
        command[1] = 1;
        command[2] = (byte)length;
        command[3] = msgId.High;
        command[4] = msgId.Low;
        command[5] = mode;

        for (int i = 0; i < parameters.Length; i++)
        {
            command[6 + i] = parameters[i] == Forbidden ? (byte)(Forbidden - 1) : parameters[i];
        }

        byte checksum = CalculateChecksum(command.AsSpan(0, command.Length - 1));

        if (checksum == Forbidden)
        {
            // Bump the low byte of the message ID and re-encode to avoid forbidden checksum
            var adjusted = new MessageId(msgId.High, (byte)(msgId.Low + 1));
            return Encode(commandId, mode, adjusted, parameters);
        }

        command[^1] = checksum;
        return command;
    }

    /// <summary>
    /// XOR checksum of bytes from index 1 onward.
    /// </summary>
    private static byte CalculateChecksum(ReadOnlySpan<byte> command)
    {
        byte checksum = command[1];
        for (int i = 2; i < command.Length; i++)
        {
            checksum ^= command[i];
        }
        return checksum;
    }

    /// <summary>
    /// Set brightness of a single color channel (manual mode).
    /// Command ID: 90, Mode: 7, Parameters: [channelId, brightness]
    /// </summary>
    public static byte[] CreateManualBrightnessCommand(MessageId msgId, byte channelId, byte brightness)
    {
        ReadOnlySpan<byte> parameters = [channelId, brightness];
        return Encode(Forbidden, 7, msgId, parameters);
    }

    /// <summary>
    /// Switch the device to auto mode.
    /// Command ID: 90, Mode: 5, Parameters: [18, 255, 255]
    /// </summary>
    public static byte[] CreateSwitchToAutoModeCommand(MessageId msgId)
    {
        ReadOnlySpan<byte> parameters = [18, 255, 255];
        return Encode(Forbidden, 5, msgId, parameters);
    }

    /// <summary>
    /// Set the current time on the device (required for auto mode scheduling).
    /// Command ID: 90, Mode: 9, Parameters: [year-2000, month, weekday(1-7), hour, minute, second]
    /// </summary>
    public static byte[] CreateSetTimeCommand(MessageId msgId, DateTime now)
    {
        ReadOnlySpan<byte> parameters =
        [
            (byte)(now.Year - 2000),
            (byte)now.Month,
            (byte)(now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek), // 1=Mon..7=Sun
            (byte)now.Hour,
            (byte)now.Minute,
            (byte)now.Second
        ];
        return Encode(Forbidden, 9, msgId, parameters);
    }

    /// <summary>
    /// Add an auto-mode scheduling setting.
    /// Command ID: 165, Mode: 25.
    /// The protocol supports 3 brightness slots mapped to channels 0, 1, 2.
    /// </summary>
    public static byte[] CreateAddAutoSettingCommand(
        MessageId msgId,
        TimeOnly sunrise,
        TimeOnly sunset,
        byte brightness0,
        byte brightness1,
        byte brightness2,
        byte rampUpMinutes,
        byte weekdays)
    {
        ReadOnlySpan<byte> parameters =
        [
            (byte)sunrise.Hour,
            (byte)sunrise.Minute,
            (byte)sunset.Hour,
            (byte)sunset.Minute,
            rampUpMinutes,
            weekdays,
            brightness0,
            brightness1,
            brightness2,
            255, 255, 255, 255, 255
        ];
        return Encode(165, 25, msgId, parameters);
    }

    /// <summary>
    /// Delete an auto-mode scheduling setting by sending brightness values of 255.
    /// </summary>
    public static byte[] CreateDeleteAutoSettingCommand(
        MessageId msgId,
        TimeOnly sunrise,
        TimeOnly sunset,
        byte rampUpMinutes,
        byte weekdays)
    {
        return CreateAddAutoSettingCommand(msgId, sunrise, sunset, 255, 255, 255, rampUpMinutes, weekdays);
    }

    /// <summary>
    /// Reset all auto-mode settings.
    /// Command ID: 90, Mode: 5, Parameters: [5, 255, 255]
    /// </summary>
    public static byte[] CreateResetAutoSettingsCommand(MessageId msgId)
    {
        ReadOnlySpan<byte> parameters = [5, 255, 255];
        return Encode(Forbidden, 5, msgId, parameters);
    }
}
