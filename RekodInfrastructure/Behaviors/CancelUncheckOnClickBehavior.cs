using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Rekod.Behaviors
{
    public static class CancelUncheckOnClickBehavior
    {
        public static bool GetCancelUncheck(DependencyObject obj)
        {
            return (bool)obj.GetValue(CancelUncheckProperty);
        }

        public static void SetCancelUncheck(DependencyObject obj, bool value)
        {
            obj.SetValue(CancelUncheckProperty, value);
        }

        // Using a DependencyProperty as the backing store for CancelUncheck.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelUncheckProperty =
            DependencyProperty.RegisterAttached("CancelUncheck", typeof(bool), typeof(CancelUncheckOnClickBehavior), new PropertyMetadata(false, new PropertyChangedCallback(OnCancelUncheckChanged)));
      
        private static void OnCancelUncheckChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton tb = obj as ToggleButton;
            tb.PreviewMouseDown += tb_PreviewMouseDown;
        }

        static void tb_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            if (tb.IsChecked == true)
            {
                e.Handled = true;
            }
        }     
    }
}