using AquaSync.Chihiros.Scheduling;
using Microsoft.UI.Xaml.Data;

namespace AquaSync.App.Converters;

/// <summary>
///     Converts a <see cref="Weekday"/> flags value to bool for a specific day flag.
///     ConverterParameter must be the enum member name (e.g., "Monday").
///     ConvertBack is not supported — use individual bool properties on the ViewModel instead.
/// </summary>
public sealed class WeekdayFlagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var current = (Weekday)value;
        var flag = Enum.Parse<Weekday>((string)parameter);
        return current.HasFlag(flag);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException(
            "WeekdayFlagConverter.ConvertBack is not supported. " +
            "Bind CheckBox.IsChecked to individual ViewModel bool properties instead.");
    }
}
