namespace AquaSync.App.Models;

/// <summary>
///     Persisted global application settings.
///     Stored as <c>settings/app-settings.json</c> via <see cref="Contracts.Services.IDataService" />.
/// </summary>
public sealed class AppSettings
{
    public VolumeUnit DefaultVolumeUnit { get; set; } = VolumeUnit.Liters;

    public DimensionUnit DefaultDimensionUnit { get; set; } = DimensionUnit.Centimeters;

    public AppTheme Theme { get; set; } = AppTheme.System;

    public string? DataFolderPath { get; set; }
}
