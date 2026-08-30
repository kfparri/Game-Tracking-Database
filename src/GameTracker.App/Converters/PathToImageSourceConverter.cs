using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Globalization;

namespace GameTracker.App.Converters
{
    public class PathToImageSourceConverter : IValueConverter
    {
        private static readonly BitmapImage Placeholder = new(new Uri("pack://application:,,,/Assets/no-cover.png"));

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return Placeholder;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                return bitmap;

            }
            catch
            {
                return Placeholder;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
    }

    /*
     * I was working with Claude on this and this was the prompt that I was using when it failed:
     * Now that I can display the data, I would like to have the ability to insert games one at a time and edit the game.  Can I have an edit button on the flyout that would replace the display with an edit flyout?  Or is there a better method?
     * */
}
