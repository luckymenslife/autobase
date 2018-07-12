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
using Rekod.DBTablesEdit;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for LabelStyleView.xaml
    /// </summary>
    public partial class LabelStyleView : UserControl
    {
        public LabelStyleView()
        {
            InitializeComponent();
        }
    }
}