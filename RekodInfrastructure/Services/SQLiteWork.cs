using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using Rekod.Repository.SettingsDB;

namespace Rekod.Services
{
    class SQLiteWork : IDisposable
    {
        #region Статические методы
        public static SQLiteConnection getCon()
        {
            SQLiteConnection liteCon = new SQLiteConnection(Program.connStringSQLite);
            liteCon.Open();
            return liteCon;
        }
        public static SQLiteCommand getCmd(string sqlCommandString)
        {
            SQLiteCommand sqliteCommand = new SQLiteCommand(sqlCommandString, getCon());
            return sqliteCommand;
        }
        public static void freeCmd(SQLiteCommand sqlCmd)
        {
            sqlCmd.Connection.Close();
        }
        public static SettingsContext GetSqLiteContext(string sqlCommandString)
        {
            return new SettingsContext(String.Format("metadata=res://*/Repository.SettingsDB.SettingsDBModel.csdl|res://*/Repository.SettingsDB.SettingsDBModel.ssdl|res://*/Repository.SettingsDB.SettingsDBModel.msl;provider=System.Data.SQLite;provider connection string='{0}';", sqlCommandString));
        }
        #endregion Статические методы

        #region Поля
        private readonly SQLiteCommand _queryCommand;
        private readonly SQLiteConnection _sqliteConnection;
        private Boolean _invokeException;
        private SQLiteDataReader _sqliteReader;
        private bool _spatialite;
        private bool _isConection;
        private bool _transaction;
        #endregion Поля

        #region Конструкторы
        public SQLiteWork(Boolean invokeexception = true)
            : this(Program.connStringSQLite, invokeexception)
        { }
        public SQLiteWork(DbConnectionStringBuilder connbuilder, bool invokeexception = true)
            : this(string.Format("{0}", connbuilder), invokeexception)
        { }
        public SQLiteWork(String connstring, bool invokeexception = true)
        {
            if (string.IsNullOrEmpty(connstring))
                new ArgumentNullException("connstring").Throw(invokeexception);
            _sqliteConnection = new SQLiteConnection(connstring);
            _queryCommand = new SQLiteCommand() { Connection = _sqliteConnection };
            _invokeException = invokeexception;
        }
        #endregion Конструкторы

        #region Свойства
        public Boolean InvokeException
        {
            get { return _invokeException; }
        }
        public String Sql
        {
            get { return _queryCommand.CommandText; }
            set { _queryCommand.CommandText = value; }
        }
        public SQLiteConnection SqliteConnection
        {
            get { return _sqliteConnection; }
        }
        #endregion  Свойства

        #region Методы
        public void Dispose()
        {
            CloseReader();
            _sqliteConnection.Close();
            _queryCommand.Dispose();
            _sqliteConnection.Dispose();
        }
        public void InstallSpatialite()
        {
            if (!_spatialite)
            {
                var str = Sql;
                Sql = @"SELECT load_extension('libspatialite-1.dll');";
                ExecuteNonQuery();
                Sql = str;
                _spatialite = true;
            }
        }

        /// <summary>
        /// Проверка получения новой строки запроса
        /// </summary>
        /// <returns>Есть ли новая сторока запроса?</returns>
        public bool CanRead()
        {
            if (_sqliteReader != null && _sqliteReader.Read())
                return true;
            return false;
        }
        /// <summary>
        /// Проверяет получение нового результата от следующего запроса
        /// </summary>
        /// <returns>Есть ли новый результат запроса?</returns>
        public bool CanNextResult()
        {
            if (_sqliteReader == null)
                return false;
            return _sqliteReader.NextResult();
        }
        /// <summary>
        /// Получает значение из запроса
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="index">Номер колонки</param>
        /// <returns>Значение</returns>
        public T GetValue<T>(int index)
        {
            if (_sqliteReader == null) return default(T);
            if (index < 0 || index >= _sqliteReader.FieldCount)
            {
                new Exception("Ошибка при чтении _dataReader").Message().Throw(_invokeException);
                return default(T);
            }

            try
            {
                object value = _sqliteReader[index];
                var newValue = ExtraFunctions.Converts.To<T>(value);
                return newValue;
            }
            catch (Exception ex)
            {
                ex.Message().Throw(_invokeException);
            }
            return default(T);

        }
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> Возвращает объект в указанном типе</returns>
        public T GetValue<T>(string name)
        {
            if (_sqliteReader == null) return default(T);
            return GetValue<T>(_sqliteReader.GetOrdinal(name));
        }
        //Возвращает byte[]
        public byte[] GetBytes(string columnName, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
        {
            return GetBytes(_sqliteReader.GetOrdinal(columnName), dataOffset, bufferOffset, length);
        }
        public byte[] GetBytes(int position, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue)
        {
            Byte[] buffer = new Byte[_sqliteReader.GetBytes(position, dataOffset, null, bufferOffset, length)];
            _sqliteReader.GetBytes(position, dataOffset, buffer, bufferOffset, length);
            return buffer;
        }
        /// <summary>
        /// Выполнить запрос без возврата результатов
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public bool ExecuteNonQuery(params SqlParam[] listParams)
        {
            return ExecuteNonQuery((IEnumerable<SqlParam>)listParams);
        }
        /// <summary>
        /// Выполнить запрос без возврата результатов
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public bool ExecuteNonQuery(IEnumerable<SqlParam> listParams = null)
        {
            try
            {
                CreateConn(_invokeException);
                FillParams(listParams);
                _queryCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ex.Message().Throw(_invokeException);
            }
            return false;
        }
        /// <summary>
        /// Выполнить запрос с возвратом первого значения из запроса указанного типа
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public T ExecuteScalar<T>(params SqlParam[] listParams)
        {
            return ExecuteScalar<T>((IEnumerable<SqlParam>)listParams);
        }
        /// <summary>
        /// Выполнить запрос с возвратом первого значения из запроса указанного типа
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public T ExecuteScalar<T>(IEnumerable<SqlParam> listParams)
        {
            try
            {
                CreateConn(_invokeException);
                FillParams(listParams);
                return ExtraFunctions.Converts.To<T>(_queryCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                ex.Message().Throw(_invokeException);
                return default(T);
            }
        }
        /// <summary>
        /// Выполнить запрос с возвратом многострочных результатов
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public void Execute(params SqlParam[] listParams)
        {
            Execute((IEnumerable<SqlParam>)listParams);
        }
        /// <summary>
        /// Выполнить запрос с возвратом многострочных результатов
        /// </summary>
        /// <param name="listParams">Параметры запроса</param>
        /// <returns>Выполнен запрос?</returns>
        public void Execute(IEnumerable<SqlParam> listParams = null)
        {
            try
            {
                CreateConn(_invokeException);
                FillParams(listParams);
                _sqliteReader = _queryCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                ex.Message().Throw(_invokeException);
            }
        }
        /// <summary>
        /// Закрывает ридер
        /// </summary>
        public void CloseReader()
        {
            if (_sqliteReader != null)
            {
                _sqliteReader.Dispose();
                _sqliteReader = null;
            }
        }

        /// <summary>
        /// Возвращает название атрибута в БД по его позиции
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public String GetFieldName(int position)
        {
            if (_sqliteReader != null)
                return _sqliteReader.GetName(position);
            else
                return null;
        }
        /// <summary>
        /// Возвращает тип данных атрибута в БД по его позиции
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public System.Type GetFieldType(int position)
        {
            if (_sqliteReader != null)
                return _sqliteReader.GetFieldType(position);
            else
                return null;
        }
        public System.Data.DbType GetFieldDbType(int position)
        {
            return TypeToDbType(GetFieldType(position));
        }
        public int GetFieldsCount()
        {
            if (_sqliteReader != null)
                return _sqliteReader.FieldCount;
            else
                return 0;
        }

        public void TransactionBegin()
        {
            if (!_transaction)
            {
                var str = Sql;
                Sql = @"BEGIN;";
                ExecuteNonQuery();
                Sql = str;
                _transaction = true;
            }
        }
        public void TransactionEnd()
        {
            if (_transaction)
            {
                CloseReader();
                var str = Sql;
                Sql = @"END;";
                ExecuteNonQuery();
                Sql = str;
                _transaction = false;
            }
        }


        private void CreateConn(bool enableException = false)
        {
            if (_isConection)
            {
                return;
            }
            _sqliteConnection.Open();
            _isConection = true;
        }
        /// <summary>
        /// Заполняет параметры в запросе
        /// </summary>
        private void FillParams(IEnumerable<SqlParam> listParams)
        {
            _queryCommand.Parameters.Clear();
            if (listParams != null)
                foreach (var param in listParams)
                {
                    SQLiteParameter newParam = new SQLiteParameter(param.Name, param.DbType);
                    newParam.Value = param.Value;
                    _queryCommand.Parameters.Add(newParam);
                }
        }
        private static DbType TypeToDbType(Type t)
        {
            if (t == null)
                return DbType.Object;
            DbType dbt;
            try
            {
                dbt = (DbType)Enum.Parse(typeof(DbType), t.Name);
            }
            catch
            {
                dbt = DbType.Object;
            }
            return dbt;
        }
        #endregion Методы
    }

    public static class ExceptionExtension
    {
        #region Методы
        /// <summary>
        /// Метод регистрации исключения
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <param name="invoke">Выбрасывать или нет</param>
        public static Exception Throw(this Exception ex, bool invoke = false)
        {
            var stack = new StackTrace(ex);
            Debug.WriteLine(stack.ToString(), "Exception");
            if (invoke)
                throw ex;
            return ex;
        }
        public static Exception Message(this Exception ex, bool invoke = false)
        {
            var stack = new StackTrace(ex);
            Debug.WriteLine(stack.ToString(), "Exception");
            if (invoke)
                throw ex;
            return ex;
        }
        #endregion Методы
    }

    public interface IResult
    {
        #region Методы
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns> Логическое значение</returns>
        bool GetBoolean(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> Логическое значение</returns>
        bool GetBoolean(string name);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns>32 разрядное целое число</returns>
        int GetInt32(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> 64 разрядное целое число</returns>
        int GetInt32(string name);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns> 64 разрядное целое число</returns>
        long GetInt64(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> 32 разрядное целое число</returns>
        long GetInt64(string name);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns>Строка</returns>
        string GetString(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns>Строка</returns>
        string GetString(string name);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns> Возвращает объект</returns>
        object GetValue(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> Возвращает объект</returns>
        object GetValue(string name);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="index">Номер колонки</param>
        /// <returns> Возвращает объект в указанном типе</returns>
        T GetValue<T>(int index);
        /// <summary>
        /// Считать поле
        /// </summary>
        /// <param name="name">Название колонки</param>
        /// <returns> Возвращает объект в указанном типе</returns>
        T GetValue<T>(string name);
        byte[] GetBytes(int index, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue);
        byte[] GetBytes(string name, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue);
        #endregion Методы
    }

    public class SqlParam
    {
        #region Поля
        private object _value;
        private string _name;
        private System.Data.DbType _dbType;
        #endregion // Поля

        #region Конструктор
        public SqlParam(string name, DbType dbType, object value)
        {
            _name = name;
            _dbType = dbType;
            _value = value;
        }
        #endregion // Конструктор

        #region Свойства
        public string Name
        { get { return _name; } }

        public DbType DbType
        { get { return _dbType; } }

        public object Value
        { get { return _value; } }
        #endregion // Свойства
    }
}