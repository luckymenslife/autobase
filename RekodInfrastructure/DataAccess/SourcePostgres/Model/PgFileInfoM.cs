using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Информация о файлах
    /// </summary>
    public class PgFileInfoM
    {
        #region Свойства
        /// <summary>
        /// Таблица содержащая информацию о файлах
        /// </summary>
        public AbsM.ITableBaseM Table { get; private set; }
        /// <summary>
        /// Наименование таблицы(хранилища) в которой хранятся файлы
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Наименование поля с первичным ключем таблицы
        /// </summary>
        public string FieldId { get; set; }
        /// <summary>
        /// Наименование поля с идентификатором объекта
        /// </summary>
        public string FieldIdObj { get; set; }
        /// <summary>
        /// Наименование поля, в которой хранятся бинарные данные
        /// </summary>
        public string FieldFile { get; set; }
        #endregion

        #region Конструктор
        public PgFileInfoM(AbsM.ITableBaseM table)
        {
            Table = table;
        }
        #endregion
    }
}
