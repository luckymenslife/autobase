using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Rekod.Converters
{
    public class BooleansToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Count() > 0 && values[0] is Boolean)
            {
                OperationType operType = (OperationType)Enum.Parse(typeof(OperationType), parameter.ToString(), true);
                bool result = (operType == OperationType.And) ? true : false;
                foreach (object value in values)
                {
                    bool boolValue = (bool)value;
                    if (operType == OperationType.And)
                    {
                        result &= boolValue;
                    }
                    else
                    {
                        result |= boolValue;
                    }
                }
                return result;
            }
            else
            {
                return false; 
            }            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum OperationType
    {
        And, Or
    }
}