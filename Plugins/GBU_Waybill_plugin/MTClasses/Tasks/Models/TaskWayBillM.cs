using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class TaskWayBillM:ViewModelBase
    {
        #region Поля
        private int _id;
        private int _id_wb;
        private int _id_obj;
        private int _id_type;
        private int _id_mt;
        private ETypeTask _type;
        #endregion

        #region Конструктор
        public TaskWayBillM(int id, int id_wb, int id_obj, int id_type, int id_mt, ETypeTask type)
        {
            _id = id;
            _id_obj = id_obj;
            _id_type = id_type;
            _id_wb = id_wb;
            _id_mt = id_mt;
            _type = type;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public int IdWb
        {
            get { return _id_wb; }
        }
        public int IdTaskObj
        {
            get { return _id_obj; }
        }
        public int IdType
        {
            get { return _id_type; }
        }
        public object IdMT
        {
            get { return _id_mt; } 
        }
        public ETypeTask TypeTask
        {
            get { return _type; }
        }
        #endregion

        
    }
}
