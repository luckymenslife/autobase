using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStyleRange
    {
        #region Поля
        private bool _rangeColors;
        private string _rangeColumn;
        private int _type;
        private int _minValue;
        private uint _minColor;
        private int _maxValue;
        private uint _maxColor;
        private uint _nullColor;
        private bool _minValueUse;
        private bool _maxValueUse;
        private bool _nullColorUse;
        #endregion // Поля

        #region Свойства
        public bool RangeColors
        {
            get { return _rangeColors; }
            set { _rangeColors = value; }
        }
        public string RangeColumn
        {
            get { return _rangeColumn; }
            set { _rangeColumn = value; }
        }
        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public int MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }
        public uint MinColor
        {
            get { return _minColor; }
            set { _minColor = value; }
        }
        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }
        public uint MaxColor
        {
            get { return _maxColor; }
            set { _maxColor = value; }
        }
        public uint NullColor
        {
            get { return _nullColor; }
            set { _nullColor = value; }
        }
        public bool MinValueUse
        {
            get { return _minValueUse; }
            set { _minValueUse = value; }
        }
        public bool MaxValueUse
        {
            get { return _maxValueUse; }
            set { _maxValueUse = value; }
        }
        public bool NullColorUse
        {
            get { return _nullColorUse; }
            set { _nullColorUse = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStyleRange()
        { }
        public objStyleRange(bool rangeColors, string rangeColumn, int type, int minValue, uint minColor, int maxValue, uint maxColor, uint color, bool minValueUse, bool maxValueUse, bool nullColorUse)
        {
            _rangeColors = rangeColors;
            _rangeColumn = rangeColumn;
            _type = type;
            _minValue = minValue;
            _minColor = minColor;
            _maxValue = maxValue;
            _maxColor = maxColor;
            _nullColor = color;
            _minValueUse = minValueUse;
            _maxValueUse = maxValueUse;
            _nullColorUse = nullColorUse;
        }
        #endregion // Конструктор

    }
}
