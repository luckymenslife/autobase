using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model; 

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    /// <summary>
    /// Принимает коллекцию полей PgFieldM, возвращает список названий полей определенного типа. Тип передается параметром конвертеру
    /// </summary>
    [ValueConversion(typeof(ObservableCollection<PgM.PgFieldM>), typeof(IEnumerable<String>))]
    public class FieldsOfTypeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String fieldTypesString = parameter.ToString();
            String[] typesStringArray = fieldTypesString.Split(new[] { '-' });

            List<AbsM.EFieldType> fieldTypes = new List<AbsM.EFieldType>();
            if (typesStringArray.Contains("Integer")) { fieldTypes.Add(AbsM.EFieldType.Integer); }
            if (typesStringArray.Contains("Real")) { fieldTypes.Add(AbsM.EFieldType.Real); }
            if (typesStringArray.Contains("Text")) { fieldTypes.Add(AbsM.EFieldType.Text); }
            if (typesStringArray.Contains("Date")) { fieldTypes.Add(AbsM.EFieldType.Date); }
            if (typesStringArray.Contains("DateTime")) { fieldTypes.Add(AbsM.EFieldType.DateTime); }
            if (typesStringArray.Contains("Geometry")) { fieldTypes.Add(AbsM.EFieldType.Geometry); }

            var coll = from fieldVM
                in (ObservableCollection<AbsM.IFieldM>)(value) 
                where (fieldTypes.Contains(fieldVM.Type))
                       select fieldVM;
            return coll;                 
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); 
        }
    }
}