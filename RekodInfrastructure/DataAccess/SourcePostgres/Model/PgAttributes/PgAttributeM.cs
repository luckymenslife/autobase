using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Rekod.Controllers;
using System.ComponentModel;

namespace Rekod.DataAccess.SourcePostgres.Model.PgAttributes
{
    [DebuggerDisplay("PgAttributesM Field: {Field._table.SchemeName}.{Field._table.Name}.{Field.Name}, Value: {Value}")]
    public class PgAttributeM : ViewModelBase, AbsM.IAttributeM, IDataErrorInfo
    {
        #region Поля
        private readonly PgAtM.IPgAttributesVM _source;
        private readonly PgM.PgFieldM _field;
        private object _value;
        private readonly ObservableCollection<PgAttributeVariantM> _variants;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Ссылка на источник работы с атрибутами объекта
        /// </summary>
        public PgAtM.IPgAttributesVM Source
        {
            get { return _source; }
        }
        /// <summary>
        /// Идентификатор поля
        /// </summary>
        public AbsM.IFieldM Field
        {
            get { return _field; }
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
        { get { return (Source.IsReadOnly || Field.IsReadOnly); } }
        /// <summary>
        /// Есть ли ошибка в атрибуте
        /// </summary>
        public bool HasError
        {
            get { return CheckHasError(); }
        }
        #endregion // Свойства

        #region Коллекции
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<PgAttributeVariantM> Variants
        { get { return _variants; } }
        #endregion // Коллекци

        #region Конструктор
        public PgAttributeM(PgAtM.IPgAttributesVM source, PgM.PgFieldM field, object value = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (field == null)
                throw new ArgumentNullException("field");
            _source = source;
            _field = field;
            _value = value;
            if ((Field as PgM.PgFieldM).RefTable != null)
                _variants = new ObservableCollection<PgAttributeVariantM>();
        }
        #endregion // Конструктор

        #region Методы
        /// <summary>
        /// Принудительно заполняет поле значением
        /// </summary>
        /// <param name="item">Атрибут</param>
        /// <param name="value">Новое значение</param>
        internal static void SetValue(PgAttributeM item, object value)
        {
            item.OnPropertyChanged(ref item._value, value, () => item.Value);
        }
        internal static object GetValue(PgAttributeM item)
        {
            switch (item.Field.Type)
            {
                case AbsM.EFieldType.Integer:
                    {
                        return ExtraFunctions.Converts.To<int?>(item.Value);
                    }
                case AbsM.EFieldType.Text:
                    {
                        return ExtraFunctions.Converts.To<string>(item.Value);
                    }
                case AbsM.EFieldType.Date:
                case AbsM.EFieldType.DateTime:
                    {
                        return ExtraFunctions.Converts.To<DateTime?>(item.Value);
                    }
                case AbsM.EFieldType.Real:
                    {
                        return ExtraFunctions.Converts.To<double?>(item.Value);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
        public override int GetHashCode()
        {
            return _source.GetHashCode() ^ _field.GetHashCode();
        }
        public bool CheckHasError()
        {
            return (this["Value"] != null);
        }
        public string Error
        {
            get { return null; }
        }
        #endregion Методы

        #region Индексатор
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "Value")
                {
                    bool isError = false;
                    object value = null;
                    switch (Field.Type)
                    {
                        case AbsM.EFieldType.Integer:
                            {
                                value = ExtraFunctions.Converts.To<int?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильно число";
                            }
                            break;
                        case AbsM.EFieldType.Text:
                            break;
                        case AbsM.EFieldType.Date:
                        case AbsM.EFieldType.DateTime:
                            {
                                value = ExtraFunctions.Converts.To<DateTime?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильно введена дата";
                            }
                            break;
                        case AbsM.EFieldType.Real:
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
        #endregion Индексатор
    }
}