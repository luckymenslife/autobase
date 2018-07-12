using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.PrintModule.RenderComponents;
using Rekod.PrintModule.LayersObjectModel;
using System.Windows;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource.ViewModel;
using System.IO;
using Rekod.PrintModule.Service;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using cti;
using Microsoft.Win32;
using Rekod.Controllers;
using Rekod.Services;
using Rekod.Behaviors;
using Rekod.PrintModule.View;

namespace Rekod.PrintModule.ViewModel
{
    public class PreviewWindowVM: ViewModelBase
    {
        #region Поля
        private AxmvMapLib.AxMapLIb _axMapLib1;
        private DrawingCanvas _drawingSurface;
        private List<PrintItem> _printItems = new List<PrintItem>(); 

        private int _marginLeft = 5;
        private int _marginTop = 5;
        private int _marginRight = 5;
        private int _marginBottom = 5;
        private string _paperName = "A4";
        private int _paperHeight = 297;
        private int _paperWidth = 210;
        private bool _landScape = false;
        private int _horizontalCount = 1;
        private int _verticalCount = 1;
        private PrintItem _currentPrintItem;
        private int _currentPrintItemNum = 1; 

        private Point _initialProjWPos;
        private Double _initialProjZoom;
        private RastrLayer _rastrLayer;
        private RastrSource _rastrSource;
        private RastrLayerObject _rasterObject;
        private PageSettings _dftPageSettings;
        private String _prevSaveSelectedPrefix;
        private String _prevSaveSelectedFolder; 
        #endregion Поля
        
        #region Конструкторы
        public PreviewWindowVM(AxmvMapLib.AxMapLIb axMapLib1, DrawingCanvas drawingCanvas)
        {
            _dftPageSettings = new PageSettings();
            PaperSize size = new PaperSize();
            size.RawKind = (int)PaperKind.A4;
            size.Height = 1169;
            size.Width = 827;
            _dftPageSettings.PaperSize = size;
            _dftPageSettings.PrinterResolution = new PrinterResolution();
            _dftPageSettings.PrinterResolution.Kind = PrinterResolutionKind.Medium;
            _dftPageSettings.Landscape = true;
            _dftPageSettings.Color = true;
            _dftPageSettings.Margins = new Margins(0, 0, 0, 0);

            LandScape = _dftPageSettings.Landscape;
            PaperName = _dftPageSettings.PaperSize.Kind.ToString();
            int sizeHeight = Convert.ToInt32(_dftPageSettings.PaperSize.Height / 3.937);
            int sizeWidth = Convert.ToInt32(_dftPageSettings.PaperSize.Width / 3.937);
            PaperWidth = LandScape ? sizeHeight : sizeWidth;
            PaperHeight = LandScape ? sizeWidth : sizeHeight;

            _axMapLib1 = axMapLib1;
            _drawingSurface = drawingCanvas;

            _rastrSource = new RastrSource(_axMapLib1, _drawingSurface.Dpi);
            mvMapLib.mvBbox mapExtent = _axMapLib1.MapExtent;
            RastrLayer rastrLayer = new RastrLayer(_drawingSurface);
            _rastrLayer = rastrLayer;
            _rasterObject = new RastrLayerObject(rastrLayer, _rastrSource);

            _rasterObject.Nodes.Add(new Node() { GlobalPoint = new Point(_rastrSource.ImageExtent.a.x, _rastrSource.ImageExtent.b.y) });
            _rasterObject.Nodes.Add(new Node() { GlobalPoint = new Point(_rastrSource.ImageExtent.b.x, _rastrSource.ImageExtent.a.y) });
            rastrLayer.LayerObjects.Add(_rasterObject);

            _drawingSurface.Layers.Add(rastrLayer);
            _drawingSurface.AddVisual(_rasterObject);
            _drawingSurface.ProjectionWPos = new Point(_rastrSource.ImageExtent.a.x, _rastrSource.ImageExtent.b.y);
            _drawingSurface.RecalculateWindowCoordinates();

            double rastrWidthPx = _rastrSource.ImageWidth;
            double rastrHeightPx = _rastrSource.ImageHeight;
            double xmin = _rastrSource.ImageExtent.a.x;
            double ymin = _rastrSource.ImageExtent.b.y;
            double xmax = _rastrSource.ImageExtent.b.x;
            double ymax = _rastrSource.ImageExtent.a.y;
            double _mainObjectWidth = _drawingSurface.GetLength(new Point(xmin, ymin), new Point(xmax, ymin), Convert.ToInt32(_axMapLib1.SRID));
            double _mainObjectHeight =_drawingSurface.GetLength(new Point(xmax, ymax), new Point(xmax, ymin), Convert.ToInt32(_axMapLib1.SRID));
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            double scaleRateX = _mainObjectWidth / (rastrWidthPx * 0.0254 / gr.DpiX);
            double scaleRateY = _mainObjectHeight / (rastrHeightPx * 0.0254 / gr.DpiY);
            if (scaleRateX > scaleRateY)
            {
                _drawingSurface.ProjectionZoom = _mainObjectWidth / rastrWidthPx;
            }
            else
            {
                _drawingSurface.ProjectionZoom = _mainObjectHeight / rastrHeightPx;
            }
            _drawingSurface.RecalculateWindowCoordinates(); 
            _drawingSurface.Scale = Convert.ToInt32(_axMapLib1.ScaleZoom);
            _drawingSurface.SetProjZoomScaleRel(_drawingSurface.ProjectionZoom / Convert.ToDouble(_axMapLib1.ScaleZoom));
            _initialProjWPos = _drawingSurface.ProjectionWPos;
            _initialProjZoom = _drawingSurface.ProjectionZoom;

#if DEBUG
            _horizontalCount = 3;
            _verticalCount = 3;
#endif

            _drawingSurface.ServiceLayer.SetPrintSurface(_horizontalCount, _verticalCount, PageWidth, PageHeight);
            _drawingSurface.Unloaded += new RoutedEventHandler(_drawingSurface_Unloaded);
            _drawingSurface.IsVisibleChanged += _drawingSurface_IsVisibleChanged;
        }
        #endregion Конструкторы

        #region Свойства
        public int MarginTop
        {
            get { return _marginTop; }
            set 
            {
                OnPropertyChanged(ref _marginTop, value, () => this.MarginTop);
                OnPropertyChanged("PageHeight"); 
            }
        }
        public int MarginLeft
        {
            get { return _marginLeft; }
            set
            {
                OnPropertyChanged(ref _marginLeft, value, () => this.MarginLeft);
                OnPropertyChanged("PageWidth"); 
            }
        }
        public int MarginRight
        {
            get { return _marginRight; }
            set 
            {
                OnPropertyChanged(ref _marginRight, value, () => this.MarginRight);
                OnPropertyChanged("PageWidth"); 
            }
        }
        public int MarginBottom
        {
            get { return _marginBottom; }
            set
            {
                OnPropertyChanged(ref _marginBottom, value, () => this.MarginBottom);
                OnPropertyChanged("PageHeight"); 
            }
        }
        public string PaperName
        {
            get { return _paperName; }
            private set { OnPropertyChanged(ref _paperName, value, () => this.PaperName); }
        }
        public int PaperWidth
        {
            get { return _paperWidth; }
            private set 
            {
                OnPropertyChanged(ref _paperWidth, value, () => this.PaperWidth);
                OnPropertyChanged("PageWidth"); 
            }
        }
        public int PaperHeight
        {
            get { return _paperHeight; }
            private set 
            {
                OnPropertyChanged(ref _paperHeight, value, () => this.PaperHeight);
                OnPropertyChanged("PageHeight");
            }
        }
        public String Orientation
        {
            get 
            {
                return _landScape ? Rekod.Properties.Resources.PrintV_Landscape : Rekod.Properties.Resources.PrintV_Portrait; 
            }
        }
        public bool LandScape
        {
            get { return _landScape; }
            private set 
            {
                OnPropertyChanged(ref _landScape, value, () => this.LandScape);
                OnPropertyChanged("Orientation"); 
            }
        }
        public int PageWidth
        {
            get { return PaperWidth - MarginLeft - MarginRight; }
        }
        public int PageHeight
        {
            get { return PaperHeight - MarginTop - MarginBottom; }
        }
        public int HorizontalCount
        {
            get { return _horizontalCount; }
            set 
            {
                if (_horizontalCount == value)
                {
                    return; 
                }
                _horizontalCount = value;
                OnPropertyChanged("HorizontalCount");
                _drawingSurface.ServiceLayer.SetPrintSurface(_horizontalCount, _verticalCount, PageWidth, PageHeight); 
            }
        }
        public int VerticalCount
        {
            get { return _verticalCount; }
            set 
            {
                if (_verticalCount == value)
                {
                    return;
                }
                _verticalCount = value;
                OnPropertyChanged("VerticalCount");
                _drawingSurface.ServiceLayer.SetPrintSurface(_horizontalCount, _verticalCount, PageWidth, PageHeight); 
            }
        }
        public RastrLayerObject RasterObject
        {
            get { return _rasterObject; }
        }
        public AxmvMapLib.AxMapLIb AxMapLib1
        {
            get { return _axMapLib1; }
        }
        #endregion Свойства

        #region Команды
        #region InscribeCommand
        private ICommand _inscribeCommand;
        /// <summary>
        /// Команда для вписывания растра в область печати
        /// </summary>
        public ICommand InscribeCommand
        {
            get { return _inscribeCommand ?? (_inscribeCommand = new RelayCommand(this.Inscribe, this.CanInscribe)); }
        }
        /// <summary>
        /// Вписывание растра в область печати
        /// </summary>
        public void Inscribe(object parameter = null)
        {
            _drawingSurface.ProjectionWPos = _rastrLayer.LayerObjects[0].Nodes[0].GlobalPoint;
            _drawingSurface.RecalculateWindowCoordinates(); 
        }
        /// <summary>
        /// Можно ли вписать растр в область печати
        /// </summary>
        public bool CanInscribe(object parameter = null)
        {
            return true;
        }
        #endregion // InscribeCommand

        #region SettingsCommand
        private ICommand _settingsCommand;
        /// <summary>
        /// Команда для вызова окна настроек
        /// </summary>
        public ICommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(this.Settings, this.CanSettings)); }
        }
        /// <summary>
        /// Вызов окна настроек
        /// </summary>
        public void Settings(object parameter = null)
        {
            using (System.Windows.Forms.PageSetupDialog pageSetupDlg = new System.Windows.Forms.PageSetupDialog())
            {
                double correction = 10;
                _dftPageSettings.Margins =
                    new System.Drawing.Printing.Margins(
                        (int)(_marginLeft * correction),
                        (int)(_marginRight * correction),
                        (int)(_marginTop * correction),
                        (int)(_marginBottom * correction));
                _dftPageSettings.Landscape = _landScape;
                pageSetupDlg.PageSettings = _dftPageSettings;

                if (_dftPageSettings.PaperSize.Kind == PaperKind.Custom)
                {
                    PaperSize size = new PaperSize();
                    size.RawKind = (int)PaperKind.A4;
                    size.Height = 1169;
                    size.Width = 827;
                    _dftPageSettings.PaperSize = size;
                }
                if (pageSetupDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _dftPageSettings = pageSetupDlg.PageSettings;
                    MarginLeft = Convert.ToInt32(_dftPageSettings.Margins.Left / 3.9370);
                    MarginRight = Convert.ToInt32(_dftPageSettings.Margins.Right / 3.9370);
                    MarginTop = Convert.ToInt32(_dftPageSettings.Margins.Top / 3.9370);
                    MarginBottom = Convert.ToInt32(_dftPageSettings.Margins.Bottom / 3.9370);

                    //MarginLeft      = MarginLeft    < 5 ? 5 : MarginLeft;
                    //MarginRight     = MarginRight   < 5 ? 5 : MarginRight;
                    //MarginTop       = MarginTop     < 5 ? 5 : MarginTop;
                    //MarginBottom    = MarginBottom  < 5 ? 5 : MarginBottom;

                    LandScape = pageSetupDlg.PageSettings.Landscape;
                    PaperName = pageSetupDlg.PageSettings.PaperSize.PaperName;
                    int formatWidth = Convert.ToInt32(pageSetupDlg.PageSettings.PaperSize.Width / 3.9370);
                    int formatHeight = Convert.ToInt32(pageSetupDlg.PageSettings.PaperSize.Height / 3.9370);
                    PaperWidth = LandScape ? formatHeight : formatWidth; 
                    PaperHeight = LandScape ? formatWidth : formatHeight;
                    _drawingSurface.ServiceLayer.SetPrintSurface(HorizontalCount, VerticalCount, PageWidth, PageHeight); 
                }
            }
        }
        /// <summary>
        /// Можно ли вызвать окно настроек
        /// </summary>
        public bool CanSettings(object parameter = null)
        {
            return true;
        }
        #endregion // SettingsCommand 

        #region PrintCommand
        //http://www.codeproject.com/Articles/339416/Printing-large-WPF-UserControls
        private ICommand _printCommand;
        /// <summary>
        /// Команда для печати
        /// </summary>
        public ICommand PrintCommand
        {
            get { return _printCommand ?? (_printCommand = new RelayCommand(this.Print, this.CanPrint)); }
        }
        /// <summary>
        /// Печать
        /// </summary>
        public void Print(object parameter = null)
        {
            PrintItems(_drawingSurface.ServiceLayer.LayerObjects);
        }
        /// <summary>
        /// Можно ли печатать
        /// </summary>
        public bool CanPrint(object parameter = null)
        {
            return !_rasterObject.NeedsUpdate;
        }
        #endregion // PrintCommand

        #region PrintAreaTableCancelValidationCommand
        private ICommand _printAreaTableCancelValidationCommand;
        /// <summary>
        /// Команда для отмены валидации количества страниц
        /// </summary>
        public ICommand PrintAreaTableCancelValidationCommand
        {
            get { return _printAreaTableCancelValidationCommand ?? (_printAreaTableCancelValidationCommand = new RelayCommand(this.PrintAreaTableCancelValidation, this.CanPrintAreaTableCancelValidation)); }
        }
        /// <summary>
        /// Отмена валидации количества страниц
        /// </summary>
        public void PrintAreaTableCancelValidation(object parameter = null)
        {
            BindingGroup bindGroup = parameter as BindingGroup;
            if (bindGroup != null)
            {
                bindGroup.CancelEdit();
                bindGroup.BeginEdit(); 
            }
        }
        /// <summary>
        /// Можно ли отменить валидацию
        /// </summary>
        public bool CanPrintAreaTableCancelValidation(object parameter = null)
        {
            return true;
        }
        #endregion // PrintAreaTableCancelValidationCommand

        #region PrintAreaTableCommitlValidationCommand
        private ICommand _printAreaTableCommitValidationCommand;
        /// <summary>
        /// Команда для применения валидации количества страниц
        /// </summary>
        public ICommand PrintAreaTableCommitValidationCommand
        {
            get { return _printAreaTableCommitValidationCommand ?? (_printAreaTableCommitValidationCommand = new RelayCommand(this.PrintAreaTableCommitValidation, this.CanPrintAreaTableCommitValidation)); }
        }
        /// <summary>
        /// Применения валидации количества страниц
        /// </summary>
        public void PrintAreaTableCommitValidation(object parameter = null)
        {
            BindingGroup bindGroup = parameter as BindingGroup;
            if (bindGroup != null)
            {
                if (bindGroup.CommitEdit())
                {
                    bindGroup.BeginEdit();
                }
            }
        }
        /// <summary>
        /// Можно ли применить валидацию
        /// </summary>
        public bool CanPrintAreaTableCommitValidation(object parameter = null)
        {
            return true;
        }
        #endregion // PrintAreaTableCommitValidationCommand

        #region RerenderCommand
        private ICommand _rerenderCommand;
        /// <summary>
        /// Команда для перерисовки
        /// </summary>
        public ICommand RerenderCommand
        {
            get { return _rerenderCommand ?? (_rerenderCommand = new RelayCommand(this.Rerender, this.CanRerender)); }
        }
        /// <summary>
        /// Перерисовка
        /// </summary>
        public void Rerender(object parameter = null)
        {
            _drawingSurface.Render();
        }
        /// <summary>
        /// Можно ли выполнить перерисовку
        /// </summary>
        public bool CanRerender(object parameter = null)
        {
            return true;
        }
        #endregion // RerenderCommand

        #region SaveCommand
        private ICommand _saveCommand;
        /// <summary>
        /// Команда для сохранения
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        /// <summary>
        /// Сохранение видимой области в файл
        /// </summary>
        public void Save(object parameter = null)
        {
            RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.Filter = "Bitmap|*.bmp"; 
            if (sfd.ShowDialog() == true)
            {
                maplib_saveByteArray(
                                0,
                                0,
                                rastrObject.RastrSource.SrcWidth,
                                rastrObject.RastrSource.SrcHeight,
                                rastrObject.RastrSource.SrcWidth,
                                rastrObject.RastrSource.SrcHeight,
                                rastrObject.RastrSource.BmpByteArrayPointer,
                                sfd.FileName
                            );
            }
        }
        /// <summary>
        /// Можно ли сохранить видимую область в файл
        /// </summary>
        public bool CanSave(object parameter = null)
        {
            RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
            return rastrObject != null && rastrObject.RastrSource.BmpByteArrayPointer != IntPtr.Zero && !_rasterObject.NeedsUpdate;
        }
        #endregion // SaveCommand
        
        #region RefreshRasterCommand
        private ICommand _refreshRasterCommand;
        /// <summary>
        /// Команда для вызова обновления растра
        /// </summary>
        public ICommand RefreshRasterCommand
        {
            get { return _refreshRasterCommand ?? (_refreshRasterCommand = new RelayCommand(this.RefreshRaster, this.CanRefreshRaster)); }
        }
        /// <summary>
        /// Обновление растра
        /// </summary>
        public void RefreshRaster(object parameter = null)
        {
            _rasterObject.UpdateSource();
        }
        /// <summary>
        /// Можно ли обновить растр
        /// </summary>
        public bool CanRefreshRaster(object parameter = null)
        {
            return true; // _rasterObject.NeedsUpdate;
        }
        #endregion // RefreshRasterCommand

        #region PrintSelectedPagesCommand
        private ICommand _printSelectedPagesCommand;
        /// <summary>
        /// Команда для печати выделенных страниц
        /// </summary>
        public ICommand PrintSelectedPagesCommand
        {
            get { return _printSelectedPagesCommand ?? (_printSelectedPagesCommand = new RelayCommand(this.PrintSelectedPages, this.CanPrintSelectedPages)); }
        }
        /// <summary>
        /// Печать выделенных страниц
        /// </summary>
        public void PrintSelectedPages(object parameter = null)
        {
            var servObjects = (from LayerObjectBase lObj in _drawingSurface.ServiceLayer.LayerObjects where (lObj as ServiceLayerObject).IsSelected select lObj).ToList();
            PrintItems(servObjects);
        }
        /// <summary>
        /// Можно ли распечатать выделенные страницы
        /// </summary>
        public bool CanPrintSelectedPages(object parameter = null)
        {
            return !_rasterObject.NeedsUpdate &&
                (from ServiceLayerObject servObject in _drawingSurface.ServiceLayer.LayerObjects where servObject.IsSelected select servObject).Count() > 0;;
        }
        #endregion PrintSelectedPagesCommand

        #region SaveSelectedPagesCommand
        private ICommand _saveSelectedPagesCommand;
        /// <summary>
        /// Команда для сохранения выделенных страниц
        /// </summary>
        public ICommand SaveSelectedPagesCommand
        {
            get { return _saveSelectedPagesCommand ?? (_saveSelectedPagesCommand = new RelayCommand(this.SaveSelectedPages, this.CanSaveSelectedPages)); }
        }
        /// <summary>
        /// Сохранение выделенных страниц
        /// </summary>
        public void SaveSelectedPages(object parameter = null)
        {
            SaveSelectedPagesWindow savePages = new SaveSelectedPagesWindow();
            savePages.PrefixBox.Text = _prevSaveSelectedPrefix;
            savePages.FolderPathBox.Text = _prevSaveSelectedFolder; 
            SaveSelectedPagesWindowVM savePagesVM = new SaveSelectedPagesWindowVM(savePages);
            savePages.DataContext = savePagesVM;
            savePages.ShowDialog();

            if (savePagesVM.DialogResult == true)
            {
                _prevSaveSelectedPrefix = savePages.PrefixBox.Text;
                _prevSaveSelectedFolder = savePages.FolderPathBox.Text; 

                RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
                var servObjects = 
                    (from LayerObjectBase lObj in _drawingSurface.ServiceLayer.LayerObjects where (lObj as ServiceLayerObject).IsSelected select lObj).ToList();
                if (servObjects.Count() > 0)
                {
                    try
                    {
                        List<String> existingFiles = new List<string>();
                        for (int j = 1; j <= servObjects.Count; j++)
                        {
                            String filePath = String.Format("{0}\\{1}_{2}.bmp",
                                    savePages.FolderPathBox.Text,
                                    savePages.PrefixBox.Text.Trim(),
                                    j);
                            if (File.Exists(filePath))
                            {
                                existingFiles.Add(filePath); 
                            }
                        }

                        bool continueSaving = true;
                        if (existingFiles.Count > 0)
                        {
                            bool moreThanFifteen = existingFiles.Count > 15;
                            int showCount = moreThanFifteen ? 15 : existingFiles.Count;
                            StringBuilder showMessage = new StringBuilder(); 
                            showMessage.AppendLine("Следующие файлы уже существуют:");
                            for (int j = 0; j < showCount; j++)
                            {
                                showMessage.AppendLine(existingFiles[j]);
                            }
                            if (moreThanFifteen)
                            {
                                showMessage.AppendLine("...");
                            }
                            showMessage.AppendLine();
                            showMessage.AppendLine("Вы хотите их перезаписать?");

                            if (MessageBox.Show(
                                        showMessage.ToString(), 
                                        "Существующие файлы", 
                                        MessageBoxButton.YesNo, 
                                        MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                SaveSelectedPages();
                                return;
                            }
                        }

                        if (continueSaving)
                        {
                            ThreadProgress.ShowWait("Print Module");
                            int i = 1;
                            foreach (var serviceObject in servObjects)
                            {
                                Point rastrTopLeftW = rastrObject.Nodes[0].WinPoint;
                                Point rastrBottomRightW = rastrObject.Nodes[1].WinPoint;

                                Point servTopLeftW = serviceObject.Nodes[0].WinPoint;
                                Point servBottomRightW = serviceObject.Nodes[1].WinPoint;

                                double rastrWidthW = rastrBottomRightW.X - rastrTopLeftW.X;
                                double rastrHeightW = rastrBottomRightW.Y - rastrTopLeftW.Y;

                                Rect rastrRectW = new Rect(rastrTopLeftW, rastrBottomRightW);
                                Rect servRectW = new Rect(servTopLeftW, servBottomRightW);
                                Rect printAreaW = Rect.Intersect(rastrRectW, servRectW);

                                if (printAreaW != Rect.Empty)
                                {
                                    int topLeftX = (int)(((printAreaW.TopLeft.X - rastrTopLeftW.X) / (double)rastrWidthW) * rastrObject.RastrSource.SrcWidth);
                                    int topLeftY = (int)(((printAreaW.TopLeft.Y - rastrTopLeftW.Y) / (double)rastrHeightW) * rastrObject.RastrSource.SrcHeight);
                                    int bottomRightX = (int)(topLeftX + (printAreaW.Width / rastrWidthW) * rastrObject.RastrSource.SrcWidth);
                                    int bottomRightY = (int)(topLeftY + (printAreaW.Height / rastrHeightW) * rastrObject.RastrSource.SrcHeight);

                                    int marginLeft = (int)((printAreaW.TopLeft.X - servTopLeftW.X) / (servRectW.Width) * _drawingSurface.ServiceLayer.PageWidth);
                                    int marginTop = (int)((printAreaW.TopLeft.Y - servTopLeftW.Y) / (servRectW.Height) * _drawingSurface.ServiceLayer.PageHeight);
                                    int marginRight = (int)((servBottomRightW.X - printAreaW.BottomRight.X) / (servRectW.Width) * _drawingSurface.ServiceLayer.PageWidth);
                                    int marginBottom = (int)((servBottomRightW.Y - printAreaW.BottomRight.Y) / (servRectW.Height) * _drawingSurface.ServiceLayer.PageHeight);

                                    String filePath = String.Format("{0}\\{1}_{2}.bmp",
                                        savePages.FolderPathBox.Text,
                                        savePages.PrefixBox.Text.Trim(),
                                        i);
                                    Thickness printMargins = new Thickness(marginLeft, marginTop, marginRight, marginBottom);

                                    ThreadProgress.SetText(
                                        String.Format("Идет сохранение {0} из {1}", i, servObjects.Count));
                                    maplib_saveByteArray(
                                            topLeftX,
                                            topLeftY,
                                            bottomRightX,
                                            bottomRightY,
                                            rastrObject.RastrSource.SrcWidth,
                                            rastrObject.RastrSource.SrcHeight,
                                            rastrObject.RastrSource.BmpByteArrayPointer,
                                            filePath
                                            );
                                    i++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ThreadProgress.Close("Print Module");
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        ThreadProgress.Close("Print Module");
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить выделенные страницы
        /// </summary>
        public bool CanSaveSelectedPages(object parameter = null)
        {
            return (from ServiceLayerObject servObject in _drawingSurface.ServiceLayer.LayerObjects where servObject.IsSelected select servObject).Count() > 0;
        }
        #endregion SaveSelectedPagesCommand

        #region FitInSelectedPagesCommand
        private ICommand _fitInSelectedPagesCommand;
        /// <summary>
        /// Команда для вписывания карты в выделенную область
        /// </summary>
        public ICommand FitInSelectedPagesCommand
        {
            get { return _fitInSelectedPagesCommand ?? (_fitInSelectedPagesCommand = new RelayCommand(this.FinInSelectedPages, this.CanFinInSelectedPages)); }
        }
        /// <summary>
        /// Вписывание карты в выделенную область
        /// </summary>
        public void FinInSelectedPages(object parameter = null)
        {
            var servObjects = (from LayerObjectBase lObj in _drawingSurface.ServiceLayer.LayerObjects where (lObj as ServiceLayerObject).IsSelected select lObj).ToList();
            double topLeftX = (from LayerObjectBase lObj in servObjects orderby lObj.Nodes[0].WinPoint.X ascending select lObj.Nodes[0].WinPoint.X).FirstOrDefault();
            double topLeftY = (from LayerObjectBase lObj in servObjects orderby lObj.Nodes[0].WinPoint.Y ascending select lObj.Nodes[0].WinPoint.Y).FirstOrDefault();
            double bottomRightX = (from LayerObjectBase lObj in servObjects orderby lObj.Nodes[1].WinPoint.X descending select lObj.Nodes[1].WinPoint.X).FirstOrDefault();
            double bottomRightY = (from LayerObjectBase lObj in servObjects orderby lObj.Nodes[1].WinPoint.Y descending select lObj.Nodes[1].WinPoint.Y).FirstOrDefault();
            topLeftX++;
            topLeftY++;
            bottomRightY--; 
            bottomRightX--;

            Point leftBottomG = _drawingSurface.WinToGlobal(new Point(topLeftX, bottomRightY));
            Point rightTopG = _drawingSurface.WinToGlobal(new Point(bottomRightX, topLeftY));

            RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
            Rect rect = new Rect(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
            rastrObject.RastrSource.LoadExtent(leftBottomG, rightTopG, rect);

            rastrObject.Nodes[0].WinPoint = new Point(topLeftX, topLeftY);
            rastrObject.Nodes[1].WinPoint = new Point(bottomRightX, bottomRightY);
            rastrObject.Nodes[0].GlobalPoint = _drawingSurface.WinToGlobal(rastrObject.Nodes[0].WinPoint);
            rastrObject.Nodes[1].GlobalPoint = _drawingSurface.WinToGlobal(rastrObject.Nodes[1].WinPoint);
            rastrObject.UpdateSource();
        }
        /// <summary>
        /// Можно ли вписать карту в выделенную область
        /// </summary>
        public bool CanFinInSelectedPages(object parameter = null)
        {
            return (from ServiceLayerObject servObject in _drawingSurface.ServiceLayer.LayerObjects where servObject.IsSelected select servObject).Count() > 0;
        }
        #endregion // FitInSelectedPagesCommand

        #region ApplyNewScaleCommand
        private ICommand _applyNewScaleCommand;
        /// <summary>
        /// Комментарий_к_команде
        /// </summary>
        public ICommand ApplyNewScaleCommand
        {
            get { return _applyNewScaleCommand ?? (_applyNewScaleCommand = new RelayCommand(this.ApplyNewScale, this.CanApplyNewScale)); }
        }
        /// <summary>
        /// Комментарий_к_методу_ActionMethod
        /// </summary>
        public void ApplyNewScale(object parameter = null)
        {
            CommandEventParameter commEvtParam = parameter as CommandEventParameter;
            if (commEvtParam != null)
            {
                TextBox textBox = commEvtParam.EventSender as TextBox;
                try 
                {
                    double newProjZoom = Convert.ToInt32(textBox.Text) * _drawingSurface.ProjZoomScaleRel;                   
                    var gptOld = new Point(
                                                (_rasterObject.Nodes[0].GlobalPoint.X + _rasterObject.Nodes[1].GlobalPoint.X) / 2,
                                                (_rasterObject.Nodes[0].GlobalPoint.Y + _rasterObject.Nodes[1].GlobalPoint.Y) / 2
                                            );
                    var gptNew = _drawingSurface.WinToGlobal(
                                            new Point(
                                                (_rasterObject.Nodes[0].WinPoint.X + _rasterObject.Nodes[1].WinPoint.X) / 2,
                                                (_rasterObject.Nodes[0].WinPoint.Y + _rasterObject.Nodes[1].WinPoint.Y) / 2
                                            ),
                                            _drawingSurface.ProjectionWPos,
                                            newProjZoom);
                    Vector zoomOffset = gptNew - gptOld;
                    _drawingSurface.ProjectionWPos = _drawingSurface.ProjectionWPos - zoomOffset;
                    _drawingSurface.ProjectionZoom = newProjZoom;
                    _rasterObject.UpdateSource();
                    foreach (Node node in _rasterObject.Nodes)
                    {
                        node.GlobalPoint = _drawingSurface.WinToGlobal(node.WinPoint);
                    }
                    _drawingSurface.Render();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// Комментарий_к_методу_CanActionMethod
        /// </summary>
        public bool CanApplyNewScale(object parameter = null)
        {
            return true;
        }
        #endregion // ApplyNewScaleCommand

        #region PrintSettingsCommand
        private ICommand _printSettingsCommand;
        /// <summary>
        /// Команда для настройки печати
        /// </summary>
        public ICommand PrintSettingsCommand
        {
            get { return _printSettingsCommand ?? (_printSettingsCommand = new RelayCommand(this.PrintSettings, this.CanPrintSettings)); }
        }
        /// <summary>
        /// Настройки печати
        /// </summary>
        public void PrintSettings(object parameter = null)
        {
            try
            {
                using (System.Windows.Forms.PrintDialog pringDlg = new System.Windows.Forms.PrintDialog())
                {
                    pringDlg.AllowSomePages = false;
                    pringDlg.PrinterSettings = (PrinterSettings)_dftPageSettings.PrinterSettings.Clone();

                    if (pringDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _dftPageSettings.PrinterSettings = pringDlg.PrinterSettings;

                        int dftPageHeight = Convert.ToInt32((LandScape ? PaperWidth : PaperHeight) * 3.9370);
                        int dftPageWidth = Convert.ToInt32((LandScape ? PaperHeight : PaperWidth) * 3.9370);
                        bool paperSizeExists = false;

                        if (_dftPageSettings.PaperSize.Kind != PaperKind.Custom)
                        {
                            paperSizeExists =
                                (from PaperSize papSize
                                    in _dftPageSettings.PrinterSettings.PaperSizes
                                 where papSize.Kind == _dftPageSettings.PaperSize.Kind
                                 select papSize).Count() > 0;
                        }
                        else
                        {
                            var sizeCollection = from PaperSize papSize
                                    in _dftPageSettings.PrinterSettings.PaperSizes
                                                 where papSize.PaperName == PaperName && papSize.Height == dftPageHeight && papSize.Width == dftPageWidth
                                                 select papSize;
                            paperSizeExists = sizeCollection.Count() > 0;
                        }

                        if(!paperSizeExists)
                        {
                            PaperSize size = new PaperSize();
                            size.RawKind = (int)PaperKind.A4;
                            size.Height = 1169;
                            size.Width = 827;
                            size.PaperName = "A4";
                            _dftPageSettings.PaperSize = size;

                            MarginLeft = Convert.ToInt32(_dftPageSettings.Margins.Left / 3.9370);
                            MarginRight = Convert.ToInt32(_dftPageSettings.Margins.Right / 3.9370);
                            MarginTop = Convert.ToInt32(_dftPageSettings.Margins.Top / 3.9370);
                            MarginBottom = Convert.ToInt32(_dftPageSettings.Margins.Bottom / 3.9370);

                            _dftPageSettings.Margins.Left = Convert.ToInt32(MarginLeft * 3.9370);
                            _dftPageSettings.Margins.Top = Convert.ToInt32(MarginTop * 3.9370);
                            _dftPageSettings.Margins.Right = Convert.ToInt32(MarginRight * 3.9370);
                            _dftPageSettings.Margins.Bottom = Convert.ToInt32(MarginBottom * 3.9370);

                            LandScape = _dftPageSettings.Landscape;
                            PaperName = _dftPageSettings.PaperSize.PaperName;
                            int formatWidth = Convert.ToInt32(_dftPageSettings.PaperSize.Width / 3.9370);
                            int formatHeight = Convert.ToInt32(_dftPageSettings.PaperSize.Height / 3.9370);
                            PaperWidth = LandScape ? formatHeight : formatWidth;
                            PaperHeight = LandScape ? formatWidth : formatHeight;
                            _drawingSurface.ServiceLayer.SetPrintSurface(HorizontalCount, VerticalCount, PageWidth, PageHeight); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Rekod.Properties.Resources.mainFrm_errPrint + ex.Message,
                    Rekod.Properties.Resources.mainFrm_printStt, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Можно ли настраивать печать
        /// </summary>
        public bool CanPrintSettings(object parameter = null)
        {
            return true;
        }
        #endregion // PrintSettingsCommand

        #region SyncWithMapLibCommand
        private ICommand _syncWithMapLibCommand;
        /// <summary>
        /// Команда для обновления экстента используя текущий экстент mapLib
        /// </summary>
        public ICommand SyncWithMapLibCommand
        {
            get { return _syncWithMapLibCommand ?? (_syncWithMapLibCommand = new RelayCommand(this.SyncWithMapLib, this.CanSyncWithMapLib)); }
        }
        /// <summary>
        /// Обновление экстента из mapLib
        /// </summary>
        public void SyncWithMapLib(object parameter = null)
        {
            //_rasterObject.UpdateSource();
            //_drawingSurface.RecalculateWindowCoordinates();
            //Inscribe();
            System.Drawing.Image mapImage = _axMapLib1.Image;
            mvMapLib.mvBbox mapExtent = _axMapLib1.MapExtent;

            double xmin = mapExtent.a.x;
            double ymin = mapExtent.b.y;
            double xmax = mapExtent.b.x;
            double ymax = mapExtent.a.y;

            double _mainObjectWidth = _drawingSurface.GetLength(new Point(xmin, ymin), new Point(xmax, ymin), Convert.ToInt32(_axMapLib1.SRID));
            double _mainObjectHeight = _drawingSurface.GetLength(new Point(xmax, ymax), new Point(xmax, ymin), Convert.ToInt32(_axMapLib1.SRID));
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            double scaleRateX = _mainObjectWidth / (mapImage.Width * 0.0254 / gr.DpiX);
            double scaleRateY = _mainObjectHeight / (mapImage.Height * 0.0254 / gr.DpiY);
            if (scaleRateX > scaleRateY)
            {
                _drawingSurface.ProjectionZoom = _mainObjectWidth / mapImage.Width;
            }
            else
            {
                _drawingSurface.ProjectionZoom = _mainObjectHeight / mapImage.Height;
            }

            //_drawingSurface.RecalculateWindowCoordinates();
            _drawingSurface.Scale = Convert.ToInt32(_axMapLib1.ScaleZoom);
            _drawingSurface.SetProjZoomScaleRel(_drawingSurface.ProjectionZoom / Convert.ToDouble(_axMapLib1.ScaleZoom));
            _initialProjWPos = _drawingSurface.ProjectionWPos;
            _initialProjZoom = _drawingSurface.ProjectionZoom;


            _rasterObject.Nodes[0].GlobalPoint = new Point(mapExtent.a.x, mapExtent.b.y);
            _rasterObject.Nodes[1].GlobalPoint = new Point(mapExtent.b.x, mapExtent.a.y);
            _rasterObject.Nodes[0].WinPoint = new Point(0, 0);
            _rasterObject.Nodes[1].WinPoint = new Point(mapImage.Width, mapImage.Height);

            using (System.Drawing.Image image = _axMapLib1.Image)
            {
                _rastrSource.LoadBitmapByExtent(_axMapLib1.MapExtent, image.Width, image.Height);
            }
            Inscribe();
            
            _rasterObject.UpdateSource();
            _drawingSurface.Render();            
            //Inscribe();
        }
        /// <summary>
        /// Комментарий_к_методу_CanActionMethod
        /// </summary>
        public bool CanSyncWithMapLib(object parameter = null)
        {
            return true;
        }
        #endregion // SyncWithMapLibCommand
        #endregion Команды

        #region Методы
        [DllImport("maplib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool maplib_saveByteArray(
                        int leftTopX,
                        int leftTopY,
                        int rightBottomX,
                        int rightBottomY,
                        int srcWidth,
                        int srcHeight,
                        IntPtr bmpArrayPtr,
                        String fileToSave);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        
        public Visual GetVisualForPrint(RastrLayerObject rastrObject, ServiceLayerObject serviceObject)
        {
            double wpfFactor = 96 / 25.4;


            Point rastrTopLeft = rastrObject.Nodes[0].WinPoint;
            Point rastrBottomRight = rastrObject.Nodes[1].WinPoint;

            Point servTopLeft = serviceObject.Nodes[0].WinPoint;
            Point servBottomRight = serviceObject.Nodes[1].WinPoint;

            double rastrWidth = rastrBottomRight.X - rastrTopLeft.X;
            double rastrHeight = rastrBottomRight.Y - rastrTopLeft.Y;
            double servWidth = servBottomRight.X - servTopLeft.X;
            double servHeight = servBottomRight.Y - servTopLeft.Y;

            double relativeTop = (servTopLeft.Y - rastrTopLeft.Y) / rastrHeight;
            double relativeLeft = (servTopLeft.X - rastrTopLeft.X) / rastrWidth;
            double relativeHeight = servHeight / rastrHeight;  //(rastrBottomRight.Y - servBottomRight.Y) / rastrHeight;
            double relativeWidth = servWidth / rastrWidth;  //(rastrBottomRight.X - servBottomRight.X) / rastrWidth; 

            VisualBrush visBrush = new VisualBrush(rastrObject);
            visBrush.Viewbox = new Rect(relativeLeft, relativeTop, relativeWidth, relativeHeight);
            visBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
            visBrush.Viewport = new Rect(MarginLeft * wpfFactor, MarginTop * wpfFactor, servWidth, servHeight);
            visBrush.ViewportUnits = BrushMappingMode.Absolute;
            visBrush.Stretch = Stretch.Fill;

            Size paperSize = new Size(PaperWidth * wpfFactor, PaperHeight * wpfFactor); //new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            Rectangle rect = new Rectangle();
            rect.Margin = new Thickness(0);
            rect.Width = paperSize.Width;
            rect.Height = paperSize.Height;
            rect.Fill = visBrush;
            rect.Measure(paperSize);
            rect.Arrange(new Rect(0, 0, paperSize.Width, paperSize.Height));

            return rect; 
        }
        public void PrintItems(IList<LayerObjectBase> servObjects)
        {
            RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
            int i = 1;
            _printItems.Clear();

            try
            {
                foreach (ServiceLayerObject serviceObject in servObjects)
                {
                    Point rastrTopLeftW = rastrObject.Nodes[0].WinPoint;
                    Point rastrBottomRightW = rastrObject.Nodes[1].WinPoint;

                    Point servTopLeftW = serviceObject.Nodes[0].WinPoint;
                    Point servBottomRightW = serviceObject.Nodes[1].WinPoint;

                    double rastrWidthW = rastrBottomRightW.X - rastrTopLeftW.X;
                    double rastrHeightW = rastrBottomRightW.Y - rastrTopLeftW.Y;

                    Rect rastrRectW = new Rect(rastrTopLeftW, rastrBottomRightW);
                    Rect servRectW = new Rect(servTopLeftW, servBottomRightW);
                    Rect printAreaW = Rect.Intersect(rastrRectW, servRectW);

                    if (printAreaW != Rect.Empty)
                    {
                        int topLeftX = (int)(((printAreaW.TopLeft.X - rastrTopLeftW.X) / (double)rastrWidthW) * rastrObject.RastrSource.SrcWidth);
                        int topLeftY = (int)(((printAreaW.TopLeft.Y - rastrTopLeftW.Y) / (double)rastrHeightW) * rastrObject.RastrSource.SrcHeight);
                        int bottomRightX = (int)(topLeftX + (printAreaW.Width / rastrWidthW) * rastrObject.RastrSource.SrcWidth);
                        int bottomRightY = (int)(topLeftY + (printAreaW.Height / rastrHeightW) * rastrObject.RastrSource.SrcHeight);


                        int marginLeft = (int)((printAreaW.TopLeft.X - servTopLeftW.X) / (servRectW.Width) * _drawingSurface.ServiceLayer.PageWidth);
                        int marginTop = (int)((printAreaW.TopLeft.Y - servTopLeftW.Y) / (servRectW.Height) * _drawingSurface.ServiceLayer.PageHeight);
                        int marginRight = (int)((servBottomRightW.X - printAreaW.BottomRight.X) / (servRectW.Width) * _drawingSurface.ServiceLayer.PageWidth);
                        int marginBottom = (int)((servBottomRightW.Y - printAreaW.BottomRight.Y) / (servRectW.Height) * _drawingSurface.ServiceLayer.PageHeight);


                        String filePath = System.IO.Path.GetTempFileName();
                        int printNum = i;
                        Thickness printMargins = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
                        _printItems.Add(new PrintItem() { ImagePath = filePath, Paddings = printMargins, Num = printNum });
                        i++;

                        maplib_saveByteArray(
                                topLeftX,
                                topLeftY,
                                bottomRightX,
                                bottomRightY,
                                rastrObject.RastrSource.SrcWidth,
                                rastrObject.RastrSource.SrcHeight,
                                rastrObject.RastrSource.BmpByteArrayPointer,
                                filePath
                             );
                    }
                }

                if (rastrObject.RastrSource.BmpHBitmapPointer != IntPtr.Zero)
                {
                    DeleteObject(rastrObject.RastrSource.BmpHBitmapPointer);
                }
            }
            catch (Exception ex)
            {
                foreach (PrintItem printItem in _printItems)
                {
                    if (File.Exists(printItem.ImagePath))
                    {
                        File.Delete(printItem.ImagePath);
                    }
                }
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrintController = new System.Drawing.Printing.StandardPrintController();
                    printDoc.DefaultPageSettings = _dftPageSettings;
                    printDoc.PrinterSettings = _dftPageSettings.PrinterSettings;
                    printDoc.PrintPage += printDoc_PrintPage;
                    _currentPrintItemNum = 1;
                    ThreadProgress.ShowWait("Print Module");
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                ThreadProgress.Close("Print Module");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ThreadProgress.Close("Print Module");
                foreach (PrintItem printItem in _printItems)
                {
                    if (File.Exists(printItem.ImagePath))
                    {
                        File.Delete(printItem.ImagePath);
                    }
                }
                rastrObject.RastrSource.LoadBitmapExParams();
            }
        
        }
        #endregion Методы

        #region Обработчики
        void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_currentPrintItemNum < _printItems.Count)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false; 
            }
            if (_currentPrintItemNum > _printItems.Count)
            {
                return; 
            }
            ThreadProgress.SetText(String.Format(Rekod.Properties.Resources.PrintV_PrintProgress, _currentPrintItemNum, _printItems.Count));

            _currentPrintItem = _printItems[_currentPrintItemNum - 1]; 
            e.Graphics.Clear(System.Drawing.Color.White); 
            double mmInchFactor = 3.937;
            var location = new System.Drawing.Point
                (
                    (int)(MarginLeft * mmInchFactor - e.PageSettings.HardMarginX + _currentPrintItem.Paddings.Left * mmInchFactor),
                    (int)(MarginTop * mmInchFactor - e.PageSettings.HardMarginY + _currentPrintItem.Paddings.Top * mmInchFactor)
                );
            int sizeWidth = (int)((_drawingSurface.ServiceLayer.PageWidth - _currentPrintItem.Paddings.Left - _currentPrintItem.Paddings.Right) * mmInchFactor);
            int sizeHeight = (int)((_drawingSurface.ServiceLayer.PageHeight - _currentPrintItem.Paddings.Top - _currentPrintItem.Paddings.Bottom) * mmInchFactor);
            var size = (new System.Drawing.Size(sizeWidth, sizeHeight));
            var newRect = new System.Drawing.Rectangle(location, size);
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(_currentPrintItem.ImagePath))
            {
                e.Graphics.DrawImage(image, newRect);
            }
            e.HasMorePages = _currentPrintItem.Num < _printItems.Count;
            _currentPrintItem = null;
            _currentPrintItemNum++;
        }
        void _drawingSurface_Unloaded(object sender, RoutedEventArgs e)
        {
            RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
            if (rastrObject.RastrSource.BmpHBitmapPointer != IntPtr.Zero)
            {
                DeleteObject(rastrObject.RastrSource.BmpHBitmapPointer);
            }
        }
        void _drawingSurface_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                RastrLayerObject rastrObject = _drawingSurface.Layers[0].LayerObjects[0] as RastrLayerObject;
                if (rastrObject.RastrSource.BmpHBitmapPointer != IntPtr.Zero)
                {
                    DeleteObject(rastrObject.RastrSource.BmpHBitmapPointer);
                }
            }
        }
        #endregion Обработчики

        #region Действия
        public Action<object> ScrollShiftAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    ScrollViewer scrollviewer = commEvtParam.EventSender as ScrollViewer;
                    MouseWheelEventArgs e = commEvtParam.EventArgs as MouseWheelEventArgs;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        if (e.Delta > 0)
                        {
                            scrollviewer.LineLeft();
                            scrollviewer.LineLeft();
                        }
                        else
                        {
                            scrollviewer.LineRight();
                            scrollviewer.LineRight();
                        }
                        e.Handled = true;
                    }
                };
            }
        }
        #endregion Действия
    }

    public class PrintItem
    {
        public String ImagePath
        {
            get;
            set; 
        }
        public Int32 Num
        {
            get;
            set; 
        }
        public Thickness Paddings
        {
            get;
            set; 
        }
    }
}