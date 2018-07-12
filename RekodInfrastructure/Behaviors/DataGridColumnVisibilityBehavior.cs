using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls; 

namespace Rekod.Behaviors
{
    public class DataGridColumnVisibilityBehavior: DependencyObject
    {
        private static Dictionary<CheckBox, DataGrid> _checkBoxDataGridAssociation = new Dictionary<CheckBox, DataGrid>(); 

        public static DataGridColumn GetColumnToHide(DependencyObject obj)
        {
            return (DataGridColumn)obj.GetValue(ColumnToHideProperty);
        }
        public static void SetColumnToHide(DependencyObject obj, DataGridColumn value)
        {
            obj.SetValue(ColumnToHideProperty, value);
        }
        public static readonly DependencyProperty ColumnToHideProperty =     
            DependencyProperty.RegisterAttached("ColumnToHide", typeof(DataGridColumn), typeof(DataGridColumnVisibilityBehavior), new UIPropertyMetadata(null));


        public static CheckBox GetCheckBoxToBind(DependencyObject obj)
        {
            return (CheckBox)obj.GetValue(CheckBoxToBindProperty);
        }
        public static void SetCheckBoxToBind(DependencyObject obj, CheckBox value)
        {
            obj.SetValue(CheckBoxToBindProperty, value);
        } public static readonly DependencyProperty CheckBoxToBindProperty =
            DependencyProperty.RegisterAttached("CheckBoxToBind", typeof(CheckBox), typeof(DataGridColumnVisibilityBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(CheckBoxToBindChanged)));
        public static void CheckBoxToBindChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CheckBox)
            {
                CheckBox chBox = e.NewValue as CheckBox;
                if (!_checkBoxDataGridAssociation.ContainsKey(chBox))
                {
                    _checkBoxDataGridAssociation.Add(chBox, (DataGrid)obj);  
                }
                chBox.Checked += new RoutedEventHandler(chBox_Checked);
                chBox.Unchecked += new RoutedEventHandler(chBox_Checked);
            }
        }
        static void chBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chBox = (CheckBox)sender;
            if(_checkBoxDataGridAssociation.ContainsKey(chBox))
            {
                DataGridColumn colToHide = GetColumnToHide(_checkBoxDataGridAssociation[chBox]);
                if ((bool)chBox.IsChecked)
                {
                    colToHide.Visibility = Visibility.Visible;
                }
                else 
                {
                    colToHide.Visibility = Visibility.Collapsed; 
                }
            }
        }
    }
}