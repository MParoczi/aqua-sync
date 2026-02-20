using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AquaSync.App.Converters;

/// <summary>
///     Converts a boolean to Visibility. True = Visible, False = Collapsed.
///     Pass any ConverterParameter to invert the logic.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var boolValue = value is true;
        var invert = parameter is not null;

        if (invert) boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
