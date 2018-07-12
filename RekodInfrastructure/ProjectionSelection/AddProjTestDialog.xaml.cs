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

namespace Rekod.ProjectionSelection
{
    /// <summary>
    /// Interaction logic for AddProjTestDialog.xaml
    /// </summary>
    public partial class AddProjTestDialog : Window
    {
        public AddProjTestDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            AddProj wnd = (AddProj)this.Owner;
            wnd.needTest = true;
            try
            {
                wnd.xCoord = Double.Parse(TBxCoord.Text);
                wnd.yCoord = Double.Parse(TByCoord.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Некорректные координаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            
            string projName = CBprojSelect.SelectedValue.ToString();
            if (projName == null)
            {

                MessageBox.Show("Проекция не выбрана!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            wnd.projKey = projName;
            this.Close();
        }

        private void enter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnOK_Click(null, null);
            }
        }
    }
}
