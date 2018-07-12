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
    public class StylesVMP_VM : StylesVM
    {
        #region Поля
        private mvMapLib.mvLayer _layer;
        #endregion

        #region Конструктор
        /// <summary>
        /// Использует информацию VMP
        /// </summary>
        /// <param name="layer"></param>
        public StylesVMP_VM(mvMapLib.mvLayer layer)
            : base()
        {
            _type = eTypeStyle.VMP;
            _layer = layer;

            var wkt = _layer.getObjectByNum(1).getWKT();
            _geomType = classesOfMetods.GetIntGeomType(wkt);
        }

        #endregion // Конструктор

        /// <summary>
        /// Получаем список объектов стилей
        /// </summary>
        public override void GetListStyles()
        {
            if (_layer.uniform)
            {
                var mvStyle = _layer.getUniformStyle();
                var style = new StylesM(mvStyle, _geomType);
                style.Name = Rekod.Properties.Resources.Styles_Uniformly;
                style.SetPreviewStyle();
                _listStyles.Add(style);
            }
            else
            {
                var count = _layer.StylesCount;
                for (int i = 0; i < count; i++)
                {
                    var mvStyle = _layer.getStyle(i);
                    var style = new StylesM(mvStyle, _geomType);
                    style.Name = string.Format(Rekod.Properties.Resources.Styles_Layer_n, i+1);
                    style.SetPreviewStyle();
                    _listStyles.Add(style);
                }
            }

        }

    }

}
