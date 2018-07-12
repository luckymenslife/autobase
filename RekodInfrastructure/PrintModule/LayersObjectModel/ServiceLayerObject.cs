using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using Rekod.PrintModule.RenderComponents;

namespace Rekod.PrintModule.LayersObjectModel
{
    public class ServiceLayerObject : LayerObjectBase
    {
        #region Поля
        private DrawingCanvas _drawingSurface;
        private Pen _transparentPen = new Pen(Brushes.Transparent, 0);
        private Pen _drawingPen;
        private Pen _selectedPen;
        private Brush _selectedBrush;
        #endregion Поля

        #region Свойства
        public Pen DrawingPen
        {
            get { return _drawingPen ?? (_drawingPen = new Pen(Brushes.Black, _drawingSurface.GetZoomSize(0.5))); }
        }
        public Pen SelectedPen
        {
            get { return _selectedPen ?? (_selectedPen = new Pen(Brushes.Blue, _drawingSurface.GetZoomSize(1))); }
        }
        public Brush SelectedBrush
        {
            get { return _selectedBrush ?? (_selectedBrush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255))); }
        }
        public ClosureType ClosureType
        {
            get;
            set; 
        }
        public Boolean IsSelected
        {
            get;
            set;
        }
        #endregion Свойства

        #region Конструкторы
        public ServiceLayerObject(LayerBase parentlayer): base(parentlayer)
        {
            _drawingSurface = parentlayer.DrawingSurface;
            _drawingSurface.ZoomChanged += _drawingSurface_ZoomChanged;
        }
        #endregion Конструкторы

        #region Методы
        public override void Render()
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                double x = Nodes[0].WinPoint.X;
                double y = Nodes[0].WinPoint.Y;
                double width = Nodes[1].WinPoint.X - Nodes[0].WinPoint.X;
                double height = Nodes[1].WinPoint.Y - Nodes[0].WinPoint.Y; 

                Rect rect = new Rect(x, y, width, height);
                dc.DrawRectangle(Brushes.Transparent, _transparentPen, rect);
                if (IsSelected)
                {
                    Rect selRect = new Rect(x+1, y+1, width-2, height-2);
                    dc.DrawRectangle(SelectedBrush, _transparentPen, selRect);
                }

                DrawingPen.Thickness = _drawingSurface.GetZoomSize(0.5);
                dc.DrawLine(DrawingPen, new Point(x, y), new Point(x, y + height));
                dc.DrawLine(DrawingPen, new Point(x, y), new Point(x + width, y));

                switch (this.ClosureType)
                {
                    case ClosureType.Bottom:
                        {
                            dc.DrawLine(DrawingPen, new Point(x, y + height), new Point(x + width, y + height));
                            break;
                        }
                    case ClosureType.Right:
                        {
                            dc.DrawLine(DrawingPen, new Point(x + width, y), new Point(x + width, y + height));
                            break;
                        }
                    case ClosureType.RightBottom:
                        {
                            dc.DrawLine(DrawingPen, new Point(x, y + height), new Point(x + width, y + height));
                            dc.DrawLine(DrawingPen, new Point(x + width, y), new Point(x + width, y + height));
                            break;
                        }
                }
            }
        }
        #endregion Методы

        #region Обработчики
        void _drawingSurface_ZoomChanged(object sender, EventArgs e)
        {
            Render();
        }
        #endregion Обработчики
    }
    public enum ClosureType
    {
        None, Bottom, Right, RightBottom
    }
}