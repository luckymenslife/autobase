using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Npgsql;
using axVisUtils.Styles;
using axVisUtils.TableData;
using System.Drawing;
using Rekod;
using Interfaces;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using Rekod.Services;

namespace axVisUtils
{
    class DataConnect
    {
        private Rekod.tablesInfo _table;
        private List<Rekod.fieldInfo> _fields;
        private int? _idObject;
        private string styleField;
        private FormTableData frm;

        private string _wkt;
        public string WKT
        {
            get { return _wkt; }
            set { _wkt = value; }
        }

        private string wkt1;
        private string geomField1, pkfield;
        private bool iswkt;
        public PgAttributesGeomVM GeometryVM;

        //public DealGeometryByHand GeometryControl { get; set; }


        private bool isStyle;
        // Мое Диас 
        public DataConnect(Rekod.tablesInfo table, int? idObject, FormTableData Form1)
        {
            _table = table;
            _idObject = idObject;
            frm = Form1;
            isStyle = table.map_style;
            iswkt = table.type == 1;
            geomField1 = _table.geomFieldName;
            pkfield = table.pkField;
            _fields = classesOfMetods.getFieldInfoTable(table.idTable);
            styleField = "fontname, fontcolor, fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch";
        }
        public void Load()
        {

            loadCol();

            frm.EndLoadDataToForm();
        }
        public Int64 Save(DataColumn[] col)
        {
            try
            {
                try
                {
                    if (isStyle)
                    {
                        return SaveDataAndStyle(col, frm.MyStyles);
                    }
                    else
                    {
                        if (col.Length != 0)
                            return SaveData(col);
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(@"s");
                            return 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Rekod.Classes.workLogFile.writeLogFile(ex, true, true);
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        private void loadCol()
        {
            DataColumn col1;
            string tName = ""
                , tNameDB = "";
            int tType = 0;

            string[] colCount = new string[0];
            string[] colCount_for_select = new string[0];

            frm.Text += _table.nameMap;

            //todo: Заменить: на список колонок в таблице
            //sanek////////////////////определяем максимальную ширину названия полей
            var fields = _fields.FindAll(f => f.visible == true).OrderBy(f => f.Order);
            float max_length = 0;
            foreach (var item in fields)
            {
                string s = item.nameDB;
                Bitmap newImage = new Bitmap(400, 20);
                Font f = new Font("Microsoft Sans Serif", 8);
                Graphics g = Graphics.FromImage(newImage);
                SizeF size = g.MeasureString(s, f);
                if (max_length < size.Width) max_length = size.Width;
            }
            if (max_length > 150) max_length = 150;
            frm.max_text_width = (int)max_length;
            ///////////////////////////sanek

            var selectFields = fields.Select(f => f.nameDB).ToArray();
            var selectName = String.Join(", ", (from String fieldName in selectFields select String.Format("\"{0}\"", fieldName)).ToArray()); 

            var dicValues = new Dictionary<Rekod.fieldInfo, object>();
            if (_idObject != null)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = string.Format(@"
                                SELECT {0} 
                                FROM ""{1}"".""{2}"" 
                                WHERE ""{3}"" = {4}",
                        selectName,
                        _table.nameSheme,
                        _table.nameDB,
                        _table.pkField,
                        _idObject
                        );

                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        foreach (var item in _fields)
                        {

                            if (item.type != 5)
                            {
                                if (item.visible == true)
                                {
                                    dicValues.Add(item, sqlCmd.GetValue(item.nameDB));
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(Rekod.Properties.Resources.InterfaceClass_NoExistsObj);
                    }
                }
            }

            foreach (var ccc in fields)
            {
                int t = ccc.type;
                DataColumn.enType t1;
                #region DataColumn
                switch (t)
                {
                    case 1:
                        t1 = DataColumn.enType.Integer;
                        break;
                    case 2:
                        t1 = DataColumn.enType.Text;
                        break;
                    case 3:
                        t1 = DataColumn.enType.Date;
                        break;
                    case 4:
                        t1 = DataColumn.enType.DateTime;
                        break;
                    case 5:
                        t1 = DataColumn.enType.Geometry;
                        break;
                    case 6:
                        t1 = DataColumn.enType.Numeric;
                        break;
                    default:
                        t1 = DataColumn.enType.Text;
                        break;
                }
                #endregion

                SqlWork sqlCmd;
                if (t1 == DataColumn.enType.Geometry)
                {
                    continue;
                }
                bool isEditable = (ccc.nameDB != _table.pkField)
                                    && (!ccc.read_only);
                col1 = new DataColumn(ccc.nameMap, ccc.nameDB, t1, isEditable);
                col1.IsNotNull = ccc.is_not_null;

                //col1.Data = ccc.name_lable;
                // Добавляем на форму элементы редактирования
                frm.AddColumn(col1, ccc);

                // Если поле является ссылочным, значит надо загружать справочник
                if (ccc.is_reference == true)
                {
                    #region begin
                    // Тип полей справочника, названия  полей в базе и таблицы в базе
                    var refTable = classesOfMetods.getTableInfo((int)ccc.ref_table);
                    var refField = classesOfMetods.getFieldInfo((int)ccc.ref_field);
                    var refFieldName = classesOfMetods.getFieldInfo((int)ccc.ref_field_name);

                    object val = null;
                    if (dicValues.ContainsKey(ccc))
                        val = dicValues[ccc];

                    if (refTable.type == 2 || refTable.type == 3)
                        loadDic(ccc, refTable, refField, refFieldName);
                    else
                        loadTable(refTable, val, refFieldName, refField, ccc.nameDB, (int)ccc.ref_table);

                    #endregion
                }
                // Если поле является по интервалу, значит надо загружать интервалы
                if (ccc.is_interval == true)
                {
                    #region begin
                    // Тип полей справочника, названия  полей в базе и таблицы в базе
                    var refTable = classesOfMetods.getTableInfo((int)ccc.ref_table);
                    var refFieldBegin = classesOfMetods.getFieldInfo((int)ccc.ref_field);
                    var refFieldEnd = classesOfMetods.getFieldInfo((int)ccc.ref_field_end);
                    // Загружаем справочник
                    //loadDic(cmd1, tmpTable, tmpFieldId, tmpFieldName, tmpUserColName, rd.GetString(0));
                    loadDicInterval(ccc, refTable, refFieldBegin, refFieldEnd);
                    #endregion
                }

            }

            // Заполняем данными?
            if (_idObject != null && selectFields.Count() >= 1)
            {
//                using (var sqlCmd = new SqlWork())
//                {
//                    sqlCmd.sql = string.Format(@"
//                            SELECT {0}
//                            FROM ""{1}"".""{2}""
//                            WHERE ""{3}"" = {4}"
//                        ,
//                        selectName,
//                        _table.nameSheme,
//                        _table.nameDB,
//                        _table.pkField,
//                        _idObject);
//                    sqlCmd.ExecuteReader();
//                    if (sqlCmd.CanRead())
//                    {
//                        foreach (var item in selectFields)
//                        {
//                            object value = sqlCmd.GetValue<object>(item);
//                            frm.loadDataToCol(item, value);
//                        }
//                    };
//                }
                foreach(var f in dicValues)
                {
                    frm.loadDataToCol(f.Key.nameDB, f.Value);
                }
            }
            if (isStyle && _idObject != null)
            {
                Styles.objStylesM s1;

                string sql = string.Format(@"
                        SELECT {0}
                        FROM ""{1}"".""{2}""
                        WHERE ""{3}"" = {4}"
                    ,
                    styleField,
                    _table.nameSheme,
                    _table.nameDB,
                    _table.pkField,
                    _idObject);
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = sql;
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        s1 = new StylesM(sqlCmd, classesOfMetods.GetIntGeomType(_table.GeomType_GC));
                    }
                    else
                        s1 = new objStylesM();
                }
                frm.MyStyles = s1;
            }
            else if (isStyle && _idObject == null)
            {
                Styles.objStylesM s1 = new axVisUtils.Styles.objStylesM();
                frm.MyStyles = s1;
            }
        }
        private void loadDic(Rekod.fieldInfo field, Rekod.tablesInfo refTable, Rekod.fieldInfo id, Rekod.fieldInfo name)
        {
            StylesVM va = Program.CachedStyles.GetStyle(_table, field);
            frm.AddDictionary(field, va.ListStyles.ToArray(), classesOfMetods.GetIntGeomType(_table.GeomType_GC));
        }
        private void loadTable(Rekod.tablesInfo table, object val, Rekod.fieldInfo colShowed, Rekod.fieldInfo colCompared, string colname, int id_table)
        {
            object showVal = null;
            if (val == null || frm.isNew)
            {
                frm.AddTable(table.nameDB, colname, null, showVal, id_table);
                return;
            }
            using (var sqlCmd = new SqlWork())
            {
                var par = new[]
                {
                    new Interfaces.Params(":value", val, System.Data.DbType.Int32)
                };
                sqlCmd.sql = string.Format(@"
                        SELECT ""{0}"" 
                        FROM ""{1}"".""{2}"" 
                        WHERE ""{3}"" = :value",
                    colShowed.nameDB,
                    table.nameSheme,
                    table.nameDB,
                    colCompared.nameDB
                    );
                showVal = sqlCmd.ExecuteScalar(par);
            }
            frm.AddTable(table.nameDB, colname, val, showVal, id_table);
        }
        #region UPD

        private void loadDicInterval(Rekod.fieldInfo field, Rekod.tablesInfo Table, Rekod.fieldInfo fBegin, Rekod.fieldInfo fEnd)
        {
            List<ValueInterval> dic1 = new List<ValueInterval>();
            try
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = string.Format(@"
                            SELECT ""{0}"", ""{1}"", ""{2}"" 
                            FROM {3}.""{4}"" 
                            ORDER BY ""{0}"";"
                        ,
                        Table.pkField,
                        fBegin.nameDB,
                        fEnd.nameDB,
                        Table.nameSheme,
                        Table.nameDB);

                    sqlCmd.Execute(false);
                    while (sqlCmd.CanRead())
                    {
                        double? d1s = sqlCmd.GetValue<double?>(fBegin.nameDB);
                        double? d2s = sqlCmd.GetValue<double?>(fEnd.nameDB);

                        //var dic = new ValueInterval(d1s, d2s);
                        dic1.Add(new ValueInterval(d1s, d2s));
                    }
                }
            }
            catch
            {
                dic1.Clear();
                dic1.Add(new ValueInterval(double.MinValue, double.MaxValue));
            }
            frm.AddInterval(field.nameDB, dic1.ToArray(), Table.idTable);

        }

        #endregion
        private Int64 SaveData(DataColumn[] col)
        {
            if (col.Length < 1)
            {
                return 0;
            }
            string sql = "";
            var param = new List<IParams>();
            if (_idObject == null)
            {
                sql = String.Format("INSERT INTO \"{0}\".\"{1}\" (  ", _table.nameSheme, _table.nameDB); // 2 пробела
                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i].BaseName != pkfield && col[i].isEdited) // Не понятно зачем это условие.. Можно было писать id и в значения подставлять siquince, не пришлось бы удалять 2 символа в конце строки, не стал менять подумал что раз есть значит надо..
                    {
                        sql += "\"" + col[i].BaseName + "\", ";
                    }
                    if (col.Length == 1) // varyag 
                    {
                        sql += col[0].BaseName + ", ";
                    }
                }
                if (iswkt)
                {
                    if (col.Length == 1) // varyag 
                    {
                        sql = sql.Trim(new char[] { ' ', ',' });
                        if (_wkt != null)
                        {
                            sql += ", " + geomField1 + ") VALUES (  "; // 2 пробела Varyag
                        }
                        else
                        {
                            sql += ") VALUES (  ";
                        }
                    }
                    else
                    {
                        if (_wkt != null)
                        {
                            sql += " " + geomField1 + ") VALUES (  "; // 2 пробела Varyag
                        }
                        else
                        {
                            sql = sql.Trim(new char[] { ' ', ',' });
                            sql += ") VALUES (  ";
                        }
                    }
                }
                else
                {
                    sql = sql.Substring(0, sql.Length - 2) + ") VALUES(  "; // 2 пробела varyag
                }

                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i].BaseName != pkfield && col[i].isEdited)
                    {
                        if (col[i].Data != null)
                        {
                            var par = new Params();
                            par.paramName = "@param" + param.Count;
                            switch (col[i].Type)
                            {
                                case DataColumn.enType.Text:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                                case DataColumn.enType.Date:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Date;
                                    break;
                                case DataColumn.enType.DateTime:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                    break;
                                case DataColumn.enType.Integer:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                                    break;
                                case DataColumn.enType.Numeric:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Numeric;
                                    break;
                                case DataColumn.enType.Geometry:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                            } 
                            par.value = col[i].Data;

                            param.Add(par);
                            sql += par.paramName + ", ";
                        }
                        else
                        {
                            sql += "null, ";
                        }

                    }
                    else
                    {
                        if (col.Length == 1) // Varyag
                        {
                            sql += "default, "; // schema_2 + "." + idNameTable + "id_sec(nextval)";
                        }
                    }
                }
                if (iswkt)
                {
                    if (_wkt != null)
                    {
                        sql += "" + _wkt + ") RETURNING " + pkfield + "; ";
                    }
                    else
                    {
                        sql = sql.Trim(new[] { ' ', ',' });
                        sql += ") RETURNING " + pkfield + "; ";
                    }
                }
                else
                {
                    sql = sql.Substring(0, sql.Length - 2) + ") RETURNING " + pkfield + "; ";
                }
            }
            else if (_idObject != null)
            {
                if (col.Length == 1 && !iswkt) // Тут тоже исправил. Было условие if(col.Length == 1). Когда не было редактирования геометрии этого условия было достаточно
                    return 0;
                sql = String.Format("UPDATE \"{0}\".\"{1}\" SET ", _table.nameSheme, _table.nameDB);
                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i] == null)
                    {
                        break;
                    }
                    if (col[i].BaseName != pkfield && col[i].isEdited)
                    {
                        if (col[i].Data != null)
                        {
                            var par = new Params();
                            par.paramName = "@param" + param.Count;
                            switch (col[i].Type)
                            {
                                case DataColumn.enType.Text:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                                case DataColumn.enType.Date:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Date;
                                    break;
                                case DataColumn.enType.DateTime:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                    break;
                                case DataColumn.enType.Integer:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                                    break;
                                case DataColumn.enType.Numeric:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Numeric;
                                    break;
                                case DataColumn.enType.Geometry:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                            }
                            par.value = col[i].Data;

                            param.Add(par);
                            sql += "\"" + col[i].BaseName + "\" =" + par.paramName + ", ";
                        }
                        else
                        {
                            sql += "\"" + col[i].BaseName + "\"=null, ";
                        }
                    }
                }
                if (iswkt && GeometryVM != null && GeometryVM.HasChanges)
                {
                    if (_wkt == null)
                    {
                        sql += geomField1 + "=NULL WHERE " + pkfield + "=" + _idObject + " RETURNING " + pkfield + "; ";
                    }
                    else
                    {
                        sql += String.Format("{0} = {1} WHERE {2} = {3} RETURNING " + pkfield + "; ",
                                                geomField1,
                                                _wkt,
                                                pkfield,
                                                _idObject);
                    }
                }
                else
                {
                    if (sql == String.Format("UPDATE \"{0}\".\"{1}\" SET ", _table.nameSheme, _table.nameDB))
                        return 0;
                    else
                        sql = sql.Substring(0, sql.Length - 2) + " WHERE " + pkfield + "=" + _idObject + "RETURNING " + pkfield + "; ";
                }
            }

            if (!String.IsNullOrEmpty(sql))
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = sql;
                    return sqlCmd.ExecuteUpdateReturningGid(param);
                }
            }
            else
            {
                return 0;
            }
        }
        private Int64 SaveDataAndStyle(DataColumn[] col, Styles.objStylesM style)
        {
            if (col.Length < 1)
            {
                return 0;
            }
            string sql = "";
            var param = new List<IParams>();
            if (_idObject == null)
            {
                sql = String.Format("INSERT INTO \"{0}\".\"{1}\" (", _table.nameSheme, _table.nameDB);

                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i].BaseName != pkfield && col[i].isEdited)
                    {
                        sql += "\"" + col[i].BaseName + "\",  ";
                    }
                }
                if (col.Length > 1) // Varyag
                {
                    sql = sql.Substring(0, sql.Length - 2);
                }
                if (col.Length == 1)
                {
                    sql += col[0].BaseName + ",";
                }
                if (isStyle)
                {
                    sql += " fontname, fontcolor, fontframecolor, fontsize, symbol," +
                        " pencolor, pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch";
                }
                sql += ") VALUES(  "; // 2 пробела Varyag
                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i].BaseName != pkfield && col[i].isEdited)
                        if (col[i].Data != null)
                        {
                            var par = new Params();
                            par.paramName = "@param" + param.Count;
                            switch (col[i].Type)
                            {
                                case DataColumn.enType.Text:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                                case DataColumn.enType.Date:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Date;
                                    break;
                                case DataColumn.enType.DateTime:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                    break;
                                case DataColumn.enType.Integer:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                                    break;
                                case DataColumn.enType.Numeric:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Numeric;
                                    break;
                                case DataColumn.enType.Geometry:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                            }
                            par.value = col[i].Data;

                            param.Add(par);
                            sql += par.paramName + ", ";
                        }
                        else
                        {
                            sql += "null, ";
                        }
                }
                sql = sql.Substring(0, sql.Length - 2);
                if (isStyle)
                {

                    if (col.Length == 1) // Varyag
                    {
                        sql += "default"; // schema_2 + "." + idNameTable + "id_sec(nextval)";
                    }
                    sql += ", '";
                    sql += style.FontStyle.FontName + "', ";
                    sql += style.FontStyle.Color + ", ";
                    sql += style.FontStyle.FrameColor + ", ";
                    sql += style.FontStyle.Size + ", ";
                    sql += style.SymbolStyle.Shape + ", ";
                    sql += style.PenStyle.Color + ", ";
                    sql += style.PenStyle.Type + ", ";
                    sql += style.PenStyle.Width + ", ";

                    sql += style.BrushStyle.bgColor + ", ";
                    sql += style.BrushStyle.fgColor + ", ";
                    sql += style.BrushStyle.Style + ", ";
                    sql += style.BrushStyle.Hatch;

                }
                sql += ") RETURNING " + pkfield + "; ";

            }
            else if (_idObject != null)
            {
                sql = String.Format("UPDATE \"{0}\".\"{1}\" SET ", _table.nameSheme, _table.nameDB);
                for (int i = 0; i < col.Length; i++)
                {
                    if (col[i].BaseName != pkfield && col[i].isEdited)
                        if (col[i].Data != null)
                        {
                            Params par = new Params();
                            par.paramName = "@param" + param.Count;
                            switch (col[i].Type)
                            {
                                case DataColumn.enType.Text:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                                case DataColumn.enType.Date:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Date;
                                    break;
                                case DataColumn.enType.DateTime:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                    break;
                                case DataColumn.enType.Integer:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                                    break;
                                case DataColumn.enType.Numeric:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Numeric;
                                    break;
                                case DataColumn.enType.Geometry:
                                    par.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                                    break;
                            }
                            par.value = col[i].Data;

                            param.Add(par);
                            sql += "\"" + col[i].BaseName + "\" =" + par.paramName + ", ";
                        }
                        else
                        {
                            sql += "\"" + col[i].BaseName + "\"=null, ";
                        }
                }
                if (col.Length != 1)
                {
                    sql = sql.Substring(0, sql.Length - 2);
                }
                if (isStyle)
                {
                    if (col.Length != 1)
                    {
                        sql += ", ";
                    }
                    sql += "fontname='" + style.FontStyle.FontName + "', ";
                    sql += "fontcolor=" + style.FontStyle.Color + ", ";
                    sql += "fontframecolor=" + style.FontStyle.FrameColor + ", ";
                    sql += "fontsize=" + style.FontStyle.Size + ", ";
                    sql += "symbol=" + style.SymbolStyle.Shape + ", ";
                    sql += "pencolor=" + style.PenStyle.Color + ", ";
                    sql += "pentype=" + style.PenStyle.Type + ", ";
                    sql += "penwidth=" + style.PenStyle.Width + ", ";
                    sql += "brushbgcolor=" + style.BrushStyle.bgColor + ", ";
                    sql += "brushfgcolor=" + style.BrushStyle.fgColor + ", ";
                    sql += "brushstyle=" + style.BrushStyle.Style + ", ";
                    sql += "brushhatch=" + style.BrushStyle.Hatch;
                }
                sql += " WHERE " + pkfield + "=" + _idObject + " RETURNING " + pkfield + "; ";
            }


            bool rezult = false;
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = sql;
                rezult = sqlCmd.ExecuteNonQuery(param);
            }
            // тут нужна перезагрузка стиля
            classesOfMetods.ReloadRelatedTables(_table.idTable);
            return 0;
        }
    }
    public struct col_info
    {
        public string name_db;
        public string name_map;
        public string name_lable;
        public int type_field;
        public bool is_reference;
        public bool is_interval;
        public bool is_style;
        public int ref_table;
        public int ref_field;
        public int ref_field_end;
        public int ref_field_name;
        public bool read_only;
    }
}