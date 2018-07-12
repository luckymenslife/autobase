using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;

namespace Rekod.Controls
{
    public class AutoFocusTextBox:TextBox
    {
        public AutoFocusTextBox()
            : base()
        {
            GotFocus += new System.Windows.RoutedEventHandler(AutoFocusTextBox_GotFocus);
            LostFocus += new System.Windows.RoutedEventHandler(AutoFocusTextBox_LostFocus);
            MouseLeftButtonDown += new MouseButtonEventHandler(AutoFocusTextBox_MouseLeftButtonDown);
            BorderBrush = Brushes.Transparent;
            Background = Brushes.Transparent;
            KeyDown += new System.Windows.Input.KeyEventHandler(AutoFocusTextBox_KeyDown);
            Padding = new Thickness(0); 
            VerticalAlignment = System.Windows.VerticalAlignment.Center; 
        }

        void AutoFocusTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectAll(); 
        }

        void AutoFocusTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BindingExpression bindExpression = GetBindingExpression(TextBox.TextProperty);
                bindExpression.UpdateSource();
                // Move to a parent that can take focus
                FrameworkElement parent = (FrameworkElement)this.Parent;
                while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
                {
                    parent = (FrameworkElement)parent.Parent;
                }

                DependencyObject scope = FocusManager.GetFocusScope(this);
                FocusManager.SetFocusedElement(scope, parent as IInputElement);
            }
        }

        void AutoFocusTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //BindingExpression bindExpression = GetBindingExpression(TextBox.TextProperty);
            //if (bindExpression.HasError)
            //{
            //    bindExpression.UpdateTarget();
            //}
            //else
            //{
            //    bindExpression.UpdateSource();
            //}
        }

        void AutoFocusTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //SelectAll(); 
        }
    }
}