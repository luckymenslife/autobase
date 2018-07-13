using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using GBU_Waybill_plugin.MTClasses.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WrapperMaplib.Wrapper.Geometry;
using WrapperMaplib.Wrapper.Map;

namespace GBU_Waybill_plugin.MTClasses.Tasks.ViewModels
{
    public class TasksVM : ViewModelBase, IDisposable
    {
        #region Поля
        private ObservableCollection<TaskGeomM> _odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _zones = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_zones = new ObservableCollection<TaskGeomM>();
        private string _find_odhs_text = "";
        private ObservableCollection<TypeTaskWithCarsM> _work_types = new ObservableCollection<TypeTaskWithCarsM>();
        private ObservableCollection<TypeTaskWithCarsM> _find_work_types = new ObservableCollection<TypeTaskWithCarsM>();
        private string _find_work_type_text = "";
        private TypeTaskWithCarsM _selected_work_type;
        private ObservableCollection<CarM> _cars = new ObservableCollection<CarM>();
        private ObservableCollection<CarM> _find_cars = new ObservableCollection<CarM>();
        private string _find_cars_text = "";
        private ObservableCollection<RouteM> _groups = new ObservableCollection<RouteM>();
        private RouteM _select_group;
        private DateTime _begin_date;
        private DateTime _end_date;
        private int _id_org;
        #endregion

        #region Конструктор
        public TasksVM(int id_org)
        {
            _id_org = id_org;
            BeginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            LoadCars();
            LoadWorkTypes();
            LoadOdhs();
            LoadZones();
            LoadGroups();
            AddHandler();
            AddZoneHandler();
            MainPluginClass.AppEvents.PropertyChanged += AppEvents_PropertyChanged;
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
        public DateTime BeginDate
        {
            get { return _begin_date; }
            set
            {
                OnPropertyChanged(ref _begin_date, value, () => BeginDate);
            }
        }
        public DateTime EndDate
        {
            get { return _end_date; }
            set
            {
                OnPropertyChanged(ref _end_date, value, () => EndDate);
            }
        }
        public ObservableCollection<TaskGeomM> Odhs
        {
            get { return _odhs; }
        }
        public ObservableCollection<TaskGeomM> FindOdhs
        {
            get
            {
                return _find_odhs;
            }
            set
            {
                OnPropertyChanged(ref _find_odhs, value, () => FindOdhs);
                OnPropertyChanged("CountOdh");
            }
        }
        public ObservableCollection<TaskGeomM> Zones
        {
            get { return _zones; }
        }
        public ObservableCollection<TaskGeomM> FindZones
        {
            get
            {
                return _find_zones;
            }
            set
            {
                OnPropertyChanged(ref _find_zones, value, () => FindZones);
                OnPropertyChanged("CountZones");
            }
        }
        public string FindOdhsText
        {
            get { return _find_odhs_text; }
            set
            {
                OnPropertyChanged(ref _find_odhs_text, value, () => FindOdhsText);
                OdhsFind(_find_odhs_text);
                ZonesFind(_find_odhs_text);
            }
        }
        public ObservableCollection<TypeTaskWithCarsM> WorkTypes
        {
            get { return _work_types; }
        }
        public ObservableCollection<TypeTaskWithCarsM> FindWorkTypes
        {
            get
            {
                return _find_work_types;
            }
            set
            {
                OnPropertyChanged(ref _find_work_types, value, () => FindWorkTypes);
            }
        }
        public string FindWorkTypesText
        {
            get { return _find_work_type_text; }
            set
            {
                OnPropertyChanged(ref _find_work_type_text, value, () => FindWorkTypesText);
                WorkTypesFind(_find_work_type_text);
            }
        }
        public TypeTaskWithCarsM SelectedWorkType
        {
            get { return _selected_work_type; }
            set
            {
                OnPropertyChanged(ref _selected_work_type, value, () => SelectedWorkType);
                CarsFind(FindCarsText);
            }
        }
        private ObservableCollection<CarM> Cars
        {
            get { return _cars; }
        }
        public ObservableCollection<CarM> FindCars
        {
            get
            {
                return _find_cars;
            }
            set
            {
                OnPropertyChanged(ref _find_cars, value, () => FindCars);
            }
        }
        public string FindCarsText
        {
            get { return _find_cars_text; }
            set
            {
                OnPropertyChanged(ref _find_cars_text, value, () => FindCarsText);
                CarsFind(_find_cars_text);
            }
        }
        public ObservableCollection<RouteM> Groups
        {
            get { return _groups; }
        }
        public RouteM SelectedGroup
        {
            get { return _select_group; }
            set
            {
                OnPropertyChanged(ref _select_group, value, () => SelectedGroup);
                if (_select_group != null)
                {
                    SetSelectedOdhFromRoute(_select_group);
                    SetSelectedZoneFromRoute(_select_group);
                }
            }
        }
        public string CountOdh
        {
            get { return String.Format("Всего: {0} Выбрано: {1}", FindOdhs.Count, FindOdhs.Where(w => w.Selected).Count()); }
        }
        public string CountZones
        {
            get { return String.Format("Всего: {0} Выбрано: {1}", FindZones.Count, FindZones.Where(w => w.Selected).Count()); }
        }
        public string CountWorkTypes
        {
            get { return String.Format("Всего: {0} Выбрано: {1}", FindWorkTypes.Count, FindWorkTypes.Count(w => (w.Cars.Count(q => q.Selected) > 0))); }
        }
        public string CountCars
        {
            get { return String.Format("Всего: {0} Выбрано: {1}", FindCars.Count, FindCars.Where(w => w.Selected).Count()); }
        }

        public VectorLayer LayerOdh { get; set; }
        public VectorLayer LayerZone { get; set; }
        public WrapperMaplib.Wrapper.trWin Map { get; set; }
        public WrapperMaplib.MaplibControl MapControl { get; set; }
        #endregion

        #region Методы
        private void OdhsFind(string _find_odhs_text)
        {
            FindOdhs = new ObservableCollection<TaskGeomM>(Odhs.Where(w => w.Name.ToUpper().Contains(_find_odhs_text.ToUpper())).OrderByDescending(w => w.Selected));
        }
        private void ZonesFind(string _find_odhs_text)
        {
            FindZones = new ObservableCollection<TaskGeomM>(Zones.Where(w => w.Name.ToUpper().Contains(_find_odhs_text.ToUpper())).OrderByDescending(w => w.Selected));
        }
        private void WorkTypesFind(string _find_work_type_text)
        {
            TypeTaskWithCarsM temp = SelectedWorkType;
            FindWorkTypes = new ObservableCollection<TypeTaskWithCarsM>(_work_types.Where(w => w.Name.ToUpper().Contains(_find_work_type_text.ToUpper())).OrderByDescending(w => w.Selected));
            SelectedWorkType = temp;
            OnPropertyChanged("CountWorkTypes");
        }
        private void CarsFind(string _find_cars_text)
        {
            if (SelectedWorkType != null)
            {
                FindCars = new ObservableCollection<CarM>(SelectedWorkType.Cars.Where(w => (w.GosNomer != null && w.GosNomer.ToUpper().Contains(_find_cars_text.ToUpper()))
                                                                                           || (w.GarNomer != null && w.GarNomer.ToUpper().Contains(_find_cars_text.ToUpper()))
                                                                                           )
                                                                                           );
            }
            OnPropertyChanged("CountCars");
        }
        private void LoadWorkTypes()
        {
            _work_types.Clear();
            foreach (var item in MainPluginClass.TypeTasks)
            {
                TypeTaskWithCarsM temp = new TypeTaskWithCarsM((int)item.Id, item.Name);
                foreach (var car in _cars)
                {
                    CarM temp_car = new CarM(car.Id, car.GosNomer, car.GarNomer, car.ExternalId);
                    temp.Cars.Add(temp_car);
                }
                temp.AddHandlerCar();
                _work_types.Add(temp);
            }
            WorkTypesFind("");
        }
        private void LoadOdhs()
        {
            _odhs.Clear();
            foreach (var item in MainPluginClass.Odhs)
            {
                _odhs.Add(new TaskGeomM(item.Id, item.Name, item.Id_Org, item.Wkt));
            }
            OdhsFind("");
        }
        private void LoadZones()
        {
            _zones.Clear();
            foreach (var item in MainPluginClass.Zones)
            {
                _zones.Add(new TaskGeomM(item.Id, item.Name, item.Id_Org, item.Wkt));
            }
            ZonesFind("");
        }
        private void LoadGroups()
        {
            this._groups.Clear();
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
                    foreach (var item in odhs)
                    {
                        route.Odhs.Add(Odhs.FirstOrDefault(w => w.Id == item));
                    }
                    var zones = sqlCmd.GetValue<int[]>("zones").ToList();
                    foreach (var item in zones)
                    {
                        route.Zones.Add(Zones.FirstOrDefault(w => w.Id == item));
                    }
                    _groups.Add(route);
                }
            }
        }
        private void LoadCars()
        {
            _cars.Clear();
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT gid, gos_no, external_id, gar_no FROM autobase.cars WHERE external_id IS NOT NULL AND gid in (SELECT * FROM autobase.get_access_cars()) ORDER BY gos_no;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    _cars.Add(new CarM(sqlCmd.GetValue<int>("gid"),
                                       sqlCmd.GetValue<string>("gos_no"),
                                       sqlCmd.GetValue<string>("gar_no"),
                                       sqlCmd.GetValue<int>("external_id")));
                }
            }
            CarsFind("");
        }
        private void SetSelectedOdhFromRoute(RouteM group)
        {
            foreach (var item in Odhs)
            {
                if (item != null)
                    item.Selected = false;
            }
            foreach (var item in group.Odhs)
            {
                if (item != null)
                    item.Selected = true;
            }
            MoveSelectedToUp();
            OnPropertyChanged("CountOdh");
        }
        private void SetSelectedZoneFromRoute(RouteM group)
        {
            foreach (var item in Zones)
            {
                if (item != null)
                    item.Selected = false;
            }
            foreach (var item in group.Zones)
            {
                if (item != null)
                    item.Selected = true;
            }
            MoveSelectedToUp();
            OnPropertyChanged("CountZones");
        }
        private void MoveSelectedToUp()
        {
            FindOdhs = new ObservableCollection<TaskGeomM>(FindOdhs.OrderByDescending(w => w.Selected));
            FindZones = new ObservableCollection<TaskGeomM>(FindZones.OrderByDescending(w => w.Selected));
        }
        private void AppEvents_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                OnPropertyChanged("CountOdh");
                OnPropertyChanged("CountZones");
                OnPropertyChanged("CountWorkTypes");
                OnPropertyChanged("CountCars");
            }
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
        public void RemoveZoneHandler()
        {
            foreach (var item in Zones)
            {
                item.PropertyChanged -= zone_PropertyChanged;
            }
        }
        public void AddZoneHandler()
        {
            foreach (var item in Zones)
            {
                item.PropertyChanged += zone_PropertyChanged;
            }
        }
        private void zone_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != null && sender is TaskGeomM)
            {
                TaskGeomM zone = (TaskGeomM)sender;
                if (zone.Selected)
                {
                    if (!string.IsNullOrEmpty(zone.Wkt))
                    {
                        vObject obj = new vObject(LayerZone);
                        obj.SetWKT(zone.Wkt);
                        obj.Gid = zone.Id;
                    }
                }
                else
                {
                    var o = LayerZone.GetObjectById(zone.Id);
                    if (o != null)
                        o.Delete();
                    LayerZone.RemoveDeletedObjects();
                }
                SetMapExtent();
                Map.Repaint(WrapperMaplib.Wrapper.trWin.UpdateMode.VerySlow);
            }
            OnPropertyChanged("CountZones");
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
        private byte[] GetMapImage()
        {
            return null;
            uint h = 750, w = 1050;
            double k = (double)w / (double)h;

            double s = 0;
            var bbox = CalcBbox(LayerOdh, LayerZone);

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
        #endregion

        #region Команды
        #region CreateTasksCmd
        private ICommand _create_tasks;
        public ICommand CreateTasksCmd
        {
            get { return _create_tasks ?? (_create_tasks = new RelayCommand(this.CreateTasks, this.CanCreateTasks)); }
        }
        private bool CanCreateTasks(object obj)
        {
            return (Odhs.FirstOrDefault(w => w.Selected) != null || Zones.FirstOrDefault(w => w.Selected) != null)
                && (FindWorkTypes.Count(w => (w.Cars.Count(q => q.Selected) > 0)) > 0);
        }
        private void CreateTasks(object obj)
        {
            if (!CanCreateTasks(null))
                return;
            //TimeSpan duration = _end_date - _begin_date;
            //if (duration <= new TimeSpan(0, 1, 0, 0) ||
            //    duration > new TimeSpan(1, 0, 0, 0))
            //{
            //    MessageBox.Show("Указан некорректный период!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
            //    return;
            //}
            if (_end_date < _begin_date)
            {
                MessageBox.Show("Дата начала должна быть раньше чем дата конца", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            string proc = null;
            try
            {
                proc = MainPluginClass.Work.OpenForm.ProcOpen("create_task");
                int group_id = CreateTasksGroupe();
                var selectedOdh = Odhs.Where(w => w.Selected).ToList();
                foreach (var odh in selectedOdh)
                {
                    foreach (var item in WorkTypes)
                    {
                        foreach (var car in item.Cars.Where(w => w.Selected))
                        {
                            int mt_id = CreateTaskInMt(odh, item, car.ExternalId, true);
                            TaskM task = new TaskM(-1, car.Id, _begin_date, _end_date, odh.Id, item.Id, mt_id, ETypeTask.route);
                            CreateTaskInBd(task, group_id);
                        }
                    }
                }
                var selectedZones = Zones.Where(w => w.Selected).ToList();
                foreach (var zone in selectedZones)
                {
                    foreach (var item in WorkTypes)
                    {
                        foreach (var car in item.Cars.Where(w => w.Selected))
                        {
                            int mt_id = CreateTaskInMt(zone, item, car.ExternalId, false);
                            TaskM task = new TaskM(-1, car.Id, _begin_date, _end_date, zone.Id, item.Id, mt_id, ETypeTask.zone);
                            CreateTaskInBd(task, group_id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainPluginClass.Work.OpenForm.ProcClose(proc);
                MessageBox.Show("Ошибка: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                MainPluginClass.Work.OpenForm.ProcClose(proc);
            }
            OnPropertyChanged("CloseCmd");
        }
        private int CreateTasksGroupe()
        {
            string group_name = "Без маршрута";
            int? id_group = null;
            int result = 0;
            if (SelectedGroup != null)
            {
                id_group = SelectedGroup.Id;
                group_name = SelectedGroup.Name;
            }
            using (var sqlCmd = MainPluginClass.App.SqlWork(true))
            {
                sqlCmd.sql = @"INSERT INTO autobase.waybills_tasks_groups(group_name, route_id, time_from, time_till)
    VALUES (@group_name, @route_id, @time_from, @time_till) RETURNING gid;";
                sqlCmd.AddParam("@time_from", BeginDate, System.Data.DbType.DateTime);
                sqlCmd.AddParam("@time_till", EndDate, System.Data.DbType.DateTime);
                sqlCmd.AddParam("@group_name", group_name, System.Data.DbType.String);
                sqlCmd.AddParam("@route_id", id_group, System.Data.DbType.Int32);
                result = sqlCmd.ExecuteScalar<int>();
            }
            return result;
        }
        private int CreateTaskInMt(TaskGeomM obj, TypeTaskM type, int car_id_mt, bool is_route)
        {
            String description = "Создал " + MainPluginClass.App.user_info.nameUser;
            int result = 0;
            TimeSpan duration = _end_date - _begin_date;

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

            return result;
        }
        private void CreateTaskInBd(TaskM task, int id_group)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                if (task.TypeTask == ETypeTask.route)
                {
                    sqlCmd.sql = String.Format(@"INSERT INTO autobase.waybills_tasks(car_id, time_from, time_till, route_id, type_id, external_id, group_id) 
VALUES ({0}, @time_from, @time_till, {1}, {2}, {3}, @group_id);
",
                        task.IdCar, task.IdTaskObj, task.IdType, task.IdMT);
                    sqlCmd.AddParam("@time_from", _begin_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@time_till", _end_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@group_id", id_group, System.Data.DbType.Int32);
                    sqlCmd.ExecuteNonQuery();
                }
                if (task.TypeTask == ETypeTask.zone)
                {
                    sqlCmd.sql = String.Format(@"INSERT INTO autobase.waybills_tasks(car_id, time_from, time_till, zone_id, type_id, external_id, group_id) 
VALUES ({0}, @time_from, @time_till, {1}, {2}, {3}, @group_id);
",
   task.IdCar, task.IdTaskObj, task.IdType, task.IdMT);
                    sqlCmd.AddParam("@time_from", _begin_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@time_till", _end_date, System.Data.DbType.DateTime);
                    sqlCmd.AddParam("@group_id", id_group, System.Data.DbType.Int32);
                    sqlCmd.ExecuteNonQuery();

                }
            }
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
        #endregion

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion
    }
}
