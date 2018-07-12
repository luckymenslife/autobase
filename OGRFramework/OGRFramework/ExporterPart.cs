using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace OGRFramework
{
    public partial class SHPWork
    {
        public enum EndWriteOption { DoNotWriteBadGeometries, AppendBadGeometries };
        public enum GeometryType { None, Point, Line, Polygon, MultiPoint, MultiLine, MultiPolygon };
        private List<Feature> exporterBadFeatures;
        private SpatialReference exporterSrid;
        private Layer exporterLayer;
        private FeatureDefn exporterFeatDefn;
        private DataSource exporterDataSource;
        public int BadObjectsCount
        {
            get
            {
                if (exporterBadFeatures != null)
                    return exporterBadFeatures.Count;
                throw new Exception("Файл не открыт в режиме записи объектов");
            }
        }
        public class TypeAndName
        {
            public string Name { get; set; }
            public Type TypeDef { get; set; }
        }
        private bool exporterOpened;

        #region Open Write
        private void openWrite(List<TypeAndName> fields, GeometryType geomType, string sridWKT, string [] creationOptions, string [] layerCreationOptions)
        {
            exporterOpened = false;
            wkbGeometryType gt;
            switch (geomType)
            {
                case GeometryType.Point:
                    gt = wkbGeometryType.wkbPoint;
                    break;
                case GeometryType.Line:
                    gt = wkbGeometryType.wkbLineString;
                    break;
                case GeometryType.Polygon:
                    gt = wkbGeometryType.wkbPolygon;
                    break;
                case GeometryType.MultiPoint:
                    gt = wkbGeometryType.wkbMultiPoint;
                    break;
                case GeometryType.MultiLine:
                    gt = wkbGeometryType.wkbMultiLineString;
                    break;
                case GeometryType.MultiPolygon:
                    gt = wkbGeometryType.wkbMultiPolygon;
                    break;
                case GeometryType.None:
                default:
                    gt = wkbGeometryType.wkbNone;
                    break;
            }
        //    gt = wkbGeometryType.wkbUnknown;
            Ogr.RegisterAll();
            var driver = Ogr.GetDriverByName(DriverName);
            if (driver == null)
                throw new Exception("Не удается найти драйвер " + DriverName);
            if (driver.name == "SQLite")
            {
                if (inputFile.Exists)
                    inputFile.Delete();
                exporterDataSource = driver.CreateDataSource(inputFile.FullName, creationOptions);
            }
            else
            {
                string ext = inputFile.Extension;
                if (ext == ".bna" || ext == ".geojson" || ext == ".gml" || ext == ".itf" || ext == ".kml" || ext == ".mif" || ext == ".gxt" || ext == ".xml" || ext == ".dxf")
                {
                    exporterDataSource = driver.CreateDataSource(inputFile.Directory.FullName, creationOptions);
                }
                else exporterDataSource = driver.CreateDataSource(inputFile.FullName, creationOptions);
            }
            if (exporterDataSource == null)
                throw new Exception("Не удается создать источник данных");
            var srid = new SpatialReference(sridWKT);
            if (srid == null)
                throw new Exception("Не удается создать проекцию");
            
            exporterSrid = srid;
            exporterLayer = exporterDataSource.CreateLayer(LayerName, srid, gt, layerCreationOptions);
            if (exporterLayer == null)
                throw new Exception("Не удается создать слой");
            exporterBadFeatures = new List<Feature>();
            createExporterFieldDefns(fields);
            exporterLayer.StartTransaction();
            exporterOpened = true;
        }

        public void OpenWrite(string[] creationOptions, string[] layerCreationOptions, List<TypeAndName> fields, out string errorMessage, GeometryType geomType = GeometryType.None, string sridWKT = "")
        {
            errorMessage = "";
            try { openWrite(fields, geomType, sridWKT, creationOptions, layerCreationOptions); }
            catch (Exception ex) { throw; errorMessage = ex.Message; }
            
        }

        public void OpenWrite(string[] creationOptions, string[] layerCreationOptions, List<TypeAndName> fields, GeometryType geomType = GeometryType.None, string sridWKT = "")
        {
            try { openWrite(fields, geomType, sridWKT, creationOptions, layerCreationOptions); }
            catch { }
        }

        private void createExporterFieldDefns(List<TypeAndName> fields)
        {
            FieldDefn fieldDefn;
            exporterFeatDefn = new FeatureDefn("ok");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                switch (field.TypeDef.ToString())
                {
                    case "System.Int32":
                        fieldDefn = new FieldDefn(field.Name, FieldType.OFTInteger);
                        break;
                    case "System.DateTime":
                        fieldDefn = new FieldDefn(field.Name, FieldType.OFTString);
                        //fieldDefn = new FieldDefn(field.Name, FieldType.OFTDateTime);
                        break;
                    case "System.Double":
                        fieldDefn = new FieldDefn(field.Name, FieldType.OFTReal);
                        break;
                    case "System.String":
                    default:
                        fieldDefn = new FieldDefn(field.Name, FieldType.OFTString);
                        break;
                }
                exporterLayer.CreateField(fieldDefn, 1);
                exporterFeatDefn.AddFieldDefn(fieldDefn);
                fieldDefn.Dispose();
            }
        }
        #endregion

        #region Add Object
        private string addObject(List<object> values, string geomWKT)
        {
            if (!exporterOpened)
                throw new Exception("Не удалось открыть файл для записи");
            var feat = new Feature(exporterFeatDefn);
            var errorMsg = "";
            var fc = feat.GetFieldCount();
            if (fc != values.Count)
                throw new Exception("Количество данных не совпадает с количеством столбцов в файле");
            for (int i = 0; i < fc; i++)
            {
                var val = values[i];
                if (val == null || val.ToString() == "")
                {
                    feat.UnsetField(i);
                    continue;
                }
                string valstring = val.ToString();
                switch (feat.GetFieldType(i))
                {
                    case FieldType.OFTInteger:
                        feat.SetField(i, int.Parse(valstring));
                        break;
                    case FieldType.OFTDateTime:
                        var dateTime = DateTime.Parse(valstring);
                        feat.SetField(i, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0);
                        break;
                    case FieldType.OFTReal:
                        feat.SetField(i, double.Parse(valstring));
                        break;
                    case FieldType.OFTString:
                    default:
#if LNG_MN
                        feat.SetField(i, Encoding.Default.GetString(Encoding.UTF8.GetBytes(valstring)));
#else
                        feat.SetField(i, valstring);
#endif
                        break;
                }
            }
            if (geomWKT != null && geomWKT != "")
                try
                {
                    var g = Ogr.CreateGeometryFromWkt(ref geomWKT, exporterSrid);
                    feat.SetGeometry(g);
                }
                catch (Exception ex) { errorMsg = ex.Message; exporterBadFeatures.Add(feat.Clone()); }
            if (errorMsg == "")
                exporterLayer.CreateFeature(feat);
            feat.Dispose();
            return errorMsg;
        }

        public string AddObject(List<object> values, string geomWKT = "")
        {
            string errorMessage = "", result = "";
            try { result = addObject(values, geomWKT); }
            catch (Exception ex) { errorMessage = ex.Message; }
            if (result + errorMessage == "")
                return "";
            else if (errorMessage != "")
                return errorMessage;
            else return result;
        }
        #endregion

        #region End Write
        private void endWrite(EndWriteOption option)
        {
            exporterOpened = false;
            if (option == EndWriteOption.AppendBadGeometries && exporterBadFeatures != null)
                foreach (var feat in exporterBadFeatures)
                    exporterLayer.CreateFeature(feat);
            exporterLayer.CommitTransaction();
            exporterLayer.SyncToDisk();
            exporterBadFeatures = null;
            exporterSrid.Dispose();
            exporterSrid = null;
            exporterFeatDefn.Dispose();
            exporterFeatDefn = null;
            exporterLayer.Dispose();
            exporterLayer = null;
            exporterDataSource.Dispose();
            exporterDataSource = null;
        }

        public void EndWrite(out string errorMessage, EndWriteOption option = EndWriteOption.DoNotWriteBadGeometries)
        {
            errorMessage = "";
            try { endWrite(option); }
            catch (Exception ex) { errorMessage = ex.Message; }
        }

        public void EndWrite(EndWriteOption option = EndWriteOption.DoNotWriteBadGeometries)
        {
            try { endWrite(option); }
            catch { }
        }
        #endregion
    }
}
