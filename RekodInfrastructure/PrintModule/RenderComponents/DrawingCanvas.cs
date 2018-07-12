using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rekod.PrintModule.LayersObjectModel;
using System.ComponentModel;
using Rekod.PrintModule.Thumbs;
using Rekod.PrintModule.Service;
using Rekod.Services;
using System.Timers;

namespace Rekod.PrintModule.RenderComponents
{
    public class DrawingCanvas : Canvas, INotifyPropertyChanged
    {
        #region Статические свойства
        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(DrawingCanvas), 
            new PropertyMetadata(0.5, new PropertyChangedCallback(DrawingCanvas_ZoomChanged)));
        private static void DrawingCanvas_ZoomChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs e)
        {
            DrawingCanvas drawingSurface = depobj as DrawingCanvas;
            if (drawingSurface.ZoomChanged != null)
            {
                drawingSurface.ZoomChanged(depobj, new EventArgs());
            }
        }

        // Using a DependencyProperty as the backing store for ZoomMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomMaxProperty =
            DependencyProperty.Register("ZoomMax", typeof(double), typeof(DrawingCanvas), new PropertyMetadata(10.0));

        // Using a DependencyProperty as the backing store for ZoomMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomMinProperty =
            DependencyProperty.Register("ZoomMin", typeof(double), typeof(DrawingCanvas), new PropertyMetadata(0.01));


        public Window ParentScroll
        {
            get { return (Window)GetValue(ParentScrollProperty); }
            set { SetValue(ParentScrollProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParentScroll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentScrollProperty =
            DependencyProperty.Register("ParentScroll", typeof(Window), typeof(DrawingCanvas), new PropertyMetadata(null, new PropertyChangedCallback(ParentScroll_Changed)));

        private static void ParentScroll_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //ScrollViewer scrollViewer = sender as ScrollViewer;
            DrawingCanvas drawingCanvas = sender as DrawingCanvas;
            drawingCanvas.AttachHandlers();
        }
        #endregion // Статические свойства

        #region Поля
        private double _zoomStep = 1.1;
        private CanvasMode _canvasMode; 
        private Mode _mode; 
        private ObservableCollection<LayerBase> _layers;
        private double _scale = 1;
        private Rect _winRect;
        private VisualCollection _visuals;
        private List<Visual> _serviceVisuals;
        private double _projectionZoom;
        private Point _projectionWPos;
        private bool _leftMouseButtonPressed = false;
        private Point _leftMouseButtonClickedPoint;
        private Point _pointExMouseOver;
        private ThumbBase _activeThumb;
        private double _dpi = 96;
        private ScrollViewer _scrollParent;
        private double _projZoomScaleRel = 0;
        private bool _selectedContextVisible;
        private bool _allowMoving = true;
        #endregion

        #region Коллекции
        public ObservableCollection<LayerBase> Layers
        {
            get { return _layers ?? (_layers = new ObservableCollection<LayerBase>()); }
        }
        public VisualCollection Visuals
        {
            get { return _visuals; }
        }
        #endregion Коллекции

        #region Свойства
        public double ProjZoomScaleRel
        {
            get { return _projZoomScaleRel; }
        }
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set 
            {
                SetValue(ZoomProperty, value);
            }
        }
        public double ZoomMax
        {
            get { return (double)GetValue(ZoomMaxProperty); }
            set { SetValue(ZoomMaxProperty, value); }
        }
        public double ZoomMin
        {
            get { return (double)GetValue(ZoomMinProperty); }
            set { SetValue(ZoomMinProperty, value); }
        }
        public double Left
        {
            get { return Margin.Left; }
            set { this.Margin = new Thickness(value, Margin.Top, Margin.Right, Margin.Bottom); }
        }
        public double Top
        {
            get { return this.Margin.Top; }
            set { this.Margin = new Thickness(Margin.Left, value, Margin.Right, Margin.Bottom); }
        }
        public CanvasMode CanvasMode
        {
            get 
            {
                return _canvasMode; 
            }
            set
            {
                if (_canvasMode == value)
                {
                    return;
                }
                _canvasMode = value;
                OnPropertyChanged("CanvasMode");
            }
        }
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value)
                {
                    return;
                }
                _scale = value;
                OnPropertyChanged("Scale");
            }
        }
        public double ProjectionZoom
        {
            get { return _projectionZoom; }
            set
            {
                if (_projectionZoom == value)
                {
                    return;
                }
                _projectionZoom = value;
                OnPropertyChanged("ProjectionZoom"); 
            }
        }
        public Point ProjectionWPos
        {
            get { return _projectionWPos; }
            set
            {
                if (_projectionWPos == value)
                {
                    return;
                }
                _projectionWPos = value;
                OnPropertyChanged("ProjectionWPos");
            }
        }
        public ServiceLayer ServiceLayer
        {
            get;
            private set; 
        }
        public double Dpi
        {
            get { return _dpi; }
            set
            {
                if (_dpi == value)
                {
                    return;
                }
                _dpi = value;
                OnPropertyChanged("ProjectionWPos");
            }
        }
        public ScrollViewer ScrollParent
        {
            get { return _scrollParent ?? (_scrollParent = Service.LogicalTreeHelperService.GetParentOfType<ScrollViewer>(this) as ScrollViewer); }
        }
        public Boolean SelectedContextVisible
        {
            get
            {
                return _selectedContextVisible; 
            }
            private set
            {
                if (_selectedContextVisible == value)
                {
                    return;
                }
                _selectedContextVisible = value;
                OnPropertyChanged("SelectedContextVisible");
            }
        }
        public bool AllowMoving
        {
            get { return _allowMoving; }
            set
            {
                if (_allowMoving == value)
                {
                    return;
                }
                _allowMoving = value;
                OnPropertyChanged("AllowMoving");
            }
        } 
        #endregion // Свойства

        #region Конструктор
        public DrawingCanvas()
        {
            _visuals = new VisualCollection(this);
            _serviceVisuals = new List<Visual>();
            _winRect = new Rect();
            _projectionWPos = new Point();
            _layers = new ObservableCollection<LayerBase>();
            _mode = Mode.CanvasMode;
            CanvasMode = RenderComponents.CanvasMode.Canvas;

            this.MouseWheel += new MouseWheelEventHandler(DrawingCanvas_MouseWheel);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(DrawingCanvas_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DrawingCanvas_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(DrawingCanvas_MouseMove);
            this.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
         
            ServiceLayer = new ServiceLayer(this);
            PropertyChanged += DrawingCanvas_PropertyChanged;
        }
        #endregion

        #region Методы
        public void AttachHandlers()
        {
            ParentScroll.MouseWheel += new MouseWheelEventHandler(DrawingCanvas_MouseWheel);
            ParentScroll.MouseLeftButtonDown += new MouseButtonEventHandler(DrawingCanvas_MouseLeftButtonDown);
            ParentScroll.MouseLeftButtonUp += new MouseButtonEventHandler(DrawingCanvas_MouseLeftButtonUp);
            ParentScroll.MouseMove += new MouseEventHandler(DrawingCanvas_MouseMove);
            ParentScroll.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
        }
        public void SetProjZoomScaleRel(double val)
        {
            _projZoomScaleRel = val;
        }
        private Vector SetCanvasPosition(Size win, Point mousePosition, double zoom, double zoomStep)
        {
            double prevZoom = Zoom / zoomStep;

            double currWidth = win.Width * Zoom;
            double currHeight = win.Height * Zoom;

            double newWidth = win.Width * prevZoom;
            double newHeight = win.Height * prevZoom;

            double xTransFactor = mousePosition.X / win.Width;
            double yTransFactor = mousePosition.Y / win.Height;

            double changeTop = (currHeight - newHeight) * yTransFactor;
            double changeLeft = (currWidth - newWidth) * xTransFactor;

            //var changeTop = increase
            //    ?   mousePosition.X /_zoomStep
            //    : mousePosition.X * _zoomStep;

            //var changeLeft = increase
            //    ? mousePosition.Y / _zoomStep
            //    : mousePosition.Y * _zoomStep;
            return new Vector(-changeLeft, -changeTop);
        }
        public void RegisterLayer(LayerBase layer)
        {
            Layers.Add(layer);
            //this.AddVisual(layer);
            //layer.WinRect = new Rect(new Size(this.Width, this.Height));
        }
        public void Render()
        {
            _winRect.Width = this.Width;
            _winRect.Height = this.Height;
            using (var disp = Dispatcher.DisableProcessing())
            {
                foreach (var item in Layers)
                {
                    item.Render(); 
                }
            }
        }
        protected override int VisualChildrenCount
        {
            get
            {
                { return _visuals.Count; }
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }
        public void AddVisual(Visual visual)
        {
            if (!_visuals.Contains(visual))
            {
                foreach (var servVis in _serviceVisuals)
                {
                    _visuals.Remove(servVis);
                }
                _visuals.Add(visual);
                foreach (var servVis in _serviceVisuals)
                {
                    _visuals.Add(servVis); 
                }
            }
        }
        public void DeleteVisual(Visual visual)
        {
            _visuals.Remove(visual);
        }
        public void AddServiceVisual(Visual visual)
        {
            if (!_serviceVisuals.Contains(visual))
            {
                _serviceVisuals.Add(visual);
                _visuals.Add(visual); 
            }
        }
        public void DeleteServiceVisual(Visual visual)
        {
            _serviceVisuals.Remove(visual);
            _visuals.Remove(visual); 
        }
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            if (hitResult == null)
            {
                return null;
            }
            return hitResult.VisualHit as DrawingVisual;
        }
        public DrawingVisual GetVisual<T>(Point point)
        {
            EllipseGeometry geometry = new EllipseGeometry(point, 0.01, 0.01);
            List<DrawingVisual> visuals = GetVisuals<T>(geometry);
            if (visuals.Count > 0)
            {
                return visuals[0];
            }
            else
            {
                return null;
            }
        }
        private List<DrawingVisual> hits = new List<DrawingVisual>();
        public List<DrawingVisual> GetVisuals<T>(Geometry region)
        {
            hits.Clear();
            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = new HitTestResultCallback(this.HitTestCallback<T>);
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }
        private HitTestResultBehavior HitTestCallback<T>(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            DrawingVisual visual = result.VisualHit as DrawingVisual;
            if (visual is T)
            {
                hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }
        public void OnPropertyChanged(String propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname)); 
            }
        }
        /// <summary>
        /// Транслирует мировые координаты в оконные
        /// </summary>
        /// <param name="globalpoint">Точка в мировых координатах</param>
        /// <param name="projectionwpos">Позиция левого верхнего угла окна в мировых коордиатах</param>
        /// <param name="projectionzoom">Масштаб карты в заданной проекции</param>
        /// <returns></returns>
        public Point GlobalToWin(Point globalpoint, Point projectionwpos, double projectionzoom)
        {
            double xGlobDelta = globalpoint.X - projectionwpos.X;
            double yGlobDelta = globalpoint.Y - projectionwpos.Y;
            return new Point(xGlobDelta / projectionzoom, -yGlobDelta / projectionzoom);
        }
        public Point GlobalToWin(Point globalpoint)
        {
            return GlobalToWin(globalpoint, ProjectionWPos, ProjectionZoom); 
        }
        public Point WinToGlobal(Point winpoint, Point projectionwpos, double projectionzoom)
        {
            double xGlob = projectionwpos.X + winpoint.X * projectionzoom;
            double yGlob = -projectionwpos.Y + winpoint.Y * projectionzoom;
            return new Point(xGlob, -yGlob);
        }
        public Point WinToGlobal(Point winpoint)
        {
            return WinToGlobal(winpoint, ProjectionWPos, ProjectionZoom); 
        }
        public void RecalculateWindowCoordinates(bool fixRastr = false)
        {
            foreach (var layer in Layers)
            {
                if (layer.LayerType != LayerType.Service)
                {
                    if (!fixRastr || layer.LayerType != LayerType.Rastr)
                    {
                        foreach (var layerobject in layer.LayerObjects)
                        {
                            foreach (var node in layerobject.Nodes)
                            {
                                Point newWinPoint = GlobalToWin(node.GlobalPoint, _projectionWPos, _projectionZoom);
                                node.WinPoint = newWinPoint;
                            }
                        }
                    }
                    else
                    {
                        foreach (var layerobject in layer.LayerObjects)
                        {
                            foreach (var node in layerobject.Nodes)
                            {
                                Point newGlobPoint = WinToGlobal(node.WinPoint, _projectionWPos, _projectionZoom);
                                node.GlobalPoint = newGlobPoint;
                            }
                        }
                    }
                }
            }
            Render();
        }
        public void RecalculateProjParams(Point topLeftW, Rect winRect, Point leftTopG, Point rightBottomG)
        {
            _projectionWPos = leftTopG;

            double xmin = leftTopG.X;
            double ymin = leftTopG.Y;
            double xmax = rightBottomG.X;
            double ymax = rightBottomG.Y;

            double _mainObjectWidth = GetLength(new Point(xmin, ymin), new Point(xmax, ymin), Convert.ToInt32(Program.mainFrm1.axMapLIb1.SRID));
            double _mainObjectHeight = GetLength(new Point(xmax, ymax), new Point(xmax, ymin), Convert.ToInt32(Program.mainFrm1.axMapLIb1.SRID));
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            double scaleRateX = _mainObjectWidth / (winRect.Width * 0.0254 / gr.DpiX);
            double scaleRateY = _mainObjectHeight / (winRect.Height * 0.0254 / gr.DpiY);
            if (scaleRateX > scaleRateY)
            {
                ProjectionZoom = _mainObjectWidth / winRect.Width;
            }
            else
            {
                ProjectionZoom = _mainObjectHeight / winRect.Height;
            }
        }
        // Возвращает размер с поправкой на зум
        public double GetZoomSize(double realsize)
        {
            return realsize / Zoom; 
        }
        public double GetLength(Point point1, Point point2, int srid, bool usegeography = false)
        {
            double x1 = point1.X;
            double y1 = point1.Y;

            double x2 = point2.X;
            double y2 = point2.Y;

            String wkt =
                String.Format("LINESTRING ({0} {1}, {2} {3})",
                                x1.ToString().Replace(',', '.'),
                                y1.ToString().Replace(',', '.'),
                                x2.ToString().Replace(',', '.'),
                                y2.ToString().Replace(',', '.'));

            using (SqlWork sqlWork = new SqlWork())
            {
                if (!usegeography)
                {
                    sqlWork.sql =
                        String.Format(@"SELECT st_length(st_geomfromtext('{0}', {1})) as width",
                                        wkt,
                                        srid);
                }
                else
                {
                    sqlWork.sql =
                        String.Format(@"SELECT ST_Length(ST_GeographyFromText('SRID=4326;'
                                      ||(SELECT st_astext(st_transform(st_geomfromtext('{0}', {1}), 4326)))))",
                                        wkt,
                                        srid);
                }
                return sqlWork.ExecuteScalar<Double>();
            }
        }
        #endregion Методы

        #region События
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> ZoomChanged;
        public event EventHandler<EventArgs> MapModeChanges; 
        #endregion События

        #region Обработчики
        void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _leftMouseButtonPressed = true;
            _leftMouseButtonClickedPoint = Mouse.GetPosition(this);
            Visual underVisual = GetVisual<ThumbBase>(_leftMouseButtonClickedPoint);
            
            if (underVisual is ThumbBase)
            {
                _mode = Mode.ThumbMode;
                _activeThumb = underVisual as ThumbBase;
                _activeThumb.OnLeftMouseButtonDown(sender, e);
                CaptureMouse();
            }
            else
            {
                _mode = Mode.CanvasMode;
                if (_canvasMode == CanvasMode.Select)
                {
                    this.ServiceLayer.OnLeftMouseButtonDown(sender, e);
                }
                CaptureMouse();
            }            
        }
        void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (_mode)
            {
                case Mode.ThumbMode:
                    {
                        _activeThumb.OnLeftMouseButtonUp(sender, e);
                        ReleaseMouseCapture(); 
                        break;
                    }
                case Mode.CanvasMode:
                    {
                        switch (_canvasMode)
                        {
                            case CanvasMode.Canvas:
                                {
                                    break;
                                }
                            case CanvasMode.Map:
                                {
                                    if (_leftMouseButtonPressed)
                                    {
                                        (Layers[0].LayerObjects[0] as RastrLayerObject).UpdateSource();
                                    }
                                    break;
                                }
                            case CanvasMode.Select:
                                {
                                    this.ServiceLayer.OnLeftMouseButtonUp(sender, e);
                                    break;
                                }
                        }
                        ReleaseMouseCapture(); 
                        break; 
                    }

            }
            _leftMouseButtonPressed = false; 
        }
        void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pointOver = Mouse.GetPosition(this);
            switch (_mode)
            {
                case Mode.ThumbMode:
                    {
                        _activeThumb.OnMouseMove(sender, e); 
                        break;
                    }
                case Mode.CanvasMode:
                    {
                        if (_leftMouseButtonPressed)
                        {
                            switch (_canvasMode)
                            {
                                case CanvasMode.Canvas:
                                    {
                                        if (_allowMoving)
                                        {
                                            Vector offset = Point.Subtract(pointOver, _pointExMouseOver);
                                            Point winDelta = new Point(offset.X, offset.Y);
                                            Point globDelta = WinToGlobal(winDelta, new Point(0, 0), ProjectionZoom);
                                            ProjectionWPos = new Point(ProjectionWPos.X - globDelta.X, ProjectionWPos.Y - globDelta.Y);
                                            RecalculateWindowCoordinates();
                                            _pointExMouseOver = pointOver;
                                        }
                                        break; 
                                    }
                                case CanvasMode.Map:
                                    {
                                        Vector offset = Point.Subtract(pointOver, _pointExMouseOver);
                                        Point winDelta = new Point(offset.X, offset.Y);
                                        Point globDelta = WinToGlobal(winDelta, new Point(0, 0), ProjectionZoom);
                                        ProjectionWPos = new Point(ProjectionWPos.X - globDelta.X, ProjectionWPos.Y - globDelta.Y);
                                        _pointExMouseOver = pointOver;
                                        if (MapModeChanges != null)
                                        {
                                            MapModeChanges(null, null);
                                        }
                                        RecalculateWindowCoordinates(true);
                                        Render();
                                        break;
                                    }
                                case CanvasMode.Select:
                                    {
                                        this.ServiceLayer.OnMouseMove(sender, e);
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
            _pointExMouseOver = pointOver;
        }
        void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; 
            Point mouseCurrentPoint = Mouse.GetPosition(this); 
            bool increase = e.Delta > 0;
            var winSize = new Size(this.Width, this.Height);
            switch (_canvasMode)
            {
                case RenderComponents.CanvasMode.Select:
                case RenderComponents.CanvasMode.Canvas:
                    {
                        if (increase && Zoom >= ZoomMax || !increase && Zoom <= ZoomMin)
                        {
                            return;
                        }
                        var zoomStep = increase
                            ? _zoomStep
                            : 1 / _zoomStep;

                        Zoom *= zoomStep;
                        InvalidateMeasure();
                        break;
                    }
                case RenderComponents.CanvasMode.Map:
                    {
                        RastrLayerObject rastrObject = Layers[0].LayerObjects[0] as RastrLayerObject;
                        Point mousePos = Mouse.GetPosition(this);
                        Double newProjZoom = (e.Delta > 0) ? ProjectionZoom / 1.1 : ProjectionZoom * 1.1;
                        Point gptOld = WinToGlobal(mousePos);
                        Point gptNew = WinToGlobal(mousePos, ProjectionWPos, newProjZoom);                        
                        Vector zoomOffset = gptNew - gptOld;
                        ProjectionZoom = newProjZoom;
                        ProjectionWPos = ProjectionWPos - zoomOffset;
                        if (MapModeChanges != null)
                        {
                            MapModeChanges(null, null);
                        }
                        rastrObject.UpdateSource();
                        foreach (Node node in rastrObject.Nodes)
                        {
                            node.GlobalPoint = WinToGlobal(node.WinPoint);
                        }
                        Render();                    
                        break;
                    }
                default:
                    break;
            }
        }
        void DrawingCanvas_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProjectionZoom")
            {
                if (_projZoomScaleRel != 0)
                {
                    Scale = Math.Round(ProjectionZoom / _projZoomScaleRel);
                }
            }
        }
        void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ServiceLayerObject underVisual = GetVisual<ServiceLayerObject>(Mouse.GetPosition(this)) as ServiceLayerObject;
            if (underVisual != null)
            {
                SelectedContextVisible = underVisual.IsSelected;
            }
        }
        #endregion Обработчики
    }
    public enum Mode
    {
        /// <summary>
        /// Действие передается объекту Thumb
        /// </summary>
        ThumbMode,
        /// <summary>
        /// Действие определяет DrawingCanvas
        /// </summary>
        CanvasMode,
        /// <summary>
        /// Действие определяет служебный слой
        /// </summary>
        ServiceMode
    }
    public enum CanvasMode
    {
        None = 0,
        /// <summary>
        /// Перемещение полотна, масштабирование полотна
        /// </summary>
        Canvas = 1,
        /// <summary>
        /// Перемещение карты, масштабирование карты
        /// </summary>
        Map = 2, 
        /// <summary>
        /// Выбор страниц
        /// </summary>
        Select = 3
    }
}