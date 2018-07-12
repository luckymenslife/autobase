using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Rekod.Behaviors
{
    public static class DataGridBehavior
    {
        #region RowNumbers property

        public static readonly DependencyProperty RowNumbersProperty =
            DependencyProperty.RegisterAttached("RowNumbers", typeof(bool), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(false, OnRowNumbersChanged));

        private static void OnRowNumbersChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            DataGrid grid = source as DataGrid;
            if (grid == null)
                return;
            if ((bool)args.NewValue)
            {
                grid.LoadingRow += onGridLoadingRow;
                grid.UnloadingRow += onGridUnloadingRow;
            }
            else
            {
                grid.LoadingRow -= onGridLoadingRow;
                grid.UnloadingRow -= onGridUnloadingRow;
            }
        }

        private static void refreshDataGridRowNumbers(object sender)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null)
                return;

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                    row.Header = row.GetIndex() + 1;
            }
        }

        private static void onGridUnloadingRow(object sender, DataGridRowEventArgs e)
        {
            //e.Row.Header = e.Row.GetIndex() + 1; 
            refreshDataGridRowNumbers(sender);
        }

        private static void onGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            //e.Row.Header = e.Row.GetIndex() + 1; 
            refreshDataGridRowNumbers(sender);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static void SetRowNumbers(DependencyObject element, bool value)
        {
            element.SetValue(RowNumbersProperty, value);
        }

        public static bool GetRowNumbers(DependencyObject element)
        {
            return (bool)element.GetValue(RowNumbersProperty);
        }

        #endregion
    }
}