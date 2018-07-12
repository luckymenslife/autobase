using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Принимает в качестве DataContext объект Rekod.Services.BindingProxy у которого свойство Data установлено в объект PgUserM
    /// </summary>
    public partial class UserConfigV : UserControl
    {
        public UserConfigV()
        {
            InitializeComponent();
        }
    }
}