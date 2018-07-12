using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using Npgsql;

namespace Rekod.ShpLoader
{
    //TODO: (Сергей)Всем: Класс который никогда не вызывается
    class LoadNewObjInPostgres
    {
        string fileName;
        int idT;
        int id_session;
        tablesInfo ti;

        public LoadNewObjInPostgres(string fileName, int idT, int id_session)
        {
            this.fileName = fileName;
            this.idT = idT;
            this.id_session = id_session;
            this.ti = classesOfMetods.getTableInfo(idT);
            LoadObjs();
        }
        private void LoadObjs()
        {
            var c = (NpgsqlConnectionStringBuilder)Program.connString;
            String args = String.Format(" -W cp1251 -N skip -d -s \"{0}\" -g {3} \"{1}\" {2} ", ti.srid.ToString(),
                fileName, ti.nameSheme + "." + ti.nameDB+"_"+id_session.ToString(), "geom");

            #region Process Initialisation
            System.Environment.SetEnvironmentVariable("PGPASSWORD", c["Password"].ToString());
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
            System.IO.StreamWriter file = new System.IO.StreamWriter("" + Program.path_string + "\\test.txt", false);
            while (!_process.StandardOutput.EndOfStream)
            {
                string sql_str = _process.StandardOutput.ReadLine();
                if (sql_str.IndexOf("DROP TABLE") == -1 && sql_str.IndexOf("DropGeometryColumn") == -1)
                {
                    if (sql_str.IndexOf("varchar") != -1)
                    {
                        if (sql_str.IndexOf("(") != -1)
                        {
                            string temp = sql_str.Substring(sql_str.IndexOf("("), (sql_str.IndexOf(")") - sql_str.IndexOf("(")+1));
                            sql_str = sql_str.Replace(temp, "");
                        }
                        file.WriteLine(sql_str);
                    }
                    else
                    {
                        file.WriteLine(sql_str);
                    }
                }
            }

            file.WriteLine("INSERT INTO \"" + ti.nameSheme + "\".\"" + ti.nameDB + "\" (" + ti.geomFieldName + ") SELECT geom FROM \"" +
                                ti.nameSheme + "\".\"" + ti.nameDB + "_" + id_session.ToString() + "\";");
            file.WriteLine("DROP TABLE \"" +ti.nameSheme + "\".\"" + ti.nameDB + "_" + id_session.ToString() + "\";");
            file.WriteLine("DELETE FROM geometry_columns WHERE f_table_schema like '" + ti.nameSheme + "' AND f_table_name LIKE '" + ti.nameDB + "_" + id_session.ToString() + "';");
            file.Close();

            args = String.Format(" -q -h {0} -p {1} -U {2} -d {3} -f \""+Program.path_string +"\\test.txt\"",
                c.Host, c.Port, c.UserName, c.Database);
            string proc = "";
            try
            {
                proc = cti.ThreadProgress.Open("loadShp");
                #region Process Initialisation
                _process = new Process();
                _pathToProgram = "\"" + Application.StartupPath + "\\pgsql&shp\\psql.exe" + "\"";
                _process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(1251);
                _process.StartInfo.FileName = _pathToProgram;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.ErrorDialog = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardError = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.Arguments = args;
                _process.Start();
                _process.WaitForExit();
            }
            catch
            {
            }
            finally
            {
                cti.ThreadProgress.Close(proc);
            }
           //MessageBox.Show( _process.StandardOutput.ReadToEnd());

           //MessageBox.Show(_process.StandardError.ReadToEnd());

            #endregion
        }
    }
}
