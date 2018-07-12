using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Interfaces;
using Npgsql;
using System.Xml;
using Rekod.Model;
using AxmvMapLib;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using VMPM = Rekod.DataAccess.SourceVMP.Model;
using mvMapLib;
using TmM = Rekod.DataAccess.TableManager.Model;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourceVMP.ViewModel
{
    /// <summary>
    /// Класс работы со слоями подложки
    /// </summary>
    public class VMPDataRepositoryVM : AbsVM.DataRepositoryVM
    {
        #region Поля
        private List<mvLayer> _bottomLayers { get; set; }
        #endregion // Поля

        #region Конструкторы
        public VMPDataRepositoryVM(TmM.ITableManagerVM source)
            : base(source, AbsM.ERepositoryType.VMP, true)
        {
            _groups.Add(new AbsM.GroupM(this, 0) { Text = "Группа слоев подложки" });

            _bottomLayers = new List<mvLayer>();
            for (int i = 0; i < _mv.LayersCount; i++)
            {
                mvLayer ll = _mv.getLayerByNum(i);
                ll.selectable = false;
                _bottomLayers.Add(ll);
            }
            //ReloadInfo();
            Title = Properties.Resources.LocBaseLayersConfig;
            Text = Properties.Resources.LocSourceBaseLayers;
        }
        #endregion // Конструкторы

        #region Методы
        /// <summary>
        /// Загрузка слоев подложки (выполняется только один раз)
        /// </summary>
        public void UpdateBottomLayers()
        {
            foreach (mvLayer layer in _bottomLayers)
            {
                var table = FindTable(layer.NAME) as VMPM.VMPTableBaseModel;
                if (table != null)
                {
                    table.UseBounds = layer.usebounds;
                    table.MinScale = (int)layer.MinScale;
                    table.MaxScale = (int)layer.MaxScale;
                }
                else
                {
                    table = new VMPM.VMPTableBaseModel(this, layer.NAME, AbsM.ETableType.BottomLayer)
                            {
                                NameMap = layer.NAME,
                                Name = layer.NAME,
                                Text = layer.NAME,
                                GeomType = AbsM.EGeomType.None,
                                MinScale = (int)layer.MinScale,
                                MaxScale = (int)layer.MaxScale,
                                UseBounds = layer.usebounds,
                                IsVisible = layer.Visible,
                                IsReadOnly = true,
                                Tag = null
                            };
                    try
                    {
                        _tables.Add(table);
                        MGroupAddTable(_groups[0], table);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        #region Методы AbsM.IDataRepositoryVM
        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public override void ReloadInfo()
        {
            UpdateBottomLayers();
        }
        public void ReloadFullInfo()
        {
            _tables.Clear();
            _groups[0].Tables.Clear();
            _bottomLayers = new List<mvLayer>();
            for (int i = 0; i < _mv.LayersCount; i++)
            {
                mvLayer ll = _mv.getLayerByNum(i);
                ll.selectable = false;
                _bottomLayers.Add(ll);
            }
            UpdateBottomLayers();
        }

        public override object OpenTable(AbsM.ITableBaseM table, object id = null, bool isSelected = false, WindowViewModelBase_VM ownerMV = null)
        {
            (new layerInfo(_mv, table.Name)).Show();
            return null;
        }
        #endregion // Методы AbsM.IDataRepositoryVM

        #region Методы AbsVM.DataRepositoryVM
        /// <summary>
        /// Функция изменения видимости слоя. Возвращает видимость слоя после попытки изменения
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        internal override bool MakeLayerVisible(AbsM.TableBaseM table, bool? value)
        {
            var vmpTable = table as VMPM.VMPTableBaseModel;
            bool isChecked = (bool)(value);
            var layer = _mv.getLayer(table.Name);
            layer.Visible = isChecked;
            if (layer.Visible)
            {
                layer.usebounds = vmpTable.UseBounds;
                layer.MinScale = (uint)vmpTable.MinScale;
                layer.MaxScale = (uint)vmpTable.MaxScale;
            }
            return layer.Visible;
        }
        /// <summary>
        /// Функция изменения выбираемости слоя. Возвращает true если слой существует и выбираемый
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal override bool MakeLayerSelectable(AbsM.TableBaseM table, bool? value)
        {
            bool isChecked = (bool)(value);
            mvMapLib.mvLayer layer = _mv.getLayer(table.Name);
            if (layer != null)
            {
                layer.selectable = isChecked;

                isChecked = layer.selectable;
            }
            else
            {
                isChecked = false;
            }
            return isChecked;
        }
        /// <summary>
        /// Показать окно настроек таблицы
        /// </summary>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        public override void OpenTableSettings(AbsM.ITableBaseM iTable, UIElement positionElement = null)
        {
            Model.VMPTableBaseModel vmpTable = iTable as Model.VMPTableBaseModel;
            View.VMPLayerV vmpLayerV = new View.VMPLayerV();
            vmpLayerV.DataContext = new Rekod.Services.BindingProxy() { Data = vmpTable };
            Window window = new Window();
            window.Title = String.Format("Редактирование свойств слоя \"{0}\"", vmpTable.Text);
            window.Content = vmpLayerV;
            window.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            window.Height = 240;
            window.Width = 370;
            window.MinHeight = 240;
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
        #endregion // Методы AbsVM.DataRepositoryVM

        #region Интерфейс IDisposable
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion // Интерфейс IDisposable
        #endregion // Методы

        #region Команды
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
            if (parameter is BindingGroup)
            {
                BindingGroup bindGroup = parameter as BindingGroup;
                bindGroup.BeginEdit();
            }
        }
        /// <summary>
        /// Можно ли начать валидацию
        /// </summary>
        public bool CanBeginValidation(object parameter = null)
        {
            return true;
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
            if (parameter is BindingGroup)
            {
                BindingGroup bindGroup = parameter as BindingGroup;
                bindGroup.CancelEdit();
                bindGroup.BeginEdit();
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
            if (parameter is BindingGroup)
            {
                BindingGroup bindGroup = parameter as BindingGroup;
                if (bindGroup.CommitEdit())
                {
                    bindGroup.BeginEdit();
                    ReloadInfo();
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить измененные значения
        /// </summary>
        public bool CanSaveValidation(object parameter = null)
        {
            return true;
        }
        #endregion // SaveValidationCommand

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
        #endregion // ErrorValidationCommand
        #endregion Команды
    }
}