using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using NotificationPlugins.Models;

namespace NotificationPlugins.Infrastructure.Converters
{
    public class PriorityToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.White);

            if (value is NotificationPriorityM)
            {
                var priority = value as NotificationPriorityM;
                var knownColor = priority.WindowsKnownColor;
                var color = System.Drawing.Color.FromKnownColor(knownColor);
                return new SolidColorBrush(new Color(){A=color.A, R = color.R, G = color.G, B = color.B});
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
