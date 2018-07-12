using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.Windows.Controls;

namespace Rekod.AttachedProperties
{
    public class propDataGrid
    {
        public static ObservableCollection<DataGridColumn> GetGenerateColumns(DependencyObject obj)
        {
            return (ObservableCollection<DataGridColumn>)obj.GetValue(GenerateColumnsProperty);
        }

        public static void SetGenerateColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value)
        {
            obj.SetValue(GenerateColumnsProperty, value);
        }

        // Using a DependencyProperty as the backing store for GenerateColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GenerateColumnsProperty =
            DependencyProperty.RegisterAttached(
                    "GenerateColumns",
                    typeof(ObservableCollection<HeaderValue>),
                    typeof(propDataGrid),
                    new PropertyMetadata(null,
                        new PropertyChangedCallback(OnSelectedItemsChanged)));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datagrid = d as DataGrid;
            if (datagrid == null)
                return;

            var coll = e.NewValue as ObservableCollection<HeaderValue>;
            if (coll == null)
                return;
            GetGridColumns(datagrid.Columns, coll);
            coll.CollectionChanged += (sender, e2) =>
            {
                GetGridColumns(datagrid.Columns, coll);
            };
        }
        private static void GetGridColumns(ObservableCollection<DataGridColumn> coll, ObservableCollection<HeaderValue> values)
        {
            var listGridColumns = new ObservableCollection<DataGridColumn>();
            foreach (var item in values)
            {
                var gridColumn = FindGridColumn(coll, item);
                if (gridColumn == null)
                    gridColumn = CreateColumn(item);
                gridColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                listGridColumns.Add(gridColumn);
            }
            if (listGridColumns.Count > 0)
            {
                listGridColumns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
            ExtraFunctions.Sorts.SortList(coll, listGridColumns);
        }
        private static DataGridColumn FindGridColumn(ObservableCollection<DataGridColumn> coll, HeaderValue value)
        {
            return coll.FirstOrDefault(f => f.Header == value.Header);
        }
        private static DataGridColumn CreateColumn(HeaderValue value)
        {
            //todo Sergey: Создать колонки нужного типа
            AbsM.IFieldM field = value.Header as AbsM.IFieldM;
            System.Windows.Data.BindingBase bindBase = null;

            switch (field.Type)
            {
                case Rekod.DataAccess.AbstractSource.Model.EFieldType.Date:
                    {
                        bindBase = new Binding(string.Format("[{0}]", value.Name)) { StringFormat = "yyyy-MM-dd" };
                        break;
                    }
                case Rekod.DataAccess.AbstractSource.Model.EFieldType.DateTime:
                    {
                        bindBase = new Binding(string.Format("[{0}]", value.Name)) { StringFormat = "yyyy-MM-dd HH:mm:ss" };
                        break;
                    }
                default:
                    {
                        bindBase = new Binding(string.Format("[{0}]", value.Name));
                        if (value.IsReadOnly)
                        {
                            (bindBase as Binding).Mode = BindingMode.OneWay;
                        }
                        break;
                    }
            }

            return new DataGridTextColumn
            {
                Header = field.Text,
                Binding = bindBase,
                SortMemberPath = value.Name
            };
        }
    }
    public class HeaderValue : ViewModelBase
    {
        string _name;
        object _header;
        private bool _isReadOnly;

        /// <summary>
        /// Название биндинга в базе
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        /// <summary>
        /// Заголовок 
        /// </summary>
        public object Header
        {
            get { return _header; }
            set { OnPropertyChanged(ref _header, value, () => this.Header); }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { OnPropertyChanged(ref _isReadOnly, value, () => this.IsReadOnly); }
        }
    }
}