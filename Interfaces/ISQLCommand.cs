using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using System.Windows.Forms;
using System.Data;
using mvMapLib;
using AxmvMapLib;

namespace Interfaces
{
    public class Params : Interfaces.IParams
    {
        public string paramName { get; set; }
        public object value { get; set; }
        public NpgsqlTypes.NpgsqlDbType typeData { get; set; }
        public DbType type { get; set; }

        public Params()
        { }
        public Params(string paramName, object value, DbType type)
        {
            this.paramName = paramName;
            this.value = value;
            this.type = type;
        }
        public Params(string paramName, object value, NpgsqlTypes.NpgsqlDbType typeData)
        {
            this.paramName = paramName;
            this.value = value;
            this.typeData = typeData;
        }
        public Params(string paramName, object value, Interfaces.TypeField type)
        {
            this.paramName = paramName;
            this.value = value;
            switch (type)
            {
                case TypeField.Integer:
                    this.typeData = NpgsqlDbType.Integer;
                    break;
                case TypeField.Text:
                    this.typeData = NpgsqlDbType.Text;
                    break;
                case TypeField.Date:
                case TypeField.DateTime:
                    this.typeData = NpgsqlDbType.Date;
                    break;
                case TypeField.Numeric:
                    this.typeData = NpgsqlDbType.Numeric;
                    break;
                default:
                    break;
            }
        }

        public Params Clone()
        {
            return new Params(paramName, value, typeData);
        }

        /// <summary>
        /// Аналогия: _paramName = ":" + paramName
        /// А также при Get удаляет ":"
        /// </summary>
        public string _paramName
        {
            get { return paramName.TrimStart(':'); }
            set { paramName = ":" + value; }
        }

#if DEBUG
        public override string ToString()
        {
            return _paramName + ": " + Convert.ToString(value);
        }
#endif
    }
    public interface IParams
    {
        string paramName { get; set; }
        object value { get; set; }
        NpgsqlTypes.NpgsqlDbType typeData { get; set; }
        DbType type { get; set; }
    }
    public interface ISQLCommand : IDisposable
    {
        void AddParam(IParams param);
        void AddParam(string name, object value, DbType type);
        /// <summary> Выполнение SQL запроса 
        /// (Устаревший метод, используйте <code>ExecuteNonQuery</code> или <code>ExecuteReader</code>)
        /// </summary>
        /// <param name="IsNonQuery">true - без возврата результата</param>
        /// <returns>статус выполнения запроса</returns>
        bool Execute(bool IsNonQuery);
        /// <summary> Выполнение SQL запроса c параметрами 
        /// (Устаревший метод, используйте <code>ExecuteNonQuery</code> или <code>ExecuteReader</code>)
        /// </summary>
        /// <param name="IsNonQuery">true - без возврата результата</param>
        /// <param name="paramArrya">Параметры запроса</param>
        /// <returns>Статус выполнения запроса</returns>
        bool Execute(bool IsNonQuery, IParams[] paramArrya);

        /// <summary>Выполнение SQL запроса с возвратом DataTable 
        /// (Устаревший метод, используйте <code>ExecuteGetTable</code>)
        /// </summary>
        /// <returns>Результат выполнения запроса</returns>
        DataTable GetTable();

        /// <summary>Выполнение SQL запроса с возвратом DataTable добавлены параметры
        /// (Устаревший метод, используйте <code>ExecuteGetTable</code>)
        /// </summary>
        /// <returns>Результат выполнения запроса</returns>
        DataTable GetTable(IParams[] paramArrya);

        /// <summary>Выполнение SQL запроса с возвратом DataTable
        /// </summary>
        /// <returns>Результат выполнения запроса</returns>
        DataTable ExecuteGetTable();

        /// <summary>Выполнение SQL запроса с возвратом DataTable добавлены параметры
        /// </summary>
        /// <returns>Результат выполнения запроса</returns>
        DataTable ExecuteGetTable(IEnumerable<IParams> paramArrya);

        /// <summary> Перемещает считыватель на следующую запись в наборе результатов
        /// </summary>
        /// <returns>Значение true, если имеется следующая строк; в противном случае — значение false</returns>
        bool CanRead();

        /// <summary> Перемещает считыватель к следующему результату при считывании результатов выполнения пакетных операторов.
        /// </summary>
        /// <returns>Значение true, если имеются и другие наборы результатов; в противном случае — значение false</returns>
        bool CanNextResult();

        /// <summary> Закрыть сессию (запрос)
        /// </summary>
        void Close();

        /// <summary> Получает значение заданного столбца в виде типа object
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <returns>Значение указанного столбца</returns>
        object GetValue(int position);
        /// <summary> Получает значение заданного столбца в виде типа object
        /// </summary>
        /// <param name="columnName">Название колонки</param>
        /// <returns>Значение указанного столбца</returns>
        object GetValue(string columnName);

        /// <summary> Получает значение заданного столбца в виде типа T
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значение</typeparam>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        T GetValue<T>(int position);
        /// <summary> Получает значение заданного столбца в виде типа T
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значение</typeparam>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        T GetValue<T>(string columnName);

        /// <summary> Получает значение заданного столбца в виде типа string
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        string GetString(int position);
        /// <summary> Получает значение заданного столбца в виде типа string
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        string GetString(string columnName);

        /// <summary> Получает значение заданного столбца в виде типа Int32
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        int GetInt32(int position);
        /// <summary> Получает значение заданного столбца в виде типа Int32
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        int GetInt32(string columnName);

        /// <summary> Получает значение заданного столбца в виде типа Int64
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        long GetInt64(int position);
        /// <summary> Получает значение заданного столбца в виде типа Int64
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        long GetInt64(string columnName);

        /// <summary> Получает значение заданного столбца в виде типа Boolean
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        bool GetBoolean(int position);
        /// <summary> Получает значение заданного столбца в виде типа Boolean
        /// </summary>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Значение указанного столбца</returns>
        bool GetBoolean(string columnName);
        /// <summary> Считывает поток байтов из указанного столбца, начиная с местоположения, указанного
        /// параметром dataOffset, в буфер, начиная с местоположения, указанного параметром bufferOffset
        /// </summary>
        /// <param name="position"> Порядковый номер (с нуля) столбца</param>
        /// <param name="dataOffset">Индекс в строке, с которого начинается операция считывания</param>
        /// <param name="bufferOffset">Индекс для буфера, в который будут копироваться данные</param>
        /// <param name="length">Наибольшее число символов для чтения</param>
        /// <returns>Буфер, в который копируются данные</returns>
        byte[] GetBytes(int position, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue);
        /// <summary> Считывает поток байтов из указанного столбца, начиная с местоположения, указанного
        /// параметром dataOffset, в буфер, начиная с местоположения, указанного параметром bufferOffset
        /// </summary>
        /// <param name="columnName">Название столбца</param>
        /// <param name="dataOffset">Индекс в строке, с которого начинается операция считывания</param>
        /// <param name="bufferOffset">Индекс для буфера, в который будут копироваться данные</param>
        /// <param name="length">Наибольшее число символов для чтения</param>
        /// <returns>Буфер, в который копируются данные</returns>
        byte[] GetBytes(string columnName, long dataOffset = 0, int bufferOffset = 0, int length = int.MaxValue);

        /// <summary>Получает количество столбцов в текущей строке
        /// </summary>
        /// <returns>Количество столбцов в текущей строке</returns>
        int GetFiealdCount();

        /// <summary>Получает тип данных указанного столбца
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Тип данных указанного столбца</returns>
        Type GetFieldType(int position);
        /// <summary>Получает тип данных указанного столбца
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Тип данных указанного столбца</returns>
        DbType GetFieldDbType(int position);

        /// <summary>Получает имя столбца при наличии заданного порядкового номера (с нуля) столбца.
        /// </summary>
        /// <param name="position">Порядковый номер (с нуля) столбца</param>
        /// <returns>Имя указанного столбца</returns>
        string GetFieldName(int position);

        /// <summary> Задает текст запроса
        /// </summary>
        string sql { get; set; }

        /// <summary> Получение номера версии базы
        /// </summary>
        int VersionPostgres { get; }

        bool ExecuteNonQuery(IEnumerable<IParams> listParams = null);
        bool ExecuteReader(IEnumerable<IParams> listParams = null);
        bool ExecuteMultipleReader(IEnumerable<IParams> listParams = null);
        object ExecuteScalar(IEnumerable<IParams> listParams = null);
        T ExecuteScalar<T>(IEnumerable<IParams> paramArrya = null);

        void EndTransaction();

        void BeginTransaction();
    }
}