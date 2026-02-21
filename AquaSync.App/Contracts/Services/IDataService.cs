namespace AquaSync.App.Contracts.Services;

/// <summary>
///     Provides JSON file-based storage for application data.
///     Files are stored in the local app data folder under AquaSync/.
/// </summary>
public interface IDataService
{
    /// <summary>
    ///     Gets whether the data-folder-redirect.json pointed to an invalid path
    ///     at startup, causing a fallback to the default location.
    /// </summary>
    bool HasRedirectFallback { get; }

    /// <summary>
    ///     Reads and deserializes a JSON file. Returns default(T) if the file does not exist.
    /// </summary>
    Task<T?> ReadAsync<T>(string folderName, string fileName) where T : class;

    /// <summary>
    ///     Serializes and writes an object to a JSON file. Creates the folder if it does not exist.
    /// </summary>
    Task SaveAsync<T>(string folderName, string fileName, T data) where T : class;

    /// <summary>
    ///     Reads and deserializes all JSON files in a folder.
    ///     Returns an empty list if the folder does not exist or contains no JSON files.
    ///     Files that fail deserialization are skipped.
    /// </summary>
    Task<IReadOnlyList<T>> ReadAllAsync<T>(string folderName) where T : class;

    /// <summary>
    ///     Deletes a JSON file if it exists.
    /// </summary>
    Task DeleteAsync(string folderName, string fileName);

    /// <summary>
    ///     Returns the full path to the application data root folder.
    /// </summary>
    string GetDataFolderPath();

    /// <summary>
    ///     Updates the root data folder path. Used when the user relocates
    ///     the data storage directory.
    /// </summary>
    void SetDataFolderPath(string newPath);
}
