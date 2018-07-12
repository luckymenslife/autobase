using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Npgsql;
using System.Windows.Input;
using System.Windows.Forms.Integration;
using System.Collections.ObjectModel;
using Rekod.SQLiteSettings;
using Rekod.DataAccess.SourcePostgres.View.PgAttributes;
using Rekod.Services;
using Rekod.DataAccess.AbstractSource.Model;
using System.Data.SQLite;
using Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using mvMapLib;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.IO;
using OGRFramework;
using System.Globalization;
using Rekod.Controllers;
using Rekod.Repository;
using Rekod.Repository.SettingsDB;
using Rekod.ProjectionSelection;
using Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes
{
    // todo: (Dias) При извлечении координат преобразовывать в Double только если смогли получить координаты
    public class PgAttributesGeomVM : ViewModelBase
    {
        #region Статические методы
        public static EGeomType GetGeometryType(string geometrytypename)
        {
            geometrytypename = geometrytypename.ToUpper().Trim();
            EGeomType geomType = EGeomType.None;
            switch (geometrytypename)
            {
                case "POINT":
                    {
                        geomType = EGeomType.Point;
                        break;
                    }
                case "LINESTRING":
                case "LINE":
                    {
                        geomType = EGeomType.Line;
                        break;
                    }
                case "POLYGON":
                    {
                        geomType = EGeomType.Polygon;
                        break;
                    }
                case "MULTIPOINT":
                    {
                        geomType = EGeomType.MultiPoint;
                        break;
                    }
                case "MULTILINESTRING":
                case "MULTILINES":
                    {
                        geomType = EGeomType.MultiLine;
                        break;
                    }
                case "MULTIPOLYGON":
                    {
                        geomType = EGeomType.MultiPolygon;
                        break;
                    }
            }
            return geomType;
        }
        #endregion Статические методы

        #region Поля
        private GObject _selectedGObject;
        private GPoint _selectedGPoint;
        private AxmvMapLib.AxMapLIb _axMapLib;
        //private SQLiteCommand _sqliteCommand;
        private SQLiteWork _sqliteWork;
        private ObservableCollection<GObject> _geometryObjects = new ObservableCollection<GObject>();
        private EGeometryControlMode _geomMode;
        private ECoordsViewType _currentCorrdsViewType = ECoordsViewType.Degrees;
        private int _currentSrid = -1;
        private EGeomType _geomType;
        private bool _previewCurrentEnabled;
        private bool _previewEnabled;
        private bool _previewNodesEnabled;
        private bool _exPreviewCurrentEnabled;
        private bool _exPreviewEnabled;
        private bool _exPreviewNodesEnabled;
        private String _geometryTypeName;
        private bool _isMulti;
        private bool _geomLoaded = false;
        private bool _hasChanges = false;
        private Guid _guid = Guid.NewGuid();

        private string _tempLayerToShow = "tempLayerToShow";
        private string _pointLayer = "pointLayer";
        private string _singlePointLayer = "singlePointLayer";

        private NpgsqlConnectionStringBuilder _connect;
        private int _tableId;
        private int? _pkFieldValue;

        private String _schemeName;
        private String _tableName;
        private String _pkField;
        private String _geomField;
        private int _tableSrid;
        private ObservableCollection<GeomCharacteristic> _geomCharacteristicsList;
        #endregion Поля

        #region Коллекции
        public ObservableCollection<GObject> GeometryObjects
        {
            get { return _geometryObjects; }
            set { _geometryObjects = value; }
        }
        public ObservableCollection<GeomCharacteristic> GeomCharacteristicsList
        {
            get { return _geomCharacteristicsList ?? (_geomCharacteristicsList = new ObservableCollection<GeomCharacteristic>()); }
        }
        #endregion Коллекции

        #region Свойства
        public GObject SelectedGObject
        {
            get
            {
                return _selectedGObject;
            }
            private set
            {
                OnPropertyChanged(ref _selectedGObject, value, () => this.SelectedGObject);
            }
        }
        public GPoint SelectedGPoint
        {
            get
            {
                return _selectedGPoint;
            }
            private set
            {
                OnPropertyChanged(ref _selectedGPoint, value, () => this.SelectedGPoint);
                OnPropertyChanged("SelectedGObjecIsNotGPoint");
            }
        }
        public bool SelectedGObjecIsNotGPoint
        {
            get
            {
                if (SelectedGObject == null)
                {
                    return true;
                }
                else
                {
                    return SelectedGObject.GType != EGeomType.Point;
                }
            }
        }
        public String GeometryTypeName
        {
            get
            {
                return _geometryTypeName;
            }
            private set
            {
                _geometryTypeName = value;
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
        public EGeomType GeomType
        {
            get { return _geomType; }
        }
        public ECoordsViewType CurrentCoordsViewType
        {
            get { return _currentCorrdsViewType; }
            set
            {
                OnPropertyChanged(ref _currentCorrdsViewType, value, () => this.CurrentCoordsViewType);
            }
        }
        public AxmvMapLib.AxMapLIb AxMapLib
        {
            get { return _axMapLib; }
        }
        public int TableSrid
        {
            get { return _tableSrid; }
        }
        public bool HasChanges
        {
            get { return _hasChanges; }
        }
        public bool PreviewCurrentEnabled
        {
            get { return _previewCurrentEnabled; }
            set
            {
                OnPropertyChanged(ref _previewCurrentEnabled, value, () => this.PreviewCurrentEnabled);
            }
        }
        public bool PreviewEnabled
        {
            get { return _previewEnabled; }
            set { OnPropertyChanged(ref _previewEnabled, value, () => this.PreviewEnabled); }
        }
        public bool PreviewNodesEnabled
        {
            get { return _previewNodesEnabled; }
            set { OnPropertyChanged(ref _previewNodesEnabled, value, () => this.PreviewNodesEnabled); }
        }
        public bool IsWgsProjection
        {
            get { return _currentSrid == 4326; }
        }
        public String WKT
        {
            get
            {
                String wkt = GetWkt();
                if (wkt == null)
                    return null;
                return String.Format("st_transform(st_geomfromtext('{0}',{1}), {2})", wkt, CurrentSrid, TableSrid);
            }
        }
        public EGeometryControlMode GeomMode
        {
            get { return _geomMode; }
            private set { OnPropertyChanged(ref _geomMode, value, () => this.GeomMode); }
        }
        #endregion Свойства

        #region Конструкторы
        public PgAttributesGeomVM(int tableid, String wkt, bool isnew, int? pk, NpgsqlConnectionStringBuilder connect, int? srid = null)
        {
            _tableId = tableid;
            _pkFieldValue = pk;
            _connect = connect;
            _axMapLib = Program.mainFrm1.axMapLIb1;

            var _table = Program.repository.Tables.FirstOrDefault(w => (int)w.Id == _tableId);
            PgTableBaseM _pgtable = null;
            if (_table != null)
            {
                _pgtable = (PgTableBaseM)_table;
            }
            _tableSrid = _pgtable.Srid.Value;
            _schemeName = _pgtable.SchemeName;
            _tableName = _pgtable.Name;
            _geomField = _pgtable.GeomField;
            _pkField =_pgtable.PrimaryKeyField.Name;
            _geometryTypeName = _pgtable.GC_GeomType;
            _geomType = GetGeometryType(_geometryTypeName);
            _isMulti = GetIsMulti(_geomType);

            CurrentSrid = (srid == null) ? _tableSrid : srid.Value;
            Init(wkt);
        }
        public PgAttributesGeomVM(PgAtVM.PgAttributesVM attributeVM, String wkt = null, int? srid = null)
        {

            _axMapLib = Program.mainFrm1.axMapLIb1;

            _schemeName = attributeVM.Table.SchemeName;
            _tableName = attributeVM.Table.Name;
            _geomField = attributeVM.Table.GeomField;
            _pkField = attributeVM.Table.PrimaryKey;
            _connect = attributeVM.Connect;
            _tableSrid = (int)attributeVM.Table.Srid;
            //todo: (Dias) _geomType брать из базы, из GeometryColumns
            _geomType = attributeVM.Table.GeomType;
            _isMulti = GetIsMulti(_geomType);

            CurrentSrid = (srid == null) ? _tableSrid : srid.Value;
            Init(wkt);
        }
        public PgAttributesGeomVM(SourceCosmetic.ViewModel.CosmeticAttributes.CosmeticAttributesListVM attributesListVM, string wkt, int? nullable)
        {
            _axMapLib = Program.mainFrm1.axMapLIb1;
            _tableSrid = (int)attributesListVM.Table.Srid;

            _attributesListVM = attributesListVM;

            string geomType = string.Empty;
            if (!string.IsNullOrEmpty(wkt))
                geomType = wkt.Substring(0, wkt.IndexOf('('));

            _geomType = GetGeometryType(geomType);
            this.wkt = wkt;
            this.nullable = nullable;

            CurrentSrid = _tableSrid;
            Init(wkt);
        }
        #endregion Конструкторы

        #region Методы
        private void Init(String wkt)
        {
            PropertyChanged += PgAttributesGeomVM_PropertyChanged;
            GeometryObjects.CollectionChanged += GeometryObjects_CollectionChanged;

            _tempLayerToShow += _guid.ToString();
            _singlePointLayer += _guid.ToString();
            _pointLayer += _guid.ToString();

            bool mapSridExists = CheckSrid(Convert.ToInt32(_axMapLib.SRID));
            if (!mapSridExists)
            {
                throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_ProjectionNotExists);
            }
            bool tableSridExists = CheckSrid(_tableSrid);
            if (!tableSridExists)
            {
                throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_ProjectionNotExists);
            }
            if (!String.IsNullOrEmpty(wkt))
            {
                _hasChanges = true;
            }

            if (String.IsNullOrEmpty(wkt) && _pkFieldValue != null)
            {
                String sql =
                    String.Format("SELECT st_astext({0}) FROM {1}.{2} WHERE {4}={3}",
                                    _geomField,
                                    _schemeName,
                                    _tableName,
                                    _pkFieldValue,
                                    _pkField);
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = sql;
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        wkt = sqlCmd.GetValue<string>(0);
                    }
                    sqlCmd.Close();
                }
            }

            if (!String.IsNullOrEmpty(wkt))
            {
                GeomMode = EGeometryControlMode.Edit;
                ParseWkt(wkt, CurrentSrid);
                _geomLoaded = true;
            }
            else
            {
                _geomLoaded = true;
                GeomMode = EGeometryControlMode.Add;
                switch (_geomType)
                {
                    case EGeomType.Point:
                    case EGeomType.MultiPoint:
                        {
                            _geometryObjects.Add(new GPoint());
                            break;
                        }
                    case EGeomType.Line:
                    case EGeomType.MultiLine:
                        {
                            _geometryObjects.Add(new GLine());
                            break;
                        }
                    case EGeomType.Polygon:
                    case EGeomType.MultiPolygon:
                        {
                            _geometryObjects.Add(new GPolygon());
                            break;
                        }
                }
            }
        }
        public void ParseWkt(String wkt, int srid)
        {
            _geometryObjects.Clear();
            try
            {
                using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, true))
                {
                    _sqliteWork.InstallSpatialite();
                    _sqliteWork.Sql = "BEGIN;";
                    _sqliteWork.ExecuteNonQuery();

                    _sqliteWork.Sql = String.Format(@"SELECT geomfromtext('{0}', {1});", wkt, srid);
                    var geomVal = _sqliteWork.ExecuteScalar<object>();
                    if (geomVal == DBNull.Value || geomVal == null)
                    {
                        throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_UnableTransformGeom);
                    }

                    SqlParam mainGeometry = new SqlParam("@maingeometry", System.Data.DbType.Binary, geomVal);
                    _sqliteWork.Sql = String.Format(@"SELECT geometrytype(@maingeometry)");
                    GeometryTypeName = _sqliteWork.ExecuteScalar<String>(mainGeometry);

                    _sqliteWork.Sql = String.Format(@"SELECT numgeometries(@maingeometry);");
                    int geomNum = _sqliteWork.ExecuteScalar<Int32>(mainGeometry);

                    for (int i = 0; i < geomNum; i++)
                    {
                        _sqliteWork.Sql = String.Format("SELECT geometryn(@maingeometry, {0});", i + 1);
                        geomVal = _sqliteWork.ExecuteScalar<object>(mainGeometry);
                        if (geomVal == DBNull.Value)
                        {
                            throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_UnableTransformGeom);
                        }

                        SqlParam geomParam = new SqlParam("@geomparam", System.Data.DbType.Binary, geomVal);
                        _sqliteWork.Sql = String.Format(@"SELECT geometrytype(@geomparam)");
                        String typeName = _sqliteWork.ExecuteScalar<String>(geomParam);

                        switch (typeName)
                        {
                            case "POLYGON":
                                {
                                    GPolygon gPolygon = new GPolygon();
                                    _sqliteWork.Sql = String.Format("SELECT exteriorring(@geomparam);");
                                    var extRing = _sqliteWork.ExecuteScalar<object>(geomParam);
                                    if (extRing == DBNull.Value)
                                    {
                                        throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_UnableTransformGeom);
                                    }
                                    FillGObject((byte[])extRing, gPolygon);
                                    _sqliteWork.Sql = String.Format("SELECT numinteriorrings(@geomparam);");
                                    int interiorRingsNum = _sqliteWork.ExecuteScalar<int>(geomParam);
                                    for (int j = 0; j < interiorRingsNum; j++)
                                    {
                                        GHole gHole = new GHole(gPolygon);
                                        gPolygon.Hollows.Add(gHole);
                                        _sqliteWork.Sql = String.Format("SELECT interiorringn(@geomparam, {0});", j + 1);
                                        var intRing = _sqliteWork.ExecuteScalar<object>(geomParam);
                                        if (intRing == DBNull.Value)
                                        {
                                            throw new NotImplementedException(Rekod.Properties.Resources.PgGeomVRec_UnableTransformGeom);
                                        }
                                        FillGObject((byte[])intRing, gHole);
                                    }
                                    _geometryObjects.Add(gPolygon);
                                    break;
                                }
                            case "POINT":
                                {
                                    GPoint gPoint = new GPoint();
                                    _sqliteWork.Sql = "SELECT x(@geomparam); SELECT y(@geomparam);";
                                    _sqliteWork.Execute(geomParam);

                                    _sqliteWork.CanRead();
                                    gPoint.X = _sqliteWork.GetValue<Double>(0);
                                    _sqliteWork.CanNextResult();
                                    _sqliteWork.CanRead();
                                    gPoint.Y = _sqliteWork.GetValue<Double>(0);
                                    _sqliteWork.CloseReader();
                                    _geometryObjects.Add(gPoint);
                                    break;
                                }
                            case "LINESTRING":
                                {
                                    GLine gLine = new GLine();
                                    FillGObject((byte[])geomVal, gLine);
                                    _geometryObjects.Add(gLine);
                                    break;
                                }
                        }
                    }

                    _sqliteWork.Sql = "END;";
                    _sqliteWork.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _geometryObjects.Clear();
                cti.ThreadProgress.Close();
                MessageBox.Show(ex.Message + "\n" + Rekod.Properties.Resources.DGBH_GeomAssignedDefaultValue, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                switch (_geomType)
                {
                    case EGeomType.Point:
                    case EGeomType.MultiPoint:
                        {
                            _geometryObjects.Add(new GPoint());
                            break;
                        }
                    case EGeomType.Line:
                    case EGeomType.MultiLine:
                        {
                            _geometryObjects.Add(new GLine());
                            break;
                        }
                    case EGeomType.Polygon:
                    case EGeomType.MultiPolygon:
                        {
                            _geometryObjects.Add(new GPolygon());
                            break;
                        }
                }
            }
        }
        private void FillGObject(byte[] linestring, GObject gobject)
        {
            var lineParam = new SqlParam("@linestring", System.Data.DbType.Binary, linestring);
            _sqliteWork.Sql = "SELECT numpoints(@linestring);";
            var numPointsVal = _sqliteWork.ExecuteScalar<object>(lineParam);

            if (numPointsVal != null)
            {
                int numPoints = Convert.ToInt32(numPointsVal);
                if (gobject.GType == EGeomType.Polygon || gobject.GType == EGeomType.Hole)
                {
                    numPoints--;
                }
                for (int j = 0; j < numPoints; j++)
                {
                    _sqliteWork.Sql = String.Format("SELECT pointn(@linestring, {0})", j + 1);
                    var pointVal = _sqliteWork.ExecuteScalar<object>(lineParam);
                    var pointParam = new SqlParam("@point", System.Data.DbType.Binary, pointVal);

                    _sqliteWork.Sql = String.Format("SELECT x(@point);");
                    Double xVal = _sqliteWork.ExecuteScalar<Double>(pointParam);

                    _sqliteWork.Sql = String.Format("SELECT y(@point);");
                    Double yVal = _sqliteWork.ExecuteScalar<Double>(pointParam);

                    gobject.Points.Add(new GPoint(xVal, yVal));
                }
            }
        }
        public String GetWkt(GObject gobject = null)
        {
            String wkt = null;
            if (gobject != null)
            {
                switch (gobject.GType)
                {
                    case EGeomType.Point:
                        {
                            if (gobject.Points.Count == 1)
                                wkt = GetPointWkt(gobject as GPoint);
                            break;
                        }
                    case EGeomType.Line:
                        {
                            if (gobject.Points.Count > 1)
                                wkt = GetLineWkt(gobject as GLine);
                            break;
                        }
                    case EGeomType.Polygon:
                        {
                            if (gobject.Points.Count > 2)
                                wkt = GetPolygonWkt(gobject as GPolygon);
                            break;
                        }
                }
            }
            else
            {
                switch (_geomType)
                {
                    case EGeomType.Point:
                        {
                            GPoint gPoint = GeometryObjects.OfType<GPoint>().ElementAt(0) as GPoint;
                            if (gPoint.Points.Count == 1)
                                wkt = GetPointWkt(gPoint);
                            break;
                        }
                    case EGeomType.MultiPoint:
                        {
                            IEnumerable<GPoint> points = GeometryObjects.OfType<GPoint>() as IEnumerable<GPoint>;
                            wkt = GetMultiPointWkt(points);
                            break;
                        }
                    case EGeomType.Line:
                        {
                            GLine gLine = GeometryObjects.OfType<GLine>().ElementAt(0) as GLine;
                            if (gLine.Points.Count > 1)
                                wkt = GetLineWkt(gLine);
                            break;
                        }
                    case EGeomType.MultiLine:
                        {
                            IEnumerable<GLine> lines = GeometryObjects.OfType<GLine>() as IEnumerable<GLine>;
                            wkt = GetMultiLineWkt(lines);
                            break;
                        }
                    case EGeomType.Polygon:
                        {
                            GPolygon gPolygon = GeometryObjects.OfType<GPolygon>().ElementAt(0) as GPolygon;
                            if (gPolygon.Points.Count > 2)
                                wkt = GetPolygonWkt(gPolygon);
                            break;
                        }
                    case EGeomType.MultiPolygon:
                        {
                            IEnumerable<GPolygon> polygons = GeometryObjects.OfType<GPolygon>() as IEnumerable<GPolygon>;
                            wkt = GetMultiPolygonWkt(polygons);
                            break;
                        }
                }
            }
            return wkt;
        }
        public String GetWktHoleAsPolygon(GHole ghole)
        {
            String wkt = "POLYGON";
            List<String> outerPointParts = new List<string>();
            foreach (GPoint gp in ghole.Points)
            {
                String wktPart = "";
                wktPart += gp.X.ToString().Replace(',', '.');
                wktPart += " ";
                wktPart += gp.Y.ToString().Replace(',', '.');
                outerPointParts.Add(wktPart);
            }
            if (outerPointParts.Count > 0)
            {
                outerPointParts.Add(outerPointParts[0]);
            }
            wkt +=
               String.Format("(({0}))",
                       String.Join(", ", outerPointParts.ToArray()));
            return wkt;
        }
        public String GetMultiPointWkt(IEnumerable<GPoint> points)
        {
            List<String> wktParts = new List<string>();
            foreach (GPoint point in points)
            {
                if (point.Points.Count == 1)
                    wktParts.Add(GetPointWkt(point, true, false));
            }
            String wkt = null;
            if (wktParts.Count > 0)
            {
                wkt = String.Format(@"MULTIPOINT ({0})", String.Join(", ", wktParts.ToArray()));
                wktParts.Clear();
            }
            return wkt;
        }
        public String GetPointWkt(GPoint point, bool partofmulti = false, bool usebrackets = true)
        {
            String wkt = null;
            if (point.Points.Count > 0)
            {
                wkt = partofmulti ? "" : "POINT";
                String xPart = point.Points[0].X.ToString().Replace(',', '.');
                String yPart = point.Points[0].Y.ToString().Replace(',', '.');
                if (usebrackets)
                {
                    wkt += String.Format("({0} {1})",
                                xPart,
                                yPart);
                }
                else
                {
                    wkt += String.Format("{0} {1}",
                                xPart,
                                yPart);
                }
            }
            return wkt;
        }
        public String GetMultiLineWkt(IEnumerable<GLine> lines)
        {
            List<String> wktParts = new List<string>();
            foreach (GLine line in lines)
            {
                if (line.Points.Count > 1)
                    wktParts.Add(GetLineWkt(line, true));
            }
            String wkt = null;
            if (wktParts.Count > 0)
                wkt = String.Format(@"MULTILINESTRING ({0})", String.Join(", ", wktParts.ToArray()));
            return wkt;
        }
        public String GetLineWkt(GLine line, bool partofmulti = false)
        {
            String wkt = partofmulti ? "" : "LINESTRING";
            List<String> pointParts = new List<string>();
            foreach (GPoint gp in line.Points)
            {
                String wktPart = "";
                wktPart += gp.X.ToString().Replace(',', '.');
                wktPart += " ";
                wktPart += gp.Y.ToString().Replace(',', '.');
                pointParts.Add(wktPart);
            }
            wkt += String.Format("({0})", String.Join(", ", pointParts.ToArray()));
            pointParts.Clear();
            return wkt;
        }
        public String GetMultiPolygonWkt(IEnumerable<GPolygon> polygons)
        {
            List<String> wktParts = new List<string>();
            foreach (GPolygon polygon in polygons)
            {
                if (polygon.Points.Count > 2)
                    wktParts.Add(GetPolygonWkt(polygon, true));
            }
            String wkt = null;
            if (wktParts.Count > 0)
                wkt = String.Format(@"MULTIPOLYGON ({0})", String.Join(", ", wktParts.ToArray()));
            return wkt;
        }
        private String GetPolygonWkt(GPolygon poly, bool partofmulti = false)
        {
            String wkt = partofmulti ? "" : "POLYGON";
            List<String> outerPointParts = new List<string>();
            foreach (GPoint gp in poly.Points)
            {
                String wktPart = "";
                wktPart += gp.X.ToString().Replace(',', '.');
                wktPart += " ";
                wktPart += gp.Y.ToString().Replace(',', '.');
                outerPointParts.Add(wktPart);
            }
            if (outerPointParts.Count > 0)
            {
                outerPointParts.Add(outerPointParts[0]);
            }
            List<String> hollowParts = new List<string>();
            foreach (GHole hole in poly.Hollows)
            {
                List<String> holePointParts = new List<string>();
                foreach (GPoint gp in hole.Points)
                {
                    String wktPart = "";
                    wktPart += gp.X.ToString().Replace(',', '.');
                    wktPart += " ";
                    wktPart += gp.Y.ToString().Replace(',', '.');
                    holePointParts.Add(wktPart);
                }
                if (holePointParts.Count > 0)
                {
                    holePointParts.Add(holePointParts[0]);
                }
                hollowParts.Add(String.Format("({0})", String.Join(", ", holePointParts.ToArray())));
            }
            if (hollowParts.Count == 0)
            {
                wkt +=
                String.Format("(({0}))",
                        String.Join(", ", outerPointParts.ToArray()));
            }
            else
            {
                wkt +=
                    String.Format("(({0}), {1})",
                            String.Join(", ", outerPointParts.ToArray()),
                            String.Join(", ", hollowParts.ToArray()));
            }
            return wkt;
        }
        public GPoint GetCentroid(String wkt, int wktsrid, int centroidsrid)
        {
            GPoint result = new GPoint();
            using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
            {
                _sqliteWork.InstallSpatialite();
                _sqliteWork.Sql =
                    String.Format(@"SELECT x(cent), y(cent) FROM centroid(transform(geomfromtext('{0}', {1}), {2})) as cent",
                                        wkt,
                                        centroidsrid);
                _sqliteWork.Execute();
                _sqliteWork.CanRead();
                result.X = _sqliteWork.GetValue<Double>(0);
                result.Y = _sqliteWork.GetValue<Double>(1);
            }
            return result;
        }
        public String TransformWkt(String wkt, int from, int to)
        {
            String result = null;
            if (from == to)
            {
                result = wkt;
            }
            else
            {
                using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
                {
                    _sqliteWork.InstallSpatialite();
                    _sqliteWork.Sql =
                        String.Format(@"SELECT astext(transform(geomfromtext('{0}', {1}) ,{2}))",
                                            wkt,
                                            from,
                                            to);
                    result = _sqliteWork.ExecuteScalar<String>();
                }
            }
            return result;
        }
        public bool ChangeSrid(int oldSrid, int newSrid)
        {
            bool result = false;
            try
            {
                using (SQLiteWork sqliteWork = new SQLiteWork(Program.connStringSQLite, true))
                {
                    sqliteWork.InstallSpatialite();
                    String oldWkt = GetWkt();
                    sqliteWork.Sql = String.Format(@"SELECT astext(transform(geomfromtext('{0}', {1}) ,{2}))",
                                            oldWkt,
                                            oldSrid,
                                            newSrid);
                    String newWkt = sqliteWork.ExecuteScalar<String>();
                    _geomLoaded = false;
                    ParseWkt(newWkt, newSrid);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            finally
            {
                _geomLoaded = true;
            }
            return result;
        }
        private bool GetIsMulti(EGeomType geomtype)
        {
            bool isMulti = false;
            switch (geomtype)
            {
                case EGeomType.Point:
                case EGeomType.Line:
                case EGeomType.Polygon:
                    {
                        isMulti = false;
                        break;
                    }
                case EGeomType.MultiPoint:
                case EGeomType.MultiLine:
                case EGeomType.MultiPolygon:
                    {
                        isMulti = true;
                        break;
                    }
            }
            return isMulti;
        }
        private bool CheckSrid(int srid)
        {
            bool result = false;
            bool sridExists =
                (from Spatial refSys in Program.SettingsDB.Spatials where refSys.Srid == srid select refSys).Count() > 0;
            if (!sridExists)
            {
                using (SqlWork sqlWork = new SqlWork(_connect))
                {
                    sqlWork.sql = String.Format(@"SELECT EXISTS(SELECT * FROM public.spatial_ref_sys WHERE srid={0});", srid);
                    sridExists = Convert.ToBoolean(sqlWork.ExecuteScalar());
                }
                if (sridExists)
                {
                    using (SqlWork sqlWork = new SqlWork(_connect))
                    {
                        sqlWork.sql = String.Format("SELECT srid, auth_name, auth_srid, srtext, proj4text FROM public.spatial_ref_sys WHERE srid={0}", srid);
                        sqlWork.ExecuteReader();
                        sqlWork.CanRead();
                        String authName = sqlWork.GetValue<String>("auth_name");
                        int authSrid = sqlWork.GetValue<Int32>("auth_srid");
                        String srText = sqlWork.GetValue<String>("srtext");
                        String projText = sqlWork.GetValue<String>("proj4text");
                        sqlWork.Close();

                        Program.SettingsDB.Spatials.AddObject(new Spatial()
                            {
                                Srid = srid,
                                AuthName = authName,
                                AuthSrid = authSrid,
                                RefSysName = srText,
                                ProjText = projText
                            });
                        Program.SettingsDB.SaveChanges();
                        result = true;
                    }
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
        public bool GObjectIsValid(GObject gobject = null)
        {
            bool valid = false;
            if (gobject == null)
            {
                valid = true;
                foreach (GObject innerGObject in _geometryObjects)
                {
                    valid &= GObjectIsValid(innerGObject);
                }
            }
            else
            {
                switch (gobject.GType)
                {
                    case EGeomType.Point:
                        {
                            if (gobject.Points.Count == 1)
                            {
                                valid = true;
                            }
                            break;
                        }
                    case EGeomType.Line:
                        {
                            if (gobject.Points.Count > 1)
                            {
                                valid = true;
                            }
                            break;
                        }
                    case EGeomType.Polygon:
                        {
                            valid = true;
                            valid &= gobject.Points.Count > 2;
                            foreach (GHole gHole in (gobject as GPolygon).Hollows)
                            {
                                valid &= gHole.Points.Count > 2;
                            }
                            break;
                        }
                    case EGeomType.Hole:
                        {
                            valid = gobject.Points.Count > 2;
                            break;
                        }
                }
            }
            return valid;
        }
        public void ShowThisWkt(String wkt)
        {
            _axMapLib.SuspendLayout();
            mvLayer ll = _axMapLib.getLayer(_tempLayerToShow);
            if (ll == null)
            {
                mvStringArray ff = new mvStringArray();
                ff.count = 1;
                ff.setElem(0, "id");
                ll = _axMapLib.CreateLayer(_tempLayerToShow, ff);
                mvPenObject p1 = new mvPenObject();
                mvBrushObject b1 = new mvBrushObject();
                mvFontObject f1 = new mvFontObject();
                mvSymbolObject s1 = new mvSymbolObject();
                p1.Color = 0x0000ff;
                p1.ctype = 2;
                p1.width = 2;
                b1.style = 0;
                b1.hatch = 0;
                b1.bgcolor = 0x55777755;
                b1.fgcolor = 0x55777755;
                f1.Color = 0x0000ff;
                f1.fontname = "Map Symbols";
                f1.framecolor = 0xff0000;
                f1.size = 12;
                s1.shape = 35;
                ll.uniform = true;
                ll.SetUniformStyle(p1, b1, s1, f1);
            }
            else
            {
                ll.RemoveObjects();
            }
            {
                // Временный слой предпросмотра заносим вниз предпросмотра точек
                int tempLayerIndex = 0;
                int showPointsLayerIndex = 0;
                bool tempLayerExist = false;
                bool showPointsLayerExist = false;
                for (int i = 0; i < _axMapLib.LayersCount; i++)
                {
                    mvLayer tLayer = _axMapLib.getLayerByNum(i);
                    if (tLayer.NAME == _tempLayerToShow)
                    {
                        tempLayerIndex = i;
                        tempLayerExist = true;
                    }
                    if (tLayer.NAME == _pointLayer)
                    {
                        showPointsLayerIndex = i;
                        showPointsLayerExist = true;
                    }
                }
                if (tempLayerIndex < showPointsLayerIndex && tempLayerExist && showPointsLayerExist)
                {
                    for (int i = 0; i < (showPointsLayerIndex - tempLayerIndex); i++)
                    {
                        mvLayer tLayer = _axMapLib.getLayer(_tempLayerToShow);
                        tLayer.MoveDown();
                    }
                }
            }
            using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
            {
                _sqliteWork.InstallSpatialite();
                _sqliteWork.Sql = String.Format("SELECT astext( transform( geomfromtext('{0}', {2}), {1}) )", wkt, _axMapLib.SRID, _currentSrid);
                wkt = _sqliteWork.ExecuteScalar<String>();
                if (String.IsNullOrEmpty(wkt))
                {
                    throw new Exception(Rekod.Properties.Resources.PgAttributes_ErrorGeomConvert);
                }

                mvStringArray f2 = new mvStringArray();
                f2.count = 1;
                f2.setElem(0, "1");
                mvVectorObject o1 = ll.CreateObject();
                o1.setWKT(wkt);
                o1.SetAttributes(f2);
                _axMapLib.ResumeLayout();

                double x = Math.Abs(o1.bbox.b.x - o1.bbox.a.x);
                double y = Math.Abs(o1.bbox.b.y - o1.bbox.a.y);
                double z = (x > y) ? x : y;

                if (z > 0)
                {
                    z = z * 8;
                }

                if (Convert.ToInt32(o1.bbox.a.x) == Convert.ToInt32(o1.bbox.b.x) || Convert.ToInt32(o1.bbox.a.y) == Convert.ToInt32(o1.bbox.b.y))
                {
                    mvMapLib.mvPointWorld gb = new mvMapLib.mvPointWorld();
                    gb.x = o1.bbox.a.x;
                    gb.y = o1.bbox.a.y;
                    Program.mainFrm1.SetPosition(gb.x, gb.y, Program.mainFrm1.axMapLIb1.ScaleZoom);
                }
                else
                {
                    mvBbox bbox_temp = Program.mainFrm1.AddPaddingToBbox(o1.bbox, Program.mainFrm1.axMapLIb1.MapExtent);
                    Program.mainFrm1.axMapLIb1.SetExtent(bbox_temp);
                }
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
        }
        private void ShowPoints(ObservableCollection<GPoint> points)
        {
            _axMapLib.SuspendLayout();
            mvLayer layer = _axMapLib.getLayer(_pointLayer);
            {
                if (layer != null)
                {
                    _axMapLib.deleteLayer(ref layer);
                }
                mvStringArray attributesArray = new mvStringArray();
                attributesArray.count = 1;
                attributesArray.setElem(0, "id");
                layer = _axMapLib.CreateLayer(_pointLayer, attributesArray);
                mvPenObject pen = new mvPenObject();
                mvBrushObject brush = new mvBrushObject();
                mvFontObject font = new mvFontObject();
                mvSymbolObject symbol = new mvSymbolObject();
                pen.Color = 0x333333;
                pen.ctype = 2;
                pen.width = 2;
                brush.bgcolor = 0xffff00;
                brush.fgcolor = 0x00ffff;
                brush.style = 0;
                brush.hatch = 2;
                font.Color = 0x00ff00;
                font.fontname = "Map Symbols";
                font.framecolor = 0xff0000;
                font.size = 8;
                symbol.shape = 35;
                layer.uniform = true;
                layer.SetUniformStyle(pen, brush, symbol, font);
            }
            layer.MoveUp();

            String wkt = "MULTIPOINT(";
            foreach (GPoint gPoint in points)
            {
                wkt += gPoint.X.ToString().Replace(",", ".");
                wkt += " ";
                wkt += gPoint.Y.ToString().Replace(",", ".");
                wkt += ", ";
            }
            wkt = wkt.Trim(new[] { ',', ' ' });
            wkt += ")";
            if (wkt != "MULTIPOINT()")
            {
                using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
                {
                    _sqliteWork.InstallSpatialite();
                    _sqliteWork.Sql = String.Format("SELECT astext( transform( geomfromtext('{0}', {2}), {1}) )", wkt, _axMapLib.SRID, CurrentSrid);
                    wkt = _sqliteWork.ExecuteScalar<String>();
                }
                mvVectorObject layerObject = layer.CreateObject();
                layerObject.setWKT(wkt);

                _axMapLib.mapRepaint();
                _axMapLib.ResumeLayout();
            }
            ShowSinglePoint(SelectedGPoint);
        }
        private void ShowSinglePoint(GPoint point)
        {
            _axMapLib.SuspendLayout();

            mvLayer lll = _axMapLib.getLayer(_singlePointLayer);
            {
                if (lll != null)
                {
                    lll.RemoveObjects();
                }
                else
                {
                    mvStringArray ff = new mvStringArray();
                    ff.count = 1;
                    ff.setElem(0, "id");
                    lll = _axMapLib.CreateLayer(_singlePointLayer, ff);
                    mvPenObject p1 = new mvPenObject();
                    mvBrushObject b1 = new mvBrushObject();
                    mvFontObject f1 = new mvFontObject();
                    mvSymbolObject s1 = new mvSymbolObject();
                    p1.Color = 0x333333;
                    p1.ctype = 2;
                    p1.width = 2;
                    b1.bgcolor = 0xffff00;
                    b1.fgcolor = 0x00ffff;
                    b1.style = 0;
                    b1.hatch = 2;
                    f1.Color = 0x0000ff;
                    f1.fontname = "Map Symbols";
                    f1.framecolor = 0xff0000;
                    f1.size = 8;
                    s1.shape = 35;
                    lll.uniform = true;
                    lll.SetUniformStyle(p1, b1, s1, f1);
                    lll.MoveUp();
                    lll.MoveUp();
                }
            }

            if (point != null)
            {
                String wkt = "MULTIPOINT(";
                wkt += point.X.ToString().Replace(",", ".");
                wkt += " ";
                wkt += point.Y.ToString().Replace(",", ".");
                wkt += ")";
                using (_sqliteWork = new SQLiteWork(Program.connStringSQLite, false))
                {
                    _sqliteWork.InstallSpatialite();
                    _sqliteWork.Sql = String.Format("SELECT astext( transform( geomfromtext('{0}', {2}), {1}) )", wkt, _axMapLib.SRID, CurrentSrid);
                    wkt = _sqliteWork.ExecuteScalar<String>();
                }
                mvVectorObject layerObject = lll.CreateObject();
                layerObject.setWKT(wkt);
            }
            lll.MoveUp();

            _axMapLib.mapRepaint();
            _axMapLib.ResumeLayout();
        }
        public void HidePoints()
        {
            mvLayer layer = _axMapLib.getLayer(_pointLayer);
            {
                if (layer != null)
                {
                    _axMapLib.deleteLayer(ref layer);
                }
            }
            layer = _axMapLib.getLayer(_singlePointLayer);
            {
                if (layer != null)
                {
                    _axMapLib.deleteLayer(ref layer);
                }
            }
            _axMapLib.mapRepaint();
        }
        public void SuspendPreview()
        {
            _exPreviewCurrentEnabled = _previewCurrentEnabled;
            _exPreviewEnabled = _previewEnabled;
            _exPreviewNodesEnabled = _previewNodesEnabled;
            PreviewCurrentEnabled = false;
            PreviewEnabled = false;
            PreviewNodesEnabled = false;
        }
        public void ResumePreview()
        {
            PreviewCurrentEnabled = _exPreviewCurrentEnabled;
            PreviewEnabled = _exPreviewEnabled;
            PreviewNodesEnabled = _exPreviewNodesEnabled;
        }
        private void UpdatePreview()
        {
            if (PreviewEnabled || PreviewCurrentEnabled)
            {
                GObject gObject = PreviewCurrentEnabled ? SelectedGObject : null;

                bool valid = GObjectIsValid(gObject);
                if (valid)
                {
                    try
                    {
                        if (gObject != null && gObject.GType == EGeomType.Hole)
                        {
                            String wkt = GetWktHoleAsPolygon(gObject as GHole);
                            ShowThisWkt(wkt);
                        }
                        else
                        {
                            String wkt = GetWkt(gObject);
                            if (wkt != null)
                                ShowThisWkt(wkt);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        valid = false;
                    }
                }
                if (!valid)
                {
                    HidePreview();
                    _previewCurrentEnabled = false;
                    _previewEnabled = false;
                    OnPropertyChanged("PreviewCurrentEnabled");
                    OnPropertyChanged("PreviewEnabled");
                }
            }
            else
            {
                HidePreview();
            }
        }
        public void HidePreview()
        {
            mvMapLib.mvLayer layer = _axMapLib.getLayer(_tempLayerToShow);
            if (layer != null)
            {
                _axMapLib.deleteLayer(ref layer);
                _axMapLib.mapRepaint();
                _axMapLib.Refresh();
            }
        }
       
        #endregion Методы

        #region Команды
        #region SaveGeometryCommand
        private ICommand _saveGeometryCommand;
        /// <summary>
        /// Команда для сохранения геометрии
        /// </summary>
        public ICommand SaveGeometryCommand
        {
            get { return _saveGeometryCommand ?? (_saveGeometryCommand = new RelayCommand(this.SaveGeometry, this.CanSaveGeometry)); }
        }
        /// <summary>
        /// Сохранение геометрии
        /// </summary>
        public void SaveGeometry(object parameter = null)
        {
            String wkt = GetWkt();

            String geometry = "";
            if (!String.IsNullOrEmpty(wkt))
            {
                using (SqlWork sqlCmd = new SqlWork(_connect))
                {
                    sqlCmd.sql = String.Format("SELECT st_transform( st_geomfromtext('{0}',{1}), {2} )",
                                                    wkt,
                                                    CurrentSrid,
                                                    _tableSrid);
                    sqlCmd.Execute(false);
                    if (sqlCmd.CanRead())
                    {
                        geometry = sqlCmd.GetValue<string>(0);
                    }
                    sqlCmd.Close();
                }
            }

            using (SqlWork sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql = String.Format("UPDATE {0}.{1} SET {2}='{3}' WHERE {4}={5}",
                                            _schemeName,
                                            _tableName,
                                            _geomField,
                                            geometry,
                                            _pkField,
                                            _pkFieldValue);
                sqlCmd.Execute(true);
                sqlCmd.Close();
            }
        }
        /// <summary>
        /// Можно ли сохранить геометрию
        /// </summary>
        public bool CanSaveGeometry(object parameter = null)
        {
            return true;
        }
        #endregion // SaveGeometryCommand

        #region SelectedGObjectChangedCommand
        private ICommand _selectedGObjectChangedCommand;
        /// <summary>
        /// Команда при изменении выбранного объекта геометрии в дереве
        /// </summary>
        public ICommand SelectedGObjectChangedCommand
        {
            get { return _selectedGObjectChangedCommand ?? (_selectedGObjectChangedCommand = new RelayCommand(this.SelectedGObjectChanged)); }
        }
        /// <summary>
        /// Выбранный объект в дереве изменился
        /// </summary>
        public void SelectedGObjectChanged(object parameter = null)
        {
            Rekod.Behaviors.CommandEventParameter commEvtParam = parameter as Rekod.Behaviors.CommandEventParameter;
            if (commEvtParam != null)
            {
                RoutedPropertyChangedEventArgs<object> e = commEvtParam.EventArgs as RoutedPropertyChangedEventArgs<object>;
                SelectedGObject = e.NewValue as GObject;
            }
        }
        #endregion SelectedGObjectChangedCommand

        #region SelectedGPointChangedCommand
        private ICommand _selectedGPointChangedCommand;
        /// <summary>
        /// Команда при изменении текущей выбранной точки
        /// </summary>
        public ICommand SelectedGPointChangedCommand
        {
            get { return _selectedGPointChangedCommand ?? (_selectedGPointChangedCommand = new RelayCommand(this.SelectedGPointChanged)); }
        }
        /// <summary>
        /// Текущая выбранная точка изменилась
        /// </summary>
        public void SelectedGPointChanged(object parameter = null)
        {
            Rekod.Behaviors.CommandEventParameter commEvtParam = parameter as Rekod.Behaviors.CommandEventParameter;
            DataGrid dataGrid = commEvtParam.EventSender as DataGrid;
            if (dataGrid.CurrentCell != null)
            {
                SelectedGPoint = dataGrid.CurrentCell.Item as GPoint;
            }
            else
            {
                SelectedGPoint = null;
            }
        }
        #endregion SelectedGPointChangedCommand

        #region InvertCoordsCommand
        private ICommand _invertCoordsCommand;
        /// <summary>
        /// Команда для инвертирования координат
        /// </summary>
        public ICommand InvertCoordsCommand
        {
            get { return _invertCoordsCommand ?? (_invertCoordsCommand = new RelayCommand(this.InvertCoords, this.CanInvertCoords)); }
        }
        /// <summary>
        /// Инвертирование координат
        /// </summary>
        public void InvertCoords(object parameter = null)
        {
            GObject gObject = parameter as GObject;
            if (gObject != null)
            {
                SuspendPreview();
                foreach (GPoint gPoint in gObject.Points)
                {
                    gPoint.Invert();
                }
                ResumePreview();
            }
        }
        /// <summary>
        /// Можно ли инвертировать координаты
        /// </summary>
        public bool CanInvertCoords(object parameter = null)
        {
            return (parameter is GObject);
        }
        #endregion InvertCoordsCommand

        #region DeleteCoordsCommand
        private ICommand _deleteCoordsCommand;
        /// <summary>
        /// Команда для удаления координат
        /// </summary>
        public ICommand DeleteCoordsCommand
        {
            get { return _deleteCoordsCommand ?? (_deleteCoordsCommand = new RelayCommand(this.DeleteCoords, this.CanDeleteCoords)); }
        }
        /// <summary>
        /// Удаление координат
        /// </summary>
        public void DeleteCoords(object parameter = null)
        {
            var points = (parameter as IList).OfType<GPoint>();
            if (points != null)
            {
                if (MessageBox.Show(Rekod.Properties.Resources.PgGeomVRec_ReallyWantDeletePoints, Rekod.Properties.Resources.PgGeomVRec_PointsDeleting, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SuspendPreview();
                    List<GPoint> selPoints = new List<GPoint>();
                    foreach (GPoint gPoint in points)
                    {
                        selPoints.Add(gPoint);
                    }
                    foreach (GPoint gPoint in selPoints)
                    {
                        SelectedGObject.Points.Remove(gPoint);
                    }
                    ResumePreview();
                }
            }
        }
        /// <summary>
        /// Можно ли удалить координаты
        /// </summary>
        public bool CanDeleteCoords(object parameter = null)
        {
            if (parameter == null || SelectedGObject == null)
            {
                return false;
            }
            else
            {
                var points = (parameter as IList).OfType<GPoint>();
                if (points != null && points.Count() > 0 && SelectedGObject.GType != EGeomType.Point)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion DeleteCoordsCommand

        #region AddCoordsCommand
        private ICommand _addCoordsCommand;
        /// <summary>
        /// Команда для добавления координат
        /// </summary>
        public ICommand AddCoordsCommand
        {
            get { return _addCoordsCommand ?? (_addCoordsCommand = new RelayCommand(this.AddCoords, this.CanAddCoords)); }
        }
        /// <summary>
        /// Добавление координат к выбранному объекту
        /// </summary>
        public void AddCoords(object parameter = null)
        {
            if (SelectedGObject != null)
            {
                SelectedGObject.Points.Add(new GPoint());
            }
        }
        /// <summary>
        /// Можно ли добавить координаты
        /// </summary>
        public bool CanAddCoords(object parameter = null)
        {
            return (SelectedGObject != null && SelectedGObject.GType != EGeomType.Point);
        }
        #endregion // AddCoordsCommand

        #region MoveUpCoordsCommand
        private ICommand _moveUpCoordsCommand;
        /// <summary>
        /// Команда для перемещения выбранных координат вверх по списку
        /// </summary>
        public ICommand MoveUpCoordsCommand
        {
            get { return _moveUpCoordsCommand ?? (_moveUpCoordsCommand = new RelayCommand(this.MoveUpCoords, this.CanMoveUpCoords)); }
        }
        /// <summary>
        /// Перемещение выбранных координат вверх по списку
        /// </summary>
        public void MoveUpCoords(object parameter = null)
        {
            var points = (parameter as IList).OfType<GPoint>();
            if (SelectedGObject != null && points.Count() == 1)
            {
                int oldIndex = SelectedGObject.Points.IndexOf(points.ElementAt(0));
                if (oldIndex > 0)
                {
                    SelectedGObject.Points.Move(oldIndex, --oldIndex);
                }
            }
        }
        /// <summary>
        /// Можно ли переместить выбранные координаты вверх по списку
        /// </summary>
        public bool CanMoveUpCoords(object parameter = null)
        {
            if (parameter == null || SelectedGObject == null)
            {
                return false;
            }
            else
            {
                var points = (parameter as IList).OfType<GPoint>();
                if (points != null && points.Count() == 1 && SelectedGObject.Points.IndexOf(points.ElementAt(0)) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion MoveUpCoordsCommand

        #region MoveDownCoordsCommand
        private ICommand _moveDownCoordsCommand;
        /// <summary>
        /// Команда для перемещения выбранных координат вниз по списку
        /// </summary>
        public ICommand MoveDownCoordsCommand
        {
            get { return _moveDownCoordsCommand ?? (_moveDownCoordsCommand = new RelayCommand(this.MoveDownCoords, this.CanMoveDownCoords)); }
        }
        /// <summary>
        /// Перемещение выбранных координат вниз по списку
        /// </summary>
        public void MoveDownCoords(object parameter = null)
        {
            var points = (parameter as IList).OfType<GPoint>();
            if (SelectedGObject != null && points.Count() == 1)
            {
                int oldIndex = SelectedGObject.Points.IndexOf(points.ElementAt(0));
                if (oldIndex < SelectedGObject.Points.Count - 1)
                {
                    SelectedGObject.Points.Move(oldIndex, ++oldIndex);
                }
            }
        }
        /// <summary>
        /// Можно ли переместить выбранные координаты вниз по списку
        /// </summary>
        public bool CanMoveDownCoords(object parameter = null)
        {
            if (parameter == null || SelectedGObject == null)
            {
                return false;
            }
            else
            {
                var points = (parameter as IList).OfType<GPoint>();
                if (points != null && points.Count() == 1 && SelectedGObject.Points.IndexOf(points.ElementAt(0)) < SelectedGObject.Points.Count - 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion MoveDownCoordsCommand

        #region TryChangeSridCommand
        private ICommand _tryChangeSridCommand;
        /// <summary>
        /// Команда для установки нового srid
        /// </summary>
        public ICommand TryChangeSridCommand
        {
            get { return _tryChangeSridCommand ?? (_tryChangeSridCommand = new RelayCommand(this.TryChangeSrid, this.CanTryChangeSrid)); }
        }
        /// <summary>
        /// Установка нового srid
        /// </summary>
        private void TryChangeSrid(object parameter = null)
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
                bool exists = CheckSrid(srid);
                if (!exists)
                {
                    MessageBox.Show(Rekod.Properties.Resources.PgGeomVRec_ProjectionNotExists, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                else
                {
                    SuspendPreview();
                    if (assign)
                    {
                        CurrentSrid = srid;
                        if (_geomLoaded)
                        {
                            _hasChanges = true;
                        }
                    }
                    else if (recalc)
                    {
                        if (GObjectIsValid())
                        {
                            if (ChangeSrid(CurrentSrid, srid))
                            {
                                CurrentSrid = srid;
                            }
                        }
                    }
                    ResumePreview();
                }
            }
        }
        /// <summary>
        /// Можно ли установить новый srid
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
        #endregion // TryChangeSridCommand

        #region ShowGeomCharacteristicsCommand
        private ICommand _showGeomCharacteristicsCommand;
        /// <summary>
        /// Команда для отображения характеристик геометрии
        /// </summary>
        public ICommand ShowGeomCharacteristicsCommand
        {
            get { return _showGeomCharacteristicsCommand ?? (_showGeomCharacteristicsCommand = new RelayCommand(this.ShowGeomCharacteristics, this.CanShowGeomCharacteristics)); }
        }
        /// <summary>
        /// Отображение характеристик геометрии
        /// </summary>
        public void ShowGeomCharacteristics(object parameter = null)
        {
            GeomCharacteristicsList.Clear();
            GObject gObject = parameter as GObject;
            bool validGeom = GObjectIsValid(gObject);
            if (validGeom)
            {
                try
                {
                    String wkt = GetWkt(gObject);
                    if (wkt == null)
                        throw new Exception(Rekod.Properties.Resources.DGBH_NotEnoughPointsToGeom);

                    bool geography_exists = false;
                    String geom = "", geom_old = "";
                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.Connection.Open();
                        String[] vers =
                            sqlCmd.Connection.ServerVersion.Split(new[] { '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        sqlCmd.Connection.Close();
                        int major = int.Parse(vers[0]);

                        sqlCmd.sql = "SELECT exists(SELECT true FROM pg_catalog.pg_type WHERE typname like 'geography')";
                        sqlCmd.Execute(false);
                        if (sqlCmd.CanRead())
                        {
                            geography_exists = sqlCmd.GetBoolean(0);
                        }
                        sqlCmd.Close();
                    }

                    if (geography_exists)
                    {
                        geom = String.Format("geography(st_transform(st_geomfromtext('{0}', {1}), {2}))", wkt, _currentSrid, 4326);
                        geom_old = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", wkt, _currentSrid, _axMapLib.SRID);
                    }
                    else
                    {
                        geom = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", wkt, _currentSrid, _axMapLib.SRID);
                        geom_old = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", wkt, _currentSrid, _axMapLib.SRID);
                    }

                    String geomType = "";
                    using (SqlWork sqlCmd = new SqlWork(_connect, false))
                    {
                        sqlCmd.sql = String.Format("SELECT st_geometrytype({0})", geom_old);
                        geomType = sqlCmd.ExecuteScalar<String>();
                        sqlCmd.Close();
                    }

                    if (String.IsNullOrEmpty(geomType))
                        throw new Exception(Rekod.Properties.Resources.PgAttributes_ErrorGeomConvert);

                    GeomCharacteristicsList.Add(new GeomCharacteristic(Rekod.Properties.Resources.PgGeomVRec_Type, geomType, ""));

                    if (geomType.ToUpper().Contains("POLYGON"))
                    {
                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = String.Format("SELECT st_area({0})", geom);
                            string temp = sqlCmd.ExecuteScalar<String>();
                            try
                            {
                                GeomCharacteristicsList.Add(new GeomCharacteristic(Rekod.Properties.Resources.PgGeomVRec_Square, temp, ""));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                            }
                            sqlCmd.Close();
                        }

                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = String.Format("SELECT ST_perimeter({0})", geom_old);
                            GeomCharacteristicsList.Add(new GeomCharacteristic(Rekod.Properties.Resources.PgGeomVRec_Perimeter, sqlCmd.ExecuteScalar<String>(), ""));
                            sqlCmd.Close();
                        }
                    }
                    else if (geomType.ToUpper().Contains("LINE"))
                    {
                        using (SqlWork sqlCmd = new SqlWork(_connect))
                        {
                            sqlCmd.sql = String.Format("SELECT st_length({0})", geom);
                            GeomCharacteristicsList.Add(new GeomCharacteristic(Rekod.Properties.Resources.PgGeomVRec_Length, sqlCmd.ExecuteScalar<String>(), ""));
                            sqlCmd.Close();
                        }
                    }
                    String centroidCoords = "X: ";
                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.sql = String.Format("SELECT st_transform(st_geomfromtext('{0}', {1}), {2})", wkt, _currentSrid, _axMapLib.SRID);
                        geom = (string)sqlCmd.ExecuteScalar();
                        sqlCmd.Close();
                    }

                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.sql = String.Format("SELECT st_x(st_centroid('{0}'))", geom);
                        double x = sqlCmd.ExecuteScalar<double>();
                        if (Math.Abs(x) < 0.000001)
                            x = 0;
                        centroidCoords += x.ToString();
                        sqlCmd.Close();
                    }

                    using (SqlWork sqlCmd = new SqlWork(_connect))
                    {
                        sqlCmd.sql = String.Format("SELECT st_y(st_centroid('{0}'))", geom);
                        double y = sqlCmd.ExecuteScalar<double>();
                        if (Math.Abs(y) < 0.000001)
                            y = 0;
                        centroidCoords += " Y: " + y.ToString();
                        sqlCmd.Close();
                    }

                    GeomCharacteristicsList.Add(new GeomCharacteristic(String.Format(Rekod.Properties.Resources.PgGeomVRec_Centroid + " ({0})", _axMapLib.SRID), centroidCoords, ""));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }
        /// <summary>
        /// Можно ли отобразить характеристики геометрии
        /// </summary>
        public bool CanShowGeomCharacteristics(object parameter = null)
        {
            GObject gObject = parameter as GObject;
            return GObjectIsValid(gObject);
        }
        #endregion // ShowGeomCharacteristicsCommand

        #region AddGeomObjectCommand
        private ICommand _addGeomObjectCommand;
        /// <summary>
        /// Команда для добавления нового объекта в геометрию
        /// </summary>
        public ICommand AddGeomObjectCommand
        {
            get { return _addGeomObjectCommand ?? (_addGeomObjectCommand = new RelayCommand(this.AddGeomObject, this.CanAddGeomObject)); }
        }
        /// <summary>
        /// Добавление нового объекта в геометрию
        /// </summary>
        public void AddGeomObject(object parameter = null)
        {
            if (parameter != null)
            {
                GObject gObject = parameter as GObject;
                switch (gObject.GType)
                {
                    case EGeomType.Point:
                        {
                            if (_isMulti)
                            {
                                _geometryObjects.Add(new GPoint());
                            }
                            break;
                        }
                    case EGeomType.Line:
                        {
                            if (_isMulti)
                            {
                                _geometryObjects.Add(new GLine());
                            }
                            break;
                        }
                    case EGeomType.Polygon:
                        {
                            if (_isMulti)
                            {
                                _geometryObjects.Add(new GPolygon());
                            }
                            break;
                        }
                }
            }
        }
        /// <summary>
        /// Можно ли добавить новый объект в геометрию
        /// </summary>
        public bool CanAddGeomObject(object parameter = null)
        {
            return _isMulti && (parameter is GObject);
        }
        #endregion // AddGeomObjectCommand

        #region RemoveGeomObjectCommand
        private ICommand _removeGeomObjectCommand;
        /// <summary>
        /// Команда для удаления геометрии
        /// </summary>
        public ICommand RemoveGeomObjectCommand
        {
            get { return _removeGeomObjectCommand ?? (_removeGeomObjectCommand = new RelayCommand(this.RemoveGeomObject, this.CanRemoveGeomObject)); }
        }
        /// <summary>
        /// Удаление геометрии
        /// </summary>
        public void RemoveGeomObject(object parameter = null)
        {
            //DGBH_DeleteObjMessage
            GObject gObject = parameter as GObject;
            if (gObject != null && MessageBox.Show(Properties.Resources.DGBH_DeleteObjMessage, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (_geometryObjects.Contains(gObject))
                {
                    _geometryObjects.Remove(gObject);
                }
            }
        }
        /// <summary>
        /// Можно ли удалить геометрию
        /// </summary>
        public bool CanRemoveGeomObject(object parameter = null)
        {
            GObject gObject = parameter as GObject;
            bool result = false;
            if (gObject != null)
            {
                if (_geometryObjects.Count > 1)
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion // RemoveGeomObjectCommand

        #region AddHoleToPolygonCommand
        private ICommand _addHoleToPolygonCommand;
        /// <summary>
        /// Команда для добавления полости в полигон
        /// </summary>
        public ICommand AddHoleToPolygonCommand
        {
            get { return _addHoleToPolygonCommand ?? (_addHoleToPolygonCommand = new RelayCommand(this.AddHoleToPolygon, this.CanAddHoleToPolygon)); }
        }
        /// <summary>
        /// Добавление полости в полигон
        /// </summary>
        public void AddHoleToPolygon(object parameter = null)
        {
            GPolygon gPolygon = parameter as GPolygon;
            if (gPolygon != null)
            {
                gPolygon.Hollows.Add(new GHole(gPolygon));
            }
        }
        /// <summary>
        /// Можно ли добавить полость в полигон
        /// </summary>
        public bool CanAddHoleToPolygon(object parameter = null)
        {
            bool result = parameter is GPolygon;
            return result;
        }
        #endregion // AddHoleToPolygonCommand

        #region RemoveHoleFromPolygonCommand
        private ICommand _removeHoleFromPolygonCommand;
        /// <summary>
        /// Команда для удаления полости из полигона
        /// </summary>
        public ICommand RemoveHoleFromPolygonCommand
        {
            get { return _removeHoleFromPolygonCommand ?? (_removeHoleFromPolygonCommand = new RelayCommand(this.RemoveHoleFromPolygon, this.CanRemoveHoleFromPolygon)); }
        }
        /// <summary>
        /// Удаление полости из полигона
        /// </summary>
        public void RemoveHoleFromPolygon(object parameter = null)
        {
            GHole gHole = parameter as GHole;
            if (gHole != null && MessageBox.Show(Properties.Resources.DGBH_HoleDeleteMessage, Properties.Resources.DGBH_HoleDeleteMessageHeader, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (gHole != null)
                {
                    gHole.Parent.Hollows.Remove(gHole);
                }
            }
        }
        /// <summary>
        /// Можно ли удалить полость из полигона
        /// </summary>
        public bool CanRemoveHoleFromPolygon(object parameter = null)
        {
            return parameter is GHole;
        }
        #endregion // RemoveHoleFromPolygonCommand

        #region ShowOnMapCommand
        private ICommand _showOnMapCommand;
        /// <summary>
        /// Команда для предпросмотра указанной геометрии
        /// </summary>
        public ICommand ShowOnMapCommand
        {
            get { return _showOnMapCommand ?? (_showOnMapCommand = new RelayCommand(this.ShowOnMap, this.CanShowOnMap)); }
        }
        /// <summary>
        /// Предпросмотр указанной геометрии
        /// </summary>
        public void ShowOnMap(object parameter = null)
        {
            GObject gObject = parameter as GObject;
            if (gObject != null)
            {
                PreviewCurrentEnabled = true;
            }
        }
        /// <summary>
        /// Можно ли включить предпросмотр геометрии
        /// </summary>
        public bool CanShowOnMap(object parameter = null)
        {
            return parameter is GObject;
        }
        #endregion // ShowOnMapCommand

        #region ExportCommand
        private ICommand _exportCommand;
        /// <summary>
        /// Команда для экспорта
        /// </summary>
        public ICommand ExportCommand
        {
            get { return _exportCommand ?? (_exportCommand = new RelayCommand(this.Export, this.CanExport)); }
        }
        /// <summary>
        /// Экспорт
        /// </summary>
        public void Export(object parameter = null)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.OverwritePrompt = false;

            sfd.Filter = @"ESRI Shape-file|*.shp|MapInfo-file|*.tab|GeoJSON|*.geojson|Mapinfo  Interchange  Format  (MIF)|*.mif|SQLite|*.sqlite|Text file|*.txt";
            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            while (File.Exists(sfd.FileName))
            {
                if (System.Windows.Forms.MessageBox.Show(Rekod.Properties.Resources.DGBH_ErrorReplaceFile,
                    Rekod.Properties.Resources.DGBH_ErrorHeader, System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.No)
                    return;
                if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
            }

            var ext = System.IO.Path.GetExtension(sfd.FileName).ToLower();
            if (ext == ".shp" || ext == ".tab" || ext == ".bna" || ext == ".csv" || ext == ".geojson" || ext == ".mif" || ext == ".dxf" ||
                ext == ".xml" || ext == ".gml" || ext == ".gmt" || ext == ".gxt" || ext == ".itf" || ext == ".kml" || ext == ".sqlite")
            {
                ExportWithOGR(sfd.FileName);
            }
            else if (System.IO.Path.GetExtension(sfd.FileName).ToUpper() == ".TXT")
            {
                if (_geometryObjects.Count == 1 || MessageBox.Show(
                    Properties.Resources.PgAttributesGeom_TxtWarning,
                    Properties.Resources.ucTable_Export,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    ExportToTxt(sfd.FileName);
                }
            }
        }
        private void ExportWithOGR(String file_name)
        {
            try
            {
                tablesInfo ti = classesOfMetods.getTableInfo(_tableId);

                var sql_string = string.Format("SELECT st_astext({0}) as sys_geom_column_for_export,* FROM {1}.{2} WHERE {4}={3}",
                    ti.geomFieldName, _schemeName, _tableName, _pkFieldValue, ti.pkField);
                SqlWork sw = new SqlWork() { sql = sql_string };
                var dt = sw.ExecuteGetTable();
                sw.Close();
                sw = new SqlWork() { sql = "SELECT srtext FROM spatial_ref_sys where srid=" + ti.srid };
                sw.ExecuteReader();
                string sridWkt = sw.CanRead() ? sw.GetString(0) : "";
                sw.Close();
                // Спрашиваем место для сохранение shp файла
                //cti.ThreadProgress.ShowWait();
                var fi = new FileInfo(file_name);
                SHPWork shpWork = new SHPWork(fi);
                var ext = fi.Extension.ToLower();
                string errorMessage = "";

                if (ext == ".gmt")
                    fi = new FileInfo(fi.Directory + "\\" + fi.Name);
                if (ext == ".bna" || ext == ".geojson" || ext == ".gml" || ext == ".itf" || ext == ".kml" ||
                    ext == ".mif" || ext == ".xml" || ext == ".gxt" || ext == ".dxf")
                    fi = new FileInfo(fi.Directory + "\\" + fi.Name + "\\" + fi.Name);

                var shpExporter = new Rekod.ImportExport.Exporters.SHPExporter(ti, fi);
                shpExporter.Export(_pkFieldValue);
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close();
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            finally
            {
                cti.ThreadProgress.Close();
            }
        }
        private void ExportToTxt(String filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                using (StreamWriter sw = fi.CreateText())
                {
                    if (SelectedGObject != null)
                    {
                        foreach (GPoint gPoint in SelectedGObject.Points)
                        {
                            sw.Write(gPoint.X.ToString() + "\t");
                            sw.WriteLine(gPoint.Y.ToString());
                        }
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
        /// <summary>
        /// Можно ли экспортировать
        /// </summary>
        public bool CanExport(object parameter = null)
        {
            return true;
        }
        #endregion // ExportCommand

        #region ImportCommand
        private ICommand _importCommand;
        private SourceCosmetic.ViewModel.CosmeticAttributes.CosmeticAttributesListVM _attributesListVM;
        private string wkt;
        private int? nullable;
        /// <summary>
        /// Команда для импорта
        /// </summary>
        public ICommand ImportCommand
        {
            get { return _importCommand ?? (_importCommand = new RelayCommand(this.Import, this.CanImport)); }
        }
        /// <summary>
        /// Импорт
        /// </summary>
        public void Import(object parameter = null)
        {
            if (SelectedGObject == null)
            {
                MessageBox.Show(Rekod.Properties.Resources.DGBH_SelectObjForInputCoord, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            SuspendPreview();
            CoordsFromFile();
            ResumePreview();
        }
        private void CoordsFromFile()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (_geomType != EGeomType.Point && _geomType != EGeomType.MultiPoint)
            {
                ofd.Filter = Rekod.Properties.Resources.LocAllSupportedFormats +
                    "|*.shp;*.tab;*.mif;*.geojson;*.sqlite;*.txt|" +
                    @"ESRI Shape файлы|*.shp|MapInfo файлы|*.tab|
GeoJSON|*.geojson|Mapinfo  Interchange  Format  (MIF)|*.mif|SQLite|*.sqlite|Текстовый файл|*.txt";
            }
            else if (_geomType == EGeomType.Point || _geomType == EGeomType.MultiPoint)
            {
                ofd.Filter = Rekod.Properties.Resources.LocAllSupportedFormats +
                    "|*.jpg;*.shp;*.tab;*.mif;*.geojson;*.sqlite;*.txt|" +
                    @"JPEG|*.jpg|ESRI Shape файлы|*.shp|MapInfo файлы|*.tab|
GeoJSON|*.geojson|Mapinfo  Interchange  Format  (MIF)|*.mif|SQLite|*.sqlite|Текстовый файл|*.txt";
            }
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var ext = System.IO.Path.GetExtension(ofd.FileName).ToLower();
                if (ext == ".shp" || ext == ".tab" || ext == ".bna" || ext == ".csv" || ext == ".gxt" || ext == ".mif" || ext == ".xml" || ext == ".dxf" ||
                    ext == ".geojson" || ext == ".gml" || ext == ".gmt" || ext == ".itf" || ext == ".kml" || ext == ".sqlite")
                {
                    // проверяем соответствие типов геометрии
                    string tempGeom = _geomType.ToString().ToUpper();
                    if (tempGeom.Contains("LINE") && !tempGeom.Contains("LINESTRING"))
                    {
                        tempGeom = tempGeom.Replace("LINE", "LINESTRING");
                    }
                    if (!tempGeom.Contains("MULTI"))
                        tempGeom = "MULTI" + tempGeom;

                    if (tempGeom != (new SHPWork(new FileInfo(ofd.FileName))).getGeomName())
                    {
                        System.Windows.Forms.MessageBox.Show(Rekod.Properties.Resources.DGBH_NotMatch, Rekod.Properties.Resources.DGBH_ErrorHeader,
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        LoadGeomWithOGR(ofd.FileName);
                    }
                    return;
                }
                else if (System.IO.Path.GetExtension(ofd.FileName).ToUpper() == ".TXT")
                {
                    StreamReader sr = new StreamReader(ofd.OpenFile());
                    try
                    {
                        SelectedGObject.Points.Clear();
                        while (!sr.EndOfStream)
                        {
                            String line = sr.ReadLine();
                            String val1 = line.Split(new char[] { ' ', '\r', '\t' })[0].Replace(",", ".");
                            String val2 = line.Split(new char[] { ' ', '\r', '\t' })[1].Replace(",", ".");

                            Double x = Convert.ToDouble(val1, new CultureInfo("en-US"));
                            Double y = Convert.ToDouble(val2, new CultureInfo("en-US"));
                            SelectedGObject.Points.Add(new GPoint(x, y));
                            if (_geomType == EGeomType.Point || _geomType == EGeomType.MultiPoint)
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show(Rekod.Properties.Resources.DGBH_WrongFormat, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                    sr.Close();
                }
                else if (System.IO.Path.GetExtension(ofd.FileName).ToUpper() == ".JPG" || System.IO.Path.GetExtension(ofd.FileName).ToUpper() == ".JPEG")
                {
                    string wkt = exif.extractexif(ofd.FileName);
                    if (wkt != null)
                    {
                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql = "SELECT st_x(st_geomfromtext('" + wkt + "',4326)), st_y(st_geomfromtext('" + wkt + "',4326))";
                        sqlCmd.Execute(false);
                        if (sqlCmd.CanRead())
                        {
                            SelectedGObject.Points.Clear();
                            SelectedGObject.Points.Add(new GPoint(sqlCmd.GetValue<double>(0), sqlCmd.GetValue<double>(1)));
                            CurrentSrid = 4326;
                        }
                        else
                        {
                            MessageBox.Show(Rekod.Properties.Resources.DGBH_CoordIncorrect, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        }
                        sqlCmd.Close();
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.DGBH_PhotoWithoutCoords, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                }
            }
        }
        private void LoadGeomWithOGR(string FileName)
        {
            SHPWork shpWork = new SHPWork(new FileInfo(FileName));
            SelectedGObject.Points.Clear();
            GeometryObjects.Clear();
            try
            {
                string errString;
                ParseWkt(shpWork.GetWKTofSingleObject(out errString), CurrentSrid);
                if (!errString.Equals(string.Empty))
                    throw new Exception(errString);
                MessageBox.Show(Rekod.Properties.Resources.DGBH_OperSuccess, Properties.Resources.ErrorMessage_header, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
                return;
            }
        }
        /// <summary>
        /// Можно ли импортировать
        /// </summary>
        public bool CanImport(object parameter = null)
        {
            return true;
        }
        #endregion // ImportCommand
        #endregion Команды

        #region Обработчики
        void PgAttributesGeomVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PreviewCurrentEnabled":
                    {
                        if (PreviewCurrentEnabled)
                        {
                            PreviewEnabled = false;
                        }
                        UpdatePreview();
                        break;
                    }
                case "PreviewEnabled":
                    {
                        if (PreviewEnabled)
                        {
                            PreviewCurrentEnabled = false;
                        }
                        UpdatePreview();
                        break;
                    }
                case "SelectedGObject":
                    {
                        UpdatePreview();
                        if (PreviewNodesEnabled)
                        {
                            ShowPoints(SelectedGObject.Points);
                        }
                        break;
                    }
                case "PreviewNodesEnabled":
                    {
                        if (PreviewNodesEnabled)
                        {
                            ShowPoints(SelectedGObject.Points);
                        }
                        else
                        {
                            HidePoints();
                        }
                        break;
                    }
                case "SelectedGPoint":
                    {
                        if (PreviewNodesEnabled)
                        {
                            ShowSinglePoint(SelectedGPoint);
                        }
                        break;
                    }
            }
        }
        void GeometryObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_geomLoaded)
            {
                _hasChanges = true;
            }
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    GObject gObject = item as GObject;
                    if (gObject != null)
                    {
                        gObject.PointsCollectionChanged += GObject_PointsCollectionChanged;
                        foreach (GPoint gPoint in gObject.Points)
                        {
                            gPoint.CoordinatesChanged += GPoint_CoordinatesChanged;
                        }
                        if (gObject.GType == EGeomType.Polygon)
                        {
                            GPolygon gPolygon = gObject as GPolygon;
                            gPolygon.HolesCollectionChanged += GPolygon_HolesCollectionChanged;
                            foreach (GHole gHole in gPolygon.Hollows)
                            {
                                gHole.PointsCollectionChanged += GObject_PointsCollectionChanged;
                                foreach (GPoint gPoint in gHole.Points)
                                {
                                    gPoint.CoordinatesChanged += GPoint_CoordinatesChanged;
                                }
                            }
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    GObject gObject = item as GObject;
                    if (gObject != null)
                    {
                        gObject.PointsCollectionChanged -= GObject_PointsCollectionChanged;
                        foreach (GPoint gPoint in gObject.Points)
                        {
                            gPoint.CoordinatesChanged -= GPoint_CoordinatesChanged;
                        }
                        if (gObject.GType == EGeomType.Polygon)
                        {
                            GPolygon gPolygon = gObject as GPolygon;
                            gPolygon.HolesCollectionChanged -= GPolygon_HolesCollectionChanged;
                            foreach (GHole gHole in gPolygon.Hollows)
                            {
                                gHole.PointsCollectionChanged -= GObject_PointsCollectionChanged;
                                foreach (GPoint gPoint in gHole.Points)
                                {
                                    gPoint.CoordinatesChanged -= GPoint_CoordinatesChanged;
                                }
                            }
                        }
                    }
                }
            }
        }
        void GPolygon_HolesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_geomLoaded)
            {
                _hasChanges = true;
            }
            UpdatePreview();
        }
        void GPoint_CoordinatesChanged(object sender, EventArgs e)
        {
            if (_geomLoaded)
            {
                _hasChanges = true;
            }
            GPoint gPoint = sender as GPoint;
            if (SelectedGObject != null && SelectedGObject.Points.Contains(gPoint))
            {
                if (PreviewNodesEnabled)
                {
                    ShowPoints(SelectedGObject.Points);
                }
                if (PreviewCurrentEnabled)
                {
                    UpdatePreview();
                }
            }
            if (PreviewEnabled)
            {
                UpdatePreview();
            }
        }
        void GObject_PointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_geomLoaded)
            {
                _hasChanges = true;
            }
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    GPoint gPoint = item as GPoint;
                    if (gPoint != null)
                    {
                        gPoint.CoordinatesChanged += GPoint_CoordinatesChanged;
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    GPoint gPoint = item as GPoint;
                    if (gPoint != null)
                    {
                        gPoint.CoordinatesChanged -= GPoint_CoordinatesChanged;
                    }
                }
            }

            GObject gObject = sender as GObject;
            if (SelectedGObject != null && SelectedGObject == gObject)
            {
                if (PreviewNodesEnabled)
                {
                    ShowPoints(SelectedGObject.Points);
                }
                if (PreviewCurrentEnabled)
                {
                    UpdatePreview();
                }
            }
            if (PreviewEnabled)
            {
                UpdatePreview();
            }
        }
        #endregion Обработчики
    }
    [TypeResource("PgGeom.EGeometryControlMode")]
    public enum EGeometryControlMode
    {
        None,
        Edit,
        Add
    }
    [TypeResource("PgGeom.ESridTypes")]
    public enum ESridTypes
    {
        LongLat = 4326,
        WorldMercator = 3395,
        Another = 1
    }
    [TypeResource("PgGeom.ECoordsViewType")]
    public enum ECoordsViewType
    {
        Degrees,
        DegreesMinutes,
        DegreesMinutesSeconds
    }
    public abstract class GObject : ViewModelBase
    {
        #region Cобытия
        public event EventHandler<NotifyCollectionChangedEventArgs> PointsCollectionChanged;
        #endregion События

        #region Свойства
        public abstract EGeomType GType
        {
            get;
        }
        #endregion Свойства

        #region Конструкторы
        public GObject()
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }
        #endregion Конструкторы

        #region Коллекции
        public abstract ObservableCollection<GPoint> Points
        {
            get;
        }
        #endregion Коллекции

        #region Обработчики
        void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (PointsCollectionChanged != null)
            {
                PointsCollectionChanged(this, e);
            }
        }
        #endregion Обработчики
    }
    public class GPoint : GObject
    {
        #region Cобытия
        public event EventHandler<EventArgs> CoordinatesChanged;
        #endregion Cобытия

        #region Поля
        private double _x;
        private double _y;
        private ObservableCollection<GPoint> _points;
        private static int _count = 1;
        #endregion Поля

        #region Конструкторы
        public GPoint(double x, double y)
        {
            _x = x;
            _y = y;
            PropertyChanged += GPoint_PropertyChanged;
        }
        public GPoint()
        {
            PropertyChanged += GPoint_PropertyChanged;
        }
        #endregion Конструкторы

        #region Свойства
        public Int32 Count
        {
            get
            {
                return _count++;
            }
        }
        public Double X
        {
            get
            {
                return _x;
            }
            set
            {
                OnPropertyChanged(ref _x, value, () => this.X);
            }
        }
        public Double Y
        {
            get
            {
                return _y;
            }
            set
            {
                OnPropertyChanged(ref _y, value, () => this.Y);
            }
        }
        public override EGeomType GType
        {
            get { return EGeomType.Point; }
        }
        public String SomeText
        {
            get;
            set;
        }
        #endregion Свойства

        #region Коллекции
        public override ObservableCollection<GPoint> Points
        {
            get
            {
                return _points ?? (_points = new ObservableCollection<GPoint>() { this });
            }
        }
        #endregion Коллекции

        #region Методы
        public void Invert()
        {
            double temp = this.X;
            X = Y;
            Y = temp;
        }
        #endregion Методы

        #region Обработчики
        void GPoint_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                if (CoordinatesChanged != null)
                {
                    CoordinatesChanged(sender, e);
                }
            }
        }
        #endregion Обработчики
    }
    public class GHole : GObject
    {
        #region Поля
        private ObservableCollection<GPoint> _points;
        private GPolygon _parent;
        #endregion Поля

        #region Конструкторы
        public GHole(GPolygon parent)
        {
            _parent = parent;
        }
        #endregion Конструкторы

        #region Коллекции
        public override ObservableCollection<GPoint> Points
        {
            get
            {
                return _points ?? (_points = new ObservableCollection<GPoint>());
            }
        }
        #endregion Коллекции

        #region Свойства
        public override EGeomType GType
        {
            get { return EGeomType.Hole; }
        }
        public GPolygon Parent
        {
            get { return _parent; }
            private set { _parent = value; }
        }
        #endregion Свойства
    }
    public class GLine : GObject
    {
        #region Поля
        private ObservableCollection<GPoint> _points;
        #endregion Поля

        #region Конструкторы
        public GLine()
        { }
        #endregion Конструкторы

        #region Коллекции
        public override ObservableCollection<GPoint> Points
        {
            get
            {
                return _points ?? (_points = new ObservableCollection<GPoint>());
            }
        }
        #endregion Коллекции

        #region Свойства
        public override EGeomType GType
        {
            get { return EGeomType.Line; }
        }
        #endregion Свойства
    }
    public class GPolygon : GObject
    {
        #region Cобытия
        public event EventHandler<NotifyCollectionChangedEventArgs> HolesCollectionChanged;
        #endregion События

        #region Поля
        private ObservableCollection<GPoint> _outerPoints;
        private ObservableCollection<GHole> _innerPoints;
        #endregion Поля

        #region Конструкторы
        public GPolygon()
        {
            Name = "Polygon";
            Hollows.CollectionChanged += Holes_CollectionChanged;
        }
        #endregion Конструкторы

        #region Коллекции
        public override ObservableCollection<GPoint> Points
        {
            get
            {
                return _outerPoints ?? (_outerPoints = new ObservableCollection<GPoint>());
            }
        }
        public ObservableCollection<GHole> Hollows
        {
            get
            {
                return _innerPoints ?? (_innerPoints = new ObservableCollection<GHole>());
            }
        }
        #endregion Коллекции

        #region Свойства
        public String Name
        {
            get;
            set;
        }
        public override EGeomType GType
        {
            get { return EGeomType.Polygon; }
        }
        #endregion Свойства

        #region Обработчики
        void Holes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (HolesCollectionChanged != null)
            {
                HolesCollectionChanged(this, e);
            }
        }
        #endregion Обработчики
    }
    public class GeomCharacteristic
    {
        #region Конструкторы
        public GeomCharacteristic(String charname, Object value, String unit)
        {
            CharacteristicName = charname;
            Value = value;
            MeasureUnit = unit;
        }
        #endregion Конструкторы

        #region Свойства
        public String CharacteristicName
        {
            get;
            private set;
        }
        public Object Value
        {
            get;
            private set;
        }
        public String MeasureUnit
        {
            get;
            private set;
        }
        #endregion Свойства
    }
}