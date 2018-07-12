using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Interfaces;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Model.WFS;
using sscSync.ViewModel;

namespace sscSync.Controller
{
    /// <summary>
    /// Класс для работы с бд PostgreSql
    /// </summary>
    public class PgData
    {
        private IMainApp app;

        public PgData(IMainApp app)
        {
            this.app = app;
        }

        /// <summary>
        /// Получение всех слоев инфраструктуры
        /// </summary>
        /// <returns></returns>
        public List<TableRight> GetPgLayers()
        {
            app.reloadInfo();
            IEnumerable<TableRight> tables = from layer in app.getTableOfType(1)
                                             select new TableRight(layer, true);
            List<TableRight> allTables = tables.ToList();
            allTables.AddRange(getOtherTables());

            var changedTables = getChangedTables();
            foreach (var id_table in changedTables)
            {
                var table = allTables.FirstOrDefault(w => w.Table.idTable == id_table);
                if (table != null)
                    table.WasChanged = true;
            }

            return allTables;
        }

        private List<TableRight> getOtherTables()
        {
            List<TableRight> tables = new List<TableRight>();
            using (var sqlCmd = app.SqlWork())
            {
                sqlCmd.sql = String.Format(@"
SELECT id, scheme_name, name_db, name_map, geom_field, geom_type, 
       (SELECT read_data FROM {0}.get_user_right({1}, id)) as can_read 
  FROM {0}.table_info
  WHERE type = 1;
", app.scheme, app.user_info.id_user);

                sqlCmd.ExecuteReader();

                while (sqlCmd.CanRead())
                {
                    bool? right = sqlCmd.GetBoolean("can_read");
                    if (right != true)
                    {
                        tables.Add(new TableRight(new tablesInfo()
                        {
                            idTable = sqlCmd.GetInt32("id"),
                            nameSheme = sqlCmd.GetString("scheme_name"),
                            nameDB = sqlCmd.GetString("name_db"),
                            nameMap = sqlCmd.GetString("name_db"),
                            geomFieldName = sqlCmd.GetString("geom_field"),
                            TypeGeom = (TypeGeometry)sqlCmd.GetInt32("geom_type")
                        }, false));
                    }
                }
            }
            return tables;
        }

        private List<int> getChangedTables()
        {
            List<int> changesTables = new List<int>();
            using (var sqlCmd = app.SqlWork())
            {
                sqlCmd.sql = String.Format(@"
SELECT DISTINCT id_table
  FROM {0}.table_events;", app.scheme);
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    changesTables.Add(sqlCmd.GetInt32("id_table"));
                }
            }
            return changesTables;
        }

        /// <summary>
        /// Получение стиля таблицы
        /// </summary>
        /// <param name="tableInfo">Информация о таблице</param>
        /// <returns>Стиль таблицы</returns>
        public LStyle GetStyleForTable(tablesInfo tableInfo)
        {
            LStyle style = null;
            using (var sqlCmd = app.SqlWork())
            {
                sqlCmd.sql = String.Format(@"
SELECT default_style, pencolor, penwidth, brushbgcolor, brushfgcolor, brushstyle, fontcolor, fontframecolor, fontsize
FROM {0}.table_info
WHERE id = {1}", app.scheme, tableInfo.idTable);
                sqlCmd.ExecuteReader();

                if (sqlCmd.CanRead())
                {
                    if (sqlCmd.GetValue<bool>("default_style"))
                    {
                        style = new LStyle();
                        if (tableInfo.TypeGeom == TypeGeometry.MULTIPOINT)
                        {
                            style.FillColor = convColor(sqlCmd.GetValue<uint>("fontcolor"));
                            style.HasStroke = true;
                            style.StrokeColor = convColor(sqlCmd.GetValue<uint>("fontframecolor"));
                            style.PointSize = sqlCmd.GetValue<int>("fontsize");
                        }
                        else if (tableInfo.TypeGeom == TypeGeometry.MULTILINESTRING)
                        {
                            style.FillColor = convColor(sqlCmd.GetValue<uint>("pencolor")); 
                            style.LineWidth = sqlCmd.GetValue<Int32>("penwidth");
                        }
                        else if (tableInfo.TypeGeom == TypeGeometry.MULTIPOLYGON)
                        {
                            var bg = sqlCmd.GetValue<uint>("brushbgcolor");
                            if (sqlCmd.GetValue<Int32>("brushstyle") == 0)
                            {
                                // сплошная заливка, берем основной цвет
                                style.PolygonOpacity = Convert.ToInt32((bg >> 24) * 100 / 255d);
                                style.FillColor = convColor(sqlCmd.GetValue<uint>("brushfgcolor"));
                            }
                            else
                            {
                                // берем цвет заливки
                                style.PolygonOpacity = 100;
                                style.FillColor = convColor(bg);
                            }

                            style.HasStroke = true;
                            style.StrokeColor = convColor(sqlCmd.GetValue<uint>("pencolor"));
                            style.StrokeWidth = sqlCmd.GetValue<Int32>("penwidth");                            
                        }
                    }
                }
            }
            return style;
        }

        /// <summary>
        /// Получение групп, в которых содержится таблица
        /// </summary>
        /// <param name="tableInfo">Информация о таблице</param>
        /// <returns>Список названий групп</returns>
        public List<string> GetGroupsForTable(tablesInfo tableInfo)
        {
            List<string> groups = new List<string>();

            try
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format(@"
SELECT tg.name_group as name_group
FROM {0}.table_groups_table tgt
JOIN {0}.table_groups tg
ON tg.id = tgt.id_group
WHERE id_table = {1};", app.scheme, tableInfo.idTable);
                    sqlCmd.ExecuteReader();

                    while (sqlCmd.CanRead())
                    {
                        var group = sqlCmd.GetValue<String>("name_group");
                        if (!String.IsNullOrEmpty(group))
                            groups.Add(group);
                    }
                }
            }
            catch (Exception e) { }
            return groups;
        }

        /// <summary>
        /// Получить схему таблицы, если нет записи в table_info
        /// </summary>
        /// <param name="name">Имя слоя</param>
        /// <returns>Имя схемы</returns>
        public string GetSchemeForTable(string name)
        {
            try
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format(@"
SELECT schemaname  
FROM pg_tables 
JOIN {0}.table_schems
ON schemaname like name 
JOIN geometry_columns
ON f_table_schema like name
AND LOWER(f_table_name) LIKE '{1}' 
WHERE LOWER(tablename) LIKE '{1}';", app.scheme, name.ToLower());
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        return sqlCmd.GetString("schemaname");
                    }
                }
            }
            catch { }
            return null; 
        }

        /// <summary>
        /// Получить тип геометрии
        /// </summary>
        /// <param name="layerName">Имя слоя</param>
        /// <param name="scheme">Схема</param>
        /// <returns>Массив строк вида: {Тип геометрии ("MULTIPOINT", "MULTILINESTRING", "MULTIPOLYGON", ..), Имя поля геометрии}</returns>
        public string[] GetGeometryType(string layerName, string scheme)
        {
            if (String.IsNullOrEmpty(layerName) || String.IsNullOrEmpty(scheme))
                throw new Exception("В описании слоя содержатся ошибки");
            try
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format(@"
SELECT type, f_geometry_column  
FROM geometry_columns
WHERE LOWER(f_table_name) LIKE '{0}'
AND LOWER(f_table_schema) LIKE '{1}'", layerName.ToLower(), scheme.ToLower());
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        return new string[] { sqlCmd.GetString("type"), sqlCmd.GetString("f_geometry_column") };
                    }
                }
            }
            catch { }
            return null; 
        }

        /// <summary>
        /// Регистрация таблицы в MapEditor
        /// </summary>
        /// <param name="layer">Слой</param>
        /// <param name="scheme">Схема</param>
        /// <returns>Результат операции</returns>
        public bool RegisterLayer(Layer layer, string scheme, string geomTypeName, string geomName, string database)
        {
            if (layer == null || layer.lname == null)
                throw new Exception("В описании слоя содержатся ошибки");
            if (String.IsNullOrEmpty(scheme))
                throw new Exception("Слой не найден в базе данных");
            if (String.IsNullOrEmpty(geomTypeName))
                throw new Exception("Геометрия слоя не определена");
            if (String.IsNullOrEmpty(database))
                throw new Exception("Стандартное хранилище не найдено");

            int geomType = 0;
            switch (geomTypeName.ToUpper())
            {
                case "POINT" : case "MULTIPOINT":
                    geomType = 1;
                    break;
                case "LINESTRING" : case "MULTILINESTRING":
                    geomType = 2;
                    break;
                case "POLYGON" : case "MULTIPOLYGON":
                    geomType = 3;
                    break;
            }

            if (geomType == 0)
                throw new Exception("Геометрия слоя не определена");

            // Ищем подходящую группу
            int? group = null;
            if (!String.IsNullOrEmpty(layer.Group_name))
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT id FROM {0}.table_groups WHERE name_group like '{1}'",
                        app.scheme, layer.Group_name);
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead()) 
                        group = sqlCmd.GetValue<Int32>("id");
                }
            }

            int idTable = -1;

            using (var sqlCmd = app.SqlWork())
            {
                sqlCmd.sql = "SELECT " + app.scheme +
                    ".register_table(@scheme, @name_db, @name_map, @type, @geom_type, @geom_field, @group)";

                var parms = new IParams[]
                                        {
                                            new Params
                                                {
                                                    paramName = "@scheme",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = scheme
                                                },
                                            new Params
                                                {
                                                    paramName = "@name_db",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = layer.lname
                                                },
                                            new Params
                                                {
                                                    paramName = "@name_map",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = layer.Name
                                                },
                                            new Params
                                                {
                                                    paramName = "@type",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = 1
                                                },
                                            new Params
                                                {
                                                    paramName = "@geom_type",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = geomType
                                                },
                                            new Params
                                                {
                                                    paramName = "@geom_field",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = geomName
                                                },
                                            new Params
                                                {
                                                    paramName = "@group",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = group
                                                }
                                        };
                try
                {
                    sqlCmd.ExecuteReader(parms);
                    if (!sqlCmd.CanRead() || (idTable = sqlCmd.GetInt32(0)) == -1)
                        throw new Exception("Не удалось создать таблицу");
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось создать таблицу", ex);
                }
            }

            RegisterFields(layer.Attributes, geomName, idTable, layer.lname, scheme, database);

            return true;
        }

        /// <summary>
        /// Регистрация полей в field_info
        /// </summary>
        /// <param name="fields">Список полей</param>
        /// <param name="geomName">Имя поля геометрии</param>
        /// <param name="idTable">ID таблицы</param>
        /// <param name="nameTable">Имя таблицы</param>
        /// <param name="nameScheme">Имя схемы</param>
        /// <param name="database">Имя базы данных</param>
        private void RegisterFields(List<LayerAttribute> fields, string geomName, int idTable, 
            string nameTable, string nameScheme, string database)
        {
            string errors = String.Empty;
            foreach (var field in fields)
            {
                if (field.Name == geomName || field.Name == "gid")
                    continue;

                int type = -1;
                using (var sqlCmd = app.SqlWork())
                {
                    // не сработает на типе GEOMETRY (определяется как USER_DEFINED)
                    sqlCmd.sql = String.Format(@"
SELECT get_field_id FROM {0}.get_field_id('{1}', '{2}', '{3}', '{4}');", 
    app.scheme, database.ToLower(), nameScheme.ToLower(), nameTable.ToLower(), field.Name.ToLower());

                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                        type = sqlCmd.GetInt32("get_field_id");
                }

                if (type == -1)
                {
                    errors += String.Format("Тип поля \"{0}\" неизвестен{1}", field.Name, Environment.NewLine);
                    continue;
                }

                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = "SELECT " + app.scheme +
                        ".register_field(@table_id, @name_db, @name_map, @type, @name_label)";
                    var parms = new IParams[]
                                        {
                                            new Params
                                                {
                                                    paramName = "@table_id",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = idTable
                                                },
                                            new Params
                                                {
                                                    paramName = "@name_db",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = field.Name
                                                },
                                            new Params
                                                {
                                                    paramName = "@name_map",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = String.IsNullOrEmpty(field.Name_ru) ? field.Name : field.Name_ru
                                                },
                                            new Params
                                                {
                                                    paramName = "@type",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = type
                                                },
                                            new Params
                                                {
                                                    paramName = "@name_label",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = String.IsNullOrEmpty(field.Name_ru) ? field.Name : field.Name_ru
                                                }   
                                        };
                    try
                    {
                        sqlCmd.ExecuteReader(parms);
                        if (!sqlCmd.CanRead() || sqlCmd.GetInt32(0) == -1)
                            throw new Exception(String.Format("Не удалось создать поле \"{0}\"", field.Name));
                    }
                    catch (Exception ex)
                    {
                        errors += Environment.NewLine + ex.Message;
                    }
                }                    
            }

            if (!String.IsNullOrEmpty(errors))
                MessageBox.Show("При создании таблицы возникли следующие ошибки:" + errors, "Регистрация таблицы", 
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// Выделить права на таблицы текущему пользователю
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <returns>Результат операции</returns>
        public bool GrantRight(tablesInfo table)
        {
            try
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT * FROM {0}.set_read_right({1}, {2}, TRUE);",
                        app.scheme, table.idTable, app.user_info.id_user);
                    return sqlCmd.ExecuteScalar<bool>();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Не удалось выделить права текущему пользователю", e);
            }
        }

        private static Color convColor(uint value)
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb((byte)255, (byte)r1, (byte)g1, (byte)b1);
            return c1;
        }

        /// <summary>
        /// Удаление события, требовавшего перерегистрации таблицы
        /// </summary>
        /// <param name="table">Измененная таблица</param>
        public void DeleteEvent(tablesInfo table)
        {
            try
            {
                using (var sqlCmd = app.SqlWork())
                {
                    sqlCmd.sql = String.Format("DELETE FROM {0}.table_events WHERE id_table = {1};",
                        app.scheme, table.idTable);
                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Не удалось удалить событие", e);
            }
        }
    }

}
