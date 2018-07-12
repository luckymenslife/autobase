using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Data;
using Rekod.Controllers;
using Rekod.Behaviors;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using Rekod.DataAccess.SourcePostgres.View.ConfigView;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    [DebuggerDisplay("PgFieldM {_table.SchemeName}.{_table.Name}({_table._id}).{Name}({Id})")]
    public class PgFieldM : ViewModelBase, AbsM.IFieldM
    {
        //TODO: Сделать вывод связанной таблицы 

        #region Статические методы
        public static DbType GetDbType(AbsM.EFieldType type)
        {
            switch (type)
            {
                case AbsM.EFieldType.Integer:
                    return System.Data.DbType.Int32;
                case AbsM.EFieldType.Text:
                    return System.Data.DbType.String;
                case AbsM.EFieldType.Date:
                    return System.Data.DbType.DateTime;
                case AbsM.EFieldType.DateTime:
                    return System.Data.DbType.DateTime;
                case AbsM.EFieldType.Real:
                    return System.Data.DbType.Double;
                default:
                    return System.Data.DbType.String;
            }
        }
        #endregion Статические методы

        #region Поля
        private int _id;
        private bool _isNew;
        private int _idTable;
        private PgM.PgTableBaseM _table;
        private AbsM.EFieldType _type;
        private string _name;
        private string _text;
        private string _description;
        private AbsM.ERefType _refType;
        private int? _idRefTable;
        private int? _idRefField;
        private int? _idRefFieldEnd;
        private int? _idRefFieldName;
        private bool _isReadOnly = false;
        private bool _isVisible = true;
        private PgTableBaseM _refTable;
        private PgFieldM _refField;
        private PgFieldM _refFieldEnd;
        private PgFieldM _refFieldName;
        #endregion // Поля

        #region Конструкторы
        public PgFieldM(AbsM.ITableBaseM table)
            : this(table, 0)
        {
            _isNew = true;
        }
        public PgFieldM(AbsM.ITableBaseM table, int id)
        {
            _id = id;
            var pgTable = table as PgM.PgTableBaseM;
            if (pgTable == null)
                throw new ArgumentNullException("Нет ссылки на таблицу");
            _table = pgTable;
            _idTable = _table.Id;
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Идентификатор атрибута
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        /// <summary>
        /// Получает признак нового поля
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
        }
        /// <summary>
        /// Идентификатор таблицы к которой относится атрибут
        /// </summary>
        public AbsM.ITableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// ID таблицы к которой относится атрибут
        /// </summary>
        public int IdTable
        {
            get { return _idTable; }
        }
        /// <summary>
        /// Тип атрибута
        /// </summary>
        public AbsM.EFieldType Type
        {
            get { return _type; }
            set { OnPropertyChanged(ref _type, value, () => this.Type); }
        }
        /// <summary>
        /// Системный тип
        /// </summary>
        public DbType DbType
        {
            get { return GetDbType(_type); }
        }
        /// <summary>
        /// Наименование атрибута
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        /// <summary>
        /// Наименование атрибута, отображаемое в интерфейсе пользователя
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        /// <summary>
        /// Описание атрибута
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { OnPropertyChanged(ref _description, value, () => this.Description); }
        }
        /// <summary>
        /// Таблица с которой связывается поле
        /// </summary>
        public int? IdRefTable
        {
            get { return _idRefTable; }
            set { OnPropertyChanged(ref _idRefTable, value, () => this.IdRefTable); }
        }
        /// <summary>
        /// Если таблица это интервал, то это начало интервала. Если таблица справочник -id колонки в таблице ref_table 
        /// </summary>
        public int? IdRefField
        {
            get { return _idRefField; }
            set { OnPropertyChanged(ref _idRefField, value, () => this.IdRefField); }
        }
        /// <summary>
        /// Если таблица интервал, то это конец интервала в этой таблице
        /// </summary>
        public int? IdRefFieldEnd
        {
            get { return _idRefFieldEnd; }
            set { OnPropertyChanged(ref _idRefFieldEnd, value, () => this.IdRefFieldEnd); }
        }
        /// <summary>
        /// То что отображается - цель связывания (id поля кт. является наименованием)
        /// </summary>
        public int? IdRefFieldName
        {
            get { return _idRefFieldName; }
            set { OnPropertyChanged(ref _idRefFieldName, value, () => this.IdRefFieldName); }
        }
        /// <summary>
        /// Атрибут только для чтения
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { OnPropertyChanged(ref _isReadOnly, value, () => this.IsReadOnly); }
        }
        /// <summary>
        /// Скрытое ли поле
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { OnPropertyChanged(ref _isVisible, value, () => this.IsVisible); }
        }
        /// <summary>
        /// Связан с таблицей ReferenceTable типом справочник
        /// </summary>
        public AbsM.ERefType RefType
        {
            get { return _refType; }
            set { OnPropertyChanged(ref _refType, value, () => this.RefType); }
        }
        /// <summary>
        /// Таблица с которой связывается поле
        /// </summary>
        public PgTableBaseM RefTable
        {
            get { return _refTable; }
            set { OnPropertyChanged(ref _refTable, value, () => this.RefTable); }
        }
        /// <summary>
        /// Ссылка на поле с идентификатором в связанной таблице
        /// </summary>
        public PgFieldM RefField
        {
            get { return _refField; }
            set { OnPropertyChanged(ref _refField, value, () => this.RefField); }
        }
        /// <summary>
        /// Ссылка на поле конца интервала в связанной таблице
        /// </summary>
        public PgFieldM RefFieldEnd
        {
            get { return _refFieldEnd; }
            set { OnPropertyChanged(ref _refFieldEnd, value, () => this.RefFieldEnd); }
        }
        /// <summary>
        /// Ссылка на поле с названием в связанной таблице
        /// </summary>
        public PgFieldM RefFieldName
        {
            get { return _refFieldName; }
            set { OnPropertyChanged(ref _refFieldName, value, () => this.RefFieldName); }
        }

        #region Свойства AbsM.IFieldM
        object AbsM.IFieldM.IdTable
        {
            get { return Id; }
        }
        #endregion // Свойства AbsM.IFieldM
        #endregion // Свойства

        #region Действия
        public Action<object> BindingGroupLoaded
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        BindingGroup bindingGroup = commEvtParam.CommandParameter as BindingGroup;
                        bindingGroup.BeginEdit();
                    };
            }
        }
        public Action<object> BindingGroupCancel
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindingGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindingGroup.CancelEdit();
                    bindingGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupSave
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindingGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindingGroup.CommitEdit())
                    {
                        (Table.Source as PgVM.PgDataRepositoryVM).DBSaveField(this);
                        bindingGroup.BeginEdit();
                    }
                };
            }
        }
        public Action<object> BindingGroupError
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    ValidationErrorEventArgs errorArgs = commEvtParam.EventArgs as ValidationErrorEventArgs;
                    if (errorArgs.Action == ValidationErrorEventAction.Added)
                    {
                        MessageBox.Show(errorArgs.Error.ErrorContent.ToString());
                    }
                };
            }
        }
        public Action<object> ChangeRefTypeBox
        {
            get 
            {
                return param => 
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    List<object> commParams = commEvtParam.CommandParameter as List<object>;
                    ComboBox RefTableTypeBox = commParams[0] as ComboBox;
                    TextBlock RefFieldIdText = commParams[1] as TextBlock;
                    TextBlock RefFieldEndText = commParams[2] as TextBlock;
                    ComboBox RefFieldEndBox = commParams[3] as ComboBox;
                    if (RefTableTypeBox.SelectedItem != null)
                    {
                        EnumWrapper enumWrap = (EnumWrapper)RefTableTypeBox.SelectedItem;
                        AbsM.ERefType refType = (AbsM.ERefType)enumWrap.Value;
                        if (refType == AbsM.ERefType.Interval)
                        {
                            RefFieldIdText.Text = Rekod.Properties.Resources.LocBeginOfInterval;
                            RefFieldEndText.Visibility = RefFieldEndBox.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            RefFieldIdText.Text = Rekod.Properties.Resources.LocSourceField;
                            RefFieldEndText.Visibility = RefFieldEndBox.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                };
            }
        }
        #endregion Действия

        #region Методы
        public override int GetHashCode()
        {
            return _table.GetHashCode() ^ _id.GetHashCode();
        }
        #endregion Методы
    }
}