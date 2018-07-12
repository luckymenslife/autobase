using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.IO;
using Rekod.ProjectionSelection;
using System.Data.SQLite;
using Rekod.Repository;
using Rekod.Services;
using System.Data.Common;
using Rekod.Repository.SettingsDB;
using Rekod.Classes;


namespace Rekod
{
    /// <summary>
    /// Логика взаимодействия для WinMain.xaml
    /// </summary>
    public partial class WinMain : Window
    {
        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                window.Closed += window_Closed;
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(window);
            }
        }
        static void window_Closed(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window != null && window.Owner != null)
            {
                window.Owner.Activate();
            }
        }
        public System.Windows.Forms.UserControl MainFrm
        {
            get
            {
                return Program.mainFrm1;
            }
        }
        public WinMain()
        {
            this.Closed += WinMain_Closed;
            //Program.SplashScreen.Close(new TimeSpan(0, 0, 0, 0));
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(Window_Loaded), true);
            Program.WinMain = this;

            if (Program.setting_file != null)
            {
                try
                {
                    CultureInfo d = Program.SettingsXML.LocalParameters.LocaleProgram;
                }
                catch
                {
                    return;
                }
                Rekod.Culture_M.UpdateCulture();
                InitializeComponent();

                if (Program.connString != null)
                {

                    Program.connStringSQLite = "Data Source=" + Program.path_string + "\\settingsdb.db;Version=3;New=False;Compress=True;";
                    CreateSqlLiteDb();
                    try
                    {
                        Program.SettingsDB = SQLiteWork.GetSqLiteContext(Program.connStringSQLite);
                        Program.OgrService.funcGetRefSysName = new Func<int, string>(GetSQLiteRefSysName);
                        cti.ThreadProgress.ShowWait();

                        var form = new mainFrm();
                        if (Program.UserParams.ContainsKey("DefaultOpenTable"))
                        {
                            if (Program.UserParams["DefaultOpenTable"].Length == 1)
                            {
                                var table = Program.UserParams["DefaultOpenTable"];
                                if (classesOfMetods.getTableInfo(Convert.ToInt32(table[0])) != null)
                                {

                                    if (classesOfMetods.getTableRight(Convert.ToInt32(table[0])).read == true)
                                    {
                                        Program.WithMap = false;
                                    }
                                    else
                                    {
                                        Program.WithMap = true;
                                    }
                                }
                                else
                                {
                                    Program.WithMap = true;
                                }
                            }
                            else
                            {
                                Program.WithMap = true;
                            }
                        }
                        else
                        {
                            Program.WithMap = true;
                        }

                        if (Program.UserParams.ContainsKey("DefaultOpenProcesses"))
                        {
                            Program.WithMap = false;
                        }

                        form.SetStart();

                        if (Program.WithMap)
                        {
                            windowsFormsHost1.Child = form;
                        }
                        else
                        {
                            this.HideControlForMap();
                            this.mapborder.Background = SystemColors.ControlBrush;

                            if (Program.UserParams.ContainsKey("DefaultOpenProcesses"))
                            {
                                if (Plugins.ProcessesControl != null)
                                {
                                    windowsFormsHost1.Child = Plugins.ProcessesControl.GetUserControl();
                                }
                            }
                            else
                            {
                                var table = Program.UserParams["DefaultOpenTable"];
                                var tableInfo = classesOfMetods.getTableInfo(Convert.ToInt32(table[0]));
                                System.Windows.Forms.UserControl _uc = null;
                                var uControl = Plugins.GetControl(null, false, tableInfo, null);
                                if (uControl != null)
                                {
                                    _uc = uControl.GetUserControl();
                                }
                                else
                                {
                                    _uc = new UcTableObjects(tableInfo);
                                }
                                windowsFormsHost1.Child = _uc;
                            }
                        }                        

                        form.bManager.TableManager = Program.TablesManager;
                        Program.WinMain.DataContext = form.bManager;
                        Program.TablesManager.OpenWindowEvent += new DataAccess.TableManager.ViewModel.TableManagerVM.OpenWindowEventHandler(TablesManager_OpenWindowEvent);

                        if (!Program.user_info.admin)
                        {
                            MiReportsSeparator.Visibility = System.Windows.Visibility.Collapsed;
                            MiReportsOpen.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        //Если текущий пользователь - не суперпользователь, скрыть пункт меню "Регистрация таблиц"
                        using (SqlWork sqlWork = new SqlWork())
                        {
                            sqlWork.sql = String.Format("SELECT usesuper FROM pg_user WHERE usename='{0}'", Program.user_info.loginUser);
                            if (!sqlWork.ExecuteScalar<Boolean>())
                            {
                                MiManageTablesRegistration.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
# if ars
                    this.Title = String.Format("ГИС Арсенал, v.{1} ({2})", Program.user_info.windowText,
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                        Program.user_info.nameUser);
# endif
# if !ars
                        Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        string verStr = "";
                        verStr += classesOfMetods.GetVersionString;
                        this.Title = String.Format("{0}, v.{1} ({2})", Program.user_info.windowText,
                            verStr,
                            Program.user_info.nameUser);
# endif
#if DEBUG
                        this.Title = this.Title + " " + Program.mainFrm1.axMapLIb1.AboutBuild();
#endif
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                    }
                    finally
                    {
                        cti.ThreadProgress.Close();
                    }
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }

            }
            Program.SplashScreen.Close(new TimeSpan(0, 0, 0));
        }

        private void HideControlForMap()
        {
            this.TableManagerView.Visibility = System.Windows.Visibility.Collapsed;
            this.statusBar.Visibility = System.Windows.Visibility.Collapsed;
            this.newVerMainBar.Visibility = System.Windows.Visibility.Collapsed;
            this.newVerBar.Visibility = System.Windows.Visibility.Collapsed;
            this.bottonBar.Visibility = System.Windows.Visibility.Collapsed;
            this.rightBar.Visibility = System.Windows.Visibility.Collapsed;
            this.leftBar.Visibility = System.Windows.Visibility.Collapsed;
            this.mainBar.Visibility = System.Windows.Visibility.Collapsed;
            this.ManagerColumn.Width = new System.Windows.GridLength(0);
            this.MiFile.Visibility = System.Windows.Visibility.Collapsed;
            this.MiView.Visibility = System.Windows.Visibility.Collapsed;
            this.MiSources.Visibility = System.Windows.Visibility.Collapsed;
            if (Program.user_info.type_user == 2)
                this.MiTools.Visibility = System.Windows.Visibility.Collapsed;
            this.MiAddMapAdminLayer.Visibility = System.Windows.Visibility.Collapsed;
            this.MiRastrLayers.Visibility = System.Windows.Visibility.Collapsed;
            this.MiBaseLayers.Visibility = System.Windows.Visibility.Collapsed;
            this.MiCosmeticLayers.Visibility = System.Windows.Visibility.Collapsed;
            this.MiSettings.Visibility = System.Windows.Visibility.Collapsed;
            this.MiLocations.Visibility = System.Windows.Visibility.Collapsed;
            this.MiSearch.Visibility = System.Windows.Visibility.Collapsed;

            
            this.MiHelp.Visibility = System.Windows.Visibility.Collapsed;
        }
        private string GetSQLiteRefSysName(int srid)
        {
            using (var sqlCmd = new SQLiteWork(false))
            {
                sqlCmd.Sql = "SELECT [ref_sys_name] FROM [spatial_ref_sys] WHERE [srid] = " + srid;
                return sqlCmd.ExecuteScalar<string>();
            }
        }

        void WinMain_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }


        void TablesManager_OpenWindowEvent(object sender, EventArgs e)
        {
            ToolsFrm.RotateFrm frm = new ToolsFrm.RotateFrm();
            frm.button1.Click += new EventHandler(RotateLeft_Click);
            frm.button2.Click += new EventHandler(RotateRight_Click);
            frm.ShowDialog();
        }
        void RotateLeft_Click(object sender, EventArgs e)
        {
            Program.mainFrm1.axMapLIb1.RotateSelected(Convert.ToDouble(((ToolsFrm.RotateFrm)((System.Windows.Forms.Button)sender).Parent).numericUpDown1.Value));
        }
        void RotateRight_Click(object sender, EventArgs e)
        {
            Program.mainFrm1.axMapLIb1.RotateSelected((-1) * Convert.ToDouble(((ToolsFrm.RotateFrm)((System.Windows.Forms.Button)sender).Parent).numericUpDown1.Value));
        }
        void CreateSqlLiteDb()
        {
            string[] sql_ddl = new string[]{ 
@"DROP TABLE IF EXISTS [copy_past_table];",
@"VACUUM;",
@"CREATE TABLE IF NOT EXISTS [spatial_ref_sys] (
    [srid] INTEGER NOT NULL PRIMARY KEY, 
    [auth_name] VARCHAR(256) NOT NULL, 
    [auth_srid] INTEGER NOT NULL, 
    [ref_sys_name] VARCHAR(256), 
    [proj4text] VARCHAR(2048) NOT NULL,
    [sys_proj] BOOLEAN DEFAULT 1);",
@"CREATE TABLE IF NOT EXISTS [sources] (
    [id] INTEGER PRIMARY KEY AUTOINCREMENT, 
    [source_name] TEXT NOT NULL, 
    [source_type] TEXT);",
@"CREATE TABLE IF NOT EXISTS [tables] (
    [id] INTEGER PRIMARY KEY AUTOINCREMENT, 
    [name_table] TEXT NOT NULL, 
    [id_source] INTEGER NOT NULL 
    CONSTRAINT [source_fk] REFERENCES [sources]([id]) ON DELETE CASCADE ON UPDATE CASCADE);",
@"CREATE TABLE IF NOT EXISTS [filters] (
    [id] INTEGER PRIMARY KEY AUTOINCREMENT, 
    [filter_name] TEXT NOT NULL, 
    [where_text] TEXT NOT NULL, 
    [id_table] INTEGER NOT NULL 
    CONSTRAINT [table_fk] REFERENCES [tables]([id]) ON DELETE CASCADE ON UPDATE CASCADE);",
@"CREATE TABLE IF NOT EXISTS [filter_elements] (
    [id] AUTOINC NOT NULL, 
    [field_name] TEXT NOT NULL, 
    [type_operation] INTEGER NOT NULL, 
    [value_str] TEXT, 
    [is_ref] BOOL NOT NULL, 
    [element_code] TEXT NOT NULL,  
    [id_filter] INTEGER NOT NULL 
    CONSTRAINT [filter_fk] REFERENCES [filter]([id]) ON DELETE CASCADE ON UPDATE CASCADE);",
@"CREATE TABLE IF NOT EXISTS [locales] (
    [locale] TEXT NOT NULL, 
    [description] CHAR NOT NULL, 
    [iscurrent] BOOLEAN);",
@"CREATE TRIGGER IF NOT EXISTS fkd_refsys_geocols 
    BEFORE DELETE ON spatial_ref_sys 
    FOR EACH ROW 
    BEGIN 
        SELECT RAISE(ROLLBACK, 'delete on table ''spatial_ref_sys'' violates constraint: ''geometry_columns.srid''') 
        WHERE (SELECT srid FROM geometry_columns WHERE srid = OLD.srid) IS NOT NULL; 
    END;",
@"CREATE TABLE IF NOT EXISTS [geometry_columns] (
    [f_table_name] VARCHAR(256) NOT NULL, 
    [f_geometry_column] VARCHAR(256) NOT NULL, 
    [type] VARCHAR(30) NOT NULL, 
    [coord_dimension] INTEGER NOT NULL, 
    [srid] INTEGER, 
    [spatial_index_enabled] INTEGER NOT NULL);",
@"CREATE TRIGGER IF NOT EXISTS fki_geocols_refsys BEFORE INSERT ON geometry_columns FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'insert on table ''geometry_columns'' violates constraint: ''spatial_ref_sys.srid''') WHERE  NEW.srid IS NOT NULL AND (SELECT srid FROM spatial_ref_sys WHERE srid = NEW.srid) IS NULL; END;",
@"CREATE TRIGGER IF NOT EXISTS fku_geocols_refsys BEFORE UPDATE ON geometry_columns FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'update on table ''geometry_columns'' violates constraint: ''spatial_ref_sys.srid''') WHERE  NEW.srid IS NOT NULL AND (SELECT srid FROM spatial_ref_sys WHERE srid = NEW.srid) IS NULL; END;",
@"CREATE UNIQUE INDEX IF NOT EXISTS idx_geocols ON geometry_columns (f_table_name, f_geometry_column);", 
@"DELETE FROM [geometry_columns] WHERE [f_table_name] = 'copy_past_table';"};
            try
            {
                if (!File.Exists(Program.path_string + "\\settingsdb.db"))
                    Program.connStringSQLite = "Data Source=" + Program.path_string + "\\settingsdb.db;Version=3;New=True;Compress=True;";
                else
                {
                    Program.connStringSQLite = "Data Source=" + Program.path_string + "\\settingsdb.db;Version=3;New=False;Compress=True;";

                }

                using (SQLiteWork sqliteWork = new SQLiteWork(Program.connStringSQLite, true))
                {
                    foreach (var sql in sql_ddl)
                    {
                        sqliteWork.Sql = sql;
                        sqliteWork.ExecuteNonQuery();
                    }
                }
                Program.connStringSQLite = "Data Source=" + Program.path_string + "\\settingsdb.db;Version=3;New=False;Compress=True;";

                CheckProjTable();
                CheckNewColums();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Program.connStringSQLite = "Data Source=" + Program.path_string + "\\settingsdb.db;Version=3;New=False;Compress=True;";
        }
        /// <summary>
        /// Проверка существования новых полей
        /// </summary>
        private void CheckNewColums()
        {
            using (SQLiteWork sqliteWork = new SQLiteWork())
            {
                sqliteWork.Sql = @"SELECT 1 FROM [sqlite_master] WHERE [name] = 'sources' AND [sql] like '%source_type%';";
                sqliteWork.Execute();
                bool exists_source_type = sqliteWork.CanRead();
                sqliteWork.CloseReader();
                if (!exists_source_type)
                {
                    sqliteWork.Sql = @"ALTER TABLE [sources] ADD COLUMN [source_type] TEXT;
UPDATE [sources] SET [source_type] = 'Postgres' WHERE [source_name] like '%@%';";
                    sqliteWork.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Проверка наличия, структуры и содержания таблицы spatial_ref_sys
        /// </summary>
        private void CheckProjTable()
        {
            // проверка существования колонки sys_proj
            using (SQLiteWork sqliteWork = new SQLiteWork())
            {
                sqliteWork.Sql = @"PRAGMA table_info('spatial_ref_sys');";
                sqliteWork.Execute();

                bool exists_col = false;
                while (sqliteWork.CanRead() && !exists_col)
                {
                    if (sqliteWork.GetValue<String>(1) == "sys_proj")
                        exists_col = true;
                    //reader.NextResult();
                }
                sqliteWork.CloseReader();
                if (!exists_col)
                {
                    sqliteWork.Sql = @"ALTER TABLE spatial_ref_sys ADD COLUMN sys_proj BOOLEAN DEFAULT 1;";
                    sqliteWork.ExecuteNonQuery();
                }
            }

            // Загрузка в локальную базу проекций из PostgreSQSL
            using (SQLiteWork sqliteWork = new SQLiteWork())
            {
                sqliteWork.Sql = @"SELECT COUNT(*) FROM [spatial_ref_sys];";
                int localdb_count = sqliteWork.ExecuteScalar<Int32>();
                int record_count = 0;
                using (SqlWork sqlWork = new SqlWork())
                {
                    sqlWork.sql = "SELECT count(*) FROM public.spatial_ref_sys";
                    record_count = sqlWork.ExecuteScalar<Int32>();
                }

                if (localdb_count < 3700)
                {
                    cti.ThreadProgress.ShowWait("insertprojections");
                    using (var sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "SELECT srid, auth_name, auth_srid, srtext, proj4text FROM spatial_ref_sys;";
                        sqlCmd.ExecuteReader();

                        List<Projection> pgProjList = new List<Projection>();
                        while (sqlCmd.CanRead())
                        {
                            pgProjList.Add(new Projection(
                                sqlCmd.GetInt32("srid"),
                                sqlCmd.GetString("auth_name"),
                                sqlCmd.GetInt32("auth_srid"),
                                sqlCmd.GetString("srtext"),
                                sqlCmd.GetString("proj4text"),
                                true));
                        }

                        sqliteWork.Sql = "SELECT srid, auth_name, auth_srid, ref_sys_name, proj4text, sys_proj FROM [spatial_ref_sys];";
                        sqliteWork.Execute();

                        List<Projection> sqliteProjList = new List<Projection>();
                        while (sqliteWork.CanRead())
                        {
                            sqliteProjList.Add(new Projection(
                                sqliteWork.GetValue<Int32>(0),
                                sqliteWork.GetValue<String>(1),
                                sqliteWork.GetValue<Int32>(2),
                                sqliteWork.GetValue<String>(3),
                                sqliteWork.GetValue<String>(4),
                                sqliteWork.GetValue<Boolean>(5)));
                        }
                        sqliteWork.CloseReader();
                        var exceptList = pgProjList.Except<Projection>(sqliteProjList);
                        //cti.ThreadProgress.SetText("Идет загрузка проекций");
                        try
                        {
                            sqliteWork.Sql = "BEGIN;";
                            sqliteWork.ExecuteNonQuery();
                            int counter = 0;
                            int exceptListCount = exceptList.Count();
                            foreach (var item in exceptList)
                            {
                                cti.ThreadProgress.SetText(String.Format(Rekod.Properties.Resources.ImportWindow_StatusProcess, ++counter, exceptListCount));
                                sqliteWork.Sql = String.Format
                                                    (
                                                        @"INSERT INTO [spatial_ref_sys] (srid, auth_name, auth_srid, ref_sys_name, proj4text, sys_proj) VALUES ({0},'{1}',{2},'{3}','{4}', 1);",
                                                        item.Srid, item.Auth_name, item.Auth_srid, item.Srtext.Replace("'", "''"), item.Proj4text, item.Sys_proj
                                                    );
                                sqliteWork.ExecuteNonQuery();
                            }
                            cti.ThreadProgress.SetText(String.Empty);
                            sqliteWork.Sql = "END;";
                            sqliteWork.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                    cti.ThreadProgress.Close("insertprojections");
                }
            }
        }

        private void TbtbScale_KeyDown(object sender, KeyEventArgs e)
        {
            Program.mainFrm1.TbtbScale_KeyDown(sender, e);
        }
        private void TbbApplySRID_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbApplySRID_Click(sender, e);
        }
        private void TbcbSrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Program.mainFrm1.TbcbSrid_SelectionChanged(sender, e);
        }
        private void TbbFullSearch_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbFullSearch_Click(sender, e);
        }
        private void TbtbFullSearch_KeyDown(object sender, KeyEventArgs e)
        {
            Program.mainFrm1.TbtbFullSearch_KeyDown(sender, e);
        }
        private void TbbSnapToLine_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbSnapToLine_CheckedChanged(sender, e);
        }
        private void TbbSnapToPoint_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbSnapToPoint_CheckedChanged(sender, e);
        }
        private void TbtbSelectingObject_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbSelectingObject_Checked(sender, e);
        }
        private void TbtbSelectingObjectsRectangle_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbSelectingObjectsRectangle_Checked(sender, e);
        }
        private void TbtbSelectingObjectPolygon_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbSelectingObjectPolygon_Checked(sender, e);
        }
        private void TbbUnselectingObjects_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbUnselectingObjects_Click(sender, e);
        }
        private void TbtbObjectAddWithMap_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbObjectAddWithMap_Checked(sender, e);
        }
        private void TbbObjectAddWithWKT_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbObjectAddWithWKT_Click(sender, e);
        }
        private void TbtbObjectAddRectange_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbObjectAddRectange_Checked(sender, e);
        }
        private void TbbLayerObjectDelete_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbLayerObjectDelete_Click(sender, e);
        }
        private void TbtbHand_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbHand_Checked(sender, e);
        }
        private void TbtbRuler_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbRuler_Checked(sender, e);
        }
        private void TbbZoomIn_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbZoomIn_Click(sender, e);
        }
        private void TbbZoomOut_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbZoomOut_Click(sender, e);
        }
        private void TbtbZoomInRegion_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbZoomInRegion_Checked(sender, e);
        }
        private void TbtbZoomOutRegion_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbZoomOutRegion_Checked(sender, e);
        }
        private void TbbZoomToObj_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbZoomToObj_Click(sender, e);
        }
        private void TbbZoomToLayer_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbZoomToLayer_Click(sender, e);
        }
        private void TbtbLayerList_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbLayerList_Checked(sender, e);
        }
        private void TbbWorkSets_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbWorkSets_Click(sender, e);
        }
        private void TbbUpdateListLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbUpdateListLayers_Click(sender, e);
        }
        private void TbtbMapPointInfo_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbMapPointInfo_CheckedChanged(sender, e);
        }
        private void TbbRotateBtn_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbRotateBtn_Click(sender, e);
        }
        private void TbtbRotateMouse_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbRotateMouse_Checked(sender, e);
        }
        private void TbtbMoveObj_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbMoveObj_Checked(sender, e);
        }
        private void TbtbVertexEdit_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbVertexEdit_Checked(sender, e);
        }
        private void TbtbJoinGeometry_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbJoinGeometry_Checked(sender, e);
        }
        private void TbtbSeparatGeometry_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbSeparatGeometry_Checked(sender, e);
        }
        private void TbtbIntersectGeometry_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbIntersectGeometry_Checked(sender, e);
        }
        private void TbtbJoinIntersectGeometry_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbJoinIntersectGeometry_Checked(sender, e);
        }
        private void TbtbSymDifference_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbSymDifference_Checked(sender, e);
        }
        private void TbtbClippingPolygonAnotherPolygon_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbClippingPolygonAnotherPolygon_Checked(sender, e);
        }
        private void TbtbClippingPolygonBySpecifyingPoints_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbClippingPolygonBySpecifyingPoints_Checked(sender, e);
        }
        private void TbtbClippingPolygonBySpecifyingLine_Checked(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbtbClippingPolygonBySpecifyingLine_Checked(sender, e);
        }
        private void TbbLayerUp_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbLayerUp_Click(sender, e);
        }
        private void TbbLayerDown_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbLayerDown_Click(sender, e);
        }
        private void TbbWorkSetChangeStyleLayer_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbWorkSetChangeStyleLayer_Click(sender, e);
        }
        private void TbbWorkSetDeleteStyleLayer_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbWorkSetDeleteStyleLayer_Click(sender, e);
        }
        private void TbbWorkSetSaveLocationLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.TbbWorkSetSaveLocationLayers_Click(sender, e);
        }
        private void MiExportPrint_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiExportPrint_Click(sender, e);
        }
        private void MiSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSaveImage_Click(sender, e);
        }
        private void MiChooseVmpFile_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiChooseVmpFile_Click(sender, e);
        }
        private void MiRastrGeoreference_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiRastrGeoreference_Click(sender, e);
        }
        private void MiPrint_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiPrint_Click(sender, e);
        }
        private void MiExit_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiExit_Click(sender, e);
        }
        private void MiManageTables_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiManageTables_Click(sender, e);
        }
        private void MiManageTablesRegistration_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiManageTablesRegistration_Click(sender, e);
        }
        private void MiAddMapAdminLayer_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiAddMapAdminLayer_Click(sender, e);
        }
        private void MiRastrLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiRastrLayers_Click(sender, e);
        }
        private void MiBaseLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiBaseLayers_Click(sender, e);
        }
        private void MiCosmeticLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiCosmeticLayers_Click(sender, e);
        }
        private void MiUserRights_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiUserRights_Click(sender, e);
        }
        private void MiHistory_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiHistory_Click(sender, e);
        }
        private void MiLocations_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiLocations_Click(sender, e);
        }
        private void MiSearch_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSearch_Click(sender, e);
        }
        private void MiFastStart_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiFastStart_Click(sender, e);
        }
        private void MiProgramPlugins_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiProgramPlugins_Click(sender, e);
        }
        private void MiSrcBaseLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSrcBaseLayers_Click(sender, e);
        }
        private void MiSrcRastrLayers_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSrcRastrLayers_Click(sender, e);
        }
        private void MiSrcPostgreSQL_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSrcPostgreSQL_Click(sender, e);
        }
        private void MiSrcShowPanel_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiSrcShowPanel_Click(sender, e);
        }
        private void MiManual_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiManual_Click(sender, e);
        }
        private void MiLicences_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiLicences_Click(sender, e);
        }
        private void MiAbout_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiAbout_Click(sender, e);
        }
        private void MiTb_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Program.mainFrm1 != null)
            {
                Program.mainFrm1.MiTb_CheckedChanged(sender, e);
            }
        }
        private void MiMapFitIn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiMapFitIn_CheckedChanged(sender, e);
        }
        private void MiShowObjectForm_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiShowObjectForm_CheckedChanged(sender, e);
        }
        private void MiTbRestoreStandard_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiTbRestoreStandard_Click(sender, e);
        }
        private void MiTbRestorePrevious_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiTbRestorePrevious_CheckedChanged(sender, e);
        }
        private void MiUpdMenu_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiUpdMenu_Click(sender, e);
        }
        private void MiUpdDics_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiUpdDics_Click(sender, e);
        }
        private void MiReportsOpen_Click(object sender, RoutedEventArgs e)
        {
            Program.mainFrm1.MiReportsOpen_Click(sender, e);
        }
        private void WinMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MiPrint_Click(MiPrint, null);
            }
            if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MiRastrLayers_Click(MiRastrLayers, null);
            }
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MiSearch_Click(MiSearch, null);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] spatial_refs = new string[]{
                Properties.Resources.PgGeom_ESridTypes_LongLat,
                Properties.Resources.PgGeom_ESridTypes_WorldMercator,
                Properties.Resources.PgGeom_ESridTypes_Another
            };
            foreach (string sr in spatial_refs)
            {
                var match = System.Text.RegularExpressions.Regex.Match(sr, "(\\d+).+");
                if (match == System.Text.RegularExpressions.Match.Empty || match != null && match.Groups.Count > 1 && match.Groups[1].Value != Program.srid)
                    TbcbSrid.Items.Add(sr);
            }
        }
    }
}