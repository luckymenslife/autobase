using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces.FastReport;

namespace Rekod.FastReportClasses
{
    public class TableModel_M
    {
        #region Поля
        private Interfaces.tablesInfo _table;
        private string _sql;
        private enTypeReport _type;
        private IFilterTable _filter;
        private bool _isFileTable;
        #endregion // Поля

        #region Свойства
        public Interfaces.tablesInfo Table
        { get { return _table; } }
        public string Sql
        {
            get { return _sql; }
            set { _sql = value; }
        }
        public enTypeReport Type
        { get { return _type; } }
        public IFilterTable Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
        public bool IsFileTable
        { get { return _isFileTable; } }
        #endregion // Свойства

        #region Конструктор
        public TableModel_M(Interfaces.tablesInfo table, enTypeReport type, bool isFileTable = false)
        {
            _table = table;
            _type = type;
            _isFileTable = isFileTable;
        }
        #endregion // Конструктор
    }
}
