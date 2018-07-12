using Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Converters
{
    public class DbDataTypeToSystemTypeConverter : IValueConverter
    {
        private static SortedList<string, ERDataType> typesDictionary = new SortedList<string, ERDataType>()
        {
            {"bigint", ERDataType.Integer}, 
            {"bigserial", ERDataType.Integer},
            {"smallint", ERDataType.Integer},
            {"serial", ERDataType.Integer},
            {"integer", ERDataType.Integer},
            {"character", ERDataType.Text},
            {"double", ERDataType.Numeric},
            {"double precision", ERDataType.Numeric},
            {"numeric", ERDataType.Numeric},
            {"real", ERDataType.Numeric},
            {"date", ERDataType.Date},
            {"timestamp without time zone", ERDataType.DateTime},
            {"timestamp with time zone", ERDataType.DateTime},
            {"character varying", ERDataType.Text},
            {"bit", ERDataType.Text},
            {"bit varying", ERDataType.Text},
            {"text", ERDataType.Text},
            {"geometry", ERDataType.Geometry}
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            String colType = value.ToString().ToLower();
            if (colType == "user-defined") colType = "geometry";
            else if (colType == "timestamp without time zone")
                colType = "timestamp with time zone";
            if (typesDictionary.ContainsKey(colType))
            {
                return typesDictionary[colType];
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}