﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace Rekod.Controls
{
    /// <summary>
    /// Interaction logic for CheckObjects.xaml
    /// </summary>
    public partial class CheckObjects : UserControl
    {
        private DataTable _dataTable;
        private String _idColumnName;
        private String _idColumnCaption;
        private Boolean _cancelled = true;
        private Window _ownerWindow = null;
        public List<int> CheckedList = null;
        public Boolean Cancelled
        {
            get
            {
                return _cancelled; 
            }
            set
            {
                _cancelled = value; 
            }
        }
        /// <summary>
        /// Принимает объекты в таблице DataTable, устанавливает внутреннюю коллекцию отмеченных элементов в delev
        /// </summary>
        /// <param name="dt">Таблица</param>
        /// <param name="defChecked">Отмеченные по умолчанию объекты</param>
        /// <param name="idColumnName">Название колонки идентификатора объектов</param>
        /// <param name="ownerWindow">Если не null, закрывает окно переданное в параметре при нажатии кнопок "Сохранить" и "Отмена"</param>
        public CheckObjects(DataTable dt, List<int> defChecked, String idColumnName, Window ownerWindow = null)
        {
            if (defChecked == null)
            {
                defChecked = new List<int>(); 
            }
            Cancelled = true;
            InitializeComponent();
            _dataTable = dt;
            _ownerWindow = ownerWindow;
            _idColumnName = idColumnName;
            _idColumnCaption = dt.Columns[idColumnName].Caption;
            
            CountBlock.Text = defChecked == null ? "0": defChecked.Count.ToString();

            DataColumn dc = new DataColumn(Rekod.Properties.Resources.SelectObjects, typeof(Boolean));
            dt.Columns.Add(dc);
            dc.SetOrdinal(0);

            MainGrid.ItemsSource = dt.DefaultView;
            MainGrid.AutoGenerateColumns = true;

            foreach (DataRow dr in dt.Rows)
            {
                if (defChecked.Contains((int)(dr[idColumnName])))
                {
                    dr[dc] = true;
                }
                else
                {
                    dr[dc] = false;
                }
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            filterRows();
        }
        private void filterRows()
        {
            DataView defView = _dataTable.DefaultView;

            int count = defView.Count;
            for (int i = 0; i < count; i++)
            {
                DataRowView drv = defView[i];
                DataGridRow dgr = MainGrid.ItemContainerGenerator.ContainerFromItem(drv) as DataGridRow;
                if (dgr != null)
                {
                    bool contains = false;
                    for (int j = 1; j < _dataTable.Columns.Count; j++)
                    {
                        if (_dataTable.Columns[j].ColumnName == _idColumnName)
                        {
                            continue;
                        }
                        if (drv[j].ToString().ToUpper().Contains(textBox.Text.ToUpper()))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        dgr.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        dgr.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            CheckedList = new List<int>();

            foreach (DataRow dr in _dataTable.Rows)
            {
                if (((bool)(dr[0])) == true)
                {
                    CheckedList.Add((int)(dr[_idColumnName]));
                }
            }
            Cancelled = false; 
            if (_ownerWindow != null)
            {
                _ownerWindow.Close(); 
            }
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            CheckedList = null;
            Cancelled = true; 
            if (_ownerWindow != null)
            {
                _ownerWindow.Close();
            }
        }
        private void MainGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            int i = 0;
            int colcount = MainGrid.Columns.Count;
            foreach (DataGridColumn dgc in MainGrid.Columns)
            {
                dgc.CanUserReorder = false;
                if (dgc.Header.ToString() == _idColumnCaption)
                {
                    dgc.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (i != 0)
                    {
                        dgc.IsReadOnly = true;
                        dgc.Header = _dataTable.Columns[dgc.Header.ToString()].Caption;
                    }
                    else
                    {
                        DataGridCheckBoxColumn dgcbc = (DataGridCheckBoxColumn)(dgc);
                        (dgcbc.Binding as Binding).UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        dgcbc.CellStyle = (Style)(MainGrid.Resources["SingleClickEditing"]);
                    }
                    if (i == colcount - 1)
                    {
                        dgc.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                    }
                }
                i++;
            }
            _dataTable.ColumnChanged += new DataColumnChangeEventHandler(dataTable_ColumnChanged);
        }
        void dataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            int count = 0;
            foreach (DataRow dr in _dataTable.Rows)
            {
                if ((bool)(dr[0]) == true)
                {
                    count++;
                }
            }
            CountBlock.Text = count.ToString();
        }
        private void MainGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            textBox.Text = "";
        }
        //
        // SINGLE CLICK EDITING
        //
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}