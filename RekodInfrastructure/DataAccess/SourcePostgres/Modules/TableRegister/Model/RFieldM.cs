using Interfaces;
using NpgsqlTypes;
using Rekod.Controllers;
using Rekod.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model
{
    public class RFieldM: ViewModelBase
    {
        #region Поля
        private RTableM _parentTable;
        private String _name;
        private bool _isSelected;
        private bool _isLoaded = false;
        private string _text;
        private bool _isRegistered;
        private string _description;
        private ERDataType _dataType;
        private int _id;
        private bool _toBeRegistered = true;
        private string _dbTypeName;
        private string _saveErrorText;
        #endregion Поля

        #region Конструкторы
        public RFieldM(RTableM parentTable)
        {
            _parentTable = parentTable;
            PropertyChanged += RFieldM_PropertyChanged;
        } 
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { OnPropertyChanged(ref _isSelected, value, () => this.IsSelected); }
        }
        public String Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        public String Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        public String DbTypeName
        {
            get { return _dbTypeName; }
            set { OnPropertyChanged(ref _dbTypeName, value, () => this.DbTypeName); }
        }
        public RTableM ParentTable
        {
            get { return _parentTable; }
        }
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set {
                OnPropertyChanged(ref _isRegistered, value, () => this.IsRegistered);
                OnPropertyChanged("IsNotRegistered");
            }
        }
        public bool IsNotRegistered
        {
            get { return !IsRegistered; }
        }
        public bool ToBeRegistered
        {
            get { return _toBeRegistered; }
            set { OnPropertyChanged(ref _toBeRegistered, value, () => this.ToBeRegistered); }
        }
        public ERDataType DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                OnPropertyChanged(ref _dataType, value, () => this.DataType);
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                OnPropertyChanged(ref _description, value, () => this.Description);
            }
        }
        public String SaveErrorText
        {
            get { return _saveErrorText; }
            set { OnPropertyChanged(ref _saveErrorText, value, () => this.SaveErrorText); }
        }
        #endregion Свойства

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
                if (IsRegistered)
                {
                    try
                    {
                        using (SqlWork sqlWork = new SqlWork(true))
                        {
                            cti.ThreadProgress.ShowWait();
                            sqlWork.sql =
                                String.Format(
                                    @"UPDATE sys_scheme.table_field_info
                                SET name_map= @name_map, name_lable = @name_lable
                            WHERE id = {0}", Id);
                            sqlWork.ExecuteNonQuery(
                                new List<Params>() {
                                        new Params("@name_map", Text, NpgsqlDbType.Varchar),
                                        new Params("@name_lable", Description, NpgsqlDbType.Varchar) });
                            cti.ThreadProgress.Close();
                            MessageBox.Show(Rekod.Properties.Resources.PgReg_ChangesSaved);
                        }
                    }
                    catch (Exception ex)
                    {
                        cti.ThreadProgress.Close();
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    if (ParentTable.IsRegistered)
                    {
                        try
                        {
                            cti.ThreadProgress.ShowWait();
                            using (SqlWork sqlWork = new SqlWork(true))
                            {
                                sqlWork.sql =
                                    String.Format(
                                        @"INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
                                    VALUES ({0}, '{1}', @name_map, {3}, {4}, @name_lable, {6}, {7});",
                                                        ParentTable.Id, Name, Text, Convert.ToInt32(DataType), true, Description, false, false);
                                sqlWork.ExecuteNonQuery(
                                    new List<Params>() {
                                new Params("@name_map", Text, NpgsqlDbType.Varchar), 
                                new Params("@name_lable", Description, NpgsqlDbType.Varchar) });
                            }
                            using (SqlWork sqlWork = new SqlWork(true))
                            {
                                sqlWork.sql =
                                    String.Format("SELECT id FROM sys_scheme.table_field_info WHERE id_table = {0} AND name_db = '{1}'",
                                        ParentTable.Id,
                                        Name);
                                Id = sqlWork.ExecuteScalar<Int32>();
                            }
                            IsRegistered = true;
                            if (ParentTable.IsSscActive)
                            {
                                Rekod.DBTablesEdit.SyncController.ReloadTable(ParentTable.Id);
                            }
                            cti.ThreadProgress.Close();
                            MessageBox.Show(Rekod.Properties.Resources.PgReg_FieldRegistered);
                        }
                        catch (Exception ex)
                        {
                            cti.ThreadProgress.Close();
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.PgReg_TableNotDescribed);
                    }
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
            if (Text == null || String.IsNullOrEmpty(Text.Trim()))
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_SystemNameNeeded; 
                result = false; 
            }
            if (Convert.ToInt32(DataType) == 0)
            {
                SaveErrorText = Rekod.Properties.Resources.PgReg_FieldTypeCantBeDefined; 
                result = false; 
            }
            if (!Rekod.DBTablesEdit.SyncController.HasRight(ParentTable.Id))
            {
                SaveErrorText = Rekod.Properties.Resources.LIV_NotRightLayer;
                result = false; 
            }
            return result;
        }
        #endregion SaveCommand 
        #endregion Команды

        #region Обработчики
        void RFieldM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (IsSelected)
                {
                    using (SqlWork sqlWork = new SqlWork())
                    {
                        sqlWork.sql =
                            String.Format(
                                @"SELECT tfi.name_db, tfi.name_map, tfi.type_field, tfi.name_lable, tfi.id
                                  FROM sys_scheme.table_field_info tfi, sys_scheme.table_info ti
                                  WHERE ti.id = tfi.id_table 
                                      AND ti.scheme_name = '{0}' 
                                      AND ti.name_db = '{1}' 
	                                  AND tfi.name_db = '{2}';",
                                                _parentTable.ParentScheme.SchemeName,
                                                _parentTable.Name,
                                                Name);
                        sqlWork.ExecuteReader();
                        if (sqlWork.CanRead())
                        {
                            Text = sqlWork.GetString("name_map");
                            Description = sqlWork.GetString("name_lable");
                            DataType = sqlWork.GetValue<ERDataType>("type_field");
                            Id = sqlWork.GetInt32("id");
                        }
                    }
                }
            }
        }
        #endregion Обработчики
    }
}