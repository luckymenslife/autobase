using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using mvMapLib;
using Rekod.DataAccess.SourcePostgres.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using CosVM = Rekod.DataAccess.SourceCosmetic.ViewModel;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public class CosmeticTableBaseM : AbsM.TableBaseM
    {
        #region Статические поля
        private static int cosmeticLayerId = 1;
        private static int cosmeticLayerObjectId = 1;
        #endregion Статические поля

        #region Поля
        private PgStyleLayerM _defaultStyle;
        private PgStyleLableM _labelStyle;
        private CosmeticFieldM _primaryKeyField;
        private ObservableCollection<CosmeticObjectM> _objects;
        private AxmvMapLib.AxMapLIb _axMapLib;
        private int? _defaultDotStyle;
        private int? _defaultPolygonStyle;
        private int? _defaultLineStyle;
        private bool _showLabels = true;
        #endregion Поля

        #region Свойства
        public int NextObjectId
        {
            get { return cosmeticLayerObjectId++; }
        }
        public int? DefaultDotStyle
        {
            get { return _defaultDotStyle; }
        }
        public int? DefaultLineStyle
        {
            get { return _defaultLineStyle; }
        }
        public int? DefaultPolygonStyle
        {
            get { return _defaultPolygonStyle; }
        }     
        public int Id
        {
            get { return _id != null ? (int)_id : -1; }
        }
        public CosmeticFieldM PrimaryKeyField
        {
            get { return _primaryKeyField; }
            set { OnPropertyChanged(ref _primaryKeyField, value, () => PrimaryKeyField); ; }
        }
        public string GeomFieldName { get { return "geom"; } }
        public string PkFieldName { get { return "id"; } }
        /// <summary>
        /// Стиль по умолчанию для слоя
        /// </summary>
        public PgStyleLayerM DefaultStyle
        {
            get { return _defaultStyle; }
            set { OnPropertyChanged(ref _defaultStyle, value, () => DefaultStyle); }
        }
        /// <summary>
        /// Стиль по умолчанию для подписи слоя
        /// </summary>
        public PgStyleLableM LabelStyle
        {
            get { return _labelStyle; }
            set { OnPropertyChanged(ref _labelStyle, value, () => LabelStyle); }
        }
        public ObservableCollection<CosmeticObjectM> Objects
        {
            get { return _objects;  }
            set { _objects = value; }
        }
        public bool ShowLabels
        {
            get { return _showLabels; }
            set { OnPropertyChanged(ref _showLabels, value, () => ShowLabels); }
        } 
        #endregion Свойства

        #region Конструкторы
        public CosmeticTableBaseM(AbsM.IDataRepositoryM source, int srid)
            : base(source, srid, AbsM.ETableType.MapLayer)
        {
            _axMapLib = (Source as AbsVM.DataRepositoryVM).MapViewer;

            _id = CosmeticTableBaseM.cosmeticLayerId++;
            _objects = new ObservableCollection<CosmeticObjectM>();

            PrimaryKeyField = new CosmeticFieldM(this, PkFieldName, "id", AbsM.EFieldType.Integer, false);

            Fields.Add(PrimaryKeyField);
            Fields.Add(new CosmeticFieldM(this, "label", Rekod.Properties.Resources.CosTableView_Label, AbsM.EFieldType.Text, false));
            Fields.Add(new CosmeticFieldM(this, GeomFieldName, "Геометрия", AbsM.EFieldType.Geometry, false));

            DefaultStyle = new PgStyleLayerM(CosmeticStyleType.All);
            LabelStyle = new PgStyleLableM();

            PropertyChanged += CosmeticTableBaseM_PropertyChanged;
        }
        #endregion Конструкторы

        #region Методы
        public void SetDefaultStyle(mvSymbolObject symbolStyle, mvFontObject fontStyle, mvPenObject penStyle, mvBrushObject brushStyle)
        {
            var layer = Program.mainFrm1.axMapLIb1.getLayer(Name);
            if (_defaultDotStyle == null || _defaultLineStyle == null || _defaultPolygonStyle == null)
            {
                _defaultDotStyle = layer.CreateDotStyle(symbolStyle, fontStyle);
                _defaultLineStyle = layer.CreateLineStyle(penStyle);
                _defaultPolygonStyle = layer.CreatePolygonStyle(penStyle, brushStyle);
            }
            else 
            {
                layer.editStyle(_defaultDotStyle.Value, null, null, symbolStyle, fontStyle);
                layer.editStyle(_defaultLineStyle.Value, penStyle, null, null, null);
                layer.editStyle(_defaultPolygonStyle.Value, penStyle, brushStyle, null, null);
            }
        }
        /// <summary>
        /// Найти объект по ID
        /// </summary>
        /// <param name="id">ID объкта</param>
        /// <returns>Возвращает null, если объект не найден</returns>
        public CosmeticObjectM GetObject(int id)
        {
            return Objects.FirstOrDefault(w => w.Id == id);
        }

        public static void SetIsNewTable(CosmeticTableBaseM layer, bool isNew)
        {
            layer._isNewTable = isNew;
        }

        /// <summary>
        /// Удалить объект из списка объектов
        /// </summary>
        /// <param name="id"></param>
        public bool DeleteObject(int id)
        {
            var item = GetObject(id);
            return item != null && Objects.Remove(item);
        }

        /// <summary>
        /// Создает объект в списке
        /// Если объект существует на карте, то его значение обновляется, иначе объект создается
        /// </summary>
        /// <param name="label">Значение подписи</param>
        /// <param name="wkt">Геометрия</param>
        /// <param name="mapObj">Объект на карте</param>
        /// <returns>Созданный объект</returns>
        public CosmeticObjectM CreateObject(string label, string wkt, mvVectorObject mapObj = null)
        {
            var tableObj = new CosmeticObjectM(NextObjectId, wkt);
            tableObj.SetAttribute("label", label);
            Objects.Add(tableObj);

            var layerMV = (Source as SourceCosmetic.ViewModel.CosmeticDataRepositoryVM).MapViewer.getLayer(NameMap);
            if (layerMV != null && tableObj != null)
            {
                if (mapObj == null)
                {
                    mapObj = layerMV.getObject(tableObj.Id);
                    if (mapObj == null)
                        mapObj = layerMV.CreateObject();
                }

                if (mapObj != null)
                {
                    mapObj.setWKT(wkt);
                    mapObj.SetAttribute("label", label);
                    mapObj.SetAttribute(PkFieldName, tableObj.Id.ToString());
                }
            }

            return tableObj;
        }

        /// <summary>
        /// Обновляет геометрию объекта в списке и на карте
        /// </summary>
        public CosmeticObjectM UpdateObject(int id, string wkt)
        {
            var tableObj = GetObject(id);
            if (tableObj != null)
            {
                tableObj.WKT = wkt;

                var layerMV = (Source as SourceCosmetic.ViewModel.CosmeticDataRepositoryVM).MapViewer.getLayer(NameMap);
                if (layerMV != null)
                {
                    var mapObj = layerMV.getObject(id);
                    if (mapObj != null)
                    {
                        mapObj.setWKT(wkt);
                    }
                }
            }

            return tableObj;
        }
        #endregion Методы

        #region Обработчики
        void CosmeticTableBaseM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowLabels")
            {
                mvLayer mvLayer = _axMapLib.getLayer(NameMap);
                if (mvLayer != null)
                {
                    mvLayer.showlabels = ShowLabels;
                }
            }
        }
        #endregion Обработчики
    }

    public enum CosmeticStyleType
    {
        All = 0,
        Point = 1,
        Line = 2,
        Polygon = 3
    }
}