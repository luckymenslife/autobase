using Commi;
using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using GBU_Waybill_plugin.MTClasses.Tools;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WrapperMaplib;
using WrapperMaplib.Wrapper.Geometry;
using WrapperMaplib.Wrapper.Map;

namespace GBU_Waybill_plugin.MTClasses.Tasks
{
    public class TasksWayBillVM : ViewModelBase
    {
        #region Поля
        private ObservableCollection<TaskGeomM> _odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _zones = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_zones = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TypeTaskM> _task_types = new ObservableCollection<TypeTaskM>();
        private ObservableCollection<TypeTaskM> _find_task_types = new ObservableCollection<TypeTaskM>();
        private ObservableCollection<TaskWayBillM> _tasks = new ObservableCollection<TaskWayBillM>();
        private ObservableCollection<RouteM> _routes = new ObservableCollection<RouteM>();
        private RouteM _select_route;
        private int _id_wb;
        private int _car_id;
        private string _driver_name;
        private string _car_description;
        private DateTime _begin_date;
        private DateTime _end_date;
        private int _id_org;
        private string _find_odh_text;
        private string _find_type_text;
        private string _find_zone_text;
        private bool _is_new;
        private string _car_model;
        private bool _only_one_work_type;
        private ETypeTask _type_task;
        private VectorLayer _layer_route;
        private VectorLayer _layer_begin;
        private VectorLayer _layer_end;
        #endregion

        #region Конструктор
        public TasksWayBillVM(int id_wb, int id_org, ETypeTask task_type, bool only_one_work_type = false)
        {
            _id_wb = id_wb;
            _id_org = id_org;
            _only_one_work_type = only_one_work_type;
            _type_task = task_type;

            LoadWayBill();
            LoadTypes();
            LoadOdhs();
            LoadZones();
            AddHandler();
            AddTypeSelectedHandler();
            AddZoneSelectedHandler();
            OdhsFind("");
            FindTypes("");
            FindZone("");
            LoadTasks();
            LoadRoutes();
        }
        #endregion

        #region Свойства
        public string RouteTabName
        {
            get { return MainPluginClass.RouteTabName; }
        }
        public string ZoneTabName
        {
            get { return MainPluginClass.ZoneTabName; }
        }
        public bool IsEnabledRouteTab
        {
            get { return _type_task == ETypeTask.route || _type_task == ETypeTask.all; }
        }
        public bool IsEnabledZoneTab
        {
            get { return _type_task == ETypeTask.zone || _type_task == ETypeTask.all; }
        }
        public int DefaultTabeIndex
        {
            get
            {
                if (IsEnabledRouteTab)
                    return 0;
                else
                    return 1;
            }
        }
        public String DriverName
        {
            get { return _driver_name; }
        }
        public String CarDescription
        {
            get { return _car_description; }
        }
        public String PeriodTask
        {
            get { return String.Format("{0} по {1}", _begin_date.ToString("dd.MM.yyyy HH:mm"), _end_date.ToString("dd.MM.yyyy HH:mm")); }
        }
        public ObservableCollection<TaskGeomM> Odhs
        {
            get { return _odhs; }
        }
        public ObservableCollection<TaskGeomM> FindOdhs
        {
            get
            {
                if (IsReadOnly)
                    return new ObservableCollection<TaskGeomM>(_find_odhs.Where(w => w.Selected == true));
                else
                    return _find_odhs;
            }
            set
            {
                OnPropertyChanged(ref _find_odhs, value, () => FindOdhs);
            }
        }
        public ObservableCollection<TypeTaskM> FindTaskTypes
        {
            get
            {
                if (IsReadOnly)
                    return new ObservableCollection<TypeTaskM>(_find_task_types.Where(w => w.Selected == true));
                else
                    return _find_task_types;
            }
            set
            {
                OnPropertyChanged(ref _find_task_types, value, () => FindTaskTypes);
            }
        }
        public ObservableCollection<TypeTaskM> TaskTypes
        {
            get { return _task_types; }
        }
        public ObservableCollection<TaskGeomM> Zones
        {
            get { return _zones; }
        }
        public ObservableCollection<TaskGeomM> FindZones
        {
            get
            {
                if (IsReadOnly)
                    return new ObservableCollection<TaskGeomM>(_find_zones.Where(w => w.Selected == true));
                else
                    return _find_zones;
            }
            set
            {
                OnPropertyChanged(ref _find_zones, value, () => FindZones);
            }
        }
        public string CountTypes
        {
            get { return String.Format("Количество: {0} Выбрано: {1}", FindTaskTypes.Count, FindTaskTypes.Where(w => w.Selected).Count()); }
        }
        public string CountOdh
        {
            get { return String.Format("Количество: {0} Выбрано: {1}", FindOdhs.Count, FindOdhs.Where(w => w.Selected).Count()); }
        }
        public string CountZones
        {
            get { return String.Format("Количество: {0} Выбрано: {1}", FindZones.Count, FindZones.Where(w => w.Selected).Count()); }
        }
        public string FindOdhText
        {
            get { return _find_odh_text; }
            set
            {
                OnPropertyChanged(ref _find_odh_text, value, () => FindOdhText);
                OdhsFind(_find_odh_text);
            }
        }
        public string FindTypeText
        {
            get { return _find_type_text; }
            set
            {
                OnPropertyChanged(ref _find_type_text, value, () => FindTypeText);
                FindTypes(_find_type_text);
            }
        }
        public string FindZoneText
        {
            get { return _find_zone_text; }
            set
            {
                OnPropertyChanged(ref _find_zone_text, value, () => FindZoneText);
                FindZone(_find_zone_text);
            }
        }
        public VectorLayer LayerOdh { get; set; }
        public VectorLayer LayerZone { get; set; }
        public WrapperMaplib.Wrapper.trWin Map { get; set; }
        public bool IsReadOnly
        {
            get { return !_is_new || (_begin_date < DateTime.Now); }
        }
        public bool IsEnabled
        {
            get { return !IsReadOnly; }
        }
        public ObservableCollection<RouteM> Routes
        {
            get { return _routes; }
        }
        public RouteM SelectedRoute
        {
            get { return _select_route; }
            set
            {
                OnPropertyChanged(ref _select_route, value, () => SelectedRoute);
                if (_select_route != null)
                {
                    SetSelectedOdhFromRoute(_select_route);
                }
            }
        }
        public String CarModel
        {
            get { return _car_model; }
        }
        public string Title
        {
            get
            {
                if (_is_new)
                {
                    return "Создание заданий";
                }
                else
                {
                    return "Созданные задания";
                }
            }
        }
        public WrapperMaplib.MaplibControl MapControl { get; set; }
        #endregion

        #region Закрытые Методы
        private void LoadWayBill()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = String.Format(@"
SELECT w.gid, c.gos_no, (((e2.lastname::text || ' '::text) || e2.firstname::text) || ' '::text) || e2.middlename::text AS driver_name,
date_out_plan begin_date,
date_in_plan end_date,
c.gid car_id,
t.name as type_name
FROM autobase.waybills w
INNER JOIN autobase.cars c ON w.car_id = c.gid
INNER JOIN autobase.employees e2 ON w.driver_id = e2.gid
INNER JOIN autobase.cars_types t ON t.gid = c.type_id
WHERE w.gid = {0};", _id_wb);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    _driver_name = sqlCmd.GetValue<string>("driver_name");
                    _car_description = sqlCmd.GetValue<string>("gos_no");
                    _begin_date = sqlCmd.GetValue<DateTime>("begin_date");
                    _end_date = sqlCmd.GetValue<DateTime>("end_date");
                    _car_id = sqlCmd.GetValue<int>("car_id");
                    _car_model = sqlCmd.GetValue<string>("type_name");
                }
                sqlCmd.Close();
            }
        }
        private void odh_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != null && sender is TaskGeomM)
            {
                TaskGeomM odh = (TaskGeomM)sender;
                if (odh.Selected)
                {
                    if (!string.IsNullOrEmpty(odh.Wkt))
                    {
                        vObject obj = new vObject(LayerOdh);
                        obj.SetWKT(odh.Wkt);
                        obj.Gid = odh.Id;
                    }
                }
                else
                {
                    var o = LayerOdh.GetObjectById(odh.Id);
                    if (o != null)
                        o.Delete();
                    LayerOdh.RemoveDeletedObjects();
                }
                SetMapExtent();
                Map.Repaint(WrapperMaplib.Wrapper.trWin.UpdateMode.VerySlow);
            }
            OnPropertyChanged("CountOdh");
        }
        private void task_type_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_only_one_work_type)
            {
                if (sender != null && sender is TypeTaskM)
                {
                    TypeTaskM type = (TypeTaskM)sender;
                    if (type.Selected)
                    {
                        foreach (var item in TaskTypes)
                        {
                            if (item != type && item.Selected)
                            {
                                item.Selected = false;
                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
            OnPropertyChanged("CountTypes");
        }
        private void zone_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != null && sender is TaskGeomM)
            {
                TaskGeomM odh = (TaskGeomM)sender;
                if (odh.Selected)
                {
                    if (!string.IsNullOrEmpty(odh.Wkt))
                    {
                        vObject obj = new vObject(LayerZone);
                        obj.SetWKT(odh.Wkt);
                        obj.Gid = odh.Id;
                    }
                }
                else
                {
                    var o = LayerZone.GetObjectById(odh.Id);
                    if (o != null)
                        o.Delete();
                    LayerZone.RemoveDeletedObjects();
                }
                SetMapExtent();
                Map.Repaint(WrapperMaplib.Wrapper.trWin.UpdateMode.VerySlow);
            }
            OnPropertyChanged("CountZones");
        }
        private int[] ListToArray(vObject[] objs)
        {
            List<int> _ids = new List<int>();
            foreach (var item in objs)
            {
                _ids.Add(item.Gid);
            }
            return _ids.ToArray();
        }
        private void LoadTypes()
        {
            _task_types.Clear();
            foreach (var item in MainPluginClass.TypeTasks)
            {
                _task_types.Add(new TypeTaskM((int)item.Id, item.Name));
            }
        }
        private void LoadOdhs()
        {
            _odhs.Clear();
            foreach (var item in MainPluginClass.Odhs)
            {
                _odhs.Add(new TaskGeomM(item.Id, item.Name, item.Id_Org, item.Wkt));
            }
        }
        private void LoadZones()
        {
            _zones.Clear();
            foreach (var item in MainPluginClass.Zones)
            {
                _zones.Add(new TaskGeomM(item.Id, item.Name, item.Id_Org, item.Wkt));
            }
        }
        private void OdhsFind(string text)
        {
            FindOdhs = new ObservableCollection<TaskGeomM>(Odhs.Where(w => w.Name.ToUpper().Contains(text.ToUpper())).OrderByDescending(w => w.Selected));
            OnPropertyChanged("CountOdh");
        }
        private void FindTypes(string text)
        {
            FindTaskTypes = new ObservableCollection<TypeTaskM>(_task_types.Where(w => w.Name.ToUpper().Contains(text.ToUpper())).OrderByDescending(w => w.Selected));
            OnPropertyChanged("CountTypes");
        }
        private void FindZone(string text)
        {
            FindZones = new ObservableCollection<TaskGeomM>(_zones.Where(w => w.Name.ToUpper().Contains(text.ToUpper())).OrderByDescending(w => w.Selected));
            OnPropertyChanged("CountZones");
        }
        private void LoadTasks()
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                _tasks.Clear();
                sqlCmd.sql = "SELECT gid, waybill_id, route_id , type_id, external_id FROM autobase.waybills_tasks WHERE route_id IS NOT NULL AND waybill_id = " + _id_wb.ToString() + ";";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    _tasks.Add(new TaskWayBillM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("waybill_id"),
                        sqlCmd.GetValue<int>("route_id"),
                        sqlCmd.GetValue<int>("type_id"),
                        sqlCmd.GetValue<int>("external_id"),
                        ETypeTask.route)
                        );
                }
            }
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT gid, waybill_id, zone_id , type_id, external_id FROM autobase.waybills_tasks WHERE zone_id IS NOT NULL AND waybill_id = " + _id_wb.ToString() + ";";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    _tasks.Add(new TaskWayBillM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("waybill_id"),
                        sqlCmd.GetValue<int>("zone_id"),
                        sqlCmd.GetValue<int>("type_id"),
                        sqlCmd.GetValue<int>("external_id"),
                        ETypeTask.zone)
                        );
                }
            }
            _is_new = (_tasks.Count == 0);

            // Если нет заданий, то загружаем из предыдущего путевого листа
            if (_tasks.Count == 0 && _begin_date > DateTime.Now)
            {
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = @"SELECT gid, waybill_id, route_id, type_id, external_id 
FROM autobase.waybills_tasks 
WHERE route_id IS NOT NULL AND waybill_id = (SELECT autobase.get_last_second_waybill(" + _car_id.ToString() + "));";
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        _tasks.Add(new TaskWayBillM(
                            sqlCmd.GetValue<int>("gid"),
                            sqlCmd.GetValue<int>("waybill_id"),
                            sqlCmd.GetValue<int>("route_id"),
                            sqlCmd.GetValue<int>("type_id"),
                            sqlCmd.GetValue<int>("external_id"),
                            ETypeTask.route)
                            );
                    }
                }
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = @"SELECT gid, waybill_id, zone_id, type_id, external_id 
FROM autobase.waybills_tasks 
WHERE zone_id IS NOT NULL AND waybill_id = (SELECT autobase.get_last_second_waybill(" + _car_id.ToString() + "));";
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        _tasks.Add(new TaskWayBillM(
                            sqlCmd.GetValue<int>("gid"),
                            sqlCmd.GetValue<int>("waybill_id"),
                            sqlCmd.GetValue<int>("zone_id"),
                            sqlCmd.GetValue<int>("type_id"),
                            sqlCmd.GetValue<int>("external_id"),
                            ETypeTask.zone)
                            );
                    }
                }
            }
        }
        private void LoadRoutes()
        {
            this._routes.Clear();
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = string.Format(@"SELECT wtg.gid, wtg.name, 
ARRAY(SELECT route_id FROM autobase.waybills_tasks_ways WHERE group_id = wtg.gid AND route_id is not null) as routes,
ARRAY(SELECT zone_id FROM autobase.waybills_tasks_ways WHERE group_id = wtg.gid AND zone_id is not null) as zones
FROM autobase.waybills_tasks_ways_groups wtg WHERE wtg.gid in (SELECT * FROM autobase.get_access_ways_groups()) ORDER BY name;", _id_org);
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    RouteM route = new RouteM(sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<string>("name"),
                        _id_org);
                    var odhs = sqlCmd.GetValue<int[]>("routes").ToList();
                    var zones = sqlCmd.GetValue<int[]>("zones").ToList();
                    foreach (var item in odhs)
                    {
                        route.Odhs.Add(Odhs.FirstOrDefault(w => w.Id == item));
                    }
                    foreach (var item in zones)
                    {
                        route.Zones.Add(Zones.FirstOrDefault(w => w.Id == item));
                    }
                    _routes.Add(route);
                }
            }
        }
        private void SetSelectedOdhFromRoute(RouteM route)
        {
            foreach (var item in Odhs)
            {
                if (item != null)
                    item.Selected = false;
            }
            foreach (var item in route.Odhs)
            {
                if (item != null)
                    item.Selected = true;
            }
            foreach (var item in Zones)
            {
                if (item != null)
                    item.Selected = false;
            }
            foreach (var item in route.Zones)
            {
                if (item != null)
                    item.Selected = true;
            }
            MoveSelectedToUp();
        }
        private void MoveSelectedToUp()
        {
            FindOdhs = new ObservableCollection<TaskGeomM>(FindOdhs.OrderByDescending(w => w.Selected));
            FindZones = new ObservableCollection<TaskGeomM>(FindZones.OrderByDescending(w => w.Selected));
        }
        private void SetMapExtent()
        {
            if (LayerOdh.Objects.Count > 0 || LayerZone.Objects.Count > 0)
            {
                Map.Extent = CalcBbox(LayerOdh, LayerZone);
                if (Map.scale < 4000)
                {
                    Map.scale = 4000;
                }
            }
        }
        private vBboxd CalcBbox(VectorLayer layer_odh)
        {
            vBboxd b_odh = new vBboxd();
            double dx = 0, dy = 0;

            if (layer_odh.Objects.Count > 0)
            {
                b_odh = layer_odh.Bbox;
                dx = (b_odh.b.x - b_odh.a.x) * 0.1;
                dy = (b_odh.b.y - b_odh.a.y) * 0.1;

                vBboxd result = new vBboxd(b_odh.a.x - dx, b_odh.a.y - dy, b_odh.b.x + dx, b_odh.b.y + dy);
                return result;
            }
            return new vBboxd();

        }
        private byte[] GetMapImage()
        {
            uint h = 750, w = 1050;
            double k = (double)w / (double)h;

            double s = 0;
            var bbox = CalcBbox(LayerOdh);

            double dx = (bbox.b.x - bbox.a.x);
            double dy = (bbox.b.y - bbox.a.y);
            double km = dx / dy;

            double ddx = 0;
            double ddy = 0;

            if (k > km)
            {
                ddx = ((dy * k) - dx) / 2;
            }
            else
            {
                ddy = (((dx * h) / w) - dy) / 2;
            }

            vBboxd bbox2 = new vBboxd(bbox.a.x - ddx, bbox.a.y - ddy, bbox.b.x + ddx, bbox.b.y + ddy);

            double dx2 = (bbox2.b.x - bbox2.a.x);
            double dy2 = (bbox2.b.y - bbox2.a.y);
            double km2 = dy2 / dx2;

            MapControl.SetApplicationParameter("TilesLoadingSynchronous", "1");
            bool result = MapControl.SaveImageByExtent(bbox2, ref s, ref h, ref w, "temp.bmp", true);
            MapControl.SetApplicationParameter("TilesLoadingSynchronous", "0");

            var file = System.IO.File.ReadAllBytes("temp.bmp");
            try
            {
                System.IO.File.Delete("temp.bmp");
            }
            catch { }
            return file;
        }
        private vBboxd CalcBbox(VectorLayer layer_odh, VectorLayer layer_zone)
        {
            vBboxd b_odh = new vBboxd();
            vBboxd b_zone = new vBboxd();
            double dx = 0, dy = 0;

            if (layer_odh.Objects.Count > 0 && layer_zone.Objects.Count == 0)
            {
                b_odh = layer_odh.Bbox;
                dx = (b_odh.b.x - b_odh.a.x) * 0.1;
                dy = (b_odh.b.y - b_odh.a.y) * 0.1;

                vBboxd result = new vBboxd(b_odh.a.x - dx, b_odh.a.y - dy, b_odh.b.x + dx, b_odh.b.y + dy);
                return result;
            }

            if (layer_odh.Objects.Count == 0 && layer_zone.Objects.Count > 0)
            {
                b_zone = layer_zone.Bbox;
                dx = (b_zone.b.x - b_zone.a.x) * 0.1;
                dy = (b_zone.b.y - b_zone.a.y) * 0.1;

                vBboxd result = new vBboxd(b_zone.a.x - dx, b_zone.a.y - dy, b_zone.b.x + dx, b_zone.b.y + dy);
                return result;
            }

            if (layer_odh.Objects.Count > 0 && layer_zone.Objects.Count > 0)
            {
                b_odh = layer_odh.Bbox;
                b_zone = layer_zone.Bbox;
                double max_x = b_odh.b.x, max_y = b_odh.b.y, min_x = b_odh.a.x, min_y = b_odh.a.y;

                if (max_x < b_odh.a.x) max_x = b_odh.a.x;
                if (max_x < b_zone.a.x) max_x = b_zone.a.x;
                if (max_x < b_zone.b.x) max_x = b_zone.b.x;

                if (max_y < b_odh.a.y) max_y = b_odh.a.y;
                if (max_y < b_zone.a.y) max_y = b_zone.a.y;
                if (max_y < b_zone.b.y) max_y = b_zone.b.y;

                if (min_x > b_odh.b.x) min_x = b_odh.b.x;
                if (min_x > b_zone.a.x) min_x = b_zone.a.x;
                if (min_x > b_zone.b.x) min_x = b_zone.b.x;

                if (min_y > b_odh.b.y) min_y = b_odh.b.y;
                if (min_y > b_zone.a.y) min_y = b_zone.a.y;
                if (min_y > b_zone.b.y) min_y = b_zone.b.y;

                dx = (max_x - min_x) * 0.1;
                dy = (max_y - min_y) * 0.1;

                vBboxd result = new vBboxd(min_x - dx, min_y - dy, max_x + dx, max_y + dy);
                return result;
            }
            return new vBboxd();

        }
        #endregion

        #region Открытые метдоы
        public void SetSelected()
        {
            foreach (var item in _tasks)
            {
                if (item.TypeTask == ETypeTask.route)
                {
                    var odh = Odhs.FirstOrDefault(w => w.Id == item.IdTaskObj);
                    if (odh != null)
                        odh.Selected = true;
                }
                else
                {
                    var zone = Zones.FirstOrDefault(w => w.Id == item.IdTaskObj);
                    if (zone != null)
                        zone.Selected = true;
                }
                var type = TaskTypes.FirstOrDefault(w => w.Id == item.IdType);
                if (type != null)
                    type.Selected = true;
            }
            MoveSelectedToUp();
        }
        public void RemoveHandler()
        {
            foreach (var item in Odhs)
            {
                item.PropertyChanged -= odh_PropertyChanged;
            }
        }
        public void AddHandler()
        {
            foreach (var item in Odhs)
            {
                item.PropertyChanged += odh_PropertyChanged;
            }
        }
        public void RemoveTypeSelectedHandler()
        {
            foreach (var item in TaskTypes)
            {
                item.PropertyChanged -= task_type_PropertyChanged;
            }
        }
        public void AddTypeSelectedHandler()
        {
            foreach (var item in TaskTypes)
            {
                item.PropertyChanged += task_type_PropertyChanged;
            }
        }
        public void RemoveZoneSelectedHandler()
        {
            foreach (var item in Zones)
            {
                item.PropertyChanged -= zone_PropertyChanged;
            }
        }
        public void AddZoneSelectedHandler()
        {
            foreach (var item in Zones)
            {
                item.PropertyChanged += zone_PropertyChanged;
            }
        }
        #endregion

        #region CreateTasksCmd
        private ICommand _create_tasks;
        public ICommand CreateTasksCmd
        {
            get { return _create_tasks ?? (_create_tasks = new RelayCommand(this.CreateTasks, this.CanCreateTasks)); }
        }
        private bool CanCreateTasks(object obj)
        {
            if (IsReadOnly)
                return false;
            return ((Odhs.FirstOrDefault(w => w.Selected) != null) || (Zones.FirstOrDefault(w => w.Selected) != null)) && (TaskTypes.FirstOrDefault(w => w.Selected) != null);
        }
        private void CreateTasks(object obj)
        {
            if (!CanCreateTasks(null))
                return;
            TimeSpan duration = _end_date - _begin_date;
            if (duration <= new TimeSpan(0, 1, 0, 0) &&
                duration > new TimeSpan(1, 0, 0, 0))
            {
                System.Windows.MessageBox.Show("Период задания не должен быть менее 1 и не более 24 часов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            var selectedOdh = Odhs.Where(w => w.Selected).ToList();
            var selectedTypes = TaskTypes.Where(w => w.Selected).ToList();
            var selectedZones = Zones.Where(w => w.Selected).ToList();
            int? carExternalId = GetExternalId(_car_id, "cars");

            if (carExternalId != null)
            {
                int group_id = CreateTasksGroupe();

                foreach (var odh in selectedOdh)
                {
                    foreach (var type in selectedTypes)
                    {

                        int mt_id = CreateTaskInMt(odh, type, carExternalId.Value, true);
                        TaskWayBillM task = new TaskWayBillM(-1, _id_wb, odh.Id, type.Id, mt_id, ETypeTask.route);
                        CreateTaskInBd(task, group_id);
                    }
                }
                foreach (var zones in selectedZones)
                {
                    foreach (var type in selectedTypes)
                    {

                        int mt_id = CreateTaskInMt(zones, type, carExternalId.Value, false);
                        TaskWayBillM task = new TaskWayBillM(-1, _id_wb, zones.Id, type.Id, mt_id, ETypeTask.zone);
                        CreateTaskInBd(task, group_id);
                    }
                }
                LoadTasks();
                OnPropertyChanged("IsReadOnly");
                OnPropertyChanged("FindTaskTypes");
                OnPropertyChanged("FindOdhs");
                OnPropertyChanged("FindZones");
            }
            else
            {
                System.Windows.MessageBox.Show("Нет идентификатора транспортного средства в МТ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int CreateTaskInMt(TaskGeomM obj, TypeTaskM type, int car_id_mt, bool is_route)
        {
            String description = "Создал " + MainPluginClass.App.user_info.nameUser;
            int result = 0;
            TimeSpan duration = _end_date - _begin_date;
            if (duration > new TimeSpan(0, 1, 0, 0) &&
                duration <= new TimeSpan(1, 0, 0, 0))
            {
                MT_CarsTask carsTask = new MT_CarsTask();
                carsTask.carIds = new List<long>() { car_id_mt };
                carsTask.description = description;
                carsTask.from = MTAPI_Helper.GetUnixTime(_begin_date);
                if (is_route)
                {
                    carsTask.routeId = obj.Id;
                    carsTask.zoneId = null;
                }
                else
                {
                    carsTask.zoneId = obj.Id;
                    carsTask.routeId = null;
                }
                carsTask.till = MTAPI_Helper.GetUnixTime(_end_date);
                carsTask.typeId = type.Id;
                result = (int)MTAPI_Helper.PostCarsTask(carsTask, UserControlAttr.Token).id;
            }

            return result;
        }
        private int? GetExternalId(long gid, String tableName)
        {
            int? result = null;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT external_id FROM autobase.{0} WHERE gid = {1}", tableName, gid);
                result = sqlCmd.ExecuteScalar<int?>();
            }
            return result;
        }
        private void CreateTaskInBd(TaskWayBillM task, int id_group)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                if (task.TypeTask == ETypeTask.route)
                {
                    sqlCmd.sql = String.Format(@"INSERT INTO autobase.waybills_tasks(waybill_id, car_id, time_from, time_till, route_id, type_id, external_id, group_id) 
VALUES (@waybill_id, {0}, @time_from, @time_till, {1}, {2}, {3}, @group_id);
",
                        _car_id, task.IdTaskObj, task.IdType, task.IdMT);
                    sqlCmd.AddParam("@time_from", _begin_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@time_till", _end_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@group_id", id_group, System.Data.DbType.Int32);
                    sqlCmd.AddParam("@waybill_id", _id_wb, System.Data.DbType.Int32);
                    sqlCmd.ExecuteNonQuery();
                }
                if (task.TypeTask == ETypeTask.zone)
                {
                    sqlCmd.sql = String.Format(@"INSERT INTO autobase.waybills_tasks(waybill_id, car_id, time_from, time_till, zone_id, type_id, external_id, group_id) 
VALUES (@waybill_id, {0}, @time_from, @time_till, {1}, {2}, {3}, @group_id);
",
    _car_id, task.IdTaskObj, task.IdType, task.IdMT);
                    sqlCmd.AddParam("@time_from", _begin_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@time_till", _end_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@group_id", id_group, System.Data.DbType.Int32);
                    sqlCmd.AddParam("@waybill_id", _id_wb, System.Data.DbType.Int32);
                    sqlCmd.ExecuteNonQuery();

                }
            }
        }
        private int CreateTasksGroupe()
        {
            string group_name = "Без маршрута";
            int? id_group = null;
            int result = 0;
            if (SelectedRoute != null)
            {
                id_group = SelectedRoute.Id;
                group_name = SelectedRoute.Name;
            }
            using (var sqlCmd = MainPluginClass.App.SqlWork(true))
            {
                sqlCmd.sql = @"INSERT INTO autobase.waybills_tasks_groups(group_name, route_id, time_from, time_till)
    VALUES (@group_name, @route_id, @time_from, @time_till) RETURNING gid;";
                sqlCmd.AddParam("@time_from", _begin_date, System.Data.DbType.DateTime);
                sqlCmd.AddParam("@time_till", _end_date, System.Data.DbType.DateTime);
                sqlCmd.AddParam("@group_name", group_name, System.Data.DbType.String);
                sqlCmd.AddParam("@route_id", id_group, System.Data.DbType.Int32);
                result = sqlCmd.ExecuteScalar<int>();
            }
            return result;
        }
        #endregion

        #region CloseCmd
        private ICommand _close;
        public ICommand CloseCmd
        {
            get { return _close ?? (_close = new RelayCommand(this.Close, this.CanClose)); }
        }
        private bool CanClose(object obj)
        {
            return true;
        }
        private void Close(object obj)
        {
            OnPropertyChanged("close");
        }
        #endregion

        #region SaveRouteCmd
        private ICommand _save_route;
        public ICommand SaveRouteCmd
        {
            get { return _save_route ?? (_save_route = new RelayCommand(this.SaveRoute, this.CanSaveRoute)); }
        }
        private bool CanSaveRoute(object obj)
        {
            return ((Odhs.FirstOrDefault(w => w.Selected) != null) || (Zones.FirstOrDefault(w => w.Selected) != null)) && SelectedRoute != null;
        }
        private void SaveRoute(object obj)
        {
            if (!CanSaveRoute(obj))
                return;

            if (System.Windows.MessageBox.Show("Вы действительно хотите пересохранить выбранную группу?", "Сохранение группы",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = String.Format("DELETE FROM autobase.waybills_tasks_ways WHERE group_id={0};", SelectedRoute.Id);
                    sqlCmd.ExecuteNonQuery();

                    foreach (var item in Odhs.Where(w => w.Selected))
                    {
                        sqlCmd.sql = String.Format("INSERT INTO autobase.waybills_tasks_ways (group_id, route_id) VALUES ({0}, {1});", SelectedRoute.Id, item.Id);
                        sqlCmd.ExecuteNonQuery();
                    }
                    foreach (var item in Zones.Where(w => w.Selected))
                    {
                        sqlCmd.sql = String.Format("INSERT INTO autobase.waybills_tasks_ways (group_id, zone_id) VALUES ({0}, {1});", SelectedRoute.Id, item.Id);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                LoadRoutes();
            }
        }
        #endregion

        #region CreateRouteCmd
        private ICommand _create_route;
        public ICommand CreateRouteCmd
        {
            get { return _create_route ?? (_create_route = new RelayCommand(this.CreateRoute, this.CanCreateRoute)); }
        }
        private bool CanCreateRoute(object obj)
        {
            return ((Odhs.FirstOrDefault(w => w.Selected) != null) || (Zones.FirstOrDefault(w => w.Selected) != null));
        }
        private void CreateRoute(object obj)
        {
            if (!CanCreateRoute(obj))
                return;

            InputFormM data = new InputFormM("Создание группы", "Наименование группы", "");
            InputForm frm = new InputForm(data);
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string name = frm.Data.Text;
                if (!string.IsNullOrEmpty(name))
                {
                    CreateRouteToDb(name);
                }
            }
        }
        private void CreateRouteToDb(string name)
        {
            int id_route = -1;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "INSERT INTO autobase.waybills_tasks_ways_groups (name, org_id) VALUES (@name, @org_id) returning gid;";
                sqlCmd.AddParam(new Params("@name", name, NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.AddParam(new Params("@org_id", _id_org, NpgsqlTypes.NpgsqlDbType.Integer));
                id_route = sqlCmd.ExecuteScalar<int>();
            }
            if (id_route > 0)
            {
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    foreach (var item in Odhs.Where(w => w.Selected))
                    {
                        sqlCmd.sql = String.Format("INSERT INTO autobase.waybills_tasks_ways (group_id, route_id) VALUES ({0}, {1});", id_route, item.Id);
                        sqlCmd.ExecuteNonQuery();
                    }
                    foreach (var item in Zones.Where(w => w.Selected))
                    {
                        sqlCmd.sql = String.Format("INSERT INTO autobase.waybills_tasks_ways (group_id, zone_id) VALUES ({0}, {1});", id_route, item.Id);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
            }
            LoadRoutes();
        }
        #endregion

        #region DeleteRouteCmd
        private ICommand _delete_route;
        public ICommand DeleteRouteCmd
        {
            get { return _delete_route ?? (_delete_route = new RelayCommand(this.DeleteRoute, this.CanDeleteRoute)); }
        }
        private bool CanDeleteRoute(object obj)
        {
            return SelectedRoute != null;
        }
        private void DeleteRoute(object obj)
        {
            if (!CanDeleteRoute(obj))
                return;

            if (System.Windows.MessageBox.Show("Вы действительно хотите удалить выбранную группу?", "Удаление группы",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = String.Format(@"DELETE FROM autobase.waybills_tasks_ways WHERE group_id={0};
DELETE FROM autobase.waybills_tasks_ways_groups WHERE gid = {0};", SelectedRoute.Id);
                    sqlCmd.ExecuteNonQuery();
                }
                LoadRoutes();
            }
        }
        #endregion

        #region BuildRouteCmd
        private ICommand _build_route;
        public ICommand BuildRouteCmd
        {
            get { return _build_route ?? (_build_route = new RelayCommand(this.BuildRoute, this.CanBuildRoute)); }
        }
        private bool CanBuildRoute(object obj)
        {
            return Zones.FirstOrDefault(w => w.Selected) != null;
        }
        private void BuildRoute(object obj)
        {
            if (!CanBuildRoute(obj))
                return;
            try
            {
                string temp = GetBestRoute();
                AddLayerRoute(Transform(temp, 4326, 3395));
            }
            catch
            {
                System.Windows.MessageBox.Show("Не удалось построить маршрут", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddLayerRoute(string wkt)
        {
            if (_layer_route == null)
            {
                _layer_route = Map.Map.CreateLayer("route", new string[] { "id", "name" });
                _layer_route.Encoding = "UTF-8";
                _layer_route.Editable = true;
                _layer_route.Uniform = true;
                var sld_layer = SldLayer.create(Map, "route_sld", _layer_route, "sld_route.xml");
                Map.Map.Layers.Add(sld_layer);

                while (Map.Map.Layers.MoveUp(sld_layer)) ;
                _layer_route.Enabled = false;
            }
            else
            {
                var o = _layer_route.GetObjectById(1);
                if (o != null)
                    o.Delete();
                _layer_route.RemoveDeletedObjects();
            }

            vObject obj = new vObject(_layer_route);
            obj.SetWKT(wkt);
            obj.Gid = 1;
            MapControl.Maplib.Repaint(WrapperMaplib.Wrapper.trWin.UpdateMode.VerySlow);
        }
        private void AddLayerBeginPoint(string wkt)
        {
            if (_layer_begin == null)
            {
                _layer_begin = Map.Map.CreateLayer("point_begin", new string[] { "id", "name" });
                _layer_begin.Encoding = "UTF-8";
                _layer_begin.Editable = true;
                _layer_begin.Uniform = true;
                var sld_layer = SldLayer.create(Map, "point_begin_sld", _layer_begin, "sld_point_begin.xml");
                Map.Map.Layers.Add(sld_layer);

                while (Map.Map.Layers.MoveUp(sld_layer)) ;
                _layer_begin.Enabled = false;
            }
            else
            {
                var o = _layer_begin.GetObjectById(1);
                if (o != null)
                    o.Delete();
                _layer_begin.RemoveDeletedObjects();
            }

            vObject obj = new vObject(_layer_begin);
            obj.SetWKT(wkt);
            obj.Gid = 1;
        }
        private void AddLayerEndPoint(string wkt)
        {
            if (_layer_end == null)
            {
                _layer_end = Map.Map.CreateLayer("point_end", new string[] { "id", "name" });
                _layer_end.Encoding = "UTF-8";
                _layer_end.Editable = true;
                _layer_end.Uniform = true;
                var sld_layer = SldLayer.create(Map, "point_end_sld", _layer_end, "sld_point_end.xml");
                Map.Map.Layers.Add(sld_layer);

                while (Map.Map.Layers.MoveUp(sld_layer)) ;
                _layer_end.Enabled = false;
            }
            else
            {
                var o = _layer_end.GetObjectById(1);
                if (o != null)
                    o.Delete();
                _layer_end.RemoveDeletedObjects();
            }

            vObject obj = new vObject(_layer_end);
            obj.SetWKT(wkt);
            obj.Gid = 1;
        }
        private string GetBestRoute(string formt = null)
        {
            var points_zone = GetZonePoints();
            Distances temp = RouteWebAPI.GetDistances(points_zone);
            int[] path = null;
            CPath c = null;
            if (points_zone.Count > 9)
            {
                c = new CPathLittle(ConvertListToArray(temp.distance_table));
            }
            else
            {
                c = new CPathBruteForce(ConvertListToArray(temp.distance_table));
            }
            c.FindBestPath();
            path = c.Path;

            points_zone = SortList(points_zone, path);
            if (formt == "gpx")
            {
                return RouteWebAPI.GetGpxRoute(points_zone);
            }
            else
            {
                Route temp_route = RouteWebAPI.GetRoute(points_zone);
                //var temp_points = decodePolyline(temp_route.route_geometry);
                var temp_points = DecodePolylinePoints(temp_route.route_geometry);
                AddLayerBeginPoint(Transform(string.Format("POINT({0} {1})", temp_points[0].Longitude.ToString().Replace(",", "."),
                    temp_points[0].Latitude.ToString().Replace(",", ".")), 4326, 3395));
                AddLayerEndPoint(Transform(string.Format("POINT({0} {1})", temp_points[temp_points.Count - 1].Longitude.ToString().Replace(",", "."),
                                    temp_points[temp_points.Count - 1].Latitude.ToString().Replace(",", ".")), 4326, 3395));
                return GeConvertToWkt(temp_points);
            }

        }
        private string GeConvertToWkt(Collection<Double> coords)
        {
            StringBuilder result = new StringBuilder();
            result.Append("LINESTRING(");
            for (int i = 0; i < coords.Count; i += 2)
            {
                result.Append(coords[i].ToString().Replace(",", ".") + " " + coords[i + 1].ToString().Replace(",", ".") + ",");
            }
            return result.ToString().TrimEnd(',') + ")";
        }
        private string GeConvertToWkt(List<Location> coords)
        {
            StringBuilder result = new StringBuilder();
            result.Append("LINESTRING(");
            for (int i = 0; i < coords.Count; i++)
            {
                result.Append(coords[i].Longitude.ToString().Replace(",", ".") + " " + coords[i].Latitude.ToString().Replace(",", ".") + ",");
            }
            return result.ToString().TrimEnd(',') + ")";
        }
        private Collection<Double> decodePolyline(string polyline)
        {
            if (polyline == null || polyline == "") return null;

            char[] polylinechars = polyline.ToCharArray();
            int index = 0;
            Collection<Double> points = new Collection<Double>();
            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylinechars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylinechars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylinechars.Length);

                if (index >= polylinechars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylinechars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylinechars.Length);

                if (index >= polylinechars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                points.Add(Convert.ToDouble(currentLat) / 1000000.0);
                points.Add(Convert.ToDouble(currentLng) / 1000000.0);
            }

            return points;
        }
        private List<Location> DecodePolylinePoints(string encodedPoints)
        {
            if (encodedPoints == null || encodedPoints == "") return null;
            List<Location> poly = new List<Location>();
            char[] polylinechars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            try
            {
                while (index < polylinechars.Length)
                {
                    // calculate next latitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    //calculate next longitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length && next5bits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                    Location p = new Location();
                    p.Latitude = Convert.ToDouble(currentLat) / 1000000.0;
                    p.Longitude = Convert.ToDouble(currentLng) / 1000000.0;
                    poly.Add(p);
                }
            }
            catch (Exception ex)
            {
                // logo it
            }
            return poly;
        }
        private List<vPointd> SortList(List<vPointd> points, int[] path)
        {
            List<vPointd> result = new List<vPointd>();
            for (int i = 0; i < path.Length - 1; i++)
            {
                result.Add(points[path[i]]);
            }
            return result;
        }
        private double[,] ConvertListToArray(List<double[]> value)
        {
            double[,] result = new double[value.Count, value.Count];
            for (int i = 0; i < value.Count; i++)
            {
                for (int j = 0; j < value[i].Length; j++)
                {
                    result[i, j] = value[i][j];
                }
            }
            return result;
        }
        private List<vPointd> GetZonePoints()
        {
            List<vPointd> result = new List<vPointd>();
            foreach (var item in LayerZone.Objects)
            {
                result.Add(Transform(item.centroid, 3395, 4326));
            }
            return result;
        }
        private vPointd Transform(vPointd point, int srid_s, int srid_t)
        {
            vPointd result = new vPointd();
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = string.Format(@"SELECT st_x(st_transform(st_geomfromtext('POINT({0} {1})', {2}), {3})) as x,
                st_y(st_transform(st_geomfromtext('POINT({0} {1})', {2}), {3})) as y;", point.x.ToString().Replace(",", "."),
                                                                                      point.y.ToString().Replace(",", "."),
                                                                                      srid_s,
                                                                                      srid_t);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    result.x = sqlCmd.GetValue<double>("x");
                    result.y = sqlCmd.GetValue<double>("y");
                }
            }
            return result;
        }
        private string Transform(string wkt, int srid_s, int srid_t)
        {
            string result = "";
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = string.Format(@"SELECT st_astext(st_transform(st_geomfromtext('{0}', {1}), {2})) as wkt;",
                                            wkt,
                                            srid_s,
                                            srid_t);
                result = sqlCmd.ExecuteScalar<string>();
            }
            return result;
        }
        #endregion

        #region DownLoadGpxCmd
        private ICommand _download_gpx;
        public ICommand DownLoadGpxCmd
        {
            get { return _download_gpx ?? (_download_gpx = new RelayCommand(this.DownLoadGpx, this.CanDownLoadGpx)); }
        }
        private bool CanDownLoadGpx(object obj)
        {
            return Zones.FirstOrDefault(w => w.Selected) != null;
        }
        private void DownLoadGpx(object obj)
        {
            if (!CanDownLoadGpx(obj))
                return;

            if (!CanBuildRoute(obj))
                return;
            try
            {
                string temp = GetBestRoute("gpx");
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "gpx| *.gpx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(sfd.FileName, temp);
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Не удалось построить маршрут", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ClearGroupCmd
        private ICommand _clear_group;
        public ICommand ClearGroupCmd
        {
            get { return _clear_group ?? (_clear_group = new RelayCommand(this.ClearGroup, this.CanClearGroup)); }
        }

        private bool CanClearGroup(object obj)
        {
            return SelectedRoute!=null;
        }

        private void ClearGroup(object obj)
        {
            if (!CanClearGroup(obj))
                return;
            SelectedRoute = null;
        }
        #endregion

        #region SaveImageCmd
        private ICommand _save_image;
        public ICommand SaveImageCmd
        {
            get { return _save_image ?? (_save_image = new RelayCommand(this.SaveImage)); }
        }

        private void SaveImage(object obj)
        {
            GetMapImage();
        }
        #endregion
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}