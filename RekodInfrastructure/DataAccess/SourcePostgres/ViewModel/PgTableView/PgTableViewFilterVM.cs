using Rekod.DataAccess.AbstractSource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Pg_M = Rekod.DataAccess.SourcePostgres.Model;
using PgTVM = Rekod.DataAccess.SourcePostgres.Model.PgTableView;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Interfaces;
using System.Data;
using Npgsql;
using Rekod.DataAccess.AbstractSource.ViewModel;
using mvMapLib;
using System.Diagnostics;
using System.Windows;
using Rekod.Controllers;
using System.Data.SQLite;
using Rekod.Repository;
using Rekod.SQLiteSettings;
using Rekod.Services;
using Rekod.Repository.SettingsDB;
using Rekod.Classes;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView
{
    public class PgTableViewFilterVM
    {
        #region Поля

        private PgTVM.PgTableViewFiltersM _dynamicFilter;
        private PgTVM.PgTableViewFilterM _fixedFilter;
        private ObservableCollection<PgTVM.PgTableViewFiltersM> _filters;

        private ICommand _addFilterCommand;
        private ICommand _addContainerCommand;
        private ICommand _removeFilterCommand;
        private ICommand _saveCommand;
        private ICommand _removeFiltersDBCommand;
        private ICommand _loadFilterCommand;
        private ICommand _applyOnMapCommand;
        private PgTableViewVM _source;

        private ObservableCollection<AbsM.IFieldM> _fields;
        private ObservableCollection<NameValue> _collRelation;
        private ObservableCollection<NameValue> _collOperationText;
        private ObservableCollection<NameValue> _collOperationValue;
        private ObservableCollection<NameValue> _idNameFilter;

        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Коллекция фильтров с одним фильтром - используется для привязки
        /// </summary>
        public ObservableCollection<PgTVM.PgTableViewFiltersM> Filters
        {
            get { return _filters; }
        }
        /// <summary>
        /// Коллекция полей для фильтра
        /// </summary>
        public ObservableCollection<AbsM.IFieldM> Fields
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
        public ObservableCollection<NameValue> IdNameFilter
        {
            get { return _idNameFilter ?? (_idNameFilter = new ObservableCollection<NameValue>()); }
        }
        public PgTableViewVM Source
        {
            get { return _source; }
        }
        public PgTVM.PgTableViewFiltersM DynamicFilter
        {
            get { return _dynamicFilter; }
        }
        public PgTVM.PgTableViewFilterM FixedFilter
        {
            get { return _fixedFilter; }
        }
        #endregion // Свойства

        #region Конструктор
        /// <summary>
        /// Конструктор менеджера фильтров. 
        /// </summary>
        /// <param name="source">Источник, в котором хранится экземпляр класса</param>
        /// <param name="fixedFilterPattern">Паттерн фиксированного фильтра</param>
        public PgTableViewFilterVM(PgTableViewVM source, FilterRelationModel fixedFilterPattern)
        {
            this._source = source;
            GetFilterField();
            GetFilterRelation();
            GetFilterOperation();
            _dynamicFilter = new PgTVM.PgTableViewFiltersM(this);
            if (fixedFilterPattern != null)
            {
                _fixedFilter = new PgTVM.PgTableViewFilterM(this);
                SetFilterFromPattern(_fixedFilter, fixedFilterPattern);
            }
            _filters = new ObservableCollection<PgTVM.PgTableViewFiltersM>(new[] { _dynamicFilter });
            ReloadFiltersFromSqLite();
        }
        #endregion Конструктор

        #region Команды
        #region AddFilterCommand
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
            var value = obj as PgTVM.PgTableViewFiltersM;
            value.Container.Add(new PgTVM.PgTableViewFilterM(value, false));
            value.OnPropertyChanged(() => value.QueryString);
        }
        private bool CanAddFilter(object obj = null)
        {
            var value = obj as PgTVM.PgTableViewFiltersM;
            return (value != null);
        }
        #endregion // AddFilterCommand

        #region AddContainerCommand
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
            var value = obj as PgTVM.PgTableViewFiltersM;
            value.Container.Add(new PgTVM.PgTableViewFiltersM(value));
            value.OnPropertyChanged(() => value.QueryString);
        }
        private bool CanAddContainer(object obj = null)
        {
            var value = obj as PgTVM.PgTableViewFiltersM;
            return (value != null);
        }
        #endregion // AddFilterCommand

        #region RemoveFilterCommand
        /// <summary>
        /// Удаляет выбранный фильтр
        /// </summary>
        public ICommand RemoveFilterCommand
        {
            get { return _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand(this.RemoveFilter, this.CanRemoveFilter)); }
        }
        /// <summary>
        /// Удаляет выбранный фильтр
        /// </summary>
        private void RemoveFilter(object obj = null)
        {
            if (!CanRemoveFilter(obj))
                return;
            var value = obj as PgTVM.IPgTableViewFilterM;
            var container = value as PgTVM.PgTableViewFiltersM;
            if (container != null)
            {
                for (; container.Container.Count > 0; )
                {
                    RemoveFilter(container.Container[0]);
                }
                container.OnPropertyChanged(() => container.QueryString);
            }
            if (value.Parent != null)
            {
                value.Parent.Container.Remove(value);
                if (value.Parent.Container.Count == 0)
                    RemoveFilter(value.Parent);
            }
        }
        private bool CanRemoveFilter(object obj = null)
        {
            var value = obj as PgTVM.IPgTableViewFilterM;
            return (value != null);
        }
        #endregion // AddFilterCommand

        #region SaveCommand
        /// <summary>
        /// Команда для сохранения активного фильтра
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        /// <summary>
        /// Сохранение активного фильтра
        /// </summary>
        public void Save(object parameter = null)
        {
            FilterSaveFrm frm = new FilterSaveFrm();
            frm.Text = "Фильтр";
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int idSource = 0;
                int idTable = 0;

                Npgsql.NpgsqlConnectionStringBuilder connect = (_source.Table.Source as PgDataRepositoryVM).Connect;
                String sourceName = String.Format("{0}@{1}", connect.Database, connect.Host);
                String sourceType = _source.Table.Source.Type.ToString();
                Pg_M.PgTableBaseM pgTable = _source.Table as Pg_M.PgTableBaseM;
                String tableName = String.Format("\"{0}\".\"{1}\"", pgTable.SchemeName, pgTable.Name);

                bool sourceExists = (from Source src
                                        in Program.SettingsDB.Sources
                                        where src.SourceName == sourceName && src.SourceType == sourceType
                                        select src).Count() > 0;
                if (!sourceExists)
                {
                    Program.SettingsDB.Sources.AddObject(new Source() { SourceName = sourceName, SourceType = sourceType });
                }
                idSource = Convert.ToInt32((from Source src
                               in Program.SettingsDB.Sources
                                            where src.SourceName == sourceName && src.SourceType == sourceType
                                            select src.Id).FirstOrDefault());
                bool tableExists = (from Table tbl 
                                        in Program.SettingsDB.Tables
                                        where tbl.SourceId == idSource && tbl.TableName == tableName
                                        select tbl).Count() > 0;
                if (!tableExists)
                {
                    Program.SettingsDB.Tables.AddObject(new Table() { TableName = tableName, SourceId = idSource });
                }
                idTable = Convert.ToInt32((from Table tbl
                              in Program.SettingsDB.Tables
                                            where tbl.TableName == tableName && tbl.SourceId == idSource
                                            select tbl.Id).FirstOrDefault());

                FilterRelationModel filterRM = GetFilterModel(_dynamicFilter);
                Program.SettingsDB.Filters.AddObject(new Filter()
                        {
                            FilterName = frm.textBox1.Text,
                            WhereText = SqlJsonParser.ToJson(filterRM),
                            IdTable = idTable
                        });
                Program.SettingsDB.SaveChanges();
                ReloadFiltersFromSqLite();
            }
        }
        /// <summary>
        /// Можно ли сохранить активный фильтр
        /// </summary>
        public bool CanSave(object parameter = null)
        {
            return !DynamicFilter.HasError;
        }
        #endregion // SaveCommand

        #region RemoveFiltersDBCommand
        /// <summary>
        /// Команда для удаления выбранных фильтров из базы SqLite
        /// </summary>
        public ICommand RemoveFiltersDBCommand
        {
            get { return _removeFiltersDBCommand ?? (_removeFiltersDBCommand = new RelayCommand(this.RemoveFiltersDB, this.CanRemoveFiltersDB)); }
        }
        /// <summary>
        /// Удаление выбранных фильтров из базы SqLite
        /// </summary>
        public void RemoveFiltersDB(object parameter = null)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id фильтра", typeof(int)));
            dt.Columns.Add(new DataColumn("Название фильтра", typeof(string)));
            foreach (NameValue filterNameId in IdNameFilter)
            {
                DataRow dr = dt.NewRow();
                dr["Id фильтра"] = filterNameId.Value;
                dr["Название фильтра"] = filterNameId.Name;
                dt.Rows.Add(dr);
            }

            Window checkWindow = new Window();
            Rekod.Controls.CheckObjects checkObjects = new Rekod.Controls.CheckObjects(dt, null, "Id фильтра", checkWindow);
            checkWindow.Title = "Фильтр";
            checkWindow.Content = checkObjects;
            checkWindow.Owner = Program.WinMain;
            checkWindow.Height = 400;
            checkWindow.Width = 750;

            checkWindow.ShowDialog();
            if (checkObjects.Cancelled == false && checkObjects.CheckedList.Count > 0)
            {
                var filtersToDelete = from Filter f
                                            in Program.SettingsDB.Filters
                                            where checkObjects.CheckedList.Contains((int)f.Id)
                                            select f; 
                foreach(var filterToDelete in filtersToDelete)
                {
                    Program.SettingsDB.Filters.DeleteObject(filterToDelete);
                }                
                Program.SettingsDB.SaveChanges();
                ReloadFiltersFromSqLite();
            }
        }
        /// <summary>
        /// Можно ли удалить фильтры
        /// </summary>
        public bool CanRemoveFiltersDB(object parameter = null)
        {
            return true;
        }
        #endregion // RemoveFiltersDBCommand

        #region LoadFilterCommand
        /// <summary>
        /// Команда которая загружает активный фильтр из SQLite
        /// </summary>
        public ICommand LoadFilterCommand
        {
            get { return _loadFilterCommand ?? (_loadFilterCommand = new RelayCommand(this.LoadFilter, this.CanLoadFilter)); }
        }
        /// <summary>
        /// Загрузка активного фильтра из SQLite
        /// </summary>
        public void LoadFilter(object parameter = null)
        {
            long filterId = Convert.ToInt64(parameter);
            foreach (var filter in (from Filter f 
                                        in Program.SettingsDB.Filters
                                        where f.Id == filterId 
                                        select f))
            {
                FilterRelationModel pattern = SqlJsonParser.FromJson<FilterRelationModel>(filter.WhereText);
                SetFilterFromPattern(_dynamicFilter, pattern);
                _dynamicFilter.OnPropertyChanged("QueryString");
            }
        }
        /// <summary>
        /// Можно ли загрузить фильтр
        /// </summary>
        public bool CanLoadFilter(object parameter = null)
        {
            return true;
        }
        #endregion // LoadFilterCommand

        #region ApplyOnMapCommand
        /// <summary>
        /// Команда для применения фильтра на карте
        /// </summary>
        public ICommand ApplyOnMapCommand
        {
            get { return _applyOnMapCommand ?? (_applyOnMapCommand = new RelayCommand(this.ApplyOnMap, this.CanApplyOnMap)); }
        }
        /// <summary>
        /// Применить фильтр на карте
        /// </summary>
        public void ApplyOnMap(object parameter = null)
        {
            AxmvMapLib.AxMapLIb axMapLib = Program.mainFrm1.axMapLIb1;
            Pg_M.PgTableBaseM pgTable = _source.Table as Pg_M.PgTableBaseM;

            mvLayer layer = axMapLib.getLayer(pgTable.NameMap);
            if (layer != null)
            {
                List<IParams> listParams = new List<IParams>();
                String query = GetWhere(DynamicFilter, listParams);
                // todo: (Dias) На текущий момент axMapLib в строке фильтра не использует параметры.
                // Поэтому приходится парсить параметры. Нужно чтобы карта поддерживала параметры в фильтрах
                foreach(IParams param in listParams)
                {
                    if (param.type == DbType.Date
                        || param.type == DbType.DateTime
                        || param.type == DbType.String)
                    {
                        query = query.Replace(param.paramName, String.Format("'{0}'", param.value)); 
                    }
                    else
                    {
                        query = query.Replace(param.paramName, String.Format("{0}", param.value));
                    }
                }
                layer.Filter = query;
            }
        }
        /// <summary>
        /// Можно ли применить фильтр на карте
        /// </summary>
        public bool CanApplyOnMap(object parameter = null)
        {
            return !DynamicFilter.HasError;
        }
        #endregion // ApplyOnMapCommand
        #endregion // Команды

        #region Комментарии
        //        private PgTVM.PgTableViewFiltersM GetNewFilter(FilterRelationModel filterElement, PgTVM.PgTableViewFiltersM filter)
        //        {
        //            PgTVM.PgTableViewFiltersM tmpFilter;
        //            if (filterElement != null)
        //            {
        //                tmpFilter = new PgTVM.PgTableViewFiltersM(this);
        //                tmpFilter.TRelation = TypeRelation.AND;
        //                SetFilterW(tmpFilter, filterElement, true);
        //                tmpFilter.Container.Add(filter);
        //            }
        //            else
        //            {
        //                tmpFilter = filter;
        //            }
        //            return tmpFilter;
        //        }

        //        /// <summary>
        //        /// Метод для установки фильтра из шаблона
        //        /// </summary>
        //        /// <param name="filter"></param>
        //        internal void SetFilter(FilterRelationModel filter)
        //        {
        //            RemoveFilter(_filter);
        //            SetFilterW(_filter, filter, false);
        //        }

        //        /// <summary>
        //        /// Создает структуру фильтров из шаблона
        //        /// </summary>
        //        /// <param name="filterM">Фильтр</param>
        //        /// <param name="filter">Шаблон</param>
        //        private static void SetFilterW(PgTVM.PgTableViewFiltersM filterM, FilterRelationModel pattern, bool isFixed)
        //        {
        //            switch (pattern.Type)
        //            {
        //                case TypeRelation.AND:
        //                case TypeRelation.OR:
        //                    var newFilter = new PgTVM.PgTableViewFiltersM(filterM)
        //                    {
        //                        TRelation = pattern.Type
        //                    };
        //                    filterM.Container.Add(newFilter);
        //                    for (int i = 0; i < pattern.Arguments.Count; i++)
        //                    {
        //                        SetFilterW(newFilter, pattern.Arguments[i], isFixed);
        //                    }
        //                    break;
        //                case TypeRelation.ELEMENT:
        //                    var element = pattern.Element;
        //                    GetElementFilter(filterM, element, isFixed);
        //                    break;
        //            }
        //        }
        //        private static void GetElementFilter(PgTVM.PgTableViewFiltersM filterM, FilterElementModel element, bool isFixed)
        //        {
        //            var newFilter2 = new PgTVM.PgTableViewFilterM(filterM, isFixed)
        //            {
        //                Field = filterM.Source.FindField(element.Column),
        //                TOpetation = element.Type,
        //                Value = element.Value
        //            };
        //            filterM.Container.Add(newFilter2);
        //        } 

        //public String GetWhere(PgTVM.PgTableViewFiltersM container)
        //{
        //    if (container == null || container.Container.Count == 0 || FilterHasError(container))
        //    {
        //        return "1 = 1";
        //    }
        //    else
        //    {
        //        List<String> conditions = new List<string>();
        //        foreach (PgTVM.IPgTableViewFilterM iFilter in container.Container)
        //        {
        //            if (iFilter is PgTVM.PgTableViewFilterM)
        //            {
        //                PgTVM.PgTableViewFilterM filterM = iFilter as PgTVM.PgTableViewFilterM;
        //                String subCondition = GetWhere(filterM);//String.Format("({0} {1} {2})", filterM.Field.Name, filterM.TOperation, filterM.Value);
        //                conditions.Add(subCondition);
        //            }
        //            else if (iFilter is PgTVM.PgTableViewFiltersM)
        //            {
        //                PgTVM.PgTableViewFiltersM filtersM = iFilter as PgTVM.PgTableViewFiltersM;
        //                conditions.Add(GetWhere(filtersM));
        //            }
        //        }
        //        String sep = String.Format(" {0} ", container.TRelation);
        //        return String.Format("({0})", string.Join(sep, conditions.ToArray()));
        //    }
        //}

        //public String GetWhere(PgTVM.PgTableViewFilterM filter)
        //{
        //    if (filter == null || FilterHasError(filter))
        //    {
        //        return "1 = 1";
        //    }
        //    else
        //    {
        //        String result = "";
        //        if (filter.Field.Table != null)
        //        {
        //            result = GetCondition(filter.Field, filter.TOperation, filter.Value, filter.UseOwnValue);
        //        }
        //        else
        //        {
        //            List<String> conditions = new List<string>();
        //            foreach (AbsM.IFieldM fieldM in filter.Source.Source.Table.Fields)
        //            {
        //                if (fieldM.Type != AbsM.EFieldType.Geometry)
        //                {
        //                    conditions.Add(GetCondition(fieldM, filter.TOperation, filter.Value, filter.UseOwnValue));
        //                }
        //            }
        //            result = String.Format("({0})", String.Join(" OR ", conditions.ToArray()));
        //        }
        //        return result;
        //    }
        //}

        //public String GetCondition(AbsM.IFieldM filterField, TypeOperation typeOperation, object filterValue, bool useOwnValue)
        //{
        //    String result = "";
        //    String fieldName = String.Format("{1}{0}", filterField.Name, useOwnValue ? "id!" : "");
        //    String field = String.Format("\"{0}\"", fieldName); 
        //    String value = "";
        //    String operation = "";
        //    switch (typeOperation)
        //    {
        //        case TypeOperation.More:
        //            operation = ">";
        //            break;
        //        case TypeOperation.Less:
        //            operation = "<";
        //            break;
        //        case TypeOperation.Equal:
        //            field = String.Format("lower(\"{0}\"::text)", fieldName); 
        //            operation = "=";
        //            break;
        //        case TypeOperation.NotEqual:
        //            field = String.Format("lower(\"{0}\"::text)", fieldName); 
        //            operation = "<>";
        //            break;
        //        case TypeOperation.MoreOrEqual:
        //            operation = ">=";
        //            break;
        //        case TypeOperation.LessOrEqual:
        //            operation = "<=";
        //            break;
        //        case TypeOperation.Empty:
        //            operation = "IS NULL";
        //            break;
        //        case TypeOperation.NotEmpty:
        //            operation = "IS NOT NULL";
        //            break;
        //        case TypeOperation.Contains:
        //        case TypeOperation.InEnd:
        //        case TypeOperation.InBegin:
        //            field = String.Format("lower(\"{0}\"::text)", fieldName); 
        //            operation = "LIKE";
        //            break;
        //        case TypeOperation.NotContains:
        //            field = String.Format("lower(\"{0}\"::text)", fieldName); 
        //            operation = "NOT LIKE";
        //            break;
        //    }
        //    if (((filterField.Type == AbsM.EFieldType.Integer || filterField.Type == AbsM.EFieldType.Real)
        //                && (typeOperation != TypeOperation.Equal && typeOperation != TypeOperation.NotEqual)))
        //    {
        //        value = String.Format("{0}", filterValue);
        //    }
        //    else 
        //    {
        //        if (filterField.Type == AbsM.EFieldType.Date || filterField.Type == AbsM.EFieldType.DateTime)
        //        {
        //            value = String.Format("'{0}'", filterValue);
        //        }
        //        else
        //        {
        //            value = String.Format("lower('{0}')", filterValue);
        //        }
        //    }
        //    switch (typeOperation)
        //    {
        //        case TypeOperation.More:
        //        case TypeOperation.Less:
        //        case TypeOperation.Equal:
        //        case TypeOperation.NotEqual:
        //        case TypeOperation.MoreOrEqual:
        //        case TypeOperation.LessOrEqual:
        //            {
        //                result = String.Format("{0} {1} {2}", field, operation, value);
        //                break;
        //            }
        //        case TypeOperation.Empty:
        //        case TypeOperation.NotEmpty:
        //            {
        //                result = String.Format("{0} {1}", field, operation);
        //                break;
        //            }
        //        case TypeOperation.Contains:
        //        case TypeOperation.InEnd:
        //        case TypeOperation.InBegin:
        //        case TypeOperation.NotContains:
        //            {
        //                result = String.Format("{0} {1} '{2}{3}{4}'",
        //                    field,
        //                    operation,
        //                    (typeOperation != TypeOperation.InBegin) ? "%" : "",
        //                    filterValue,
        //                    (typeOperation != TypeOperation.InEnd) ? "%" : "");
        //                break;
        //            }
        //    }
        //    return String.Format("({0})", result);
        //}
        #endregion Комментарии

        #region Методы
        /// <summary>
        /// Находит и возвращает поле в коллекции _fields источника
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private AbsM.IFieldM FindField(string name)
        {
            return _fields.FirstOrDefault(f => f.Name == name);
        }
        /// <summary>
        /// Устанавливает активный фильтр из FilterRelationModel
        /// </summary>
        /// <param name="iFilter"></param>
        /// <param name="pattern"></param>
        public void SetFilterFromPattern(PgTVM.IPgTableViewFilterM iFilter, FilterRelationModel pattern)
        {
            switch (pattern.Type)
            {
                case TypeRelation.AND:
                case TypeRelation.OR:
                    {
                        PgTVM.PgTableViewFiltersM container = iFilter as PgTVM.PgTableViewFiltersM;

                        container.Container.Clear();
                        container.TRelation = pattern.Type;
                        foreach (FilterRelationModel relM in pattern.Arguments)
                        {
                            if (relM.Type == TypeRelation.ELEMENT)
                            {
                                PgTVM.PgTableViewFilterM innerelement = new PgTVM.PgTableViewFilterM(container, false);
                                container.Container.Add(innerelement);
                                SetFilterFromPattern(innerelement, relM);
                            }
                            else
                            {
                                PgTVM.PgTableViewFiltersM innercontainer = new PgTVM.PgTableViewFiltersM(container, false);
                                container.Container.Add(innercontainer);
                                SetFilterFromPattern(innercontainer, relM);
                            }
                        }
                        break;
                    }
                case TypeRelation.ELEMENT:
                    {
                        PgTVM.PgTableViewFilterM element = (PgTVM.PgTableViewFilterM)iFilter;
                        element.Field = element.Source.FindField(pattern.Element.Column);
                        element.TOperation = pattern.Element.Type;
                        element.Value = pattern.Element.Value;
                        break;
                    }
            }
        }
        /// <summary>
        /// Обновление списка колонок таблицы
        /// </summary>
        private void GetFilterField()
        {
            if (_fields == null)
            {
                _fields = new ObservableCollection<AbsM.IFieldM>();
                _fields.Add(new PgTVM.PgFieldAllFilterM());
                if (_source.Table.Fields != null)
                    foreach (var f in _source.Table.Fields.Where(f => f.Type != AbsM.EFieldType.Geometry))
                    {
                        _fields.Add(f);
                    }
            }
            //var fields = new List<AbsM.IFieldM>();
            //fields.Add(new PgTVM.PgFieldAllFilterM());
            //if (_source.Table.Fields != null)
            //    fields.AddRange(_source.Table.Fields.Where(f => f.Type != AbsM.EFieldType.Geometry));
        }
        /// <summary>
        /// Обновление списка операторов
        /// </summary>
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
        /// <summary>
        /// Загружает из базы SQLite названия и идентификаторы всех фильтров, относящихся к таблице _source.Table
        /// </summary>
        private void ReloadFiltersFromSqLite()
        {
            IdNameFilter.Clear();
            Npgsql.NpgsqlConnectionStringBuilder connect = (_source.Table.Source as PgDataRepositoryVM).Connect;
            Pg_M.PgTableBaseM pgTable = _source.Table as Pg_M.PgTableBaseM;
            String sourceName = String.Format("{0}@{1}", connect.Database, connect.Host);
            String sourceType = pgTable.Source.Type.ToString();
            String tableName = String.Format("\"{0}\".\"{1}\"", pgTable.SchemeName, pgTable.Name);
            var filters = from Filter f
                            in Program.SettingsDB.Filters
                            where f.Table.TableName == tableName && f.Table.Source.SourceName == sourceName && f.Table.Source.SourceType == sourceType
                            select f;
            foreach (var filter in filters)
            {
                IdNameFilter.Add(new NameValue(filter.Id, filter.FilterName));
            }
        }
        /// <summary>
        /// Получает FilterRelationModel из фильтра
        /// </summary>
        /// <param name="iFilter"></param>
        /// <returns></returns>
        private FilterRelationModel GetFilterModel(PgTVM.IPgTableViewFilterM iFilter)
        {
            switch (iFilter.Type)
            {
                case PgTVM.PgTableViewFilterType.Filter:
                    {
                        var filter = iFilter as PgTVM.PgTableViewFilterM;
                        if (filter == null)
                            break;
                        //if (filter.CheckHasError())
                        //    return null;
                        return new FilterRelationModel()
                        {
                            Type = TypeRelation.ELEMENT,
                            Element = new FilterElementModel()
                            {
                                Column = filter.Field.Name,
                                Type = filter.TOperation,
                                Value = ((filter.Value == null)
                                            ? String.Empty
                                            : filter.Value.ToString())
                            }
                        };
                    }
                case PgTVM.PgTableViewFilterType.Container:
                    {
                        var filters = iFilter as PgTVM.PgTableViewFiltersM;
                        if (filters == null)
                            break;
                        return GetFilterModelContainer(filters);
                    }
            }
            throw new Exception("Ошибка при выборе фильтра");
        }
        /// <summary>
        /// Получает FilterRelationModel из контейнера
        /// </summary>
        /// <param name="cont"></param>
        /// <returns></returns>
        private FilterRelationModel GetFilterModelContainer(PgTVM.PgTableViewFiltersM cont)
        {
            var RelModel = new FilterRelationModel();
            RelModel.Type = cont.TRelation;
            RelModel.Arguments = new ObservableCollection<FilterRelationModel>();
            foreach (var item in cont.Container)
            {
                var filt = GetFilterModel(item);
                if (filt != null)
                    RelModel.Arguments.Add(filt);
            }
            if (RelModel.Arguments.Count == 0)
                return null;
            return RelModel;
        }

        public static bool FilterHasError(PgTVM.IPgTableViewFilterM iFilter)
        {
            if (iFilter is PgTVM.PgTableViewFilterM)
            {
                PgTVM.PgTableViewFilterM filter = iFilter as PgTVM.PgTableViewFilterM;
                return filter.HasError;
            }
            else if (iFilter is PgTVM.PgTableViewFiltersM)
            {
                PgTVM.PgTableViewFiltersM container = iFilter as PgTVM.PgTableViewFiltersM;
                bool result = false;
                foreach (PgTVM.IPgTableViewFilterM innerIFilter in container.Container)
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
        public static string GetWhere(PgTVM.IPgTableViewFilterM iFilter, List<IParams> listParams)
        {
            if (iFilter == null)
            {
                return "1 = 1";
            }
            else
            {
                if (iFilter is PgTVM.PgTableViewFilterM && FilterHasError(iFilter))
                {
                    return "1 = 1"; 
                }
                else if (iFilter is PgTVM.PgTableViewFiltersM)
                {
                    PgTVM.PgTableViewFiltersM container = iFilter as PgTVM.PgTableViewFiltersM;
                    if (container.Container.Count == 0 || FilterHasError(iFilter))
                    {
                        return "1=1";
                    }
                }
            }

            switch (iFilter.Type)
            {
                case PgTVM.PgTableViewFilterType.Filter:
                    {
                        var filter = iFilter as PgTVM.PgTableViewFilterM;
                        if (filter == null)
                            break;
                        if (filter.CheckHasError())
                        {
                            if (filter.UseOwnValue)
                            {
                                return null;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        var param = GetWhereParams(filter, listParams.Count + 1);
                        listParams.Add(param);
                        return GetWhereFilter(filter, param);
                    }
                case PgTVM.PgTableViewFilterType.Container:
                    {
                        var filters = iFilter as PgTVM.PgTableViewFiltersM;
                        if (filters == null)
                            break;
                        return GetWhereContainer(filters, listParams);
                    }
            }
            throw new Exception("Ошибка при выборе фильтра");
        }
        private static Params GetWhereParams(PgTVM.PgTableViewFilterM filter, int n)
        {
            string name = (filter.Field.Table == null) ? "all" : filter.Field.Name;
            var par = new Params(
                string.Format(":{0}{1}", name, n),
                 PgTVM.PgTableViewFilterM.GetValue(filter),
                 Pg_M.PgFieldM.GetDbType(filter.FieldType)
            );
            return par;
        }
        private static string GetWhereFilter(PgTVM.PgTableViewFilterM filter, Params paParams)
        {
            string format = "lower(\"{0}\") LIKE lower({1})";
            if (filter.FieldType == AbsM.EFieldType.Text)
                switch (filter.TOperation)
                {
                    case TypeOperation.Empty:
                        format = "\"{0}\" is NULL";
                        break;
                    case TypeOperation.NotEmpty:
                        format = "\"{0}\" is NOT NULL";
                        break;
                    case TypeOperation.Equal:
                        format = "lower(\"{0}\") LIKE lower({1})";
                        break;
                    case TypeOperation.NotEqual:
                        format = "lower(\"{0}\") NOT LIKE lower({1})";
                        break;
                    case TypeOperation.Contains:
                        paParams.value = string.Format("%{0}%", paParams.value);
                        break;
                    case TypeOperation.InEnd:
                        paParams.value = string.Format("%{0}", paParams.value);
                        break;
                    case TypeOperation.InBegin:
                        paParams.value = string.Format("{0}%", paParams.value);
                        break;
                    case TypeOperation.NotContains:
                        paParams.value = string.Format("%{0}%", paParams.value);
                        format = "lower(\"{0}\") NOT LIKE lower({1})";
                        break;
                }
            else
                switch (filter.TOperation)
                {
                    case TypeOperation.Empty:
                        format = "\"{0}\" is NULL";
                        break;
                    case TypeOperation.NotEmpty:
                        format = "\"{0}\" is NOT NULL";
                        break;
                    case TypeOperation.More:
                        format = "\"{0}\" > {1}";
                        break;
                    case TypeOperation.MoreOrEqual:
                        format = "\"{0}\" >= {1}";
                        break;
                    case TypeOperation.Less:
                        format = "\"{0}\" < {1}";
                        break;
                    case TypeOperation.LessOrEqual:
                        format = "\"{0}\" <= {1}";
                        break;
                    case TypeOperation.Equal:
                        format = "\"{0}\" = {1}";
                        break;
                    case TypeOperation.NotEqual:
                        format = "\"{0}\" <> {1}";
                        break;
                }
            if (filter.Field.Table == null)
            {
                // todo: (Dias) Для поле даты (и времени) использовать соответствующий параметр
                var fields = filter.Source.Fields.Where(f => f.Table != null).Select(f => string.Format(format, f.Name + "\"::\"text", paParams.paramName)).ToArray();
                var join = string.Join(" OR ", fields);
                return string.Format("({0})", join);
            }
            else
            {
                if (filter.UseOwnValue && ((Pg_M.PgFieldM)filter.Field).RefTable != null)
                    return string.Format(format, "id!" + filter.Field.Name, paParams.paramName);
                else
                    return string.Format(format, filter.Field.Name, paParams.paramName);
            }
        }
        private static string GetWhereContainer(PgTVM.PgTableViewFiltersM cont, List<IParams> listParams)
        {
            string sep;
            switch (cont.TRelation)
            {
                case TypeRelation.AND:
                    sep = " AND ";
                    break;
                case TypeRelation.OR:
                    sep = " OR ";
                    break;
                default:
                    goto default;
            }
            List<string> list = new List<string>();
            try
            {
                foreach (var item in cont.Container)
                {
                    var filt = GetWhere(item, listParams);
                    if (string.IsNullOrEmpty(filt))
                        continue;
                    list.Add(filt);
                }
            }
            catch (Exception ex)
            {
                if (cont.Parent == null)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            if (list.Count == 0)
                return null;
            return string.Format("({0})", string.Join(sep, list.ToArray()));
        }

        #endregion Методы
    }
}