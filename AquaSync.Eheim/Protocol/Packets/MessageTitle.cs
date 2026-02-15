namespace AquaSync.Eheim.Protocol.Packets;

/// <summary>
/// Known WebSocket message "title" values used by the EHEIM protocol.
/// </summary>
internal static class MessageTitle
{
    public const string UsrDta = "USRDTA";
    public const string GetUsrDta = "GET_USRDTA";
    public const string MeshNetwork = "MESH_NETWORK";
    public const string FilterData = "FILTER_DATA";
    public const string GetFilterData = "GET_FILTER_DATA";
    public const string SetFilterPump = "SET_FILTER_PUMP";
    public const string StartFilterNormalModeWithoutComp = "START_FILTER_NORMAL_MODE_WITHOUT_COMP";
    public const string StartFilterNormalModeWithComp = "START_FILTER_NORMAL_MODE_WITH_COMP";
    public const string StartFilterPulseMode = "START_FILTER_PULSE_MODE";
    public const string StartNocturnalMode = "START_NOCTURNAL_MODE";
}
