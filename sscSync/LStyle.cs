using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using RESTLib.Model.REST.LayerStyle;
using sscSync.ViewModel;

namespace sscSync
{
    public class LStyle : ViewModelBase
    {
        private RESTLib.Enums.RESTStyles _type;
        private Color _fillColor;

        private bool _hasStroke;
        private int _strokeWidth;
        private Color _strokeColor;

        private double _lineWidth;

        private int _pointSize;
        private Figure _pointFigure;

        private double _polygonOpacity;

        public RESTLib.Enums.RESTStyles Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }

        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; OnPropertyChanged("FillColor"); }
        }

        public bool HasStroke
        {
            get { return _hasStroke; }
            set { _hasStroke = value; OnPropertyChanged("HasStroke"); }
        }

        public int StrokeWidth
        {
            get { return _strokeWidth; }
            set { _strokeWidth = value; OnPropertyChanged("StrokeWidth"); }
        }

        public Color StrokeColor
        {
            get { return _strokeColor; }
            set { _strokeColor = value; OnPropertyChanged("StrokeColor"); }
        }

        public double LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = value; OnPropertyChanged("LineWidth"); }
        }

        public int PointSize
        {
            get { return _pointSize; }
            set { _pointSize = value; OnPropertyChanged("PointSize"); }
        }

        public Figure PointFigure
        {
            get { return _pointFigure; }
            set { _pointFigure = value; OnPropertyChanged("PointFigure"); }
        }

        public double PolygonOpacity
        {
            get { return _polygonOpacity; }
            set { _polygonOpacity = value; OnPropertyChanged("PolygonOpacity"); }
        }

        public LayerStyle RESTLayerStyle
        {
            get
            {
                var fc = System.Drawing.Color.FromArgb(FillColor.A, FillColor.R, FillColor.G, FillColor.B);
                var sc = System.Drawing.Color.FromArgb(StrokeColor.A, StrokeColor.R, StrokeColor.G, StrokeColor.B);
                switch (Type)
                {
                    case RESTLib.Enums.RESTStyles.Point:
                        return new LayerStyle(fc, HasStroke, StrokeWidth, sc, PointSize)
                        {
                            PointFigure = (int)PointFigure
                        };
                    case RESTLib.Enums.RESTStyles.Line:
                        return new LayerStyle(fc, HasStroke, StrokeWidth, sc, LineWidth);
                    case RESTLib.Enums.RESTStyles.Polygon:
                        return new LayerStyle(fc, Math.Round(PolygonOpacity / 100d, 1), HasStroke, StrokeWidth, sc);
                }
                return null;
            }
        }

        public bool IsValid 
        {
            get
            {
                switch (Type)
                {
                    case RESTLib.Enums.RESTStyles.Point:
                        return (!HasStroke || StrokeWidth > 0) && PointSize > 0 && PointFigure != 0;
                    case RESTLib.Enums.RESTStyles.Line:
                        return (!HasStroke || StrokeWidth > 0) && LineWidth > 0;
                    case RESTLib.Enums.RESTStyles.Polygon:
                        return (!HasStroke || StrokeWidth > 0);
                }
                return false;
            }
        }
    }

    public enum Figure
    {
        Неизвестный = 0,
        Круг = 1,
        Квадрат = 2,
        Треугольник = 3
    }
}