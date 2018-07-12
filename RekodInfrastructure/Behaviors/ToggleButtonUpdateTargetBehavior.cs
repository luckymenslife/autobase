using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Rekod.Behaviors
{
    public class ToggleButtonUpdateTargetBehavior
    {
        public static bool GetUpdateTargetOnCheckedChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateTargetOnCheckedChangedProperty);
        }

        public static void SetUpdateTargetOnCheckedChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateTargetOnCheckedChangedProperty, value);
        }

        // Using a DependencyProperty as the backing store for UpdateTargetOnCheckedChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateTargetOnCheckedChangedProperty =
            DependencyProperty.RegisterAttached(
                "UpdateTargetOnCheckedChanged",
                typeof(bool),
                typeof(ToggleButtonUpdateTargetBehavior),
                new UIPropertyMetadata(false, new PropertyChangedCallback(UpdateTargetOnCheckedChangedCallback)));

        public static void UpdateTargetOnCheckedChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton toggleButton = obj as ToggleButton;
            toggleButton.Checked += new RoutedEventHandler(toggleButton_CheckedChanged);
            toggleButton.Unchecked += new RoutedEventHandler(toggleButton_CheckedChanged);
        }

        static void toggleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            BindingExpression binding = toggleButton.GetBindingExpression(ToggleButton.IsCheckedProperty);
            binding.UpdateTarget();
        }
    }
}