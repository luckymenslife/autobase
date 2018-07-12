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
using Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Windows.Threading;
using Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for TableRightsV.xaml
    /// </summary>
    public partial class TableRightsV : UserControl
    {
        public TableRightsV()
        {
            InitializeComponent();
        }
    }
}