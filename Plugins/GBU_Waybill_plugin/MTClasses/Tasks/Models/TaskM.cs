using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class TaskM: ViewModelBase
    {
        #region Поля
        private int _id;
        private int _id_car;
        private DateTime _begin_date;
        private DateTime _end_date;
        private int _id_obj;
        private int _id_type;
        private int _id_mt;
        private ETypeTask _type;
        #endregion

        #region Конструтор
        public TaskM(int id, int id_car, DateTime begin_date, DateTime end_date, int id_obj, int id_type, int id_mt, ETypeTask type)
        {
            _id = id;
            _id_obj = id_obj;
            _id_type = id_type;
            _id_car = id_car;
            _begin_date = begin_date;
            _end_date = end_date;
            _id_mt = id_mt;
            _type = type;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public int IdCar
        {
            get { return _id_car; }
        }
        public DateTime BeginDate
        {
            get { return _begin_date; }
        }
        public DateTime EndDate
        {
            get { return _end_date; }
        }
        public int IdTaskObj
        {
            get { return _id_obj; }
        }
        public int IdType
        {
            get { return _id_type; }
        }
        public int IdMT
        {
            get { return _id_mt; }
        }
        public ETypeTask TypeTask
        {
            get { return _type; }
        }
        #endregion
    }
    public class TaskInTableM : TaskM
    {
        #region Поля
        private string _car_nomer;
        private string _obj_name;
        private string _type_name;
        private EStatusTask _status;
        private string _fio;
        private DateTime _create_date;
        private int _garNumber;
        #endregion

        #region Конструтор
        public TaskInTableM(int id, int id_car, string car_nomer, DateTime begin_date, DateTime end_date, 
                            int id_obj, string obj_name, int id_type, string type_name, int id_mt, ETypeTask type, EStatusTask status, 
            string fio, DateTime create_date, int garNumber) :base(id, id_car, begin_date, end_date, id_obj, id_type, id_mt, type)
        {
            _car_nomer = car_nomer;
            _obj_name = obj_name;
            _type_name = type_name;
            _status = status;
            _fio = fio;
            _create_date = create_date;
            _garNumber = garNumber;
        }
        #endregion

        #region Свойства
        public int GarNumber { get { return _garNumber; } set { OnPropertyChanged(ref _garNumber, value, () => GarNumber); } }
        public string CarNomer
        {
            get { return _car_nomer; }
            set
            {
                OnPropertyChanged(ref _car_nomer, value, () => CarNomer);
            }
        }
        public string ObjName
        {
            get { return _obj_name; }
            set
            {
                OnPropertyChanged(ref _obj_name, value, () => ObjName);
            }
        }
        public string TypeName
        {
            get { return _type_name; }
            set
            {
                OnPropertyChanged(ref _type_name, value, () => TypeName);
            }
        }
        public EStatusTask Status
        {
            get { return _status; }
            set
            {
                OnPropertyChanged(ref _status, value, () => Status);
            }
        }       
        public TaskGroupM Group { get; set; }
        public string GroupName
        {
            get
            {
                return String.Format("{0} ({1} — {2})", Group.GroupName, Group.FromTime, Group.TillTime);
            }
        }
        public string FIO
        {
            get { return _fio; }
        }
        public DateTime CreateDate
        {
            get { return _create_date; }
        }
        #endregion
    }
    public enum EStatusTask { assigned = 0, in_progress = 1, performed = 2, overdue = 3 }
}
