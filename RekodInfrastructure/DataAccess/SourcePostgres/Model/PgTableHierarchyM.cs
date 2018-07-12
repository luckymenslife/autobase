using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgTableHierarchyM : PgTableBaseM
    {
        public List<PgTableHierarchyM> Children
        {
            get;
            set;
        }

        #region Конструкторы
        public PgTableHierarchyM()
            : base(null, Convert.ToInt32(Program.srid), ETableType.CommonType)
        { }
        public PgTableHierarchyM(IDataRepositoryM source, int? srid, ETableType type) :
            base(source, srid, type)
        { }
        public PgTableHierarchyM(IDataRepositoryM source, Int32 id, int? srid, ETableType type)
            : base(source, id, srid, type)
        {
            _id = id;
        }
        #endregion // Конструкторы
    }
}
