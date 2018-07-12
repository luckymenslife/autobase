using System;
using System.Collections.Generic;
using System.IO;
using OGRFramework;
using System.Data;
using System.ComponentModel;
using Rekod.Services;
using Rekod.ProjectionSelection;
using System.Windows;

namespace Rekod.ImportExport.Exporters
{
    public class SHPExporter
    {
        private tablesInfo tableInfo;
        private FileInfo fileInfo;
        private readonly string progressKey;
        public SHPExporter(tablesInfo tableInfo, FileInfo fileInfo)
        {
            this.tableInfo = tableInfo;
            this.fileInfo = fileInfo;
            progressKey = "SHPExporterProgress" + DateTime.Now.Ticks.ToString();
        }

        public void Export(int? pkValue = null, string where = null, List<Interfaces.IParams> pars = null)
        {
            try
            {
                cti.ThreadProgress.Close(progressKey);

                // Выбор проекции файла
                int? srid = SelectProjectionDialog.Select(tableInfo.srid);
                if (srid == null)
                {
                    return;
                }

                cti.ThreadProgress.ShowWait(progressKey);

                string whereSql = (where == null) ? " where 1=1" : " where " + where;
                // Если мы выгружаем только один объект. Нпример во вкладке геометрия.
                if (pkValue != null)
                    whereSql += string.Format(" and \"{0}\"={1}", tableInfo.pkField, pkValue);

                var rowCount = 0;
                using (var sw = new SqlWork())
                {
                    sw.sql = string.Format("select count(*) from {0}.{1} {2}", tableInfo.nameSheme, tableInfo.nameDB, whereSql);
                    rowCount = sw.ExecuteScalar<Int32>(pars);
                }

                string sridWkt = "";
                using (var sw = new SqlWork())
                {
                    sw.sql = "SELECT srtext FROM spatial_ref_sys where srid=" + srid;
                    sridWkt = sw.ExecuteScalar<String>();
                }

                var shpWork = new SHPWork(fileInfo);
                List<OGRFramework.SHPWork.TypeAndName> fields = new List<SHPWork.TypeAndName>();

                var lf = Program.app.getTableInfo(tableInfo.idTable).ListField;
                lf.Remove(lf.Find(w => w.nameMap == tableInfo.geomFieldName));

                string selString = FormShapeFields(fields, lf);

                var errors = new DataTable();
                string errorString = "";

                string[] creationOptions;
                string[] layerCreationOptions;

                GetCreationOptions(out creationOptions, out layerCreationOptions);
                                        
                
                shpWork.OpenWrite(creationOptions, layerCreationOptions, fields, out errorString, GetGeometryType(), sridWkt);

                if (errorString != "")
                    throw new Exception(errorString);
                    
                errors.Columns.Add("ERROR");

                using (var sw = new SqlWork())
                {
                    sw.sql = string.Format(
                        "SELECT {3} st_astext(st_transform({2}, {5})) as sys_geom_column_for_export FROM {0}.{1} {4}",
                        tableInfo.nameSheme, tableInfo.nameDB, tableInfo.geomFieldName, selString, whereSql, srid);
                    sw.ExecuteReader(pars);

                    List<object> values = new List<object>();
                    
                    for (int rCount = 1; sw.CanRead(); rCount++ )
                    {
                        try
                        {
                            values.Clear();

                            foreach (var f in fields)
                                values.Add(sw.GetValue(f.Name));

                            string result = shpWork.AddObject(values, sw.GetString("sys_geom_column_for_export"));

                            if (result != "" && errors.Rows.Count < 100)
                                errors.Rows.Add(string.Format(
                                    Rekod.Properties.Resources.SHPExporter_Error, 
                                    sw.GetInt32(tableInfo.pkField), 
                                    result, 
                                    sw.GetString("sys_geom_column_for_export")));

                            cti.ThreadProgress.SetText(string.Format(
                                Rekod.Properties.Resources.SHPExporter_ProcessObjCount, 
                                rCount, rowCount));
                        }
                        catch (AccessViolationException ex) { }
                    }
                }

                if (LoadBadObjects(shpWork, errors))
                    shpWork.EndWrite(out errorString, SHPWork.EndWriteOption.AppendBadGeometries);
                else
                    shpWork.EndWrite(out errorString);

                if (errorString != "")
                    throw new Exception(errorString);
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(progressKey);
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            cti.ThreadProgress.Close(progressKey);
        }

        private static string FormShapeFields(List<OGRFramework.SHPWork.TypeAndName> fields, List<Interfaces.fieldInfo> lf)
        {
            string selString = "";
            foreach (var f in lf)
            {
                Type t;
                switch (f.TypeField)
                {
                    case Interfaces.TypeField.Date:
                    case Interfaces.TypeField.DateTime:
                        t = typeof(DateTime);
                        break;
                    case Interfaces.TypeField.Geometry:
                        continue;
                    case Interfaces.TypeField.Integer:
                        t = typeof(Int32);
                        break;
                    case Interfaces.TypeField.Numeric:
                        t = typeof(double);
                        break;
                    case Interfaces.TypeField.Text:
                    case Interfaces.TypeField.Default:
                    default:
                        t = typeof(string);
                        break;
                }
                fields.Add(new SHPWork.TypeAndName() { Name = f.nameDB, TypeDef = t });

                if (f.TypeField == Interfaces.TypeField.DateTime)
                    selString += "\"" + f.nameDB + "\"::timestamp without time zone,";
                else
                    selString += "\"" + f.nameDB + "\",";
            }
            return selString;
        }

        /// <summary>
        /// Загружать ли объекты с некорректной геометрией
        /// </summary>
        /// <param name="errors">Ошибки при выгрузке</param>
        /// <returns>True - если выгрузить без геометрии, False - не загружать объекты</returns>
        private bool LoadBadObjects(SHPWork shpWork, DataTable errors)
        {
            if (errors.Rows.Count > 0)
            {
                cti.ThreadProgress.Close(progressKey);

                ImportExport.ExportQuestion eq = new ImportExport.ExportQuestion(errors);
                eq.countsTB.Text = string.Format(Rekod.Properties.Resources.SHPExporter_LoadForPreview, errors.Rows.Count);
                eq.ShowDialog();
                var ans = eq.Answer;

                cti.ThreadProgress.ShowWait(progressKey);

                if (ans == 0)
                {
                    var bads = shpWork.BadObjectsCount.ToString();
                    //cti.ThreadProgress.SetText("Дозапись объектов: " + bads); форма не успевает так быстро
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Получить тип геометрии 
        /// </summary>
        private SHPWork.GeometryType GetGeometryType()
        {
            // определяем тип геометрии 
            SHPWork.GeometryType gt = SHPWork.GeometryType.None;
            switch (tableInfo.GeomType_GC.ToUpper())
            {
                case "POINT":
                    gt = SHPWork.GeometryType.Point;
                    break;
                case "LINESTRING":
                    gt = SHPWork.GeometryType.Line;
                    break;
                case "POLYGON":
                    gt = SHPWork.GeometryType.Polygon;
                    break;
                case "MULTIPOINT":
                    gt = SHPWork.GeometryType.MultiPoint;
                    break;
                case "MULTILINESTRING":
                    gt = SHPWork.GeometryType.MultiLine;
                    break;
                case "MULTIPOLYGON":
                    gt = SHPWork.GeometryType.MultiPolygon;
                    break;
            }
            return gt;
        }

        /// <summary>
        /// Параметры создания файла
        /// </summary>
        private void GetCreationOptions(out string[] creationOptions, out string[] layerCreationOptions)
        {
            switch (fileInfo.Extension)
            {
                case ".mif":
                    creationOptions = new string[] { "FORMAT=MIF" };
                    layerCreationOptions = null;
                    break;
                case ".xml":
                    creationOptions = new string[] { "USE_EXTENSIONS=YES" };
                    layerCreationOptions = null;
                    break;
                case ".csv":
                    layerCreationOptions = new string[] { "GEOMETRY=AS_WKT" };
                    creationOptions = null;
                    break;
                case ".shp":
                    layerCreationOptions = new string[] { "ENCODING=UTF-8"};
                    creationOptions = null;
                    break;
                default:
                    creationOptions = null;
                    layerCreationOptions = null;
                    break;
            }
        }
    }
}