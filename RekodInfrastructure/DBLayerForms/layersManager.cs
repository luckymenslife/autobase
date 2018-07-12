using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AxmvMapLib;
using Npgsql;
using mvMapLib;
using System.Drawing;
using axVisUtils;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using axVisUtils.TableData;
using Rekod.Services;
using System.Linq;

namespace Rekod
{
    public class layersManager
    {
        private AxMapLIb mapLib;
        public mainFrm mainFrm1;

        public AxMapLIb MapLib
        {
            get { return mapLib; }
        }
        public struct DeltaColor
        {
            public double R;
            public double G;
            public double B;
        }
        public layersManager(mainFrm parentForm)
        {
            mapLib = parentForm.axMapLIb1;
            mainFrm1 = parentForm;
        }

        public void openTableGrid(int idT)//нужно
        {
            if (idT > -1)
            {
                var frm = new itemsTableGridForm(idT);
                frm.Show();
                frm.Activate();
            }
        }

        public void addPoint(int idT)//Нужно
        {
            //FormTableData FormAddPoint = new FormTableData(idT, -1);
            //FormAddPoint.Show();
            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(idT), -1, true, null, null);
        }
        public void addPolygon(int idT)//Нужно
        {
            //FormTableData FormAddPoly = new FormTableData(idT, -1);
            //FormAddPoly.Show();

            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(idT), -1, true, null, null);
        }
        public void addLine(int idT)//Нужно
        {
            //FormTableData FormAddLine = new FormTableData(idT, -1);
            //FormAddLine.Show();

            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(idT), -1, true, null, null);
        }

        private fieldArray[] loadFieldArray() //Используется 1 раз
        {
            fieldArray[] fa = new fieldArray[Program.field_info.Count];
            for (int i = 0; Program.field_info.Count > i; i++)
            {
                fa[i].idField = Program.field_info[i].idField;
                fa[i].nameDB = Program.field_info[i].nameDB;
                fa[i].nameMap = Program.field_info[i].nameMap;
            }
            return fa;
        }
        private tablesArray[] loadTablesArray()//Используется 1 раз
        {
            tablesArray[] fa = new tablesArray[Program.tables_info.Count];
            for (int i = 0; Program.tables_info.Count > i; i++)
            {
                fa[i].idTable = Program.tables_info[i].idTable;
                fa[i].nameDB = Program.tables_info[i].nameDB;
                fa[i].nameMap = Program.tables_info[i].nameMap;
            }
            return fa;
        }
        private string getTableNameDB(tablesArray[] fa, int id)
        {

            for (int i = 0; fa.Length > i; i++)
            {
                if (fa[i].idTable == id)
                {
                    return fa[i].nameDB;
                }
            }
            return null;
        }

        public void loadLayerFromSource(int idT)//Нужно
        {
            tablesInfo ti = classesOfMetods.getTableInfo(idT);
            if (ti.idTable != 0)
            {
                String nameInBd = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                if (mapLib.getLayer(nameInBd) != null)
                {
                    mapLib.getLayer(nameInBd).Update();
                    return;
                }
                mapLib.mapRepaint();
                var c = (NpgsqlConnectionStringBuilder)Program.connString;
                if (Program.postgres == null)
                {
                    Program.postgres = mapLib.CreateExternalSource(mvExternalSourceTypes.mvPostGIS);

                    var str = string.Format("host={0} port={1} user={2} password={3} dbname={4}",
                        c.Host,
                        c.Port,
                        c.UserName,
                        c["Password"],
                        c.Database);

                    Program.postgres.prepare(str, Convert.ToInt32(Program.srid));
                    Program.postgres.Connect();
                    Program.postgres.logFile("mv_postgre.log");
                }
                else
                {
                    if (!Program.postgres.isConnected())
                    {
                        var str = string.Format("host={0} port={1} user={2} password={3} dbname={4}",
                           c.Host,
                           c.Port,
                           c.UserName,
                           c["Password"],
                           c.Database);

                        Program.postgres.prepare(str, Convert.ToInt32(Program.srid));
                        Program.postgres.Connect();
                    }
                }
                try
                {
                    var style = ti.Style;
                    Program.RelationVisbleBdUser.addRelation(ti.nameMap, "#" + ti.idTable + ": " + this.RemoveBadSymbols(ti.nameMap), ti.idTable);
                    mvLayer layer = null;
                    string lable_name = "";
                    if (String.IsNullOrEmpty(ti.lableFieldName))
                    {
                        lable_name = "";
                    }
                    else
                    {
                        lable_name = "(" + ti.lableFieldName + ")::text";
                    }
                    if (style.DefaultStyle)
                    {
                        if (!style.Range.RangeColors || style.Range.RangeColumn == null)
                        {
                            layer = Program.postgres.addLayer(ti.nameSheme + "." + ti.nameDB,
                                Program.RelationVisbleBdUser.GetNameInBd(ti.idTable), ti.pkField, ti.geomFieldName,
                                lable_name, "", "", "", "", "");

                        }
                        else
                        {
                            var range = style.Range;
                            int type_field = 1;
                            int min_val = 0;

                            SqlWork sqlCmd = new SqlWork();
                            sqlCmd.sql = @"SELECT type_field FROM " + Program.scheme + @".table_field_info 
                                    WHERE id_table = " + ti.idTable.ToString() + " AND name_db like '" + range.RangeColumn + "'";
                            sqlCmd.Execute(false);
                            if (sqlCmd.CanRead())
                            {
                                type_field = sqlCmd.GetInt32(0);
                            }
                            sqlCmd.Close();

                            if (range.MinValueUse)
                            {
                                min_val = range.MinValue;
                            }
                            else
                            {
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "SELECT (min(" + range.RangeColumn + ")*10^" + (ti.precision_point).ToString() +
                                    ")::INTEGER FROM " + ti.nameSheme + "." + ti.nameDB;
                                sqlCmd.Execute(false);
                                if (sqlCmd.CanRead())
                                {
                                    min_val = sqlCmd.GetInt32(0);
                                }
                                sqlCmd.Close();
                            }

                            if (type_field == 1)
                            {
                                nameInBd = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                                layer = Program.postgres.addLayer(ti.nameSheme + "." + ti.nameDB,
                                        nameInBd, ti.pkField, ti.geomFieldName,
                                        lable_name, "(" + range.RangeColumn + "-(" + min_val.ToString() + "))::INTEGER", "", "", "", "");
                            }
                            else
                            {
                                nameInBd = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                                layer = Program.postgres.addLayer(ti.nameSheme + "." + ti.nameDB,
                                        nameInBd, ti.pkField, ti.geomFieldName,
                                        lable_name, "((" + range.RangeColumn + "*10^" + (ti.precision_point).ToString() +
                                        ")-(" + min_val.ToString() + "))::INTEGER",
                                        "", "", "", "");

                            }
                        }
                    }
                    else
                    {
                        string intervalSql = getIntervalSelect(idT);
                        if (intervalSql == "")
                        {
                            var fields = classesOfMetods.getFieldInfoTable(ti.idTable);
                            var styleField = fields.FirstOrDefault(w => w.nameDB == ti.style_field);
                            if (styleField.ref_table != null
                                && styleField.ref_field != null)
                            {
                                var refTable = classesOfMetods.getTableInfo((int)styleField.ref_table);
                                var refField = classesOfMetods.getFieldInfo((int)styleField.ref_field);

                                String imageSql = "";

                                if (!String.IsNullOrEmpty(ti.imageColumn)
                                    && ti.GeomType_GC.ToUpper().Contains("POINT"))
                                {
                                    imageSql = String.Format("(SELECT {0} FROM {1}.{2} WHERE {1}.{2}.{3} = {4}.{5}.{6}) as RSfiles",
                                               ti.imageColumn, refTable.nameSheme, refTable.nameDB, refField.nameDB, ti.nameSheme, ti.nameDB, ti.style_field);
                                }

                                layer = Program.postgres.addLayer(
                                    tablename: ti.nameSheme + "." + ti.nameDB,
                                    mvname: Program.RelationVisbleBdUser.GetNameInBd(ti.idTable),
                                    PKfield: ti.pkField,
                                    GeomField: ti.geomFieldName,
                                    LabelField: lable_name,
                                    StyleField: "(" + ti.style_field + ")::INTEGER",
                                    GeoLinkField: "",
                                    imageField: imageSql,
                                    ImageXField: ti.angleColumn,
                                    Filter: "");

                                classesOfMetods.ReloadRelatedTables(refTable.idTable);
                            }
                        }
                        else
                        {
                            layer = Program.postgres.addLayer(ti.nameSheme + "." + ti.nameDB,
                                Program.RelationVisbleBdUser.GetNameInBd(ti.idTable), ti.pkField, ti.geomFieldName,
                                lable_name, intervalSql, "", ti.imageColumn, ti.angleColumn, "");
                        }
                    }


                    //loadStyle(layer, ti.idTable);
                    if (layer != null)
                    {
                        layer.selectable = false;
                        layer.MinObjectSize = ti.MinObjectSize;
                        //if (!String.IsNullOrEmpty(ti.imageColumn)
                        //    || ti.GeomType_GC.ToUpper().Contains("POINT"))
                        //{
                        loadStyle(layer, ti.idTable);
                        //}
                        if (ti.useBounds)
                        {
                            layer.usebounds = ti.useBounds;
                            layer.MaxScale = Convert.ToUInt32(ti.maxScale);
                            layer.MinScale = Convert.ToUInt32(ti.minScale);
                        }
                        if (!String.IsNullOrEmpty(ti.lableFieldName))
                        {
                            layer.showlabels = true;
                        }
                        mapLib.mapRepaint();
                    }

                    if (layer != null)
                    {
                        if (ti.label_showlabel)
                        {
                            layer.showlabels = true;
                            SetLabelStyle(idT);
                        }
                        else
                        {
                            layer.showlabels = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.mainFrm1.StatusInfo = Rekod.Properties.Resources.LM_ErrorLayerLoad + " " + ex.Message + "";
                }
            }
        }
        private string RemoveBadSymbols(string layerName)
        {
            List<Char> newLayerName = new List<char>();
            foreach (var item in layerName)
            {
                if ((item >= 'a' && item <= 'z')
                    || (item >= 'A' && item <= 'Z')
                    || (item >= '0' && item <= '9')
                    || (item >= 'а' && item <= 'я')
                    || (item >= 'А' && item <= 'Я'))
                {
                    newLayerName.Add(item);
                }
                else
                {
                    newLayerName.Add('_');
                }

            }

            return new string(newLayerName.ToArray());
        }
        // Установка стиля подписей в соответствии со значениями в базе. Значения берутся из tables_info
        public void SetLabelStyle(int idT)//Нужно
        {
            tablesInfo ti = classesOfMetods.getTableInfo(idT);
            String layerName = Program.RelationVisbleBdUser.GetNameInBd(idT);
            mvLayer layer = Program.app.mapLib.getLayer(layerName);
            try
            {
                mvFontObject mvfontObject = Program.app.mapLib.createFontObject();
                mvfontObject.Color = ti.label_fontcolor;
                mvfontObject.fontname = ti.label_fontname;
                mvfontObject.size = ti.label_fontsize;
                mvfontObject.strikeout = ti.label_fontstrikeout;
                mvfontObject.italic = ti.label_fontitalic;
                mvfontObject.underline = ti.label_fontunderline;
                mvfontObject.graphicUnits = ti.label_graphicunits;
                if (ti.label_showframe)
                {
                    mvfontObject.framecolor = ti.label_framecolor;
                }
                else
                {
                    mvfontObject.framecolor = 0xFFFFFFFF;
                }
                if (ti.label_fontbold)
                {
                    mvfontObject.weight = 650;
                }
                if (ti.label_usebounds)
                {
                    layer.labelBounds = true;
                    layer.labelMinScale = ti.label_minscale;
                    layer.labelMaxScale = ti.label_maxscale;
                }
                layer.labelParallel = ti.label_parallel;
                layer.labelOverlap = ti.label_overlap;
                layer.labelOffset = ti.label_offset;
                layer.SetLabelstyle(mvfontObject);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Rekod.Properties.Resources.LM_ErorLoadStyleLabel + " " + ex.Message);
            }
            Program.app.mapLib.Update();
            Program.app.mapLib.mapRepaint();
        }

        private string getIntervalSelect(int idT)//Нужно
        {
            string temp_sql = "";
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT ref_table, ref_field, ref_field_end, name_db FROM sys_scheme.table_field_info " +
                "WHERE is_interval = TRUE AND is_style = TRUE AND id_table=" + idT;
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                int ref_table = sqlCmd.GetInt32(0);
                int ref_field_begin = sqlCmd.GetInt32(1);
                int ref_field_end = sqlCmd.GetInt32(2);
                string field_name = sqlCmd.GetValue<string>(3);
                temp_sql = "(SELECT itrv__t." + classesOfMetods.getTableInfo(ref_table).pkField +
                                " FROM " + classesOfMetods.getTableInfo(ref_table).nameSheme + "." +
                                classesOfMetods.getTableInfo(ref_table).nameDB + " itrv__t " +
                    "WHERE " + field_name + ">itrv__t." + classesOfMetods.getFieldInfo(ref_field_begin).nameDB +
                                " AND " + field_name + "<=itrv__t." + classesOfMetods.getFieldInfo(ref_field_end).nameDB +
                                " LIMIT 1)";
            }
            sqlCmd.Close();
            return temp_sql;
        }
        private DeltaColor getDeltaColor(Color clr1, Color clr2, int countStyle)//Нужно
        {
            DeltaColor temp;
            if (clr1.R - clr2.R != 0)
            {
                temp.R = ((double)(clr2.R - clr1.R)) / countStyle;
            }
            else
            {
                temp.R = 0;
            }
            if (clr1.G - clr2.G != 0)
            {
                temp.G = ((double)(clr2.G - clr1.G)) / countStyle;
            }
            else
            {
                temp.G = 0;
            }
            if (clr1.B - clr2.B != 0)
            {
                temp.B = ((double)(clr2.B - clr1.B)) / countStyle;
            }
            else
            {
                temp.B = 0;
            }
            return temp;
        }
        public static Color convColor(uint value)//Нужно
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb(r1, g1, b1);
            return c1;
        }
        public static uint convToUInt(Color clr)//Нужно
        {
            uint temp = 0;
            temp = Convert.ToUInt32(clr.R + (clr.G << 8) + (clr.B << 16));
            return temp;
        }
        private uint getColorForItem(Color clr, DeltaColor dColor, int i)//Нужно
        {
            int r = Convert.ToInt32(clr.R + (dColor.R * i));
            int g = Convert.ToInt32(clr.G + (dColor.G * i));
            int b = Convert.ToInt32(clr.B + (dColor.B * i));
            return convToUInt(Color.FromArgb(r, g, b));
        }
        public void loadStyle(mvLayer templayer, int idT)//не понятно
        {
            tablesInfo ti = classesOfMetods.getTableInfo(idT);
            var style = ti.Style;
            if (style.DefaultStyle)
            {
                var range = style.Range;
                if (!range.RangeColors || range.RangeColumn == null)
                {
                    templayer.uniform = true;
                    templayer.SetUniformStyle(getPenObjectDefault(idT), getmvBrushObjectDefault(idT), getSymbolDefault(idT), getFontObjectDefault(idT));
                }
                else
                {
                    templayer.uniform = false;
                    DeltaColor dColor = new DeltaColor();
                    string range_column = "";
                    int count_style = 0, min_val = 0, id_type_geom = 0, p_point = 1, max_val = 0;
                    mvMapLib.mvSymbolObject Symbol = new mvMapLib.mvSymbolObject();
                    mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
                    mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
                    mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();
                    id_type_geom = classesOfMetods.GetIntGeomType(classesOfMetods.getTableInfo(idT).GeomType_GC);
                    List<int> idDBList = new List<int>();
                    //узнаем колонку для вычисления стилей
                    using (var sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "SELECT range_column FROM " + Program.scheme + ".table_info WHERE id = " + idT;
                        sqlCmd.Execute(false);
                        if (sqlCmd.CanRead())
                        {
                            range_column = sqlCmd.GetValue<string>(0);
                        }
                        sqlCmd.Close();
                    }
                    //узнаем какого типа колонка
                    int type_field = 1;
                    using (var sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = @"SELECT type_field FROM sys_scheme.table_field_info 
                                    WHERE id_table = " + ti.idTable
                                                           + " AND name_db like '" + range.RangeColumn + "'";
                        sqlCmd.Execute(false);
                        if (sqlCmd.CanRead())
                        {
                            type_field = sqlCmd.GetInt32(0);
                        }
                        sqlCmd.Close();
                    }
                    //присваем правильное значение параметру кол знаков после запятой
                    if (type_field == 1)
                    {
                        p_point = 0;
                    }
                    else
                    {
                        p_point = ti.precision_point;
                    }
                    if (range_column != "")
                    {
                        //узнаем количество градаций
                        Color tempColor = new Color();
                        string min_val_str = "", max_val_str = "";
                        if (ti.Style.Range.MinValueUse)
                        {
                            min_val_str = ti.Style.Range.MinValue.ToString();
                        }
                        else
                        {
                            min_val_str = "min(" + range_column + ")*10^" + p_point;
                        }
                        if (ti.Style.Range.MaxValueUse)
                        {
                            max_val_str = ti.Style.Range.MaxValue.ToString();
                        }
                        else
                        {
                            max_val_str = "max(" + range_column + ")*10^" + p_point;
                        }
                        //узнаем минимальное и максимальное значение, а так же количество градаций
                        using (var sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = String.Format("SELECT ({0}-({1}))::INTEGER as count, ({1})::INTEGER as min, ({4})::INTEGER as max FROM {2}.{3};",
                                max_val_str, min_val_str, ti.nameSheme, ti.nameDB, max_val_str);
                            sqlCmd.Execute(false);
                            if (sqlCmd.CanRead())
                            {
                                count_style = sqlCmd.GetInt32("count");
                                min_val = sqlCmd.GetInt32("min");
                                max_val = sqlCmd.GetInt32("max");
                            }
                            sqlCmd.Close();
                        }
                        //получаем коэффициент изменения цвета на каждом шаге
                        uint fontcolor = 0, fontframecolor = 0, pencolor = 0, brushbgcolor = 0, brushfgcolor = 0;
                        int typeColor = 0;
                        using (var sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = "SELECT min_color, max_color, type_color, fontcolor, fontframecolor, pencolor, brushbgcolor, brushfgcolor FROM "
                                + Program.scheme + ".table_info WHERE id = " + idT.ToString();
                            sqlCmd.Execute(false);


                            if (sqlCmd.CanRead())
                            {
                                tempColor = convColor(sqlCmd.GetValue<uint>(0));
                                dColor = getDeltaColor(convColor(sqlCmd.GetValue<uint>(0)),
                                    convColor(sqlCmd.GetValue<uint>(1)), count_style);
                                typeColor = sqlCmd.GetValue<int>(2);
                                fontcolor = sqlCmd.GetValue<uint>(3);
                                fontframecolor = sqlCmd.GetValue<uint>(4);
                                pencolor = sqlCmd.GetValue<uint>(5);
                                brushbgcolor = sqlCmd.GetValue<uint>(6);
                                brushfgcolor = sqlCmd.GetValue<uint>(7);
                            }
                            sqlCmd.Close();
                        }
                        //получаем список значений для постороения градаций
                        using (var sqlCmd = new SqlWork())
                        {
                            //sqlCmd.sql = String.Format("SELECT (({0}*10^{1})-({2}))::INTEGER FROM {3}.{4} WHERE (({0}*10^{1})+1)>=({2}) ORDER BY {0}",
                            sqlCmd.sql = String.Format("SELECT (({0}*10^{1})-({2}))::INTEGER FROM {3}.{4} WHERE (({0}*10^{1}))>=({2}) AND (({0}*10^{1}))<=({5}) ORDER BY {0}",
                                range_column, p_point, min_val, ti.nameSheme, ti.nameDB, max_val);
                            sqlCmd.Execute(false);
                            while (sqlCmd.CanRead())
                            {
                                idDBList.Add(sqlCmd.GetInt32(0));
                                // MessageBox.Show(getColorForItem(tempColor, dColor, Program.sqlCmd.GetInt32(0)).ToString());
                            }
                            sqlCmd.Close();
                        }
                        // формирование стилей
                        for (int t = 0; idDBList.Count > t; t++)
                        {
                            switch (id_type_geom)
                            {
                                /*Информация про typeColor
                                 * Основного 0
                                 * Фона 1
                                 * Границ или линий 2
                                 * Каймы 3
                                */
                                case 1:
                                    Symbol = getSymbolDefault(idT);
                                    font = getFontObjectDefault(idT);
                                    font.Color = fontcolor;
                                    font.framecolor = fontframecolor;
                                    if (typeColor == 0)
                                        font.Color = getColorForItem(tempColor, dColor, idDBList[t]);
                                    if (typeColor == 3)
                                        font.framecolor = getColorForItem(tempColor, dColor, idDBList[t]);
                                    templayer.AddExtStyle(idDBList[t], templayer.CreateDotStyle(Symbol, font));
                                    break;
                                case 2:
                                    pen = getPenObjectDefault(idT);
                                    pen.Color = getColorForItem(tempColor, dColor, idDBList[t]);
                                    templayer.AddExtStyle(idDBList[t], templayer.CreateLineStyle(pen));
                                    break;
                                case 3:
                                    pen = getPenObjectDefault(idT);
                                    brush = getmvBrushObjectDefault(idT);
                                    var color = getColorForItem(tempColor, dColor, idDBList[t]);
                                    pen.Color = pencolor;
                                    brush.bgcolor = brushbgcolor;
                                    brush.fgcolor = brushfgcolor;
                                    if (typeColor == 0)
                                        brush.fgcolor = color;
                                    if (typeColor == 1)
                                        brush.bgcolor = color;
                                    if (typeColor == 2)
                                        pen.Color = color;
                                    templayer.AddExtStyle(idDBList[t], templayer.CreatePolygonStyle(pen, brush));
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                bool is_ref = false;
                int? ref_table = null, ref_field = null, ref_field_end = null, ref_field_name = null;
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT id, name_db, name_map, is_reference, is_interval, ref_table, ref_field, ref_field_end, ref_field_name " +
                        "FROM " + Program.scheme + ".table_field_info " +
                        "WHERE is_style = true AND id_table = " + idT;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        is_ref = sqlCmd.GetBoolean(3);
                        ref_table = sqlCmd.GetValue<int?>(5);
                        ref_field = sqlCmd.GetValue<int?>(6);
                        ref_field_end = sqlCmd.GetValue<int?>(7);
                        ref_field_name = sqlCmd.GetValue<int?>(8);
                    }
                    sqlCmd.Close();
                }

                if (ref_table != null)
                {
                    var userRight = Program.tables_right.FirstOrDefault(p => p.id_table == ref_table);
                    if (userRight == null || userRight.read == false)
                    {
                        throw new NullReferenceException("У вас нет прав на связанную таблицу");
                    }
                }

                fieldArray[] fa = loadFieldArray();
                tablesArray[] ta = loadTablesArray();

                mvMapLib.mvSymbolObject Symbol = new mvMapLib.mvSymbolObject();
                mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
                mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
                mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();

                int count = 0;
                if (ref_table == null)
                {
                    return;
                }
                using (var sqlCmd = new SqlWork())
                {
                    tablesInfo tInfo = classesOfMetods.getTableInfo((int)ref_table);
                    String tableNameDB = getTableNameDB(ta, (int)ref_table);
                    sqlCmd.sql = "SELECT count(*)::INTEGER as val FROM " + tInfo.nameSheme +
                        "." + tableNameDB;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        count = sqlCmd.GetInt32(0);
                    }
                    sqlCmd.Close();
                }

                int id_type_geom = 0;
                string id_name_type_geom = "", map_name = "";
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT ttg.id, ttg.name, ti.name_map " +
                        "FROM " + Program.scheme + ".table_info ti, " + Program.scheme + ".table_type_geom ttg WHERE ti.geom_type = ttg.id AND ti.id = " + idT;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        id_type_geom = sqlCmd.GetInt32(0);
                        id_name_type_geom = sqlCmd.GetValue<string>(1);
                        map_name = sqlCmd.GetValue<string>(2);
                    }
                    sqlCmd.Close();
                }

                objStyleRef[] styleRef = new objStyleRef[count];
                fieldInfo fi;
                tablesInfo ti_style;
                if (!is_ref)
                {
                    fi = classesOfMetods.getFieldInfoTable((int)ref_table).Find(w => w.nameDB == classesOfMetods.getTableInfo((int)ref_table).pkField);
                    ti_style = classesOfMetods.getTableInfo((int)ref_table);
                }
                else
                {
                    fi = classesOfMetods.getFieldInfo((int)ref_field);
                    ti_style = classesOfMetods.getTableInfo((int)ref_table);
                }
                using (var sqlCmd = new SqlWork())
                {

                    sqlCmd.sql = String.Format(@"SELECT exists_style, 
       fontname, fontcolor, fontframecolor, fontsize, symbol, pencolor, 
       pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch, " +
       "\"{2}\" FROM \"{0}\".\"{1}\"", ti_style.nameSheme, ti_style.nameDB, fi.nameDB);
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        switch (id_type_geom)
                        {
                            case 1:
                                Symbol = getSymbolRef(sqlCmd.GetValue<uint>("symbol"));
                                font = getFontObjectRef(sqlCmd.GetValue<string>("fontname"),
                                                sqlCmd.GetValue<uint>("fontcolor"),
                                                sqlCmd.GetValue<uint>("fontframecolor"),
                                                sqlCmd.GetValue<int>("fontsize"), ti_style.graphic_units);
                                templayer.AddExtStyle(sqlCmd.GetValue<int>(fi.nameDB), templayer.CreateDotStyle(Symbol, font));
                                break;
                            case 2:
                                pen = getPenObjectRef(sqlCmd.GetValue<uint>("pencolor"), sqlCmd.GetValue<uint>("penwidth"), sqlCmd.GetValue<ushort>("pentype"));
                                templayer.AddExtStyle(sqlCmd.GetValue<int>(fi.nameDB), templayer.CreateLineStyle(pen));
                                break;
                            case 3:
                                pen = getPenObjectRef(sqlCmd.GetValue<uint>("pencolor"), sqlCmd.GetValue<uint>("penwidth"), sqlCmd.GetValue<ushort>("pentype"));
                                brush = getmvBrushObjectRef(sqlCmd.GetValue<uint>("brushbgcolor"),
                                    sqlCmd.GetValue<uint>("brushfgcolor"),
                                    sqlCmd.GetValue<ushort>("brushstyle"),
                                    sqlCmd.GetValue<ushort>("brushhatch"));
                                templayer.AddExtStyle(sqlCmd.GetValue<int>(fi.nameDB), templayer.CreatePolygonStyle(pen, brush));
                                break;
                        }
                    }
                    sqlCmd.Close();
                }
                //int[] idList = getList(ti_style.nameDB, count, fi.nameDB);
                //for (int i = 0; idList.Length > i; i++)
                //{
                //    switch (id_type_geom)
                //    {
                //        case 1:
                //            Symbol = getSymbolRef(ti_style, fi, idList[i]);
                //            font = getFontObjectRef(ti_style, fi, idList[i]);
                //            templayer.AddExtStyle(idList[i], templayer.CreateDotStyle(Symbol, font));
                //            break;
                //        case 2:
                //            pen = getPenObjectRef(ti_style, fi, idList[i]);
                //            templayer.AddExtStyle(idList[i], templayer.CreateLineStyle(pen));
                //            break;
                //        case 3:
                //            pen = getPenObjectRef(ti_style, fi, idList[i]);
                //            brush = getmvBrushObjectRef(ti_style, fi, idList[i]);
                //            templayer.AddExtStyle(idList[i], templayer.CreatePolygonStyle(pen, brush));
                //            break;
                //    }
                //}
            }
        }

        private mvMapLib.mvSymbolObject getSymbolDefault(int idT)//Нужно
        {
            mvMapLib.mvSymbolObject symb = new mvMapLib.mvSymbolObject();
            bool exists = false;
            if (!Program.WorkSets.CurrentWorkSet.IsDefault)
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT symbol FROM " + Program.scheme + ".table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table = " + idT;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        symb.shape = Convert.ToUInt32(sqlCmd.GetInt32(0));
                        exists = true;
                    }
                    sqlCmd.Close();
                    if (exists)
                        return symb;
                }
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT symbol FROM " + Program.scheme + ".table_info WHERE id = " + idT;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    symb.shape = Convert.ToUInt32(sqlCmd.GetInt32(0));
                }
                sqlCmd.Close();
            }
            return symb;
        }
        private mvMapLib.mvPenObject getPenObjectDefault(int idT)//Нужно
        {
            mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
            bool exists = false;
            if (!Program.WorkSets.CurrentWorkSet.IsDefault)
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT pencolor, penwidth, pentype FROM " + Program.scheme + ".table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table = " + idT;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        exists = true;
                        pen = new mvMapLib.mvPenObject();
                        pen.Color = Convert.ToUInt32(sqlCmd.GetInt32(0));
                        pen.width = Convert.ToUInt32(sqlCmd.GetInt32(1));
                        pen.ctype = Convert.ToUInt16(sqlCmd.GetInt32(2));
                        exists = true;
                    }
                    sqlCmd.Close();
                }
                if (exists)
                    return pen;
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT pencolor, penwidth, pentype FROM " + Program.scheme + ".table_info WHERE id = " + idT;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    pen.Color = Convert.ToUInt32(sqlCmd.GetInt32(0));
                    pen.width = Convert.ToUInt32(sqlCmd.GetInt32(1));
                    pen.ctype = Convert.ToUInt16(sqlCmd.GetInt32(2));
                }
                sqlCmd.Close();
            }
            return pen;
        }
        private mvMapLib.mvFontObject getFontObjectDefault(int idT)//Нужно
        {
            mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
            bool exists = false;
            if (!Program.WorkSets.CurrentWorkSet.IsDefault)
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT fontname, fontcolor, fontframecolor, fontsize, graphic_units FROM " + Program.scheme + ".table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table = " + idT;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        font.fontname = sqlCmd.GetValue<string>(0);
                        font.Color = sqlCmd.GetValue<uint>(1);
                        font.framecolor = sqlCmd.GetValue<uint>(2);
                        font.size = sqlCmd.GetInt32(3);
                        font.graphicUnits = sqlCmd.GetBoolean(4);
                        exists = true;
                    }
                    sqlCmd.Close();
                    if (exists)
                        return font;
                }
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT fontname, fontcolor, fontframecolor, fontsize, graphic_units FROM " + Program.scheme + ".table_info WHERE id = " + idT;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    font.fontname = sqlCmd.GetValue<string>(0);
                    font.Color = sqlCmd.GetValue<uint>(1);
                    font.framecolor = sqlCmd.GetValue<uint>(2);
                    font.size = sqlCmd.GetInt32(3);
                    font.graphicUnits = sqlCmd.GetBoolean(4);
                }
                sqlCmd.Close();
            }
            return font;
        }
        private mvMapLib.mvBrushObject getmvBrushObjectDefault(int idT)//нужно
        {
            mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();
            bool exists = false;
            if (!Program.WorkSets.CurrentWorkSet.IsDefault)
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM " + Program.scheme + ".table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table = " + idT.ToString();
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        brush.bgcolor = sqlCmd.GetValue<uint>(0);
                        brush.fgcolor = sqlCmd.GetValue<uint>(1);
                        brush.style = sqlCmd.GetValue<ushort>(2);
                        brush.hatch = sqlCmd.GetValue<ushort>(3);
                        if (brush.style == 5 && brush.hatch == 1)
                        {
                            brush.style = 1;
                            brush.hatch = 0;
                        }
                        if (brush.style == 5 && brush.hatch == 2)
                        {
                            brush.style = 0;
                            brush.hatch = 0;
                        }
                        if (brush.style == 0) // прозрачность для сплошной заливки
                        {
                            int grey = (int)((brush.bgcolor >> 24) & 255);
                            brush.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
                        }
                        exists = true;
                    }
                    sqlCmd.Close();
                    if (exists)
                        return brush;
                }
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM " + Program.scheme + ".table_info WHERE id = " + idT.ToString();
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    brush.bgcolor = sqlCmd.GetValue<uint>(0);
                    brush.fgcolor = sqlCmd.GetValue<uint>(1);
                    brush.style = sqlCmd.GetValue<ushort>(2);
                    brush.hatch = sqlCmd.GetValue<ushort>(3);

                    if (brush.style == 5 && brush.hatch == 1)
                    {
                        brush.style = 1;
                        brush.hatch = 0;
                    }
                    if (brush.style == 5 && brush.hatch == 2)
                    {
                        brush.style = 0;
                        brush.hatch = 0;
                    }
                    if (brush.style == 0) // прозрачность для сплошной заливки
                    {
                        int grey = (int)((brush.bgcolor >> 24) & 255);
                        brush.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
                    }
                }
                sqlCmd.Close();
            }
            return brush;
        }

        private int[] getList(string tableName, int count, string styleField)//Нужно
        {
            int[] temp = new int[count];
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT " + styleField +
                " FROM " + classesOfMetods.getTableInfoOfNameDB(tableName).nameSheme + "." + tableName + " ORDER BY " + classesOfMetods.getTableInfoOfNameDB(tableName).pkField + ";";
            sqlCmd.Execute(false);
            int i = 0;
            while (sqlCmd.CanRead())
            {
                temp[i] = sqlCmd.GetInt32(0);
                i++;
            }
            sqlCmd.Close();
            return temp;
        }

        private mvMapLib.mvSymbolObject getSymbolRef(tablesInfo ti, fieldInfo fi, int idTR)//Нужно
        {
            mvMapLib.mvSymbolObject symb = new mvMapLib.mvSymbolObject();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT symbol FROM " + ti.nameSheme + "." + ti.nameDB + " WHERE " + fi.nameDB + " = " + idTR.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                symb.shape = Convert.ToUInt32(sqlCmd.GetInt32(0));
            }
            sqlCmd.Close();
            return symb;
        }
        private mvMapLib.mvSymbolObject getSymbolRef(uint shape)//Нужно
        {
            mvMapLib.mvSymbolObject symb = new mvMapLib.mvSymbolObject();
            symb.shape = shape;
            return symb;
        }
        private mvMapLib.mvFontObject getFontObjectRef(tablesInfo ti, fieldInfo fi, int idTR)//Нужно
        {
            mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT fontname, fontcolor, fontframecolor, fontsize FROM " + ti.nameSheme + "." + ti.nameDB + " WHERE " + fi.nameDB + " = " + idTR.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                font.fontname = sqlCmd.GetValue<string>(0);
                font.Color = sqlCmd.GetValue<uint>(1);
                font.framecolor = sqlCmd.GetValue<uint>(2);
                font.size = sqlCmd.GetInt32(3);
                font.graphicUnits = ti.graphic_units;
            }
            sqlCmd.Close();
            return font;
        }
        private mvMapLib.mvFontObject getFontObjectRef(String fontname, uint Color, uint framecolor, int size, bool graphicUnits)//Нужно
        {
            mvMapLib.mvFontObject font = new mvMapLib.mvFontObject();
            font.fontname = fontname;
            font.Color = Color;
            font.framecolor = framecolor;
            font.size = size;
            font.graphicUnits = graphicUnits;
            return font;
        }
        private mvMapLib.mvPenObject getPenObjectRef(tablesInfo ti, fieldInfo fi, int idTR)//Нужно
        {
            mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT pencolor, penwidth, pentype FROM " + ti.nameSheme + "." + ti.nameDB + " WHERE " + fi.nameDB + " = " + idTR.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                pen.Color = Convert.ToUInt32(sqlCmd.GetInt32(0));
                pen.width = Convert.ToUInt32(sqlCmd.GetInt32(1));
                pen.ctype = Convert.ToUInt16(sqlCmd.GetInt32(2));
            }
            sqlCmd.Close();
            return pen;
        }
        private mvMapLib.mvPenObject getPenObjectRef(uint Color, uint width, ushort ctype)//Нужно
        {
            mvMapLib.mvPenObject pen = new mvMapLib.mvPenObject();
            pen.Color = Color;
            pen.width = width;
            pen.ctype = ctype;
            return pen;
        }
        private mvMapLib.mvBrushObject getmvBrushObjectRef(tablesInfo ti, fieldInfo fi, int idTR)//нужно
        {
            mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM " + ti.nameSheme + "." + ti.nameDB + " WHERE " + fi.nameDB + " = " + idTR.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                brush.bgcolor = sqlCmd.GetValue<uint>(0);
                brush.fgcolor = sqlCmd.GetValue<uint>(1);
                brush.style = sqlCmd.GetValue<ushort>(2);
                brush.hatch = sqlCmd.GetValue<ushort>(3);

                if (brush.style == 5 && brush.hatch == 1)
                {
                    brush.style = 1;
                    brush.hatch = 0;
                }
                if (brush.style == 5 && brush.hatch == 2)
                {
                    brush.style = 0;
                    brush.hatch = 0;
                }
                if (brush.style == 0) // прозрачность для сплошной заливки
                {
                    int grey = (int)((brush.bgcolor >> 24) & 255);
                    brush.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
                }
            }
            sqlCmd.Close();
            return brush;
        }

        private mvMapLib.mvBrushObject getmvBrushObjectRef(uint bgcolor, uint fgcolor, ushort style, ushort hatch)//нужно
        {
            mvMapLib.mvBrushObject brush = new mvMapLib.mvBrushObject();

            brush.bgcolor = bgcolor;
            brush.fgcolor = fgcolor;
            brush.style = style;
            brush.hatch = hatch;

            if (brush.style == 5 && brush.hatch == 1)
            {
                brush.style = 1;
                brush.hatch = 0;
            }
            if (brush.style == 5 && brush.hatch == 2)
            {
                brush.style = 0;
                brush.hatch = 0;
            }
            if (brush.style == 0) // прозрачность для сплошной заливки
            {
                int grey = (int)((brush.bgcolor >> 24) & 255);
                brush.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
            }
            return brush;
        }

        public void removeDeletedObjectFromMap(string layer_name, int id_deleted_obj, int id_table)
        {
            mvMapLib.mvLayer ll = mapLib.getLayer(layer_name);
            if (ll != null)
            {
                ll.DeleteID(id_deleted_obj);
                ll.editable = true;
            }
        }

    }
}