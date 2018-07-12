using AxmvMapLib;
using mvMapLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.MapLayersItemInfo.Model;
using System.Collections.ObjectModel;
using Rekod.Services;
using Rekod.Controllers;
using System.Windows.Input;
using System.Windows;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.Behaviors;

namespace Rekod.MapLayersItemInfo.ViewModel
{
    public class MapLayersItemInfoVM: WindowViewModelBase_VM
    {
        const int BUFFER_PIXEL_SIZE=10;
        #region Поля
        private AxMapLIb _axMapLib;
        private mvMapLib.Cursors? _currentCursor;
        private mvPointWorld? _centerGlobal;
        private ObservableCollection<LayerInfo> _layers;
        private System.Windows.Controls.Primitives.ToggleButton _toolStripInfoButton;
        private System.Globalization.CultureInfo _cultureInfo = new System.Globalization.CultureInfo("en-US");
        private bool _movingToObject;
        private bool _selectObject = true;
        #endregion Поля

        #region Коллекции
        public ObservableCollection<LayerInfo> Layers
        {
            get { return _layers ?? (_layers = new ObservableCollection<LayerInfo>()); }
        }
        #endregion Коллекции

        #region Свойства
        /// <summary>
        /// Нужно ли перемещаться к объектам
        /// </summary>
        public bool MovingToObject
        {
            get { return _movingToObject; }
            set { OnPropertyChanged(ref _movingToObject, value, () => this.MovingToObject); }
        }
        public bool SelectObject
        {
            get { return _selectObject; }
            set { OnPropertyChanged(ref _selectObject, value, () => this.SelectObject); }
        }
        #endregion Свойства

        #region Конструкторы
        public MapLayersItemInfoVM()
        {
            Title = Properties.Resources.Info_ObjectsInfo;
            _axMapLib = Program.mainFrm1.axMapLIb1;
            _toolStripInfoButton = Program.WinMain.TbtbMapPointInfo;
            _toolStripInfoButton.Checked += TbtbMapPointInfo_CheckedChanged;
            _toolStripInfoButton.Unchecked += TbtbMapPointInfo_CheckedChanged;
            if ((bool)_toolStripInfoButton.IsChecked)
            {
                _currentCursor = (mvMapLib.Cursors?)_axMapLib.CtlCursor;
                _axMapLib.SetCursor(mvMapLib.Cursors.mlGeoLink);
                _axMapLib.MouseMoveEvent += AxMapLib_MouseMoveEvent;
                _axMapLib.MouseDownEvent += AxMapLib_MouseDownEvent;
            }
        }
        #endregion Конструкторы

        #region Методы
        private void LoadLayerInfo()
        {
            Layers.Clear();
            if ((bool)_toolStripInfoButton.IsChecked)
            {
                if (_centerGlobal == null) throw new Exception();
                if (CreateTempLayer() == false) throw new Exception();

                for (int i = 0; i < _axMapLib.LayersCount; i++)
                {
                    mvLayer mvL = _axMapLib.getLayerByNum(i);
                    mvL = _axMapLib.getLayer(mvL.NAME);
                    if (mvL.NAME == "tmpLayer") continue;
                    if (mvL.Visible == false) continue;
                    if (mvL.External)
                    {
                        mvIntArray pointedObjects = mvL.getObjectsAtPoint(_centerGlobal.Value, BUFFER_PIXEL_SIZE);
                        if (pointedObjects.count > 0)
                        {
                            string sys_name_table = Program.RelationVisbleBdUser.GetNameForUser(mvL.NAME);
                            tablesInfo ti = classesOfMetods.getTableInfoOfNameMap(sys_name_table);
                            int idT = ti.idTable;
                            var fList = classesOfMetods.getFieldInfoTable(idT);
                            if (idT > 0)
                            {
                                LayerInfo layer = new LayerInfo(ti.GeomType_GC, AbsM.ETableType.MapLayer);
                                layer.LayerId = idT;
                                layer.LayerName = mvL.NAME;
                                layer.Text = ti.nameMap;
                                layer.MvLayer = mvL;
                                for (int j = 0; j < pointedObjects.count; j++)
                                {
                                    mvVectorObject vObj = mvL.getObjectByNum(pointedObjects.getElem(j));
                                    if (vObj == null) continue;
                                    ObjectInfo obj = new ObjectInfo(layer, Convert.ToInt32(vObj.fieldValue("id").ToString()));
                                    obj.Text = String.Format("Id: {0}", obj.ObjectId);
                                    obj.ObjectCenter = vObj.Centroid;
                                    obj.MvObject = vObj;
                                    layer.Objects.Add(obj);
                                }
                                if (layer.Objects.Count > 0)
                                {
                                    Layers.Add(layer);
                                }
                            }
                        }
                    }
                    else
                    {
                        mvIntArray pointedObjects = mvL.getObjectsAtPoint(_centerGlobal.Value, BUFFER_PIXEL_SIZE);
                        if (pointedObjects.count > 0)
                        {                           
                            AbsM.ITableBaseM iTableBase = 
                                Program.TablesManager.CosmeticRepository.Tables.FirstOrDefault(t => (t as AbsM.TableBaseM).NameMap == mvL.NAME);
                            LayerInfo layer = null;
                            if (iTableBase != null)
                            {
                                layer = new LayerInfo("NONE", AbsM.ETableType.Cosmetic);
                                layer.Text = iTableBase.Text;
                            }
                            else
                            {
                                layer = new LayerInfo("NONE", AbsM.ETableType.BottomLayer);
                                layer.Text = mvL.NAME;
                            }
                            layer.LayerId = -1;
                            layer.LayerName = mvL.NAME;
                            layer.MvLayer = mvL;
                            for (int j = 0; j < pointedObjects.count; j++)
                            {
                                mvVectorObject vObj = mvL.getObjectByNum(pointedObjects.getElem(j));
                                if (vObj == null) continue;

                                ObjectInfo objectInfo = new ObjectInfo(layer, Convert.ToInt32(vObj.fieldValue("id")));
                                objectInfo.ObjectCenter = vObj.Centroid;
                                objectInfo.MvObject = vObj;
                                for (int t = 0; t < mvL.FieldsCount; t++)
                                {
                                    AbsM.FieldM field = new AbsM.FieldM(-1, mvL.FieldName(t).ToString()) { Text = mvL.FieldName(t).ToString() };
                                    bool temp = false;
                                    String fieldValue = "";
                                    object val = mvL.FieldValueByNum(pointedObjects.getElem(j), t, out temp);
                                    if (val != null)
                                    {
                                        fieldValue = val.ToString();
                                    }
                                    AttributeInfo atr = new AttributeInfo(field, fieldValue) { IsReadOnly = true };
                                    objectInfo.Text = String.Format("{0}: {1}", field.Name, fieldValue);
                                    objectInfo.Attributes.Add(atr);
                                }
                                layer.Objects.Add(objectInfo);
                            }
                            if (layer.Objects.Count > 0)
                            {
                                Layers.Add(layer);
                            }                  
                        }
                    }
                }
                _axMapLib.getLayer("tmpLayer").deleteLayer();

                if (Layers.Count > 0)
                {
                    if (Layers[0].Objects.Count > 0)
                    {
                        Layers[0].Objects[0].IsSelected = true;
                    }
                }
                if (!AttachedWindow.IsVisible)
                {
                    AttachedWindow.Show();
                }
            }
        }
        private bool CreateTempLayer()
        {
            try
            {
                var fieldsr = new mvStringArray();
                fieldsr.count = 1;
                fieldsr.setElem(0, "id");
                if (_axMapLib.getLayer("tmpLayer") != null)
                {
                    _axMapLib.getLayer("tmpLayer").deleteLayer();
                }
                mvLayer tmpLayer = _axMapLib.CreateLayer("tmpLayer", fieldsr);
                double radiuse = _axMapLib.ScaleZoom * 0.0015;

                //tmpLayer.AddObjectDot(1, centrG.Value.x, centrG.Value.y);
                mvVectorObject tmpObj = tmpLayer.CreateObject();
                string sWKT = string.Format(_cultureInfo, "MULTIPOLYGON ((({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1})))",
                    _centerGlobal.Value.x - radiuse, _centerGlobal.Value.y - radiuse, _centerGlobal.Value.x + radiuse, _centerGlobal.Value.y + radiuse);
                tmpObj.setWKT(sWKT);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private string GetNameField(List<fieldInfo> fLiset, string nameDb)
        {
            return fLiset.Find(w => w.nameDB == nameDb).nameMap;
        }
        protected override bool Closing(object obj)
        {
            Window window = obj as Window;
            window.Hide();
            return false;
        }
        #endregion Методы

        #region Команды
        #region HideWindowCommand
        private ICommand _hideWindowCommand;
        /// <summary>
        /// Команда для скрытия окна поиска
        /// </summary>
        public ICommand HideWindowCommand
        {
            get { return _hideWindowCommand ?? (_hideWindowCommand = new RelayCommand(this.HideWindow, this.CanHideWindow)); }
        }
        /// <summary>
        /// Скрытие окна поиска
        /// </summary>
        public void HideWindow(object parameter = null)
        {
            AttachedWindow.Hide();
        }
        /// <summary>
        /// Можно ли скрыть окно поиска
        /// </summary>
        public bool CanHideWindow(object parameter = null)
        {
            return true;
        }
        #endregion HideWindowCommand 

        #region MoveToObjectCommand
        private ICommand _moveToObjectCommand;
        /// <summary>
        /// Команда для перелета к объекту
        /// </summary>
        public ICommand MoveToObjectCommand
        {
            get { return _moveToObjectCommand ?? (_moveToObjectCommand = new RelayCommand(this.MoveToObject, this.CanMoveToObject)); }
        }
        /// <summary>
        /// Перелет к объекту
        /// </summary>
        public void MoveToObject(object parameter = null)
        {
            if (CanMoveToObject(parameter))
            {
                CommandEventParameter commEvtParam = parameter as CommandEventParameter;
                RoutedPropertyChangedEventArgs<object> eventArgs = commEvtParam.EventArgs as RoutedPropertyChangedEventArgs<object>;
                if (eventArgs.NewValue is ObjectInfo)
                {
                    ObjectInfo searchObject = eventArgs.NewValue as ObjectInfo;
                    if (_movingToObject)
                    {
                        searchObject.ShowObject();
                    }
                    if (_selectObject)
                    {
                        searchObject.SelectObject();
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли перелететь к объекту
        /// </summary>
        public bool CanMoveToObject(object parameter = null)
        {
            return _movingToObject || _selectObject;
        }
        #endregion MoveToObjectCommand
        #endregion Команды

        #region Обработчики
        void TbtbMapPointInfo_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton toolStripButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (toolStripButton == null) return;
            if ((bool)toolStripButton.IsChecked)
            {
                _currentCursor = _axMapLib.CtlCursor;
                _axMapLib.SetCursor(mvMapLib.Cursors.mlGeoLink);
                _axMapLib.MouseMoveEvent += AxMapLib_MouseMoveEvent;
                _axMapLib.MouseDownEvent += AxMapLib_MouseDownEvent;
            }
            else
            {
                if (_axMapLib.CtlCursor == mvMapLib.Cursors.mlGeoLink && _currentCursor != null)
                {
                    if (_currentCursor.Value == mvMapLib.Cursors.mlAddDot ||
                        _currentCursor.Value == mvMapLib.Cursors.mlAddLine ||
                        _currentCursor.Value == mvMapLib.Cursors.mlAddPipe ||
                        _currentCursor.Value == mvMapLib.Cursors.mlAddPolygon ||
                        _currentCursor.Value == mvMapLib.Cursors.mlAddPolyLine ||
                        _currentCursor.Value == mvMapLib.Cursors.mlAddVx ||
                        _currentCursor.Value == mvMapLib.Cursors.mlDelVx ||
                        _currentCursor.Value == mvMapLib.Cursors.mlMoveVx)
                    {
                        _currentCursor = mvMapLib.Cursors.mlSelect;
                    }
                    if (_currentCursor == 0)
                    {
                        _axMapLib.SetCursor(mvMapLib.Cursors.mlPan);
                    }
                    else
                    {
                        _axMapLib.SetCursor(_currentCursor.Value);
                    }
                }
                _axMapLib.MouseMoveEvent -= new AxmvMapLib.IMapLIbEvents_MouseMoveEventHandler(AxMapLib_MouseMoveEvent);
                _axMapLib.MouseDownEvent -= new AxmvMapLib.IMapLIbEvents_MouseDownEventHandler(AxMapLib_MouseDownEvent);
            }
        }
        void AxMapLib_MouseDownEvent(object sender, IMapLIbEvents_MouseDownEvent e)
        {
            if (e.button == TxMouseButton.mvMouseLeft)
            {
                LoadLayerInfo();
            }
        }
        void AxMapLib_MouseMoveEvent(object sender, IMapLIbEvents_MouseMoveEvent e)
        {
            mvPointWindow centr = new mvPointWindow() { x = e.winx, y = e.winy };
            _centerGlobal = _axMapLib.win2Global(centr);
        }
        #endregion Обработчики
    }
}