using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStyleBrush : ICloneable
    {
        #region Поля
        private uint _bgСolor;
        private uint _fgСolor;
        private ushort _hatch;
        private ushort _style;
        #endregion // Поля

        #region Свойства
        public uint bgColor
        {
            get { return _bgСolor; }
            set { _bgСolor = value; }
        }
        public uint fgColor
        {
            get { return _fgСolor; }
            set { _fgСolor = value; }
        }
        public ushort Hatch
        {
            get { return _hatch; }
            set { _hatch = value; }
        }
        public ushort Style
        {
            get { return _style; }
            set { _style = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStyleBrush()
        {
            _bgСolor = 16711680;
            _fgСolor = 16711680;
            _hatch = 1;
            _style = 0;
        }

        public objStyleBrush(uint bgСolor, uint fgСolor, ushort hatch, ushort style)
        {
            _bgСolor = bgСolor;
            _fgСolor = fgСolor;
            _hatch = hatch;
            _style = style;
        }
        #endregion // Конструктор

        public object Clone()
        {
            return new objStyleBrush(_bgСolor, _fgСolor, _hatch, _style);
        }
    }
}
