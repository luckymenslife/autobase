using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using RastVM = Rekod.DataAccess.SourceRastr.ViewModel;
using VMPVM = Rekod.DataAccess.SourceVMP.ViewModel;
using CosVM = Rekod.DataAccess.SourceCosmetic.ViewModel;
using System.Windows;
using System.Windows.Controls;
using TmM = Rekod.DataAccess.TableManager.Model;
using TmV = Rekod.DataAccess.TableManager.View;
using mvMapLib;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using Rekod.Controllers;
using Rekod.Behaviors;
using Rekod.Services;

namespace Rekod.DataAccess.TableManager.ViewModel
{
    public class TableManagerVM : WindowViewModelBase_VM, TmM.ITableManagerVM
    {
        #region Классы
        public class SelectedObjectInfo
        {
            public int? SelectedObjectId
            {
                get;
                set;
            }
            public mvMapLib.mvLayer SelectedObjectLayer
            {
                get;
                set;
            }
        }
        #endregion Классы

        #region Поля
        private AxmvMapLib.AxMapLIb _axMapLib1;
        private WinMain _winMain;

        private ObservableCollection<AbsM.IDataRepositoryM> _dataRepositories;
        private ObservableCollection<AbsM.GroupM> _visibleLayersGroup = new ObservableCollection<AbsM.GroupM>();
        private List<AbsM.ERepositoryType> _addableSourceTypes = new List<AbsM.ERepositoryType>()
        {
            AbsM.ERepositoryType.Postgres
        }; 

        AbsM.IDataRepositoryM _rastrRepository;
        AbsM.IDataRepositoryM _VMPReposotory;
        AbsM.IDataRepositoryM _cosmeticRepository;
        AbsM.TableBaseM _selectedLayer;
        AbsM.ILayerM _editableLayer;
        private String _wktCopyObject = null;
        private mvMapLib.Cursors _currentCursor;
        private bool _showTableManagerView = true;
        private ICommand _reloadRepositories;
        private ICommand _openTableCommand;
        private ICommand _createObjectInMapCommand;
        private ICommand _deleteSelectedObjectCommand;
        private ICommand _toggleModeCommand;
        private ICommand _scaleZoomCommand;
        private ICommand _showLegendCommand;
        private ICommand _reloadAllLayerDataCommand;
        private ICommand _deselectObjectsInMapCommand;
        private ICommand _snapToKnotCommand;
        private ICommand _rotateObjectOnDegreeCommand;
        private ICommand _copyMapObjectCommand;
        private ICommand _pasteMapObjectCommand;
        private ICommand _createHoleByMouseCommand;
        private ICommand _createHoleTwoPolygonCommand;
        private ICommand _addSourceCommand;
        private ICommand _deleteSourceCommand;
        private bool _snappedToKnot = false;
        #endregion // Поля

        #region Свойства
        public AbsM.IDataRepositoryM RastrRepository
        { get { return _rastrRepository; } }
        public AbsM.IDataRepositoryM VMPReposotory
        { get { return _VMPReposotory; } }
        public AbsM.IDataRepositoryM CosmeticRepository
        { get { return _cosmeticRepository; } }
        /// <summary>
        /// Текущий редактируемый слой
        /// </summary>
        public AbsM.ILayerM EditableLayer
        {
            get { return _editableLayer; }
            set { _editableLayer = value; }
        }
        /// <summary>
        /// Текущий выбранный в менеждере слой
        /// </summary>
        public AbsM.ILayerM SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                _selectedLayer = (AbsM.TableBaseM)value;
                OnPropertyChanged(() => this.SelectedLayer);
            }
        }
        public AxmvMapLib.AxMapLIb mv
        {
            get { return _axMapLib1; }
        }
        /// <summary>
        /// Показывать или нет менеджер слоев в пользовательском интерфейсе
        /// </summary>
        public bool ShowTableManagerView
        {
            get { return _showTableManagerView; }
            set
            {
                if (value)
                {
                    //_winMain.TableManagerView.Visibility = Visibility.Visible;
                    //_winMain.MainGrid.ColumnDefinitions[1].Width = new GridLength(350);
                }
                else
                {
                    //_winMain.TableManagerView.Visibility = Visibility.Collapsed;
                    //_winMain.MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
                OnPropertyChanged(ref _showTableManagerView, value, () => this.ShowTableManagerView);
            }
        }
        /// <summary>
        /// Привязка к узлам на карте
        /// </summary>
        public bool SnappedToKnot
        {
            get { return _axMapLib1.Snap; }
            set
            {
                _axMapLib1.Snap = value;
                OnPropertyChanged(ref _snappedToKnot, value, () => this.SnappedToKnot);
            }
        }
        /// <summary>
        /// Устанавливает или получает текущий курсор на карте
        /// </summary>
        public mvMapLib.Cursors CurrentCursor
        {
            get { return _axMapLib1.CtlCursor; }
            set
            {
                if (_axMapLib1.CtlCursor == value)
                {
                    return;
                }
                _axMapLib1.CtlCursor = value;
                OnPropertyChanged(() => this.CurrentCursor);
                RelayCommand.UpdateStatus();
            }
        }
        /// <summary>
        /// Информация о выбранном объекта - идентификатор и слой в котором выбран объект
        /// </summary>
        public SelectedObjectInfo SelectedMapObjectInfo
        {
            get;
            private set;
        }
        #endregion // Свойства

        #region Коллекции
        /// <summary>
        /// Хранилища данных 
        /// </summary>
        public IEnumerable<AbsM.IDataRepositoryM> DataRepositories
        {
            get { return _dataRepositories; }
        }
        /// <summary>
        /// Группа видимых слоев
        /// </summary>
        public ObservableCollection<AbsM.GroupM> VisibleLayersGroup
        {
            get { return _visibleLayersGroup; }
        }
        /// <summary>
        /// Коллекция типов источников, которые допускают добавление
        /// </summary>
        public List<AbsM.ERepositoryType> AddableSourceTypes
        {
            get { return _addableSourceTypes; }   
        }
        #endregion // Конструктор

        #region Конструктор
        public TableManagerVM(AxmvMapLib.AxMapLIb mv)
        {
            _dataRepositories = new ObservableCollection<AbsM.IDataRepositoryM>();
            _axMapLib1 = mv;
            _visibleLayersGroup.Add(new AbsM.GroupM(null, 0) { Description = "Группа видимых слоев", Text = "Группа видимых слоев" });

            AddRepository(_rastrRepository = new RastVM.RastrDataRepositoryVM(this, Program.path_string + "\\" + Program.setting_file));
            AddRepository(_VMPReposotory = new VMPVM.VMPDataRepositoryVM(this));
            
            AddRepository(_cosmeticRepository = new CosVM.CosmeticDataRepositoryVM(this));
            (_cosmeticRepository as CosVM.CosmeticDataRepositoryVM).CreateLayer(Properties.Resources.Loc_CosmeticLayer);

            _winMain = Program.WinMain;
            CurrentCursor = mvMapLib.Cursors.mlPan;
            SelectedMapObjectInfo = new TableManagerVM.SelectedObjectInfo();
            _axMapLib1.MouseDownEvent += new AxmvMapLib.IMapLIbEvents_MouseDownEventHandler(_axMapLib1_MouseDownEvent);
            _axMapLib1.ObjectSelected += _axMapLib1_ObjectSelected;
            _axMapLib1.ObjectUnselected += _axMapLib1_ObjectUnselected;
            _axMapLib1.ObjectAfterCreate += new AxmvMapLib.IMapLIbEvents_ObjectAfterCreateEventHandler(_axMapLib1_ObjectAfterCreate);
            //_axMapLib1.OnDblClick += new EventHandler(_axMapLib1_OnDblClick);
            _axMapLib1.OnCursorChange += new AxmvMapLib.IMapLIbEvents_OnCursorChangeEventHandler(_axMapLib1_OnCursorChange);
        }

        static TableManagerVM()
        {
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseDoubleClickEvent,
                                                        new MouseButtonEventHandler(OnTreeViewItem_MouseDoubleClick));
        }
        #endregion // Конструктор

        #region Методы
        public void AddRepository(AbsM.IDataRepositoryM repository)
        {

            try
            {

                for (int i = 0; i < _dataRepositories.Count(); i++)
                {
                    if (_dataRepositories[i].Equals(repository))
                        throw new Exception("Такой источник уже есть");
                }
                if (!DataRepositories.Contains(repository))
                {
                    if (repository.CheckRepository())
                    {
                        _dataRepositories.Add(repository);
                        repository.TablePropertyChanged += repository_TablePropertyChanged;
                        repository.ReloadInfo();
                    }
                }
            }
            catch
            {
                repository.TablePropertyChanged -= repository_TablePropertyChanged;
                throw;
            }

        }
        public void RemoveRepository(AbsM.IDataRepositoryM repository)
        {
            if (DataRepositories.Contains(repository))
            {
                _dataRepositories.Remove(repository);
                repository.TablePropertyChanged -= repository_TablePropertyChanged;
                repository.Dispose();
            }
        }
        /// <summary>
        /// Открыть конфигурирование объекта
        /// </summary>
        /// <param name="type"></param>
        public void OpenConfigurator(AbsM.ERepositoryType type)
        {
            switch (type)
            {
                case AbsM.ERepositoryType.Rastr:
                    {
                        RastrRepository.OpenWindow(
                                new Rekod.DataAccess.SourceRastr.View.RastrRepositoryV(),
                                (WindowViewModelBase_VM)RastrRepository,
                                640, 360,
                                640, 360
                            );
                        break;
                    }
                case AbsM.ERepositoryType.VMP:
                    {
                        VMPReposotory.OpenWindow(
                                   new SourceVMP.View.VMPRepositoryV(),
                                   (WindowViewModelBase_VM)VMPReposotory,
                                   630, 300,
                                   630, 300
                               );
                        break;
                    }
                case AbsM.ERepositoryType.Postgres:
                    {
                        WindowViewModelBase_VM.OpenWindow(
                                    new SourcePostgres.View.PostgresRepositoryV(),
                                    (WindowViewModelBase_VM)new PgVM.PgRepositoriesConfig_VM(this),
                                    767, 570,
                                    600, 300,
                                    Program.WinMain
                                  );
                        break;
                    }
                case AbsM.ERepositoryType.Cosmetic:
                    {
                        VMPReposotory.OpenWindow(
                                   new SourceCosmetic.View.CosmeticRepositoryV(),
                                   (WindowViewModelBase_VM)CosmeticRepository,
                                   630, 500,
                                   630, 300
                               );
                        break;
                    }
            }
        }
        /// <summary>
        /// Открытие окна настройки таблицы (слоя)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        public void OpenTableSettings(AbsM.ITableBaseM iTable, System.Windows.UIElement positionElement = null)
        {
            var type = iTable.Source.Type;
            switch (type)
            {
                case AbsM.ERepositoryType.Rastr:
                    {
                        RastrRepository.OpenTableSettings(iTable);
                        break;
                    }
                case AbsM.ERepositoryType.VMP:
                    {
                        VMPReposotory.OpenTableSettings(iTable);
                        break;
                    }
                case AbsM.ERepositoryType.Postgres:
                    {
                        break;
                    }
                case AbsM.ERepositoryType.Cosmetic:
                    {
                        CosmeticRepository.OpenTableSettings(iTable);
                        break;
                    }
            }
        }
        /// <summary>
        /// Получение полигона с дыркой. Wkt полигона и дырки предполагаются в проекции карты
        /// </summary>
        /// <param name="wktPolygon">Wkt полигона</param>
        /// <param name="wktHole">Wkt дырки</param>
        /// <param name="newSrid">Новая проекция</param>
        /// <param name="connect">Строка соединения с базой Postgres</param>
        /// <returns>Итоговая wkt</returns>
        public String GetPolygonWithHole(String wktPolygon, String wktHole, int newSrid, Npgsql.NpgsqlConnectionStringBuilder connect)
        {
            String wktResult = null;

            //if (wktPolygon.IndexOf("MULTI") != -1)
            //{
            //    String sql =
            //        String.Format("SELECT st_astext(st_transform(ST_Multi(ST_Difference(st_geomfromtext('{1}', {2}), st_geomfromtext('{0}', {2}))), {3}));",
            //                        wktHole,
            //                        wktPolygon,
            //                        _axMapLib1.SRID,
            //                        newSrid);
            //    using (SqlWork sqlWork = new SqlWork(connect))
            //    {
            //        sqlWork.sql = sql;
            //        sqlWork.Execute(false);
            //        if (sqlWork.CanRead())
            //        {
            //            if (sqlWork.GetValue<string>(0) == "GEOMETRYCOLLECTION EMPTY")
            //            {
            //                sqlWork.Close();
            //                throw new Exception("Пустая геометрия");
            //            }
            //            else
            //            {
            //                wktResult = sqlWork.GetValue<string>(0);
            //            }
            //            sqlWork.Close();
            //        }
            //        else
            //        {
            //            sqlWork.Close();
            //            throw new Exception("Невозможно обработать запрос");
            //        }
            //    }
            //}
            //else
            //{
            //    String sql = String.Format("SELECT st_within(st_geomfromtext('{0}', {2}), st_geomfromtext('{1}', {2}));",
            //                                    wktHole,
            //                                    wktPolygon,
            //                                    _axMapLib1.SRID);
            //    using (SqlWork sqlWork = new SqlWork(connect))
            //    {
            //        sqlWork.sql = sql;
            //        if ((bool)sqlWork.ExecuteScalar())
            //        {
            //            sql = String.Format("SELECT st_astext( st_transform(ST_Difference(st_geomfromtext('{1}', {2}), st_geomfromtext('{0}', {2})), {3}) );",
            //                                    wktHole,
            //                                    wktPolygon,
            //                                    _axMapLib1.SRID,
            //                                    newSrid);
            //            using (SqlWork sqlWorkInner = new SqlWork(connect))
            //            {
            //                sqlWorkInner.sql = sql;
            //                sqlWorkInner.Execute(false);
            //                if (sqlWorkInner.CanRead())
            //                {
            //                    wktResult = sqlWorkInner.GetValue<string>(0);
            //                    sqlWorkInner.Close();
            //                }
            //                else
            //                {
            //                    sqlWorkInner.Close();
            //                    throw new Exception("Невозможно обработать запрос");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            sqlWork.Close();
            //            throw new Exception("Невозможная операция для не мультиплощадного объекта!");
            //        }
            //    }
            //}
            return wktResult;
        }
        #endregion // Методы

        #region Команды
        #region ReloadRepositoriesCommand
        /// <summary>
        /// Обновить информацию во всех источниках
        /// </summary>
        public ICommand ReloadRepositoriesCommand
        {
            get { return _reloadRepositories ?? (_reloadRepositories = new RelayCommand(this.ReloadRepositories)); }
        }
        /// <summary>
        /// Обновить информацию во всех источниках
        /// </summary>
        private void ReloadRepositories(object obj = null)
        {
            cti.ThreadProgress.ShowWait();
            foreach (var item in DataRepositories)
            {
                item.ReloadInfo();
            }
            cti.ThreadProgress.Close();
        }
        #endregion // ReloadRepositoriesCommand

        #region DeleteSelectedObjectCommand
        /// <summary>
        /// Удаление объекта в выделенном слое
        /// </summary>
        public ICommand DeleteSelectedObjectCommand
        {
            get { return _deleteSelectedObjectCommand ?? (_deleteSelectedObjectCommand = new RelayCommand(this.DeleteSelectedObject, this.CanDeleteSelectedObject)); }
        }
        /// <summary>
        /// Удаление объекта в выделенном слое
        /// </summary>
        public void DeleteSelectedObject(object param = null)
        {
            if (CanDeleteSelectedObject())
            {
                if (SelectedLayer.Source is PgVM.PgDataRepositoryVM && SelectedMapObjectInfo.SelectedObjectId != null)
                {
                    var mess =MessageBox.Show("Вы действительно хотите удалить выбранный объект?",
                                        "Удаление объекта", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mess == MessageBoxResult.Yes)
                    {
                        mvMapLib.mvLayer layer = _axMapLib1.getLayer(_selectedLayer.NameMap);
                        using (SqlWork sqlCmd = new SqlWork((SelectedLayer.Source as PgVM.PgDataRepositoryVM).Connect))
                        {
                            sqlCmd.sql = String.Format("DELETE FROM {0}.{1} WHERE {2}={3}",
                                                            (SelectedLayer as PgM.PgTableBaseM).SchemeName,
                                                            SelectedLayer.Name,
                                                            (SelectedLayer as PgM.PgTableBaseM).PrimaryKey,
                                                            SelectedMapObjectInfo.SelectedObjectId);
                            sqlCmd.Execute(true);
                            sqlCmd.Close();
                        }

                        layer.ExternalFullReloadVisible();
                        Classes.workLogFile.writeLogFile(String.Format("deleted --> table={0} object={1}",
                                                                            SelectedLayer.Name,
                                                                            SelectedMapObjectInfo.SelectedObjectId.ToString()), false, false);
                        Program.mainFrm1.StatusInfo = "Выбранный объект удален!";
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли удалить объект
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteSelectedObject(object param = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectId != null
                        && SelectedLayer != null
                        && SelectedLayer.IsEditable
                        && SelectedMapObjectInfo.SelectedObjectLayer.NAME == _selectedLayer.NameMap)
            {
                mvMapLib.mvLayer layer = _axMapLib1.getLayer(_selectedLayer.NameMap);
                if (layer != null)
                {
                    return (layer.SelectedCount > 0);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion // DeleteSelectedObjectCommand

        #region ToggleModeCommand
        /// <summary>
        /// Команда для переключения курсора
        /// </summary>
        public ICommand ToggleModeCommand
        {
            get { return _toggleModeCommand ?? (_toggleModeCommand = new RelayCommand(this.ToggleMode, this.CanToggleMode)); }
        }
        /// <summary>
        /// Переключения курсора
        /// </summary>
        public void ToggleMode(object parameter = null)
        {
            mvMapLib.Cursors cursor = (mvMapLib.Cursors)(Enum.Parse(typeof(mvMapLib.Cursors), (String)(parameter)));
            CurrentCursor = cursor;
            if (cursor == mvMapLib.Cursors.mlDistance)
            {
                Program.mainFrm1.StatusInfo = "Для замера площади удерживайте клавишу 'Shift'";
            }
        }
        /// <summary>
        /// Можно ли переключить курсор
        /// </summary>
        public bool CanToggleMode(object parameter = null)
        {
            mvMapLib.Cursors cursor = (mvMapLib.Cursors)(Enum.Parse(typeof(mvMapLib.Cursors), (String)(parameter)));
            bool cursorIsDifferent = _axMapLib1.CtlCursor != cursor;
            bool additionalCondition = true;

            switch (cursor)
            {
                case mvMapLib.Cursors.mlRotateObj:
                case mvMapLib.Cursors.mlMoveObj:
                    {
                        additionalCondition = SelectedLayer != null
                            && SelectedLayer.IsEditable;
                        break;
                    }
                case mvMapLib.Cursors.mlRectangle:
                    {
                        additionalCondition = SelectedLayer != null
                            && SelectedLayer.IsEditable
                            && SelectedLayer is PgM.PgTableBaseM
                            && (SelectedLayer as PgM.PgTableBaseM).GeomType == AbsM.EGeomType.Polygon;
                        break;
                    }
                case mvMapLib.Cursors.mlAddVx:
                case mvMapLib.Cursors.mlDelVx:
                case mvMapLib.Cursors.mlMoveVx:
                    {
                        if (SelectedLayer != null && SelectedLayer.IsEditable)
                        {
                            mvMapLib.mvLayer layer = _axMapLib1.getLayer(_selectedLayer.NameMap);
                            if (layer != null)
                            {
                                additionalCondition = (layer.SelectedCount > 0);
                            }
                            else
                            {
                                additionalCondition = false;
                            }
                        }
                        else
                        {
                            additionalCondition = false;
                        }
                        break;
                    }
            }
            return cursorIsDifferent && additionalCondition;
        }
        #endregion // EnableMoveModeCommand

        #region ScaleZoomCommand
        /// <summary>
        /// Команда для изменения масштаба карты
        /// </summary>
        public ICommand ScaleZoomCommand
        {
            get { return _scaleZoomCommand ?? (_scaleZoomCommand = new RelayCommand(this.ScaleZoom, this.CanScaleZoom)); }
        }
        /// <summary>
        /// Изменение масштаба карты
        /// </summary>
        public void ScaleZoom(object parameter = null)
        {
            _axMapLib1.ScaleZoom = _axMapLib1.ScaleZoom * ((parameter.ToString() == "plus") ? 0.5 : 2);
        }
        /// <summary>
        /// Можно ли изменить масштаб карты
        /// </summary>
        public bool CanScaleZoom(object parameter = null)
        {
            return true;
        }
        #endregion // ScaleZoomCommand

        #region ShowLegendCommand
        /// <summary>
        /// Команда для отображения легенды
        /// </summary>
        public ICommand ShowLegendCommand
        {
            get { return _showLegendCommand ?? (_showLegendCommand = new RelayCommand(this.ShowLegend)); }
        }
        /// <summary>
        /// Отображение легенды
        /// </summary>
        public void ShowLegend(object parameter = null)
        {
            _axMapLib1.showLegend();
        }
        #endregion // ShowLegendCommand

        #region ReloadAllLayerDataCommand
        /// <summary>
        /// Команда для перезагрузки всех слоев
        /// </summary>
        public ICommand ReloadAllLayerDataCommand
        {
            get { return _reloadAllLayerDataCommand ?? (_reloadAllLayerDataCommand = new RelayCommand(this.ReloadAllLayerData)); }
        }
        /// <summary>
        /// Перезагрузка всех слоев
        /// </summary>
        public void ReloadAllLayerData(object parameter = null)
        {
            cti.ThreadProgress.ShowWait();
            for (int i = 0; _axMapLib1.LayersCount > i; i++)
            {
                mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                if (tempLayer != null)
                {
                    if (tempLayer.External)
                    {
                        tempLayer.ExternalFullReloadVisible();
                    }
                }
            }
            _axMapLib1.mapRepaint();
            cti.ThreadProgress.Close();
        }
        #endregion // ReloadAllLayerDataCommand

        #region DeselectObjectsInMapCommand
        /// <summary>
        /// Команда для снятия выделения со всех объектов на карте
        /// </summary>
        public ICommand DeselectObjectsInMapCommand
        {
            get { return _deselectObjectsInMapCommand ?? (_deselectObjectsInMapCommand = new RelayCommand(this.DeselectObjectsInMap)); }
        }
        /// <summary>
        /// Снятие выделения со всех объектов на карте
        /// </summary>
        public void DeselectObjectsInMap(object parameter = null)
        {
            for (int i = 0; i < _axMapLib1.LayersCount; i++)
            {
                mvMapLib.mvLayer layer = _axMapLib1.getLayerByNum(i);
                //if (layer.SelectedCount > 0)
                {
                    layer.DeselectAll();
                }
            }
            _axMapLib1.mapRepaint();
        }
        #endregion // DeselectObjectsInMapCommand

        #region RotateObjectOnDegreeCommand
        /// <summary>
        /// Команда для поворота объекта на заданный угол
        /// </summary>
        public ICommand RotateObjectOnDegreeCommand
        {
            get { return _rotateObjectOnDegreeCommand ?? (_rotateObjectOnDegreeCommand = new RelayCommand(this.RotateObjectOnDegree, this.CanRotateObjectOnDegree)); }
        }
        /// <summary>
        /// Поворот объекта на заданный угол
        /// </summary>
        public void RotateObjectOnDegree(object parameter = null)
        {
            if (OpenWindowEvent != null)
            {
                OpenWindowEvent(this, null);
            }
        }
        /// <summary>
        /// Можно ли повернуть объект на заданный угол
        /// </summary>
        public bool CanRotateObjectOnDegree(object parameter = null)
        {
            if (SelectedLayer != null && SelectedLayer.IsEditable)
            {
                mvMapLib.mvLayer layer = _axMapLib1.getLayer(_selectedLayer.NameMap);
                if (layer != null)
                {
                    return (layer.SelectedCount > 0);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion // RotateObjectOnDegreeCommand

        #region CopyMapObjectCommand
        /// <summary>
        /// Команда для копирования геометрии объекта на карте
        /// </summary>
        public ICommand CopyMapObjectCommand
        {
            get { return _copyMapObjectCommand ?? (_copyMapObjectCommand = new RelayCommand(this.CopyMapObject, this.CanCopyMapObject)); }
        }
        /// <summary>
        /// Копирование геометрии объекта на карте
        /// </summary>
        public void CopyMapObject(object parameter = null)
        {
            using (SqlWork sqlCmd = new SqlWork((SelectedLayer.Source as PgVM.PgDataRepositoryVM).Connect))
            {
                PgM.PgTableBaseM table = SelectedLayer as PgM.PgTableBaseM;
                sqlCmd.sql =
                    String.Format("SELECT st_astext(st_transform({0}, 4326)) FROM {1}.{2} WHERE {3} = {4}",
                                   table.GeomField,
                                   table.SchemeName,
                                   table.Name,
                                   table.PrimaryKey,
                                   SelectedMapObjectInfo.SelectedObjectId);
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    _wktCopyObject = sqlCmd.GetValue<string>(0);
                }
                sqlCmd.Close();
            }
            Program.mainFrm1.StatusInfo = String.Format("Скопирован объект id = {0}", SelectedMapObjectInfo.SelectedObjectId);
        }
        /// <summary>
        /// Можно ли скопировать геометрию объекта на карте
        /// </summary>
        public bool CanCopyMapObject(object parameter = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectId != null
                 && SelectedLayer != null
                 && SelectedLayer.Source is PgVM.PgDataRepositoryVM
                 && SelectedMapObjectInfo.SelectedObjectLayer.NAME == _selectedLayer.NameMap
                 && SelectedMapObjectInfo.SelectedObjectLayer.SelectedCount == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // CopyMapObjectCommand

        #region PasteMapObjectCommand
        /// <summary>
        /// Команда для вставки скопированной геометрии в выбранный слой
        /// </summary>
        public ICommand PasteMapObjectCommand
        {
            get { return _pasteMapObjectCommand ?? (_pasteMapObjectCommand = new RelayCommand(this.PasteMapObject, this.CanPasteMapObject)); }
        }
        /// <summary>
        /// Вставка скопированной геометрии в выбранный слой
        /// </summary>
        public void PasteMapObject(object parameter = null)
        {
            String copyPolygonType = null;
            String pasteLayerType = null;
            PgVM.PgDataRepositoryVM pgRepo = SelectedLayer.Source as PgVM.PgDataRepositoryVM;
            using (SqlWork sqlCmd = new SqlWork(pgRepo.Connect))
            {
                sqlCmd.sql = String.Format("SELECT st_geometrytype(st_geomfromtext('{0}', 4326))", _wktCopyObject);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    copyPolygonType = sqlCmd.GetValue(0).ToString().ToUpper().Replace("ST_", "");
                }
                sqlCmd.Close();
            }
            using (SqlWork sqlCmd = new SqlWork(pgRepo.Connect))
            {
                sqlCmd.sql =
                    String.Format("SELECT type FROM geometry_columns WHERE f_table_schema='{0}' AND f_table_name='{1}' AND f_geometry_column='{2}'",
                    (SelectedLayer as PgM.PgTableBaseM).SchemeName,
                    (SelectedLayer as PgM.PgTableBaseM).Name,
                    (SelectedLayer as PgM.PgTableBaseM).GeomField);
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    pasteLayerType = sqlCmd.GetValue(0).ToString();
                }
                sqlCmd.Close();
            }
            if (!String.IsNullOrEmpty(copyPolygonType)
                    && !String.IsNullOrEmpty(pasteLayerType)
                    && pasteLayerType.Contains(copyPolygonType))
            {
                string sql_str = String.Format(@"SELECT st_astext(st_transform(st_geomfromtext('{0}', {1}), {2}));",
                                                 _wktCopyObject,
                                                 "4326",
                                                 (SelectedLayer as PgM.PgTableBaseM).Srid);
                string wkt_temp = "";
                using (SqlWork sqlCmd = new SqlWork(pgRepo.Connect))
                {
                    sqlCmd.sql = sql_str;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        wkt_temp = sqlCmd.GetValue<string>(0);
                    }
                    sqlCmd.Close();
                }
                pgRepo.OpenObject(SelectedLayer, null, wkt_temp);
            }
            else
            {
                MessageBox.Show("Невозможно вставить данный тип геометрии в выбранный слой!");
            }
        }
        /// <summary>
        /// Можно ли вставить скопированную геометрию в выбранный слой
        /// </summary>
        public bool CanPasteMapObject(object parameter = null)
        {
            if (!String.IsNullOrEmpty(_wktCopyObject)
                    && SelectedLayer != null
                    && SelectedLayer.IsEditable
                    && SelectedLayer.Source is PgVM.PgDataRepositoryVM)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // PasteMapObjectCommand

        #region CreateHoleByMouseCommand
        /// <summary>
        /// Команда для создания полости полигона указанием точек
        /// </summary>
        public ICommand CreateHoleByMouseCommand
        {
            get { return _createHoleByMouseCommand ?? (_createHoleByMouseCommand = new RelayCommand(this.CreateHoleByMouse, this.CanCreateHoleByMouse)); }
        }
        /// <summary>
        /// Используется в команде
        /// </summary>
        private SelectedObjectInfo _tempMapHoleByMouseObjectInfo = null;
        /// <summary>
        /// Создание полости полигона указанием точек
        /// </summary>
        public void CreateHoleByMouse(object parameter = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectLayer != null)
            {
                _tempMapHoleByMouseObjectInfo = new SelectedObjectInfo()
                {
                    SelectedObjectId = SelectedMapObjectInfo.SelectedObjectId,
                    SelectedObjectLayer = SelectedMapObjectInfo.SelectedObjectLayer
                };
                _tempMapHoleByMouseObjectInfo.SelectedObjectLayer.DeselectAll();
                _tempMapHoleByMouseObjectInfo.SelectedObjectLayer.editable = false;
                {
                    mvLayer tempLayer = _axMapLib1.getLayer("CreateHoleCommandTempLayer");
                    if (tempLayer == null)
                    {
                        var ff = new mvStringArray();
                        ff.count = 1;
                        ff.setElem(0, "id");
                        tempLayer = _axMapLib1.CreateLayer("CreateHoleCommandTempLayer", ff);
                        var p1 = new mvPenObject
                        {
                            Color = 0x333333,
                            ctype = 2,
                            width = 2
                        };
                        var b1 = new mvBrushObject
                        {
                            bgcolor = 0xffff00,
                            fgcolor = 0x00ffff,
                            style = 0,
                            hatch = 2
                        };
                        var f1 = new mvFontObject
                        {
                            Color = 0xff0000,
                            fontname = "Map Symbols",
                            framecolor = 0xff0000,
                            size = 12
                        };
                        var s1 = new mvSymbolObject();
                        s1.shape = 35;
                        tempLayer.uniform = true;
                        tempLayer.SetUniformStyle(p1, b1, s1, f1);
                        tempLayer.editable = true;
                    }
                    else
                    {
                        tempLayer.RemoveObjects();
                        tempLayer.editable = true;
                    }
                }
                mvVectorObject obj = _tempMapHoleByMouseObjectInfo.SelectedObjectLayer.getObject((int)_tempMapHoleByMouseObjectInfo.SelectedObjectId);
                CurrentCursor = mvMapLib.Cursors.mlAddPolygon;
            }
        }
        /// <summary>
        /// Можно ли создать полость полигона
        /// </summary>
        public bool CanCreateHoleByMouse(object parameter = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectId != null
                && SelectedLayer != null
                && SelectedLayer.Source is PgVM.PgDataRepositoryVM
                && (SelectedLayer as PgM.PgTableBaseM).GeomType == AbsM.EGeomType.Polygon
                && SelectedMapObjectInfo.SelectedObjectLayer.NAME == _selectedLayer.NameMap
                && SelectedMapObjectInfo.SelectedObjectLayer.SelectedCount == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // CreateHoleByMouseCommand

        #region CreateHoleTwoPolygonCommand
        /// <summary>
        /// Команда для создания полости в полигоне указанием другого полигона
        /// </summary>
        public ICommand CreateHoleTwoPolygonCommand
        {
            get { return _createHoleTwoPolygonCommand ?? (_createHoleTwoPolygonCommand = new RelayCommand(this.CreateHoleTwoPolygon, this.CanCreateHoleTwoPolygon)); }
        }
        /// <summary>
        /// Используется в команде
        /// </summary>
        private int _selectedObjectId;
        private PgM.PgTableBaseM _selectedPgTable = null;
        private String _wktPolygon = null;
        /// <summary>
        /// Создание полости в полигоне указанием другого полигона
        /// </summary>
        public void CreateHoleTwoPolygon(object parameter = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectLayer != null)
            {
                _selectedObjectId = (int)SelectedMapObjectInfo.SelectedObjectId;
                _selectedPgTable = SelectedLayer as PgM.PgTableBaseM;
                _wktPolygon = SelectedMapObjectInfo.SelectedObjectLayer.getObject((int)SelectedMapObjectInfo.SelectedObjectId).getWKT();

                SelectedMapObjectInfo.SelectedObjectLayer.DeselectAll();
                Rekod.SelectedSetFrm frm = new SelectedSetFrm();

                //frm.Location = new System.Drawing.Point((int)(Program.WinMain.Left + _axMapLib1.Size.Width / 2 - frm.Size.Width / 2),
                //(int)(Program.WinMain.Top + Program.WinMain.Height - frm.Size.Height));

                double x = Program.WinMain.Left + Program.WinMain.ActualWidth / 2 - 130;
                double y = Program.WinMain.Top + Program.WinMain.ActualHeight - 170;
                frm.Location = new System.Drawing.Point((int)x, (int)y);

                frm.TopMost = true;
                frm.selectSetBtn.Click += new EventHandler(SelectedSetButtonClick);
                frm.cancelBtn.Click += new EventHandler(CancelButtonClick);
                frm.Show();
            }
        }
        public void SelectedSetButtonClick(object sender, EventArgs e)
        {
            if (_selectedPgTable != null && SelectedLayer != null)
            {
                int id = (int)SelectedMapObjectInfo.SelectedObjectId;
                mvLayer layer = SelectedMapObjectInfo.SelectedObjectLayer;
                if (layer.getObject(id).getWKT().ToUpper().Contains("POLYGON"))
                {
                    PgVM.PgDataRepositoryVM pgRepo = _selectedPgTable.Source as PgVM.PgDataRepositoryVM;
                    String wktHole = layer.getObject(id).getWKT();
                    Npgsql.NpgsqlConnectionStringBuilder connect = pgRepo.Connect;
                    int srid = (int)_selectedPgTable.Srid;
                    try
                    {
                        String wktResult = GetPolygonWithHole(_wktPolygon, wktHole, srid, connect);
                        using (SqlWork sqlWork = new SqlWork(connect))
                        {
                            sqlWork.sql =
                                String.Format("SELECT type FROM geometry_columns WHERE f_table_schema='{0}' AND f_table_name='{1}';",
                                                    _selectedPgTable.SchemeName,
                                                    _selectedPgTable.Name);
                            String polygonType = sqlWork.ExecuteScalar().ToString();
                            sqlWork.Close();
                            if (polygonType == "MULTIPOLYGON")
                            {
                                using (SqlWork subSqlWork = new SqlWork(connect))
                                {
                                    subSqlWork.sql = String.Format("SELECT st_astext(st_multi('{0}'))", wktResult);
                                    wktResult = subSqlWork.ExecuteScalar().ToString();
                                    subSqlWork.Close();
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(wktResult))
                        {
                            pgRepo.OpenObject(_selectedPgTable, _selectedObjectId, wktResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Выбранным объектом невозможно создать полость");
                }
                _selectedPgTable = null;
                ((sender as System.Windows.Forms.Button).Parent as System.Windows.Forms.Form).Close();
            }
        }
        public void CancelButtonClick(object sender, EventArgs e)
        {
            ((sender as System.Windows.Forms.Button).Parent as System.Windows.Forms.Form).Close();
        }
        /// <summary>
        /// Можно ли создать полость в полигоне
        /// </summary>
        public bool CanCreateHoleTwoPolygon(object parameter = null)
        {
            if (SelectedMapObjectInfo.SelectedObjectId != null
                 && SelectedLayer != null
                 && SelectedLayer.Source is PgVM.PgDataRepositoryVM
                 && (SelectedLayer as PgM.PgTableBaseM).GeomType == AbsM.EGeomType.Polygon
                 && SelectedMapObjectInfo.SelectedObjectLayer.NAME == _selectedLayer.NameMap
                 && SelectedMapObjectInfo.SelectedObjectLayer.SelectedCount == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // CreateHoleTwoPolygonCommand

        #region AddSourceCommand
        /// <summary>
        /// Команда для добавления источника
        /// </summary>
        public ICommand AddSourceCommand
        {
            get { return _addSourceCommand ?? (_addSourceCommand = new RelayCommand(this.AddSource, this.CanAddSource)); }
        }
        /// <summary>
        /// Добавление источника
        /// </summary>
        public void AddSource(object parameter = null)
        {
            Grid grid = parameter as Grid;
            if (grid.Name == "PostgreSQL")
            {
                try
                {
                    String logParam = (grid.Children[1] as TextBox).Text;
                    String login = (grid.Children[3] as TextBox).Text;
                    String password = (grid.Children[5] as PasswordBox).Password;

                    String database = logParam.Split(new[] { '@' })[0];
                    String host = logParam.Split(new[] { '@' })[1].Split(new[] { ':' })[0];
                    String port = null;
                    if (logParam.Split(new[] { '@' })[1].Contains(':'))
                    {
                        port = logParam.Split(new[] { '@' })[1].Split(new[] { ':' })[1];
                    }
                    if (String.IsNullOrEmpty(port))
                    {
                        port = "5432";
                    }

                    Npgsql.NpgsqlConnectionStringBuilder connStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder()
                    {
                        Host = host,
                        Database = database,
                        Port = Convert.ToInt32(port),
                        UserName = login,
                        Password = password,
                        Pooling = true,
                        MinPoolSize = 0,
                        MaxPoolSize = 100,
                        CommandTimeout = 300,
                        ConnectionLifeTime = 300,
                        SSL = true,
                        SslMode = Npgsql.SslMode.Prefer
                    };

                    cti.ThreadProgress.ShowWait();
                    var rep = new PgVM.PgDataRepositoryVM(this, connStringBuilder);
                    AddRepository(rep);
                    cti.ThreadProgress.Close();
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close();
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        /// <summary>
        /// Можно ли добавить источник
        /// </summary>
        public bool CanAddSource(object parameter = null)
        {
            return true;
        }
        #endregion // AddSourceCommand        

        #region DeleteSourceCommand
        /// <summary>
        /// Команда для удаления источника
        /// </summary>
        public ICommand DeleteSourceCommand
        {
            get { return _deleteSourceCommand ?? (_deleteSourceCommand = new RelayCommand(this.DeleteSource, this.CanDeleteSource)); }
        }
        /// <summary>
        /// Удаление источника
        /// </summary>
        public void DeleteSource(object parameter = null)
        {
            AbsVM.DataRepositoryVM repo = parameter as AbsVM.DataRepositoryVM;
            if (CanDeleteSource(repo))
            {
                _dataRepositories.Remove(repo);
            }
        }
        /// <summary>
        /// Можно ли удалить источник
        /// </summary>
        public bool CanDeleteSource(object parameter = null)
        {
            AbsVM.DataRepositoryVM repo = parameter as AbsVM.DataRepositoryVM;
            if (repo != null && AddableSourceTypes.Contains(repo.Type))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion // DeleteSourceCommand
        #endregion // Команды

        #region Обработчики
        public static void OnTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TreeViewItem item = sender as TreeViewItem;
            //var layer = item.Header as AbsM.ITableBaseM;
            //if (layer != null)
            //{
            //    layer.Source.OpenTable(layer);
            //}
        }
        void _axMapLib1_MouseDownEvent(object sender, AxmvMapLib.IMapLIbEvents_MouseDownEvent e)
        {
            RelayCommand.UpdateStatus();
        }
        void ObjSelected(mvLayer layer, int id)
        {
            SelectedMapObjectInfo.SelectedObjectId = id;
            SelectedMapObjectInfo.SelectedObjectLayer = layer;
        }
        void _axMapLib1_ObjectSelected(object sender, AxmvMapLib.IMapLIbEvents_ObjectSelectedEvent e)
        {
            if (e.ids.count == 0)
                return;
            var id = e.ids.getElem(e.ids.count - 1);
            ObjSelected(e.layer, id);
        }
        void _axMapLib1_ObjectUnselected(object sender, AxmvMapLib.IMapLIbEvents_ObjectUnselectedEvent e)
        {
            SelectedMapObjectInfo.SelectedObjectId = null;
            SelectedMapObjectInfo.SelectedObjectLayer = null;
        }
        void _axMapLib1_ObjectAfterCreate(object sender, AxmvMapLib.IMapLIbEvents_ObjectAfterCreateEvent e)
        {
            if (e.layer.NAME == "CreateHoleCommandTempLayer"
                    && _tempMapHoleByMouseObjectInfo != null
                    && _selectedLayer != null
                    && _tempMapHoleByMouseObjectInfo.SelectedObjectLayer.NAME == _selectedLayer.NameMap)
            {
                mvVectorObject obj = _tempMapHoleByMouseObjectInfo.SelectedObjectLayer.getObject((int)_tempMapHoleByMouseObjectInfo.SelectedObjectId);
                PgM.PgTableBaseM table = _selectedLayer as PgM.PgTableBaseM;
                PgVM.PgDataRepositoryVM pgRepo = table.Source as PgVM.PgDataRepositoryVM;

                String wktPolygon = null;
                String wktHollow = e.obj.getWKT();
                using (SqlWork sqlWork = new SqlWork(pgRepo.Connect))
                {
                    sqlWork.sql = String.Format("SELECT st_astext(st_transform({0}, {5})) FROM {1}.{2} WHERE {3}={4};",
                                                    table.GeomField,
                                                    table.SchemeName,
                                                    table.Name,
                                                    table.PrimaryKey,
                                                    (int)_tempMapHoleByMouseObjectInfo.SelectedObjectId,
                                                    _axMapLib1.SRID);
                    sqlWork.Execute(false);
                    if (sqlWork.CanRead())
                    {
                        wktPolygon = sqlWork.GetValue<string>(0);
                    }
                    sqlWork.Close();
                }

                try
                {
                    String wktResult = GetPolygonWithHole(wktPolygon, wktHollow, (int)table.Srid, pgRepo.Connect);
                    pgRepo.OpenObject(table, (int)_tempMapHoleByMouseObjectInfo.SelectedObjectId, wktResult);
                    _tempMapHoleByMouseObjectInfo = null;
                    CurrentCursor = mvMapLib.Cursors.mlPan;
                    e.layer.deleteLayer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    e.layer.RemoveObjects();
                }
            }
            else
            {
                try
                {
                    _tempMapHoleByMouseObjectInfo = null;

                    String layerName = e.layer.NAME;
                    int repositoryId = Convert.ToInt32(layerName.Split(new[] { ':' })[0].Replace("#", ""));
                    int tableId = Convert.ToInt32(Convert.ToInt32(layerName.Split(new[] { ':' })[1]));
                    int objectId = Convert.ToInt32(e.obj.fieldValue("id"));

                    AbsM.IDataRepositoryM repository = DataRepositories.FirstOrDefault(r => r.Id == repositoryId);
                    AbsM.ITableBaseM table = repository.Tables.FirstOrDefault(t => (int)t.Id == tableId);
                    repository.OpenObject(table, objectId);
                }
                catch (Exception ex)
                {
                }
            }
        }
        void _axMapLib1_OnDblClick(object sender, EventArgs e)
        {
            //if (CurrentCursor == mvMapLib.Cursors.mlSelect)
            //{                
            //    String layerName = SelectedMapObjectInfo.SelectedObjectLayer.NAME;
            //    var split =layerName.Split(new[] { ':' });
            //    int repositoryId = Convert.ToInt32(split[0].Replace("#", ""));
            //    int tableId = Convert.ToInt32(Convert.ToInt32(split[1]));
            //    int objectId = (int)SelectedMapObjectInfo.SelectedObjectId;
            //
            //    AbsM.IDataRepositoryM repository = DataRepositories.FirstOrDefault(r => r.Id == repositoryId);
            //    AbsM.ITableBaseM table = repository.Tables.FirstOrDefault(t => (int)t.Id == tableId);
            //    repository.OpenObject(table, objectId);
            //}
        }
        void _axMapLib1_OnCursorChange(object sender, AxmvMapLib.IMapLIbEvents_OnCursorChangeEvent e)
        {
            if (CurrentCursor != e.newCursor)
            {
                CurrentCursor = e.newCursor;
            }            
        }
        void repository_TablePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var layer = sender as AbsM.ILayerM;
            if (!layer.IsLayer)
                return;
            switch (e.PropertyName)
            {
                case "IsVisible":
                    if (layer.IsVisible)
                    {
                        if (!VisibleLayersGroup[0].Tables.Contains(layer))
                        {
                            VisibleLayersGroup[0].Tables.Add(layer);
                        }
                    }
                    else
                    {
                        if (VisibleLayersGroup[0].Tables.Contains(layer))
                        {
                            VisibleLayersGroup[0].Tables.Remove(layer);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion Обработчики

        #region События
        public delegate void OpenWindowEventHandler(object sender, EventArgs e);
        /// <summary>
        /// Служит для извещения внешнего кода о том, что нужно открыть окно. 
        /// </summary>
        /// <remarks>
        /// Модель представления не должна напрямую отображать визуальные объекты. 
        /// </remarks>
        public event OpenWindowEventHandler OpenWindowEvent;

        public event NotifyCollectionChangedEventHandler DataRepositoriesCollectionChanged
        {
            add { _dataRepositories.CollectionChanged += value; }
            remove { _dataRepositories.CollectionChanged -= value; }
        }
        #endregion События
    }
}