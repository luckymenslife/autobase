using Interfaces;
using Rekod.Classes;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourceCosmetic.Model;
using Rekod.DataAccess.SourcePostgres.Model.PgTableView;
using Rekod.SQLiteSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Rekod.DataAccess.SourceCosmetic.ViewModel
{
    public class CosmeticTableViewFilterVM: ViewModelBase
    {
        public static bool FilterHasError(ITableViewFilterM iFilter)
        {
            if (iFilter is CosTableViewFilterM)
            {
                CosTableViewFilterM filter = iFilter as CosTableViewFilterM;
                return filter.HasError;
            }
            else if (iFilter is CosTableViewFiltersM)
            {
                CosTableViewFiltersM container = iFilter as CosTableViewFiltersM;
                bool result = false;
                foreach (ITableViewFilterM innerIFilter in container.Container)
                {
                    result |= FilterHasError(innerIFilter);
                }
                return result;
            }
            else
            {
                return false;
            }
        }
        public static string GetWhere(ITableViewFilterM iFilter, List<IParams> listParams)
        {
            if (iFilter == null)
            {
                return "1 = 1";
            }
            else
            {
                if (iFilter is CosTableViewFilterM && FilterHasError(iFilter))
                {
                    return "1 = 1";
                }
                else if (iFilter is CosTableViewFiltersM)
                {
                    CosTableViewFiltersM container = iFilter as CosTableViewFiltersM;
                    if (container.Container.Count == 0 || FilterHasError(iFilter))
                    {
                        return "1=1";
                    }
                }
            }

            switch (iFilter.Type)
            {
                case TableViewFilterType.Filter:
                    {
                        var filter = iFilter as CosTableViewFilterM;
                        if (filter == null)
                            break;
                        if (filter.HasError)
                        {
                            //if (filter.UseOwnValue)
                            //{
                            //    return null;
                            //}
                            //else
                            {
                                throw new Exception();
                            }
                        }
                        //var param = GetWhereParams(filter, listParams.Count + 1);
                        //listParams.Add(param);
                        //return GetWhereFilter(filter, param);
                        return "";
                    }
                //case PgTVM.PgTableViewFilterType.Container:
                //    {
                //        var filters = iFilter as PgTVM.PgTableViewFiltersM;
                //        if (filters == null)
                //            break;
                //        return GetWhereContainer(filters, listParams);
                //    }
            }
            throw new Exception("Ошибка при выборе фильтра");
        }



        #region Поля
        private CosmeticTableViewVM _source;
        private CosTableViewFiltersM _dynamicFilter;
        private CosTableViewFilterM _fixedFilter;
        private ObservableCollection<ITableViewFiltersM> _filters;
        private ObservableCollection<IFieldM> _fields;
        private ObservableCollection<NameValue> _collRelation;
        private ObservableCollection<NameValue> _collOperationText;
        private ObservableCollection<NameValue> _collOperationValue;
        #endregion Поля

        #region Конструкторы
        public CosmeticTableViewFilterVM(CosmeticTableViewVM source, FilterRelationModel fixedFilterPattern)
        {
            _source = source;
            GetFilterField();
            GetFilterOperation();
            GetFilterRelation();
            _dynamicFilter = new CosTableViewFiltersM(this);
            _filters = new ObservableCollection<ITableViewFiltersM>(new[] { _dynamicFilter });
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<ITableViewFiltersM> Filters
        {
            get { return _filters; }
        }
        public ObservableCollection<IFieldM> Fields
        {
            get { return _fields; }
        }
        public ObservableCollection<NameValue> CollRelation
        {
            get { return _collRelation; }
        }
        public ObservableCollection<NameValue> CollOperationText
        {
            get { return _collOperationText; }
        }
        public ObservableCollection<NameValue> CollOperationValue
        {
            get { return _collOperationValue; }
        }
        #endregion Коллекции

        #region Методы
        private void GetFilterField()
        {
            if (_fields == null)
            {
                _fields = new ObservableCollection<IFieldM>();
                _fields.Add(new PgFieldAllFilterM());
                if (_source.CosTable.Fields != null)
                    foreach (var f in _source.CosTable.Fields.Where(f => f.Type != EFieldType.Geometry))
                    {
                        _fields.Add(f);
                    }
            }
        }
        private void GetFilterOperation()
        {
            var empty = new NameValue(TypeOperation.Empty, "Пустое");
            var notEmpty = new NameValue(TypeOperation.NotEmpty, "Не пустое");
            var Equal = new NameValue(TypeOperation.Equal, "=");
            var NotEqual = new NameValue(TypeOperation.NotEqual, "<>");

            if (_collOperationText == null)
                _collOperationText = new ObservableCollection<NameValue>(
                    new[] 
                    {
                        Equal,
                        NotEqual,
                        new NameValue(TypeOperation.Contains, "Содержит"),
                        new NameValue(TypeOperation.InBegin, "В начале"),
                        new NameValue(TypeOperation.InEnd, "В конце"),
                        empty,
                        notEmpty
                    });
            if (_collOperationValue == null)
                _collOperationValue = new ObservableCollection<NameValue>(
                    new[]
                    {
                        Equal,
                        NotEqual,
                        new NameValue(TypeOperation.More, ">"),
                        new NameValue(TypeOperation.MoreOrEqual, ">="),
                        new NameValue(TypeOperation.Less, "<"),
                        new NameValue(TypeOperation.LessOrEqual, "<="),
                        empty,
                        notEmpty
                    });
        }
        /// <summary>
        /// Обновление списка коллекций логических операций (и, или)
        /// </summary>
        private void GetFilterRelation()
        {
            if (_collRelation == null)
                _collRelation = new ObservableCollection<NameValue>(
                    new[] 
                    {
                        new NameValue(TypeRelation.AND, "И"),
                        new NameValue(TypeRelation.OR, "ИЛИ")
                    });
        }
        #endregion

        #region Команды
        #region RemoveFilterCommand
        private ICommand _removeFilterCommand;
        /// <summary>
        /// Команда для удаления фильтра из коллекции
        /// </summary>
        public ICommand RemoveFilterCommand
        {
            get { return _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand(this.RemoveFilter, this.CanRemoveFilter)); }
        }
        /// <summary>
        /// Удаление фильтра из коллекции
        /// </summary>
        public void RemoveFilter(object parameter = null)
        {
            CosTableViewFilterM filter = parameter as CosTableViewFilterM;
            if(filter != null)
            {
                filter.Parent.Container.Remove(filter);
            }
        }
        /// <summary>
        /// Можно ли удалить фильтр
        /// </summary>
        public bool CanRemoveFilter(object parameter = null)
        {
            return true;
        }
        #endregion RemoveFilterCommand 

        #region AddFilterCommand
        private ICommand _addFilterCommand;
        /// <summary>
        /// Добавляет фильтр в указанный контейнер
        /// </summary>
        public ICommand AddFilterCommand
        {
            get { return _addFilterCommand ?? (_addFilterCommand = new RelayCommand(this.AddFilter, this.CanAddFilter)); }
        }
        /// <summary>
        /// Добавляет фильтр в указанный контейнер
        /// </summary>
        private void AddFilter(object obj = null)
        {
            if (!CanAddFilter(obj))
                return;
            var value = obj as CosTableViewFiltersM;
            value.Container.Add(new CosTableViewFilterM(this, value));
        }
        private bool CanAddFilter(object obj = null)
        {
            var value = obj as CosTableViewFiltersM;
            return (value != null);
        }
        #endregion AddFilterCommand

        #region AddContainerCommand
        private ICommand _addContainerCommand;
        /// <summary>
        /// Добавляет новый контейнер в указанный контейнер
        /// </summary>
        public ICommand AddContainerCommand
        {
            get { return _addContainerCommand ?? (_addContainerCommand = new RelayCommand(this.AddContainer, this.CanAddContainer)); }
        }
        /// <summary>
        /// Добавляет новый контейнер в указанный контейнер
        /// </summary>
        private void AddContainer(object obj = null)
        {
            if (!CanAddContainer(obj))
                return;
            var value = obj as CosTableViewFiltersM;
            value.Container.Add(new CosTableViewFiltersM(this, value));
        }
        private bool CanAddContainer(object obj = null)
        {
            var value = obj as CosTableViewFiltersM;
            return (value != null);
        }
        #endregion AddFilterCommand
        #endregion Команды
    }
}