using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Rekod.DataAccess.TableManager.Converters
{
    public class CursorEqualsCurrentCursorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            mvMapLib.Cursors cursor = (mvMapLib.Cursors)(Enum.Parse(typeof(mvMapLib.Cursors), (String)(parameter)));
            if ((mvMapLib.Cursors)(value) == cursor)
            {
                return Brushes.Gray; 
            }
            else
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}