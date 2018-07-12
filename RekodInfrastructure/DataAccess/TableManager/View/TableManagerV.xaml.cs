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
using TmVM = Rekod.DataAccess.TableManager.ViewModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.ComponentModel;
using System.Collections.ObjectModel; 

namespace Rekod.DataAccess.TableManager.View
{
    /// <summary>
    /// Interaction logic for TableManagerV.xaml
    /// </summary>
    public partial class TableManagerV : UserControl
    {
        public TableManagerV()
        {
            InitializeComponent();
        }
    }
}