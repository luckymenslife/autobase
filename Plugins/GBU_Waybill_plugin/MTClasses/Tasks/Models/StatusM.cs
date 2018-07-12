using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class StatusM : ViewModelBase
    {
        #region Поля
        private int _id;
        private string _name;
        #endregion
        #region Конструктор
        public StatusM(int id, string name)
        {
            this._id = id;
            this._name = name;
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
        }
        #endregion
    }
}
