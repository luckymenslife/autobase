using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using Interfaces;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.UserSets
{
    public class WorkSetGroup
    {
        public String Name { get; set; }
        public MTObservableCollection<WorkSetItem_S> Items { get; set; }
    }

    public class WorkSets_VM : WindowViewModelBase_VM
    {
        #region Поля
        private MTObservableCollection<WorkSetItem_S> _workSetsMy;
        private MTObservableCollection<WorkSetItem_S> _workSetsOther;
        private WorkSetItem_S _currentSet;
        private MessageInfo_VM _messStatus;

        private RelayCommand _reloadCommand;
        private RelayCommand _addSetCommand;
        private RelayCommand _editSetCommand;
        private RelayCommand _deleteSetCommand;
        private RelayCommand _switchSetCommand;
        private RelayCommand _cancelCommand;
        private WorkSets_M _model;
        #endregion // Поля

        #region Свойства
        public MTObservableCollection<object> WorkSetGroups
        {
            get
            {
                return new MTObservableCollection<object>()
                {
                    DefaultSet,
                    new WorkSetGroup()
                    {
                        Name = Rekod.Properties.Resources.WorkSets_V_ListBoxGroupOther, Items = _workSetsOther
                    },
                    new WorkSetGroup()
                    {
                        Name = Rekod.Properties.Resources.WorkSets_V_ListBoxGroupMy, Items = _workSetsMy
                    }
                };
            }
        }

        /// <summary>
        /// Текущий набор
        /// </summary>
        public WorkSetItem_S CurrentSet
        {
            get { return _currentSet; }
            set { OnPropertyChanged(ref _currentSet, value, () => this.CurrentSet); }
        }

        /// <summary>
        /// Сообщения пользователю
        /// </summary>
        public MessageInfo_VM MessStatus
        {
            get { return _messStatus; }
            set { OnPropertyChanged(ref _messStatus, value, () => this.MessStatus); }
        }

        public WorkSetItem_S DefaultSet
        { get { return _model.DefaultWorkSet; } }
        #endregion // Свойства

        #region Конструктор
        public WorkSets_VM(WorkSets_M model)
        {
            _model = model;
            ((INotifyCollectionChanged)_model.ListWorkSets).CollectionChanged += WorkSets_VM_CollectionChanged;
            _messStatus = new MessageInfo_VM();
            _workSetsMy = new MTObservableCollection<WorkSetItem_S>();
            _workSetsOther = new MTObservableCollection<WorkSetItem_S>();

        }
        #endregion // Конструктор

        #region Команды
        #region ReloadCommand
        public RelayCommand ReloadCommand
        { get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload)); } }
        public void Reload(object obj = null)
        {

            var list = _model.ListWorkSets.Where(f => f.IsDefault == false);
            //Заполняем коллекцию моих элементов
            var setsMy = list.Where(f => f.IdUser == Program.user_info.id_user);
            ExtraFunctions.Sorts.SortList(_workSetsMy, setsMy);

            var setOther = list.Where(f => f.IdUser != Program.user_info.id_user);
            ExtraFunctions.Sorts.SortList(_workSetsOther, setOther);
        }
        #endregion // ReloadCommand

        #region AddSetCommand
        public RelayCommand AddSetCommand
        { get { return _addSetCommand ?? (_addSetCommand = new RelayCommand(this.AddSet)); } }

        private void AddSet(object obj = null)
        {
            MessStatus.ClearStatus();
            try
            {
                SaveSetFrm saveSetFrm = new SaveSetFrm();
                saveSetFrm.DataContext = new WorkSetItem_S(_model, Rekod.Properties.Resources.WorkSets_VM_NewWorkSet);
                if (saveSetFrm.ShowDialog() == true)
                {
                    Reload();
                }
            }
            catch (Exception ex)
            {
                MessStatus.SetStatus(ex.Message, enMessageStatus.Error);
            }
        }
        #endregion // AddSetCommand

        #region EditSetCommand
        public RelayCommand EditSetCommand
        { get { return _editSetCommand ?? (_editSetCommand = new RelayCommand(this.EditSet, this.CanEditSet)); } }
        private void EditSet(object obj = null)
        {
            MessStatus.ClearStatus();
            try
            {
                SaveSetFrm saveSetFrm = new SaveSetFrm();
                saveSetFrm.DataContext = CurrentSet.Clone();
                if (saveSetFrm.ShowDialog() == true)
                {
                    Reload();
                }
            }
            catch (Exception ex)
            {
                MessStatus.SetStatus(ex.Message, enMessageStatus.Error);
            }
        }
        private bool CanEditSet(object obj)
        {
            return (_model.AccessChecked(CurrentSet));
        }
        #endregion // EditSetCommand

        #region DeleteSetCommand
        public RelayCommand DeleteSetCommand
        { get { return _deleteSetCommand ?? (_deleteSetCommand = new RelayCommand(this.DeleteSet, this.CanDeleteSet)); } }

        public void DeleteSet(object obj = null)
        {
            if (!CanDeleteSet(obj))
                return;
            Action<int> result = (e) =>
            {
                if (e == 0)
                    _model.Delete(CurrentSet);
            };
            var buttonResult = new[]{
            Rekod.Properties.Resources.WorkSets_VM_MessageQuestionYes,
            Rekod.Properties.Resources.WorkSets_VM_MessageQuestionNo
            };
            _messStatus.SetQuestion(Rekod.Properties.Resources.WorkSets_VM_MessageQuestionDeleteWorkSet, result, buttonResult);

            Reload();
        }
        private bool CanDeleteSet(object obj)
        {
            return _model.AccessChecked(CurrentSet);
        }
        #endregion // DeleteSetCommand

        #region SwitchSetCommand
        public RelayCommand SwitchSetCommand
        { get { return _switchSetCommand ?? (_switchSetCommand = new RelayCommand(this.SwitchSet, this.CanSwitchSet)); } }

        public void SwitchSet(object obj = null)
        {
            if (!CanSwitchSet(obj))
                return;

            var prevSet = Program.WorkSets.CurrentWorkSet;

            _model.SwitchSet(CurrentSet);

            prevSet.OnPropertyChanged("IsCurrent");
            CurrentSet.OnPropertyChanged("IsCurrent");
        }
        private bool CanSwitchSet(object obj)
        {
            return (CurrentSet != null && !CurrentSet.IsCurrent);
        }
        #endregion // SwitchSetCommand

        #region CancelCommand
        public RelayCommand CancelCommand
        { get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(this.Cancel)); } }

        private void Cancel(object obj = null)
        {
            if (obj != null && obj is Window)
                ((Window)obj).Close();
        }
        #endregion CancelCommand

        #endregion

        #region Методы WindowViewModelBase_VM
        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            MessStatus.ClearStatus();
        }
        #endregion // Методы WindowViewModelBase_VM

        #region Реализация событий
        void WorkSets_VM_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Reload();
        }
        #endregion // Реализация событий
    }


}