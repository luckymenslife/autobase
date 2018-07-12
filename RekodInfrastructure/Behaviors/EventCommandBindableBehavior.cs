using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rekod.Behaviors
{
    public class EventCommandBindableBehavior: DependencyObject
    {
        public static object GetBindableCommand(DependencyObject obj)
        {
            return (object)obj.GetValue(BindableCommandProperty);
        }

        public static void SetBindableCommand(DependencyObject obj, object value)
        {
            obj.SetValue(BindableCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for BindableObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindableCommandProperty =
            DependencyProperty.RegisterAttached("BindableCommand", typeof(object), typeof(EventCommandBindableBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(BindableCommandChangedCallback)));

        public static void BindableCommandChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TreeView && e.NewValue is ICommand)
            {
                TreeView tv = obj as TreeView;
                ICommand command = e.NewValue as ICommand; 
                tv.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(
                (sender, args) =>
                {
                    if (command.CanExecute(tv.SelectedItem))
                    {
                        command.Execute(tv.SelectedItem);
                    }
                }); 
            }
            else if (obj is Window && e.NewValue is ICommand)
            {
                Window windowTarget = obj as Window;
                ICommand command = e.NewValue as ICommand; 
                windowTarget.Closed += new EventHandler(
                (sender, args) =>
                {
                    if (command.CanExecute(null))
                    {
                        command.Execute(true); 
                    }
                });
            }
        }
    }
}