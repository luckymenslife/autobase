using Interfaces;
using NpgsqlTypes;
using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Converters;
using Rekod.Services;
using RESTLib.Model.REST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model
{
    public class RTableM : ViewModelBase
    {
        #region Поля
        private String _name;
        private ObservableCollection<RFieldM> _fields;
        private bool _isSelected;
        private bool _isLoaded = false;
        private RSchemeM _parentScheme;
        private bool _isRegistered;
        private String _text;
        private RFieldM _pkField;
        private RFieldM _geomField;
        private RFieldM _labelField;
        private ERTableType _tableType;
        private ERGeomType _geomType = ERGeomType.None;
        private bool _hasFiles;
        private bool _isMapStyle;
        private bool _isMapLayer;
        private int _id;
        private String _saveErrorText;
        private DbDataTypeToSystemTypeConverter _typeConverter = new DbDataTypeToSystemTypeConverter();
        private Group _sscGroup;
        #endregion Поля

        #region Конструкторы
        public RTableM(RSchemeM parentScheme)
        {
            _parentScheme = parentScheme;
            PropertyChanged += RTableM_PropertyChanged;
            MapAdmin = Program.mapAdmin;
        }
        #endregion Конструкторы

        #region Свойства
        public sscSync.Controller.MapAdmin MapAdmin
        {
            get;
            private set;
        }
        public Group SscGroup
        {
            get { return _sscGroup; }
            set
            {
                OnPropertyChanged(ref _sscGroup, value, () => this.SscGroup);
            }
        }
        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        public String Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        public String Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        public RSchemeM ParentScheme
        {
            get { return _parentScheme; }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { OnPropertyChanged(ref _isSelected, value, () => this.IsSelected); }
        }
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                OnPropertyChanged(ref _isRegistered, value, () => this.IsRegistered);
                OnPropertyChanged("IsNotRegistered");
            }
        }
        public bool IsNotRegistered
        {
            get { return !IsRegistered; }
        }
        public ERTableType TableType
        {
            get { return _tableType; }
            set { OnPropertyChanged(ref _tableType, value, () => this.TableType); }
        }
        public ERGeomType GeomType
        {
            get { return _geomType; }
            set { OnPropertyChanged(ref _geomType, value, () => this.GeomType); }
        }
        public bool HasFiles
        {
            get { return _hasFiles; }
            set { OnPropertyChanged(ref _hasFiles, value, () => this.HasFiles); }
        }
        public bool IsMapStyle
        {
            get { return _isMapStyle; }
            set { OnPropertyChanged(ref _isMapStyle, value, () => this.IsMapStyle); }
        }
        public bool IsSscActive
        {
            get
            {
                return (MapAdmin != null);
            }
        }
        public RFieldM PkField
        {
            get { return _pkField; }
            set { OnPropertyChanged(ref _pkField, value, () => this.PkField); }
        }
        public RFieldM GeomField
        {
            get { return _geomField; }
            set { OnPropertyChanged(ref _geomField, value, () => this.GeomField); }
        }
        public RFieldM LabelField
        {
            get { return _labelField; }
            set { OnPropertyChanged(ref _labelField, value, () => this.LabelField); }
        }
        public bool IsMapLayer
        {
            get { return _isMapLayer; }
            set { OnPropertyChanged(ref _isMapLayer, value, () => this.IsMapLayer); }
        }
        public String SaveErrorText
        {
            get { return _saveErrorText; }
            set { OnPropertyChanged(ref _saveErrorText, value, () => this.SaveErrorText); }
        }
        #endregion Свойства

        #region Коллекции
        public ObservableCollection<RFieldM> Fields
        {
            get { return _fields ?? (_fields = new ObservableCollection<RFieldM>()); }
        }
        #endregion Коллекции

        #region Команды
        #region SaveCommand
        private ICommand _saveCommand;
        /// <summary>
        /// Команда для сохранения
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        /// <summary>
        /// Сохранение
        /// </summary>
        public void Save(object parameter = null)
        {
            if (CanSave())
            {
                cti.ThreadProgress.ShowWait();
                try
                {
                    if (IsRegistered)
                    {
                        using (SqlWork sqlWork = new SqlWork(true))
                        {
                            sqlWork.sql =
                                String.Format(@"UPDATE sys_scheme.table_info 
                                    SET name_map = @name_map, photo = {1}, lablefiled = '{2}', pk_fileld = '{3}',  geom_field = '{4}'
                                    WHERE id = {5};",
                                              Text,
                                              HasFiles,
                                              (LabelField != null) ? LabelField.Name : "",
                                              (PkField != null) ? PkField.Name : "",
                                              (GeomField != null) ? GeomField.Name : "",
                                              Id);
                            sqlWork.ExecuteNonQuery(new List<Params>() { new Params("@name_map", Text, NpgsqlDbType.Varchar) });
                        }
                    }
                    else
                    {
                        using (SqlWork sqlWork = new SqlWork(true))
                        {
                            sqlWork.BeginTransaction();
                            sqlWork.sql = "SELECT nextval('sys_scheme.table_info_id_seq')::integer as val";
                            int idTable = sqlWork.ExecuteScalar<Int32>();
                            sqlWork.CloseReader();
                            sqlWork.sql =
                                String.Format(@"INSERT INTO sys_scheme.table_info 
                                                (id, name_db, name_map, geom_type, type, map_style, scheme_name, photo, pk_fileld, lablefiled, geom_field) 
                                            VALUES ({0}, '{1}',@name_map, {3}, {4}, {5}, '{6}', {7}, '{8}', '{9}', '{10}')",
                                                        idTable,
                                                        Name,
                                                        Text,
                                                        Convert.ToInt32(GeomType),
                                                        Convert.ToInt32(TableType),
                                                        IsMapStyle,
                                                        ParentScheme.SchemeName,
                                                        HasFiles,
                                                        PkField.Name,
                                                        (LabelField != null) ? LabelField.Name : "",
                                                        (GeomField != null) ? GeomField.Name : "");
                            sqlWork.ExecuteNonQuery(new List<Params>() { new Params("@name_map", Text, NpgsqlDbType.Varchar) });
                            if (IsMapStyle && (TableType == ERTableType.Interval || TableType == ERTableType.Catalog))
                            {
                                bool exists_style = false;
                                sqlWork.sql = String.Format("SELECT * FROM {0}.{1} LIMIT 1", ParentScheme.SchemeName, Name);
                                sqlWork.ExecuteReader();
                                if (sqlWork.CanRead())
                                {
                                    for (int i = 0; sqlWork.GetFiealdCount() > i; i++)
                                    {
                                        if (sqlWork.GetFieldName(i) == "exists_style")
                                        {
                                            exists_style = true;
                                            break;
                                        }
                                    }
                                }
                                sqlWork.CloseReader();
                                if (!exists_style)
                                {
                                    MessageBoxResult dr = MessageBox.Show(
                                            Rekod.Properties.Resources.PgReg_NeedAdditionalFields,
                                            Rekod.Properties.Resources.PgReg_StyleCreation, MessageBoxButton.YesNo);
                                    if (dr == MessageBoxResult.Yes)
                                    {
                                        sqlWork.sql =
                                            String.Format("ALTER TABLE {0}.{1} " +
                                                                      "ADD exists_style boolean DEFAULT true," +
                                                                      "ADD fontname character varying DEFAULT 'Map Symbols'," +
                                                                      "ADD fontcolor integer DEFAULT 16711680," +
                                                                      "ADD fontframecolor integer DEFAULT 16711680," +
                                                                      "ADD fontsize integer DEFAULT 12," +
                                                                      "ADD symbol integer DEFAULT 35," +
                                                                      "ADD pencolor integer DEFAULT 16711680," +
                                                                      "ADD pentype integer DEFAULT 2," +
                                                                      "ADD penwidth integer DEFAULT 1," +
                                                                      "ADD brushbgcolor bigint DEFAULT 16711680," +
                                                                      "ADD brushfgcolor integer DEFAULT 16711680," +
                                                                      "ADD brushstyle integer DEFAULT 0," +
                                                                      "ADD brushhatch integer DEFAULT 1;",
                                                                ParentScheme.SchemeName, Name);
                                        sqlWork.ExecuteNonQuery();
                                    }
                                }
                            }
                            // Регистрация полей
                            foreach (var f in Fields)
                            {
                                if (f.ToBeRegistered)
                                {
                                    if (f.DataType == 0)
                                    {
                                        f.DataType = ERDataType.Text;
                                    }

                                    sqlWork.CloseReader();
                                    sqlWork.sql =
                                        String.Format(
                                            @"INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
                                                VALUES ({0}, '{1}', '{2}', {3}, {4}, @name_lable, {6}, {7});",
                                                idTable, f.Name, f.Name, Convert.ToInt32(f.DataType), true, f.Description, false, false);
                                    sqlWork.ExecuteNonQuery(new List<Params>() { new Params("@name_lable", f.Description, NpgsqlDbType.Varchar) });
                                }
                            }
                            sqlWork.CloseReader();
                            if (HasFiles)
                            {
                                bool exist = false;
                                sqlWork.sql =
                                    String.Format("SELECT EXISTS(SELECT * FROM pg_tables WHERE schemaname='{0}' AND tablename='photo_{1}');",
                                        ParentScheme.SchemeName,
                                        Name);
                                exist = sqlWork.ExecuteScalar<Boolean>();
                                if (exist)
                                {
                                    bool est_v_tabl = false;
                                    sqlWork.sql = String.Format("SELECT EXISTS(SELECT id FROM sys_scheme.table_photo_info WHERE id_table = {0});", idTable);
                                    est_v_tabl = sqlWork.ExecuteScalar<Boolean>();
                                    if (!est_v_tabl)
                                    {
                                        sqlWork.sql =
                                            String.Format("INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)" +
                                                                    " VALUES ({0}, 'photo_{1}', 'id_obj', 'file', '{2}');",
                                                                    idTable, Name, PkField.Name);
                                        sqlWork.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    sqlWork.sql = String.Format("SELECT sys_scheme.set_photo_table({0}, true)", idTable);
                                    sqlWork.ExecuteNonQuery();
                                }
                            }
                            else
                            {

                                sqlWork.sql = String.Format("DELETE FROM sys_scheme.table_photo_info WHERE id_table = {0};", Id);
                                sqlWork.ExecuteNonQuery();
                            }
                            sqlWork.sql = String.Format("SELECT sys_scheme.set_table_rights_for_admins({0});", idTable.ToString());
                            sqlWork.ExecuteNonQuery();

                            sqlWork.EndTransaction();
                            IsRegistered = true;
                            foreach (var f in Fields)
                            {
                                if (f.ToBeRegistered)
                                {
                                    f.IsRegistered = true;
                                }
                            }
                            Id = idTable;
                        }
                    }
                    if (IsSscActive)
                    {
                        if (SscGroup != null)
                        {
                            Interfaces.tablesInfo tableInfo =
                                new Interfaces.tablesInfo()
                                {
                                    idTable = Id,
                                    nameDB = Name,
                                    nameMap = Text,
                                    pkField = PkField.Name,
                                    geomFieldName = (GeomField == null) ? null : GeomField.Name,
                                    lableFieldName = (LabelField == null) ? null : LabelField.Name,
                                    TypeGeom = (Interfaces.TypeGeometry)GeomType
                                };
                            tableInfo.ListField = new List<Interfaces.fieldInfo>();
                            foreach (var f in Fields.Where(p => p.IsRegistered))
                            {
                                tableInfo.ListField.Add(new Interfaces.fieldInfo() { nameDB = Name, nameMap = Text });
                            }
                            MapAdmin.RegisterTable(tableInfo, SscGroup);
                        }
                    }
                    cti.ThreadProgress.Close();
                    MessageBox.Show(Rekod.Properties.Resources.PgReg_ChangesSaved);
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close();
                    MessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить
        /// </summary>
        public bool CanSave(object parameter = null)
        {
            bool result = true;
            SaveErrorText = "";
            if ((TableType != ERTableType.Interval && TableType != ERTableType.Catalog) && IsMapStyle)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_OnlyIntervals;
                result = false;
            }
            if (TableType == ERTableType.MapLayer && GeomType == ERGeomType.None)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_MapLayerHasGeom;
                result = false;
            }
            if (IsMapLayer && GeomField == null)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_ChooseGeomField;
                result = false;
            }
            if (PkField == null)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_ChoosePK;
                result = false;
            }
            if (Convert.ToInt32(TableType) == 0)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_ChooseTableType;
                result = false;
            }
            if (Text == null || String.IsNullOrEmpty(Text.Trim()))
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_SystemNameAbsent;
                result = false;
            }
            return result;
        }
        #endregion SaveCommand
        #endregion Команды

        #region Обработчики
        void RTableM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (IsSelected)
                {
                    using (SqlWork sqlWork = new SqlWork())
                    {
                        Fields.Clear();
                        sqlWork.sql = String.Format(
                                        @"SELECT table_schema, table_name, column_name, data_type, 
                                          EXISTS(SELECT 1 FROM sys_scheme.table_info ti, sys_scheme.table_field_info tfi 
	                                         WHERE ti.scheme_name = '{0}' AND ti.name_db = '{1}' 
	                                         AND tfi.id_table = ti.id AND tfi.name_db = information_schema.columns.column_name)::boolean as is_exsist 
                                          FROM information_schema.columns WHERE table_name = '{1}' AND table_schema = '{0}' AND column_name<>'_ResultLabel_' ORDER BY ordinal_position;", _parentScheme.SchemeName, Name);
                        sqlWork.ExecuteReader();
                        while (sqlWork.CanRead())
                        {
                            var dataTypeVal = _typeConverter.Convert(sqlWork.GetString("data_type"), typeof(ERDataType), null, null);
                            RFieldM rField = new RFieldM(this)
                            {
                                Name = sqlWork.GetString("column_name"),
                                IsRegistered = sqlWork.GetBoolean("is_exsist"),
                                DbTypeName = sqlWork.GetString("data_type"),
                                DataType = (ERDataType)dataTypeVal
                            };
                            Fields.Add(rField);
                        }
                    }

                    using (SqlWork sqlWork = new SqlWork())
                    {
                        sqlWork.sql =
                            String.Format(
                                @"SELECT id, name_map, type, geom_type, photo, map_style, lablefiled, pk_fileld, geom_field
                                    FROM sys_scheme.table_info WHERE name_db='{1}' AND scheme_name='{0}'",
                                _parentScheme.SchemeName,
                                Name);
                        sqlWork.ExecuteReader();
                        if (sqlWork.CanRead())
                        {
                            Id = sqlWork.GetInt32("id");
                            Text = sqlWork.GetString("name_map");
                            TableType = sqlWork.GetValue<ERTableType>("type");
                            GeomType = sqlWork.GetValue<ERGeomType>("geom_type");
                            HasFiles = sqlWork.GetBoolean("photo");
                            IsMapStyle = sqlWork.GetBoolean("map_style");
                            String labelFieldName = sqlWork.GetString("lablefiled");
                            String pk_fileld = sqlWork.GetString("pk_fileld");
                            String geom_field = sqlWork.GetString("geom_field");

                            if (!String.IsNullOrEmpty(labelFieldName))
                            {
                                LabelField = Fields.FirstOrDefault(f => f.Name == labelFieldName);
                            }
                            if (!String.IsNullOrEmpty(pk_fileld))
                            {
                                PkField = Fields.FirstOrDefault(f => f.Name == pk_fileld);
                            }
                            if (!String.IsNullOrEmpty(geom_field))
                            {
                                GeomField = Fields.FirstOrDefault(f => f.Name == geom_field);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }

    [TypeResource("PgTableRegisterM.ERTableType")]
    public enum ERTableType
    {
        MapLayer = 1, Catalog, Interval, Data
    }

    [TypeResource("PgTableRegisterM.ERGeomType")]
    public enum ERGeomType
    {
        None = 0, MultiPoint, MultiLinestring, MultiPolygon
    }

    [TypeResource("PgTableRegisterM.ERDataType")]
    public enum ERDataType
    {
        Integer = 1, Text, Date, DateTime, Geometry, Numeric
    }
}