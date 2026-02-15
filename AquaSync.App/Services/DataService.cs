using System.Text.Json;
using AquaSync.App.Contracts.Services;

namespace AquaSync.App.Services;

/// <summary>
/// JSON file-based data storage under %LOCALAPPDATA%/AquaSync/.
/// </summary>
public sealed class DataService : IDataService
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _rootPath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public DataService()
    {
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AquaSync");

        Directory.CreateDirectory(_rootPath);
    }

    public string GetDataFolderPath() => _rootPath;

    public async Task<T?> ReadAsync<T>(string folderName, string fileName) where T : class
    {
        var filePath = GetFilePath(folderName, fileName);

        if (!File.Exists(filePath))
        {
            return default;
        }

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

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetFilePath(string folderName, string fileName)
    {
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        return Path.Combine(_rootPath, folderName, fileName);
    }
}
