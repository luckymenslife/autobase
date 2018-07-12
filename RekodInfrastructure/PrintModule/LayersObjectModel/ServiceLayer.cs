using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.PrintModule.RenderComponents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace Rekod.PrintModule.LayersObjectModel
{
    public class ServiceLayer: LayerBase
    {
        #region Поля
        private Double _width;
        private Double _height;
        private int _horizontalCount = 2;
        private int _verticalCount = 2;
        private int _pageWidth = 297;
        private int _pageHeight = 210;
        private Point _leftMouseButtonClickedPoint;
        private Pen _selectionSquarePen = new Pen(Brushes.Black, 0.5) { DashStyle = DashStyles.Dash };
        private Brush _selectionSquareBrush = Brushes.Transparent;
        private DrawingVisual _selectionSquare;
        #endregion Поля

        #region Конструкторы
        public ServiceLayer(DrawingCanvas drawingSurface)
            : base(drawingSurface)
        {
            this.LayerType = LayersObjectModel.LayerType.Service;
            SetPrintSurface(1, 1, 180, 200);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ServiceLayer_PropertyChanged);
            Render();
        }
        #endregion Конструкторы

        #region Свойства
        public Double Height
        {
            get { return _height; }
            set 
            {
                OnPropertyChanged(ref _height, value, () => this.Height); 
            }
        }
        public Double Width
        {
            get { return _width; }
            set 
            {
                OnPropertyChanged(ref _width, value, () => this.Width); 
            }
        }
        public int PageWidth
        {
            get { return _pageWidth; }
            //set 
            //{
            //    if (_pageWidth == value)
            //    {
            //        return; 
            //    }
            //    _pageWidth = LimitVariable(value, 10, 10000);
            //    OnPropertyChanged("PageWidth");
            //    SetPrintSurface(_horizontalCount, _verticalCount, _pageWidth, _pageHeight);
            //}
        }
        public int PageHeight
        {
            get { return _pageHeight; }
            //set
            //{
            //    if (_pageHeight == value)
            //    {
            //        return;
            //    }
            //    _pageHeight = LimitVariable(value, 10, 10000);
            //    OnPropertyChanged("PageHeight"); 
            //    SetPrintSurface(_horizontalCount, _verticalCount, _pageWidth, _pageHeight);
            //}
        }
        public int HorizontalCount
        {
            get { return _horizontalCount; }
            //set
            //{
            //    if (_horizontalCount == value)
            //    {
            //        return;
            //    }
            //    _horizontalCount = LimitVariable(value, 1, 100);
            //    OnPropertyChanged("HorizontalCount"); 
            //    SetPrintSurface(_horizontalCount, _verticalCount, _pageWidth, _pageHeight);
            //}
        }
        public int VerticalCount
        {
            get { return _verticalCount; }
            //set
            //{
            //    if (_verticalCount == value)
            //    {
            //        return;
            //    }
            //    _verticalCount = LimitVariable(value, 1, 100);
            //    OnPropertyChanged("VerticalCount"); 
            //    SetPrintSurface(_horizontalCount, _verticalCount, _pageWidth, _pageHeight);
            //}
        }
        #endregion Свойства

        #region Методы
        public void SetPrintSurface(int hcount, int vcount, int pagewidth, int pageheight)
        {
            double pixelWidth = pagewidth*DrawingSurface.Dpi/25.4;
            double pixelHeight = pageheight*DrawingSurface.Dpi/25.4;

            _pageHeight = pageheight;
            _pageWidth = pagewidth;
            _horizontalCount = hcount;
            _verticalCount = vcount; 

            foreach (var obj in LayerObjects)
            {
                DrawingSurface.DeleteServiceVisual(obj); 
            }
            LayerObjects.Clear();

            for (int j = 0; j < vcount; j++)
            {
                for (int i = 0; i < hcount; i++)
                {
                    ServiceLayerObject serviceLayerObject = new ServiceLayerObject(this);

                    double topleftx = i * pixelWidth;
                    double toplefty = j * pixelHeight;
                    double bottomrightx = (i + 1) * pixelWidth;
                    double bottomrighty = (j + 1) * pixelHeight; 
                    
                    serviceLayerObject.Nodes.Add(new Node() { WinPoint = new Point(topleftx, toplefty) });
                    serviceLayerObject.Nodes.Add(new Node() { WinPoint = new Point(bottomrightx, bottomrighty) });
                    LayerObjects.Add(serviceLayerObject);

                    DrawingSurface.AddServiceVisual(serviceLayerObject);

                    if (i == hcount - 1 && j != vcount - 1)
                    {
                        serviceLayerObject.ClosureType = ClosureType.Right; 
                    }
                    else if (i != hcount - 1 && j == vcount - 1)
                    {
                        serviceLayerObject.ClosureType = ClosureType.Bottom; 
                    }
                    else if (i == hcount - 1 && j == vcount - 1)
                    {
                        serviceLayerObject.ClosureType = ClosureType.RightBottom; 
                    }
                }
            }
            Width = hcount * pixelWidth;
            Height = vcount * pixelHeight; 
            Render(); 
        }
        public int LimitVariable(int variable, int min, int max)
        {
            int res = variable; 
            if (min <= max)
            {
                if (res < min)
                {
                    res = min;
                }
                if (res > max)
                {
                    res = max;
                }
            }
            return res; 
        }
        private void DrawSelectionSquare(Point point1, Point point2)
        {
            _selectionSquarePen.Thickness = DrawingSurface.GetZoomSize(0.5);
            using (DrawingContext dc = _selectionSquare.RenderOpen())
            {
                dc.DrawRectangle(_selectionSquareBrush, _selectionSquarePen, new Rect(point1, point2));
            }
        }
        #endregion Методы

        #region Обработчики
        void ServiceLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if ((new[] { "PageWidth", "PageHeight", "HorizontalCount", "VerticalCount" }).Contains(e.PropertyName))
            //{
            //    SetPrintSurface(_horizontalCount, _verticalCount, _pageWidth, _pageHeight);
            //}
        }
        internal void OnLeftMouseButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _leftMouseButtonClickedPoint = Mouse.GetPosition(DrawingSurface);
            _selectionSquare = new DrawingVisual();
            DrawingSurface.AddVisual(_selectionSquare);
            foreach (var servObject in LayerObjects)
            {
                (servObject as ServiceLayerObject).IsSelected = false;
                servObject.Render();
            }
        }
        internal void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePoint = Mouse.GetPosition(DrawingSurface);
            if (currentMousePoint == _leftMouseButtonClickedPoint)
            {
                foreach (var servObjectDeselect in LayerObjects)
                {
                    (servObjectDeselect as ServiceLayerObject).IsSelected = false;
                    servObjectDeselect.Render();
                }
            }
            else
            {
                foreach (var servObject in LayerObjects)
                {
                    Rect nodeRect = new Rect(servObject.Nodes[0].WinPoint, servObject.Nodes[1].WinPoint);
                    Rect selectRect = new Rect(currentMousePoint, _leftMouseButtonClickedPoint);
                    if (Rect.Intersect(nodeRect, selectRect) != Rect.Empty)
                    {
                        (servObject as ServiceLayerObject).IsSelected = true;
                        servObject.Render();
                    }
                }
            }
            DrawingSurface.DeleteVisual(_selectionSquare);
        }
        internal void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePoint = Mouse.GetPosition(DrawingSurface);
            DrawSelectionSquare(currentMousePoint, _leftMouseButtonClickedPoint);
        }
        #endregion Обработчики        
    }
}