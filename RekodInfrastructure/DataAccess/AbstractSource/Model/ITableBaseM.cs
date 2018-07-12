using System;
using System.Collections.Generic;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.Services; 

namespace Rekod.DataAccess.AbstractSource.Model
{
    public interface ITableBaseM
    {
        object Id { get; }
        bool IsHidden { get; }
        bool IsReadOnly { get; }
        bool IsLayer { get; }
        string Name { get; }
        object Tag { get; }
        string Text { get; }
        IEnumerable<AbsM.IFieldM> Fields { get; }
        AbsM.ETableType Type { get; }
        AbsM.IDataRepositoryM Source { get; }
        List<ITableBaseM> RefTables { get; }

    }

    #region ETableType
    /// <summary>
    /// Тип таблицы
    /// </summary>
    [TypeResource("AbsM.ETableType")]
    public enum ETableType
    {
        /// <summary>
        /// Растровый слой
        /// </summary>
        Rastr = -2,
        /// <summary>
        /// Слой подложки
        /// </summary>
        BottomLayer = -1,
        /// <summary>
        /// Таблица без типа - таблица общего типа
        /// </summary>
        CommonType = 0,
        /// <summary>
        /// Тематический слой карты
        /// </summary>
        MapLayer = 1,
        /// <summary>
        /// Справочник
        /// </summary>
        Catalog = 2,
        /// <summary>
        /// Интервал
        /// </summary>
        Interval = 3,
        /// <summary>
        /// Таблица с данными
        /// </summary>
        Data = 4,
        /// <summary>
        /// Пользовательское представление
        /// </summary>
        View = 5, 
        /// <summary>
        /// Косметический слой
        /// </summary>
        Cosmetic = 6
    }
    #endregion

}