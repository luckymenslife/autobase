using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows;
using Rekod.PrintModule.Thumbs;
using Rekod.PrintModule.Service;
using System.Windows.Forms.Integration;
using System.Windows.Controls;
using Rekod.PrintModule.RenderComponents;
using System.Windows.Controls.Primitives;

namespace Rekod.PrintModule.LayersObjectModel
{
    public class RastrLayerObject: LayerObjectBase
    {
        #region Поля
        private RastrSource _rastrSource;
        private DrawingCanvas _drawingSurface;
        private List<double> _exParams;
        private FrameworkElement _scrollParent;
        private bool _needsUpdate;
        #endregion Поля

        #region Свойства
        public ResizeThumb TopLeftThumb
        {
            get;
            private set;
        }
        public ResizeThumb TopRightThumb
        {
            get;
            private set;
        }
        public ResizeThumb BottomLeftThumb
        {
            get;
            private set;
        }
        public ResizeThumb BottomRightThumb
        {
            get;
            private set;
        }
        public RastrSource RastrSource
        {
            get { return _rastrSource; }
            set { _rastrSource = value; }
        }
        public bool NeedsUpdate
        {
            get
            {
                return _needsUpdate; 
            }
            set
            {
                if (_needsUpdate == value)
                {
                    return;
                }
                _needsUpdate = value; 
                OnPropertyChanged("NeedsUpdate");
            }
        }
        #endregion Свойства

        #region Конструкторы
        public RastrLayerObject(LayerBase parentlayer, RastrSource rastrsource): base(parentlayer)
        {
            _rastrSource = rastrsource;
            _drawingSurface = parentlayer.DrawingSurface;
            _drawingSurface.ScrollParent.ScrollChanged += ScrollParent_ScrollChanged;

            TopLeftThumb = new ResizeThumb(this) { CornerType =  CornerType.TopLeft };
            TopRightThumb = new ResizeThumb(this) { CornerType = CornerType.TopRight };
            BottomLeftThumb = new ResizeThumb(this) { CornerType = CornerType.BottomLeft };
            BottomRightThumb = new ResizeThumb(this) { CornerType = CornerType.BottomRight };

            parentlayer.DrawingSurface.AddVisual(TopLeftThumb);
            parentlayer.DrawingSurface.AddVisual(TopRightThumb);
            parentlayer.DrawingSurface.AddVisual(BottomLeftThumb);
            parentlayer.DrawingSurface.AddVisual(BottomRightThumb);

            TopLeftThumb.ObjectSizeChanged += new EventHandler<ObjectSizeChangedEventArgs>(ResizeThumb_ObjectSizeChanged);
            TopRightThumb.ObjectSizeChanged += new EventHandler<ObjectSizeChangedEventArgs>(ResizeThumb_ObjectSizeChanged);
            BottomLeftThumb.ObjectSizeChanged += new EventHandler<ObjectSizeChangedEventArgs>(ResizeThumb_ObjectSizeChanged);
            BottomRightThumb.ObjectSizeChanged += new EventHandler<ObjectSizeChangedEventArgs>(ResizeThumb_ObjectSizeChanged);

            _scrollParent = _drawingSurface;
            do
            {
                _scrollParent = _scrollParent.Parent as FrameworkElement;
            }
            while (!(_scrollParent is ScrollViewer));
            _drawingSurface.MapModeChanges += _drawingSurface_MapModeChanges;
        }
        #endregion Конструкторы

        #region Методы
        public override void Render()
        {
            if (_drawingSurface.IsLoaded)
            {
                // Оконные размеры и оконные координаты объекта растра 
                double x = Nodes[0].WinPoint.X;
                double y = Nodes[0].WinPoint.Y;
                double width = Nodes[1].WinPoint.X - Nodes[0].WinPoint.X;
                double height = Nodes[1].WinPoint.Y - Nodes[0].WinPoint.Y;

                Point parentTopLeft = _scrollParent.TranslatePoint(new Point(0, 0), _drawingSurface);
                double parentWidth = _scrollParent.ActualWidth / _drawingSurface.Zoom;
                double parentHeight = _scrollParent.ActualHeight / _drawingSurface.Zoom;

                // Оконные размеры и оконные координаты области предпросмотра
                Rect prevRectW = new Rect(
                    new Point()
                    {
                        X = (parentTopLeft.X > x) ? parentTopLeft.X : x,
                        Y = (parentTopLeft.Y > y) ? parentTopLeft.Y : y
                    },
                    new Point()
                    {
                        X = (x + width < parentTopLeft.X + parentWidth) ?
                                        (x + width) :
                                        (parentTopLeft.X + parentWidth),
                        Y = (y + height < parentTopLeft.Y + parentHeight) ?
                                        (y + height) :
                                        (parentTopLeft.Y + parentHeight)
                    });

                Rect rastRectG = _rastrSource.RastrRectG;
                // Вычисление оконных координат и размеров области растра
                Point rastTopLeftW = _drawingSurface.GlobalToWin(rastRectG.TopLeft);
                Point rastBottomRightW = _drawingSurface.GlobalToWin(rastRectG.BottomRight);
                Rect rastRectW = new Rect(rastTopLeftW, rastBottomRightW);
                prevRectW.Intersect(rastRectW);

                if (prevRectW.Height > 0 && prevRectW.Width > 0)
                {
                    // Вычисление относительных размеров области пересечения относительно растра
                    double leftTopXRel = (prevRectW.TopLeft.X - rastRectW.TopLeft.X) / rastRectW.Width;
                    double leftTopYRel = (prevRectW.TopLeft.Y - rastRectW.TopLeft.Y) / rastRectW.Height;
                    double prevBoxWidthRel = prevRectW.Width / rastRectW.Width;
                    double prevBoxHeightRel = prevRectW.Height / rastRectW.Height;
                    int resWidth = (int)(prevRectW.Width * _drawingSurface.Zoom);
                    int resHeight = (int)(prevRectW.Height * _drawingSurface.Zoom);

                    // Обновление предпросмотра
                    if (!prevRectW.IsEmpty && (int)prevRectW.Width > 0 && (int)prevRectW.Height > 0)
                    {
                        _rastrSource.UpdatePreview(leftTopXRel, leftTopYRel, prevBoxWidthRel, prevBoxHeightRel, resWidth, resHeight);
                    }
                }

                using (DrawingContext dc = this.RenderOpen())
                {
                    if (TopLeftThumb != null) { TopLeftThumb.Render(); }
                    if (TopRightThumb != null) { TopRightThumb.Render(); }
                    if (BottomLeftThumb != null) { BottomLeftThumb.Render(); }
                    if (BottomRightThumb != null) { BottomRightThumb.Render(); }

                    dc.DrawRectangle(
                        Brushes.Violet,
                        new Pen(Brushes.Gray, 1),
                        new Rect(x, y, width, height));
                                 
                    if (!prevRectW.IsEmpty)
                    {
                        dc.DrawImage(_rastrSource.ImageSource, new Rect(prevRectW.TopLeft.X, prevRectW.TopLeft.Y, prevRectW.Width, prevRectW.Height));
                    }
                }
            }
        }

        private void MarkRectagle(DrawingContext dc, Rect rect, int pictwidth)
        {
            DrawRectangle(dc, rect.TopLeft, 1, pictwidth);
            DrawRectangle(dc, rect.TopRight, 2, pictwidth);
            DrawRectangle(dc, rect.BottomLeft, 4, pictwidth);
            DrawRectangle(dc, rect.BottomRight, 3, pictwidth); 
        }
        private void DrawRectangle(DrawingContext dc, Point pt, int place, int pictwidth)
        {
            double x = (place == 1 || place == 4) ? pt.X : pt.X - pictwidth;
            double y = (place == 1 || place == 2) ? pt.Y : pt.Y - pictwidth;
            dc.DrawRectangle(
                       Brushes.Yellow,
                       new Pen(Brushes.Gray, 1),
                       new Rect(x, y, pictwidth, pictwidth));
        }

        public override Point GetCorner(CornerType cornertype)
        {
            Point requiredPoint = Nodes[0].WinPoint; 
            switch (cornertype)
            {
                case CornerType.TopLeft:
                    {
                        break;
                    }
                case CornerType.TopRight:
                    {
                        requiredPoint = new Point(
                            Nodes[1].WinPoint.X,
                            Nodes[0].WinPoint.Y);
                        break;
                    }
                case CornerType.BottomLeft:
                    {
                        requiredPoint = new Point(
                            Nodes[0].WinPoint.X,
                            Nodes[1].WinPoint.Y);
                        break;
                    }
                case CornerType.BottomRight:
                    {
                        requiredPoint = Nodes[1].WinPoint;
                        break;
                    }
            }
            return requiredPoint; 
        }
        private bool ExParamsDifferent(List<double> newParams)
        {
            bool changed = false;
            if (_exParams == null)
            {
                changed = true;
            }
            else
            {
                for(int i = 0; i<newParams.Count; i++)
                {
                    if (newParams[i] != _exParams[i])
                    {
                        changed = true; 
                    }
                }
            }
            _exParams = new List<double>(newParams);
            return changed; 
        }
        public void UpdateSource()
        {
            Point leftBottomW = GetCorner(CornerType.BottomLeft);
            Point rightTopW = GetCorner(CornerType.TopRight);
            Point leftBottomG = _drawingSurface.WinToGlobal(leftBottomW);
            Point rightTopG = _drawingSurface.WinToGlobal(rightTopW);
            _rastrSource.LoadExtent(leftBottomG, rightTopG, new Rect(leftBottomW, rightTopW));
            if (_rastrSource.LoadSuccess)
            {
                NeedsUpdate = false;
            }
            Render();
        }
        #endregion Методы

        #region Обработчики
        void ResizeThumb_ObjectSizeChanged(object sender, ObjectSizeChangedEventArgs e)
        {
            if (_drawingSurface.CanvasMode == CanvasMode.Map)
            {
                Point ptG1 = Nodes[0].GlobalPoint;
                Point ptG2 = Nodes[1].GlobalPoint;
                double xmin = ptG1.X;
                double ymin = ptG1.Y;
                double xmax = ptG2.X;
                double ymax = ptG2.Y;
                double mainObjectWidth = _drawingSurface.GetLength(new Point(xmin, ymin), new Point(xmax, ymin), Convert.ToInt32(Program.mainFrm1.axMapLIb1.SRID));
                double mainObjectHeight = _drawingSurface.GetLength(new Point(xmax, ymax), new Point(xmax, ymin), Convert.ToInt32(Program.mainFrm1.axMapLIb1.SRID));
                int rastrWidthPx = (int)e.WinRectangle.Width;
                int rastrHeightPx = (int)e.WinRectangle.Height;
                double scaleRateX = mainObjectWidth / (rastrWidthPx * 0.0254 / _rastrSource.Dpi);
                double scaleRateY = mainObjectHeight / (rastrHeightPx * 0.0254 / _rastrSource.Dpi);
                if (scaleRateX > scaleRateY)
                {
                    _drawingSurface.ProjectionZoom = mainObjectWidth / rastrWidthPx;
                }
                else
                {
                    _drawingSurface.ProjectionZoom = mainObjectHeight / rastrHeightPx;
                }
                Nodes[0].WinPoint = e.TopLeftWindow;
                Nodes[1].WinPoint = new Point(
                        e.TopLeftWindow.X + e.WinRectangle.Width,
                        e.TopLeftWindow.Y + e.WinRectangle.Height
                    );
                Nodes[0].GlobalPoint = _drawingSurface.WinToGlobal(Nodes[0].WinPoint);
                Nodes[1].GlobalPoint = _drawingSurface.WinToGlobal(Nodes[1].WinPoint);

                Vector v = ptG1 - Nodes[0].GlobalPoint;
                Nodes[0].GlobalPoint = Nodes[0].GlobalPoint + v;
                Nodes[1].GlobalPoint = Nodes[1].GlobalPoint + v;
                _drawingSurface.ProjectionWPos = _drawingSurface.ProjectionWPos + v;

                Point leftBottomG = new Point(Nodes[0].GlobalPoint.X, Nodes[1].GlobalPoint.Y);
                Point rightTopG = new Point(Nodes[1].GlobalPoint.X, Nodes[0].GlobalPoint.Y);
                _rastrSource.LoadExtent(leftBottomG, rightTopG, e.WinRectangle);
                Render();
            }
            else
            {
                _rastrSource.LoadExtent(e.LeftBottomGlobal, e.RightTopGlobal, e.WinRectangle);
                Nodes[0].WinPoint = e.TopLeftWindow;
                Nodes[0].GlobalPoint = new Point(e.LeftBottomGlobal.X, e.RightTopGlobal.Y);
                Nodes[1].WinPoint = new Point(
                        e.TopLeftWindow.X + e.WinRectangle.Width,
                        e.TopLeftWindow.Y + e.WinRectangle.Height
                    );
                Nodes[1].GlobalPoint = new Point(e.RightTopGlobal.X, e.LeftBottomGlobal.Y);
            }
            Render();
        }
        void ScrollParent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Render();            
        }
        void _drawingSurface_MapModeChanges(object sender, EventArgs e)
        {
            NeedsUpdate = true;
        }
        #endregion Обработчики
    }
}