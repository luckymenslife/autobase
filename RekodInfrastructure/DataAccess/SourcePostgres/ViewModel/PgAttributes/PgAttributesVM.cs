using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using System.Collections.ObjectModel;
using Rekod.DataAccess.AbstractSource;
using System.Windows.Input;
using PgTV = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using Rekod.SQLiteSettings;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;
using System.Threading;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes
{
    public class PgAttributesVM : WindowViewModelBase_VM
    {
        #region Поля
        private readonly Npgsql.NpgsqlConnectionStringBuilder _connect;
        private readonly bool _isDebug = false;
        private readonly bool _isReadOnly;

        private readonly PgM.PgAttributes.IPgAttributesVM _attributesListVM;
        private PgAtVM.PgAttributeFilesVM _filesVM;
        private PgM.PgTableBaseM _table;

        private PgAttributesStyleVM _styleVM;
        private PgAttributesGeomVM _pgGeometryVM;
        private ObservableCollection<PgTV.PgTableViewVM> _relatedTables;
        private PgDataRepositoryVM _source;

        private ICommand _saveCommand;
        private ICommand _reloadCommand;
        private ICommand _openHistoryCommand;
        private ICommand _openTableCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Ссылка на таблицу
        /// </summary>
        public PgM.PgTableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// Ссылка на источник
        /// </summary>
        public PgVM.PgDataRepositoryVM Source
        {
            get { return _source; }
        }
        public bool IsDebug
        {
            get { return _isDebug; }
        }
        /// <summary>
        /// Доступ к объекту с атрибутами
        /// </summary>
        public bool IsReadOnly
        { get { return _isReadOnly; } }

        public Npgsql.NpgsqlConnectionStringBuilder Connect
        {
            get { return _connect; }
        }
        /// <summary>
        /// ViewModel списка атрибутов объекта
        /// </summary>
        public PgM.PgAttributes.IPgAttributesVM AttributesListVM
        {
            get { return _attributesListVM; }
        }
        /// <summary>
        /// ViewModel списка файлов объекта
        /// </summary>
        public PgAtVM.PgAttributeFilesVM FilesVM
        {
            get { return _filesVM; }
        }
        /// <summary>
        /// ViewModel стиля объекта
        /// </summary>
        public PgAtVM.PgAttributesStyleVM StyleVM
        {
            get { return _styleVM; }
        }
        /// <summary>
        /// ViewModel геометрии объекта
        /// </summary>
        public PgAtVM.PgAttributesGeomVM PgGeometryVM
        {
            get { return _pgGeometryVM; }
        }
        #endregion // Свойства

        #region Коллекции
        public ObservableCollection<PgTV.PgTableViewVM> RelatedTables
        {
            get { return _relatedTables ?? (_relatedTables = new MTObservableCollection<PgTV.PgTableViewVM>()); }
        }
        #endregion Коллекции

        #region Конструктор
        public PgAttributesVM(AbsM.ITableBaseM table, object idObject, bool isReadOnly = false, bool isDebug = false, String wkt = null)
        {
            var pgTable = table as PgM.PgTableBaseM;
            if (pgTable == null)
                throw new ArgumentNullException("Нет ссылки на таблицу");
            else if (pgTable.PrimaryKeyField == null)
                throw new ArgumentNullException("В таблице нет первичного ключа");

            _isDebug = isDebug;
            _connect = ((PgVM.PgDataRepositoryVM)pgTable.Source).Connect;
            _table = pgTable;
            _source = pgTable.Source as PgVM.PgDataRepositoryVM;

            // Проверка прав на объект с атрибутами
            if (isReadOnly || pgTable.IsReadOnly || !pgTable.CanWrite)
                _isReadOnly = true;

            _attributesListVM = new PgAtVM.PgAttributesListVM(this, idObject);
            if (_table.FileInfo != null)
                _filesVM = new PgAtVM.PgAttributeFilesVM(this);
            if (_table.IsMapStyle && idObject != null)
                _styleVM = new PgAttributesStyleVM(this);
            if (_table.Type == AbsM.ETableType.MapLayer || _table.Type == AbsM.ETableType.View)
            {
                //_pgGeometryVM = new PgAttributesGeomVM(this, wkt);
                _pgGeometryVM = new PgAttributesGeomVM(_table.Id, wkt, false, (int?)_attributesListVM.PkAttribute.Value, (_table.Source as PgVM.PgDataRepositoryVM).Connect); 
            }
            Title = String.Format("Объект: [id: {0}]; Таблица: \"{1}\"; Источник: \"{3}@{4}:{5} ({6})\"; Тип: \"{2}\"",
                                   idObject ?? "null",
                                   _table.Text,
                                   _table.Source.Type,
                                   _source.Connect.Database,
                                   _source.Connect.Host,
                                   _source.Connect.Port,
                                   _source.CurrentUser.NameFull);
        }
        #endregion // Конструктор

        #region Методы
        /// <summary>
        /// Загрузка связанных таблиц
        /// </summary>
        private void LoadRelatedTables(object idObject)
        {
            if (idObject != null && (_table.Type == AbsM.ETableType.Data || _table.Type == AbsM.ETableType.MapLayer))
            {
                RelatedTables.Clear();
                using (SqlWork sqlWork = new SqlWork(Connect))
                {
                    // Запрос для получения идентификаторов связанных таблиц
                    sqlWork.sql = String.Format(@"SELECT tfi.id_table, tfi.name_db, tfi.ref_field
                                    FROM sys_scheme.table_field_info tfi 
                                    INNER JOIN sys_scheme.table_info ti ON tfi.id_table = ti.id
                                    INNER JOIN sys_scheme.table_type_table ttt ON ttt.id = ti.type
                                    WHERE tfi.ref_table = {0} AND (ttt.id = 1 OR ttt.id = 4)", _table.Id);
                    sqlWork.ExecuteReader();
                    while (sqlWork.CanRead())
                    {
                        int idTable = sqlWork.GetInt32("id_table");
                        string colName = sqlWork.GetString("name_db");
                        int refFieldId = sqlWork.GetInt32("ref_field");

                        PgM.PgFieldM field = _table.Fields.FirstOrDefault(f => f.Id == refFieldId) as PgM.PgFieldM;

                        if (field != null && field.Table != null && field.Table.Id is int 
                            && classesOfMetods.getRefTableRight(_table.Id, (int)field.Table.Id))
                        {
                            PgAtM.PgAttributeM attr = _attributesListVM.Attributes.FirstOrDefault(a => a.Field == field);
                            String filterValue = null;
                            if (attr.Value != null)
                            {
                                filterValue = attr.Value.ToString();
                            }

                            PgM.PgTableBaseM pgTable = _table.Source.Tables.FirstOrDefault(p => (int)p.Id == idTable) as PgM.PgTableBaseM;
                            if (pgTable != null)
                            {
                                FilterRelationModel filterRM = new FilterRelationModel()
                                {
                                    Type = TypeRelation.ELEMENT,
                                    Element = new FilterElementModel()
                                    {
                                        Column = colName,
                                        Type = TypeOperation.Equal,
                                        Value = filterValue
                                    }
                                };
                                RelatedTables.Add(new PgTV.PgTableViewVM(pgTable, filterRM));
                            }
                        }
                    }
                    sqlWork.Close();
                }
            }
            OnPropertyChanged("HasRelatedTables");
        }
        #endregion Методы

        #region Команды
        #region Command: SaveCommand
        /// <summary>
        /// Сохраняет атрибуты объекта
        /// </summary>
        public ICommand SaveCommand
        {
            get
            { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        void Save(object param = null)
        {
            if (!CanSave(param))
                return;
            if (PgGeometryVM != null && PgGeometryVM.CanSaveGeometry())
            {
                AttributesListVM.SaveCommand.Execute(param);
                if (StyleVM != null)
                {
                    StyleVM.SaveStyle();
                }
                PgGeometryVM.SaveGeometry();
                base.CloseWindow();
            }
            else if (PgGeometryVM != null)
            {
                PgGeometryVM.SaveGeometry();
            }
            if (PgGeometryVM == null)
            {
                AttributesListVM.SaveCommand.Execute(param);
                if (StyleVM != null)
                {
                    StyleVM.SaveStyle();
                }
                base.CloseWindow();
            }
        }
        bool CanSave(object param = null)
        {
            return !_isReadOnly;
        }
        #endregion

        #region Command: ReloadCommand
        /// <summary>
        /// Сохраненяет атрибуты объекта
        /// </summary>
        public ICommand ReloadCommand
        {
            get
            { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload)); }
        }
        public void Reload(object param = null)
        {
            AttributesListVM.ReloadCommand.Execute(param);
            if (FilesVM != null)
                FilesVM.Reload(param);
            new Thread(delegate()
            {
                LoadRelatedTables(_attributesListVM.PkAttribute.Value);
            }).Start();
        }
        #endregion

        #region Command: OpenHistoryCommand
        public ICommand OpenHistoryCommand
        {
            get { return _openHistoryCommand ?? (_openHistoryCommand = new RelayCommand(OpenHistory, CanOpenHistory)); }
        }
        private void OpenHistory(object obj = null)
        {
            if (!CanOpenHistory(obj))
                return;
            _source.OpenHistoryWindow(AttributesListVM.PkAttribute);
        }

        private bool CanOpenHistory(object obj)
        {
            return (!AttributesListVM.IsNew && Table.HasHistory);
        }
        #endregion

        #region Command: OpenTableCommand
        public ICommand OpenTableCommand
        {
            get { return _openTableCommand ?? (_openTableCommand = new RelayCommand(this.OpenTable)); }
        }
        private void OpenTable(object obj = null)
        {
            _source.OpenTable(Table, AttributesListVM.PkAttribute.Value);
        }
        #endregion
        #endregion Команды

        #region Обработчики
        protected override bool Closing(object obj)
        {
            if (PgGeometryVM != null)
            {
                PgGeometryVM.HidePoints();
                PgGeometryVM.HidePreview();
            }
            base.Closing(obj);
            return true;
        }
        #endregion Обработчики
    }
}