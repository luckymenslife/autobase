using System;
using System.Diagnostics;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.Services; 

namespace Rekod.DataAccess.AbstractSource.Model
{
    public interface IFieldM
    {
        /// <summary>
        /// Идентификатор атрибута
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Идентификатор таблицы к которой относится атрибут
        /// </summary>
        object IdTable { get; }
        /// <summary>
        /// Идентификатор таблицы к которой относится атрибут
        /// </summary>
        AbsM.ITableBaseM Table { get; }
        /// <summary>
        /// Наименование атрибута
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Наименование атрибута, отображаемое в интерфейсе пользователя
        /// </summary>
        string Text { get; }
        /// <summary>
        /// Тип атрибута
        /// </summary>
        AbsM.EFieldType Type { get; }
        /// <summary>
        /// Поле для чтения
        /// </summary>
        bool IsReadOnly { get; }
    }
    [TypeResource("AbsM.EFieldType")]
    public enum EFieldType
    {
        Integer = 1,
        Text = 2,
        Date = 3,
        DateTime = 4,
        Geometry = 5,
        Real = 6
    }
    [TypeResource("AbsM.ERefType")]
    public enum ERefType
    {
        None = 0,
        Reference = 1,
        Interval = 2,
        Data = 4
    }
}