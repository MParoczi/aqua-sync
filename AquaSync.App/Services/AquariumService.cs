using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;

namespace AquaSync.App.Services;

/// <summary>
/// Provides CRUD operations for aquarium profiles and manages associated gallery files.
/// </summary>
public sealed class AquariumService : IAquariumService
{
    private const string AquariumsFolder = "aquariums";
    private const string GalleryFolder = "gallery";
    private const string ThumbnailPrefix = "thumbnail";

    private readonly IDataService _dataService;

    public AquariumService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IReadOnlyList<Aquarium>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var aquariums = await _dataService.ReadAllAsync<Aquarium>(AquariumsFolder).ConfigureAwait(false);

        return aquariums
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
    }

    public async Task<Aquarium?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dataService.ReadAsync<Aquarium>(AquariumsFolder, id.ToString()).ConfigureAwait(false);
    }

    public async Task SaveAsync(Aquarium aquarium, CancellationToken cancellationToken = default)
    {
        await _dataService.SaveAsync(AquariumsFolder, aquarium.Id.ToString(), aquarium).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dataService.DeleteAsync(AquariumsFolder, id.ToString()).ConfigureAwait(false);

        var galleryPath = GetGalleryPath(id);

        if (Directory.Exists(galleryPath))
        {
            Directory.Delete(galleryPath, recursive: true);
        }
    }

    public async Task<string> SaveThumbnailAsync(Guid aquariumId, string sourceFilePath, CancellationToken cancellationToken = default)
    {
        var galleryPath = GetGalleryPath(aquariumId);
        Directory.CreateDirectory(galleryPath);

        // Delete any existing thumbnail before copying.
        DeleteExistingThumbnails(galleryPath);

        var extension = Path.GetExtension(sourceFilePath);
        var targetFileName = ThumbnailPrefix + extension;
        var targetPath = Path.Combine(galleryPath, targetFileName);

        await Task.Run(() => File.Copy(sourceFilePath, targetPath, overwrite: true), cancellationToken).ConfigureAwait(false);

        // Return relative path for storage in the aquarium JSON.
        return Path.Combine(GalleryFolder, aquariumId.ToString(), targetFileName);
    }

    public Task DeleteThumbnailAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var galleryPath = GetGalleryPath(aquariumId);

        if (Directory.Exists(galleryPath))
        {
            DeleteExistingThumbnails(galleryPath);
        }

        return Task.CompletedTask;
    }

    private string GetGalleryPath(Guid aquariumId)
    {
        return Path.Combine(_dataService.GetDataFolderPath(), GalleryFolder, aquariumId.ToString());
    }

    private static void DeleteExistingThumbnails(string galleryPath)
    {
        foreach (var file in Directory.GetFiles(galleryPath, $"{ThumbnailPrefix}.*"))
        {
            File.Delete(file);
        }
    }
}
