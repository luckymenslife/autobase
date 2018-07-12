using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using FastReport.Data;
using System.Diagnostics;
using System.Configuration;
using Rekod.Services;
using System.Threading;
using System.IO;
using System.Globalization;
using Npgsql;


namespace Rekod
{
    /// <summary>
    /// Логика взаимодействия для app.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            
#if LNG_EN
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
#elif LNG_MN
            CultureInfo culture = new CultureInfo("mn-MN");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("mn-MN");
#else
            CultureInfo culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
#endif
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Program.Args = e.Args;
            if (e.Args.Count() < 5)
            {
#if gs
                Program.SplashScreen.Show(false);
                Thread.Sleep(1500);
#endif
            }

            // Проверка, есть ли провайдер sqlite. Если нет, то провайдер добавляется
            var dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
            bool sqliteProviderExists =
                (from System.Data.DataRow row
                    in dataSet.Tables[0].Rows
                 where row["InvariantName"].ToString() == "System.Data.SQLite"
                 select row).Count() > 0;
            if (!sqliteProviderExists)
            {
                dataSet.Tables[0].Rows.Add("SQLite Data Provider"
                , ".Net Framework Data Provider for SQLite"
                , "System.Data.SQLite"
                , "System.Data.SQLite.SQLiteFactory, System.Data.SQLite, Version=1.0.87.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139");
            }

            FastReport.Utils.RegisteredObjects.AddConnection(typeof(PostgresDataConnection));
            OGRFramework.GdalConfiguration.ConfigureOgr();
            OGRFramework.GdalConfiguration.ConfigureGdal();
            

            string startPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            String activeXPath = startPath + "\\MVActiveX;";
            String path = System.Environment.GetEnvironmentVariable("Path");
            if (!path.Contains(activeXPath))
            {
                System.Environment.SetEnvironmentVariable("Path", activeXPath + path);
            }

            System.Windows.Forms.Application.EnableVisualStyles();

            OpenLoginForm();
        }
        #region Для окна аторизации
        private void OpenLoginForm()
        {
            FileInfo settings = CheckedSettingsFile();
            if (settings == null)
            {
                settings = OpenDialogChooseSettings();
            }
            if (settings != null)
            {
                Program.setting_file = settings.Name;
                Program.path_string = settings.DirectoryName;

                var isConn = ConnectionDB(Program.Args);
                if (!isConn)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        FileInfo CheckedSettingsFile()
        {
            string fileName;
            string path;

            if (Program.Args.Length == 1)
            {
                string pathSettings = Program.Args[0];

                if (Path.HasExtension(pathSettings))
                {
                    var file = new FileInfo(pathSettings.ToLower());
                    if (file.Exists && (file.Extension == ".aews"
                                    || file.Extension == ".mews"
                                    || file.Extension == ".riws"))
                    {
                        return file;
                    }
                }
            }
            else
            {
                fileName = Program.setting_file;
                path = Environment.CurrentDirectory;

                if (!File.Exists(path + "\\" + fileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                    fileName += ".aews";
                    if (!File.Exists(path + "\\" + fileName))
                    {
                        return null;
                    }
                    return new FileInfo(path + "\\" + fileName);
                }
                else
                {
                    return new FileInfo(path + "\\" + fileName);
                }
            }
            return null;
        }
        FileInfo OpenDialogChooseSettings()
        {
            var frm = new ToolsFrm.NotFindSettingFrm();
            Program.SplashScreen.Close(new TimeSpan(0, 0, 0));
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                { return new FileInfo(frm.FileName); }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Rekod.Properties.Resources.login_errWrFile, Rekod.Properties.Resources.login_err, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
            return null;
        }
        private bool ConnectionDB(string[] args)
        {
            Npgsql.NpgsqlConnectionStringBuilder sqlBuilder = null;
            int? idUser = null;

            string exceptionText = null;

            if (args.Length >= 5)
            {
                try
                {
                    sqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder()
                    {
                        Database = args[4],
                        Host = args[2],
                        Port = ExtraFunctions.Converts.To<int>(args[3]),
                        UserName = args[0],
                        Password = Encrypting.CreateMD5(args[1]).ToLower()
                    };
                    sqlBuilder.ApplicationName = "MapClient_" + classesOfMetods.GetVersionString;
                    SqlWork.CorrectConnectBuilder(sqlBuilder);
                    idUser = SqlWork.CheckedUser(sqlBuilder);
                }
                catch (NpgsqlException iex)
                {
                    try
                    {
                        sqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder()
                        {
                            Database = args[4],
                            Host = args[2],
                            Port = ExtraFunctions.Converts.To<int>(args[3]),
                            UserName = args[0],
                            Password = args[1]
                        };
                        SqlWork.CorrectConnectBuilder(sqlBuilder);
                        idUser = SqlWork.CheckedUser(sqlBuilder);
                    }
                    catch (Exception ex)
                    {
                        if (ex is NpgsqlException)
                        {
                            if ((ex as NpgsqlException).Code == "28P01")
                                exceptionText = Rekod.Properties.Resources.login_errWPass + sqlBuilder.UserName + "'";
                        }
                        else
                            exceptionText = ex.Message;
                    }
                }
                catch (Exception ex)
                {
                     exceptionText = ex.Message;
                }

                if (args.Length >= 8) // пользователь заходит с параметрами SSC
                {
                    Program.sscUser = new Interfaces.sscUserInfo(
                        new Uri(args[5]), args[6], args[7]);

                    if (args.Length >= 10) // пользователь заходит с параметрами proxy
                    {
                        try
                        {
                            Program.SettingsXML.LocalParameters.Proxy.IP = args[8];
                            Program.SettingsXML.LocalParameters.Proxy.Port = int.Parse(args[9]);
                            Program.SettingsXML.LocalParameters.Proxy.Status = Repository.SettingsFile.eProxyStatus.Customer;

                            if (args.Length >= 12) // пользователь заходит с параметрами proxy, требуещим авторизацию
                            {
                                Program.SettingsXML.LocalParameters.Proxy.Login = args[10];
                                Program.SettingsXML.LocalParameters.Proxy.Password = args[11];
                            }

                        }
                        catch (Exception e)
                        {
                            System.Windows.MessageBox.Show(Rekod.Properties.Resources.login_errIncorrectProxySettings + ": " + e.Message);
                            Program.SettingsXML.LocalParameters.Proxy.Status = Repository.SettingsFile.eProxyStatus.Non;
                        }

                        // применить параметры proxy
                        Program.SettingsXML.ApplyLocalParameters();
                    }
                }
            }
            else
            {
                string line, user;
                Program.SettingsXML.GetLastDataBase(out line, out user);
                Thread.CurrentThread.CurrentUICulture = Program.SettingsXML.LocalParameters.LocaleProgram;
                sqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder() { UserName = user };
                sqlBuilder.ApplicationName = "MapClient_" + classesOfMetods.GetVersionString;
                try
                {
                    Login.ParseServerParams(line, sqlBuilder);
                }
                catch (Exception ex) { }
            }

            if (idUser == null)
            {
                Program.SplashScreen.Close(new TimeSpan(0, 0, 0));
                var frm = new Login();
                frm.SetListDB(Program.SettingsXML.DataBases);
                frm.SetConnectionParams(sqlBuilder);
                if (exceptionText != null)
                {
                    frm.SetErorr(exceptionText);
                    frm.ShowDetail(true);
                }
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    frm.GetConnectionParams(out sqlBuilder, out idUser);
                }
            }
            if (idUser != null && sqlBuilder != null)
            {
                var db = Login.ConcatenateServer(sqlBuilder);
                Program.SettingsXML.SetLastDataBase(db, sqlBuilder.UserName);

                Program.connString = sqlBuilder;
                Program.id_user = idUser.Value;
                return true;
            }
            return false;
        }
        #endregion
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Program.SettingsDB != null)
            {
                Program.SettingsDB.Dispose();
            }
            using (var sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
            {
                sqliteWork.Sql = @"DELETE FROM [geometry_columns] WHERE [f_table_name] = 'copy_past_table'; DROP TABLE IF EXISTS [copy_past_table]; VACUUM;";
                sqliteWork.ExecuteNonQuery();
            }

        }
    }

    #region enable menu, tool strip without focus
    /// <summary>
    /// winform native constants
    /// </summary>
    public class NativeConstants
    {
        internal const uint WM_MOUSEACTIVATE = 0x21;
        internal const uint MA_ACTIVATE = 1;
        internal const uint MA_ACTIVATEANDEAT = 2;
        internal const uint MA_NOACTIVATE = 3;
        internal const uint MA_NOACTIVATEANDEAT = 4;
    }

    /// <summary>
    /// Click through method implementation
    /// </summary>
    public class FocusMenuStrip : MenuStrip //or MenuStrip, BindingNavigator etc...
    {
        public FocusMenuStrip() : base() { }

        /// <summary>
        /// Overided for activete direct click on a toolstrip item
        /// and not default behavior :  set focus to the form container and fire event until you click again
        /// </summary>
        /// <param name="m"></param>
        [DebuggerStepThrough]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == NativeConstants.WM_MOUSEACTIVATE &&
            m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
                m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
        }
    }

    /// <summary>
    /// Click through method implementation
    /// </summary>
    public class FocusToolStrip : ToolStrip //or MenuStrip, BindingNavigator etc...
    {
        public FocusToolStrip() : base() { }

        /// <summary>
        /// Overided for activete direct click on a toolstrip item
        /// and not default behavior :  set focus to the form container and fire event until you click again
        /// </summary>
        /// <param name="m"></param>
        [DebuggerStepThrough]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == NativeConstants.WM_MOUSEACTIVATE &&
            m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
                m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                  System.Drawing.Color.Gray, 1, ButtonBorderStyle.Solid,
                                  System.Drawing.Color.Gray, 1, ButtonBorderStyle.Solid,
                                  System.Drawing.Color.Gray, 1, ButtonBorderStyle.Solid,
                                  System.Drawing.Color.Gray, 1, ButtonBorderStyle.Solid);
        }
    }
    #endregion
}
