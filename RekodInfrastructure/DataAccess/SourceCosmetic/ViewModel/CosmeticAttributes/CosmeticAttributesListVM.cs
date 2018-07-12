using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using PgMAtt = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rekod.Behaviors;
using mvMapLib;
using System.Windows;
using System.Windows.Controls;

namespace Rekod.DataAccess.SourceCosmetic.ViewModel.CosmeticAttributes
{
    public class CosmeticAttributesListVM : ViewModelBase
    {
        #region Поля
        private CosmeticAttributesVM _attributeVM;
        private bool _isDebug;
        private readonly CosM.CosmeticTableBaseM _table;

        private bool _isNew;

        private readonly CosM.CosmeticAttributes.CosmeticAttributeM _pkAttribute;
        private readonly CosM.CosmeticAttributes.CosmeticAttributeM _geomAttribute;
        private readonly ObservableCollection<CosM.CosmeticAttributes.CosmeticAttributeM> _attributes;

        private ICommand _reloadCommand;
        private ICommand _saveCommand;
        private ICommand _clearValueInFieldCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// ViewModel окна атрибутов объекта
        /// </summary>
        public CosmeticAttributesVM AttributeVM
        {
            get { return _attributeVM; }
        }
        /// <summary>
        /// Таблица
        /// </summary>
        public CosM.CosmeticTableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// Идентификатор новой записи
        /// </summary>
        public bool IsNew
        { get { return _isNew; } }
        /// <summary>
        /// Идентификатор объекта с атрибутами
        /// </summary>
        public CosM.CosmeticAttributes.CosmeticAttributeM PkAttribute
        { get { return _pkAttribute; } }
        /// <summary>
        /// Геометрия объекта
        /// </summary>
        public CosM.CosmeticAttributes.CosmeticAttributeM GeomAttribute
        { get { return _geomAttribute; } }
        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion Свойства

        #region Коллекции
        /// <summary>
        /// Коллекция загруженных данных
        /// </summary>
        public ObservableCollection<CosM.CosmeticAttributes.CosmeticAttributeM> Attributes
        { get { return _attributes; } }
        #endregion Коллекции

        #region Конструкторы
        /// <summary>
        /// Работы с атрибутами объекта
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="mapObj">Объекта слоя с атрибутами.</param>
        /// <exception cref="ArgumentNullException"/>
        public CosmeticAttributesListVM(CosmeticAttributesVM attributeVM)
        {
            _attributeVM = attributeVM;

            _isDebug = attributeVM.IsDebug;
            _table = attributeVM.Table;

            _geomAttribute = new CosM.CosmeticAttributes.CosmeticAttributeM((CosM.CosmeticFieldM)_table.Fields.First(p => p.Name == _table.GeomFieldName));
            _pkAttribute = new CosM.CosmeticAttributes.CosmeticAttributeM((CosM.CosmeticFieldM)_table.PrimaryKeyField);

            if (_attributeVM.TableObject != null)
            {
                int id = _attributeVM.TableObject.Id;
                if (id >= 0)
                    _pkAttribute.Value = id;
            }

            _isNew = (_pkAttribute.Value == null);

            _attributes = new ObservableCollection<CosM.CosmeticAttributes.CosmeticAttributeM>();
        }
        #endregion Конструктор

        #region Команды
        #region ReloadCommand
        /// <summary> 
        /// Загружает или обнавляет список атрибутов
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        private bool CanReload(object obj)
        {
            return obj != null && obj is FrameworkElement;
        }

        /// <summary> 
        /// Загружает или обновляет список атрибутов
        /// </summary>
        public void Reload(object param = null)
        {
            var addAttributes = new List<CosM.CosmeticAttributes.CosmeticAttributeM>();
            foreach (CosM.CosmeticFieldM item in _table.Fields)
            {
                if (item.Type != AbsM.EFieldType.Geometry
                        && PkAttribute.Field != item)
                {
                    var attribute = FindAttribute(item);
                    if (attribute == null)
                        attribute = new CosM.CosmeticAttributes.CosmeticAttributeM(item);
                    addAttributes.Add(attribute);
                }
            }
            ExtraFunctions.Sorts.SortList(Attributes, addAttributes);
            var layer = _attributeVM.Source.MapViewer.getLayer(Table.Name);
            if (layer != null)
            {
                for (int t = 0; t < layer.FieldsCount; t++)
                {
                    String fieldName = layer.FieldName(t).ToString();
                    var vmObj = layer.getObject(_attributeVM.TableObject.Id);
                    Object fieldValue = vmObj.fieldValue(fieldName);

                    if (PkAttribute.Field.Name == fieldName)
                    {
                        PkAttribute.Value = fieldValue;
                    }
                    else
                    {
                        var attr = Attributes.FirstOrDefault(a => a.Field.Name == fieldName);
                        if (attr != null)
                        {
                            attr.Value = fieldValue;
                        }
                    }
                }
            }
            CancelValidation(param);
        }
        /// <summary>
        /// Отменить введенные значения
        /// </summary>
        public void CancelValidation(object parameter = null)
        {
            var control = parameter as FrameworkElement;
            if (control != null)
            {
                var bindings = CosmeticDataRepositoryVM.FindBindingGroups(control);
                foreach (var binding in bindings)
                {
                    binding.CancelEdit();
                    binding.BeginEdit();
                }
            }
        }
        #endregion ReloadCommand

        #region SaveCommand
        /// <summary> 
        /// Сохраненяет атрибуты объекта
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        public void Save(object param = null)
        {
            var control = param as FrameworkElement;
            if (control != null)
            {
                var bindings = CosmeticDataRepositoryVM.FindBindingGroups(control);
                for (int i = 0; i < bindings.Count; i++)
                {
                    bindings[i].CommitEdit();
                }
            }
            foreach (var attr in Attributes)
            {
                var layerMV = _attributeVM.Source.MapViewer.getLayer(_table.NameMap);
                if (layerMV == null)
                    continue;
                var objMV = layerMV.getObject(_attributeVM.TableObject.Id);
                if (objMV == null)
                    continue;
                objMV.SetAttribute(attr.Field.Name, (string)attr.Value);
                if (_attributeVM.TableObject != null)
                    _attributeVM.TableObject.SetAttribute(attr.Field.Name, attr.Value);
            }
            // Создание события на обновление атрибута
            //var source = (CosmeticDataRepositoryVM)Table.Source;
            //source.SetEventAttribute(Table, PkAttribute.Value, ...);
            Reload();
        }
        bool CanSave(object param = null)
        {
            return param != null && param is FrameworkElement;
        }
        #endregion SaveCommand

        #region ClearValueInFieldCommand
        public ICommand ClearValueInFieldCommand
        {
            get
            {
                return _clearValueInFieldCommand ?? (_clearValueInFieldCommand
                    = new RelayCommand(this.ClearValueInField, this.CanClearValueInField));
            }
        }
        void ClearValueInField(object param = null)
        {
            if (CanClearValueInField(param))
            {
                CosM.CosmeticAttributes.CosmeticAttributeM attr = null;
                if (param is CommandEventParameter)
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    attr = commEvtParam.CommandParameter as CosM.CosmeticAttributes.CosmeticAttributeM;
                }
                else if (param is CosM.CosmeticAttributes.CosmeticAttributeM)
                {
                    attr = param as CosM.CosmeticAttributes.CosmeticAttributeM;
                }
                attr.Value = null;
            }
        }
        bool CanClearValueInField(object param = null)
        {
            CosM.CosmeticAttributes.CosmeticAttributeM attr = null;
            if (param is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = param as CommandEventParameter;
                attr = commEvtParam.CommandParameter as CosM.CosmeticAttributes.CosmeticAttributeM;
            }
            else if (param is CosM.CosmeticAttributes.CosmeticAttributeM)
            {
                attr = param as CosM.CosmeticAttributes.CosmeticAttributeM;
            }
            return (attr != null && attr.Value != null);
        }
        #endregion ClearValueInFieldCommand

        #region BeginValidationCommand
        private ICommand _beginValidationCommand;
        /// <summary>
        /// Команда для начала валидации
        /// </summary>
        public ICommand BeginValidationCommand
        {
            get { return _beginValidationCommand ?? (_beginValidationCommand = new RelayCommand(this.BeginValidation, this.CanBeginValidation)); }
        }
        /// <summary>
        /// Начало валидации
        /// </summary>
        public void BeginValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                var control = commEventPar.CommandParameter as FrameworkElement;
                if (control != null)
                {
                    var bindings = CosmeticDataRepositoryVM.FindBindingGroups(control);
                    foreach (var binding in bindings)
                    {
                        binding.BeginEdit();
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли начать валидацию
        /// </summary>
        public bool CanBeginValidation(object parameter = null)
        {
            return parameter != null;
        }
        #endregion // BeginValidationCommand

        #region ErrorValidationCommand
        private ICommand _errorValidationCommand;
        /// <summary>
        /// Команда, которая запускается, если при валидации обнаружены ошибки
        /// </summary>
        public ICommand ErrorValidationCommand
        {
            get { return _errorValidationCommand ?? (_errorValidationCommand = new RelayCommand(this.ErrorValidation, this.CanErrorValidation)); }
        }
        /// <summary>
        /// Обработать ошибки валидации
        /// </summary>
        public void ErrorValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;

                ValidationErrorEventArgs e = commEventPar.EventArgs as ValidationErrorEventArgs;
                if (e != null && e.Action == ValidationErrorEventAction.Added)
                {
                    System.Windows.MessageBox.Show(e.Error.ErrorContent.ToString());
                }
            }
        }
        /// <summary>
        /// Можно ли обработать ошибки валидации
        /// </summary>
        public bool CanErrorValidation(object parameter = null)
        {
            return true;
        }
        #endregion ErrorValidationCommand
        #endregion Команды

        #region Методы
        /// <summary>
        /// Находит атрибут по полю таблицы
        /// </summary>
        /// <param name="field">Поле таблицы</param>
        /// <returns>Атрибут</returns>
        public CosM.CosmeticAttributes.CosmeticAttributeM FindAttribute(AbsM.IFieldM field)
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                var item = _attributes[i];
                if (item.Field.Equals(field))
                    return item;
            }
            return null;
        }
        #endregion // Методы
    }
}