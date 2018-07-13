using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using axVisUtils;
using Interfaces;
using NpgsqlTypes;
using OGRFramework;
using Rekod.Classes;
using Rekod.DBInfoEdit;
using Rekod.DBInfoEdit.ViewModel;
using Rekod.FastReportClasses;
using Rekod.Services;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.DataAccess.SourcePostgres.View.History;
using Rekod.SQLiteSettings;
using ExtraFunctions;
using Rekod.ProjectionSelection;
using DataTable = System.Data.DataTable;
using Interfaces.FastReport;

namespace Rekod
{
    public partial class UcTableObjects : UserControl
    {
        #region subclasses
        public class FieldInfoFull : Interfaces.fieldInfo
        {
            public string MaxWord = "";
            public override string ToString()
            {
                return base.nameMap;
            }
            public DataGridViewColumn Column;
            public int ColumnIndex = -1;
        }
        private void InThread(MethodInvoker mth)
        {
            try
            {
                if (IsDisposed)
                    return;
                if (InvokeRequired)
                    Invoke(mth);
                else
                    mth();
            }
            catch { }
        }
        struct DataValues
        {
            public List<object[]> data;
            public List<int> Types;
            public string Separator;
            public string FileName;
        }

        private class TableViewInfo
        {
            private TableViewInfo(SqlWork sqlResult)
            {
                ViewId = sqlResult.GetInt32(0);
                TableId = sqlResult.GetInt32(1);
                UserId = sqlResult.GetInt32(2);
                Title = sqlResult.GetString(3);
                IsDefault = sqlResult.GetBoolean(4);
            }

            public int ViewId { get; private set; }
            public int TableId { get; private set; }
            public int UserId { get; private set; }
            public string Title { get; private set; }
            public bool IsDefault { get; private set; }

            public static List<TableViewInfo> GetViews(tablesInfo table)
            {
                var list = new List<TableViewInfo>();
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format(@"SELECT id, table_id, user_id, title, is_default
                                                 FROM sys_scheme.table_view_info twi
                                                 WHERE table_id = {0} AND user_id = {1};",
                                                table.idTable,
                                                Program.id_user);

                    if (!sqlCmd.Execute(false))
                        throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);

                    while (sqlCmd.CanRead())
                    {
                        list.Add(new TableViewInfo(sqlCmd));
                    }
                }

                return list;
            }

            public static void SaveCurrentView(tablesInfo table, List<DataGridViewColumn> columns)
            {
                var frm = new FilterSaveFrm();
                frm.Text = "Вид";
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    using (var sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = String.Format(@"INSERT INTO sys_scheme.table_view_info(
                                                    table_id, user_id, title)
                                                    VALUES ({0}, {1}, '{2}');",
                                                    table.idTable,
                                                    Program.id_user,
                                                    frm.textBox1.Text);

                        if (!sqlCmd.Execute(false))
                            throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);
                    }

                    int viewId = 0;
                    using (var sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = String.Format(@"SELECT id FROM sys_scheme.table_view_info ORDER BY id DESC LIMIT 1");

                        if (!sqlCmd.Execute(false))
                            throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);

                        while (sqlCmd.CanRead())
                        {
                            viewId = sqlCmd.GetInt32(0);
                        }
                    }

                    foreach (var dataGridViewColumn in columns)
                    {
                        using (var sqlCmd = new SqlWork())
                        {
                            int workFieldId = 0;
                            using (var sqlCmdInt = new SqlWork())
                            {

                                sqlCmdInt.sql = String.Format(@"SELECT id FROM sys_scheme.table_field_info tfi WHERE tfi.name_db = '{0}' AND tfi.id_table = {1} LIMIT 1;",
                                                                dataGridViewColumn.Name,
                                                                table.idTable
                                                                );

                                if (!sqlCmdInt.Execute(false))
                                    throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);

                                while (sqlCmdInt.CanRead())
                                {
                                    workFieldId = sqlCmdInt.GetInt32(0);
                                }
                            }

                            sqlCmd.sql = String.Format(@"INSERT INTO sys_scheme.table_view_field_info(
                                                        view_id, field_id, order_number, is_visible)
                                                        VALUES ({0}, {1}, {2}, {3});",
                                                            viewId,
                                                            workFieldId,
                                                            dataGridViewColumn.DisplayIndex,
                                                            dataGridViewColumn.Visible
                                                            );

                            if (!sqlCmd.Execute(false))
                                throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);
                        }
                    }
                }
            }

            public override string ToString()
            {
                return String.Format("{0}, ViewId: {1}, TableId: {2}", Title, ViewId, TableId);
            }

            public static void DeleteView(int viewId)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format(@"DELETE FROM sys_scheme.table_view_info
                                                 WHERE id = {0}",
                                                viewId);

                    if (!sqlCmd.Execute(false))
                        throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);
                }

                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format(@"DELETE FROM sys_scheme.table_view_field_info
                                                 WHERE view_id = {0};",
                                                viewId);

                    if (!sqlCmd.Execute(false))
                        throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);
                }
            }
        }

        private class TableViewFieldInfo
        {
            private TableViewFieldInfo(SqlWork sqlResult)
            {
                Id = sqlResult.GetInt32(0);
                ViewId = sqlResult.GetInt32(1);
                FieldId = sqlResult.GetInt32(2);
                OrderNumber = sqlResult.GetInt32(3);
                IsVisible = sqlResult.GetBoolean(4);
                FieldName = sqlResult.GetString(5);
                FieldDBName = sqlResult.GetString(6);

            }

            public int Id { get; private set; }
            public int ViewId { get; private set; }
            public int FieldId { get; private set; }
            public int OrderNumber { get; set; }
            public bool IsVisible { get; private set; }
            public string FieldName { get; private set; }
            public string FieldDBName { get; private set; }

            public static List<TableViewFieldInfo> GetViewFields(TableViewInfo view)
            {
                var list = new List<TableViewFieldInfo>();
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format(@"SELECT id, view_id, field_id, order_number, is_visible, 
                                                    (SELECT name_map FROM sys_scheme.table_field_info WHERE id = VFI.field_id LIMIT 1) as field_name,
                                                    (SELECT name_db FROM sys_scheme.table_field_info WHERE id = VFI.field_id LIMIT 1) as field_db_name
                                                 FROM sys_scheme.table_view_field_info VFI
                                                 WHERE view_id = {0};",
                                                view.ViewId);

                    if (!sqlCmd.Execute(false))
                        throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);

                    while (sqlCmd.CanRead())
                    {
                        var viewFieldInfo = new TableViewFieldInfo(sqlCmd);

                        //if (viewFieldInfo.FieldName == geomFieldName)
                        //{
                        //    continue;
                        //}

                        list.Add(viewFieldInfo);
                    }
                }

                return list;
            }

            public override string ToString()
            {
                return String.Format("{0}, Order: {1}, IsVisible: {2}", FieldName, OrderNumber, IsVisible);
            }
        }
        #endregion

        #region globals
        bool @break;
        tablesInfo _tableInfo;
        private int MaxRowsInList = 500;
        private int CurrentNumList = 0;
        private int maxNumList = 0;
        private bool isMapMoveTo = false;
        private bool isUserMapFiter = false;
        private int _rowCount;
        Thread _loadThread;
        Thread _lastThread;
        bool _isWork = false;
        private string sqll = null;
        private int _idObj = -1;
        private List<int> _idObjects = new List<int>();
        string importFilesDialogFilter, exportFilesDialogFilter;
        List<FieldInfoFull> _listField = new List<FieldInfoFull>();
        private readonly List<FieldInfoFull> _listIs = new List<FieldInfoFull>();
        private FieldKeyValue _searchValue;
        internal SynchronizationContext uiContext;
        private int _lastScrollPositionColumnIndex = 0;
        private TableViewInfo defaultViewInfo;
        private int _pkFieldValue;
        #endregion

        #region Свойства
        public enTypeReport Type
        { get { return enTypeReport.Table; } }
        public int IdTable
        { get { return _tableInfo.idTable; } }
        public int IdObj
        { get { return _idObj; } }
        public string Where2
        {
            get
            {
                List<IParams> listParams = null;
                List<FindRequest2> ftemp = GetFilters();
                string where = GetWhere(ftemp, out listParams);
                return GetSqlWhereStringForMap(where, listParams);
            }
        }
        #endregion

        #region initialization
        public UcTableObjects(FieldKeyValue searchValue, int pkFieldValue)
        {
            _tableInfo = classesOfMetods.getTableInfo(searchValue.Key.idTable);
            _searchValue = searchValue;
            this._pkFieldValue = pkFieldValue;
            
            SetStart();

            this.Load += OnLoad;
        }

        public UcTableObjects(tablesInfo tablesInfo, int? goToObject = null, bool isSelected = false)
        {
            _tableInfo = tablesInfo;
            SetStart(goToObject, isSelected: isSelected);

            this.Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            InitMaxRowsInList();
            ActivateDefaultView();
            this.Load -= OnLoad;
        }

        private void SetStart(int? goToObject = null, string selectField = null, bool isSelected = false)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            LoadFilters();
            uiContext = SynchronizationContext.Current;


            Program.ReportModel.ListReports.CollectionChanged += (s, arg) =>
            {
                MenuReloadReports();
            };
            MenuReloadReports();

            if (!Program.user_info.admin)
            {
                toolStripSeparator1.Visible = false;
                открытьМенеджерОтчетовToolStripMenuItem.Visible = false;
            }
            //Составление поддерживаемых форматов для загрузки/выгрузки
            //            string fromShp = _tableInfo.type == 1 ?
            //                @"ESRI Shape-файлы|*.shp|
            //MapInfo-файлы|*.tab|
            //Atlas BNA|*.bna|
            //CSV|*.csv|
            //AutoCAD DXF|*.dxf|
            //INTERLIS 1|*.itf|
            //GeoJSON|*.geojson|
            //Geographic Markup Language (GML)|*.gml|
            //Genering Mapping Tools (GMT)|*.gmt|
            //GeoConcept Text|*.gxt|
            //Keyhole Markup Language (KML)|*.kml|
            //Mapinfo  Interchange  Format  (MIF)|*.mif|
            //SQLite|*.sqlite|
            //Geographically Encoded Objects for RSS feeds (GeoRSS)|*.xml|" : "";
            //            string fromExcel = _tableInfo.type == 4 ? "Файл Excel|*.xls;*.xlsx" : "";
            //            string fromDBase = _tableInfo.type == 4 ? "Файл dBase|*.dbf" : "";
            //            importFilesDialogFilter = fromDBase + "|" + fromExcel + "|" + fromShp;
            //            importFilesDialogFilter = importFilesDialogFilter.Replace("||", "|");
            //            importFilesDialogFilter = importFilesDialogFilter.Trim('|');
            //            exportFilesDialogFilter = fromShp + "|TXT файл|*.txt";
            //            exportFilesDialogFilter = exportFilesDialogFilter.Replace("||", "|");
            //            exportFilesDialogFilter = exportFilesDialogFilter.Trim('|');

            string allFormats = "", fromShp = "";
            if (_tableInfo.type == 1)
            {
                allFormats = Rekod.Properties.Resources.LocAllSupportedFormats + "|*.shp;*.tab;*.mif;*.xls;*.xlsx;*.geojson;*.sqlite;*.dbf";
                fromShp = @"ESRI Shape-files|*.shp|
MapInfo-files|*.tab|
MapInfo MIF/MID|*.mif|
MS Excel|*.xls;*.xlsx|
GeoJSON|*.geojson|
SQLite|*.sqlite|";
            }
            else
            {
                allFormats = Rekod.Properties.Resources.LocAllSupportedFormats + "|*.xls;*.xlsx;*.dbf";
            }
            string tmp_filter = fromShp + ((_tableInfo.type != 1) ? "|MS Excel|*.xls;*.xlsx" : "") + "|" + "File dBase|*.dbf";
            tmp_filter = tmp_filter.Replace("||", "|");
            tmp_filter = tmp_filter.Trim('|');
            importFilesDialogFilter = allFormats + "|" + tmp_filter;
            exportFilesDialogFilter = tmp_filter + "|TXT file|*.txt";

            //Визуализация кнопок
            flpEditorTable.Visible = !classesOfMetods.getTableInfo(_tableInfo.idTable).read_only;
            flpSelecter.Visible = isSelected;
            pButtons.Visible = !classesOfMetods.getTableInfo(_tableInfo.idTable).read_only || isSelected;
            tsHistory.Visible = _tableInfo.isHistory;

            tsExport.Visible = Program.UserActionRight.FirstOrDefault(f => f.Action.Name == "Export").Allowed;
            importFromFile.Visible = Program.UserActionRight.FirstOrDefault(f => f.Action.Name == "Import").Allowed;

            cbMapMoveTo.Visible = (_tableInfo.type == 1);
            применитьНаКартеToolStripMenuItem.Visible = (_tableInfo.type == 1);

            this.dataGridView1.CellMouseDoubleClick += isSelected ?
                new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick_isSelected) :
                new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick_isEditor);

            sqll = GetSqlTableNew(_tableInfo);
            if (goToObject != null && goToObject != -1)
            {
                int currentRow = 0;


                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = string.Format(@"
SELECT row_num
FROM (SELECT {2} FROM {0}.{1} ) As tables
	CROSS JOIN (	SELECT ARRAY(SELECT {2} 
			FROM {0}.{1}  
			ORDER BY {2}) As array_id)  AS table_id
	CROSS JOIN generate_series(1, (	SELECT COUNT(*) 
					FROM {0}.{1} )) AS row_num
WHERE table_id.array_id[row_num] =  tables.{2} AND tables.{2} = :idd
ORDER BY row_num;",
                            _tableInfo.nameSheme, _tableInfo.nameDB, _tableInfo.pkField);
                    sqlCmd.ExecuteReader(new IParams[]
									   {
										   new Params()
											   {
												   _paramName = "idd",
												   typeData = NpgsqlDbType.Integer,
												   value = (int) goToObject
											   }
									   });
                    if (sqlCmd.CanRead())
                    {
                        int? obj = sqlCmd.GetValue<int?>(0);
                        if (obj != null)
                            currentRow = (int)obj - 1;
                        CurrentNumList = (currentRow - currentRow % MaxRowsInList) / MaxRowsInList;
                        _idObj = (int)goToObject;
                    }
                }
                //var field = _listField.Find(f => f.nameDB == _tableInfo.pkField);
                //AddFilter(new FindBox2(field, (int)Selected));
            }
            AddFilter(new FindBox2());
            Plugins.ListAddMenuInTable.ForEach(f =>
                                                   {
                                                       try
                                                       {
                                                           if (f.IdTable == _tableInfo.idTable)
                                                               menuStrip2.Items.Add(f.ToolStripMenuItem.Invoke());
                                                       }
                                                       catch
                                                       {
                                                       }
                                                   });

            удалитьВсеЗаписиToolStripMenuItem.Visible = false;

            if (!classesOfMetods.getWriteTable(_tableInfo.idTable))
            {
                btnAdd.Enabled = false;
                btnDel.Enabled = false;
                BtnEdit.Enabled = false;
                importFromFile.Enabled = false;
                создатьКопиюВыделеннойСтрокиToolStripMenuItem1.Enabled = false;
            }
            else if (Program.user_info != null && Program.user_info.admin)
            {
                удалитьВсеЗаписиToolStripMenuItem.Visible = true;
            }

            UpdateViewHeaderMenu();
        }


        #endregion

        private void LoadData()
        {
            ErrorMess();
            Debug.WriteLine("Запрос на обнавление данных");
            _lastThread = new Thread(delegate()
            {
                Thread CurrentThread = Thread.CurrentThread;
                @break = false;
                Debug.WriteLine(1);
                try
                {
                    Debug.WriteLine(string.Format("Старт потока: {0}\t\t{1}, {2}",
                        CurrentThread.Name, _lastThread, _loadThread));
                    SaveScrollPosition();
                    GetSqlTableMaxMinID(sqll, _tableInfo);
                    Debug.WriteLine(2);
                    if (@break == true)
                        Debug.WriteLine(string.Format("Поток {0}: прерван", CurrentThread.Name));
                }
                catch (Exception ex) { Debug.WriteLine(string.Format("Поток {0}, ошибка {1}", CurrentThread.Name, ex.Message)); }
                finally
                {
                    Debug.WriteLine(3);
                    InThread(() =>
                    {
                        Debug.WriteLine(41);
                        _isWork = false;
                        if (_lastThread != null)
                        {
                            Debug.WriteLine(4);
                            _isWork = true;

                            _loadThread = _lastThread;
                            _lastThread = null;
                            _loadThread.Priority = ThreadPriority.Lowest;
#if DEBIG
							_loadThread.Name = string.Format("start {0} от {1}", _loadThread.ManagedThreadId, CurrentThread.Name);
							Debug.WriteLine(string.Format("Запуск потока: {0}", _loadThread.Name));
#endif
                            _loadThread.Start();
                            Debug.WriteLine(5);
                        }
                        else
                        {
                            Debug.WriteLine(6);
                            _loadThread = null;
                        }
                    });
                    Debug.WriteLine(string.Format("Конец потока: {0}\t\t{1}, {2}, {3}",
                        CurrentThread.Name, _lastThread, _loadThread, @break));
                    LoadScrollPosition();
                    CurrentThread.Abort();
                }
            });
            if (_isWork == true)
            {
                @break = true;
                //if (_loadThread != null && _loadThread.ThreadState != ThreadState.AbortRequested)
                //{
                //    //Debug.WriteLine(string.Format("Прерывание потока: {0}", _loadThread.ManagedThreadId));

                //}
                //return;
            }
            if (_loadThread == null)
            {
                _isWork = true;
                _lastThread.Priority = ThreadPriority.Lowest;
#if DEBUG
                Debug.WriteLine(string.Format("Запуск потока: {0} в LoadData()", _lastThread.ManagedThreadId));
                _lastThread.Name = "start 1";
#endif
                _lastThread.Start();
                _loadThread = _lastThread;
                _lastThread = null;
            }
            ApplyUserMapFilter(true);
        }

        private void LoadScrollPosition()
        {


            int scrollTo = 0;
            //GetViews();

            //Program.id_user
            //classesOfMetods.getFieldInfoTable

            for (int i = _lastScrollPositionColumnIndex - 1; i >= 0; i--)
            {
                try
                {
                    if (dataGridView1.Columns[i].Visible)
                    {
                        scrollTo += dataGridView1.Columns[i].Width;
                    }
                }
                catch (Exception)
                { }
            }

            InThread(() =>
            {
                dataGridView1.HorizontalScrollingOffset = scrollTo;
            });
        }

        private void SaveScrollPosition()
        {
            var currentOffset = dataGridView1.HorizontalScrollingOffset;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (currentOffset >= column.Width)
                {
                    if (column.Visible)
                    {
                        currentOffset -= column.Width;
                    }

                }
                else
                {
                    _lastScrollPositionColumnIndex = column.Index;
                    break;
                }
            }
        }

        public void AddFilter(FindBox2 filterBox)
        {
            filterBox.ListIS = _listIs;
            filterBox.Top = 0;
            InThread(() => pFilter.Controls.Add(filterBox));
        }
        public void AddFilterInPanel(FindBox2 filterBox)
        {
            foreach (FindBox2 item in pFilter.Controls)
            {
                item.RemoveFunc = true;
                item.VisibolFindButton = false;
            }
            filterBox.RemoveFunc = false;
            filterBox.VisibolFindButton = true;
            InThread(() => pFilter.Controls.Add(filterBox));
        }
        public void RemoveFilter(FindBox2 filterBox)
        {
            filterBox.Top = 0;
            InThread(() => pFilter.Controls.Remove(filterBox));
        }
        public void RemoveFilterAll()
        {
            InThread(() => pFilter.Controls.Clear());
        }
        private string GetSqlTableNew(tablesInfo table)
        {
            if (!string.IsNullOrEmpty(sqll))
                return sqll;
            var tmpListField = new List<FieldInfoFull>();
            var tmp2ListField = new List<FieldInfoFull>();
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = @"
SELECT id, name_db, name_map, type_field, is_reference, is_interval, ref_table, ref_field, ref_field_end, ref_field_name, name_lable, 
 (SELECT ttt.type_field FROM sys_scheme.table_field_info ttt WHERE ttt.id = ti.ref_field_name) as ref_type
FROM sys_scheme.table_field_info ti
WHERE id_table = " + table.idTable + " AND visible=TRUE ORDER BY num_order;";

                if (!sqlCmd.Execute(false))
                    throw new Exception(Rekod.Properties.Resources.ucTable_ErrorStructTable);

                while (sqlCmd.CanRead())
                {
                    var field = new FieldInfoFull
                    {
                        idField = sqlCmd.GetInt32(0),
                        nameDB = sqlCmd.GetString(1),
                        nameMap = sqlCmd.GetString(2),
                        TypeField = (TypeField)sqlCmd.GetInt32(3),
                        is_reference = sqlCmd.GetBoolean(4),
                        is_interval = sqlCmd.GetBoolean(5),
                        MaxWord = sqlCmd.GetString(2),
                        name_lable = sqlCmd.GetString(10)
                    };

                    if (field.is_reference || field.is_interval)
                    {
                        field.ref_table = sqlCmd.GetInt32(6);
                        field.ref_field = sqlCmd.GetInt32(7);
                        field.ref_field_end = sqlCmd.GetValue<int?>(8);
                        field.ref_field_name = sqlCmd.GetInt32(9);
                    }
                    tmpListField.Add(field);

                    var field2 = new FieldInfoFull
                    {
                        idField = sqlCmd.GetInt32(0),
                        nameDB = sqlCmd.GetString(1),
                        nameMap = sqlCmd.GetString(2),
                        TypeField = (TypeField)sqlCmd.GetInt32(3),
                        is_reference = sqlCmd.GetBoolean(4),
                        is_interval = sqlCmd.GetBoolean(5),
                        MaxWord = sqlCmd.GetString(2),
                        name_lable = sqlCmd.GetString(10)
                    };


                    if (field2.is_reference || field2.is_interval)
                    {
                        field2.ref_table = sqlCmd.GetInt32(6);
                        field2.ref_field = sqlCmd.GetInt32(7);
                        field2.ref_field_end = sqlCmd.GetValue<int?>(8);
                        field2.ref_field_name = sqlCmd.GetInt32(9);
                    }

                    if (field2.is_reference)
                    {
                        field2.TypeField = (TypeField)sqlCmd.GetInt32(11);
                    }
                    tmp2ListField.Add(field2);
                }
            }
            //колонки
            _listIs.Clear();
            _listIs.Add(new FieldInfoFull() { nameMap = Rekod.Properties.Resources.ucTable_inAll, idField = -1 });
            _listIs.AddRange(collection: tmp2ListField.Where(f => f.TypeField != TypeField.Geometry));


            byte countBreak = 0;
            for (int i = 0; i < tmpListField.Count(); i++)
            {
                if (tmpListField[i].TypeField == TypeField.Geometry) { countBreak++; continue; }
                var field = _listField.FirstOrDefault(f => f.idField == tmpListField[i].idField);
                if (field == null)
                {
                    var columField = new System.Windows.Forms.DataGridViewTextBoxColumn
                    {
                        DisplayIndex = i - countBreak,
                        Name = tmpListField[i].nameDB,
                        HeaderText = tmpListField[i].nameMap,
                        Width = 100,
                        ReadOnly = true
                    };
                    switch (tmpListField[i].TypeField)
                    {
                        case TypeField.DateTime:
                            columField.ValueType = typeof(DateTime);
                            columField.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            break;
                        case TypeField.Date:
                            columField.ValueType = typeof(DateTime);
                            break;
                        case TypeField.Text:
                            columField.ValueType = typeof(string);
                            break;
                        case TypeField.Integer:
                            columField.ValueType = typeof(int);
                            break;
                    }

                    InThread(() => tmpListField[i].ColumnIndex = this.dataGridView1.Columns.Add(columField));
                    tmpListField[i].Column = columField;
                    columField.Tag = tmpListField[i];
                }
                else
                {

                    tmpListField[i].Column = field.Column;
                    tmpListField[i].ColumnIndex = field.ColumnIndex;
                    InThread(() => tmpListField[i].Column.DisplayIndex = i - countBreak);
                    _listField.Remove(field);
                }
            }
            for (int i = 0; i < _listField.Count(); i++)
            {
                if (_listField[i].TypeField != TypeField.Geometry)
                    InThread(() => this.dataGridView1.Columns.Remove(_listField[i].Column));
            }
            _listField = tmpListField;
            InThread(() =>
            {
                tsКолони.Text = dataGridView1.Columns.Count.ToString();
            });

            //Генерация SQL запроса
            using (SqlWork sqlCmd = new SqlWork())
            {
                //if (_searchValue != null)
                //    sqlCmd.sql = string.Format("SELECT {0}.get_sql_for_table_new({1});",
                //        Program.scheme, table.idTable);
                //else
                if (this._searchValue != null)
                {
                    tablesInfo ti_ref = classesOfMetods.getTableInfo(this._searchValue.Key.ref_table.Value);
                    if (ti_ref != null && (ti_ref.type == 4 || ti_ref.type == 1))
                    {
                        fieldInfo tfi = classesOfMetods.getFieldInfo(this._searchValue.Key.ref_field.Value);
                        if (tfi.nameDB != ti_ref.pkField)
                        {
                            sqlCmd.sql = string.Format("SELECT {0}.get_sql_for_table({1}, {2}, {3});",
                                    Program.scheme, table.idTable, ti_ref.idTable, this._pkFieldValue);
                        }
                        else
                        {
                            sqlCmd.sql = string.Format("SELECT {0}.get_sql_for_table({1});",
                                Program.scheme, table.idTable);
                        }
                    }
                    else
                    {
                        sqlCmd.sql = string.Format("SELECT {0}.get_sql_for_table({1});",
                            Program.scheme, table.idTable);
                    }
                }
                else
                {
                    sqlCmd.sql = string.Format("SELECT {0}.get_sql_for_table({1});",
    Program.scheme, table.idTable);
                }

                sqlCmd.Execute(false);

                while (sqlCmd.CanRead())
                {
                    sqll = sqlCmd.GetString(0);
                }
            }
            return sqll;
        }
        private string GetSqlWhereStringForMap(string _where, List<IParams> prms)
        {
            foreach (var item in prms)
            {
                string val = "";
                if (item.value != null)
                {
                    switch (item.type)
                    {
                        case DbType.Boolean:
                            val = BooleanParamToStr(item.value);
                            break;
                        case DbType.Date:
                            val = DateTimeParamToStr(item.value);
                            break;
                        case DbType.DateTime:
                            val = DateTimeParamToStr(item.value);
                            break;
                        case DbType.DateTime2:
                            val = DateTimeParamToStr(item.value);
                            break;
                        case DbType.DateTimeOffset:
                            val = DateTimeParamToStr(item.value);
                            break;
                        case DbType.Decimal:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.Double:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.Int16:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.Int32:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.Int64:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.Single:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.String:
                            val = TextParamToStr(item.value);
                            break;
                        case DbType.Time:
                            val = DateTimeParamToStr(item.value);
                            break;
                        case DbType.UInt16:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.UInt32:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.UInt64:
                            val = NumericParamToStr(item.value);
                            break;
                        case DbType.VarNumeric:
                            val = NumericParamToStr(item.value);
                            break;
                        default:
                            val = TextParamToStr(item.value);
                            break;
                    }
                }
                _where = _where.Replace(item.paramName, val);
            }
            return _where;
        }

        private string DateTimeParamToStr(object p)
        {
            return "'" + Convert.ToDateTime(p).ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }

        private string TextParamToStr(object p)
        {
            return "'" + p.ToString() + "'";
        }

        private string BooleanParamToStr(object p)
        {
            return p.ToString();
        }

        private string NumericParamToStr(object p)
        {
            return p.ToString();
        }
        private void GetSqlTableMaxMinID(string sql, tablesInfo table)
        {
            var b = new Bitmap(1, 1);
            var g = Graphics.FromImage((Image)b);

            DataGridViewRow SelectRow = null;
            try
            {
                InThread(() =>
                {
                    this.dataGridView1.SelectionChanged -= new System.EventHandler(this.dataGridView1_SelectionChanged);
                    this.dataGridView1.SelectionChanged -= new System.EventHandler(this.dataGridView1_SelectionChanged_map);
                    dataGridView1.Rows.Clear();
                    toolStripProgressBar1.Visible = true;
                });
                //строчки
                string _pkField = "ss." + table.pkField;
                var dgRows = new List<DataGridViewRow>();
                List<IParams> listParams = null;
                string where = "";

                var filters = GetFilters();
                InThread(() => where = GetWhere(filters, out listParams));
                string select = GetSelect(_listField);
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = string.Format("SELECT COUNT({0}) \nFROM ({1}) as ss \nWHERE {2}",
                                               _pkField, sql, where);
                    sqlCmd.ExecuteReader(listParams);
                    if (sqlCmd.CanRead())
                        _rowCount = sqlCmd.GetInt32("COUNT");
                    else
                        return;
                }

                string sqlLimit = "";
                string orderField;
                InThread(() =>
                {
                    if (@break == true)
                        return;
                    tsОбъектовВсего.Text = _rowCount.ToString();

                    PListUpdate(_rowCount, cbViewAll.Checked);
                    if (cbViewAll.Checked == false)
                    {
                        int offset;
                        if (CurrentNumList >= 1)
                            offset = CurrentNumList * MaxRowsInList;
                        else
                            offset = 0;
                        sqlLimit = string.Format("LIMIT {0} OFFSET {1}", MaxRowsInList, offset);
                    }
                });
                orderField = GetOrderBy();
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql =
                        string.Format(
                            @"
								SELECT {4} 
								FROM({0})as gg 
								WHERE {1} 
								ORDER BY gg.{2}
								{3}",
                            sql,
                            where,
                            orderField,
                            sqlLimit, select);

                    var ok = sqlCmd.ExecuteReader(listParams);
                    if (!ok)
                        return;
                    while (sqlCmd.CanRead())
                    {
                        var dgvRow = new DataGridViewRow();
                        int currID = -1;
                        foreach (var t in _listField)
                        {

                            if (t.TypeField == TypeField.Geometry)
                                continue;
                            var cell = new DataGridViewTextBoxCell();
                            try
                            {
                                var obj = sqlCmd.GetValue(t.nameDB);
                                cell.Value = obj;

                                if (cell.Value == null)
                                    obj = "";
                                else
                                    obj = obj.ToString().Replace("\n", "").Replace("\t", "").Replace("\r", "");
                                if (t.MaxWord.Length < obj.ToString().Length)
                                    t.MaxWord = obj.ToString();
                                if (t.nameDB == _tableInfo.pkField)
                                    currID = Convert.ToInt32(cell.Value);

                                if (IsMatchFilter(t, cell, filters))
                                {
                                    cell.Style.BackColor = Color.Yellow;
                                }
                            }
                            catch
                            {
                                continue;
                            }
                            dgvRow.Cells.Add(cell);

                        }
                        if (currID == _idObj)
                            SelectRow = dgvRow;
                        dgRows.Add(dgvRow);
                    }

                }
                InThread(() =>
                {
                    if (@break == true)
                        return;
                    tsОбъектов.Text = dgRows.Count.ToString();
                    if (dgRows.Count() == 0 || !classesOfMetods.getWriteTable(_tableInfo.idTable))
                    {
                        btnDel.Enabled = false;
                        BtnEdit.Enabled = false;
                        btnOK.Enabled = false;
                        создатьКопиюВыделеннойСтрокиToolStripMenuItem1.Enabled = false;
                    }
                    else
                    {
                        btnDel.Enabled = true;
                        BtnEdit.Enabled = true;
                        btnOK.Enabled = true;
                        создатьКопиюВыделеннойСтрокиToolStripMenuItem1.Enabled = true;
                    }
                });



                foreach (var t in _listField)
                {
                    if (t.TypeField == TypeField.Geometry)
                        continue;
                    var sizef = g.MeasureString(t.MaxWord, dataGridView1.Font);

                    var t1 = t;
                    InThread(() =>
                    {
                        var dataGridViewColumn = dataGridView1.Columns[t1.nameDB];
                        if (dataGridViewColumn != null)
                            dataGridViewColumn.Width = (sizef.Width + 10 < 500)
                                                        ? (int)sizef.Width + 10
                                                        : 500;
                    });
                }
                g.Dispose();
                InThread(() =>
                {
                    this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
                    dataGridView1.Rows.AddRange(dgRows.ToArray());
                    if (SelectRow != null)
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.FirstDisplayedScrollingRowIndex = SelectRow.Index;
                        dataGridView1.Refresh();
                    }
                });
                //uiContext.Send(d =>
                //{
                //    //string layerWhere = GetSqlWhereStringForMap(where, listParams);
                //    //SetFilterInMap(layerWhere, table.idTable);
                //}, null);
            }
            catch (Exception ex)
            {
                InThread(() =>
                   {
                       ErrorMess(ex.Message);
                   });
            }
            finally
            {
                if (@break != true)
                {
                    InThread(() =>
                    {

                        if (SelectRow != null)
                        {
                            dataGridView1.CurrentCell = SelectRow.Cells[0];
                            SelectRow.Selected = true;
                        }
                        this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged_map);
                        toolStripProgressBar1.Visible = false;

                    });
                }
            }
        }

        private string GetOrderBy()
        {
            string orderField;
            FieldInfoFull sortFild = null;
            var sortOrder = SortOrder.Ascending;
            InThread(() =>
            {
                if (dataGridView1.SortedColumn != null)
                {
                    sortFild = dataGridView1.SortedColumn.Tag as FieldInfoFull;
                    sortOrder = dataGridView1.SortOrder;
                }
            });

            if (sortFild == null)
                orderField = _tableInfo.pkField;
            else if (sortFild.is_interval)
                orderField = "id!" + sortFild.nameDB;
            else
                orderField = sortFild.nameDB;

            string order = "";
            if (sortFild == null && Program.UserParams.ContainsKey("DescOrderTable"))
            {
                string str_idtable = Program.UserParams["DescOrderTable"].FirstOrDefault(w => w == IdTable.ToString());
                if (!string.IsNullOrEmpty(str_idtable))
                {
                    order = "DESC";
                }
                else
                {
                    order = (sortOrder == SortOrder.Ascending) ? "ASC" : "desc";
                }
            }
            else
            {
                order = (sortOrder == SortOrder.Ascending) ? "ASC" : "desc";
            }

            orderField = string.Format("\"{0}\" {1}", orderField, order);
            return orderField;
        }


        private void InitMaxRowsInList()
        {
            //MessageBox.Show("init MaxRowsInList = " + MaxRowsInList, "ИНФО");
            if (Program.UserParams.ContainsKey("MaxRowsInList"))
            {
                int mxRowsNumber = MaxRowsInList;

                string maxRows = Program.UserParams["MaxRowsInList"].DefaultIfEmpty(mxRowsNumber.ToString()).First();
                //MessageBox.Show("before try maxRows = " + maxRows, "ИНФО");
                try
                {

                    //MessageBox.Show("in try MaxRowsInList = " + MaxRowsInList, "ИНФО");
                    MaxRowsInList = !string.IsNullOrEmpty(maxRows) ? int.Parse(maxRows) : mxRowsNumber;
                }
                catch
                {
                }
            }
            
        }

        private object SubStringInterval(FieldInfoFull fi, object value)
        {
            if (fi.is_interval)
            {
                if (value != null && value.ToString().Contains('('))
                {
                    value = value.ToString().Substring(value.ToString().LastIndexOf('(') + 1);
                    value = value.ToString().Remove(value.ToString().LastIndexOf(')'));
                }
                value = ExtraFunctions.Converts.To<double?>(value);
            }
            return value;
        }
        private bool IsMatchFilter(FieldInfoFull fi, DataGridViewCell cell, List<FindRequest2> filters)
        {
            var value = cell.Value;

            foreach (var filter in filters)
            {
                try
                {
                    if (fi.idField == filter.Col.idField || filter.Col.idField == -1)
                    {

                        if (filter.TypeFr == Rekod.Properties.Resources.FindBox2_Empty)
                        {
                            if (value == null)
                                return true;
                        }
                        else if (filter.TypeFr == Rekod.Properties.Resources.FindBox2_IsNotEmpty)
                        {
                            if (value != null)
                                return true;
                        }
                        else if (filter.FindValue is string && string.IsNullOrWhiteSpace(((string)filter.FindValue)))
                        {
                            return false;
                        }
                        else if (filter.TypeFr == Rekod.Properties.Resources.FindBox2_Contains)
                        {
                            if (value != null && value.ToString().ToLower().Contains(filter.FindValue.ToString().ToLower()))
                                return true;
                        }
                        else if (filter.TypeFr == Rekod.Properties.Resources.FindBox2_StartsWith)
                        {
                            if (value != null && value.ToString().ToLower().StartsWith(filter.FindValue.ToString().ToLower()))
                                return true;
                        }
                        else if (filter.TypeFr == Rekod.Properties.Resources.FindBox2_DoesNotContain)
                        {
                            if (!(value != null && value.ToString().ToLower().Contains(filter.FindValue.ToString().ToLower())))
                                return true;
                        }
                        else if (filter.TypeFr == ">")
                        {
                            value = SubStringInterval(fi, value);
                            if (value != null
                                 && ((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) > Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) > Converts.To<DateTime>(filter.FindValue)))
                                return true;
                        }
                        else if (filter.TypeFr == "<")
                        {
                            value = SubStringInterval(fi, value);
                            if (((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) < Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) < Converts.To<DateTime>(filter.FindValue)))
                                return true;
                        }
                        else if (filter.TypeFr == "=")
                        {
                            value = SubStringInterval(fi, value);
                            if (((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) == Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) == Converts.To<DateTime>(filter.FindValue))
                                || (fi.TypeField == TypeField.Text && value.ToString().ToLower() == filter.FindValue.ToString().ToLower()))
                                return true;
                        }
                        else if (filter.TypeFr == ">=")
                        {
                            value = SubStringInterval(fi, value);
                            if (((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) >= Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) >= Converts.To<DateTime>(filter.FindValue)))
                                return true;
                        }
                        else if (filter.TypeFr == "<=")
                        {
                            value = SubStringInterval(fi, value);
                            if (((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) <= Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) <= Converts.To<DateTime>(filter.FindValue)))
                                return true;
                        }
                        else if (filter.TypeFr == "<>")
                        {
                            value = SubStringInterval(fi, value);
                            if (value != null
                                 && ((fi.TypeField == TypeField.Numeric || fi.TypeField == TypeField.Integer) && Converts.To<double>(value) != Converts.To<double>(filter.FindValue)
                                  || (fi.TypeField == TypeField.Date || fi.TypeField == TypeField.DateTime) && Converts.To<DateTime>(value) != Converts.To<DateTime>(filter.FindValue)))
                                return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }
        private void SetFilterInMap(string _where, int id_table)
        {
            mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(id_table));
            if (layer == null && !string.IsNullOrWhiteSpace(_where))
            {
                Program.mainFrm1.layerItemsView1.SetLayerVisible(id_table);
                layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(id_table));
            }
            if (layer == null)
                return;
            if (layer.Visible != true)
                return;

            try
            {
                string filter = "";
                if (layer.Filter == null)
                {
                    filter = "";
                }
                else
                {
                    filter = layer.Filter;
                }
                if (filter != _where)
                {
                    layer.Filter = _where;
                    classesOfMetods.reloadLayerData(layer);
                }
            }
            catch (Exception ex)
            {
                Program.mainFrm1.StatusInfo = ex.Message;
            }
            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        private string GetSelect(List<FieldInfoFull> _listField, string p = null)
        {
            string select = "";
            string Разделитель = null;
            if (p != null)
                p += '.';
            for (int i = 0; i < _listField.Count; i++)
            {
                if (_listField[i].TypeField == TypeField.Geometry)
                    continue;
                if (_listField[i].TypeField == TypeField.DateTime)
                    select += string.Format("{0} {1}\"{2}\"::timestamp without time zone", Разделитель, p, _listField[i].nameDB);
                else
                    select += string.Format("{0} {1}\"{2}\"", Разделитель, p, _listField[i].nameDB);

                if (string.IsNullOrEmpty(Разделитель))
                    Разделитель = ", ";
            }
            return select;
        }

        void PListUpdate(int countRow, bool isAll)
        {
            //Вычисляем максимальное кол-во листов
            int Остаток = countRow % MaxRowsInList;
            if (Остаток != 0 || countRow > MaxRowsInList)
                maxNumList = countRow + (MaxRowsInList - Остаток);
            maxNumList = maxNumList / MaxRowsInList;
            if (maxNumList > 0 && Остаток == 0)
                maxNumList--;
            if (isAll && maxNumList != 0)
                maxNumList = 1;  //Если выбранн "Загрузить все"

            if (CurrentNumList >= maxNumList)
                CurrentNumList = maxNumList - 1;
            if (CurrentNumList < 0 && maxNumList > 0)
                CurrentNumList = 0;

            btnNextNext.Enabled = btnNext.Enabled = CurrentNumList + 1 < maxNumList;
            btnPrevPrev.Enabled = btnPrev.Enabled = CurrentNumList > 0;

            btnNum.Text = string.Format("{0} из {1}", CurrentNumList + 1, maxNumList);
            if (maxNumList == 0)
                nNum.Maximum = nNum.Minimum = 0;
            else
            {
                nNum.Minimum = 1;
                nNum.Maximum = maxNumList;
            }
        }

        private string GetWhere(IList<FindRequest2> listRequest, out List<IParams> listParams, object id = null)
        {
            int n = 0;
            listParams = new List<IParams>();
            var SB = new StringBuilder();

            if (id != null)
            {
                var par = GetParams(id, _listField.FirstOrDefault(f => f.nameDB == _tableInfo.pkField), n++, true);
                listParams.Add(par);
                SB.AppendFormat("\"{0}\" = {1}", _tableInfo.pkField, par.paramName);
            }
            if (_searchValue != null)
            {
                if (n++ != 0)
                    SB.Append(" AND ");
                var field = _listField.FirstOrDefault(f => f.idField == _searchValue.Key.idField);
                var par = GetParams(_searchValue.Value, field, n++, true);
                listParams.Add(par);
                SB.AppendFormat("\"id!{0}\" = {1}", field.nameDB, par.paramName);
            }
            if (listRequest != null)
                foreach (FindRequest2 t in listRequest)
                {
                    if (n++ != 0)
                        SB.Append(" AND ");

                    var item = t;
                    var par = GetParams(item.FindValue, item.Col, n);

                    listParams.Add(par);
                    SB.Append(GetSqLFindInCell(t, par));
                }
            if (listParams.Count() == 0)
                return " 1 = 1 ";
            return SB.ToString();
        }

        private Params GetParams(object value, FieldInfoFull field, int n, bool isID = false)
        {
            var par = new Params
            {
                _paramName = ((field.idField == -1) ? "all" : field.nameDB) + n,
                value = value,
            };
            if (!isID && field.is_reference)
            {
                par.type = DbType.String;
                return par;
            }
            switch (field.TypeField)
            {
                case TypeField.Text:
                    par.type = DbType.String;
                    break;
                case TypeField.Date:
                    par.type = DbType.DateTime;
                    par.value = Convert.ToDateTime(value).Date;
                    break;
                case TypeField.DateTime:
                    par.type = DbType.DateTime;
                    break;
                case TypeField.Integer:
                    par.type = DbType.Int32;
                    break;
                case TypeField.Numeric:
                    par.type = DbType.Double;
                    break;
                default:
                    par.type = DbType.String;
                    break;
            }
            return par;
        }

        SqlWork GetSqlWork(object id = null)
        {
            List<IParams> listParams = null;
            string where = GetWhere(GetFilters(), out listParams, id);
            string select = GetSelect(_listField);
            FieldInfoFull sortFild = null;
            var sortOrder = SortOrder.Ascending;

            if (dataGridView1.SortedColumn != null)
            {
                sortFild = dataGridView1.SortedColumn.Tag as FieldInfoFull;
                sortOrder = dataGridView1.SortOrder;
            }
            var sqlCmd = new SqlWork();
            sqlCmd.sql = string.Format("SELECT {4} FROM ({0}) as gg \nWHERE {1} \nORDER BY gg.{2} {3}",
                        _tableInfo.sql_view_string,
                        where,
                        (sortFild != null) ? sortFild.nameDB : _tableInfo.pkField,
                        (sortOrder == SortOrder.Ascending) ? "ASC" : "desc",
                        select);
            sqlCmd.ExecuteReader(listParams);
            return sqlCmd;
        }

        public void ReloadRow(int idObj)
        {
            int rowNum = -1;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (idObj == (Convert.ToInt32(dataGridView1.Rows[i].Cells[_tableInfo.pkField].Value)))
                {
                    rowNum = i;
                    break;
                }
            }
            if (rowNum < 0)
                return;
            var dgRow = dataGridView1.Rows[rowNum];
            //получаем sql для загрузки строчки
            string sqll = GetSqlTableNew(_tableInfo);
            List<IParams> listParams = null;
            string where = GetWhere(GetFilters(), out listParams, idObj);
            string select = GetSelect(_listField);
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format("SELECT {1} FROM({0}) as gg WHERE {2}",
                        sqll, select, where);
                sqlCmd.ExecuteReader(listParams);
                while (sqlCmd.CanRead())
                {
                    foreach (var t in _listField)
                    {

                        if (t.TypeField == TypeField.Geometry)
                            continue;
                        var cell = dataGridView1.Rows[rowNum].Cells[t.nameDB];
                        try
                        {
                            var obj = sqlCmd.GetValue(t.nameDB);
                            cell.Value = (obj == DBNull.Value) ? null : obj;

                            dataGridView1.UpdateCellValue(cell.ColumnIndex, cell.RowIndex);
                            if (obj == DBNull.Value || obj == null)
                                obj = "";
                            else
                                obj = obj.ToString().Replace("\n", "").Replace("\t", "");
                            if (t.MaxWord.Length < obj.ToString().Length)
                                t.MaxWord = obj.ToString();
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellMouseDoubleClick_isSelected(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (ParentForm == null)
                return;
            ParentForm.DialogResult = DialogResult.OK;
            ParentForm.Close();
        }

        private void dataGridView1_CellMouseDoubleClick_isEditor(object sender, DataGridViewCellMouseEventArgs e)
        {
            int idObj;
            if (e.RowIndex > -1)
            {
                var cell = dataGridView1.Rows[e.RowIndex].Cells[_tableInfo.pkField];
                if (cell != null && cell.Value != null)
                {
                    idObj = Convert.ToInt32(cell.Value.ToString());

                    //var form = new FormTableData(_tableInfo, idObj, false, "");
                    //form.Show();
                    //form.ActionResult = (DialogResult s) => ReloadRow(idObj);

                    Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(_tableInfo.idTable), idObj, false, this.ParentForm,
                        (DialogResult s) =>
                        {
                            ReloadRow(idObj);
                            if (_tableInfo.type == 2)
                            {
                                Program.CachedStyles.ReloadStyleTable(_tableInfo.idTable);
                            }
                        });
                }
            }
            else
            { idObj = -1; }
        }

        private void tsRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void tsHistory_Click(object sender, EventArgs e)
        {
            PgTableBaseM pgTable = Program.repository.FindTable(_tableInfo.idTable) as PgTableBaseM;
            PgHistoryVM pgHistVM = new PgHistoryVM(Program.repository, table: pgTable);
            PgHistoryV pgHistV = new PgHistoryV(pgHistVM);
            pgHistV.Owner = Program.WinMain;
            pgHistV.Height = 600;
            pgHistV.Width = 900;
            pgHistV.Show();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!classesOfMetods.getWriteTable(_tableInfo.idTable))
            {
                MessageBox.Show(Rekod.Properties.Resources.ucTableNoRight);
                return;
            }
            referInfo referValue = null;
            if (_searchValue != null)
            {
                referValue = new referInfo() { nameField = _searchValue.Key.nameDB, idObj = _searchValue.Value };
            }
            else
            {
                referValue = new referInfo() { nameField = "", idObj = -1 };
            }
            //var form = new FormTableData(_tableInfo, -1, true, "", referValue);
            //form.Show();
            //form.ActionResult = (DialogResult s) => LoadData();

            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(_tableInfo.idTable), -1, true, this.ParentForm, "",
                    (DialogResult s) =>
                    {
                        LoadData();
                        if (_tableInfo.type == 2)
                        {
                            Program.CachedStyles.ReloadStyleTable(_tableInfo.idTable);
                        }
                    },
                    referValue.nameField, referValue.idObj);
        }

        private string GetSqLFindInCell(FindRequest2 fr, Params paParams)
        {
            string format = "lower(\"{0}\"::text) LIKE lower({1})";

            if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_Empty)
            {
                format = "(\"{0}\" is NULL OR (\"{0}\")::text = '')";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_IsNotEmpty)
            {
                format = "(\"{0}\" is NOT NULL AND (\"{0}\")::text <> '')";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_Contains)
            {
                paParams.value = string.Format("%{0}%", paParams.value);
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_StartsWith)
            {
                paParams.value = string.Format("{0}%", paParams.value);
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_DoesNotContain)
            {
                paParams.value = string.Format("%{0}%", paParams.value);
                format = "lower(\"{0}\"::text) NOT LIKE lower({1})";
            }
            else if (fr.TypeFr == "=" && (fr.Col.TypeField == TypeField.Text || fr.Col.TypeField == TypeField.Default))
            {
                paParams.value = string.Format("{0}", paParams.value);
                format = "(\"{0}\")::text" + fr.TypeFr + "{1}";
            }
            else
            {
                if (fr.Col.is_interval)
                    format = "\"id!{0}\"" + fr.TypeFr + "{1}";
                else
                    format = "\"{0}\"" + fr.TypeFr + "{1}";
            }


            //switch (fr.TypeFr)
            //{
            //    case "Пустое":
            //        format = "\"{0}\" is NULL";
            //        break;
            //    case "Не пустое":
            //        format = "\"{0}\" is NOT NULL";
            //        break;
            //    case "Входит":
            //        paParams.value = string.Format("%{0}%", paParams.value);
            //        break;
            //    case "Входит сначала":
            //        paParams.value = string.Format("{0}%", paParams.value);
            //        break;
            //    case "Не входит":
            //        paParams.value = string.Format("%{0}%", paParams.value);
            //        format = "lower(\"{0}\"::text) NOT LIKE lower({1})";
            //        break;
            //    default:
            //        if (fr.Col.is_interval)
            //            format = "\"id!{0}\"" + fr.TypeFr + "{1}";
            //        else
            //            format = "\"{0}\"" + fr.TypeFr + "{1}";
            //        break;
            //}
            if (fr.Col.idField == -1)
            {
                var join = string.Join(" OR ",
                    _listField.Where(f => f.TypeField != TypeField.Geometry).Select(f => string.Format(format, f.nameDB, paParams.paramName)).ToArray());
                return string.Format("({0})", join);
            }
            else
            {
                return string.Format(format, fr.Col.nameDB, paParams.paramName);
            }
        }

        List<FindRequest2> GetFilters()
        {
            return pFilter.Controls.Cast<FindBox2>().Select(item => item.GetFilter()).Where(filter => filter != null).ToList();
        }

        // проверка наличия неверных фильтров
        private bool CheckFilters()
        {
            return (pFilter.Controls.Cast<FindBox2>().Select(item => item.GetFilter()).Where(filter => filter == null).Count() == 0);
        }
        public void ApplyFilter()
        {
            if (CheckFilters())
                LoadData();
            else
                MessageBox.Show(Rekod.Properties.Resources.ucTable_IncorretFilterFormat, Rekod.Properties.Resources.DGBH_ErrorHeader,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void ExportToExcel(string fileName = null)
        {
            List<string> listFilt = new List<string>();
            foreach (var filt in GetFilters())
                listFilt.Add(filt.Col.nameDB);
            string selectCmd = null, select = null;
            if (Program.field_info.Any(w => (w.idTable == _tableInfo.idTable) && (w.type != 5) && (w.visible == true) && (w.is_reference || w.is_interval)))
            {
                DBInfoEdit.ExportToExcel exportForm = new DBInfoEdit.ExportToExcel(_tableInfo, listFilt);
                if (exportForm.ShowDialog() != DialogResult.OK)
                    return;
                selectCmd = exportForm._sqlCmd;
                select = exportForm._select;
            }
            else
            {
                selectCmd = sqll;
                select = "*";
            }

            cti.ThreadProgress.ShowWait();
            List<object[]> listValue = new List<object[]>();
            List<int> ListType = new List<int>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                FieldInfoFull sortFild = null;
                var sortOrder = SortOrder.Ascending;

                if (dataGridView1.SortedColumn != null)
                {
                    sortFild = dataGridView1.SortedColumn.Tag as FieldInfoFull;
                    sortOrder = dataGridView1.SortOrder;
                }

                List<IParams> listParams = null;
                string where = GetWhere(GetFilters(), out listParams);

                sqlCmd.sql = string.Format("SELECT {4} FROM ({0} ) ff\nWHERE {1} \nORDER BY ff.{2} {3}",
                            selectCmd,
                            where,
                            (sortFild != null) ? sortFild.nameDB : _tableInfo.pkField,
                            (sortOrder == SortOrder.Ascending) ? "ASC" : "desc",
                            select);
                sqlCmd.ExecuteReader(listParams);

                int fieldCount = sqlCmd.GetFiealdCount();

                int i = 0;

                List<object> ValueRow = new List<object>();
                foreach (var t in _listField)
                {
                    if (t.TypeField == TypeField.Geometry)
                        continue;
                    if (dataGridView1.Columns.Contains(t.nameDB) && dataGridView1.Columns[t.nameDB].Visible)
                    {
                        ValueRow.Add(t.nameMap);
                        ListType.Add((int)t.TypeField);
                    }
                }
                listValue.Add(ValueRow.ToArray()); // Добавляем названия колонок
                while (sqlCmd.CanRead())
                {
                    i = 0;
                    ValueRow = new List<object>();
                    foreach (var t in _listField)
                    {
                        if (t.TypeField == TypeField.Geometry)
                            continue;
                        if (dataGridView1.Columns.Contains(t.nameDB) && dataGridView1.Columns[t.nameDB].Visible)
                        {
                            try
                            {
                                var obj = sqlCmd.GetValue(t.nameDB);
                                ValueRow.Add((obj == DBNull.Value) ? null : obj);
                                i++;
                            }
                            catch { continue; }
                        }
                    }
                    listValue.Add(ValueRow.ToArray()); // Добавляем значения в колонки
                }
                sqlCmd.Close();
            }
            //asd(new { data = ConvertArray(listValue), Type = ListType });
            var thread = new Thread(StartExportToExcel);

            thread.Start(new DataValues() { data = listValue, Types = ListType, FileName = fileName });

        }

        private static void StartExportToExcel(object data)
        {
            try
            {
                DataValues value = (DataValues)data;
                Rekod.Classes.ExportExcelManager.ExportToExcel(value.data, value.Types, value.FileName);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode == -2147221164)
                {
                    MessageBox.Show("Необходимо установить MS Excel!", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex.Message, true, true);
            }
        }
        private static void StartExportToTxt(object data)
        {
            DataValues value = (DataValues)data;
            Rekod.Classes.ExportTxtManager.ExportToTxt(value.data, value.Types, value.Separator, value.FileName);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (!classesOfMetods.getWriteTable(_tableInfo.idTable))
            {
                MessageBox.Show(Rekod.Properties.Resources.ucTableNoRight);
                return;
            }
            if (_idObjects.Count > 0)
            {
                var dr = MessageBox.Show(Rekod.Properties.Resources.ucTable_DeleteRows,
                Rekod.Properties.Resources.ucTable_DeleteRowsHeader, MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        foreach (var idObject in _idObjects)
                        {
                            sqlCmd.sql = string.Format("DELETE FROM {0}.{1} WHERE {2} = {3}",
                                                       _tableInfo.nameSheme, _tableInfo.nameDB, _tableInfo.pkField, idObject);
                            sqlCmd.Execute(true);
                        }
                        if (_tableInfo.type == 2)
                        {
                            Program.CachedStyles.ReloadStyleTable(_tableInfo.idTable);
                        }
                    }
                    //Program.mainFrm1.layersManager1.removeDeletedObjectFromMap(layerName, idObj, idT);
                    //var layerName = new relation().GetNameInBd(_tableInfo.idTable);
                    var layerName = Program.RelationVisbleBdUser.GetNameInBd(_tableInfo.idTable);
                    {
                        var ll = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                        if (ll != null)
                        {
                            ll.ExternalFullReloadVisible();
                            //foreach (var idObject in _idObjects)
                            //    ll.DeleteID(idObject);
                            //ll.editable = true;
                        }
                    }
                    if (dataGridView1.SelectedCells.Count > 0)
                    {
                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                            dataGridView1.Rows.Remove(row);
                    }
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.ucTable_NoRowsSeleted);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            _idObjects.Clear();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                _idObjects.Add(Convert.ToInt32(row.Cells[_tableInfo.pkField].Value));
            }

            BtnEdit.Enabled = !(_idObjects.Count > 1) && classesOfMetods.getWriteTable(_tableInfo.idTable);
            _idObj = (_idObjects.Count == 1) ? _idObjects[0] : -1;
            this.Tag = _idObj;

            tsВыбОбъекты.Text = _idObjects.Count.ToString();

            btnDel.Enabled = (_idObjects.Count >= 1);
            BtnEdit.Enabled = (_idObjects.Count == 1);
            отчетыСТекущимОбъектомToolStripMenuItem.Enabled = (_idObjects.Count == 1);

        }

        private void dataGridView1_SelectionChanged_map(object sender, EventArgs e)
        {
            MoveTo();
        }
        private void MoveTo()
        {
            if (isMapMoveTo)
            {
                var layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(_tableInfo.idTable));
                if (layer == null || layer.Visible != true)
                {
                    // пытаемся включить слой
                    Program.mainFrm1.layerItemsView1.SetLayerVisible(_tableInfo.idTable);
                    layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(_tableInfo.idTable));
                    if (layer == null || layer.Visible != true)
                        return;
                }

                try
                {
                    if (_idObj != -1)
                    {
                        mvMapLib.mvVectorObject mvObj = layer.getObject(_idObj);
                        layer.DeselectAll();
                        layer.SelectId(_idObj);

                        Debug.WriteLineIf(mvObj == null, "mvObj == null", "UcTableObjects");
                        if (mvObj != null)
                        {
                            if (mvObj.VectorType != mvMapLib.mvVecTypes.mvVecPoint)
                            {
                                if (mvObj.points.count == 0)
                                {
                                    Program.mainFrm1.StatusInfo = "Объект не содержит геометрию";
                                }
                                else
                                {
                                    if (Program.SettingsXML.LocalParameters.EnterTheScreen)
                                    {
                                        Program.mainFrm1.MoveToSelectedObjects(layer);
                                        //Program.mainFrm1.axMapLIb1.SetExtent(mvObj.bbox);
                                        //Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("layer.MoveTo(_idObj, true);", "UcTableObjects");
                                        layer.MoveTo(_idObj, true);
                                    }
                                }
                            }
                            else
                            {
                                Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                            }

                            //mvObj.Selected = true;
                            Program.mainFrm1.axMapLIb1.mapRepaint();
                        }
                    }
                    else
                    {
                        layer.DeselectAll();
                        mvMapLib.mvIntArray ids = new mvMapLib.mvIntArray();
                        ids.count = _idObjects.Count;
                        int i = 0;
                        foreach (int id in _idObjects)
                        {
                            ids.setElem(i, id);
                            i++;
                        }
                        layer.SelectArray(ids);
                        Program.mainFrm1.MoveToSelectedObjects(layer);
                        //layer.MoveToArray(layer.getSelected(), true);
                    }

                }
                catch { } // Не получилось изменить масштаб. 
            }
        }
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var cell = dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[_tableInfo.pkField].Value;
            int idObj = dataGridView1.SelectedCells.Count > 0 && cell != null
                            ? ExtraFunctions.Converts.To<int>(cell)
                            : -1;
            //var form = new FormTableData(_tableInfo, idObj, false, "");
            //form.Show();


            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(_tableInfo.idTable), idObj, false, this.ParentForm,
                        (DialogResult s) =>
                        {
                            ReloadRow(idObj);
                            if (_tableInfo.type == 2)
                            {
                                Program.CachedStyles.ReloadStyleTable(_tableInfo.idTable);
                            }
                        }
                    );
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            var btn = sender as System.Windows.Forms.Button;
            if (btn == null)
                return;
            if (btn == btnNextNext)
                CurrentNumList = maxNumList - 1;
            else if (btn == btnNext)
                CurrentNumList++;
            else if (btn == btnPrev)
                CurrentNumList--;
            else if (btn == btnPrevPrev)
                CurrentNumList = 0;
            else
                MessageBox.Show(Rekod.Properties.Resources.DGBH_ErrorHeader);
            LoadData();
        }

        private void btnNum_Click(object sender, EventArgs e)
        {
            nNum.Visible = true;
            btnNum.Visible = false;
            nNum.Select(0, nNum.Text.Length);
            nNum.Focus();
        }
        private void nNum_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                nNum.Visible = false;
        }

        private void nNum_Validated(object sender, EventArgs e)
        {
            nNum.Visible = false;
            btnNum.Visible = true;
            if (CurrentNumList == ((int)nNum.Value) - 1)
                return;
            CurrentNumList = ((int)nNum.Value) - 1;
            LoadData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void Close()
        {
            if (ParentForm != null)
                ParentForm.Close();
        }

        private void cbViewAll_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ParentForm == null)
                return;
            ParentForm.DialogResult = DialogResult.OK;
            ParentForm.Close();
        }

        private void CreateTempTable()
        {
            bool exists_bool = false;
            string table_sql = "SELECT exists(SELECT true FROM pg_catalog.pg_statio_all_tables WHERE relname like 'temp_table_exp')";
            SqlWork cmd = new SqlWork();
            cmd.sql = table_sql;
            cmd.Execute(false);
            if (cmd.CanRead())
            {
                exists_bool = cmd.GetBoolean(0);
            }
            cmd.Close();
            if (!exists_bool)
            {
                string sql =
@"CREATE TABLE temp_table_exp
(
  id integer NOT NULL,
  id_session integer NOT NULL
);
GRANT ALL ON TABLE temp_table_exp TO public;";
                cmd = new SqlWork();
                cmd.sql = sql;
                cmd.Execute(true);
                cmd.Close();
            }
        }
        private void InsertInTempTable(int id_session, int[] ids)
        {
            string sql_string = "";
            for (int i = 0; ids.Length > i; i++)
            {
                sql_string = sql_string + "INSERT INTO temp_table_exp VALUES (" + ids[i].ToString() + "," + id_session.ToString() + ");";
            }
            SqlWork cmd = new SqlWork();
            cmd.sql = sql_string;
            cmd.Execute(true);
            cmd.Close();
        }
        private void InsertInTempTable(int id_session, int[] ids, int id_count)
        {
            string sql_string = "";
            for (int i = 0; id_count > i; i++)
            {
                sql_string = sql_string + "INSERT INTO temp_table_exp VALUES (" + ids[i].ToString() + "," + id_session.ToString() + ");";
            }
            SqlWork cmd = new SqlWork();
            cmd.sql = sql_string;
            cmd.Execute(true);
            cmd.Close();
        }
        private void DeleteInTempTable(int id_session)
        {
            SqlWork cmd = new SqlWork();
            cmd.sql = "DELETE FROM temp_table_exp WHERE id_session = " + id_session.ToString();
            cmd.Execute(true);
            cmd.Close();
        }

        private void CopyObj()
        {
            //int max_id = 0;
            SqlWork cmd = new SqlWork();
            //cmd.sql = "SELECT MAX(" + _tableInfo.pkField + ") FROM " + _tableInfo.nameSheme + "." + _tableInfo.nameDB;
            //cmd.ExecuteReader();
            //if (cmd.CanRead())
            //    max_id = cmd.GetInt32(0);
            //cmd.Close();

            string sql_field = "";
            List<fieldInfo> fInfo = classesOfMetods.getFieldInfoTable(_tableInfo.idTable);
            for (int i = 0; fInfo.Count > i; i++)
            {
                if (fInfo[i].nameDB != _tableInfo.pkField)
                {
                    sql_field += "\"" + fInfo[i].nameDB + "\",";
                }
            }
            sql_field = sql_field.Substring(0, sql_field.Length - 1);
            string comma = (sql_field.Length > 1) ? "," : "";

            var param = new List<IParams>();
            param.Add(new Params() { paramName = "@copy_id", typeData = NpgsqlDbType.Integer });

            sql_field = "INSERT INTO \"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\" (" + sql_field +
                ") SELECT " + sql_field + " FROM \"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB +
                "\" WHERE " + _tableInfo.pkField + "=" + param[0].paramName;

            cmd = new SqlWork();
            foreach (var id in _idObjects)
            {
                cmd.sql = sql_field;
                param[0].value = id;
                cmd.Execute(true, param.ToArray());
            }
            cmd.Close();
            LoadData();
            MessageBox.Show(Rekod.Properties.Resources.DGBH_OperSuccess, Rekod.Properties.Resources.InformationMessage_Header,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void создатьКопиюВыделеннойСтрокиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyObj();
        }

        private void cbMapMoveTo_CheckedChanged(object sender, EventArgs e)
        {
            isMapMoveTo = cbMapMoveTo.Checked;
            MoveTo();
        }

        private void ExportToTXT(string fileName, string separator)
        {
            List<object[]> listValue = new List<object[]>();
            List<int> ListType = new List<int>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                List<IParams> listParams = null;
                string where = GetWhere(GetFilters(), out listParams);
                string select = GetSelect(_listField);
                FieldInfoFull sortFild = null;
                var sortOrder = SortOrder.Ascending;

                if (dataGridView1.SortedColumn != null)
                {
                    sortFild = dataGridView1.SortedColumn.Tag as FieldInfoFull;
                    sortOrder = dataGridView1.SortOrder;
                }

                sqlCmd.sql = string.Format("SELECT {4} FROM ({0}) as gg \nWHERE {1} \nORDER BY gg.{2} {3}",
                            GetSqlTableNew(_tableInfo),
                            where,
                            (sortFild != null) ? sortFild.nameDB : _tableInfo.pkField,
                            (sortOrder == SortOrder.Ascending) ? "ASC" : "desc",
                            select);
                sqlCmd.ExecuteReader(listParams);

                int fieldCount = sqlCmd.GetFiealdCount();

                int i = 0;

                object[] ValueRow = new object[fieldCount];
                foreach (var t in _listField)
                {
                    if (t.TypeField == TypeField.Geometry)
                        continue;
                    ValueRow[i++] = t.nameMap;
                    ListType.Add((int)t.TypeField);
                }
                listValue.Add(ValueRow);
                while (sqlCmd.CanRead())
                {
                    i = 0;
                    ValueRow = new object[fieldCount];
                    foreach (var t in _listField)
                    {
                        if (t.TypeField == TypeField.Geometry)
                            continue;
                        try
                        {
                            var obj = sqlCmd.GetValue(t.nameDB);
                            ValueRow[i] = obj;
                            i++;
                        }
                        catch
                        { continue; }

                    }
                    listValue.Add(ValueRow);
                }
                sqlCmd.Close();
            }
            Thread thread = new Thread(StartExportToTxt);
            thread.Start(new DataValues() { data = listValue, Types = ListType, Separator = separator, FileName = fileName });
        }

        private void удалитьВсеЗаписиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Rekod.Properties.Resources.ucTable_DeleteAllRows,
                Rekod.Properties.Resources.ucTable_DeleteAllRowsConfirm,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format("DELETE FROM \"{0}\".\"{1}\";", _tableInfo.nameSheme, _tableInfo.nameDB);
                    sqlCmd.ExecuteNonQuery();
                    sqlCmd.Close();
                    classesOfMetods.reloadLayerData(_tableInfo);
                    LoadData();
                }
            }
        }

        private void importFromFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = importFilesDialogFilter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var iw = new ImportExport.ImportWindow(new FileInfo(ofd.FileName), _tableInfo) { Owner = Program.WinMain };
                    iw.ShowDialog();
                }
            }
        }

        private void tsExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = false;
            sfd.Filter = exportFilesDialogFilter;
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            while (File.Exists(sfd.FileName))
            {
                if (MessageBox.Show(Rekod.Properties.Resources.DGBH_ErrorReplaceFile,
                    Rekod.Properties.Resources.DGBH_ErrorHeader, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                    return;
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
            }
            FileInfo fi = new FileInfo(sfd.FileName);
            if (fi.Extension.ToLower() == ".txt")
            {
                //TXT
                try
                {
                    Rekod.DBInfoEdit.SetSeparatorFrm frm = new DBInfoEdit.SetSeparatorFrm();
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        cti.ThreadProgress.ShowWait();
                        ExportToTXT(sfd.FileName, frm.Separator);
                        cti.ThreadProgress.Close();
                    }
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close();
                    Classes.workLogFile.writeLogFile(ex, true, true);
                }
            }

            List<IParams> listParams = null;
            string where = "";
            List<FindRequest2> ftemp = GetFilters();

            where = GetWhereForMap(ftemp, out listParams);

            var ext = fi.Extension.ToLower();
            if (ext == ".shp" || ext == ".tab" || ext == ".csv" ||
                ext == ".geojson" || ext == ".gxt" || ext == ".dxf" ||
                ext == ".gmt" || ext == ".itf" || ext == ".kml" ||
                ext == ".mif" || ext == ".sqlite" || ext == ".xml")
            {

                if (ext == ".gmt")
                    fi = new FileInfo(fi.Directory + "\\" + fi.Name);
                if (ext == ".geojson" || ext == ".itf" || ext == ".kml" || ext == ".mif" || ext == ".gxt" || ext == ".xml" || ext == ".dxf")
                    fi = new FileInfo(fi.Directory + "\\" + fi.Name + "\\" + fi.Name);
                var shpExporter = new ImportExport.Exporters.SHPExporter(_tableInfo, fi);
                shpExporter.Export(null, where, listParams);
            }
            else if (ext == ".xls" | ext == ".xlsx")
            {
                try
                {
                    ExportToExcel(fi.FullName);
                    cti.ThreadProgress.Close();
                }
                catch (Exception ex)
                {
                    Classes.workLogFile.writeLogFile(ex, true, true);
                    cti.ThreadProgress.Close();
                }
            }
            else if (ext == ".dbf")
            {
                var dbfExp = new ImportExport.Exporters.ExportDBF(_tableInfo, fi);
                dbfExp.Export(where, listParams);
            }
        }

        private void открытьВMSExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExportToExcel();
                cti.ThreadProgress.Close();
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
                cti.ThreadProgress.Close();
            }
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetFilters(Convert.ToInt32(((ToolStripMenuItem)sender).Tag));
        }
        private void LoadFilters()
        {
            filtersToolStripMenuItem1.DropDownItems.Clear();
            var _tFilter = Program.Filters.FilterByTable("\"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\"");

            foreach (var _item in _tFilter)
            {
                ToolStripMenuItem filter = new ToolStripMenuItem(_item.FilterName);
                filter.Click += new EventHandler(filterToolStripMenuItem_Click);
                filter.Tag = _item.ID;
                filtersToolStripMenuItem1.DropDownItems.Add(filter);
            }
            filtersToolStripMenuItem1.Enabled = (_tFilter.Count != 0);
            удалениеФильтровToolStripMenuItem.Enabled = (_tFilter.Count != 0);
        }
        private void SetFilters(int idFilter)
        {
            RemoveFilterAll();

            var filter = Program.Filters.GetFilter(idFilter);
            if (filter.Filter.Type == TypeRelation.ELEMENT)
            {
                FindBox2 filter_control = new FindBox2();
                filter_control.SetFilter(filter.Filter.Element, _listIs);
                this.AddFilterInPanel(filter_control);
            }
            else
            {
                foreach (var item in filter.Filter.Arguments)
                {
                    FindBox2 filter_control = new FindBox2();
                    filter_control.SetFilter(item.Element, _listIs);
                    this.AddFilterInPanel(filter_control);
                }
            }
            LoadData();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new FilterSaveFrm();
            frm.Text = Rekod.Properties.Resources.ucTable_Filter;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var f = new FilterViewModel();
                f.FilterName = frm.textBox1.Text;
                f.Table = "\"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\"";
                f.SourceName = Login.ConcatenateServerWithoutPort(Program.connString);
                f.Filter.Element = new FilterElementModel();
                if (pFilter.Controls.Count == 1)
                {
                    f.Filter.Type = TypeRelation.ELEMENT;
                    f.Filter.Element = ((FindBox2)pFilter.Controls[0]).GetFilterElementModel();
                }
                else
                {
                    f.Filter.Type = TypeRelation.AND;
                    f.Filter.Arguments = new System.Collections.ObjectModel.ObservableCollection<FilterRelationModel>();
                    foreach (FindBox2 item in pFilter.Controls)
                    {
                        f.Filter.Arguments.Add(new FilterRelationModel(TypeRelation.ELEMENT,
                            item.GetFilterElementModel(), null));
                    }
                }
                f.Save();
                var cl = new classesOfMetods();
                cl.LoadFilters();
                LoadFilters();
            }

        }

        private void удалениеФильтровToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable("Filters");

            dt.Columns.Add(new DataColumn("id", typeof(int)));
            dt.Columns.Add(new DataColumn(Rekod.Properties.Resources.ucTable_FilterName, typeof(string)));

            var _tFilter = Program.Filters.FilterByTable("\"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\"");

            foreach (var _item in _tFilter)
            {
                dt.Rows.Add(new object[] { _item.ID, _item.FilterName });
            }
            DBHistory.CheckObjects cheks = new DBHistory.CheckObjects(dt, new List<int>(), "id", Rekod.Properties.Resources.CheckObjects_Title);
            if (cheks.ShowDialog() == true)
            {
                if (cheks.CheckedList != null)
                {
                    if (MessageBox.Show(Rekod.Properties.Resources.ucTable_DeleteFilter, Rekod.Properties.Resources.ucTable_DeletingHeader,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        foreach (var item in cheks.CheckedList)
                        {
                            Program.Filters.Delete(item);
                        }
                    }
                }
            }
            LoadFilters();
        }
        private string GetWhereForMap(IList<FindRequest2> listRequest, out List<IParams> listParams, object id = null)
        {
            int n = 0;
            listParams = new List<IParams>();
            var SB = new StringBuilder();

            if (id != null)
            {
                var par = GetParams(id, _listField.FirstOrDefault(f => f.nameDB == _tableInfo.pkField), n++, true);
                listParams.Add(par);
                SB.AppendFormat("\"{0}\" = {1}", _tableInfo.pkField, par.paramName);
            }
            if (_searchValue != null)
            {
                if (n++ != 0)
                    SB.Append(" AND ");
                var field = _listField.FirstOrDefault(f => f.idField == _searchValue.Key.idField);
                var par = GetParams(_searchValue.Value, field, n++, true);
                listParams.Add(par);
                SB.AppendFormat("\"id!{0}\" = {1}", field.nameDB, par.paramName);
            }
            if (listRequest != null)
                foreach (FindRequest2 t in listRequest)
                {
                    if (n++ != 0)
                        SB.Append(" AND ");

                    var item = t;
                    var par = GetParams(item.FindValue, item.Col, n);

                    listParams.Add(par);
                    SB.Append(GetSqLFindInCellForMap(t, par));
                }
            if (listParams.Count() == 0)
                return " 1 = 1 ";
            return SB.ToString();
        }
        public void ApplyUserMapFilter(bool Clear = false)
        {
            if (Clear)
            {
                SetFilterInMap("", _tableInfo.idTable);
                return;
            }
            List<IParams> listParams = null;
            string where = "";
            List<FindRequest2> ftemp = GetFilters();
            where = GetWhereForMap(ftemp, out listParams);

            string layerWhere = GetSqlWhereStringForMap(where, listParams);
            SetFilterInMap(layerWhere, _tableInfo.idTable);
        }
        private string GetSqLFindInCellForMap(FindRequest2 fr, Params paParams)
        {
            string format = "lower(\"{0}\"::text) LIKE lower({1})";
            string format_is_ref = "\"{0}\" in (SELECT \"{1}\" FROM \"{2}\".\"{3}\" as \"{4}\" WHERE &&&&&&&&)";
            string query = "";


            if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_Empty)
            {
                format = "(\"{0}\" is NULL OR \"{0}\" = '')";
                query = "(\"{5}\" is NULL OR \"{5}\" = '')";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_IsNotEmpty)
            {
                format = "(\"{0}\" is NOT NULL AND (\"{0}\")::text <> '')";
                query = "(\"{5}\" is NOT NULL AND (\"{5}\")::text <> '')";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_Contains)
            {
                paParams.value = string.Format("%{0}%", paParams.value);
                query = "lower(\"{5}\"::text) LIKE lower({6})";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_StartsWith)
            {
                paParams.value = string.Format("{0}%", paParams.value);
                query = "lower(\"{5}\"::text) LIKE lower({6})";
            }
            else if (fr.TypeFr == Rekod.Properties.Resources.FindBox2_DoesNotContain)
            {
                paParams.value = string.Format("%{0}%", paParams.value);
                format = "lower(\"{0}\"::text) NOT LIKE lower({1})";
                query = "lower(\"{5}\"::text) NOT LIKE lower({6})";
            }
            else
            {
                format = "\"{0}\"" + fr.TypeFr + "{1}";
            }

            format_is_ref = format_is_ref.Replace("&&&&&&&&", query);
            if (fr.Col.idField == -1)
            {
                var join = string.Join(" OR ",
                    _listField.Where(f => f.TypeField != TypeField.Geometry && f.is_reference == false).Select(f => string.Format(format, f.nameDB, paParams.paramName)).ToArray());
                var join_is_ref = string.Join(" OR ",
                    _listField.Where(f => f.TypeField != TypeField.Geometry && f.is_reference == true).Select(f =>
                        string.Format(format_is_ref,
                        f.nameDB,
                        classesOfMetods.getFieldInfo(f.ref_field.Value).nameDB,
                        classesOfMetods.getTableInfo(f.ref_table.Value).nameSheme,
                        classesOfMetods.getTableInfo(f.ref_table.Value).nameDB,
                        "qwer" + f.idField.ToString(),
                        classesOfMetods.getFieldInfo(f.ref_field_name.Value).nameDB,
                        paParams.paramName)).ToArray());
                if (!String.IsNullOrEmpty(join_is_ref))
                {
                    return string.Format("({0} OR {1})", join, join_is_ref);
                }
                else
                {
                    return join;
                }
            }
            else
            {
                if (fr.Col.is_reference == false)
                    return string.Format(format, fr.Col.nameDB, paParams.paramName);
                else
                    return string.Format(format_is_ref,
                        fr.Col.nameDB,
                        classesOfMetods.getFieldInfo(fr.Col.ref_field.Value).nameDB,
                        classesOfMetods.getTableInfo(fr.Col.ref_table.Value).nameSheme,
                        classesOfMetods.getTableInfo(fr.Col.ref_table.Value).nameDB,
                        "qwer" + fr.Col.idField.ToString(),
                        classesOfMetods.getFieldInfo(fr.Col.ref_field_name.Value).nameDB,
                        paParams.paramName);
            }
        }

        private void применитьНаКартеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyUserMapFilter();
        }
        public void SetVisibleUseMapFilterMenu(bool isVisible)
        {
            if (_tableInfo.type == 1)
                применитьНаКартеToolStripMenuItem.Visible = isVisible;
            else
                применитьНаКартеToolStripMenuItem.Visible = false;
        }



        /// <summary>
        /// Вывести сообщение об ошибке
        /// </summary>
        /// <param name="text">Текст ошибки, если текста нет, то окно с ошибкой проподает</param>
        private void ErrorMess(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                pMess.Visible = false;
                lblMess.Text = "";
            }
            else
            {
                pMess.Visible = true;
                lblMess.Text = text;
            }
        }

        private void UcTableObjects_Load(object sender, EventArgs e)
        {
            // Если ограничения по загрузке таблицы при старте 
            if (!_tableInfo.DisplayWhenOpening)
            {
                ErrorMess(Rekod.Properties.Resources.ucTable_NotLoaded);
            }
            else
                try
                {
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Rekod.Properties.Resources.DGBH_ErrorHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void MenuReloadReports()
        {
            отчетыСТаблицейToolStripMenuItem.DropDownItems.Clear();
            отчетыСТекущимОбъектомToolStripMenuItem.DropDownItems.Clear();

            var listReports = Program.ReportModel.FindReportsByIdTable(_tableInfo.idTable);
            foreach (var item in listReports)
            {
                switch (item.Type)
                {
                    case enTypeReport.Object:
                        {
                            CreateItemReport(отчетыСТекущимОбъектомToolStripMenuItem, item);
                        }
                        break;
                    case enTypeReport.Table:
                        {
                            CreateItemReport(отчетыСТаблицейToolStripMenuItem, item);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void CreateItemReport(ToolStripMenuItem tsItemMenu, IReportItem_M item)
        {
            var tsItem = new ToolStripMenuItem(item.Caption);
            tsItem.Tag = item;

            switch (item.Type)
            {
                case enTypeReport.Object:
                    {
                        tsItem.Click += tsItemOpenReportObject_Click;
                    }
                    break;
                case enTypeReport.Table:
                    {
                        tsItem.Click += tsItemOpenReportTable_Click;
                    }
                    break;
                default:
                    break;
            }
            tsItemMenu.DropDownItems.Add(tsItem);
        }


        void tsItemOpenReportObject_Click(object sender, EventArgs e)
        {
            var tsItemObject = sender as ToolStripItem;
            var report = tsItemObject.Tag as ReportItem_M;

            try
            {
                if (IdObj == -1)
                    throw new Exception("Укажите текущий объект");
                List<IParams> listParams = null;
                List<FindRequest2> ftemp = GetFilters();
                string where = GetWhere(ftemp, out listParams);
                where = GetSqlWhereStringForMap(where, listParams);
                var order = GetOrderBy();

                var filter = new FilterTable(IdTable, IdObj, where, order);
                Program.ReportModel.OpenReport(report, filter);
            }
            catch (Exception ex)
            {
                ErrorMess(ex.Message);
            }
        }
        void tsItemOpenReportTable_Click(object sender, EventArgs e)
        {
            var tsItemObject = sender as ToolStripItem;
            var report = tsItemObject.Tag as ReportItem_M;
            try
            {
                List<IParams> listParams = null;
                List<FindRequest2> ftemp = GetFilters();
                string where = GetWhere(ftemp, out listParams);
                where = GetSqlWhereStringForMap(where, listParams);
                var order = GetOrderBy();
                var filter = new FilterTable(IdTable, IdObj, where, order);
                Program.ReportModel.OpenReport(report, filter);
            }
            catch (Exception ex)
            {
                ErrorMess(ex.Message);
            }
        }

        private void открытьМенеджерОтчетовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<IParams> listParams = null;
                List<FindRequest2> ftemp = GetFilters();
                string where = GetWhere(ftemp, out listParams);
                where = GetSqlWhereStringForMap(where, listParams);
                var order = GetOrderBy();
                var filter = new FilterTable(IdTable, IdObj, where, order);
                Program.ReportModel.OpenReportEditor(filter, enTypeReport.Table);
            }
            catch (Exception ex)
            {
                ErrorMess(ex.Message);
            }
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnDel_Click(null, null);
            }
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            var field = e.Column.Tag as FieldInfoFull;

            var value1 = e.CellValue1 as IComparable;
            var value2 = e.CellValue2 as IComparable;
            if (field.is_interval)
            {
                value1 = SubStringInterval(field, e.CellValue1) as IComparable;
                value2 = SubStringInterval(field, e.CellValue2) as IComparable;

                if (value1 != null && value2 != null)
                    e.SortResult = value1.CompareTo(value2);
                else if (value1 == null && value2 == null)
                    e.SortResult = 0;
                else if (value1 == null)
                    e.SortResult = 1;
                else if (value2 == null)
                    e.SortResult = -1;
            }
            e.Handled = true;
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                UpdateDataGridContextMenu();
                contextMenuForDataGridHeader.Show(dataGridView1, dataGridView1.PointToClient(Cursor.Position));
            }
        }

        private void UpdateDataGridContextMenu()
        {
            contextMenuForDataGridHeader.Items.Clear();

            foreach (var menuItem in
                        from DataGridViewColumn column
                            in dataGridView1.Columns
                        where column.Name != _tableInfo.pkField
                        select new ToolStripMenuItem()
                                    {
                                        Name = column.Name,
                                        Checked = column.Visible,
                                        Text = column.HeaderText,
                                        CheckOnClick = true
                                    })
            {
                menuItem.CheckStateChanged += (sender, args) =>
                {
                    var stripMenuItem = sender as ToolStripMenuItem;
                    dataGridView1
                        .Columns
                        .Cast<DataGridViewColumn>()
                        .First(x => x.Name == stripMenuItem.Name)
                        .Visible = stripMenuItem.Checked;
                };

                contextMenuForDataGridHeader.Items.Add(menuItem);
            }

            contextMenuForDataGridHeader.Items.Add(new ToolStripSeparator());

            var lastMenuItem = new ToolStripMenuItem()
            {
                Text = "Настроить..."
            };
            lastMenuItem.Click += (sender, args) => ShowSetting();
            contextMenuForDataGridHeader.Items.Add(lastMenuItem);
        }

        private void ShowSetting()
        {
            var viewSetting = new GridViewSettingVM(
                dataGridView1
                    .Columns
                    .Cast<DataGridViewColumn>()
                    .OrderBy(x => x.DisplayIndex)
                    .Select(
                        x =>
                            new GridViewSettingVM.Column()
                            {
                                Name = x.Name,
                                Text = x.HeaderText,
                                IsChecked = x.Visible,
                                IsEnabled = x.Name != _tableInfo.pkField
                            })
                );

            if (viewSetting.IsCanceled)
            {
                return;
            }

            foreach (var column in dataGridView1.Columns.Cast<DataGridViewColumn>())
            {
                column.Visible = viewSetting
                    .ColumnList
                    .First(x => x.Name == column.Name)
                    .IsChecked;
                column.DisplayIndex = viewSetting
                    .ColumnList
                    .IndexOf(
                        viewSetting
                            .ColumnList
                            .First(x => x.Name == column.Name)
                    );
            }
        }

        private void UpdateViewHeaderMenu()
        {
            сохраненныеВидыToolStripMenuItem
                    .DropDown
                    .Items.Clear();

            var views = TableViewInfo.GetViews(_tableInfo);

            if (views.Any())
            {
                сохраненныеВидыToolStripMenuItem
                    .DropDown
                    .Items
                    .AddRange(
                        views
                            .Select((x) =>
                            {
                                var item = new ToolStripMenuItem(x.Title);
                                item.Click += (sender, args) => ActivateView(x);
                                return item;
                            })
                            .ToArray());
                defaultViewInfo = views.FirstOrDefault(x => x.IsDefault);
            }

            if (сохраненныеВидыToolStripMenuItem
                .DropDown
                .Items
                .Count == 0)
            {
                сохраненныеВидыToolStripMenuItem.Enabled = false;
                удалениеВидовToolStripMenuItem.Enabled = false;
            }
            else
            {
                сохраненныеВидыToolStripMenuItem.Enabled = true;
                удалениеВидовToolStripMenuItem.Enabled = true;
            }
        }

        private void ActivateView(TableViewInfo viewInfo)
        {
            var fields = TableViewFieldInfo.GetViewFields(viewInfo);
            if (fields.Any())
            {
                int counter = 0;
                fields = fields
                    .OrderBy(x => x.OrderNumber)
                    .Where(x => dataGridView1
                                    .Columns
                                    .Cast<DataGridViewColumn>()
                                    .Any(y => y.Name == x.FieldDBName))
                    .Select(x =>
                    {
                        x.OrderNumber = counter++;
                        return x;
                    })
                    .ToList();

                foreach (var column in dataGridView1.Columns.Cast<DataGridViewColumn>())
                {
                    if (fields.Any((x => x.FieldDBName == column.Name)))
                    {
                        column.Visible = fields
                            .First(x => x.FieldDBName == column.Name)
                            .IsVisible;
                        column.DisplayIndex = fields
                            .First(x => x.FieldDBName == column.Name)
                            .OrderNumber;
                    }
                    else
                    {
                        column.Visible = true;
                        column.DisplayIndex = counter++;
                    }
                }
            }
        }

        private void сохранитьТекущийВидToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TableViewInfo.SaveCurrentView(_tableInfo, dataGridView1.Columns.Cast<DataGridViewColumn>().ToList());
            UpdateViewHeaderMenu();
        }

        private void удалениеВидовToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DataTable dt = new DataTable("Filters");

            dt.Columns.Add(new DataColumn("id", typeof(int)));
            dt.Columns.Add(new DataColumn(Rekod.Properties.Resources.ucTable_FilterName, typeof(string)));

            var views = TableViewInfo.GetViews(_tableInfo);
            if (views.Any())
            {
                foreach (var item in views)
                {
                    dt.Rows.Add(new object[] { item.ViewId, item.Title });
                }
            }

            DBHistory.CheckObjects cheks = new DBHistory.CheckObjects(dt, new List<int>(), "id", Rekod.Properties.Resources.CheckObjects_Title);

            if (cheks.ShowDialog() == true)
            {
                if (cheks.CheckedList != null)
                {
                    if (MessageBox.Show("Вы действительно хотите удалить выбранные виды?", Rekod.Properties.Resources.ucTable_DeletingHeader,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        foreach (var item in cheks.CheckedList)
                        {
                            TableViewInfo.DeleteView(item);
                        }
                    }
                }
            }

            UpdateViewHeaderMenu();
        }

        private void видПоУмолчаниюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivateDefaultView();
        }

        private void ActivateDefaultView()
        {
            if (defaultViewInfo != null)
            {
                ActivateView(defaultViewInfo);
            }
            else
            {
                foreach (var column in dataGridView1.Columns.Cast<DataGridViewColumn>())
                {
                    column.Visible = true;
                    column.DisplayIndex = column.Index;
                }
            }
        }

        private void настроитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSetting();
        }
    }
}