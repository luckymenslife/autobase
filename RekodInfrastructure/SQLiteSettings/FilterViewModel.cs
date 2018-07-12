using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using Rekod.Repository;
using Rekod.Services;
using Rekod.Repository.SettingsDB;

namespace Rekod.SQLiteSettings
{
    /// <summary>Класс работы с фильтром (чтение из базы, удаление, изменение)
    /// </summary>
    public class FilterViewModel
    {
        #region Конструктор
        public FilterViewModel(int id_filter)
        {
            this.Load(id_filter);
        }
        public FilterViewModel()
        {
        }
        #endregion
        #region Переменные
        private FilterRelationModel _filter = new FilterRelationModel();
        private string _table;
        private string _filterName;
        private string _sourceName;
        private int _id;
        private int _idTableSqLite;
        private bool _isNew = true;
        #endregion
        #region Свойства
        public string SourceName
        {
            get { return _sourceName; }
            set { _sourceName = value; }
        }
        /// <summary>Идентификатор таблицы
        /// </summary>
        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }
        public string FilterName
        {
            get { return _filterName; }
            set { _filterName = value; }
        }
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public FilterRelationModel Filter
        {
            get { return _filter; }
        }
        #endregion
        #region Методы
        public void Load(int id_filter)
        {
            var filter = (from Filter f in Program.SettingsDB.Filters where f.Id == id_filter select f).FirstOrDefault();
            if (filter != null)
            {
                ID = (int)filter.Id;
                _filter.Type = TypeRelation.UNKNOWN;
                _table = filter.Table.TableName;
                _idTableSqLite = (int)filter.IdTable;
                _sourceName = filter.Table.Source.SourceName;
                _filter.Arguments = null;
                _filter.Element = null;
                _filterName = filter.FilterName;
                _isNew = false;
                
                FilterRelationModel temp = SqlJsonParser.FromJson<FilterRelationModel>(filter.WhereText);
                if (temp != null)
                {
                    if (temp.Type == TypeRelation.ELEMENT)
                        this._filter.Element = temp.Element;
                    else
                        this._filter.Arguments = temp.Arguments;
                    this._filter.Type = temp.Type;
                }
            }
        }
        public void Save()
        {
            if (this._isNew)
            {
                GetExistsTable();
                Filter newFilter = new Filter()
                            { FilterName = _filterName, WhereText = SqlJsonParser.ToJson(_filter), IdTable = _idTableSqLite }; 
                Program.SettingsDB.Filters.AddObject(newFilter);
                Program.SettingsDB.SaveChanges();
                ID = (int)newFilter.Id;
            }
            else
            {
                var filter = (from Filter f in Program.SettingsDB.Filters where f.Id == this.ID select f).FirstOrDefault();
                if (filter != null)
                {
                    filter.FilterName = _filterName;
                    filter.WhereText = SqlJsonParser.ToJson(_filter);
                    filter.IdTable = _idTableSqLite;
                }
                Program.SettingsDB.SaveChanges();
            }
            this.Load(this.ID);
        }
        private void GetExistsTable()
        {
            if(String.IsNullOrEmpty(this.Table))
            {
                throw new Exception(Rekod.Properties.Resources.FilterViewModel_ExceptTableName);
            }

            var table = (from Table t in Program.SettingsDB.Tables where t.TableName == this.Table select t).FirstOrDefault();
            if (table != null)
            {
                _idTableSqLite = (int)table.Id;
            }
            else
            {
                Table newTable = new Table() 
                    {
                        TableName = Table,
                        SourceId = GetExistsSource()
                    };
                Program.SettingsDB.Tables.AddObject(newTable);
                Program.SettingsDB.SaveChanges();
                _idTableSqLite = (int)newTable.Id;
            }
        }
        private int GetExistsSource()
        {
            if (String.IsNullOrEmpty(this.Table))
            {
                throw new Exception(Rekod.Properties.Resources.FilterViewModel_ExceptSource);
            }
            int id_source = 0;
            Source src = (from Source s in Program.SettingsDB.Sources where s.SourceName == _sourceName select s).FirstOrDefault();
            if (src != null)
            {
                id_source = (int)src.Id; 
            }
            else
            {
                Source newSource = new Source() { SourceName = _sourceName };
                Program.SettingsDB.Sources.AddObject(newSource);
                Program.SettingsDB.SaveChanges();
                id_source = (int)newSource.Id;
            }
            return id_source;
        }
        #endregion
    }
    public class FiltersViewModel
    {
        #region Переменные
        ObservableCollection<FilterViewModel> _filters = new ObservableCollection<FilterViewModel>();
        #endregion

        #region Свойства
        
        #endregion

        #region Методы
            #region Public
        public void Load(string sourseName)
        {
            _filters.Clear();
            var sourcesByName = from Source s in Program.SettingsDB.Sources where s.SourceName == sourseName select s;
            if (sourcesByName.Count() == 1)
            {
                var sourceByName = sourcesByName.FirstOrDefault();
                var filters = from Filter f in Program.SettingsDB.Filters where f.Table.SourceId == sourceByName.Id select f;
                foreach (Filter filter in filters)
                {
                    _filters.Add(new FilterViewModel((int)filter.Id));
                }
            }
        }
        public void Delete(int id)
        {
            var filter = (from Filter f in Program.SettingsDB.Filters where f.Id == id select f).FirstOrDefault();
            if (filter != null)
            {
                Program.SettingsDB.Filters.DeleteObject(filter);
                Program.SettingsDB.SaveChanges();
            }
            _filters.RemoveAt(GetIndex(id));
        }
        public int Add(FilterViewModel item)
        {
            item.Save();
            _filters.Add(item);
            return item.ID;
        }
        public ObservableCollection<FilterViewModel> FilterByTable(string table)
        {
            ObservableCollection<FilterViewModel> temp = new ObservableCollection<FilterViewModel>();
            for (int i = 0; i < _filters.Count; i++)
            {
                if (_filters[i].Table == table)
                {
                    temp.Add(_filters[i]);
                }
            }
            return temp;
        }
        public FilterViewModel GetFilter(int IdFilter)
        {
            for (int i = 0; i < _filters.Count; i++)
            {
                if (_filters[i].ID == IdFilter)
                {
                    return _filters[i];
                }
            }
            return null;
        }
            #endregion
            #region Privat
        private int GetIndex(int id_filter)
        {
            for (int i = 0; i < _filters.Count; i++)
            {
                if (_filters[i].ID == id_filter)
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion
        #endregion
    }
}
