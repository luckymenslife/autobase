using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using TMVM = Rekod.DataAccess.TableManager.ViewModel;

namespace Rekod.DataAccess.SourcePostgres.View
{
    /// <summary>
    /// Interaction logic for PostgreRepositoryV.xaml
    /// </summary>
    public partial class PostgresRepositoryV : UserControl
    {
        public PostgresRepositoryV()
        {
            InitializeComponent();
        }
    }
}