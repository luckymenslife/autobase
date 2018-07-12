using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using Rekod.PrintModule.LayersObjectModel;
using System.Windows.Input; 

namespace Rekod.PrintModule.Thumbs
{
    public class ResizeThumb: ThumbBase
    {
        #region Поля
        private DrawingVisual _resizeRect;
        private Pen _resizeRectPen; 
        #endregion Поля

        #region Свойства
        public CornerType CornerType
        {
            get;
            set;
        }
        public Pen ResizeRectPen
        {
            get 
            {
                _resizeRectPen = new Pen(Brushes.Gray, DrawingSurface.GetZoomSize(0.7)) 
                    {
                        DashStyle = DashStyles.Dash
                    };
                return _resizeRectPen;
            }
        }
        #endregion Свойства

        #region Конструкторы
        public ResizeThumb(LayerObjectBase associatedobject)
            : base(associatedobject)
        {
            DrawingSurface.ZoomChanged += DrawingSurface_ZoomChanged;
        }
        #endregion Конструкторы

        #region Методы
        public override void Render()
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                if (AssociatedObject != null)
                {
                    Point requiredPoint = AssociatedObject.GetCorner(this.CornerType); 
                    dc.DrawEllipse(Brushes.Gray, new Pen(Brushes.Gray, 1), requiredPoint, 
                        DrawingSurface.GetZoomSize(7),
                        DrawingSurface.GetZoomSize(7)); 
                }
            }
        }
        public override void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _resizeRect = new DrawingVisual();
            DrawingSurface.AddVisual(_resizeRect);
            DrawingSurface.CaptureMouse(); 
        }
        public override void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            DrawingSurface.DeleteVisual(_resizeRect);
            DrawingSurface.ReleaseMouseCapture();

            if (ObjectSizeChanged != null)
            {
                CornerType cornerOrigin = OppositeCorner(CornerType); 
                Point firstPointW = AssociatedObject.GetCorner(cornerOrigin); 
                Point secondPointW = Mouse.GetPosition(DrawingSurface);

                Point leftBottomW = new Point(
                    firstPointW.X < secondPointW.X?firstPointW.X:secondPointW.X, 
                    firstPointW.Y > secondPointW.Y?firstPointW.Y:secondPointW.Y);
                Point rightTopW = new Point(
                    firstPointW.X >= secondPointW.X ? firstPointW.X : secondPointW.X,
                    firstPointW.Y <= secondPointW.Y ? firstPointW.Y : secondPointW.Y);

                Point leftBottomG = AssociatedObject.ParentLayer.DrawingSurface.WinToGlobal(leftBottomW);
                Point rightTopG = AssociatedObject.ParentLayer.DrawingSurface.WinToGlobal(rightTopW); 
                ObjectSizeChanged(this, new ObjectSizeChangedEventArgs()
                {
                    WinRectangle = new Rect(leftBottomW, rightTopW),
                    LeftBottomGlobal = leftBottomG, 
                    RightTopGlobal = rightTopG,
                    TopLeftWindow = new Point(leftBottomW.X, rightTopW.Y)
                }); 
            }
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            using (DrawingContext dc = _resizeRect.RenderOpen())
            {
                CornerType cornerOrigin = OppositeCorner(CornerType);
                dc.DrawRectangle(
                    Brushes.Transparent,
                    ResizeRectPen,
                    new Rect(
                        AssociatedObject.GetCorner(cornerOrigin),
                        Mouse.GetPosition(DrawingSurface)));
            }
        }
        public CornerType OppositeCorner(CornerType cornerType)
        {
            CornerType cornerOpposite = LayersObjectModel.CornerType.TopLeft;
            switch (cornerType)
            {
                case CornerType.TopLeft:
                    cornerOpposite = CornerType.BottomRight;
                    break;
                case CornerType.TopRight:
                    cornerOpposite = CornerType.BottomLeft;
                    break;
                case CornerType.BottomLeft:
                    cornerOpposite = CornerType.TopRight;
                    break;
                case CornerType.BottomRight:
                    cornerOpposite = CornerType.TopLeft;
                    break;
            }
            return cornerOpposite; 
        }
        #endregion Методы

        #region Обработчики
        void DrawingSurface_ZoomChanged(object sender, EventArgs e)
        {
            Render(); 
        }
        #endregion Обработчики

        #region События
        public event EventHandler<ObjectSizeChangedEventArgs> ObjectSizeChanged;
        #endregion События
    }

    public class ObjectSizeChangedEventArgs: EventArgs
    {
        #region Свойства
        public Rect WinRectangle
        {
            get;
            set;
        }
        public Point LeftBottomGlobal
        {
            get;
            set; 
        }
        public Point RightTopGlobal
        {
            get;
            set; 
        }
        public Point TopLeftWindow
        {
            get;
            set; 
        }
        #endregion Свойства
    }
}