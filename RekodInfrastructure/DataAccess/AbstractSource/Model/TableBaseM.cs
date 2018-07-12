using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rekod.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.DataAccess.AbstractSource.Model
{
    [DebuggerDisplay("TableBaseM {Name}({Id}) Srid:{Srid}")]
    public abstract class TableBaseM : ViewModelBase, AbsM.ITableBaseM, AbsM.ILayerM
    {
        #region Статические методы
        internal static void UpdateIsVisible(TableBaseM table, bool value)
        {
            table.OnPropertyChanged(ref table._isVisible, value, () => table.IsVisible);
        }
        internal static void UpdateIsSelectable(TableBaseM table, bool value)
        {
            table.OnPropertyChanged(ref table._isSelectable, value, () => table.IsSelectable);
        }
        internal static void UpdateIsEditable(TableBaseM table, bool value)
        {
            table.OnPropertyChanged(ref table._isEditable, value, () => table.IsEditable);
        }
        internal static void UpdateIsNewTable(TableBaseM table, bool value)
        {
            table.OnPropertyChanged(ref table._isNewTable, value, () => table.IsNewTable);
        }
        #endregion // Статические методы

        #region Поля
        protected bool _isNewTable = false;
        protected object _id;
        protected AbsM.ETableType _type;
        private bool _isLayer;
        private AbsM.EGeomType _geomType;
        protected String _text;
        protected String _name;
        protected Boolean _isReadOnly;
        protected Boolean _isHidden;
        protected bool _canWrite;
        protected Object _tag;
        protected AbsM.IDataRepositoryM _source;
        protected bool _isVisible;
        protected bool _isEditable;
        protected bool _isSelectable;
        protected string _nameMap;
        ObservableCollection<AbsM.IFieldM> _fields;
        private AbsM.UserAccess _userAccess;
        private ICommand _nameCommand;
        protected int? _srid;
        #endregion // Поля

        #region Конструкторы
        public TableBaseM(AbsM.IDataRepositoryM source, int? srid, AbsM.ETableType type)
            : this(source, 0, srid, type)
        {
            _isNewTable = true;
        }
        public TableBaseM(AbsM.IDataRepositoryM source, object id, int? srid, AbsM.ETableType type)
        {
            _source = source;
            _id = id;
            _type = type;
            _srid = srid;
            _isLayer = srid.HasValue;
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Источник данных
        /// </summary>
        public AbsM.IDataRepositoryM Source
        {
            get { return _source; }
        }
        /// <summary>
        /// Получает признак новой таблицы
        /// </summary>
        public bool IsNewTable
        {
            get { return _isNewTable; }
        }
        /// <summary>
        /// Id таблицы
        /// </summary>
        public object Id
        {
            get { return _id; }
        }
        /// <summary>
        /// Тип таблицы
        /// </summary>
        public AbsM.ETableType Type
        {
            get { return _type; }
            //set { OnPropertyChanged(ref _type, value, () => this.Type); }
        }
        /// <summary>
        /// Является ли таблица слоем
        /// </summary>
        public bool IsLayer
        {
            get { return _isLayer; }
        }

        /// <summary>
        /// Тип геометрии таблицы
        /// </summary>
        public EGeomType GeomType
        {
            get { return _geomType; }
            set { OnPropertyChanged(ref _geomType, value, () => GeomType); }
        }

        /// <summary>
        /// Текст отображаемый в пользовательском интерфейсе
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        /// <summary>
        /// Название таблицы. Если таблица из базы - название таблицы в базе
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        /// <summary>
        /// Название слоя на карте MapLib
        /// </summary>
        public string NameMap
        {
            get { return _nameMap; }
            set { OnPropertyChanged(ref _nameMap, value, () => this.NameMap); }
        }
        /// <summary>
        /// Проекция геометрии таблицы
        /// </summary>
        public int? Srid
        {
            get { return _srid; }
        }
        /// <summary>
        /// Является ли таблица только для чтения
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { OnPropertyChanged(ref _isReadOnly, value, () => this.IsReadOnly); }
        }
        /// <summary>
        /// Права текущего пользователя на таблицу
        /// </summary>
        public UserAccess UserAccess
        {
            get { return _userAccess; }
            set { OnPropertyChanged(ref _userAccess, value, () => this.UserAccess); }
        }
        /// <summary>
        /// Является ли таблица скрытой
        /// </summary>
        public bool IsHidden
        {
            get { return _isHidden; }
            set { OnPropertyChanged(ref _isHidden, value, () => this.IsHidden); }
        }
        /// <summary>
        /// Имеет ли текущий пользователь права на редактирование таблицы
        /// </summary>
        public bool CanWrite
        {
            get { return _canWrite; }
            set { OnPropertyChanged(ref _canWrite, value, () => this.CanWrite); }
        }
        /// <summary>
        /// Объект ассоциированный с таблицей
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set { OnPropertyChanged(ref _tag, value, () => this.Tag); }
        }
        /// <summary>
        /// Видимость слоя (если таблица слой)
        /// </summary>
        public virtual bool IsVisible
        {
            get
            { return _isVisible; }
            set
            {
                ((AbsVM.DataRepositoryVM)Source).SetInvertVisible(this, value);
            }
        }
        /// <summary>
        /// Выбираемость слоя (если таблица слой)
        /// </summary>
        public bool IsSelectable
        {
            get
            { return _isSelectable; }
            set
            {
                if (_isSelectable == value)
                    return;
                ((AbsVM.DataRepositoryVM)Source).SetInvertSelectable(this, value);
            }
        }
        /// <summary>
        /// Редактируемость слоя (если таблица слой)
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                if (_isEditable == value)
                    return;
                ((AbsVM.DataRepositoryVM)Source).SetInvertEditable(this, value);
            }
        }
        public List<AbsM.ITableBaseM> RefTables { get; set; }

        public string NameText
        {
            get { return String.Format("{0} ({1})", _text, _name); }
        }
        #endregion // Свойства

        #region Коллекции
        #region Интерфейс ITableBaseM
        IEnumerable<IFieldM> ITableBaseM.Fields
        {
            get
            {
                return _fields;
            }
        }
        #endregion // Интерфейс ITableBaseM


        /// <summary>
        /// Коллекция всех полей таблицы
        /// </summary>
        public ObservableCollection<AbsM.IFieldM> Fields
        {
            get
            {
                return (!Source.IsTable)
                            ? (ObservableCollection<AbsM.IFieldM>)null
                            : (_fields ?? (_fields = new ObservableCollection<IFieldM>()));
            }
            set
            {
                _fields = (ObservableCollection<AbsM.IFieldM>)value;
            }
        }
        #endregion Коллекции

        #region Методы
        #region Переопределения стандартных методов
        public override int GetHashCode()
        {
            return _source.GetHashCode() ^ _id.GetHashCode();
        }
        #endregion // Переопределения стандартных методов
        #endregion Методы

        #region Команды
        #region ShowSettingsCommand
        /// <summary>
        /// Команда для отображения окна редактирования настроек
        /// </summary>
        public ICommand ShowSettingsCommand
        {
            get { return _nameCommand ?? (_nameCommand = new RelayCommand(this.ShowSettings)); }
        }
        /// <summary>
        /// Отображнеие окна редактирования настроек
        /// </summary>
        public void ShowSettings(object parameter = null)
        {
            Source.OpenTableSettings(this, parameter as System.Windows.UIElement);
        }

        #endregion // ShowSettingsCommand
        #endregion Команды
    }
    /// <summary>
    /// Перечисление прав текущего пользователя на таблицу
    /// </summary>
    public enum UserAccess
    {
        None = 0, Read = 1, Write = 2
    }
}