using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgStyleObjectM : ViewModelBase
    {
        #region Поля
        private AbsM.TableBaseM _table;
        private object _idObject;

        private int _brushBgcolor;
        private int _brushFgcolor;
        private int _brushHatch;
        private int _brushStyle;
        private double _opacity;

        private string _fontName;
        private int _fontColor;
        private int _fontFrameColor;
        private int _fontSize;

        private int _penColor;
        private int _penType;
        private int _penWidth;

        private int _symbol;
        private SourceCosmetic.Model.CosmeticStyleType _cosmeticStyleType;
        private bool _hasBackground;
        #endregion Поля

        #region Конструкторы
        public PgStyleObjectM(AbsM.TableBaseM table, object idObject)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (idObject == null)
                throw new ArgumentNullException("idObject");
            _table = table;
            _idObject = idObject;
        }

        public PgStyleObjectM(SourceCosmetic.Model.CosmeticStyleType type)
        {
            this._cosmeticStyleType = type;
        }
        #endregion Конструкторы

        #region Свойства

        public SourceCosmetic.Model.CosmeticStyleType CosmeticStyleType
        {
            get { return _cosmeticStyleType; }
        }
        /// <summary>
        /// Таблица
        /// </summary>
        public AbsM.TableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public object IdObject
        {
            get { return _idObject; }
        }
       
        #region Настройка кисти
        public int BrushBgColor
        {
            get { return _brushBgcolor; }
            set { OnPropertyChanged(ref _brushBgcolor, value, () => this.BrushBgColor); }
        }
        public int BrushFgColor
        {
            get { return _brushFgcolor; }
            set { OnPropertyChanged(ref _brushFgcolor, value, () => this.BrushFgColor); }
        }
        public int BrushHatch
        {
            get { return _brushHatch; }
            set { OnPropertyChanged(ref _brushHatch, value, () => this.BrushHatch); }
        }
        public int BrushStyle
        {
            get { return _brushStyle; }
            set { OnPropertyChanged(ref _brushStyle, value, () => this.BrushStyle); }
        }
        /// <summary>
        /// Прозрачность полигона
        /// </summary>
        public double Opacity
        {
            get { return _opacity; }
            set { OnPropertyChanged(ref _opacity, value, () => this.Opacity); }
        }
        /// <summary>
        /// Есть ли у полигона фон
        /// </summary>
        public bool HasBackground
        {
            get { return _hasBackground; }
            set { OnPropertyChanged(ref _hasBackground, value, () => this.HasBackground); }
        }
        #endregion Настройка кисти

        #region Настройка шрифта
        public string FontName
        {
            get { return _fontName; }
            set 
            {
                _fontName = value;
                OnPropertyChanged(() => this.FontName); 
            }
        }
        public int FontColor
        {
            get { return _fontColor; }
            set
            {
                _fontColor = value;
                OnPropertyChanged(() => this.FontColor);
            }
        }
        public int FontFrameColor
        {
            get { return _fontFrameColor; }
            set
            {
                _fontFrameColor = value;
                OnPropertyChanged(() => this.FontFrameColor);
            }
        }
        public int FontSize
        {
            get { return _fontSize; }
            set 
            {
                _fontSize = value;
                OnPropertyChanged(() => this.FontSize); 
            }
        }
        #endregion // Настройка шрифта

        #region Параметры карандаша
        public int PenColor
        {
            get { return _penColor; }
            set { OnPropertyChanged(ref _penColor, value, () => this.PenColor); }
        }
        public int PenType
        {
            get { return _penType; }
            set { OnPropertyChanged(ref _penType, value, () => this.PenType); }
        }
        public int PenWidth
        {
            get { return _penWidth; }
            set { OnPropertyChanged(ref _penWidth, value, () => this.PenWidth); }
        }
        #endregion Параметры карандаша

        #region Параметры символа
        public int Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value, () => this.Symbol); }
        }
        #endregion // Параметры символа
        #endregion Свойства

        #region Методы
        public void SetFont(string name, int color, int frameColor, int size)
        {
            _fontName = name;
            _fontColor = color;
            _fontFrameColor = frameColor;
            _fontSize = size;
        }
        public void SetBrush(int bgColor, int fgColor, int hatch, int style)
        {
            _brushBgcolor = bgColor;
            _brushFgcolor = fgColor;
            _brushHatch = hatch;
            _brushStyle = style;
        }
        public void SetPen(int color, int type, int width)
        {
            _penColor = color;
            _penType = type;
            _penWidth = width;
        }
        public void SetSymbol(int shape)
        {
            _symbol = shape;
        }
        
        public override string ToString()
        {
            return _table.Name;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PgStyleObjectM);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        bool Equals(PgStyleObjectM obj)
        {
            if (obj == null)
                return false;
            return (this.Table == obj.Table
                && this.IdObject.Equals(obj.IdObject));
        }
        /// <summary>
        /// Преобразования wdtnf из uint в RGB
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color convColor(uint value)
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb(r1, g1, b1);
            return c1;
        }
        #endregion // Методы
    }
}