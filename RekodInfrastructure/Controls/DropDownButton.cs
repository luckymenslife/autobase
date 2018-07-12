using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Rekod.Controls
{
    public class DropDownButton : ToggleButton
    {
        public static readonly DependencyProperty DropDownProperty =
            DependencyProperty.Register(
            "DropDown",
            typeof(ContextMenu),
            typeof(DropDownButton),
            new UIPropertyMetadata(null));

        public DropDownButton()
        {
            this.SetBinding(
                IsCheckedProperty,
                new Binding("DropDown.IsOpen") { Source = this });
        }

        public ContextMenu DropDown
        {
            get { return (ContextMenu)GetValue(DropDownProperty); }
            set { SetValue(DropDownProperty, value); }
        }

        protected override void OnClick()
        {
            if (DropDown != null)
            {
                DropDown.PlacementTarget = this;
                DropDown.Placement = PlacementMode.Bottom;
                this.OnToggle();
            }
        }
    }
}