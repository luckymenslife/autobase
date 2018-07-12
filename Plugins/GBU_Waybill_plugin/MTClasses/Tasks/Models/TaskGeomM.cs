using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class TaskGeomM: SelectableClass
    {
        #region Поля
        private int _id;
        private string _name;
        private int _id_org;
        private string _wkt;
        #endregion

        #region Конструктор
        public TaskGeomM(int id, string name, int id_org, string wkt)
        {
            _id = id;
            _name = name;
            _id_org = id_org;
            _wkt = wkt;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                OnPropertyChanged(ref _name, value, () => Name);
            }
        }
        public int Id_Org
        {
            get { return _id_org; }
            set
            {
                OnPropertyChanged(ref _id_org, value, () => Id_Org);
            }
        }
        public string Wkt
        {
            get { return _wkt; }
            set
            {
                OnPropertyChanged(ref _wkt, value, () => Wkt);
            }
        }
        #endregion
    }
}
