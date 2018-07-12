using GBU_Waybill_plugin.MTClasses.Tasks.Models;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WrapperMaplib.Wrapper.Geometry;
using WrapperMaplib.Wrapper.Map;

namespace GBU_Waybill_plugin.MTClasses.Tasks.ViewModels
{
    public class RoutesVM : ViewModelBase
    {
        #region Поля
        private ETypeTask _type_task;
        private ObservableCollection<TaskGeomM> _odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _zones = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _find_zones = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<RouteM> _routes = new ObservableCollection<RouteM>();
        private RouteM _select_route;
        private int _id_org;
        private string _find_odh_text;
        private string _find_zone_text;
        #endregion

        #region Конструктор
        public RoutesVM(int id_org, ETypeTask task_type)
        {
            _id_org = id_org;
            _type_task = task_type;

            LoadOdhs();
            LoadZones();
            AddHandler();
            AddZoneSelectedHandler();
            OdhsFind("");
            FindZone("");
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
            }
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
        #endregion

        #region Закрытые Методы
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
        private void FindZone(string text)
        {
            FindZones = new ObservableCollection<TaskGeomM>(_zones.Where(w => w.Name.ToUpper().Contains(text.ToUpper())).OrderByDescending(w => w.Selected));
            OnPropertyChanged("CountZones");
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
            OnPropertyChanged("Routes");
        }
        private void SetSelectedOdhFromRoute(RouteM route)
        {
            foreach (var item in Odhs)
            {
                item.Selected = false;
            }
            foreach (var item in route.Odhs)
            {
                item.Selected = true;
            }
            foreach (var item in Zones)
            {
                item.Selected = false;
            }
            foreach (var item in route.Zones)
            {
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

        #region
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

        #region Команды
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

            if (MessageBox.Show("Вы действительно хотите пересохранить выбранный маршрут?", "Сохранение маршрута",
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
        
        #region RenameRouteCmd
        private ICommand _renameRoute;
        public ICommand RenameRouteCmd
        {
            get { return _renameRoute ?? (_renameRoute = new RelayCommand(this.Rename, this.CanRename)); }
        }
        private bool CanRename(object obj)
        {
            return SelectedRoute != null;
        }
        private void Rename(object obj)
        {
            if (!CanRename(obj))
                return;

            InputFormM data = new InputFormM("Создание маршрута", "Наименование маршрута", SelectedRoute.Name);
            InputForm frm = new InputForm(data);
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string newName = frm.Data.Text;
                if (!string.IsNullOrEmpty(newName) && newName != SelectedRoute.Name)
                {
                    RenameRouteToDb(SelectedRoute, newName);
                }
            }
        }
        private void RenameRouteToDb(RouteM route, string name)
        {           
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "UPDATE autobase.waybills_tasks_ways_groups SET name = :newname WHERE gid = :id;";
                sqlCmd.AddParam(new Params(":newname", name, NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.AddParam(new Params(":id", route.Id, NpgsqlTypes.NpgsqlDbType.Integer));
                sqlCmd.ExecuteNonQuery();
            }

            LoadRoutes();
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

            InputFormM data = new InputFormM("Создание маршрута", "Наименование маршрута", "");
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

            if (MessageBox.Show("Вы действительно хотите удалить выбранный маршрут?", "Удаление маршрута",
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
        #endregion
    }
}
