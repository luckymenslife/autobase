using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Windows.Controls;
using Rekod.Services;

namespace Rekod.ImportExport.Importers
{
    class DBFImporter : Importer
    {
        private FileInfo inputFile;
        private tablesInfo tableInfo;
        private int rowsCount;
        private readonly int rowsInSect = ImporterHelper.RowsToLoadInPortion;
        /// <summary>
        /// Имя файла без пути и расширения
        /// </summary>
        private string Name { get { return inputFile.Name.Substring(0, inputFile.Name.LastIndexOf(inputFile.Extension)); } }
        /// <summary>
        /// Deprecated. Use Connection
        /// </summary>
        private OleDbConnection connection;
        /// <summary>
        /// Соединение с файлом
        /// </summary>
        private OleDbConnection Connection
        {
            get
            {
                if (connection != null)
                    return connection;
                connection = new OleDbConnection(string.Format(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=dBASE IV;User ID=Admin;Password=;",
                inputFile.Directory));
                connection.Open();
                return connection;
            }
        }
        override public UserControl SettingsPanel { get { return null; } }
        override public int RowsCount { get { return rowsCount; } }

        override public void Init(FileInfo inputFile, tablesInfo tableInfo)
        {
            this.inputFile = inputFile;
            this.tableInfo = tableInfo;
            var reader = (new OleDbCommand(string.Format("SELECT count(*) FROM [{0}#dbf]", Name), Connection)).ExecuteReader();
            if (!reader.Read() || !int.TryParse(reader[0].ToString(), out rowsCount))
                rowsCount = 0;
            WorkerReportsProgress = true;
        }
        override public void SetTableInfo(tablesInfo tableInfo)
        {
            this.tableInfo = tableInfo;
        }
        override public DataTable GetPreviewTable(int previewRowsCount = 100)
        {
            DataTable result;
            using (ImportExport.ImportExportUsingOleDb impExp = new ImportExport.ImportExportUsingOleDb(Connection.ConnectionString))
            {
                String query = String.Format("SELECT * FROM [{0}#dbf] where 1=0", Name);
                result = impExp.GetDataTable(query, inputFile.Name);
            }
            var reader = (new OleDbCommand(string.Format("SELECT * FROM [{0}#dbf]", Name), Connection)).ExecuteReader();
            for (int i = 0; i < previewRowsCount && reader.Read(); i++)
            {
                List<object> colData = new List<object>();
                for (int j = 0; j < result.Columns.Count; j++)
                    colData.Add(reader[j]);
                result.Rows.Add(colData.ToArray());
            }
            reader.Close();
            return result;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                load((List<FieldMatch>)e.Argument);
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        override public void Load(List<FieldMatch> fields)
        {
            RunWorkerAsync(fields);
        }

        private void load(List<FieldMatch> fields)
        {
            if (fields.Count == 0)
                return;
            var parameters = new List<Interfaces.Params>();
            var insertSql = "insert into {0}.{1} (";
            var selectSql = "select ";
            string valuesStr = ") values (";
            DataTable datas = new DataTable();
            foreach (var f in fields)
            {
                insertSql += "\"" + f.Dest.nameDB + "\",";
                valuesStr += ":" + f.Dest.nameDB + ",";
                selectSql += f.Src + ",";
                var p = new Interfaces.Params() { paramName = f.Dest.nameDB };
                ImporterHelper.SetParamType(f.Dest, p);
                parameters.Add(p);
                datas.Columns.Add(f.Src);
            }
            selectSql = selectSql.TrimEnd(',');
            insertSql = insertSql.TrimEnd(',');
            valuesStr = valuesStr.TrimEnd(',');
            insertSql += valuesStr + ")";
            selectSql += string.Format(" FROM [{0}#dbf]", Name);
            //инициируем считывание
            var reader = (new OleDbCommand(selectSql, Connection)).ExecuteReader();
            var sw = new SqlWork(true);
            sw.BeginTransaction();
            sw.sql = string.Format(insertSql, tableInfo.nameSheme, tableInfo.nameDB);
            for (int i = 0, report = 0; reader.Read(); report++)
            {
                //добавляем в таблицу
                List<object> colData = new List<object>();
                for (int j = 0; j < datas.Columns.Count; j++)
                    colData.Add(reader[j]);
                datas.Rows.Add(colData.ToArray());
                i++;
                if (i >= rowsInSect)
                {
                    //кладем в базу
                    for (int j = 0; j < datas.Rows.Count; j++)
                    {
                        for (int k = 0; k < datas.Columns.Count; k++)
                            ImporterHelper.SetParamValue(parameters[k], datas.Rows[j][k]);
                        sw.ExecuteNonQuery(parameters.ToArray());
                    }
                    i = 0;
                    datas.Clear();
                }
                ReportProgress(0, report);
            }
            reader.Close();
            sw.EndTransaction();
        }
    }
}