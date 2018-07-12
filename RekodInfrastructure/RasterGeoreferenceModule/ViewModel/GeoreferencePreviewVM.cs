using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using System.Collections.ObjectModel;
using DeepZoom.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Data;
using System.Windows;
using DeepZoom.TestOverview.View;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Rekod.RasterGeoreferenceModule.Model;
using System.Reflection;
using Rekod.Repository.SettingsDB;
using AxmvMapLib;
using mvMapLib;
using RasM = Rekod.DataAccess.SourceRastr.Model;
using RasVM = Rekod.DataAccess.SourceRastr.ViewModel;

namespace Rekod.RasterGeoreferenceModule.ViewModel
{
    public class GeoreferencePreviewVM : WindowViewModelBase_VM
    {
        #region Поля
        private MultiScaleImage _multiScaleImage;
        private GDALOverviewsTileSource _tileSource;
        private Boolean _pointsAddMode;
        private double _topOffset = 0;
        private double _leftOffset = 0;
        private double _width;
        private double _height;
        private double _scale;
        private AxMapLIb _axMapLib;
        private bool _getCoordsFromMap = false;
        private int _lastWinX;
        private int _lastWinY;
        private int? _clickedWinX;
        private int? _clickedWinY;
        #endregion Поля

        #region Конструкторы
        public GeoreferencePreviewVM(MultiScaleImage multiScaleImage)
        {
            _multiScaleImage = multiScaleImage;
            _multiScaleImage.PropertyChanged += _multiScaleImage_PropertyChanged;

            Binding myBinding = new Binding("TileSource");
            myBinding.Source = this;
            _axMapLib = Program.mainFrm1.axMapLIb1;
            _multiScaleImage.SetBinding(MultiScaleImage.SourceProperty, myBinding);
            _multiScaleImage.ImagePointClicked += MultiScaleImage_ImagePointClicked;

            _axMapLib.MouseMoveEvent += AxMapLIb1_MouseMoveEvent;
            _axMapLib.MouseDownEvent += AxMapLIb1_MouseDownEvent;
            long mapSrid = Convert.ToInt64(_axMapLib.SRID);
            MapSridDef = 
                    (from Spatial refSys in Program.SettingsDB.Spatials where refSys.Srid == mapSrid select refSys.ProjText).FirstOrDefault();
            Title = Rekod.Properties.Resources.RstGrf_Title; 
        }

        void _multiScaleImage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
 
            }
        }
        #endregion Конструкторы

        #region Коллекции
        
        #endregion Коллекции

        #region Свойства
        public bool GetCoordsFromMap
        {
            get { return _getCoordsFromMap; }
            set 
            {
                if (value)
                {
                    _axMapLib.SetCursor(mvMapLib.Cursors.mlSelect);
                }
                OnPropertyChanged(ref _getCoordsFromMap, value, () => this.GetCoordsFromMap); 
            }
        }
        public MultiScaleImage MultiScaleImage
        {
            get { return _multiScaleImage; }
        }
        public GDALOverviewsTileSource TileSource
        {
            get { return _tileSource; }
            private set { OnPropertyChanged(ref _tileSource, value, () => this.TileSource); }
        }
        public Boolean PointsAddMode
        {
            get { return _pointsAddMode; }
            set { OnPropertyChanged(ref _pointsAddMode, value, () => this.PointsAddMode); }
        }
        public Double TopOffset
        {
            get { return _topOffset; }
            private set { OnPropertyChanged(ref _topOffset, value, () => this.TopOffset); }
        }
        public Double LeftOffset
        {
            get { return _leftOffset; }
            private set { OnPropertyChanged(ref _leftOffset, value, () => this.LeftOffset); }
        }
        public Double Width
        {
            get { return _width; }
            private set { OnPropertyChanged(ref _width, value, () => this.Width); }
        }
        public Double Height
        {
            get { return _height; }
            private set { OnPropertyChanged(ref _height, value, () => this.Height); }
        }
        public double Scale
        {
            get { return _scale; }
            private set { OnPropertyChanged(ref _scale, value, () => this.Scale); }
        }
        public String MapSridDef
        {
            get;
            private set;
        }
        #endregion Свойства

        #region Методы
        private void OpenWindow(UserControl userControl, int width, int height)
        {
            Window window = new Window() { Content = userControl, Width = width, Height = height, WindowStartupLocation = WindowStartupLocation.CenterScreen};
            window.ShowDialog();
        }
        #endregion Методы

        #region Команды
        #region OpenRastrCommand
        private ICommand _openRastrCommand;
        /// <summary>
        /// Команда для открытия нового растра
        /// </summary>
        public ICommand OpenRastrCommand
        {
            get { return _openRastrCommand ?? (_openRastrCommand = new RelayCommand(this.OpenRastr, this.CanOpenRastr)); }
        }
        /// <summary>
        /// Открытие растра
        /// </summary>
        public void OpenRastr(object parameter = null)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Rekod.Properties.Resources.LocAllSupportedFormats +
                   "|*.jpg;*.tif;*.tiff;*.bmp;*.png;|" + "JPEG|*.jpg|Tiff|*.tif;*.tiff|BMP|*.bmp|PNG|*.png";
            if (ofd.ShowDialog() == true)
            {
                cti.ThreadProgress.ShowWait();
                try
                {
                    GDALOverviewsM gdalOverviewsM = new GDALOverviewsM(ofd.FileName);
                    if (gdalOverviewsM.PrepareOverviews())
                    {
                        TileSource = new GDALOverviewsTileSource(gdalOverviewsM, 256);
                        Title = String.Format("{0} - {1}", Rekod.Properties.Resources.RstGrf_Title, Path.GetFileName(ofd.FileName));
                    }
                }
                catch (Exception ex)
                {
                    cti.ThreadProgress.Close();
                    MessageBox.Show(ex.Message);
                }
                cti.ThreadProgress.Close();
            }
        }
        /// <summary>
        /// Можно ли открыть растр
        /// </summary>
        public bool CanOpenRastr(object parameter = null)
        {
            return true;
        }
        #endregion // OpenRastrCommand

        #region StartGeolinkCommand
        private ICommand _startGeolinkCommand;
        /// <summary>
        /// Команда для начала геопривязки
        /// </summary>
        public ICommand StartGeolinkCommand
        {
            get { return _startGeolinkCommand ?? (_startGeolinkCommand = new RelayCommand(this.Geolink, this.CanGeolink)); }
        }
        /// <summary>
        /// Начало геопривязки
        /// </summary>
        public void Geolink(object parameter = null)
        {
            // 1: gdal_translate.exe -of GTiff -gcp 136 172 49 -11.3333 -gcp 3363 187 49.5 -11.3333 -gcp 56594 208 50 -11.3333 source.tif output.tif
            if(CanGeolink())
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Tiff|*.tif;*.tiff";
                if (sfd.ShowDialog() == true)
                {
                    bool resFileCreated = false;
                    String resFilePath = null;
                    // Создаем новый файл с привязкой
                    try
                    {
                        GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
                        Process gdalTranslate = new Process();
                        String exeDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
                        gdalTranslate.StartInfo.FileName = exeDirectory + @"\gdal\bin\gdal_translate.exe";
                        String outputFilePath = System.IO.Path.GetTempFileName();
                        resFilePath = sfd.FileName;

                        String sridDef =
                            (from Spatial refSys in Program.SettingsDB.Spatials where refSys.Srid == gdalOverviewsTileSource.GDALOverviewsM.CurrentSrid select refSys.ProjText).FirstOrDefault();
                        if (String.IsNullOrEmpty(sridDef))
                        {
                            throw new Exception(Rekod.Properties.Resources.RstGrf_SridShouldNotBeEmpty);
                        }

                        String pointsArguments = String.Join(" ", (from LinkedPointM lp
                                                                       in gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints
                                                                   where lp.IsActive
                                                                   select
                                                                       String.Format("-gcp {0} {1} {2} {3}", lp.RastrX, lp.RastrY, lp.MapX, lp.MapY)).ToArray());
                        gdalTranslate.StartInfo.Arguments =
                            String.Format(" -of GTiff -a_srs \"{0}\" {1} \"{2}\" \"{3}\"",
                                    sridDef,
                                    pointsArguments,
                                    gdalOverviewsTileSource.GDALOverviewsM.FileName,
                                    outputFilePath);
                        gdalTranslate.Start();
                        gdalTranslate.WaitForExit();

                        // 2: gdalwarp.exe -s_srs "+proj=longlat +ellps=krass +datum=Pulkovo_1942" -t_srs "+proj=longlat +ellps=krass +datum=Pulkovo_1942" output.tif result.tif
                        Process gdalWarp = new Process();
                        gdalWarp.StartInfo.FileName = exeDirectory + @"\gdal\bin\gdalwarp.exe";
                        gdalWarp.StartInfo.Arguments =
                            String.Format(" -multi -co \"TFW=YES\" \"{0}\" \"{1}\"", outputFilePath, resFilePath);
                        gdalWarp.Start();
                        gdalWarp.WaitForExit();
                        resFileCreated = true;
                        if (File.Exists(outputFilePath))
                        {
                            File.Delete(outputFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                    // Если новый файл с привязкой создан - создаем новый растровый слой для него
                    if (resFileCreated)
                    {
                        try
                        {
                            RasVM.RastrDataRepositoryVM rastrRepo = Program.TablesManager.RastrRepository as RasVM.RastrDataRepositoryVM;
                            RasM.RastTableBaseM rastrTable = rastrRepo.AddLayer(resFilePath);
                            rastrTable.ConnectType = RasM.EConnectType.Gdal;
                            rastrTable.BuildPyramids = true;
                            rastrRepo.SaveRastrLayers();
                            MessageBox.Show(String.Format(Rekod.Properties.Resources.RstGrf_LayerCreated, rastrTable.Text));
                        }
                        catch (Exception ex)
                        {
                            Program.TablesManager.RastrRepository.ReloadInfo();
                            MessageBox.Show(String.Format(Rekod.Properties.Resources.RstGrf_LayerNotCreatedGrfFile, ex.Message));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли начать геопривязку
        /// </summary>
        public bool CanGeolink(object parameter = null)
        {
            bool result = false;
            GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
            if(gdalOverviewsTileSource != null)
            {
                int activePointsCount = gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints.Count(p => p.IsActive);
                if (activePointsCount >= 4)
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion // StartGeolinkCommand

        #region SaveControlPointsCommand
        private ICommand _saveControlPointsCommand;
        /// <summary>
        /// Команда для сохранения контрольных точек
        /// </summary>
        public ICommand SaveControlPointsCommand
        {
            get { return _saveControlPointsCommand ?? (_saveControlPointsCommand = new RelayCommand(this.SaveControlPoints, this.CanSaveControlPoints)); }
        }
        /// <summary>
        /// Сохранение контрольных точек
        /// </summary>
        public void SaveControlPoints(object parameter = null)
        {
            if (CanSaveControlPoints())
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Points|*.points";
                if (sfd.ShowDialog() == true)
                {
                    GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<LinkedPointM>));
                    TextWriter fs = new StreamWriter(sfd.FileName);
                    serializer.Serialize(fs, gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints);
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить контрольные точки
        /// </summary>
        public bool CanSaveControlPoints(object parameter = null)
        {
            GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
            return (gdalOverviewsTileSource != null && gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints.Count > 0);
        }
        #endregion // SaveControlPointsCommand

        #region LoadControlPointsCommand
        private ICommand _loadControlPointsCommand;
        /// <summary>
        /// Команда для загрузки контрольных точек
        /// </summary>
        public ICommand LoadControlPointsCommand
        {
            get { return _loadControlPointsCommand ?? (_loadControlPointsCommand = new RelayCommand(this.LoadControlPoints, this.CanLoadControlPoints)); }
        }
        /// <summary>
        /// Загрузка контрольных точек
        /// </summary>
        public void LoadControlPoints(object parameter = null)
        {
            if (CanLoadControlPoints())
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Points|*.points";
                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
                        XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<LinkedPointM>));
                        TextReader fs = new StreamReader(ofd.FileName);
                        gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints.Clear();
                        foreach (var pt in (ObservableCollection<LinkedPointM>)serializer.Deserialize(fs))
                        {
                            gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints.Add(pt);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли загрузить контрольные точки
        /// </summary>
        public bool CanLoadControlPoints(object parameter = null)
        {
            return (TileSource as GDALOverviewsTileSource != null);
        }
        #endregion // LoadControlPointsCommand
        #endregion Команды

        #region Обработчики
        int _pointNum = 1; 
        void MultiScaleImage_ImagePointClicked(object sender, PointEventArgs e)
        {
            if (PointsAddMode)
            {
                GDALOverviewsTileSource gdalOverviewsTileSource = TileSource as GDALOverviewsTileSource;
                int imageX = Convert.ToInt32(e.ImagePoint.X);
                int imageY = Convert.ToInt32(e.ImagePoint.Y);
                if (0 <= imageX && 
                        imageX <= gdalOverviewsTileSource.GDALOverviewsM.ImageWidth && 
                    0 <= imageY &&
                        imageY <= gdalOverviewsTileSource.GDALOverviewsM.ImageHeight)
                {
                    var newPoint = new LinkedPointM(String.Format(Rekod.Properties.Resources.RstGrf_PointNum, _pointNum++), imageX, imageY, 0, 0);
                    gdalOverviewsTileSource.GDALOverviewsM.LinkedPoints.Add(newPoint);
                    PointsAddMode = false;
                }
            }
        }
        void AxMapLIb1_MouseDownEvent(object sender, AxmvMapLib.IMapLIbEvents_MouseDownEvent e)
        {
            if (GetCoordsFromMap)
            {
                GetCoordsFromMap = false;
                _clickedWinX = _lastWinX;
                _clickedWinY = _lastWinY;
                LinkedPointM pointToChange = (from LinkedPointM lp in TileSource.GDALOverviewsM.LinkedPoints where lp.IsSelected == true select lp).FirstOrDefault();
                if (pointToChange != null)
                {
                    var p = new mvPointWindow();
                    p.x = (int)_clickedWinX;
                    p.y = (int)_clickedWinY;
                    var pw = _axMapLib.win2Global(p);
                    pointToChange.MapX = pw.x;
                    pointToChange.MapY = pw.y;

                    if (MapSridDef != TileSource.GDALOverviewsM.CurrentSridDef)
                    {
                        List<LinkedPointM> pointsToChange = new List<LinkedPointM>() { pointToChange };
                        TileSource.GDALOverviewsM.ChangeMapSridLinkedPoints(pointsToChange, Convert.ToInt32(_axMapLib.SRID), TileSource.GDALOverviewsM.CurrentSrid);
                    }
                }
            }
        }
        void AxMapLIb1_MouseMoveEvent(object sender, AxmvMapLib.IMapLIbEvents_MouseMoveEvent e)
        {
            _lastWinX = e.winx;
            _lastWinY = e.winy;
        }
        #endregion Обработчики
    }
}