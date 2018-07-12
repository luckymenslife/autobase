using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Rekod.Controls;
using System.Windows.Media;

namespace Rekod.PrintModule.Behaviors
{
    public class OnDoubleClickEditBehavior: DependencyObject
    {
        private static Dictionary<AutoFocusTextBox, TextBlock> _autoFocusTextBlockDict = new Dictionary<AutoFocusTextBox,TextBlock>();
        private static Dictionary<TextBlock, DateTime> _textBlockLastClickDict = new Dictionary<TextBlock,DateTime>(); 

        public static AutoFocusTextBox GetEditBox(DependencyObject obj)
        {
            return (AutoFocusTextBox)obj.GetValue(EditBoxProperty);
        }

        public static void SetEditBox(DependencyObject obj, AutoFocusTextBox value)
        {
            obj.SetValue(EditBoxProperty, value);
        }

        // Using a DependencyProperty as the backing store for EditBox.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditBoxProperty =
            DependencyProperty.RegisterAttached(
                "EditBox", 
                typeof(AutoFocusTextBox), 
                typeof(OnDoubleClickEditBehavior), 
                new PropertyMetadata(null, new PropertyChangedCallback(OnEditBox_Changed)));

        private static void OnEditBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            textBlock.MouseDown += textBlock_MouseDown;
            AutoFocusTextBox autoFocusTextBox = e.NewValue as AutoFocusTextBox;
            if (autoFocusTextBox != null)
            {
                autoFocusTextBox.LostFocus += autoFocusTextBox_LostFocus;
            }
            _autoFocusTextBlockDict.Add(autoFocusTextBox, textBlock);
        }

        static void textBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            AutoFocusTextBox autoFocusTextBox = textBlock.GetValue(OnDoubleClickEditBehavior.EditBoxProperty) as AutoFocusTextBox;
            if (_textBlockLastClickDict.ContainsKey(textBlock))
            {
                var timeDiff = (DateTime.Now - _textBlockLastClickDict[textBlock]);
                if (timeDiff < new TimeSpan(0, 0, 0, 0, 200))
                {
                    autoFocusTextBox.Visibility = Visibility.Visible;
                    textBlock.Visibility = Visibility.Collapsed;
                    autoFocusTextBox.BorderThickness = new Thickness(1.01);
                    autoFocusTextBox.BorderBrush = Brushes.LightGray;
                    autoFocusTextBox.Background = Brushes.White;
                }
            }
            _textBlockLastClickDict[textBlock] = DateTime.Now;
        }

        static void autoFocusTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AutoFocusTextBox autoFocusTextBox = sender as AutoFocusTextBox;
            if (autoFocusTextBox != null)
            {
                autoFocusTextBox.Visibility = Visibility.Collapsed;
                _autoFocusTextBlockDict[autoFocusTextBox].Visibility = Visibility.Visible;
            }
        }        
    }
}