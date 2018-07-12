using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Rekod.Services;

namespace Rekod.ShpLoader
{
    class LoadObjInPostgres
    {
        string fileName;
        int idT;
        tablesInfo ti;
        public string wkt;
        public LoadObjInPostgres(string fileName, int idT)
        {
            this.fileName = fileName;
            this.idT = idT;
            this.ti = classesOfMetods.getTableInfo(idT);
            loadSqlStr();
        }
        private void loadSqlStr()
        {
            String args = String.Format(" -D -W cp1251 -N skip -a -s \"{0}\" -g {3} \"{1}\" {2}", ti.srid.ToString(),
                fileName, ti.nameSheme + "." + ti.nameDB, ti.geomFieldName);
            

            #region Process Initialisation
            Process _process = new Process();
            string _pathToProgram = Application.StartupPath + "\\pgsql&shp\\shp2pgsql.exe";
            _process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(1251);
            _process.StartInfo.FileName = _pathToProgram;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.ErrorDialog = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.Arguments = args;
            _process.Start();
            #endregion

            String mess = "";
            mess = _process.StandardOutput.ReadToEnd();
            GetGeomString(mess);
        }
        private void GetGeomString(string sql_string)
        {
            List<int> tabPos = new List<int>();
            if (sql_string.LastIndexOf("\t") > -1)
            {
                int lastTabPos = sql_string.LastIndexOf("\t");
                sql_string = sql_string.Substring(lastTabPos, sql_string.Length - lastTabPos - 13);
                GetGwomWkt(sql_string);
            }
        }
        private void GetGwomWkt(string wkb)
        {
            SqlWork cmd = new SqlWork();
            cmd.sql = "SELECT st_astext('" + wkb + "')";
            cmd.Execute(false);
            if (cmd.CanRead())
            {
                wkt = cmd.GetValue<string>(0);
            }
            cmd.Close();
        }
    }
}