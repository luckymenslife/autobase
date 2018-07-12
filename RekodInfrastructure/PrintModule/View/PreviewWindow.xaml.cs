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
using System.Windows.Shapes;
using Rekod.PrintModule.LayersObjectModel;
using Rekod.PrintModule.ViewModel;
using System.Windows.Controls.Primitives;
using System.Drawing.Printing;

namespace Rekod.PrintModule.View
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        #region Конструкторы
        public PreviewWindow(AxmvMapLib.AxMapLIb axMapLib1)
        {
            InitializeComponent();
            PreviewWindowVM previewWindowVM = new PreviewWindowVM(axMapLib1, DrawingSurface);
            DataContext = previewWindowVM;
        }
        #endregion Конструкторы
    }
}