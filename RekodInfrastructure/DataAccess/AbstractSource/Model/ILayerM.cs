using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Services;

namespace Rekod.DataAccess.AbstractSource.Model
{
    public interface ILayerM : ITableBaseM
    {
        bool IsVisible { get; set; }
        bool IsSelectable { get; set; }
        bool IsEditable { get; set; }
        string NameMap { get; set; }
        EGeomType GeomType { get; set; }
        int? Srid { get; }
    }

    #region EGeomType
    /// <summary>
    /// Тип геометрии
    /// </summary>
    [TypeResource("AbsM.EGeomType")]
    public enum EGeomType
    {
        /// <summary> Любая геометрия
        /// </summary>
        Any = -1,
        /// <summary> Без геометрии
        /// </summary>
        None = 0,
        /// <summary> Точки
        /// </summary>
        Point = 1,
        /// <summary> Линии
        /// </summary>
        Line = 2,
        /// <summary> Площадные объекты
        /// </summary>
        Polygon = 3,
        /// <summary> Множество точек
        /// </summary>
        MultiPoint = 4,
        /// <summary> Множество линий
        /// </summary>
        MultiLine = 5,
        /// <summary> Множество полигонов
        /// </summary>
        MultiPolygon = 6,
        /// <summary> Коллекция геометрий
        /// </summary>
        GeometryCollection = 7,
        /// <summary> Пустота полигона
        /// </summary>
        Hole = 8
    }
    #endregion
}