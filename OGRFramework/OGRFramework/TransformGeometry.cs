using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Windows;

namespace OGRFramework
{
    public class TransformGeometry
    {
        /// <summary>
        /// Преобразовывает геометрию из одной проекции в другую
        /// </summary>
        public static Point[] transform(Point[] points, string geomType, string projTo, string projFrom)
        {
            wkbGeometryType gt = wkbGeometryType.wkbNone;
            switch (geomType.ToUpper())
            {
                case "POINT":
                    gt = wkbGeometryType.wkbPoint;
                    break;
                case "LINESTRING":
                    gt = wkbGeometryType.wkbLineString;
                    break;
                case "POLYGON":
                    gt = wkbGeometryType.wkbPolygon;
                    break;
                case "MULTIPOINT":
                    gt = wkbGeometryType.wkbMultiPoint;
                    break;
                case "MULTILINESTRING":
                    gt = wkbGeometryType.wkbMultiLineString;
                    break;
                case "MULTIPOLYGON":
                    gt = wkbGeometryType.wkbMultiPolygon;
                    break;
                default:
                    gt = wkbGeometryType.wkbNone;
                    break;
            }

            string wkt = GetWktFromGeometry(points, gt);

            if (String.IsNullOrEmpty(wkt) || String.IsNullOrEmpty(wkt))
                return null;

            Geometry geom = transform(wkt, projFrom, projTo);
            return GetPointsFromGeometry(geom);
        }

        private static Geometry transform(string wkt, string projFrom, string projTo)
        {
            SpatialReference srTo = new SpatialReference(projTo),
                srFrom = new SpatialReference(projFrom);

            Geometry geom = Geometry.CreateFromWkt(wkt);
            geom.AssignSpatialReference(srFrom);

            if (geom != null)
            {
                int res = geom.TransformTo(srTo);
            }

            return geom;
        }

        /// <summary>
        /// Извлекает массив точек из геометрии
        /// </summary>
        private static Point[] GetPointsFromGeometry(Geometry geom)
        {
            Point[] points;
            if (geom.GetGeometryType() == wkbGeometryType.wkbPoint)
            {
                points = new Point[geom.GetPointCount()];
                for (int p = 0; p < geom.GetPointCount(); p++)
                {
                    double[] point = new double[2];
                    geom.GetPoint_2D(p, point);
                    points[p].X = point[0]; points[p].Y = point[1];
                }
                return points;
            }
            else return null;
        }

        /// <summary>
        /// Получает WKT из заданной геометрии
        /// </summary>
        /// <param name="points">Массив координат точек</param>
        /// <param name="type">Тип геометрии</param>
        /// <returns>WKT</returns>
        public static string GetWktFromGeometry(Point[] points, wkbGeometryType type)
        {
            Geometry geom = new Geometry(type);

            for (int i = 0; i < points.Count(); i++)
            {
                geom.AddPoint_2D(points[i].X, points[i].Y);
            }
            string wkt = String.Empty;
            geom.ExportToWkt(out wkt);

            return wkt;
        }
    }
}
