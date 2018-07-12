using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using System.IO;
using Interfaces;
using System.Threading;
using System.Data;
using Rekod.Services;


namespace Rekod.DataAccess.SourcePostgres.ImportExport.Exporters
{
    public class TXTExporter
    {
        private PgM.PgTableBaseM _pgTable;
        private FileInfo _fileInfo;
        private NpgsqlConnectionStringBuilder _connect;

        public TXTExporter(PgM.PgTableBaseM table, NpgsqlConnectionStringBuilder connect, FileInfo fileInfo)
        {
            _connect = connect;
            _pgTable = table;
            _fileInfo = fileInfo;
        }

        public void Export()
        {
            try
            {
                Rekod.DBInfoEdit.SetSeparatorFrm frm = new DBInfoEdit.SetSeparatorFrm();
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (SqlWork sqlWork = new SqlWork(_connect))
                    {
                        cti.ThreadProgress.ShowWait();
                        System.IO.StreamWriter file = new System.IO.StreamWriter(_fileInfo.FullName, false);

                        List<String> fieldsList = new List<string>(); 
                        foreach (PgM.PgFieldM field in _pgTable.Fields)
                        {
                            if(field.Type != AbstractSource.Model.EFieldType.Geometry)
                            {
                                fieldsList.Add(field.Name); 
                            }
                        }
                        if (fieldsList.Count > 0)
                        {
                            int rowCount = 0; 
                            using (var sw = new SqlWork(_connect))
                            {
                                sw.sql = string.Format("select count(*) from {0}.{1}",
                                                            _pgTable.SchemeName,
                                                            _pgTable.Name);
                                sw.ExecuteReader();
                                rowCount = sw.CanRead() ? sw.GetInt32(0) : 0;
                                sw.Close();
                            }

                            sqlWork.sql =
                                String.Format("SELECT {0} FROM {1}.{2}", String.Join(",", fieldsList.ToArray()), _pgTable.SchemeName, _pgTable.Name);
                            sqlWork.ExecuteReader();

                            string line = "";
                            foreach (String fieldName in fieldsList)
                            {
                                line += fieldName + frm.Separator;
                            }
                            file.WriteLine(line);

                            int i = 0; 
                            while (sqlWork.CanRead())
                            {
                                line = "";
                                foreach (String fieldName in fieldsList)
                                {
                                    object val = sqlWork.GetValue(fieldName);
                                    line += (val==null?"":val) + frm.Separator;
                                }
                                file.WriteLine(line);                                   
                                cti.ThreadProgress.SetText(string.Format("Обработано объектов {0} из {1}", ++i, rowCount));
                            }
                        }
                        
                        file.Close();
                        cti.ThreadProgress.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close();
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
        }
    }
}