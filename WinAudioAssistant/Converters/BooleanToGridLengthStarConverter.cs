using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WinAudioAssistant.Converters
{
    public class BooleanToGridLengthStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            if (value is bool && (bool)value)
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }
            else
            {
                return new System.Windows.GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
