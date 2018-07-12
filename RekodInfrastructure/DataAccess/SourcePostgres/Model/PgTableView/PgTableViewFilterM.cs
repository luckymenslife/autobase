using Rekod.SQLiteSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgTV_VM = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using Rekod.Controllers;
using Rekod.Classes;

namespace Rekod.DataAccess.SourcePostgres.Model.PgTableView
{
    public class PgTableViewFilterM : ViewModelBase, IDataErrorInfo, IPgTableViewFilterM
    {
        #region Поля
        private PgTableViewFilterType _type = PgTableViewFilterType.Filter;
        PgTV_VM.PgTableViewFilterVM _source;
        PgTableViewFiltersM _parent;

        AbsM.IFieldM _field;
        TypeOperation _tOperation = TypeOperation.Equal;
        object _value;

        private AbsM.EFieldType _fieldType;
        private IEnumerable<NameValue> _collOperation;

        private bool _useOwnValue;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Тип фильтра
        /// </summary>
        public PgTableViewFilterType Type
        {
            get { return _type; }
        }
        /// <summary>
        /// Родитель
        /// </summary>
        public PgTableViewFiltersM Parent
        {
            get { return _parent; }
        }
        /// <summary>
        /// Источник Фильтра 
        /// </summary>
        public PgTV_VM.PgTableViewFilterVM Source
        {
            get { return _source; }
        }
        /// <summary>
        /// Фильтр по колонке
        /// </summary>
        public AbsM.IFieldM Field
        {
            get { return _field; }
            set
            {
                OnPropertyChanged(ref _field, value, () => this.Field);

                var pgField = _field as PgM.PgFieldM;
                AbsM.EFieldType type;
                if (!UseOwnValue && pgField != null && pgField.RefType != AbsM.ERefType.None)
                    type = AbsM.EFieldType.Text;
                else
                    type = _field.Type;
                IEnumerable<NameValue> collOperation;
                if (type == AbsM.EFieldType.Text)
                    collOperation = Source.CollOperationText;
                else
                    collOperation = Source.CollOperationValue;

                OnPropertyChanged(ref _fieldType, type, () => this.FieldType);
                OnPropertyChanged(ref _collOperation, collOperation, () => this.CollOperation);
                OnPropertyChanged(() => this.HasError);
            }
        }
        /// <summary>
        /// Тип опрации фильтра
        /// </summary>
        public TypeOperation TOperation
        {
            get { return _tOperation; }
            set 
            {
                OnPropertyChanged(ref _tOperation, value, () => this.TOperation);
                OnPropertyChanged(() => this.HasError);
            }
        }
        /// <summary>
        /// Значение фильтра
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                OnPropertyChanged(ref _value, value, () => this.Value);
                OnPropertyChanged(() => this.HasError);
            }
        }

        public IEnumerable<NameValue> CollOperation
        {
            get { return _collOperation; }
        }
        /// <summary>
        /// Тип поля с учетом вязей
        /// </summary>
        public AbsM.EFieldType FieldType
        {
            get { return _fieldType; }
        }
        /// <summary>
        /// Есть ли ошибка в фильтре
        /// </summary>
        public bool HasError
        {
            get
            {
                if (TOperation != TypeOperation.Empty &&
                    TOperation != TypeOperation.NotEmpty &&
                        (Value == null || Value.ToString() == "" || ContainsForbidden(Value.ToString())) && 
                        !UseOwnValue)
                    return true;
                return (!string.IsNullOrEmpty(this[null]));
            }
        }
        /// <summary>
        /// Не изменяемый фильтр
        /// </summary>
        public bool UseOwnValue
        {
            get { return _useOwnValue; }
        }
        #endregion

        #region Конструктор
        /// <summary>
        /// Последующие элементы фильтра
        /// </summary>
        /// <param name="parent"></param>
        public PgTableViewFilterM(PgTableViewFiltersM parent, bool isFixed)
        {
            _parent = parent;
            _source = parent.Source;
            Field = _source.Fields[0];
            _useOwnValue = isFixed;
        }

        /// <summary>
        /// Последующие элементы фильтра
        /// </summary>
        /// <param name="source"></param>
        public PgTableViewFilterM(PgTV_VM.PgTableViewFilterVM source)
        {
            _source = source; 
            Field = _source.Fields[0];
            _useOwnValue = true;
        }
        #endregion // Конструктор
       
        #region Индексаторы
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "TOperation" || string.IsNullOrEmpty(propertyName))
                {
                    if (Value != null &&
                            TOperation != TypeOperation.Empty &&
                            TOperation != TypeOperation.NotEmpty)
                    {
                        bool isExist = false;
                        if (FieldType == AbsM.EFieldType.Text)
                            foreach (var item in Source.CollOperationText)
                            {
                                if (item.Value.Equals(TOperation))
                                {
                                    isExist = true;
                                    continue;
                                }
                            }
                        else
                            foreach (var item in Source.CollOperationValue)
                            {
                                if (item.Value.Equals(TOperation))
                                {
                                    isExist = true;
                                    continue;
                                }
                            }
                        if (!isExist)
                            return "Не указан тип операции";
                    }
                    else
                    {
                        return null;
                    }
                }
                if (propertyName == "Value" || string.IsNullOrEmpty(propertyName))
                {
                    bool isError = false;
                    object value = null;
                    if (Value == null && UseOwnValue)
                        return null;
                    switch (FieldType)
                    {
                        case AbsM.EFieldType.Integer:
                            {
                                value = ExtraFunctions.Converts.To<int?>(Value);
                                if (value == null && Value != null)
                                    return "Неправильное число";
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
        #endregion Индексаторы

        #region Методы
        public bool CheckHasError()
        {
            OnPropertyChanged(() => this.Value);
            OnPropertyChanged(() => this.TOperation);
            return HasError;
        }
        internal static object GetValue(PgTableViewFilterM item)
        {
            switch (item.FieldType)
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
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        public bool ContainsForbidden(String value)
        {
            value = value.ToUpper(); 
            if (value.Contains("INSERT")
                || value.Contains("UPDATE")
                || value.Contains("DELETE")
                || value.Contains("DROP"))
            {
                return true;
            }
            else 
            {
                return false; 
            }
        }
        #endregion Методы
    }
}