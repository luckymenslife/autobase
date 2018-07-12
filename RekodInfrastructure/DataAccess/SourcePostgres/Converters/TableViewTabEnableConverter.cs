using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    class TableViewTabEnableConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string tabname = parameter.ToString(); 
            bool isNewTable = System.Convert.ToBoolean(value);
            if (tabname != "Свойства" && isNewTable)
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
            throw new NotImplementedException();
        }
    }
}
