using System;
using System.Collections.Generic;
using System.IO;
using OGRFramework;
using System.Data;
using System.ComponentModel;
using Npgsql;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using Rekod.Services;
using Rekod.ProjectionSelection; 

namespace Rekod.DataAccess.SourcePostgres.ImportExport.Exporters
{
    public class SHPExporter
    {
        private PgM.PgTableBaseM _pgTable; 
        private FileInfo _fileInfo;
        private NpgsqlConnectionStringBuilder _connect; 
        private readonly string progressKey;

        public SHPExporter(PgM.PgTableBaseM pgTable, FileInfo fileInfo, NpgsqlConnectionStringBuilder connect)
        {
            _connect = connect; 
            _pgTable = pgTable; 
            _fileInfo = fileInfo;
            progressKey = "SHPExporterProgress" + DateTime.Now.Ticks.ToString();
        }
        
        public void Export(int? pkValue = null)
        {
            try
            {
                cti.ThreadProgress.Close(progressKey);

                // Выбор проекции файла
                int? srid = SelectProjectionDialog.Select(_pgTable.Srid);
                if (srid == null)
                {
                    return;
                }

                cti.ThreadProgress.ShowWait(progressKey);

                string whereSql = " where 1=1";
                if (pkValue != null)
                    whereSql += string.Format(" and \"{0}\"={1}", _pgTable.PrimaryKey, pkValue);

                int rowCount; 
                using (var sw = new SqlWork(_connect))
                {
                    sw.sql = string.Format("select count(*) from {0}.{1} {2}", 
                                                _pgTable.SchemeName, 
                                                _pgTable.Name, 
                                                whereSql);
                    sw.ExecuteReader();
                    rowCount = sw.CanRead() ? sw.GetInt32(0) : 0;
                    sw.Close();
                }

                string sridWkt = "";
                using (var sw = new SqlWork())
                {
                    sw.sql = "SELECT srtext FROM spatial_ref_sys where srid=" + srid;
                    sridWkt = sw.ExecuteScalar<String>();
                }

                var shpWork = new SHPWork(_fileInfo);

                List<OGRFramework.SHPWork.TypeAndName> fields;
                string selString = FormShapeFields(out fields);

                string errorString = "";
                shpWork.OpenWrite(new[] { "" }, new[] { "" }, fields, out errorString, GetGeometryType(), sridWkt);
                if (errorString != "")
                    throw new Exception(errorString);

                var errors = new DataTable();
                errors.Columns.Add("ERROR");

                using (SqlWork sw = new SqlWork(_connect))
                {
                    sw.sql = string.Format(
                        "SELECT {3} st_astext(st_transform({2}, {5})) as sys_geom_column_for_export FROM {0}.{1} {4}",
                        _pgTable.SchemeName,
                        _pgTable.Name,
                        _pgTable.GeomField,
                        selString,
                        whereSql,
                        srid);

                    sw.ExecuteReader();

                    List<object> values = new List<object>();

                    for (int rCount = 1; sw.CanRead(); rCount++)
                    {
                        try
                        {
                            values.Clear();

                            foreach (var f in fields)
                                values.Add(sw.GetValue(f.Name));

                            string result = shpWork.AddObject(values, sw.GetString("sys_geom_column_for_export"));

                            if (result != "" && errors.Rows.Count < 100)
                                errors.Rows.Add(string.Format("ID={0} Ошибка={1} Геометрия={2}",
                                    sw.GetInt32(_pgTable.PrimaryKey),
                                    result,
                                    sw.GetString("sys_geom_column_for_export")));

                            cti.ThreadProgress.SetText(string.Format("Обработано объектов {0} из {1}", rCount, rowCount));
                        }
                        catch (AccessViolationException ex) { }
                    }
                    
                    if (LoadBadObjects(shpWork, errors))
                        shpWork.EndWrite(out errorString, SHPWork.EndWriteOption.AppendBadGeometries);
                    else
                        shpWork.EndWrite(out errorString);

                    if (errorString != "")
                        throw new Exception(errorString);
                }
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(progressKey);
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            cti.ThreadProgress.Close(progressKey);
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
            SHPWork.GeometryType gt;
            switch (_pgTable.GeomType)
            {
                case Rekod.DataAccess.AbstractSource.Model.EGeomType.Point:
                    gt = SHPWork.GeometryType.Point;
                    break;
                case Rekod.DataAccess.AbstractSource.Model.EGeomType.Line:
                    gt = SHPWork.GeometryType.Line;
                    break;
                case Rekod.DataAccess.AbstractSource.Model.EGeomType.Polygon:
                    gt = SHPWork.GeometryType.Polygon;
                    break;
                case Rekod.DataAccess.AbstractSource.Model.EGeomType.None:
                default:
                    gt = SHPWork.GeometryType.None;
                    break;
            }
            return gt;
        }

        private string FormShapeFields(out List<OGRFramework.SHPWork.TypeAndName> fields)
        {
            string selString = String.Empty;
            fields = new List<SHPWork.TypeAndName>();

            foreach (var f in _pgTable.Fields)
            {
                Type t;
                if (f.Type != AbstractSource.Model.EFieldType.Geometry)
                {
                    switch (f.Type)
                    {
                        case Rekod.DataAccess.AbstractSource.Model.EFieldType.Date:
                        case Rekod.DataAccess.AbstractSource.Model.EFieldType.DateTime:
                            t = typeof(DateTime);
                            break;
                        case Rekod.DataAccess.AbstractSource.Model.EFieldType.Integer:
                            t = typeof(Int32);
                            break;
                        case Rekod.DataAccess.AbstractSource.Model.EFieldType.Real:
                            t = typeof(double);
                            break;
                        case Rekod.DataAccess.AbstractSource.Model.EFieldType.Text:
                        default:
                            t = typeof(string);
                            break;
                    }
                    fields.Add(new SHPWork.TypeAndName() { Name = f.Name, TypeDef = t });
                    selString += "\"" + f.Name + "\",";
                }
            }
            return selString;
        }
    }
}