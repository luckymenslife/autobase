using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.MapLayersItemInfo.Model
{
    public class AttributeInfo : ViewModelBase, IAttributeM
    {
        #region Поля
        private object _value = "";
        private IFieldM _field;
        private string _text;
        private bool _isReadOnly;
        #endregion Поля

        #region Конструкторы
        public AttributeInfo(IFieldM field, object value)
        {
            _field = field;
            _value = value;
        }
        #endregion Конструкторы

        #region Свойства
        public IFieldM Field
        {
            get { return _field; }
        }
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        #endregion Свойства
    }
}