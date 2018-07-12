using mvMapLib;
using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.MapLayersItemInfo.Model
{
    public class LayerInfo : ViewModelBase
    {
        #region Поля
        private string _layerName = "";
        private int _layerId = -1;
        private ObservableCollection<ObjectInfo> _objects;
        private AbsM.EGeomType _geomType;
        private AbsM.ETableType _tableType;
        private string _text;
        #endregion Поля

        #region Конструкторы
        public LayerInfo(String geomTypeName, AbsM.ETableType tableType)
        {
            _tableType = tableType;
            _geomType = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes.PgAttributesGeomVM.GetGeometryType(geomTypeName);
        }
        #endregion Конструкторы

        #region Свойства
        public string LayerName
        {
            get { return _layerName; }
            set { _layerName = value; }
        }
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        public int LayerId
        {
            get { return _layerId; }
            set { _layerId = value; }
        }
        public AbsM.EGeomType GeomType
        {
            get { return _geomType; }
        }
        public AbsM.ETableType TableType
        {
            get { return _tableType; }
        }
        public ObservableCollection<ObjectInfo> Objects
        {
            get { return _objects ?? (_objects = new ObservableCollection<ObjectInfo>()); }
        }
        public mvLayer MvLayer { get; set; }
        #endregion Свойства

        #region Команды
        #region OpenTableCommand
        private ICommand _openTableCommand;
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
        public void OpenTable(object parameter = null)
        {
            switch (TableType)
            {
                case Rekod.DataAccess.AbstractSource.Model.ETableType.BottomLayer:
                    {
                        try
                        {
                            layerInfo LayerInfo1 = new layerInfo(Program.mainFrm1.axMapLIb1, LayerName);
                            LayerInfo1.Show();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Rekod.Properties.Resources.LIV_ErrorOpenTable + ": " + ex.Message);
                        }
                        break;
                    }
                case Rekod.DataAccess.AbstractSource.Model.ETableType.MapLayer:
                case Rekod.DataAccess.AbstractSource.Model.ETableType.Catalog:
                case Rekod.DataAccess.AbstractSource.Model.ETableType.Interval:
                case Rekod.DataAccess.AbstractSource.Model.ETableType.Data:
                case Rekod.DataAccess.AbstractSource.Model.ETableType.View:
                    {
                        try
                        {
                            Program.mainFrm1.layersManager1.openTableGrid(LayerId);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Rekod.Properties.Resources.LIV_NotOpenTable + ":\n" + ex.Message);
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// Можно ли открыть таблицу
        /// </summary>
        public bool CanOpenTable(object parameter = null)
        {
            return true;
        }
        #endregion OpenTableCommand 

        #region ZoomToSelectedObjectsCommand
        private ICommand _zoomToSelectedObjectsCommand;
        /// <summary>
        /// Команда для перехода к выделенным объектам
        /// </summary>
        public ICommand ZoomToSelectedObjectsCommand
        {
            get { return _zoomToSelectedObjectsCommand ?? (_zoomToSelectedObjectsCommand = new RelayCommand(this.ZoomToSelectedObjects, this.CanZoomToSelectedObjects)); }
        }
        /// <summary>
        /// Переход к выделенным объектам
        /// </summary>
        public void ZoomToSelectedObjects(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(LayerName);
            Program.mainFrm1.MoveToSelectedObjects(layer);
        }
        /// <summary>
        /// Можно ли перейти к выделенным объектам
        /// </summary>
        public bool CanZoomToSelectedObjects(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(LayerName);
            if (layer!=null)
            {
                return layer.SelectedCount > 0;
            }
            else
            {
                return false;
            }
        }
        #endregion ZoomToSelectedObjectsCommand

        #region ZoomToLayerCommand
        private ICommand _zoomToLayerCommand;
        /// <summary>
        /// Команда для перехода ко всему слою
        /// </summary>
        public ICommand ZoomToLayerCommand
        {
            get { return _zoomToLayerCommand ?? (_zoomToLayerCommand = new RelayCommand(this.ZoomToLayer, this.CanZoomToLayer)); }
        }
        /// <summary>
        /// Переход к слою
        /// </summary>
        public void ZoomToLayer(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(LayerName);
            Program.mainFrm1.SetExtentFromLayer(layer);           
        }
        /// <summary>
        /// Можно ли перейти к слою
        /// </summary>
        public bool CanZoomToLayer(object parameter = null)
        {
            return true;
        }
        #endregion // ZoomToLayerCommand

        #region MakeEditableCommand
        private ICommand _makeEditableCommand;
        /// <summary>
        /// Сделать слой редактируемым
        /// </summary>
        public ICommand MakeEditableCommand
        {
            get { return _makeEditableCommand ?? (_makeEditableCommand = new RelayCommand(this.MakeEditable, this.CanMakeEditable)); }
        }
        /// <summary>
        /// Сделать слой редактируемым
        /// </summary>
        public void MakeEditable(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(LayerName);
            if (layer != null)
            {
                Program.mainFrm1.layerItemsView1.SetLayerIsEditable(LayerId);
                Program.mainFrm1.bManager.SetButtonsState();
                //layer.editable = true;
            }
        }
        /// <summary>
        /// Команда чтобы сделать слой редактируемым
        /// </summary>
        public bool CanMakeEditable(object parameter = null)
        {
            return TableType == AbsM.ETableType.MapLayer;
        }
        #endregion MakeEditableCommand
        #endregion Команды
    }
}