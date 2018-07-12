using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourcePostgres.Model.PgAttributes;

namespace Rekod.DataAccess.SourceCosmetic.Model.CosmeticAttributes
{
    public class CosmeticAttributeM : ViewModelBase, IDataErrorInfo, IAttributeM
    {
        private object _value;
        private IFieldM _field;

        /// <summary>
        /// Соответствующее поле таблицы
        /// </summary>
        public IFieldM Field
        {
            get { return _field; }
            set { _field = value; }
        }

        /// <summary>
        /// Значение поля
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                if (!IsReadOnly)
                {
                    OnPropertyChanged(ref _value, value, () => this.Value);
                    OnPropertyChanged(() => this.HasError);
                }
            }
        }
        /// <summary>
        /// Атрибут только для чтения
        /// </summary>
        public bool IsReadOnly
        { get { return (Field.IsReadOnly); } }
        /// <summary>
        /// Есть ли ошибка в атрибуте
        /// </summary>
        public bool HasError
        {
            get { return CheckHasError(); }
        }

        public bool CheckHasError()
        {
            return (this["Value"] != null);
        }
        public string Error
        {
            get { return null; }
        }
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "Value")
                {
                    object value = null;
                    switch (Field.Type)
                    {
                        case EFieldType.Integer:
                            {
                                value = ExtraFunctions.Converts.To<int?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильно число";
                            }
                            break;
                        case EFieldType.Text:
                            break;
                        case EFieldType.Date:
                        case EFieldType.DateTime:
                            {
                                value = ExtraFunctions.Converts.To<DateTime?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильно введена дата";
                            }
                            break;
                        case EFieldType.Real:
                            {
                                value = ExtraFunctions.Converts.To<double?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильно число";
                            }
                            break;

                    }
                }
                return null;
            }
        }
        
        #region Конструктор
        public CosmeticAttributeM(IFieldM field, object value = null)
        {
            if (field == null)
                throw new ArgumentNullException("field is null");
            _field = field;
            _value = value;
        }
        #endregion Конструктор
    }
}