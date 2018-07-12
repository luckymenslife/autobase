using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Interfaces;
using mvMapLib;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using VmpM = Rekod.DataAccess.SourceVMP.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using Rekod.Controllers;

namespace Rekod.Services
{
    public class SQLiteCopyPaste : Rekod.Classes.Singleton<SQLiteCopyPaste>, INotifyPropertyChanged
    {
        #region Поля
        public event PropertyChangedEventHandler PropertyChanged;

        private TypeGeometry _typeGeom;
        private string _copyTableName;
        private bool _existPaste;

        #endregion Поля

        #region Свойства
        public TypeGeometry TypeGeom
        {
            get { return _typeGeom; }
            private set { OnPropertyChanged(ref _typeGeom, value, () => this.TypeGeom); }
        }
        public bool ExistsPaste
        {
            get { return _existPaste; }
            private set { OnPropertyChanged(ref _existPaste, value, () => this.ExistsPaste); }
        }
        private mainFrm _mf
        { get { return Program.mainFrm1; } }
        private AxmvMapLib.AxMapLIb _mv
        { get { return _mf.axMapLIb1; } }
        private Classes.ButtonManager bManager
        { get { return _mf.bManager; } }
        #endregion Свойства

        #region Команды
        #endregion Команды

        #region Конструктор
        private SQLiteCopyPaste()
        {
        }
        #endregion Конструктор


        public int CopyInSQLite(object table, IEnumerable<int> objIds)
        {
            int rowNum = 0;
            ExistsPaste = false;
            var tableM = table as AbsM.ILayerM;
            if (table is Interfaces.tablesInfo)
            {
                var ti = table as Interfaces.tablesInfo;
                TypeGeom = ti.TypeGeom;
                rowNum = CopyToSQLitePg(ti, objIds);
            }
            else if (tableM != null)
            {
                TypeGeom = TypeGeometry.MISSING;
                rowNum = CopyToSQLiteMVLayer(tableM, objIds);
            }
            else
            {
                throw new Exception("Выберете таблицу");
            }
            ExistsPaste = rowNum > 0;
            return rowNum;
        }

        public int CountRowsSQLite()
        {
            using (var slCmd = new SQLiteWork(Program.connStringSQLite, false))
            {
                slCmd.Sql = "SELECT count(*) AS [count] FROM [copy_past_table];";
                return slCmd.ExecuteScalar<int>();
            }
        }
        public int PastFromSQLite(object table)
        {
            var tableM = table as AbsM.ILayerM;
            if (table is Interfaces.tablesInfo)
            {
                var ti = table as Interfaces.tablesInfo;
                return PastFromSQLitePg(Program.app.getTableInfo(ti.idTable));
            }
            else if (tableM != null && tableM.IsEditable)
            {
                return PastFromSQLiteCosm(tableM);
            }
            else
            {
                throw new Exception("Выберете таблицу");
            }
        }

        #region Закрытые методы

        private int CopyToSQLitePg(Interfaces.tablesInfo ti, IEnumerable<int> objIds)
        {
            if (ti == null)
                throw new Exception("Неправильно указана таблица");
            if (objIds == null || objIds.Count() == 0)
                throw new Exception(Rekod.Properties.Resources.MainFrm_SelectedObjNot);

            using (var slCmd = new SQLiteWork(Program.connStringSQLite, true))
            {
                slCmd.TransactionBegin();
                slCmd.Sql = @"DROP TABLE IF EXISTS [copy_past_table];";
                slCmd.ExecuteNonQuery();

                slCmd.InstallSpatialite();

                int rowNum = 0;
                int rowCount = objIds.Count();
                int countErrorGeom = 0;

                cti.ThreadProgress.ShowWait("copyObjects");

                _copyTableName = ti.nameSheme + "." + ti.nameDB;
                var fields = ti.ListField.Where(f => f.nameDB != ti.pkField);
                var fieldsForCreate = new List<string>();
                var fieldsCopy = new List<string>();
                var fieldGeom = string.Empty;
                string createTextGeometryField = string.Empty;

                foreach (var item in fields)
                {
                    switch (item.TypeField)
                    {
                        case Interfaces.TypeField.Integer:
                            fieldsForCreate.Add("[" + item.nameDB + "] INTEGER");
                            fieldsCopy.Add(item.nameDB);
                            break;
                        case Interfaces.TypeField.Text:
                            fieldsForCreate.Add("[" + item.nameDB + "] TEXT");
                            fieldsCopy.Add(item.nameDB);
                            break;
                        case Interfaces.TypeField.Date:
                            fieldsForCreate.Add("[" + item.nameDB + "] DATE");
                            fieldsCopy.Add(item.nameDB);
                            break;
                        case Interfaces.TypeField.DateTime:
                            fieldsForCreate.Add("[" + item.nameDB + "] DATETIME");
                            fieldsCopy.Add(item.nameDB);
                            break;
                        case Interfaces.TypeField.Numeric:
                            fieldsForCreate.Add("[" + item.nameDB + "] NUMERIC");
                            fieldsCopy.Add(item.nameDB);
                            break;
                        case Interfaces.TypeField.Geometry:
                            fieldGeom = item.nameDB;
                            fieldsForCreate.Add("[the_geom] BLOB"); //"+ ti.TypeGeom"
                            //createTextGeometryField = string.Format("SELECT RecoverGeometryColumn('copy_past_table', 'the_geom', {0}, '{1}',2);\n", ti.srid, ti.TypeGeom);
                            //createTextGeometryField = string.Format("SELECT AddGeometryColumn('copy_past_table', 'the_geom', {0}, '{1}', 'XY');\n", ti.srid, ti.TypeGeom);
                            break;
                    }
                }
                string createTextWithFields = string.Join(", \n\t", fieldsForCreate);
                string sqlCreate = string.Format("CREATE TABLE [copy_past_table] (\n\t{0});\n{1}", createTextWithFields, createTextGeometryField);

                slCmd.Sql = sqlCreate;
                slCmd.ExecuteNonQuery();

                var fieldsForSelectPg = new List<string>(fieldsCopy);
                //fieldsForSelectPg.Add("st_astext(" + fieldGeom + ") AS the_geom");
                //fieldsForSelectPg.Add("the_geom::bytea");

                fieldsForSelectPg.Add("st_asbinary(" + fieldGeom + ") AS the_geom");
                string selectWithGeomPG = string.Join(", ", fieldsForSelectPg);


                var fieldsForNameInsertSL = new List<string>(fieldsCopy);
                fieldsForNameInsertSL.Add("the_geom");
                string insertNameWithGeomSL = string.Join(", ", fieldsForNameInsertSL);

                var fieldsForInsertParamsSL = new List<string>(fieldsCopy.Select(f => ":" + f));
                //fieldsForInsertParamsSL.Add("GeomFromText(:the_geom, " + ti.srid + ")");
                fieldsForInsertParamsSL.Add(String.Format("GeomFromWKB(:the_geom, {0})", ti.srid));
                string insertParamsWithGeomSL = string.Join(", ", fieldsForInsertParamsSL);

                List<int> listID = Program.mainFrm1.SelectedObjectsIds.ToList();
                String tableName = ti.nameSheme + "." + ti.nameDB;
                if (listID.Count > 0)
                {
                    using (var sqlCmd = new SqlWork())
                    {
                        string ids = string.Join(", ", listID);
                        sqlCmd.sql = string.Format("SELECT {0} FROM {1} WHERE {2} in ({3})", selectWithGeomPG, tableName, ti.pkField, ids);
                        sqlCmd.ExecuteReader();
                        while (sqlCmd.CanRead())
                        {
                            rowNum++;
                            var count = sqlCmd.GetFiealdCount();
                            var listParams = new List<SqlParam>();
                            for (int i = 0; i < count; i++)
                            {
                                String curFieldName = sqlCmd.GetFieldName(i);
                                if (curFieldName == fieldGeom)
                                {
                                    var param = new SqlParam(":" + curFieldName, DbType.Binary, sqlCmd.GetBytes(i));
                                    listParams.Add(param);
                                }
                                else
                                {
                                    var param = new SqlParam(":" + curFieldName, sqlCmd.GetFieldDbType(i), sqlCmd.GetValue(i));
                                    listParams.Add(param);
                                }
                            }

                            slCmd.Sql = string.Format("INSERT INTO [copy_past_table] ({0}) VALUES ({1})", insertNameWithGeomSL, insertParamsWithGeomSL);
                            slCmd.ExecuteNonQuery(listParams);
                            if (rowNum % 100 == 0)
                            {
                                cti.ThreadProgress.SetText(string.Format("Скопировано: {0} из {1}", rowNum, rowCount));
                            }
                        }
                    }
                }

                cti.ThreadProgress.Close("copyObjects");
                rowNum -= countErrorGeom;

                slCmd.TransactionEnd();

                return rowNum;
            }
        }
        private int CopyToSQLiteMVLayer(AbsM.ILayerM table, IEnumerable<int> objIds)
        {
            if (objIds == null || objIds.Count() == 0)
                throw new Exception(Rekod.Properties.Resources.MainFrm_SelectedObjNot);

            using (var slCmd = new SQLiteWork(Program.connStringSQLite, true))
            {
                slCmd.TransactionBegin();
                slCmd.Sql = @"DROP TABLE IF EXISTS [copy_past_table];";
                slCmd.ExecuteNonQuery();

                slCmd.InstallSpatialite();

                int rowNum = 0;
                int rowCount = objIds.Count();
                int countErrorGeom = 0;

                cti.ThreadProgress.ShowWait("copyObjects");

                _copyTableName = table.NameMap;

                slCmd.Sql = @"CREATE TABLE [copy_past_table] ([the_geom] BLOB);";
                slCmd.ExecuteNonQuery();

                foreach (var item in objIds)
                {
                    rowNum++;
                    var wkt = _mf.SelectedLayer.getObject(item).getWKT();
                    string geomTransform = "GeomFromText(:the_geom, " + Program.srid + ")";
                    var param = new SqlParam(":the_geom", DbType.String, wkt);


                    slCmd.Sql = "INSERT INTO [copy_past_table] (the_geom) VALUES (" + geomTransform + ")";
                    slCmd.ExecuteNonQuery(param);

                    cti.ThreadProgress.SetText(string.Format("Скопировано: {0} из {1}", rowNum, rowCount));
                }

                cti.ThreadProgress.Close("copyObjects");
                rowNum -= countErrorGeom;
                slCmd.TransactionEnd();

                return rowNum;
            }
        }

        private bool GetEqualsGeomType(string wktTypeIn, string wktTypeMain)
        {
            if (string.IsNullOrEmpty(wktTypeMain) || string.IsNullOrEmpty(wktTypeMain))
                return false;

            string tempGeomTypeIn = wktTypeIn.Substring(0, wktTypeIn.IndexOf("("));

            string wktTypeGeomIn = tempGeomTypeIn.ToUpper().Trim();
            string wktTypeGeomMain = wktTypeMain.ToUpper().Trim();

            return (wktTypeGeomIn == wktTypeGeomMain);
        }
        private int PastFromSQLitePg(Interfaces.tablesInfo ti)
        {
            if (ti == null)
                throw new Exception("Неправильно указана таблица");
            int rowNum = 0;
            using (var slCmd = new SQLiteWork(Program.connStringSQLite, false))
            {
                slCmd.InstallSpatialite();
                slCmd.TransactionBegin();

                string where = string.Empty;
                cti.ThreadProgress.ShowWait("pasteobjects");
                switch (ti.TypeGeom)
                {
                    case TypeGeometry.MULTIPOINT:
                        where = "[the_geom_type] = \"MULTIPOINT\" OR [the_geom_type] = \"POINT\"";
                        break;
                    case TypeGeometry.MULTILINESTRING:
                        where = "[the_geom_type] = \"MULTILINESTRING\" OR [the_geom_type] = \"LINESTRING\"";
                        break;
                    case TypeGeometry.MULTIPOLYGON:
                        where = "[the_geom_type] = \"MULTIPOLYGON\" OR [the_geom_type] = \"POLYGON\"";
                        break;
                    default:
                        where = "1 = 1;";
                        break;
                }
                slCmd.Sql = @"SELECT Count(*)
FROM   ( 
       SELECT *, GeometryType([the_geom]) AS [the_geom_type]
       FROM [copy_past_table]       
       )
WHERE " + where + ";";
                var countRows = slCmd.ExecuteScalar<int>();
                if (countRows == 0)
                {
                    cti.ThreadProgress.Close("pasteobjects");
                    return 0;
                   
                }
                
                string selectWithGeomSL;
                string insertNameWithGeomPG;
                string insertParamsWithGeomPG;
                var tableName = ti.nameSheme + "." + ti.nameDB;

                List<string> fieldsForInsertParamsPG = new List<string>();
                if (_copyTableName == tableName)
                {
                    var fields = Program.app.getFieldInfoTable(ti.idTable).Where(f => f.TypeField != Interfaces.TypeField.Geometry && f.nameDB != ti.pkField).Select(f => f.nameDB);

                    var collSQL = new List<string>();
                    var collParams = new List<Interfaces.Params>();

                    var fieldsForSelectSL = new List<string>(fields);
                    fieldsForSelectSL.Add("asbinary(the_geom) AS the_geom, GeometryType(the_geom) as the_geom_type, SRID(the_geom) as the_geom_srid");
                    selectWithGeomSL = string.Join(", ", fieldsForSelectSL);


                    var fieldsForNameInsertPG = new List<string>(fields);
                    fieldsForNameInsertPG.Add("the_geom");
                    insertNameWithGeomPG = string.Join(", ", fieldsForNameInsertPG);

                    fieldsForInsertParamsPG.AddRange(fields.Select(f => ":" + f + "{0}"));
                }
                else
                {
                    selectWithGeomSL = "asbinary(the_geom) AS the_geom, GeometryType(the_geom) as the_geom_type, SRID(the_geom) as the_geom_srid";
                    insertNameWithGeomPG = ti.geomFieldName;
                }

                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.BeginTransaction();
                    var listParams = new List<Interfaces.Params>();
                    var collSQL = new List<string>();

                    slCmd.Sql = string.Format("SELECT {0} FROM [copy_past_table] WHERE {1};", selectWithGeomSL, where);
                    slCmd.Execute();
                    while (slCmd.CanRead())
                    {
                        string type = slCmd.GetValue<string>("the_geom_type");
                        int srid = slCmd.GetValue<int>("the_geom_srid");

                        string geomTransform = GetGeomTransform(type, srid, ti.TypeGeom.ToString(), ti.srid);

                        var listFieldPG = new List<string>();
                        listFieldPG.AddRange(fieldsForInsertParamsPG);
                        listFieldPG.Add(geomTransform);

                        insertParamsWithGeomPG = string.Join(", ", listFieldPG);
                        if (GetEqualsGeomType(ti, type))
                        {
                            rowNum++;
                            for (int i = 0; i < slCmd.GetFieldsCount(); i++)
                            {
                                String curFieldName = slCmd.GetFieldName(i);
                                switch (curFieldName)
                                {
                                    case "the_geom":
                                        {
                                            var param = new Interfaces.Params(":" + curFieldName + rowNum, slCmd.GetBytes(i), DbType.Binary);
                                            listParams.Add(param);
                                        } break;
                                    case "the_geom_type":
                                    case "the_geom_srid":
                                        break;
                                    default:
                                        {
                                            var param = new Interfaces.Params(":" + curFieldName + rowNum, slCmd.GetValue<object>(i), slCmd.GetFieldDbType(i));
                                            listParams.Add(param);
                                        } break;
                                }
                            }
                        }
                        var valueString = string.Format("(" + insertParamsWithGeomPG + ")", rowNum);
                        collSQL.Add(valueString);

                        if (collSQL.Count >= 50 || rowNum == countRows)
                        {
                            var valuesFull = string.Join(", \n", collSQL);
                            sqlCmd.sql = string.Format("INSERT INTO {0} ({1}) VALUES \n{2}", tableName, insertNameWithGeomPG, valuesFull);
                            sqlCmd.ExecuteNonQuery(listParams);

                            collSQL.Clear();
                            listParams.Clear();
                        }
                        if (rowNum % 100 == 0 || rowNum == countRows)
                        {
                            cti.ThreadProgress.SetText(string.Format("Вставлено: {0} из {1}", rowNum, countRows));
                        }
                    }
                    slCmd.CloseReader();
                    sqlCmd.EndTransaction();

                }
                cti.ThreadProgress.Close("pasteobjects");

                classesOfMetods.reloadUseBounds(ti.idTable);

                slCmd.TransactionEnd();
            }
            return rowNum;
        }

        private int PastFromSQLiteCosm(AbsM.ILayerM table)
        {
            if (table == null)
                throw new Exception("Неправильно указана таблица");
            int rowNum = 0;
            using (var slCmd = new SQLiteWork(Program.connStringSQLite, false))
            {
                slCmd.InstallSpatialite();
                slCmd.TransactionBegin();

                cti.ThreadProgress.ShowWait("pasteobjects");
                slCmd.Sql = @"SELECT Count(*) FROM [copy_past_table]";
                var countRows = slCmd.ExecuteScalar<int>();
                if (countRows == 0)
                {
                    cti.ThreadProgress.Close("pasteobjects");
                    return 0;
                }
                slCmd.Sql = "SELECT AsText(Transform([the_geom], " + _mv.SRID + ")) AS [the_geom], GeometryType(the_geom) as the_geom_type FROM [copy_past_table]";
                slCmd.Execute();

                var layer = _mv.getLayer(table.NameMap);
                if (layer == null)
                {
                    cti.ThreadProgress.Close("pasteobjects");
                    return 0;
                }

                while (slCmd.CanRead())
                {
                    var geom = slCmd.GetValue<string>("the_geom");
                    var geomType = slCmd.GetValue<string>("the_geom_type");
                    var mvObj = layer.CreateObject();
                    mvObj.setWKT(geom);
                    switch (geomType)
                    {
                        case "POINT":
                        case "MULTIPOINT":
                            mvObj.style = (table as CosM.CosmeticTableBaseM).DefaultDotStyle.Value;
                            break;
                        case "LINESTRING":
                        case "MULTILINESTRING":
                            mvObj.style = (table as CosM.CosmeticTableBaseM).DefaultLineStyle.Value;
                            break;
                        case "POLYGON":
                        case "MULTIPOLYGON":
                            mvObj.style = (table as CosM.CosmeticTableBaseM).DefaultPolygonStyle.Value;
                            break;
                    }
                    rowNum++;
                    cti.ThreadProgress.SetText(string.Format("Скопировано: {0} из {1}", rowNum, countRows));
                }

                slCmd.CloseReader();

                cti.ThreadProgress.Close("pasteobjects");

                //classesOfMetods.reloadUseBounds(table.idTable);

                slCmd.TransactionEnd();
            }
            return rowNum;
        }

        private static string GetGeomTransform(string typeGeomIn, int sridIn, string typeGeomTable, int? sridTable)
        {
            string geomTransform = "{0}";
            if (sridIn != sridTable)
                geomTransform = "st_transform({0}, " + sridTable + ")";

            geomTransform = string.Format(geomTransform, @"st_geomfromwkb(:the_geom{0}, " + sridIn + ")");

            if (!typeGeomTable.StartsWith("MULTI") && typeGeomIn.StartsWith("MULTI"))
            {
                geomTransform = "ST_GeometryN(" + geomTransform + ", 1)";
            }
            else if (typeGeomTable.StartsWith("MULTI") && !typeGeomIn.StartsWith("MULTI"))
            {
                geomTransform = "st_multi(" + geomTransform + ")";
            }
            return geomTransform;
        }

        private bool GetEqualsGeomType(Interfaces.tablesInfo ti, string wktTypeGeom)
        {
            if (string.IsNullOrEmpty(wktTypeGeom))
                return false;

            string tableTypeGeom = ti.TypeGeom.ToString().ToUpper().Replace("MULTI", "").Trim();

            wktTypeGeom = wktTypeGeom.ToUpper().Replace("MULTI", "").Trim();

            return (tableTypeGeom == wktTypeGeom);

        }
        #endregion Закрытые методы

        #region INotifyPropertyChanged Members
        // Пример использования: OnPropertyChanged(ref _defaultSet, value, () => this.DefaultSet);
        internal void OnPropertyChanged<T>(ref T Value, T newValue, Expression<Func<T>> action)
        {
            if (Value == null && newValue == null)
                return;
            if (Value != null && Value.Equals(newValue))
                return;
            Value = newValue;
            OnPropertyChanged(GetPropertyName(action));
        }
        public void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            OnPropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
