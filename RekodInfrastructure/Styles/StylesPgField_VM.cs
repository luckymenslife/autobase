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
using Rekod.Services;

namespace axVisUtils.Styles
{
    public class StylesPgField_VM : StylesVM
    {
        #region Поля
        private tablesInfo _table;
        private fieldInfo _field;
        private tablesInfo _tableStyle;
        private List<fieldInfo> _listField;
        #endregion

        #region Конструктор
        /// <summary>
        /// Использует ифномацию о таблице Pg
        /// </summary>
        /// <param name="table"></param>
        public StylesPgField_VM(tablesInfo table, fieldInfo field)
            : base()
        {
            _type = eTypeStyle.PgLayer;
            _table = table;
            string name = _table.style_field;

            _field = field;
            if (_field.ref_table != null)
            {
                _tableStyle = classesOfMetods.getTableInfo(_field.ref_table.Value);
                _listField = classesOfMetods.getFieldInfoTable(_field.ref_table.Value);
            }
            _geomType = classesOfMetods.GetIntGeomType(table.GeomType_GC);
        }
        #endregion Конструктор

        /// <summary>
        /// Получаем список объектов стилей
        /// </summary>
        public override void GetListStyles()
        {
            _listStyles.Clear();
            if (_tableStyle == null || _field.ref_table == null)
            {
                return;
            }
            var refFieldId = classesOfMetods.getFieldInfo((int)_field.ref_field);
            var refFieldName = classesOfMetods.getFieldInfo((int)_field.ref_field_name);

            using (SqlWork sqlCmd = new SqlWork())
            {
                string styleSQL = "", pictureSQL = "";
                if (_field.is_style)
                {
                    styleSQL = @",
                                    fontname, 
                                    fontcolor, 
                                    fontframecolor, 
                                    fontsize, 
                                    symbol, 
                                    pencolor, 
                                    pentype, 
                                    penwidth, 
                                    brushbgcolor, 
                                    brushfgcolor, 
                                    brushstyle, 
                                    brushhatch";

                    if (!String.IsNullOrEmpty(_table.imageColumn))
                    {
                        pictureSQL = ", " + _table.imageColumn;
                    }
                }

                sqlCmd.sql = string.Format(@"
                                SELECT
                                        {0}, 
                                        {1} {2} {3}
                                FROM    {4}.{5} 
                                ORDER BY {1};"
                            ,
                        refFieldId.nameDB,
                        refFieldName.nameDB,
                        styleSQL,
                        pictureSQL,
                        _tableStyle.nameSheme,
                        _tableStyle.nameDB
                        );
                sqlCmd.ExecuteReader();

                while (sqlCmd.CanRead())
                {
                    var dic = new StylesM(sqlCmd, _geomType, _field.is_style);
                    dic.Id = sqlCmd.GetInt32(refFieldId.nameDB);
                    dic.Name = sqlCmd.GetString(refFieldName.nameDB);

                    if (!String.IsNullOrEmpty(_table.imageColumn))
                    {
                        dic.FileName = sqlCmd.GetString(_table.imageColumn);
                    } 

                    dic.SetPreviewStyle();
                    _listStyles.Add(dic);
                }
            }
        }
       
    }

}
