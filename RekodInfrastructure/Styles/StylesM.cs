using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using Rekod;

namespace axVisUtils.Styles
{
    public class StylesM : objStylesM
    {
        #region Поля
        private string _name;
        private int _id;
        private int _geomType;
        private object _tag;
        private Image _peview;
        private string _fileName;
        private float _angle;
        #endregion // Поля
        
        #region Конструктор
        public StylesM(int geomType)
        {
            _geomType = geomType;
            _name = "";
            _id = 0;
        }
        public StylesM(objStylesM style, int geomType)
            : this(geomType)
        {
            this.FontStyle = style.FontStyle;
            this.SymbolStyle = style.SymbolStyle;
            this.PenStyle = style.PenStyle;
            this.BrushStyle = style.BrushStyle;
            this.Range = style.Range;
        }
        public StylesM(Interfaces.ISQLCommand sqlCmd, int geomType, bool isStyle = true) :
            this(geomType)
        {
            if (isStyle)
            {
                FontStyle = new objStyleFont()
                {
                    FontName = sqlCmd.GetString("fontname"),
                    Color = sqlCmd.GetValue<uint>("fontcolor"),
                    FrameColor = sqlCmd.GetValue<uint>("fontframecolor"),
                    Size = sqlCmd.GetValue<int>("fontsize"),
                };
                SymbolStyle = new objStyleSymbol()
                {
                    Shape = sqlCmd.GetValue<uint>("symbol")
                };
                PenStyle = new objStylePen()
                {
                    Color = sqlCmd.GetValue<uint>("pencolor"),
                    Type = sqlCmd.GetValue<ushort>("pentype"),
                    Width = sqlCmd.GetValue<uint>("penwidth")
                };
                BrushStyle = new objStyleBrush()
                {
                    bgColor = sqlCmd.GetValue<uint>("brushbgcolor"),
                    fgColor = sqlCmd.GetValue<uint>("brushfgcolor"),
                    Hatch = sqlCmd.GetValue<ushort>("brushhatch"),
                    Style = sqlCmd.GetValue<ushort>("brushstyle")
                };
            }
            else
            {
                FontStyle = new objStyleFont();
                SymbolStyle = new objStyleSymbol();
                PenStyle = new objStylePen();
                BrushStyle = new objStyleBrush();
            }
        }

        public StylesM(mvMapLib.mvLayerStyle mvStyle, int geomType)
            : this(geomType)
        {
            var font = mvStyle.Font;
            if (font != null)
                this.FontStyle = new objStyleFont(font.Color, font.fontname, font.framecolor, font.size);
            else
                this.FontStyle = new objStyleFont();

            if (mvStyle.Symbol != null)
                this.SymbolStyle = new objStyleSymbol(mvStyle.Symbol.shape);
            else
                this.SymbolStyle = new objStyleSymbol();

            var pen = mvStyle.Pen;
            if (pen != null)
                this.PenStyle = new objStylePen(pen.Color, pen.ctype, pen.width);
            else
                this.PenStyle = new objStylePen();

            var brush = mvStyle.Brush;
            if (brush != null)
                this.BrushStyle = new objStyleBrush(brush.bgcolor, brush.fgcolor, brush.hatch, brush.style);
            else
                this.BrushStyle = new objStyleBrush();
        }
        #endregion // Конструктор

        #region Свойства
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public int GeomType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }
        public Image Preview
        {
            get { return _peview; }
            set { _peview = value; }
        }
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        #endregion // Свойства

        public override string ToString()
        {
            return _name ?? "";
        }

        public void SetPreviewStyle(bool isRang = false)
        {
            if (isRang)
            {
                var styleMin = (objStylesM)this.Clone();
                Image imageMin;
                var styleMax = (objStylesM)this.Clone();
                Image imageMax;
                switch (_geomType)
                {
                    case 1:
                        {
                            switch (Range.Type)
                            {
                                case 0:
                                    styleMin.FontStyle.Color = Range.MinColor;
                                    styleMax.FontStyle.Color = Range.MaxColor;
                                    break;
                                case 3:
                                    styleMin.FontStyle.FrameColor = Range.MinColor;
                                    styleMax.FontStyle.FrameColor = Range.MaxColor;
                                    break;
                                default:
                                    return;
                            }
                            imageMin = GetPreviewStylePoint(styleMin);
                            imageMax = GetPreviewStylePoint(styleMax);

                            this.Preview = GetPreviewStyleRange(imageMin, imageMax);
                        }
                        break;
                    case 2:
                        {
                            if (Range.Type == 2)
                            {
                                styleMin.PenStyle.Color = Range.MinColor;
                                styleMax.PenStyle.Color = Range.MaxColor;
                            }
                            imageMin = GetPreviewStyleLine(styleMin);
                            imageMax = GetPreviewStyleLine(styleMax);

                            this.Preview = GetPreviewStyleRange(imageMin, imageMax);
                        }
                        break;
                    case 3:
                        {
                            switch (Range.Type)
                            {
                                case 0:
                                    styleMin.BrushStyle.fgColor = Range.MinColor;
                                    styleMax.BrushStyle.fgColor = Range.MaxColor;
                                    break;
                                case 1:
                                    styleMin.BrushStyle.bgColor = Range.MinColor;
                                    styleMax.BrushStyle.bgColor = Range.MaxColor;
                                    break;
                                case 2:
                                    styleMin.PenStyle.Color = Range.MinColor;
                                    styleMax.PenStyle.Color = Range.MaxColor;
                                    break;
                                default:
                                    return;
                            }
                            imageMin = GetPreviewStylePoligon(styleMin);
                            imageMax = GetPreviewStylePoligon(styleMax);

                            this.Preview = GetPreviewStyleRange(imageMin, imageMax);
                        }
                        break;
                    default:
                        this.Preview = null;
                        break;
                }
            }
            else
                switch (_geomType)
                {
                    case 1:
                        this.Preview = GetPreviewStylePoint(this, FileName);
                        break;
                    case 2:
                        this.Preview = GetPreviewStyleLine(this);
                        break;
                    case 3:
                        this.Preview = GetPreviewStylePoligon(this);
                        break;
                    default:
                        this.Preview = null;
                        break;
                }
        }
        #region Статические методы
        private static Image GetPreviewStylePoint(objStylesM collection, string imageName = null)
        {
            var rectMain = new Rectangle(0, 0, 32, 32);
            Bitmap newPictures = new Bitmap(32, 32);
            var graphics = Graphics.FromImage(newPictures);

            if (String.IsNullOrEmpty(imageName) || !System.IO.File.Exists(Program.path_string +"\\"+ Program.imagesPath + imageName))
            {
                // Фон картинки
                //if (collection.FontStyle.Color != collection.FontStyle.FrameColor)
                //{
                //    var FrameColor = new SolidBrush(convColor((uint)collection.FontStyle.FrameColor));
                //    graphics.FillRectangle(FrameColor, rectMain);
                //}

                var sf = new StringFormat(StringFormatFlags.LineLimit)
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                // Символ точечного объекта 

                //var fontFrame = new Font(collection.FontStyle.FontName, 20, System.Drawing.FontStyle.Bold);
                //var frameColor = new SolidBrush(convColor((uint)collection.FontStyle.FrameColor));
                //graphics.DrawString(((char)collection.SymbolStyle.Shape).ToString(), fontFrame, frameColor, rectMain, sf);

                var font = new Font(collection.FontStyle.FontName, 20);
                var Color = new SolidBrush(convColor((uint)collection.FontStyle.Color));
                var FrameColor = new SolidBrush(convColor((uint)collection.FontStyle.FrameColor));
                string symbol = ((char)collection.SymbolStyle.Shape).ToString();

                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, 0, -1), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, 1, -1), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, 1, 0), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, 1, 1), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, 0, 1), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, -1, 1), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, -1, 0), sf);
                graphics.DrawString(symbol, font, FrameColor, RectangleOffset(rectMain, -1, -1), sf);
                graphics.DrawString(symbol, font, Color, rectMain, sf);
            }
            else
            {
                var image = new Bitmap(Program.path_string +"\\"+Program.imagesPath + imageName);
                if (image != null)
                {
                    float scale = Math.Min(32f / image.Width, 32f / image.Height);

                    var scaleWidth = (int)(image.Width * scale);
                    var scaleHeight = (int)(image.Height * scale);

                    graphics.FillRectangle(new SolidBrush(Color.Transparent), new RectangleF(0, 0, 32, 32));
                    graphics.DrawImage(image, new Rectangle((32 - scaleWidth) / 2, (32 - scaleHeight) / 2, scaleWidth, scaleHeight));
                }
            }

            return newPictures;
        }
        private static Rectangle RectangleOffset(Rectangle dd, int x, int y)
        {
            var rec = new Rectangle(dd.X, dd.Y, dd.Width, dd.Height);
            rec.Offset(x, y);
            return rec;
        }
        private static Image GetPreviewStyleLine(objStylesM collection)
        {
            var newPictures = new Bitmap(32, 32);
            var rectMain = new Rectangle(0, 0, 32, 32);

            var graphics = Graphics.FromImage(newPictures);

            // Фон картинки
            //graphics.FillRectangle(SystemBrushes.Window, rectMain);

            // Линия
            ColorPalette pal;
            var pen = lineRes.ResourceManager.GetObject(String.Format("LINE{0:00}", collection.PenStyle.Type)) as Image;

            if (pen != null)
            {
                pal = pen.Palette;
                pal.Entries[1] = Color.FromArgb(0, 0, 0, 0);
                pal.Entries[0] = convColor((uint)collection.PenStyle.Color);
                pen.Palette = pal;
                var rect = new RectangleF(4, 2 - (float)collection.PenStyle.Width / 2 + 16, 32, 5);
                var rectImage = new Rectangle(5, 6, 6, 1);
                graphics.DrawImage(pen, Rectangle.Ceiling(rect), rectImage, GraphicsUnit.Pixel);

                graphics.Transform.Reset();
            }
            return newPictures;
        }
        private static Image GetPreviewStylePoligon(objStylesM collection)
        {
            var newPictures = new Bitmap(32, 32);

            var graphics = Graphics.FromImage(newPictures);
            var brFC = convColor((uint)collection.BrushStyle.fgColor);
            if (collection.BrushStyle.Style == 0)
                // прозрачность
                brFC = Color.FromArgb(255 - (int)((uint)collection.BrushStyle.bgColor >> 24), brFC);
            var brBC = convColor((uint)collection.BrushStyle.bgColor);
            ColorPalette pal;

            // Заливка полигона
            if (!(collection.BrushStyle.Style == 1 && collection.BrushStyle.Hatch == 0))
            {
                var brush = ImageBrushRes.ResourceManager.GetObject(String.Format("poly_{0}_{1}", collection.BrushStyle.Style, collection.BrushStyle.Hatch)) as Image;
                if (brush == null)
                    brush = ImageBrushRes.poly_5_2; // Стиль полной заливки

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

            var pen = lineRes.ResourceManager.GetObject(String.Format("LINE{0:00}", collection.PenStyle.Type)) as Image;
            if (collection.PenStyle.Width > 0 && collection.PenStyle.Type != 1 && pen != null)
            {
                pal = pen.Palette;
                pal.Entries[1] = Color.FromArgb(0, 0, 0, 0);
                pal.Entries[0] = convColor((uint)collection.PenStyle.Color);
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
            return newPictures;
        }
        private static Image GetPreviewStyleRange(Image imgMin, Image imgMax)
        {
            var gap = 5;
            var newPictures = new Bitmap(imgMin.Width + gap + imgMax.Width, 32);

            var graphics = Graphics.FromImage(newPictures);

            graphics.DrawImage(imgMin, 0, 0, imgMin.Size.Width, imgMin.Size.Height);
            graphics.DrawImage(imgMax, imgMin.Width + gap, 0, imgMax.Size.Width, imgMax.Size.Height);

            return newPictures;
        }
        private static Color convColor(uint value)
        {
            if (value == Program.mainFrm1.axMapLIb1.mvTransparentColor)
                return Color.Transparent;
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
        #endregion // Статические методы
    }
}
