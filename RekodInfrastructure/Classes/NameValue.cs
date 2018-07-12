using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Classes
{
    public class NameValue
    {
        #region Поля
        object _value;
        string _name;
        #endregion Поля

        #region Свойства
        /// <summary>
        /// Значение поля
        /// </summary>
        public object Value
        {
            get { return _value; }
        }
        /// <summary>
        /// Название поля для отображения
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        #endregion Свойства

        #region Конструктор
        public NameValue(object value, string name)
        {
            _value = value;
            _name = name;
        }
        #endregion Конструктор
    }
}