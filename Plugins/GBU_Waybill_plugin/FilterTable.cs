using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces.FastReport;

namespace GBU_Waybill_plugin
{
    class FilterTable : IFilterTable
    {
        private int _id_obj;
        private int _id_table;
        private string _order;
        private string _where;
        public FilterTable(int id_obj, int id_table, string order, string where)
        {
            this._id_obj = id_obj;
            this._id_table = id_table;
            this._order = order;
            this._where = where;
        }
        public int IdObj
        {
            get { return this._id_obj; }
        }

        public int IdTable
        {
            get { return this._id_table; }
        }

        public string Order
        {
            get { return this._order; }
        }

        public string Where
        {
            get { return this._where; }
        }
    }
}
