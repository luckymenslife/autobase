using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rekod.Behaviors
{
    public static class WindowHideOnCloseBehavior
    {
        public static bool GetHideOnClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(HideOnCloseProperty);
        }

        public static void SetHideOnClose(DependencyObject obj, bool value)
        {
            obj.SetValue(HideOnCloseProperty, value);
        }

        // Using a DependencyProperty as the backing store for HideOnClose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideOnCloseProperty =
            DependencyProperty.RegisterAttached(
                "HideOnClose", 
                typeof(bool), 
                typeof(WindowHideOnCloseBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(HideOnCloseProperty_Changed)));

        private static void HideOnCloseProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Window senderWindow = sender as Window;
            if (senderWindow != null)
            {
                senderWindow.Closing += SenderWindow_Closing;
            }
        }

        private static void SenderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window senderWindow = sender as Window; 
            senderWindow.Hide();
            e.Cancel = true; 
        }
    }
}