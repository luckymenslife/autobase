using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStyleSymbol : ICloneable
    {
        #region Поля
        private uint _shape;
        #endregion // Поля

        #region Свойства
        public uint Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStyleSymbol()
        {
            _shape = 35;
        }

        public objStyleSymbol(uint shape)
        {
            // TODO: Complete member initialization
            this._shape = shape;
        }
        #endregion // Конструктор


        public object Clone()
        {
            return new objStyleSymbol(_shape);
        }
    }
}
