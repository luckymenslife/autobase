using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;


namespace Rekod.Converters
{
    [ValueConversion(typeof(String), typeof(Boolean))]
    public class YesNoBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            if ((String)(value) == Rekod.Properties.Resources.LocYes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if ((bool)(value))
            {
                return Rekod.Properties.Resources.LocYes;
            }
            else
            {
                return Rekod.Properties.Resources.LocNo;
            }
        }
    }
}