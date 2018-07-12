using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class RouteM: ViewModelBase
    {
        #region Поля
        private int _id;
        private String _name;
        private ObservableCollection<TaskGeomM> _odhs = new ObservableCollection<TaskGeomM>();
        private ObservableCollection<TaskGeomM> _zones = new ObservableCollection<TaskGeomM>();
        private int _id_org;
        #endregion

        #region Конструктор
        public RouteM(int id, string name, int id_org)
        {
            _id = id;
            _name = name;
            _id_org = id_org;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public String Name
        {
            get { return _name; }
            set
            {
                OnPropertyChanged(ref _name, value, () => this.Name);
            }
        }
        public ObservableCollection<TaskGeomM> Odhs
        {
            get { return _odhs; }
            set
            {
                OnPropertyChanged(ref _odhs, value, () => this.Odhs);
            }
        }
        public ObservableCollection<TaskGeomM> Zones
        {
            get { return _zones; }
            set
            {
                OnPropertyChanged(ref _zones, value, () => this.Zones);
            }
        }
        public int IdOrg
        {
            get { return _id_org; }
            set
            {
                OnPropertyChanged(ref _id_org, value, () => this.IdOrg);
            }
        }
        #endregion
    }
}
