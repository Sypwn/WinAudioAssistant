using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WinAudioAssistant.Converters
{
    /// <summary>
    /// Converter to allow a grid column/row to have a width/height of "*" when a specified boolean is true, and "0" when false.
    /// Will probably refactor UI code later to make this unnecessary.
    /// </summary>
    public class BooleanToGridLengthStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return value is bool boolValue && boolValue
                ? new System.Windows.GridLength(1, System.Windows.GridUnitType.Star)
                : (object)new System.Windows.GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
