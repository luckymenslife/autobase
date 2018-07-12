using Rekod.Behaviors;
using Rekod.DataAccess.SourceRastr.View.Behaviors;
using Rekod.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.DataAccess.SourceRastr.Model
{
    public class RastTableBaseM : AbsM.TableBaseM
    {
        #region Статические члены
        public static void SetIsNewTable(RastTableBaseM layer, bool isnew)
        {
            layer._isNewTable = isnew;
        }

        #endregion // Статические члены

        #region Поля
        private bool _isExternal;
        private string _filePath;
        private bool _useBounds;
        private int _minScale;
        private int _maxScale;
        private String _description;
        private bool _isDefault;
        private bool _buildPyramids;
        private EConnectType _connectType;
        private object _content;
        #endregion // Поля

        #region Конструкторы
        public RastTableBaseM(AbsM.IDataRepositoryM source, AbsM.ETableType type)
            : base(source, Convert.ToInt32(Program.srid), type)
        {
        }
        public RastTableBaseM(AbsM.IDataRepositoryM source, int id, string filePath, AbsM.ETableType type)
            : base(source, id, Convert.ToInt32(Program.srid), type)
        {
            FilePath = filePath;
            ConnectType = FileExtensionConnectTypeBehavior.GetConnectType(filePath);
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Путь к локальному файлу-источнику растрового слоя (Id)
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                OnPropertyChanged(ref _filePath, value, () => this.FilePath);
                OnPropertyChanged("IsExternal");
            }
        }
        public bool IsXML
        {
            get
            {
                if (_filePath != null)
                    return _filePath.ToLower().EndsWith(".xml")
                        || _filePath.ToLower().EndsWith(".gxml")
                        || _filePath.ToLower().EndsWith(".rwms")
                        || _filePath.ToLower().EndsWith(".rtwms")
                        || _filePath.ToLower().EndsWith(".rtms");
                else
                    return false;
            }
        }
        /// <summary>
        /// Подгружается растровый слой динамически через сервер или нет
        /// </summary>
        public bool IsExternal
        {
            get
            {
                if (IsXML)
                {
                    _isExternal = true;
                }
                return _isExternal;
            }
            set { OnPropertyChanged(ref _isExternal, value, () => this.IsExternal); }
        }
        /// <summary>
        /// Подгружается растровый слой и строят пирамиду
        /// </summary>
        public bool BuildPyramids
        {
            get { return _buildPyramids; }
            set { OnPropertyChanged(ref _buildPyramids, value, () => this.BuildPyramids); }
        }
        /// <summary>
        /// Получает или задает описание растрового слоя
        /// </summary>
        public String Description
        {
            get { return _description; }
            set { OnPropertyChanged(ref _description, value, () => this.Description); }
        }
        /// <summary>
        /// Использовать ли границы видимости для слоя
        /// </summary>
        public bool UseBounds
        {
            get { return _useBounds; }
            set
            {
                if (_useBounds != value)
                {
                    var rastrSource = _source as AbsVM.DataRepositoryVM;
                    var imLayer = rastrSource.MapViewer.getImageLayer(FilePath);
                    if (imLayer != null)
                    {
                        imLayer.usebounds = value;
                        rastrSource.MapViewer.mapRepaint();
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
                if (_minScale != value)
                {
                    var rastrSource = _source as AbsVM.DataRepositoryVM;
                    var imLayer = rastrSource.MapViewer.getImageLayer(FilePath);
                    if (imLayer != null)
                    {
                        imLayer.MinScale = (uint)value;
                        rastrSource.MapViewer.mapRepaint();
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
                if (_maxScale != value)
                {
                    AbsVM.DataRepositoryVM rastrSource = _source as AbsVM.DataRepositoryVM;
                    var imLayer = rastrSource.MapViewer.getImageLayer(FilePath);
                    if (imLayer != null)
                    {
                        imLayer.MaxScale = (uint)value;
                        rastrSource.MapViewer.mapRepaint();
                    }
                }
                OnPropertyChanged(ref _maxScale, value, () => this.MaxScale);
            }
        }
        public bool IsDefault
        {
            get { return _isDefault; }
            set { OnPropertyChanged(ref _isDefault, value, () => this.IsDefault); }
        }
        public override bool IsVisible
        {
            get
            {
                var imLayer = (Source as AbsVM.DataRepositoryVM).MapViewer.getImageLayer(FilePath);
                if (imLayer != null)
                {
                    _isVisible = imLayer.Visible;
                }
                return _isVisible;
            }
            set
            {
                ((AbsVM.DataRepositoryVM)Source).SetInvertVisible(this, value);
            }
        }
        public EConnectType ConnectType
        {
            get { return _connectType; }
            set { OnPropertyChanged(ref _connectType, value, () => this.ConnectType); }
        }

        public object Content
        {
            get { return _content; }
            set { OnPropertyChanged(ref  _content, value, () => this.Content); }
        }
        #endregion // Свойства

        #region Действия
        public Action<object> OpenFileAction
        {
            get
            {
                return param =>
                    {
                        var commEvtParam = param as CommandEventParameter;
                        var ofd = new System.Windows.Forms.OpenFileDialog();
                        var commParams = commEvtParam.CommandParameter as List<object>;
                        var FilePathBox = commParams[0] as TextBox;
                        var RastrNameBox = commParams[1] as TextBox;

                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (System.IO.File.Exists(ofd.FileName))
                            {
                                if (ofd.FileName.StartsWith(System.Windows.Forms.Application.StartupPath))
                                {
                                    FilePathBox.Text = ofd.FileName.Replace(System.Windows.Forms.Application.StartupPath, ".");
                                }
                                else
                                {
                                    FilePathBox.Text = ofd.FileName;
                                }
                                RastrNameBox.Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                            }
                        }
                    };

            }
        }
        #endregion Действия

    }
    /// <summary>
    /// Способ подключения
    /// </summary>
    [TypeResource("RasM.EConnectType")]
    public enum EConnectType
    {
        Standard = 0,
        Gdal = 1
    }
}