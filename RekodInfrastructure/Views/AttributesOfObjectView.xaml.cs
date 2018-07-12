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
using Rekod.ViewModel;

namespace Rekod.Views
{
    /// <summary>
    /// Логика взаимодействия для AttributesOfObjectView.xaml
    /// </summary>
    public partial class AttributesOfObjectView : UserControl
    {

        public AttributesOfObjectView(Interfaces.tablesInfo tablesInfo, int id)
        {
            InitializeComponent();
            var attributesOfObject = new AttributesOfObjectViewModel(tablesInfo, id);
            var filesOfObject = new FilesOfObjectViewModel(tablesInfo, id);
            attributesOfObject.GetAttributesOfObjectCommand.Execute(null);
            attributesOfObject.GetCollectionOfVariantsCommand.Execute(null);
            filesOfObject.LoadFilesCommand.Execute(null);
            gAttributes.DataContext = attributesOfObject;
            gFiles.DataContext = filesOfObject;
        }
    }
}
