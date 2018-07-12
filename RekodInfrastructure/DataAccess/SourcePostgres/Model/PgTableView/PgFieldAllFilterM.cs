using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.Model.PgTableView
{
    public class PgFieldAllFilterM : AbsM.IFieldM
    {
        public int Id
        {
            get { return 0; }
        }

        public object IdTable
        {
            get { return 0; }
        }

        public AbsM.ITableBaseM Table
        {
            get { return null; }
        }

        public string Name
        {
            get { return ""; }
        }

        public string Text
        {
            get { return "Все"; }
        }

        public AbsM.EFieldType Type
        {
            get { return AbsM.EFieldType.Text; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
    }
}