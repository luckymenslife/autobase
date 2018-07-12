﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.TableManager.View.Templates
{
    public partial class VisibleLayersTreeViewTemplate
    {
        private void BindedObjectStateChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chBox = sender as CheckBox;
            BindingExpression binding = chBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateTarget();
        }
        void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView.SelectedItem is AbsM.ITableBaseM)
            {
                Program.TablesManager.SelectedLayer = treeView.SelectedItem as AbsM.TableBaseM;
            }
        }
    }
}
