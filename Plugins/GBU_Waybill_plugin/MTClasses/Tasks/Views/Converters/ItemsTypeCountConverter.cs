using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace GBU_Waybill_plugin.MTClasses.Views.Converters
{
    /// <summary>
    /// Конвертирует значение EStatusTask в значение SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(IEnumerable<TaskInTableM>), typeof(int))]
    public class ItemsTypeCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //switch (parameter)
            //{IEnumerable<TaskInTableM> 

            //}

            //var performedBrush = new SolidColorBrush(Color.FromArgb(255, 161, 208, 120));
            //var assignedBrush = new SolidColorBrush(Color.FromArgb(255, 118, 198, 222));
            //var inprogressBrush = new SolidColorBrush(Color.FromArgb(255, 220, 150, 68));
            //var overdueBrush = new SolidColorBrush(Color.FromArgb(255, 252, 93, 93));
            //SolidColorBrush result = performedBrush;
            
            int result = 0;
            var tasks = (value as ReadOnlyObservableCollection<object>).Cast<TaskInTableM>();
            try
            {

                if (parameter is string)
                {
                    switch (parameter as string)
                    {
                        case "assigned":
                            result = tasks.Count(x => x.Status == EStatusTask.assigned);
                            break;
                        case "in_progress":
                            result = tasks.Count(x => x.Status == EStatusTask.in_progress);
                            break;
                        case "overdue":
                            result = tasks.Count(x => x.Status == EStatusTask.overdue);
                            break;
                        default:
                        case "performed":
                            result = tasks.Count(x => x.Status == EStatusTask.performed);
                            break;
                    }
                    return result;
                }
            }
            catch {}
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }
}