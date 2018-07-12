using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using Npgsql;
using System.Windows.Forms;
using System.Linq;
using Rekod.PluginSettings;
using Rekod.SQLiteSettings;
using axVisUtils.Styles;
using Interfaces;
using NpgsqlTypes;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Services;
using System.Diagnostics;

namespace Rekod
{
    class classesOfMetods
    {
        #region Работа с Формами
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);
        static public void SetFormOwner(Form frm)
        {
            if (Program.WinMain == null) return;
            var helper = new WindowInteropHelper(Program.WinMain);
            SetWindowLong(new HandleRef(frm, frm.Handle), -8, helper.Handle.ToInt32());
            frm.FormClosed += frm_FormClosed;
        }
        static void frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.WinMain.Activate();
        }
        public static void SetFormOwner(System.Windows.Forms.Form ownerForm, System.Windows.Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = ownerForm.Handle;
        }
        #endregion
        //Загрузка данных
        public List<userInfo> LoadUsers()
        {
            List<userInfo> temp = new List<userInfo>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = @"SELECT id, name, pass, name_full, login, otdel, admin, id_rajon, glava, 
       window_name, typ
  FROM sys_scheme.user_db;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    userInfo item = new userInfo();
                    item.admin = sqlCmd.GetBoolean(6);
                    item.id_user = sqlCmd.GetInt32(0);
                    item.loginUser = sqlCmd.GetString(4);
                    item.nameUser = sqlCmd.GetString(3);
                    temp.Add(item);
                }
            }
            return temp;
        }
        public Dictionary<string, string[]> loadParamServ()
        {
            Dictionary<string, string[]> ParamsServ = new Dictionary<string, string[]>();

            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT name, values FROM " + Program.scheme + ".user_params WHERE id_user = " + Program.user_info.id_user + " OR id_user IS NULL";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    ParamsServ.Add(sqlCmd.GetValue<string>("name"), sqlCmd.GetValue<string[]>("values"));
                }
            }
            return ParamsServ;
        }
        //Загрузка данных
        public List<user_right> loadTableRight()
        {
            Program.UserActionRight = ReloadUserActionRight();
            List<user_right> temp = new List<user_right>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT id_table, read_data, write_data FROM " + Program.scheme +
                             ".table_right WHERE id_user = " + Program.id_user.ToString();
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    user_right item = new user_right();
                    item.id_table = sqlCmd.GetInt32(0);
                    item.read = sqlCmd.GetBoolean(1);
                    item.write = sqlCmd.GetBoolean(2);
                    temp.Add(item);
                }
            }
            return temp;
        }
        /// <summary>
        /// Загружаем ограничения на просмотр связанных таблиц (во вкладках окна атрибутов)
        /// </summary>
        public List<ref_table_constr> loadRefTableRight()
        {
            List<ref_table_constr> temp = new List<ref_table_constr>();

            try
            {
                bool exists = false;
                using (SqlWork sqlCmd = new SqlWork(false))
                {
                    sqlCmd.sql = String.Format(@"
SELECT EXISTS(
    SELECT * 
    FROM information_schema.tables 
    WHERE 
      table_schema = '{0}' AND 
      table_name = 'ref_table_restriction'
);", Program.scheme);
                    exists = sqlCmd.ExecuteScalar<bool>();
                }

                if (exists)
                {
                    using (SqlWork sqlCmd = new SqlWork(false))
                    {
                        sqlCmd.sql = "SELECT id_table, id_ref_table FROM " + Program.scheme +
                                     ".ref_table_restriction WHERE id_user = " + Program.id_user.ToString();
                        sqlCmd.ExecuteReader();
                        while (sqlCmd.CanRead())
                        {
                            ref_table_constr item = new ref_table_constr();
                            item.id_table = sqlCmd.GetInt32("id_table");
                            item.id_ref_table = sqlCmd.GetInt32("id_ref_table");
                            temp.Add(item);
                        }
                    }
                }
            }
            catch { /* такой таблицы не существует или на нее нет прав, а значит, запретов на связанные таблицы нет */}
            
            return temp;
        }
        public List<tablesInfo> loadTableInfo()
        {
            List<tablesInfo> temp = new List<tablesInfo>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql =
                    @"SELECT ti.id, ti.scheme_name, ti.name_db, ti.name_map,
ti.lablefiled, ti.type, ti.photo, ti.geom_type, ti.read_only, ti.geom_field, ti.pk_fileld, 
ti.map_style, geometry_columns.srid, ti.source_layer, ti.image_column, ti.angle_column, 
ti.use_bounds, ti.min_scale, ti.max_scale, ti.default_visibl, ti.default_style, ti.style_field, ti.view_name,
ti.sql_view_string, ti.range_colors, ti.range_column, ti.precision_point, ti.type_color, 
ti.min_color, ti.min_val, ti.max_color, ti.max_val, ti.use_min_val, ti.null_color, ti.use_null_color,
ti.use_max_val, ti.hidden, 
label_showframe, label_framecolor, label_parallel, label_overlap, label_usebounds, 
label_minscale, label_maxscale, label_offset, label_graphicunits, label_fontname, label_fontcolor, 
label_fontsize, label_fontstrikeout, label_fontitalic, label_fontunderline, label_fontbold, label_uselabelstyle, label_showlabel,
th.haves, ti.min_object_size, ti.ref_table, ti.graphic_units, ti.display_when_opening,
  ti.fontname,
  ti.fontcolor,
  ti.fontframecolor,
  ti.fontsize,
  ti.symbol,
  ti.pencolor,
  ti.pentype,
  ti.penwidth,
  ti.brushbgcolor,
  ti.brushfgcolor,
  ti.brushstyle,
  ti.brushhatch,
  geometry_columns.type as gc_geomtype                
FROM " +
                    Program.scheme + ".table_info ti " +
                    "left join " + Program.scheme + ".table_right tr on tr.id_table=ti.id " +
                    "left join geometry_columns on f_table_schema = ti.scheme_name AND f_table_name = ti.name_db AND f_geometry_column = ti.geom_field " +
                    "left join (SELECT id_table, true as haves FROM " + Program.scheme +
                    ".table_history_info GROUP BY  id_table) th ON ti.id = th.id_table " +
"WHERE tr.read_data=true AND tr.id_user=" + Program.id_user + @" 
AND ti.id not in (SELECT id_table FROM sys_scheme.table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + ") ORDER BY ti.order_num, ti.name_map;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var item = new tablesInfo();
                    item.idTable = sqlCmd.GetInt32(0);
                    item.nameSheme = sqlCmd.GetString(1);
                    item.nameDB = sqlCmd.GetString(2);
                    item.nameMap = sqlCmd.GetString(3);
                    item.lableFieldName = sqlCmd.GetString(4);
                    item.type = sqlCmd.GetInt32(5);
                    item.photo = sqlCmd.GetBoolean(6);
                    //item.typeGeom = sqlCmd.GetInt32(7);
                    item.read_only = sqlCmd.GetBoolean(8);
                    item.geomFieldName = sqlCmd.GetString(9);
                    item.pkField = sqlCmd.GetString(10);
                    item.map_style = sqlCmd.GetBoolean(11);
                    if (sqlCmd.GetValue("haves") != null)
                        item.isHistory = true;

                    item.srid = sqlCmd.GetValue<int?>(12);

                    //int? srid = sqlCmd.GetValue<int?>(12);
                    //if (srid != null)
                    //    item.srid = srid.Value;
                    //else
                    //    item.srid = 4326;

                    item.GeomType_GC = sqlCmd.GetString("gc_geomtype");
                    item.sourceLayer = sqlCmd.GetBoolean(13);
                    item.imageColumn = sqlCmd.GetString(14);
                    item.angleColumn = sqlCmd.GetString(15);
                    item.useBounds = sqlCmd.GetBoolean(16);
                    item.minScale = sqlCmd.GetInt32(17);
                    item.maxScale = sqlCmd.GetInt32(18);
                    item.MinObjectSize = sqlCmd.GetInt32("min_object_size");
                    item.RefTable = sqlCmd.GetValue<int?>("ref_table");
                    item.style_field = sqlCmd.GetString(21);
                    item.view_name = sqlCmd.GetString(22);
                    item.sql_view_string = sqlCmd.GetString(23);
                    item.precision_point = sqlCmd.GetInt32(26);
                    item.hidden = sqlCmd.GetBoolean(36);
                    item.defaultVisible = sqlCmd.GetBoolean(19);
                    item.DisplayWhenOpening = sqlCmd.GetBoolean("display_when_opening");

                    // showframe = 37; 
                    // framecolor = 38; 
                    // parallel = 39; 
                    // overlap = 40; 
                    // usebounds = 41; 
                    // minscale = 42; 
                    // maxscale = 43; 
                    // offset = 44; 
                    // graphicunits = 45; 
                    // fontname = 46; 
                    // fontcolor = 47; 
                    // fontsize = 48; 
                    // fontstrikeout = 49; 
                    // fontitalic = 50; 
                    // fontunderline = 51; 
                    // fontbold = 52; 
                    // uselabelstyle = 53; 
                    // showlabel = 54; 

                    item.label_showlabel = sqlCmd.GetBoolean(54);
                    item.label_uselabelstyle = sqlCmd.GetBoolean(53);
                    item.label_showframe = sqlCmd.GetBoolean(37);
                    item.label_framecolor = (uint)(sqlCmd.GetInt32(38));
                    item.label_parallel = sqlCmd.GetBoolean(39);
                    item.label_overlap = sqlCmd.GetBoolean(40);
                    item.label_usebounds = sqlCmd.GetBoolean(41);
                    item.label_minscale = (uint)(sqlCmd.GetInt32(42));
                    item.label_maxscale = (uint)(sqlCmd.GetInt32(43));
                    item.label_offset = sqlCmd.GetInt32(44);
                    item.label_graphicunits = sqlCmd.GetBoolean(45);
                    item.label_fontname = sqlCmd.GetString(46);
                    item.label_fontcolor = (uint)(sqlCmd.GetInt32(47));
                    item.label_fontsize = sqlCmd.GetInt32(48);
                    item.label_fontstrikeout = sqlCmd.GetBoolean(49);
                    item.label_fontitalic = sqlCmd.GetBoolean(50);
                    item.label_fontunderline = sqlCmd.GetBoolean(51);
                    item.label_fontbold = sqlCmd.GetBoolean(52);
                    item.graphic_units = sqlCmd.GetBoolean("graphic_units");

                    item.Style = new StylesM(sqlCmd, classesOfMetods.GetIntGeomType(item.GeomType_GC));
                    item.Style.DefaultStyle = sqlCmd.GetBoolean("default_style");
                    item.Style.Range = new objStyleRange(
                        sqlCmd.GetBoolean("range_colors"),
                        sqlCmd.GetString("range_column"),
                        sqlCmd.GetInt32("type_color"),
                        sqlCmd.GetInt32("min_val"),
                        sqlCmd.GetValue<uint>("min_color"),
                        sqlCmd.GetInt32("max_val"),
                        sqlCmd.GetValue<uint>("max_color"),
                        sqlCmd.GetValue<uint>("null_color"),
                        sqlCmd.GetBoolean("use_min_val"),
                        sqlCmd.GetBoolean("use_max_val"),
                        sqlCmd.GetBoolean("use_null_color")
                        );

                    Debug.WriteLine(item.nameSheme + "." + item.nameDB);
                    temp.Add(item);
                }
                sqlCmd.Close();
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql =
@"SELECT ti.id_table, ti.scheme_name, ti.name_db, ti.name_map,
ti.lablefiled, ti.type, ti.photo, ti.geom_type, ti.read_only, ti.geom_field, ti.pk_fileld, 
ti.map_style, geometry_columns.srid, ti.source_layer, ti.image_column, ti.angle_column, 
ti.use_bounds, ti.min_scale, ti.max_scale, ti.default_visibl, ti.default_style, ti.style_field, ti.view_name,
ti.sql_view_string, ti.range_colors, ti.range_column, ti.precision_point, ti.type_color, 
ti.min_color, ti.min_val, ti.max_color, ti.max_val, ti.use_min_val, ti.null_color, ti.use_null_color,
ti.use_max_val, ti.hidden, 
label_showframe, label_framecolor, label_parallel, label_overlap, label_usebounds, 
label_minscale, label_maxscale, label_offset, label_graphicunits, label_fontname, label_fontcolor, 
label_fontsize, label_fontstrikeout, label_fontitalic, label_fontunderline, label_fontbold, label_uselabelstyle, label_showlabel,
th.haves, ti.min_object_size, ti.ref_table, ti.graphic_units, ti.display_when_opening,
  ti.fontname,
  ti.fontcolor,
  ti.fontframecolor,
  ti.fontsize,
  ti.symbol,
  ti.pencolor,
  ti.pentype,
  ti.penwidth,
  ti.brushbgcolor,
  ti.brushfgcolor,
  ti.brushstyle,
  ti.brushhatch,
  geometry_columns.type as gc_geomtype                                       
FROM " +
Program.scheme + ".table_info_sets ti " +
"left join " + Program.scheme + ".table_right tr on tr.id_table=ti.id_table " +
"left join geometry_columns on f_table_schema = ti.scheme_name AND f_table_name = ti.name_db AND f_geometry_column = ti.geom_field " +
"left join (SELECT id_table, true as haves FROM " + Program.scheme +
".table_history_info GROUP BY  id_table) th ON ti.id_table = th.id_table " +
"WHERE tr.read_data=true AND tr.id_user=" + Program.id_user + @" AND ti.id_set = " + Program.WorkSets.CurrentWorkSet.Id + " ORDER BY ti.order_num, ti.name_map;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    tablesInfo item = new tablesInfo();
                    item.idTable = sqlCmd.GetInt32(0);
                    item.nameSheme = sqlCmd.GetString(1);
                    item.nameDB = sqlCmd.GetString(2);
                    item.nameMap = sqlCmd.GetString(3);
                    item.lableFieldName = sqlCmd.GetString(4);
                    item.type = sqlCmd.GetInt32(5);
                    item.photo = sqlCmd.GetBoolean(6);
                    //item.typeGeom = sqlCmd.GetInt32(7);
                    item.read_only = sqlCmd.GetBoolean(8);
                    item.geomFieldName = sqlCmd.GetString(9);
                    item.pkField = sqlCmd.GetString(10);
                    item.map_style = sqlCmd.GetBoolean(11);
                    if (sqlCmd.GetValue("haves") != null)
                        item.isHistory = true;

                    int? srid = sqlCmd.GetValue<int?>(12);

                    if (srid != null)
                        item.srid = srid.Value;
                    else
                        item.srid = 4326;
                    item.GeomType_GC = sqlCmd.GetString("gc_geomtype");
                    item.sourceLayer = sqlCmd.GetBoolean(13);
                    item.imageColumn = sqlCmd.GetString(14);
                    item.angleColumn = sqlCmd.GetString(15);
                    item.useBounds = sqlCmd.GetBoolean(16);
                    item.minScale = sqlCmd.GetInt32(17);
                    item.maxScale = sqlCmd.GetInt32(18);
                    item.MinObjectSize = sqlCmd.GetInt32("min_object_size");
                    item.RefTable = sqlCmd.GetValue<int?>("ref_table");
                    item.style_field = sqlCmd.GetString(21);
                    item.view_name = sqlCmd.GetString(22);
                    item.sql_view_string = sqlCmd.GetString(23);
                    item.precision_point = sqlCmd.GetInt32(26);
                    item.hidden = sqlCmd.GetBoolean(36);
                    item.defaultVisible = sqlCmd.GetBoolean(19);
                    item.DisplayWhenOpening = sqlCmd.GetBoolean("display_when_opening");

                    // showframe = 37; 
                    // framecolor = 38; 
                    // parallel = 39; 
                    // overlap = 40; 
                    // usebounds = 41; 
                    // minscale = 42; 
                    // maxscale = 43; 
                    // offset = 44; 
                    // graphicunits = 45; 
                    // fontname = 46; 
                    // fontcolor = 47; 
                    // fontsize = 48; 
                    // fontstrikeout = 49; 
                    // fontitalic = 50; 
                    // fontunderline = 51; 
                    // fontbold = 52; 
                    // uselabelstyle = 53; 
                    // showlabel = 54; 

                    item.label_showlabel = sqlCmd.GetBoolean(54);
                    item.label_uselabelstyle = sqlCmd.GetBoolean(53);
                    item.label_showframe = sqlCmd.GetBoolean(37);
                    item.label_framecolor = (uint)(sqlCmd.GetInt32(38));
                    item.label_parallel = sqlCmd.GetBoolean(39);
                    item.label_overlap = sqlCmd.GetBoolean(40);
                    item.label_usebounds = sqlCmd.GetBoolean(41);
                    item.label_minscale = (uint)(sqlCmd.GetInt32(42));
                    item.label_maxscale = (uint)(sqlCmd.GetInt32(43));
                    item.label_offset = sqlCmd.GetInt32(44);
                    item.label_graphicunits = sqlCmd.GetBoolean(45);
                    item.label_fontname = sqlCmd.GetString(46);
                    item.label_fontcolor = (uint)(sqlCmd.GetInt32(47));
                    item.label_fontsize = sqlCmd.GetInt32(48);
                    item.label_fontstrikeout = sqlCmd.GetBoolean(49);
                    item.label_fontitalic = sqlCmd.GetBoolean(50);
                    item.label_fontunderline = sqlCmd.GetBoolean(51);
                    item.label_fontbold = sqlCmd.GetBoolean(52);
                    item.graphic_units = sqlCmd.GetBoolean("graphic_units");

                    item.Style = new StylesM(sqlCmd, classesOfMetods.GetIntGeomType(item.GeomType_GC));
                    item.Style.DefaultStyle = sqlCmd.GetBoolean("default_style");
                    item.Style.Range = new objStyleRange(
                        sqlCmd.GetBoolean("range_colors"),
                        sqlCmd.GetString("range_column"),
                        sqlCmd.GetInt32("type_color"),
                        sqlCmd.GetInt32("min_val"),
                        sqlCmd.GetValue<uint>("min_color"),
                        sqlCmd.GetInt32("max_val"),
                        sqlCmd.GetValue<uint>("max_color"),
                        sqlCmd.GetValue<uint>("null_color"),
                        sqlCmd.GetBoolean("use_min_val"),
                        sqlCmd.GetBoolean("use_max_val"),
                        sqlCmd.GetBoolean("use_null_color")
                        );
                    temp.Add(item);
                    Debug.WriteLine(item.nameSheme + "." + item.nameDB);
                }
            }
            return temp;
        }
        public void ReloadStyleTables()
        {
            foreach (var item in Program.tables_info)
            {
                try
                {
                    if (item.type == 1)
                    {
                        item.StyleVM = Program.CachedStyles.GetStyle(item);
                    }
                }
                catch (Exception ex)
                {
                    Classes.workLogFile.writeLogFile(ex, true, true);
                }
            }
        }
        public List<fieldInfo> loadFieldInfo()
        {
            List<fieldInfo> temp = new List<fieldInfo>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT tfi.id, tfi.id_table, tfi.name_db, tfi.name_map, tfi.type_field, tfi.name_lable, " +
                    "tfi.is_reference, tfi.is_interval, tfi.is_style, tfi.ref_table, tfi.ref_field, tfi.ref_field_end, "+
                    "tfi.ref_field_name, tfi.read_only,tfi.visible, tfi.num_order, tfi.is_not_null " +
                    "FROM " + Program.scheme + ".table_field_info tfi, " + Program.scheme + ".table_right tr " +
                    "WHERE tr.id_table=tfi.id_table AND tr.read_data=true AND tr.id_user=" + Program.id_user.ToString() + " ORDER BY num_order;";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                fieldInfo item = new fieldInfo();
                item.idField = sqlCmd.GetInt32(0);
                item.idTable = sqlCmd.GetInt32(1);
                item.nameDB = sqlCmd.GetValue<string>(2);
                item.nameMap = sqlCmd.GetValue<string>(3);
                item.nameLable = sqlCmd.GetValue<string>(5);
                item.type = sqlCmd.GetInt32(4);
                item.is_reference = sqlCmd.GetBoolean(6);
                item.is_interval = sqlCmd.GetBoolean(7);
                item.is_style = sqlCmd.GetBoolean(8);
                item.read_only = sqlCmd.GetBoolean(13);

                item.ref_table = sqlCmd.GetValue<int?>(9);
                item.ref_field = sqlCmd.GetValue<int?>(10);
                item.ref_field_end = sqlCmd.GetValue<int?>(11);
                item.ref_field_name = sqlCmd.GetValue<int?>(12);

                item.visible = sqlCmd.GetBoolean(14);
                item.Order = sqlCmd.GetInt32("num_order");
                item.is_not_null = sqlCmd.GetValue<bool>("is_not_null");
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public List<filtrTableInfo> loadFiltrTableInfo()
        {
            List<filtrTableInfo> temp = new List<filtrTableInfo>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT tffi.id, tffi.id_table, tffi.id_field, tffi.tip_operator " +
                    "FROM " + Program.scheme + ".table_filtr_field_info tffi, " + Program.scheme + ".table_right tr " +
                    "WHERE tr.id_table=tffi.id_table AND tr.read_data=true AND tr.id_user=" + Program.id_user.ToString() + "";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                filtrTableInfo item = new filtrTableInfo();
                item.idFilter = sqlCmd.GetInt32(0);
                item.idTable = sqlCmd.GetInt32(1);
                item.idField = sqlCmd.GetInt32(2);
                item.idOperator = sqlCmd.GetInt32(3);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public List<tipTable> loadTipTable()
        {
            List<tipTable> temp = new List<tipTable>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name, map_layer " +
                    "FROM " + Program.scheme + ".table_type_table ORDER BY id";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                tipTable item = new tipTable();
                item.idTipTable = sqlCmd.GetInt32(0);
                item.nameTip = sqlCmd.GetValue<string>(1);
                item.mapLayer = sqlCmd.GetBoolean(2);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public List<tipGeom> loadTipGeom()
        {
            List<tipGeom> temp = new List<tipGeom>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name, namedb " +
                    "FROM " + Program.scheme + ".table_type_geom ";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                tipGeom item = new tipGeom();
                item.idTipGeom = sqlCmd.GetInt32(0);
                item.nameGeom = sqlCmd.GetValue<string>(1);
                item.nameDb = sqlCmd.GetValue<string>(2);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public List<tipData> loadTipField()
        {
            List<tipData> temp = new List<tipData>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name, name_db " +
                    "FROM " + Program.scheme + ".table_type ";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                tipData item = new tipData();
                item.idTipData = sqlCmd.GetInt32(0);
                item.nameTipData = sqlCmd.GetValue<string>(1);
                item.nameTipDataDB = sqlCmd.GetString(2);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }

        public List<groupInfo> loadGroupInfo()
        {
            var temp = new List<groupInfo>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT id, name_group, descript from " + Program.scheme +
                                     ".get_group_list_without_null() ORDER BY order_num";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var item = new groupInfo();
                    item.id = sqlCmd.GetInt32(0);
                    item.name = sqlCmd.GetString(1);
                    item.descript = sqlCmd.GetString(2);
                    temp.Add(item);
                }
            }
            return temp;
        }
        public List<groupInfo> loadGroupFullInfo()
        {
            var temp = new List<groupInfo>();
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT id, name_group, descript from " + Program.scheme +
                                     ".get_group_list() ORDER BY order_num";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var item = new groupInfo();
                    item.id = sqlCmd.GetInt32(0);
                    item.name = sqlCmd.GetString(1);
                    item.descript = sqlCmd.GetString(2);
                    temp.Add(item);
                }
            }
            return temp;
        }

        public List<photoInfo> loadPhotoInfo()
        {
            List<photoInfo> temp = new List<photoInfo>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT tpi.id_table, tpi.id_field_tble, tpi.photo_table, tpi.photo_field, tpi.photo_file " +
                    "FROM " + Program.scheme + ".table_photo_info tpi, " + Program.scheme + ".table_right tr, " + Program.scheme + ".table_info ti " +
                    "WHERE tr.id_table=tpi.id_table AND tr.read_data=true AND tr.id_user=" + Program.id_user.ToString() + " AND " +
                    " ti.id = tpi.id_table;";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                photoInfo item = new photoInfo();
                item.idTable = sqlCmd.GetInt32(0);
                item.nameFieldID = sqlCmd.GetValue<string>(1);
                item.namePhotoTable = sqlCmd.GetValue<string>(2);
                item.namePhotoField = sqlCmd.GetValue<string>(3);
                item.namePhotoFile = sqlCmd.GetValue<string>(4);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public List<string> loadSchems()
        {
            List<string> temp = new List<string>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name " +
                    "FROM " + Program.scheme + ".table_schems ";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                string item = "";
                item = sqlCmd.GetValue<string>(1);
                temp.Add(item);
            }
            sqlCmd.Close();
            return temp;
        }
        public userInfo loadUserInfo()
        {
            userInfo temp = new userInfo();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_full, admin, window_name, login, typ " +
                      "FROM " + Program.scheme + ".user_db " +
                      "WHERE id = " + Program.id_user;
            sqlCmd.ExecuteReader();
            if (sqlCmd.CanRead())
            {
                temp.id_user = Program.id_user;
                temp.nameUser = sqlCmd.GetValue<string>(1);
                temp.admin = sqlCmd.GetBoolean(2);
                temp.windowText = sqlCmd.GetValue<string>(3);
                temp.loginUser = sqlCmd.GetValue<string>(4);
                temp.type_user = sqlCmd.GetInt32(5);
            }
            sqlCmd.Close();
            return temp;
        }
        public FiltersViewModel LoadFilters()
        {
            FiltersViewModel temp = new FiltersViewModel();
            temp.Load(Login.ConcatenateServer(Program.connString));
            Program.Filters = temp;
            return temp;
        }
        public List<string> loadReservedWords()
        {
            List<string> reservedWords = new List<string>();
            reservedWords.AddRange(new[] { "A", "ABORT", "ABS", "ABSOLUTE", "ACCESS", "ACTION", "ADA", 
                "ADD", "ADMIN", "AFTER", "AGGREGATE", "ALIAS", "ALL", "ALLOCATE", "ALSO", "ALTER", 
                "ALWAYS", "ANALYSE", "ANALYZE", "AND", "ANY", "ARE", "ARRAY", "AS", "ASC", "ASENSITIVE", 
                "ASSERTION", "ASSIGNMENT", "ASYMMETRIC", "AT", "ATOMIC", "ATTRIBUTE", "ATTRIBUTES", "AUDIT", 
                "AUTHORIZATION", "AUTO_INCREMENT", "AVG", "AVG_ROW_LENGTH", "BACKUP", "BACKWARD", "BEFORE", 
                "BEGIN", "BERNOULLI", "BETWEEN", "BIGINT", "BINARY", "BIT", "BIT_LENGTH", "BITVAR", "BLOB", 
                "BOOL", "BOOLEAN", "BOTH", "BREADTH", "BREAK", "BROWSE", "BULK", "BY", "C", "CACHE", "CALL", 
                "CALLED", "CARDINALITY", "CASCADE", "CASCADED", "CASE", "CAST", "CATALOG", "CATALOG_NAME", 
                "CEIL", "CEILING", "CHAIN", "CHANGE", "CHAR", "CHAR_LENGTH", "CHARACTER", "CHARACTER_LENGTH", 
                "CHARACTER_SET_CATALOG", "CHARACTER_SET_NAME", "CHARACTER_SET_SCHEMA", "CHARACTERISTICS", 
                "CHARACTERS", "CHECK", "CHECKED", "CHECKPOINT", "CHECKSUM", "CLASS", "CLASS_ORIGIN", "CLOB", 
                "CLOSE", "CLUSTER", "CLUSTERED", "COALESCE", "COBOL", "COLLATE", "COLLATION", "COLLATION_CATALOG", 
                "COLLATION_NAME", "COLLATION_SCHEMA", "COLLECT", "COLUMN", "COLUMN_NAME", "COLUMNS", "COMMAND_FUNCTION", 
                "COMMAND_FUNCTION_CODE", "COMMENT", "COMMIT", "COMMITTED", "COMPLETION", "COMPRESS", "COMPUTE", 
                "CONDITION", "CONDITION_NUMBER", "CONNECT", "CONNECTION", "CONNECTION_NAME", "CONSTRAINT", 
                "CONSTRAINT_CATALOG", "CONSTRAINT_NAME", "CONSTRAINT_SCHEMA", "CONSTRAINTS", "CONSTRUCTOR", 
                "CONTAINS", "CONTAINSTABLE", "CONTINUE", "CONVERSION", "CONVERT", "COPY", "CORR", "CORRESPONDING", 
                "COUNT", "COVAR_POP", "COVAR_SAMP", "CREATE", "CREATEDB", "CREATEROLE", "CREATEUSER", "CROSS", 
                "CSV", "CUBE", "CUME_DIST", "CURRENT", "CURRENT_DATE", "CURRENT_DEFAULT_TRANSFORM_GROUP", 
                "CURRENT_PATH", "CURRENT_ROLE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_TRANSFORM_GROUP_FOR_TYPE", 
                "CURRENT_USER", "CURSOR", "CURSOR_NAME", "CYCLE", "DATA", "DATABASE", "DATABASES", "DATE", "DATETIME", 
                "DATETIME_INTERVAL_CODE", "DATETIME_INTERVAL_PRECISION", "DAY", "DAY_HOUR", "DAY_MICROSECOND", 
                "DAY_MINUTE", "DAY_SECOND", "DAYOFMONTH", "DAYOFWEEK", "DAYOFYEAR", "DBCC", "DEALLOCATE", "DEC", 
                "DECIMAL", "DECLARE", "DEFAULT", "DEFAULTS", "DEFERRABLE", "DEFERRED", "DEFINED", "DEFINER", 
                "DEGREE", "DELAY_KEY_WRITE", "DELAYED", "DELETE", "DELIMITER", "DELIMITERS", "DENSE_RANK", "DENY", 
                "DEPTH", "DEREF", "DERIVED", "DESC", "DESCRIBE", "DESCRIPTOR", "DESTROY", "DESTRUCTOR", "DETERMINISTIC", 
                "DIAGNOSTICS", "DICTIONARY", "DISABLE", "DISCONNECT", "DISK", "DISPATCH", "DISTINCT", "DISTINCTROW", 
                "DISTRIBUTED", "DIV", "DO", "DOMAIN", "DOUBLE", "DROP", "DUAL", "DUMMY", "DUMP", "DYNAMIC", 
                "DYNAMIC_FUNCTION", "DYNAMIC_FUNCTION_CODE", "EACH", "ELEMENT", "ELSE", "ELSEIF", "ENABLE", 
                "ENCLOSED", "ENCODING", "ENCRYPTED", "END", "END-EXEC", "ENUM", "EQUALS", "ERRLVL", "ESCAPE", 
                "ESCAPED", "EVERY", "EXCEPT", "EXCEPTION", "EXCLUDE", "EXCLUDING", "EXCLUSIVE", "EXEC", "EXECUTE", 
                "EXISTING", "EXISTS", "EXIT", "EXP", "EXPLAIN", "EXTERNAL", "EXTRACT", "FALSE", "FETCH", "FIELDS", 
                "FILE", "FILLFACTOR", "FILTER", "FINAL", "FIRST", "FLOAT", "FLOAT4", "FLOAT8", "FLOOR", "FLUSH", 
                "FOLLOWING", "FOR", "FORCE", "FOREIGN", "FORTRAN", "FORWARD", "FOUND", "FREE", "FREETEXT", 
                "FREETEXTTABLE", "FREEZE", "FROM", "FULL", "FULLTEXT", "FUNCTION", "FUSION", "G", "GENERAL", 
                "GENERATED", "GET", "GLOBAL", "GO", "GOTO", "GRANT", "GRANTED", "GRANTS", "GREATEST", "GROUP", 
                "GROUPING", "HANDLER", "HAVING", "HEADER", "HEAP", "HIERARCHY", "HIGH_PRIORITY", "HOLD", "HOLDLOCK", 
                "HOST", "HOSTS", "HOUR", "HOUR_MICROSECOND", "HOUR_MINUTE", "HOUR_SECOND", "IDENTIFIED", "IDENTITY", 
                "IDENTITY_INSERT", "IDENTITYCOL", "IF", "IGNORE", "ILIKE", "IMMEDIATE", "IMMUTABLE", "IMPLEMENTATION", 
                "IMPLICIT", "IN", "INCLUDE", "INCLUDING", "INCREMENT", "INDEX", "INDICATOR", "INFILE", "INFIX", "INHERIT", 
                "INHERITS", "INITIAL", "INITIALIZE", "INITIALLY", "INNER", "INOUT", "INPUT", "INSENSITIVE", "INSERT", 
                "INSERT_ID", "INSTANCE", "INSTANTIABLE", "INSTEAD", "INT", "INT1", "INT2", "INT3", "INT4", "INT8", 
                "INTEGER", "INTERSECT", "INTERSECTION", "INTERVAL", "INTO", "INVOKER", "IS", "ISAM", "ISNULL", 
                "ISOLATION", "ITERATE", "JOIN", "K", "KEY", "KEY_MEMBER", "KEY_TYPE", "KEYS", "KILL", "LANCOMPILER", 
                "LANGUAGE", "LARGE", "LAST", "LAST_INSERT_ID", "LATERAL", "LEADING", "LEAST", "LEAVE", "LEFT", "LENGTH", 
                "LESS", "LEVEL", "LIKE", "LIMIT", "LINENO", "LINES", "LISTEN", "LN", "LOAD", "LOCAL", "LOCALTIME", 
                "LOCALTIMESTAMP", "LOCATION", "LOCATOR", "LOCK", "LOGIN", "LOGS", "LONG", "LONGBLOB", "LONGTEXT", "LOOP", 
                "LOW_PRIORITY", "LOWER", "M", "MAP", "MATCH", "MATCHED", "MAX", "MAX_ROWS", "MAXEXTENTS", "MAXVALUE", 
                "MEDIUMBLOB", "MEDIUMINT", "MEDIUMTEXT", "MEMBER", "MERGE", "MESSAGE_LENGTH", "MESSAGE_OCTET_LENGTH", 
                "MESSAGE_TEXT", "METHOD", "MIDDLEINT", "MIN", "MIN_ROWS", "MINUS", "MINUTE", "MINUTE_MICROSECOND", 
                "MINUTE_SECOND", "MINVALUE", "MLSLABEL", "MOD", "MODE", "MODIFIES", "MODIFY", "MODULE", "MONTH", "MONTHNAME", 
                "MORE", "MOVE", "MULTISET", "MUMPS", "MYISAM", "NAME", "NAMES", "NATIONAL", "NATURAL", "NCHAR", "NCLOB", 
                "NESTING", "NEW", "NEXT", "NO", "NO_WRITE_TO_BINLOG", "NOAUDIT", "NOCHECK", "NOCOMPRESS", "NOCREATEDB", 
                "NOCREATEROLE", "NOCREATEUSER", "NOINHERIT", "NOLOGIN", "NONCLUSTERED", "NONE", "NORMALIZE", "NORMALIZED", 
                "NOSUPERUSER", "NOT", "NOTHING", "NOTIFY", "NOTNULL", "NOWAIT", "NULL", "NULLABLE", "NULLIF", "NULLS", 
                "NUMBER", "NUMERIC", "OBJECT", "OCTET_LENGTH", "OCTETS", "OF", "OFF", "OFFLINE", "OFFSET", "OFFSETS", 
                "OIDS", "OLD", "ON", "ONLINE", "ONLY", "OPEN", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML", 
                "OPERATION", "OPERATOR", "OPTIMIZE", "OPTION", "OPTIONALLY", "OPTIONS", "OR", "ORDER", "ORDERING", "ORDINALITY", 
                "OTHERS", "OUT", "OUTER", "OUTFILE", "OUTPUT", "OVER", "OVERLAPS", "OVERLAY", "OVERRIDING", "OWNER", "PACK_KEYS", 
                "PAD", "PARAMETER", "PARAMETER_MODE", "PARAMETER_NAME", "PARAMETER_ORDINAL_POSITION", "PARAMETER_SPECIFIC_CATALOG", 
                "PARAMETER_SPECIFIC_NAME", "PARAMETER_SPECIFIC_SCHEMA", "PARAMETERS", "PARTIAL", "PARTITION", "PASCAL", "PASSWORD", 
                "PATH", "PCTFREE", "PERCENT", "PERCENT_RANK", "PERCENTILE_CONT", "PERCENTILE_DISC", "PLACING", "PLAN", "PLI", 
                "POSITION", "POSTFIX", "POWER", "PRECEDING", "PRECISION", "PREFIX", "PREORDER", "PREPARE", "PREPARED", "PRESERVE", 
                "PRIMARY", "PRINT", "PRIOR", "PRIVILEGES", "PROC", "PROCEDURAL", "PROCEDURE", "PROCESS", "PROCESSLIST", "PUBLIC", 
                "PURGE", "QUOTE", "RAID0", "RAISERROR", "RANGE", "RANK", "RAW", "READ", "READS", "READTEXT", "REAL", "RECHECK", 
                "RECONFIGURE", "RECURSIVE", "REF", "REFERENCES", "REFERENCING", "REGEXP", "REGR_AVGX", "REGR_AVGY", "REGR_COUNT", 
                "REGR_INTERCEPT", "REGR_R2", "REGR_SLOPE", "REGR_SXX", "REGR_SXY", "REGR_SYY", "REINDEX", "RELATIVE", "RELEASE", 
                "RELOAD", "RENAME", "REPEAT", "REPEATABLE", "REPLACE", "REPLICATION", "REQUIRE", "RESET", "RESIGNAL", "RESOURCE", 
                "RESTART", "RESTORE", "RESTRICT", "RESULT", "RETURN", "RETURNED_CARDINALITY", "RETURNED_LENGTH", "RETURNED_OCTET_LENGTH", 
                "RETURNED_SQLSTATE", "RETURNS", "REVOKE", "RIGHT", "RLIKE", "ROLE", "ROLLBACK", "ROLLUP", "ROUTINE", "ROUTINE_CATALOG", 
                "ROUTINE_NAME", "ROUTINE_SCHEMA", "ROW", "ROW_COUNT", "ROW_NUMBER", "ROWCOUNT", "ROWGUIDCOL", "ROWID", "ROWNUM", "ROWS", 
                "RULE", "SAVE", "SAVEPOINT", "SCALE", "SCHEMA", "SCHEMA_NAME", "SCHEMAS", "SCOPE", "SCOPE_CATALOG", "SCOPE_NAME", 
                "SCOPE_SCHEMA", "SCROLL", "SEARCH", "SECOND", "SECOND_MICROSECOND", "SECTION", "SECURITY", "SELECT", "SELF", "SENSITIVE", 
                "SEPARATOR", "SEQUENCE", "SERIALIZABLE", "SERVER_NAME", "SESSION", "SESSION_USER", "SET", "SETOF", "SETS", "SETUSER", 
                "SHARE", "SHOW", "SHUTDOWN", "SIGNAL", "SIMILAR", "SIMPLE", "SIZE", "SMALLINT", "SOME", "SONAME", "SOURCE", "SPACE", 
                "SPATIAL", "SPECIFIC", "SPECIFIC_NAME", "SPECIFICTYPE", "SQL", "SQL_BIG_RESULT", "SQL_BIG_SELECTS", "SQL_BIG_TABLES", 
                "SQL_CALC_FOUND_ROWS", "SQL_LOG_OFF", "SQL_LOG_UPDATE", "SQL_LOW_PRIORITY_UPDATES", "SQL_SELECT_LIMIT", "SQL_SMALL_RESULT", 
                "SQL_WARNINGS", "SQLCA", "SQLCODE", "SQLERROR", "SQLEXCEPTION", "SQLSTATE", "SQLWARNING", "SQRT", "SSL", "STABLE", 
                "START", "STARTING", "STATE", "STATEMENT", "STATIC", "STATISTICS", "STATUS", "STDDEV_POP", "STDDEV_SAMP", "STDIN", "STDOUT", 
                "STORAGE", "STRAIGHT_JOIN", "STRICT", "STRING", "STRUCTURE", "STYLE", "SUBCLASS_ORIGIN", "SUBLIST", "SUBMULTISET", "SUBSTRING", 
                "SUCCESSFUL", "SUM", "SUPERUSER", "SYMMETRIC", "SYNONYM", "SYSDATE", "SYSID", "SYSTEM", "SYSTEM_USER", "TABLE", "TABLE_NAME", 
                "TABLES", "TABLESAMPLE", "TABLESPACE", "TEMP", "TEMPLATE", "TEMPORARY", "TERMINATE", "TERMINATED", "TEXT", "TEXTSIZE", "THAN", 
                "THEN", "TIES", "TIME", "TIMESTAMP", "TIMEZONE_HOUR", "TIMEZONE_MINUTE", "TINYBLOB", "TINYINT", "TINYTEXT", "TO", "TOAST", 
                "TOP", "TOP_LEVEL_COUNT", "TRAILING", "TRAN", "TRANSACTION", "TRANSACTION_ACTIVE", "TRANSACTIONS_COMMITTED", 
                "TRANSACTIONS_ROLLED_BACK", "TRANSFORM", "TRANSFORMS", "TRANSLATE", "TRANSLATION", "TREAT", "TRIGGER", "TRIGGER_CATALOG", 
                "TRIGGER_NAME", "TRIGGER_SCHEMA", "TRIM", "TRUE", "TRUNCATE", "TRUSTED", "TSEQUAL", "TYPE", "UESCAPE", "UID", "UNBOUNDED", 
                "UNCOMMITTED", "UNDER", "UNDO", "UNENCRYPTED", "UNION", "UNIQUE", "UNKNOWN", "UNLISTEN", "UNLOCK", "UNNAMED", "UNNEST", 
                "UNSIGNED", "UNTIL", "UPDATE", "UPDATETEXT", "UPPER", "USAGE", "USE", "USER", "USER_DEFINED_TYPE_CATALOG", "USER_DEFINED_TYPE_CODE", 
                "USER_DEFINED_TYPE_NAME", "USER_DEFINED_TYPE_SCHEMA", "USING", "UTC_DATE", "UTC_TIME", "UTC_TIMESTAMP", "VACUUM", "VALID", 
                "VALIDATE", "VALIDATOR", "VALUE", "VALUES", "VAR_POP", "VAR_SAMP", "VARBINARY", "VARCHAR", "VARCHAR2", "VARCHARACTER", "VARIABLE", 
                "VARIABLES", "VARYING", "VERBOSE", "VIEW", "VOLATILE", "WAITFOR", "WHEN", "WHENEVER", "WHERE", "WHILE", "WIDTH_BUCKET", "WINDOW", 
                "WITH", "WITHIN", "WITHOUT", "WORK", "WRITE", "WRITETEXT", "X509", "XOR", "YEAR", "YEAR_MONTH", "ZEROFILL", "ZONE" });
            return reservedWords;
        }

        //Получение инфы
        static public user_right getTableRight(int id_table)
        {
            return Program.tables_right.Find(w => w.id_table == id_table);
        }
        static public bool getRefTableRight(int id_table, int id_ref_table)
        {
            return !Program.ref_tables_right.Any(w => w.id_table == id_table && w.id_ref_table == id_ref_table);
        }
        static public tablesInfo getTableInfo(int id_table)
        {
            return Program.tables_info.Find(w => w.idTable == id_table);
        }
        static public tablesInfo getTableInfoOfNameMap(string nameMap)
        {
            return Program.tables_info.Find(w => w.nameMap == nameMap);
        }
        static public tablesInfo getTableInfoOfNameDB(string nameDB)
        {
            return Program.tables_info.Find(w => w.nameDB == nameDB);
        }
        static public tablesInfo getTableInfoOfNameDB(string nameDB, string schema)
        {
            return Program.tables_info.Find(w => w.nameDB == nameDB && w.nameSheme == schema);
        }
        static public List<tablesInfo> getTableOfType(int id_type)
        {
            return Program.tables_info.FindAll(w => w.type == id_type);
        }
        static public fieldInfo getFieldInfo(int id_field)
        {
            return Program.field_info.Find(w => w.idField == id_field);
        }
        static public List<fieldInfo> getFieldInfoTable(int id_table)
        {
            return Program.field_info.FindAll(w => w.idTable == id_table);
        }
        static public filtrTableInfo getFiltrInfo(int id_filter)
        {
            return Program.filtr_table_info.Find(w => w.idFilter == id_filter);
        }
        static public List<filtrTableInfo> getFiltrTableInfo(int id_table)
        {
            return Program.filtr_table_info.FindAll(w => w.idTable == id_table);
        }
        static public tipTable getTipTable(int idTip)
        {
            return Program.tip_table.Find(w => w.idTipTable == idTip);
        }
        static public tipGeom getTipGeom(int idTip)
        {
            return Program.tip_geom.Find(w => w.idTipGeom == idTip);
        }
        static public tipData getTipField(int idTip)
        {
            return Program.tip_data.Find(w => w.idTipData == idTip);
        }
        static public tipOperator getTipOperator(int idTip)
        {
            return Program.tip_operator.Find(w => w.idTipOperator == idTip);
        }
        static public photoInfo getPhotoInfo(int id_table)
        {
            return Program.photo_info.Find(w => w.idTable == id_table);
        }
        static public bool getOpenTable(int id_table)
        {
            return Program.tables_right.Find(w => w.id_table == id_table).read;
        }
        static public bool getWriteTable(int id_table)
        {
            var right = Program.tables_right.FirstOrDefault(w => w.id_table == id_table);
            if (right == null)
                return false;
            return right.write;
        }
        static public bool isReservedName(string name)
        {
            return !String.IsNullOrEmpty(name) && Program.reserved_words.Contains(name.ToUpper());
        }
        static public Version GetPostgisVersion()
        {
            string version = "";
            Version temp = null;
            using(SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT postgis_version();";
                version = sqlCmd.ExecuteScalar<String>();
            }
            if(!String.IsNullOrEmpty(version))
            {
                version = version.Substring(0, version.IndexOf(" "));
                temp = new Version(version);
            }
            return temp;
        }
        static public String GetValidFunction(String function21, String function15)
        {
            if(Program.postgisVersion<(new Version("2")))
            {
                return function15;
            }
            else
            {
                return function21;
            }
        }
        public void reloadInfo()
        {
            Program.postgisVersion = GetPostgisVersion();
            Program.tables_right = loadTableRight();
            Program.ref_tables_right = loadRefTableRight();

            Program.tables_info = loadTableInfo();
            Program.field_info = loadFieldInfo();
            Program.filtr_table_info = loadFiltrTableInfo();
            Program.tip_table = loadTipTable();
            Program.tip_geom = loadTipGeom();
            Program.tip_data = loadTipField();
            Program.photo_info = loadPhotoInfo();
            Program.schems = loadSchems();
            Program.user_info = loadUserInfo();
            Program.UserParams = loadParamServ();
            Program.tablegroups_info = loadTablesGroupsInfo();
            Program.group_info = loadGroupInfo();
            Program.group_info_full = loadGroupFullInfo();
            Program.users_info = LoadUsers();
            Program.Filters = LoadFilters();
            ReloadStyleTables();

            Program.reserved_words = loadReservedWords();
            Program.WorkSets.Reload();
            Program.ReportModel.Reload();
        }

        private List<PgActionRightM> ReloadUserActionRight()
        {
            if (Program.repository == null)
                return null;
            var userRights = new PgListUserRightsVM(Program.repository);
            userRights.User = Program.repository.CurrentUser;
            return userRights.ActionRights.ToList();
        }
        /// <summary>Добавить параметр
        /// </summary>
        /// <typeparam name="T">Тип параметра</typeparam>
        /// <param name="data"> XML файл параметров</param>
        /// <param name="paramName">Параметр</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        public static object AddParam<T>(System.Xml.Linq.XElement data, string paramName, T defaultValue)
        {
            var xElement = data.Element(paramName);
            return xElement != null
                ? Convert.ChangeType(xElement.Value, typeof(T))
                : defaultValue;
        }

        public List<tableAndGroupInfo> loadTablesGroupsInfo()
        {
            List<tableAndGroupInfo> TableGroupList = new List<tableAndGroupInfo>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "Select tgi.id_table, tgi.id_group, tgi.order_num from " + Program.scheme + ".table_groups_table tgi, sys_scheme.table_info ti " +
                "WHERE tgi.id_table = ti.id AND ti.hidden = false AND ti.type=1 ORDER BY tgi.id_group, tgi.order_num;";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                TableGroupList.Add(new tableAndGroupInfo(sqlCmd.GetInt32(0), sqlCmd.GetInt32(1), sqlCmd.GetInt32(2)));
            }
            return TableGroupList;
        }
        static public string[] GetUserParams(string param)
        {
            if (!Program.UserParams.ContainsKey(param))
                return new string[0];
            return Program.UserParams[param];
        }

        static public bool GetUserParamsContains(string param)
        {
            return Program.UserParams.ContainsKey(param);
        }
        static public userInfo getUserByLogin(string login)
        {
            return Program.users_info.Find(x => x.loginUser == login);
        }
        static public void setComboBox(System.Windows.Forms.ComboBox cmb, int id)
        {
            for (int i = 0; cmb.Items.Count > i; i++)
            {
                if (((itemObj)cmb.Items[i]).Id_o == id)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }
        static public void setComboBox(System.Windows.Controls.ComboBox cmb, int id)
        {
            for (int i = 0; cmb.Items.Count > i; i++)
            {
                if (((itemObj)cmb.Items[i]).Id_o == id)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }
        #region Перезагрузка слоев
        static public void reloadStyleForDefault(int idT)
        {
            List<itemObj> tables = new List<itemObj>();
            string Namelayer = Program.RelationVisbleBdUser.GetNameInBd(idT);
            if (Program.mainFrm1.axMapLIb1.getLayer(Namelayer) != null)
            {
                tables.Add(new itemObj(idT, classesOfMetods.getTableInfo(idT).nameDB, Namelayer));
            }
            for (int i = 0; tables.Count > i; i++)
            {

                mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(tables[i].Layer);
                tempLayer.ClearExtStyles();
                Program.mainFrm1.layersManager1.loadStyle(tempLayer, tables[i].Id_o);
                tempLayer.ExternalFullReloadVisible();
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
        }
        static public void reloadUseBounds(int idT)
        {
            tablesInfo ti = classesOfMetods.getTableInfo(idT);
            if (ti == null)
            {
                return;
            }
            if (ti.type == 1)
            {
                if (ti.hidden == false)
                {
                    string NamelayerInBD = Program.RelationVisbleBdUser.GetNameInBd(idT);
                    string NamelayerForUser = Program.RelationVisbleBdUser.GetNameForUser(idT);
                    if (Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD) != null)
                    {
                        mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD);
                        bool isVisible = tempLayer.Visible;
                        bool isSelectable = tempLayer.selectable;
                        bool isEditable = tempLayer.editable;

                        //mvMapLib.mvBbox bbox = tempLayer.getBbox();
                        Program.mainFrm1.axMapLIb1.deleteLayer(ref tempLayer);
                        Program.RelationVisbleBdUser.deleteRelation(NamelayerForUser, NamelayerInBD);

                        Program.mainFrm1.layersManager1.loadLayerFromSource(idT);
                        tempLayer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(idT));
                        tempLayer.Visible = isVisible;
                        tempLayer.selectable = isSelectable;
                        tempLayer.editable = isEditable;
                        Program.mainFrm1.axMapLIb1.mapRepaint();
                    }
                }
                else
                {
                    string NamelayerInBD = Program.RelationVisbleBdUser.GetNameInBd(idT);
                    string NamelayerForUser = Program.RelationVisbleBdUser.GetNameForUser(idT);
                    if (Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD) != null)
                    {
                        mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD);
                        Program.mainFrm1.axMapLIb1.deleteLayer(ref tempLayer);
                        Program.RelationVisbleBdUser.deleteRelation(NamelayerForUser, NamelayerInBD);
                        Program.mainFrm1.axMapLIb1.mapRepaint();
                    }
                }
            }
        }
        static public void reloadLayerData(mvMapLib.mvLayer mvL)
        {
            if (mvL.External)
                mvL.ExternalFullReloadVisible();
            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        static public void reloadLayerData(tablesInfo ti)
        {
            string NamelayerInBD = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
            mvMapLib.mvLayer tempLayer= Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD);
            if(tempLayer!=null)
            {
                reloadLayerData(tempLayer);
            }
        }
        static public void reloadAllLayerData()
        {
            cti.ThreadProgress.ShowWait();
            for (int i = 0; Program.mainFrm1.axMapLIb1.LayersCount > i; i++)
            {
                mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);

                tempLayer = Program.mainFrm1.axMapLIb1.getLayer(tempLayer.NAME);
                if (tempLayer != null)
                {
                    if (tempLayer.External)
                    {
                        tempLayer.ExternalFullReloadVisible();
                    }
                }
            }
            Program.mainFrm1.axMapLIb1.mapRepaint();
            cti.ThreadProgress.Close();
        }
        static public void reloadAllLayerData(int idTable)
        {
            cti.ThreadProgress.ShowWait();
            string NamelayerInBD = Program.RelationVisbleBdUser.GetNameInBd(idTable);
            mvMapLib.mvLayer tempLayer;
            if ((tempLayer = Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD)) != null)
            {
                if (tempLayer != null)
                    if (tempLayer.External)
                        tempLayer.ExternalFullReloadVisible();
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
            cti.ThreadProgress.Close();
        }
        static public void reloadLayer(int id_table)
        {
            var cls = new classesOfMetods();
            cls.reloadInfo();
            string nameLayer = Program.RelationVisbleBdUser.GetNameInBd(id_table);
            if (Program.mainFrm1.axMapLIb1.getLayer(nameLayer) != null)
            {
                mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(nameLayer);
                bool visbl = tempLayer.Visible; 
                bool editable = tempLayer.editable;
                int layerOrderNum = -1;
                for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                {
                    var ll = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                    if (ll.NAME == tempLayer.NAME)
                    {
                        layerOrderNum = i;
                        break;
                    }
                }

                Program.mainFrm1.axMapLIb1.deleteLayer(ref tempLayer);

                Program.mainFrm1.layersManager1.loadLayerFromSource(id_table);
                tempLayer = Program.mainFrm1.axMapLIb1.getLayer(nameLayer);
                if (tempLayer != null)
                {
                    for (int i = 0; i < layerOrderNum; i++)
                        tempLayer.MoveDown();
                    tempLayer.Visible = visbl;
                    tempLayer.editable = editable;
                }
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
        }
        static public void ReloadRelatedTables(int idT)
        {
            // получить список связанных таблиц
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = @"SELECT ti.id FROM sys_scheme.table_info ti, sys_scheme.table_field_info tfi
  WHERE ti.id=tfi.id_table AND ti.type = 1 AND tfi.is_style = true AND tfi.ref_table = " + idT.ToString() + ";";
            sqlCmd.ExecuteReader();
            while (sqlCmd.CanRead())
            {
                reloadStyleForDefault(sqlCmd.GetInt32(0));
            }
            sqlCmd.Close();
        }
        #endregion
        static public void DeleteLayerInMap(int idT)
        {
            string NamelayerInBD = Program.RelationVisbleBdUser.GetNameInBd(idT);
            if (Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD) != null)
            {
                mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(NamelayerInBD);
                Program.mainFrm1.axMapLIb1.deleteLayer(ref tempLayer);
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
        }
        static public int? GetPKValue(tablesInfo ti, fieldInfo fi, object val)
        {
            int? pkVal = null;
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format(@"SELECT {2} FROM {0}.{1} tables WHERE tables.{3} = {4}", ti.nameSheme, ti.nameDB,
                                                        ti.pkField, fi.nameDB, val);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    pkVal = sqlCmd.GetValue<int?>(0);
                }
            }
            return pkVal;
        }

        static public bool ExistsOpeningObject(int idTable, int idObj)
        {
            // Нужно что бы проверить не удален ли объект с момента последней загрузки.
            bool result = false;
            using (SqlWork sqlCmd = new SqlWork())
            {
                tablesInfo ti = classesOfMetods.getTableInfo(idTable);
                sqlCmd.sql = String.Format("SELECT \"{1}\" FROM \"{3}\".\"{0}\" WHERE \"{0}\".\"{1}\"={2}", ti.nameDB, ti.pkField, idObj, ti.nameSheme);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                sqlCmd.Close();
            }
            return result;
        }

        static public List<string> GetTableNotRight(int idTable)
        {
            List<string> list = new List<string>();

            var fields = classesOfMetods.getFieldInfoTable(idTable);
            foreach (var item in fields)
            {
                if (item.ref_table != null)
                {
                    user_right ur = classesOfMetods.getTableRight(item.ref_table.Value);
                    if (ur == null)
                    {
                        string table_name = GetTableNameFromDb(item.ref_table.Value);
                        if (table_name != "")
                        {
                            list.Add(String.Format(Rekod.Properties.Resources.COM_NoRightToTable, table_name));
                        }
                        else
                        {
                            list.Add(String.Format(Rekod.Properties.Resources.COM_ConnAnNoExistsTable, item.nameMap));
                        }
                    }
                    else if (!ur.read)
                    {
                        string table_name = GetTableNameFromDb(item.ref_table.Value);
                        list.Add(String.Format(Rekod.Properties.Resources.COM_NoRightToTable, table_name));
                    }
                }
            }
            return list;
        }
        static public string GetTableNameFromDb(int idTable)
        {
            string table_name = "";
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT name_map FROM {0}.table_info WHERE id={1}",
                    Program.scheme, idTable);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    table_name = sqlCmd.GetString(0);
                }
                sqlCmd.Close();
            }
            return table_name;
        }

        static public bool getUserByLoginInDB(string login)
        {
            var listParams = new List<Interfaces.IParams>
                                 {
                                     new Params()
                                         {
                                             _paramName = "user_login",
                                             typeData = NpgsqlDbType.Text,
                                             value = login.ToLower()
                                         }
                                 };
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT * FROM pg_roles WHERE rolname = :user_login";
                sqlCmd.ExecuteReader(listParams);
                if (sqlCmd.CanRead())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>Для определения типа геометрии из строки
        /// </summary>
        /// <param name="wkt"></param>
        /// <returns></returns>
        public static int GetIntGeomType(string wkt)
        {
            if (String.IsNullOrEmpty(wkt))
                return 0;
            if (wkt.StartsWith("POLYGON") || wkt.StartsWith("MULTIPOLYGON"))
                return 3;
            else if (wkt.StartsWith("LINESTRING") || wkt.StartsWith("MULTILINESTRING"))
                return 2;
            else if (wkt.StartsWith("POINT") || wkt.StartsWith("MULTIPOINT"))
                return 1;
            else
                return 0;
        }
        static public string GetVersionString
        {
            get
            {
                Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string res = ver.Major.ToString() + "." +
                    ver.Minor.ToString() + "." +
                    ver.Build.ToString();
                return res;
            }
        }

        static public string GetBuildNumber
        {
            get
            {
                if (String.IsNullOrEmpty(Program.buildNumber))
                {
                    return "";
                }
                else
                {
                    return " (" + Program.buildNumber + ")";
                }
            }
        }
    }
}
