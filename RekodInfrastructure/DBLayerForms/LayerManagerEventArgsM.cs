using System;
using System.Collections.Generic;
using System.Text;

namespace Rekod
{
    public class LayerManagerEventArgsM : EventArgs
    {
        #region Поля
        private string name1;
        private bool edited1;
        private bool visible1;
        private bool selected1;
        private int id1;
        #endregion // Поля

        #region Свойства
        public int LayerId
        {
            get { return id1; }
        }
        public string LayerName
        {
            get { return name1; }
        }

        public bool LayerEdited
        {
            get { return edited1; }
        }
        public bool LayerVisible
        {
            get { return visible1; }
        }
        public bool LayerSelected
        {
            get { return selected1; }
        }
        #endregion // Свойства

        #region Конструктор
        public LayerManagerEventArgsM(string name, bool edited, bool visible, bool selected, int id)
        {
            name1 = name;
            edited1 = edited;
            visible1 = visible;
            selected1 = selected;
            id1 = id;
        }
        #endregion // Конструктор
    }
}
