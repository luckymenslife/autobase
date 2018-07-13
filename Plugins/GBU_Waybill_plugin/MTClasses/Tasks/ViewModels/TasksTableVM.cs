using FastReport;
using FastReport.Data;
using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using GBU_Waybill_plugin.MTClasses.Tasks.WinForms;
using GBU_Waybill_plugin.MTClasses.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace GBU_Waybill_plugin.MTClasses.Tasks.ViewModels
{
    public class TasksTableVM : ViewModelBase
    {
        #region Поля
        private ObservableCollection<TaskGroupM> _groups = new ObservableCollection<TaskGroupM>();
        private ObservableCollection<TaskInTableM> _tasks = new ObservableCollection<TaskInTableM>();
        private ObservableCollection<TaskInTableM> _find_tasks = new ObservableCollection<TaskInTableM>();
        private ObservableCollection<StatusM> _statuses = new ObservableCollection<StatusM>();
        private TaskInTableM _selected_task;
        private string _fint_tasks_text = "";
        private List<MT_CarsTask> _mt_tasks = new List<MT_CarsTask>();
        private int _id_org;
        private ICollectionView _findTasksView;
        private DateTime _leftDate;
        private DateTime _rightDate;
        private StatusM _selected_status;
        #endregion

        #region Конструтор
        public TasksTableVM(int id_org)
        {
            _id_org = id_org;
            RightDate = DateTime.Now.AddDays(2);
            LeftDate = DateTime.Now.AddDays(-2);
        }
        #endregion

        #region Свойства
        public DateTime LeftDate
        {
            get { return _leftDate; }
            set
            {
                OnPropertyChanged(ref _leftDate, value, () => LeftDate);
            }
        }
        public DateTime RightDate
        {
            get { return _rightDate; }
            set
            {
                OnPropertyChanged(ref _rightDate, value, () => RightDate);
            }
        }
        public ObservableCollection<TaskInTableM> Tasks
        {
            get { return _tasks; }
        }
        public ICollectionView FindTasksView
        {
            get
            {
                if (_findTasksView == null)
                {
                    ICollectionView tasksView = CollectionViewSource.GetDefaultView(Tasks);
                    tasksView.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                    tasksView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

                    _findTasksView = tasksView;
                }
                _findTasksView.Refresh();
                return _findTasksView;
            }
        }
        public TaskInTableM SelectedTask
        {
            get { return _selected_task; }
            set
            {
                OnPropertyChanged(ref _selected_task, value, () => SelectedTask);
            }
        }
        public string FindTasksText
        {
            get { return _fint_tasks_text; }
            set
            {
                OnPropertyChanged(ref _fint_tasks_text, value, () => FindTasksText);
            }
        }
        public string CountAssigned
        {
            get { return String.Format("{0} назначено", _tasks.Count(w => w.Status == EStatusTask.assigned)); }
        }
        public string CountInProgress
        {
            get { return String.Format("{0} в процессе", _tasks.Count(w => w.Status == EStatusTask.in_progress)); }
        }
        public string CountPerformed
        {
            get { return String.Format("{0} выполнено", _tasks.Count(w => w.Status == EStatusTask.performed)); }
        }
        public string CountOverdue
        {
            get { return String.Format("{0} просрочено", _tasks.Count(w => w.Status == EStatusTask.overdue)); }
        }
        public ObservableCollection<StatusM> Statuses
        {
            get { return _statuses; }
        }
        public StatusM SelectedStatus
        {
            get { return _selected_status; }
            set
            {
                OnPropertyChanged(ref _selected_status, value, () => SelectedStatus);
            }
        }
        #endregion

        #region Методы
        private void LoadTasks()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                _tasks.Clear();
                sqlCmd.sql = @"
SELECT wt.gid, wt.external_id, c.gid as car_id, c.gos_no as gos_no, o.gid as route_id, 
o.name as route_name, z.gid as zone_id, z.name as zone_name, wt.time_from, wt.time_till, 
wt.type_id, vwt.status, wt.group_id, u.name_full as fio, wt.createdate, c.gar_no as gar_no
FROM autobase.waybills_tasks wt
INNER JOIN autobase.cars c ON c.gid = wt.car_id 
LEFT JOIN autobase.v_waybills_tasks_routes o ON o.gid = wt.route_id
LEFT JOIN autobase.v_waybills_tasks_zones z ON z.gid = wt.zone_id
INNER JOIN sys_scheme.user_db u ON u.id = wt.user_id
INNER JOIN autobase.v_waybills_tasks vwt ON vwt.id = wt.external_id
INNER JOIN autobase.v_waybills_tasks_types vwtt on vwtt.id = wt.type_id
WHERE wt.status_id <> 4 AND  c.org_id = :idOrg
AND :leftDate < wt.time_from
AND :rightDate > wt.time_from;";
                sqlCmd.AddParam(":idOrg", _id_org, System.Data.DbType.Int32);
                sqlCmd.AddParam(":leftDate", LeftDate, System.Data.DbType.DateTime);
                sqlCmd.AddParam(":rightDate", RightDate, System.Data.DbType.DateTime);

                if (!String.IsNullOrEmpty(FindTasksText))
                {
                    sqlCmd.sql = sqlCmd.sql.Substring(0, sqlCmd.sql.Length - 1) +
                        String.Format(@"
                        AND (o.name ILIKE {0}
                            OR z.name ILIKE {0}
                            OR c.gos_no ILIKE {0}
                            OR c.gar_no ILIKE {0}
                            OR u.name_full ILIKE {0}
                            OR vwtt.name ILIKE {0});", "'%" + FindTasksText.ToLower() + "%'");
                }
                if (SelectedStatus != null && SelectedStatus.Id != -1)
                {
                    sqlCmd.sql = sqlCmd.sql.Substring(0, sqlCmd.sql.Length - 1) +
                        @"
                        AND (vwt.status = :statusId);";
                    sqlCmd.AddParam(":statusId", SelectedStatus.Id, System.Data.DbType.Int32);
                }


                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int groupId =
                        (sqlCmd.GetValue<Int32?>("group_id") == null) ? 0 : sqlCmd.GetValue<Int32?>("group_id").Value;
                    if(sqlCmd.GetValue<int?>("route_id").HasValue)
                    {
                        TaskInTableM item = new TaskInTableM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("car_id"),
                        sqlCmd.GetValue<string>("gos_no"),
                        sqlCmd.GetValue<DateTime>("time_from"),
                        sqlCmd.GetValue<DateTime>("time_till"),
                        sqlCmd.GetValue<int>("route_id"),
                        sqlCmd.GetValue<string>("route_name"),
                        sqlCmd.GetValue<int>("type_id"),
                        MainPluginClass.TypeTasks.FirstOrDefault(w => w.Id == sqlCmd.GetValue<int>("type_id")).Name,
                        sqlCmd.GetValue<int>("external_id"),
                        ETypeTask.route,
                        (EStatusTask)sqlCmd.GetValue<int>("status"),
                        sqlCmd.GetValue<string>("fio"),
                        sqlCmd.GetValue<DateTime>("createdate"),
                        sqlCmd.GetValue<int>("gar_no")
                        );
                        item.Group = _groups.FirstOrDefault(g => g.Id == groupId);
                        _tasks.Add(item);
                    }
                    if (sqlCmd.GetValue<int?>("zone_id").HasValue)
                    {
                        TaskInTableM item = new TaskInTableM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("car_id"),
                        sqlCmd.GetValue<string>("gos_no"),
                        sqlCmd.GetValue<DateTime>("time_from"),
                        sqlCmd.GetValue<DateTime>("time_till"),
                        sqlCmd.GetValue<int>("zone_id"),
                        sqlCmd.GetValue<string>("zone_name"),
                        sqlCmd.GetValue<int>("type_id"),
                        MainPluginClass.TypeTasks.FirstOrDefault(w => w.Id == sqlCmd.GetValue<int>("type_id")).Name,
                        sqlCmd.GetValue<int>("external_id"),
                        ETypeTask.zone,
                        (EStatusTask)sqlCmd.GetValue<int>("status"),
                        sqlCmd.GetValue<string>("fio"),
                        sqlCmd.GetValue<DateTime>("createdate"),
                        sqlCmd.GetValue<int>("gar_no")
                        );
                        item.Group = _groups.FirstOrDefault(g => g.Id == groupId);
                        _tasks.Add(item);
                    }
                    

                }
            }
        }
        
        #endregion

        #region Команды
        private ICommand _reloadCommand;

        #region CreateTasksCmd
        private ICommand _create_tasks;
        public ICommand CreateTasksCmd
        {
            get { return _create_tasks ?? (_create_tasks = new RelayCommand(this.CreateTasks, this.CanCreateTasks)); }
        }
        private bool CanCreateTasks(object obj)
        {
            return true;
        }
        private void CreateTasks(object obj)
        {
            CreateTasksForm frm = new CreateTasksForm(_id_org);
        }
        #endregion
        #region DeleteTaskCmd
        private ICommand _delete_task;

        private bool CanDeleteTask(object obj)
        {
            //return SelectedTask != null && SelectedTask.Status == EStatusTask.assigned;
            return SelectedTask != null;
        }
        
        #endregion
        #region PrintTasksCmd
        private ICommand _print_tasks;
        public ICommand PrintTasksCmd
        {
            get { return _print_tasks ?? (_print_tasks = new RelayCommand(this.PrintTasks, this.CanPrintTasks)); }
        }
        private bool CanPrintTasks(object obj)
        {
            return SelectedTask != null;
        }
        private void PrintTasks(object obj)
        {
            if (!CanPrintTasks(obj))
                return;
            try
            {
                Report rep = LoadReportFromDB(SelectedTask.Group.Id);
                rep.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataConnectionBase FindConnection(FastReport.Report report, string name)
        {
            foreach (DataConnectionBase item in report.Dictionary.Connections)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }
        private Report LoadReportFromDB(int id_group)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = @"
SELECT 
    id, 
    body,
    caption 
FROM 
    sys_scheme.report_templates_sys
WHERE caption = 'automap_tasks_report_staticmap';";
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    var report = CreateReport(sqlCmd.GetValue<string>("body"), id_group);
                    var bytes= GetMapWithGeom(id_group, new System.Drawing.Size(1050,750));
                    report.Parameters.FindByName("img").Value = bytes;
                    return report;
                }
            }
            return null;
        }
        public byte[] GetMapWithGeom(int group_id, System.Drawing.Size imgSize)
        {
            try
            {
                string centr_x = "";
                string centr_y = "";
                string geom = "";
                double realDistanceH = 0;
                double realDistanceW = 0;
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
//                    sqlCmd.sql = @"SELECT centr_x,centr_y,geojson, 
//        ST_Distance(ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,1)),4326)),26986),ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,2)),4326)),26986)),
//        ST_Distance(ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,1)),4326)),26986),ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,4)),4326)),26986))
//                                    FROM
//                                    	(SELECT ST_ExteriorRing(st_envelope(r.the_geom)) as ext_ring, 
//                                    		st_x(st_centroid(r.the_geom)) as centr_x,
//                                    		st_y(st_centroid(r.the_geom)) as centr_y,
//                                    		st_asgeojson(r.the_geom,8,1) as geojson
//                                    	FROM	
//                                    		(SELECT st_union(the_geom) as the_geom
//                                    		FROM autobase.v_waybills_tasks_routes 
//                                    		INNER JOIN autobase.waybills_tasks
//                                    		ON v_waybills_tasks_routes.gid=autobase.waybills_tasks.route_id
//                                    		WHERE autobase.waybills_tasks.group_id=" + group_id + @") as r) 
//                                    	as aa;";
                    sqlCmd.sql = @"SELECT centr_x,centr_y,geojson, 
        ST_Distance(ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,1)),4326)),26986),ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,2)),4326)),26986)),
        ST_Distance(ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,1)),4326)),26986),ST_Transform((ST_GeomFromText(st_astext(ST_PointN(ext_ring,4)),4326)),26986))
                                    FROM
                                    	(SELECT ST_ExteriorRing(st_envelope(r.the_geom)) as ext_ring, 
                                    		st_x(st_centroid(ST_Envelope(r.the_geom))) as centr_x,
                                    		st_y(st_centroid(ST_Envelope(r.the_geom))) as centr_y,
                                    		st_asgeojson(r.the_geom,8,1) as geojson
                                    	FROM	
                                    		(SELECT st_union(the_geom) as the_geom FROM (SELECT the_geom
							FROM autobase.v_waybills_tasks_routes 
							INNER JOIN autobase.waybills_tasks ON v_waybills_tasks_routes.gid=autobase.waybills_tasks.route_id
                                    		WHERE autobase.waybills_tasks.group_id=" + group_id + @"
                                    		UNION
							SELECT st_union(the_geom) as the_geom
							FROM autobase.v_waybills_tasks_zones 
							INNER JOIN autobase.waybills_tasks ON v_waybills_tasks_zones.gid=autobase.waybills_tasks.zone_id
                                    		WHERE autobase.waybills_tasks.group_id=" + group_id + @") as r1) as r) 
                                    	as aa;";
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        centr_x = (sqlCmd.GetValue<double>(0)).ToString(NumberFormatInfo.InvariantInfo);
                        centr_y = (sqlCmd.GetValue<double>(1)).ToString(NumberFormatInfo.InvariantInfo);
                        geom = sqlCmd.GetValue<string>(2);
                        realDistanceH = sqlCmd.GetValue<double>(3);
                        realDistanceW = sqlCmd.GetValue<double>(4);
                    }
                }
                int zoom = CalculateZoom(realDistanceW, realDistanceH);
                if (zoom < 6) zoom = 6;
                byte[] responseData = Post(MakeData(centr_x, centr_y, geom, zoom, imgSize));
                return responseData;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private int CalculateZoom(double realDistanceW, double realDistanceH)
        {
            double scaleWidth = realDistanceW / 0.2775;
            double scaleHeight = realDistanceH / 0.19;
            double factor = 41000;
            double scale = (scaleWidth > scaleHeight) ? scaleWidth : scaleHeight;
            if (scale > factor * 13)
                return 0;
            if (scale > factor * 12)
                return 1;
            if (scale > factor * 11)
                return 2;
            if (scale > factor * 10)
                return 3;
            if (scale > factor * 9)
                return 4;
            if (scale > factor * 8)
                return 5;
            if (scale > factor * 7)
                return 6;
            if (scale > factor * 6)
                return 7;
            if (scale > factor * 5)
                return 8;
            if (scale > factor * 4)
                return 9;
            if (scale > factor * 3)
                return 10;
            if (scale > factor * 2)
                return 11;
            if (scale > factor)
                return 12;
            if (scale > factor / 2)
                return 13;
            if (scale > factor / 4)
                return 14;
            if (scale > factor / 8)
                return 15;
            if (scale > factor / 16)
                return 16;
            if (scale > factor / 32)
                return 17;
            if (scale > factor / 64)
                return 18;
            else
                return 18;
        }
        private string MakeData(string centr_x, string centr_y, string geom,int zoom, System.Drawing.Size imgSize)
        {
            string rest = string.Format("{0}x{1}{2}{3}{4}{5}{6}", "baseMap=osm&zoom=" + zoom.ToString() + "&size=" + imgSize.Width, imgSize.Height,
                "&center=", centr_y + "," + centr_x, "&geoJsons[]={\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"style\":{\"color\":\"#FF0000\",\"weight\":2},\"geometry\":",
                 geom, "}]}");
            return rest;
        }
        public static byte[] GetBytesFromStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        private byte[] Post(string data)
        {
            try
            {
                WebRequest request = WebRequest.Create("http://staticmap.geo4.me/image");
                request.Method = "POST";
                //request.Proxy = null;
                string postData = data;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                var bytes = GetBytesFromStream(dataStream);
                dataStream.Close();
                response.Close();
                return bytes;
            }
            catch(Exception e)
            {
                return null;
            }
        }
        private FastReport.Report CreateReport(String body, int id_group)
        {
            Report report = new Report();
            report.LoadFromString(body);

            var postgresConn = FindConnection(report, "Current data") as PostgresDataConnection;
            report.Parameters.FindByName("idGroup").Value = id_group.ToString();
            if (postgresConn == null)
            {
                postgresConn = new PostgresDataConnection()
                {
                    Name = "Current data",
                    Enabled = true
                };
                report.Dictionary.Connections.Add(postgresConn);
            }
            postgresConn.ConnectionString = MainPluginClass.App.ConnectionString;

            foreach (TableDataSource item in postgresConn.Tables)
            {
                item.SelectCommand = item.SelectCommand.Replace("@idGroup", id_group.ToString());
            }
            return report;
        }

        #endregion
        #endregion
    }
}
