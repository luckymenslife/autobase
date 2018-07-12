using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Properties;
using Rekod.UserSets;
using Rekod.Behaviors;
using System.Windows.Input;
using Interfaces.FastReport;

namespace Rekod.FastReportClasses
{
    public class ReportGroup
    {
        public String Name { get; set; }
        public MTObservableCollection<IReportItem_M> Items { get; set; }
        public Visibility Visible { get; set; }
    }

    public class ReportEditor_VM : WindowViewModelBase_VM
    {
        #region Поля
        private Report_M _model;
        private MTObservableCollection<IReportItem_M> _listReportsObject;
        private MTObservableCollection<IReportItem_M> _listReportsTables;
        private MTObservableCollection<IReportItem_M> _listReportsAll;
        private ReportItem_M _currentSet;
        private IFilterTable _filter;

        private RelayCommand _reloadCommand;
        private RelayCommand _addReportCommand;
        private RelayCommand _editReportCommand;
        private RelayCommand _deleteReportCommand;
        private RelayCommand _openReportCommand;
        private RelayCommand _cancelCommand;

        private MessageInfo_VM _messStatus;
        private enTypeReport _typeEditor;

        #endregion // Поля

        #region Свойства

        public MTObservableCollection<ReportGroup> ReportGroups
        {
            get
            {
                var groupAll = new ReportGroup()
                    {
                        Name = Rekod.Properties.Resources.ReportEditor_V_ListReportsAll,
                        Items = _listReportsAll,
                        Visible = (_typeEditor == enTypeReport.All) ? Visibility.Visible : Visibility.Collapsed
                    };
                var groupObject = new ReportGroup()
                    {
                        Name = Rekod.Properties.Resources.ReportEditor_V_ListReportsObject,
                        Items = _listReportsObject,
                        Visible = (_typeEditor != enTypeReport.All) ? Visibility.Visible : Visibility.Collapsed
                    };
                var groupTable = new ReportGroup()
                    {
                        Name = Rekod.Properties.Resources.ReportEditor_V_ListReportsTable,
                        Items = _listReportsTables,
                        Visible = (_typeEditor == enTypeReport.Table) ? Visibility.Visible : Visibility.Collapsed
                    };
                var s = new MTObservableCollection<ReportGroup>() { groupAll, groupObject, groupTable };

                return s;
            }
        }

        // Фильтр
        public IFilterTable Filter
        {
            get { return _filter; }
        }
        // Фильтр
        public enTypeReport TypeEditor
        {
            get { return _typeEditor; }
        }
        /// <summary>
        /// Текущий набор
        /// </summary>
        public ReportItem_M CurrentReport
        {
            get { return _currentSet; }
            set { OnPropertyChanged(ref _currentSet, value, () => this.CurrentReport); }
        }

        /// <summary>
        /// Сообщения пользователю
        /// </summary>
        public MessageInfo_VM MessStatus
        {
            get { return _messStatus; }
            set { OnPropertyChanged(ref _messStatus, value, () => this.MessStatus); }
        }
        #endregion // Свойства

        #region Конструктор
        public ReportEditor_VM(Report_M model, IFilterTable filter, enTypeReport typeEditor)
        {
            _model = model;
            _messStatus = new MessageInfo_VM();
            _listReportsObject = new MTObservableCollection<IReportItem_M>();
            _listReportsTables = new MTObservableCollection<IReportItem_M>();
            _listReportsAll = new MTObservableCollection<IReportItem_M>();
            model.ListReports.CollectionChanged += ListReports_CollectionChanged;
            _filter = filter;
            _typeEditor = typeEditor;
        }

        #endregion

        #region Команды
        #region ReloadCommand
        public RelayCommand ReloadCommand
        { get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload)); } }
        public void Reload(object obj = null)
        {
            switch (_typeEditor)
            {
                case enTypeReport.All:
                    {
                        var repAll = _model.ListReports.Where(f => f.Type == enTypeReport.All);
                        ExtraFunctions.Sorts.SortList(_listReportsAll, repAll);
                    } break;
                case enTypeReport.Table:
                    {
                        var list = _model.ListReports.Where(f => f.IdTable == _filter.IdTable);
                        var repObject1 = list.Where(f => f.Type == enTypeReport.Object);
                        ExtraFunctions.Sorts.SortList(_listReportsObject, repObject1);

                        var repTable = list.Where(f => f.Type == enTypeReport.Table);
                        ExtraFunctions.Sorts.SortList(_listReportsTables, repTable);
                    } break;
                case enTypeReport.Object:
                    {
                        var repObject2 = _model.ListReports.Where(f => f.Type == enTypeReport.Object && f.IdTable == _filter.IdTable);
                        ExtraFunctions.Sorts.SortList(_listReportsObject, repObject2);
                    } break;
                default:
                    break;
            }

            //Заполняем коллекцию моих элементов
        }
        #endregion // ReloadCommand

        #region AddSetCommand
        public RelayCommand AddReportCommand
        { get { return _addReportCommand ?? (_addReportCommand = new RelayCommand(this.AddReport, this.CanAddReport)); } }
        private void AddReport(object obj = null)
        {
            if (!CanAddReport(obj))
                return;
            MessStatus.ClearStatus();
            CurrentReport = null;

            //todo: нужно сделать выбор добавления отчета
            enTypeReport typeReport = (enTypeReport)Enum.Parse(typeof(enTypeReport), (string)obj);

            ReportItem_M report = _model.NewReport(typeReport, _filter.IdTable);

            MessStatus.ClearStatus();
            switch (report.Type)
            {
                case enTypeReport.All:
                    _model.OpenDesignAll(report, _filter);
                    break;
                case enTypeReport.Table:
                    _model.OpenDesignTable(report, _filter);
                    break;
                case enTypeReport.Object:
                    _model.OpenDesignObject(report, _filter);
                    break;
                default:
                    break;
            }
        }
        private bool CanAddReport(object obj)
        {
            return Enum.IsDefined(typeof(enTypeReport), obj) && _model.AccessChecked(null);
        }
        #endregion // AddSetCommand

        #region EditReportCommand
        public RelayCommand EditReportCommand
        { get { return _editReportCommand ?? (_editReportCommand = new RelayCommand(this.EditReport, this.CanEditReport)); } }
        private void EditReport(object obj = null)
        {
            if (!CanEditReport(obj))
                return;
            MessStatus.ClearStatus();
            try
            {
                switch (CurrentReport.Type)
                {
                    case enTypeReport.All:
                        _model.OpenDesignAll(CurrentReport, _filter);
                        break;
                    case enTypeReport.Table:
                        _model.OpenDesignTable(CurrentReport, _filter);
                        break;
                    case enTypeReport.Object:
                        _model.OpenDesignObject(CurrentReport, _filter);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessStatus.SetStatus(ex.Message, enMessageStatus.Error);
            }

        }
        private bool CanEditReport(object obj)
        {
            return (CurrentReport != null && _model.AccessChecked(CurrentReport));
        }
        #endregion // EditReportCommand

        #region DeleteSetCommand
        public RelayCommand DeleteReportCommand
        { get { return _deleteReportCommand ?? (_deleteReportCommand = new RelayCommand(this.DeleteReport, this.CanDeleteReport)); } }
        public void DeleteReport(object obj = null)
        {
            if (!CanDeleteReport(obj))
                return;
            MessStatus.ClearStatus();
            Action<int> result = (e) =>
            {
                if (e == 0)
                    _model.Delete(CurrentReport);
            };
            var buttonResult = new[]{
            Resources.ReportEditor_VM_QuestionDeleteYes,
            Resources.ReportEditor_VM_QuestionDeleteNo
            };
            _messStatus.SetQuestion(Resources.ReportEditor_VM_QuestionDeleteMes, result, buttonResult);

        }
        private bool CanDeleteReport(object obj)
        {
            return (CurrentReport != null && _model.AccessChecked(CurrentReport));
        }
        #endregion // DeleteSetCommand

        #region OpenReportCommand
        public RelayCommand OpenReportCommand
        { get { return _openReportCommand ?? (_openReportCommand = new RelayCommand(this.OpenReport, this.CanOpenReport)); } }

        public void OpenReport(object obj = null)
        {
            if (!CanOpenReport(obj))
                return;

            if (obj is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = obj as CommandEventParameter;
                MouseButtonEventArgs e = commEvtParam.EventArgs as MouseButtonEventArgs;
                if (e.ClickCount < 2 || e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }
            }

            MessStatus.ClearStatus();
            try
            {
                _model.OpenReport(CurrentReport, _filter);
            }
            catch (Exception ex)
            {
                MessStatus.SetStatus(ex.Message, enMessageStatus.Error);
            }

        }
        private bool CanOpenReport(object obj)
        {
            return (CurrentReport != null);
        }
        #endregion // OpenReportCommand

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
        #endregion

        #region Обработка событий

        void ListReports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Reload();
        }
        #endregion // Обработка событий
    }
}
