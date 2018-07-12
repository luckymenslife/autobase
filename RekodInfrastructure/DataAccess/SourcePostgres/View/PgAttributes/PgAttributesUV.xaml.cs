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
using PgCV = Rekod.DataAccess.SourcePostgres.View.ConfigView;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;

namespace Rekod.DataAccess.SourcePostgres.View
{
    /// <summary>
    /// Логика взаимодействия для AttributesOfObjectView.xaml
    /// </summary>
    public partial class PgAttributesUV : UserControl
    {
        public PgAttributesUV()
        {
            InitializeComponent();
        }
    }
}