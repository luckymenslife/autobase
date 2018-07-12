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

namespace Rekod.DataAccess.SourcePostgres.View.History
{
    /// <summary>
    /// Interaction logic for PgHistoryV.xaml
    /// </summary>
    public partial class PgHistoryV : Window
    {
        public PgHistoryV(PgHistoryVM pgHistoryVM)
        {
            this.DataContext = pgHistoryVM;
            pgHistoryVM.AttachedWindow = this;
            InitializeComponent();
        }
    }
}