using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rekod.Behaviors;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgGroupsVM : ViewModelBase 
    {
        #region Поля
        private PgVM.PgDataRepositoryVM _source;
        private AbsM.GroupM _currentGroup;
        private ObservableCollection<PgM.PgTableBaseM> _containsList;
        private ObservableCollection<PgM.PgTableBaseM> _doesntContainList;
        #endregion Поля

        #region Конструкторы
        public PgGroupsVM(PgVM.PgDataRepositoryVM repo)
        {
            _source = repo;
            PropertyChanged += PgGroupsVM_PropertyChanged;
            (repo.Tables as ObservableCollection<AbsM.ITableBaseM>).CollectionChanged += PgGroupsVM_CollectionChanged;
        }        
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<PgM.PgTableBaseM> ContainsList
        {
            get { return _containsList ?? (_containsList = new ObservableCollection<PgM.PgTableBaseM>()); }
        }
        public ObservableCollection<PgM.PgTableBaseM> DoesntContainList
        {
            get { return _doesntContainList ?? (_doesntContainList = new ObservableCollection<PgM.PgTableBaseM>()); }
        }
        #endregion Коллекции

        #region Свойства
        public PgVM.PgDataRepositoryVM Source
        {
            get { return _source; }
        }
        public AbsM.GroupM CurrentGroup
        {
            get
            {
                return _currentGroup;
            }
            set
            {
                if (_currentGroup != null)
                {
                    _currentGroup.Tables.CollectionChanged -= CurrentGroupTables_CollectionChanged;
                }
                OnPropertyChanged(ref _currentGroup, value, () => this.CurrentGroup);
            }
        }
        #endregion Свойства

        #region Методы
        public void ReloadTables()
        {
            if (_source != null && CurrentGroup != null)
            {
                ContainsList.Clear();
                DoesntContainList.Clear();
                var doesntContainCollection = from layer in _source.Layers
                                              where (!CurrentGroup.Tables.Contains(layer) && !layer.IsHidden)
                                              select layer as PgM.PgTableBaseM;
                foreach (PgM.PgTableBaseM pgTable in CurrentGroup.Tables)
                {
                    _containsList.Add(pgTable);
                }
                foreach (PgM.PgTableBaseM pgTable in doesntContainCollection)
                {
                    _doesntContainList.Add(pgTable);
                }
            }
            else
            {
                DoesntContainList.Clear();
                ContainsList.Clear();
            }
        }
        #endregion Методы

        #region Обработчики
        void PgGroupsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentGroup")
            {
                ReloadTables();
                if (CurrentGroup != null)
                {
                    CurrentGroup.Tables.CollectionChanged -= CurrentGroupTables_CollectionChanged;
                }
            }
        }
        void PgGroupsVM_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReloadTables();
        }
        void CurrentGroupTables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReloadTables();
        }
        #endregion Обработчики

        #region Команды
        #region IncludeInGroupCommand
        private ICommand _includeInGroupCommand;
        /// <summary>
        /// Команда для добавления таблицы в группу
        /// </summary>
        public ICommand IncludeInGroupCommand
        {
            get { return _includeInGroupCommand ?? (_includeInGroupCommand = new RelayCommand(this.IncludeInGroup, this.CanIncludeInGroup)); }
        }
        /// <summary>
        /// Добавление таблицы в группу
        /// </summary>
        public void IncludeInGroup(object parameter = null)
        {
            PgM.PgTableBaseM pgTable = parameter as PgM.PgTableBaseM;
            if (!_containsList.Contains(pgTable))
            {
                _containsList.Add(pgTable);
                _doesntContainList.Remove(pgTable);
            }
        }
        /// <summary>
        /// Можно ли добавить таблицу в группу
        /// </summary>
        public bool CanIncludeInGroup(object parameter = null)
        {
            return parameter is PgM.PgTableBaseM;
        }
        #endregion // IncludeInGroupCommand

        #region ExcludeFromGroupCommand
        private ICommand _excludeFromGroupCommand;
        /// <summary>
        /// Команда для удаления таблицы из группы
        /// </summary>
        public ICommand ExcludeFromGroupCommand
        {
            get { return _excludeFromGroupCommand ?? (_excludeFromGroupCommand = new RelayCommand(this.ExcludeFromGroup, this.CanExcludeFromGroup)); }
        }
        /// <summary>
        /// Удаление таблицы из группы
        /// </summary>
        public void ExcludeFromGroup(object parameter = null)
        {
            PgM.PgTableBaseM pgTable = parameter as PgM.PgTableBaseM;
            if (!_doesntContainList.Contains(pgTable))
            {
                _doesntContainList.Add(pgTable);
                _containsList.Remove(pgTable);
            }
        }
        /// <summary>
        /// Можно ли удалить таблицу из группы
        /// </summary>
        public bool CanExcludeFromGroup(object parameter = null)
        {
            return parameter is PgM.PgTableBaseM;
        }
        #endregion // ExcludeFromGroupCommand

        #region NewGroupCommand
        private ICommand _newGroupCommand;
        /// <summary>
        /// Команда для создания новой группы
        /// </summary>
        public ICommand NewGroupCommand
        {
            get { return _newGroupCommand ?? (_newGroupCommand = new RelayCommand(this.NewGroup, this.CanNewGroup)); }
        }
        /// <summary>
        /// Создание новой группый
        /// </summary>
        public void NewGroup(object parameter = null)
        {
            CurrentGroup = null;
            CurrentGroup = new AbsM.GroupM(_source);
        }
        /// <summary>
        /// Можно ли создать новую группу
        /// </summary>
        public bool CanNewGroup(object parameter = null)
        {
            return true;
        }
        #endregion // NewGroupCommand
        #endregion Команды

        #region Действия
        public Action<object> BindingGroupLoadedAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                        bindGroup.BeginEdit();
                    };
            }
        } 
        public Action<object> BindingGroupErrorAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        ValidationErrorEventArgs errorArgs = commEvtParam.EventArgs as ValidationErrorEventArgs;
                        if (errorArgs.Action == ValidationErrorEventAction.Added)
                        {
                            MessageBox.Show(errorArgs.Error.ErrorContent.ToString());
                        }
                    };
            }
        }
        public Action<object> BindingGroupCancelAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindGroup.CancelEdit();
                    ReloadTables();
                    bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupSaveAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    if (bindGroup.CommitEdit())
                    {
                        if (CurrentGroup != null)
                        {
                            AbsM.GroupM TempGroup = CurrentGroup;
                            if (CurrentGroup.IsNewGroup)
                            {
                                int? newgroupid = _source.DBSaveGroup(CurrentGroup);
                                if (newgroupid != null)
                                {
                                    TempGroup = _source.FindGroup(newgroupid) as AbsM.GroupM;
                                }
                                else
                                {
                                    TempGroup = null;
                                    CurrentGroup = null;
                                    return;
                                }
                            }
                            else
                            {
                                _source.DBSaveGroup(CurrentGroup);
                            }
                            foreach (PgM.PgTableBaseM layerM in _containsList)
                            {
                                if (!TempGroup.Tables.Contains(layerM))
                                {
                                    _source.TableGroupMove(layerM, TempGroup, true);
                                }
                            }
                            foreach (PgM.PgTableBaseM layerM in _doesntContainList)
                            {
                                if (TempGroup.Tables.Contains(layerM))
                                {
                                    _source.TableGroupMove(layerM, TempGroup, false);
                                }
                            }
                            CurrentGroup = TempGroup;
                        }

                        bindGroup.BeginEdit();
                    }
                };
            }
        } 
        #endregion Действия
    }
}
