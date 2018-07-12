using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Rekod.Behaviors
{
    public class TabItemDisabledBehavior : DependencyObject
    {
        public static object GetRevolve(DependencyObject obj)
        {
            return (object)obj.GetValue(RevolveProperty);
        }

        public static void SetRevolve(DependencyObject obj, object value)
        {
            obj.SetValue(RevolveProperty, value);
        }

        // Using a DependencyProperty as the backing store for Revolve.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RevolveProperty =
            DependencyProperty.RegisterAttached("Revolve", typeof(object), typeof(TabItemDisabledBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(RevolveChangedCallback)));

        public static void RevolveChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TabItem tabItem = obj as TabItem;
            tabItem.IsEnabledChanged += new DependencyPropertyChangedEventHandler(tabItem_IsEnabledChanged);
        }

        static void tabItem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TabItem tabItem = sender as TabItem;
            if (!tabItem.IsEnabled)
            {
                TabControl tabControl = tabItem.Parent as TabControl;
                bool revolved = false; 
                if (tabControl != null)
                {
                    foreach (object item in tabControl.Items)
                    {
                        if (item is TabItem)
                        {
                            tabItem = item as TabItem;
                        }
                        else
                        {
                            tabItem = tabControl.ItemContainerGenerator.ContainerFromItem(item) as TabItem;
                        }
                        if (tabItem != null && tabItem.IsEnabled)
                        {
                            tabItem.IsSelected = true;
                            revolved = true;
                            break; 
                        }
                    }
                }

                if (!revolved)
                {
                    object item = tabControl.Items[0];
                    if (item is TabItem)
                    {
                        tabItem = item as TabItem;
                    }
                    else
                    {
                        tabItem = tabControl.ItemContainerGenerator.ContainerFromItem(item) as TabItem;
                    }
                    tabItem.IsSelected = true;
                }
            }
        }
    }
}