using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgStyleM
    {
        #region Поля
        PgM.PgStyleLableM _lable;
        PgM.PgStyleLayerM _layer;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Стиль подписей
        /// </summary>
        public PgM.PgStyleLableM Lable
        {
            get { return _lable; }
            set { _lable = value; }
        }
        /// <summary>
        /// Стиль слоев
        /// </summary>
        public PgM.PgStyleLayerM Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }
        #endregion // Свойства
    }
}
