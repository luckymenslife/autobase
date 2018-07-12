using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Globalization;
using Rekod.Controllers;
using Rekod.Model;

namespace Rekod.Converters
{
    class AttributesOfObjectConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string name = parameter as string;
            Debug.Assert(values.Length == 2, Rekod.Properties.Resources.ErrorArraySize);

            var attribute = values[1] as AttributesOfObjectModel;
            if (attribute == null) return null;

            var refValue = ViewModel.AttributesOfObjectViewModel.GetRefValue(attribute);
            if (refValue != null)
                switch (name)
                {
                    case "reference":
                        return string.Format("{0}",refValue.NewValue);
                    case "interval":
                    return string.Format("({0})", refValue.NewValue);
                    default :
                        return refValue;
                }
            else
            {
                switch (name)
                {
                    case "interval":
                        return string.Format("({0})", Rekod.Properties.Resources.OutsideDiapason);
                    default:
                        return refValue;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
