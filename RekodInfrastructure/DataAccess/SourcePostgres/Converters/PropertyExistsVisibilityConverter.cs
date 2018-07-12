using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using AbsM = Rekod.DataAccess.AbstractSource.Model;


namespace Rekod.DataAccess.SourcePostgres.Converters
{
    public class PropertyExistsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;
            AbsM.ETableType tType = (AbsM.ETableType)(value);
            switch (parameter.ToString())
            {
                case "GeometryType":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break; 
                    }
                case "MinObjectSize":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "UseBounds":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "MinScale":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "MaxScale":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "UseGraphicUnits":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "DefaultVisible":
                    {
                        isVisible = (tType == AbsM.ETableType.MapLayer || tType == AbsM.ETableType.View);
                        break;
                    }
                case "IsMapStyle":
                    {
                        isVisible = (tType == AbsM.ETableType.Interval || tType == AbsM.ETableType.Catalog); 
                        break; 
                    }
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
