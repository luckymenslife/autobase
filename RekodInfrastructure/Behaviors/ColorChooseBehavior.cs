using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Rekod.Behaviors
{
    public static class ColorChooseBehavior
    {
        // Using a DependencyProperty as the backing store for ChooseColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChooseColorProperty =
            DependencyProperty.RegisterAttached("ChooseColor", typeof(bool), typeof(ColorChooseBehavior), new UIPropertyMetadata(false, new PropertyChangedCallback(OnChooseColorChanged)));

        public static bool GetChooseColor(DependencyObject obj)
        {
            return (bool)obj.GetValue(ChooseColorProperty);
        }

        public static void SetChooseColor(DependencyObject obj, bool value)
        {
            obj.SetValue(ChooseColorProperty, value);
        }

        private static void OnChooseColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Rectangle rect = obj as Rectangle;
            if ((bool)e.NewValue)
            {
                rect.MouseDown += new System.Windows.Input.MouseButtonEventHandler(rect_MouseDown);
            }
            else 
            {
                rect.MouseDown -= new System.Windows.Input.MouseButtonEventHandler(rect_MouseDown);
            }
        }

        static void rect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle; 
            if (rect != null)
            {
                using (System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog())
                {
                    var rectColor = (rect.Fill as SolidColorBrush).Color;
                    System.Drawing.Color formColor = System.Drawing.Color.FromArgb(rectColor.R, rectColor.G, rectColor.B);
                    colorDialog.Color = formColor;
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        (rect.Fill as SolidColorBrush).Color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                    }
                }
            }
        }
    }
}