using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel; 
using System.Windows;

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    /// <summary>
    /// Определяет видимость вкладки в окне просмотра свойств таблицы
    /// </summary>
    class TableViewTabVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PgM.PgTableBaseM table = (PgM.PgTableBaseM)value;
            if (table == null)
            {
                return Visibility.Collapsed; 
            }
            AbsM.ETableType tabletype = table.Type;
            String par = parameter.ToString();

            switch (par)
            {
                case "Структура":
                case "Свойства":
                    {
                        return Visibility.Visible; 
                    }
                case "Группы":
                case "Подписи":
                case "Стили":
                    {
                        if (tabletype == AbsM.ETableType.MapLayer || tabletype == AbsM.ETableType.View)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed; 
                        }
                    }
                case "Права":
                    {
                        PgM.PgUserM user = (table.Source as PgVM.PgDataRepositoryVM).CurrentUser; 
                        if(user == null || user.Type == PgM.UserType.User)
                        {
                            return Visibility.Collapsed; 
                        }
                        else
                        {
                            return Visibility.Visible; 
                        }
                    }
                case "Индекс":
                    {
                        return Visibility.Visible;
                    }
                default:
                    {
                        return Visibility.Collapsed; 
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}