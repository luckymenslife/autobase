using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; 
using System.Data.OleDb;
using System.Data.Common;
using System.IO;
using System.Data;

namespace Rekod.DataAccess.SourcePostgres.ImportExport
{
    class ImportExportUsingOleDb: IDisposable
    {
        private String _connectionString;
        private OleDbConnection _oleDbConnection = null;
        private OleDbDataAdapter _oleDbDataAdapter = null;
        private OleDbCommand _oleDbCommand = null; 
        public String ConnectionString
        {
            get { return _connectionString; }
        }
        public String Sql
        {
            get { return _oleDbCommand.CommandText; }
            set { _oleDbCommand.CommandText = value; }
        }
        public void Dispose()
        {
            // Уничтожаем все Disposable объекты
            if (_oleDbConnection != null) _oleDbConnection.Dispose();
            if (_oleDbDataAdapter != null) _oleDbDataAdapter.Dispose();
            if (_oleDbCommand != null) _oleDbCommand.Dispose(); 
        }
        public ImportExportUsingOleDb(String connString)
        {
            _oleDbConnection = new OleDbConnection(connString);
            _oleDbCommand = new OleDbCommand();
            _oleDbCommand.Connection = _oleDbConnection;
            _connectionString = connString; 
        }
        public DataTable GetDataTable(String query, String fileName)
        {
            _oleDbConnection.Open();
            _oleDbCommand.CommandText = query;
            DataSet dataSet = new DataSet();
            try
            {
                _oleDbDataAdapter = new OleDbDataAdapter(_oleDbCommand);
                _oleDbDataAdapter.AcceptChangesDuringFill = false; 
                _oleDbDataAdapter.Fill(dataSet, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _oleDbConnection.Close(); 
                return null; 
            }
            _oleDbConnection.Close();
            return dataSet.Tables[0]; 
        }
    }
}