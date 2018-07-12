using mvMapLib;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.MapLayersItemInfo.Model
{
    public class ObjectInfo : ViewModelBase
    {
        #region Поля
        private int _objectId = -1;
        private ObservableCollection<IAttributeM> _attributes;
        private mvPointWorld _objectCenter;
        private bool _isSelected;
        private LayerInfo _parentLayer;
        private string _text;
        #endregion Поля

        #region Конструкторы
        public ObjectInfo(LayerInfo parentLayer, Int32 objectId)
        {
            _objectId = objectId;
            _parentLayer = parentLayer;
            PropertyChanged += ObjectInfo_PropertyChanged;
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<IAttributeM> Attributes
        {
            get { return _attributes ?? (_attributes = new ObservableCollection<IAttributeM>()); }
        }
        #endregion Коллекции

        #region Свойства
        public bool IsSelected
        {
            get { return _isSelected; }
            set { OnPropertyChanged(ref _isSelected, value, () => this.IsSelected); }
        }
        public int ObjectId
        {
            get { return _objectId; }
        }
        public mvPointWorld ObjectCenter
        {
            get { return _objectCenter; }
            set { _objectCenter = value; }
        }
        public mvVectorObject MvObject { get; set; }
        public LayerInfo ParentLayer
        {
            get { return _parentLayer; }
        }
        public String Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion Свойства

        #region Команды
        #region OpenAttributesWindowCommand
        private ICommand _openAttributesWindowCommand;
        /// <summary>
        /// Команда для открытия окна атрибутики
        /// </summary>
        public ICommand OpenAttributesWindowCommand
        {
            get { return _openAttributesWindowCommand ?? (_openAttributesWindowCommand = new RelayCommand(this.OpenAttributesWindow, this.CanOpenAttributesWindow)); }
        }
        /// <summary>
        /// Открытие окна атрибутики
        /// </summary>
        public void OpenAttributesWindow(object parameter = null)
        {
            if (CanOpenAttributesWindow(parameter))
            {
                if (ParentLayer.TableType == ETableType.Cosmetic)
                {
                    var cosmLayer = Program.TablesManager.CosmeticRepository.FindTableByName(ParentLayer.LayerName) as DataAccess.SourceCosmetic.Model.CosmeticTableBaseM;
                    cosmLayer.Source.OpenObject(cosmLayer, ObjectId);
                }
                else
                {
                    var table = Program.app.getTableInfo(ParentLayer.LayerId);
                    Program.work.OpenForm.ShowAttributeObject(table, ObjectId, (ObjectId <= 0), null);
                }
            }
        }
        /// <summary>
        /// Можно ли открыть окно атрибутики
        /// </summary>
        public bool CanOpenAttributesWindow(object parameter = null)
        {
            bool result = false;
            if (ParentLayer.TableType == AbsM.ETableType.MapLayer || ParentLayer.TableType == ETableType.Cosmetic)
            {
                result = true;
            }
            return result;
        }
        #endregion OpenAttributesWindowCommand

        #region ShowObjectCommand
        private ICommand _showObjectCommand;
        /// <summary>
        /// Команда для показа объекта
        /// </summary>
        public ICommand ShowObjectCommand
        {
            get { return _showObjectCommand ?? (_showObjectCommand = new RelayCommand(this.ShowObject, this.CanShowObject)); }
        }
        /// <summary>
        /// Показать объект
        /// </summary>
        public void ShowObject(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(ParentLayer.LayerName);
            if (layer == null || layer.Visible != true)
            {
                // Пытаемся включить слой
                Program.mainFrm1.layerItemsView1.SetLayerVisible(ParentLayer.LayerId);
                layer = Program.mainFrm1.axMapLIb1.getLayer(ParentLayer.LayerName);
                if (layer == null || layer.Visible != true)
                    return;
            }
            try
            {
                if (ObjectId != -1)
                {
                    mvMapLib.mvVectorObject mvObj = layer.getObject(ObjectId);
                    layer.DeselectAll();
                    if (Program.SettingsXML.LocalParameters.EnterTheScreen)
                    {
                        if (mvObj.VectorType != mvMapLib.mvVecTypes.mvVecPoint)
                        {
                            mvIntArray ids = new mvIntArray();
                            ids.count = 1;
                            ids.setElem(0, ObjectId);
                            Program.mainFrm1.MoveToObjects(ids, layer);
                            //Program.mainFrm1.axMapLIb1.SetExtent(mvObj.bbox);
                        }
                        Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                    }
                    else
                    {
                        layer.MoveTo(ObjectId, false);
                    }
                }
                else
                {
                    Program.mainFrm1.axMapLIb1.MoveTo(ObjectCenter, Convert.ToInt32(Program.mainFrm1.axMapLIb1.ScaleZoom));
                }
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
        }
        /// <summary>
        /// Можно ли показать объект
        /// </summary>
        public bool CanShowObject(object parameter = null)
        {
            return true;
        }
        #endregion ShowObjectCommand

        #region SelectObjectCommand
        private ICommand _selectObjectCommand;
        /// <summary>
        /// Команда для выделения объекта
        /// </summary>
        public ICommand SelectObjectCommand
        {
            get { return _selectObjectCommand ?? (_selectObjectCommand = new RelayCommand(this.SelectObject, this.CanSelectObject)); }
        }
        /// <summary>
        /// Выделение объекта
        /// </summary>
        public void SelectObject(object parameter = null)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(ParentLayer.LayerName);
            if (layer == null || layer.Visible != true)
            {
                // Пытаемся включить слой
                Program.mainFrm1.layerItemsView1.SetLayerVisible(ParentLayer.LayerId);
                layer = Program.mainFrm1.axMapLIb1.getLayer(ParentLayer.LayerName);
                if (layer == null || layer.Visible != true)
                    return;
            }
            if (ObjectId != -1)
            {
                layer.SelectId(ObjectId);
                Program.mainFrm1.axMapLIb1.mapRepaint();
                //mvMapLib.mvVectorObject mvObj = layer.getObject(ObjectId);
                //mvObj.Selected = true;
            }
        }
        /// <summary>
        /// Можно ли выделить объект
        /// </summary>
        public bool CanSelectObject(object parameter = null)
        {
            return ObjectId != -1;
        }
        #endregion SelectObjectCommand
        #endregion Команды

        #region Обработчики
        void ObjectInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (IsSelected)
                {
                    if (_parentLayer.TableType == ETableType.MapLayer)
                    {
                        Attributes.Clear();
                        int idTable = Program.RelationVisbleBdUser.GetIdTable(_parentLayer.LayerName);
                        if (idTable > 0)
                        {
                            PgM.PgTableBaseM pgTable = Program.repository.FindTable(idTable) as PgM.PgTableBaseM;
                            PgVM.PgAttributes.PgAttributesListVM pgAttrListVM = new PgVM.PgAttributes.PgAttributesListVM(pgTable, ObjectId, false, loadStyle: false, isReadOnly: true);
                            pgAttrListVM.Reload();

                            Attributes.Add(pgAttrListVM.PkAttribute);
                            foreach (var attr in pgAttrListVM.Attributes)
                            {
                                Attributes.Add(attr as IAttributeM);
                            }
                        }
                    }
                }
            }
        }
        #endregion Обработчики
    }
}