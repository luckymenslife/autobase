using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using mvMapLib;
using AxmvMapLib;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Data.Common;
using Rekod.Repository;
using Rekod.Repository.SettingsDB;
using Rekod.Services;

namespace Rekod
{
    static class Program
    {
        static public Version postgisVersion;
        static public string buildNumber = "";
        static public string[] Args;
        static public Interfaces.IMainApp app { get; set; }
        public static Interfaces.IWorkClass work { get; set; }
        static public int id_user = 0;
        static public string path_string = "";
        static public string scheme = "sys_scheme";
        static public string srid = "32639";
        static public string proj4Map = "";

        static public string setting_file = "settings.mews";

        static public DbConnectionStringBuilder connString = null;
        //static public string connStringSQLite = "Data Source=" + System.Windows.Forms.Application.StartupPath + "\\settingsdb.db;Version=3;New=True;Compress=True;";
        static public string connStringSQLite = "";
        static public List<user_right> tables_right = new List<user_right>();
        static public List<ref_table_constr> ref_tables_right = new List<ref_table_constr>();
        static public List<tablesInfo> tables_info = new List<tablesInfo>();
        static public List<userInfo> users_info = new List<userInfo>();
        static public List<fieldInfo> field_info = new List<fieldInfo>();
        static public List<filtrTableInfo> filtr_table_info = new List<filtrTableInfo>();
        static public List<tipTable> tip_table = new List<tipTable>();
        static public List<tipGeom> tip_geom = new List<tipGeom>();
        static public List<tipData> tip_data = new List<tipData>();
        static public List<tipOperator> tip_operator = new List<tipOperator>();
        static public List<photoInfo> photo_info = new List<photoInfo>();
        static public List<tableAndGroupInfo> tablegroups_info = new List<tableAndGroupInfo>();
        public static List<groupInfo> group_info = new List<groupInfo>();
        public static List<groupInfo> group_info_full = new List<groupInfo>();
        static public List<string> schems = new List<string>();
        static public userInfo user_info = new userInfo();
        static public PgDataRepositoryVM repository;
        static public Npgsql.NpgsqlConnection conn_old;
        static public IExternalSource postgres;
        static public relation RelationVisbleBdUser;
        static public List<String> reserved_words = new List<String>();
        public static mainFrm mainFrm1;
        public static WinMain WinMain;
        public static Dictionary<string, string[]> UserParams { get; set; }

        public static Interfaces.sscUserInfo sscUser { get; set; }
        public static sscSync.Controller.MapAdmin mapAdmin { get; set; }

        public static Rekod.Repository.SettingsXML SettingsXML
        { get { return Rekod.Repository.SettingsXML.Method; } }
        public static SettingsContext SettingsDB;

        public static Rekod.SQLiteSettings.FiltersViewModel Filters;
        public static UserSets.WorkSets_M WorkSets;
        public static Rekod.FastReportClasses.Report_M ReportModel;
        //public static Interfaces.AppSettings AppSettings = new Interfaces.AppSettings();
        private static object _appSettingsObject = null;
        //static public CultureInfo Culture
        //{ get { return (CultureInfo)culture.Clone(); } }
        public static Object AppSettingsObject
        {
            get { return _appSettingsObject; }
            set
            {
                _appSettingsObject = value;
                mainFrm1.ApplyNewSettings();
            }
        }
        public static int bgMap = 0xffffff;
        public static int rasterBgMap = 0xffffff;
        public static Object GetAppSettingByName(String Name)
        {
            if (_appSettingsObject == null)
            {
                return null;
            }
            object retValue = null;
            Type setType = _appSettingsObject.GetType();
            PropertyInfo propInfo = setType.GetProperty(Name);
            if (propInfo != null)
            {
                retValue = propInfo.GetValue(_appSettingsObject, null);
            }
            return retValue;
        }
        public static Rekod.DataAccess.TableManager.ViewModel.TableManagerVM TablesManager;
        public static List<DataAccess.SourcePostgres.Model.PgActionRightM> UserActionRight;
        public static string oKey
        {
            get
            {
                return @"<RSAKeyValue><Modulus>2Gmi+h4VyTxw1HzqwzPHr03lsTshI7bc4hvSeaOH1uBpwxyHTHiJtf9RIh/cIRV53wioPUFEdyHHa6j6wSOchD21C4ZvV+dTO8XWg/yKDnvJTPG07h0qGg30oTKwv4SbTvlLpStOQEdokQB1DFZeV5w62XHFWkzxzNUVUKi825s=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            }
        }
        public static Classes.ButtonManager BManager { get; set; }
        /// <summary>
        /// Временное поле, указывающее активен ли режим третьей версии
        /// </summary>
        public static bool IsV3 = false;
        public static SplashScreen SplashScreen = new SplashScreen("/Resources/mapeditor_splashscreen.png");
        public static Rekod.PrintModule.View.PreviewWindow PreviewWindow = null;
        public static Classes.SSC ssc;

        public static SQLiteCopyPaste SqlCopyPaste
        { get { return SQLiteCopyPaste.Method; } }
        public static string imagesPath { get { return "images/"; } }

        public static axVisUtils.Styles.PgCachedStylesManager CachedStyles
        { get { return axVisUtils.Styles.PgCachedStylesManager.Method; } }

        public static Services.OgrService OgrService
        { get { return Services.OgrService.Method; } }

        public static bool WithMap
        {
            get;
            set;
        }
    }

    public class TextBoxInfo
    {
        string str;
        int start;
        public TextBox t;
        public bool shanged;
        public TextBoxInfo()
        {

        }
        public TextBoxInfo(TextBox tt)
        {
            t = tt;
            str = tt.Text;
            start = tt.SelectionStart;

        }
        public void undo()
        {
            shanged = true;
            t.Text = str;
            t.SelectionStart = start;
        }
        public void saveText()
        {
            str = t.Text;
            start = t.SelectionStart;
        }
        public void saveText(int k)
        {
            str = t.Text;
            start = t.SelectionStart;
            start += k;
            if (start < 0) start = 0;
            if (start > t.Text.Length) start = t.Text.Length;
        }
    }
}