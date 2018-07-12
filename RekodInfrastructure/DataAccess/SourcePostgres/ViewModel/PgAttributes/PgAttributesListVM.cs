using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Interfaces;
using NpgsqlTypes;
using Rekod.Controllers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using axVisUtils.TableData;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using PgHistM = Rekod.DataAccess.SourcePostgres.Model.PgHistory;
using PgCV = Rekod.DataAccess.SourcePostgres.View.ConfigView;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.AbstractSource;
using System.Windows;
using System.Data;
using Rekod.Behaviors;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes
{
    /// <summary>
    /// Работы с атрибутами объекта
    /// </summary>
    public class PgAttributesListVM : ViewModelBase, PgAtM.IPgAttributesVM
    {
        #region Поля
        private PgAttributesVM _attributeVM;
        private readonly Npgsql.NpgsqlConnectionStringBuilder _connect;
        private bool _isDebug;
        private readonly PgM.PgTableBaseM _table;
        private bool _isNew;
        private readonly PgAtM.PgAttributeM _pkAttribute;
        private readonly PgAtM.PgAttributeM _geomAttribute;
        private readonly bool _isReadOnly;
        private readonly ObservableCollection<PgAtM.PgAttributeM> _attributes;

        private ICommand _reloadCommand;
        private ICommand _saveCommand;
        private ICommand _openTableToSelectCommand;
        private ICommand _clearValueInFieldCommand;
        private ICommand _openTableToViewCommand;
        private ICommand _getCollectionOfVariantsCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// ViewModel окна атрибутов объекта
        /// </summary>
        public PgAtVM.PgAttributesVM AttributeVM
        {
            get { return _attributeVM; }
        }
        /// <summary>
        /// Идентификатор новой записи
        /// </summary>
        public bool IsNew
        { get { return _isNew; } }
        /// <summary>
        /// Идентификатор объекта с атрибутами
        /// </summary>
        public PgAtM.PgAttributeM PkAttribute
        { get { return _pkAttribute; } }
        /// <summary>
        /// Геометрия объекта
        /// </summary>
        public PgAtM.PgAttributeM GeomAttribute
        { get { return _geomAttribute; } }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        /// <summary>
        /// Id таблицы истории (если загружаются атрибуты из таблицы истории)
        /// </summary>
        public int IdHistoryTable
        {
            get;
            private set;
        }
        /// <summary>
        /// Id объекта в таблице истории (если загружаются атрибуты из таблицы истории)
        /// </summary>
        public int IdHistoryObject
        {
            get;
            private set;
        }
        /// <summary>
        /// Загружаются ли атрибуты из таблицы истории
        /// </summary>
        public bool LoadFromHistory
        {
            get;
            private set;
        }
        /// <summary>
        /// Загружать стили для объектов
        /// </summary>
        public bool LoadStyle
        {
            get;
            private set; 
        }
        #endregion // Свойства

        #region Коллекции
        /// <summary>
        /// Коллекция загруженных данных
        /// </summary>
        public ObservableCollection<PgAtM.PgAttributeM> Attributes
        { get { return _attributes; } }
        #endregion Коллекции

        #region Конструкторы
        /// <summary>
        /// Работы с атрибутами объекта
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="idObject">Идентификатор объекта с атрибутами.
        /// Если он null создается новый</param>
        /// <exception cref="ArgumentNullException"/>
        public PgAttributesListVM(PgAtVM.PgAttributesVM attributeVM, object idObject, bool loadStyle = true)
        {
            LoadStyle = loadStyle; 
            _attributeVM = attributeVM;

            _isDebug = attributeVM.IsDebug;
            _connect = attributeVM.Connect;
            _isReadOnly = attributeVM.IsReadOnly;
            _table = attributeVM.Table;
            if (!String.IsNullOrEmpty(_table.GeomField))
            {
                _geomAttribute = new PgAtM.PgAttributeM(this, (PgM.PgFieldM)_table.Fields.First(p => p.Name == _table.GeomField));
            }

            _pkAttribute = new PgAtM.PgAttributeM(this, (PgM.PgFieldM)_table.PrimaryKeyField, idObject);
            if (idObject == null)
                _isNew = true;

            _attributes = new ObservableCollection<PgAtM.PgAttributeM>();
        }
        /// <summary>
        /// Используется для работы с историей
        /// </summary>
        /// <param name="table">Таблица для которой загружается история</param>
        /// <param name="idObject">Идентификатор объекта для которой загружается история</param>
        /// <param name="loadFromHistory">Использовать для загрузки атрибутов таблицу истории</param>
        /// <param name="idHistoryTable">Идентификатор таблицы истории из которой загружаются атрибуты</param>
        /// <param name="idHistoryObject">Идентификатор объекта в таблице истории</param>
        /// <param name="isReadOnly">Открываются ли атрибуты только для чтения</param>
        /// <param name="IsDebug">Запускается ли работа с атрибутами в режиме отладки</param>
        public PgAttributesListVM(PgM.PgTableBaseM table, object idObject, bool loadFromHistory, int? idHistoryTable = null, int? idHistoryObject = null, bool isReadOnly = false, bool IsDebug = false, bool loadStyle = true)
        {
            LoadStyle = loadStyle;
            if (loadFromHistory)
            {
                LoadFromHistory = loadFromHistory;
                IdHistoryTable = (int)idHistoryTable;
                IdHistoryObject = (int)idHistoryObject;
            }
            _isDebug = IsDebug;
            _connect = ((PgVM.PgDataRepositoryVM)table.Source).Connect;
            if (isReadOnly || table.IsReadOnly || !table.CanWrite)
                _isReadOnly = true;
            _table = table;
            if (!String.IsNullOrEmpty(_table.GeomField))
            {
                _geomAttribute = new PgAtM.PgAttributeM(this, (PgM.PgFieldM)_table.Fields.First(p => p.Name == _table.GeomField));
            }

            _pkAttribute = new PgAtM.PgAttributeM(this, (PgM.PgFieldM)_table.PrimaryKeyField, idObject);
            if (idObject == null)
                _isNew = true;

            _attributes = new ObservableCollection<PgAtM.PgAttributeM>();
        }
        #endregion // Конструктор

        #region Команды
        #region ReloadCommand
        /// <summary> 
        /// Загружает или обнавляет список атрибутов со связанными справочниками
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload)); }
        }
        /// <summary> 
        /// Загружает или обнавляет список атрибутов со связанными справочниками
        /// </summary>
        public void Reload(object param = null)
        {
            if (LoadFromHistory)
            {
                LoadAttributesFromHistoryTable(IdHistoryTable, IdHistoryObject);
            }
            else
            {
                var addAttributes = new List<PgAtM.PgAttributeM>();
                foreach (PgM.PgFieldM item in _table.Fields)
                {
                    if (item.Type != AbsM.EFieldType.Geometry
                            && PkAttribute.Field != item
                            && item.IsVisible)
                    {
                        var attribute = FindAttribute(item);
                        if (attribute == null)
                            attribute = new PgAtM.PgAttributeM(this, item);
                        addAttributes.Add(attribute);
                    }
                }
                ExtraFunctions.Sorts.SortList(Attributes, addAttributes);

                if (!IsNew) // Если это не новый объект загружаем данные атрибутов из базы
                {
                    string mainSql = GetSQLQuery(_table, string.Format(@"""{0}"" = :{0}", PkAttribute.Field.Name));
                    var arrayParams = new IParams[] 
                {
                    new Interfaces.Params(@":" + PkAttribute.Field.Name, PkAttribute.Value, (PkAttribute.Field as PgM.PgFieldM).DbType),
                    new Interfaces.Params(@":srid", Program.srid, System.Data.DbType.Int32) 
                };

                    using (var sqlCmd = new SqlWork(_connect, _isDebug))
                    {
                        sqlCmd.sql = mainSql;
                        if (sqlCmd.ExecuteReader(arrayParams) && sqlCmd.CanRead())
                        {
                            foreach (var item in Attributes)
                            {
                                PgAtM.PgAttributeM.SetValue(item, sqlCmd.GetValue<object>(item.Field.Name));
                            }
                            object pkvalue = sqlCmd.GetValue<object>(PkAttribute.Field.Name);
                            if (pkvalue != null)
                            {
                                PgAtM.PgAttributeM.SetValue(PkAttribute, pkvalue);
                            }

                            if (!String.IsNullOrEmpty(_table.GeomField))
                            {
                                object geomvalue = sqlCmd.GetValue<object>(GeomAttribute.Field.Name);
                                if (geomvalue != null)
                                {
                                    PgAtM.PgAttributeM.SetValue(GeomAttribute, geomvalue);
                                }
                            }
                        }
                    }
                }
                var atributes = Attributes;
                GetAttributesVariants(atributes);
            }
        }
        #endregion ReloadCommand

        #region SaveCommand
        /// <summary> 
        /// Сохраненяет атрибуты объекта
        /// </summary>
        public ICommand SaveCommand
        {
            get
            { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        public void Save(object param = null)
        {
            if (!CanSave(param)) return;

            List<string> columns = new List<string>();

            var ListParams = new List<IParams>();
            for (int index = 0; index < Attributes.Count; index++)
            {
                var attribute = Attributes[index];
                var field = attribute.Field as PgM.PgFieldM;
                var p = new Params { _paramName = field.Name };

                p.type = (attribute.Field as PgM.PgFieldM).DbType;
                p.value = PgAtM.PgAttributeM.GetValue(attribute);

                string column = (IsNew)
                                ? p._paramName
                                : string.Format("\"{0}\" = :{0}", p._paramName);

                columns.Add(column);
                ListParams.Add(p);
            }
            PgAtM.attributeTypeChange change;
            if (IsNew)
            {
                change = PgAtM.attributeTypeChange.Create;
                // Построение Insert запроса
                using (var sqlCmd = new SqlWork(_connect, _isDebug))
                {
                    string strColumns = string.Empty;
                    string strValues = string.Empty;
                    if (columns.Count != 0)
                    {
                        strColumns = "\"" + string.Join("\", \n\t\"", columns.ToArray()) + "\"";
                        strValues = ":" + string.Join(", \n\t:", columns.ToArray());
                    }
                    sqlCmd.sql =
                        string.Format(@"
                                INSERT INTO 
                                    {0}.{1}
                                (
                                    ""{2}"",
                                    {3}
                                ) VALUES (
                                    DEFAULT, 
                                    {4}
                                ) RETURNING 
                                    ""{2}""",
                                _table.SchemeName,
                                _table.Name,
                                _table.PrimaryKeyField.Name,
                                strColumns,
                                strValues);

                    var value = sqlCmd.ExecuteScalar<object>(ListParams);

                    if (value != null)
                    {
                        if (_table.IsMapStyle)
                        {
                            using (var sqlInsert = new SqlWork(_connect, _isDebug))
                            {
                                sqlInsert.sql = String.Format("INSERT INTO sys_scheme.style_object_info (id_table, id_obj) VALUES ({0}, {1})",
                                                       _table.Id, value);
                                sqlInsert.ExecuteNonQuery();
                            }
                        }

                        PgAtM.PgAttributeM.SetValue(PkAttribute, value);
                        _isNew = false;


                    }
                }
            }
            else
            {
                if (ListParams.Count == 0)
                    return;
                change = PgAtM.attributeTypeChange.Change;

                // Посторение Update запроса
                ListParams.Add(new Params(":" + PkAttribute.Field.Name, PkAttribute.Value, (PkAttribute.Field as PgM.PgFieldM).DbType));
                using (var sqlCmd = new SqlWork(_connect, _isDebug))
                {
                    sqlCmd.sql =
                        string.Format(@"
                                UPDATE 
                                    {0}.{1}
                                SET
                                    {2}
                                WHERE 
                                    ""{3}"" = :{3};",
                                _table.SchemeName,
                                _table.Name,
                                string.Join(", \n\t", columns.ToArray()),
                                _table.PrimaryKeyField.Name);

                    sqlCmd.ExecuteNonQuery(ListParams);
                }

            }
            // Создание события на обновление атрибута
            var source = (PgVM.PgDataRepositoryVM)_table.Source;
            source.SetEventAttribute(_table, PkAttribute.Value, change);
            Reload();
        }
        bool CanSave(object param = null)
        {
            return !_isReadOnly;
        }
        #endregion SaveCommand

        #region OpenTableToSelectCommand
        public ICommand OpenTableToSelectCommand
        {
            get
            {
                return _openTableToSelectCommand ?? (_openTableToSelectCommand
                    = new RelayCommand(this.OpenTableToSelect, this.CanOpenTableToSelect));
            }
        }
        void OpenTableToSelect(object param)
        {
            if (!CanOpenTableToSelect(param)) return;
            var attr = param as PgAtM.PgAttributeM;
            if (attr == null) return;
            var variant = GetRefValue(attr);
            object id = null;
            if (variant != null)
                id = variant.Id;
            var value = (attr.Field as PgM.PgFieldM).RefTable.Source.OpenTable((attr.Field as PgM.PgFieldM).RefTable, id, true, AttributeVM) as DataRowView;
            if (value == null)
                return;
            object newValue = value[(attr.Field as PgM.PgFieldM).RefField.Name];
            if (newValue == DBNull.Value)
                newValue = null;
            attr.Value = newValue;
            GetAttributesVariants(new[] { attr });
            attr.Value = null;
            attr.Value = newValue; // Для установки соответствия после загрузки справочника
        }
        bool CanOpenTableToSelect(object param)
        {
            var attr = param as PgAtM.PgAttributeM;
            return (attr != null && !attr.IsReadOnly && (attr.Field as PgM.PgFieldM).RefTable != null);
        }
        #endregion OpenTableToSelectCommand

        #region OpenTableToViewCommand
        public ICommand OpenTableToViewCommand
        {
            get
            {
                return _openTableToViewCommand ?? (_openTableToViewCommand
                    = new RelayCommand(this.OpenTableToView, this.CanOpenTableToView));
            }
        }
        void OpenTableToView(object param = null)
        {
            if (!CanOpenTableToView(param)) return;

            var attr = param as PgAtM.PgAttributeM;
            if (attr == null) return;
            var variant = GetRefValue(attr);
            object id = null;
            if (variant != null)
                id = variant.Id;
            (attr.Field as PgM.PgFieldM).RefTable.Source.OpenTable((attr.Field as PgM.PgFieldM).RefTable, id, false, AttributeVM);
        }
        bool CanOpenTableToView(object param = null)
        {
            var attr = param as PgAtM.PgAttributeM;
            return (attr != null && (attr.Field as PgM.PgFieldM).RefTable != null);
        }
        #endregion OpenTableToViewCommand

        #region ClearValueInFieldCommand
        public ICommand ClearValueInFieldCommand
        {
            get
            {
                return _clearValueInFieldCommand ?? (_clearValueInFieldCommand
                    = new RelayCommand(this.ClearValueInField, this.CanClearValueInField));
            }
        }
        void ClearValueInField(object param = null)
        {
            if (CanClearValueInField(param))
            {
                PgAtM.PgAttributeM attr = null;
                if (param is CommandEventParameter)
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    attr = commEvtParam.CommandParameter as PgAtM.PgAttributeM;
                }
                else if (param is PgAtM.PgAttributeM)
                {
                    attr = param as PgAtM.PgAttributeM;
                }
                attr.Value = null;
            }
        }
        bool CanClearValueInField(object param = null)
        {
            if (_isReadOnly == true)
                return false;
            PgAtM.PgAttributeM attr = null; 
            if(param is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = param as CommandEventParameter;
                attr = commEvtParam.CommandParameter as PgAtM.PgAttributeM;
            }
            else if (param is PgAtM.PgAttributeM)
            {
                attr = param as PgAtM.PgAttributeM;
            }
            return (attr != null && attr.Value != null);
        }
        #endregion ClearValueInFieldCommand
        #endregion Команды

        #region Методы
        /// <summary>
        /// Находит атрибут по полю таблицы
        /// </summary>
        /// <param name="field">Поле таблицы</param>
        /// <returns>Атрибут</returns>
        public PgAtM.PgAttributeM FindAttribute(AbsM.IFieldM field)
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                var item = _attributes[i];
                if (item.Field.Equals(field))
                    return item;
            }
            return null;
        }
        /// <summary>
        /// Заполняет связанные атрибуты списком вариантов выбора
        /// </summary>
        /// <param name="atributes"></param>
        private void GetAttributesVariants(IEnumerable<PgAtM.PgAttributeM> atributes)
        {
            var listAttributeRef = new List<PgAtM.PgAttributeM>();
            var listSqlRefTable = new List<string>();

            foreach (var item in atributes)
            {
                string where = string.Empty;
                switch ((item.Field as PgM.PgFieldM).RefType)
                {
                    case AbsM.ERefType.Reference:
                        if (_isReadOnly == true)
                            where = string.Format(@"
                                                (
                                                    ""{1}"" = :value{0} 
                                                )",
                                            item.Field.Id,
                                            (item.Field as PgM.PgFieldM).RefField.Name
                                            );
                        else
                            where = "1 = 1";
                        break;
                    case AbsM.ERefType.Interval:
                        if (_isReadOnly == true)
                            where = string.Format(@"
                                                (
                                                    ""{1}"" < :value{0} 
                                                ) AND (
                                                    :value{0} <= ""{2}""
                                                )",
                                            (item.Field as PgM.PgFieldM).Id,
                                            (item.Field as PgM.PgFieldM).RefField.Name,
                                            (item.Field as PgM.PgFieldM).RefFieldEnd.Name
                                            );
                        else
                            where = "1 = 1";
                        break;
                    case AbsM.ERefType.Data:
                        where = string.Format(@"
                                            (
                                                ""{1}"" = :value{0} 
                                            )",
                                        item.Field.Id,
                                        (item.Field as PgM.PgFieldM).RefField.Name
                                        );

                        break;
                    default:
                        continue;
                }

                listAttributeRef.Add(item);
                String sqlQueryReftable = GetSQLQueryRefTable((item.Field as PgM.PgFieldM), where, "sys_scheme");
                listSqlRefTable.Add(sqlQueryReftable);
            }
            // Получение текущих значений для ссылок и интервалов
            GetCollectionOfVariandsForBD(listAttributeRef, listSqlRefTable);
        }
        /// <summary>
        /// Функция считывания с БД 
        /// </summary>
        /// <param name="listAttributeRef"> Список </param>
        /// <param name="listSqlRefTable"></param>
        private void GetCollectionOfVariandsForBD(List<PgAtM.PgAttributeM> listAttributeRef, List<string> listSqlRefTable)
        {
            List<IParams> ParamsArray = new List<IParams>();
            for (int i = 0; i < listAttributeRef.Count; i++)
            {
                var item = listAttributeRef[i];
                ParamsArray.Add(new Params(":value" + item.Field.Id, PgAtM.PgAttributeM.GetValue(item), (item.Field as PgM.PgFieldM).DbType));
            }
            if (listSqlRefTable.Count > 0)
            {
                using (var sqlCmd = new SqlWork(_connect, _isDebug))
                {
                    sqlCmd.sql = string.Join(";\n", listSqlRefTable.ToArray());
                    sqlCmd.ExecuteMultipleReader(ParamsArray);
                    int i = 0;
                    do
                    {
                        var item = listAttributeRef[i++];
                        item.Variants.Clear();
                        while (sqlCmd.CanRead())
                        {
                            var id = sqlCmd.GetValue<object>((item.Field as PgM.PgFieldM).RefTable.PrimaryKeyField.Name);
                            var fieldName = sqlCmd.GetValue<string>((item.Field as PgM.PgFieldM).RefFieldName.Name);
                            var idValue = sqlCmd.GetValue<object>((item.Field as PgM.PgFieldM).RefField.Name);

                            PgM.PgStyleObjectM style = null;
                            if ((item.Field as PgM.PgFieldM).RefTable.IsMapStyle && LoadStyle)
                            {
                                style = new PgM.PgStyleObjectM((item.Field as PgM.PgFieldM).RefTable, id);

                                style.SetFont(
                                    name: sqlCmd.GetString("!fontname"),
                                    color: sqlCmd.GetValue<int>("!fontcolor"),
                                    frameColor: sqlCmd.GetValue<int>("!fontframecolor"),
                                    size: sqlCmd.GetInt32("!fontsize"));
                                style.SetSymbol(
                                    shape: sqlCmd.GetValue<int>("!symbol"));
                                style.SetPen(
                                    color: sqlCmd.GetValue<int>("!pencolor"),
                                    type: sqlCmd.GetValue<int>("!pentype"),
                                    width: sqlCmd.GetValue<int>("!penwidth"));
                                style.SetBrush(
                                    bgColor: sqlCmd.GetValue<int>("!brushbgcolor"),
                                    fgColor: sqlCmd.GetValue<int>("!brushfgcolor"),
                                    style: sqlCmd.GetValue<int>("!brushstyle"),
                                    hatch: sqlCmd.GetValue<int>("!brushhatch"));
                            }

                            if ((item.Field as PgM.PgFieldM).RefType == AbsM.ERefType.Reference)
                            {
                                item.Variants.Add(new PgAtM.PgAttributeReferenceM(item, fieldName, style, idValue));
                            }
                            else if ((item.Field as PgM.PgFieldM).RefType == AbsM.ERefType.Interval)
                            {
                                var intervalMin = ExtraFunctions.Converts.To<double>(idValue);
                                var intervalMax = sqlCmd.GetValue<double>((item.Field as PgM.PgFieldM).RefFieldEnd.Name);
                                item.Variants.Add(new PgAtM.PgAttributeIntervalM(item, fieldName, style, id, intervalMin, intervalMax));
                            }
                            else if ((item.Field as PgM.PgFieldM).RefType == AbsM.ERefType.Data)
                            {
                                item.Variants.Add(new PgAtM.PgAttributeReferenceM(item, fieldName, style, idValue));
                            }
                        }
                    } while (sqlCmd.CanNextResult());
                }
            }
        }
        /// <summary>
        /// Функция генерации sql запроса
        /// </summary>
        /// <param name="table">Таблица с полями</param>
        /// <param name="whereId">Where запрос</param>
        /// <param name="isStyle">Таблица со стилем</param>
        /// <returns>Сгенерированая строка SQL запроса</returns>
        private string GetSQLQuery(PgM.PgTableBaseM table, string whereId)
        {
            var listColumns = new List<string>();
            for (int i = table.Fields.Count - 1; i >= 0; i--)
            {
                var item = table.Fields[i] as PgM.PgFieldM;

                string select = "";
                if (item.Type == AbsM.EFieldType.Geometry)
                    select = String.Format(@"st_ASTEXT(st_TRANSFORM(""{0}"", :srid)) AS ""{0}""", item.Name);
                else
                    select = String.Format(@"""{0}""", item.Name);

                listColumns.Add(select);
            }

            return string.Format(@"
                                SELECT 
                                    {0} 
                                FROM 
                                    {1}.{2} 
                                WHERE 
                                    {3}",
                    string.Join(", \n\t", listColumns.ToArray()),
                    table.SchemeName,
                    table.Name,
                    whereId);
        }
        /// <summary>
        /// Функция генерации sql запроса
        /// </summary>
        /// <param name="field">Поле с ссылкой на таблицу</param>
        /// <param name="whereId">Where запрос</param>
        /// <param name="isStyle">Таблица со стилем</param>
        /// <returns>Сгенерированая строка SQL запроса</returns>
        private string GetSQLQueryRefTable(PgM.PgFieldM field, string whereId, string sysScheme)
        {
            var listColumns = new List<string>();

            if (field.RefTable == null)
                return null;

            if (field.RefField != null)
                listColumns.Add(string.Format("t1.\"{0}\"", field.RefField.Name));
            if (field.RefFieldEnd != null)
                listColumns.Add(string.Format("t1.\"{0}\"", field.RefFieldEnd.Name));
            if (field.RefFieldName != null)
                listColumns.Add(string.Format("t1.\"{0}\"", field.RefFieldName.Name));

            // Добавляем идентификатор в коллекции
            var pkField = field.RefTable.PrimaryKeyField;
            if (field.RefField != pkField
                && field.RefFieldEnd != pkField
                && field.RefFieldName != pkField)
                listColumns.Add(string.Format("t1.\"{0}\"", pkField.Name));

            if (field.RefTable.IsMapStyle && LoadStyle)
            {
                listColumns.AddRange(new[] {
                    "soi.fontname AS \"!fontname\"", 
                    "soi.fontcolor AS \"!fontcolor\"", 
                    "soi.fontframecolor AS \"!fontframecolor\"",
                    "soi.fontsize AS \"!fontsize\"",
                    "soi.symbol AS \"!symbol\"", 
                    "soi.pencolor AS \"!pencolor\"",
                    "soi.pentype AS \"!pentype\"", 
                    "soi.penwidth AS \"!penwidth\"",
                    "soi.brushbgcolor AS \"!brushbgcolor\"",
                    "soi.brushfgcolor AS \"!brushfgcolor\"", 
                    "soi.brushstyle AS \"!brushstyle\"",
                    "soi.brushhatch AS \"!brushhatch\"" });
            }

            String sql = string.Format(@"
                                SELECT {0}
                                FROM 
                                    ""{1}"".""{2}"" t1 " +
                                (LoadStyle ? @"LEFT 
                                        JOIN 
                                            ""{3}"".style_object_info soi 
                                        ON
                                            (t1.""{4}"" = soi.id_obj AND soi.id_table = {5}) " : " ") +
                                @"WHERE 
                                    {6}",
                    "\n\t" + string.Join(", \n\t", listColumns.ToArray()),
                    field.RefTable.SchemeName,
                    field.RefTable.Name,
                    sysScheme,
                    field.RefTable.PrimaryKey,
                    field.RefTable.Id,
                    whereId);
            return sql;
        }
        /// <summary>
        /// Получает текущее связанное значение 
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public PgAtM.PgAttributeVariantM GetRefValue(PgAtM.PgAttributeM attribute)
        {
            if (attribute == null)
                return null;
            if ((attribute.Field as PgM.PgFieldM).RefType == AbsM.ERefType.None)
                return null;
            if (attribute.Variants == null)
                return null;
            if (attribute.Value == null)
                return null;
            foreach (var item in attribute.Variants)
            {
                // Ищим подходящий элемент для текущей записи
                if (item.CheckValue(attribute.Value))
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// Загрузка атрибутов из таблицы истории
        /// </summary>
        /// <param name="idHistoryTable"></param>
        /// <param name="idHistoryObject"></param>
        public void LoadAttributesFromHistoryTable(int idHistoryTable, int idHistoryObject)
        {
            var addAttributes = new List<PgAtM.PgAttributeM>();
            foreach (PgM.PgFieldM item in _table.Fields)
            {
                if (item.Type != AbsM.EFieldType.Geometry
                        && PkAttribute.Field != item
                        && item.IsVisible)
                {
                    var attribute = FindAttribute(item);
                    if (attribute == null)
                        attribute = new PgAtM.PgAttributeM(this, item);
                    addAttributes.Add(attribute);
                }
            }
            ExtraFunctions.Sorts.SortList(Attributes, addAttributes);

            // Список полей основной таблицы
            List<String> columnsInTablelist = new List<string>();
            // Список полей таблицы истории, которые также существуют в основной таблице
            List<String> columnsInHistoryTablelist = new List<string>();
            foreach (PgM.PgFieldM field in _table.Fields)
            {
                columnsInTablelist.Add(field.Name);
            }

            //Получаем список полей в таблице истории, которые также существуют в основной таблице
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                String sql = String.Format(@"
                    SELECT isc.column_name
                    FROM sys_scheme.table_history_info thi
                    INNER JOIN sys_scheme.table_info ti
                    ON thi.id_table = ti.id
                    INNER JOIN information_schema.columns isc
                    ON isc.table_name = thi.history_table_name AND isc.table_schema = ti.scheme_name
                    WHERE thi.id_history_table = {0} AND  isc.column_name in ('{1}')",
                            idHistoryTable,
                            String.Join("', '", columnsInTablelist.ToArray()));

                sqlWork.sql = sql;
                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    columnsInHistoryTablelist.Add(sqlWork.GetString(0));
                }
                sqlWork.Close();
            }

            // Получаем название таблицы истории
            String historyTableName;
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql = String.Format(@"SELECT thi.history_table_name
                                                            FROM sys_scheme.table_info ti 
                                                            INNER JOIN sys_scheme.table_history_info thi ON ti.id = thi.id_table
                                                            WHERE thi.id_history_table = {0}", idHistoryTable);
                historyTableName = sqlWork.ExecuteScalar().ToString();
            }

            //Получаем значения "После изменения"
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                String fieldsListForSql = String.Join(", ", columnsInHistoryTablelist.ToArray());
                if (!String.IsNullOrEmpty(_table.GeomField))
                {
                    fieldsListForSql = fieldsListForSql.Replace(
                                            _table.GeomField,
                                            String.Format(@"st_ASTEXT(st_TRANSFORM(""{0}"", {1})) AS ""{0}""", _table.GeomField, Program.srid));
                }

                sqlWork.sql = String.Format("SELECT {0}, type_operation FROM {1}.{2} WHERE id_history = {3}",
                                          fieldsListForSql,
                                          _table.SchemeName,
                                          historyTableName,
                                          idHistoryObject);
                sqlWork.ExecuteReader();
                if (sqlWork.CanRead())
                {
                    PgHistM.PgHistoryTypeOperation TypeOperation = sqlWork.GetValue<PgHistM.PgHistoryTypeOperation>("type_operation");

                    if (TypeOperation == PgHistM.PgHistoryTypeOperation.Delete)
                    {
                        foreach (PgAtM.PgAttributeM attr in Attributes)
                        {
                            PgAtM.PgAttributeM.SetValue(attr, null); 
                        }
                    }
                    else
                    {
                        foreach (PgAtM.PgAttributeM attr in Attributes)
                        {
                            if (columnsInHistoryTablelist.Contains(attr.Field.Name))
                            {
                                PgAtM.PgAttributeM.SetValue(attr, sqlWork.GetValue<object>(attr.Field.Name));
                            }
                        }
                        object value = sqlWork.GetValue<object>(PkAttribute.Field.Name);
                        if (value != null)
                        {
                            PgAtM.PgAttributeM.SetValue(PkAttribute, value);
                        }
                        if (!String.IsNullOrEmpty(_table.GeomField))
                        {
                            object geomvalue = sqlWork.GetValue<object>(GeomAttribute.Field.Name);
                            if (geomvalue != null)
                            {
                                PgAtM.PgAttributeM.SetValue(GeomAttribute, geomvalue);
                            }
                        }
                    }
                }
            }
            GetAttributesVariants(Attributes);
        }
        #endregion // Методы
    }
}