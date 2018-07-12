using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Interfaces;
using RESTLib.Model.REST;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Model.WFS;
using sscSync.Controller;

namespace sscSync.ViewModel
{
    public class LayerRight
    {
        Layer layer;
        bool hasRight;

        public bool HasRight
        {
            get { return hasRight; }
        }

        public Layer Layer
        {
            get { return layer; }
        }

        public string DisplayName
        {
            get { return layer.Name + (hasRight ? "" : " [нет прав]");}
        }

        public LayerRight(Layer l, bool right)
        {
            layer = l;
            hasRight = right;
        }
    }

    public class TableRight
    {
        tablesInfo table;
        bool hasRight;
        bool wasChanged;

        public bool WasChanged
        {
            get { return wasChanged; }
            set { wasChanged = value; }
        }

        public bool HasRight { get { return hasRight; } }
        public tablesInfo Table { get { return table; } }

        public string DisplayName
        {
            get { return Table.nameMap + (WasChanged ? " [изменен]" : ""); }
        }

        public TableRight(tablesInfo ti, bool right)
        {
            this.table = ti;
            this.hasRight = right;
            this.wasChanged = false;
        }
    }

    public class MainViewModel: ViewModelBase
    {
        #region Поля
        private ObservableCollection<Object> _layers;
        private IWorkClass work;
        private string _filter;
        private bool _isMapAdminRegister;
        private MapAdmin _mapAdmin;
        private List<TableRight> _allPgTables;
        private List<Layer> _allSSCLayers;
        #endregion Поля

        #region Свойства

        /// <summary>
        /// Регистрируется ли слои SSC
        /// </summary>
        public bool IsMapAdminRegister
        {
            get { return _isMapAdminRegister; }
            set
            {
                _isMapAdminRegister = value;
                OnPropertyChanged("IsMapAdminRegister");
            }
        }

        public string ListTitle
        {
            get
            {
                return IsMapAdminRegister
                    ? "Таблицы, не зарегистрированные в MapAdmin:"
                    : "Таблицы, не зарегистрированные в программе:";
            }
        }
        
        /// <summary>
        /// Фильтр таблиц
        /// </summary>
        public string Filter
        {
            get { return _filter; }
            set { 
                _filter = value;
                OnPropertyChanged("Filter");

                ICollectionView defView = CollectionViewSource.GetDefaultView(_layers);
                if (defView != null)
                {
                    defView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
                    if (_filter != null)
                    {
                        defView.Filter = delegate(object o)
                            {
                                String text = o.GetType().GetProperty("DisplayName").GetValue(o, null).ToString();
                                return text.ToUpper().Contains(_filter.ToUpper());
                            };
                    }
                    else
                        defView.Filter = null;
                }
            }
        }

        /// <summary>
        /// Слои
        /// </summary>
        public ObservableCollection<Object> Layers
        {
            get { return _layers; }
        }

        #endregion Свойстваа

        #region Конструктор
        public MainViewModel(IMainApp app, IWorkClass work, RESTLib.Model.REST.User user, bool isMapAdminRegister)
        {
            IsMapAdminRegister = isMapAdminRegister;

            _mapAdmin = new MapAdmin(new Interfaces.sscUserInfo(user.Uri, user.Username, user.Password), app, work);
            this.work = work;

            ReloadTables(true, true);
        }
        #endregion Конструктор

        #region Методы

        /// <summary>
        /// Загрузка слоев
        /// </summary>
        private void ReloadTables(bool loadPg, bool loadSSC)
        {
            string key = work.OpenForm.ProcOpen();
            DateTime time = DateTime.Now;

            try
            {                
                if (loadSSC)
                    _allSSCLayers = _mapAdmin.SscData.GetSSCLayers(work.OpenForm);
                if (loadPg)
                    _allPgTables = _mapAdmin.PgData.GetPgLayers();
                System.Diagnostics.Debug.WriteLine("Скачивание :" + (DateTime.Now - time).TotalMinutes + " мин.");

            }
            catch (System.Net.WebException e)
            {
                work.OpenForm.ProcClose(key);
                MessageBox.Show(e.InnerException != null ? e.InnerException.Message : e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                work.OpenForm.ProcClose(key);
                MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                work.OpenForm.ProcClose(key);
            }
        }

        /// <summary>
        /// Распределить слои по листам
        /// </summary>
        private ObservableCollection<object> FormLists(bool isMapAdminRegister)
        {
            var layers = new ObservableCollection<object>();

            List<LayerRight> layersInTableInfo = new List<LayerRight>();
            
            // Находим пересечение множеств
            foreach (var table in _allPgTables)
            {
                var layer = _allSSCLayers.Find(w => w.lname == table.Table.nameDB || w.lname == table.Table.view_name);
                if (layer == null || table.WasChanged)
                {
                    if (isMapAdminRegister && table.HasRight)
                    {   // зарегистрировать можно только таблицы,
                        // на которые есть права                        
                        if (_mapAdmin.SscData.IsAdmin || !table.WasChanged || layer == null) // может перерегистрировать
                            _layers.Add(table);
                        else if (layer != null)
                            layersInTableInfo.Add(new LayerRight(layer, table.HasRight));
                    }
                }
                else
                {
                    // слои, которые есть в table_info
                    layersInTableInfo.Add(new LayerRight(layer, table.HasRight));
                }
            }

            if (!isMapAdminRegister)
            {
                // проверяем наличие слоя в базе
                foreach (var layer in _allSSCLayers)
                {
                    bool toAdd = true;
                    var layerRight = layersInTableInfo.Find(w => w.Layer == layer);

                    if (layer.lname == null
                        || layer.From_infrastructure != "t"
                        || layerRight != null && layerRight.HasRight == true)
                        // исключаем слои не из базы и слои, права на которые есть
                        toAdd = false;
                    else
                    {
                        var scheme = _mapAdmin.PgData.GetSchemeForTable(layer.lname);
                        if (String.IsNullOrEmpty(scheme))
                            toAdd = false; // слой отсутствует в базе
                    }
                    if (toAdd)
                        _layers.Add(new LayerRight(layer, layerRight != null ? false : true));
                    // слой либо будет опубликован, либо пользователю будут даны права на него
                }
            }

            return layers;
        //    OnPropertyChanged("Layers");
        //    Filter = _filter;
        }


        /// <summary>
        /// Регистрация слоя в инфраструктуре
        /// </summary>
        /// <param name="layer">Выбранный слой</param>
        public void RegisterLayer(LayerRight layer)
        {
            string key = work.OpenForm.ProcOpen();
            bool isSuccess = false;

            try
            {
                if (layer.HasRight)
                {
                    string scheme = _mapAdmin.PgData.GetSchemeForTable(layer.Layer.lname);

                    string[] geomInfo = _mapAdmin.PgData.GetGeometryType(layer.Layer.lname, scheme);

                    if (geomInfo == null || geomInfo.Length < 2)
                        throw new Exception("Нет информации о геометрии слоя");

                    var standart_ds = _mapAdmin.SscData.GetStandartDatastore();
                    if (standart_ds == null)
                        throw new Exception("Standart datastore not found");

                    isSuccess = _mapAdmin.PgData.RegisterLayer(layer.Layer, scheme, geomInfo[0], geomInfo[1], standart_ds.database);
                    if (isSuccess)
                        ReloadTables(true, false);
                    else
                        throw new Exception("Регистрация таблицы не удалась");
                }
                else
                    isSuccess = true;

                var table = _allPgTables.Find(w => w.Table.nameDB == layer.Layer.lname);
                if (table != null)
                {
                    isSuccess &= _mapAdmin.PgData.GrantRight(table.Table);
                    ReloadTables(true, false);
                }
            }

            catch (System.Net.WebException e)
            {
                work.OpenForm.ProcClose(key);
                MessageBox.Show(e.InnerException != null ? e.InnerException.Message : e.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception e)
            {
                work.OpenForm.ProcClose(key);
                MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            work.OpenForm.ProcClose(key);
            if (isSuccess)
            {
                MessageBox.Show("Слой успешно зарегистрирован.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                Layers.Remove(layer);
            }
            else
            {
                MessageBox.Show("Операция не удалась.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Регистрация
        /// </summary>
        /// <param name="parameter">Слой</param>
        private void Register(object parameter = null)
        {
            if (IsMapAdminRegister)
            {
                if (parameter != null && parameter is TableRight)
                {
                    //TODO: вернуть код, если понадобится
                    //var table = parameter as TableRight;
                    //if (table.WasChanged)
                    //    _mapAdmin.ReloadTable(table.Table);
                    //else
                    //    _mapAdmin.RegisterTable(table.Table);
                }
            }
            else
            {
                if (parameter != null && parameter is LayerRight)
                    RegisterLayer(parameter as LayerRight);
            }
        }

        #endregion Методы

        #region Команды

        ICommand _registerCommand = null;
        public ICommand RegisterCommand
        {
            get
            {
                return _registerCommand ?? (_registerCommand = new RelayCommand(
                    (o) => { Register(o); }, (o) => { return (o != null); }));
            }
        }

        ICommand _refreshCommand = null;
        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand((o) =>
                    {
                        ReloadTables(true, true);
                        FormLists(IsMapAdminRegister);
                    }));
            }            
        }

        #endregion Команды
    }
}
