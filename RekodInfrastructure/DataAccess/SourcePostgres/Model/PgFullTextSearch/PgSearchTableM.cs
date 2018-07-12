using Npgsql;
using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourcePostgres.Model.PgFullTextSearch
{
    public class PgSearchTableM: ViewModelBase
    {
        #region Поля
        private ObservableCollection<PgSearchObjectM> _searchObjects;
        private PgDataRepositoryVM _source;
        private PgTableBaseM _table;
        private bool _searchWithin = true;
        #endregion Поля

        #region Конструкторы
        public PgSearchTableM(PgDataRepositoryVM source, int idtable)
        {
            _source = source;
            var tableList = from PgTableBaseM table in source.Tables where table.Id == idtable select table;
            if (tableList.Count() == 1)
            {
                _table = tableList.ElementAt(0);
            }
        }
        public PgSearchTableM(PgTableBaseM pgTable)
        {
            _source = pgTable.Source as PgDataRepositoryVM;
            _table = pgTable;
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<PgSearchObjectM> SearchObjects
        {
            get { return _searchObjects ?? (_searchObjects = new ObservableCollection<PgSearchObjectM>()); }
        }  
        #endregion Коллекции

        #region Свойства
        public PgTableBaseM Table
        {
            get { return _table; }
        }
        public Boolean SearchWithin
        {
            get { return _searchWithin; }
            set { OnPropertyChanged(ref _searchWithin, value, () => this.SearchWithin); }
        }
        #endregion Свойства

        #region Методы
        
        #endregion Методы
    }
}