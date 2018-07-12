using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using mvMapLib;
using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using CosVM = Rekod.DataAccess.SourceCosmetic.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;

namespace Rekod.DataAccess.SourceCosmetic.ViewModel.CosmeticAttributes
{
    public class CosmeticAttributesVM : WindowViewModelBase_VM
    {
        #region Поля
        private readonly bool _isDebug = false;

        private readonly CosmeticAttributesListVM _attributesListVM;
        private CosM.CosmeticTableBaseM _table;
        private CosM.CosmeticObjectM _tableObject;

        private PgM.PgStyleObjectM _styleVM;
        private PgVM.PgAttributes.PgAttributesGeomVM _pgGeometryVM;
        private CosmeticDataRepositoryVM _source;

        private ICommand _saveCommand;
        private ICommand _reloadCommand;
        private ICommand _openHistoryCommand;
        private ICommand _openTableCommand;
        private bool _hasOwnStyle;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Ссылка на таблицу
        /// </summary>
        public CosM.CosmeticTableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// Ссылка на источник
        /// </summary>
        public CosmeticDataRepositoryVM Source
        {
            get { return _source; }
        }
        public bool IsDebug
        {
            get { return _isDebug; }
        }
        /// <summary>
        /// ViewModel списка атрибутов объекта
        /// </summary>
        public CosmeticAttributesListVM AttributesListVM
        {
            get { return _attributesListVM; }
        }
        /// <summary>
        /// ViewModel стиля объекта
        /// </summary>
        public PgM.PgStyleObjectM StyleVM
        {
            get { return _styleVM; }
        }
        /// <summary>
        /// ViewModel геометрии объекта
        /// </summary>
        public PgVM.PgAttributes.PgAttributesGeomVM PgGeometryVM
        {
            get { return _pgGeometryVM; }
        }
        public CosM.CosmeticObjectM TableObject
        {
            get { return _tableObject; }
        }
        public Boolean HasOwnStyle
        {
            get { return _hasOwnStyle; }
            set { OnPropertyChanged(ref _hasOwnStyle, value, () => this.HasOwnStyle); }
        }
        #endregion Свойства

        #region Коллекции
        #endregion Коллекции

        #region Конструктор
        public CosmeticAttributesVM(AbsM.ITableBaseM table, int? obj, bool isReadOnly = false, string wkt = null, bool isDebug = false)
        {
            _table = table as CosM.CosmeticTableBaseM;
            if (_table == null)
                throw new ArgumentNullException(Rekod.Properties.Resources.CosAttributes_TableIsNull);
            else if (_table.PrimaryKeyField == null)
                throw new ArgumentNullException(Rekod.Properties.Resources.CosAttributes_TableWithoutPK);
            if (obj == null)
                throw new ArgumentNullException(Rekod.Properties.Resources.CosAttributes_MvObjectIsNull);

            _isDebug = isDebug;
            _source = _table.Source as CosVM.CosmeticDataRepositoryVM;

            _tableObject = GetOrCreateTableObject(obj);

            _attributesListVM = new CosmeticAttributesListVM(this);

            if (_table.Type == AbsM.ETableType.MapLayer || _table.Type == AbsM.ETableType.View)
            {
                string curWKT = string.Empty;
                if (string.IsNullOrWhiteSpace(wkt))
                {
                    var layerMV = Source.MapViewer.getLayer(_table.NameMap);
                    if (layerMV != null)
                    {
                        var objMV = layerMV.getObject(_tableObject.Id);
                        if (objMV != null)
                        {
                            curWKT = objMV.getWKT();
                        }
                    }
                }
                else
                    curWKT = wkt;
                _pgGeometryVM = new PgVM.PgAttributes.PgAttributesGeomVM(_attributesListVM, curWKT, (int?)_attributesListVM.PkAttribute.Value);
            }

            _styleVM = GetStyle();
            Title = String.Format("{0}: [id: {3}]; {1}: \"{4}\"; {2}: \"{5}\"",
                                    Rekod.Properties.Resources.CosAttributes_Object,
                                    Rekod.Properties.Resources.CosAttributes_Table,
                                    Rekod.Properties.Resources.CosAttributes_Type, 
                                   _tableObject != null ? _tableObject.Id.ToString() : "null",
                                   _table.Text,
                                   _table.Source.Type);
        }
        #endregion Конструктор

        #region Методы
        /// <summary>
        /// Получить сохраненный стиль объекта или экземпляр стиля по умолчанию
        /// </summary>
        /// <returns></returns>
        private PgM.PgStyleObjectM GetStyle()
        {
            PgM.PgStyleObjectM result = null;                      
            CosM.CosmeticStyleType type = CosM.CosmeticStyleType.All;
            if (_pgGeometryVM != null && !String.IsNullOrEmpty(_pgGeometryVM.GeometryTypeName))
            {
                switch (_pgGeometryVM.GeometryTypeName.ToUpper())
                {
                    case "POINT":
                    case "MULTIPOINT":
                        type = CosM.CosmeticStyleType.Point;
                        break;
                    case "LINE":
                    case "MULTILINE":
                    case "LINESTRING":
                    case "MULTILINESTRING":
                        type = CosM.CosmeticStyleType.Line;
                        break;
                    case "POLYGON":
                    case "MULTIPOLYGON":
                        type = CosM.CosmeticStyleType.Polygon;
                        break;
                }
                var layer = _source.MapViewer.getLayer(Table.Name);
                var layerObj = layer.getObject(TableObject.Id);
                if (layerObj != null)
                {
                    int objStyleId = layerObj.style;
                    if (Table.DefaultDotStyle == objStyleId || Table.DefaultLineStyle == objStyleId || Table.DefaultPolygonStyle == objStyleId)
                    {
                        result = Table.DefaultStyle.GetStyleInstance(type);
                    }
                    else
                    {
                        HasOwnStyle = true;
                        mvLayerStyle mvStyle = layer.getStyle(objStyleId);
                        result = new PgM.PgStyleObjectM(type) { FontName = "Map Symbols" };
                        if (mvStyle.Brush != null)
                        {
                            result.BrushBgColor = (int)mvStyle.Brush.bgcolor;
                            result.BrushFgColor = (int)mvStyle.Brush.fgcolor;
                            result.BrushHatch = mvStyle.Brush.hatch;
                            result.BrushStyle = mvStyle.Brush.style;
                            if (mvStyle.Brush.hatch == 0 && mvStyle.Brush.style == 0)
                            {
                                var tt = (mvStyle.Brush.bgcolor >> 24);
                                result.Opacity = (mvStyle.Brush.bgcolor % 256) / 256d;
                            }
                            if (mvStyle.Brush.bgcolor == 4294967295)
                            {
                                result.HasBackground = false;
                            }
                            else
                            {
                                result.HasBackground = true;
                            }
                        }
                        if (mvStyle.Font != null)
                        {
                            result.FontColor = (int)mvStyle.Font.Color;
                            result.FontFrameColor = (int)mvStyle.Font.framecolor;
                            result.FontName = mvStyle.Font.fontname;
                            result.FontSize = mvStyle.Font.size;
                        }
                        if (mvStyle.Pen != null)
                        {
                            result.PenColor = (int)mvStyle.Pen.Color;
                            result.PenType = (int)mvStyle.Pen.ctype;
                            result.PenWidth = (int)mvStyle.Pen.width;
                        }
                        if (mvStyle.Symbol != null)
                        {
                            result.Symbol = (int)mvStyle.Symbol.shape;
                        }
                    }
                }
            }
            return result;            
        }
        /// <summary>
        /// Сохранить стиль объекта и применить к объекту
        /// </summary>
        private void SaveStyle()
        {
            if (TableObject != null)
                TableObject.Style = StyleVM;
            var layer = _source.MapViewer.getLayer(Table.Name);
            var layerObj = layer.getObject(TableObject.Id);
            if (layer != null)
            {
                if (HasOwnStyle)
                {
                    int ownStyleNum = layerObj.style;
                    bool ownStyleIsDefault =
                        ownStyleNum == Table.DefaultDotStyle.Value
                        || ownStyleNum == Table.DefaultLineStyle.Value
                        || ownStyleNum == Table.DefaultPolygonStyle.Value;
                    switch (StyleVM.CosmeticStyleType)
                    {
                        case CosM.CosmeticStyleType.Point:
                            mvSymbolObject symbol = new mvSymbolObject() { shape = (uint)StyleVM.Symbol };
                            mvFontObject font = new mvFontObject()
                            {
                                fontname = StyleVM.FontName,
                                size = StyleVM.FontSize,
                                Color = (uint)StyleVM.FontColor,
                                framecolor = (uint)StyleVM.FontFrameColor
                            };
                            if (ownStyleIsDefault)
                            {
                                layerObj.style = layer.CreateDotStyle(symbol, font);
                            }
                            else
                            {
                                layer.editStyle(ownStyleNum, null, null, symbol, font);
                            }                            
                            break;
                        case CosM.CosmeticStyleType.Line:
                            mvPenObject pen_line = new mvPenObject()
                            {
                                Color = (uint)StyleVM.PenColor,
                                ctype = (ushort)StyleVM.PenType,
                                width = (uint)StyleVM.PenWidth
                            };
                            if (ownStyleIsDefault)
                            {
                                layerObj.style = layer.CreateLineStyle(pen_line);
                            }
                            else
                            {
                                layer.editStyle(ownStyleNum, pen_line, null, null, null);
                            }
                            break;
                        case CosM.CosmeticStyleType.Polygon:
                            mvPenObject stroke = new mvPenObject()
                            {
                                Color = (uint)StyleVM.PenColor,
                                ctype = (ushort)StyleVM.PenType,
                                width = (uint)StyleVM.PenWidth
                            };
                            mvBrushObject brush = new mvBrushObject()
                            {
                                bgcolor = (uint)StyleVM.BrushBgColor,
                                fgcolor = (uint)StyleVM.BrushFgColor,
                                style = (ushort)StyleVM.BrushStyle,
                                hatch = (ushort)StyleVM.BrushHatch
                            };

                            if (StyleVM.BrushStyle == 0 && StyleVM.BrushHatch == 0)
                            {
                                int grey = Convert.ToInt32(255 & (int)(StyleVM.Opacity * 255));
                                brush.bgcolor = Convert.ToUInt32(grey + (grey << 8) + (grey << 16));
                            }
                            else if (!StyleVM.HasBackground)
                            {
                                brush.bgcolor = 0xffffffff;
                            }

                            if (ownStyleIsDefault)
                            {
                                layerObj.style = layer.CreatePolygonStyle(stroke, brush);
                            }
                            else
                            {
                                layer.editStyle(ownStyleNum, stroke, brush, null, null);
                            }
                            break;
                    }
                }
                else
                {
                    switch (StyleVM.CosmeticStyleType)
                    {
                        case CosM.CosmeticStyleType.Point:
                            layerObj.style = Table.DefaultDotStyle.Value; 
                            break;
                        case CosM.CosmeticStyleType.Line:
                            layerObj.style = Table.DefaultLineStyle.Value; 
                            break;
                        case CosM.CosmeticStyleType.Polygon:
                            layerObj.style = Table.DefaultPolygonStyle.Value; 
                            break;
                    }
                }
                layer.ExternalFullReloadVisible();
            }
        }
        /// <summary>
        /// Создать объект, если его не существует
        /// </summary>
        private CosM.CosmeticObjectM GetOrCreateTableObject(int? obj)
        {
            CosM.CosmeticObjectM tableObject = null;

            var layerMV = Source.MapViewer.getLayer(_table.NameMap);
            if (layerMV == null)
                return null;

            int id = obj.Value;

            string wkt = string.Empty;
            var objMV = layerMV.getObject(id);
            if (objMV != null)
                wkt = objMV.getWKT();

            tableObject = _table.GetObject(id);
            if (tableObject == null)
                tableObject = new CosM.CosmeticObjectM(id, wkt);

            if (objMV != null)
            {
                foreach (var field in _table.Fields)
                {
                    if (field.Name != _table.GeomFieldName
                        && field.Name != _table.PkFieldName)
                    {
                        tableObject.SetAttribute(field.Name, (string)objMV.fieldValue(field.Name));
                    }
                }
            }

            return tableObject;
        }
        #endregion Методы

        #region Команды
        #region Command: SaveCommand
        /// <summary>
        /// Сохраняет атрибуты объекта
        /// </summary>
        public ICommand SaveCommand
        {
            get
            { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        void Save(object param = null)
        {
            if (!CanSave(param))
                return;

            var layerMV = _source.MapViewer.getLayer(_table.NameMap);
            if (layerMV == null)
                return;
            var objMV = layerMV.getObject(TableObject.Id);
            if (objMV == null)
                return;

            if (PgGeometryVM != null && PgGeometryVM.CanSaveGeometry())
            {
                AttributesListVM.SaveCommand.Execute(param);
                var wkt = PgGeometryVM.GetWkt();
                wkt = PgGeometryVM.TransformWkt(wkt, PgGeometryVM.CurrentSrid, Convert.ToInt32(_source.MapViewer.SRID));

                objMV.setWKT(wkt);

                TableObject.WKT = wkt;

                base.CloseWindow();
            }
            else if (PgGeometryVM != null)
            {
                var wkt = PgGeometryVM.GetWkt();
                objMV.setWKT(wkt);
                TableObject.WKT = wkt;
            }
            if (PgGeometryVM == null)
            {
                AttributesListVM.SaveCommand.Execute(param);
                base.CloseWindow();
            }

            layerMV.RemoveDeletedObjects();
            SaveStyle();
            _source.MapViewer.mapRepaint();
        }
        bool CanSave(object param = null)
        {
            return true;
        }
        #endregion

        #region Command: ReloadCommand
        /// <summary>
        /// Сохраненяет атрибуты объекта
        /// </summary>
        public ICommand ReloadCommand
        {
            get
            { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload)); }
        }
        public void Reload(object param = null)
        {
            AttributesListVM.ReloadCommand.Execute(param);
        }
        #endregion

        #region Command: OpenTableCommand
        public ICommand OpenTableCommand
        {
            get { return _openTableCommand ?? (_openTableCommand = new RelayCommand(this.OpenTable)); }
        }
        private void OpenTable(object obj = null)
        {
            _source.OpenTable(Table, AttributesListVM.PkAttribute.Value);
        }
        #endregion
        #endregion Команды

        #region Обработчики
        protected override bool Closing(object obj)
        {
            if (PgGeometryVM != null)
            {
                PgGeometryVM.HidePoints();
                PgGeometryVM.HidePreview();
            }
            base.Closing(obj);
            return true;
        }
        #endregion Обработчики
    }
}