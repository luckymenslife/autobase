using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Drawing;
using sp = Rekod.DataAccess.SourcePostgres; 

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    [ValueConversion(typeof(Int32), typeof(BitmapSource))]
    public class PenTypeBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Bitmap bmp = global::Rekod.lineRes.ResourceManager.GetObject(string.Format("LINE{0:00}", value)) as Bitmap;
            if (bmp == null)
            {
                return null; 
            }
            BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(), IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            return bmpSource;            
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return sp.View.ConfigView.ObjectProviderValues.Pens.IndexOf(value as BitmapSource) + 1; 
        }
    }
}