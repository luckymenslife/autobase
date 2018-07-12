using System;
using System.Collections.Generic;
using System.Data;
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
using PgTV_VM = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using System.ComponentModel;
using System.Windows.Threading;

namespace Rekod.DataAccess.SourcePostgres.View.TableView
{
    /// <summary>
    /// Interaction logic for TableViewV.xaml
    /// </summary>
    public partial class PgTableViewV : UserControl
    {
        public PgTableViewV()
        {
            InitializeComponent();
        }
    }
}