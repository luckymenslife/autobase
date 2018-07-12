using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Interfaces;
using Rekod.Behaviors;
using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.View;
using Rekod.ImportExport;
using Rekod.ImportExport.Importers;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.Properties;
using Rekod.Services;
using Rekod.DBLayerForms;
using RESTLib.Model.REST;
using Rekod.DataAccess.SourcePostgres.View.ConfigView;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgImportTableVM : ViewModelBase
    {
        #region Поля
        private FileInfo _fileInfo;
        private String _scheme;
        private String _nameDB;
        private String _nameSystem;
        private AbsM.ETableType _tableType;
        private AbsM.EGeomType _geomType;
        private int _srid;
        private DataTable _dataTable;
        private Importer _importer;
        private ImportTableV _importWindow;
        private tablesInfo _tableInfo;
        private bool _knownGeometry;
        private string _progressKey;
        private bool _canLoad;
        private Group _group;
        #endregion Поля

        #region Свойства
        /// <summary>
        /// Импортируемый файл
        /// </summary>
        public String FileName
        {
            get { return _fileInfo.FullName; }
        }

        /// <summary>
        /// Схема таблицы
        /// </summary>
        public String Scheme
        {
            get { return _scheme; }
            set
            {
                OnPropertyChanged(ref _scheme, value, () => this.Scheme);
                OnPropertyChanged("NeedGroup");
            }
        }

        /// <summary>
        /// Имя таблицы в базе данных
        /// </summary>
        public String NameDB
        {
            get { return _nameDB; }
            set { OnPropertyChanged(ref _nameDB, value, () => this.NameDB); }
        }

        /// <summary>
        /// Системное имя таблицы
        /// </summary>
        public String NameSystem
        {
            get { return _nameSystem; }
            set { OnPropertyChanged(ref _nameSystem, value, () => this.NameSystem); }
        }

        /// <summary>
        /// Тип таблицы
        /// </summary>
        public AbsM.ETableType TableType
        {
            get { return _tableType; }
            set
            {
                OnPropertyChanged(ref _tableType, value, () => this.TableType);
                OnPropertyChanged("NeedGeometry");
                OnPropertyChanged("NeedGroup");
            }
        }

        /// <summary>
        /// Не занят ли процесс
        /// </summary>
        public bool CanLoad
        {
            get { return _canLoad; }
            set { OnPropertyChanged(ref _canLoad, value, () => this.CanLoad); }
        }

        /// <summary>
        /// Тип геометрии
        /// </summary>
        public AbsM.EGeomType GeomType
        {
            get { return _geomType; }
            set { OnPropertyChanged(ref _geomType, value, () => this.GeomType); }
        }

        /// <summary>
        /// Группа (в MapAdmin)
        /// </summary>
        public Group Group
        {
            get { return _group; }
            set { OnPropertyChanged(ref _group, value, () => this.Group); }
        }

        public int? SystemGroup
        {
            get
            {
                if (Program.group_info != null && Group != null)
                {
                    var group = Program.group_info.FirstOrDefault(w => w.name == Group.name);
                    if (group.id != 0)
                        return group.id;
                }
                return null;
            }
        }

        /// <summary>
        /// Должен ли пользователь явно указать тип геометрии
        /// </summary>
        public bool NeedGeometry
        {
            get { return !_knownGeometry && _tableType == AbsM.ETableType.MapLayer; }
        }

        public bool NeedGroup
        {
            get { return Program.mapAdmin != null && TableType == AbsM.ETableType.MapLayer && Scheme == "data"; }
        }

        /// <summary>
        /// Таблица данных
        /// </summary>
        public DataView DataTable
        {
            get { return _dataTable != null ? _dataTable.DefaultView : null; }
        }

        /// <summary>
        /// Информация о таблице (после создания)
        /// </summary>
        public tablesInfo TableInfo
        {
            get { return _tableInfo; }
        }

        public int RowCount
        {
            get { return _importer.RowsCount; }
        }

        /// <summary>
        /// Можно ли создать таблицу
        /// </summary>
        public bool CanCreate
        {
            get
            {
                string errors = "";
                if (string.IsNullOrEmpty(Scheme))
                    errors += "выбрать схему, ";
                if (string.IsNullOrEmpty(NameDB))
                    errors += "ввести имя таблицы в базе, ";
                if (string.IsNullOrEmpty(NameSystem))
                    errors += "ввести системное имя таблицы, ";
                if (TableType == AbsM.ETableType.CommonType)
                    errors += "выбрать тип таблицы, ";
                if (_geomType == 0 && _tableType == AbsM.ETableType.MapLayer)
                    errors += "выбрать тип геометрии, ";
                if (NeedGroup && Group == null)
                    errors += "выбрать группу, ";

                if (errors != "")
                {
                    MessageBox.Show("Для сохранения таблицы необходимо " + errors.Remove(errors.Length - 2), "Невозможно создать таблицу", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
        }

        #endregion Свойства

        #region Коллекции
        public List<String> SchemeNames
        {
            get { return Program.schems; }
        }
        public Dictionary<String, AbsM.ETableType> TableTypes
        {
            get
            {
                return new Dictionary<string, AbsM.ETableType>() { 
                    {Resources.AbsM_ETableType_MapLayer, AbsM.ETableType.MapLayer}, 
                    {Resources.AbsM_ETableType_Catalog, AbsM.ETableType.Catalog}, 
                    {Resources.AbsM_ETableType_Interval, AbsM.ETableType.Interval}, 
                    {Resources.AbsM_ETableType_Data, AbsM.ETableType.Data}
                };
            }
        }

        public List<Group> Groups
        {
            get
            {
                if (Program.mapAdmin != null)
                {
                    return Program.mapAdmin.SscGroups;
                }
                else
                {
                    return null;
                }

            }
        }
        #endregion Коллекции

        #region Конструктор
        public PgImportTableVM(FileInfo fileInfo, ImportTableV importWindow)
        {
            _fileInfo = fileInfo;
            _importWindow = importWindow;
            _tableInfo = null;
            _dataTable = null;
            _geomType = 0;
            _progressKey = "importProgress" + DateTime.Now.Ticks.ToString();
            _srid = -1;
            CanLoad = true;

            switch (_fileInfo.Extension.ToLower())
            {
                case ".dbf":
                    _importer = new DBFImporter();
                    break;
                case ".xls":
                case ".xlsx":
                    _importer = new ExcelImporter();
                    break;
                case ".shp":
                case ".tab":
                case ".bna":
                case ".csv":
                case ".geojson":
                case ".gml":
                case ".gmt":
                case ".itf":
                case ".kml":
                case ".mif":
                case ".xml":
                case ".gxt":
                case ".dxf":
                case ".sqlite":
                    _importer = new SHPImporter();
                    break;
                default:
                    _importer = null;
                    break;
            }

            if (_importer == null)
            {
                MessageBox.Show(Rekod.Properties.Resources.ImportWindow_NotSupportExtension);
                return;
            }
            _importer.WorkerSupportsCancellation = true;
            _importer.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(importer_ProgressChanged);
            _importer.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(importer_RunWorkerCompleted);

            try
            {
                _importer.Init(_fileInfo, _tableInfo);
                _dataTable = _importer.GetPreviewTable();
                if (_importer is SHPImporter)
                {
                    _geomType = (AbsM.EGeomType)((SHPImporter)_importer).GetGeometryType();
                    _srid = ((SHPImporter)_importer).GetSRID();
                }
            }
            catch (Exception e)
            {
                cti.ThreadProgress.Close(_progressKey);
                MessageBox.Show(Properties.Resources.LabelControl_ErrorShowResult + Environment.NewLine + e.Message, Properties.Resources.error, MessageBoxButton.OK, MessageBoxImage.Error);

                _importer.CancelAsync();
                _importer.Dispose();
                return;
            }

            if (_dataTable == null)
            {
                MessageBox.Show(Properties.Resources.LabelControl_ErrorShowResult);
                _importer.CancelAsync();
                _importer.Dispose();
                return;
            }

            _knownGeometry = _geomType != 0;
        }
        #endregion Конструктор

        #region Методы
        public static List<T> FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            List<T> foundChild = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild.AddRange(FindChild<T>(child, childName));
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild.Add((T)child);
                    }
                    else if (frameworkElement != null)
                        // recursively drill down the tree
                        foundChild.AddRange(FindChild<T>(child, childName));
                }
                else
                {
                    // child element found.
                    foundChild.Add((T)child);
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Создание таблицы
        /// </summary>
        /// <returns>Информация о таблице</returns>
        public tablesInfo CreateTable()
        {
            bool exist_table = false;
            bool find_name = false;
            string name_table = _nameDB;
            int countt = 0;

            if (classesOfMetods.isReservedName(name_table))
            {
                cti.ThreadProgress.Close(_progressKey);
                name_table += "_";
                if (MessageBox.Show(
                    string.Format(Resources.AET_NameIsReserved + Environment.NewLine + Resources.AET_SaveTableRename, name_table),
                    Resources.AET_CreatingTable, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    MessageBox.Show(Resources.AET_ConNotSave + Environment.NewLine +
                        Resources.AET_ChangeTable, Resources.AET_CreatingTable, MessageBoxButton.OK, MessageBoxImage.Information);
                    return null;
                }
            }

            while (!find_name)
            {
                find_name = true;
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT count(*) FROM pg_tables WHERE tablename='" + name_table + "' AND schemaname='" + _scheme + "';";
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        if (sqlCmd.GetInt32(0) > 0)
                        {
                            exist_table = true;
                            find_name = false;
                            countt++;
                        }
                    }
                    if (!find_name)
                        name_table = _nameDB + countt.ToString();
                }
            }
            if (exist_table)
            {
                cti.ThreadProgress.Close(_progressKey);
                if (MessageBox.Show(
                    string.Format(Resources.AET_AlreadyExistsTable + Environment.NewLine + Resources.AET_SaveTableRename, name_table),
                    Resources.AET_CreatingTable, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    MessageBox.Show(Resources.AET_ConNotSave + Environment.NewLine +
                        Resources.AET_ChangeTable, Resources.AET_CreatingTable, MessageBoxButton.OK, MessageBoxImage.Information);
                    return null;
                }
                cti.ThreadProgress.ShowWait(_progressKey);
            }

            int id = -1;
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT " + Program.scheme + ".create_table(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9)";
                var parms = new IParams[]
                                        {
                                            new Params
                                                {
                                                    paramName = "@val1",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = _scheme
                                                },
                                            new Params
                                                {
                                                    paramName = "@val2",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = name_table
                                                },
                                            new Params
                                                {
                                                    paramName = "@val3",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = _nameSystem
                                                },
                                            new Params
                                                {
                                                    paramName = "@val4",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = (int)_tableType
                                                },
                                            new Params
                                                {
                                                    paramName = "@val5",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = _geomType
                                                },
                                            new Params
                                                {
                                                    paramName = "@val6",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = _srid != -1 ? _srid : 4326
                                                },
                                            new Params
                                                {
                                                    paramName = "@val7",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = false
                                                },
                                            new Params
                                                {
                                                    paramName = "@val8",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = false
                                                },
                                            new Params
                                                {
                                                    paramName = "@val9",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = SystemGroup
                                                }
                                        };
                try
                {
                    sqlCmd.ExecuteReader(parms);
                    if (sqlCmd.CanRead())
                        id = sqlCmd.GetInt32(0);
                    else
                        throw new Exception(Resources.AET_ErrorCreatingTable);
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close(_progressKey);
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            var cls = new classesOfMetods();
            Program.tables_info = cls.loadTableInfo();
            var ti = classesOfMetods.getTableInfo(id);
            return ti;
        }

        /// <summary>
        /// Создание поля таблицы
        /// </summary>
        /// <param name="nameDB">Название поля в БД</param>
        /// <param name="nameSystem">Системное название поля</param>
        /// <param name="name">Название поля в файле данных</param>
        /// <param name="ti">Информация о таблице</param>
        public FieldMatch CreateField(string nameDB, string nameSystem, int fieldTypeID, string name, tablesInfo ti)
        {
            if (nameDB != ti.pkField)
            {
                try
                {
                    bool existField = false;
                    string nameField = nameDB;
                    int countt = 0;
                    var fieldNames = new List<string>();

                    if (classesOfMetods.isReservedName(nameField))
                    {
                        cti.ThreadProgress.Close(_progressKey);
                        nameField += "_";
                        if (MessageBox.Show(Resources.AEAtr_ReservedName + Environment.NewLine + String.Format(Resources.AEAtr_SaveRenameField, nameField),
                            Resources.AEAtr_CreatingAttribute, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            MessageBox.Show(Resources.AEAtr_NotSaveField, Resources.AEAtr_CreatingAttribute,
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            cti.ThreadProgress.ShowWait(_progressKey);
                            return null;
                        }
                        cti.ThreadProgress.ShowWait(_progressKey);
                    }

                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "SELECT name_db FROM " + Program.scheme + ".table_field_info WHERE id_table=" + ti.idTable.ToString();
                        sqlCmd.ExecuteReader();
                        while (sqlCmd.CanRead())
                        {
                            fieldNames.Add(sqlCmd.GetString(0));
                        }
                        sqlCmd.Close();//считали все поля таблицы с базы и занеесли в лист
                        while (fieldNames.Contains(nameField))
                        {
                            existField = true;
                            countt++;
                            nameField = nameDB + countt.ToString();//если такое имя уже существует пробуем другое имя
                        }
                        if (existField)
                        {
                            cti.ThreadProgress.Close(_progressKey);
                            if (MessageBox.Show(String.Format(Resources.AEAtr_SaveRenameField, nameField),
                                Resources.AEAtr_CreatingAttribute, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            {
                                MessageBox.Show(Resources.AEAtr_NotSaveField, Resources.AEAtr_CreatingAttribute,
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                cti.ThreadProgress.ShowWait(_progressKey);
                                return null;
                            }
                            cti.ThreadProgress.ShowWait(_progressKey);
                        }
                    }

                    int? id_f = null;
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "SELECT " + Program.scheme + ".create_field(" + ti.idTable.ToString() + ",@field_name_db,@field_name_map," + fieldTypeID + ",@field_name_lable)";
                        var parms = new Params[]
                                            {
                                                new Params
                                                    {
                                                        paramName = "@field_name_db",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = nameField
                                                    },
                                                new Params
                                                    {
                                                        paramName = "@field_name_map",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = nameSystem
                                                    },
                                                new Params
                                                    {
                                                        paramName = "@field_name_lable",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = null
                                                    }
                                            };

                        sqlCmd.ExecuteReader(parms);
                        if (sqlCmd.CanRead())
                        {
                            id_f = sqlCmd.GetInt32(0);
                        }
                        if (id_f != null)
                        {
                            return new FieldMatch()
                            {
                                Dest = (new Interfaces.fieldInfo()
                                {
                                    nameDB = nameField,
                                    TypeField = (Interfaces.TypeField)fieldTypeID
                                }),
                                Src = name
                            };
                        }
                        else return null;
                    }
                }
                catch (Exception ex)
                {
                    Rekod.Classes.workLogFile.writeLogFile(Resources.AEAtr_ErrorCreateAtrField + " " +
                        Resources.DGBH_ErrorHeader + ": " + ex.Message, true, true);
                    return null;
                }
            }
            else
            {
                return null;
                //return new FieldMatch()
                //{
                //    Dest = (new Interfaces.fieldInfo()
                //    {
                //        nameDB = nameDB,
                //        TypeField = TypeField.Integer
                //    }),
                //    Src = name
                //};
            }
        }

        /// <summary>
        /// Закрыть окно импорта (view)
        /// </summary>
        private void CloseWindow()
        {
            _importWindow.Close();
        }

        private int getType(Type type)
        {
            if (type == typeof(int))
                return 1;
            else if (type == typeof(string))
                return 2;
            else if (type == typeof(DateTime))
                return 4;
            else if (type == typeof(double) || type == typeof(float))
                return 6;
            else return 2;
        }
        #endregion Методы

        #region Обработчики

        void importer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            cti.ThreadProgress.SetText(string.Format(Rekod.Properties.Resources.ImportWindow_StatusProcess, (int)e.UserState + 1, _importer.RowsCount));
        }

        void importer_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            cti.ThreadProgress.Close(_progressKey);
            CloseWindow();
        }

        #endregion Обработчики

        #region Команды
        #region ResizeColumnsCommand
        private ICommand _resizeColumnsCommand;
        public ICommand ResizeColumnsCommand
        {
            get { return _resizeColumnsCommand ?? (_resizeColumnsCommand = new RelayCommand(ResizeColumns)); }
        }
        public void ResizeColumns(object parameter = null)
        {
            DataGrid importGrid = ((Rekod.Behaviors.CommandEventParameter)parameter).CommandParameter as DataGrid;

            for (int i = 0; i < importGrid.Columns.Count; i++)
            {
                FindChild<TextBox>(importGrid, "headerSystemName")[i + 1].Text = (string)importGrid.Columns[i].Header;
                FindChild<ComboBox>(importGrid, "headerFieldTypeCb")[i + 1].SelectedIndex = getType(_dataTable.Columns[i].DataType) - 1;
            }

            foreach (var column in importGrid.Columns)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                column.MinWidth = 50;
            }
        }
        #endregion ResizeColumnsCommand

        #region CreateCommand
        private ICommand _createCommand;
        public ICommand CreateCommand
        {
            get { return _createCommand ?? (_createCommand = new RelayCommand(Create)); }
        }
        public void Create(object parameter = null)
        {
            if (CanCreate)
            {
                cti.ThreadProgress.ShowWait(_progressKey);
                try
                {
                    CanLoad = false;
                    _tableInfo = CreateTable();

                    if (_tableInfo != null)
                    {
                        DataGrid importGrid = parameter as DataGrid;
                        var headers = FindChild<StackPanel>(importGrid, "HeaderStackPanel");

                        List<FieldMatch> fields = new List<FieldMatch>();
                        for (int i = 0; i < headers.Count - 1; i++)
                        {
                            bool loadField = (bool)FindChild<CheckBox>(importGrid, "LoadField")[i + 1].IsChecked;
                            if (!loadField)
                                continue;
                            string nameDB = FindChild<TextBox>(importGrid, "headerDBName")[i + 1].Text;
                            string nameSystem = FindChild<TextBox>(importGrid, "headerSystemName")[i + 1].Text;
                            string name = (string)importGrid.Columns[i].Header;
                            var type_item = FindChild<ComboBox>(importGrid, "headerFieldTypeCb")[i + 1].SelectedValue;
                            EnumWrapper enumWrapper = type_item as EnumWrapper;
                            AbsM.EFieldType eFieldType = (AbsM.EFieldType)enumWrapper.Value;

                            if (loadField)
                            {
                                var field = CreateField(nameDB, nameSystem, (int)eFieldType, name, _tableInfo);
                                if (field != null)
                                    fields.Add(field);
                            }
                        }

                        var cls = new classesOfMetods();
                        Program.field_info = cls.loadFieldInfo();

                        _importer.SetTableInfo(_tableInfo);
                        if (_importer is SHPImporter)
                            (_importer as SHPImporter).Load(fields, _tableType == AbsM.ETableType.MapLayer);
                        else _importer.Load(fields);
                        if (Program.sscUser != null)
                        {
                            DBTablesEdit.SyncController.RegisterTable(_tableInfo.idTable, Group.name);
                        }
                    }
                    else
                    {
                        cti.ThreadProgress.Close(_progressKey);
                        CanLoad = true;
                    }
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close(_progressKey);
                    CanLoad = true;
                    MessageBox.Show(ex.Message, Resources.AET_CreatingTable, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        #endregion CreateCommand

        #region CancelCommand
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); }
        }
        public void Cancel(object parameter = null)
        {
            CloseWindow();
        }
        #endregion CancelCommand
        #endregion Команды
    }
}
