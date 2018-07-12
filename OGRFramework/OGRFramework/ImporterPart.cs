using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
//using ProjNet.Converters.WellKnownText;
//using ProjNet.CoordinateSystems;
//using ProjNet.CoordinateSystems.Transformations;

namespace OGRFramework
{
    public partial class SHPWork
    {
        private Layer importLayer;
        private DataSource importDataSource;
        private Feature importerCurrentFeature;
        private List<string> shapeFieldNames;

        #region Start End Read
        public void StartRead()
        {
            try
            {
                Ogr.RegisterAll();
                importDataSource = Ogr.Open(inputFile.FullName, 0);
                if (importDataSource == null)
                    throw new Exception("Невозможно открыть файл");
                var layerCount = importDataSource.GetLayerCount();
                if (layerCount != 1)
                    throw new Exception("В файле должен быть ровно один слой");
                importLayer = importDataSource.GetLayerByIndex(0);

                shapeFieldNames = GetShapeFieldNames();
            }
            catch
            {
                importLayer = null;
                importDataSource = null;
            }
        }
        
        public bool Read()
        {
            importerCurrentFeature = importLayer.GetNextFeature();
            return importerCurrentFeature != null;
        }

        public void EndRead()
        {
            try { importLayer.Dispose(); }
            catch { }
            try { importDataSource.Dispose(); }
            catch { }
        }
        #endregion

        #region Feature Count
        private int getFeatureCount(out string errorMessage)
        {
            errorMessage = "";
            int featureCount = 0;
            try
            {
                DataSource ds = Ogr.Open(inputFile.FullName, 0);
                if (ds == null)
                    throw new Exception("Невозможно открыть файл");
                var layerCount = ds.GetLayerCount();
                if (layerCount != 1)
                    throw new Exception("В файле должен быть ровно один слой");
                var layer = ds.GetLayerByIndex(0);
                featureCount = layer.GetFeatureCount(1);
                layer.Dispose();
                ds.Dispose();
            }
            catch (Exception ex) { errorMessage = ex.Message; }
            return featureCount;
        }
        public int GetFeattureCount(out string errorMessage)
        {
            return GetFeattureCount(out errorMessage);
        }
        public int GetFeatureCount()
        {
            string errorMessage;
            return getFeatureCount(out errorMessage);
        }
        #endregion

        #region Get Shape Fields
        private Dictionary<string, Type> getShapeFields(out string errorMessage)
        {
            errorMessage = "";
            Dictionary<string, Type> fields = new Dictionary<string, Type>();
            try
            {
                DataSource ds = Ogr.Open(inputFile.FullName, 0);
                if (ds == null)
                    throw new Exception("Невозможно открыть файл");
                var layerCount = ds.GetLayerCount();
                if (layerCount != 1)
                    throw new Exception("В файле должен быть ровно один слой");
                var layer = ds.GetLayerByIndex(0);

                FeatureDefn featDef = layer.GetLayerDefn();
                var fc = featDef.GetFieldCount();
                for (int i = 0; i < fc; i++)
                {
                    var field = featDef.GetFieldDefn(i);
                    fields.Add(field.GetNameRef(), convertType(field.GetFieldType()));
                }
                featDef.Dispose();
                layer.Dispose();
                ds.Dispose();
            }
            catch (Exception ex) { errorMessage = ex.Message; }

            return fields;
        }
        private Type convertType(FieldType type)
        {
            switch (type)
            {
                case FieldType.OFTDate:
                case FieldType.OFTDateTime:
                case FieldType.OFTTime:
                    return typeof(DateTime);
                case FieldType.OFTInteger:
                    return typeof(int);
                case FieldType.OFTReal:
                    return typeof(double);
                case FieldType.OFTString:
                default:
                    return typeof(string);
            }
        }
        public Dictionary<string, Type> GetShapeFields(out string errorMessage)
        {
            return getShapeFields(out errorMessage);
        }
        public Dictionary<string, Type> GetShapeFields()
        {
            string errorMessage = null;
            var fields = getShapeFields(out errorMessage);
            if (!String.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage);
            return fields;
        }
        public List<string> GetShapeFieldNames(out string errorMessage)
        {
            return getShapeFields(out errorMessage).Keys.ToList();
        }
        public List<string> GetShapeFieldNames()
        {
            string errorMessage;
            return getShapeFields(out errorMessage).Keys.ToList();
        }
        #endregion

        #region operator []
        public object this[int i]
        {
            get
            {
                var feat = importerCurrentFeature;
                int pnYear, pnMonth, pnDay, pnHour, pnMinute, pnSecond, pnTzFlag, valCount;
                var fc = feat.GetFieldCount();
                if (i >= fc)
                    return null;
                int iField = i;
                if (feat.IsFieldSet(iField) && feat.GetFieldAsString(iField) != "")
                    switch (feat.GetFieldType(iField))
                    {
                        case FieldType.OFTBinary:
                            return feat.GetFieldAsString(iField);
                        case FieldType.OFTDate:
                        case FieldType.OFTTime:
                        case FieldType.OFTDateTime:
                            feat.GetFieldAsDateTime(iField, out pnYear, out pnMonth, out pnDay, out pnHour, out pnMinute, out pnSecond, out pnTzFlag);
                            return new DateTime(pnYear, pnMonth, pnDay, pnHour, pnMinute, pnSecond);
                        case FieldType.OFTInteger:
                            return feat.GetFieldAsInteger(iField);
                        case FieldType.OFTIntegerList:
                            return feat.GetFieldAsIntegerList(iField, out valCount);
                        case FieldType.OFTReal:
                            return feat.GetFieldAsDouble(iField);
                        case FieldType.OFTRealList:
                            return feat.GetFieldAsDoubleList(iField, out valCount);
                        case FieldType.OFTString:
                        case FieldType.OFTWideString:
                            string return_string = feat.GetFieldAsString(iField);
                            if (DriverName == "ESRI Shapefile")
                            {
                                return_string = StringInTargetEncoding(return_string, Encoding.Default, Encoding.UTF8);
                            }
                            //Encoding ansi = Encoding.GetEncoding(1251);
                            //Encoding utf8 = Encoding.UTF8;
                            //Encoding iso8859 = ASCIIEncoding.GetEncoding("ISO-8859-1");
                            //String firstConvert =  StringInTargetEncoding(returnValue, ansi, utf8);
                            //String secondConvert = StringInTargetEncoding(firstConvert, iso8859, ansi);
                            return return_string;
                        case FieldType.OFTStringList:
                        case FieldType.OFTWideStringList:
                            return feat.GetFieldAsStringList(iField);
                    }
                return null;
            }
        }

        private String StringInTargetEncoding(String srcString, Encoding srcEnc, Encoding trgEnc)
        {
            byte[] srcBytes = srcEnc.GetBytes(srcString);
            return trgEnc.GetString(srcBytes);
        }

        public object this[string name]
        {
            get
            {
                return this[shapeFieldNames.IndexOf(name)];
            }
        }
        #endregion

        public byte[] TransformedCurrentGeometry(string wktTo, string wktFrom)
        {
            if (importerCurrentFeature == null)
                    return null;
            return transform(wktTo, wktFrom);
        }

        private byte[] transform(string wktTo, string wktFrom)
        {
            if (importLayer == null) 
                return null;

            SpatialReference srFrom = null;
            double[] toWGS84Data = new double[7];
            if (wktFrom == null)
            {
                srFrom = importLayer.GetSpatialRef();
            }
            else
            {
                srFrom = new SpatialReference(wktFrom);
            }
            srFrom.GetTOWGS84(toWGS84Data);
            if (toWGS84Data[0] == 0)
            {
                srFrom.SetTOWGS84(24, -123, -94, 0.02, -0.25, -0.13, 1.1);
            }

            var geom = importerCurrentFeature.GetGeometryRef();

            if (importLayer.GetSpatialRef() == null)
            {
                geom.AssignSpatialReference(srFrom);
            }

            SpatialReference srTo = new SpatialReference(wktTo);
            srTo.GetTOWGS84(toWGS84Data);
            if (toWGS84Data[0] == 0)
            {
                srTo.SetTOWGS84(24, -123, -94, 0.02, -0.25, -0.13, 1.1);
            }
            byte[] wkb = null;
            //sr.SetTOWGS84(24, -123, -94, 0.02, -0.25, -0.13, 1.1);
            if (geom != null)
            {
                string srFromWKT, srToWKT;
                srFrom.ExportToWkt(out srFromWKT);
                srTo.ExportToWkt(out srToWKT);

                if (srFromWKT != srToWKT)
                {
                    geom.TransformTo(srTo);
                }

                wkb = new byte[geom.WkbSize()];
                geom.ExportToWkb(wkb);
            }

            geom.Dispose();

            return wkb;
        }
        #region PROJ transform
        //private string Transform(string wkt, string FromProj, string ToProj)
        //{
        //    var latlon = (ICoordinateSystem)CoordinateSystemWktReader.Parse(FromProj);
        //    //var latlon = (IGeographicCoordinateSystem)CoordinateSystemWktReader.Parse(ToProj);
        //    var mercator = (ICoordinateSystem)CoordinateSystemWktReader.Parse(ToProj);
        //    var ctfac = new CoordinateTransformationFactory();
        //    var transformation = ctfac.CreateFromCoordinateSystems(latlon, mercator);
        //    var transform = transformation;
        //    double[] data = GetGeomPoint(ref wkt);
        //    object[] dataObj = new object[data.Length];
        //    for (int i = 0; data.Length > i; i = i + 2)
        //    {
        //        var converted = transform.MathTransform.Transform(new double[] { data[i], data[i + 1] });
        //        dataObj[i] = converted[0].ToString().Replace(",", ".");
        //        dataObj[i + 1] = converted[1].ToString().Replace(",", ".");
        //    }

        //    return String.Format(wkt, dataObj);
        //}
        //private double[] GetGeomPoint(ref string wkt)
        //{
        //    char s;
        //    int start = 0, count = 0;
        //    List<double> listDouble = new List<double>();
        //    List<char> charList = new List<char>();
        //    List<char> charListFull = new List<char>();
        //    double[] date;
        //    if (wkt.IndexOf("(") > -1)
        //    {
        //        start = wkt.IndexOf("(");
        //    }
        //    for (int i = 0; i < start; i++)
        //    {
        //        charListFull.Add(wkt[i]);
        //    }
        //    for (int i = start; wkt.Length > i; i++)
        //    //for (int i = start; 200 > i; i++)
        //    {
        //        s = wkt[i];
        //        if (s != ' ' && s != ',' && s != '(' && s != ')')
        //        {
        //            if (charList.Count == 0)
        //            {
        //                charListFull.Add('{');
        //                charListFull.AddRange(count.ToString());
        //                charListFull.Add('}');
        //                count++;
        //            }
        //            charList.Add(s);
        //        }
        //        else
        //        {
        //            charListFull.Add(s);
        //            string d = new string(charList.ToArray());
        //            if (d != "" && d != null)
        //            {
        //                d = d.Replace(".", ",");
        //                listDouble.Add(Convert.ToDouble(d));
        //            }
        //            charList.Clear();
        //        }
        //    }
        //    wkt = new string(charListFull.ToArray());
        //    date = listDouble.ToArray();
        //    return date;
        //}
        #endregion
        public string getGeomName()
        {
            DataSource ds = Ogr.Open(inputFile.FullName, 0);
            if (ds == null)
                throw new Exception("Невозможно открыть файл");
            var layerCount = ds.GetLayerCount();
            if (layerCount != 1)
                throw new Exception("В файле должен быть ровно один слой");
            var layer = ds.GetLayerByIndex(0);

            string geom_name = layer.GetGeomType().ToString().ToUpper();
            if (geom_name.Length > 3)
                geom_name = geom_name.Substring(3);
            if (string.IsNullOrEmpty(geom_name) || layer.GetGeomType() == wkbGeometryType.wkbUnknown)
            {
                Feature feat = null;
                int count = layer.GetFeatureCount(1);
                for (int i = 0; i < count + 1; i++)
                {
                    try
                    {
                        feat = layer.GetFeature(i);
                    }
                    catch
                    {
                    }
                    if (feat != null)
                        break;
                }

                if (feat == null)
                {
                    return string.Empty;
                }
                Geometry geom = feat.GetGeometryRef();
                if (geom == null)
                {
                    return string.Empty;
                }
                geom_name = geom.GetGeometryName().ToUpper();
                feat.Dispose();
            }

            layer.Dispose();
            ds.Dispose();

            if (!geom_name.Contains("MULTI"))
                geom_name = "MULTI" + geom_name;

            return geom_name;
        }
        
        public int getSRID()
        {
            DataSource ds = Ogr.Open(inputFile.FullName, 0);
            if (ds == null)
                throw new Exception("Невозможно открыть файл");
            var layerCount = ds.GetLayerCount();
            if (layerCount != 1)
                throw new Exception("В файле должен быть ровно один слой");
            var layer = ds.GetLayerByIndex(0);
            
            SpatialReference sp = layer.GetSpatialRef();
            string srid = sp.GetAttrValue("AUTHORITY", 1);

            layer.Dispose();
            ds.Dispose();

            return String.IsNullOrEmpty(srid) ? -1 : Int32.Parse(srid);
        }

        private string getWKT(int featureID)
        {
            DataSource ds = Ogr.Open(inputFile.FullName, 0);
            if (ds == null)
                throw new Exception("Невозможно открыть файл");
            var layerCount = ds.GetLayerCount();
            if (layerCount != 1)
                throw new Exception("В файле должен быть ровно один слой");
            var layer = ds.GetLayerByIndex(0);

            var feat = layer.GetFeature(featureID);
            if (feat == null) return string.Empty;

            Geometry geom = feat.GetGeometryRef();
            if (geom == null) return string.Empty;

            string geom_wkt;
            geom.ExportToWkt(out geom_wkt);

            feat.Dispose();

            return geom_wkt;
        }

        public string GetWKTofSingleObject(out string errorMessage)
        {
            string wkt = "";
            errorMessage = string.Empty;
            try
            {
                DataSource ds = Ogr.Open(inputFile.FullName, 0);
                if (ds == null)
                    throw new Exception("Невозможно открыть файл");
                var layerCount = ds.GetLayerCount();
                if (layerCount != 1)
                    throw new Exception("В файле должен быть ровно один слой");
                var layer = ds.GetLayerByIndex(0);

                var featDef = layer.GetLayerDefn();
                Feature feat = layer.GetNextFeature();
                if (feat == null)
                    throw new Exception("В файле не задан объект");
                wkt = getWKT(feat.GetFID());

                feat.Dispose();
                layer.Dispose();
                ds.Dispose();
            }
            catch (Exception ex) { errorMessage = ex.Message; }
            return wkt;
        }
    }
}