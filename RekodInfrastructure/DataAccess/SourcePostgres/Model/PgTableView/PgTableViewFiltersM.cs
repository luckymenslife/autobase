using Rekod.SQLiteSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgTV_VM = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;
using Interfaces;
using Rekod.DataAccess.AbstractSource.ViewModel;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Classes;

namespace Rekod.DataAccess.SourcePostgres.Model.PgTableView
{
    public class PgTableViewFiltersM : ViewModelBase, IPgTableViewFilterM
    {
        #region Поля
        private PgTableViewFilterType _type = PgTableViewFilterType.Container;
        PgTV_VM.PgTableViewFilterVM _source;
        private PgTableViewFiltersM _parent;

        private TypeRelation _tRelation = TypeRelation.AND;
        private ObservableCollection<IPgTableViewFilterM> _container;
        private ICommand _updateQueryStringCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Имеет ли фильтр ошибки
        /// </summary>
        public bool HasError
        {
            get 
            {
                if (Container.Count == 0)
                {
                    return true;
                }
                else
                {
                    bool hasError = false;
                    foreach (IPgTableViewFilterM iFilter in Container)
                    {
                        hasError |= iFilter.HasError; 
                    }
                    return hasError; 
                }
            }
        }
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
        /// Источник фильтра 
        /// </summary>
        public PgTV_VM.PgTableViewFilterVM Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Тип коллекции
        /// </summary>
        public TypeRelation TRelation
        {
            get { return _tRelation; }
            set 
            {
                OnPropertyChanged(ref _tRelation, value, () => this.TRelation);
                OnPropertyChanged(() => this.QueryString); 
            }
        }
        /// <summary>
        /// Список элементов контейнера
        /// </summary>
        public ObservableCollection<IPgTableViewFilterM> Container
        {
            get { return _container ?? (_container = new ObservableCollection<IPgTableViewFilterM>()); }
        }
        /// <summary>
        /// Глубина вложенности контейнера
        /// </summary>
        public int Depth
        {
            get
            {
                int depth = (Parent == null) ? 0 : (Parent.Depth + 1);
                return depth % 5 + 1;
            }
        }
        /// <summary>
        /// Строка запроса фильтра
        /// </summary>
        public string QueryString
        {
            get
            {
                List<Interfaces.IParams> listParams = new List<Interfaces.IParams>();
                String query = "";
                foreach (IPgTableViewFilterM iFilter in Container)
                {
                    if (iFilter is PgTableViewFilterM)
                    {
                        PgTableViewFilterM filter = iFilter as PgTableViewFilterM;

                        //if (filter.Value != null)
                        {
                            NameValue nv = filter.Source.CollOperationText.FirstOrDefault(p => (TypeOperation)p.Value == filter.TOperation);
                            if (nv == null)
                            {
                                nv = filter.Source.CollOperationValue.FirstOrDefault(p => (TypeOperation)p.Value == filter.TOperation);
                            }
                            TypeOperation nvType = (TypeOperation)nv.Value;

                            String operation = "";
                            String filtervalue = ""; 

                            switch (nvType)
                            {
                                case TypeOperation.More:
                                case TypeOperation.Less:
                                case TypeOperation.Equal:
                                case TypeOperation.NotEqual:
                                case TypeOperation.MoreOrEqual:
                                case TypeOperation.LessOrEqual:
                                    {
                                        filtervalue = String.Format(" '{0}'", filter.Value); 
                                        operation = nv.Name; 
                                        break;
                                    }
                                case TypeOperation.Empty:
                                case TypeOperation.NotEmpty:
                                    {
                                        operation = String.Format("[{0}]", nv.Name);
                                        break;
                                    }
                                case TypeOperation.Init:
                                case TypeOperation.Contains:
                                case TypeOperation.InEnd:
                                case TypeOperation.InBegin:
                                case TypeOperation.NotContains:
                                    {
                                        operation = String.Format("[{0}]", nv.Name);
                                        filtervalue = String.Format(" '{0}'", filter.Value); 
                                        break; 
                                    }
                                default:
                                    break;
                            }

                            String condition = String.Format("({0} {1}{2})", filter.Field.Text, operation, filtervalue);
                            if (!String.IsNullOrEmpty(query))
                            {
                                query += String.Format(" {0} ", TRelation == TypeRelation.OR ? "ИЛИ" : "И");
                            }
                            query += condition;
                        }
                    }
                    else if (iFilter is PgTableViewFiltersM)
                    {
                        PgTableViewFiltersM container = iFilter as PgTableViewFiltersM;
                        String subQueryString = container.QueryString;
                        if (!String.IsNullOrEmpty(subQueryString))
                        {
                            if (!String.IsNullOrEmpty(query))
                            {
                                query += String.Format(" {0} ", TRelation==TypeRelation.OR?"ИЛИ":"И");
                            }
                            query += String.Format("({0})", subQueryString);
                        }
                    }
                }
                return query;
            }
        }
        #endregion Свойства

        #region Конструктор
        /// <summary>
        /// Первый элемент фильтра
        /// </summary>
        /// <param name="source"></param>
        /// <param name="setDefault">Если true - во внутренней коллекции по умолчанию сразу создается фильтр</param>
        public PgTableViewFiltersM(PgTV_VM.PgTableViewFilterVM source, bool setDefault = true)
        {
            _source = source;
            if (setDefault)
            {
                Container.Add(new PgTableViewFilterM(this, false));
            }
            PropertyChanged += new PropertyChangedEventHandler(PgTableViewFiltersM_PropertyChanged);
        }
        /// <summary>
        /// Первый элемент фильтра
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="setDefault">Если true - во внутренней коллекции по умолчанию сразу создается фильтр</param>
        public PgTableViewFiltersM(PgTableViewFiltersM parent, bool setDefault = true)
            : this(parent.Source, setDefault)
        {
            _parent = parent;
        }
        #endregion // Конструктор

        #region Команды
        #region UpdateQueryStringCommand
        /// <summary>
        /// Команда для обновления строки запроса
        /// </summary>
        public ICommand UpdateQueryStringCommand
        {
            get { return _updateQueryStringCommand ?? (_updateQueryStringCommand = new RelayCommand(this.UpdateQueryString, this.CanUpdateQueryString)); }
        }
        /// <summary>
        /// Обновление строки запроса
        /// </summary>
        public void UpdateQueryString(object parameter = null)
        {
            OnPropertyChanged("QueryString");
        }
        /// <summary>
        /// Можно ли обновить строку запроса
        /// </summary>
        public bool CanUpdateQueryString(object parameter = null)
        {
            return true;
        }
        #endregion // UpdateQueryStringCommand 
        #endregion Команды

        #region Обработчики
        void PgTableViewFiltersM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "QueryString")
            {
                OnPropertyChanged("HasError");
            }
        }
        #endregion Обработчики
    }
}