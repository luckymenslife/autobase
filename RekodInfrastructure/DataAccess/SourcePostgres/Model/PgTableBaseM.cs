using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Rekod.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Behaviors;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Базовый класс для работы с Postgres
    /// </summary>
    [DebuggerDisplay("PgTableBaseM {SchemeName}.{Name}({Id})")]
    public class PgTableBaseM : AbsM.TableBaseM, AbsM.ITableBaseM, ICloneable
    {
        #region Поля
        protected new int _id;
        private string _primaryKey;
        private string _schemeName;
        private bool _isView;
        private bool _hasHistory;
        private bool _hasFiles;
        private int? _referenceTable;
        private string _viewName;
        private string _geomField;
        private bool _isMapStyle;
        private AbsM.IFieldM _primaryKeyField;
        private PgFileInfoM _fileInfo;
        private string _GC_GeomType;
        #endregion // Поля

        #region Конструкторы
        public PgTableBaseM(AbsM.IDataRepositoryM source, int? srid, AbsM.ETableType type) :
            base(source, srid, type)
        { }
        public PgTableBaseM(AbsM.IDataRepositoryM source, Int32 id, int? srid, AbsM.ETableType type)
            : base(source, id, srid, type)
        {
            _id = id;
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Id таблицы
        /// </summary>
        public new int Id
        { get { return _id; } }
        /// <summary>
        /// Имя атрибута первичного ключа в базе
        /// </summary>
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value, () => this.PrimaryKey); }
        }
        /// <summary>
        /// Ссылка на атрибут первичного ключа в базе
        /// </summary>
        public AbsM.IFieldM PrimaryKeyField
        {
            get { return _primaryKeyField; }
            set { OnPropertyChanged(ref _primaryKeyField, value, () => this.PrimaryKeyField); }
        }
        /// <summary>
        ///  Название схемы в которой находится таблица (если таблица получена из базы данных)
        /// </summary>
        public string SchemeName
        {
            get { return _schemeName; }
            set { OnPropertyChanged(ref _schemeName, value, () => this.SchemeName); }
        }
        /// <summary>
        /// Является ли таблица пользовательским представлением
        /// </summary>
        public bool IsView
        {
            get { return _isView; }
            set { OnPropertyChanged(ref _isView, value, () => this.IsView); }
        }
        /// <summary>
        /// Ведется ли история для таблицы
        /// </summary>
        public bool HasHistory
        {
            get { return _hasHistory; }
            set { OnPropertyChanged(ref _hasHistory, value, () => this.HasHistory); }
        }
        /// <summary>
        /// Можно ли добавлять файлы в таблицу
        /// </summary>
        public bool HasFiles
        {
            get { return _hasFiles; }
            set { OnPropertyChanged(ref _hasFiles, value, () => this.HasFiles); }
        }
        /// <summary>
        /// Таблица на которую ссылается данная таблица
        /// </summary>
        public int? ReferenceTable
        {
            get { return _referenceTable; }
            set { OnPropertyChanged(ref _referenceTable, value, () => this.ReferenceTable); }
        }
        /// <summary>
        /// Название вьюшки таблицы. Система генерирует вьюшки после регистрации таблицы
        /// </summary>
        public string ViewName
        {
            get { return _viewName; }
            set { OnPropertyChanged(ref _viewName, value, () => this.ViewName); }
        }
        /// <summary>
        /// Стиль по умолчанию для слоя
        /// </summary>
        public PgStyleLayerM Style
        {
            get
            {
                PgVM.PgDataRepositoryVM source = Source as PgVM.PgDataRepositoryVM;
                if (source.DefaultSet != null && source.DefaultSet.Layers != null && source.DefaultSet.Layers.ContainsKey(this))
                {
                    return source.DefaultSet.Layers[this];
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Стиль по умолчанию для подписи слоя
        /// </summary>
        public PgStyleLableM LabelStyle
        {
            get
            {
                PgVM.PgDataRepositoryVM source = Source as PgVM.PgDataRepositoryVM;
                if (source.DefaultSet != null && source.DefaultSet.Labels != null && source.DefaultSet.Labels.ContainsKey(this))
                {
                    PgStyleLableM labelStyle = source.DefaultSet.Labels[this];
                    return labelStyle;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Информация о таблице с файлами
        /// </summary>
        public PgFileInfoM FileInfo
        {
            get { return _fileInfo; }
            set { _fileInfo = value; }
        }
        /// <summary>
        /// Таблица доступна только для чтения
        /// </summary>
        bool  AbsM.ITableBaseM.IsReadOnly
        {
            get { return (_isReadOnly || !CanWrite); }
        }
        /// <summary>
        /// Возвращает список прав всех пользователей к данной таблице (если текущий пользователь администратор)
        /// </summary>
        public PgListUserRightsVM UsersRights
        {
            get
            {
                PgUserM curUser = (Source as PgVM.PgDataRepositoryVM).CurrentUser;
                if (curUser != null && curUser.Type == UserType.Admin)
                {
                    PgListUserRightsVM userRights = new PgListUserRightsVM(Source);
                    userRights.Table = this;
                    return userRights;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Таблицы, которые ссылаются на текущую
        /// </summary>
        public List<AbsM.ITableBaseM> RefTables
        {
            get;
            set;
        }
        #region Свойства справочников и интервалов
        /// <summary>
        /// Содержит ли таблица стили (только к справочникам и интервалам)
        /// </summary>
        public bool IsMapStyle
        {
            get { return _isMapStyle; }
            set
            {
                if (_type == AbsM.ETableType.Interval || _type == AbsM.ETableType.Catalog)
                    OnPropertyChanged(ref _isMapStyle, value, () => this.IsMapStyle);
                else
                    Debug.Fail("Это поле только для интервалов и полей");
            }
        }
        #endregion // Свойства справочников и интервалов
        #region Свойства слоя
        /// <summary>
        /// Поле геометрии таблицы
        /// </summary>
        public string GeomField
        {
            get { return _geomField; }
            set
            {
                if (_type == AbsM.ETableType.MapLayer)
                    OnPropertyChanged(ref _geomField, value, () => GeomField);
                else
                    Debug.Fail("Это поле только для геометрии");
            }
        }

        public string GC_GeomType
        {
            get { return this._GC_GeomType; }
            set
            {
                if (_type == AbsM.ETableType.MapLayer)
                    OnPropertyChanged(ref _GC_GeomType, value, () => GC_GeomType);
                else
                    Debug.Fail("Это поле только для геометрии");
            }
        }
        /// <summary>
        /// VM для работы с группами
        /// </summary>
        public PgLayerGroupsVM LayerGroups
        {
            get
            {
                return new PgLayerGroupsVM(this);
            }
        }

        public PgIndexVM Index
        {
            get
            {
                return new PgIndexVM(this);
            }
        }
        #endregion // Свойства слоя
        #endregion // Свойства

        #region Методы
        public object Clone()
        {
            PgTableBaseM pgTable = this.MemberwiseClone() as PgTableBaseM;
            PgTableBaseM.UpdateIsNewTable(pgTable, true);

            int postfixIndex = 0;
            pgTable.Name = "";
            do
            {
                String newNameInBase = Name + "_copy" + ((postfixIndex == 0) ? "" : String.Format("_{0}", postfixIndex));
                String newText = Text + " копия" + ((postfixIndex == 0) ? "" : String.Format(" {0}", postfixIndex));
                postfixIndex++;
                using (SqlWork sqlWork = new SqlWork((Source as PgVM.PgDataRepositoryVM).Connect))
                {
                    sqlWork.sql = String.Format("SELECT EXISTS(SELECT * FROM sys_scheme.table_info WHERE scheme_name = '{0}' AND name_db = '{1}')",
                        SchemeName, newNameInBase);
                    if (!(Boolean)(sqlWork.ExecuteScalar()))
                    {
                        pgTable.Name = newNameInBase;
                        pgTable.Text = newText;
                    }
                }
            }
            while (String.IsNullOrEmpty(pgTable.Name));
            return pgTable;
        }
        #endregion Методы

        #region Действия
        public Action<object> BindingGroupLoadedAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupErrorAction
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
        public Action<object> BindingGroupCancelAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindGroup.CancelEdit();
                    bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupSaveAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindGroup.CommitEdit())
                    {
                        (Source as PgVM.PgDataRepositoryVM).DBSaveTable(this);
                        (Source as AbstractSource.ViewModel.DataRepositoryVM).UpdateGroupVisibility();
                        bindGroup.BeginEdit();
                    }
                };
            }
        }
        #endregion Действия
    }
}