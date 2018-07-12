using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Rekod.PrintModule.RenderComponents;

namespace Rekod.PrintModule.Converters
{
    class EnumBooleanConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CanvasMode moveTypeParam = (CanvasMode)Enum.Parse(typeof(CanvasMode), parameter.ToString());
            CanvasMode moveTypeValue = (CanvasMode)value;
            return (moveTypeParam == moveTypeValue);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CanvasMode moveTypeParam = CanvasMode.None; 
            if ((bool)value == true)
            {
                moveTypeParam = (CanvasMode)Enum.Parse(typeof(CanvasMode), parameter.ToString());
            }
            return moveTypeParam; 
        }
    }
}