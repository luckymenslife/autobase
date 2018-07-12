using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;

namespace Rekod.PrintModule.LayersObjectModel
{
    public abstract class LayerObjectBase: DrawingVisual, INotifyPropertyChanged
    {
        #region Поля
        private List<Node> _nodes = null;
        private LayerBase _parentLayer = null;
        #endregion Поля

        #region Свойства
        public LayerBase ParentLayer
        {
            get { return _parentLayer; }
            set { _parentLayer = value; }
        }
        #endregion Свойства

        #region Конструкторы
        public LayerObjectBase(LayerBase parentlayer)
        {
            _parentLayer = parentlayer;
        }
        #endregion Конструкторы

        #region Коллекции
        public List<Node> Nodes
        {
            get { return _nodes ?? (_nodes = new List<Node>()); }
        }
        #endregion Коллекции

        #region Методы
        public abstract void Render();
        public void OnPropertyChanged(String propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }
        public virtual Point GetCorner(CornerType cornertype)
        {
            return new Point(0, 0); 
        }
        #endregion Методы

        #region События
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion События
    }

    public enum CornerType
    {
        TopLeft, TopRight, BottomLeft, BottomRight
    }
}