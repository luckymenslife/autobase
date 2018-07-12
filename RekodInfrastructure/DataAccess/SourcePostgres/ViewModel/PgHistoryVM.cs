using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgAt_M = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgHistM = Rekod.DataAccess.SourcePostgres.Model.PgHistory;
using System.Collections.ObjectModel;
using Npgsql;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource;
using System.Data;
using Rekod.DataAccess.SourcePostgres.View.History;
using mvMapLib;
using Interfaces;
using System.Windows;
using Rekod.Controllers;
using Rekod.Services;
using System.Windows.Media;
using cti;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgHistoryVM : ViewModelBase
    {
        #region Поля
        private ObservableCollection<PgHistM.PgHistoryDate> _dates;
        private ObservableCollection<PgM.PgUserM> _users;
        private ObservableCollection<PgM.PgTableBaseM> _tables;
        private ObservableCollection<Int32> _objects;
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private NpgsqlConnectionStringBuilder _connect;
        private ICommand _loadAssociatedDataCommand;
        private ICommand _chooseTablesCommand;
        private ICommand _chooseUsersCommand;
        private ICommand _loadDatesCommand;
        private ICommand _clearUsersAndTablesCommand;
        private ICommand _clearGeometryCommand;
        private ICommand _restoreEventCommand;
        private ICommand _restoreAttributeCommand;
        private AxmvMapLib.AxMapLIb _axMapLib;
        private Guid _guid;
        private Boolean _showChanges;
        private Brush _dateBrush = Brushes.Gray;
        #endregion Поля

        #region Конструкторы
        public PgHistoryVM(PgVM.PgDataRepositoryVM source, PgM.PgUserM user = null, PgM.PgTableBaseM table = null, int? idObject = null)
        {
            _guid = Guid.NewGuid();
            _axMapLib = Program.mainFrm1.axMapLIb1;
            this.Source = source;
            UseUsersFilter = UseTablesFilter = true;
            _connect = source.Connect;
            if (user != null)
            {
                Users.Add(user);
                UseUsersFilter = false;
            }
            if (table != null)
            {
                Tables.Add(table);
                UseTablesFilter = false;
            }
            if (idObject != null)
            {
                Objects.Add(idObject.Value);
            }
            DateFrom = DateTime.Now.AddMonths(-1);
            DateTo = DateTime.Now;
            LoadDates();
            Users.CollectionChanged += Users_CollectionChanged;
            Tables.CollectionChanged += Tables_CollectionChanged;
        }
        #endregion Конструкторы

        #region Свойства
        public Window AttachedWindow { get; set; }
        public PgVM.PgDataRepositoryVM Source { get; private set; }
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                OnPropertyChanged(ref _dateFrom, value, () => this.DateFrom);
                DateBrush = _dateFrom > _dateTo ? Brushes.Red : Brushes.Gray;                
            }
        }
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                OnPropertyChanged(ref _dateTo, value, () => this.DateTo);
                DateBrush = _dateFrom > _dateTo ? Brushes.Red : Brushes.Gray;
            }
        }
        public Brush DateBrush
        {
            get { return _dateBrush; }
            set { OnPropertyChanged(ref _dateBrush, value, () => this.DateBrush); }
        }
        public Boolean HideActualValues
        {
            get;
            set;
        }
        public Boolean UseUsersFilter
        {
            get;
            private set;
        }
        public Boolean UseTablesFilter
        {
            get;
            private set;
        }
        public Boolean ShowChanges
        {
            get { return _showChanges; }
            set { OnPropertyChanged(ref _showChanges, value, () => this.ShowChanges); }
        }
        #endregion Свойства

        #region Коллекции
        public ObservableCollection<PgHistM.PgHistoryDate> Dates
        {
            get
            { return _dates ?? (_dates = new ObservableCollection<PgHistM.PgHistoryDate>()); }
        }
        public ObservableCollection<PgM.PgUserM> Users
        {
            get
            { return _users ?? (_users = new ObservableCollection<PgM.PgUserM>()); }
        }
        public ObservableCollection<PgM.PgTableBaseM> Tables
        {
            get
            { return _tables ?? (_tables = new ObservableCollection<PgM.PgTableBaseM>()); }
        }
        public ObservableCollection<Int32> Objects
        {
            get { return _objects ?? (_objects = new ObservableCollection<int>()); }
        }
        public List<String> TableIds
        {
            get
            {
                List<String> _tableIds = new List<String>();
                foreach (PgM.PgTableBaseM table in Tables)
                {
                    _tableIds.Add(table.Id.ToString());
                }
                return _tableIds;
            }
        }
        public List<String> UserLogins
        {
            get
            {
                List<String> _userLogins = new List<String>();
                foreach (PgM.PgUserM user in Users)
                {
                    if (user.ID != null)
                        _userLogins.Add(user.Login);
                }
                return _userLogins;
            }
        }
        #endregion Коллекции

        #region Методы
        /// <summary>
        /// Загружает дату событиями
        /// </summary>
        /// <param name="histDate">Дата которую нужно заполнить или обновить</param>
        public void LoadDate(PgHistM.PgHistoryDate histDate, bool loadMore = false)
        {
            ThreadProgress.ShowWait();

            try
            {
                int selectLimit = 0;
                int selectOffset = 0;

                if (!loadMore)
                {
                    ClearGeometry(false);
                    histDate.Events.Clear();
                    selectLimit = histDate.LoadByCount;
                    selectOffset = 0;
                }
                else
                {
                    selectLimit = histDate.LoadByCount;
                    selectOffset = histDate.EventsLoaded;
                }

                String usersCondition = "";
                if (Users.Count > 0)
                {
                    usersCondition = String.Format("AND hl.user_name in ('{0}')", String.Join("', '", UserLogins.ToArray()));
                }
                String tablesCondition = "";
                if (Tables.Count > 0)
                {
                    tablesCondition = String.Format("AND hl.id_table in ({0})", String.Join(", ", TableIds.ToArray()));
                }
                String objectsCondition = "";
                if (Objects.Count > 0)
                {
                    objectsCondition += String.Format("AND hl.id_object in ({0})", String.Join(", ", Objects.ToArray()));
                }
                String sql = String.Format(@"
                                SELECT count(*)
                                FROM sys_scheme.get_history_list_by_object() hl
                                    INNER JOIN sys_scheme.user_db us ON (hl.user_name = us.login)
                                    INNER JOIN sys_scheme.table_info ti ON (hl.id_table = ti.id)
                                    INNER JOIN sys_scheme.table_right tr ON hl.id_table = tr.id_table AND tr.id_user = {0} AND tr.read_data = TRUE
                                WHERE hl.data_changes::date = '{1}' {2} {3} {4}",
                                    Program.repository.CurrentUser.ID,
                                    histDate.HistoryDate.ToString("yyyy-MM-dd"),
                                    usersCondition,
                                    tablesCondition,
                                    objectsCondition);


                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    sqlWork.sql = sql;
                    int count = sqlWork.ExecuteScalar<Int32>();
                    sqlWork.Connection.Open();

                    histDate.EventsCount = count;
                    histDate.EventsLoaded =
                        histDate.Events.Count + ((selectLimit < count - histDate.Events.Count) ? selectLimit : count - histDate.Events.Count);
                    histDate.LoadMore = histDate.EventsLoaded < histDate.EventsCount;

                    sql =
                    String.Format(@"
                                SELECT 
                                    hl.user_name,
                                    hl.data_changes, 
                                    hl.id_history,
                                    hl.type_operation, 
                                    hl.id_history_table, 
                                    hl.id_table,
                                    hl.id_object,
                                    us.name_full, 
                                    ti.name_map
                                FROM sys_scheme.get_history_list_by_object() hl
                                    INNER JOIN sys_scheme.user_db us ON (hl.user_name = us.login)
                                    INNER JOIN sys_scheme.table_info ti ON (hl.id_table = ti.id)
                                    INNER JOIN sys_scheme.table_right tr ON hl.id_table = tr.id_table AND tr.id_user = {0} AND tr.read_data = TRUE
                                WHERE hl.data_changes::date = '{1}' {2} {3} {4}
                                ORDER BY hl.data_changes DESC LIMIT {5} OFFSET {6}",
                                    Program.repository.CurrentUser.ID,
                                    histDate.HistoryDate.ToString("yyyy-MM-dd"),
                                    usersCondition,
                                    tablesCondition,
                                    objectsCondition,
                                    selectLimit,
                                    selectOffset);

                    sqlWork.sql = sql;
                    sqlWork.ExecuteReader();
                    while (sqlWork.CanRead())
                    {
                        PgHistM.PgHistoryEvent histEvent = new PgHistM.PgHistoryEvent(histDate)
                        {
                            UserName = sqlWork.GetString(7),
                            ChangeTime = DateTime.Parse(sqlWork.GetString(1)),
                            IdHistoryObject = sqlWork.GetInt32(2),
                            TypeOperation = (PgHistM.PgHistoryTypeOperation)sqlWork.GetInt32(3),
                            IdHistoryTable = sqlWork.GetInt32(4),
                            IdTable = sqlWork.GetInt32(5),
                            IdObject = sqlWork.GetInt32(6),
                            TableName = sqlWork.GetString(8)
                        };
                        histDate.Events.Add(histEvent);
                    }
                    sqlWork.Close();
                }
                histDate.EventsAreLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ThreadProgress.Close();
            }
        }
        /// <summary>
        /// Загружает событие атрибутами
        /// </summary>
        /// <param name="histEvent">Событие которое нужно заполнить или обновить</param>
        public void LoadEvent(PgHistM.PgHistoryEvent histEvent)
        {
            PgM.PgTableBaseM table = (PgM.PgTableBaseM)Source.FindTable(histEvent.IdTable);
            histEvent.Attributes.Clear();

            // Составляем список атрибутов которые попадут в событие истории
            foreach (PgM.PgFieldM field in table.Fields)
            {
                if (field.Name != table.PrimaryKey)
                {
                    PgHistM.PgHistoryAttribute attr = new PgHistM.PgHistoryAttribute(histEvent);
                    attr.Field = field;
                    if (field.Name == table.GeomField)
                    {
                        attr.IsGeomField = true;
                    }
                    histEvent.Attributes.Add(attr);
                }
            }
            bool loadStyle = Source == Program.repository ? false : true;
            PgVM.PgAttributes.PgAttributesListVM actualValues =
                new PgVM.PgAttributes.PgAttributesListVM(table, histEvent.IdObject, false, isReadOnly: true, loadStyle: loadStyle);
            actualValues.Reload();

            PgVM.PgAttributes.PgAttributesListVM afterValues =
                new PgVM.PgAttributes.PgAttributesListVM(table, histEvent.IdObject, true, histEvent.IdHistoryTable, histEvent.IdHistoryObject, true, loadStyle: loadStyle);
            afterValues.Reload();

            histEvent.AfterValuesListVM = afterValues;
            PgVM.PgAttributes.PgAttributesListVM beforeValues = null;

            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql =
                    String.Format(@"SELECT id_history, id_history_table
                                FROM sys_scheme.get_history_list_by_object({0}, {1})
                                WHERE id_history < {2}
                                ORDER BY id_history DESC LIMIT 1",
                                                    histEvent.IdTable,
                                                    histEvent.IdObject,
                                                    histEvent.IdHistoryObject);
                sqlWork.ExecuteReader();
                if (sqlWork.CanRead())
                {
                    int idHistoryObject = sqlWork.GetInt32("id_history");
                    int idHistoryTable = sqlWork.GetInt32("id_history_table");
                    beforeValues = new PgVM.PgAttributes.PgAttributesListVM(table, histEvent.IdObject, true, idHistoryTable, idHistoryObject, true, loadStyle: loadStyle);
                    beforeValues.Reload();
                }
                sqlWork.Close();
            }
            foreach (PgHistM.PgHistoryAttribute attr in histEvent.Attributes)
            {
                attr.ActualValue = actualValues.FindAttribute(attr.Field);
                attr.AfterValue = afterValues.FindAttribute(attr.Field);
                if (beforeValues != null)
                {
                    attr.BeforeValue = beforeValues.FindAttribute(attr.Field);
                }
                if (attr.IsGeomField)
                {
                    attr.ActualValue = actualValues.GeomAttribute;
                    attr.AfterValue = afterValues.GeomAttribute;
                    if (beforeValues != null)
                    {
                        attr.BeforeValue = beforeValues.GeomAttribute;
                    }
                }
            }
        }
        /// <summary>
        /// Отображает или скрывает геометрию на карте
        /// </summary>
        /// <param name="valueType">Тип значения, который нужно отобразить</param>
        /// <param name="geomWkt">Геометрия, которую нужно отобразить. Если null - удаляет объект</param>
        public void SwitchGeometry(PgHistM.PgHistoryAttribute.HistoryAttributeValueType valueType, String geomWkt = null)
        {
            String layerName = String.Format("HistoryView_{0}", _guid);
            mvLayer layer = _axMapLib.getLayer(layerName);
            if (layer != null)
            {
                int idToDelete = (int)valueType + 2;
                layer.DeleteID(idToDelete);
                layer.RemoveDeletedObjects();
            }

            if (geomWkt != null)
            {
                int styleIndex = -1;
                if (geomWkt.Contains("POINT"))
                    styleIndex = 0 + (int)valueType;
                if (geomWkt.Contains("LINE"))
                    styleIndex = 3 + (int)valueType;
                if (geomWkt.Contains("POLYGON"))
                    styleIndex = 6 + (int)valueType;

                if (layer == null)
                {
                    mvStringArray ff = new mvStringArray();
                    ff.count = 1;
                    ff.setElem(0, "id");
                    layer = _axMapLib.CreateLayer(layerName, ff);
                }

                #region задаем стили
                int[] styles = new int[9];

                //DotStyles
                mvSymbolObject symbol = new mvSymbolObject() { shape = 0x21 };
                mvFontObject font = new mvFontObject()
                {
                    fontname = "Map Symbols",
                    size = 15,
                    Color = 0x00FF00,
                    framecolor = 0x00FF00
                };
                styles[0] = layer.CreateDotStyle(symbol, font);
                font = new mvFontObject()
                {
                    fontname = "Map Symbols",
                    size = 15,
                    Color = 0xFF0000,
                    framecolor = 0xFF0000
                };
                styles[1] = layer.CreateDotStyle(symbol, font);
                font = new mvFontObject()
                {
                    fontname = "Map Symbols",
                    size = 15,
                    Color = 0xFFFF00,
                    framecolor = 0xFFFF00
                };
                styles[2] = layer.CreateDotStyle(symbol, font);
                //LineStyles, PolygonStyles
                mvPenObject pen_line = new mvPenObject()
                {
                    Color = 0x00FF00,
                    ctype = 2,
                    width = 2
                };
                styles[3] = layer.CreateLineStyle(pen_line);
                mvBrushObject brush = new mvBrushObject()
                {
                    bgcolor = 0x00FF00,
                    fgcolor = 0x00FF00,
                    style = 0,
                    hatch = 2
                };
                styles[6] = layer.CreatePolygonStyle(pen_line, brush);
                pen_line = new mvPenObject()
                {
                    Color = 0xFF0000,
                    ctype = 2,
                    width = 2
                };
                styles[4] = layer.CreateLineStyle(pen_line);
                brush = new mvBrushObject()
                {
                    bgcolor = 0xFF0000,
                    fgcolor = 0xFF0000,
                    style = 0,
                    hatch = 2
                };
                styles[7] = layer.CreatePolygonStyle(pen_line, brush);
                pen_line = new mvPenObject()
                {
                    Color = 0x008Ea1c8,
                    ctype = 2,
                    width = 2
                };
               
                //Converters.IntColorConverter intColorConverter = new Converters.IntColorConverter();
                //Color color = Color.FromRgb(185, 122, 87);
                //uint colorPgValue = Convert.ToUInt32(intColorConverter.ConvertBack(color, null, null, null)); 
                
                //bgcolor = Convert.ToUInt32(
                //        185 +
                //        (122 << 8) +
                //        (87 << 16) +
                //        (0 << 24)),

                styles[5] = layer.CreateLineStyle(pen_line);
                brush = new mvBrushObject()
                {
                    bgcolor = 0x00000000,
                    fgcolor = 0x008Ea1c8,
                    style = 0,
                    hatch = 2
                };
                styles[8] = layer.CreatePolygonStyle(pen_line, brush);
                #endregion

                int newObjectId = (int)valueType + 2;
                mvStringArray f2 = new mvStringArray();
                f2.count = 1;
                f2.setElem(0, (newObjectId).ToString());

                mvVectorObject o1 = layer.CreateObject();
                o1.setWKT(geomWkt);
                o1.SetAttributes(f2);
                o1.style = styles[styleIndex];

                mvPointWorld gb = new mvPointWorld();
                gb.x = o1.Centroid.x;
                gb.y = o1.Centroid.y;
                float x = Convert.ToSingle(Math.Abs(o1.bbox.b.x - o1.bbox.a.x));
                float y = Convert.ToSingle(Math.Abs(o1.bbox.b.y - o1.bbox.a.y));
                float z = (x > y) ? x : y;
                if (z > 0)
                {
                    _axMapLib.ScaleZoom = z * 8;
                }
                _axMapLib.MoveTo(gb);
            }
            _axMapLib.mapRepaint();
        }
        /// <summary>
        /// Восстанавливает значения из таблицы истории
        /// </summary>
        /// <param name="parentEvent">Событие которое используется для восстановления</param>
        /// <param name="attr">Если не null, восстанавливает единственное значение</param>
        public void Restore(PgHistM.PgHistoryEvent parentEvent, PgHistM.PgHistoryAttribute attr = null, bool afterValue = true)
        {
            List<String> columnsInTableList = new List<string>();
            List<String> columnsInHistoryTableList = new List<string>();
            PgM.PgTableBaseM table = Source.FindTable(parentEvent.IdTable) as PgM.PgTableBaseM;
            if (attr == null)
            {
                foreach (PgHistM.PgHistoryAttribute eventAttr in parentEvent.Attributes)
                {
                    columnsInTableList.Add(eventAttr.Field.Name);
                }
            }
            else
            {
                columnsInTableList.Add(attr.Field.Name);
            }
            //Получаем список полей в таблице истории, которые также существуют в основной таблице
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                String sql = String.Format(@"
                    SELECT isc.column_name
                    FROM sys_scheme.table_history_info thi
                    INNER JOIN sys_scheme.table_info ti
                    ON thi.id_table = ti.id
                    INNER JOIN information_schema.columns isc
                    ON isc.table_name = thi.history_table_name AND isc.table_schema = ti.scheme_name
                    WHERE thi.id_history_table = {0} AND  isc.column_name in ('{1}')",
                            parentEvent.IdHistoryTable,
                            String.Join("', '", columnsInTableList.ToArray()));
                sqlWork.sql = sql;
                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    columnsInHistoryTableList.Add(sqlWork.GetString(0));
                }
                sqlWork.Close();
            }


            if (columnsInHistoryTableList.Count > 0)
            {
                // Получаем название таблицы истории
                String historyTableName;
                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    sqlWork.sql = String.Format(@"SELECT thi.history_table_name
                                                            FROM sys_scheme.table_info ti 
                                                            INNER JOIN sys_scheme.table_history_info thi ON ti.id = thi.id_table
                                                            WHERE thi.id_history_table = {0}", parentEvent.IdHistoryTable);
                    historyTableName = sqlWork.ExecuteScalar().ToString();
                }
                // Восстановление данных из таблицы истории
                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    String setList = "";
                    foreach (String colName in columnsInHistoryTableList)
                    {
                        if (!String.IsNullOrEmpty(setList))
                        {
                            setList += ", ";
                        }
                        setList += String.Format("{0}=__histtable.{0}", colName);
                    }
                    sqlWork.sql = String.Format("UPDATE {0}.{1} __maintable SET {5} FROM {0}.{2} __histtable WHERE __maintable.{3} = :idvalue AND __histtable.id_history = {4}",
                                                    table.SchemeName,
                                                    table.Name,
                                                    historyTableName,
                                                    table.PrimaryKey,
                                                    parentEvent.IdHistoryObject,
                                                    setList);
                    List<IParams> paramsArray = new List<IParams>();
                    paramsArray.Add(new Params(":idvalue", parentEvent.IdObject, (table.PrimaryKeyField as PgM.PgFieldM).DbType));
                    sqlWork.ExecuteNonQuery(paramsArray);
                }
            }
        }
        /// <summary>
        /// Восстановление из таблицы истории
        /// </summary>
        /// <param name="idTable">Идентификатор таблицы</param>
        /// <param name="idObject">Идентификатор объекта</param>
        /// <param name="idHistoryTable">Идентификатор таблицы истории</param>
        /// <param name="idHistoryObject">Идентификатор записи в таблице истории</param>
        /// <param name="tableFields">Список полей которые нужно восстановить</param>
        /// <returns>Возвращает список атрибутов, которые удалось восстановить</returns>
        public void RestoreFromHistory(int idTable, int idObject, int idHistoryTable, int idHistoryObject, List<String> tableFields)
        {
            List<String> tableHistoryFields = new List<string>();
            PgM.PgTableBaseM table = Source.FindTable(idTable) as PgM.PgTableBaseM;

            bool canWrite = false;
            //Определяем, есть ли у текущего пользователя права на запись в таблицу
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format(
                    "SELECT write_data FROM sys_scheme.table_right WHERE id_user = {0} AND id_table = {1};",
                    Program.repository.CurrentUser.ID,
                    idTable);
                canWrite = sqlWork.ExecuteScalar<bool>();
            }
            if (!canWrite)
            {
                throw new Exception("У вас нет прав на запись в данную таблицу!");
            }
            else
            {
                //Получаем список полей в таблице истории, которые также существуют в основной таблице
                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    String sql = String.Format(@"
                    SELECT isc.column_name
                    FROM sys_scheme.table_history_info thi
                    INNER JOIN sys_scheme.table_info ti
                    ON thi.id_table = ti.id
                    INNER JOIN information_schema.columns isc
                    ON isc.table_name = thi.history_table_name AND isc.table_schema = ti.scheme_name
                    WHERE thi.id_history_table = {0} AND  isc.column_name in ('{1}')",
                                idHistoryTable,
                                String.Join("', '", tableFields.ToArray()));
                    sqlWork.sql = sql;
                    sqlWork.ExecuteReader();
                    while (sqlWork.CanRead())
                    {
                        tableHistoryFields.Add(sqlWork.GetString(0));
                    }
                    sqlWork.Close();
                }
                if (tableHistoryFields.Count > 0)
                {
                    // Получаем название таблицы истории
                    String historyTableName;
                    using (SqlWork sqlWork = new SqlWork(_connect))
                    {
                        sqlWork.sql = String.Format(@"SELECT thi.history_table_name
                                                            FROM sys_scheme.table_info ti 
                                                            INNER JOIN sys_scheme.table_history_info thi ON ti.id = thi.id_table
                                                            WHERE thi.id_history_table = {0}", idHistoryTable);
                        historyTableName = sqlWork.ExecuteScalar().ToString();
                    }
                    // Проверяем, есть ли такой объект в базе
                    bool objectExists = false;
                    using (SqlWork sqlWork = new SqlWork(_connect))
                    {
                        sqlWork.sql = String.Format("SELECT EXISTS(SELECT * FROM \"{0}\".\"{1}\" WHERE gid = {2})",
                                            table.SchemeName,
                                            table.Name,
                                            idObject);
                        objectExists = sqlWork.ExecuteScalar<Boolean>();
                    }
                    // Восстановление данных из таблицы истории
                    if (objectExists)
                    {
                        using (SqlWork sqlWork = new SqlWork(_connect))
                        {
                            String setList = "";
                            foreach (String colName in tableHistoryFields)
                            {
                                if (!String.IsNullOrEmpty(setList))
                                {
                                    setList += ", ";
                                }
                                setList += String.Format("\"{0}\"=__histtable.\"{0}\"", colName);
                            }
                            sqlWork.sql =
                                String.Format(
                                    "UPDATE \"{0}\".\"{1}\" __maintable SET {5} FROM \"{0}\".\"{2}\" __histtable WHERE __maintable.{3} = {6} AND __histtable.id_history = {4}",
                                        table.SchemeName,
                                        table.Name,
                                        historyTableName,
                                        table.PrimaryKey,
                                        idHistoryObject,
                                        setList,
                                        idObject);
                            sqlWork.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlWork sqlWork = new SqlWork(_connect))
                        {
                            tableHistoryFields.Add(table.PrimaryKey);
                            List<String> insertFields = (from String f in tableHistoryFields select String.Format("\"{0}\"", f)).ToList();
                            sqlWork.sql = String.Format(
                                "INSERT INTO \"{0}\".\"{1}\"({3}) SELECT {3} FROM \"{0}\".\"{2}\" WHERE id_history = {4}",
                                table.SchemeName,
                                table.Name,
                                historyTableName,
                                String.Join(",", insertFields),
                                idHistoryObject
                                );
                            sqlWork.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        #endregion Методы

        #region Команды
        #region LoadAssociatedDataCommand
        /// <summary>
        /// Команда для подгрузки ассоциированных с параметром данных
        /// </summary>
        public ICommand LoadAssociatedDataCommand
        {
            get { return _loadAssociatedDataCommand ?? (_loadAssociatedDataCommand = new RelayCommand(this.LoadAssociatedData, this.CanLoadAssociatedData)); }
        }
        /// <summary>
        /// Загрузка ассоциированных данных
        /// </summary>
        public void LoadAssociatedData(object parameter = null)
        {
            if (parameter is PgHistM.PgHistoryDate)
            {
                PgHistM.PgHistoryDate histDate = parameter as PgHistM.PgHistoryDate;
                if (!histDate.EventsAreLoaded)
                {
                    LoadDate(histDate);
                }
            }
            else if (parameter is PgHistM.PgHistoryEvent)
            {
                PgHistM.PgHistoryEvent histEvent = parameter as PgHistM.PgHistoryEvent;
                LoadEvent(histEvent);                
            }
        }
        /// <summary>
        /// Можно ли подгрузить ассоциированные с параметром данные
        /// </summary>
        public bool CanLoadAssociatedData(object parameter = null)
        {
            if (parameter is PgHistM.PgHistoryDate || parameter is PgHistM.PgHistoryEvent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // LoadAssociatedDataCommandCommand

        #region ChooseTablesCommand
        /// <summary>
        /// Команда для выбора таблиц для поиска
        /// </summary>
        public ICommand ChooseTablesCommand
        {
            get { return _chooseTablesCommand ?? (_chooseTablesCommand = new RelayCommand(this.ChooseTables, this.CanChooseTables)); }
        }
        /// <summary>
        /// Выбор таблиц для поиска
        /// </summary>
        public void ChooseTables(object parameter = null)
        {
            DataTable selTablesDt = null;
            using (var selTables = new SqlWork(_connect))
            {
                selTables.sql = string.Format(@"SELECT DISTINCT ti.id, ttt.name, ti.name_map, 
                            (select trim(trailing ', ' from {0}.concat(name_group||', ')) from 
                            (select tg.name_group from {0}.table_groups_table tgt
                            left join {0}.table_groups tg on tg.id=tgt.id_group
                            where tgt.id_table=ti.id
                            order by tgt.order_num) as g) as groups
                            FROM {0}.table_info ti left join {0}.table_right tr 
                             on tr.id_table=ti.id 
                              left join geometry_columns 
                               on f_table_schema = ti.scheme_name AND f_table_name = ti.name_db AND f_geometry_column = ti.geom_field 
                                left join 
                                 (SELECT id_table, true as haves FROM {0}.table_history_info GROUP BY id_table) th 
                                  ON ti.id = th.id_table
                                   left join {0}.table_type_table ttt
                                    on ttt.id=ti.type
                            WHERE tr.read_data=true", "sys_scheme");
                selTables.sql += " ORDER BY ti.name_map";

                selTablesDt = selTables.ExecuteGetTable();
                selTables.Close();
            }
            selTablesDt.Columns["name"].Caption = Rekod.Properties.Resources.HistoryForm_Type;
            selTablesDt.Columns["name_map"].Caption = Rekod.Properties.Resources.HistoryForm_TableName;
            selTablesDt.Columns["groups"].Caption = Rekod.Properties.Resources.HistoryForm_Groups;

            List<int> defChecked = new List<int>();
            foreach (PgM.PgTableBaseM table in Tables)
            {
                defChecked.Add(table.Id);
            }

            Window checkWindow = new Window();
            Rekod.Controls.CheckObjects co = new Rekod.Controls.CheckObjects(selTablesDt, defChecked, "id", checkWindow);
            checkWindow.Title = Rekod.Properties.Resources.HistoryForm_SelectTable;
            checkWindow.Content = co; 
            checkWindow.Owner = AttachedWindow;
            checkWindow.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            checkWindow.Height = 400;
            checkWindow.Width = 750; 

            checkWindow.ShowDialog();
            if (co.Cancelled == false)
            {
                bool collDifferent = false;
                if (Tables.Count != co.CheckedList.Count)
                {
                    collDifferent = true;
                }
                else
                {
                    var diffColl = from PgM.PgTableBaseM pgTable in Tables select !co.CheckedList.Contains(pgTable.Id); 
                    foreach (var diff in diffColl)
                    {
                        collDifferent |= diff; 
                    }
                }
                if (collDifferent)
                {
                    Tables.Clear();
                    foreach (int checkedId in co.CheckedList)
                    {
                        PgM.PgTableBaseM table = Source.FindTable(checkedId) as PgM.PgTableBaseM;
                        Tables.Add(table);
                    }
                }
            }            
        }
        /// <summary>
        /// Можно ли открыть окно для выбора таблиц
        /// </summary>
        public bool CanChooseTables(object parameter = null)
        {
            return UseTablesFilter;
        }
        #endregion // ChooseTablesCommand

        #region ChooseUsersCommand
        /// <summary>
        /// Команда для выбора пользователей в фильтре
        /// </summary>
        public ICommand ChooseUsersCommand
        {
            get { return _chooseUsersCommand ?? (_chooseUsersCommand = new RelayCommand(this.ChooseUsers, this.CanChooseUsers)); }
        }
        /// <summary>
        /// Выбор пользователей
        /// </summary>
        public void ChooseUsers(object parameter = null)
        {
            DataTable selUsersDt = null;
            using (var selUsers = new SqlWork(_connect))
            {
                selUsers.sql = string.Format(
                    @"SELECT id, login, name_full, type_name 
                    FROM {0}.user_db udb left join {0}.typ_users tu
                    on udb.typ=tu.id_user", "sys_scheme");
                selUsersDt = selUsers.ExecuteGetTable();
                selUsers.Close();
            }
            List<int> defChecked = new List<int>();
            foreach (PgM.PgUserM user in Users)
            {
                defChecked.Add((int)user.ID);
            }
            selUsersDt.Columns["login"].Caption = Rekod.Properties.Resources.HistoryForm_Login;
            selUsersDt.Columns["name_full"].Caption = Rekod.Properties.Resources.HistoryForm_UserName;
            selUsersDt.Columns["type_name"].Caption = Rekod.Properties.Resources.HistoryForm_TypeUser;

            Window checkWindow = new Window();
            Rekod.Controls.CheckObjects co = new Rekod.Controls.CheckObjects(selUsersDt, defChecked, "id", checkWindow);
            checkWindow.Title = Rekod.Properties.Resources.HistoryForm_SelectUsers;
            checkWindow.Content = co;
            checkWindow.Owner = AttachedWindow;
            checkWindow.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            checkWindow.Height = 400;
            checkWindow.Width = 750;

            checkWindow.ShowDialog();
            if (co.Cancelled == false)
            {
                bool collDifferent = false;
                if (Users.Count != co.CheckedList.Count)
                {
                    collDifferent = true;
                }
                else
                {
                    var diffColl = from PgM.PgUserM pgUser in Users select !co.CheckedList.Contains((int)pgUser.ID);
                    foreach (var diff in diffColl)
                    {
                        collDifferent |= diff;
                    }
                }
                if (collDifferent)
                {
                    Users.Clear();
                    foreach (int userId in co.CheckedList)
                    {
                        Users.Add(Source.Users.First(user => user.ID == userId));
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли выбрать пользователей
        /// </summary>
        public bool CanChooseUsers(object parameter = null)
        {
            return UseUsersFilter;
        }
        #endregion // ChooseUsersCommand

        #region LoadDatesCommand
        /// <summary>
        /// Команда для загрузки дат
        /// </summary>
        public ICommand LoadDatesCommand
        {
            get { return _loadDatesCommand ?? (_loadDatesCommand = new RelayCommand(this.LoadDates, this.CanLoadDates)); }
        }
        /// <summary>
        /// Загрузка дат
        /// </summary>
        public void LoadDates(object parameter = null)
        {
            if (_dateFrom > _dateTo)
            {
                MessageBox.Show("Дата начала больше даты конца");
            }
            else
            {
                bool loadHistory = false;
                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    sqlWork.sql = "SELECT count(*) FROM sys_scheme.table_history_info;";
                    loadHistory = sqlWork.ExecuteScalar<Int32>() > 0;
                }               

                if (loadHistory)
                {
                    cti.ThreadProgress.ShowWait();
                    // Строка с дополнительными условиями
                    String conditions = "TRUE";
                    conditions += String.Format(" AND hl.data_changes::date <= '{0}-{1}-{2}'", DateTo.Year, DateTo.Month, DateTo.Day);
                    conditions += String.Format(" AND hl.data_changes::date >= '{0}-{1}-{2}'", DateFrom.Year, DateFrom.Month, DateFrom.Day);
                    if (Users.Count > 0)
                    {
                        conditions += String.Format(" AND hl.user_name in ('{0}')", String.Join("', '", UserLogins.ToArray()));
                    }
                    if (Tables.Count > 0)
                    {
                        conditions += String.Format(" AND hl.id_table in ('{0}')", String.Join("', '", TableIds.ToArray()));
                    }
                    if (Objects.Count > 0)
                    {
                        conditions += String.Format(" AND hl.id_object in ({0})", String.Join(", ", Objects.ToArray()));
                    }

                    String sql =
                        String.Format(@"
                            SELECT 
                                DISTINCT data_changes::date 
                            FROM (  
                                SELECT 
                                    hl.data_changes::date, 
                                    hl.user_name, 
                                    hl.id_table, 
                                    hl.id_object
                                FROM sys_scheme.get_history_list_by_object() hl
                                INNER JOIN sys_scheme.table_right tr ON tr.id_table = hl.id_table AND tr.id_user = {0} AND tr.read_data = TRUE
                                WHERE {1} 
                                GROUP BY 
                                    hl.data_changes::date, 
                                    hl.user_name, 
                                    hl.id_table,
                                    hl.id_object) sub 
                            ORDER BY 
                                data_changes::date DESC
                            ",
                            Program.repository.CurrentUser.ID,
                            conditions);

                    using (SqlWork sqlWork = new SqlWork(_connect))
                    {
                        sqlWork.sql = sql;
                        if (sqlWork.ExecuteReader())
                        {
                            Dates.Clear();
                            while (sqlWork.CanRead())
                            {
                                Dates.Add(new PgHistM.PgHistoryDate(sqlWork.GetValue<DateTime>(0), this));
                            }
                        }
                        sqlWork.Close();
                    }
                    cti.ThreadProgress.Close();
                }
            }
        }
        /// <summary>
        /// Можно ли загрузить даты
        /// </summary>
        public bool CanLoadDates(object parameter = null)
        {
            return true;
        }
        #endregion // LoadDatesCommand

        #region ClearUsersAndTablesCommand
        /// <summary>
        /// Команда для очистки списков пользователей и таблиц
        /// </summary>
        public ICommand ClearUsersAndTablesCommand
        {
            get { return _clearUsersAndTablesCommand ?? (_clearUsersAndTablesCommand = new RelayCommand(this.ClearUsersAndTables, this.CanClearUsersAndTables)); }
        }
        /// <summary>
        /// Очистка списком пользователей и таблиц
        /// </summary>
        public void ClearUsersAndTables(object parameter = null)
        {
            if (UseUsersFilter)
            {
                Users.Clear();
            }
            if (UseTablesFilter)
            {
                Tables.Clear();
            }
        }
        /// <summary>
        /// Можно ли очистить списки
        /// </summary>
        public bool CanClearUsersAndTables(object parameter = null)
        {
            return ((Users.Count > 0 || Tables.Count > 0) && (UseTablesFilter || UseUsersFilter));
        }
        #endregion // ClearUsersAndTablesCommand

        #region ClearGeometryCommand
        /// <summary>
        /// Команда для очистки временного слоя для предпросмотра геометрии
        /// </summary>
        public ICommand ClearGeometryCommand
        {
            get { return _clearGeometryCommand ?? (_clearGeometryCommand = new RelayCommand(this.ClearGeometry, this.CanClearGeometry)); }
        }
        /// <summary>
        /// Очистка слоя предпросмотра
        /// </summary>
        /// <param name="parameter">True - удалить слой. False - очищает только объекты</param>
        public void ClearGeometry(object parameter = null)
        {
            if (parameter is Boolean && (bool)(parameter) == true)
            {
                String layerName = String.Format("HistoryView_{0}", _guid);
                mvLayer layer = _axMapLib.getLayer(layerName);
                if (layer != null)
                {
                    layer.deleteLayer();
                }
                _axMapLib.mapRepaint();
            }
            else
            {
                SwitchGeometry(PgHistM.PgHistoryAttribute.HistoryAttributeValueType.Before);
                SwitchGeometry(PgHistM.PgHistoryAttribute.HistoryAttributeValueType.After);
                SwitchGeometry(PgHistM.PgHistoryAttribute.HistoryAttributeValueType.Actual);
            }
        }
        /// <summary>
        /// Можно ли очистить слой
        /// </summary>
        public bool CanClearGeometry(object parameter = null)
        {
            return true;
        }
        #endregion // ClearGeometryCommand

        #region RestoreAfterValuesCommand
        private ICommand _restoreAfterValuesCommand;
        /// <summary>
        /// Команда для восстановления значений после
        /// </summary>
        public ICommand RestoreAfterValuesCommand
        {
            get { return _restoreAfterValuesCommand ?? (_restoreAfterValuesCommand = new RelayCommand(this.RestoreAfterValues, this.CanRestoreAfterValues)); }
        }
        /// <summary>
        /// Восстановление значений после
        /// </summary>
        public void RestoreAfterValues(object parameter = null)
        {
            PgHistM.PgHistoryEvent histEvent = parameter as PgHistM.PgHistoryEvent;
            List<String> tableFields =
               (from PgHistM.PgHistoryAttribute attr
                    in histEvent.Attributes
                where attr.RestoreValues
                select attr.Field.Name).ToList();
            try
            {
                RestoreFromHistory(histEvent.IdTable, histEvent.IdObject, histEvent.IdHistoryTable, histEvent.IdHistoryObject, tableFields);
                MessageBox.Show("Поля восстановлены");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Можно ли восстановить значения после
        /// </summary>
        public bool CanRestoreAfterValues(object parameter = null)
        {
            bool result = false; 
            PgHistM.PgHistoryEvent pgEvent = parameter as PgHistM.PgHistoryEvent;
            if (pgEvent != null)
            {
                foreach (PgHistM.PgHistoryAttribute attr in pgEvent.Attributes)
                {
                    if (attr.RestoreValues)
                    {
                        result = true; break;
                    }
                }
                result &= (pgEvent.TypeOperation != PgHistM.PgHistoryTypeOperation.Delete); 
            }
            return result;
        }
        #endregion // RestoreAfterValuesCommand

        #region RestoreBeforeValuesCommand
        private ICommand _restoreBeforeValuesCommand;
        /// <summary>
        /// Команда для восстановления значений до
        /// </summary>
        public ICommand RestoreBeforeValuesCommand
        {
            get { return _restoreBeforeValuesCommand ?? (_restoreBeforeValuesCommand = new RelayCommand(this.RestoreBeforeValues, this.CanRestoreBeforeValues)); }
        }
        /// <summary>
        /// Восстановление значений до
        /// </summary>
        public void RestoreBeforeValues(object parameter = null)
        {
            PgHistM.PgHistoryEvent histEvent = parameter as PgHistM.PgHistoryEvent;
            List<String> tableFields =
               (from PgHistM.PgHistoryAttribute attr
                    in histEvent.Attributes
                where attr.RestoreValues
                select attr.Field.Name).ToList(); 

            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql =
                    String.Format(@"SELECT glbo.id_history, glbo.id_history_table, thi.history_table_name
                                    FROM sys_scheme.get_history_list_by_object({0}, {1}) glbo
                                        INNER JOIN sys_scheme.table_history_info thi 
	                                    ON glbo.id_history_table = thi.id_history_table
                                    WHERE id_history < {2}
                                    ORDER BY glbo.id_history DESC LIMIT 1",
                                                   histEvent.IdTable,
                                                   histEvent.IdObject,
                                                   histEvent.IdHistoryObject);
                sqlWork.ExecuteReader();
                if (sqlWork.CanRead())
                {
                    int idHistoryObject = sqlWork.GetInt32("id_history");
                    int idHistoryTable = sqlWork.GetInt32("id_history_table");
                    try
                    {
                        RestoreFromHistory(histEvent.IdTable, histEvent.IdObject, idHistoryTable, idHistoryObject, tableFields);
                        MessageBox.Show("Поля восстановлены");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                sqlWork.Close();
            }
        }
        /// <summary>
        /// Можно ли восстановить значения до
        /// </summary>
        public bool CanRestoreBeforeValues(object parameter = null)
        {
            bool result = false;
            PgHistM.PgHistoryEvent pgEvent = parameter as PgHistM.PgHistoryEvent;
            if (pgEvent != null)
            {
                foreach (PgHistM.PgHistoryAttribute attr in pgEvent.Attributes)
                {
                    if (attr.RestoreValues)
                    {
                        result = true; break;
                    }
                }
                result &= (pgEvent.TypeOperation != PgHistM.PgHistoryTypeOperation.Insert);
            }
            return result;
        }
        #endregion // RestoreBeforeValuesCommand
        #endregion Команды

        #region Обработчики
        void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadDates();
        }
        void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadDates();
        }
        #endregion Обработчики
    }
}