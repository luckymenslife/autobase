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
    public class StylesPgTable_VM : StylesVM
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
        public StylesPgTable_VM(tablesInfo table)
            : base()
        {
            _type = eTypeStyle.PgLayer;
            _table = table;
            string name = _table.style_field;

            _field = classesOfMetods.getFieldInfoTable(_table.idTable).FirstOrDefault(f => f.nameDB == name);
            if (_field.ref_table != null)
            {
                _tableStyle = classesOfMetods.getTableInfo(_field.ref_table.Value);
                _listField = classesOfMetods.getFieldInfoTable(_field.ref_table.Value);
            }
            _geomType = classesOfMetods.GetIntGeomType(table.GeomType_GC);
        }
        #endregion // Конструктор

        /// <summary>
        /// Получаем список объектов стилей
        /// </summary>
        public override void GetListStyles()
        {
            var style = _table.Style;
            if (style.DefaultStyle)
            {
                _listStyles.Clear();
                var range = style.Range;
                if (!range.RangeColors || range.RangeColumn == null)
                {
                    var dic = new StylesM(_table.Style, _geomType);
                    
                    dic.Name = Rekod.Properties.Resources.Styles_Uniformly;
                    dic.SetPreviewStyle();
                    //dic.Tag = _table;
                    _listStyles.Add(dic);
                }
                else
                {
                    var dic = new StylesM(_table.Style, _geomType);
                    dic.Name = Rekod.Properties.Resources.Styles_Range;
                    dic.SetPreviewStyle(true);
                    //dic.Tag = _table;
                    _listStyles.Add(dic);
                }
            }
            else
            {
                if (_tableStyle == null || _field.ref_table == null)
                {
                    return;
                }
                _listStyles.Clear();
                var refFieldId = classesOfMetods.getFieldInfo((int)_field.ref_field);
                var refFieldName = classesOfMetods.getFieldInfo((int)_field.ref_field_name);
                var refTable = classesOfMetods.getTableInfo((int)_field.ref_table);

                using (SqlWork sqlCmd = new SqlWork())
                {
                    string pictureSQL = "";
                    if (!String.IsNullOrEmpty(_table.imageColumn))
                    {
                        pictureSQL = ", " + _table.imageColumn;
                    }

                    sqlCmd.sql = string.Format(@"
                                SELECT
                                        {0},
                                        {1}, 
                                        fontname,
                                        fontcolor,
                                        fontframecolor,
                                        fontsize,
                                        symbol,
                                        pencolor,
                                        penwidth,
                                        pentype,
                                        brushbgcolor,
                                        brushfgcolor,
                                        brushstyle,
                                        brushhatch
                                        {4}
                                FROM    {2}.{3} 
                                ORDER BY {1};"
                                ,
                            refFieldId.nameDB,
                            refFieldName.nameDB,
                            _tableStyle.nameSheme,
                            _tableStyle.nameDB,
                            pictureSQL
                            );
                    sqlCmd.ExecuteReader();

                    while (sqlCmd.CanRead())
                    {
                        var dic = new StylesM(sqlCmd, _geomType);
                        dic.Id = sqlCmd.GetInt32(refFieldId.nameDB);
                        dic.Name = sqlCmd.GetString(refFieldName.nameDB);

                        if (!String.IsNullOrEmpty(_table.imageColumn))
                        {
                            dic.FileName = sqlCmd.GetString(_table.imageColumn);
                        } 

                        dic.SetPreviewStyle();
                        dic.Tag = _tableStyle;
                        _listStyles.Add(dic);
                    }
                    sqlCmd.Close();
                }
            }
        }
    }

}
