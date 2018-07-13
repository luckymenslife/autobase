using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Interfaces;
using Npgsql;
using NpgsqlTypes;
using System.Windows.Forms;
using System.Data;
using Rekod.Classes;
using Rekod.Controllers;
using System.Reflection;
using System.IO;

namespace Rekod.Services
{
    class SqlWork : Interfaces.ISQLCommand
    {
        #region Статические члены
        /// <summary>
        /// Статический метод проверки соединения
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        /// <exception cref="NpgsqlException"/>
        public static bool CheckedConnection(NpgsqlConnectionStringBuilder stringBuilder)
        {
            using (SqlWork sqlCmd = new SqlWork(stringBuilder, true))
            {
                sqlCmd.CheckConnectionWithException();
            }
            return true;
        }
        /// <exception cref="NpgsqlException"/>
        /// <exception cref="Exception"/>
        public static int CheckedUser(NpgsqlConnectionStringBuilder stringBuilder)
        {
            using (SqlWork sqlCmd = new SqlWork(stringBuilder, true))
            {
                try
                {

                    sqlCmd.CheckConnectionWithException();
                }
                catch (NpgsqlException ex)
                {
                    switch (ex.Code)
                    {
                        case "28000":
                            throw new Exception(Rekod.Properties.Resources.login_errWPass + stringBuilder.UserName + "'");
                        case "28P01":
                            throw ex;
                            //throw new Exception(Rekod.Properties.Resources.login_errWPass + stringBuilder.UserName + "'");
                        case "3D000":
                            throw new Exception(Rekod.Properties.Resources.login_errNoDB + stringBuilder["DataBase"] + Rekod.Properties.Resources.login_errNoDB2);
                        default:
                            throw new Exception(Rekod.Properties.Resources.login_msgNoConDB);
                    }
                }

                sqlCmd.sql = "SELECT id FROM " + Program.scheme + ".user_db WHERE login = @file_blob";
                sqlCmd.AddParam("@file_blob", stringBuilder.UserName, DbType.String);

                var userId = sqlCmd.ExecuteScalar<int?>();
                if (userId == null)
                {
                    throw new Exception(Rekod.Properties.Resources.login_msgNoUser + ' ' + stringBuilder.UserName + ' ' + Rekod.Properties.Resources.login_msgNoUser2);
                }
                return userId.Value;
            }

        }
        public static void CorrectConnectBuilder(NpgsqlConnectionStringBuilder conn)
        {
            conn.Pooling = true;
            conn.MinPoolSize = 0;
            conn.MaxPoolSize = 100;
            conn.CommandTimeout = 300;
            conn.ConnectionLifeTime = 300;
            conn.SSL = true;
            conn.SslMode = SslMode.Prefer;
        }
        #endregion Статические члены

        #region Поля
        private String _sqlCommandText;
        private readonly NpgsqlCommand _queryCommand;
        private NpgsqlDataReader _dataReader;
        private bool _isTransaction = false;
        private bool _isMultipleResults = false;
        private NpgsqlConnection _pgCon;
        private bool _enableException;
        private bool _isConection = false;
        private Exception _lastError;
        private List<IParams> _params;
        #endregion Поля

        #region Конструкторы
        public SqlWork(bool enableException = false)
        {
            _params = new List<IParams>();
            _queryCommand = new NpgsqlCommand();
            _pgCon = new NpgsqlConnection(Program.connString.ToString());
            this._enableException = enableException;
            _queryCommand.Connection = _pgCon;
            //CreateConn(enableException);
            //_queryCommand.Connection = _pgCon; 
            //reconnectMap();
            ParamsLoaded += SqlWork_ParamsLoaded;
        }
        public SqlWork(NpgsqlConnectionStringBuilder connect, bool enableException = false)
        {
            _params = new List<IParams>();
            _queryCommand = new NpgsqlCommand();
            string strConnect = (connect != null)
                                ? connect.ToString()
                                : Program.connString.ToString();
            _pgCon = new NpgsqlConnection(strConnect);
            this._enableException = enableException;
            _queryCommand.Connection = _pgCon;
            ParamsLoaded += SqlWork_ParamsLoaded;
        }

        #endregion Конструкторы

        #region Методы
        private void ExceptionFunc(Exception ex, bool enableException)
        {
            _lastError = ex;
            workLogFile.writeLogFile(ex, !enableException, true);
            if (enableException)
                throw ex;
        }

        #region Новые методы
        // Возвращает true если экзекуция прошла успешно
        public bool ExecuteNonQuery(IEnumerable<IParams> listParams = null)
        {
            try
            {
                CreateConn();
                LoadParams(listParams);
                _queryCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
            }
            return false;
        }
        public bool ExecuteReader(IEnumerable<IParams> listParams = null)
        {
            try
            {
                CreateConn();
                LoadParams(listParams);
                _dataReader = _queryCommand.ExecuteReader();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
            }
            return false;
        }
        public bool ExecuteMultipleReader(IEnumerable<IParams> listParams = null)
        {
            try
            {
                CreateConn();
                LoadParams(listParams);
                _dataReader = _queryCommand.ExecuteReader();
                _isMultipleResults = true;
                return true;
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
            }
            return false;
        }
        public object ExecuteScalar(IEnumerable<IParams> listParams = null)
        {
            return ExecuteScalar<object>(listParams);
        }
        public Int64 ExecuteUpdateReturningGid(IEnumerable<IParams> listParams = null)
        {
            try
            {
                CreateConn();
                LoadParams(listParams);
                var value = _queryCommand.ExecuteScalar();
                return ExtraFunctions.Converts.To<Int64>(value);
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
                return 0;
            }
            finally
            {
                Close();
            }
        }
        public T ExecuteScalar<T>(IEnumerable<IParams> paramArrya = null)
        {
            try
            {
                CreateConn();
                LoadParams(paramArrya);
                var value = _queryCommand.ExecuteScalar();
                return ExtraFunctions.Converts.To<T>(value);
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
                return default(T);
            }
            finally
            {
                Close();
            }
        }
        public DataTable ExecuteGetTable()
        {
            return ExecuteGetTable(paramArrya: null);
        }
        // Возвращает DataTable используя параметры
        public DataTable ExecuteGetTable(IEnumerable<IParams> paramArrya)
        {
            var table = new DataTable();
            LoadParams(paramArrya);
            try
            {
                var sqlAdap = new NpgsqlDataAdapter(_queryCommand);
                sqlAdap.Fill(table);
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
            }
            return table;
        }
        // Возвращает DataTable используя параметры
        public DataTable ExecuteGetTable(DataTable table, IEnumerable<IParams> listParams = null)
        {
            if (table == null)
                table = new DataTable();
            else
                table.Clear();
            try
            {
                CreateConn();
                LoadParams(listParams);
                var sqlAdap = new NpgsqlDataAdapter(_queryCommand);
                sqlAdap.Fill(table);
            }
            catch (Exception ex)
            {
                ExceptionFunc(ex, _enableException);
                return null;
            }
            return table;
        }
        public DataView ExecuteGetTable(DataView tableView, IEnumerable<IParams> listParams = null)
        {
            DataTable table = (tableView == null)
                                ? (DataTable)null
                                : tableView.Table;
            table = ExecuteGetTable(table, listParams);

            return (table == null)
                    ? (DataView)null
                    : table.DefaultView;
        }
        DataTable ISQLCommand.GetTable()
        {
            return ExecuteGetTable();
        }
        DataTable ISQLCommand.GetTable(Interfaces.IParams[] paramArray)
        {
            return ExecuteGetTable(paramArray);
        }
        /// <summary>
        /// Проверяет подключение к базе
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {
            try
            {
                CreateConn(this._enableException);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Проверяет подключение к базе и выдает Exception
        /// </summary>
        /// <returns></returns>
        public void CheckConnectionWithException()
        {
            CreateConn(this._enableException);
        }
        #endregion

        #region Команды считывания значений с DataReader

        public object GetValue(int position)
        {
            return GetValue<object>(position);
        }

        public Object GetValue(string columnName)
        {
            return GetValue<object>(columnName);
        }

        public T GetValue<T>(int position)
        {
            if (_dataReader == null) return default(T);
            if (position < 0 || position >= _dataReader.FieldCount)
            {
                Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.ErrorReadDataReader, false, true);
                return default(T);
            }

            try
            {
                object value = _dataReader[position];
                var newValue = ExtraFunctions.Converts.To<T>(value);
                return newValue;
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, false, true);
            }
            return default(T);

        }

        public T GetValue<T>(string ColumnName)
        {
            if (_dataReader == null) return default(T);
            if (!_dataReader.HasOrdinal(ColumnName))
            {
                Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.ErrorReadDataReader, false, true);
                return default(T);
            }

            return GetValue<T>(_dataReader.GetOrdinal(ColumnName));
        }

        // Возвращает значение атрибута конверитруя в string
        public string GetString(int position)
        {
            return GetValue<string>(position);
        }

        public string GetString(string columnName)
        {
            return GetValue<string>(columnName);
        }

        // Возвращает значение атрибута конверитруя в INT32
        public int GetInt32(int position)
        {
            return GetValue<int>(position);
        }
        public int GetInt32(string columnName)
        {
            return GetValue<int>(columnName);
        }

        // Возвращает значение атрибута конверитруя в INT32
        public Int64 GetInt64(int position)
        {
            return GetValue<Int64>(position);
        }

        public Int64 GetInt64(string columnName)
        {
            return GetValue<Int64>(columnName);
        }

        // Возвращает значение атрибута конверитруя в bool
        public bool GetBoolean(int position)
        {
            return GetValue<bool>(position);
        }

        public bool GetBoolean(string columnName)
        {
            return GetValue<bool>(columnName);
        }

        //Возвращает byte[]
        public byte[] GetBytes(string columnName, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
        {
            return GetBytes(_dataReader.GetOrdinal(columnName), dataOffset, bufferOffset, length);
        }

        public byte[] GetBytes(int position, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
        {
            if (!_dataReader.IsDBNull(position))
            {
                var lengthBuf = _dataReader.GetBytes(position, dataOffset, null, bufferOffset, length);
                Byte[] buffer = new Byte[lengthBuf];
                _dataReader.GetBytes(position, dataOffset, buffer, bufferOffset, length);
                return buffer;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Устаревшие методы

        /// <summary> Устаревший метод
        /// </summary>
        public bool Execute(bool IsNonQuery)
        {
            return IsNonQuery
                       ? ExecuteNonQuery()
                       : ExecuteReader();
        }

        /// <summary> Устаревший метод
        /// </summary>
        public bool Execute(bool IsNonQuery, Interfaces.IParams[] paramArrya)
        {
            return IsNonQuery
                ? ExecuteNonQuery(paramArrya)
                : ExecuteReader(paramArrya);
        }

        #endregion

        #region Управление SqlWork
        // Этот метод не обязательно использовать всегда
        // в методе CanRead() соединение и ридер закрываются автоматически
        public bool CanRead()
        {
            if (_dataReader != null && _dataReader.Read())
                return true;
            if (!_isMultipleResults)
                Close();
            return false;
        }
        public bool CanNextResult()
        {
            if (_dataReader != null && _dataReader.NextResult())
                return true;
            Close();

            return false;
        }

        //Закрытие соединения, командера и ридера
        public void Close()
        {
            CloseReader();

            if (!_isTransaction)
            {
                _queryCommand.Dispose();
                _pgCon.Close();
                //_pgCon.ClearPool(); Прикольно... можно сделать что бы ваще небыло коннектов
            }
        }
        internal void CloseReader()
        {
            if (_dataReader != null)
            {
                _isMultipleResults = false;
                _dataReader.Close();
                _dataReader.Dispose();
                _dataReader = null;
            }
        }

        public void BeginTransaction()
        {
            _isTransaction = true;
            sql = "Begin";
            ExecuteNonQuery();
        }
        public void EndTransaction()
        {
            _isTransaction = false;
            sql = "End;";
            ExecuteNonQuery();
        }
        // Возвращает количество колонок
        public int GetFiealdCount()
        {
            return _dataReader.FieldCount;
        }
        //Возвращает тип данных атрибута в БД по его позиции
        public System.Type GetFieldType(int position)
        {
            return _dataReader.GetFieldType(position);
        }
        public System.Data.DbType GetFieldDbType(int position)
        {
            return _dataReader.GetFieldDbType(position);
        }

        /// <summary>
        /// Возвращает название атрибута в БД по его позиции
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public String GetFieldName(int position)
        {
            return _dataReader.GetName(position);
        }

        // Возвращает значение атрибута

        private void CreateConn(bool enableException = false)
        {
            _lastError = null;
            if (_isConection)
            {
                return;
            }
            try
            {
                _pgCon.Open();
            }
            catch (NpgsqlException ex)
            {
                if ((ex.Code.StartsWith("08") || ex.Message.StartsWith("Failed to establish a connection to")) && !enableException)
                {
                    bool continue_ask = true;
                    while (continue_ask)
                    {
                        if (MessageBox.Show(Rekod.Properties.Resources.ConnectionIsBroken_Message,
                            Rekod.Properties.Resources.ConnectionIsBroken_Header, MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) == DialogResult.Retry)
                        {
                            CreateConn(enableException);
                            continue_ask = false;
                        }
                        else
                        {
                            Application.Exit();
                            Process.GetCurrentProcess().Kill();
                            continue_ask = false;
                        }
                    }
                }
                //workLogFile.writeLogFile(ex, !enableException, true);
                if (enableException)
                    throw ex;
            }
            catch (ThreadAbortException ex)
            { }
            catch (Exception ex)
            {
                workLogFile.writeLogFile(ex, !enableException, true);
                if (enableException)
                    throw ex;

            }
            _isConection = true;
            //return _pgCon;
        }
        #endregion Управление SqlWork

        #region Дополнительные функции
        public void AddParam(IParams param)
        {
            _params.Add(param);
        }
        public void AddParam(string name, object value, DbType type)
        {
            _params.Add(new Params(name, value, type));
        }
        public void AddParam(string name, object value, NpgsqlDbType type)
        {
            _params.Add(new Params(name, value, type));
        }
        private void LoadParams(IEnumerable<IParams> listParams)
        {
            _queryCommand.Parameters.Clear();
            if (listParams != null)
                _params.AddRange(listParams);
            foreach (var param in _params)
            {
                NpgsqlParameter newParam;
                if (param.typeData == 0)
                    newParam = new NpgsqlParameter(param.paramName, param.type);
                else
                    newParam = new NpgsqlParameter(param.paramName, param.typeData);
                newParam.Value = param.value;
                _queryCommand.Parameters.Add(newParam);
            }
            _params.Clear();
            if (ParamsLoaded != null)
            {
                ParamsLoaded(this, null);
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Close();
        }
        #endregion
        #endregion Методы

        #region Свойства
        public Exception LastError
        {
            get { return _lastError; }
        }
        public int VersionPostgres
        { get { return _pgCon.PostgreSqlVersion.Major; } }
        public String sql
        {
            get { return _sqlCommandText; }
            set
            {
//#if DEBUG
//                var sw = File.AppendText("D:\\load_sql.txt");
//                sw.WriteLine(value.Trim().EndsWith(";") ? value : value + ";");
//                sw.WriteLine("==============================================================================================================");
//                sw.Close();
//#endif

                _sqlCommandText = value;
//#if DEBUG
//                Classes.workLogFile.writeLogFile(_sqlCommandText, false, false);
//#endif
                _queryCommand.CommandText = _sqlCommandText;
            }
        }
        /// <summary>
        /// Возвращает sql запрос с подставленными параметрами
        /// </summary>
        public String FullSql
        {
            get
            {
                if (_queryCommand != null)
                {
                    String fullSql = sql;
                    foreach (NpgsqlParameter par in _queryCommand.Parameters)
                    {
                        fullSql = fullSql.Replace(par.ParameterName, par.NpgsqlValue.ToString());
                    }
                    return fullSql;
                }
                else
                {
                    return sql;
                }
            }
        }
        public String ServerVersion
        {
            get
            {
                if (_pgCon != null)
                {
                    CreateConn();
                    return _pgCon.ServerVersion;
                }
                else
                    return "";
            }
        }
        /// <summary>
        /// Соединение
        /// </summary>
        public NpgsqlConnection Connection
        {
            get { return _pgCon; }
        }
        #endregion Свойства

        #region Классы
        [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]  // multiuse attribute
        public class pgColumn : System.Attribute
        {
            #region Поля
            string _field;
            Rights _rights;
            PropertyInfo _propertyInfo;
            #endregion  // Поля

            #region Конструктор
            public pgColumn(string field, Rights rights = Rights.Default)
            {
                _field = field;
                _rights = rights;
            }
            #endregion // Конструктор

            #region Свойства

            public string Field
            {
                get { return _field; }
            }
            public Rights Rights
            {
                get { return _rights; }
                set { _rights = value; }
            }
            public PropertyInfo PropertyInfo
            {
                get { return _propertyInfo; }
                set { _propertyInfo = value; }
            }

            #endregion // Свойства
        }
        private class ResultSQLWork : System.Collections.IEnumerator
        {
            private readonly SqlWork _sqlWork;
            public ResultSQLWork(SqlWork sqlWork)
            {
                _sqlWork = sqlWork;
            }
            public RowSqlWork Current
            {
                get { throw new NotImplementedException(); }
            }
            public bool MoveNext()
            {
                return _sqlWork.CanRead();
            }
            public void Reset()
            {

            }
            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }
        }
        private class RowSqlWork
        {
            private readonly SqlWork _sqlWork;
            public RowSqlWork(SqlWork sqlWork)
            {
                _sqlWork = sqlWork;
            }
            public object GetValue(int position)
            {
                return GetValue<object>(position);
            }
            public Object GetValue(string columnName)
            {
                return GetValue<object>(columnName);
            }
            public T GetValue<T>(int position)
            {
                if (_sqlWork._dataReader == null) return default(T);
                if (position < 0 || position >= _sqlWork._dataReader.FieldCount)
                {
                    Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.ErrorReadDataReader, false, true);
                    return default(T);
                }
                return GetValue<T>(_sqlWork._dataReader.GetName(position));
            }
            public T GetValue<T>(string ColumnName)
            {
                if (_sqlWork._dataReader == null) return default(T);
                if (!_sqlWork._dataReader.HasOrdinal(ColumnName))
                {
                    Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.ErrorReadDataReader, false, true);
                    return default(T);
                }

                try
                {
                    object value = _sqlWork._dataReader[ColumnName];
                    var newValue = ExtraFunctions.Converts.To<T>(value);
                    return newValue;
                }
                catch (Exception ex)
                {
                    Classes.workLogFile.writeLogFile(ex, false, true);
                }
                return default(T);
            }
            // Возвращает значение атрибута конверитруя в string
            public string GetString(int position)
            {
                return GetValue<string>(position);
            }
            public string GetString(string columnName)
            {
                return GetValue<string>(columnName);
            }
            // Возвращает значение атрибута конверитруя в INT32
            public int GetInt32(int position)
            {
                return GetValue<int>(position);
            }
            public int GetInt32(string columnName)
            {
                return GetValue<int>(columnName);
            }
            // Возвращает значение атрибута конверитруя в INT32
            public Int64 GetInt64(int position)
            {
                return GetValue<Int64>(position);
            }
            public Int64 GetInt64(string columnName)
            {
                return GetValue<Int64>(columnName);
            }
            // Возвращает значение атрибута конверитруя в bool
            public bool GetBoolean(int position)
            {
                return GetValue<bool>(position);
            }
            public bool GetBoolean(string columnName)
            {
                return GetValue<bool>(columnName);
            }
            //Возвращает byte[]
            public byte[] GetBytes(string columnName, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
            {
                return GetBytes(_sqlWork._dataReader.GetOrdinal(columnName), dataOffset, bufferOffset, length);
            }
            public byte[] GetBytes(int position, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
            {
                var buffer = new Byte[_sqlWork._dataReader.GetBytes(position, dataOffset, null, bufferOffset, length)];
                _sqlWork._dataReader.GetBytes(position, dataOffset, buffer, bufferOffset, length);
                return buffer;
            }
        }
        #endregion Классы

        #region События
        public event EventHandler<EventArgs> ParamsLoaded;
        #endregion События

        #region Обработчики
        void SqlWork_ParamsLoaded(object sender, EventArgs e)
        {
            //var sw = File.AppendText("E:\\load_sql.txt");
            //String value = FullSql;
            //sw.WriteLine(value.Trim().EndsWith(";") ? value : value + ";");
            //sw.Close();
        }
        #endregion Обработчики
    }
    [Flags]
    public enum Rights
    {
        /// <summary>
        /// Права задаются относительно свойства get и set
        /// </summary>
        Default = 0,    //00
        Read = 1,       //01
        Write = 2,      //10
        ReadWrite = 3   //11
    }
}