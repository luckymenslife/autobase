using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Rekod.PrintModule.RenderComponents;


namespace Rekod.PrintModule.LayersObjectModel
{
    class RastrLayer: LayerBase
    {
        #region Конструкторы
        public RastrLayer(DrawingCanvas drawingsurface): base(drawingsurface)
        {
            this.LayerType = LayersObjectModel.LayerType.Rastr;
        }
        #endregion Конструкторы
    }
}