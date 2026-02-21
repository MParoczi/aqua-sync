using System.Text.Json;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;

namespace AquaSync.App.Services;

/// <summary>
///     JSON file-based data storage under %LOCALAPPDATA%/AquaSync/.
/// </summary>
public sealed class DataService : IDataService
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    private string _rootPath;

    public DataService()
    {
        var defaultRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AquaSync");

        Directory.CreateDirectory(defaultRoot);

        var redirectPath = Path.Combine(defaultRoot, "data-folder-redirect.json");
        if (File.Exists(redirectPath))
        {
            try
            {
                var json = File.ReadAllText(redirectPath);
                var redirect = JsonSerializer.Deserialize<DataFolderRedirect>(json, s_jsonOptions);
                if (redirect?.CustomDataFolderPath is not null
                    && Directory.Exists(redirect.CustomDataFolderPath))
                {
                    _rootPath = redirect.CustomDataFolderPath;
                    return;
                }
            }
            catch (JsonException)
            {
                // Corrupt redirect file — fall back to default.
            }

            HasRedirectFallback = true;
        }

        _rootPath = defaultRoot;
    }

    public bool HasRedirectFallback { get; private set; }

    public string GetDataFolderPath()
    {
        return _rootPath;
    }

    public void SetDataFolderPath(string newPath)
    {
        _rootPath = newPath;
        HasRedirectFallback = false;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<T?> ReadAsync<T>(string folderName, string fileName) where T : class
    {
        var filePath = GetFilePath(folderName, fileName);

        if (!File.Exists(filePath)) return default;

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream, s_jsonOptions).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<T>> ReadAllAsync<T>(string folderName) where T : class
    {
        var folderPath = Path.Combine(_rootPath, folderName);

        if (!Directory.Exists(folderPath)) return [];

        var files = Directory.GetFiles(folderPath, "*.json");

        if (files.Length == 0) return [];

        var results = new List<T>(files.Length);

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            foreach (var filePath in files)
                try
                {
                    await using var stream = File.OpenRead(filePath);
                    var item = await JsonSerializer.DeserializeAsync<T>(stream, s_jsonOptions).ConfigureAwait(false);

                    if (item is not null) results.Add(item);
                }
                catch (JsonException)
                {
                    // Skip files that fail deserialization (FR-037).
                }
        }
        finally
        {
            _lock.Release();
        }

        return results;
    }

    public async Task SaveAsync<T>(string folderName, string fileName, T data) where T : class
    {
        var filePath = GetFilePath(folderName, fileName);
        var folder = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(folder);

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, data, s_jsonOptions).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task DeleteAsync(string folderName, string fileName)
    {
        var filePath = GetFilePath(folderName, fileName);

        if (File.Exists(filePath)) File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string GetFilePath(string folderName, string fileName)
    {
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) fileName += ".json";

        return Path.Combine(_rootPath, folderName, fileName);
    }
}
