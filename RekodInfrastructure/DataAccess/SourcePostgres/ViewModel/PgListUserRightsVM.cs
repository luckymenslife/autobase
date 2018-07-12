using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.SourcePostgres.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using Interfaces;
using System.Data;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource;
using System.Windows.Data;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;
using Rekod.Services;
using System.Windows;
using Rekod.DataAccess.SourcePostgres.View;
using PgVR = Rekod.DataAccess.SourcePostgres.View.PgRights;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    /// <summary>
    /// Это класс который содержит в себе список пользователей с их правами
    /// </summary>
    public class PgListUserRightsVM : WindowViewModelBase_VM
    {
        #region Поля
        private PgDataRepositoryVM _source;
        private ObservableCollection<PgUserM> _users;
        private ObservableCollection<PgUserRightsM> _rightsList;
        private ObservableCollection<PgActionM> _actions;
        private ObservableCollection<PgActionRightM> _actionRightsList;
        private ObservableCollection<TableTypeRightsVM> _tableTypeRights;
        private PgTableBaseM _table;
        private PgUserM _user;
        private PgActionM _action;
        private PgUserM _userToReplaceWith;
        private PgTableBaseM _tableToReplaceWith;

        private ICommand _reloadCommand;
        private ICommand _saveChangesCommand;
        private ICommand _saveChangesACommand;
        private ICommand _filterDefaultViewCommand;
        private ICommand _mergeUserRightsCommand;
        private ICommand _mergeUserActionRightsCommand;
        private ICommand _mergeTableRightsCommand;
        private ICommand _addNewUserCommand;
        private ICommand _deleteUserCommand;
        private bool? _canAllRead;
        private bool? _canAllWrite;
        #endregion Поля

        #region Конструкторы
        public PgListUserRightsVM(AbsM.IDataRepositoryM repository)
        {
            Title = Rekod.Properties.Resources.PgUserList_AdmUserRaght;
            _rightsList = new ObservableCollection<PgUserRightsM>();
            _rightsList.CollectionChanged += new NotifyCollectionChangedEventHandler(_rightsList_CollectionChanged);
            _actionRightsList = new ObservableCollection<PgActionRightM>();
            _actionRightsList.CollectionChanged += new NotifyCollectionChangedEventHandler(_actionRightsList_CollectionChanged);
            _source = repository as PgDataRepositoryVM;
            _users = _source.Users;
            _actions = _source.Actions;
            foreach (var right in _source.RightsStructure)
            {
                _rightsList.Add(right);
            }
            //foreach (var table in Source.Tables)
            //{
            //    if (table.RefTables == null)
            //        ((PgTableBaseM)table).RefTables = GetRefTablesDB((int)table.Id);
            //}
            this.PropertyChanged += new PropertyChangedEventHandler(PgListUserRightsVM_PropertyChanged);
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<PgUserM> Users { get { return _users; } }
        public ObservableCollection<PgUserRightsM> Rights { get { return _rightsList; } }
        public ObservableCollection<PgActionM> Actions { get { return _actions; } }
        public ObservableCollection<PgActionRightM> ActionRights { get { return _actionRightsList; } }
        public ObservableCollection<TableTypeRightsVM> TableTypeRights { get { return _tableTypeRights; } }
        public Dictionary<string, List<AbsM.ITableBaseM>> TableHierarchy
        {
            get
            {
                return new Dictionary<string, List<AbsM.ITableBaseM>>(){
                    {Rekod.Properties.Resources.PgListUserRightsVM_MapLayers, Source.Tables.Where(w => w.Type == DataAccess.AbstractSource.Model.ETableType.MapLayer).ToList()},
                    {Rekod.Properties.Resources.PgListUserRightsVM_References, Source.Tables.Where(w => w.Type == DataAccess.AbstractSource.Model.ETableType.Catalog).ToList()},
                    {Rekod.Properties.Resources.PgListUserRightsVM_Intervals, Source.Tables.Where(w => w.Type == DataAccess.AbstractSource.Model.ETableType.Interval).ToList()},
                    {Rekod.Properties.Resources.PgListUserRightsVM_DataTables, Source.Tables.Where(w => w.Type == DataAccess.AbstractSource.Model.ETableType.Data).ToList()}
                };
            }
        }
        #endregion Коллекции

        #region Свойства
        public bool HasChanges { get { return _rightsList.Count(ur => ur.IsChanged == true) != 0; } }
        /// <summary>
        /// Получает или устанавливает таблицу, для которой загружаются права пользователей
        /// </summary>
        public PgTableBaseM Table
        {
            get { return _table;  }
            set
            {
                OnPropertyChanged(ref _table, value, () => this.Table);
            }
        }
        /// <summary>
        /// Получает или устанавливает пользователя, для которого загружаются права на таблицу
        /// </summary>
        public PgUserM User
        {
            get { return _user; }
            set
            {
                OnPropertyChanged(ref _user, value, () => this.User);
            }
        }
        /// <summary>
        /// Получает или устанавливает действие, для которого загружаются права на пользователя
        /// </summary>
        public PgActionM Action
        {
            get { return _action; }
            set
            {
                OnPropertyChanged(ref _action, value, () => this.Action);
            }
        }
        /// <summary>
        /// Получает или устанавливает возможность чтения для всех таблиц/пользователей
        /// </summary>
        public bool? CanAllRead
        {
            get { return _canAllRead; }
            set
            {
                if (_canAllRead == value)
                {
                    return;
                }
                OnPropertyChanged(ref _canAllRead, value, () => this.CanAllRead);
            }

            //get
            //{
            //    int canReadCount = 0; 
            //    foreach (PgUserRightsM right in _rightsList)
            //    {
            //        if (right.CanRead)
            //        {
            //            canReadCount++; 
            //        }
            //    }
            //    if (canReadCount == 0)
            //    {
            //        return false; 
            //    }
            //    else if (canReadCount < _rightsList.Count)
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        return true; 
            //    }
            //}
            //set 
            //{
            //    if (value == null)
            //    {
            //        return; 
            //    }
            //    else
            //    {
            //        foreach (PgUserRightsM right in _rightsList)
            //        {
            //            right.CanRead = (bool)value; 
            //        }
            //    }
            //    OnPropertyChanged("CanAllRead");
            //}
        }
        /// <summary>
        /// Получает или устанавливает возможность записи для всех таблиц/пользователей
        /// </summary>
        public bool? CanAllWrite
        {
            get { return _canAllWrite; }
            set
            {
                if (_canAllWrite == value)
                {
                    return;
                }
                OnPropertyChanged(ref _canAllWrite, value, () => this.CanAllWrite);
            }
            //get
            //{
            //    int canWriteCount = 0;
            //    foreach (PgUserRightsM right in _rightsList)
            //    {
            //        if (right.CanWrite)
            //        {
            //            canWriteCount++;
            //        }
            //    }
            //    if (canWriteCount == 0)
            //    {
            //        return false;
            //    }
            //    else if (canWriteCount < _rightsList.Count)
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //}
            //set
            //{
            //    if (value == null)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        foreach (PgUserRightsM right in _rightsList)
            //        {
            //            right.CanWrite = (bool)value;
            //        }
            //    }
            //    OnPropertyChanged("CanAllWrite");
            //}
        }
        /// <summary>
        /// Получает или устанавливает возможность действия для всех таблиц/пользователей
        /// </summary>
        public bool? AllAllowed
        {
            get
            {
                int allAllowedCount = 0;
                foreach (PgActionRightM right in _actionRightsList)
                {
                    if (right.Allowed)
                    {
                        allAllowedCount++;
                    }
                }
                if (allAllowedCount == 0)
                {
                    return false;
                }
                else if (allAllowedCount < _actionRightsList.Count)
                {
                    return null;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                else
                {
                    foreach (PgActionRightM right in _actionRightsList)
                    {
                        right.Allowed = (bool)value;
                    }
                }
                OnPropertyChanged("AllAllowed");
            }
        }
        /// <summary>
        /// Источник к которому относится список прав
        /// </summary>
        public PgDataRepositoryVM Source
        {
            get { return _source; }
        }
        /// <summary>
        /// Пользователь, чьи права используются при слиянии (используется в привязках)
        /// </summary>
        public PgUserM UserToReplaceWith
        {
            get { return _userToReplaceWith; }
            set { OnPropertyChanged(ref _userToReplaceWith, value, () => this.UserToReplaceWith); }
        }
        /// <summary>
        /// Таблица, чьи права используются при слиянии (используется в привязках)
        /// </summary>
        public PgTableBaseM TableToReplaceWith
        {
            get { return _tableToReplaceWith; }
            set { OnPropertyChanged(ref _tableToReplaceWith, value, () => this.TableToReplaceWith); }
        }
        #endregion Свойства

        #region Обработчики
        void PgListUserRightsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Table":
                    {
                        GetTableRightsDB(_table);
                        break;
                    }
                case "User":
                    {
                        GetUserRightsDB(_user);
                        _tableTypeRights = new ObservableCollection<TableTypeRightsVM>
                        {
                            new TableTypeRightsVM(
                                Rekod.Properties.Resources.PgListUserRightsVM_MapLayers, 
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.MapLayer)
                            ),
                            new TableTypeRightsVM(
                                Rekod.Properties.Resources.PgListUserRightsVM_References,
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Catalog)
                            ),
                            new TableTypeRightsVM( 
                                Rekod.Properties.Resources.PgListUserRightsVM_Intervals,  
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Interval)
                            ),
                            new TableTypeRightsVM(
                                Rekod.Properties.Resources.PgListUserRightsVM_DataTables, 
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Data)
                            )
                        };
                        GetUserActionRightsDB(_user);
                        break;
                    }
                case "Action":
                    {
                        GetActionRightsDB(_action);
                        break;
                    }
                case "CanAllRead":
                    {
                        if (CanAllRead != null)
                        {
                            bool canAllRead = (bool)CanAllRead;
                            foreach (var right in Rights)
                            {
                                right.CanRead = canAllRead;
                            }
                        }
                        break; 
                    }
                case "CanAllWrite":
                    {
                        if (CanAllWrite != null)
                        {
                            bool canAllWrite = (bool)CanAllWrite;
                            foreach (var right in Rights)
                            {
                                right.CanWrite = canAllWrite;
                            }
                        }
                        break;
                    }
            }
        }
        void _rightsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PgUserRightsM userRight in e.OldItems)
                {
                    userRight.PropertyChanged -= new PropertyChangedEventHandler(userRight_PropertyChanged);
                }
            }
            if (e.NewItems != null)
            {
                foreach (PgUserRightsM userRight in e.NewItems)
                {
                    userRight.PropertyChanged += new PropertyChangedEventHandler(userRight_PropertyChanged);
                }
            }
        }
        void _actionRightsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("AllAllowed");
            if (e.NewItems != null)
            {
                foreach (PgActionRightM userActionRight in e.NewItems)
                {
                    userActionRight.PropertyChanged += new PropertyChangedEventHandler(userActionRight_PropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (PgActionRightM userActionRight in e.OldItems)
                {
                    userActionRight.PropertyChanged -= new PropertyChangedEventHandler(userActionRight_PropertyChanged);
                }
            }
        }
        void userActionRight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("AllAllowed");
        }
        void userRight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CanRead":
                    {
                        UpdateCanAllRead();
                        break;
                    }
                case "CanWrite":
                    {
                        UpdateCanAllWrite();
                        break;
                    }
            }
        }
        #endregion Обработчики

        #region Методы
        private void resetRightsChangeStates()
        {
            foreach (var ur in _rightsList)
                ur.Reset();
        }
        private void resetActionRightsChangeStates()
        {
            foreach (var ur in _actionRightsList)
                ur.Reset();
        }

        /// <summary>
        /// Получает все зависимые таблицы для данной
        /// </summary>
        public List<AbsM.ITableBaseM> GetRefTablesDB(int id_table)
        {
            List<AbsM.ITableBaseM> ref_tables = new List<AbsM.ITableBaseM>();
            using (SqlWork sqlCmd = new SqlWork(Source.Connect))
            {
                sqlCmd.sql = "SELECT sys_scheme.get_ref_tables(" + id_table + ")";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int id = sqlCmd.GetInt32(0);
                    var table_base = Source.Tables.FirstOrDefault(w => (int)w.Id == id);
                    if (table_base == null)
                        continue;
                    ref_tables.Add(table_base);
                }
            }
            return ref_tables;
        }

        /// <summary>
        /// Загрузка/обновление прав для пользователя
        /// </summary>
        /// <param name="user">Пользователь для которого загружаются/обновляются права</param>
        public void GetUserRightsDB(PgUserM user)
        {
            if (user.ID == null)
                return;

            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                sqlCmd.sql = @"SELECT ti.id as id, 
                                    r.id as right_id,  
                                    COALESCE(r.read_data, FALSE) as read_data, 
                                    COALESCE(r.write_data, FALSE) as write_data    
                                    FROM
                                        sys_scheme.table_info ti 
                                        LEFT OUTER JOIN  
                                            sys_scheme.table_right r  
                                        	ON r.id_table = ti.id and id_user = " + user.ID;
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int id_table = sqlCmd.GetInt32("id");
                    PgUserRightsM right = _rightsList.FirstOrDefault(w => w.TableId == id_table);
                    if (right == null)
                        continue;
                    right.User = user;
                    int id = sqlCmd.GetInt32("right_id");
                    if (id != 0)
                        right.ID = id;
                    right.UpdateRef = false;
                    right.CanRead = sqlCmd.GetBoolean("read_data");
                    right.CanWrite = sqlCmd.GetBoolean("write_data");
                    right.UpdateRef = true;
                }
                sqlCmd.Close();
            }

            //foreach (var right in _rightsList)
            //{
            //    foreach (PgUserRightsM ref_right in right.RefTables)
            //    {
            //        var ref_r = ref_right;
            //        ref_r = _rightsList.FirstOrDefault(w => w.TableId == ref_right.TableId);
            //    }
            //}
        }
        /// <summary>
        /// Загрузка/обновление прав пользователей на таблицу
        /// </summary>
        /// <param name="table">Таблица для которой загружаются/обновляются права</param>
        public void GetTableRightsDB(PgTableBaseM table)
        {
            var tempRights = new ObservableCollection<PgUserRightsM>();
            foreach (PgUserM user in _source.Users)
            {
                PgUserRightsM right = new PgUserRightsM();
                right.User = user;
                right.Table = table;
                tempRights.Add(right);
            }

            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                sqlCmd.sql = string.Format(@"
                        SELECT id, 
                            id_table, 
                            id_user, 
                            read_data, 
                            write_data
                        FROM {0}.{1} where id_table={2}",
                        "sys_scheme", "table_right", table.Id);

                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    int id = sqlCmd.GetInt32("id");
                    int id_user = sqlCmd.GetInt32("id_user");
                    bool canread = sqlCmd.GetBoolean("read_data");
                    bool canwrite = sqlCmd.GetBoolean("write_data");

                    PgUserRightsM right = tempRights.FirstOrDefault(r => r.User.ID == id_user);
                    if (right != null)
                    {
                        right.ID = id;
                        right.CanRead = canread;
                        right.CanWrite = canwrite;
                    }
                }
                sqlCmd.CloseReader();

                Rights.Clear();
                foreach (var item in tempRights)
                {
                    Rights.Add(item);
                }
            }
        }

        /// <summary>
        /// Загрузка/обновление прав на действия для пользователя
        /// </summary>
        /// <param name="user">Пользователь для которого загружаются/обновляются права</param>
        public void GetUserActionRightsDB(PgUserM user)
        {
            if (user.ID == null)
                return;

            var tempActionRights = new ObservableCollection<PgActionRightM>();

            using (SqlWork sqlWork = new SqlWork(_source.Connect))
            {
                sqlWork.sql = string.Format(@"
                    SELECT  a.id as id, name, a.table_data as table_data, a.name_visible as name_visible, a.operation as operation,
	                r.id as right_id, COALESCE(r.allowed, FALSE) as allowed
	                FROM 
		                sys_scheme.action a
		                LEFT OUTER JOIN 
			                sys_scheme.actions_users r 
			                ON r.id_action = a.id and id_user = {0}", user.ID);

                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    PgActionRightM right = new PgActionRightM(user, user.Source);
                    right.Action = new PgActionM(
                        Source,
                        sqlWork.GetInt32("id"),
                        sqlWork.GetString("name"),
                        sqlWork.GetString("name_visible"),
                        sqlWork.GetBoolean("table_data"),
                        sqlWork.GetBoolean("operation"));
                    right.Allowed = sqlWork.GetBoolean("allowed");
                    int id = sqlWork.GetInt32("right_id");
                    if (id != 0)
                        right.ID = id;
                    tempActionRights.Add(right);
                }
                sqlWork.Close();
            }

            ExtraFunctions.Sorts.SortList(_actionRightsList, tempActionRights);
        }

        /// <summary>
        /// Загрузка/обновление прав на действия для пользователя
        /// </summary>
        /// <param name="action">Пользователь для которого загружаются/обновляются права</param>
        public void GetActionRightsDB(PgActionM action)
        {
            var tempActionRights = new ObservableCollection<PgActionRightM>();

            foreach (PgUserM user in _source.Users)
            {
                PgActionRightM right = new PgActionRightM();
                right.User = user;
                right.Action = action;
                tempActionRights.Add(right);
            }

            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                if (action.ID != null)
                {
                    sqlCmd.sql = string.Format(@"
                        SELECT id, 
                            id_user, 
                            allowed
                        FROM {0}.{1} where id_action={2}",
                            "sys_scheme", "actions_users", action.ID);

                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        PgActionRightM right = tempActionRights.FirstOrDefault(r => r.User.ID == sqlCmd.GetInt32("id_user"));
                        right.ID = sqlCmd.GetInt32("id");
                        right.Allowed = sqlCmd.GetBoolean("allowed");
                    }
                    sqlCmd.CloseReader();
                }
                ExtraFunctions.Sorts.SortList(_actionRightsList, tempActionRights);
            }
        }


        /// <summary>
        /// Метод перезагружает из базы все права относительно пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="rightId">Идентификатор права</param>
        public ObservableCollection<PgUserRightsM> GetUserRights(PgUserM user, int? rightId = null)
        {
            ObservableCollection<PgUserRightsM> tempRights = getUserRights(user, rightId);
            //rightsList+=недостающие таблицы в списке
            foreach (var t in _source.Tables)
                if (tempRights.FirstOrDefault(ur => ur.Table == t) == null)
                    tempRights.Add(new PgUserRightsM() { User = user, Table = t as PgTableBaseM, TableType = t.Type, CanRead = false, CanWrite = false });
            //      resetRightsChangeStates();
            return tempRights;
        }
        private ObservableCollection<PgUserRightsM> getUserRights(PgUserM user, int? rightId = null)
        {
            var tempRights = new ObservableCollection<PgUserRightsM>();
            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                sqlCmd.sql = string.Format(@"
                        SELECT id, 
                            id_table, 
                            id_user, 
                            read_data, 
                            write_data
                        FROM {0}.{1} where id_user={2}",
                        "sys_scheme", "table_right", user.ID);
                if (rightId != null)
                {
                    sqlCmd.sql += " and id = " + rightId;
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        updateOrAddRightInCollection(tempRights, rightId, sqlCmd);
                        sqlCmd.CloseReader();
                    }
                    else
                    {
                        sqlCmd.CloseReader();
                        var right = _rightsList.FirstOrDefault(u => u.ID == rightId);
                        if (right != null)
                            _rightsList.Remove(right);
                    }
                }
                else
                {
                    int id;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        id = sqlCmd.GetInt32("id");
                        tempRights.Add(updateOrAddRightInCollection(tempRights, id, sqlCmd));
                    }
                    sqlCmd.CloseReader();
                    //       Rekod.Controllers.ExtraFunctions.SortList(_rightsList, tempRights);
                }
            }
            return tempRights;
        }

        /// <summary>
        /// Метод перезагружает из базы все права на действия относительно пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="rightId">Идентификатор права</param>
        public ObservableCollection<PgActionRightM> GetUserActionRights(PgUserM user, int? rightId = null)
        {
            ObservableCollection<PgActionRightM> tempRights = getUserActionRights(user, rightId);
            foreach (var action in _source.Actions)
                if (tempRights.FirstOrDefault(ur => ur.Action == action) == null)
                    tempRights.Add(new PgActionRightM() { User = user, Action = action as PgActionM, Allowed = false });
            //      resetActionRightsChangeStates();
            return tempRights;
        }
        private ObservableCollection<PgActionRightM> getUserActionRights(PgUserM user, int? actionRightId = null)
        {
            var tempActionRights = new ObservableCollection<PgActionRightM>();
            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                sqlCmd.sql = string.Format(@"
                        SELECT id, 
                            id_action,
                            allowed
                        FROM {0}.{1} where id_user={2}",
                        "sys_scheme", "actions_users", user.ID);
                if (actionRightId != null)
                {
                    sqlCmd.sql += " and id = " + actionRightId;
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        updateOrAddActionRightInCollection(tempActionRights, actionRightId, sqlCmd);
                        sqlCmd.CloseReader();
                    }
                    else
                    {
                        sqlCmd.CloseReader();
                        var right = _actionRightsList.FirstOrDefault(u => u.ID == actionRightId);
                        if (right != null)
                            _actionRightsList.Remove(right);
                    }
                }
                else
                {
                    int id;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        id = sqlCmd.GetInt32("id");
                        tempActionRights.Add(updateOrAddActionRightInCollection(tempActionRights, id, sqlCmd));
                    }
                    sqlCmd.CloseReader();
                    //    Rekod.Controllers.ExtraFunctions.SortList(_actionRightsList, tempActionRights);
                }
            }
            return tempActionRights;
        }

        /// <summary>
        /// Метод перезагружает из базы права относительно таблицы
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="rightId">Идентификатор права</param>
        /// <returns></returns>
        public ObservableCollection<PgUserRightsM> GetTableRights(DataAccess.AbstractSource.Model.ITableBaseM table, int? rightId = null)
        {
            ObservableCollection<PgUserRightsM> tempRights = getTableRights(table, rightId);
            //rightsList+=недостающие юзеры в списке
            foreach (var u in _source.Users)
                if (tempRights.FirstOrDefault(ur => ur.User == u) == null)
                    tempRights.Add(new PgUserRightsM() { User = u, Table = table as PgTableBaseM, CanRead = false, CanWrite = false });
            //    resetRightsChangeStates();
            return tempRights;
        }
        private ObservableCollection<PgUserRightsM> getTableRights(DataAccess.AbstractSource.Model.ITableBaseM table, int? rightId = null)
        {
            ObservableCollection<PgUserRightsM> tempRights = new ObservableCollection<PgUserRightsM>();
            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                sqlCmd.sql = string.Format(@"
                        SELECT id, 
                            id_table, 
                            id_user, 
                            read_data, 
                            write_data
                        FROM {0}.{1} where id_table={2}",
                        "sys_scheme", "table_right", table.Id);
                if (rightId != null)
                {
                    sqlCmd.sql += " and id = " + rightId;
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        updateOrAddRightInCollection(tempRights, rightId, sqlCmd);
                        sqlCmd.CloseReader();
                    }
                    else
                    {
                        sqlCmd.CloseReader();
                        var right = tempRights.FirstOrDefault(u => u.ID == rightId);
                        if (right != null)
                            tempRights.Remove(right);
                    }
                }
                else
                {
                    int id;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        id = sqlCmd.GetInt32("id");
                        tempRights.Add(updateOrAddRightInCollection(tempRights, id, sqlCmd));
                    }
                    sqlCmd.CloseReader();
                    //      Rekod.Controllers.ExtraFunctions.SortList(_rightsList, tempRights);
                }
            }
            return tempRights;
        }
        /// <summary>
        /// Проверяет есть ли объект с таким ID в коллекции. Если есть обновляет иначе добавляет
        /// </summary>
        /// <param name="userId">ID объекта</param>
        /// <param name="sqlCmd">Открытый читатель SQL откуда нужно взять данные</param>
        private PgUserRightsM updateOrAddRightInCollection(ObservableCollection<PgUserRightsM> collection, int? rightId, SqlWork sqlCmd)
        {
            var right = collection.FirstOrDefault(ur => ur.ID == rightId);
            if (right == null)
            {
                right = new DataAccess.SourcePostgres.Model.PgUserRightsM();
                collection.Add(right);
            }
            right.ID = sqlCmd.GetInt32("id");
            right.CanRead = sqlCmd.GetBoolean("read_data");
            right.CanWrite = sqlCmd.GetBoolean("write_data");
            right.User = _users.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_user"));
            if (right.User == null)
            {
                _source.UpdateUsers();
                right.User = _users.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_user"));
            }
            right.Table = _source.FindTable(sqlCmd.GetInt32("id_table")) as PgTableBaseM;
            if (right.Table == null)
            {
                _source.UpdateTables();
                right.Table = _source.FindTable(sqlCmd.GetInt32("id_table")) as PgTableBaseM;
            }
            right.TableType = right.Table.Type;
            return right;
        }
        /// <summary>
        /// Проверяет есть ли объект с таким ID в коллекции. Если есть обновляет иначе добавляет
        /// </summary>
        /// <param name="userId">ID объекта</param>
        /// <param name="sqlCmd">Открытый читатель SQL откуда нужно взять данные</param>
        private PgActionRightM updateOrAddActionRightInCollection(ObservableCollection<PgActionRightM> collection, int? rightId, SqlWork sqlCmd)
        {
            var actionRight = collection.FirstOrDefault(ur => ur.ID == rightId);
            if (actionRight == null)
            {
                actionRight = new DataAccess.SourcePostgres.Model.PgActionRightM();
                collection.Add(actionRight);
            }
            actionRight.ID = sqlCmd.GetInt32("id");
            actionRight.Allowed = sqlCmd.GetBoolean("allowed");
            actionRight.User = _users.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_user"));
            if (actionRight.User == null)
            {
                _source.UpdateUsers();
                actionRight.User = _users.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_user"));
            }
            actionRight.Action = _actions.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_action")) as PgActionM;
            if (actionRight.Action == null)
            {
                _source.UpdateActions();
                actionRight.Action = _actions.FirstOrDefault(u => u.ID == sqlCmd.GetInt32("id_action")) as PgActionM;
            }
            return actionRight;
        }
        /// <summary>
        /// Метод который сделает либо update в базу либо insert и обновит коллекцию
        /// </summary>
        /// <param name="user">Пользователь что нужно либо обновить либо добавить в базе</param>
        public void InsertUpdateUserRight(PgUserRightsM userRight)
        {
            int id = -1;
            using (var sqlCmd = new SqlWork(_source.Connect))
            {
                var prms = new Params[5];
                prms[0] = new Params() { paramName = "id", type = DbType.Int32, typeData = NpgsqlTypes.NpgsqlDbType.Integer, value = userRight.ID };
                prms[1] = new Params() { paramName = "id_table", type = DbType.Int32, typeData = NpgsqlTypes.NpgsqlDbType.Integer, value = userRight.TableId };
                prms[2] = new Params() { paramName = "id_user", type = DbType.Int32, typeData = NpgsqlTypes.NpgsqlDbType.Integer, value = userRight.User.ID };
                prms[3] = new Params() { paramName = "read_data", type = DbType.Boolean, typeData = NpgsqlTypes.NpgsqlDbType.Boolean, value = userRight.CanRead };
                prms[4] = new Params() { paramName = "write_data", type = DbType.Boolean, typeData = NpgsqlTypes.NpgsqlDbType.Boolean, value = userRight.CanWrite };
                if (userRight.ID == null)
                {
                    _rightsList.Remove(userRight);
                    sqlCmd.sql =
                        string.Format(@"
                        INSERT INTO {0}.{1}
                        (
                            id_table, 
                            id_user, 
                            read_data, 
                            write_data
                        )
                        VALUES
                        (
                            :id_table, 
                            :id_user, 
                            :read_data, 
                            :write_data
                        ) returning id;", "sys_scheme", "table_right");
                    sqlCmd.ExecuteReader(prms);
                    if (sqlCmd.CanRead())
                        id = sqlCmd.GetInt32(0);
                    sqlCmd.CloseReader();
                }
                else
                {
                    sqlCmd.sql = string.Format(@"
                        update {0}.{1} set
                            id_table=:id_table,
                            id_user=:id_user,
                            read_data=:read_data,
                            write_data=:write_data
                        where id=:id;", "sys_scheme", "table_right");
                    sqlCmd.ExecuteReader(prms);
                    sqlCmd.Close();
                    id = (int)userRight.ID;
                }
            }
            getUserRights(userRight.User, id);
        }
        /// <summary>
        /// Удаляет пользователя из базы и обновляет коллекцию
        /// </summary>
        /// <param name="user">Пользователь ждя уничтожения</param>
        public void DeleteUserRight(PgUserRightsM userRight)
        {
            using (SqlWork sqlCmd = new SqlWork(_source.Connect)
            {
                sql = string.Format(@"delete from {0}.{1} where id={2}",
                "sys_scheme", "table_right", userRight.ID)
            })
            {
                sqlCmd.ExecuteReader();
                sqlCmd.Close();
                getUserRights(userRight.User, userRight.ID);
            }
        }
        public PgActionRightM FindActionRight(string name)
        {
            return ActionRights.FirstOrDefault(f => f.Action.Name == name);
        }
        public PgActionRightM FindActionRight(int idAction)
        {
            return ActionRights.FirstOrDefault(f => f.Action.ID == idAction);
        }

        public void UpdateCanAllRead()
        {
            if (Rights.Count == 0)
            {
                CanAllRead = false;
            }
            else
            {
                int trueCount = 0;
                foreach (var right in Rights)
                {
                    if (right.CanRead)
                    {
                        trueCount++;
                    }
                }
                if (trueCount == 0)
                {
                    CanAllRead = false;
                }
                else if (Rights.Count == trueCount)
                {
                    CanAllRead = true;
                }
                else
                {
                    CanAllRead = null;
                }
            }
        }
        public void UpdateCanAllWrite()
        {
            if (Rights.Count == 0)
            {
                CanAllWrite = false;
            }
            else
            {
                int trueCount = 0;
                foreach (var right in Rights)
                {
                    if (right.CanWrite)
                    {
                        trueCount++;
                    }
                }
                if (trueCount == 0)
                {
                    CanAllWrite = false;
                }
                else if (Rights.Count == trueCount)
                {
                    CanAllWrite = true;
                }
                else
                {
                    CanAllWrite = null;
                }
            }
        }
        #endregion Методы

        #region Команды
        #region ReloadCommand
        /// <summary>
        /// Команда для обновления прав из базы
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        /// <summary>
        /// Обновление прав из базы
        /// </summary>
        public void Reload(object parameter = null)
        {
            if (_table != null)
            {
                GetTableRightsDB(_table);
            }
            if (_user != null)
            {
                GetUserRightsDB(_user);
                _tableTypeRights = new ObservableCollection<TableTypeRightsVM>
                        {
                            new TableTypeRightsVM(
                                Rekod.Properties.Resources.PgListUserRightsVM_MapLayers,
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.MapLayer)
                            ),
                            new TableTypeRightsVM( 
                                Rekod.Properties.Resources.PgListUserRightsVM_References,
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Catalog)
                            ),
                            new TableTypeRightsVM( 
                                Rekod.Properties.Resources.PgListUserRightsVM_Intervals,
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Interval)
                            ),
                            new TableTypeRightsVM(
                                Rekod.Properties.Resources.PgListUserRightsVM_DataTables,
                                _rightsList.Where(w => w.TableType == DataAccess.AbstractSource.Model.ETableType.Data)
                            )
                        };
                GetUserActionRightsDB(_user);

            }
            if (_action != null)
            {
                GetActionRightsDB(_action);
            }
            UpdateCanAllRead();
            UpdateCanAllWrite();
        }
        /// <summary>
        /// Можно ли обновить права
        /// </summary>
        public bool CanReload(object parameter = null)
        {
            return true;
        }
        #endregion // ReloadCommand

        #region SaveChangesCommand
        /// <summary>
        /// Команда для сохранения изменений
        /// </summary>
        public ICommand SaveChangesCommand
        {
            get { return _saveChangesCommand ?? (_saveChangesCommand = new RelayCommand(this.SaveChanges, this.CanSaveChanges)); }
        }
        /// <summary>
        /// Сохранение изменений
        /// </summary>
        public void SaveChanges(object parameter = null)
        {
            bool canSave = true;
            String cantSaveMessage = null;
            if (_rightsList != null)
            {
                foreach (var right in _rightsList)
                {
                    if (right.RefTables != null && right.RefTables.Count > 0)
                    {
                        foreach (var refright in right.RefTables)
                        {
                            if (!refright.CanRead)
                            {
                                var linkedTables = from PgUserRightsM pgr in _rightsList
                                                   where
                                                       pgr.RefTables != null && pgr.RefTables.Contains(refright) && pgr.CanRead
                                                   select String.Format("\"{0}\"", pgr.TableText);
                                if (linkedTables.Count() > 0)
                                {
                                    canSave = false;
                                    cantSaveMessage =
                                        String.Format("Необходимо установить право на чтение для таблицы \"{0}\", так как следующие таблицы ссылаются на нее:\n{1}",
                                            refright.TableText,
                                            String.Join("\n", linkedTables.ToArray()));
                                    break;
                                }
                            }
                        }
                    }
                    if (canSave == false)
                    {
                        break;
                    }
                }
            }
            if (canSave)
            {
                String setRightSql = "";
                foreach (PgUserRightsM right in _rightsList)
                {
                    setRightSql += String.Format("SELECT sys_scheme.set_right({0}, {1}, {2}, {3});\n",
                                                            right.TableId,
                                                            right.User.ID,
                                                            right.CanRead,
                                                            right.CanWrite);
                }
                if (!String.IsNullOrEmpty(setRightSql))
                {
                    using (SqlWork sqlWork = new SqlWork(_source.Connect))
                    {
                        sqlWork.sql = setRightSql;
                        sqlWork.ExecuteNonQuery();
                    }
                }
            }
            else 
            {
                if (!String.IsNullOrEmpty(cantSaveMessage))
                {
                    MessageBox.Show(cantSaveMessage, "Сохранение невозможно", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Команда для сохранения изменений
        /// </summary>
        public ICommand SaveChangesACommand
        {
            get { return _saveChangesACommand ?? (_saveChangesACommand = new RelayCommand(this.SaveChangesA, this.CanSaveChanges)); }
        }
        /// <summary>
        /// Сохранение изменений
        /// </summary>
        public void SaveChangesA(object parameter = null)
        {
            String setRightSql = "";
            foreach (PgActionRightM right in _actionRightsList)
            {
                setRightSql += String.Format("SELECT sys_scheme.set_action_right({0}, {1}, {2});\n",
                                                        right.Action.ID,
                                                        right.User.ID,
                                                        right.Allowed);
            }
            if (!String.IsNullOrEmpty(setRightSql))
            {
                using (SqlWork sqlWork = new SqlWork(_source.Connect))
                {
                    sqlWork.sql = setRightSql;
                    sqlWork.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Можно ли сохранить изменения
        /// </summary>
        public bool CanSaveChanges(object parameter = null)
        {
            return true;
        }
        #endregion // SaveChangesCommand

        #region FilterDefaultViewCommand
        /// <summary>
        /// Команда для фильтрации ICollectionView по умолчанию списка пользователей
        /// </summary>
        public ICommand FilterDefaultViewCommand
        {
            get { return _filterDefaultViewCommand ?? (_filterDefaultViewCommand = new RelayCommand(this.FilterDefaultView, this.CanFilterDefaultView)); }
        }
        /// <summary>
        /// Фильтрация ICollectionView по умолчанию списка пользователей
        /// </summary>
        public void FilterDefaultView(object parameter = null)
        {
            if (parameter == null || Users == null)
            {
                return;
            }
            ICollectionView defView = CollectionViewSource.GetDefaultView(Users);
            if (defView != null)
            {
                defView.Filter = delegate(object o)
                {
                    String fio = (o as PgUserM).NameFull.ToUpper();
                    String login = (o as PgUserM).Login.ToUpper();
                    String searchtext = parameter.ToString().ToUpper();
                    return (fio.Contains(searchtext) || login.Contains(searchtext));
                };
            }
        }
        /// <summary>
        /// Можно ли отфильтровать список пользователей
        /// </summary>
        public bool CanFilterDefaultView(object parameter = null)
        {
            if (parameter == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion // FilterDefaultViewCommand

        #region MergeRightsCommand
        #region Merge user rights
        /// <summary>
        /// Команда для слияния прав
        /// </summary>
        public ICommand MergeUserRightsCommand
        {
            get { return _mergeUserRightsCommand ?? (_mergeUserRightsCommand = new RelayCommand(this.MergeUserRights, this.CanUserMergeRights)); }
        }
        /// <summary>
        /// Слияние прав
        /// </summary>
        public void MergeUserRights(object parameter = null)
        {
            bool addRigths = (bool)parameter;
            PgUserM pgUserSource = UserToReplaceWith as PgUserM;
            if (pgUserSource != null && User != null)
            {
                ObservableCollection<PgUserRightsM> rightsListToMerge = null;
                PgUserM curUser = pgUserSource.Source.CurrentUser;
                if (curUser != null && curUser.Type == UserType.Admin)
                {
                    rightsListToMerge = GetUserRights(pgUserSource);
                }
                foreach (PgUserRightsM rightSource in rightsListToMerge)
                {
                    PgUserRightsM right = Rights.FirstOrDefault(p => p.TableId == rightSource.TableId);
                    if (right != null)
                    {
                        if (addRigths || !addRigths && rightSource.CanRead)
                        {
                            right.CanRead = rightSource.CanRead;
                        }
                        if (addRigths || !addRigths && rightSource.CanWrite)
                        {
                            right.CanWrite = rightSource.CanWrite;
                        }
                    }
                }
            }
            UserToReplaceWith = null;
        }
        /// <summary>
        /// Можно ли слиять права
        /// </summary>
        public bool CanUserMergeRights(object parameter = null)
        {
            return (UserToReplaceWith != null && User != null);
        }
        #endregion Merge user rights

        #region Merge user action rights
        /// <summary>
        /// Команда для слияния прав
        /// </summary>
        public ICommand MergeUserActionRightsCommand
        {
            get { return _mergeUserActionRightsCommand ?? (_mergeUserActionRightsCommand = new RelayCommand(this.MergeUserActionRights, this.CanUserMergeRights)); }
        }
        /// <summary>
        /// Слияние прав
        /// </summary>
        public void MergeUserActionRights(object parameter = null)
        {
            bool addRigths = (bool)parameter;
            PgUserM pgUserSource = UserToReplaceWith as PgUserM;
            if (pgUserSource != null && User != null)
            {
                ObservableCollection<PgActionRightM> rightsListToMerge = null;
                PgUserM curUser = pgUserSource.Source.CurrentUser;
                if (curUser != null && curUser.Type == UserType.Admin)
                {
                    rightsListToMerge = GetUserActionRights(pgUserSource);
                }
                foreach (PgActionRightM rightSource in rightsListToMerge)
                {
                    PgActionRightM right = ActionRights.FirstOrDefault(p => p.Action.ID == rightSource.Action.ID);
                    if (right != null)
                    {
                        if (addRigths || !addRigths && rightSource.Allowed)
                        {
                            right.Allowed = rightSource.Allowed;
                        }
                    }
                }
            }
            UserToReplaceWith = null;
        }

        #endregion Merge user action rights

        #region Merge table rights
        /// <summary>
        /// Команда для слияния прав
        /// </summary>
        public ICommand MergeTableRightsCommand
        {
            get { return _mergeTableRightsCommand ?? (_mergeTableRightsCommand = new RelayCommand(this.MergeTableRights, this.CanTableMergeRights)); }
        }
        /// <summary>
        /// Слияние прав
        /// </summary>
        public void MergeTableRights(object parameter = null)
        {
            bool addRigths = (bool)parameter;
            PgTableBaseM pgTableSource = TableToReplaceWith as PgTableBaseM;
            if (pgTableSource != null && Table != null)
            {
                ObservableCollection<PgUserRightsM> rightsListToMerge = null;
                PgUserM curUser = Source.CurrentUser;
                if (curUser != null && curUser.Type == UserType.Admin)
                {
                    rightsListToMerge = GetTableRights(pgTableSource);
                }
                foreach (PgUserRightsM rightSource in rightsListToMerge)
                {
                    PgUserRightsM right = Rights.FirstOrDefault(p => p.User.ID == rightSource.User.ID);
                    if (right != null)
                    {
                        if (addRigths || !addRigths && rightSource.CanRead)
                        {
                            right.CanRead = rightSource.CanRead;
                        }
                        if (addRigths || !addRigths && rightSource.CanWrite)
                        {
                            right.CanWrite = rightSource.CanWrite;
                        }
                    }
                }
            }
            TableToReplaceWith = null;
        }
        /// <summary>
        /// Можно ли слиять права
        /// </summary>
        public bool CanTableMergeRights(object parameter = null)
        {
            return (TableToReplaceWith != null && Table != null);
        }
        #endregion Merge table rights
        #endregion // MergeRightsCommand

        /// <summary>
        /// Команда на добавление пользователя
        /// </summary>
        public ICommand AddNewUserCommand
        {
            get { return _addNewUserCommand ?? (_addNewUserCommand = new RelayCommand(this.AddNewUser)); }
        }
        /// <summary>
        /// Добавление пользователя
        /// </summary>
        public void AddNewUser(object parameter = null)
        {
            //PgVR.AddUserV addFrm = new PgVR.AddUserV();
            //BindingProxy binding = new BindingProxy();
            //binding.Data = new PgUserM(Source);
            //addFrm.DataContext = binding;
            //addFrm.ShowDialog();
            //_users = Source.Users;

            BindingProxy bindProxy = parameter as BindingProxy;
            bindProxy.Data = null;
            bindProxy.Data = new PgUserM(Source);
        }

        /// <summary>
        /// Команда на удаление пользователя
        /// </summary>
        public ICommand DeleteUserCommand
        {
            get { return _deleteUserCommand ?? (_deleteUserCommand = new RelayCommand(this.DeleteUser)); }
        }
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        public void DeleteUser(object parameter = null)
        {
            if (parameter == null || parameter is BindingProxy == false
                || ((BindingProxy)parameter).Data is PgUserM == false)
                return;
            var deletingUser = ((BindingProxy)parameter).Data as PgUserM;
            MessageBoxResult dr = MessageBox.Show("Вы действительно хотите удалить пользователя " + deletingUser.NameFull + "?",
                    "Удаление пользователя", MessageBoxButton.YesNo);
            if (dr == MessageBoxResult.Yes)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "DELETE FROM " + Program.scheme + ".actions_users WHERE id_user = " + deletingUser.ID;
                    sqlCmd.ExecuteNonQuery();
                }
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "DELETE FROM " + Program.scheme + ".user_db WHERE id = " + deletingUser.ID;
                    sqlCmd.ExecuteNonQuery();
                }
                Source.UpdateUsers();
                _users = Source.Users;
            }
        }
        #endregion Команды
    }
}