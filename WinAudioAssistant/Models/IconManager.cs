using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Loads and maintains a cache of audio device icons and images.
    /// </summary>
    public partial class IconManager
    {
        #region Constructors
        /// <summary>
        /// Initializes the IconManager and loads the generic icon.
        /// </summary>
        public IconManager()
        {
            _notFoundIcon = GetIconFromResourcePath("Resources/Icons/MissingFile.ico");
            _notFoundBitmap = GetBitmapFromResourcePath("Resources/Icons/MissingFile.ico");
        }
        #endregion

        #region Private Fields
        private readonly Dictionary<string, Icon> _iconCache = new(); // Cache of Icon references. This is probably not necessary to keep
        private readonly Dictionary<string, BitmapSource> _bitmapCache = new(); // Cache of BitmapSource references
        private readonly Icon _notFoundIcon; // Generic icon that is returned when an icon path is not found
        private readonly BitmapSource _notFoundBitmap; // Generic bitmap that is returned when an icon path is not found
        #endregion

        #region Public Methods
        /// <summary>
        /// Fetches an icon from the cache or file system and returns it as an Icon.
        /// </summary>
        /// <param name="iconPath">Path to the icon file. Can include an icon index or resource identifier 
        /// at the end of the string separated by a comma, or simply a path to an .ico file.</param>
        /// <returns>The Icon at the path if found, or a generic icon if not found.</returns>
        public Icon GetIconFromIconPath(string? iconPath)
        {
            if (iconPath is null || iconPath == string.Empty)
                return _notFoundIcon;

            if (_iconCache.TryGetValue(iconPath, out Icon? icon))
                return icon;

            if (CacheFromIconPath(iconPath))
                return _iconCache[iconPath];
            else
                return _notFoundIcon;
        }

        /// <summary>
        /// Fetches an icon from the cache or file system and returns it as a BitmapSource.
        /// </summary>
        /// <param name="iconPath">Path to the icon file. Can include an icon index or resource identifier 
        /// at the end of the string separated by a comma, or simply a path to an .ico file.</param>
        /// <returns>A BitmapSource from the icon at the path if found, or a generic bitmap if not found.</returns>
        public BitmapSource GetBitmapFromIconPath(string? iconPath)
        {
            if (iconPath is null || iconPath == string.Empty)
                return _notFoundBitmap;

            if (_bitmapCache.TryGetValue(iconPath, out BitmapSource? image))
                return image;

            if (CacheFromIconPath(iconPath))
                return _bitmapCache[iconPath];
            else
                return _notFoundBitmap;
        }

        /// <summary>
        /// Fetches an icon from the cache or application resources and returns it as an Icon.
        /// </summary>
        /// <param name="resourcePath">Path to the resource, which must be an .ico file.</param>
        /// <returns>The Icon at the path.</returns>
        public Icon GetIconFromResourcePath(string resourcePath)
        {
            if (_iconCache.TryGetValue(resourcePath, out Icon? icon))
                return icon;

            CacheFromIconResource(resourcePath);
            return _iconCache[resourcePath];
        }

        /// <summary>
        /// Fetches an icon from the cache or application resources and returns it as a BitmapSource.
        /// </summary>
        /// <param name="resourcePath">Path to the resource, which must be an .ico file.</param>
        /// <returns>A BitmapSource from the icon at the path.</returns>
        public BitmapSource GetBitmapFromResourcePath(string resourcePath)
        {
            if (_bitmapCache.TryGetValue(resourcePath, out BitmapSource? image))
                return image;

            CacheFromIconResource(resourcePath);
            return _bitmapCache[resourcePath];
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
        private static BitmapSource ConvertIconToBitmapSource(Icon icon)
            => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(EditDeviceView.IconSize, EditDeviceView.IconSize));

        /// <summary>
        /// Attempts to fetch an icon from the file system and cache it as both an Icon and a BitmapSource.
        /// </summary>
        /// <param name="iconPath">Path to the icon file. Can include an icon index or resource identifier 
        /// at the end of the string separated by a comma, or simply a path to an .ico file.</param>
        /// <returns>True if successful</returns>
        private bool CacheFromIconPath(string iconPath)
        {
            Icon? icon;
            Match match = IconPathPattern().Match(iconPath);
            try
            {
                if (match.Success)
                    icon = Icon.ExtractIcon(match.Groups[1].Value, int.Parse(match.Groups[2].Value));
                else
                    icon = Icon.ExtractIcon(iconPath, 0);
            }
            catch (Exception)
            {
                return false;
            }

            if (icon is null) return false;
            _iconCache[iconPath] = icon; // NOTE: If we decide not to cache the icons in the future, we must add a call
                                         // to icon.Dispose() after, or else the BitmapSource will keep it active with a reference.
            _bitmapCache[iconPath] = ConvertIconToBitmapSource(icon);
            return true;
        }

        /// <summary>
        /// Fetches an icon from the application resource and caches it as both an Icon and a BitmapSource.
        /// Will throw error if icon is not found.
        /// </summary>
        /// <param name="resourcePath">Path to the resource.</param>
        private void CacheFromIconResource(string resourcePath)
        {
            Uri resourceUri = new(resourcePath, UriKind.Relative);
            StreamResourceInfo streamInfo = App.GetResourceStream(resourceUri) ?? throw new FileNotFoundException("Resource not found", resourcePath);
            using Stream stream = streamInfo.Stream;
            Icon icon = new(stream);
            _iconCache[resourcePath] = icon;
            _bitmapCache[resourcePath] = ConvertIconToBitmapSource(icon);
        }
        #endregion
    }
}
