using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Rekod.DBHistory
{
    public class ChooseTextBox : TextBox
    {
        bool downAtMouse = false;
        public Object HyperlinkContent
        {
            get { return base.GetValue(HyperlinkContentProperty); }
            set { base.SetValue(HyperlinkContentProperty, value); }
        }
        public static readonly DependencyProperty HyperlinkContentProperty =
          DependencyProperty.Register("HyperlinkContent", typeof(object), typeof(ChooseTextBox));

        public bool HyperlinkEnabling
        {
            get { return (bool)base.GetValue(HyperlinkEnablingProperty); }
            set { base.SetValue(HyperlinkEnablingProperty, (bool)value); }
        }
        public static readonly DependencyProperty HyperlinkEnablingProperty =
          DependencyProperty.Register("HyperlinkEnabling", typeof(object), typeof(ChooseTextBox));

        public static readonly RoutedEvent HLClickEvent = EventManager.RegisterRoutedEvent(
            "HLClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChooseTextBox));

        // Provide CLR accessors for the event
        public event RoutedEventHandler HLClick
        {
            add { AddHandler(HLClickEvent, value); }
            remove { RemoveHandler(HLClickEvent, value); }
        }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            var child = VisualTreeHelper.GetChild(feSource, 0);
            List<TextBlock> mb = FindChild<TextBlock>(child, "hlTb");
            if (mb.Count == 1)
            {
                TextBlock b = mb[0];
                var p = System.Windows.Input.Mouse.GetPosition(b);
                if (p.X <= 0 || p.Y <= 0 || p.X >= b.ActualWidth || p.Y >= b.ActualHeight) return;
                downAtMouse = true;
            }
        }

        protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            var child = VisualTreeHelper.GetChild(feSource, 0);
            List<TextBlock> mb = FindChild<TextBlock>(child, "hlTb");
            if (mb.Count == 1)
            {
                TextBlock b = mb[0];
                var p = System.Windows.Input.Mouse.GetPosition(b);
                if (!b.IsEnabled || p.X <= 0 || p.Y <= 0 || p.X >= b.ActualWidth || p.Y >= b.ActualHeight || downAtMouse == false) return;
                RaiseEvent(new RoutedEventArgs(ChooseTextBox.HLClickEvent));
            }
            downAtMouse = false;
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            downAtMouse = false;
        }

        public static List<T> FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            List<T> foundChild = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild.AddRange(FindChild<T>(child, childName));
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild.Add((T)child);
                    }
                }
                else
                {
                    // child element found.
                    foundChild.Add((T)child);
                    break;
                }
            }

            return foundChild;
        }
    }
}
