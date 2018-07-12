using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Rekod.Converters
{
    class CollectionViewConverter: IValueConverter
    {
        /// <summary>
        /// Возвращает новый экземпляр ICollectionView для коллекции
        /// </summary>
        /// <param name="parameter">Если задано - указывает свойство по которому нужно сортировать</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CollectionViewSource collViewSource = new CollectionViewSource();
            collViewSource.Source = value;
            if (parameter != null && parameter is String)
            {
                SortDescription sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);
                collViewSource.View.SortDescriptions.Add(sort);
            }
            return collViewSource.View;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}