using System.Data;
using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace Rekod.ImportExport
{
    public abstract class Importer : BackgroundWorker, IImporter
    {
        abstract public UserControl SettingsPanel { get; }
        abstract public void Init(FileInfo inputFile, tablesInfo tableInfo);
        abstract public void SetTableInfo(tablesInfo _tableInfo);
        abstract public DataTable GetPreviewTable(int rowsCount = 100);
        abstract public void Load(List<FieldMatch> fields);
        abstract public int RowsCount { get; }
    }
    public class FieldMatch
    {
        public string Src { get; set; }
        public Interfaces.fieldInfo Dest { get; set; }
    }

    public class ImporterHelper
    {
        public static string InvalidFileFormatError { get { return Rekod.Properties.Resources.ImporterHelper_ExceptFileName; } }
        public static int RowsToLoadInPortion { get { return 1; } }
        public static void SetParamType(Interfaces.fieldInfo field, Interfaces.IParams param)
        {
            switch (field.TypeField)
            {
                case Interfaces.TypeField.Date:
                    param.type = DbType.Date;
                    param.typeData = NpgsqlTypes.NpgsqlDbType.Date;
                    break;
                case Interfaces.TypeField.DateTime:
                    param.type = DbType.DateTime;
                    param.typeData = NpgsqlTypes.NpgsqlDbType.TimestampTZ;
                    break;
                case Interfaces.TypeField.Integer:
                    param.type = DbType.Int32;
                    param.typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                    break;
                case Interfaces.TypeField.Numeric:
                    param.type = DbType.Double;
                    param.typeData = NpgsqlTypes.NpgsqlDbType.Numeric;
                    break;
                case Interfaces.TypeField.Text:
                case Interfaces.TypeField.Geometry:
                case Interfaces.TypeField.Default:
                default:
                    param.type = DbType.String;
                    param.typeData = NpgsqlTypes.NpgsqlDbType.Text;
                    break;
            }
        }
        public static void SetParamValue(Interfaces.IParams param, object value)
        {
            if (value == null || value.ToString() == "")
            {
                param.value = DBNull.Value;
                return;
            }
            DateTime datVal;
            int intVal;
            double doubVal;
            switch (param.typeData)
            {
                case NpgsqlTypes.NpgsqlDbType.Date:
                case NpgsqlTypes.NpgsqlDbType.TimestampTZ:
                    if (value.ToString() == "")
                        param.value = DBNull.Value;
                    else
                    {
                        if (!DateTime.TryParse(value.ToString(), out datVal))
                            throw new Exception(String.Format("{0} {1} (\"{2}\")", Rekod.Properties.Resources.ImporterHelper_ExceptConvertDate, param.paramName, value));
                        param.value = DateTime.Parse(value.ToString());
                    }
                    break;
                case NpgsqlTypes.NpgsqlDbType.Integer:
                    if (value.ToString() == "")
                        param.value = DBNull.Value;
                    else
                    {
                        if (!int.TryParse(value.ToString(), out intVal))
                            throw new Exception(String.Format("{0} {1} (\"{2}\")", Rekod.Properties.Resources.ImporterHelper_ExceptConvertInt, param.paramName, value));
                        param.value = int.Parse(value.ToString());
                    }
                    break;
                case NpgsqlTypes.NpgsqlDbType.Numeric:
                    if (value.ToString() == "")
                        param.value = DBNull.Value;
                    else
                    {
                        if (!double.TryParse(value.ToString().Replace('.', ','), out doubVal))
                            throw new Exception(String.Format("{0} {1} (\"{2}\")", Rekod.Properties.Resources.ImporterHelper_ExceptConvertNumeric, param.paramName, value));
                        param.value = double.Parse(value.ToString().Replace('.', ','));
                    }
                    break;
                case NpgsqlTypes.NpgsqlDbType.Bytea:
                    param.value = (byte[])value;
                    break;
                case NpgsqlTypes.NpgsqlDbType.Text:
                default:
                    param.value = value.ToString();
                    break;
            }
        }
    }

    public interface IImporter
    {
        /// <summary>
        /// Элемент управления, который будет вставлен в окно загрузчика как настраиваемые параметры конкретного типа загрузчика
        /// </summary>
        UserControl SettingsPanel { get; }
        /// <summary>
        /// Метод инициализации загрузчика. Предполагается, что тут он будет считывать данные из файла в соответствиии с заданными натсройкаами
        /// </summary>
        /// <param name="inputFile">Файл для загрузки</param>
        /// <param name="tableInfo">Таблица-приемник данных</param>
        void Init(FileInfo inputFile, tablesInfo tableInfo);
        /// <summary>
        /// Возвращает таблицу предпросмотра
        /// </summary>
        /// <param name="previewRowsCount">Количество строк для предпросмотра. По умолчанию - 100</param>
        /// <returns>Таблица с данными</returns>
        DataTable GetPreviewTable(int previewRowsCount = 100);
        /// <summary>
        /// Грузит в базу
        /// </summary>
        /// <param name="fields">Порядок полей как выбрал юзер. Для пустого поля = null</param>
        void Load(List<FieldMatch> fields);
        /// <summary>
        /// Возвращает количество строк в файле
        /// </summary>
        int RowsCount { get; }
    }

}