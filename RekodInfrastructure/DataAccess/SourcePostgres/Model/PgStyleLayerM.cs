using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Services; 
using Rekod.Controllers;
using System.Windows;
using Rekod.Behaviors;
using System.Windows.Controls;
using Rekod.DataAccess.SourcePostgres.View.ConfigView;
using System.Windows.Data;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Стили слоев PostgreSql
    /// </summary>
    public class PgStyleLayerM : ViewModelBase
    {
        #region Поля
        private bool _isNewTable;
        private int _id;
        private PgTableBaseM _table;
        private PgStyleSetsM _set;
        private EStyleType _styleType;
        private PgM.PgFieldM _styleField;
        private string _imageColumn;
        private string _angleColumn;
        private int _minObjectSize;
        private bool _useBounds;
        private int _minScale;
        private int _maxScale;
        private int _rangPrecisionPoint = 1;
        private EChangeColor _rangTypeColor;
        private int _rangMinColor;
        private int? _rangMinVal;
        private int _rangMaxColor;
        private int? _rangMaxVal;
        private int? _rangNullColor;
        private bool _graphicUnits = false;
        private string _fontName = "Map Symbols";
        private int _fontColor = 16711680;
        private int _fontFrameColor = 16711680;
        private int _fontSize = 12;
        private int _symbol = 35;
        private int _penColor = 16711680;
        private int _penType = 2;
        private int _penWidth = 1;
        private int _brushBgColor = 16711680;
        private int _brushFgColor = 16711680;
        private int _brushStyle = 0;
        private int _brushHatch = 0;
        private bool _rangUseMinValue;
        private bool _rangUseMaxValue;
        private bool _rangUseNullColor;
        private SourceCosmetic.Model.CosmeticStyleType _cosmeticStyleType;
        private double _opacity = 0.5;
        private bool _hasBackground;
        #endregion Поля

        #region Конструкторы
        public PgStyleLayerM(PgTableBaseM table, PgStyleSetsM set)
        {
            _isNewTable = true;
            _table = table;
            _set = set;
        }
        public PgStyleLayerM(int id, PgTableBaseM table, PgStyleSetsM set)
        {
            _id = id;
            _table = table;
            _set = set;
        }
        public PgStyleLayerM(SourceCosmetic.Model.CosmeticStyleType cosmeticStyleType)
        {
            _cosmeticStyleType = cosmeticStyleType;
        }

        #endregion Конструкторы

        #region Свойства

        /// <summary>
        /// Признак нового элемента
        /// </summary>
        public bool IsNewTable
        {
            get { return _isNewTable; }
        }
        /// <summary>
        /// Идентификатор подписи
        /// </summary>
        public int Id
        { get { return _id; } }
        /// <summary>
        /// Ссылка на таблицу
        /// </summary>
        public PgTableBaseM Table
        { get { return _table; } }
        /// <summary>
        /// Ссылка на набор
        /// </summary>
        public PgStyleSetsM Set
        { get { return _set; } }
        /// <summary>
        /// тип стиля: Единообразно, по диапазону, по справочнику и  интервалу
        /// </summary>
        public EStyleType StyleType
        {
            get { return _styleType; }
            set { OnPropertyChanged(ref _styleType, value, () => this.StyleType); }
        }
        /// <summary>
        /// тип геометрии, для которой определяется стиль 
        /// (для косметических слоев, используется в SourceCosmetic.View.DefaultStyleView)
        /// </summary>
        public SourceCosmetic.Model.CosmeticStyleType CosmeticStyleType
        {
            get { return _cosmeticStyleType; }
        }

        /// <summary>
        /// Колонка по которой идет раскраска, если раскраска осуществляется по справочнику или по интервалу. Иначе null. 
        /// Эта поле находится в той таблице, для которой создан стиль. 
        /// </summary>
        public PgM.PgFieldM StyleField
        {
            get { return _styleField; }
            set { OnPropertyChanged(ref _styleField, value, () => this.StyleField); }
        }
        /// <summary>
        /// Наименование колонки, в которой храняться пути до значков.
        /// </summary>
        public string ImageColumn
        {
            get { return _imageColumn; }
            set { OnPropertyChanged(ref _imageColumn, value, () => this.ImageColumn); }
        }
        /// <summary>
        /// Наименование колонки, в которой храниться угол повората значка.
        /// </summary>
        public string AngleColumn
        {
            get { return _angleColumn; }
            set { OnPropertyChanged(ref _angleColumn, value, () => this.AngleColumn); }
        }
        /// <summary>
        /// минимальный размер объекта в пикселях на карте, который будет рисоваться
        /// </summary>
        public int MinObjectSize
        {
            get { return _minObjectSize; }
            set { OnPropertyChanged(ref _minObjectSize, value, () => this.MinObjectSize); }
        }
        /// <summary>
        /// Использование при рисовании объектов верхнюю и нижнюю границу масшатаба
        /// </summary>
        public bool UseBounds
        {
            get { return _useBounds; }
            set { OnPropertyChanged(ref _useBounds, value, () => this.UseBounds); }
        }
        /// <summary>
        /// Нижняя граница масштаба
        /// </summary>
        public int MinScale
        {
            get { return _minScale; }
            set { OnPropertyChanged(ref _minScale, value, () => this.MinScale); }
        }
        /// <summary>
        /// Вержняя граница масштаба
        /// </summary>
        public int MaxScale
        {
            get { return _maxScale; }
            set { OnPropertyChanged(ref _maxScale, value, () => this.MaxScale); }
        }
        /// <summary>
        /// Количество знаков после запятой для значений при построении раскрасски по диапазону
        /// </summary>
        public int RangPrecisionPoint
        {
            get { return _rangPrecisionPoint; }
            set { OnPropertyChanged(ref _rangPrecisionPoint, value, () => this.RangPrecisionPoint); }
        }
        /// <summary>
        /// Элемент стиля, который будет изменяться по диапазону
        /// </summary>
        public EChangeColor RangTypeColor
        {
            get { return _rangTypeColor; }
            set { OnPropertyChanged(ref _rangTypeColor, value, () => this.RangTypeColor); }
        }
        /// <summary>
        /// Цвет для минимального значения для объектов по диапазону
        /// </summary>
        public int RangMinColor
        {
            get { return _rangMinColor; }
            set { OnPropertyChanged(ref _rangMinColor, value, () => this.RangMinColor); }
        }
        /// <summary>
        /// Использовать минимальное значение для диапазона
        /// </summary>
        public Boolean RangUseMinValue
        {
            get { return _rangUseMinValue; }
            set { OnPropertyChanged(ref _rangUseMinValue, value, () => this.RangUseMinValue); }
        }
        /// <summary>
        /// Минимальное значение у диапазона
        /// </summary>
        public int? RangMinVal
        {
            get { return _rangMinVal; }
            set { OnPropertyChanged(ref _rangMinVal, value, () => this.RangMinVal); }
        }
        /// <summary>
        /// Цвет для максимального значения при расскраске по диапазону
        /// </summary>
        public int RangMaxColor
        {
            get { return _rangMaxColor; }
            set { OnPropertyChanged(ref _rangMaxColor, value, () => this.RangMaxColor); }
        }
        /// <summary>
        /// Использовать максимальное значение для диапазона
        /// </summary>
        public Boolean RangUseMaxValue
        {
            get { return _rangUseMaxValue; }
            set { OnPropertyChanged(ref _rangUseMaxValue, value, () => this.RangUseMaxValue); }
        }
        /// <summary>
        /// Максимальное значения диапазона
        /// </summary>
        public int? RangMaxVal
        {
            get { return _rangMaxVal; }
            set { OnPropertyChanged(ref _rangMaxVal, value, () => this.RangMaxVal); }
        }
        /// <summary>
        /// Использовать цвет для объектов со значением NULL
        /// </summary>
        public Boolean RangUseNullColor
        {
            get { return _rangUseNullColor; }
            set { OnPropertyChanged(ref _rangUseNullColor, value, () => this.RangUseNullColor); }
        }
        /// <summary>
        /// Цвет для объектов со значением NULL
        /// </summary>
        public int? RangNullColor
        {
            get { return _rangNullColor; }
            set { OnPropertyChanged(ref _rangNullColor, value, () => this.RangNullColor); }
        }
        /// <summary>
        /// Рисовать объекты в единицах проекции
        /// </summary>
        public bool GraphicUnits
        {
            get { return _graphicUnits; }
            set { OnPropertyChanged(ref _graphicUnits, value, () => this.GraphicUnits); }
        }
        /// <summary>
        /// Наименование шрифта символа
        /// </summary>
        public string FontName
        {
            get { return _fontName; }
            set { OnPropertyChanged(ref _fontName, value, () => this.FontName); }
        }
        /// <summary>
        /// цвет шрифта
        /// </summary>
        public int FontColor
        {
            get { return _fontColor; }
            set { OnPropertyChanged(ref _fontColor, value, () => this.FontColor); }
        }
        /// <summary>
        /// Цвет каймы шрифта
        /// </summary>
        public int FontFrameColor
        {
            get { return _fontFrameColor; }
            set { OnPropertyChanged(ref _fontFrameColor, value, () => this.FontFrameColor); }
        }
        /// <summary>
        /// Размер символов
        /// </summary>
        public int FontSize
        {
            get { return _fontSize; }
            set { OnPropertyChanged(ref _fontSize, value, () => this.FontSize); }
        }
        /// <summary>
        /// Код символа
        /// </summary>
        public int Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value, () => this.Symbol); }
        }
        /// <summary>
        /// Цвет карандаша
        /// </summary>
        public int PenColor
        {
            get { return _penColor; }
            set { OnPropertyChanged(ref _penColor, value, () => this.PenColor); }
        }
        /// <summary>
        /// Тип линии
        /// </summary>
        public int PenType
        {
            get { return _penType; }
            set { OnPropertyChanged(ref _penType, value, () => this.PenType); }
        }
        /// <summary>
        /// Ширина карандаша
        /// </summary>
        public int PenWidth
        {
            get { return _penWidth; }
            set { OnPropertyChanged(ref _penWidth, value, () => this.PenWidth); }
        }
        /// <summary>
        /// Фон заливки
        /// </summary>
        public int BrushBgColor
        {
            get { return _brushBgColor; }
            set { OnPropertyChanged(ref _brushBgColor, value, () => this.BrushBgColor); }
        }
        /// <summary>
        /// Цвет заливки
        /// </summary>
        public int BrushFgColor
        {
            get { return _brushFgColor; }
            set { OnPropertyChanged(ref _brushFgColor, value, () => this.BrushFgColor); }
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
        /// Стиль заливки
        /// </summary>
        public int BrushStyle
        {
            get { return _brushStyle; }
            set { OnPropertyChanged(ref _brushStyle, value, () => this.BrushStyle); }
        }
        /// <summary>
        /// Другой стиль заливки
        /// </summary>
        public int BrushHatch
        {
            get { return _brushHatch; }
            set { OnPropertyChanged(ref _brushHatch, value, () => this.BrushHatch); }
        }
        public bool HasBackground
        {
            get { return _hasBackground; }
            set
            {
                OnPropertyChanged(ref _hasBackground, value, () => this.HasBackground);
            }
        }
        #endregion // Свойства

        #region Действия
        public Action<object> StyleTypeChangedAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;

                        ComboBox StyleBox = commParams[0] as ComboBox;
                        StyleIntervalReference StyleIntervalReferenceControl = commParams[1] as StyleIntervalReference;
                        StyleRange StyleRangeControl = commParams[2] as StyleRange;

                        if (StyleBox.SelectedItem == null)
                        {
                            StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Collapsed;
                            StyleRangeControl.Visibility = System.Windows.Visibility.Collapsed;
                            return;
                        }
                        EnumWrapper enumWrap = (EnumWrapper)(StyleBox.SelectedItem);
                        switch ((PgM.EStyleType)(enumWrap.Value))
                        {
                            case PgM.EStyleType.interval:
                                {
                                    StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Visible;
                                    StyleIntervalReferenceControl.IsInterval = true;
                                    StyleRangeControl.Visibility = System.Windows.Visibility.Collapsed;
                                    break;
                                }
                            case PgM.EStyleType.directory:
                                {
                                    StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Visible;
                                    StyleIntervalReferenceControl.IsInterval = false;
                                    StyleRangeControl.Visibility = System.Windows.Visibility.Collapsed;
                                    break;
                                }
                            case PgM.EStyleType.uniformly:
                                {
                                    StyleRangeControl.RangeIsEnabled = false;
                                    StyleRangeControl.Visibility = System.Windows.Visibility.Visible;
                                    StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Collapsed;
                                    break;
                                }
                            case PgM.EStyleType.range:
                                {
                                    StyleRangeControl.RangeIsEnabled = true;
                                    StyleRangeControl.Visibility = System.Windows.Visibility.Visible;
                                    StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Collapsed;
                                    break;
                                }
                            default:
                                {
                                    StyleIntervalReferenceControl.Visibility = System.Windows.Visibility.Collapsed;
                                    StyleRangeControl.Visibility = System.Windows.Visibility.Collapsed;
                                    break;
                                }
                        }
                    };
            }
        }
        public Action<object> BindingGroupLoadedAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindGroup != null) 
                        bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupErrorAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    ValidationErrorEventArgs errorArgs = commEvtParam.EventArgs as ValidationErrorEventArgs;
                    if (errorArgs.Action == ValidationErrorEventAction.Added)
                    {
                        MessageBox.Show(errorArgs.Error.ErrorContent.ToString());
                    }
                };
            }
        }
        public Action<object> BindingGroupCancelAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindGroup != null)
                    {
                        bindGroup.CancelEdit();
                        bindGroup.BeginEdit();
                    }
                };
            }
        }
        public Action<object> BindingGroupSaveAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindGroup != null && bindGroup.CommitEdit())
                    {

                        if (Table != null)
                        {
                            (Table.Source as PgVM.PgDataRepositoryVM).DBSaveStyle(Table.Style);
                            if (Table.IsVisible)
                            {
                                Table.IsVisible = false;
                                Table.IsVisible = true;
                            }
                        }

                        bindGroup.BeginEdit();
                    }
                };
            }
        }
        #endregion Действия

        #region Methods
        public PgStyleObjectM GetStyleInstance(SourceCosmetic.Model.CosmeticStyleType type)
        {
            return new PgStyleObjectM(type)
            {
                BrushBgColor = BrushBgColor,
                BrushFgColor = BrushFgColor,
                BrushHatch = BrushHatch,
                BrushStyle = BrushStyle,
                FontColor = FontColor,
                FontFrameColor = FontFrameColor,
                FontName = FontName,
                FontSize = FontSize,
                PenColor = PenColor,
                PenType = PenType,
                PenWidth = PenWidth,
                Symbol = Symbol,
                Opacity = Opacity
            };
        }
        #endregion Methods
    }
    [TypeResource("PgM.EStyleType")]
    public enum EStyleType
    {
        /// <summary> Единообразно
        /// </summary>
        uniformly = 0,
        /// <summary> По диапазону
        /// </summary>
        range = 1,
        /// <summary> По справочнику
        /// </summary>
        directory = 2,
        /// <summary> По интервалу
        /// </summary>
        interval = 3
    }

    [TypeResource("PgM.EChangeColor")]
    public enum EChangeColor
    {
        /// <summary>
        /// Основной цвет
        /// </summary>
        Foreground = 0,
        /// <summary>
        /// Фон
        /// </summary>
        Background = 1,
        /// <summary>
        /// Границы и линии
        /// </summary>
        LinesBounds = 2,
        /// <summary>
        /// Кайма
        /// </summary>
        Fringe = 3
    }
}