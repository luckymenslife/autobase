using Rekod.AttachedProperties;
using Rekod.Controllers;
using Rekod.Behaviors;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourceCosmetic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Data;
using mvMapLib;
using System.Diagnostics;

namespace Rekod.DataAccess.SourceCosmetic.ViewModel
{
    public class CosmeticTableViewVM : WindowViewModelBase_VM
    {
        #region Поля
        private CosmeticTableBaseM _cosTable;
        private ObservableCollection<HeaderValue> _columns;
        private bool _isReadOnly;
        private int _tableRows;
        private DataTable _data;
        private AxmvMapLib.AxMapLIb _axMapLib;
        private mvMapLib.mvLayer _mvLayer;
        private DataRowView _currentRowView;
        private bool _moveToObject;
        private string _rowFilter;
        private ObservableCollection<DataRowView> _selectedItems;
        #endregion Поля

        #region Конструкторы
        public CosmeticTableViewVM(CosmeticTableBaseM cosTable)
        {
            this.MoveToObject = false;
            _axMapLib = (cosTable.Source as CosmeticDataRepositoryVM).MapViewer;
            _mvLayer = _axMapLib.getLayer(cosTable.Name);
            _cosTable = cosTable;
            _isReadOnly = true;
            _filterVM = new CosmeticTableViewFilterVM(this, null);
            Title = String.Format("{0}: \"{1}\"", Rekod.Properties.Resources.CosTableView_CosmeticLayer, cosTable.Text);

            foreach (var field in _cosTable.Fields)
            {
                if (field.Type != AbsM.EFieldType.Geometry)
                {
                    DataColumn dc = new DataColumn(field.Name, typeof(String));
                    Data.Columns.Add(dc);
                    if (cosTable.PrimaryKeyField == field)
                    {
                        dc.ReadOnly = true;
                        Data.PrimaryKey = new[] { dc };
                    }
                }
            }

            GetColumns();
            Reload();

            PropertyChanged += CosmeticTableViewVM_PropertyChanged;
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            Reload();
        }
        #endregion Конструкторы

        #region Свойства
        public String RowFilter
        {
            get { return _rowFilter; }
            set { OnPropertyChanged(ref _rowFilter, value, () => this.RowFilter); }
        }
        public CosmeticTableBaseM CosTable
        {
            get { return _cosTable; }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        /// <summary>
        /// Кол-во строк в таблице
        /// </summary>
        public int TableRows
        {
            get { return _tableRows; }
            private set { OnPropertyChanged(ref _tableRows, value, () => this.TableRows); }
        }
        public bool MoveToObject
        {
            get { return _moveToObject; }
            set { OnPropertyChanged(ref _moveToObject, value, () => this.MoveToObject); }
        }
        public CosmeticTableViewFilterVM FilterVM
        {
            get { return _filterVM; }
        }
        #endregion Свойства

        #region Коллекции
        /// <summary>
        /// Коллекция колонок
        /// </summary>
        public ObservableCollection<HeaderValue> Columns
        {
            get { return _columns ?? (_columns = new ObservableCollection<HeaderValue>()); }
        }
        /// <summary>
        /// Данные с таблицы
        /// </summary>
        public DataTable Data
        {
            get
            {
                return _data ?? (_data = new DataTable());
            }
        }
        public ObservableCollection<DataRowView> SelectedItems
        {
            get { return _selectedItems ?? (_selectedItems = new ObservableCollection<DataRowView>()); }
        }
        #endregion Коллекции

        #region Методы
        private void GetColumns()
        {
            var listGridColumns = new List<HeaderValue>();
            foreach (var item in _cosTable.Fields)
            {
                if (item.Type == AbsM.EFieldType.Geometry)
                    continue;
                var gridColumn = Columns.FirstOrDefault(f => f.Name == item.Name);
                if (gridColumn == null)
                    gridColumn = new HeaderValue
                    {
                        Header = item,
                        Name = item.Name
                    };
                if (item == _cosTable.PrimaryKeyField)
                {
                    gridColumn.IsReadOnly = true;
                }
                else
                {
                    gridColumn.IsReadOnly = false;
                }
                listGridColumns.Add(gridColumn);
            }
            ExtraFunctions.Sorts.SortList(Columns, listGridColumns);
        }
        public void MoveToSelectedObjects()
        {
            try
            {
                if (SelectedItems.Count > 0)
                {
                    if (SelectedItems.Count == 1)
                    {
                        int _idObj = Convert.ToInt32(SelectedItems[0].Row["id"]);
                        mvMapLib.mvVectorObject mvObj = _mvLayer.getObject(_idObj);
                        _mvLayer.DeselectAll();
                        _mvLayer.SelectId(_idObj);

                        if (mvObj != null)
                        {
                            if (mvObj.VectorType != mvMapLib.mvVecTypes.mvVecPoint)
                            {
                                if (mvObj.points.count == 0)
                                {
                                    Program.mainFrm1.StatusInfo = "Объект не содержит геометрию";
                                }
                                else
                                {
                                    if (Program.SettingsXML.LocalParameters.EnterTheScreen)
                                    {
                                        Program.mainFrm1.MoveToSelectedObjects(_mvLayer);
                                        //Program.mainFrm1.axMapLIb1.SetExtent(mvObj.bbox);
                                        //Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                                    }
                                    else
                                    {
                                        _mvLayer.MoveTo(_idObj, true);
                                    }
                                }
                            }
                            else
                            {
                                Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                            }

                            //mvObj.Selected = true;
                            Program.mainFrm1.axMapLIb1.mapRepaint();
                        }
                    }
                    else
                    {
                        _mvLayer.DeselectAll();
                        mvMapLib.mvIntArray ids = new mvMapLib.mvIntArray();
                        ids.count = SelectedItems.Count;
                        int i = 0;
                        foreach (var row in SelectedItems)
                        {
                            ids.setElem(i, Convert.ToInt32(row.Row["id"]));
                            i++;
                        }
                        _mvLayer.SelectArray(ids);
                        Program.mainFrm1.MoveToSelectedObjects(_mvLayer);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.mainFrm1.StatusInfo = "Ошибка: " + ex.Message;
            }
        }
        #endregion Методы

        #region Команды
        #region ReloadCommand
        private ICommand _reloadCommand;
        /// <summary>
        /// Команда обнавления списка объектов
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        /// <summary>
        /// Команда обнавления списка объектов
        /// </summary>
        public void Reload(object obj = null)
        {
            Data.Clear();
            _mvLayer.RemoveDeletedObjects();
            TableRows = _mvLayer.ObjectsCount;
            for (int i = 1; i <= TableRows; i++)
            {
                DataRow dr = Data.NewRow();
                mvVectorObject mvObject = _mvLayer.getObjectByNum(i);
                foreach (DataColumn col in Data.Columns)
                {
                    var colValue = mvObject.fieldValue(col.ColumnName);
                    dr[col.ColumnName] = colValue;
                }
                Data.Rows.Add(dr);
            }
        }
        private bool CanReload(object obj = null)
        {
            return true;
        }
        #endregion ReloadCommand

        #region AddObjectCommand
        private ICommand _addObjectCommand;
        /// <summary>
        /// Команда для добавления объекта
        /// </summary>
        public ICommand AddObjectCommand
        {
            get { return _addObjectCommand ?? (_addObjectCommand = new RelayCommand(this.AddObject, this.CanAddObject)); }
        }
        /// <summary>
        /// Добавление объекта
        /// </summary>
        public void AddObject(object parameter = null)
        {
            String newObjectType = parameter.ToString().ToUpper();
            switch (newObjectType)
            {
                case "POINT":
                    {
                        _cosTable.Source.OpenObject(_cosTable, null);
                        break;
                    }
                case "LINE":
                    {
                        break;
                    }
                case "POLYGON":
                    {
                        break;
                    }
            }
        }
        /// <summary>
        /// Можно ли добавить объект
        /// </summary>
        public bool CanAddObject(object parameter = null)
        {
            return true;
        }
        #endregion // AddObjectCommand

        #region OpenObjectCommand
        private ICommand _openObjectCommand;
        /// <summary>
        /// Команда для открытия окна атрибутики выбранного объекта
        /// </summary>
        public ICommand OpenObjectCommand
        {
            get { return _openObjectCommand ?? (_openObjectCommand = new RelayCommand(this.OpenObject, this.CanOpenObject)); }
        }
        /// <summary>
        /// Открытие окна атрибутики выбранного объекта
        /// </summary>
        public void OpenObject(object parameter = null)
        {
            if (CanOpenObject(parameter))
            {
                DataRow dr = SelectedItems[0].Row;
                int objectId = Convert.ToInt32(dr[Data.PrimaryKey[0]]);
                _cosTable.Source.OpenObject(_cosTable, objectId);
            }
        }
        /// <summary>
        /// Можно ли открыть окно атрибутики
        /// </summary>
        public bool CanOpenObject(object parameter = null)
        {
            return (this.SelectedItems.Count == 1);
        }
        #endregion OpenObjectCommand

        #region DeleteObjectCommand
        private ICommand _deleteObjectCommand;
        private CosmeticTableViewFilterVM _filterVM;
        /// <summary>
        /// Команда для удаления текущего объекта
        /// </summary>
        public ICommand DeleteObjectCommand
        {
            get { return _deleteObjectCommand ?? (_deleteObjectCommand = new RelayCommand(this.DeleteObject, this.CanDeleteObject)); }
        }
        /// <summary>
        /// Удаление текущего объекта
        /// </summary>
        public void DeleteObject(object parameter = null)
        {
            if (CanDeleteObject(parameter))
            {
                mvIntArray ids = new mvIntArray();
                ids.count = SelectedItems.Count;
                for (int i = 0; i < SelectedItems.Count; i++)
                {
                    DataRow dr = SelectedItems[i].Row;
                    ids.setElem(i, Convert.ToInt32(dr["id"]));
                }
                _mvLayer.DeleteArray(ids);
                _mvLayer.RemoveDeletedObjects();
                _axMapLib.mapRepaint();
                Reload();
            }
        }
        /// <summary>
        /// Можно ли удалить текущий объект
        /// </summary>
        public bool CanDeleteObject(object parameter = null)
        {
            return SelectedItems.Count > 0;
        }
        #endregion DeleteObjectCommand

        #region RemoveAllCommand
        private ICommand _removeAllCommand;
        /// <summary>
        /// Команда для удаления всех строк
        /// </summary>
        public ICommand RemoveAllCommand
        {
            get { return _removeAllCommand ?? (_removeAllCommand = new RelayCommand(this.RemoveAll, this.CanRemoveAll)); }
        }
        /// <summary>
        /// Удаление всех строк
        /// </summary>
        public void RemoveAll(object parameter = null)
        {
            _mvLayer.RemoveDeletedObjects();
            _mvLayer.RemoveObjects();
            _mvLayer.RemoveDeletedObjects();
            _axMapLib.mapRepaint();
            Reload();
        }
        /// <summary>
        /// Можно ли удалить все строки
        /// </summary>
        public bool CanRemoveAll(object parameter = null)
        {
            return true;
        }
        #endregion RemoveAllCommand

        #region UpdateObjectAttributeCommand
        private ICommand _updateObjectAttributeCommand;
        /// <summary>
        /// Команда для обновления атрибута объекта
        /// </summary>
        public ICommand UpdateObjectAttributeCommand
        {
            get { return _updateObjectAttributeCommand ?? (_updateObjectAttributeCommand = new RelayCommand(this.UpdateObjectAttribute, this.CanUpdateObjectAttribute)); }
        }
        /// <summary>
        /// Обновление атрибута объекта
        /// </summary>
        public void UpdateObjectAttribute(object parameter = null)
        {
            var commEvtParam = parameter as CommandEventParameter;
            if (commEvtParam != null)
            {
                if (this.SelectedItems.Count > 0)
                {
                    System.Windows.Controls.DataGridCellEditEndingEventArgs e = commEvtParam.EventArgs as System.Windows.Controls.DataGridCellEditEndingEventArgs;
                    String labelValue = (e.EditingElement as System.Windows.Controls.TextBox).Text;
                    int objectId = Convert.ToInt32(this.SelectedItems[this.SelectedItems.Count-1].Row.ItemArray[0]);
                    _mvLayer.getObject(objectId).SetAttribute("label", labelValue);
                    _mvLayer.RemoveDeletedObjects();
                    _mvLayer.Update();
                    _axMapLib.mapRepaint();
                }
            }
        }
        /// <summary>
        /// Можно ли обновить атрибут объекта
        /// </summary>
        public bool CanUpdateObjectAttribute(object parameter = null)
        {
            return true;
        }
        #endregion // UpdateObjectAttributeCommand

        #region CopySelectedObjectsCommand
        private ICommand _copySelectedObjectsCommand;
        /// <summary>
        /// Команда для копирования выделенных объектов
        /// </summary>
        public ICommand CopySelectedObjectsCommand
        {
            get { return _copySelectedObjectsCommand ?? (_copySelectedObjectsCommand = new RelayCommand(this.CopySelectedObjects, this.CanCopySelectedObjects)); }
        }
        /// <summary>
        /// Копирование выделенных объектов
        /// </summary>
        public void CopySelectedObjects(object parameter = null)
        {
            _mvLayer.RemoveDeletedObjects();
            try
            {
                foreach (DataRowView drv in SelectedItems)
                {
                    DataRow dr = drv.Row;
                    int id = Convert.ToInt32(dr["id"]);
                    mvVectorObject mvObj = _mvLayer.getObject(id);
                    mvVectorObject newObj = _mvLayer.CreateObject();
                    newObj.setWKT(mvObj.getWKT());
                    String labelValue = mvObj.fieldValue("label") == null ? null : mvObj.fieldValue("label").ToString();
                    newObj.SetAttribute("label", labelValue);
                    newObj.SetAttribute("id", _cosTable.NextObjectId.ToString());
                    switch (mvObj.VectorType)
                    {
                        case mvVecTypes.mvVecLine:
                            newObj.style = _cosTable.DefaultLineStyle.Value;
                            break;
                        case mvVecTypes.mvVecPoint:
                            newObj.style = _cosTable.DefaultDotStyle.Value;
                            break;
                        case mvVecTypes.mvVecRegion:
                            newObj.style = _cosTable.DefaultPolygonStyle.Value;
                            break;
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                System.Windows.MessageBox.Show(Rekod.Properties.Resources.CosTableView_ObjectNotExists);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            Reload();
            _mvLayer.ExternalFullReloadVisible();
            _axMapLib.mapRepaint();
        }
        /// <summary>
        /// Можно ли копировать выделенные объекты
        /// </summary>
        public bool CanCopySelectedObjects(object parameter = null)
        {
            return SelectedItems.Count > 0;
        }
        #endregion CopySelectedObjectsCommand
        #endregion Команды

        #region Обработчики
        void CosmeticTableViewVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RowFilter":
                    {
                        Data.DefaultView.RowFilter = String.Format("id LIKE '%{0}%' OR label LIKE '%{0}%'", RowFilter);
                        break;
                    }
                case "MoveToObject":
                    {
                        if (MoveToObject)
                        {
                            MoveToSelectedObjects();
                        }
                        break;
                    }
            }
        }
        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (MoveToObject)
            {
                MoveToSelectedObjects();
            }
        }
        #endregion Обработчики
    }
}