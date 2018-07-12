using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using TMVM = Rekod.DataAccess.TableManager.ViewModel;
using System.Windows.Input;
using Pg_V = Rekod.DataAccess.SourcePostgres.View;
using Pg_VM = Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Windows;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgRepositoriesConfig_VM : WindowViewModelBase_VM
    {
        #region Поля
        TMVM.TableManagerVM _source;
        ObservableCollection<AbsM.IDataRepositoryM> _dataRepositories;
        AbsM.IDataRepositoryM _currentRepository;

        private ICommand _addConnectionCommand;
        private ICommand _deleteConnectionCommand;
        #endregion // Поля

        #region Свойства
        public TMVM.TableManagerVM Source
        {
            get { return _source; }
        }
        public ObservableCollection<AbsM.IDataRepositoryM> DataRepositories
        {
            get { return _dataRepositories; }
        }
        public AbsM.IDataRepositoryM CurrentRepository
        {
            get { return _currentRepository; }
            set
            {
                OnPropertyChanged(ref _currentRepository, value, () => this.CurrentRepository);
                OnPropertyChanged("Title");
            }
        }
        public override string Title
        {
            get
            {
                _title = Rekod.Properties.Resources.LocPostgreConfig;
                return _title;
            }
            set
            {
                OnPropertyChanged("Title");
            }
        }
        #endregion // Свойства

        #region Конструктор
        public PgRepositoriesConfig_VM(TMVM.TableManagerVM source)
        {
            _source = source;
            _dataRepositories = new ObservableCollection<AbsM.IDataRepositoryM>();

            source.DataRepositoriesCollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(dataRepositories_CollectionChanged);
            dataRepositories_CollectionChanged(source.DataRepositories, null);
            OnPropertyChanged("Title");
        }
        #endregion // Конструктор

        #region Команды
        #region DeleteConnectionCommand
        /// <summary>
        /// Удаление источника
        /// </summary>
        public ICommand DeleteConnectionCommand
        {
            get { return _deleteConnectionCommand ?? (_deleteConnectionCommand = new RelayCommand(this.DeleteConnection, this.CanDeleteConnection)); }
        }
        /// <summary>
        /// Удаление источника
        /// </summary>
        private void DeleteConnection(object obj = null)
        {
            if (!CanDeleteConnection(obj))
                return;
            _source.RemoveRepository(CurrentRepository);
            CurrentRepository = null;
        }
        private bool CanDeleteConnection(object obj = null)
        {
            return (CurrentRepository != null);
        }
        #endregion // DeleteConnectionCommand
        #endregion Команды

        #region Методы
        void dataRepositories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var array = sender as ObservableCollection<AbsM.IDataRepositoryM>;
            if (array == null) return;
            var values = array.Where(f => f.Type == AbsM.ERepositoryType.Postgres);
            ExtraFunctions.Sorts.SortList(_dataRepositories, values);
        }

        #region Методы AbsVM.WindowViewModelBase_VM
        protected override bool Closing(object obj)
        {
            _source.DataRepositoriesCollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(dataRepositories_CollectionChanged);
            return base.Closing(obj);
        }
        #endregion
        #endregion // Методы
    }
}