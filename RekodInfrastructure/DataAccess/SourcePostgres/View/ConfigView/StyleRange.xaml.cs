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
using PgM = Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for StyleRange.xaml
    /// </summary>
    public partial class StyleRange : UserControl
    {
        public StyleRange()
        {
            InitializeComponent();
        }

        public Boolean RangeIsEnabled
        {
            get { return (Boolean)GetValue(RangeIsEnabledProperty); }
            set { SetValue(RangeIsEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RangeIsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RangeIsEnabledProperty =
            DependencyProperty.Register("RangeIsEnabled", typeof(Boolean), typeof(StyleRange), new PropertyMetadata(true));
    }
}