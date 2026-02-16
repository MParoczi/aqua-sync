using AquaSync.Eheim.Devices.Enums;

namespace AquaSync.Eheim.Data;

/// <summary>
///     Static lookup tables mapping flow rate indices and frequency steps to real-world values.
///     All flow rates are in liters per hour (metric). All frequencies are in Hz.
/// </summary>
internal static class FlowRateTable
{
    /// <summary>
    ///     Flow rate values in L/h, indexed 0–14, keyed by filter version code.
    /// </summary>
    private static readonly Dictionary<int, IReadOnlyList<int>> FlowRatesLiters = new()
    {
        [74] = [400, 440, 480, 515, 550, 585, 620, 650, 680, 710, 740, 770, 800, 830, 860],
        [76] = [400, 460, 515, 565, 610, 650, 690, 730, 770, 805, 840, 875, 910, 945, 980],
        [78] = [400, 470, 540, 600, 650, 700, 745, 785, 825, 865, 905, 945, 985, 1025, 1065]
    };

    /// <summary>
    ///     Manual frequency values in Hz, indexed 0–14, keyed by filter version code.
    /// </summary>
    private static readonly Dictionary<int, IReadOnlyList<double>> ManualFrequencies = new()
    {
        [74] = [35, 37.5, 40.5, 43, 45.5, 48, 51, 53.5, 56, 59, 61.5, 64, 66.5, 69.5, 72],
        [76] = [35, 38, 41, 44, 46.5, 49.5, 52.5, 55.5, 58.5, 61.5, 64.5, 67, 70, 73, 76],
        [78] = [35, 38, 41.5, 44.5, 48, 51, 54, 57.5, 60.5, 64, 67, 70, 73.5, 76.5, 80]
    };

    public static IReadOnlyList<int> GetFlowRates(int filterVersion)
    {
        return FlowRatesLiters.GetValueOrDefault(filterVersion, FlowRatesLiters[78]);
    }

    public static IReadOnlyList<double> GetManualSpeeds(int filterVersion)
    {
        return ManualFrequencies.GetValueOrDefault(filterVersion, ManualFrequencies[78]);
    }

    public static EheimFilterModel ResolveFilterModel(int filterVersion, string tankConfig)
    {
        return filterVersion switch
        {
            74 => EheimFilterModel.Filter350,
            76 => EheimFilterModel.Filter450,
            78 when tankConfig == "WITH_THERMO" => EheimFilterModel.Filter600T,
            78 => EheimFilterModel.Filter700,
            _ => EheimFilterModel.Filter700
        };
    }
}
