using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotificationPlugins.Views
{
    /// <summary>
    /// Логика взаимодействия для NotificationsV.xaml
    /// </summary>
    public partial class NotificationsV : Window
    {
        public NotificationsV()
        {
            InitializeComponent();
        }

        private void NotificationsV_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

    }
}
