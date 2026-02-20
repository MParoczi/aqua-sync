namespace AquaSync.App.Contracts.Services;

/// <summary>
///     Maps string page keys to their corresponding Page types.
/// </summary>
public interface IPageService
{
    Type GetPageType(string key);

    string GetPageKey(Type pageType);
}
