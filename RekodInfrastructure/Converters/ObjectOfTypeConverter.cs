using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Rekod.Converters
{
    /// <summary>
    /// Возвращает true, если переданный объект соответствует типу, который передается в параметре. Иначе false
    /// </summary>
    [ValueConversion(typeof(Object), typeof(Boolean))]
    public class ObjectOfTypeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                String paramTypeName = parameter.ToString();
                String valueTypeName = value.GetType().ToString();

                if (paramTypeName == valueTypeName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}