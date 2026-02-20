using Microsoft.UI.Xaml.Controls;

namespace AquaSync.App.Helpers;

/// <summary>
///     Extension methods for <see cref="Frame" />.
/// </summary>
public static class FrameExtensions
{
    /// <summary>
    ///     Gets the DataContext (ViewModel) of the currently displayed page.
    /// </summary>
    public static object? GetPageViewModel(this Frame frame)
    {
        return frame.Content is Page page ? page.DataContext : null;
    }
}
