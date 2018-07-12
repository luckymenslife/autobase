using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.DataAccess.SourceVMP.Model
{
    public class VMPTableBaseModel : AbsM.TableBaseM
    {
        #region Поля
        private bool _useBounds;
        private int _minScale;
        private int _maxScale;
        #endregion // Поля

        #region Конструкторы
        public VMPTableBaseModel(AbsM.IDataRepositoryM source, AbsM.ETableType type) :
            base(source, Convert.ToInt32(((AbsVM.DataRepositoryVM)source).MapViewer.SRID), type)
        { }
        public VMPTableBaseModel(AbsM.IDataRepositoryM source, String id, AbsM.ETableType type)
            : base(source, id, Convert.ToInt32(((AbsVM.DataRepositoryVM)source).MapViewer.SRID), type)
        {
        }
        #endregion // Конструкторы

        #region Свойства
        public bool UseBounds
        {
            get { return _useBounds; }
            set
            {
                if (value != _useBounds)
                {
                    AbsVM.DataRepositoryVM vmpSource = _source as AbsVM.DataRepositoryVM;
                    mvMapLib.mvLayer mvLayer = vmpSource.MapViewer.getLayer(Name);
                    if (mvLayer != null)
                    {
                        mvLayer.usebounds = value;
                        vmpSource.MapViewer.mapRepaint();
                        vmpSource.MapViewer.Update();
                    }
                }
                OnPropertyChanged(ref _useBounds, value, () => this.UseBounds);
            }
        }
        /// <summary>
        /// Если использовать границы видимости, минимальный масштаб при котором слой отображается
        /// </summary>
        public int MinScale
        {
            get { return _minScale; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value != _minScale)
                {
                    AbsVM.DataRepositoryVM vmpSource = _source as AbsVM.DataRepositoryVM;
                    mvMapLib.mvLayer mvLayer = vmpSource.MapViewer.getLayer(Name);
                    if (mvLayer != null)
                    {
                        if (mvLayer.MaxScale >= value)
                        {
                            mvLayer.MinScale = (uint)value;
                            vmpSource.MapViewer.mapRepaint();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(Rekod.Properties.Resources.LocSaveScaleError, Rekod.Properties.Resources.error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                OnPropertyChanged(ref _minScale, value, () => this.MinScale);
            }
        }
        /// <summary>
        /// Если использовать границы видимости, максимальный масштаб при котором слой отображается
        /// </summary>
        public int MaxScale
        {
            get { return _maxScale; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value != _maxScale)
                {
                    AbsVM.DataRepositoryVM vmpSource = _source as AbsVM.DataRepositoryVM;
                    mvMapLib.mvLayer mvLayer = vmpSource.MapViewer.getLayer(Name);
                    if (mvLayer != null)
                    {
                        if (mvLayer.MinScale <= value)
                        {
                            mvLayer.MaxScale = (uint)value;
                            //TODO: Layer.NAME = null делается тут;
                            vmpSource.MapViewer.mapRepaint();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(Rekod.Properties.Resources.LocSaveScaleError, Rekod.Properties.Resources.error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                OnPropertyChanged(ref _maxScale, value, () => this.MaxScale);
            }
        }
        #endregion // Свойства

    }
}