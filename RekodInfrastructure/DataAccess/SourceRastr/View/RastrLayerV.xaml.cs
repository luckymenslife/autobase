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
using RasM = Rekod.DataAccess.SourceRastr.Model;
using RasVM = Rekod.DataAccess.SourceRastr.ViewModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.IO;
using Rekod.Services;

namespace Rekod.DataAccess.SourceRastr.View
{
    /// <summary>
    /// Interaction logic for RastrLayerV.xaml
    /// </summary>
    public partial class RastrLayerV : UserControl
    {
        public RastrLayerV()
        {
            InitializeComponent();
        }

        public BindingGroup PropertiesBindingGroup
        {
            get { return RastrLayerPropertiesBindingGroup; }
        }
    }
}