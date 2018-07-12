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
using System.Collections.ObjectModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for StyleInterval.xaml
    /// </summary>
    public partial class StyleIntervalReference : UserControl
    {
        public StyleIntervalReference()
        {
            InitializeComponent();
        }

        public bool IsInterval
        {
            get { return (bool)GetValue(IsIntervalProperty); }
            set { SetValue(IsIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIntervalProperty =
            DependencyProperty.Register("IsInterval", typeof(bool), typeof(StyleIntervalReference), new UIPropertyMetadata(false));
    }
}