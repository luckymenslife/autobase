using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.AbstractSource;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgTVM = Rekod.DataAccess.SourcePostgres.Model.PgTableView;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Data;
using Npgsql;
using System.Windows.Input;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Rekod.AttachedProperties;
using System.ComponentModel;
using Interfaces;
using System.Windows.Forms;
using System.IO;
using Rekod.SQLiteSettings;
using Rekod.DataAccess.SourcePostgres.Model.PgTableView;
using Rekod.Controllers;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView
{
    /// <summary>
    /// Модель представления для отображения таблицы
    /// </summary>
    public class PgTableViewVM : WindowViewModelBase_VM
    {
        #region Статические поля
        /// <summary>
        /// Макс. кол-во строк на странице (обычно 500)
        /// </summary>
        private static int __pageMaxRows = 500;
        #endregion Статические поля

        #region Поля
        private NpgsqlConnectionStringBuilder _connect;
        private string _sysScheme;
        private PgM.PgTableBaseM _table;
        private PgDataRepositoryVM _source;
        private ObservableCollection<HeaderValue> _columns;
        private bool _isReadOnly;
        private bool _isDebug;
        private bool _toSelected;

        private string sqlSelect;
        private int _pageShowRows = 0;
        private int? _pageCurrent = 1;

        private int _tableRows;
        private int _tablePages;

        private object _objectCurrent = null;

        private DataView _data;
        private DataRowView _currentRowView;
        private PgTableView.PgTableViewFilterVM _filterVM;
        private ObservableCollection<PgTableView.PgTableViewFilterVM> _filters; 

        private bool _isGoToMap;

        private ICommand _openObjectCommand;
        private ICommand _createObjectCommand;
        private ICommand _reloadCommand;
        private ICommand _goToFirstCommand;
        private ICommand _addFilterCommand;
        private ICommand _removeFilterCommand;
        private ICommand _addContainerCommand;
        private ICommand _exportCommand;
        private ICommand _importCommand;
        private ICommand _deleteObjectCommand;
        private ICommand _goToMapCommand;
        private ICommand _choiceCommand;

        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Ссылка на таблицу
        /// </summary>
        public AbsM.ITableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// Только для чтения
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        public PgVM.PgDataRepositoryVM Source
        {
            get { return _source; }
        }
        /// <summary>
        /// Для выбора объекта
        /// </summary>
        public bool ToSelected
        {
            get { return _toSelected; }
        }
        /// <summary>
        /// Кол-во строк загруженных на странице
        /// </summary>
        public int PageShowRows
        {
            get { return _pageShowRows; }
            private set { OnPropertyChanged(ref _pageShowRows, value, () => this.PageShowRows); }
        }
        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int? PageCurrent
        {
            get { return _pageCurrent; }
            private set
            {
                OnPropertyChanged(ref _pageCurrent, value, () => this.PageCurrent);
                OnPropertyChanged(() => this.ShowAllRows);
            }
        }
        /// <summary>
        /// Кол-во строк в таблице
        /// </summary>
        public int TableRows
        {
            get { return _tableRows; }
            private set { OnPropertyChanged(ref _tableRows, value, () => this.TableRows); }
        }
        /// <summary>
        /// Кол-во страниц в таблице
        /// </summary>
        public int TablePages
        {
            get { return _tablePages; }
            private set { OnPropertyChanged(ref _tablePages, value, () => this.TablePages); }
        }
        /// <summary>
        /// отображать полный список 
        /// </summary>
        public bool ShowAllRows
        { get { return PageCurrent == null; } }
        /// <summary>
        /// Данные с таблицы
        /// </summary>
        public DataView Data
        {
            get { return _data; }
            private set { OnPropertyChanged(ref _data, value, () => this.Data); }
        }
        /// <summary>
        /// Идентификатор текущего объекта
        /// </summary>
        public object CurrentPK
        {
            get { return _objectCurrent; }
        }
        /// <summary>
        /// Выделенная строка с данными
        /// </summary>
        public DataRowView CurrentRowView
        {
            get { return _currentRowView; }
            set
            {
                OnPropertyChanged(ref  _currentRowView, value, () => this.CurrentRowView);
                OnPropertyChanged(ref _objectCurrent, GetPK(_currentRowView), () => this.CurrentPK);
            }
        }
        /// <summary>
        /// ViewModel фильтра
        /// </summary>
        public PgTableView.PgTableViewFilterVM FilterVM
        {
            get { return _filterVM; }
        }
        /// <summary>
        /// признак перехода к объекту на карте
        /// </summary>
        public bool IsGoToMap
        {
            get { return _isGoToMap; }
            set { OnPropertyChanged(ref _isGoToMap, value, () => this.IsGoToMap); }
        }
        #endregion Свойства

        #region Коллекции
        /// <summary>
        /// Коллекция колонок
        /// </summary>
        public ObservableCollection<HeaderValue> Columns
        { get { return _columns; } }
        #endregion Коллекции

        #region Конструкторы
        /// <summary>
        /// ViewModel для окна списка объектов
        /// </summary>
        /// <param name="table">Таблица с объектами</param>
        /// <param name="filter">Фильтр который нужно применить для данного окна</param>
        /// <param name="id">Перейти к нужному объекту</param>
        /// <param name="toSelected"> Открыть окно для выбора объекта</param>
        /// <param name="isDebug"> Debug режим </param>
        public PgTableViewVM(AbsM.ITableBaseM table, FilterRelationModel filter = null, object id = null, bool toSelected = false, bool isDebug = false)
        {
            var pgTable = table as PgM.PgTableBaseM;
            if (pgTable == null)
                throw new ArgumentNullException("table");
            _table = pgTable;
            _isReadOnly = _table.IsReadOnly;
            _source = pgTable.Source as PgVM.PgDataRepositoryVM;
            _connect = Source.Connect;
            _sysScheme = "sys_scheme";
            _isDebug = isDebug;
            Title = String.Format("Таблица: \"{0}\"; Источник: \"{2}@{3}:{4} ({5})\"; Тип: \"{1}\"",
                                    pgTable.Text,
                                    _source.Type,
                                    _source.Connect.Database,
                                    _source.Connect.Host,
                                    _source.Connect.Port,
                                    _source.CurrentUser.NameFull);

            _columns = new ObservableCollection<HeaderValue>();
            sqlSelect = GetSQL();

            _filterVM = new PgTableView.PgTableViewFilterVM(this, filter);

            if (id != null)
            {
                int? page = GetPageInTable(id);
                if (page != null)
                {
                    PageCurrent = page;
                }
            }
            GetColumns();
            Reload();
            if (id != null)
            {
                CurrentRowView = GetRow(id);
            }
            _toSelected = toSelected;

            _source.EventUpdateAttribute += _source_EventUpdateAttribute;
            this.PropertyChanged += PgTableViewVM_PropertyChanged;
        }
        #endregion Конструкторы 

        #region Команды
        #region ReloadCommand
        /// <summary>
        /// Команда обнавления списка объектов
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        /// <summary>
        /// Команда обнавления списка объектов
        /// </summary>
        public void Reload(object obj = null)
        {
            if (!CanReload(obj))
                return;

            int? pageNum = _pageCurrent;
            if (obj is string)
            {
                string value = (string)obj;
                switch (value)
                {
                    case "first": pageNum = 1; break;
                    case "previous": pageNum = _pageCurrent - 1; break;
                    case "next": pageNum = _pageCurrent + 1; break;
                    case "last": pageNum = _tablePages; break;
                    case "check": pageNum = (_pageCurrent == null) ? 1 : (int?)null; break;
                    default:
                        break;
                }
            }
            if (obj is int)
            {
                pageNum = (int)obj;
            }
            if (1 > pageNum)
                return;

            int tableRows;
            int tablePages;
            int pageShowRows;
            Data = GetData(Data, pageNum, out tableRows, out tablePages, out pageShowRows);
            TableRows = tableRows;
            TablePages = tablePages;
            PageShowRows = pageShowRows;
            PageCurrent = pageNum;
        }
        private bool CanReload(object obj = null)
        {
            if (obj is string)
            {
                string value = (string)obj;
                switch (value)
                {
                    case "first": return ShowAllRows == false && _pageCurrent > 1;
                    case "previous": return ShowAllRows == false && _pageCurrent > 1;
                    case "next": return ShowAllRows == false && _tablePages - _pageCurrent > 0;
                    case "last": return ShowAllRows == false && _tablePages - _pageCurrent > 0;
                    case "check": return true;
                    default:
                        break;
                }
            }
            else if (obj is int)
            {
                int valueI = (int)obj;
                if (ShowAllRows)
                    return true;
                return (0 < valueI && valueI <= _tablePages);
            }
            return true;
        }
        #endregion ReloadCommand

        #region DeleteObjectCommand
        /// <summary>
        /// Удалить объект 
        /// </summary>
        public ICommand DeleteObjectCommand
        {
            get { return _deleteObjectCommand ?? (_deleteObjectCommand = new RelayCommand(this.DeleteObject, this.CanDeleteObject)); }
        }
        /// <summary>
        /// Удалить объект 
        /// </summary>
        private void DeleteObject(object obj = null)
        {
            if (!CanDeleteObject(obj))
                return;
            using (SqlWork sqlCmd = new SqlWork(_connect, _isDebug))
            {
                var arrayParams = new IParams[]
                {
                    new Interfaces.Params(":id", obj, PgM.PgFieldM.GetDbType( _table.PrimaryKeyField.Type))
                };
                sqlCmd.sql = string.Format(@"
                        DELETE FROM 
                            {0}.{1} 
                        WHERE 
                            {2} = :id;
                        ",
                    _table.SchemeName,
                    _table.Name,
                    _table.PrimaryKey);

                sqlCmd.ExecuteNonQuery(arrayParams);
            }
            Source.SetEventAttribute(_table, obj, PgM.PgAttributes.attributeTypeChange.Delete);
        }
        private bool CanDeleteObject(object obj = null)
        {
            return !IsReadOnly && obj != null;
        }
        #endregion // DeleteObjectCommand

        #region OpenObjectCommand
        /// <summary>
        /// Открыть окно атрибутов объекта
        /// </summary>
        public ICommand OpenObjectCommand
        {
            get { return _openObjectCommand ?? (_openObjectCommand = new RelayCommand(this.OpenObject, this.CanOpenObject)); }
        }
        /// <summary>
        /// Открыть окно атрибутов объекта
        /// </summary>
        private void OpenObject(object obj = null)
        {
            if (!CanOpenObject(obj))
                return;
            Table.Source.OpenObject(Table, obj);
        }
        private bool CanOpenObject(object obj = null)
        {
            return (obj != null);
        }
        #endregion // ReloadRepositoriesCommand

        #region CreateObjectCommand
        /// <summary>
        /// Создать объект с атрибутами
        /// </summary>
        public ICommand CreateObjectCommand
        {
            get { return _createObjectCommand ?? (_createObjectCommand = new RelayCommand(this.CreateObject, this.CanCreateObject)); }
        }
        /// <summary>
        /// Создать объект с атрибутами
        /// </summary>
        private void CreateObject(object obj = null)
        {
            if (!CanCreateObject())
                return;
            Table.Source.OpenObject(Table, null);
        }
        private bool CanCreateObject(object obj = null)
        {
            return !IsReadOnly;
        }
        #endregion // ReloadRepositoriesCommand

        #region ExportCommand
        /// <summary>
        /// Команда для экспорта
        /// </summary>
        public ICommand ExportCommand
        {
            get { return _exportCommand ?? (_exportCommand = new RelayCommand(this.Export, this.CanExport)); }
        }
        /// <summary>
        /// Экспорт данных
        /// </summary>
        private void Export(object parameter = null)
        {
            string fromShp = Table.IsLayer ?
                @"ESRI Shape-файлы|*.shp|MapInfo-файлы|*.tab|Atlas BNA|*.bna|
CSV|*.csv|GeoJSON|*.geojson|Geographic Markup Language (GML)|*.gml|Genering Mapping Tools (GMT)|*.gmt|
INTERLIS 1|*.itf|SQLite|*.sqlite" : "";
            String exportFilesDialogFilter = fromShp + "|TXT файл|*.txt";
            exportFilesDialogFilter = exportFilesDialogFilter.Replace("||", "|");
            exportFilesDialogFilter = exportFilesDialogFilter.Trim('|');

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = exportFilesDialogFilter;
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            FileInfo fi = new FileInfo(sfd.FileName);
            if (fi.Extension.ToLower() == ".txt")
            {
                //TXT
                try
                {
                    var txtExporter = new ImportExport.Exporters.TXTExporter(_table, _connect, fi);
                    txtExporter.Export();
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close();
                    Classes.workLogFile.writeLogFile(ex, true, true);
                }
            }
            var ext = fi.Extension.ToLower();
            if (ext == ".shp" || ext == ".tab" || ext == ".bna" || ext == ".csv" ||
                ext == ".geojson" || ext == ".gml" || ext == ".gmt" || ext == ".itf" || ext == ".sqlite")
            {
                if (ext == ".csv")
                    if (MessageBox.Show("Сохранение файла в эту папку приведет к удалению всего ее содержимого. Продолжить?",
                        "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    else
                    {
                        var di = fi.Directory;
                        di.Delete(true);
                    }
                if (ext == ".gmt")
                    fi = new FileInfo(fi.Directory + "\\gmt_dir\\" + fi.Name);
                if (ext == ".bna" || ext == ".geojson" || ext == ".gml" || ext == ".itf")
                    fi = new FileInfo(fi.Directory + "\\" + fi.Name + "\\" + fi.Name);
                var shpExporter = new ImportExport.Exporters.SHPExporter(_table, fi, _connect);
                shpExporter.Export();
            }
        }
        /// <summary>
        /// Активна ли команда
        /// </summary>
        private bool CanExport(object parameter = null)
        {
            return true;
        }
        #endregion // ExportCommand

        #region ImportCommand
        /// <summary>
        /// Команда для импорта
        /// </summary>
        public ICommand ImportCommand
        {
            get { return _importCommand ?? (_importCommand = new RelayCommand(this.Import, this.CanImport)); }
        }
        /// <summary>
        /// Импорт
        /// </summary>
        private void Import(object parameter = null)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                String importFilesDialogFilter = "";
                string fromShp = _table.IsLayer ?
                @"ESRI Shape-файлы|*.shp|MapInfo-файлы|*.tab|Atlas BNA|*.bna|
CSV|*.csv|GeoJSON|*.geojson|Geographic Markup Language (GML)|*.gml|Genering Mapping Tools (GMT)|*.gmt|
INTERLIS 1|*.itf|SQLite|*.sqlite" : "";
                string fromExcel = (_table.Type == AbsM.ETableType.Data) ? "Файл Excel|*.xls;*.xlsx" : "";
                string fromDBase = (_table.Type == AbsM.ETableType.Data) ? "Файл dBase|*.dbf" : "";
                importFilesDialogFilter = fromDBase + "|" + fromExcel + "|" + fromShp + "|Все файлы|*.*";
                importFilesDialogFilter = importFilesDialogFilter.Replace("||", "|");
                importFilesDialogFilter = importFilesDialogFilter.Trim('|');

                ofd.Filter = importFilesDialogFilter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var iw = new ImportExport.ImportWindow(new FileInfo(ofd.FileName), _table) { Owner = Program.WinMain };
                    iw.ShowDialog();
                }
            }
        }
        /// <summary>
        /// Доступна ли команда
        /// </summary>
        private bool CanImport(object parameter = null)
        {
            return true;
        }
        #endregion // ImportCommand

        #region ChoiceCommand
        /// <summary>
        /// Выбор объекта
        /// </summary>
        public ICommand ChoiceCommand
        {
            get { return _choiceCommand ?? (_choiceCommand = new RelayCommand(this.Choice, this.CanChoice)); }
        }
        /// <summary>
        /// Выбор объекта
        /// </summary>
        private void Choice(object obj = null)
        {
            if (!CanChoice(_currentRowView))
                return;

            var row = _currentRowView as DataRowView;
            if (row == null)
                return;
            if (ToSelected)
                CloseCommand.Execute(row);
            else
                OpenObject(GetPK(row));
        }
        private bool CanChoice(object obj = null)
        { 
            var row = obj as DataRowView;
            if (row == null)
                return false;
            return true;
        }
        #endregion // ChoiceCommand
        #endregion Команды

        #region Методы
        private void GoToObjeect()
        {
            Source.GoToObject(_table, CurrentPK);
        }
        /// <summary>
        /// Получает первичный ключ текущей строки
        /// </summary>
        private object GetPK(DataRowView data)
        {
            if (data == null)
                return null;
            if (!data.Row.Table.Columns.Contains(_table.PrimaryKey))
                return null;
            return data[_table.PrimaryKey];
        }
        private DataRowView GetRow(object pkField)
        {
            if (pkField == null || Data == null)
                return null;
            var rows = Data.FindRows(pkField);
            if (rows.Count() > 0)
                return rows[0];
            return null;
        }
        /// <summary>
        /// Получение SQL запроса со связанными данными
        /// </summary>
        /// <returns></returns>
        private string GetSQL()
        {
            //Генерация SQL запроса
            using (SqlWork sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql = string.Format(@"
                        SELECT 
                            {0}.get_sql_for_table({1});
                        ",
                    _sysScheme,
                    _table.Id);

                return sqlCmd.ExecuteScalar<string>();
            }
        }
        private string GetSQLOrderBy(DataView data)
        {
            if (data != null && !string.IsNullOrEmpty(data.Sort))
                return data.Sort.Replace("[", "\"").Replace("]", "\"");
            else
                return string.Format(@"""{0}""", _table.PrimaryKey);
        }
        private int? GetPageInTable(object id)
        {
            using (var sqlCmd = new SqlWork(_connect, _isDebug))
            {
                var arrayParams = new IParams[]
									   {
										   new Params()
											   {
												   paramName = ":id",
												   type = PgM.PgFieldM.GetDbType(_table.PrimaryKeyField.Type),
												   value = id
											   }
									   };
                sqlCmd.sql = string.Format(@"
                                            SELECT 
                                                row_num
                                            FROM 
                                                (
                                                    SELECT 
                                                        {2} 
                                                    FROM 
                                                        {0}.{1} 
                                                ) As tables
	                                            CROSS JOIN 
                                                    (
                                                        SELECT 
                                                            ARRAY
                                                            (
                                                                SELECT {2} 
			                                                    FROM {0}.{1}  
			                                                    ORDER BY {2}
                                                            ) As array_id
                                                    )  AS table_id
	                                            CROSS JOIN 
                                                        generate_series
                                                                (
                                                                    1, 
                                                                    (
                                                                        SELECT 
                                                                            COUNT(*) 
					                                                    FROM {0}.{1} 
                                                                    )
                                                                ) AS row_num
                                            WHERE 
                                                table_id.array_id[row_num] =  tables.{2} 
                                                AND tables.{2} = :id
                                            ORDER BY 
                                                row_num;",
                                    _table.SchemeName,
                                    _table.Name,
                                    _table.PrimaryKey);
                var value = sqlCmd.ExecuteScalar<int?>(arrayParams);
                if (value != null)
                {
                    return (value.Value / __pageMaxRows) + 1;
                }
                return null;
            }
        }
        private void GetColumns()
        {
            // Создаем список колонок в таблице
            var listGridColumns = new List<HeaderValue>();
            foreach (var item in _table.Fields)
            {
                if (item.Type == AbsM.EFieldType.Geometry)
                    continue;
                var gridColumn = FindGridColumn(item);
                if (gridColumn == null)
                    gridColumn = new HeaderValue
                                    {
                                        Header = item,
                                        Name = item.Name
                                    };
                listGridColumns.Add(gridColumn);
            }
            ExtraFunctions.Sorts.SortList(Columns, listGridColumns);
        }
        /// <summary>
        /// Получаем общее кол-во строк в таблице
        /// </summary>
        private int GetCountRows()
        {
            using (SqlWork sqlCmd = new SqlWork(_connect, _isDebug))
            {
                sqlCmd.sql = String.Format(@"
                                    SELECT 
                                        count(*) 
                                    FROM 
                                        {0}.{1}
                                    ",
                                _table.SchemeName,
                                _table.Name);
                return sqlCmd.ExecuteScalar<int>();
            }
        }
        /// <summary>
        /// Найти колонку в таблице
        /// </summary>
        /// <param name="field">Поле </param>
        /// <returns></returns>
        private HeaderValue FindGridColumn(AbsM.IFieldM field)
        {
            return _columns.FirstOrDefault(f => f.Name == field.Name);
        }
        /// <summary>
        /// Загружает данные с БД
        /// </summary>
        /// <param name="page">Указывает страницу для загрузки или загружать все</param>
        /// <param name="tableRows"></param>
        /// <param name="tablePages"></param>
        /// <param name="pageShowRows"></param>
        /// <returns></returns>
        private DataView GetData(DataView data, int? page, out int tableRows, out int tablePages, out int pageShowRows, FilterRelationModel filterPattern = null)
        {
            tableRows = GetCountRows();
            pageShowRows = 0;
            if (page != null)
            {
                tablePages = tableRows / __pageMaxRows;
                if (tableRows % __pageMaxRows > 0)
                    tablePages++;
            }
            else
                tablePages = 1;
            if (tableRows != 0)
            {
                using (SqlWork sqlCmd = new SqlWork(_connect, _isDebug))
                {
                    List<IParams> listParams = new List<IParams>();
                    string whereDynamic = PgTableViewFilterVM.GetWhere(FilterVM.DynamicFilter, listParams);
                    string whereFixed = PgTableViewFilterVM.GetWhere(FilterVM.FixedFilter, listParams);
                    string whereUpdated = "1 = 1";
                    if (filterPattern != null)
                    {
                        PgTVM.PgTableViewFilterM filterUpdated = new PgTableViewFilterM(FilterVM);
                        FilterVM.SetFilterFromPattern(filterUpdated, filterPattern);
                        whereUpdated = PgTableViewFilterVM.GetWhere(filterUpdated, listParams);
                    }
                    sqlCmd.sql =
                            string.Format(
                                @"
                                    SELECT *
                                    FROM (
                                        SELECT *
                                        FROM (
                                            {0}
                                            ) as gg 
                                        WHERE {1} AND {2} 
                                        ORDER BY {3}
                                        {4}
                                    ) s
                                    WHERE {5}",
                                sqlSelect,
                                whereFixed,
                                whereDynamic,
                                GetSQLOrderBy(data),
                                (page == null) ? "" : string.Format("LIMIT {0} OFFSET {1}", __pageMaxRows, ((int)page - 1) * __pageMaxRows),
                                whereUpdated);

                    data = sqlCmd.ExecuteGetTable(data, listParams);
                }
            }
            if (data != null)
            {
                pageShowRows = data.Table.Rows.Count;
                if (data.Sort == "")
                {
                    data.Sort = _table.PrimaryKey;
                }
            }
            return data;
        }
        #endregion // Методы

        #region Обработчики
        /// <summary>
        /// Обработчик событий обновления атрибута
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _source_EventUpdateAttribute(object sender, PgM.PgAttributes.PgAttributeEventArgs e)
        {
            if (e.Id == null)
                return;
            if (e.Table == Table)
            {
                var rowView = GetRow(e.Id);
                if (e.TypeChange == PgM.PgAttributes.attributeTypeChange.Delete)
                {
                    rowView.Delete();
                    if (rowView != null)
                        TableRows--;
                    PageShowRows--;
                    return;
                }

                if (rowView != null || e.TypeChange == PgM.PgAttributes.attributeTypeChange.Create)
                {
                    if (Data == null)
                    {
                        Reload();
                        return;
                    }
                    else
                    {
                        //Загрузка таблицы 
                        var filter = new FilterRelationModel()
                        {
                            Type = TypeRelation.ELEMENT,
                            Element = new FilterElementModel()
                            {
                                Column = _table.PrimaryKey,
                                Type = TypeOperation.Equal,
                                Value = e.Id.ToString()
                            }
                        };
                        int tmpTableRows, tmpTablePages, tmpTableShowPages;
                        DataView tableChange = GetData(null, PageCurrent, out tmpTableRows, out tmpTablePages, out tmpTableShowPages, filter);
                        if (tableChange == null || tableChange.Table.Rows.Count != 1)
                            return;
                        DataRow row;
                        if (e.TypeChange == PgM.PgAttributes.attributeTypeChange.Create)
                        {
                            row = Data.Table.NewRow();
                            Data.Table.Rows.Add(row);
                            TableRows++;
                            PageShowRows++;
                        }
                        else
                        {
                            row = rowView.Row;
                        }
                        var rowChange = tableChange.Table.Rows;
                        for (int i = 0; i < Data.Table.Columns.Count; i++)
                        {
                            string columnName = Data.Table.Columns[i].ColumnName;
                            if (tableChange.Table.Columns.Contains(columnName))
                                row[columnName] = rowChange[0][columnName];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Обработка события обнавления свойств ViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PgTableViewVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsGoToMap":
                case "CurrentPK":
                    if (IsGoToMap)
                        GoToObjeect();
                    break;
                default:
                    break;
            }
        }

        #region Обработчики AbsVM.WindowViewModelBase_VM
        protected override bool Closing(object obj)
        {
            _source.EventUpdateAttribute -= _source_EventUpdateAttribute;
            mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(_table.NameMap); 
            if(layer!=null)
            {
                layer.Filter = null; 
            }
            return base.Closing(obj);
        }
        #endregion // AbsVM.WindowViewModelBase_VM
        #endregion
    }
}