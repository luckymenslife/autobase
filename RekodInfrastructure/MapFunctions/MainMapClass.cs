using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AxmvMapLib;
using mvMapLib;

namespace Rekod.MapFunctions
{
    /// <summary>
    /// Пока класс не действует
    /// Основой класс работы с картой( класс обертка над Маплиибом)
    /// Должет содержать:
    ///     методы загрузки карты
    ///     смена поддложки
    ///     текущий выделенный слой
    ///     список слоев в маплибе
    ///     и другая информация по маполибу
    /// </summary>
    class MainMapClass : Rekod.Classes.Singleton<MainMapClass>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private mvLayer _selectLayer;
        private ObservableCollection<mvVectorObject> _selectObjects;

        private AxMapLIb _mv
        { get { return Program.mainFrm1.axMapLIb1; } }

        private MainMapClass()
        {
            _mv.ObjectSelected += _mv_ObjectSelected;
            _mv.ObjectUnselected += _mv_ObjectUnselected;
        }


        void _mv_ObjectSelected(object sender, IMapLIbEvents_ObjectSelectedEvent e)
        {
            _selectLayer = e.layer;
            if (this._selectLayer == null || e.layer.NAME == this._selectLayer.NAME)
            {
                ReloadSelectedList(e.layer);
            }
        }
        void _mv_ObjectUnselected(object sender, IMapLIbEvents_ObjectUnselectedEvent e)
        {
            if (this._selectLayer == null || e.layer.NAME == this._selectLayer.NAME)
            {
                ReloadSelectedList(e.layer);
            }
        }

        private void ReloadSelectedList(mvLayer layer)
        {
            //TODO: надо работать через порядковые номера
            this._selectLayer = layer;
            this._selectObjects.Clear();
            var _ids = layer.getSelectedOrderNums();
            for (int i = 0; _ids.count > i; i++)
            {
                this._selectObjects.Add(layer.getObjectByNum(_ids.getElem(i)));
            }
        }
        #region INotifyPropertyChanged Members
        // Пример использования: OnPropertyChanged(ref _defaultSet, value, () => this.DefaultSet);
        internal void OnPropertyChanged<T>(ref T Value, T newValue, Expression<Func<T>> action)
        {
            if (Value == null && newValue == null)
                return;
            if (Value != null && Value.Equals(newValue))
                return;
            Value = newValue;
            OnPropertyChanged(GetPropertyName(action));
        }
        public void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            OnPropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
