using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rekod.Behaviors
{
    public class TreeViewRightClickBehavior: DependencyObject
    {

        public static bool GetSelectOnRightClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectOnRightClickProperty);
        }

        public static void SetSelectOnRightClick(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectOnRightClickProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectOnRightClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectOnRightClickProperty =
            DependencyProperty.RegisterAttached("SelectOnRightClick", typeof(bool), typeof(TreeViewRightClickBehavior), new UIPropertyMetadata(false, new PropertyChangedCallback(RevolveChangedCallback)));

        public static void RevolveChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = obj as TreeView;
            if (treeView != null)
            {
                treeView.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(treeView_PreviewMouseRightButtonDown);
            }
        }

        static void treeView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);
            TreeViewItem treeViewItem = source as TreeViewItem;

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
    }
}