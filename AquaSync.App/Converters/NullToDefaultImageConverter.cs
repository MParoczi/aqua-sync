using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AquaSync.App.Converters;

/// <summary>
/// Returns a BitmapImage for the thumbnail path. Falls back to the bundled default
/// aquarium graphic when the path is null or the file is missing (FR-038).
/// </summary>
public sealed class NullToDefaultImageConverter : IValueConverter
{
    private static readonly Uri s_defaultAssetUri = new("ms-appx:///Assets/aquarium-default.png");

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string thumbnailPath && !string.IsNullOrEmpty(thumbnailPath))
        {
            var rootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AquaSync");
            var fullPath = Path.Combine(rootPath, thumbnailPath);

            if (File.Exists(fullPath))
            {
                return new BitmapImage(new Uri(fullPath));
            }
        }

        return new BitmapImage(s_defaultAssetUri);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
