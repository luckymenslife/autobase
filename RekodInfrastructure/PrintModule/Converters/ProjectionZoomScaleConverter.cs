using Rekod.PrintModule.RenderComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Rekod.PrintModule.View;
namespace Rekod.PrintModule.Converters
{
    public class ProjectionZoomScaleConverter : DependencyObject, IValueConverter
    {
        public double ProjZoomScaleRel { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ProjZoomScaleRel == 0)
            {
                ProjZoomScaleRel = 1;
            }
            return Math.Round(System.Convert.ToDouble(value) / ProjZoomScaleRel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * ProjZoomScaleRel;
        }
    }
}