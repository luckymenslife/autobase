using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Interfaces;
using RESTLib;
using RESTLib.Enums;
using RESTLib.GML2;
using RESTLib.Model.REST;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Model.WFS;
using RESTLib.Queries;

namespace sscSync.Controller
{
    public class SSCData
    {
        private User _user;
        private Layers _layers;
        private List<Group> _groups_from_layers = new List<Group>();
        private List<DepUser> _departmentUsers;

        public bool IsAdmin
        {
            get { return _user.Role == RESTUserRoles.DepartamentAdmin || _user.Role == RESTUserRoles.Administrator; }
        }

        public SSCData(User user)
        {
            _user = user;
            _layers = new Layers(user);
            
            if (_user.Role == RESTUserRoles.DepartamentAdmin)
            {
                try
                {
                    Users users = new Users(_user);
                    if (users != null)
                    {
                        _departmentUsers = users.GetDepUsers(_user.DepartamentId.ToString());
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Получение всех слоев из SSC
        /// </summary>
        /// <returns></returns>
        public List<Layer> GetSSCLayers(IOpenForms openForms)
        {
            _groups_from_layers.Clear();
            var layers = _layers.GetLayers();
            int count = 0;
            foreach (var layer in layers)
            {
                if (!String.IsNullOrEmpty(layer.lname)) continue;
                try
                {
                    if (_groups_from_layers.FirstOrDefault(w => w.id.ToString() == layer.Group_id) == null)
                    {
                        _groups_from_layers.Add(new Group() { id = Convert.ToInt32(layer.Group_id), name = layer.Group_name });
                    }
                    string[] test = layer.Type_name.Split(':');
                    //var detailedLayer = _layers.GetLayer(layer.Layer_id);
                    layer.lname = test[1];
                    openForms.SetText(String.Format("Загружено {0} из {1}", ++count, layers.Count));
                }
                catch { }
            }
            return layers;
        }
        public List<Group> GroupFromLayer
        {
            get { return this._groups_from_layers; }
        }
        public List<Group> GetGroups()
        {
            return _layers.GetGroups();
        }

        public StandartDatastore GetStandartDatastore()
        {
            Users users = new Users(_user);
            return users.GetStandartDatastore();
        }

        /// <summary>
        /// Публикация слоя
        /// </summary>
        /// <param name="tableInfo">Информация о слое</param>
        /// <param name="style">Стиль слоя</param>
        /// <param name="groupsNames">Группы, в которых содержится слой</param>
        /// <returns>Результат операции</returns>
        public bool RegisterTable(tablesInfo tableInfo, LayerStyle style, Group group)
        {
            if (style == null)
                throw new Exception("Не указан стиль слоя");

            // get attributes
            if (tableInfo.ListField == null || tableInfo.ListField.Count == 0)
                throw new Exception("Невозможно создать слой без атрибутов");

            List<LayerAttribute> attributes = new List<LayerAttribute>();
            foreach(var field in tableInfo.ListField)
            {
                if (field.nameDB != tableInfo.pkField && field.nameDB != tableInfo.geomFieldName)
                {
                    attributes.Add(new LayerAttribute()
                    {
                        Name = field.nameDB,
                        Name_ru = field.nameMap,
                        Title = (tableInfo.lableFieldName == field.nameDB) ? 't' : 'f'
                    });
                }
            }

            //type
            GeomType geomType = GeomType.UNKNOWN;
            switch (tableInfo.TypeGeom)
            {
                case TypeGeometry.MULTIPOINT:
                    geomType = GeomType.POINT;
                    break;
                case TypeGeometry.MULTILINESTRING:
                    geomType = GeomType.LINESTRING;
                    break;
                case TypeGeometry.MULTIPOLYGON:
                    geomType = GeomType.POLYGON;
                    break;
            }

            if (geomType == GeomType.UNKNOWN)
                throw new Exception("Геометрия слоя неизвестна");

            int layerId = -1;
            if (IsAdmin)
            {
                var standart_ds = GetStandartDatastore();
                if (standart_ds == null)
                    throw new Exception("Standart datastore not found");
                else
                {
                    // creating layer
                    layerId = _layers.PublishLayer(
                        standart_ds.id,
                        tableInfo.nameDB,
                        geomType,
                        style,
                        attributes,
                        tableInfo.nameMap,
                        group.id,
                        LayerType.WMS);
                }
            }
            else
            {
                // creating layer
                layerId = _layers.PublishLayer(
                    tableInfo.nameDB,
                    geomType,
                    style,
                    attributes,
                    tableInfo.nameMap,
                    group.id,
                    LayerType.WMS);
            }
            
            return layerId > 0;
        }
        
        /// <summary>
        /// Перезагрузить слой
        /// </summary>
        /// <param name="layer">Слой</param>
        public void ReloadTable(Layer layer)
        {
            if (layer == null)
                throw new Exception("Не указан слой");

            _layers.ReloadLayer(layer.Layer_id);
        }
        public bool DeleteLayer(Layer layer, bool deleteFromDB)
        {
            try
            {
                this._layers.DeleteLayer(layer.Layer_id, true, true, deleteFromDB);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return true;
        }
        /// <summary>
        /// Перезагрузить стиль слоя
        /// </summary>
        /// <param name="layer">Слой</param>
        public void ReloadStyle(Layer layer)
        {
            if (layer == null)
                throw new Exception("Не указан слой");

            _layers.ReloadLayerStyle(layer.Layer_id);
        }

        /// <summary>
        /// Создать слой
        /// </summary>
        /// <param name="nameEng">Английское имя</param>
        /// <param name="nameRus">Системное имя слоя</param>
        /// <param name="geomType">Тип геометрии</param>
        /// <param name="attributes">Список атрибутов</param>
        /// <param name="group">Группа</param>
        /// <param name="style">Стиль</param>
        /// <returns>Результат операции</returns>
        public bool CreateLayer(string nameEng, string nameRus, GeomType geomType, 
            List<LayerAttribute> attributes, Group group, LayerStyle style)
        {
            if (_user.IsAdminAllDepartaments)
                throw new Exception("Для данного типа пользователей операция не поддерживается");

            if (style == null)
                throw new Exception("Не указан стиль слоя");

            if (group == null)
                throw new Exception("Не указана группа слоя");

            if (geomType == GeomType._point || geomType == GeomType.UNKNOWN)
                throw new Exception("Не указан тип слоя");

            if (attributes == null)
                attributes = new List<LayerAttribute>();

            int layerId = -1;

            if (IsAdmin)
            {
                var standart_ds = GetStandartDatastore();
                if (standart_ds == null)
                    throw new Exception("Standart datastore not found");
                else
                {
                    // creating layer
                    layerId = _layers.CreateLayer(
                        standart_ds.id,
                        nameEng,
                        geomType,
                        style,
                        attributes,
                        group.id,
                        nameRus,
                        LayerType.WMS);
                }
            }
            else
            {
                // creating layer
                layerId = _layers.CreateLayer(
                    geomType,
                    style,
                    attributes,
                    group.id,
                    nameRus,
                    LayerType.WMS);
            }

            return layerId > 0;
        }

        /// <summary>
        /// Стиль по умолчанию
        /// </summary>
        /// <param name="geomType">Тип геометрии</param>
        public LayerStyle getDefaultStyle(TypeGeometry geomType)
        {
            var lStyle = getDefaultStyle();
            switch (geomType)
            {
                case TypeGeometry.MULTIPOINT:
                    lStyle.Type = RESTStyles.Point;
                    break;
                case TypeGeometry.MULTILINESTRING:
                    lStyle.Type = RESTStyles.Line;
                    break;
                case TypeGeometry.MULTIPOLYGON:
                    lStyle.Type = RESTStyles.Polygon;
                    break;
            }
            return lStyle.RESTLayerStyle;
        }

        public LStyle getDefaultStyle()
        {
            LStyle style = new LStyle();
            style.FillColor = Color.FromRgb(0, 0, 255);
            style.HasStroke = true;
            style.LineWidth = 1.0;
            style.PointFigure = Figure.Круг;
            style.PointSize = 12;
            style.PolygonOpacity = 1;
            style.StrokeColor = Color.FromRgb(0, 0, 255);
            style.StrokeWidth = 1;

            return style;
        }

        /// <summary>
        /// Находится ли данный пользователь в департаменте текущего пользователя
        /// </summary>
        /// <param name="idUser">ID пользователя</param>
        public bool IsInCurrentDepartment(string idUser)
        {
            if (_departmentUsers != null)
            {
                return _departmentUsers.Any(w => w.id == idUser);
            }
            return false;
        }
    }
}
