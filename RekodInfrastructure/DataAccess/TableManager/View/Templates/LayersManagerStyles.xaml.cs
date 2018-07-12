using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rekod.DataAccess.TableManager.View.Templates
{
    public partial class LayersManagerStyles
    {
        private void BindedObjectStateChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chBox = sender as CheckBox;
            BindingExpression binding = chBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateTarget();
        }
    }
}