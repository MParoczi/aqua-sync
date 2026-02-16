using System.Text.Json.Serialization;

namespace AquaSync.App.Models;

/// <summary>
/// Represents a single aquarium profile. Stored as an individual JSON file at aquariums/{id}.json.
/// </summary>
public sealed class Aquarium
{
    /// <summary>
    /// Primary identifier; also used as the JSON filename.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Aquarium name. Max 100 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Aquarium water volume. Must be > 0.
    /// Display up to 1 decimal place; stored at input precision.
    /// </summary>
    public double Volume { get; set; }

    /// <summary>
    /// Unit for volume measurement. Locked after creation.
    /// </summary>
    public VolumeUnit VolumeUnit { get; set; }

    /// <summary>
    /// Aquarium length. Must be > 0.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Aquarium width. Must be > 0.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Aquarium height. Must be > 0.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Unit for dimension measurements. Locked after creation.
    /// </summary>
    public DimensionUnit DimensionUnit { get; set; }

    /// <summary>
    /// Freshwater, Saltwater, or Brackish. Locked after creation.
    /// </summary>
    public AquariumType AquariumType { get; set; }

    /// <summary>
    /// Date the aquarium was set up. Date-only input (time stored as midnight UTC).
    /// Locked after creation.
    /// </summary>
    public DateTimeOffset SetupDate { get; set; }

    /// <summary>
    /// Optional notes. Max 2000 characters.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Relative path to thumbnail image in gallery folder.
    /// Null means use the default aquarium graphic.
    /// </summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// Active or Archived lifecycle status.
    /// </summary>
    public AquariumStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the profile was created. Set once, never modified.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Ordered collection of substrate and additive entries.
    /// </summary>
    public List<SubstrateEntry> Substrates { get; set; } = [];

    // --- Display helpers (not serialized) ---

    [JsonIgnore]
    public string VolumeDisplay => VolumeUnit switch
    {
        VolumeUnit.Liters => $"{Volume:0.#} L",
        VolumeUnit.Gallons => $"{Volume:0.#} gal",
        _ => Volume.ToString("0.#"),
    };

    [JsonIgnore]
    public string SetupDateDisplay => SetupDate.ToString("MMM d, yyyy");

    [JsonIgnore]
    public bool IsArchived => Status == AquariumStatus.Archived;
}
