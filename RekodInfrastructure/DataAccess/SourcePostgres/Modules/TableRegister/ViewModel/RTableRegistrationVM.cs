using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model;
using System.Collections.ObjectModel;
using Rekod.Services;
using System.Windows.Input;
using System.Windows;
using Interfaces;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.ViewModel
{
    public class RTableRegistrationVM: WindowViewModelBase_VM
    {
        #region Поля
        private ObservableCollection<RSchemeM> _schems;
        private ObservableCollection<RTableTypeM> _tableTypes;
        private RSchemeM _selectedScheme;
        #endregion Поля

        #region Коллекции
        public ObservableCollection<RSchemeM> Schems
        {
            get { return _schems ?? (_schems = new ObservableCollection<RSchemeM>()); }
        }
        public ObservableCollection<RTableTypeM> TableTypes
        {
            get { return _tableTypes ?? (_tableTypes = new ObservableCollection<RTableTypeM>()); }
        } 
        #endregion Коллекции

        #region Свойства
        public RSchemeM SelectedScheme
        {
            get { return _selectedScheme; }
            set { OnPropertyChanged(ref _selectedScheme, value, () => this.SelectedScheme); }
        } 
        #endregion Свойства

        #region Конструкторы
        public RTableRegistrationVM()
        {
            PropertyChanged += RTableRegistrationVM_PropertyChanged;
            Title = Rekod.Properties.Resources.PgReg_Title;
            ReloadSchems(); 
        }
        #endregion Конструкторы

        #region Методы
        private void ReloadSchems()
        {
            Schems.Clear();
            using (SqlWork sqlWork = new SqlWork())
            {
                sqlWork.sql = @"SELECT catalog_name, schema_name 
                                FROM information_schema.schemata
                                WHERE schema_name not like 'pg_temp_%' 
	                                AND schema_name not like 'pg_toast_temp_%'
	                                AND schema_name not like 'information_schema'
	                                AND schema_name not like 'pg_catalog'
	                                AND schema_name not like 'pg_toast'
                                    AND schema_name not like 'rosreestr'
                                ORDER BY schema_name";
                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    Schems.Add(new RSchemeM(sqlWork.GetString("schema_name")));
                }
            }
            if (Schems.Count > 0)
            {
                SelectedScheme = Schems[0];
            }
        }
        protected override bool Closing(object obj)
        {
            Window window = obj as Window;
            window.Hide();
            return false;
        }
        #endregion Методы

        #region Команды
        #region CreateNewSchemeCommand
        private ICommand _createNewSchemeCommand;
        /// <summary>
        /// Команда для создания новой схемы
        /// </summary>
        public ICommand CreateNewSchemeCommand
        {
            get { return _createNewSchemeCommand ?? (_createNewSchemeCommand = new RelayCommand(this.CreateNewScheme, this.CanCreateNewScheme)); }
        }
        /// <summary>
        /// Создание новой схемы
        /// </summary>
        public void CreateNewScheme(object parameter = null)
        {
            if (CanCreateNewScheme(parameter))
            {
                try
                {
                    String newSchemeName = parameter.ToString().Trim().ToLower();
                    if (newSchemeName.Length > 0)
                    {
                        if (newSchemeName[0] >= 'a' && newSchemeName[0] <= 'z')
                        {
                            bool valid = true;
                            for (int i = 1; i < newSchemeName.Length; i++)
                            {
                                if ((newSchemeName[i] >= 'a' && newSchemeName[i] <= 'z') || (newSchemeName[i] >= '0' && newSchemeName[i] <= '9') || newSchemeName[i] == '_')
                                { }
                                else 
                                {
                                    valid = false;
                                }
                            }
                            if (!valid)
                            {
                                MessageBox.Show("В названии схемы могут присутствовать только латинские символы, цифры и знак подчеркивания");
                            }
                            else
                            {
                                using (SqlWork sqlWork = new SqlWork(true))
                                {
                                    sqlWork.BeginTransaction();
                                    sqlWork.sql = String.Format(@"CREATE SCHEMA {0} " +
                                            "AUTHORIZATION postgres; " +
                                            "GRANT ALL ON SCHEMA public TO postgres; " +
                                            "GRANT ALL ON SCHEMA public TO public; " +
                                            "COMMENT ON SCHEMA public IS 'scheme created by the system';", newSchemeName);
                                    sqlWork.ExecuteNonQuery();
                                    sqlWork.sql = String.Format("INSERT INTO sys_scheme.table_schems(\"name\") VALUES ('{0}');", newSchemeName);
                                    sqlWork.ExecuteNonQuery();
                                    sqlWork.EndTransaction();
                                }
                                ReloadSchems();
                                MessageBox.Show("Новая схема создана", "Создание новое схемы", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Первый символ в названии схемы должен быть латинской буквой");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Вы должны ввести хотя бы один символ в название схемы");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// Можно ли создать новую схему
        /// </summary>
        public bool CanCreateNewScheme(object parameter = null)
        {
            bool result = false;
            if (parameter != null)
            {
                String newSchemeName = parameter.ToString();
                if (!String.IsNullOrEmpty(newSchemeName))
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion CreateNewSchemeCommand 

        #region RegisterSchemeCommand
        private ICommand _registerSchemeCommand;
        /// <summary>
        /// Команда для регистрации схемы
        /// </summary>
        public ICommand RegisterSchemeCommand
        {
            get { return _registerSchemeCommand ?? (_registerSchemeCommand = new RelayCommand(this.RegisterScheme, this.CanRegisterScheme)); }
        }
        /// <summary>
        /// Регистрация схемы
        /// </summary>
        public void RegisterScheme(object parameter = null)
        {
            if (SelectedScheme != null)
            {
                try
                {
                    using (SqlWork sqlWork = new SqlWork(true))
                    {
                        sqlWork.sql =
                            String.Format("INSERT INTO sys_scheme.table_schems(\"name\") VALUES ('{0}');", SelectedScheme.SchemeName);
                        sqlWork.ExecuteNonQuery();
                        MessageBox.Show(@"Схема зарегистрирована!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// Можно ли зарегистрировать схему
        /// </summary>
        public bool CanRegisterScheme(object parameter = null)
        {
            return SelectedScheme != null;
        }
        #endregion RegisterSchemeCommand        

        #region LaunchSuperFixDbCommand
        private ICommand _launchSuperFixDbCommand;
        /// <summary>
        /// Команда для запуска процедуры ремонта БД
        /// </summary>
        public ICommand LaunchSuperFixDbCommand
        {
            get { return _launchSuperFixDbCommand ?? (_launchSuperFixDbCommand = new RelayCommand(this.LaunchSuperFixDb, this.CanLaunchSuperFixDb)); }
        }
        /// <summary>
        /// Запуск процедуры ремонта БД
        /// </summary>
        public void LaunchSuperFixDb(object parameter = null)
        {
            try
            {
                cti.ThreadProgress.ShowWait();
                using (SqlWork sqlWork = new SqlWork(true))
                {
                    sqlWork.sql = 
                        String.Format("SELECT * FROM sys_scheme.super_fix_db();");
                    sqlWork.ExecuteReader();
                    if (sqlWork.CanRead())
                    {
                        if (sqlWork.GetBoolean(0) == false)
                        {
                            MessageBox.Show("Ошибка выполнения функции!", "ERROR FUNCTION", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show("Операция прошла успешно!");
                        }
                    }
                }
                cti.ThreadProgress.Close();
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close();
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Можно ли запустить процедуру ремонта БД
        /// </summary>
        public bool CanLaunchSuperFixDb(object parameter = null)
        {
            return true;
        }
        #endregion // LaunchSuperFixDbCommand
        #endregion Команды

        #region Обработчики
        void RTableRegistrationVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedScheme")
            {
                TableTypes.Clear();
                if (SelectedScheme != null)
                {
                    cti.ThreadProgress.ShowWait();
                    using (SqlWork sqlWork = new SqlWork())
                    {
                        RTableTypeM rTableType = new RTableTypeM() { TypeName = Rekod.Properties.Resources.PgReg_TablesLayersCatalogs };
                        sqlWork.sql = String.Format(
                                        @"SELECT pg.schemaname, pg.tablename, 
                                            EXISTS(SELECT 1 FROM sys_scheme.table_info ti WHERE pg.schemaname = ti.scheme_name AND pg.tablename = ti.name_db ) as is_exsist, 
                                            EXISTS(SELECT 1 FROM geometry_columns WHERE f_table_name = pg.tablename) as is_layer
                                        FROM pg_tables pg WHERE pg.schemaname = '{0}' ORDER BY pg.tablename;", SelectedScheme.SchemeName);
                        sqlWork.ExecuteReader();
                        while (sqlWork.CanRead())
                        {
                            RTableM rTable = new RTableM(SelectedScheme)
                            {
                                Name = sqlWork.GetString("tablename"),
                                IsRegistered = sqlWork.GetBoolean("is_exsist"),
                                TableType = sqlWork.GetBoolean("is_layer") ? ERTableType.MapLayer : ERTableType.Data,
                                IsMapLayer = sqlWork.GetBoolean("is_layer")
                            };
                            rTableType.Tables.Add(rTable);
                        }
                        TableTypes.Add(rTableType);
                    }
                    using (SqlWork sqlWork = new SqlWork())
                    {
                        RTableTypeM rViewType = new RTableTypeM() { TypeName = Rekod.Properties.Resources.PgReg_Views };
                        sqlWork.sql = String.Format(
                                        @"SELECT pg.schemaname, pg.viewname, 
                                            EXISTS(SELECT 1 FROM sys_scheme.table_info ti WHERE pg.schemaname = ti.scheme_name AND pg.viewname = ti.name_db ) as is_exsist, 
                                            EXISTS(SELECT 1 FROM geometry_columns WHERE f_table_name = pg.viewname) as is_layer 
                                        FROM pg_views pg WHERE pg.schemaname like '{0}' ORDER BY pg.viewname;", SelectedScheme.SchemeName);
                        sqlWork.ExecuteReader();
                        while (sqlWork.CanRead())
                        {
                            RTableM rTable = new RTableM(SelectedScheme)
                            {
                                Name = sqlWork.GetString("viewname"),
                                IsRegistered = sqlWork.GetBoolean("is_exsist"),
                                TableType = sqlWork.GetBoolean("is_layer") ? ERTableType.MapLayer : ERTableType.Data,
                                IsMapLayer = sqlWork.GetBoolean("is_layer")
                            };
                            rViewType.Tables.Add(rTable);
                        }
                        TableTypes.Add(rViewType);
                    }
                    cti.ThreadProgress.Close();
                }
            }
        }
        #endregion Обработчики
    }
}