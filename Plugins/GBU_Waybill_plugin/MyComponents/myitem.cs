using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin
{
    public class myItem
    {
        private string _name;
        private int _id;
        private object _data;
        public myItem(string NameItem, int IdItem, object Data = null)
        {
            _name = NameItem;
            _id = IdItem;
            _data = Data;
        }
        public override string ToString()
        {
            return _name;
        }
        public String Name
        {
            get { return this._name; }
        }
        public int GetId
        {
            get
            {
                return _id;
            }
        }
        public object Data
        {
            get { return this._data; }
        }
    }
}
