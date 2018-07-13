using GBU_Waybill_plugin.MTClasses;
using GBU_Waybill_plugin.MTClasses.Tasks;
using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using GBU_Waybill_plugin.MTClasses.Tasks.WinForms;
using GBU_Waybill_plugin.MTClasses.Tools;
using GBU_Waybill_plugin.RemoteMedService;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace GBU_Waybill_plugin
{
    public class MainPluginClass : IMainPlugin
    {
        protected internal static IMainApp App;
        protected internal static IWorkClass Work;
        internal static ObservableCollection<TaskGeomM> Odhs = new ObservableCollection<TaskGeomM>();
        internal static ObservableCollection<TaskGeomM> Zones = new ObservableCollection<TaskGeomM>();
        internal static ObservableCollection<TypeTaskM> TypeTasks = new ObservableCollection<TypeTaskM>();
        internal static ETypeTask _type_task = ETypeTask.all;
        internal static string[] OrgsIds = new string[]{};
        public static string[] SyncOrgAutomapIds;
        public static bool CanSyncAutoMapWaybills;
        public static string RouteTabName = "Маршруты";
        public static string ZoneTabName = "Геозоны";
        public static AppEventsClass AppEvents = new AppEventsClass();

        public int MapEditorTablePutList { get { return 460; } }

        public string GUID
        {
            get { return "85bd2042-8266-4728-8d53-60357694e070"; }
        }

        public string Name
        {
            get { return "Плагин для ввода данных по путевым листам"; }
        }

        public Interfaces.Forms.IControlSettings SettingsForm
        {
            get { return null; }
        }

        public void StartPlugin(System.Xml.Linq.XElement XSettings, IMainApp app, IWorkClass work)
        {
            App = app;
            Work = work;
            work.AddSpoofingAttributesOfObject(MapEditorTablePutList, ReplaceAttrForm);
            //GetParams();
            //LoadMedCheckParams();
            //CalcCanSyncAutoMapWaybills();

            //if (CreationAllowedTasks())
            //{
            //    LoadOdh();
            //    LoadTypes();
            //    LoadZones();

            //    System.Windows.Controls.MenuItem menuTasks = new System.Windows.Controls.MenuItem();
            //    menuTasks.Header = "Задания";

            //    System.Windows.Controls.MenuItem menuTasksMng = new System.Windows.Controls.MenuItem();
            //    menuTasksMng.Header = "Управление заданиями";
            //    menuTasksMng.Click += menuTasks_Click;
            //    menuTasks.Items.Add(menuTasksMng);

            //    System.Windows.Controls.MenuItem menuRoutesMng = new System.Windows.Controls.MenuItem();
            //    menuRoutesMng.Header = "Управление маршрутами";
            //    menuRoutesMng.Click += menuRoutesMng_Click;
            //    menuTasks.Items.Add(menuRoutesMng);

            //    work.MainForm.Menu(menuTasks);
            //}
            //var t = app.getTableInfoOfNameDB("v_employees");
            //if (t != null)
            //{
            //    var write_259 = app.getTableRight(t.idTable);
            //    if (EmployeesSync.Url != null && write_259 != null && write_259.write)
            //    {
            //        work.AddMenuInTable(259, () =>
            //                                 {
            //                                     ToolStripMenuItem menu = new ToolStripMenuItem("Синхронизация");
            //                                     ToolStripMenuItem menu2 = new ToolStripMenuItem("Синхронизация c МедСервисом");

            //                                 //menu2.Click += Employees_Sync;
            //                                 menu2.Click += menu2_Click;
            //                                     menu.DropDownItems.Add(menu2);
            //                                     return menu;
            //                                 });
            //    }
            //}
        }

        private void LoadMedCheckParams()
        {
            EmployeesSync.IdOrg = GetMyOrg();
            if (string.IsNullOrEmpty(EmployeesSync.Url))
            {
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = @"SELECT org_id, url_service, lgn, pwd, lastupdate, rest_url, medservice_org_id
  FROM autobase.waybills_med_checks_org_params
  WHERE org_id = " + EmployeesSync.IdOrg.ToString() + ";";
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        EmployeesSync.Url = sqlCmd.GetValue<string>("url_service");
                        EmployeesSync.Login = sqlCmd.GetValue<string>("lgn");
                        EmployeesSync.Pwd = sqlCmd.GetValue<string>("pwd");
                        EmployeesSync.UrlRest = sqlCmd.GetValue<string>("rest_url");
                        EmployeesSync.MedServiceOrgId = sqlCmd.GetValue<int?>("medservice_org_id");
                    }
                }
            }
        }

        private void menuRoutesMng_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RoutesForm frm = new RoutesForm();
            frm.ShowDialog();
        }

        void menuTasks_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TasksTableForm frm = new TasksTableForm();
            frm.ShowDialog();
        }

        private void updateOdhList(object sender, EventArgs e)
        {
            LoadOdh();
            LoadTypes();
            LoadZones();
        }

        void menu2_Click(object sender, EventArgs e)
        {
            EmployeesSync sync = new EmployeesSync();
            sync.Main();
        }

        private Interfaces.UserControls.IUserControlMain ReplaceAttrForm(int id_table, int? id_object, referInfo refer = null)
        {
            return new UserControlAttr(id_object, id_table);
            //return new nsWaybill.Waybill_VM(id_object, id_table);
        }
        private bool CreationAllowedTasks()
        {
            bool result=false;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT autobase.access_create_tasks_mt();";
                result = sqlCmd.ExecuteScalar<bool>();
            }
            return result;
        }
        private int GetMyOrg()
        {
            int result = -1;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT autobase.get_my_org();";
                result = sqlCmd.ExecuteScalar<int>();
            }
            return result;
        }
        private void LoadOdh()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                Odhs.Clear();
                sqlCmd.sql = String.Format(@"
SELECT gid, name, org_id, st_astext(st_transform(the_geom, 3395)) wkt_text
FROM autobase.v_waybills_tasks_routes WHERE gid in (SELECT * FROM  autobase.get_access_routes()) ORDER BY name;");
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    TaskGeomM odh = new TaskGeomM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<string>("name"),
                        sqlCmd.GetValue<int>("org_id"),
                        sqlCmd.GetValue<string>("wkt_text"));
                    Odhs.Add(odh);
                }

            }
        }
        private void LoadZones()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                Zones.Clear();
                sqlCmd.sql = String.Format(@"
SELECT gid, name, org_id, st_astext(st_transform(the_geom, 3395)) wkt_text
FROM autobase.v_waybills_tasks_zones WHERE org_id in (SELECT * FROM  autobase.get_access_orgs()) ORDER BY name;");
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    TaskGeomM odh = new TaskGeomM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<string>("name"),
                        sqlCmd.GetValue<int>("org_id"),
                        sqlCmd.GetValue<string>("wkt_text"));
                    Zones.Add(odh);
                }

            }
        }
        private void LoadTypes()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                TypeTasks.Clear();
                sqlCmd.sql = String.Format(@"
SELECT id, name
FROM autobase.v_waybills_tasks_types;");
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    TypeTasks.Add(new TypeTaskM(sqlCmd.GetValue<int>("id"), sqlCmd.GetValue<string>("name")));
                }

            }
            //try
            //{
            //    TypeTasks.Clear();
            //    String gaugeTypesJSon = MTAPI_Helper.Get(String.Format(MTAPI_Helper.mt_url + "/modules/carroutes/tasks/types?token={0}", UserControlAttr.Token));
            //    List<MT_TaskType> gaugeTypes = JsonHelper.JsonDeserialize<List<MT_TaskType>>(gaugeTypesJSon);
            //    foreach (var item in gaugeTypes)
            //    {
            //        TypeTasks.Add(new TypeTaskM((int)item.id, item.name));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string error = ex.Message;
            //    System.Threading.Thread.Sleep(1000);
            //    LoadTypes();
            //}
        }
        private void GetParams()
        {
            using (var sqlCmd = App.SqlWork())
            {
                sqlCmd.sql = "SELECT autobase.get_automap_task_orgs_ids();";
                MainPluginClass.OrgsIds = sqlCmd.ExecuteScalar<string[]>();
                if (OrgsIds == null)
                {
                    MainPluginClass.OrgsIds = new string[] { };
                }
            }
            using (var sqlCmd = App.SqlWork())
            {
                sqlCmd.sql = "SELECT name, values FROM autobase.params;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    if (sqlCmd.GetValue<string>("name") == "automap_url")
                    {
                        try
                        {
                            MTAPI_Helper.mt_url = sqlCmd.GetValue<string[]>("values")[0];
                        }
                        catch
                        {

                        }
                    }
                    if (sqlCmd.GetValue<string>("name") == "automap_route_name")
                    {
                        try
                        {
                            MainPluginClass.RouteTabName = sqlCmd.GetValue<string[]>("values")[0];
                        }
                        catch
                        {

                        }
                    }
                    if (sqlCmd.GetValue<string>("name") == "automap_zone_name")
                    {
                        try
                        {
                            MainPluginClass.ZoneTabName = sqlCmd.GetValue<string[]>("values")[0];
                        }
                        catch
                        {

                        }
                    }
                    if (sqlCmd.GetValue<string>("name") == "automap_task_type")
                    {
                        try
                        {
                            string type = sqlCmd.GetValue<string[]>("values")[0];
                            switch (type)
                            {
                                case "all":
                                    MainPluginClass._type_task = ETypeTask.all;
                                    break;
                                case "route":
                                    MainPluginClass._type_task = ETypeTask.route;
                                    break;
                                case "zone":
                                    MainPluginClass._type_task = ETypeTask.zone;
                                    break;
                                default:
                                    MainPluginClass._type_task = ETypeTask.all;
                                    break;
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (sqlCmd.GetValue<string>("name") == "automap_sync_waybills")
                    {
                        try
                        {
                            MainPluginClass.SyncOrgAutomapIds = sqlCmd.GetValue<string[]>("values");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
        private void CalcCanSyncAutoMapWaybills()
        {
            MainPluginClass.CanSyncAutoMapWaybills = false;
            string[] ids = null;
            using(var sqlCmd = App.SqlWork())
            {
                sqlCmd.sql = "SELECT ARRAY(SELECT get_access_orgs::text as id_org FROM autobase.get_access_orgs());";
                ids = sqlCmd.ExecuteScalar<string[]>();
            }
            foreach (var id in MainPluginClass.SyncOrgAutomapIds)
            {
                if (ids != null && ids.Contains(id))
                {
                    MainPluginClass.CanSyncAutoMapWaybills = true;
                    return;
                }
            }

        }
    }

}