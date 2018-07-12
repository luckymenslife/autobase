using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace axVisUtils.Styles
{
    public class objStylesM : ICloneable
    {
        #region Поля
        private bool _defaultStyle;
        private objStyleFont _fontStyle;
        private objStyleSymbol _symbolStyle;
        private objStylePen _penStyle;
        private objStyleBrush _brushStyle;
        private objStyleRange _range;
        #endregion // Поля

        #region Свойства
        public bool DefaultStyle
        {
            get { return _defaultStyle; }
            set { _defaultStyle = value; }
        }
        public objStyleFont FontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; }
        }
        public objStyleSymbol SymbolStyle
        {
            get { return _symbolStyle; }
            set { _symbolStyle = value; }
        }
        public objStylePen PenStyle
        {
            get { return _penStyle; }
            set { _penStyle = value; }
        }
        public objStyleBrush BrushStyle
        {
            get { return _brushStyle; }
            set { _brushStyle = value; }
        }
        public objStyleRange Range
        {
            get { return _range; }
            set { _range = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public objStylesM()
        {
            _defaultStyle = true;
            _fontStyle = new objStyleFont();
            _symbolStyle = new objStyleSymbol();
            _penStyle = new objStylePen();
            _brushStyle = new objStyleBrush();
        }
        #endregion // Конструктор


        public object Clone()
        {
            var obj = new objStylesM();
            obj._defaultStyle = _defaultStyle;
            obj._fontStyle = (objStyleFont)_fontStyle.Clone();
            obj._symbolStyle = (objStyleSymbol)_symbolStyle.Clone();
            obj._penStyle = (objStylePen)_penStyle.Clone();
            obj._brushStyle = (objStyleBrush)_brushStyle.Clone();
            return obj;
        }
    }
}
