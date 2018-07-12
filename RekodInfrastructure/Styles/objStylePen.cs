using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStylePen:ICloneable
    {
        #region Поля
        private uint _color;
        private ushort _type;
        private uint _width;
        #endregion // Поля

        #region Свойства
        public uint Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public ushort Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public uint Width
        {
            get { return _width; }
            set { _width = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStylePen()
        {
            _color = 16711680;
            _type = 2;
            _width = 1;
        }

        public objStylePen(uint color, ushort type, uint width)
        {
            _color = color;
            _type = type;
            _width = width;
        }
        #endregion // Конструктор

        public object Clone()
        {
            return new objStylePen(_color, _type, _width);
        }
    }
}
