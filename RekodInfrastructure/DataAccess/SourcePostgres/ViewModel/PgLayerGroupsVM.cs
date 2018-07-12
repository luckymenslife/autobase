using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgLayerGroupsVM: ViewModelBase
    {
        #region Поля
        private PgM.PgTableBaseM _table;
        private PgVM.PgDataRepositoryVM _source;
        private ObservableCollection<AbsM.GroupM> _groupsIn;
        private ObservableCollection<AbsM.GroupM> _groupsOut;
        #endregion Поля

        #region Конструкторы
        public PgLayerGroupsVM(PgM.PgTableBaseM pgtable)
        {
            _table = pgtable;
            _source = pgtable.Source as PgVM.PgDataRepositoryVM;
            _source.GroupsCollectionChanged += Source_GroupsCollectionChanged;
            foreach (AbsM.GroupM groupM in _source.Groups)
            {
                groupM.Tables.CollectionChanged += Tables_CollectionChanged;
            }
            ReloadGroups();
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<AbsM.GroupM> GroupsOut
        {
            get { return _groupsOut ?? (_groupsOut = new ObservableCollection<AbsM.GroupM>()); }
        }
        public ObservableCollection<AbsM.GroupM> GroupsIn
        {
            get { return _groupsIn ?? (_groupsIn = new ObservableCollection<AbsM.GroupM>()); }
        } 
        #endregion Коллекции

        #region Свойства
        public PgM.PgTableBaseM Table
        {
            get { return _table; }
        }
        #endregion Свойства

        #region Методы
        public void ReloadGroups()
        {
            GroupsIn.Clear();
            GroupsOut.Clear();
            
            PgVM.PgDataRepositoryVM dataRepo = (_table.Source as PgVM.PgDataRepositoryVM);
          
            var containsCollection = from thematicgroup in dataRepo.Groups
                                        where thematicgroup.Tables.Contains(_table)
                                        select thematicgroup as AbsM.GroupM;
            foreach (AbsM.GroupM groupM in containsCollection)
            {
                _groupsIn.Add(groupM);
            }

            var doesntContaintCollection = from thematicgroup in dataRepo.Groups
                                              where !thematicgroup.Tables.Contains(_table)
                                              select thematicgroup as AbsM.GroupM;
            foreach (AbsM.GroupM groupM in doesntContaintCollection)
            {
                _groupsOut.Add(groupM);
            }  
        }
        #endregion Методы

        #region Обработчики
        void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReloadGroups();
        }
        void Source_GroupsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (AbsM.GroupM groupVM in e.OldItems)
                {
                    groupVM.Tables.CollectionChanged -= Tables_CollectionChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (AbsM.GroupM groupVM in e.NewItems)
                {
                    groupVM.Tables.CollectionChanged += Tables_CollectionChanged;
                }
            }
            ReloadGroups();
        }
        #endregion Обработчики

        #region Команды
        #region SaveCommand
        private ICommand _saveCommand;
        /// <summary>
        /// Команда для сохранения
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        /// <summary>
        /// Сохранение
        /// </summary>
        public void Save(object parameter = null)
        {
            List<AbsM.GroupM> tempInList = new List<AbsM.GroupM>();
            List<AbsM.GroupM> tempOutList = new List<AbsM.GroupM>();
            tempInList.AddRange(_groupsIn);
            tempOutList.AddRange(_groupsOut);
            foreach (AbsM.GroupM group in tempInList)
            {
                if (!group.Tables.Contains(_table))
                {
                    _source.TableGroupMove(_table, group, true);
                }
            }
            foreach (AbsM.GroupM group in tempOutList)
            {
                if (group.Tables.Contains(_table))
                {
                    _source.TableGroupMove(_table, group, false);
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить
        /// </summary>
        public bool CanSave(object parameter = null)
        {
            return true;
        }
        #endregion // SaveCommand

        #region ReloadCommand
        private ICommand _reloadCommand;
        /// <summary>
        /// Команда для обновления списков
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        /// <summary>
        /// Обновление списков
        /// </summary>
        public void Reload(object parameter = null)
        {
            ReloadGroups();
        }
        /// <summary>
        /// Можно ли обновить списки
        /// </summary>
        public bool CanReload(object parameter = null)
        {
            return true;
        }
        #endregion // ReloadCommand 

        #region AddToGroupsCommand
        private ICommand _addToGroupsCommand;
        /// <summary>
        /// Команда для добавления таблицы в группы
        /// </summary>
        public ICommand AddToGroupsCommand
        {
            get { return _addToGroupsCommand ?? (_addToGroupsCommand = new RelayCommand(this.AddToGroups, this.CanAddToGroups)); }
        }
        /// <summary>
        /// Добавление таблицы в группы
        /// </summary>
        public void AddToGroups(object parameter = null)
        {
            System.Collections.ICollection icol = parameter as System.Collections.ICollection;
            List<AbsM.GroupM> selGroups = new List<AbsM.GroupM>(icol.OfType<AbsM.GroupM>());
            foreach (AbsM.GroupM item in selGroups)
            {
                if (!_groupsIn.Contains(item))
                {
                    _groupsIn.Add(item);
                    _groupsOut.Remove(item);
                }
            }
        }
        /// <summary>
        /// Можно ли добавить таблицы в группы
        /// </summary>
        public bool CanAddToGroups(object parameter = null)
        {
            System.Collections.ICollection icol = parameter as System.Collections.ICollection;
            return icol.Count > 0;
        }
        #endregion // AddToGroupsCommand

        #region RemoveFromGroupsCommand
        private ICommand _removeFromGroupsCommand;
        /// <summary>
        /// Команда для удаления таблицы из групп
        /// </summary>
        public ICommand RemoveFromGroupsCommand
        {
            get { return _removeFromGroupsCommand ?? (_removeFromGroupsCommand = new RelayCommand(this.RemoveFromGroups, this.CanRemoveFromGroups)); }
        }
        /// <summary>
        /// Удаление таблицы из групп
        /// </summary>
        public void RemoveFromGroups(object parameter = null)
        {
            System.Collections.ICollection icol = parameter as System.Collections.ICollection;
            List<AbsM.GroupM> selGroups = new List<AbsM.GroupM>(icol.OfType<AbsM.GroupM>());
            foreach (AbsM.GroupM item in selGroups)
            {
                if (!_groupsOut.Contains(item))
                {
                    _groupsOut.Add(item);
                    _groupsIn.Remove(item);
                }
            }
        }
        /// <summary>
        /// Можно ли удалить таблицу из групп
        /// </summary>
        public bool CanRemoveFromGroups(object parameter = null)
        {
            System.Collections.ICollection icol = parameter as System.Collections.ICollection;
            return icol.Count > 0;
        }
        #endregion // RemoveFromGroupsCommand
        #endregion Команды
    }
}
