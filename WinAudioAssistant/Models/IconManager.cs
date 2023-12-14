using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Loads and maintains a cache of BitmapSources from icons.
    /// </summary>
    public partial class IconManager
    {
        #region Private Fields
        private const string _notFoundResourcePath = "Resources/Icons/MissingFile.ico"; // Resource path to generic icon that is returned when an icon path is not found
        private readonly Dictionary<Tuple<string, int>, BitmapSource> _bitmapCache = new(); // Cache of icon bitmaps, keyed by icon/resource path and bitmap size
        #endregion

        #region Public Methods
        /// <summary>
        /// Fetches an icon from the cache or file system and returns it as a BitmapSource.
        /// </summary>
        /// <param name="iconPath">Path to the icon file. Can include an icon index or resource identifier 
        /// at the end of the string separated by a comma, or simply a path to an .ico file.</param>
        /// <param name="bitmapSize">Size of the bitmap to return, in pixels.</param>
        /// <returns>A BitmapSource from the icon at the path if found, or a generic bitmap if not found.</returns>
        public BitmapSource GetBitmapFromIconPath(string? iconPath, int bitmapSize)
        {
            // Bitmap size sanity check
            if (bitmapSize < 16 || bitmapSize % 4 != 0)
                throw new ArgumentException("Invalid bitmap size", nameof(bitmapSize));

            // Check for null or empty path
            if (iconPath is null || iconPath == string.Empty)
                return GetBitmapFromResourcePath(_notFoundResourcePath, bitmapSize);

            // Path is valid, check if bitmap is cached
            Tuple<string, int> key = new(iconPath, bitmapSize);
            if (_bitmapCache.TryGetValue(key, out BitmapSource? bitmap))
                return bitmap;

            // Bitmap is not cached, attempt retrieve icon from file system
            Icon? icon = null;
            try
            {
                Match match = IconPathPattern().Match(iconPath);
                icon = match.Success
                    ? Icon.ExtractIcon(match.Groups[1].Value, int.Parse(match.Groups[2].Value)) // Icon has an index or resource identifier
                    : Icon.ExtractIcon(iconPath, 0); // Icon is a file path

            }
            catch (Exception) { }

            if (icon is not null)
            {
                // Icon was found, cache bitmap and return it
                bitmap = ConvertIconToBitmapSource(icon, bitmapSize);
                icon.Dispose();
                _bitmapCache[key] = bitmap;
                return bitmap;
            }

            // Icon was not found, cache a generic bitmap and return it
            bitmap = GetBitmapFromResourcePath(_notFoundResourcePath, bitmapSize);
            _bitmapCache[key] = bitmap;
            return bitmap;
        }

        /// <summary>
        /// Fetches an icon from the cache or application resources and returns it as a BitmapSource.
        /// </summary>
        /// <param name="resourcePath">Path to the resource, which must be an .ico file.</param>
        /// <param name="bitmapSize">Size of the bitmap to return, in pixels.</param>
        /// <returns>A BitmapSource from the icon at the path.</returns>
        public BitmapSource GetBitmapFromResourcePath(string resourcePath, int bitmapSize)
        {
            // Bitmap size sanity check
            if (bitmapSize < 16 || bitmapSize % 4 != 0)
                throw new ArgumentException("Invalid bitmap size", nameof(bitmapSize));

            // Check if bitmap is cached
            Tuple<string, int> key = new(resourcePath, bitmapSize);
            if (_bitmapCache.TryGetValue(key, out BitmapSource? bitmap))
                return bitmap;

            // Bitmap is not cached, retrieve icon from application resources
            Uri resourceUri = new(resourcePath, UriKind.Relative);
            StreamResourceInfo streamInfo = App.GetResourceStream(resourceUri) ?? throw new FileNotFoundException("Resource not found", resourcePath);
            using Stream stream = streamInfo.Stream;
            Icon icon = new(stream);

            // Cache bitmap and return it
            bitmap = ConvertIconToBitmapSource(icon, bitmapSize);
            icon.Dispose();
            _bitmapCache[key] = bitmap;
            return bitmap;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns a compile-time generated regex instance that matches a string with an icon index/resourceid at the end.
        /// </summary>
        [GeneratedRegex(@"^(.+),(-?\d+)$")]
        private static partial Regex IconPathPattern();

        /// <summary>
        /// Converts an Icon into a BitmapSource for use in WPF elements.
        /// </summary>
        private static BitmapSource ConvertIconToBitmapSource(Icon icon, int bitmapSize)
            => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmapSize, bitmapSize));
        #endregion
    }
}
