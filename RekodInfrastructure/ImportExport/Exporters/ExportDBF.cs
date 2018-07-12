using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.OleDb;
using System.Windows;
using Rekod.Services;

namespace Rekod.ImportExport.Exporters
{
    class ExportDBF
    {
        private FileInfo _fi;
        private tablesInfo _ti;
        private string progressKey;

        public ExportDBF(tablesInfo ti, FileInfo fi)
        {
            _fi = fi;
            _ti = ti;
            progressKey = "SHPExporterProgress" + DateTime.Now.Ticks.ToString();
        }

        public void Export(string where, List<Interfaces.IParams> listParams)
        {
            string connectionString = "Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + _fi.Directory + ";Extended Properties=dBASE IV;";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            using (OleDbCommand command = connection.CreateCommand())
            {
                connection.Open();

                string errors = "";
                try
                {
                    cti.ThreadProgress.ShowWait(progressKey);

                    // создать таблицу
                    string create_cmd = "CREATE TABLE " + _fi.Name + " (";
                    string select = "";
                    List<fieldInfo> fieldList = classesOfMetods.getFieldInfoTable(_ti.idTable);
                    int al_count = 0;
                    foreach (var field in fieldList)
                    {
                        string field_name = field.nameDB;
                        if (field_name.Length > 10)
                            field_name = field_name.Remove(8) + (al_count++);
                        switch (field.type)
                        {
                            case 1:
                                create_cmd += field_name + " Integer, ";
                                break;
                            case 2:
                                create_cmd += field_name + " Character, ";
                                break;
                            case 3:
                                create_cmd += field_name + " Character, ";
                                break;
                            case 4:
                                create_cmd += field_name + " Character, ";
                                break;
                            case 5:
                                break; // геометрия
                            case 6:
                                create_cmd += field_name + " Double, ";
                                break;
                        }
                        select += field.nameDB + ", ";
                    }
                    create_cmd = create_cmd.Remove(create_cmd.Length - 2, 2);
                    select = select.Remove(select.Length - 2, 2);
                    command.CommandText = create_cmd.ToString() + ");";
                    command.ExecuteNonQuery();
                    
                    // заполнить данными
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        int rCount = 0;

                        var rowCount = 0;
                        using (var sw = new SqlWork())
                        {
                            sw.sql = string.Format("select count(*) from {0}.{1} where {2}", _ti.nameSheme, _ti.nameDB, where);
                            sw.ExecuteReader(listParams);
                            rowCount = sw.CanRead() ? sw.GetInt32(0) : 0;
                            sw.Close();
                        }
                        sqlCmd.sql = "SELECT " + select + " FROM " + _ti.nameSheme + "." + _ti.nameDB + " WHERE " + where;
                        sqlCmd.ExecuteReader(listParams);

                        while (sqlCmd.CanRead())
                        {
                            string insertValues = "";
                            int count = sqlCmd.GetFiealdCount();
                            for (int i = 0; i < count; i++)
                            {
                                switch (fieldList[i].type)
                                {
                                    case 1:
                                        insertValues += sqlCmd.GetInt32(i) + ", ";
                                        break;
                                    case 2:
                                        string value = sqlCmd.GetString(i);
                                        if (value != null && value.Contains("'"))
                                            value = value.Replace("'", "\"");
                                        insertValues += "'" + value + "', ";
                                        break;
                                    case 3:
                                        insertValues += "'" + sqlCmd.GetString(i) + "', ";
                                        break;
                                    case 4:
                                        insertValues += "'" + sqlCmd.GetString(i) + "', ";
                                        break;
                                    case 5:
                                        continue;
                                    case 6:
                                        insertValues += sqlCmd.GetValue<double>(i) + ", ";
                                        break;
                                }
                            }
                            insertValues = insertValues.Remove(insertValues.Length - 2, 2);
                            command.CommandText = "INSERT INTO " + _fi.Name + " VALUES (" + insertValues + ");";
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                errors += e.Message + "\r\n";
                                continue;
                            }

                            cti.ThreadProgress.SetText(string.Format(Rekod.Properties.Resources.SHPExporter_ProcessObjCount, ++rCount, rowCount));                            
                        }
                    }
                }
                catch (Exception e)
                {
                    cti.ThreadProgress.Close(progressKey);
                    MessageBox.Show(e.Message);
                }
                finally
                {
                    cti.ThreadProgress.Close(progressKey);
                    if (errors != "")
                        MessageBox.Show(Rekod.Properties.Resources.ExportDBF_Errors + ":\r\n");
                    if (connection != null)
                        connection.Close();
                }
            }
        }
    }
}
