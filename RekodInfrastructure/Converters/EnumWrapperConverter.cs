using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Rekod.DataAccess.SourcePostgres.View.ConfigView; 

namespace Rekod.Converters
{
    public class EnumWrapperConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            EnumWrapper enumWrapper = null;
            List<EnumWrapper> enumValues = ObjectProviderValues.GetEnumValues(value.GetType());
            var foundList = from EnumWrapper wrap in enumValues where (wrap.Value.Equals(value)) select wrap;
            if (foundList.Count() == 1)
            {
                enumWrapper = foundList.ElementAt(0); 
            }
            return enumWrapper; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((EnumWrapper)(value)).Value; 
        }
    }
}