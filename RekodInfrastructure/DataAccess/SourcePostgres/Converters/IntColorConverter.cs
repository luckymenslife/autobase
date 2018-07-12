using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    [ValueConversion(typeof(Int32), typeof(Color))]
    public class IntColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte blue = (byte)(System.Convert.ToInt64(value) / 65536);
            byte green = (byte)((System.Convert.ToInt64(value) / 256) % 256);
            byte red = (byte)(System.Convert.ToInt64(value) % 256);
            return Color.FromArgb(255, red, green, blue);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(((Color)(value)).R + (((Color)(value)).G << 8) + (((Color)(value)).B << 16));
        }
    }
}