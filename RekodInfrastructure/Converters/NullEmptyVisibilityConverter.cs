using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Rekod.Converters
{
    public class NullEmptyVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result = Visibility.Collapsed;
            if (value != null)
            {
                if (value is String)
                {
                    String valStr = value.ToString();
                    if (!String.IsNullOrEmpty(valStr))
                    {
                        result = Visibility.Visible;
                    }
                }
                else
                {
                    result = Visibility.Visible;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
