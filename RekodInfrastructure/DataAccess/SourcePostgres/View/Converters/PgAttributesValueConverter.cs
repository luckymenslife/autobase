using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rekod.DataAccess.SourcePostgres.Model.PgAttributes;

namespace Rekod.DataAccess.SourcePostgres.View.Converters
{
    /// <summary>
    /// Ищет в коллекции PgAttributeVariantsM подходящее значение
    /// </summary>
    public class PgAttributesValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2)
                return null;
            var variants = values[1] as ObservableCollection<PgAttributeVariantM>;
            if (variants == null)
                return null;
            for (int i = 0; i < variants.Count; i++)
            {
                if (variants[i].CheckValue(values[0]))
                    return variants[i];
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        //public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    var attribute = parameter as PgAttributesM;
        //    if (attribute == null)
        //        throw new ArgumentNullException("parameter");
        //    for (int i = 0; i < attribute.Variants.Count; i++)
        //    {
        //        if (attribute.Variants[i].CheckValue(value))
        //            return attribute.Variants[i];
        //    }
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    var attribute = parameter as PgAttributesM;
        //    if (attribute == null)
        //        throw new ArgumentNullException("parameter");

        //    var variant = value as PgAttributeReferenceM;
        //    if (variant == null)
        //        return null;


        //}
    }
}
