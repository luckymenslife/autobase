using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Rekod.Behaviors
{
    public static class OnLoadedSelectFirstBehavior
    {
        public static bool GetSelectFirstOnLoaded(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectFirstOnLoadedProperty);
        }

        public static void SetSelectFirstOnLoaded(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectFirstOnLoadedProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectFirstOnLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectFirstOnLoadedProperty =
            DependencyProperty.RegisterAttached(
                    "SelectFirstOnLoaded", 
                    typeof(bool), 
                    typeof(OnLoadedSelectFirstBehavior), 
                    new PropertyMetadata(false, new PropertyChangedCallback(SelectFirstOnLoadedChanged))
            );

        public static void SelectFirstOnLoadedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = sender as Selector;
            if ((bool)e.NewValue == true)
            {
                selector.Loaded += selector_Loaded;
            }
            else
            {
                selector.Loaded -= selector_Loaded;
            }
        }

        static void selector_Loaded(object sender, RoutedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector.Items.Count > 0)
            {
                selector.SelectedIndex = 0;
            }
        }
    }
}