using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;


namespace Rekod.DataAccess.SourcePostgres.Model.PgAttributes
{
    public enum attributeTypeChange { Create, Change, Delete }
    public class PgAttributeEventArgs : EventArgs
    {
        #region Поля
        AbsM.ITableBaseM _table;
        object _id;
        attributeTypeChange _typeChange;
        #endregion // Поля

        #region Свойства
        public AbsM.ITableBaseM Table
        {
            get { return _table; }
        }
        public object Id
        {
            get { return _id; }
        }
        public attributeTypeChange TypeChange
        {
            get { return _typeChange; }
        }
        #endregion // Свойства

        #region Конструктор
        public PgAttributeEventArgs(AbsM.ITableBaseM table, object id, attributeTypeChange typeChange)
        {
            _table = table;
            _id = id;
            _typeChange = typeChange;
        }
        #endregion // Конструктор

    }
}
