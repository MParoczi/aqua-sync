namespace AquaSync.App.Models;

/// <summary>
///     Represents a single substrate or additive layer within an aquarium.
///     Embedded in the parent Aquarium's JSON (not a separate file).
/// </summary>
public sealed class SubstrateEntry
{
    /// <summary>
    ///     Uniquely identifies the entry within the substrate list.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Manufacturer or brand name.
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    ///     Specific product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    ///     Substrate, Additive, or SoilCap.
    /// </summary>
    public SubstrateType Type { get; set; }

    /// <summary>
    ///     Layer depth in the parent aquarium's dimension unit. Must be > 0.
    /// </summary>
    public double LayerDepth { get; set; }

    /// <summary>
    ///     Date the substrate was added. Date-only input (time stored as midnight UTC).
    /// </summary>
    public DateTimeOffset DateAdded { get; set; }

    /// <summary>
    ///     Optional notes about this substrate entry.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Display order in the substrate list (0-based).
    /// </summary>
    public int DisplayOrder { get; set; }
}
