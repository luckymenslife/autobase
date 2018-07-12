using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Rekod.Converters
{
    public class TrueFalseConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)(value) == true)
            {
                return false;
            }
            else
            {
                return true; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)(value) == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}