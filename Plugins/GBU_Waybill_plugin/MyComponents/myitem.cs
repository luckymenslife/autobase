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
        private int _order;
        public myItem(string NameItem, int IdItem, object Data = null)
        {
            _name = NameItem;
            _id = IdItem;
            _data = Data;
            _order = 1;
        }
        public myItem(string NameItem, int IdItem, int Order, object Data = null)
        {
            _name = NameItem;
            _id = IdItem;
            _data = Data;
            _order = Order;
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
        public int GetOrder
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
            }
        }
        public int SetOrder
        {
            set
            {
                _order = value;
            }
        }
        public object Data
        {
            get { return this._data; }
        }
    }
}
