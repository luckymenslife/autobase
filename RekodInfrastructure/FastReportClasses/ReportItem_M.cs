using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces.FastReport;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.FastReportClasses
{
    public class ReportItem_M : ViewModelBase, ICloneable, IReportItem_M
    {
        #region Поля
        private int _idReport;
        private enTypeReport _type;
        private string _body;
        private string _caption;
        private int? _idTable;
        private bool _isNew;
        #endregion // Поля

        #region Свойства
        public int IdReport
        {
            get { return _idReport; }
        }
        public enTypeReport Type
        {
            get { return _type; }
        }
        public string Body
        {
            get { return _body; }
            set { OnPropertyChanged(ref _body, value, () => this.Body); }
        }
        public string Caption
        {
            get { return _caption; }
            set
            {
                OnPropertyChanged(ref _caption, value, () => this.Caption);
                OnPropertyChanged("IsValid");
                OnPropertyChanged("IsUsedName");
                OnPropertyChanged("IsEmptyName");
            }
        }
        public int? IdTable
        {
            get { return _idTable; }
        }
        public bool IsNew
        { get { return _isNew; } }

        /// <summary>
        /// Имя уже занято 
        /// </summary>
        public bool IsUsedName
        { get { return Program.ReportModel.FindNameMatch(this); } }

        /// <summary>
        /// Пустое имя
        /// </summary>
        public bool IsEmptyName
        { get { return String.IsNullOrWhiteSpace(_caption); } }

        /// <summary>
        /// Имя может использоваться
        /// </summary>
        public bool IsValid
        { get { return !IsUsedName && !IsEmptyName; } }
        #endregion // Свойства

        #region Конструктор
        public ReportItem_M(IReportItem_M rep)
        {
            _isNew = false;
            _idReport = rep.IdReport;
            _type = rep.Type;
            _idTable = rep.IdTable;
        }
        public ReportItem_M(int idReport, enTypeReport type, int? table)
        {
            _isNew = false;
            _idReport = idReport;
            _type = type;
            _idTable = table;
        }
        public ReportItem_M(enTypeReport type, int? table)
            : this( 0, type, table)
        {
            _isNew = true;
        }
        #endregion // Конструктор

        #region Методы
        public void SetNew()
        {
            _idReport = 0;
            _isNew = true;
        }
        #endregion

        #region RenameCommand
        private RelayCommand _renameCommand;
        public RelayCommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand = new RelayCommand(Rename, CanRename));
            }
        }
        /// <summary>
        /// Переименовать отчет
        /// </summary>
        /// <param name="obj">Отчет</param>
        public void Rename(object obj = null)
        {
            ReportItem_M item = (ReportItem_M)obj;

            var frm = new SaveReportFrm();
            frm.DataContext = item.Clone();
            var result = frm.ShowDialog();
            if (result == true)
            {
                Program.ReportModel.Apply((ReportItem_M)frm.DataContext);
            }
        }
        public bool CanRename(object obj = null)
        {
            return (obj != null && obj is ReportItem_M);
        }
        #endregion RenameCommand

        #region Интерфейст ICloneable
        public object Clone()
        {
            return new ReportItem_M(this._type, this._idTable)
            {
                _idReport = this._idReport,
                _body = this._body,
                _caption = this._caption,
                _isNew = this._isNew
            };
        }
        #endregion // Интерфейст ICloneable

        #region Статические методы

        public static void SetReportId(ReportItem_M item, int id)
        {
            item._idReport = id;
            item._isNew = false;
        }
        #endregion
    }
}
