using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AquaSync.App.Converters;

/// <summary>
/// Converts a string to Visibility. Non-empty = Visible, null/empty = Collapsed.
/// Used to show/hide inline validation error TextBlocks (FR-014).
/// </summary>
public sealed class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is string s && !string.IsNullOrEmpty(s)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
