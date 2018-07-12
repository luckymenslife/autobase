using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using System.Windows.Media;

namespace GBU_Waybill_plugin.MTClasses.Views.Converters
{
    /// <summary>
    /// Конвертирует значение EStatusTask в значение SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(EStatusTask), typeof(SolidColorBrush))]
    public class StatusTaskSolidBrushConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            var performedBrush = new SolidColorBrush(Color.FromArgb(255, 161, 208, 120));
            var assignedBrush = new SolidColorBrush(Color.FromArgb(255, 118, 198, 222));
            var inprogressBrush = new SolidColorBrush(Color.FromArgb(255, 220, 150, 68));
            var overdueBrush = new SolidColorBrush(Color.FromArgb(255, 252, 93, 93));
            SolidColorBrush result = performedBrush;

            if (value is EStatusTask)
            {
                switch ((EStatusTask)value)
                {
                    case EStatusTask.assigned:
                        result = assignedBrush;
                        break;
                    case EStatusTask.in_progress:
                        result = inprogressBrush;
                        break;
                    case EStatusTask.overdue:
                        result = overdueBrush;
                        break;
                    default:
                    case EStatusTask.performed:
                        result = performedBrush;
                        break;
                }
                return result;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }
}