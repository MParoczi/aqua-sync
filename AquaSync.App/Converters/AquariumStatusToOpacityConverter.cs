using AquaSync.App.Models;
using Microsoft.UI.Xaml.Data;

namespace AquaSync.App.Converters;

/// <summary>
/// Returns 0.5 opacity for Archived status, 1.0 for Active.
/// </summary>
public sealed class AquariumStatusToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AquariumStatus status && status == AquariumStatus.Archived)
        {
            return 0.5;
        }

        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
