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

namespace Rekod.UserSets
{
    /// <summary>
    /// Логика взаимодействия для WorkSets_V.xaml
    /// </summary>
    public partial class WorkSets_V : Window
    {
        WorkSets_VM _vm
        {
            get { return this.DataContext as WorkSets_VM; }
        }

        public WorkSets_V()
        {
            InitializeComponent();
        }

        private void lbListSets_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_vm == null)
                return;
            if (e.NewValue is TreeViewItem)
                _vm.CurrentSet = (e.NewValue as TreeViewItem).DataContext as WorkSetItem_S;
            else
                _vm.CurrentSet = e.NewValue as WorkSetItem_S;
        }

        private void WinWorkSets_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVM = e.OldValue as WorkSets_VM;
            if (oldVM != null)
                oldVM.MessStatus.PropertyChanged -= _vm_MessStatus_PropertyChanged;

            var newVM = e.NewValue as WorkSets_VM;
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
                        var res = MessageBox.Show(mes.Text, Rekod.Properties.Resources.WorkSets_V_MessageQuestion, MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (res == MessageBoxResult.OK)
                            mes.ButtonClickConnamd.Execute(Rekod.Properties.Resources.WorkSets_VM_MessageQuestionYes);
                        else
                            mes.ButtonClickConnamd.Execute(Rekod.Properties.Resources.WorkSets_VM_MessageQuestionNo);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
