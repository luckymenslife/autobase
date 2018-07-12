using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mvMapLib;
using System.Windows;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Rekod.ViewModel;
using Rekod.Controllers;
using Rekod.DataAccess.TableManager.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using Interfaces;
using Rekod.Services;

namespace Rekod.Classes
{
    public class ButtonManager : ViewModelBase
    {
        #region Переменные
        private mvMapLib.Cursors _oldCursor;
        private mvMapLib.Cursors _currCursor;
        private TypeGeometry _typeGeom = TypeGeometry.MISSING;
        private int _selectCount = 0;
        private bool _writeTable = false;
        private int _currentIdTable = -1;

        private bool _existsPaste;
        private TypeGeometry _copyTypeGeom;


        private Interfaces.tablesInfo _currentTablePg;
        private AbsM.ILayerM _currentLayerM;
        private object _currentTable;


        private ObservableCollection<ToolBar_VM> _listToolBars;

        private bool _canOpenTable;


        private RelayCommand _openTableCommand;
        private RelayCommand _addGeometryPointCommand;
        private bool _canAddGeometryPoint;
        private RelayCommand _addGeometryLineCommand;
        private bool _canAddGeometryLine;
        private RelayCommand _addGeometryGeometryCommand;
        private bool _canAddGeometryGeometry;
        private RelayCommand _addGeometryRectangleCommand;
        private bool _canAddGeometryRectangle;
        private bool _addGeometryPointIsChecked;
        private bool _addGeometryLineIsChecked;
        private bool _addGeometryRectangleIsChecked;
        private bool _addGeometryGeometryIsChecked;
        private RelayCommand _deleteGeometryCommand;
        private bool _canDeleteGeometry;
        private RelayCommand _nodeAddCommand;
        private bool _canNodeAdd;
        private RelayCommand _nodeMoveCommand;
        private bool _canNodeMove;
        private RelayCommand _nodeDelCommand;
        private bool _canNodeDel;
        private bool _nodeAddIsChecked;
        private bool _nodeMoveIsChecked;
        private bool _nodeDelIsChecked;
        #endregion Переменные

        #region События
        private event PropertyChangedEventHandler _listToolBars_PropertyChanged;
        private RelayCommand _copyInSQLiteCommand;
        private bool _canCopyInSQLite;
        private RelayCommand _pastFromSQLiteCommand;
        private bool _canPastFromSQLite;
        public event PropertyChangedEventHandler ListToolBars_PropertyChanged
        {
            add { _listToolBars_PropertyChanged += value; }
            remove { _listToolBars_PropertyChanged -= value; }
        }
        #endregion

        #region Конструктор
        /// <summary>
        /// 
        /// </summary>
        public ButtonManager()
        {
            _listToolBars = new ObservableCollection<ToolBar_VM>();
            mv.OnCursorChange += new AxmvMapLib.IMapLIbEvents_OnCursorChangeEventHandler(axMapLIb1_OnCursorChange);
            mv.ObjectSelected += mv_ObjectSelected;
            mv.ObjectUnselected += mv_ObjectUnselected;
            SqlCopyPaste.PropertyChanged += SqlCopyPaste_PropertyChanged;
            mf.layerItemsView1.PropertyChanged += layerItemsView1_PropertyChanged;
            //mv.LayerChanged += new AxmvMapLib.IMapLIbEvents_LayerChangedEventHandler(mv_LayerChanged);
            this.PropertyChanged += ButtonManager_PropertyChanged;
        }

        #endregion // Конструктор

        #region Свойства
        public SQLiteCopyPaste SqlCopyPaste
        { get { return Program.SqlCopyPaste; } }
        private mainFrm mf
        { get { return Program.mainFrm1; } }
        private AxmvMapLib.AxMapLIb mv
        { get { return Program.mainFrm1.axMapLIb1; } }
        private WinMain wm
        { get { return Program.WinMain; } }

        public object CurrentTable
        {
            get { return _currentTable; }
            private set { OnPropertyChanged(ref _currentTable, value, () => this.CurrentTable); }
        }
        public TableManagerVM TableManager { get; set; }
        public mvMapLib.Cursors CurrentCursor
        {
            get { return Program.mainFrm1.axMapLIb1.CtlCursor; }
        }
        private StateType CurrState
        {
            get
            {
                if (ExistsEditedObject())
                {
                    return StateType.SelectEditeObject;
                }
                else if (ExistsSelectObject())
                {
                    return StateType.SelectObject;
                }
                else if (ExistsSelectLayer())
                {
                    return StateType.SelectLayer;
                }
                else if (ExistsEditedLayer())
                {
                    return StateType.EditedLayer;
                }
                else
                {
                    return StateType.None;
                }
            }
        }
        private StapeTypeLayerPanel CurrStatePanel
        {
            get
            {
                if (ExistsSelectVisibleLayerInPanel())
                {
                    return StapeTypeLayerPanel.SelectVisible;
                }
                else if (ExistsSelectLayerInPanel())
                {
                    return StapeTypeLayerPanel.SelectLayer;
                }
                else
                {
                    return StapeTypeLayerPanel.None;
                }
            }
        }
        public IEnumerable<ToolBar_VM> ListToolBars
        {
            get { return _listToolBars; }
        }

        public bool AddGeometryPointIsChecked
        {
            get { return _addGeometryPointIsChecked; }
            set { OnPropertyChanged(ref _addGeometryPointIsChecked, value, () => this.AddGeometryPointIsChecked); }
        }
        public bool AddGeometryLineIsChecked
        {
            get { return _addGeometryLineIsChecked; }
            set { OnPropertyChanged(ref _addGeometryLineIsChecked, value, () => this.AddGeometryLineIsChecked); }
        }
        public bool AddGeometryRectangleIsChecked
        {
            get { return _addGeometryRectangleIsChecked; }
            set { OnPropertyChanged(ref _addGeometryRectangleIsChecked, value, () => this.AddGeometryRectangleIsChecked); }
        }
        public bool AddGeometryGeometryIsChecked
        {
            get { return _addGeometryGeometryIsChecked; }
            set { OnPropertyChanged(ref _addGeometryGeometryIsChecked, value, () => this.AddGeometryGeometryIsChecked); }
        }
        public bool NodeAddIsChecked
        {
            get { return _nodeAddIsChecked; }
            set { OnPropertyChanged(ref _nodeAddIsChecked, value, () => this.NodeAddIsChecked); }
        }
        public bool NodeMoveIsChecked
        {
            get { return _nodeMoveIsChecked; }
            set { OnPropertyChanged(ref _nodeMoveIsChecked, value, () => this.NodeMoveIsChecked); }
        }
        public bool NodeDelIsChecked
        {
            get { return _nodeDelIsChecked; }
            set { OnPropertyChanged(ref _nodeDelIsChecked, value, () => this.NodeDelIsChecked); }
        }
        #endregion

        #region Команды
        public RelayCommand OpenTableCommand
        {
            get { return _openTableCommand ?? (_openTableCommand = new RelayCommand(this.OpenTable, param => this.CanOpenTable)); }
        }
        private void OpenTable(object obj)
        {
            mf.layerItemsView1.openTableToolStripMenuItem_Click(null, null);
        }
        private bool CanOpenTable
        {
            get { return _canOpenTable; }
            set
            {
                _canOpenTable = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand AddGeometryPointCommand
        {
            get { return _addGeometryPointCommand ?? (_addGeometryPointCommand = new RelayCommand(this.AddGeometryPoint, param => this.CanAddGeometryPoint)); }
        }
        private void AddGeometryPoint(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddDot);
            Program.mainFrm1.layerItemsView1.addToolStripMenuItem_Click(null, null);
        }
        private bool CanAddGeometryPoint
        {
            get { return _canAddGeometryPoint; }
            set
            {
                _canAddGeometryPoint = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand AddGeometryLineCommand
        {
            get { return _addGeometryLineCommand ?? (_addGeometryLineCommand = new RelayCommand(this.AddGeometryLine, param => this.CanAddGeometryLine)); }
        }
        private void AddGeometryLine(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolyLine);
            Program.mainFrm1.layerItemsView1.addToolStripMenuItem_Click(null, null);
        }
        private bool CanAddGeometryLine
        {
            get { return _canAddGeometryLine; }
            set
            {
                _canAddGeometryLine = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand AddGeometryGeometryCommand
        {
            get { return _addGeometryGeometryCommand ?? (_addGeometryGeometryCommand = new RelayCommand(this.AddGeometryGeometry, param => this.CanAddGeometryGeometry)); }
        }
        private void AddGeometryGeometry(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolygon);
            Program.mainFrm1.layerItemsView1.addToolStripMenuItem_Click(null, null);
        }
        private bool CanAddGeometryGeometry
        {
            get { return _canAddGeometryGeometry; }
            set
            {
                _canAddGeometryGeometry = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand AddGeometryRectangleCommand
        {
            get { return _addGeometryRectangleCommand ?? (_addGeometryRectangleCommand = new RelayCommand(this.AddGeometryRectangle, param => this.CanAddGeometryRectangle)); }
        }
        private void AddGeometryRectangle(object obj)
        {
            mf.layerItemsView1.addToolStripMenuItemRectangle_Click(null, null);
        }
        private bool CanAddGeometryRectangle
        {
            get { return _canAddGeometryRectangle; }
            set
            {
                _canAddGeometryRectangle = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand DeleteGeometryCommand
        {
            get { return _deleteGeometryCommand ?? (_deleteGeometryCommand = new RelayCommand(this.DeleteGeometry, p => this.CanDeleteGeometry)); }
        }
        public void DeleteGeometry(object obj)
        {
            if (!CanDeleteGeometry)
                return;
            mf.layerItemsView1.DeleteObjectFromLayer();
            SetButtonsState();
        }
        public bool CanDeleteGeometry
        {
            get { return _canDeleteGeometry; }
            set
            {
                _canDeleteGeometry = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand NodeAddCommand
        {
            get { return _nodeAddCommand ?? (_nodeAddCommand = new RelayCommand(this.NodeAdd, (n) => this.CanNodeAdd)); }
        }
        private void NodeAdd(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddVx);
        }
        public bool CanNodeAdd
        {
            get { return _canNodeAdd; }
            set
            {
                _canNodeAdd = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand NodeMoveCommand
        { get { return _nodeMoveCommand ?? (_nodeMoveCommand = new RelayCommand(this.NodeMove, (n) => this.CanNodeMove)); } }
        private void NodeMove(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlMoveVx);
        }
        public bool CanNodeMove
        {
            get { return _canNodeMove; }
            set
            {
                _canNodeMove = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand NodeDelCommand
        { get { return _nodeDelCommand ?? (_nodeDelCommand = new RelayCommand(this.NodeDel, (n) => this.CanNodeDel)); } }
        private void NodeDel(object obj)
        {
            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlDelVx);
        }
        public bool CanNodeDel
        {
            get { return _canNodeDel; }
            set
            {
                _canNodeDel = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand CopyInSQLiteCommand
        { get { return _copyInSQLiteCommand ?? (_copyInSQLiteCommand = new RelayCommand(this.CopyInSQLite, (n) => this.CanCopyInSQLite)); } }
        private void CopyInSQLite(object obj)
        {
            var rowNum = 0;
            TimeSpan tSnap = TimeSpan.Zero;
            try
            {
                var startDT = DateTime.Now;
                rowNum = SqlCopyPaste.CopyInSQLite(_currentTable, mf.SelectedObjectsIds);
                tSnap = DateTime.Now - startDT;
            }
            catch (Exception ex)
            {
                mf.StatusInfo = ex.Message;
            }

            if (rowNum > 0)
            {
                var countErrorGeom = mf.SelectedObjectsIds.Count - rowNum;
                var str = "Объектов скопировано: " + rowNum;
                if (countErrorGeom != 0)
                    str = str + ", пропущено: " + countErrorGeom;
                mf.StatusInfo = str;
#if DEBUG
                mf.StatusInfo += " \tЗатраченое время: " + tSnap;
#endif
                SetButtonsState();
            }
        }
        public bool CanCopyInSQLite
        {
            get { return _canCopyInSQLite; }
            set
            {
                _canCopyInSQLite = value;
                RelayCommand.UpdateStatus();
            }
        }

        public RelayCommand PastFromSQLiteCommand
        { get { return _pastFromSQLiteCommand ?? (_pastFromSQLiteCommand = new RelayCommand(this.PastFromSQLite, (n) => this.CanPastFromSQLite)); } }
        private void PastFromSQLite(object obj)
        {
            int countRow = 0;
            var rowNum = 0;
            TimeSpan tSnap = TimeSpan.Zero;
            try
            {
                var startDT = DateTime.Now;
                countRow = SqlCopyPaste.CountRowsSQLite();
                rowNum = SqlCopyPaste.PastFromSQLite(CurrentTable);
                tSnap = DateTime.Now - startDT;
            }
            catch (Exception ex)
            {
                mf.StatusInfo = ex.Message;
            }

            if (rowNum > 0)
            {
                var countErrorGeom = countRow - rowNum;
                var str = "Объектов вставлено: " + rowNum;
                if (countErrorGeom != 0)
                    str = str + ", пропущено: " + countErrorGeom;
                mf.StatusInfo = str;
#if DEBUG
                mf.StatusInfo += " \tЗатраченое время: " + tSnap;
#endif
                if (CurrentTable is DataAccess.SourceCosmetic.Model.CosmeticTableBaseM)
                {
                    mvLayer layer = mf.axMapLIb1.getLayer((CurrentTable as DataAccess.SourceCosmetic.Model.CosmeticTableBaseM).NameMap);
                    layer.ExternalFullReloadVisible();
                }
                mf.axMapLIb1.mapRepaint();
            }
        }
        public bool CanPastFromSQLite
        {
            get { return _canPastFromSQLite; }
            set
            {
                _canPastFromSQLite = value;
                RelayCommand.UpdateStatus();
            }
        }

        #endregion

        #region Методы
        /// <summary>
        /// Регистрирует новую панель ToolBar
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        internal ToolBar_VM AddPanel(string panelName)
        {
            var toolBar = new ToolBar_VM(panelName);
            toolBar.PropertyChanged += toolBar_PropertyChanged;
            _listToolBars.Add(toolBar);
            return toolBar;
        }
        public void SetButtonsState(bool isCursorChange = false)
        {
            var oldTypeGeom = _typeGeom;
            if (_currentTablePg != null)
            {
                _writeTable = classesOfMetods.getWriteTable(_currentTablePg.idTable);
                _typeGeom = _currentTablePg.TypeGeom;
            }
            else
            {
                _writeTable = false;
            }

            //if (!Program.IsV3)
            {
                //if (!_writeTable)
                if (!isCursorChange)
                {
                    switch (mv.CtlCursor)
                    {
                        case Cursors.mlAddDot:
                        case Cursors.mlAddLine:
                        case Cursors.mlAddPipe:
                        case Cursors.mlAddPolyLine:
                        case Cursors.mlAddPolygon:
                        case Cursors.mlAddVx:
                        case Cursors.mlRectangle:
                            mv.SetCursor(Cursors.mlSelect);
                            return;
                    }
                }
                //курсор добавления прямоугольника включенный в полигональном слое и переключенный в другой тип слоя
                // изменить курсор
                if (mv.CtlCursor == Cursors.mlRectangle && oldTypeGeom == TypeGeometry.MULTIPOLYGON && _typeGeom != TypeGeometry.MULTIPOLYGON)
                {
                    mv.SetCursor(Cursors.mlSelect);
                    return;
                }
            }

            switch (this.CurrStatePanel)
            {
                case StapeTypeLayerPanel.SelectLayer:
                    SetButtonStateForSelectLayerInPanel();
                    break;
                case StapeTypeLayerPanel.SelectVisible:
                    SetButtonStateForSelectVisibleLayerInPanel();
                    break;
                default:
                    SetButtonStateForNoneInPanel();
                    break;
            }
            switch (this.CurrState)
            {
                case StateType.None:
                    SetButtonStateForNone();
                    break;
                case StateType.SelectObject:
                    SetButtonStateForSelectObject();
                    break;
                case StateType.EditedLayer:
                    SetButtonStateForEditedLayer();
                    break;
                case StateType.SelectEditeObject:
                    SetButtonStateForEditeObject();
                    break;
                case StateType.SelectLayer:
                    SetButtonStateForSelectLayer();
                    break;
                default:
                    SetButtonStateForNone();
                    break;
            }

            var workSet = Program.WorkSets.CurrentWorkSet.IsDefault || Program.WorkSets.CurrentWorkSet.IsAccess;
            wm.TbbWorkSetSaveLocationLayers.IsEnabled = workSet;
        }

        /// <summary>Делать если нет выбранного слоя в панели управления слоями
        /// </summary>
        private void SetButtonStateForNoneInPanel()
        {
            CanOpenTable = false;
            CanAddGeometryPoint = false;
            CanAddGeometryLine = false;
            CanAddGeometryGeometry = false;
            wm.TbbWorkSetChangeStyleLayer.IsEnabled = false;
            wm.TbbWorkSetDeleteStyleLayer.IsEnabled = false;
            wm.TbtbAddPointOnMap.IsEnabled = false;
            wm.TbtbAddLineOnMap.IsEnabled = false;
            wm.TbtbAddPolygonOnMap.IsEnabled = false;
            wm.TbbObjectAddWithWKT.IsEnabled = false;
            wm.TbtbObjectAddRectange.IsEnabled = false;
            CanPastFromSQLite = false;
            wm.TbbZoomToObj.IsEnabled = false;
            wm.TbbZoomToLayer.IsEnabled = false;
        }

        /// <summary>Делать если выбран слой в панели управления слоями
        /// </summary>
        private void SetButtonStateForSelectLayerInPanel()
        {
            CanOpenTable = true;
            CanAddGeometryPoint = (_currentLayerM != null && _currentLayerM.IsEditable);
            CanAddGeometryLine = (_currentLayerM != null && _currentLayerM.IsEditable);
            CanAddGeometryGeometry = (_currentLayerM != null && _currentLayerM.IsEditable);
            wm.TbbWorkSetChangeStyleLayer.IsEnabled = false;
            wm.TbbWorkSetDeleteStyleLayer.IsEnabled = false;

            SetAddObjectEnabled();

            wm.TbbObjectAddWithWKT.IsEnabled = _writeTable;
            wm.TbtbObjectAddRectange.IsEnabled = (_typeGeom == TypeGeometry.MULTIPOLYGON) && _writeTable;
            CanPastFromSQLite = false;
            wm.TbbZoomToObj.IsEnabled = false;
            wm.TbbZoomToLayer.IsEnabled = false;
        }

        private void SetAddObjectEnabled()
        {
            if(_currentLayerM != null)
            {
                switch (_currentLayerM.GeomType)
                {
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.Any:
                        wm.TbtbAddPointOnMap.IsEnabled = true;
                        wm.TbtbAddLineOnMap.IsEnabled = true;
                        wm.TbtbAddPolygonOnMap.IsEnabled = true;
                        wm.TbtbObjectAddRectange.IsEnabled = true;
                        break;
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.Point:
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.MultiPoint:
                        CanAddGeometryPoint = true;
                        wm.TbtbAddPointOnMap.IsEnabled = true;
                        break;
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.Line:
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.MultiLine:
                        CanAddGeometryLine = true;
                        wm.TbtbAddLineOnMap.IsEnabled = true;
                        break;
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.Polygon:
                    case Rekod.DataAccess.AbstractSource.Model.EGeomType.MultiPolygon:
                        CanAddGeometryGeometry = true;
                        wm.TbtbAddPolygonOnMap.IsEnabled = true;
                        break;
                }
            }
        }

        /// <summary>Делать если выбран включенный слой в панели управления слоями
        /// </summary>
        private void SetButtonStateForSelectVisibleLayerInPanel()
        {
            var workSet = _currentIdTable > 0 && !Program.WorkSets.CurrentWorkSet.IsDefault && Program.WorkSets.CurrentWorkSet.IsAccess;
            CanOpenTable = true;
            CanAddGeometryPoint = (_currentLayerM != null && _currentLayerM.IsEditable);
            CanAddGeometryLine = (_currentLayerM != null && _currentLayerM.IsEditable);
            CanAddGeometryGeometry = (_currentLayerM != null && _currentLayerM.IsEditable);
            wm.TbbWorkSetChangeStyleLayer.IsEnabled = workSet;
            wm.TbbWorkSetDeleteStyleLayer.IsEnabled = workSet;

            SetAddObjectEnabled();

            wm.TbbObjectAddWithWKT.IsEnabled = _writeTable;
            if (_currentTablePg != null)
            {
                wm.TbtbObjectAddRectange.IsEnabled = (_typeGeom == TypeGeometry.MULTIPOLYGON) && _writeTable;
            }
            CanPastFromSQLite = SqlCopyPaste.ExistsPaste && (_writeTable || (SqlCopyPaste.TypeGeom == _typeGeom || SqlCopyPaste.TypeGeom == TypeGeometry.MISSING));
            wm.TbbZoomToObj.IsEnabled = (_selectCount > 0);
            wm.TbbZoomToLayer.IsEnabled = true;
        }
        /***********************************************************************/
        /// <summary>Делать если есть выбранный объект для редактирования
        /// </summary>
        private void SetButtonStateForEditeObject()
        {
            wm.TbbSnapToPoint.IsEnabled = true;
            wm.TbbSnapToLine.IsEnabled = true;
            wm.TbtbSelectingObject.IsEnabled = true;
            wm.TbtbSelectingObjectsRectangle.IsEnabled = true;
            wm.TbtbSelectingObjectPolygon.IsEnabled = true;
            wm.TbbUnselectingObjects.IsEnabled = true;
            wm.TbtbObjectAddRectange.IsEnabled = (_currentTable is CosM.CosmeticTableBaseM) || (_typeGeom == TypeGeometry.MULTIPOLYGON && _writeTable);
            CanCopyInSQLite = _selectCount > 0;
            CanDeleteGeometry = _writeTable || (_currentLayerM != null && _currentLayerM.IsEditable);
            wm.TbbRotateBtn.IsEnabled = ((_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable)) && _selectCount == 1;
            wm.TbtbRotateMouse.IsEnabled = (_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable);
            wm.TbtbMoveObj.IsEnabled = true;
            wm.TbtbVertexEdit.IsEnabled = (_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable);
            CanNodeAdd = (_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable);
            CanNodeMove = (_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable);
            CanNodeDel = (_typeGeom != TypeGeometry.MULTIPOINT && _writeTable) || (_currentLayerM != null && _currentLayerM.IsEditable);
            wm.TbtbJoinGeometry.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbIntersectGeometry.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbJoinIntersectGeometry.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbSymDifference.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbSeparatGeometry.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount > 0);
            wm.TbtbClippingPolygonAnotherPolygon.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount == 1);
            wm.TbtbClippingPolygonBySpecifyingLine.IsEnabled = (_writeTable || (_currentLayerM != null && _currentLayerM.IsEditable)) && (_selectCount > 0) && !(_typeGeom == TypeGeometry.MULTIPOINT);
        }

        /// <summary>Делать если есть выбранный слой
        /// </summary>
        private void SetButtonStateForEditedLayer()
        {
            wm.TbbSnapToPoint.IsEnabled = (_currentTablePg != null);
            wm.TbbSnapToLine.IsEnabled = (_currentTablePg != null);
            wm.TbtbSelectingObject.IsEnabled = (_currentTablePg != null);
            wm.TbtbSelectingObjectsRectangle.IsEnabled = (_currentTablePg != null);
            wm.TbtbSelectingObjectPolygon.IsEnabled = (_currentTablePg != null);
            wm.TbbUnselectingObjects.IsEnabled = (_currentTablePg != null);
            CanCopyInSQLite = _selectCount > 0;
            CanDeleteGeometry = (_currentTablePg != null || _currentLayerM != null && _currentLayerM.IsEditable) && _writeTable;
            wm.TbbRotateBtn.IsEnabled = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && (_selectCount == 1) && _writeTable;
            wm.TbtbRotateMouse.IsEnabled = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && _writeTable && (_selectCount > 0);
            wm.TbtbMoveObj.IsEnabled = (_currentTablePg != null) && _writeTable;
            wm.TbtbVertexEdit.IsEnabled = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && _writeTable;
            CanNodeAdd = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && _writeTable || _currentLayerM != null && _currentLayerM.IsEditable;
            CanNodeMove = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && _writeTable || _currentLayerM != null && _currentLayerM.IsEditable;
            CanNodeDel = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && _writeTable || _currentLayerM != null && _currentLayerM.IsEditable;
            wm.TbtbJoinGeometry.IsEnabled = (_currentTablePg != null) && _writeTable && (_selectCount == 1);
            wm.TbtbIntersectGeometry.IsEnabled = (_currentTablePg != null) && _writeTable && (_selectCount == 1);
            wm.TbtbJoinIntersectGeometry.IsEnabled = (_currentTablePg != null) && _writeTable && (_selectCount == 1);
            wm.TbtbSymDifference.IsEnabled = (_currentTablePg != null) && _writeTable && (_selectCount == 1);
            wm.TbtbSeparatGeometry.IsEnabled = (_currentTablePg != null) && _writeTable && (_selectCount > 0);
            wm.TbtbClippingPolygonAnotherPolygon.IsEnabled = (_currentTablePg != null) && (_selectCount == 1) && _writeTable;
            wm.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = (_currentTablePg != null) && (_selectCount == 1) && _writeTable;
            wm.TbtbClippingPolygonBySpecifyingLine.IsEnabled = (_currentTablePg != null) && !(_typeGeom == TypeGeometry.MULTIPOINT) && (_selectCount == 1) && _writeTable;
        }

        /// <summary>Делать если есть слои для выбора
        /// </summary>
        private void SetButtonStateForSelectLayer()
        {
            wm.TbbSnapToPoint.IsEnabled = true;
            wm.TbbSnapToLine.IsEnabled = true;
            wm.TbtbSelectingObject.IsEnabled = true;
            wm.TbtbSelectingObjectsRectangle.IsEnabled = true;
            wm.TbtbSelectingObjectPolygon.IsEnabled = true;
            wm.TbbUnselectingObjects.IsEnabled = true;
            CanCopyInSQLite = false;
            CanDeleteGeometry = false;
            wm.TbbRotateBtn.IsEnabled = false;


            wm.TbtbRotateMouse.IsEnabled = false;
            if (_currentTable is CosM.CosmeticTableBaseM)
            {
                wm.TbtbRotateMouse.IsEnabled = true; 
            }
            else if (_currentTable is Interfaces.tablesInfo && _writeTable)
            {
                tablesInfo tInfo = classesOfMetods.getTableInfo((_currentTable as Interfaces.tablesInfo).idTable);
                if (tInfo != null && tInfo.GeomType_GC.Trim() != "POINT")
                {
                    wm.TbtbRotateMouse.IsEnabled = true;
                }
            }

            wm.TbtbMoveObj.IsEnabled = (_currentTable != null) && (_writeTable || _currentTable is CosM.CosmeticTableBaseM);
            wm.TbtbVertexEdit.IsEnabled = false;
            CanNodeAdd = false;
            CanNodeMove = false;
            CanNodeDel = false;
            wm.TbtbJoinGeometry.IsEnabled = false;
            wm.TbtbIntersectGeometry.IsEnabled = false;
            wm.TbtbJoinIntersectGeometry.IsEnabled = false;
            wm.TbtbSymDifference.IsEnabled = false;
            wm.TbtbSeparatGeometry.IsEnabled = false;
            wm.TbtbClippingPolygonAnotherPolygon.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingLine.IsEnabled = false;
        }

        /// <summary>Делать если есть выбранный объект
        /// </summary>
        private void SetButtonStateForSelectObject()
        {
            wm.TbbSnapToPoint.IsEnabled = true;
            wm.TbbSnapToLine.IsEnabled = true;
            wm.TbtbSelectingObject.IsEnabled = true;
            wm.TbtbSelectingObjectsRectangle.IsEnabled = true;
            wm.TbtbSelectingObjectPolygon.IsEnabled = true;
            wm.TbbUnselectingObjects.IsEnabled = true;
            CanCopyInSQLite = _selectCount > 0;
            CanDeleteGeometry = false;
            wm.TbbRotateBtn.IsEnabled = false;
            wm.TbtbRotateMouse.IsEnabled = false;
            wm.TbtbMoveObj.IsEnabled = false;
            wm.TbtbVertexEdit.IsEnabled = false;
            CanNodeAdd = false;
            CanNodeMove = false;
            CanNodeDel = false;
            wm.TbtbJoinGeometry.IsEnabled = false;
            wm.TbtbIntersectGeometry.IsEnabled = false;
            wm.TbtbJoinIntersectGeometry.IsEnabled = false;
            wm.TbtbSymDifference.IsEnabled = false;
            wm.TbtbSeparatGeometry.IsEnabled = false;
            wm.TbtbClippingPolygonAnotherPolygon.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingLine.IsEnabled = false;
        }

        /// <summary>Делать если нет выбранный объектов и слоев
        /// </summary>
        private void SetButtonStateForNone()
        {
            wm.TbbSnapToPoint.IsEnabled = true;
            wm.TbbSnapToLine.IsEnabled = true;
            wm.TbtbSelectingObject.IsEnabled = false;
            wm.TbtbSelectingObjectsRectangle.IsEnabled = false;
            wm.TbtbSelectingObjectPolygon.IsEnabled = false;
            wm.TbbUnselectingObjects.IsEnabled = false;
            CanCopyInSQLite = false;
            CanDeleteGeometry = false;
            wm.TbbZoomToLayer.IsEnabled = false;
            wm.TbbRotateBtn.IsEnabled = false;
            wm.TbtbRotateMouse.IsEnabled = false;
            wm.TbtbMoveObj.IsEnabled = false;
            wm.TbtbVertexEdit.IsEnabled = false;
            CanNodeAdd = false;
            CanNodeMove = false;
            CanNodeDel = false;
            wm.TbtbJoinGeometry.IsEnabled = false;
            wm.TbtbIntersectGeometry.IsEnabled = false;
            wm.TbtbJoinIntersectGeometry.IsEnabled = false;
            wm.TbtbSymDifference.IsEnabled = false;
            wm.TbtbSeparatGeometry.IsEnabled = false;
            wm.TbtbClippingPolygonAnotherPolygon.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = false;
            wm.TbtbClippingPolygonBySpecifyingLine.IsEnabled = false;
        }
        /****************************************************************************/
        /// <summary>Есть ли слоя для редакитирования
        /// </summary>
        /// <returns></returns>
        private bool ExistsEditedLayer()
        {
            for (int i = 0; mv.LayersCount > i; i++)
            {
                if (mv.getLayerByNum(i).editable)
                    return true;
            }
            return false;
        }
        /// <summary>Есть выбранный объект
        /// </summary>
        /// <returns></returns>
        private bool ExistsSelectObject()
        {
            _selectCount = 0;
            for (int i = 0; mv.LayersCount > i; i++)
            {
                if (mv.getLayerByNum(i).selectable)
                {

                    if (mv.getLayerByNum(i).SelectedCount != 0)
                    {
                        _selectCount += mv.getLayerByNum(i).SelectedCount;
                    }
                }
            }
            if (_selectCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Есть ли выбранный объект в редактируемом слое
        /// </summary>
        /// <returns></returns>
        private bool ExistsEditedObject()
        {
            for (int i = 0; mv.LayersCount > i; i++)
            {
                if (mv.getLayerByNum(i).editable)
                {
                    if (mv.getLayerByNum(i).SelectedCount != 0)
                    {
                        _selectCount = mv.getLayerByNum(i).SelectedCount;
                        return true;
                    }
                    else
                    {
                        _selectCount = 0;
                        return false;
                    }
                }
            }
            return false;
        }
        /// <summary>Есть ли выбранный слой в панели управления слоями
        /// </summary>
        /// <returns></returns>
        private bool ExistsSelectLayerInPanel()
        {
            if (mf.layerItemsView1._clickedNode != null && !(mf.layerItemsView1._clickedNode.Tag is Rekod.DBLayerForms.RastrInfo))
                return true;
            else
                return false;
        }
        /// <summary>Есть ли выбранный слой, который показывается
        /// </summary>
        /// <returns></returns>
        private bool ExistsSelectVisibleLayerInPanel()
        {
            if (mf.layerItemsView1._clickedNode != null && mf.layerItemsView1._clickedNode.Checked && !(mf.layerItemsView1._clickedNode.Tag is Rekod.DBLayerForms.RastrInfo))
                return true;
            else
                return false;
        }
        /// <summary>Есть ли слои для выбора
        /// </summary>
        /// <returns></returns>
        private bool ExistsSelectLayer()
        {
            for (int i = 0; mv.LayersCount > i; i++)
            {
                if (mv.getLayerByNum(i).selectable)
                    return true;
            }
            return false;
        }

        /// <summary>Убирает все галочки на редактирование
        /// </summary>
        public void SetNotEdited()
        {
            for (int i = 0; mv.LayersCount > i; i++)
            {
                mv.getLayerByNum(i).editable = false;
            }
        }

        public ToolBar_VM FindToolBar(string name)
        {
            return _listToolBars.FirstOrDefault(f => f.Name == name);
        }
        #endregion

        #region Обработка событий
        void mv_LayerChanged(object sender, AxmvMapLib.IMapLIbEvents_LayerChangedEvent e)
        {
            MessageBox.Show(e.layer.ToString());
        }
        private void axMapLIb1_OnCursorChange(object sender, AxmvMapLib.IMapLIbEvents_OnCursorChangeEvent e)
        {
            _currCursor = e.newCursor;
            _oldCursor = e.oldCursor;
            SetButtonsState(true);

            object buttonToChange = null;
            switch (_currCursor)
            {
                case Cursors.mlDistance: buttonToChange = wm.TbtbRuler; break;
                case Cursors.mlMoveObj: buttonToChange = wm.TbtbMoveObj; break;
                case Cursors.mlPan: buttonToChange = wm.TbtbHand; break;
                case Cursors.mlRectangle: buttonToChange = wm.TbtbObjectAddRectange; break;
                case Cursors.mlRotateObj: buttonToChange = wm.TbtbRotateMouse; break;
                case Cursors.mlSelect: buttonToChange = wm.TbtbSelectingObject; break;
                case Cursors.mlZoomIn: buttonToChange = wm.TbtbZoomInRegion; break;
                case Cursors.mlZoomOut: buttonToChange = wm.TbtbZoomOutRegion; break;
                case Cursors.mlSelectRegion: buttonToChange = wm.TbtbSelectingObjectsRectangle; break;
                case Cursors.mlSelectPoly: buttonToChange = wm.TbtbSelectingObjectPolygon; break;
                case Cursors.mlVertexEdit: buttonToChange = wm.TbtbVertexEdit; break;
                case Cursors.mlAddPolygon: buttonToChange = wm.TbtbAddPolygonOnMap; break;
                case Cursors.mlAddDot: buttonToChange = wm.TbtbAddPointOnMap; break;
                case Cursors.mlAddLine: buttonToChange = wm.TbtbAddLineOnMap; break;
                case Cursors.mlGeoLink: buttonToChange = wm.TbtbMapPointInfo; break;
            }

            if (buttonToChange != wm.TbtbSelectingObject) { wm.TbtbSelectingObject.IsChecked = false; }
            if (buttonToChange != wm.TbtbSelectingObjectsRectangle) { wm.TbtbSelectingObjectsRectangle.IsChecked = false; }
            if (buttonToChange != wm.TbtbSelectingObjectPolygon) { wm.TbtbSelectingObjectPolygon.IsChecked = false; }

            if (buttonToChange != wm.TbtbAddPolygonOnMap) { wm.TbtbAddPolygonOnMap.IsChecked = false; }
            if (buttonToChange != wm.TbtbAddPointOnMap) { wm.TbtbAddPointOnMap.IsChecked = false; }
            if (buttonToChange != wm.TbtbAddLineOnMap) { wm.TbtbAddLineOnMap.IsChecked = false; }

            if (buttonToChange != wm.TbtbObjectAddRectange) { wm.TbtbObjectAddRectange.IsChecked = false; }
            if (buttonToChange != wm.TbtbHand) { wm.TbtbHand.IsChecked = false; }
            if (buttonToChange != wm.TbtbRuler) { wm.TbtbRuler.IsChecked = false; }
            if (buttonToChange != wm.TbtbZoomInRegion) { wm.TbtbZoomInRegion.IsChecked = false; }
            if (buttonToChange != wm.TbtbZoomOutRegion) { wm.TbtbZoomOutRegion.IsChecked = false; }
            if (buttonToChange != wm.TbtbRotateMouse) { wm.TbtbRotateMouse.IsChecked = false; }
            if (buttonToChange != wm.TbtbMoveObj) { wm.TbtbMoveObj.IsChecked = false; }
            if (buttonToChange != wm.TbtbVertexEdit) { wm.TbtbVertexEdit.IsChecked = false; }
            if (buttonToChange != wm.TbtbNodeAdd) { wm.TbtbNodeAdd.IsChecked = false; }
            if (buttonToChange != wm.TbtbNodeMove) { wm.TbtbNodeMove.IsChecked = false; }
            if (buttonToChange != wm.TbtbNodeDelete) { wm.TbtbNodeDelete.IsChecked = false; }
            if(buttonToChange != wm.TbtbMapPointInfo) { wm.TbtbMapPointInfo.IsChecked = false; }


            AddGeometryPointIsChecked = false;
            AddGeometryLineIsChecked = false;
            AddGeometryGeometryIsChecked = false;
            NodeAddIsChecked = false;
            NodeMoveIsChecked = false;
            NodeDelIsChecked = false;


            switch (_currCursor)
            {
                case Cursors.mlDistance:
                    wm.TbtbRuler.IsChecked = true;
                    break;
                case Cursors.mlMoveObj:
                    wm.TbtbMoveObj.IsChecked = true;
                    break;
                case Cursors.mlPan:
                    wm.TbtbHand.IsChecked = true;
                    break;
                case Cursors.mlRectangle:
                    wm.TbtbObjectAddRectange.IsChecked = true;
                    break;
                case Cursors.mlRotateObj:
                    wm.TbtbRotateMouse.IsChecked = true;
                    break;
                case Cursors.mlSelect:
                    wm.TbtbSelectingObject.IsChecked = true;
                    break;
                case Cursors.mlZoomIn:
                    wm.TbtbZoomInRegion.IsChecked = true;
                    break;
                case Cursors.mlZoomOut:
                    wm.TbtbZoomOutRegion.IsChecked = true;
                    break;
                case Cursors.mlSelectRegion:
                    wm.TbtbSelectingObjectsRectangle.IsChecked = true;
                    break;
                case Cursors.mlSelectPoly:
                    wm.TbtbSelectingObjectPolygon.IsChecked = true;
                    break;
                case Cursors.mlVertexEdit:
                    wm.TbtbVertexEdit.IsChecked = true;
                    break;

                case Cursors.mlAddVx:
                    NodeAddIsChecked = true;
                    break;
                case Cursors.mlMoveVx:
                    NodeMoveIsChecked = true;
                    break;
                case Cursors.mlDelVx:
                    NodeDelIsChecked = true;
                    break;
                default:
                    break;
            }
            if (_currentIdTable > 0)
            {
                switch (_currCursor)
                {
                    case Cursors.mlAddPolygon:
                        wm.TbtbAddPolygonOnMap.IsChecked = true;
                        break;
                    case Cursors.mlAddDot:
                        wm.TbtbAddPointOnMap.IsChecked = true;
                        break;
                    case Cursors.mlAddPolyLine:
                        wm.TbtbAddLineOnMap.IsChecked = true;
                        break;
                     case Cursors.mlGeoLink:
                        wm.TbtbMapPointInfo.IsChecked = true;
                        break;
                }
            }
            else if (_currentLayerM != null)
            {
                switch (_currCursor)
                {
                    case Cursors.mlAddDot:
                        AddGeometryPointIsChecked = true;
                        break;
                    case Cursors.mlAddPolyLine:
                    case Cursors.mlAddLine:
                        AddGeometryLineIsChecked = true;
                        break;
                    case Cursors.mlAddPolygon:
                        AddGeometryGeometryIsChecked = true;
                        break;
                }
            }
        }
        void mv_ObjectUnselected(object sender, AxmvMapLib.IMapLIbEvents_ObjectUnselectedEvent e)
        {
            SetButtonsState();
        }
        void mv_ObjectSelected(object sender, AxmvMapLib.IMapLIbEvents_ObjectSelectedEvent e)
        {
            this._selectCount = e.ids.count;
            SetButtonsState();
        }

        /// <summary>
        /// Обработка событий изменения свойств 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
        void layerItemsView1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "EditableIdLayer":
                    {
                        _currentIdTable = mf.layerItemsView1.EditableIdLayer;
                        _currentTablePg = Program.app.getTableInfo(_currentIdTable);
                        CurrentTable = _currentTablePg;
                    } break;
                case "EditableLayer":
                    {
                        _currentLayerM = mf.layerItemsView1.EditableLayer;
                        CurrentTable = _currentLayerM;
                    } break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Перессылка событий с toolBar элемента на событие ListToolBarsPropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolBar_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _listToolBars_PropertyChanged(sender, e);
        }
        void SqlCopyPaste_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ExistsPaste":
                    {
                        _existsPaste = SqlCopyPaste.ExistsPaste;
                    } break;
                case "TypeGeom":
                    {
                        _copyTypeGeom = SqlCopyPaste.TypeGeom;
                    } break;
                default:
                    break;
            }
        }
        #endregion // Обработка событий

        public enum StateType { None, SelectObject, EditedLayer, SelectEditeObject, SelectLayer };
        public enum StapeTypeLayerPanel { None, SelectLayer, SelectVisible };
    }
}