using Microsoft.UI.Xaml.Data;

namespace AquaSync.App.Converters;

/// <summary>
/// Returns the default aquarium asset path when the thumbnail path is null,
/// otherwise resolves the full path from the data folder.
/// </summary>
public sealed class NullToDefaultImageConverter : IValueConverter
{
    private const string DefaultAssetPath = "ms-appx:///Assets/aquarium-default.png";

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
                return fullPath;
            }
        }

        return DefaultAssetPath;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
