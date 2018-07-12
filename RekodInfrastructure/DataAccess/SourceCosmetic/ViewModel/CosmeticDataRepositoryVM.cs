using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.TableManager.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using PgV = Rekod.DataAccess.SourcePostgres.View;
using CosV = Rekod.DataAccess.SourceCosmetic.View;
using System.Windows.Data;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Services;
using System.Windows.Controls;
using Rekod.Behaviors;
using mvMapLib;
using System.Windows;
using System.Windows.Media;

namespace Rekod.DataAccess.SourceCosmetic.ViewModel
{
    public class CosmeticDataRepositoryVM : AbsVM.DataRepositoryVM
    {
        #region Поля
        private int _selectedTabControlIndex = 0;
        #endregion Поля

        #region Свойства
        public int SelectedTabControlIndex
        {
            get { return _selectedTabControlIndex; }
            set { OnPropertyChanged(ref _selectedTabControlIndex, value, () => SelectedTabControlIndex); }
        }
        #endregion Свойства

        #region Команды
        #region AddLayersCommand
        private ICommand _addLayersCommand;
        /// <summary>
        /// Добавить новый косметический слой
        /// </summary>
        public ICommand AddLayersCommand
        {
            get { return _addLayersCommand ?? (_addLayersCommand = new RelayCommand(AddLayer)); }
        }
        /// <summary>
        /// Добавить новый слой в коллекцию источника
        /// </summary>
        /// <param name="parameter">Слой который нужно добавить</param>
        public void AddLayer(object parameter = null)
        {
            if (!(parameter is BindingProxy))
            {
                return;
            }
            (parameter as BindingProxy).Data = null;
            (parameter as BindingProxy).Data = new CosM.CosmeticTableBaseM(this, Convert.ToInt32(this.MapViewer.SRID));
            SelectedTabControlIndex = 0;
        }
        #endregion AddLayersCommand

        #region DeleteLayersCommand
        private ICommand _deleteLayerCommand;
        /// <summary>
        /// Удалить выбранный слой
        /// </summary>
        public ICommand DeleteLayerCommand
        {
            get { return _deleteLayerCommand ?? (_deleteLayerCommand = new RelayCommand(DeleteLayer, CanDeleteLayer)); }
        }
        private void DeleteLayer(object obj = null)
        {
            var table = (CosM.CosmeticTableBaseM)obj;
            if (_tables.Contains(table))
            {
                _tables.Remove(table);
                var layer = _source.mv.getLayer(table.Name);
                if (layer != null)
                {
                    layer.deleteLayer();
                    Program.mainFrm1.layerItemsView1.RefreshLayers();
                }
            }
        }
        private bool CanDeleteLayer(object obj = null)
        {
            return obj != null && obj is CosM.CosmeticTableBaseM;
        }
        #endregion DeleteLayersCommand

        #region BeginValidationCommand
        private ICommand _beginValidationCommand;
        /// <summary>
        /// Команда для начала валидации
        /// </summary>
        public ICommand BeginValidationCommand
        {
            get { return _beginValidationCommand ?? (_beginValidationCommand = new RelayCommand(this.BeginValidation, this.CanBeginValidation)); }
        }
        /// <summary>
        /// Начало валидации
        /// </summary>
        public void BeginValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                var control = commEventPar.CommandParameter as FrameworkElement;
                if (control != null)
                {
                    var bindings = FindBindingGroups(control);
                    foreach (var binding in bindings)
                    {
                        binding.BeginEdit();
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли начать валидацию
        /// </summary>
        public bool CanBeginValidation(object parameter = null)
        {
            return parameter != null;
        }
        #endregion // BeginValidationCommand

        #region CancelValidationCommand
        private ICommand _cancelValidationCommand;
        /// <summary>
        /// Отменяет введенные значения в BindingGroup
        /// </summary>
        public ICommand CancelValidationCommand
        {
            get { return _cancelValidationCommand ?? (_cancelValidationCommand = new RelayCommand(this.CancelValidation, this.CanCancelValidation)); }
        }
        /// <summary>
        /// Отменить введенные значения
        /// </summary>
        public void CancelValidation(object parameter = null)
        {
            var control = parameter as FrameworkElement;
            if (control != null)
            {
                var bindings = FindBindingGroups(control);
                foreach (var binding in bindings)
                {
                    binding.CancelEdit();
                    binding.BeginEdit();
                }
            }
        }
        /// <summary>
        /// Можно ли отменить введенные значения
        /// </summary>
        public bool CanCancelValidation(object parameter = null)
        {
            return true;
        }
        #endregion // CancelValidationCommand

        #region SaveValidationCommand
        private ICommand _saveValidationCommand;
        /// <summary>
        /// Применить измененные значения в BindingGroup
        /// </summary>
        public ICommand SaveValidationCommand
        {
            get { return _saveValidationCommand ?? (_saveValidationCommand = new RelayCommand(this.SaveValidation, this.CanSaveValidation)); }
        }
        /// <summary>
        /// Сохранить измененные значения в BindingGroup
        /// </summary>
        public void SaveValidation(object parameter = null)
        {
            CosM.CosmeticTableBaseM cosLayer = null;
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                var commEventPar = parameter as Rekod.Behaviors.CommandEventParameter;
                var control = commEventPar.CommandParameter as FrameworkElement;
                if (control != null)
                {
                    var bindings = FindBindingGroups(control);
                    for (int i = 0; i < bindings.Count; i++)
                    {
                        if (bindings[i].CommitEdit() == false)
                        {
                            return;
                        }
                    }
                }
                var bindProxy = commEventPar.ExtraParameter as BindingProxy;
                cosLayer = bindProxy.Data as CosM.CosmeticTableBaseM;
                if (cosLayer.IsNewTable)
                {
                    RegisterLayer(cosLayer);
                }
                ApplyStyleOnMap(cosLayer);
                var t = Tables.FirstOrDefault(p => (p as CosM.CosmeticTableBaseM).Name == cosLayer.Name);
                bindProxy.Data = t;
            }
        }
        /// <summary>
        /// Можно ли сохранить измененные значения
        /// </summary>
        public bool CanSaveValidation(object parameter = null)
        {
            return true;
        }
        #endregion SaveValidationCommand

        #region ErrorValidationCommand
        private ICommand _errorValidationCommand;
        /// <summary>
        /// Команда, которая запускается, если при валидации обнаружены ошибки
        /// </summary>
        public ICommand ErrorValidationCommand
        {
            get { return _errorValidationCommand ?? (_errorValidationCommand = new RelayCommand(this.ErrorValidation, this.CanErrorValidation)); }
        }
        /// <summary>
        /// Обработать ошибки валидации
        /// </summary>
        public void ErrorValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;

                ValidationErrorEventArgs e = commEventPar.EventArgs as ValidationErrorEventArgs;
                if (e != null && e.Action == ValidationErrorEventAction.Added)
                {
                    System.Windows.MessageBox.Show(e.Error.ErrorContent.ToString());
                }
            }
        }
        /// <summary>
        /// Можно ли обработать ошибки валидации
        /// </summary>
        public bool CanErrorValidation(object parameter = null)
        {
            return true;
        }
        #endregion ErrorValidationCommand
        #endregion Команды

        #region Конструкторы
        public CosmeticDataRepositoryVM(ITableManagerVM source) : base(source, AbsM.ERepositoryType.Cosmetic, true)
        {
            Title = Properties.Resources.LocCosmeticConfig;
            Text = Properties.Resources.LocSourceCosmetic;
            var group = new AbsM.GroupM(this, 0) { Text = "Группа косметических слоев" };
            _groups.Add(group);
            ReloadInfo();
        }
        #endregion Конструкторы

        #region Методы
        #region Методы AbsM.IDataRepositoryVM
        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public override void ReloadInfo()
        {
            _groups[0].Tables.Clear();
            var listTabe = _tables.ToList();
            foreach (var item in listTabe)
            {
                var table = (CosM.CosmeticTableBaseM)item;
                if(MapViewer.getLayer(table.NameMap)==null)
                {
                    this.DeleteLayer(table);
                }
            }
            foreach (var item in _tables)
            {
                var table = (CosM.CosmeticTableBaseM)item;
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    MGroupAddTable(_groups[0], table);
                }
            }
            Program.mainFrm1.layerItemsView1.RefreshLayers();

            CollectionViewSource.GetDefaultView(_layers).Refresh();
            CollectionViewSource.GetDefaultView(Groups.ElementAt(0).Tables).Refresh();
        }
        public override object OpenTable(AbsM.ITableBaseM table, object id = null, bool isSelected = false, WindowViewModelBase_VM ownerMV = null)
        {
            //(new layerInfo(_mv, table.Name)).Show();
            //return null;

            if (table != null)
            {
                var tableViewVM = new CosmeticTableViewVM(table as CosM.CosmeticTableBaseM);
                var tableViewV = new CosV.CosmeticTableViewV();
                if (isSelected)
                    return OpenWindowDialog(
                                tableViewV,
                                tableViewVM,
                                767, 570,
                                500, 300,
                                ownerMV
                            );
                else
                    OpenWindow(
                                tableViewV,
                                tableViewVM,
                                767, 570,
                                500, 300,
                                ownerMV
                            );
            }
            return null;
        }
        public override void OpenTableSettings(AbsM.ITableBaseM iTable, UIElement positionElement = null)
        {
            var vmpTable = iTable as Model.CosmeticTableBaseM;
            var vmpLayerV = new View.CosmeticLayerV();
            vmpLayerV.DataContext = new Rekod.Services.BindingProxy() { Data = vmpTable };
            Window window = new Window();
            window.Title = String.Format("Редактирование свойств слоя \"{0}\"", vmpTable.Text);
            window.Content = vmpLayerV;
            window.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            window.Height = 450;
            window.Width = 370;
            window.MinHeight = 450;
            window.MinWidth = 370;
            window.Owner = Program.WinMain;
            if (positionElement != null)
            {
                System.Windows.Point pt = positionElement.TranslatePoint(new System.Windows.Point(0, 0), Program.WinMain);
                window.Top = pt.Y;
                window.Left = pt.X;
            }
            window.Show();
        }
        #endregion Методы AbsM.IDataRepositoryVM

        #region Методы AbsVM.DataRepositoryVM

        /// <summary>
        /// Найти все BindingGroups
        /// </summary>
        public static List<BindingGroup> FindBindingGroups(FrameworkElement depObj)
        {
            List<BindingGroup> bindings = new List<BindingGroup>();

            foreach (var e in FindElementsWithBindingGroups(depObj))
            {
                if (!bindings.Contains(e.BindingGroup))
                    bindings.Add(e.BindingGroup);
            }

            return bindings;
        }

        /// <summary>
        /// Найти дочерние элементы, содержащие BindingGroup
        /// </summary>
        private static IEnumerable<FrameworkElement> FindElementsWithBindingGroups(FrameworkElement depObj)
        {
            if (depObj != null)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (child != null
                        && child is FrameworkElement)
                    {
                        var fe = child as FrameworkElement;
                        if (fe.BindingGroup != null)
                        {
                            yield return fe;
                        }
                        else
                        {
                            foreach (FrameworkElement childOfChild in FindElementsWithBindingGroups(fe))
                            {
                                yield return childOfChild;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Удалить объекты слоя
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="ids">Список ID объектов</param>
        /// <returns>Результат операции</returns>
        public bool DeleteObjects(CosM.CosmeticTableBaseM table, mvIntArray ids)
        {
            if (table != null && ids != null)
            {
                try
                {
                    var layer = MapViewer.getLayer(table.Name);
                    if (layer != null)
                    {
                        bool result = true;

                        for (int i = 0; i < ids.count; i++)
                        {
                            try
                            {
                                mvVectorObject obj = layer.getObject(ids.getElem(i));
                                int id = Convert.ToInt32((string)obj.fieldValue(table.PkFieldName));
                                result &= table.DeleteObject(id);
                            }
                            catch { result = false; }
                        }
                        result &= layer.DeleteArray(ids);

                        return result;
                    }
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// Функция изменения видимости слоя. Возвращает видимость слоя после попытки изменения
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        internal override bool MakeLayerVisible(AbsM.TableBaseM table, bool? value)
        {
            mvLayer layer = _mv.getLayer(table.Name);
            layer.Visible = value ?? false;
            return layer.Visible;
        }
        internal override bool MakeLayerEditable(AbsM.TableBaseM table, bool? value)
        {
            var layer = _mv.getLayer(table.Name);
            if (layer != null)
            {
                layer.editable = value ?? false;
                return layer.editable;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Функция изменения выбираемости слоя. Возвращает true если слой существует и выбираемый
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal override bool MakeLayerSelectable(AbsM.TableBaseM table, bool? value)
        {
            mvMapLib.mvLayer layer = _mv.getLayer(table.Name);
            if (layer != null)
            {
                layer.selectable = value ?? false;
                return layer.selectable;
            }
            else
            {
                return false;
            }
        }
        #endregion // Методы AbsVM.DataRepositoryVM

        /// <summary>
        /// Открыть окно атрибутов объекта
        /// </summary>
        /// <param name="table">Слой</param>
        /// <param name="id"></param>
        /// <param name="wkt"></param>
        /// <param name="ownerMV"></param>
        public override void OpenObject(AbsM.ITableBaseM table, object idObject, string wkt = null, WindowViewModelBase_VM ownerMV = null)
        {
            if (idObject != null && !(idObject is int))
                throw new Exception("Неверный параметр");

            var cosmTable = table as CosM.CosmeticTableBaseM;
            var cosmAttributeVM = new CosmeticAttributes.CosmeticAttributesVM(table, (int?)idObject, false, wkt);

            cosmAttributeVM.Reload();

            OpenWindow(
                new View.CosmeticAttributes.CosmeticAttributesV(),
                cosmAttributeVM,
                750, 550,
                400, 300,
                ownerMV);
        }
        /// <summary>
        /// Создает новый слой в системе с указанным именем
        /// </summary>
        /// <param name="layerName"></param>
        public void CreateLayer(String layerName)
        {
            var cosLayer = new CosM.CosmeticTableBaseM(this, Convert.ToInt32(_mv.SRID))
                {
                    Text = layerName,
                    GeomType = AbsM.EGeomType.Any
                };
            RegisterLayer(cosLayer);
            ApplyStyleOnMap(cosLayer);
        }
        /// <summary>
        /// Регистрирует новый косметический слой в системе
        /// </summary>
        /// <param name="cosLayer">Слой который нужно зарегистрировать в источнике</param>
        private void RegisterLayer(CosM.CosmeticTableBaseM cosLayer)
        {
            // Если таблица новая, создаем слой на карте
            if (cosLayer.IsNewTable)
            {
                cosLayer.Name = Guid.NewGuid().ToString();
                cosLayer.NameMap = cosLayer.Name;
                _tables.Add(cosLayer);
                Groups.ElementAt(0).Tables.Add(cosLayer);
                mvStringArray ff = new mvStringArray();
                ff.count = cosLayer.Fields.Count - 1;
                ff.setElem(0, cosLayer.PkFieldName);
                int i = 1;
                foreach (var f in cosLayer.Fields)
                {
                    if (f.Name != cosLayer.GeomFieldName
                        && f.Name != cosLayer.PkFieldName)
                    {
                        ff.setElem(i++, f.Name);
                    }
                }
                var layer = _mv.CreateLayer(cosLayer.NameMap, ff);
                cosLayer.IsVisible = true;
            }
            CosM.CosmeticTableBaseM.UpdateIsNewTable(cosLayer, false);
        }
        private void ApplyStyleOnMap(CosM.CosmeticTableBaseM cosLayer)
        {
            var mvLayer = Program.mainFrm1.axMapLIb1.getLayer(cosLayer.Name);
            bool isPrevVisible = mvLayer != null ? mvLayer.Visible : false;

            cosLayer.IsVisible = false;

            // Указываем стиль по умолчанию для слоя и стиль подписей                       
            mvLayer = Program.mainFrm1.axMapLIb1.getLayer(cosLayer.Name);
            if (mvLayer != null)
            {
                mvLayer.MaxScale = (uint)cosLayer.DefaultStyle.MaxScale;
                mvLayer.MinScale = (uint)cosLayer.DefaultStyle.MinScale;
                mvLayer.usebounds = cosLayer.DefaultStyle.UseBounds;

                var symbolStyle = new mvSymbolObject()
                {
                    shape = (uint)cosLayer.DefaultStyle.Symbol
                };
                var penStyle = new mvPenObject()
                {
                    Color = (uint)cosLayer.DefaultStyle.PenColor,
                    ctype = (ushort)cosLayer.DefaultStyle.PenType,
                    width = (uint)cosLayer.DefaultStyle.PenWidth
                };
                var brushStyle = new mvBrushObject()
                {                     
                    bgcolor = (uint)cosLayer.DefaultStyle.BrushBgColor,
                    fgcolor = (uint)cosLayer.DefaultStyle.BrushFgColor,
                    hatch = (ushort)cosLayer.DefaultStyle.BrushHatch,
                    style = (ushort)cosLayer.DefaultStyle.BrushStyle
                };
                var fontStyle = new mvFontObject()
                {
                    fontname = cosLayer.DefaultStyle.FontName,
                    Color = (uint)cosLayer.DefaultStyle.FontColor,
                    framecolor = (uint)cosLayer.DefaultStyle.FontFrameColor,
                    graphicUnits = cosLayer.DefaultStyle.GraphicUnits,
                    size = cosLayer.DefaultStyle.FontSize
                };

                if (cosLayer.DefaultStyle.BrushStyle == 0 && cosLayer.DefaultStyle.BrushHatch == 0)
                {
                    int grey = Convert.ToInt32(255 & (int)(cosLayer.DefaultStyle.Opacity * 255));
                    brushStyle.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
                }
                else if (!cosLayer.DefaultStyle.HasBackground)
                {
                    brushStyle.bgcolor = 0xffffffff;
                }


                cosLayer.SetDefaultStyle(symbolStyle, fontStyle, penStyle, brushStyle);

                mvFontObject labelFontObj = new mvFontObject();
                labelFontObj.Color = cosLayer.LabelStyle.LabelFontColor;
                labelFontObj.fontname = cosLayer.LabelStyle.LabelFontName;
                if (cosLayer.LabelStyle.LabelShowFrame)
                {
                    labelFontObj.framecolor = cosLayer.LabelStyle.LabelFrameColor;
                }
                else
                {
                    labelFontObj.framecolor = 0xFFFFFFFF;
                }
                labelFontObj.graphicUnits = cosLayer.LabelStyle.LabelUseGraphicUnits;
                labelFontObj.italic = cosLayer.LabelStyle.LabelFontItalic;
                labelFontObj.size = cosLayer.LabelStyle.LabelFontSize;
                labelFontObj.strikeout = cosLayer.LabelStyle.LabelFontStrikeout;
                labelFontObj.underline = cosLayer.LabelStyle.LabelFontUnderline;
                if (cosLayer.LabelStyle.LabelFontBold)
                {
                    labelFontObj.weight = 650;
                }
                mvLayer.showlabels = true;
                if (cosLayer.LabelStyle.LabelUseBounds)
                {
                    mvLayer.labelBounds = true;
                    mvLayer.labelMinScale = cosLayer.LabelStyle.LabelMinScale;
                    mvLayer.labelMaxScale = cosLayer.LabelStyle.LabelMaxScale;
                }
                mvLayer.labelParallel = cosLayer.LabelStyle.LabelParallel;
                mvLayer.labelOverlap = cosLayer.LabelStyle.LabelOverlap;
                mvLayer.labelOffset = cosLayer.LabelStyle.LabelOffset;
                mvLayer.LabelField = 1;
                mvLayer.SetLabelstyle(labelFontObj);

                cosLayer.IsVisible = isPrevVisible;

                _mv.mapRepaint();
                mvLayer.Update();
                ReloadInfo();
            }
        }
        #endregion Методы
    }
}