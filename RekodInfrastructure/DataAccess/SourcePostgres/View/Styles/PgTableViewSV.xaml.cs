using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Rekod.DataAccess.SourcePostgres.View.Styles
{
    public partial class PgTableViewSV
    {
        private void SpButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.ContextMenu.PlacementTarget = button; 
            button.ContextMenu.IsOpen = true;
        }
    }
}