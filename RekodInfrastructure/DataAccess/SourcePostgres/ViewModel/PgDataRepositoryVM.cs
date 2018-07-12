using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Interfaces;
using Npgsql;
using System.Collections.ObjectModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using Pg = Rekod.DataAccess.SourcePostgres;
using PgV = Rekod.DataAccess.SourcePostgres.View;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using PgTV_VM = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using Pg_VM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AxmvMapLib;
using mvMapLib;
using System.Windows.Input;
using System.Drawing;
using Rekod.Controllers;
using System.Windows;
using TmM = Rekod.DataAccess.TableManager.Model;
using TmVM = Rekod.DataAccess.TableManager.ViewModel;
using Rekod.Services;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    /// <summary>
    /// Класс работы с источником Postgres
    /// </summary>
    public class PgDataRepositoryVM : AbsVM.DataRepositoryVM
    {
        #region Поля
        private Version ActualVersion = new Version(2, 9, 0, 0);
        private bool _isDebug = false;      // Включить дебаг режим
        private readonly NpgsqlConnectionStringBuilder _connect;
        private int _idUser = 0;
        private PgM.PgStyleSetsM _currentSet;
        private PgM.PgStyleSetsM _defaultSet;
        private ObservableCollection<PgM.PgTableBaseM> _catalogTables = null;
        private ObservableCollection<PgM.PgTableBaseM> _intervalTables = null;
        private ObservableCollection<PgM.PgTableBaseM> _dataTables = null;
        private ObservableCollection<SourcePostgres.Model.PgUserM> _users = null;
        private ObservableCollection<PgM.PgExtentM> _extents = null;
        private ObservableCollection<AbsM.GroupM> _thematicGroups = null;
        private ObservableCollection<PgM.PgStyleSetsM> _sets = null;
        private ObservableCollection<PgM.PgActionM> _actions = null;
        private ObservableCollection<PgM.PgUserRightsM> _rightsStruct = null;
        private ObservableCollection<String> _schems = null;
        private ObservableCollection<PgActionRightM> _actionRightsList = null;

        private IExternalSource _postgresMapAdapter = null;
        private ICommand _addLayerToDataContextCommand;
        private ICommand _addGroupToDataContextCommand;
        private ICommand _removeTableCommand;
        private ICommand _removeGroupCommand;
        private ICommand _saveFieldCommandCommand;
        private ICommand _removeFieldCommand;
        private ICommand _openHistoryWindowCommand;
        private ICommand _addNewUserCommand;
        private ICommand _deleteUserCommand;
        private ICommand _createObjectCommand;
        private ICommand _createObjectInMapCommand;
        private ICommand _copyTableCommand;
        private event EventHandler<PgAtM.PgAttributeEventArgs> _eventUpdateAttribute;
        private AxMapLIb _axMapLib1;
        private TmVM.TableManagerVM _tableManager;
        private Services.DialogService _dialogService = Services.DialogService.DialogServiceObject;
        private PgM.PgUserM _currentUser;

        private Pg_VM.PgGroupsVM _pgGroupsVM;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Текущий рабочий набор
        /// </summary>
        public PgM.PgStyleSetsM CurrentSet
        {
            get { return _currentSet; }
            set { OnPropertyChanged(ref _currentSet, value, () => this.CurrentSet); }
        }
        /// <summary>
        /// Рабочий набор по умолчанию
        /// </summary>
        public PgM.PgStyleSetsM DefaultSet
        {
            get { return _defaultSet; }
            set { OnPropertyChanged(ref _defaultSet, value, () => this.DefaultSet); }
        }
        /// <summary>
        /// Соединение с базое
        /// </summary>
        public NpgsqlConnectionStringBuilder Connect
        {
            get { return _connect; }
        }
        /// <summary>
        /// Пользователь, под которым было установлено соединение с базой
        /// </summary>
        public PgM.PgUserM CurrentUser
        {
            get
            {
                return _currentUser ?? (_currentUser = Users.FirstOrDefault(p => p.ID == _idUser));
            }
        }
        public PgListUserRightsVM ListUserRightsVM
        {
            get { return new PgListUserRightsVM(this); }
        }
        /// <summary>
        /// Заголовок источника
        /// </summary>
        public override String Title
        {
            get
            {
                if (CurrentUser != null)
                {
                    return String.Format("{0}@{1}:{2} ({3})",
                        Connect.Database,
                        Connect.Host,
                        Connect.Port,
                        CurrentUser.NameFull);
                }
                else
                {
                    return "";
                }
            }
        }
        public PgGroupsVM PgGroupsVM
        {
            get { return _pgGroupsVM ?? (_pgGroupsVM = new PgGroupsVM(this)); }
        }
        #endregion

        #region События
        /// <summary>
        /// Событие обнавление атрибутов
        /// </summary>
        public event EventHandler<PgAtM.PgAttributeEventArgs> EventUpdateAttribute
        {
            add { lock (this) { _eventUpdateAttribute += value; } }
            remove { lock (this) { _eventUpdateAttribute -= value; } }
        }
        #endregion // События

        #region Коллекции
        public ObservableCollection<SourcePostgres.Model.PgUserM> Users
        {
            get { return _users ?? (_users = new ObservableCollection<SourcePostgres.Model.PgUserM>()); }
        }
        /// <summary>
        /// Список справочников
        /// </summary>
        public ObservableCollection<PgM.PgTableBaseM> CatalogTables
        {
            get { return _catalogTables; }
        }
        /// <summary>
        /// Список интервалов
        /// </summary>
        public ObservableCollection<PgM.PgTableBaseM> IntervalTables
        {
            get { return _intervalTables; }
        }
        /// <summary>
        /// Список таблиц с данными
        /// </summary>
        public ObservableCollection<PgM.PgTableBaseM> DataTables
        {
            get { return _dataTables; }
        }
        /// <summary>
        /// Список наборов
        /// </summary>
        public ObservableCollection<PgM.PgStyleSetsM> Sets
        {
            get { return _sets; }
        }
        /// <summary>
        /// Список схем
        /// </summary>
        public ObservableCollection<String> Schems
        {
            get { return _schems; }
        }
        /// <summary>
        /// Список экстентов
        /// </summary>
        public ObservableCollection<PgM.PgExtentM> Extents
        {
            get { return _extents; }
        }
        /// <summary>
        /// Список действий над таблицами
        /// </summary>
        public ObservableCollection<PgM.PgActionM> Actions
        {
            get { return _actions; }
        }
        public ObservableCollection<PgM.PgUserRightsM> RightsStructure
        {
            get { return _rightsStruct; }
            set { _rightsStruct = value; }
        }
        #endregion // Коллекции

        #region Конструкторы
        /// <summary>
        /// Источник Postgres
        /// </summary>
        /// <param name="connect">Параметры соединения с базой</param>
        /// <param name="isDebug">Включить полный вывод ошибок</param>
        /// <exception cref="ArgumentNullException"/>
        public PgDataRepositoryVM(TmM.ITableManagerVM source, NpgsqlConnectionStringBuilder connect, bool isDebug = false)
            : base(source, AbsM.ERepositoryType.Postgres, true)
        {
            _catalogTables = new ObservableCollection<PgM.PgTableBaseM>();
            _intervalTables = new ObservableCollection<PgM.PgTableBaseM>();
            _dataTables = new ObservableCollection<PgM.PgTableBaseM>();
            _sets = new ObservableCollection<PgM.PgStyleSetsM>();
            _schems = new ObservableCollection<string>();
            _extents = new ObservableCollection<PgM.PgExtentM>();
            _actions = new ObservableCollection<PgM.PgActionM>();
            _rightsStruct = new ObservableCollection<PgM.PgUserRightsM>();
            if (Program.mainFrm1 != null)
                _axMapLib1 = Program.mainFrm1.axMapLIb1;
            _tableManager = source as TmVM.TableManagerVM;
            if (connect == null)
                throw new ArgumentNullException("connect");
            _connect = connect;
            _isDebug = isDebug;
        }
        #endregion // Конструкторы

        #region Методы
        /// <summary>
        /// Обновление списка схем
        /// </summary>
        public void UpdateSchems()
        {
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT name FROM sys_scheme.table_schems");
                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    String name = sqlWork.GetString(0);
                    if (!Schems.Contains(name))
                    {
                        Schems.Add(name);
                    }
                }
                sqlWork.Close();
            }
        }
        /// <summary>
        /// Загрузка и обновление списка групп
        /// </summary>
        /// <param name="idGroup"></param>
        public void UpdateGroups(int? idGroup = null)
        {
            using (SqlWork sqlWork = new SqlWork(_connect, _isDebug))
            {
                sqlWork.sql =
                    String.Format(@" 
                                    SELECT
                                        id, 
                                        name_group, 
                                        descript, 
                                        order_num 
                                    FROM sys_scheme.table_groups 
                                    ORDER BY 
                                            order_num");
                sqlWork.ExecuteReader();
                List<AbsM.IGroupM> listGroups = new List<AbsM.IGroupM>();
                while (sqlWork.CanRead())
                {
                    int id = sqlWork.GetInt32("id");
                    var group = FindGroup(id) as AbsM.GroupM;
                    if (group == null)
                        group = new AbsM.GroupM(this, id);


                    group.Text = sqlWork.GetString("name_group");
                    group.Description = sqlWork.GetString("descript");

                    listGroups.Add(group);
                }
                ExtraFunctions.Sorts.SortList(_groups, listGroups);
                sqlWork.Close();
            }
        }
        /// <summary>
        /// Загрузка и обновление коллекии методанных таблиц postgres
        /// </summary>
        /// <param name="id">Указать id, если нужно обновить одну таблицу</param>
        public void UpdateTables(int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32),
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql =
                    string.Format(@"
                                    SELECT
                                        ti.id,
                                        ti.type,
                                        ti.scheme_name,
                                        ti.name_db,
                                        ti.name_map,
                                        ti.pk_fileld,
                                        ti.hidden,
                                        ti.has_history,
                                        ti.has_photo,
                                        ti.read_only,
                                        ti.map_style,
                                        ti.view_name,
                                        (isv IS NOT NULL) AS is_view,
                                        ti.ref_table,
                                        ti.geom_field,
                                        ti.geom_type,
                                        gc.srid,
                                        tr.write_data,
                                        tr.read_data
                                    FROM {0}.table_info ti
                                        LEFT JOIN {0}.table_right tr
                                                ON (ti.id = tr.id_table)
                                        LEFT JOIN geometry_columns gc
                                                ON (ti.scheme_name = gc.f_table_schema
                                                AND ti.name_db = gc.f_table_name
                                                AND ti.geom_field = gc.f_geometry_column)
                                        LEFT JOIN information_schema.views isv 
                                                ON (ti.scheme_name = isv.table_schema
                                                AND ti.name_db = isv.table_name)
                                    WHERE
                                        tr.id_user = :id_user
                                        {1}
                                    ORDER BY
                                        ti.name_map;",
                                                         "sys_scheme",
                                                         (idTable != null) ? "AND ti.id = :id_table" : null);
                List<AbsM.ITableBaseM> ListTable = new List<AbsM.ITableBaseM>();
                sqlCmd.ExecuteReader(listParam);
                while (sqlCmd.CanRead())
                {
                    var id = sqlCmd.GetInt32("id");
                    // 1 - слой карты, 2 - справочник, 3 - интервал, 4 - таблица с данными
                    var type = sqlCmd.GetValue<AbsM.ETableType>("type");
                    var srid = sqlCmd.GetValue<int?>("srid");

                    var table = FindTable(id) as PgM.PgTableBaseM;
                    if (table == null)
                        table = new PgM.PgTableBaseM(this, id, srid, type);

                    ListTable.Add(table);

                    Debug.Assert(table != null, "table != null");

                    table.SchemeName = sqlCmd.GetString("scheme_name");
                    table.Name = sqlCmd.GetString("name_db");
                    table.Text = sqlCmd.GetString("name_map");
                    table.PrimaryKey = sqlCmd.GetString("pk_fileld");
                    table.IsHidden = sqlCmd.GetBoolean("hidden");
                    table.HasHistory = sqlCmd.GetBoolean("has_history");
                    table.HasFiles = sqlCmd.GetBoolean("has_photo");
                    table.IsReadOnly = sqlCmd.GetBoolean("read_only");
                    table.ViewName = sqlCmd.GetString("view_name");
                    table.IsView = sqlCmd.GetBoolean("is_view");
                    table.ReferenceTable = sqlCmd.GetValue<int?>("ref_table");

                    bool canRead = sqlCmd.GetBoolean("read_data");
                    bool canWrite = sqlCmd.GetBoolean("write_data");
                    table.CanWrite = canWrite;
                    table.UserAccess = AbsM.UserAccess.None;
                    if (canRead)
                    {
                        table.UserAccess = AbsM.UserAccess.Read;
                    }
                    if (canWrite)
                    {
                        table.UserAccess = AbsM.UserAccess.Write;
                    }

                    switch (table.Type)
                    {
                        case AbsM.ETableType.Catalog:
                        case AbsM.ETableType.Interval:
                            {
                                table.IsMapStyle = sqlCmd.GetBoolean("map_style");
                            }
                            break;
                        case AbsM.ETableType.MapLayer:
                            {
                                // 0 - без геометрии, 1 - точки, 2 - линии, 3 - площадные объекты
                                table.GeomType = sqlCmd.GetValue<AbsM.EGeomType>("geom_type");
                                table.GeomField = sqlCmd.GetString("geom_field");
                            }
                            break;
                    }
                }
                if (idTable == null)
                    ExtraFunctions.Sorts.SortList(_tables, ListTable);
                else
                {
                    var table = FindTable(idTable) as PgM.PgTableBaseM;
                    if (ListTable.Count == 0)
                    {
                        if (table != null)
                            DBRemoveTable(table);
                    }
                    else
                    {
                        if (table == null)
                            _tables.Add(ListTable[0]);
                    }

                }
            }
            UpdateFileInfo(idTable);
        }
        /// <summary>
        /// Загрузка и обновление коллекии методанных таблиц postgres (минимум информации)
        /// </summary>
        /// <param name="id">Указать id, если нужно обновить одну таблицу</param>
        public void UpdatePartTables(int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32),
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql =
                    string.Format(@"
                                    SELECT
                                        ti.id,
                                        ti.type,
                                        ti.pk_fileld, 
                                        ti.scheme_name,
                                        ti.name_db,
                                        ti.name_map, 
                                        ti.geom_type, 
                                        ti.geom_field,
                                        ti.map_style,
                                        gc.srid,
                                        gc.type as gc_type_geom
                                    FROM {0}.table_info ti
                                            LEFT JOIN geometry_columns gc
                                            ON (ti.scheme_name = gc.f_table_schema
                                            AND ti.name_db = gc.f_table_name
                                            AND ti.geom_field = gc.f_geometry_column)
                                    ORDER BY
                                        ti.name_map;",
                                                         "sys_scheme",
                                                         (idTable != null) ? "AND ti.id = :id_table" : null);
                List<AbsM.ITableBaseM> ListTable = new List<AbsM.ITableBaseM>();
                sqlCmd.ExecuteReader(listParam);
                while (sqlCmd.CanRead())
                {
                    var id = sqlCmd.GetInt32("id");
                    // 1 - слой карты, 2 - справочник, 3 - интервал, 4 - таблица с данными
                    var type = sqlCmd.GetValue<AbsM.ETableType>("type");
                    var srid = sqlCmd.GetValue<int?>("srid");
                    var table = FindTable(id) as PgM.PgTableBaseM;
                    if (table == null)
                        table = new PgM.PgTableBaseM(this, id, srid, type);

                    ListTable.Add(table);

                    Debug.Assert(table != null, "table != null");

                    table.SchemeName = sqlCmd.GetString("scheme_name");
                    table.Name = sqlCmd.GetString("name_db");
                    table.Text = sqlCmd.GetString("name_map");
                    table.PrimaryKey = sqlCmd.GetString("pk_fileld");
                    //if (type == AbsM.ETableType.MapLayer)
                    //{
                    //    table.GeomType = sqlCmd.GetValue<AbsM.EGeomType>("geom_type");
                    //}
                    switch (table.Type)
                    {
                        case AbsM.ETableType.Catalog:
                        case AbsM.ETableType.Interval:
                            {
                                table.IsMapStyle = sqlCmd.GetBoolean("map_style");
                            }
                            break;
                        case AbsM.ETableType.MapLayer:
                            {
                                // 0 - без геометрии, 1 - точки, 2 - линии, 3 - площадные объекты
                                table.GeomType = sqlCmd.GetValue<AbsM.EGeomType>("geom_type");
                                table.GC_GeomType = sqlCmd.GetString("gc_type_geom");
                                table.GeomField = table.GeomType != AbsM.EGeomType.None ? sqlCmd.GetString("geom_field") : null;
                            }
                            break;
                    }
                }
                if (idTable == null)
                    ExtraFunctions.Sorts.SortList(_tables, ListTable);
                else
                {
                    var table = FindTable(idTable) as PgM.PgTableBaseM;
                    if (ListTable.Count == 0)
                    {
                        if (table != null)
                            DBRemoveTable(table);
                    }
                    else
                    {
                        if (table == null)
                            _tables.Add(ListTable[0]);
                    }

                }
            }
        }
        /// <summary>
        /// Загружает права пользователей
        /// </summary>
        public void LoadUserRights()
        {
            PgM.PgUserM user = this.CurrentUser;
            if (user.ID == null)
                return;

            var tempRights = new List<PgM.PgUserRightsM>();
            Dictionary<int, int[]> refs = new Dictionary<int, int[]>();

            using (var sqlCmd = new SqlWork(this.Connect))
            {
                //                sqlCmd.sql = @"SELECT ti.id as id, 
                //                                    ti.scheme_name as scheme_name,  
                //                                    ti.name_db as name_db,  
                //                                    ti.name_map as name_map,  
                //                                    ti.type as type,  
                //                                    r.id as right_id,  
                //                                    COALESCE(r.read_data, FALSE) as read_data, 
                //                                    COALESCE(r.write_data, FALSE) as write_data
                //                                    FROM
                //                                        sys_scheme.table_info ti 
                //                                        LEFT OUTER JOIN  
                //                                            sys_scheme.table_right r  
                //                                        	ON r.id_table = ti.id and id_user = " + user.ID;
                sqlCmd.sql = @"SELECT ti.id as id, 
                                    ti.scheme_name as scheme_name,  
                                    ti.name_db as name_db,  
                                    ti.name_map as name_map,  
                                    ti.type as type,  
                                    r.id as right_id,  
                                    COALESCE(r.read_data, FALSE) as read_data, 
                                    COALESCE(r.write_data, FALSE) as write_data,
                                    (SELECT sys_scheme.array_accum(ref_table) FROM sys_scheme.get_ref_tables(ti.id) as ref_table) as refs 
                                    FROM
                                        sys_scheme.table_info ti 
                                        LEFT OUTER JOIN  
                                            sys_scheme.table_right r  
                                        	ON r.id_table = ti.id and id_user = " + user.ID;
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var right = new PgM.PgUserRightsM();
                    right.User = user;
                    right.TableId = sqlCmd.GetInt32("id");
                    right.TableName = sqlCmd.GetString("name_db");
                    right.TableScheme = sqlCmd.GetString("scheme_name");
                    right.TableText = sqlCmd.GetString("name_map");

                    switch (sqlCmd.GetInt32("type"))
                    {
                        case 1:
                            right.TableType = DataAccess.AbstractSource.Model.ETableType.MapLayer;
                            break;
                        case 2:
                            right.TableType = DataAccess.AbstractSource.Model.ETableType.Catalog;
                            break;
                        case 3:
                            right.TableType = DataAccess.AbstractSource.Model.ETableType.Interval;
                            break;
                        case 4:
                            right.TableType = DataAccess.AbstractSource.Model.ETableType.Data;
                            break;
                    }
                    int id = sqlCmd.GetInt32("right_id");
                    if (id != 0)
                        right.ID = id;
                    right.CanRead = sqlCmd.GetBoolean("read_data");
                    right.CanWrite = sqlCmd.GetBoolean("write_data");

                    refs.Add(right.TableId, sqlCmd.GetValue<int[]>("refs"));
                    tempRights.Add(right);
                }
                sqlCmd.Close();
            }

            foreach (var right in tempRights)
            {
                //right.RefTables =  GetTableChildrenRightsDB(right.TableId, user, tempRights);
                right.RefTables = new List<PgUserRightsM>();
                foreach (var id in refs[right.TableId])
                {
                    right.RefTables.Add(tempRights.Find(w => w.TableId == id &&
                                w.User == user));
                }
            }
            ExtraFunctions.Sorts.SortList(_rightsStruct, tempRights);
        }
        /// <summary>
        /// Получает права всех зависимых таблиц
        /// </summary>
        /// <param name="id_table">Таблица-родитель</param>
        /// <param name="user">Пользователь, что права ищутся</param>
        /// <returns></returns>
        public List<PgM.PgUserRightsM> GetTableChildrenRightsDB(int id_table, PgM.PgUserM user, List<PgM.PgUserRightsM> rights)
        {
            List<PgM.PgUserRightsM> tempRights = new List<PgM.PgUserRightsM>();
            using (SqlWork sqlCmd = new SqlWork(this.Connect))
            {
                sqlCmd.sql = string.Format(@"SELECT ref.ref_table as id_table,
                                                COALESCE(id, -1) AS id, 
                                                COALESCE(read_data, FALSE) AS read_data, 
                                                COALESCE(write_data, FALSE) AS write_data
			                                    FROM 
				                                    (SELECT {0}.get_ref_tables({3}) as ref_table) as ref
				                                    LEFT OUTER JOIN 
				                                    {0}.{1} r 
					                                    ON r.id_table = ref.ref_table AND r.id_user={2}",
                        "sys_scheme", "table_right", user.ID, id_table);

                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int id = sqlCmd.GetInt32("id");
                    int idTable = sqlCmd.GetInt32("id_table");
                    bool canread = sqlCmd.GetBoolean("read_data");
                    bool canwrite = sqlCmd.GetBoolean("write_data");

                    PgM.PgUserRightsM right = null;
                    if (id > 0)
                    {
                        right = rights.FirstOrDefault(w => w.ID == id);
                    }
                    if (right == null)
                    {
                        right = rights.FirstOrDefault(w => w.TableId == idTable && w.User == user);
                        if (right == null)
                        {
                            right = new PgM.PgUserRightsM
                            {
                                ID = id,
                                CanRead = canread,
                                CanWrite = canwrite,
                                User = user,
                                Table = (PgM.PgTableBaseM)Tables.FirstOrDefault(w => (int)w.Id == idTable)
                            };
                            if (right.Table != null)
                            {
                                right.TableName = right.Table.Name;
                                right.TableText = right.Table.NameMap;
                            }
                            else continue;
                        }
                    }
                    tempRights.Add(right);
                }
            }
            return tempRights;
        }
        /// <summary>
        /// Загрузка и обновление информации об файлах
        /// </summary>
        /// <param name="idTable"></param>
        public void UpdateFileInfo(int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql =
                    string.Format(@"
                                    SELECT
                                        id_table, 
                                        id_field_tble,
                                        photo_table, 
                                        photo_field, 
                                        photo_file
                                    FROM 
                                        {0}.table_photo_info
                                    WHERE
                                        {1}
                                    ORDER BY
                                        id_table;",
                                                         "sys_scheme",
                                                         (idTable != null) ? "id_table = :id_table" : "1 = 1");

                var listFileInfo = new List<AbsM.ITableBaseM>();
                if (idTable == null)
                    listFileInfo.AddRange(Tables);

                sqlCmd.ExecuteReader(listParam);
                while (sqlCmd.CanRead())
                {
                    idTable = sqlCmd.GetInt32("id_table");
                    var table = FindTable(idTable) as PgM.PgTableBaseM;
                    if (table == null)
                        continue;

                    if (table.FileInfo == null)
                        table.FileInfo = new PgM.PgFileInfoM(table);
                    table.FileInfo.TableName = sqlCmd.GetString("photo_table");
                    table.FileInfo.FieldId = sqlCmd.GetString("id_field_tble");
                    table.FileInfo.FieldIdObj = sqlCmd.GetString("photo_field");
                    table.FileInfo.FieldFile = sqlCmd.GetString("photo_file");
                    listFileInfo.Remove(table);
                }
                //Удаление устаревшей информации о таблице
                foreach (PgM.PgTableBaseM item in listFileInfo)
                {
                    if (item.FileInfo != null)
                        item.FileInfo = null;
                }
            }
        }
        /// <summary>
        /// Загрузка и обновление коллекии методанных полей таблиц postgres
        /// </summary>
        public void UpdateFields(int? idField = null, int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32),
                                    new Params(":id_field", idField, DbType.Int32),
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {

                sqlCmd.sql = string.Format(@"
                    SELECT 
                        tfi.id, 
                        tfi.id_table, 
                        tfi.type_field, 
                        tfi.name_db, 
                        tfi.name_map, 
                        tfi.name_lable, 
                        tfi.read_only,
                        tfi.visible,
                        tfi.ref_type
                    FROM 
                        {0}.table_field_info tfi
                        LEFT JOIN {0}.table_right tr 
                                ON (tfi.id_table = tr.id_table) 
                    WHERE 
                        tr.read_data = true 
                        AND tr.id_user = :id_user 
                        {1}
                        {2}
                    ORDER BY 
                        tfi.id_table,
                        num_order;",
                        "sys_scheme",
                        (idField != null) ? "AND tfi.id = :id_field" : null,
                        (idTable != null) ? "AND tfi.id_table = :id_table" : null);


                sqlCmd.ExecuteReader(listParam);

                int? oldTableId = null;
                PgM.PgTableBaseM table = null;
                List<AbsM.IFieldM> listFields = new List<AbsM.IFieldM>();

                while (sqlCmd.CanRead())
                {
                    PgM.PgFieldM field = null;

                    int fieldId = sqlCmd.GetInt32("id");
                    int tableId = sqlCmd.GetInt32("id_table");

                    if (oldTableId != tableId) // меняется текущая таблица
                    {
                        if (table != null)
                            ExtraFunctions.Sorts.SortList(table.Fields, listFields); // Сортируем список полей по шаблону созданному только что
                        listFields.Clear();

                        // обнавляем информацию о таблице
                        table = FindTable(tableId) as PgM.PgTableBaseM;
                        if (table == null)
                            continue;           // Если таблицы значит ошибка
                        oldTableId = tableId;
                    }

                    field = (PgM.PgFieldM)table.Fields.FirstOrDefault(f => f.Id == fieldId);
                    if (field == null)      //если его нет создаем
                        field = new PgM.PgFieldM(table, fieldId);

                    listFields.Add(field);

                    // Заполняем текущее поле 
                    field.Type = sqlCmd.GetValue<AbsM.EFieldType>("type_field");
                    field.Name = sqlCmd.GetString("name_db");
                    field.Text = sqlCmd.GetString("name_map");
                    field.Description = sqlCmd.GetString("name_lable");
                    field.IsReadOnly = sqlCmd.GetBoolean("read_only");
                    field.IsVisible = sqlCmd.GetBoolean("visible");

                    // Убираем связь если ее нет
                    if (sqlCmd.GetValue<AbsM.ERefType>("ref_type") == AbsM.ERefType.None)
                    {
                        field.RefType = AbsM.ERefType.None;
                        field.RefTable = null;
                        field.RefField = null;
                        field.RefFieldEnd = null;
                        field.RefFieldName = null;
                    }
                    // Устанавлкиваем ссылку на поле с Primary Key
                    if (table.PrimaryKey == field.Name)
                    {
                        table.PrimaryKeyField = field;
                        field.IsReadOnly = true;
                    }
                }
                // Применяем изменения к последней таблицы
                if (table != null)
                    ExtraFunctions.Sorts.SortList(table.Fields, listFields); // Сортируем список полей по шаблону созданному только что
            }

            // Обновим ссылки на таблицы
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql = string.Format(@"
                    SELECT 
                        tfi.id,
                        tfi.id_table, 
                        tfi.ref_type,
                        tfi.ref_table, 
                        tfi.ref_field, 
                        tfi.ref_field_end, 
                        tfi.ref_field_name
                    FROM {0}.table_field_info tfi
                        LEFT JOIN {0}.table_right tr 
                                            ON (tfi.id_table = tr.id_table)
                    WHERE tr.read_data = true 
                        AND tr.id_user = {1} 
                        AND tfi.ref_type <> 0
                        {2}
                    ORDER BY num_order;",
                        "sys_scheme",
                        _idUser,
                        (idField != null) ? ("AND tfi.id = " + idField) : (""));
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    PgM.PgTableBaseM table = null;
                    PgM.PgFieldM field = null;

                    int fieldId = sqlCmd.GetInt32("id");
                    int tableId = sqlCmd.GetInt32("id_table");

                    table = FindTable(tableId) as PgM.PgTableBaseM;     // Поиск таблицы
                    if (table == null)
                    {
                        Debug.Fail("При добавлении поля таблица не найдена", "Таблица №" + tableId);
                        continue;
                    }

                    field = FindField(table, fieldId) as PgM.PgFieldM;
                    if (field == null)      //если его нет создаем
                        table.Fields.Add(field = new PgM.PgFieldM(table, fieldId));

                    field.RefType = sqlCmd.GetValue<AbsM.ERefType>("ref_type");

                    field.IdRefTable = sqlCmd.GetValue<int>("ref_table");
                    field.IdRefField = sqlCmd.GetValue<int?>("ref_field");
                    field.IdRefFieldEnd = sqlCmd.GetValue<int?>("ref_field_end");
                    field.IdRefFieldName = sqlCmd.GetValue<int?>("ref_field_name");


                    var refTable = FindTable(field.IdRefTable) as PgM.PgTableBaseM;
                    if (refTable == null)    // Поиск таблицы, если ее нет значит доступа к таблицы нет
                    {
                        field.RefType = AbsM.ERefType.None;
                        continue;   // оставляем TypeRef как есть, будет признак что нет доступа к связанной таблице (при изменении свойств текущего поля)
                    }
                    var refField = FindField(refTable, field.IdRefField);
                    var refFieldEnd = FindField(refTable, field.IdRefFieldEnd);
                    var refFieldName = FindField(refTable, field.IdRefFieldName);

                    // Проверяем все ли поля загружены правильно
                    if (field.RefType == AbsM.ERefType.Interval)
                    {
                        if (refField == null
                            || refFieldName == null
                            || refFieldEnd == null)
                        {
                            field.RefType = AbsM.ERefType.None;
                            continue;
                        }
                    }
                    else
                    {
                        if (refField == null
                            || refFieldName == null)
                        {
                            field.RefType = AbsM.ERefType.None;
                            continue;
                        }
                        refFieldEnd = null;
                    }
                    field.RefTable = refTable;
                    field.RefField = (PgM.PgFieldM)refField;
                    field.RefFieldEnd = (PgM.PgFieldM)refFieldEnd;
                    field.RefFieldName = (PgM.PgFieldM)refFieldName;
                    // заполняем если поля все верны

                }
            }
        }
        /// <summary>
        /// Загрузка и обновление коллекии методанных полей таблиц postgres (минимум информации)
        /// </summary>
        public void UpdatePartFields(int? idField = null, int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32),
                                    new Params(":id_field", idField, DbType.Int32),
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {

                sqlCmd.sql = string.Format(@"
                    SELECT 
                        tfi.id, 
                        tfi.id_table, 
                        tfi.type_field, 
                        tfi.name_db, 
                        tfi.name_map, 
                        tfi.name_lable, 
                        tfi.read_only,
                        tfi.visible,
                        tfi.is_reference, 
                        tfi.is_interval,
                        tfi.ref_table
                    FROM 
                        {0}.table_field_info tfi
                        LEFT JOIN {0}.table_right tr 
                                ON (tfi.id_table = tr.id_table) 
                    WHERE 
                        {3} 
                        AND tr.id_user = :id_user 
                        {1}
                        {2}
                    ORDER BY 
                        tfi.id_table,
                        num_order;",
                        "sys_scheme",
                        (idField != null) ? "AND tfi.id = :id_field" : null,
                        (idTable != null) ? "AND tfi.id_table = :id_table" : null,
                        (!CurrentUser.IsAdmin) ? "tr.read_data = true" : "1=1");


                sqlCmd.ExecuteReader(listParam);

                int? oldTableId = null;
                PgM.PgTableBaseM table = null;
                List<AbsM.IFieldM> listFields = new List<AbsM.IFieldM>();

                while (sqlCmd.CanRead())
                {
                    PgM.PgFieldM field = null;

                    int fieldId = sqlCmd.GetInt32("id");
                    int tableId = sqlCmd.GetInt32("id_table");

                    if (oldTableId != tableId) // меняется текущая таблица
                    {
                        if (table != null)
                            ExtraFunctions.Sorts.SortList(table.Fields, listFields); // Сортируем список полей по шаблону созданному только что
                        listFields.Clear();

                        // обнавляем информацию о таблице
                        table = FindTable(tableId) as PgM.PgTableBaseM;
                        if (table == null)
                            continue;           // Если таблицы значит ошибка
                        if (table.RefTables == null)
                        {
                            table.RefTables = new List<AbsM.ITableBaseM>();
                        }
                        else
                        {
                            table.RefTables.Clear();
                        }
                        oldTableId = tableId;
                    }

                    field = (PgM.PgFieldM)table.Fields.FirstOrDefault(f => f.Id == fieldId);
                    if (field == null)      //если его нет создаем
                        field = new PgM.PgFieldM(table, fieldId);

                    listFields.Add(field);

                    // Заполняем текущее поле 
                    field.Type = sqlCmd.GetValue<AbsM.EFieldType>("type_field");
                    field.Name = sqlCmd.GetString("name_db");
                    field.Text = sqlCmd.GetString("name_map");
                    field.Description = sqlCmd.GetString("name_lable");
                    field.IsReadOnly = sqlCmd.GetBoolean("read_only");
                    field.IsVisible = sqlCmd.GetBoolean("visible");
                    bool is_ref = sqlCmd.GetValue<Boolean>("is_reference");
                    bool is_int = sqlCmd.GetValue<Boolean>("is_interval");
                    field.RefType = AbsM.ERefType.None;
                    if (is_ref)
                    {
                        field.RefType = AbsM.ERefType.Reference;
                    }
                    else if (is_int)
                    {
                        field.RefType = AbsM.ERefType.Interval;
                    }

                    // Убираем связь если ее нет
                    if (field.RefType == AbsM.ERefType.None)
                    {
                        field.RefType = AbsM.ERefType.None;
                        field.RefTable = null;
                        field.RefField = null;
                        field.RefFieldEnd = null;
                        field.RefFieldName = null;
                    }
                    // Устанавлкиваем ссылку на поле с Primary Key
                    if (table.PrimaryKey == field.Name)
                    {
                        table.PrimaryKeyField = field;
                        field.IsReadOnly = true;
                    }
                    //var refTable = FindTable(sqlCmd.GetInt32("ref_table"));
                    //if (refTable != null)
                    //{
                    //    table.RefTables.Add(refTable);
                    //}
                }
                // Применяем изменения к последней таблицы
                if (table != null)
                    ExtraFunctions.Sorts.SortList(table.Fields, listFields); // Сортируем список полей по шаблону созданному только что
            }

            // Обновим ссылки на таблицы
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql = string.Format(@"
                    SELECT 
                        tfi.id,
                        tfi.id_table, 
                        tfi.is_reference,
                        tfi.is_interval,
                        tfi.ref_table, 
                        tfi.ref_field, 
                        tfi.ref_field_end, 
                        tfi.ref_field_name
                    FROM {0}.table_field_info tfi
                        LEFT JOIN {0}.table_right tr 
                                            ON (tfi.id_table = tr.id_table)
                    WHERE tr.read_data = true 
                        AND tr.id_user = {1} 
                        AND (tfi.is_reference OR tfi.is_interval)
                        {2}
                    ORDER BY num_order;",
                        "sys_scheme",
                        _idUser,
                        (idField != null) ? ("AND tfi.id = " + idField) : (""));
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    PgM.PgTableBaseM table = null;
                    PgM.PgFieldM field = null;

                    int fieldId = sqlCmd.GetInt32("id");
                    int tableId = sqlCmd.GetInt32("id_table");

                    table = FindTable(tableId) as PgM.PgTableBaseM;     // Поиск таблицы
                    if (table == null)
                    {
                        Debug.Fail("При добавлении поля таблица не найдена", "Таблица №" + tableId);
                        continue;
                    }

                    field = FindField(table, fieldId) as PgM.PgFieldM;
                    if (field == null)      //если его нет создаем
                        table.Fields.Add(field = new PgM.PgFieldM(table, fieldId));

                    bool is_ref = sqlCmd.GetValue<Boolean>("is_reference");
                    bool is_int = sqlCmd.GetValue<Boolean>("is_interval");
                    field.RefType = AbsM.ERefType.None;
                    if (is_ref)
                    {
                        field.RefType = AbsM.ERefType.Reference;
                    }
                    else if (is_int)
                    {
                        field.RefType = AbsM.ERefType.Interval;
                    }

                    field.IdRefTable = sqlCmd.GetValue<int>("ref_table");
                    field.IdRefField = sqlCmd.GetValue<int?>("ref_field");
                    field.IdRefFieldEnd = sqlCmd.GetValue<int?>("ref_field_end");
                    field.IdRefFieldName = sqlCmd.GetValue<int?>("ref_field_name");


                    var refTable = FindTable(field.IdRefTable) as PgM.PgTableBaseM;
                    if (refTable == null)    // Поиск таблицы, если ее нет значит доступа к таблицы нет
                    {
                        field.RefType = AbsM.ERefType.None;
                        continue;   // оставляем TypeRef как есть, будет признак что нет доступа к связанной таблице (при изменении свойств текущего поля)
                    }
                    else
                    {

                        table.RefTables.Add(refTable);
                    }
                    var refField = FindField(refTable, field.IdRefField);
                    var refFieldEnd = FindField(refTable, field.IdRefFieldEnd);
                    var refFieldName = FindField(refTable, field.IdRefFieldName);

                    // Проверяем все ли поля загружены правильно
                    if (field.RefType == AbsM.ERefType.Interval)
                    {
                        if (refField == null
                            || refFieldName == null
                            || refFieldEnd == null)
                        {
                            field.RefType = AbsM.ERefType.None;
                            continue;
                        }
                    }
                    else
                    {
                        if (refField == null
                            || refFieldName == null)
                        {
                            field.RefType = AbsM.ERefType.None;
                            continue;
                        }
                        refFieldEnd = null;
                    }
                    field.RefTable = refTable;
                    field.RefField = (PgM.PgFieldM)refField;
                    field.RefFieldEnd = (PgM.PgFieldM)refFieldEnd;
                    field.RefFieldName = (PgM.PgFieldM)refFieldName;
                    // заполняем если поля все верны
                }
            }
        }
        /// <summary>
        /// Загрузка и обновление коллекии группы
        /// </summary>
        public void UpdateTablesInGroups()
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32)
                                };
            using (SqlWork sqlWork = new SqlWork(_connect, _isDebug))
            {
                sqlWork.sql = String.Format(@"
                                            SELECT 
                                                tgi.id_group,
                                                tgi.id_table, 
                                                tgi.order_num
                                            FROM 
                                                sys_scheme.table_groups_table tgi
                                                LEFT JOIN sys_scheme.table_info ti 
                                                                        ON (tgi.id_table = ti.id) 
                                                LEFT JOIN sys_scheme.table_right tr
                                                                        ON (tgi.id_table = tr.id_table)
                                            WHERE
                                                tr.read_data = true
                                                AND tr.id_user = :id_user
                                                AND ti.type = 1
                                            GROUP BY tgi.id_group, -- кастыль из-за повторяющихся элементов
                                                tgi.id_table, 
                                                tgi.order_num
                                            ORDER BY 
                                                tgi.id_group ASC,
                                                tgi.order_num ASC;");
                sqlWork.ExecuteReader(listParam);

                int? old_id_group = null;
                AbsM.IGroupM group = null;
                List<AbsM.ILayerM> listTable = new List<AbsM.ILayerM>();

                while (sqlWork.CanRead())
                {
                    int id_group = sqlWork.GetInt32("id_group");
                    int id_table = sqlWork.GetInt32("id_table");

                    if (old_id_group != id_group)      // Сменилась ли группа
                    {
                        if (group != null)
                            ExtraFunctions.Sorts.SortList(group.Tables, listTable); // Сортируем группу по шаблону
                        listTable.Clear();

                        group = FindGroup(id_group);
                        if (group == null)
                            continue;

                        // обновляем информацию о группе
                        old_id_group = id_group;
                    }

                    // Добавляем таблицу в группу

                    var table = FindTable(id_table) as AbsM.TableBaseM;
                    if (table == null)
                    {
                        Debug.Fail("Таблица " + id_table + " ненайдена");
                        continue;
                    }
                    listTable.Add(table);
                }
                // Применяем изменения к последней группе
                if (group != null)
                    ExtraFunctions.Sorts.SortList(group.Tables, listTable); // Сортируем группу по шаблону

            }
        }
        /// <summary>
        /// Загрузка и обновление наборов стилей, а также заполняет CurrentSet если он пустой
        /// </summary>
        public void UpdateSets()
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql = string.Format(@"
                    SELECT 
                        ss.id, 
                        ss.owner_user, 
                        ss.is_default, 
                        ss.is_public, 
                        ss.name
                    FROM 
                        sys_scheme.style_sets ss
                        RIGHT JOIN sys_scheme.user_sets us 
                                ON (ss.id = us.id_set)
                    WHERE 
                        us.id_user = :id_user");

                sqlCmd.ExecuteReader(listParam);

                List<PgM.PgStyleSetsM> collectionForRemoveSets = new List<PgM.PgStyleSetsM>();
                collectionForRemoveSets.AddRange(Sets);

                while (sqlCmd.CanRead())
                {

                    var id = sqlCmd.GetInt32("id");

                    var current = collectionForRemoveSets.FirstOrDefault(f => f.Id == id);
                    if (current == null)
                        Sets.Add(current = new PgM.PgStyleSetsM(
                                                        id,
                                                        sqlCmd.GetInt32("owner_user"),
                                                        sqlCmd.GetBoolean("is_default"),
                                                        sqlCmd.GetBoolean("is_public")
                                                        ));
                    current.Name = sqlCmd.GetString("name");
                    collectionForRemoveSets.Remove(current);
                }

                //Удаление старых наборов
                for (int i = 0; i < collectionForRemoveSets.Count; i++)
                {
                    Sets.Remove(collectionForRemoveSets[i]);
                }
                // Устанавливаем дефолтный набор
                if (CurrentSet == null)
                {
                    DefaultSet = CurrentSet = Sets.FirstOrDefault(f => f.IsDefault == true);
                }
            }
        }
        /// <summary>
        /// Загрузка и обновление наборов стилей слоев
        /// </summary>
        public void UpdateLayerStyle(int? idTable = null)
        {
            var listParam = new List<Interfaces.IParams> 
                                {
                                    new Params(":id_user", _idUser, DbType.Int32),
                                    new Params(":id_table", idTable, DbType.Int32)
                                };
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql =
                    string.Format(@"
                                    SELECT
                                        sli.id,
                                        sli.id_table,
                                        sli.id_set,
                                        sli.style_type,
                                        sli.style_field,
                                        sli.image_column,
                                        sli.angle_column,
                                        sli.min_object_size,
                                        sli.use_bounds, min_scale,
                                        sli.max_scale,
                                        sli.rang_precision_point,
                                        sli.rang_type_color,
                                        sli.rang_min_color,
                                        sli.rang_min_val,
                                        sli.rang_max_color,
                                        sli.rang_max_val,
                                        sli.rang_use_min_val, 
                                        sli.rang_null_color,
                                        sli.rang_use_null_color,
                                        sli.rang_use_max_val,
                                        sli.graphic_units,
                                        sli.fontname,
                                        sli.fontcolor,
                                        sli.fontframecolor,
                                        sli.fontsize,
                                        sli.symbol,
                                        sli.pencolor,
                                        sli.pentype,
                                        sli.penwidth,
                                        sli.brushbgcolor,
                                        sli.brushfgcolor,
                                        sli.brushstyle,
                                        sli.brushhatch
                                    FROM 
                                        {0}.style_layer_info sli
                                        LEFT JOIN {0}.user_sets us 
                                                ON (sli.id_set = us.id_set)
                                        LEFT JOIN {0}.table_right tr
                                                ON (sli.id_table = tr.id_table)
                                    WHERE 
                                        us.id_user = :id_user
                                        AND tr.read_data = true
                                        AND tr.id_user = :id_user
                                        {1};",
                        "sys_scheme",
                        (idTable != null) ? "AND sli.id_table = :id_table" : null);

                sqlCmd.ExecuteReader(listParam);

                while (sqlCmd.CanRead())
                {
                    var id = sqlCmd.GetInt32("id");
                    var id_set = sqlCmd.GetInt32("id_set");
                    idTable = sqlCmd.GetInt32("id_table");

                    var set = Sets.FirstOrDefault(f => f.Id == id_set);
                    if (set == null)
                        new Exception("Запрос не должен возвращать таблицы стили от недоступных наборов");

                    PgM.PgTableBaseM table = null;

                    table = FindTable(idTable.Value) as PgM.PgTableBaseM;
                    if (table == null)
                        new Exception("Запрос не должен возвращать таблицы стили от недоступных таблиц");

                    PgM.PgStyleLayerM layer = null;
                    if (set.Layers.ContainsKey(table))
                        layer = set.Layers[table];
                    else
                        set.Layers.Add(table, layer = new PgM.PgStyleLayerM(id, table, set));

                    //sqlCmd.GetMethod(layer);
                    layer.StyleType = sqlCmd.GetValue<PgM.EStyleType>("style_type");

                    String styleFieldName = sqlCmd.GetString("style_field");
                    layer.StyleField = layer.Table.Fields.FirstOrDefault(p => p.Name == styleFieldName) as PgM.PgFieldM;

                    layer.ImageColumn = sqlCmd.GetString("image_column");
                    layer.AngleColumn = sqlCmd.GetString("angle_column");
                    layer.MinObjectSize = sqlCmd.GetInt32("min_object_size");
                    layer.UseBounds = sqlCmd.GetBoolean("use_bounds");
                    layer.MinScale = sqlCmd.GetInt32("min_scale");
                    layer.MaxScale = sqlCmd.GetInt32("max_scale");
                    layer.RangPrecisionPoint = sqlCmd.GetInt32("rang_precision_point");
                    layer.RangTypeColor = (PgM.EChangeColor)sqlCmd.GetInt32("rang_type_color");
                    layer.RangMinColor = sqlCmd.GetInt32("rang_min_color");
                    layer.RangMinVal = sqlCmd.GetInt32("rang_min_val");
                    layer.RangMaxVal = sqlCmd.GetInt32("rang_max_color");
                    // rang_use_min_val
                    layer.RangNullColor = sqlCmd.GetInt32("rang_null_color");
                    // rang_use_null_color
                    // rang_use_max_val
                    layer.GraphicUnits = sqlCmd.GetBoolean("graphic_units");
                    layer.FontName = sqlCmd.GetString("fontname");
                    layer.FontColor = sqlCmd.GetInt32("fontcolor");
                    layer.FontFrameColor = sqlCmd.GetInt32("fontframecolor");
                    layer.FontSize = sqlCmd.GetInt32("fontsize");
                    layer.Symbol = sqlCmd.GetInt32("symbol");
                    layer.PenColor = sqlCmd.GetInt32("pencolor");
                    layer.PenType = sqlCmd.GetInt32("pentype");
                    layer.PenWidth = sqlCmd.GetInt32("penwidth");
                    layer.BrushBgColor = sqlCmd.GetInt32("brushbgcolor");
                    layer.BrushFgColor = sqlCmd.GetInt32("brushfgcolor");
                    layer.BrushStyle = sqlCmd.GetInt32("brushstyle");
                    layer.BrushHatch = sqlCmd.GetInt32("brushhatch");
                }
            }
        }
        /// <summary>
        /// Загрузка и обновление наборов стилей подписей
        /// </summary>
        public void UpdateLabelStyle(int? idTable = null)
        {
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql =
                    string.Format(@"SELECT 
                                        sli.id,
                                        sli.id_table,
                                        sli.id_set,    
                                        sli.lable_mask, 
                                        sli.label_showframe,
                                        sli.label_framecolor, 
                                        sli.label_parallel,
                                        sli.label_overlap, 
                                        sli.label_usebounds, 
                                        sli.label_minscale, 
                                        sli.label_maxscale, 
                                        sli.label_offset, 
                                        sli.label_graphicunits, 
                                        sli.label_fontname, 
                                        sli.label_fontcolor, 
                                        sli.label_fontsize, 
                                        sli.label_fontstrikeout, 
                                        sli.label_fontitalic, 
                                        sli.label_fontunderline, 
                                        sli.label_fontbold, 
                                        sli.label_showlabel
                                    FROM sys_scheme.style_lable_info sli
                                        LEFT JOIN sys_scheme.user_sets us 
                                                ON (sli.id_set = us.id_set)
                                        LEFT JOIN sys_scheme.table_right tr
                                                ON (sli.id_table = tr.id_table)
                                    WHERE 
                                        us.id_user = {0}
                                        AND tr.read_data = true
                                        AND tr.id_user = {0}
                                        {1};",
                                    _idUser,
                                    (idTable != null) ? "AND sli.id_table = " + idTable : null);

                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var id = sqlCmd.GetInt32("id");
                    var id_set = sqlCmd.GetInt32("id_set");
                    idTable = sqlCmd.GetInt32("id_table");

                    var set = Sets.FirstOrDefault(f => f.Id == id_set);
                    if (set == null)
                        new Exception("Запрос не должен возвращать таблицы стили от недоступных наборов");

                    PgM.PgTableBaseM table = null;
                    table = FindTable(idTable.Value) as PgM.PgTableBaseM;
                    if (table == null)
                        new Exception("Запрос не должен возвращать таблицы стили от недоступных таблиц");

                    PgM.PgStyleLableM labelstyle = null;
                    if (set.Labels.ContainsKey(table))
                        labelstyle = set.Labels[table];
                    else
                        set.Labels.Add(table, labelstyle = new PgM.PgStyleLableM(id, table, set));


                    labelstyle.LableMask = sqlCmd.GetString("lable_mask");
                    labelstyle.LabelShowFrame = sqlCmd.GetBoolean("label_showframe");
                    labelstyle.LabelFrameColor = (uint)sqlCmd.GetInt32("label_framecolor");
                    labelstyle.LabelParallel = sqlCmd.GetBoolean("label_parallel");
                    labelstyle.LabelOverlap = sqlCmd.GetBoolean("label_overlap");
                    labelstyle.LabelUseBounds = sqlCmd.GetBoolean("label_usebounds");
                    labelstyle.LabelMinScale = (uint)sqlCmd.GetInt32("label_minscale");
                    labelstyle.LabelMaxScale = (uint)sqlCmd.GetInt32("label_maxscale");
                    labelstyle.LabelOffset = sqlCmd.GetInt32("label_offset");
                    labelstyle.LabelUseGraphicUnits = sqlCmd.GetBoolean("label_graphicunits");
                    labelstyle.LabelFontName = sqlCmd.GetString("label_fontname");
                    labelstyle.LabelFontColor = (uint)sqlCmd.GetInt32("label_fontcolor");
                    labelstyle.LabelFontSize = sqlCmd.GetInt32("label_fontsize");
                    labelstyle.LabelFontStrikeout = sqlCmd.GetBoolean("label_fontstrikeout");
                    labelstyle.LabelFontItalic = sqlCmd.GetBoolean("label_fontitalic");
                    labelstyle.LabelFontUnderline = sqlCmd.GetBoolean("label_fontunderline");
                    labelstyle.LabelFontBold = sqlCmd.GetBoolean("label_fontbold");
                    labelstyle.LabelShowLabel = sqlCmd.GetBoolean("label_showlabel");
                }
            }
        }
        /// <summary>
        /// Загрузка или обновление списка экстентов
        /// </summary>
        public void UpdateExtents(int? extentId = null)
        {
            var tempExtents = new ObservableCollection<PgM.PgExtentM>();
            using (var sqlCmd = new SqlWork(Connect))
            {
                sqlCmd.sql = "SELECT id, \"name\", extent FROM sys_scheme.map_extents";
                if (extentId != null)
                {
                    sqlCmd.sql += " WHERE id=" + extentId;
                }
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int id = sqlCmd.GetInt32("id");
                    string name = sqlCmd.GetString("name");
                    string extentstr = sqlCmd.GetString("extent");

                    PgM.PgExtentM extent = Extents.FirstOrDefault(p => p.Id == id);
                    if (extent != null)
                    {
                        extent.Name = name;
                        extent.SetExtent(extentstr);
                    }
                    else
                    {
                        extent = new PgM.PgExtentM(id, name, extentstr);
                    }
                    tempExtents.Add(extent);
                }
                sqlCmd.Close();
            }
            if (extentId == null)
            {
                ExtraFunctions.Sorts.SortList(Extents, tempExtents);
            }
            else
            {
                if (tempExtents.Count > 0 && !Extents.Contains(tempExtents[0]))
                {
                    Extents.Remove(tempExtents[0]);
                }
            }
        }
        /// <summary>
        /// Обновление списка действий над таблицами
        /// </summary>
        public void UpdateActions()
        {
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT id, name, table_data, name_visible, operation FROM sys_scheme.action");
                sqlWork.ExecuteReader();
                var actionsList = new ObservableCollection<PgM.PgActionM>();
                while (sqlWork.CanRead())
                {
                    PgM.PgActionM action = new PgM.PgActionM(
                        this,
                        sqlWork.GetInt32("id"),
                        sqlWork.GetString("name"),
                        sqlWork.GetString("name_visible"),
                        sqlWork.GetBoolean("table_data"),
                        sqlWork.GetBoolean("operation"));
                    actionsList.Add(action);
                }
                sqlWork.Close();
                ExtraFunctions.Sorts.SortList(_actions, actionsList);
            }
        }
        /// <summary>
        /// Метод перезагружает из базы всех пользователей либо одного из них
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        public void UpdateUsers(int? userId = null)
        {
            using (var sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql =
                    string.Format(@"
                        SELECT udb.id,
                               udb.pass, 
                               udb.name_full, 
                               udb.login, 
                               udb.otdel, 
                               udb.window_name, 
                               udb.typ, 
                               me.id as extent_id, 
                               me.name as extent_name, 
                               me.extent as extent_str
                        FROM sys_scheme.user_db udb
                        LEFT JOIN sys_scheme.user_params up ON up.id_user = udb.id AND up.name = 'id_map_extents'
                        LEFT JOIN sys_scheme.map_extents me ON me.id = up.values[1]::integer");

                if (userId != null)
                {
                    sqlCmd.sql += " where udb.id = " + userId;
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        updateOrCreateUser(userId, sqlCmd);
                        sqlCmd.CloseReader();
                    }
                    else
                    {
                        sqlCmd.CloseReader();
                        var user = Users.FirstOrDefault(u => u.ID == userId);
                        if (user != null)
                            Users.Remove(user);
                    }
                }
                else
                {
                    var tempUsers = new ObservableCollection<PgM.PgUserM>();
                    int id;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        id = sqlCmd.GetInt32("id");
                        tempUsers.Add(updateOrCreateUser(id, sqlCmd));
                    }
                    sqlCmd.CloseReader();
                    ExtraFunctions.Sorts.SortList(Users, tempUsers);
                }
            }
        }
        /// <summary>
        /// Проверяет есть ли объект с таким ID в коллекции. Если есть обновляет иначе добавляет
        /// </summary>
        /// <param name="userId">ID объекта</param>
        /// <param name="sqlCmd">Открытый читатель SQL откуда нужно взять данные</param>
        private PgM.PgUserM updateOrCreateUser(int? userId, SqlWork sqlCmd)
        {
            var user = Users.FirstOrDefault(u => u.ID == userId);
            if (user == null)
            {
                user = new PgM.PgUserM(this);
                Users.Add(user);
            }
            user.ID = sqlCmd.GetInt32("id");
            user.PassSync = sqlCmd.GetString("pass");
            user.NameFull = sqlCmd.GetString("name_full");
            user.Login = sqlCmd.GetString("login");
            user.Otdel = sqlCmd.GetString("otdel");
            user.WindowName = sqlCmd.GetString("window_name");
            user.Type = (PgM.UserType)sqlCmd.GetInt32("typ");

            int? extentId = sqlCmd.GetInt32("extent_id");
            if (extentId != null)
            {
                PgM.PgExtentM extent = Extents.FirstOrDefault(p => p.Id == (int)extentId);
                user.Extent = extent;
            }
            return user;
        }
        /// <summary>
        /// Сохраняет изменения в свойств полей в базе
        /// </summary>
        /// <param name="field"></param>
        public int? DBSaveField(PgM.PgFieldM field)
        {
            int? id_f = field.Id;

            var parms = new List<Interfaces.IParams> 
                        {
                            new Params(":id_table", field.IdTable, NpgsqlTypes.NpgsqlDbType.Integer),
                            new Params(":name_db", field.Name, NpgsqlTypes.NpgsqlDbType.Text),
                            new Params(":name_map", field.Text, NpgsqlTypes.NpgsqlDbType.Text),
                            new Params(":type", (int)field.Type, NpgsqlTypes.NpgsqlDbType.Integer),
                            new Params(":name_lable", field.Description, NpgsqlTypes.NpgsqlDbType.Text),
                            new Params(":read_only", field.IsReadOnly, NpgsqlTypes.NpgsqlDbType.Boolean),
                            new Params(":visible", field.IsVisible, NpgsqlTypes.NpgsqlDbType.Boolean), 
                            new Params(":ref_type", (short)field.RefType, NpgsqlTypes.NpgsqlDbType.Smallint),
                            new Params(":ref_table", field.RefTable != null ? (int?)field.RefTable.Id : null, NpgsqlTypes.NpgsqlDbType.Integer),
                            new Params(":ref_field", field.RefField != null ? (int?)field.RefField.Id : null, NpgsqlTypes.NpgsqlDbType.Integer),
                            new Params(":ref_field_end", field.RefFieldEnd != null ? (int?)field.RefFieldEnd.Id : null, NpgsqlTypes.NpgsqlDbType.Integer),
                            new Params(":ref_field_name", field.RefFieldName != null ? (int?)field.RefFieldName.Id : null, NpgsqlTypes.NpgsqlDbType.Integer)
                        };

            if (field.IsNew)
            {
                //Создаем новое поле
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = string.Format(@"
                                        SELECT 
                                            sys_scheme.create_field(
                                                :id_table,
                                                :name_db,
                                                :name_map,
                                                :type,
                                                :name_lable
                                                );");

                    sqlCmd.ExecuteReader(parms);
                    if (sqlCmd.CanRead())
                    {
                        id_f = sqlCmd.GetInt32(0);
                    }
                }
                if (id_f == null)
                    Debug.Fail("Поле не добавилось");
                else
                    parms[0].value = id_f.Value;
            }
            else
            {
                // Проверяем, все ли поля  видимы
                var table = (PgM.PgTableBaseM)field.Table;
                int k = 0;
                for (int i = table.Fields.Count - 1; i >= 0; i--)
                {
                    if ((table.Fields[i] as PgM.PgFieldM).IsVisible)
                        k++;
                }
                if (k == 0)
                {
                    System.Windows.MessageBox.Show("Таблица должна иметь не менее одного не скрытого атрибута. Если вы хотите скрыть все атрибуты таблицы, воспользуйтесь свойством 'скрытое' для таблицы", "Атрибуты таблиц", MessageBoxButton.OK, MessageBoxImage.Information);
                    parms[7].value = true;
                }

            }
            using (SqlWork sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql = string.Format(@"
                                                UPDATE 
                                                    sys_scheme.table_field_info 
                                                SET 
                                                    name_map = :name_map,
                                                    read_only = :read_only, 
                                                    visible = :visible, 
                                                    type_field = :type, 
                                                    name_lable = :name_lable, 
                                                    ref_table = :ref_table, 
                                                    ref_field = :ref_field, 
                                                    ref_field_end = :ref_field_end, 
                                                    ref_field_name = :ref_field_name, 
                                                    ref_type = :ref_type
                                                WHERE 
                                                    id = {0}", id_f);
                sqlCmd.ExecuteNonQuery(parms);
            }
            UpdateFields(idTable: field.IdTable);

            return id_f;
        }
        /// <summary>
        /// Сохрание таблицы в БД Postgres. Если таблица новая - создает новую таблицу в базе
        /// </summary>
        /// <param name="table">Таблица для сохранения</param>
        public void DBSaveTable(PgM.PgTableBaseM table)
        {
            // todo: (Диас) Проверить все свойства
            if (table == null)
                return;
            int id = 0;

            try
            {
                if (!table.IsNewTable && !table.CanWrite)
                    return;
                var listParam = new List<Interfaces.IParams> 
                {
                    new Params(":id", table.Id, DbType.Int32),
                    new Params(":type", table.Type, DbType.Int32),
                    new Params(":scheme_name", table.SchemeName, DbType.String),
                    new Params(":name_db", table.Name, DbType.String),
                    new Params(":name_map", table.Text, DbType.String),
                    new Params(":hidden", table.IsHidden, DbType.Boolean),
                    new Params(":has_history", table.HasHistory, DbType.Boolean),
                    new Params(":photo", table.HasFiles, DbType.Boolean),
                    new Params(":read_only", table.IsReadOnly, DbType.Boolean),
                    new Params(":map_style", table.IsMapStyle, DbType.Boolean),
                    new Params(":geom_field", table.GeomField, DbType.String),
                    new Params(":geom_type", table.GeomType, DbType.Int32)
                };

                if (table.IsNewTable)
                {
                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.sql = string.Format(@"
                                        SELECT 
                                            sys_scheme.table_create(
                                                :scheme_name,
                                                :name_db,
                                                :name_map,
                                                :type,
                                                :geom_type, 
                                                :map_style,
                                                :hidden) as id;");
                        sqlCmd.ExecuteReader(listParam);
                        if (sqlCmd.CanRead())
                        {
                            id = sqlCmd.GetInt32("id");
                            Params idParam = listParam.FirstOrDefault<Interfaces.IParams>(p => p.paramName == ":id") as Params;
                            if (idParam != null)
                            {
                                idParam.value = id;
                            }
                        }
                    }
                }
                else
                {
                    id = (int)table.Id;
                }

                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = string.Format(@"
                                        UPDATE 
                                            {0}.table_info 
                                        SET 
                                            name_map = :name_map, 
                                            hidden = :hidden,
                                            has_history = :has_history,
                                            has_photo = :photo,
                                            read_only = :read_only,
                                            map_style = :map_style
                                        WHERE id = :id;

                                        SELECT 
                                            has_history 
                                        FROM 
                                            {0}.table_info
                                        WHERE
                                            id = :id;",
                                        Program.scheme);
                    sqlCmd.ExecuteReader(listParam);
                    if (sqlCmd.CanRead())
                    {
                        table.HasHistory = sqlCmd.GetBoolean("has_history");
                    }
                }
            }
            finally
            {
                UpdateTables(id);
                UpdateFields(idTable: id);
                UpdateLayerStyle(idTable: id);
                UpdateLabelStyle(idTable: id);
            }
        }
        /// <summary>
        /// Сохрание группы в БД Postgres. Если группа новая - создает новую группу в базе
        /// </summary>
        /// <param name="table">Таблица для сохранения</param>
        public int? DBSaveGroup(AbsM.GroupM group)
        {
            int? id = null;
            if (group == null)
            {
                return null;
            }
            else
            {
                id = group.Id;
            }
            var listParam = new List<Interfaces.IParams> 
            {
                new Params(":name", group.Text, DbType.String),
                new Params(":description", group.Description, DbType.String),
            };

            if (group.IsNewGroup)
            {
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = string.Format(@"
                                            SELECT sys_scheme.group_create(
                                                :name,
                                                :description) as id;");
                    sqlCmd.ExecuteReader(listParam);
                    if (sqlCmd.CanRead())
                    {
                        id = (int)sqlCmd.GetInt32("id");
                    }
                }
            }
            else
            {
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = string.Format("UPDATE sys_scheme.table_groups SET name_group = '{0}', descript = '{1}' WHERE id={2}", group.Text, group.Description, id);
                    sqlCmd.ExecuteNonQuery();
                }
            }
            UpdateGroups(id);
            return id;
        }
        /// <summary>
        /// Удаление таблицы из БД Postgres
        /// </summary>
        /// <param name="table"></param>
        public void DBRemoveTable(PgM.PgTableBaseM table)
        {
            // todo: (Диас) В скрипте удаления таблицы также удалять записи о таблице в связанных наборах
            // todo: (Диас) Сделать возможность указания реального удаления таблицы
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT sys_scheme.delete_table({0}, {1})", table.Id, true);
                object obj = sqlWork.ExecuteScalar();
                _tables.Remove(table);
            }
        }
        /// <summary>
        /// Удаление поля из БД Postgres
        /// </summary>
        /// <param name="field"></param>
        public void DBRemoveField(PgM.PgFieldM field, bool realDelete)
        {
            // todo: (Диас) Сделать возможность указания реального удаления поля
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT sys_scheme.field_delete({0}, {1})", field.Id, realDelete);
                object obj = sqlWork.ExecuteScalar();
                MFieldRemove(field);
            }
        }
        /// <summary>
        /// Функция удаления поля из источника. Поле удаляется из коллекции полей связанной таблицы
        /// </summary>
        /// <param name="field">Поле которое необходимо удалить</param>
        public void MFieldRemove(AbsM.IFieldM field)
        {
            var fieldPg = field as PgM.PgFieldM;
            if (fieldPg != null)
            {
                (fieldPg.Table as PgM.PgTableBaseM).Fields.Remove(fieldPg);
            }
        }
        /// <summary>
        /// Удаление групппы из БД Postgres
        /// </summary>
        /// <param name="group"></param>
        private void DBRemoveGroup(AbsM.GroupM group)
        {
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT sys_scheme.group_delete({0})", group.Id);
                object obj = sqlWork.ExecuteScalar();
            }
        }
        /// <summary>
        /// Добавляет таблицу в группу или удаляет таблицу из группы в базе
        /// </summary>
        /// <param name="table">Таблица которую нужно переместить</param>
        /// <param name="group">Группа в которой происходит перемещение</param>
        /// <param name="add">Если true, таблица добавляется в группу</param>
        public bool TableGroupMove(PgM.PgTableBaseM table, AbsM.GroupM group, bool add)
        {
            // todo: (Диас) Узнавать порядковый номер таблицы в группе (Сергей) Можно с помощью index = Array.IndexOf(group.Tables, (AbsM.ILayerM)table)

            bool isok = false;
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                if (add)
                {
                    sqlWork.sql = String.Format("SELECT sys_scheme.group_set_table({0},{1}, null)", group.Id, table.Id);
                }
                else
                {
                    sqlWork.sql = String.Format("SELECT sys_scheme.group_delete_table({0},{1})", group.Id, table.Id);
                }
                isok = sqlWork.ExecuteScalar<bool>();
            }
            if (add)
            {
                MGroupAddTable(group, table);
            }
            else
            {
                MGroupRemoveTable(group, table);
            }
            return isok;
        }
        /// <summary>
        /// Сохранение информации о стиле в базе данных
        /// </summary>
        /// <param name="style">Стиль который необходимо сохранить</param>
        public void DBSaveStyle(PgM.PgStyleLayerM style)
        {
            var listParam = new List<Interfaces.IParams>
                {
                    new Params(":style_type",style.StyleType,DbType.Int32),
                    new Params(":style_field", style.StyleField !=null ? style.StyleField.Name : "" ,DbType.String),
                    new Params(":image_column",style.ImageColumn != null ? style.ImageColumn : "",DbType.String),
                    new Params(":angle_column",style.AngleColumn != null ? style.AngleColumn : "",DbType.String),
                    new Params(":min_object_size",style.MinObjectSize,DbType.Int32),
                    new Params(":use_bounds",style.UseBounds,DbType.Boolean),
                    new Params(":min_scale",style.MinScale,DbType.Int32),
                    new Params(":max_scale",style.MaxScale,DbType.Int32),
                    new Params(":rang_precision_point",style.RangPrecisionPoint,DbType.Int32),
                    new Params(":rang_type_color",(int)style.RangTypeColor,DbType.Int32),
                    new Params(":rang_min_color",style.RangMinColor,DbType.Int64),
                    new Params(":rang_min_val",style.RangMinVal,DbType.Int32),
                    new Params(":rang_max_color",style.RangMaxColor,DbType.Int64),
                    new Params(":rang_max_val",style.RangMaxVal,DbType.Int32),
                    new Params(":rang_use_min_val",style.RangUseMinValue,DbType.Boolean),
                    new Params(":rang_use_max_val",style.RangUseMaxValue,DbType.Boolean),
                    new Params(":rang_null_color",style.RangNullColor,DbType.Int64),
                    new Params(":rang_use_null_color",style.RangUseNullColor,DbType.Boolean),
                    new Params(":graphic_units",style.GraphicUnits,DbType.Boolean),
                    new Params(":fontname",style.FontName,DbType.String),
                    new Params(":fontcolor",style.FontColor,DbType.Int32),
                    new Params(":fontframecolor",style.FontFrameColor,DbType.Int32),
                    new Params(":fontsize",style.FontSize,DbType.Int32),
                    new Params(":symbol",style.Symbol,DbType.Int32),
                    new Params(":pencolor",style.PenColor,DbType.Int32),
                    new Params(":pentype",style.PenType,DbType.Int32),
                    new Params(":penwidth",style.PenWidth,DbType.Int32),
                    new Params(":brushbgcolor",style.BrushBgColor,DbType.Int64),
                    new Params(":brushfgcolor",style.BrushFgColor,DbType.Int64),
                    new Params(":brushstyle",style.BrushStyle,DbType.Int32),
                    new Params(":brushhatch",style.BrushHatch,DbType.Int32),
                };

            using (SqlWork sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql = string.Format(@"
                                        UPDATE sys_scheme.style_layer_info SET
                                            style_type=:style_type,
                                            style_field=:style_field,
                                            image_column=:image_column,
                                            angle_column=:angle_column,
                                            min_object_size=:min_object_size,
                                            use_bounds=:use_bounds,
                                            min_scale=:min_scale,
                                            max_scale=:max_scale,
                                            rang_precision_point=:rang_precision_point,
                                            rang_type_color=:rang_type_color,
                                            rang_min_color=:rang_min_color,
                                            rang_min_val=:rang_min_val,
                                            rang_max_color=:rang_max_color,
                                            rang_max_val=:rang_max_val,
                                            rang_null_color=:rang_null_color,
                                            graphic_units=:graphic_units,
                                            fontname=:fontname,
                                            fontcolor=:fontcolor,
                                            fontframecolor=:fontframecolor,
                                            fontsize=:fontsize,
                                            symbol=:symbol,
                                            pencolor=:pencolor,
                                            pentype=:pentype,
                                            penwidth=:penwidth,
                                            brushbgcolor=:brushbgcolor,
                                            brushfgcolor=:brushfgcolor,
                                            brushstyle=:brushstyle,
                                            brushhatch=:brushhatch,
                                            rang_use_null_color=:rang_use_null_color,
                                            rang_use_min_val=:rang_use_min_val,
                                            rang_use_max_val=:rang_use_max_val
                                        WHERE id={0}", style.Id);
                sqlCmd.ExecuteNonQuery(listParam);
            }
        }
        /// <summary>
        /// Сохранение информации о стиле подписи в базе данных
        /// </summary>
        /// <param name="style">Стиль подписи который необходимо сохранить</param>
        public void DBSaveLabelStyle(PgM.PgStyleLableM labelstyle)
        {
            var listParam = new List<Interfaces.IParams> 
            {                      
                new Params(":lable_mask",labelstyle.LableMask,DbType.String),
                new Params(":label_showframe",labelstyle.LabelShowFrame,DbType.Boolean),
                new Params(":label_framecolor",labelstyle.LabelFrameColor,DbType.Int32),
                new Params(":label_parallel",labelstyle.LabelParallel,DbType.Boolean),
                new Params(":label_overlap",labelstyle.LabelOverlap,DbType.Boolean),
                new Params(":label_usebounds",labelstyle.LabelUseBounds,DbType.Boolean),
                new Params(":label_minscale",labelstyle.LabelMinScale,DbType.Int32),
                new Params(":label_maxscale",labelstyle.LabelMaxScale,DbType.Int32),
                new Params(":label_offset",labelstyle.LabelOffset,DbType.Int32),
                new Params(":label_graphicunits",labelstyle.LabelUseGraphicUnits,DbType.Boolean),
                new Params(":label_fontname",labelstyle.LabelFontName,DbType.String),
                new Params(":label_fontcolor",labelstyle.LabelFontColor,DbType.Int32),
                new Params(":label_fontsize",labelstyle.LabelFontSize,DbType.Int32),
                new Params(":label_fontstrikeout",labelstyle.LabelFontStrikeout,DbType.Boolean),
                new Params(":label_fontitalic",labelstyle.LabelFontItalic,DbType.Boolean),
                new Params(":label_fontunderline",labelstyle.LabelFontUnderline,DbType.Boolean),
                new Params(":label_fontbold",labelstyle.LabelFontBold,DbType.Boolean),
                new Params(":label_showlabel",labelstyle.LabelShowLabel,DbType.Boolean)
            };


            using (SqlWork sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql = string.Format(@"
                                        UPDATE sys_scheme.style_lable_info SET
                                              lable_mask=:lable_mask,
                                              label_showframe=:label_showframe,
                                              label_framecolor=:label_framecolor,
                                              label_parallel=:label_parallel,
                                              label_overlap=:label_overlap,
                                              label_usebounds=:label_usebounds,
                                              label_minscale=:label_minscale,
                                              label_maxscale=:label_maxscale,
                                              label_offset=:label_offset,
                                              label_graphicunits=:label_graphicunits,
                                              label_fontname=:label_fontname,
                                              label_fontcolor=:label_fontcolor,
                                              label_fontsize=:label_fontsize,
                                              label_fontstrikeout=:label_fontstrikeout,
                                              label_fontitalic=:label_fontitalic,
                                              label_fontunderline=:label_fontunderline,
                                              label_fontbold=:label_fontbold,
                                              label_showlabel=:label_showlabel
                                         WHERE id={0}", labelstyle.Id);
                sqlCmd.ExecuteNonQuery(listParam);
            }
        }
        /// <summary>
        /// Загрузка слоя из базы Postgres на карту MapLib
        /// </summary>
        /// <param name="table"></param>
        public bool LoadLayerFromSource(AbsM.TableBaseM tablebase)
        {
            PgM.PgTableBaseM table = tablebase as PgM.PgTableBaseM;
            if (_mv.getLayer(table.NameMap) != null)
            {
                //_mv.getLayer(table.NameMap).Update();
                return true;
            }
            if (_postgresMapAdapter == null)
            {
                _postgresMapAdapter = _mv.CreateExternalSource(mvExternalSourceTypes.mvPostGIS);
                String connectionString = String.Format("host={0} port={1} user={2} password={3} dbname={4}",
                                                         _connect.Host,
                                                         _connect.Port,
                                                         _connect.UserName,
                                                         _connect["Password"],
                                                         _connect.Database);
                _postgresMapAdapter.prepare(connectionString, Convert.ToInt32(Program.srid));
                _postgresMapAdapter.Connect();
            }
            else
            {
                if (!_postgresMapAdapter.isConnected())
                {
                    String connectionString = String.Format("host={0} port={1} user={2} password={3} dbname={4}",
                                                         _connect.Host,
                                                         _connect.Port,
                                                         _connect.UserName,
                                                         _connect["Password"],
                                                         _connect.Database);
                    _postgresMapAdapter.prepare(connectionString, Convert.ToInt32(Program.srid));
                    _postgresMapAdapter.Connect();
                }
            }
            try
            {
                table.NameMap = String.Format("#{0}:{1}:{2}", this.Id, table.Id, table.Text);

                PgM.PgStyleLayerM layerStyle = CurrentSet.Layers.ContainsKey(table) ? CurrentSet.Layers[table] : null;
                PgM.PgStyleLableM labelStyle = CurrentSet.Labels.ContainsKey(table) ? CurrentSet.Labels[table] : null;

                mvLayer mapLayer;
                switch (layerStyle.StyleType)
                {
                    case PgM.EStyleType.uniformly:
                        {
                            mapLayer = _postgresMapAdapter.addLayer(
                                table.SchemeName + "." + table.Name,
                                table.NameMap,
                                table.PrimaryKey,
                                table.GeomField,
                                labelStyle.LableMask,
                                "",
                                "",
                                layerStyle.ImageColumn,
                                layerStyle.AngleColumn,
                                "");
                            break;
                        }
                    case PgM.EStyleType.range:
                        {
                            int min_val = 0;

                            if (layerStyle.RangUseMinValue)
                            {
                                min_val = (int)layerStyle.RangMinVal;
                            }
                            else
                            {
                                using (SqlWork sqlWork = new SqlWork(_connect))
                                {
                                    //присваем правильное значение параметру кол знаков после запятой
                                    int type_field = (int)(layerStyle.StyleField.Type);
                                    int p_point = 1;
                                    if (type_field == 1)
                                    {
                                        p_point = 0;
                                    }
                                    else
                                    {
                                        p_point = layerStyle.RangPrecisionPoint;
                                    }
                                    sqlWork.sql = String.Format("SELECT (min({0})*10^{1})::INTEGER FROM {2}.{3}",
                                                                layerStyle.StyleField.Name,
                                                                p_point,
                                                                table.SchemeName,
                                                                table.Name);
                                    sqlWork.Execute(false);
                                    if (sqlWork.CanRead())
                                    {
                                        min_val = sqlWork.GetInt32(0);
                                    }
                                    sqlWork.Close();
                                }
                            }


                            String rangeSql = GetRangeSelect(table);
                            mapLayer = _postgresMapAdapter.addLayer(
                                                table.SchemeName + "." + table.Name,
                                                table.NameMap,
                                                table.PrimaryKey,
                                                table.GeomField,
                                                labelStyle.LableMask,
                                //String.Format("(({0}*10^{1})-({2}))::INTEGER", layerStyle.StyleField.Name, layerStyle.RangPrecisionPoint, min_val),
                                                rangeSql,
                                                "",
                                                layerStyle.ImageColumn,
                                                layerStyle.AngleColumn,
                                                "");
                            break;
                        }
                    case PgM.EStyleType.directory:
                        {
                            mapLayer = _postgresMapAdapter.addLayer(
                                                table.SchemeName + "." + table.Name,
                                                table.NameMap,
                                                table.PrimaryKey,
                                                table.GeomField,
                                                labelStyle.LableMask,
                                                layerStyle.StyleField.Name,
                                                "",
                                                layerStyle.ImageColumn,
                                                layerStyle.AngleColumn,
                                                "");
                            break;
                        }
                    case PgM.EStyleType.interval:
                        {
                            string intervalSql = GetIntervalSelect(table);
                            mapLayer = _postgresMapAdapter.addLayer(
                                                table.SchemeName + "." + table.Name,
                                                table.NameMap,
                                                table.PrimaryKey,
                                                table.GeomField,
                                                labelStyle.LableMask,
                                                intervalSql,
                                                "",
                                                layerStyle.ImageColumn,
                                                layerStyle.AngleColumn,
                                                "");
                            break;
                        }
                    default:
                        mapLayer = null;
                        break;
                }

                if (mapLayer != null)
                {
                    mapLayer.selectable = false;
                    mapLayer.MinObjectSize = layerStyle.MinObjectSize;
                    if (String.IsNullOrEmpty(layerStyle.ImageColumn))
                    {
                        loadStyle(mapLayer, table, layerStyle);
                    }
                    if (layerStyle.UseBounds)
                    {
                        mapLayer.usebounds = layerStyle.UseBounds;
                        mapLayer.MaxScale = Convert.ToUInt32(layerStyle.MaxScale);
                        mapLayer.MinScale = Convert.ToUInt32(layerStyle.MinScale);
                    }
                    if (labelStyle != null)
                    {
                        mapLayer.showlabels = labelStyle.LabelShowLabel;
                        SetLabelStyle(table);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Program.mainFrm1.StatusInfo = "Ошибка загрузки слоя! " + ex.Message + "";
                return false;
            }
        }
        /// <summary>
        /// Формирование запроса для загрузки стиля по интервалу
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private string GetIntervalSelect(PgM.PgTableBaseM table)
        {
            PgM.PgStyleLayerM layerStyle = CurrentSet.Layers[table];
            string temp_sql = "";
            int refTableId = (int)layerStyle.StyleField.IdRefTable;
            int refFieldBegin = (int)layerStyle.StyleField.IdRefField;
            int refFieldEnd = (int)layerStyle.StyleField.IdRefFieldEnd;
            string refFieldName = layerStyle.StyleField.Name;

            var intervalTable = FindTable(refTableId) as PgM.PgTableBaseM;
            if (intervalTable != null)
            {
                temp_sql = String.Format(
                    "(SELECT itrv.{0} FROM {1}.{2} itrv WHERE {6}.{3}>=itrv.{4} AND {6}.{3}<=itrv.{5} LIMIT 1)",
                         intervalTable.PrimaryKey,
                         intervalTable.SchemeName,
                         intervalTable.Name,
                         refFieldName,
                         intervalTable.Fields.First(fd => fd.Id == refFieldBegin).Name,
                         intervalTable.Fields.First(fd => fd.Id == refFieldEnd).Name,
                         table.Name);
            }
            return temp_sql;
        }
        private String GetRangeSelect(PgM.PgTableBaseM table)
        {
            PgM.PgStyleLayerM layerStyle = CurrentSet.Layers.ContainsKey(table) ? CurrentSet.Layers[table] : null;
            String rangeColumn = layerStyle.StyleField.Name;
            String result = rangeColumn;
            if (layerStyle != null && rangeColumn != null)
            {
                //узнаем какого типа колонка
                int typeField = (int)(layerStyle.StyleField.Type);
                //присваем правильное значение параметру кол знаков после запятой
                int precPoint = (typeField == 1) ? 0 : layerStyle.RangPrecisionPoint;
                String minValStr = layerStyle.RangUseMinValue ? layerStyle.RangMinVal.ToString() : String.Format("min({0})*10^{1}", rangeColumn, precPoint);
                String maxValStr = layerStyle.RangUseMaxValue ? layerStyle.RangMaxVal.ToString() : String.Format("max({0})*10^{1}", rangeColumn, precPoint);
                int minVal = 0;
                int maxVal = 0;

                // Узнаем минимальное и максимальное значение
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = String.Format("SELECT ({0})::INTEGER, ({1})::INTEGER FROM {2}.{3};", minValStr, maxValStr, table.SchemeName, table.Name);
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        minVal = sqlCmd.GetInt32(0);
                        maxVal = sqlCmd.GetInt32(1);
                    }
                    sqlCmd.Close();
                }

                if (maxVal != minVal)
                {
                    result = String.Format(
                                    "(SELECT (({0}-{1}::DECIMAL)/({2}-{1})*255)::INTEGER)",
                                    rangeColumn,
                                    minVal,
                                    maxVal);
                }
            }
            return result;
        }
        /// <summary>
        /// Загрузка стиля для слоя
        /// </summary>
        /// <param name="templayer">Слой для которого происходит загрузка стиля</param>
        /// <param name="table">Таблица ассоциированная со слоем</param>
        /// <param name="layerstyle">Объект стиля для слоя</param>
        public void loadStyle(mvLayer templayer, PgM.PgTableBaseM table, PgM.PgStyleLayerM layerstyle)
        {
            switch (layerstyle.StyleType)
            {
                case Rekod.DataAccess.SourcePostgres.Model.EStyleType.uniformly:
                    {
                        templayer.uniform = true;
                        templayer.SetUniformStyle(
                            new mvMapLib.mvPenObject()
                            {
                                Color = (uint)(layerstyle.PenColor),
                                ctype = (ushort)(layerstyle.PenType),
                                width = (uint)(layerstyle.PenWidth)
                            },
                            new mvMapLib.mvBrushObject()
                            {
                                bgcolor = (uint)layerstyle.BrushBgColor,
                                fgcolor = (uint)layerstyle.BrushFgColor,
                                style = (ushort)layerstyle.BrushStyle,
                                hatch = (ushort)layerstyle.BrushHatch
                            },
                            new mvMapLib.mvSymbolObject()
                            {
                                shape = (uint)layerstyle.Symbol
                            },
                            new mvMapLib.mvFontObject()
                            {
                                fontname = layerstyle.FontName,
                                Color = (uint)layerstyle.FontColor,
                                framecolor = (uint)layerstyle.FontFrameColor,
                                size = layerstyle.FontSize,
                                graphicUnits = layerstyle.GraphicUnits
                            });
                        break;
                    }
                case Rekod.DataAccess.SourcePostgres.Model.EStyleType.range:
                    {
                        templayer.uniform = false;
                        DeltaColor deltaColor = new DeltaColor();
                        int minVal = 0, maxVal = 0, idTypeGeom = 0, precPoint = 1;

                        mvMapLib.mvSymbolObject mvSymbol = new mvMapLib.mvSymbolObject();
                        mvMapLib.mvFontObject mvFont = new mvMapLib.mvFontObject();
                        mvMapLib.mvPenObject mvPen = new mvMapLib.mvPenObject();
                        mvMapLib.mvBrushObject mvBrush = new mvMapLib.mvBrushObject();
                        idTypeGeom = (int)(table.GeomType);

                        //узнаем колонку для вычисления стилей
                        String rangeColumn = layerstyle.StyleField.Name;

                        //узнаем какого типа колонка
                        int typeField = (int)(layerstyle.StyleField.Type);

                        //присваем правильное значение параметру кол знаков после запятой
                        precPoint = (typeField == 1) ? 0 : layerstyle.RangPrecisionPoint;

                        if (rangeColumn != "")
                        {
                            // Узнаем количество градаций

                            Color minColor = Color.Empty;
                            Color maxColor = Color.Empty;
                            String minValStr = layerstyle.RangUseMinValue ? layerstyle.RangMinVal.ToString() : String.Format("min({0})*10^{1}", rangeColumn, precPoint);
                            String maxValStr = layerstyle.RangUseMaxValue ? layerstyle.RangMaxVal.ToString() : String.Format("max({0})*10^{1}", rangeColumn, precPoint);

                            // Узнаем минимальное и максимальное значение
                            using (SqlWork sqlCmd = new SqlWork(_connect))
                            {
                                sqlCmd.sql = String.Format("SELECT ({0})::INTEGER, ({1})::INTEGER FROM {2}.{3};", minValStr, maxValStr, table.SchemeName, table.Name);
                                sqlCmd.Execute(false);
                                if (sqlCmd.CanRead())
                                {
                                    minVal = sqlCmd.GetInt32(0);
                                    maxVal = sqlCmd.GetInt32(1);
                                }
                                sqlCmd.Close();
                            }

                            //получаем коэффициент изменения цвета на каждом шаге
                            int typeColor = 0;
                            int totalRange = 0;
                            uint fontColor = 0, fontFrameColor = 0, brushBgColor = 0, brushFgColor = 0, penColor = 0;
                            using (SqlWork sqlCmd = new SqlWork(_connect))
                            {
                                sqlCmd.sql = "SELECT rang_min_color, rang_max_color, rang_type_color, fontcolor, fontframecolor, pencolor, brushbgcolor, brushfgcolor FROM "
                                    + "sys_scheme.style_layer_info WHERE id = " + layerstyle.Id.ToString();

                                sqlCmd.Execute(false);
                                if (sqlCmd.CanRead())
                                {
                                    minColor = convColor(sqlCmd.GetValue<uint>(0));
                                    maxColor = convColor(sqlCmd.GetValue<uint>(1));

                                    int redRange = Math.Abs(maxColor.R - minColor.R);
                                    int greenRange = Math.Abs(maxColor.G - minColor.G);
                                    int blueRange = Math.Abs(maxColor.B - minColor.B);
                                    int valueRange = Math.Abs(maxVal - minVal);

                                    int colorRange = Math.Max(redRange, Math.Max(greenRange, blueRange));
                                    totalRange = Math.Min(colorRange, valueRange);

                                    deltaColor = GetDeltaColor(minColor, maxColor, totalRange);
                                    typeColor = sqlCmd.GetValue<int>(2);
                                    fontColor = sqlCmd.GetValue<uint>(3);
                                    fontFrameColor = sqlCmd.GetValue<uint>(4);
                                    penColor = sqlCmd.GetValue<uint>(5);
                                    brushBgColor = sqlCmd.GetValue<uint>(6);
                                    brushFgColor = sqlCmd.GetValue<uint>(7);
                                }
                                sqlCmd.Close();
                            }

                            // формирование стилей
                            for (int t = 0; t <= totalRange; t++)
                            {
                                int styleId = Convert.ToInt32(t * 255 / (double)totalRange);
                                int r = Convert.ToInt32(minColor.R + (deltaColor.R * t));
                                int g = Convert.ToInt32(minColor.G + (deltaColor.G * t));
                                int b = Convert.ToInt32(minColor.B + (deltaColor.B * t));

                                switch (idTypeGeom)
                                {
                                    /*Информация про typeColor
                                     * Основного 0
                                     * Фона 1
                                     * Границ или линий 2
                                     * Каймы 3
                                    */

                                    case 1:
                                        {
                                            mvSymbol.shape = (uint)(layerstyle.Symbol);
                                            mvFont.fontname = layerstyle.FontName;
                                            mvFont.Color = (uint)(layerstyle.FontColor);
                                            mvFont.framecolor = (uint)(layerstyle.FontFrameColor);
                                            mvFont.size = layerstyle.FontSize;
                                            if (typeColor == 0)
                                            {
                                                mvFont.Color = convToUInt(Color.FromArgb(r, g, b));
                                            }
                                            if (typeColor == 3)
                                            {
                                                mvFont.framecolor = convToUInt(Color.FromArgb(r, g, b));
                                            }
                                            templayer.AddExtStyle(styleId, templayer.CreateDotStyle(mvSymbol, mvFont));
                                            break;
                                        }
                                    case 2:
                                        {
                                            mvPen.Color = (uint)(layerstyle.PenColor);
                                            mvPen.ctype = (ushort)(layerstyle.PenType);
                                            mvPen.width = (uint)(layerstyle.PenWidth);
                                            mvPen.Color = convToUInt(Color.FromArgb(r, g, b));
                                            templayer.AddExtStyle(styleId, templayer.CreateLineStyle(mvPen));
                                            break;
                                        }
                                    case 3:
                                        {
                                            mvPen.Color = (uint)(layerstyle.PenColor);
                                            mvPen.ctype = (ushort)(layerstyle.PenType);
                                            mvPen.width = (uint)(layerstyle.PenWidth);
                                            mvBrush.bgcolor = (uint)(layerstyle.BrushBgColor);
                                            mvBrush.fgcolor = (uint)(layerstyle.BrushFgColor);
                                            mvBrush.hatch = (ushort)(layerstyle.BrushHatch);
                                            mvBrush.style = (ushort)(layerstyle.BrushStyle);

                                            var color = convToUInt(Color.FromArgb(r, g, b));
                                            mvPen.Color = penColor;
                                            mvBrush.bgcolor = brushBgColor;
                                            mvBrush.fgcolor = brushFgColor;
                                            if (typeColor == 0)
                                                mvBrush.fgcolor = color;
                                            if (typeColor == 1)
                                                mvBrush.bgcolor = color;
                                            if (typeColor == 2)
                                                mvPen.Color = color;

                                            templayer.AddExtStyle(styleId, templayer.CreatePolygonStyle(mvPen, mvBrush));
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
                case Rekod.DataAccess.SourcePostgres.Model.EStyleType.directory:
                case Rekod.DataAccess.SourcePostgres.Model.EStyleType.interval:
                    {
                        int? ref_table = null, ref_field = null, ref_field_end = null, ref_field_name = null;
                        PgM.PgFieldM styleField = layerstyle.StyleField;

                        ref_table = (styleField.RefTable != null) ? (int?)styleField.RefTable.Id : null;
                        ref_field = (styleField.RefField != null) ? (int?)styleField.RefField.Id : null;
                        ref_field_end = (styleField.RefFieldEnd != null) ? (int?)styleField.RefFieldEnd.Id : null;
                        ref_field_name = (styleField.RefFieldName != null) ? (int?)styleField.RefFieldName.Id : null;

                        mvMapLib.mvSymbolObject Symbol = new mvMapLib.mvSymbolObject();
                        mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
                        mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
                        mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();

                        int count = 0;
                        if (ref_table == null)
                        {
                            return;
                        }

                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = String.Format("SELECT count(*)::INTEGER as val FROM {0}.{1}",
                                                        styleField.RefTable.SchemeName,
                                                        styleField.RefTable.Name);
                            sqlCmd.Execute(false);
                            if (sqlCmd.CanRead())
                            {
                                count = sqlCmd.GetInt32(0);
                            }
                            sqlCmd.Close();
                        }

                        int[] style = new int[count];

                        int id_type_geom = 0;
                        string id_name_type_geom = "", map_name = "";
                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = "SELECT ttg.id, ttg.name, ti.name_map " +
                                "FROM " + Program.scheme + ".table_info ti, " + Program.scheme + ".table_type_geom ttg WHERE ti.geom_type = ttg.id AND ti.id = " + table.Id;
                            sqlCmd.Execute(false);
                            if (sqlCmd.CanRead())
                            {
                                id_type_geom = sqlCmd.GetInt32(0);
                                id_name_type_geom = sqlCmd.GetValue<string>(1);
                                map_name = sqlCmd.GetValue<string>(2);
                            }
                            sqlCmd.Close();
                        }

                        int[] idList = new int[count];
                        int[] valueList = new int[count];
                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = String.Format("SELECT {0}, {1} FROM {2}.{3} ORDER BY {0};",
                                                            styleField.RefTable.PrimaryKey,
                                                            styleField.RefField.Name,
                                                            styleField.RefTable.SchemeName,
                                                            styleField.RefTable.Name);
                            sqlCmd.Execute(false);
                            int i = 0;
                            while (sqlCmd.CanRead())
                            {
                                idList[i] = sqlCmd.GetInt32(0);
                                valueList[i] = sqlCmd.GetInt32(1);
                                i++;
                            }
                            sqlCmd.Close();
                        }


                        for (int i = 0; idList.Length > i; i++)
                        {
                            switch (id_type_geom)
                            {
                                case 1:
                                    {
                                        Symbol = new mvMapLib.mvSymbolObject();
                                        using (SqlWork sqlCmd = new SqlWork(_connect))
                                        {
                                            sqlCmd.sql = String.Format("SELECT symbol FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1}",
                                                                        styleField.RefTable.Id, idList[i]);
                                            sqlCmd.Execute(false);
                                            if (sqlCmd.CanRead())
                                            {
                                                Symbol.shape = Convert.ToUInt32(sqlCmd.GetInt32(0));
                                            }
                                            sqlCmd.Close();
                                        }
                                        font = new mvMapLib.mvFontObject();

                                        using (SqlWork sqlCmd = new SqlWork(_connect))
                                        {
                                            sqlCmd.sql =
                                                String.Format("SELECT fontname, fontcolor, fontframecolor, " +
                                                              "fontsize FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1}",
                                                              styleField.RefTable.Id, idList[i]);
                                            sqlCmd.Execute(false);
                                            if (sqlCmd.CanRead())
                                            {
                                                font.fontname = sqlCmd.GetValue<string>(0);
                                                font.Color = sqlCmd.GetValue<uint>(1);
                                                font.framecolor = sqlCmd.GetValue<uint>(2);
                                                font.size = sqlCmd.GetInt32(3);
                                                font.graphicUnits = layerstyle.GraphicUnits;
                                            }
                                            sqlCmd.Close();
                                        }

                                        templayer.AddExtStyle(
                                            (layerstyle.StyleType == PgM.EStyleType.interval) ? idList[i] : valueList[i],
                                            templayer.CreateDotStyle(Symbol, font));
                                        break;
                                    }
                                case 2:
                                    {
                                        pen = new mvMapLib.mvPenObject();
                                        using (SqlWork sqlCmd = new SqlWork(_connect))
                                        {
                                            sqlCmd.sql = String.Format("SELECT pencolor, penwidth, pentype FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1}",
                                                              styleField.RefTable.Id, idList[i]);
                                            sqlCmd.Execute(false);
                                            if (sqlCmd.CanRead())
                                            {
                                                pen.Color = Convert.ToUInt32(sqlCmd.GetInt32(0));
                                                pen.width = Convert.ToUInt32(sqlCmd.GetInt32(1));
                                                pen.ctype = Convert.ToUInt16(sqlCmd.GetInt32(2));
                                            }
                                            sqlCmd.Close();
                                        }
                                        templayer.AddExtStyle(
                                            (layerstyle.StyleType == PgM.EStyleType.interval) ? idList[i] : valueList[i],
                                            templayer.CreateLineStyle(pen));
                                        break;
                                    }
                                case 3:
                                    {
                                        pen = new mvMapLib.mvPenObject();
                                        using (SqlWork sqlCmd = new SqlWork(_connect))
                                        {
                                            sqlCmd.sql = String.Format("SELECT pencolor, penwidth, pentype FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1}",
                                                              styleField.RefTable.Id, idList[i]);
                                            sqlCmd.Execute(false);
                                            if (sqlCmd.CanRead())
                                            {
                                                pen.Color = Convert.ToUInt32(sqlCmd.GetInt32(0));
                                                pen.width = Convert.ToUInt32(sqlCmd.GetInt32(1));
                                                pen.ctype = Convert.ToUInt16(sqlCmd.GetInt32(2));
                                            }
                                            sqlCmd.Close();
                                        }
                                        brush = new mvMapLib.mvBrushObject();
                                        using (SqlWork sqlCmd = new SqlWork(_connect))
                                        {
                                            sqlCmd.sql = String.Format("SELECT brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1}",
                                                              styleField.RefTable.Id, idList[i]);
                                            sqlCmd.Execute(false);
                                            if (sqlCmd.CanRead())
                                            {
                                                brush.bgcolor = sqlCmd.GetValue<uint>(0);
                                                brush.fgcolor = sqlCmd.GetValue<uint>(1);
                                                brush.style = sqlCmd.GetValue<ushort>(2);
                                                brush.hatch = sqlCmd.GetValue<ushort>(3);
                                            }
                                            sqlCmd.Close();
                                        }
                                        templayer.AddExtStyle(
                                            (layerstyle.StyleType == PgM.EStyleType.interval) ? idList[i] : valueList[i],
                                            templayer.CreatePolygonStyle(pen, brush));
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// Установка стиля подписей 
        /// </summary>
        /// <param name="table">Таблица для которой устанавливается стиль подписи</param>
        public void SetLabelStyle(PgM.PgTableBaseM table)
        {
            mvLayer layer = _axMapLib1.getLayer(table.NameMap);
            PgM.PgStyleLableM labelstyle = table.LabelStyle;
            if (layer == null || labelstyle == null)
            {
                return;
            }
            try
            {
                mvFontObject mvfontObject = _axMapLib1.createFontObject();
                mvfontObject.Color = labelstyle.LabelFontColor;
                mvfontObject.fontname = labelstyle.LabelFontName;
                mvfontObject.size = labelstyle.LabelFontSize;
                mvfontObject.strikeout = labelstyle.LabelFontStrikeout;
                mvfontObject.italic = labelstyle.LabelFontItalic;
                mvfontObject.underline = labelstyle.LabelFontUnderline;
                mvfontObject.graphicUnits = labelstyle.LabelUseGraphicUnits;
                if (labelstyle.LabelShowFrame)
                {
                    mvfontObject.framecolor = labelstyle.LabelFrameColor;
                }
                else
                {
                    mvfontObject.framecolor = 0xFFFFFFFF;
                }
                if (labelstyle.LabelFontBold)
                {
                    mvfontObject.weight = 650;
                }
                if (labelstyle.LabelUseBounds)
                {
                    layer.labelBounds = true;
                    layer.labelMinScale = labelstyle.LabelMinScale;
                    layer.labelMaxScale = labelstyle.LabelMaxScale;
                }
                layer.labelParallel = labelstyle.LabelParallel;
                layer.labelOverlap = labelstyle.LabelOverlap;
                layer.labelOffset = labelstyle.LabelOffset;
                layer.SetLabelstyle(mvfontObject);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить стиль подписи объектов: " + ex.Message);
            }
            _axMapLib1.Update();
            _axMapLib1.mapRepaint();
        }
        /// <summary>
        /// Преобразует целое значение - представление цвета в базе в объект Color
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color convColor(uint value)
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb(r1, g1, b1);
            return c1;
        }
        /// <summary>
        /// Получение объекта DeltaColor представляющее плавное изменение цвета от одного значения к другому при загрузке стиля по интервалу
        /// </summary>
        /// <param name="clr1"></param>
        /// <param name="clr2"></param>
        /// <param name="countStyle"></param>
        /// <returns></returns>
        public DeltaColor GetDeltaColor(Color clr1, Color clr2, int countStyle)
        {
            DeltaColor temp;
            if (clr1.R - clr2.R != 0)
            {
                temp.R = ((double)(clr2.R - clr1.R)) / countStyle;
            }
            else
            {
                temp.R = 0;
            }
            if (clr1.G - clr2.G != 0)
            {
                temp.G = ((double)(clr2.G - clr1.G)) / countStyle;
            }
            else
            {
                temp.G = 0;
            }
            if (clr1.B - clr2.B != 0)
            {
                temp.B = ((double)(clr2.B - clr1.B)) / countStyle;
            }
            else
            {
                temp.B = 0;
            }
            return temp;
        }
        /// <summary>
        /// Конвертирует цвет в представление в виде целого числа (uint) для хранения в базе
        /// </summary>
        /// <param name="clr"></param>
        /// <returns></returns>
        public static uint convToUInt(Color clr)
        {
            uint temp = 0;
            temp = Convert.ToUInt32(clr.R + (clr.G << 8) + (clr.B << 16));
            return temp;
        }
        /// <summary>
        /// Переход к объекту
        /// </summary>
        /// <param name="_table"></param>
        /// <param name="id"></param>
        internal void GoToObject(AbsM.ITableBaseM _table, object id)
        {
            int? n_id = ExtraFunctions.Converts.To<int?>(id);
            if (n_id == null)
                return;
            var table = _table as PgM.PgTableBaseM;
            if (table == null)
                return;
            var layer = _tableManager.mv.getLayer(table.NameMap);
            if (layer == null)
                return;
            layer.MoveTo(n_id.Value, false);
        }
        /// <summary>
        /// Метод который сделает либо update в базу либо insert и обновит коллекцию
        /// </summary>
        /// <param name="user">Пользователь что нужно либо обновить либо добавить в базе</param>
        /// <returns>Идентификатор созданного либо обновленного пользователя</returns>
        public int InsertUpdateUser(PgM.PgUserM user)
        {
            int id = -1;
            using (var sqlCmd = new SqlWork(_connect))
            {
                var prms = new Params[8];
                prms[0] = new Params() { paramName = "id", type = DbType.Int32, typeData = NpgsqlTypes.NpgsqlDbType.Integer, value = user.ID };
                prms[1] = new Params() { paramName = "pass", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.Pass };
                prms[2] = new Params() { paramName = "pass_sync", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.PassSync };
                prms[3] = new Params() { paramName = "name_full", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.NameFull };
                prms[4] = new Params() { paramName = "login", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.Login };
                prms[5] = new Params() { paramName = "otdel", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.Otdel };
                prms[6] = new Params() { paramName = "window_name", type = DbType.String, typeData = NpgsqlTypes.NpgsqlDbType.Text, value = user.WindowName };
                prms[7] = new Params() { paramName = "typ", type = DbType.Int32, typeData = NpgsqlTypes.NpgsqlDbType.Integer, value = user.Type };

                if (user.ID == null)
                {
                    sqlCmd.sql = string.Format(@"
                        SELECT sys_scheme.create_user
                        (
                            :login, 
                            :pass, 
                            :pass_sync, 
                            :name_full, 
                            :otdel, 
                            :window_name,
                            :typ);");
                    sqlCmd.ExecuteReader(prms);
                    if (sqlCmd.CanRead())
                        id = sqlCmd.GetInt32(0);
                    sqlCmd.CloseReader();
                }
                else
                {
                    sqlCmd.sql = string.Format(@"
                        update sys_scheme.user_db set
                            name_full=:name_full,
                            login=:login,
                            otdel=:otdel,
                            window_name=:window_name,
                            typ=:typ
                        where id=:id;");
                    sqlCmd.ExecuteNonQuery(prms);
                    sqlCmd.Close();
                    id = (int)user.ID;

                    if (!String.IsNullOrEmpty(user.Pass))
                    {
                        using (SqlWork sqlWork = new SqlWork(Connect))
                        {
                            sqlWork.sql = "SELECT sys_scheme.user_change_pw(:id, :pass, :pass_sync);";
                            sqlWork.ExecuteNonQuery(prms);
                            if (!string.IsNullOrEmpty(user.Pass) && CurrentUser.ID == user.ID)
                            {
                                Connect.Password = user.Pass;
                            }
                        }
                    }
                }
            }
            if (user.Extent != null)
            {
                using (SqlWork sqlWork = new SqlWork(Connect))
                {
                    sqlWork.sql =
                        String.Format("SELECT sys_scheme.set_user_param('id_map_extents', {0}, array['{1}']);", id, user.Extent.Id);
                    sqlWork.ExecuteNonQuery();
                    sqlWork.Close();
                }
            }
            UpdateUsers(id);
            return id;
        }
        /// <summary>
        /// Создать событие на обнавление атрибутов объекта
        /// </summary>
        /// <param name="pgAttributesVM"></param>
        internal void SetEventAttribute(AbsM.ITableBaseM table, object id, PgAtM.attributeTypeChange type)
        {
            EventHandler<PgAtM.PgAttributeEventArgs> temp = _eventUpdateAttribute;
            if (temp != null)
                temp(this, new PgAtM.PgAttributeEventArgs(table, id, type));
        }

        #region Методы AbsM.IDataRepositoryVM
        /// <summary>
        /// Проверка соединения с БД
        /// </summary>
        /// <returns></returns>
        public override bool CheckRepository()
        {
            Version curentVersion = null;
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format(@"
                                        SELECT 
	                                        major, 
	                                        minor, 
	                                        build, 
	                                        revision
                                        FROM 
	                                        sys_scheme.db_version
                                        ORDER BY 
	                                        major DESC, 
	                                        minor DESC, 
	                                        build DESC, 
	                                        revision DESC
                                        LIMIT 1;");
                if (sqlWork.CheckConnection() && sqlWork.ExecuteReader() && sqlWork.CanRead())
                {
                    curentVersion = new Version(
                            sqlWork.GetInt32("major"),
                            sqlWork.GetInt32("minor"),
                            sqlWork.GetInt32("build"),
                            sqlWork.GetInt32("revision")
                            );
                }
            }
            if (curentVersion == null || curentVersion < ActualVersion)
                throw new Exception("Версия БД слишком старая!");

            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format("SELECT id FROM sys_scheme.user_db WHERE login='{1}'",
                                             "sys_scheme", _connect["UserName"], _connect["Password"]);
                var idUser = sqlWork.ExecuteScalar<int?>();
                if (idUser == null)
                    throw new Exception("У вас нет прав для подключения к данному источнику");
                _idUser = (int)idUser;
            }

            Text = String.Format("{0}@{1}:{2} ({3})",
                        Connect.Database,
                        Connect.Host,
                        Connect.Port,
                        Connect.UserName);

            return base.CheckRepository();
        }
        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public override void ReloadInfo()
        {
            UpdateSchems();
            UpdateGroups();
            UpdateTables();
            UpdateTablesInGroups();
            UpdateFields();
            UpdateSets();
            UpdateLayerStyle();
            UpdateLabelStyle();
            UpdateUsers();
            UpdateExtents();
        }

        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public void ReloadPartInfo()
        {
            UpdateActions();
            UpdateUsers();
            UpdateExtents();
            LoadUserRights();
            UpdatePartTables();
            UpdatePartFields();
        }
        /// <summary>
        /// Поиск поля в таблце
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатор поля</param>
        /// <returns>Найденое поле</returns>
        public AbsM.IFieldM FindField(AbsM.ITableBaseM table, int? id)
        {
            var pgTable = table as PgM.PgTableBaseM;
            if (id == null || pgTable == null)
                return null;

            var fields = pgTable.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Id == id)
                    return fields[i];
            }
            return null;
        }
        /// <summary>
        /// Открыть окно списка объектов
        /// </summary>
        /// <param name="table">Таблица</param>
        public override object OpenTable(AbsM.ITableBaseM table, object id = null, bool isSelected = false, WindowViewModelBase_VM ownerMV = null)
        {
            if (table != null)
            {
                var tableViewVM = new PgTV_VM.PgTableViewVM(table, null, id, isSelected);
                var tableViewV = new PgV.TableView.PgTableViewV();
                if (isSelected)
                    return OpenWindowDialog(
                                tableViewV,
                                tableViewVM,
                                767, 570,
                                500, 300,
                                ownerMV
                            );
                else
                    OpenWindow(
                                tableViewV,
                                tableViewVM,
                                767, 570,
                                500, 300,
                                ownerMV
                            );
            }
            return null;
        }
        /// <summary>
        /// Открыть окно список атрибутов объекта
        /// </summary>
        /// <param name="table"> Таблица</param>
        /// <param name="id"> Идентификатор объекта</param>
        public override void OpenObject(AbsM.ITableBaseM table, object id, String wkt = null, WindowViewModelBase_VM ownerMV = null)
        {
            var pgTable = table as PgM.PgTableBaseM;
            var PgAttributeVM = new PgAtVM.PgAttributesVM(table, id, false, wkt: wkt);

            PgAttributeVM.Reload();

            OpenWindow(
                new PgV.PgAttributesUV(),
                PgAttributeVM,
                750, 550,
                400, 300,
                ownerMV);
        }
        /// <summary>
        /// Показать окно настроек таблицы
        /// </summary>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        public override void OpenTableSettings(AbsM.ITableBaseM iTable, UIElement positionElement = null)
        {
            PgM.PgTableBaseM table = iTable as PgM.PgTableBaseM;
            Rekod.DataAccess.SourcePostgres.View.ConfigView.MapLayerView mapLayerView
                = new PgV.ConfigView.MapLayerView();
            mapLayerView.DataContext = table;
            Window window = new Window();
            window.Title = String.Format("Редактирование свойств таблицы {0}", table.Text);
            window.Content = mapLayerView;
            window.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            window.Height = 500;
            window.Width = 390;
            window.Owner = Program.WinMain;

            System.Windows.Point pt = positionElement.TranslatePoint(new System.Windows.Point(0, 0), Program.WinMain);
            window.Top = pt.Y;
            window.Left = pt.X;
            window.Show();
        }
        #endregion // Методы AbsM.IDataRepositoryVM

        #region Методы AbsVM.DataRepositoryVM
        /// <summary>
        /// Функция изменения видимости слоя. Возвращает видимость слоя после попытки изменения
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        internal override bool MakeLayerVisible(AbsM.TableBaseM table, bool? value)
        {
            mvLayer layer = _mv.getLayer(table.NameMap);
            bool isChecked = (bool)(value);
            cti.ThreadProgress.ShowWait();
            if (isChecked)
            {
                isChecked &= LoadLayerFromSource(table);
            }
            else
            {
                table.IsSelectable = false;
                table.IsEditable = false;
                if (layer != null)
                {
                    layer.deleteLayer();
                    layer = _mv.getLayer(table.NameMap);
                    isChecked = (layer != null);
                }
            }
            cti.ThreadProgress.Close();
            return isChecked;
        }
        /// <summary>
        /// Функция изменения выбираемости слоя. Возвращает true если слой существует и выбираемый
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal override bool MakeLayerSelectable(AbsM.TableBaseM table, bool? value)
        {
            mvLayer layer = _mv.getLayer(table.NameMap);
            bool isChecked = (bool)(value);
            if (layer != null)
            {
                if (isChecked)
                {
                    layer.selectable = true;
                }
                else if (layer != null)
                {
                    layer.selectable = false;
                }
                isChecked = layer.selectable;
            }
            else
            {
                isChecked = false;
            }
            if (layer == null || isChecked == false)
            {
                if (table.IsEditable)
                {
                    table.IsEditable = false;
                }
            }
            return isChecked;
        }
        /// <summary>
        /// Функция изменения редактируемости слоя. Возвращает true если слой существует и редактируемый
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal override bool MakeLayerEditable(AbsM.TableBaseM table, bool? value)
        {
            if (_tableManager.EditableLayer != null && _tableManager.EditableLayer != table)
            {
                _tableManager.EditableLayer.IsEditable = false;
            }
            mvLayer layer = _mv.getLayer(table.NameMap);
            bool isChecked = (bool)(value);
            if (layer != null)
            {
                if (isChecked)
                {
                    if (!table.IsSelectable)
                    {
                        table.IsSelectable = true;
                    }
                    layer.editable = true;
                }
                else if (layer != null)
                {
                    layer.editable = false;
                }
                isChecked = layer.editable;
                if (isChecked)
                {
                    _tableManager.EditableLayer = table;
                }
            }
            else
            {
                isChecked = false;
            }
            return isChecked;
        }
        /// <summary>
        /// Поиск поля в таблце
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатор поля</param>
        /// <returns>Найденное поле</returns>
        public override AbsM.IFieldM FindField(AbsM.ITableBaseM table, object id)
        {
            return FindField(table, id as int?);
        }
        #endregion // Методы AbsVM.DataRepositoryVM

        #region Интерфейс IDisposable
        public override void Dispose()
        {
            base.Dispose();

            Users.Clear();
            Sets.Clear();
            Schems.Clear();
            Extents.Clear();
        }
        #endregion // Интерфейс IDisposable

        /// <summary>
        /// Открывает окно управления правами
        /// </summary>
        internal void OpenAccessWindow()
        {
            string proc = "";
            var frm = new Rekod.DataAccess.SourcePostgres.View.PgRights.AdminRightsV(); //AdminFrm(null);

            try
            {
                proc = cti.ThreadProgress.Open("OpenWindowDialog");
                ReloadPartInfo();
                var vm = new PgListUserRightsVM(this);
                cti.ThreadProgress.Close(proc);
                OpenWindowDialog(frm, vm, 800, 600, 800, 600);
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(proc);
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            finally
            {
                cti.ThreadProgress.Close(proc);
            }

            try
            {
                proc = cti.ThreadProgress.Open("ReloadPartInfo");
                ReloadPartInfo();
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(proc);
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            finally
            {
                cti.ThreadProgress.Close(proc);
            }
            var cls = new classesOfMetods();
            Program.tables_right = cls.loadTableRight();
        }
        public override bool Equals(AbsM.IDataRepositoryM other)
        {
            var rep = other as PgDataRepositoryVM;

            if (rep == null)
                return false;

            if (string.Compare(this._connect.Database, rep._connect.Database, true) == 0    // Database
                && string.Compare(this._connect.Host, rep._connect.Host, true) == 0         // Host
                && this._connect.Port.Equals(rep._connect.Port)                             // Port
                && string.Compare(this._connect.UserName, rep._connect.UserName, true) == 0)// UserName
                return true;
            else
                return false;
        }
        #endregion

        #region Команды
        #region AddGroupToDataContextCommand
        public ICommand AddGroupToDataContextCommand
        {
            get { return _addGroupToDataContextCommand ?? (_addGroupToDataContextCommand = new RelayCommand(this.AddGroupToDataContext)); }
        }
        /// <summary>
        /// Добавляет новую группу в контекст данных элемента который передается в параметре
        /// </summary>
        /// <param name="element">Элемент</param>
        public void AddGroupToDataContext(object element = null)
        {
            if (element is System.Windows.FrameworkElement)
            {
                AbsM.GroupM group = new AbsM.GroupM(this);
                (element as System.Windows.FrameworkElement).DataContext = group;
            }
        }
        #endregion // AddGroupToDataContextCommand

        #region SaveFieldCommand
        public ICommand SaveFieldCommand
        {
            get { return _saveFieldCommandCommand ?? (_saveFieldCommandCommand = new RelayCommand(this.SaveField, this.CanSaveField)); }
        }
        private void SaveField(object obj = null)
        {
            if (!CanSaveField(obj))
                return;
            PgM.PgFieldM field = obj as PgM.PgFieldM;
            DBSaveField(field);
        }
        private bool CanSaveField(object obj = null)
        {
            PgM.PgFieldM field = obj as PgM.PgFieldM;
            return (field != null);
        }
        #endregion

        #region RemoveFieldCommand
        /// <summary>
        /// Команда для удаления атрибута
        /// </summary>
        public ICommand RemoveFieldCommand
        {
            get { return _removeFieldCommand ?? (_removeFieldCommand = new RelayCommand(this.RemoveField, this.CanRemoveField)); }
        }
        /// <summary>
        /// Удаление поля Postgres из программы и базы
        /// </summary>
        /// <param name="fieldobject"></param>
        public void RemoveField(object fieldobject = null)
        {
            PgM.PgFieldM field = fieldobject as PgM.PgFieldM;
            if (field != null)
            {
                MFieldRemove(field);
                DBRemoveField(field, true);
            }
        }
        /// <summary>
        /// Возможно ли удалить поле указанное в параметре
        /// </summary>
        /// <param name="fieldobject"></param>
        /// <returns></returns>
        public bool CanRemoveField(object fieldobject = null)
        {
            PgM.PgFieldM field = fieldobject as PgM.PgFieldM;
            if (field == null)
            {
                return false;
            }
            else
            {
                PgM.PgTableBaseM table = field.Table as PgM.PgTableBaseM;
                if (field.Name == table.PrimaryKey || field.Name == table.GeomField)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        #endregion // RemoveFieldCommand

        #region RemoveTableCommand
        /// <summary>
        /// Команда для удаления таблицы
        /// </summary>
        public ICommand RemoveTableCommand
        {
            get { return _removeTableCommand ?? (_removeTableCommand = new RelayCommand(this.RemoveTable, this.CanRemoveTable)); }
        }
        /// <summary>
        /// Удаление таблицы Postgres из программы и из базы
        /// </summary>
        /// <param name="table"></param>
        public void RemoveTable(object tableobject = null)
        {
            PgM.PgTableBaseM table = tableobject as PgM.PgTableBaseM;
            if (table != null)
            {
                if (MessageBox.Show(
                        String.Format("Вы действительно хотите удалить таблицу \"{0}\"?", table.Text),
                        "Удаление таблицы",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (table != null)
                    {
                        _tables.Remove(table);
                        DBRemoveTable(table);
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли удалить таблицу указанную в параметре
        /// </summary>
        /// <param name="tableobject"></param>
        /// <returns></returns>
        public bool CanRemoveTable(object tableobject = null)
        {
            PgM.PgTableBaseM table = tableobject as PgM.PgTableBaseM;
            return (table != null);
        }
        #endregion // RemoveTableCommand

        #region CopyTableCommand
        /// <summary>
        /// Команда для создания копии существующей таблицы
        /// </summary>
        public ICommand CopyTableCommand
        {
            get { return _copyTableCommand ?? (_copyTableCommand = new RelayCommand(this.CopyTable, this.CanCopyTable)); }
        }
        /// <summary>
        /// Создание новой таблицы через копирование таблицы в параметре
        /// </summary>
        public void CopyTable(object parameter = null)
        {
            PgM.PgTableBaseM pgTable = parameter as PgM.PgTableBaseM;
            if (pgTable != null)
            {
                PgM.PgTableBaseM pgTableMemberwiseClone = ((ICloneable)(pgTable)).Clone() as PgM.PgTableBaseM;
                DBSaveTable(pgTableMemberwiseClone);
            }
        }
        /// <summary>
        /// Можно ли создать копию таблицы
        /// </summary>
        public bool CanCopyTable(object parameter = null)
        {
            return (parameter is PgM.PgTableBaseM);
        }
        #endregion // CopyTableCommand

        #region RemoveGroupCommand
        public ICommand RemoveGroupCommand
        {
            get { return _removeGroupCommand ?? (_removeGroupCommand = new RelayCommand(this.RemoveGroup, this.CanRemoveGroup)); }
        }
        /// <summary>
        /// Удаление группы из Postgres и из базы
        /// </summary>
        /// <param name="table"></param>
        public void RemoveGroup(object groupobject = null)
        {
            AbsM.GroupM group = groupobject as AbsM.GroupM;
            if (group != null)
            {
                _groups.Remove(group);
                DBRemoveGroup(group);
            }
        }
        /// <summary>
        /// Можно ли удалить группу указанную в параметре
        /// </summary>
        /// <param name="tableobject"></param>
        /// <returns></returns>
        public bool CanRemoveGroup(object groupobject = null)
        {
            AbsM.GroupM group = groupobject as AbsM.GroupM;
            return (group != null && group is AbsM.GroupM);
        }
        #endregion // RemoveGroupCommand

        #region OpenHistoryWindowCommand
        /// <summary>
        /// Команда для открытия окна истории
        /// </summary>
        public ICommand OpenHistoryWindowCommand
        {
            get { return _openHistoryWindowCommand ?? (_openHistoryWindowCommand = new RelayCommand(this.OpenHistoryWindow, this.CanOpenHistoryWindow)); }
        }

        /// <summary>
        /// Открытие окна истории
        /// </summary>
        public void OpenHistoryWindow(object obj = null)
        {
            if (!CanOpenHistoryWindow(obj))
                return;

            var table = obj as PgM.PgTableBaseM;
            var user = obj as PgM.PgUserM;
            var rep = obj as Pg_VM.PgDataRepositoryVM;

            Pg_VM.PgHistoryVM pgHistoryVM = new Pg_VM.PgHistoryVM(rep, user, table);
            PgV.History.PgHistoryV pgHistoryV = new PgV.History.PgHistoryV(pgHistoryVM);
            pgHistoryV.Show();
        }
        private bool CanOpenHistoryWindow(object obj)
        {
            var table = obj as PgM.PgTableBaseM;
            var user = obj as PgM.PgUserM;
            var attr = obj as AbsM.IAttributeM;
            var rep = obj as Pg_VM.PgDataRepositoryVM;

            if (table == null && user == null && attr == null && rep == null)
                return false;

            return true;
        }
        #endregion // OpenHistoryWindowCommand

        #region AddNewUserCommand
        /// <summary>
        /// Команда для добавления нового пользователя
        /// </summary>
        public ICommand AddNewUserCommand
        {
            get { return _addNewUserCommand ?? (_addNewUserCommand = new RelayCommand(this.AddNewUser, this.CanAddNewUser)); }
        }
        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        public void AddNewUser(object parameter = null)
        {
            BindingProxy bp = parameter as BindingProxy;
            bp.Data = null;
            bp.Data = new PgM.PgUserM(this);
        }
        /// <summary>
        /// Можно ли добавить пользователя
        /// </summary>
        public bool CanAddNewUser(object parameter = null)
        {
            return true;
        }
        #endregion // AddNewUserCommand

        #region DeleteUserCommand
        /// <summary>
        /// Команда для удаления пользователя
        /// </summary>
        public ICommand DeleteUserCommand
        {
            get { return _deleteUserCommand ?? (_deleteUserCommand = new RelayCommand(this.DeleteUser, this.CanDeleteUser)); }
        }
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        public void DeleteUser(object parameter = null)
        {
            BindingProxy bp = parameter as BindingProxy;
            PgM.PgUserM pgUser = bp.Data as PgM.PgUserM;
            if (pgUser != null)
            {
                if (MessageBox.Show("Вы действительно хотите удалить пользователя?", "Удаление пользователя", MessageBoxButton.YesNo)
                        == MessageBoxResult.Yes)
                {
                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.sql = string.Format(@"delete from sys_scheme.user_db where id={0}", pgUser.ID);
                        sqlCmd.ExecuteNonQuery();
                        sqlCmd.Close();
                        UpdateUsers(pgUser.ID);
                    }
                    bp.Data = null;
                }
            }
        }
        /// <summary>
        /// Можно ли удалить пользователя
        /// </summary>
        public bool CanDeleteUser(object parameter = null)
        {
            BindingProxy bp = parameter as BindingProxy;
            PgM.PgUserM pgUser = bp.Data as PgM.PgUserM;
            return (pgUser != null && pgUser.ID != null);
        }
        #endregion // DeleteUserCommand

        #region CreateObjectCommand
        /// <summary>
        /// Создание объекта через окно списка атрибутов объекта
        /// </summary>
        public ICommand CreateObjectCommand
        {
            get { return _createObjectCommand ?? (_createObjectCommand = new RelayCommand(this.CreateObject, this.CanCreateObject)); }
        }
        /// <summary>
        /// Создание объекта через окно списка атрибутов объекта
        /// </summary>
        public void CreateObject(object param = null)
        {
            AbsM.TableBaseM tableBase = param as AbsM.TableBaseM;
            tableBase.IsVisible = true;
            tableBase.IsEditable = true;
            OpenObject(tableBase, null);
        }
        /// <summary>
        /// Можно ли создать объект
        /// </summary>
        /// <param name="param">Таблица, в которой нужно создать объект</param>
        /// <returns></returns>
        private bool CanCreateObject(object param = null)
        {
            AbsM.TableBaseM tableBase = param as AbsM.TableBaseM;
            if (tableBase != null && tableBase.UserAccess == AbsM.UserAccess.Write)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // CreateObjectCommand

        #region CreateObjectInMapCommand
        /// <summary>
        /// Добавить объект на карту
        /// </summary>
        public ICommand CreateObjectInMapCommand
        {
            get { return _createObjectInMapCommand ?? (_createObjectInMapCommand = new RelayCommand(this.CreateObjectInMap, this.CanCreateObject)); }
        }
        /// <summary>
        /// Добавить объекта на карту
        /// </summary>
        public void CreateObjectInMap(object param = null)
        {
            AbsM.TableBaseM tableBase = param as AbsM.TableBaseM;

            tableBase.IsVisible = true;
            tableBase.IsEditable = true;

            switch ((tableBase as PgM.PgTableBaseM).GeomType)
            {
                case AbsM.EGeomType.Point:
                    {
                        _axMapLib1.CtlCursor = mvMapLib.Cursors.mlAddDot;
                        break;
                    }
                case AbsM.EGeomType.Line:
                    {
                        _axMapLib1.CtlCursor = mvMapLib.Cursors.mlAddPolyLine;
                        break;
                    }
                case AbsM.EGeomType.Polygon:
                    {
                        _axMapLib1.CtlCursor = mvMapLib.Cursors.mlAddPolygon;
                        break;
                    }
            }
        }
        #endregion // AddObjectToMapCommand

        #region NewFieldCommand
        private ICommand _newFieldCommand;
        /// <summary>
        /// Команда для создания нового поля
        /// </summary>
        public ICommand NewFieldCommand
        {
            get { return _newFieldCommand ?? (_newFieldCommand = new RelayCommand(this.NewField, this.CanNewField)); }
        }
        /// <summary>
        /// Создание нового поля, передача его в BindingProxy
        /// </summary>
        public void NewField(object parameter = null)
        {
            List<object> commParams = parameter as List<object>;
            PgM.PgFieldM pgField = new PgFieldM(commParams[0] as PgM.PgTableBaseM);
            BindingProxy bindProxy = commParams[1] as BindingProxy;
            bindProxy.Data = null;
            bindProxy.Data = pgField;
        }
        /// <summary>
        /// Можно ли создать новое поле
        /// </summary>
        public bool CanNewField(object parameter = null)
        {
            List<object> commParams = parameter as List<object>;
            return commParams[0] is PgM.PgTableBaseM;
        }
        #endregion // NewFieldCommand

        #region NewTableCommand
        private ICommand _newTableCommand;
        /// <summary>
        /// Команда для создания новой таблицы и помещения в BindingProxy
        /// </summary>
        public ICommand NewTableCommand
        {
            get { return _newTableCommand ?? (_newTableCommand = new RelayCommand(this.NewTable, this.CanNewTable)); }
        }
        /// <summary>
        /// Создание новой таблицы. Помещение в BindingProxy
        /// </summary>
        public void NewTable(object parameter = null)
        {
            List<object> commParams = parameter as List<object>;
            BindingProxy bindProxy = commParams[0] as BindingProxy;
            AbsM.ETableType type = (AbsM.ETableType)(Enum.Parse(typeof(AbsM.ETableType), commParams[1].ToString()));
            PgM.PgTableBaseM pgTable = new PgTableBaseM(this, 0, type);
            bindProxy.Data = null;
            bindProxy.Data = pgTable;
        }
        /// <summary>
        /// Комментарий_к_методу_CanActionMethod
        /// </summary>
        public bool CanNewTable(object parameter = null)
        {
            return true;
        }
        #endregion // NewTableCommand
        #endregion // Команды

        #region Обработчики
        /// <summary>
        /// Обработка событий изменения коллекий из AbsVM.DataRepositoryVM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.Tables_CollectionChanged(sender, e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var tablePg = e.NewItems[i] as PgM.PgTableBaseM;
                        switch (tablePg.Type)
                        {
                            case AbsM.ETableType.MapLayer:    // выполняется в базовом классе
                                break;
                            case AbsM.ETableType.Catalog:
                                _catalogTables.Add(tablePg);
                                break;
                            case AbsM.ETableType.Interval:
                                _intervalTables.Add(tablePg);
                                break;
                            case AbsM.ETableType.Data:
                                _dataTables.Add(tablePg);
                                break;
                            default:
                                Debug.Fail("Нельзя добавлять таблицу не связанную с Postgres");
                                break;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var tablePg = e.OldItems[i] as PgM.PgTableBaseM;
                        switch (tablePg.Type)
                        {
                            case AbsM.ETableType.MapLayer:      // выполняется в базовом классе
                                break;
                            case AbsM.ETableType.Catalog:
                                _catalogTables.Remove(tablePg);
                                break;
                            case AbsM.ETableType.Interval:
                                _intervalTables.Remove(tablePg);
                                break;
                            case AbsM.ETableType.Data:
                                _dataTables.Remove(tablePg);
                                break;
                            default:
                                Debug.Fail("Нельзя удалять таблицу не связанную с Postgres");
                                break;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _catalogTables.Clear();
                    _intervalTables.Clear();
                    _dataTables.Clear();
                    break;
            }
        }
        #endregion Обработчики
    }

    public struct DeltaColor
    {
        public double R;
        public double G;
        public double B;
    }
}