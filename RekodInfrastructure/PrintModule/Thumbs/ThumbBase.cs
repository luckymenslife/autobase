using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.PrintModule.LayersObjectModel;
using System.Windows.Media;
using System.Windows.Input;
using Rekod.PrintModule.RenderComponents;

namespace Rekod.PrintModule.Thumbs
{
    public abstract class ThumbBase: DrawingVisual
    {
        #region Поля
        private LayerObjectBase _associatedObject;
        private DrawingCanvas _drawingSurface; 
        #endregion Поля

        #region Свойства
        public LayerObjectBase AssociatedObject
        {
            get { return _associatedObject; }
            set { _associatedObject = value; }
        }
        public DrawingCanvas DrawingSurface
        {
            get { return _drawingSurface; }
        }
        #endregion Свойства

        #region Конструкторы
        public ThumbBase(LayerObjectBase associatedobject)
        {
            _associatedObject = associatedobject;
            _drawingSurface = associatedobject.ParentLayer.DrawingSurface; 
        }
        #endregion Конструкторы

        #region Методы
        public abstract void Render();
        public virtual void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        public virtual void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
        }
        public virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
        }
        #endregion Методы
    }
}