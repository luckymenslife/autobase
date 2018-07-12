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
using Rekod.UserSets;

namespace Rekod.FastReportClasses
{
    /// <summary>
    /// Логика взаимодействия для Report_V.xaml
    /// </summary>
    public partial class ReportEditor_V : Window
    {
        ReportEditor_VM _vm
        {
            get { return this.DataContext as ReportEditor_VM; }
        }

        public ReportEditor_V()
        {
            InitializeComponent();
        }

        private void lbListReports_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_vm == null)
                return;
            if (e.NewValue is TreeViewItem)
                _vm.CurrentReport = (e.NewValue as TreeViewItem).DataContext as ReportItem_M;
            else
                _vm.CurrentReport = e.NewValue as ReportItem_M;
        }

        private void WinWorkReports_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVM = e.OldValue as ReportEditor_VM;
            if (oldVM != null)
                oldVM.MessStatus.PropertyChanged -= _vm_MessStatus_PropertyChanged;

            var newVM = e.NewValue as ReportEditor_VM;
            if (newVM != null)
            {
                newVM.MessStatus.PropertyChanged -= _vm_MessStatus_PropertyChanged;
                newVM.MessStatus.PropertyChanged += _vm_MessStatus_PropertyChanged;
            }
        }

        void _vm_MessStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var mes = sender as MessageInfo_VM;
            if (mes == null)
                return;

            switch (e.PropertyName)
            {
                case "Status":
                    if (mes.Status == enMessageStatus.Question)
                    {
                        var res = MessageBox.Show(mes.Text, Rekod.Properties.Resources.ReportEditor_V_QuestionDeleteMesTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (res == MessageBoxResult.OK)
                            mes.ButtonClickConnamd.Execute(Rekod.Properties.Resources.ReportEditor_VM_QuestionDeleteYes);
                        else
                            mes.ButtonClickConnamd.Execute(Rekod.Properties.Resources.ReportEditor_VM_QuestionDeleteNo);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
