using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Rekod.PrintModule.RenderComponents;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.PrintModule.LayersObjectModel
{
    public abstract class LayerBase: ViewModelBase
    {
        #region Поля
        private List<LayerObjectBase> _layerObjects = null;
        private DrawingCanvas _drawingSurface;
        #endregion Поля

        #region Свойства
        public DrawingCanvas DrawingSurface
        {
            get { return _drawingSurface; }
        }
        #endregion Свойства

        #region Коллекции
        public List<LayerObjectBase> LayerObjects
        {
            get { return _layerObjects ?? (_layerObjects = new List<LayerObjectBase>()); }
        }
        #endregion Коллекции
   
        #region Конструкторы
        public LayerBase(DrawingCanvas drawingsurface)
        {
            _drawingSurface = drawingsurface; 
        }
        #endregion Конструкторы
   
        #region Свойства
        public LayerType LayerType
        {
            get;
            set;
        }
        #endregion Свойства
   
        #region Методы
        public virtual void Render()
        {
            if (_layerObjects != null)
            {
                foreach (LayerObjectBase layerObjectBase in _layerObjects)
                {
                    layerObjectBase.Render();
                }
            }
        }
        #endregion Методы

        #region События
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion События
    }

    public enum LayerType
    {
        Rastr, Vector, Service
    }
}