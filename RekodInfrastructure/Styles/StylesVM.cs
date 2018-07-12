using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Rekod;
using Rekod.DBTablesEdit;

namespace axVisUtils.Styles
{
    public abstract class StylesVM
    {
        protected enum eTypeStyle
        {
            PgLayer,
            VMP,
            StyleField
        }
        #region Поля
        protected eTypeStyle _type;
        protected int _geomType;
        protected List<StylesM> _listStyles;
        private mvMapLib.mvLayer _layer;
        #endregion

        #region Свойства
        public IEnumerable<StylesM> ListStyles
        {
            get { return _listStyles; }
        }
        #endregion

        #region Конструктор
        public StylesVM()
        {
            _listStyles = new List<StylesM>();
        }
        
        #endregion // Конструктор

        /// <summary>
        /// Получаем список объектов стилей
        /// </summary>
        public abstract void GetListStyles();
    }

}
