using System;
using System.Windows;
using System.Windows.Data;

namespace NotificationPlugins.Infrastructure.Converters
{
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                String param = parameter.ToString();
                if (param == "Reverse")
                {
                    return (bool)value ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                String param = parameter.ToString();
                if (param == "Reverse")
                {
                    return ((Visibility)value == Visibility.Visible) ? false : true;
                }
                else
                {
                    return ((Visibility)value == Visibility.Visible) ? true : false;
                }
            }
            else
            {
                return ((Visibility)value == Visibility.Visible) ? true : false;
            }
        }
    }
}
