using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStyleFont : ICloneable
    {
        #region Поля
        private string _fontName;
        private uint _color;
        private uint _frameColor;
        private int _size;

        #endregion // Поля

        #region Свойства
        public string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }
        public uint Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public uint FrameColor
        {
            get { return _frameColor; }
            set { _frameColor = value; }
        }
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStyleFont()
        {
            _fontName = "Map Symbols";
            _color = 16711680;
            _frameColor = 16711680;
            _size = 12;
        }

        public objStyleFont(uint color, string fontName, uint frameColor, int size)
        {
            // TODO: Complete member initialization
            _color = color;
            _fontName = fontName;
            _frameColor = frameColor;
            _size = size;
        }
        #endregion // Конструктор

        public object Clone()
        {
            return new objStyleFont(_color, _fontName, _frameColor, _size);
        }
    }
}
