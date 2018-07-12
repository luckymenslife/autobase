using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using OSGeo.GDAL;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using Rekod.Controllers;
using System.Windows.Input;
using Rekod.Repository.SettingsDB;
using Rekod.Services;
using System.Globalization;

namespace Rekod.RasterGeoreferenceModule.Model
{
    public class GDALOverviewsM: ViewModelBase
    {
        #region Поля
        private String _fileName = null;
        private int _imageWidth;
        private int _imageHeight;
        private int _maxTileLevel;
        private int _minTileLevel = 8;
        private Dataset _dataSet = null;
        private ObservableCollection<LinkedPointM> _linkedPoints;
        private Band[,] _bandsCollection = null;
        private ECoordsViewType _currentCorrdsViewType = ECoordsViewType.Degrees;
        private int _currentSrid = -1;
        #endregion Поля

        #region Конструкторы
        public GDALOverviewsM(String fileName)
        {
            _fileName = fileName;
            LinkedPoints.CollectionChanged += LinkedPoints_CollectionChanged;
        }
        #endregion Конструкторы

        #region Свойства
        public String FileName
        {
            get { return _fileName; }
        }
        public int ImageWidth
        {
            get { return _imageWidth; }
        }
        public int ImageHeight
        {
            get { return _imageHeight; }
        }
        public int MaxTileLevel
        {
            get { return _maxTileLevel; }
        }
        public int MinTileLevel
        {
            get { return _minTileLevel; }
        }
        public Dataset DataSet
        {
            get { return _dataSet; }
        }
        public Band[,] BandsCollection
        {
            get { return _bandsCollection; }
        }
        public bool IsWgsProjection
        {
            get { return _currentSrid == 4326; }
        }
        public ECoordsViewType CurrentCoordsViewType
        {
            get { return _currentCorrdsViewType; }
            set
            {
                OnPropertyChanged(ref _currentCorrdsViewType, value, () => this.CurrentCoordsViewType);
            }
        }
        public Int32 CurrentSrid
        {
            get
            {
                return _currentSrid;
            }
            private set
            {
                OnPropertyChanged(ref _currentSrid, value, () => this.CurrentSrid);
                OnPropertyChanged("IsWgsProjection");
            }
        }
        public String CurrentSridDef
        {
            get;
            private set;
        }
        public bool? AllPointsActive
        {
            get
            {
                int count = LinkedPoints.Count;
                int activeCount = 0; 
                if (count == 0)
                {
                    return false;
                }
                else
                {
                    foreach (var p in LinkedPoints)
                    {
                        if (p.IsActive)
                        {
                            activeCount++;
                        }
                    }
                    if (count == activeCount)
                    {
                        return true;
                    }
                    else if (activeCount == 0)
                    {
                        return false;
                    }
                    else 
                    {
                        return null;
                    }
                }
            }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < LinkedPoints.Count; i++)
                    {
                        LinkedPoints[i].IsActive = (bool)value;
                    }
                }
            }
        }
        #endregion Свойства

        #region Коллекции
        public ObservableCollection<LinkedPointM> LinkedPoints
        {
            get { return _linkedPoints ?? (_linkedPoints = new ObservableCollection<LinkedPointM>()); }
        }
        #endregion Коллекции

        #region Методы
        public bool PrepareOverviews()
        {
            _dataSet = Gdal.Open(_fileName, Access.GA_ReadOnly);
            String proj = _dataSet.GetProjection();
            String projRef = _dataSet.GetProjectionRef();

            if (_dataSet.RasterCount != 3)
            {
                return false;
            }

            Band redBand = _dataSet.GetRasterBand(1);
            int overviewsCount = redBand.GetOverviewCount();
            if (redBand.GetRasterColorInterpretation() != ColorInterp.GCI_RedBand)
            {
                return false;
            }
            bool hasOverviews = overviewsCount > 0;
            if (!hasOverviews)
            {
                int[] levels = new[] { 2, 4, 8, 16, 32, 64, 128 };
                var buildResult = _dataSet.BuildOverviews("AVERAGE", levels, GDALOverviews_Progress, "Sample data");
                if (buildResult != (int)CPLErr.CE_None)
                {
                    MessageBox.Show(Rekod.Properties.Resources.RstGrf_PyramidsNotCreated);
                    return false;
                }
            }
            overviewsCount = redBand.GetOverviewCount();
            hasOverviews = overviewsCount > 0;
            if (hasOverviews)
            {
                _imageWidth = redBand.XSize;
                _imageHeight = redBand.YSize;
                _maxTileLevel = Convert.ToInt32(Math.Ceiling(Math.Max(Math.Log(_imageWidth, 2), Math.Log(_imageHeight, 2))));
                _minTileLevel = Math.Max(8, _maxTileLevel - overviewsCount);


                _bandsCollection = new Band[4, _maxTileLevel - _minTileLevel + 1];

                Band greenBand = _dataSet.GetRasterBand(2);
                Band blueBand = _dataSet.GetRasterBand(3);

                _bandsCollection[0, _maxTileLevel - _minTileLevel] = redBand;
                _bandsCollection[1, _maxTileLevel - _minTileLevel] = greenBand;
                _bandsCollection[2, _maxTileLevel - _minTileLevel] = blueBand;


                for (int i = 0; i < _maxTileLevel - _minTileLevel; i++)
                {
                    var rb = redBand.GetOverview(i);
                    var bb = blueBand.GetOverview(i);
                    var gb = greenBand.GetOverview(i);

                    _bandsCollection[0, _maxTileLevel - _minTileLevel - 1 - i] = rb;
                    _bandsCollection[1, _maxTileLevel - _minTileLevel - 1 - i] = gb;
                    _bandsCollection[2, _maxTileLevel - _minTileLevel - 1 - i] = bb;
                }

                CurrentSrid = Convert.ToInt32(Program.mainFrm1.axMapLIb1.SRID);
                CurrentSridDef =
                    (from Spatial refSys in Program.SettingsDB.Spatials where refSys.Srid == CurrentSrid select refSys.ProjText).FirstOrDefault();
                return true;
            }
            return false;
        }
        public void ChangeMapSridLinkedPoints(IList<LinkedPointM> linkedPoints, int sridFrom, int sridTo)
        {
            using (SQLiteWork sqliteWork = new SQLiteWork(Program.connStringSQLite, true))
            {
                sqliteWork.InstallSpatialite();
                foreach (LinkedPointM linkedPoint in linkedPoints)
                {
                    sqliteWork.Sql = 
                        String.Format(
                            CultureInfo.CreateSpecificCulture("en-US"),
                            "SELECT x(point) as x, y(point) as y FROM (SELECT transform(geomfromtext('POINT({0} {1})', {2}), {3}) as point)",
                            linkedPoint.MapX,
                            linkedPoint.MapY,
                            sridFrom,
                            sridTo);
                    sqliteWork.Execute();
                    if (sqliteWork.CanRead())
                    {
                        linkedPoint.MapX = sqliteWork.GetValue<Double>("x");
                        linkedPoint.MapY = sqliteWork.GetValue<Double>("y");
                    }
                    sqliteWork.CloseReader();
                }
            }
        }
        #endregion Методы

        #region Обработчики
        public int GDALOverviews_Progress(double Complete, IntPtr Message, IntPtr Data)
        {
            Console.Write("Processing ... " + Complete * 100 + "% Completed.");
            if (Message != IntPtr.Zero)
                Console.Write(" Message:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Message));
            if (Data != IntPtr.Zero)
                Console.Write(" Data:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Data));
            Console.WriteLine("");
            return 1;
        }
        void LinkedPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("AllPointsActive");
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as LinkedPointM).PropertyChanged += LinkedPointM_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as LinkedPointM).PropertyChanged -= LinkedPointM_PropertyChanged;
                }
            }
        }
        void LinkedPointM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                OnPropertyChanged("AllPointsActive");
            }
        }
        #endregion Обработчики

        #region Команды
        #region TryChangeSridCommand
        private ICommand _tryChangeSridCommand;
        /// <summary>
        /// Команда для изменения проекции
        /// </summary>
        public ICommand TryChangeSridCommand
        {
            get { return _tryChangeSridCommand ?? (_tryChangeSridCommand = new RelayCommand(this.TryChangeSrid, this.CanTryChangeSrid)); }
        }
        /// <summary>
        /// Изменение проекции
        /// </summary>
        public void TryChangeSrid(object parameter = null)
        {
            List<object> valuesList = parameter as List<object>;
            ESridTypes sridType = (ESridTypes)valuesList[0];
            String sridValue = valuesList[1].ToString();
            Boolean assign = Convert.ToBoolean(valuesList[2]);
            Boolean recalc = Convert.ToBoolean(valuesList[3]);

            int srid;
            bool cantry = true;
            if (sridType == ESridTypes.Another)
            {
                cantry = int.TryParse(sridValue, out srid);
            }
            else
            {
                srid = (int)sridType;
            }

            if (cantry && CurrentSrid != srid)
            {
                String sridDef =
                    (from Spatial refSys in Program.SettingsDB.Spatials where refSys.Srid == srid select refSys.ProjText).FirstOrDefault();

                if (String.IsNullOrEmpty(sridDef))
                {
                    MessageBox.Show(Rekod.Properties.Resources.PgGeomVRec_ProjectionNotExists, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                else
                {
                    if (assign)
                    {
                        CurrentSrid = srid;
                        CurrentSridDef = sridDef;
                    }
                    else if (recalc)
                    {
                        if (LinkedPoints.Count != 0)
                        {
                            try
                            {
                                ChangeMapSridLinkedPoints(LinkedPoints, CurrentSrid, srid);
                                CurrentSrid = srid;
                                CurrentSridDef = sridDef;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли изменить проекцию
        /// </summary>
        public bool CanTryChangeSrid(object parameter = null)
        {
            bool result = true;
            List<object> valuesList = parameter as List<object>;
            ESridTypes sridType = (ESridTypes)valuesList[0];
            String sridValue = valuesList[1].ToString();
            Boolean assign = Convert.ToBoolean(valuesList[2]);
            Boolean recalc = Convert.ToBoolean(valuesList[3]);
            if (sridType != ESridTypes.Another)
            {
                result = CurrentSrid != (int)sridType;
            }
            else
            {
                result = CurrentSrid.ToString() != sridValue.Trim();
            }
            return result;
        }
        #endregion TryChangeSridCommand 
        #endregion Команды
    }
}