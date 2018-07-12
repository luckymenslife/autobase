using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rekod.Services;

namespace Rekod.Classes
{
    public class TransformGeometry
    {
        /// <summary>
        /// Получает из базы wkt проекции
        /// </summary>
        public static string GetProjWKT(int srid)
        {
            string wkt = String.Empty;
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT srtext FROM spatial_ref_sys WHERE srid=" + srid;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    wkt = sqlCmd.GetString(0);
                }
                else return null;
            }
            return wkt;
        }

        /// <summary>
        /// Получает из базы проекции в формате proj4
        /// </summary>
        public static string GetProj4(int srid)
        {
            string wkt = String.Empty;
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT proj4text FROM spatial_ref_sys WHERE srid=" + srid;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    wkt = sqlCmd.GetString(0);
                }
                else return null;
            }
            return wkt;
        }

    }
}
