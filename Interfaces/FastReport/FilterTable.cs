using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces.FastReport;

namespace Interfaces.FastReport
{
    public class FilterTable : IFilterTable
    {
        #region Поля
        private int _idTable;
        private int _idObj;
        private string _where;
        private string _order;
        #endregion // Поля

        #region Свойства

        public int IdTable
        {
            get { return _idTable; }
        }

        public int IdObj
        {
            get { return _idObj; }
        }

        public string Where
        {
            get { return _where; }
        }
        public string Order
        {
            get { return _order; }
        }
        #endregion // Свойства

        #region Конструктор
        public FilterTable(int idTable, int idObj, string where, string order = "")
        {
            _idTable = idTable;
            _idObj = idObj;
            _where = where;
            _order = order;
        }

        public FilterTable()
        {
            _idTable = 0;
            _idObj = 0;
            _where = "1 = 1";
            _order = "";
        }

        #endregion // Конструктор
    }
}
