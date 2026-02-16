using AquaSync.App.Models;

namespace AquaSync.App.Contracts.Services;

/// <summary>
/// Provides CRUD operations for aquarium profiles and manages associated gallery files.
/// </summary>
public interface IAquariumService
{
    /// <summary>
    /// Returns all aquarium profiles (active and archived), ordered by CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<Aquarium>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single aquarium by its ID, or null if not found.
    /// </summary>
    Task<Aquarium?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates an aquarium profile.
    /// For new aquariums, Id and CreatedAt should already be set by the caller.
    /// </summary>
    Task SaveAsync(Aquarium aquarium, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes an aquarium profile and its associated gallery folder.
    /// No-op if the aquarium does not exist.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies an image file to the aquarium's gallery folder and returns the relative path.
    /// Deletes any existing thumbnail before copying.
    /// </summary>
    Task<string> SaveThumbnailAsync(Guid aquariumId, string sourceFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the thumbnail image from the aquarium's gallery folder.
    /// No-op if no thumbnail exists.
    /// </summary>
    Task DeleteThumbnailAsync(Guid aquariumId, CancellationToken cancellationToken = default);
}
