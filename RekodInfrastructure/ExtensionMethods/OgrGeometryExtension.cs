using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using System.Diagnostics;

namespace OSGeo.OGR
{
    public static class GeometryExtension
    {
        public static Geometry CreateFromWkb(byte[] wkb, int srid)
        {
            var geom = Geometry.CreateFromWkb(wkb);
            var spatialRef = Rekod.Services.OgrService.Method.GetSpatialReference(srid);
            geom.AssignSpatialReference(spatialRef);
            return geom;
        }
        public static Geometry CreateFromWkt(string wkt, int srid)
        {
            var geom = Geometry.CreateFromWkt(wkt);
            var spatialRef = Rekod.Services.OgrService.Method.GetSpatialReference(srid);
            geom.AssignSpatialReference(spatialRef);
            return geom;
        }
        public static void SetSrid(this Geometry geom, int srid)
        {
            var spatialRef = Rekod.Services.OgrService.Method.GetSpatialReference(srid);
            geom.AssignSpatialReference(spatialRef);
        }
        public static int GetSrid(this Geometry geom)
        {
            var spatialRef = geom.GetSpatialReference();
            var srid = Rekod.Services.OgrService.Method.GetSrid(spatialRef);
            return srid;
        }
        public static Geometry Transform(this Geometry geom, int srid)
        {
            var spatialRef = Rekod.Services.OgrService.Method.GetSpatialReference(srid);
            geom.TransformTo(spatialRef);
            return geom;
        }
        public static Geometry Union(this Geometry geom, List<Geometry> geoms)
        {
            foreach (var item in geoms)
            {
                geom = geom.Union(item);
            }
            return geom;
        }
        public static string GetWkt(this Geometry geom)
        {
            string wkt = string.Empty;
            geom.ExportToWkt(out wkt);
            return wkt;
        }
        public static Byte[] GetWkb(this Geometry geom)
        {
            var temp = new Byte[geom.WkbSize()];
            geom.ExportToWkb(temp);
            return temp;
        }
        public static bool NotStrictEqualityGeomType(this Geometry geom, Geometry other_geom)
        {
            string str_type1 = geom.GetGeometryType().ToString().ToUpper().Replace("WKB", "").Replace("MULTI", "");
            string str_type2 = other_geom.GetGeometryType().ToString().ToUpper().Replace("WKB", "").Replace("MULTI", "");
            if (str_type1.Equals(str_type2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
