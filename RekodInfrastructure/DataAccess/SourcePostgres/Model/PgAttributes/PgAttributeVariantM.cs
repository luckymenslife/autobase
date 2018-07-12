using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;

namespace Rekod.DataAccess.SourcePostgres.Model.PgAttributes
{
    [DebuggerDisplay("PgAttributeVariantsM {_source.Field.RefTable.SchemeName}.{_source.Field.RefTable.Name}({_source.Field.RefTable._id}), ID: {_source.Field.RefField.Name}({_source.Field.RefField.Id) Value: {Value} Min:{_min} Max: {_max}")]
    public abstract class PgAttributeVariantM
    {
        #region Поля
        private object _id;
        private PgAttributeM _source;
        private string _text;
        BitmapImage _preview;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Cсылка на источник данной коллекции
        /// </summary>
        public PgAttributeM Source
        { get { return _source; } }
        /// <summary>
        /// То что будет отображатся на экране
        /// </summary>
        public string Text
        { get { return _text; } }
        /// <summary>
        /// Идентификатор объекта справочника
        /// </summary>
        public object Id
        { get { return _id; } }
        /// <summary>
        /// Отображается картинка предпросмотра стиля
        /// </summary>
        public BitmapImage Preview
        { get { return _preview; } }
        #endregion // Свойства

        #region Конструктор
        public PgAttributeVariantM(PgAttributeM source, string text, PgStyleObjectM style, object id)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _id = id;
            _source = source;
            _text = text;
            PgAttributesListVM attrList = this.Source.Source as PgAttributesListVM;

            if (style != null && attrList.LoadStyle)
            {
                switch (((PgM.PgTableBaseM)source.Field.Table).GeomType)
                {
                    case AbsM.EGeomType.Point:
                        _preview = GetPreviewStylePoint(style);
                        break;
                    case AbsM.EGeomType.Line:
                        _preview = GetPreviewStyleLine(style);
                        break;
                    case AbsM.EGeomType.Polygon:
                        _preview = GetPreviewStylePoligon(style);
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion // Конструктор

        #region Абстрактные методы
        /// <summary>
        /// Метод проверки соответствия варианта
        /// </summary>
        /// <param name="value">Значение для проверки соответствия</param>
        /// <returns></returns>
        public abstract bool CheckValue(object checkValue);
        #endregion // Абстрактные методы

        #region Методы
        public static BitmapImage GetPreviewStylePoint(PgM.PgStyleObjectM collection)
        {
            var newPictures = new Bitmap(32, 32);
            var rectMain = new Rectangle(0, 0, 32, 32);

            var graphics = Graphics.FromImage(newPictures);
            var brFC = PgM.PgStyleObjectM.convColor((uint)collection.BrushFgColor);
            var brBC = PgM.PgStyleObjectM.convColor((uint)collection.BrushBgColor);

            // Фон картинки
            graphics.FillRectangle(SystemBrushes.Window, rectMain);

            // Символ точечного объекта
            var font = new Font(collection.FontName, collection.FontSize);
            var sf = new StringFormat(StringFormatFlags.NoClip)
            {
                LineAlignment = StringAlignment.Center
            };
            var fontColor = new SolidBrush(PgM.PgStyleObjectM.convColor((uint)collection.FontColor));

            graphics.DrawString(((char)collection.Symbol).ToString(), font, fontColor, rectMain, sf);

            // Конвертирует из System.Drawing в BitmapImage
            using (var stream = new MemoryStream())
            {
                newPictures.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        public static BitmapImage GetPreviewStyleLine(PgM.PgStyleObjectM collection)
        {
            var newPictures = new Bitmap(32, 32);
            var rectMain = new Rectangle(0, 0, 32, 32);

            var graphics = Graphics.FromImage(newPictures);
            var brFC = PgM.PgStyleObjectM.convColor((uint)collection.BrushFgColor);
            var brBC = PgM.PgStyleObjectM.convColor((uint)collection.BrushBgColor);

            // Фон картинки
            graphics.FillRectangle(SystemBrushes.Window, rectMain);

            // Линия
            ColorPalette pal;
            var pen = lineRes.ResourceManager.GetObject(String.Format("LINE{0:00}", collection.PenType)) as Image;

            if (pen != null)
            {
                pal = pen.Palette;
                pal.Entries[1] = Color.FromArgb(0, 0, 0, 0);
                pal.Entries[0] = PgM.PgStyleObjectM.convColor((uint)collection.PenColor);
                pen.Palette = pal;

                for (int i = 0; i < collection.PenWidth; i++)
                {
                    graphics.DrawImage(pen, new Rectangle(4, i + 2 - collection.PenWidth / 2 + 16, 32, 1), 5, 6, 32, 1, GraphicsUnit.Pixel);
                }
                graphics.Transform.Reset();
            }
            // Конвертирует из System.Drawing в BitmapImage
            using (var stream = new MemoryStream())
            {
                newPictures.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        public static BitmapImage GetPreviewStylePoligon(PgM.PgStyleObjectM collection)
        {
            var newPictures = new Bitmap(32, 32);

            var graphics = Graphics.FromImage(newPictures);
            var brFC = PgM.PgStyleObjectM.convColor((uint)collection.BrushFgColor);
            var brBC = PgM.PgStyleObjectM.convColor((uint)collection.BrushBgColor);
            ColorPalette pal;

            // Заливка полигона
            var brush = ImageBrushRes.ResourceManager.GetObject(String.Format("poly_{0}_{1}", collection.BrushStyle, collection.BrushHatch)) as Image;
            if (brush != null)
            {
                pal = brush.Palette;
                if (pal.Entries.Length == 2)
                {
                    pal.Entries[0] = brFC;
                    pal.Entries[1] = brBC;
                }
                else if (pal.Entries.Length == 1)
                {
                    pal.Entries[0] = brBC;
                }
                brush.Palette = pal;
                graphics.DrawImage(brush, 0, 0, brush.Size.Width, brush.Size.Height);//20, (float)Bounds.Height - 4);
            }

            // Границы полигона
            var pen = lineRes.ResourceManager.GetObject(String.Format("LINE{0:00}", collection.PenType)) as Image;
            if (pen != null)
            {
                pal = pen.Palette;
                pal.Entries[1] = Color.FromArgb(0, 0, 0, 0);
                pal.Entries[0] = Color.Brown;
                pen.Palette = pal;

                for (int i = 0; i < 5; i++)
                {
                    graphics.DrawImage(pen, new Rectangle(0, i, 32, 1), 5, 6, 32, 1,
                                       GraphicsUnit.Pixel);
                    graphics.DrawImage(pen, new Rectangle(0, 32 - i, 32, 1), 5, 6, 32, 1,
                                       GraphicsUnit.Pixel);
                }

                var matr = new Matrix();
                matr.RotateAt(90, new PointF(16, 16));
                graphics.Transform = matr;
                for (int i = 0; i < 5; i++)
                {
                    graphics.DrawImage(pen, new Rectangle(0, i, 32, 1), 5, 6, 32, 1, GraphicsUnit.Pixel);
                    graphics.DrawImage(pen, new Rectangle(0, 32 - i, 32, 1), 5, 6, 32, 1,
                                       GraphicsUnit.Pixel);
                }
                graphics.Transform.Reset();
            }
            // Конвертирует из System.Drawing в BitmapImage
            using (var stream = new MemoryStream())
            {
                newPictures.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        #endregion // Методы
    }
    [DebuggerDisplay("PgAttributeIntervalM {Source.Field._refTable.SchemeName}.{Source.Field._refTable.Name}({_source.Field._refTable._id}), ID: {_source.Field._refField.Name}({_source.Field._refField._id) Value: {Value} Min:{_min} Max: {_max}")]
    public class PgAttributeIntervalM : PgAttributeVariantM
    {
        #region Поля
        private double _min = double.MinValue;
        private double _max = double.MaxValue;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Минимальное значения интервала
        /// </summary>
        public double? Min
        { get { return _min; } }
        /// <summary>
        /// Максимальное значение интервала
        /// </summary>
        public double? Max
        { get { return _max; } }
        #endregion // Свойства

        #region Конструктор
        public PgAttributeIntervalM(PgAttributeM source, string text, PgStyleObjectM style, object id, double min, double max)
            : base(source, text, style, id)
        {
            if (min != null)
                _min = min;
            if (max != null)
                _max = max;
        }
        #endregion // Конструктор

        #region Методы
        public override bool CheckValue(object checkValue)
        {
            var value = ExtraFunctions.Converts.To<double?>(checkValue);
            if (value == null)
                return false;
            return (Min < value && value <= Max);
        }
        #endregion // Методы
    }
    [DebuggerDisplay("PgAttributeReferenceM {Source.Field.RefTable.SchemeName}.{Source.Field.RefTable.Name}({Source.Field.RefTable._id}), ID: {Source.Field.RefField.Name}({Source.Field.RefField.Id) Value: {Value} id: {Id}")]
    public class PgAttributeReferenceM : PgAttributeVariantM
    {
        #region Конструктор
        public PgAttributeReferenceM(PgAttributeM source, string text, PgStyleObjectM style, object id)
            : base(source, text, style, id)
        {
        }
        #endregion // Конструктор

        #region Методы
        public override bool CheckValue(object id)
        {
            return (Id == null)
                ? false
                : Id.Equals(id);
        }
        #endregion // Методы
    }

    public enum EVariantsType
    {
        Not = 0,
        Reference = 1,
        Interval = 2,
        Style = 4
    }
}