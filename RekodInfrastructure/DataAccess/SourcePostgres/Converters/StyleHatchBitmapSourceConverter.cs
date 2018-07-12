using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using sp = Rekod.DataAccess.SourcePostgres; 

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    public class StyleHatchBitmapSourceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == System.Windows.DependencyProperty.UnsetValue || values[1] == System.Windows.DependencyProperty.UnsetValue)
            {
                return null; 
            }
            int style = System.Convert.ToInt32(values[0]);
            int hatch = System.Convert.ToInt32(values[1]);
            BitmapSource bmpSource = null;
            if (sp.View.ConfigView.ObjectProviderValues.StyleHatchToBitmapSource.ContainsKey(new sp.View.ConfigView.StyleHatch(style, hatch)))
            {
                bmpSource =
                sp.View.ConfigView.ObjectProviderValues.StyleHatchToBitmapSource[new sp.View.ConfigView.StyleHatch(style, hatch)];
            }

            return bmpSource; 
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapSource bmpSource = value as BitmapSource;

            if (bmpSource != null 
                && sp.View.ConfigView.ObjectProviderValues.BitmapSourceToStyleHatch.ContainsKey(bmpSource))
            {
                sp.View.ConfigView.StyleHatch styleHatch =
                    sp.View.ConfigView.ObjectProviderValues.BitmapSourceToStyleHatch[bmpSource];
                return new object[] { styleHatch.Style, styleHatch.Hatch };
            }
            else
            {
                return null; 
            }
        }
    }
}