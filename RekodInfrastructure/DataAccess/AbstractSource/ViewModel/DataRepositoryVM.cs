using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Interfaces;
using Npgsql;
using System.Xml;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using TmM = Rekod.DataAccess.TableManager.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Behaviors;

namespace Rekod.DataAccess.AbstractSource.ViewModel
{
    /// <summary>
    /// Абстрактный класс работы с источниками
    /// </summary>
    public abstract class DataRepositoryVM : WindowViewModelBase_VM, AbsM.IDataRepositoryM, IDisposable, IEquatable<AbsM.IDataRepositoryM>
    {
        #region Статические члены
        private static int _idSource;
        static DataRepositoryVM()
        {
            _idSource = 0;
        }
        #endregion // Статические члены

        #region Поля
        protected AxmvMapLib.AxMapLIb _mv;
        protected TmM.ITableManagerVM _source;
        private AbsM.ERepositoryType _type;
        private String _text;
        protected Dictionary<object, AbsM.ITableBaseM> _hashTables = null;
        protected ObservableCollection<AbsM.ITableBaseM> _tables = null;
        protected ObservableCollection<AbsM.IGroupM> _groups = null;
        protected ObservableCollection<AbsM.ILayerM> _layers = null;
        private Dictionary<object, AbsM.IGroupM> _hashGroups;
        private Dictionary<WindowViewModelBase_VM, Window> _hasVMWindow;
        private ICommand _openTableCommand;
        private int _id;
        private bool _isTable;
        #endregion // Поля

        #region Конструктор
        public DataRepositoryVM(TmM.ITableManagerVM source, AbsM.ERepositoryType type, bool isTable)
        {
            _source = source;
            if (_source != null)
                _mv = source.mv;
            else
                _mv = Program.mainFrm1.axMapLIb1;

            _isTable = isTable;
            _type = type;
            _id = ++_idSource;
            _hasVMWindow = new Dictionary<WindowViewModelBase_VM, Window>();

            _hashTables = new Dictionary<object, AbsM.ITableBaseM>();

            _layers = new ObservableCollection<AbsM.ILayerM>();
            ICollectionView defView = CollectionViewSource.GetDefaultView(_layers);
            defView.Filter = delegate(object o)
            {
                return !(o as AbsM.TableBaseM).IsHidden;
            };

            _groups = new ObservableCollection<AbsM.IGroupM>();
            _hashGroups = new Dictionary<object, AbsM.IGroupM>();
            _tables = new ObservableCollection<AbsM.ITableBaseM>();


            _tables.CollectionChanged += Tables_CollectionChanged;
            _groups.CollectionChanged += Groups_CollectionChanged;
        }
        #endregion // Конструктор

        #region Свойства
        /// <summary>
        /// Тип источника
        /// </summary>
        public AbsM.ERepositoryType Type
        { get { return _type; } }
        /// <summary>
        /// Идентификатор источника
        /// </summary>
        public int Id
        { get { return _id; } }
        /// <summary>
        /// Название источника
        /// </summary>
        public String Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        public String ClassType
        { get { return "Repository"; } }

        public bool IsTable
        { get { return _isTable; } }

        public AxmvMapLib.AxMapLIb MapViewer
        {
            get { return _mv; }
            set { _mv = value; }
        }
        #endregion // Свойства

        #region События
        public event NotifyCollectionChangedEventHandler GroupsCollectionChanged
        {
            add { lock (this) { _groups.CollectionChanged += value; } }
            remove { lock (this) { _groups.CollectionChanged -= value; } }
        }
        public event NotifyCollectionChangedEventHandler TablesCollectionChanged
        {
            add { lock (this) { _tables.CollectionChanged += value; } }
            remove { lock (this) { _tables.CollectionChanged -= value; } }
        }
        public event PropertyChangedEventHandler TablePropertyChanged;
        #endregion

        #region Коллекции
        public IEnumerable<AbsM.ITableBaseM> Tables
        {
            get { return _tables; }
        }
        public IEnumerable<AbsM.IGroupM> Groups
        {
            get { return _groups; }
        }
        /// <summary>
        /// Коллекция всех слоев в источнике.
        /// </summary>
        /// <remarks>
        /// Для представления по умолчанию создается фильтр который скрывает слои со свойством IsHidden равное true
        /// </remarks>
        public IEnumerable<AbsM.ILayerM> Layers
        {
            get
            {
                if (_layers == null)
                {
                    _layers = new ObservableCollection<AbsM.ILayerM>();
                    ICollectionView defView = CollectionViewSource.GetDefaultView(_layers);
                    defView.Filter = delegate(object o)
                    {
                        return !(o as AbsM.TableBaseM).IsHidden;
                    };
                }
                return _layers;
            }
        }

        #region Коллекции AbsM.IDataRepositoryM
        IEnumerable<AbsM.IGroupM> AbsM.IDataRepositoryM.Groups
        {
            get { return this.Groups; }
        }
        IEnumerable<AbsM.ITableBaseM> AbsM.IDataRepositoryM.Tables
        {
            get { return this.Tables; }
        }
        #endregion // Коллекции AbsM.IDataRepositoryM
        #endregion // Коллекции

        #region Методы

        /// <summary>
        /// Изменяет позицию таблицы в группе. 
        /// Не чего не происходит если:
        /// Таблицы нет или позиция совпадает с текущей
        /// </summary>
        /// <param name="group">Ссылка на группу</param>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="index">Новая позиция</param>
        protected virtual void MGroupMoveTable(AbsM.IGroupM group, AbsM.ILayerM table, int index)
        {
            var pgGroup = group as AbsM.GroupM;
            if (pgGroup == null)
                return;
            var tables = AbsM.GroupM.GetTables(pgGroup);
            if (group.Tables.Contains(table))
            {
                int old_index = tables.IndexOf(table);
                if (old_index != index)
                    if (index < tables.Count)
                        tables.Move(old_index, index);
                    else
                        Debug.Fail("Неправильно указан индекс", "MoveTableIntoGroup");
            }
        }
        /// <summary>
        /// Добавление таблицы в группу. Если таблица уже есть в группе ничего не происходит
        /// </summary>
        /// <param name="group">Ссылка на группу</param>
        /// <param name="table">Ссылка на таблицу</param>
        /// <pparam name="index">Новая позиция</pparam>
        protected virtual void MGroupAddTable(AbsM.IGroupM group, AbsM.ILayerM table, int? index = null)
        {
            var pgGroup = group as AbsM.GroupM;
            if (pgGroup == null)
                return;
            var tables = AbsM.GroupM.GetTables(pgGroup);
            if (!group.Tables.Contains(table))
            {
                if (index == null)
                    tables.Add(table);
                else
                {
                    if (index <= tables.Count)
                        tables.Insert(index.Value, table);
                    else
                        Debug.Fail("Неправильно указан индекс", "AddTableIntoGroup");
                }
            }
        }
        /// <summary>
        /// Удаление таблицы из группы. Если таблицы нет в группе ничего не происходит
        /// </summary>
        /// <param name="group">Ссылка на группу</param>
        /// <param name="table">Ссылка на таблицу</param>
        protected virtual void MGroupRemoveTable(AbsM.IGroupM group, AbsM.ILayerM table)
        {
            var pgGroup = group as AbsM.GroupM;
            if (pgGroup == null)
                return;
            var tables = AbsM.GroupM.GetTables(pgGroup);
            if (group.Tables.Contains(table))
            {
                tables.Remove(table);
            }
        }

        internal void SetInvertVisible(object param = null, bool? value = null)
        {
            if (CanSetInvertVisable(param) == false) return;
            AbsM.TableBaseM table = param as AbsM.TableBaseM;
            bool ExIsVisible = table.IsVisible;
            if (table.IsLayer == false)
                value = false;
            bool IsVisible = MakeLayerVisible(table, value);
            AbsM.TableBaseM.UpdateIsVisible(table, IsVisible);
            if (IsVisible == false)
                table.IsSelectable = false;
            if (ExIsVisible != IsVisible)
            {
                UpdateGroupVisibility();
            }
            _mv.mapRepaint();
        }
        internal virtual bool MakeLayerVisible(AbsM.TableBaseM table, bool? value)
        {
            return false;
        }
        internal virtual bool CanSetInvertVisable(object param = null)
        {
            return param is AbsM.TableBaseM;
        }

        internal void SetInvertSelectable(object param = null, bool? value = null)
        {
            if (CanSetInvertSelectable(param) == false) return;

            AbsM.TableBaseM table = param as AbsM.TableBaseM;
            if (table.IsVisible == false)
                value = false;
            bool IsSelectable = MakeLayerSelectable(table, value);
            AbsM.TableBaseM.UpdateIsSelectable(table, IsSelectable);
            if (IsSelectable == false)
                table.IsEditable = false;
        }
        internal virtual bool MakeLayerSelectable(AbsM.TableBaseM table, bool? value)
        {
            return false;
        }
        internal virtual bool CanSetInvertSelectable(object param = null)
        {
            return param is AbsM.TableBaseM;
        }

        internal virtual void SetInvertEditable(object param = null, bool? value = null)
        {
            if (CanSetInvertEditable(param) == false) return;

            AbsM.TableBaseM table = param as AbsM.TableBaseM;

            if (table.IsSelectable == false || table.IsReadOnly)
                value = false;
            bool IsEditable = MakeLayerEditable(table, value);
            AbsM.TableBaseM.UpdateIsEditable(table, IsEditable);
        }
        internal virtual bool MakeLayerEditable(AbsM.TableBaseM table, bool? value)
        {
            return false;
        }
        internal virtual bool CanSetInvertEditable(object param = null)
        {
            return param is AbsM.TableBaseM;
        }

        internal void SetGroupVisible(object param = null, bool? value = null)
        {
            AbsM.GroupM group = param as AbsM.GroupM;
            bool? IsVisible = MakeGroupVisible(group, value);
            AbsM.GroupM.UpdateIsVisible(group, IsVisible);
        }
        internal virtual bool? MakeGroupVisible(AbsM.GroupM group, bool? value)
        {
            if (value == false)
            {
                ObservableCollection<AbsM.TableBaseM> tempCol = new ObservableCollection<AbsM.TableBaseM>();
                foreach (AbsM.TableBaseM lr in group.Tables)
                {
                    tempCol.Add(lr);
                }
                foreach (AbsM.TableBaseM lr in tempCol)
                {
                    if (lr.IsVisible != false && !lr.IsHidden)
                    {
                        lr.IsVisible = false;
                    }
                }
                tempCol.Clear();
            }
            else if (value == true)
            {
                int visibleLayersCount = 0;
                int totalLayersCount = 0;
                try
                {
                    for (int i = 0; i < group.Tables.Count; i++)
                    {
                        if (group.Tables[i].IsVisible != true && !group.Tables[i].IsHidden)
                        {
                            group.Tables[i].IsVisible = true;
                        }
                        if (!group.Tables[i].IsHidden)
                        {
                            totalLayersCount++;
                            if (group.Tables[i].IsVisible)
                            {
                                visibleLayersCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Возникло исключение в методе MakeGroupVisible()\n" + ex.Message);
                    throw ex;
                }
                finally
                {
                    if (visibleLayersCount == 0)
                    {
                        value = false;
                    }
                    else if (visibleLayersCount < totalLayersCount)
                    {
                        value = null;
                    }
                    else if (visibleLayersCount == totalLayersCount)
                    {
                        value = true;
                    }
                }
            }
            else
            {
                value = null;
            }
            return value;
        }
        public void UpdateGroupVisibility()
        {
            foreach (AbsM.GroupM gr in Groups)
            {
                int visLayersCount = 0;
                int totalLayersCount = 0;
                foreach (AbsM.TableBaseM lr in gr.Tables)
                {
                    if (!lr.IsHidden)
                    {
                        totalLayersCount++;
                        if (lr.IsVisible)
                        {
                            visLayersCount++;
                        }
                    }
                }
                if (visLayersCount == 0)
                {
                    gr.IsVisible = false;
                }
                else if (visLayersCount == totalLayersCount)
                {
                    gr.IsVisible = true;
                }
                else
                {
                    gr.IsVisible = null;
                }
            }
        }

        public void OpenWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, WindowViewModelBase_VM ownerMV = null)
        {
            var newWindow = RegistredWindow(control, dataContext, width, height, minwidth, minheight, null, ownerMV);
            newWindow.Show();
        }
        public void OpenWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, Window ownerWindow)
        {
            var newWindow = RegistredWindow(control, dataContext, width, height, minwidth, minheight, null, null);
            newWindow.Owner = ownerWindow;
            newWindow.Show();
        }

        public object OpenWindowDialog(
            UserControl control,
            WindowViewModelBase_VM dataContext,
            double width,
            double height,
            double minwidth,
            double minheight,
            WindowViewModelBase_VM ownerMV = null)
        {
            object value = null;
            Action<object> close = delegate(object obj) { value = obj; };
            var newWindow = RegistredWindow(control, dataContext, width, height, minwidth, minheight, close, ownerMV);
            newWindow.ShowDialog();
            return value;
        }
        private Window RegistredWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, Action<object> close = null, WindowViewModelBase_VM ownerMV = null)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            Window newWindow;
            if (_hasVMWindow.ContainsKey(dataContext))
            {
                newWindow = _hasVMWindow[dataContext];
                newWindow.Activate();
            }
            else
            {
                Action<object> closeAct = delegate(object obj)
                {
                    _hasVMWindow.Remove(dataContext);
                    if (close != null)
                        close(obj);
                };
                var ownerWindow = (ownerMV != null && _hasVMWindow.ContainsKey(ownerMV))
                                    ? _hasVMWindow[ownerMV]
                                    : Program.WinMain;
                newWindow = WindowViewModelBase_VM.GetWindow(control, dataContext, width, height, minwidth, minheight, ownerWindow, closeAct);
                if (dataContext != null)
                {
                    newWindow.Title = dataContext.Title;
                }
                _hasVMWindow.Add(dataContext, newWindow);
            }
            return newWindow;
        }
        #region Интерфейс AbsM.IDataRepositoryM
        /// <summary>
        /// Проверка выполнения источника
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckRepository()
        {
            return true;
        }
        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public abstract void ReloadInfo();
        /// <summary>
        /// Найти таблицу по ID
        /// </summary>
        /// <param name="id">Идентификатор таблицы</param>
        /// <returns>Таблица</returns>
        public AbsM.ITableBaseM FindTable(object id)
        {
            if (_hashTables.ContainsKey(id))
                return _hashTables[id];
            return null;
        }
        /// <summary>
        /// Найти таблицу по имени
        /// </summary>
        /// <param name="name">Имя таблицы</param>
        /// <returns>Таблица</returns>
        public AbsM.ITableBaseM FindTableByName(string name)
        {
            return Tables.FirstOrDefault(w => w.Name == (string)name);
        }
        /// <summary>
        /// Поиск поля в таблце
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатор поля</param>
        /// <returns></returns>
        public virtual AbsM.IFieldM FindField(AbsM.ITableBaseM table, object id)
        {
            //todo-Yoda: сделать поиск поля по всем таблицам, а если таблица указана, то только по одной таблице
            return table.Fields.FirstOrDefault(f => f.Id == (int)id);
        }

        internal AbsM.IGroupM FindGroup(object id)
        {
            if (!_hashGroups.ContainsKey(id))
                return null;
            return _hashGroups[id];
        }
        public virtual object OpenTable(AbsM.ITableBaseM table, object id = null, bool isSelected = false, WindowViewModelBase_VM ownerMV = null)
        { return null; }
        public virtual void OpenObject(AbsM.ITableBaseM table, object id, String wkt = null, WindowViewModelBase_VM ownerMV = null)
        { return; }
        /// <summary>
        /// Показать окно настроек таблицы
        /// </summary>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        public virtual void OpenTableSettings(AbsM.ITableBaseM iTable, System.Windows.UIElement positionElement = null) 
        { }
        #endregion // Интерфейс AbsM.IDataRepositoryM

        #region Интерфейс IDisposable
        public virtual void Dispose()
        {
            foreach (var item in _hasVMWindow)
            {
                item.Key.CloseWindow();
            }
            _tables.Clear();
            _groups.Clear();

            _tables.CollectionChanged -= Tables_CollectionChanged;
            _groups.CollectionChanged -= Groups_CollectionChanged;
        }
        #endregion // Интерфейс IDisposable

        #region Интерфейс IEquatable<DataRepositoryVM>
        public override bool Equals(object other)
        {
            //Последовательность проверки должна быть именно такой.
            //Если не проверить на null объект other, то other.GetType() может выбросить //NullReferenceException.            
            if (other == null)
                return false;

            //Если ссылки указывают на один и тот же адрес, то их идентичность гарантирована.
            if (object.ReferenceEquals(this, other))
                return true;

            //Если класс находится на вершине иерархии или просто не имеет наследников, то можно просто
            //сделать Vehicle tmp = other as Vehicle; if(tmp==null) return false; 
            //Затем вызвать экземплярный метод, сразу передав ему объект tmp.
            //if (this.GetType() != other.GetType())
            //    return false;

            return this.Equals(other as AbsM.IDataRepositoryM);
        }
        public virtual bool Equals(AbsM.IDataRepositoryM other)
        {
            return object.ReferenceEquals(this, other);

        }
        #endregion // Интерфейс IEquatable<DataRepositoryVM>

        public void UpdateLayerItemsView()
        {
            Program.mainFrm1.layerItemsView1.RefreshLayers();
        }

        #endregion // Методы

        #region Обработчики
        /// <summary>
        /// Обработка событий изменения коллекции таблиц
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var table = e.NewItems[i] as AbsM.TableBaseM;
                        table.PropertyChanged += table_PropertyChanged;
                        _hashTables.Add(table.Id, table);
                        if (table.IsLayer)
                            _layers.Add((AbsM.ILayerM)table);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var table = e.OldItems[i] as AbsM.TableBaseM;
                        table.PropertyChanged -= table_PropertyChanged;
                        _hashTables.Remove(table.Id);
                        if (table.IsLayer)
                        {
                            _layers.Remove((AbsM.ILayerM)table);
                            for (int y = 0; y < Groups.Count(); y++)
                            {
                                MGroupRemoveTable(Groups.ElementAt(y), (AbsM.ILayerM)table);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in _hashTables)
                    {
                        var table = item.Value as AbsM.TableBaseM;
                        table.PropertyChanged -= table_PropertyChanged;
                    }
                    _hashTables.Clear();
                    _layers.Clear();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработка событий изменения таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void table_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (TablePropertyChanged != null)
                TablePropertyChanged(sender, e);
        }

        /// <summary>
        /// Обработка событий изменения коллекции групп
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var group = e.NewItems[i] as AbsM.GroupM;
                        _hashGroups.Add(group.Id, group);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var group = e.OldItems[i] as AbsM.GroupM;
                        _hashGroups.Remove(group.Id);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _hashGroups.Clear();
                    break;
                default:
                    break;
            }
        }
        #endregion Обработчики

        #region Команды
        #region OpenTableCommand
        /// <summary>
        /// Команда для открытия таблицы
        /// </summary>
        public ICommand OpenTableCommand
        {
            get { return _openTableCommand ?? (_openTableCommand = new RelayCommand(this.OpenTable, this.CanOpenTable)); }
        }
        /// <summary>
        /// Открытие таблицы
        /// </summary>
        /// <param name="parameter">Таблица, которую необходимо открыть</param>
        public void OpenTable(object parameter = null)
        {
            AbsM.ITableBaseM iTable = null;
            if (parameter is CommandEventParameter)
            {
                iTable = (parameter as CommandEventParameter).CommandParameter as AbsM.ITableBaseM;
            }
            else
            {
                iTable = parameter as AbsM.ITableBaseM;
            }

            if (iTable != null)
            {
                OpenTable(iTable);
            }
        }
        /// <summary>
        /// Можно ли открыть таблицу
        /// </summary>
        public bool CanOpenTable(object parameter = null)
        {
            return ((parameter is AbsM.ITableBaseM) && (Type != AbsM.ERepositoryType.Rastr));
        }
        #endregion // OpenTableCommand
        #endregion Команды
    }
}