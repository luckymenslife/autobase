using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Interfaces;
using NpgsqlTypes;
using Rekod.Controllers;
using Rekod.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using axVisUtils.TableData;
using axVisUtils.Styles;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Services;

namespace Rekod.ViewModel
{
    public class AttributesOfObjectViewModel : ViewModelBase
    {
        #region Fields
        private Interfaces.tablesInfo _tableInfo;
        private int? _idObject;
        private bool _isReadOnly;

        private ObservableCollection<AttributesOfObjectModel> _collectionOfAttributes;
        private AttributesOfObjectModel _pkField;

        private RelayCommand _getAttributesOfObjectCommand;
        private RelayCommand _saveAttributesOfObjectCommand;
        private RelayCommand _openTableToSelectCommand;
        private RelayCommand _clearValueInFieldCommand;
        private RelayCommand _openTableToViewCommand;
        private RelayCommand _getCollectionOfVariantsCommand;
        private Func<AttributesOfObjectModel, int?> _funcOpenTableToSelect;
        private Action<AttributesOfObjectModel> _funcOpenTableToView;
        private Func<AttributesOfObjectModel, VariantsForAttributesModel[]> _funcGetCollectionOfVariants;
        #endregion

        #region Properties

        public Interfaces.tablesInfo TableInfo
        {
            get { return _tableInfo; }
            set { _tableInfo = value; }
        }

        public ObservableCollection<AttributesOfObjectModel> CollectionOfAttributes
        {
            get { return _collectionOfAttributes; }
            set { _collectionOfAttributes = value; OnPropertyChanged(() => this.CollectionOfAttributes); }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public Func<AttributesOfObjectModel, int?> FuncOpenTableToSelect
        {
            get { return _funcOpenTableToSelect ?? (_funcOpenTableToSelect = FuncOpenTableToSelectExecution); }
            set { _funcOpenTableToSelect = value; }
        }
        public Action<AttributesOfObjectModel> FuncOpenTableToView
        {
            get { return _funcOpenTableToView ?? (_funcOpenTableToView = FuncOpenTableToViewExecution); }
            set { _funcOpenTableToView = value; }
        }
        public Func<AttributesOfObjectModel, VariantsForAttributesModel[]> FuncGetCollectionOfVariants
        {
            get { return _funcGetCollectionOfVariants ?? (_funcGetCollectionOfVariants = FuncGetCollectionOfVariantsExecution); }
            set { _funcGetCollectionOfVariants = value; }
        }

        public int? IdObject
        {
            get { return _idObject; }
            set { _idObject = value; }
        }
        public AttributesOfObjectModel PkField
        {
            get { return _pkField; }
        }
        #endregion
        public AttributesOfObjectViewModel(Interfaces.tablesInfo table, int? idObject)
        {

            TableInfo = table;
            IdObject = idObject;

            if (table.read_only)
                _isReadOnly = true;
            else
                _isReadOnly = !Program.app.getWriteTable(TableInfo.idTable);
        }


        #region Command: GetAttributesOfObjectCommand
        /// <summary> Загрузка атрибутов объекта из базы и их значения
        /// </summary>
        public RelayCommand GetAttributesOfObjectCommand
        {
            get
            {
                return _getAttributesOfObjectCommand ?? (_getAttributesOfObjectCommand
                    = new RelayCommand(GetAttributesOfObject));
            }
        }
        void GetAttributesOfObject(object param = null)
        {
            if (CollectionOfAttributes == null)
                CollectionOfAttributes = new ObservableCollection<AttributesOfObjectModel>();
            else
                CollectionOfAttributes.Clear();
            _pkField = null;
            var listAttributes = new List<AttributesOfObjectModel>();
            AttributesOfObjectModel pkField = null;

            var listFields = TableInfo.ListField;
            if (listFields == null) return;
            foreach (var t in listFields)
            {
                Interfaces.tablesInfo refTable = null;
                if (t.ref_table.HasValue)
                {
                    refTable = Program.app.getTableInfo(t.ref_table.Value);
                    if (t.ref_field.HasValue)
                    {
                        var tempListField = new List<Interfaces.fieldInfo>(refTable.ListField);
                        foreach (var f in tempListField)
                        {
                            if (f.idField != t.ref_field
                                && f.idField != t.ref_field_end
                                && f.idField != t.ref_field_name
                                && f.nameDB != _tableInfo.pkField)

                                refTable.ListField.Remove(f);
                        }
                    }
                }
                if (IsReadOnly)
                    t.read_only = true;
                var field = new AttributesOfObjectModel(t, refTable);
                if (field.Type == TypeField.Geometry) continue;
                if (field.NameBD == TableInfo.pkField)
                {
                    pkField = field;
                    continue;
                }
                listAttributes.Add(field);
            }

            //Загрузка значений из базы
            if (IdObject != null)
            {
                var listColumns = new List<string>();
                string mainSql = GetSQLQuery(TableInfo, string.Format("\"{0}\" = {1}", TableInfo.pkField, IdObject));
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = mainSql;
                    if (sqlCmd.ExecuteReader() && sqlCmd.CanRead())
                    {
                        foreach (var item in listAttributes)
                        {
                            item.Value = sqlCmd.GetValue<object>(item.NameBD);
                            CollectionOfAttributes.Add(item);
                        }
                        if (pkField != null)
                        {
                            pkField.Value = sqlCmd.GetValue<object>(pkField.NameBD);
                            _pkField = pkField;
                        }
                    }
                    else
                    {
                        //Ошибка загрузки данных;
                        CollectionOfAttributes = null;
                        _pkField = null;
                        return;
                    }
                }

            }
            else
            {
                foreach (var t in listAttributes)
                {
                    CollectionOfAttributes.Add(t);
                }
                _pkField = pkField;
            }
        }
        #endregion

        #region Command: SaveAttributesOfObjectCommand
        /// <summary> Сохранение значений полей атрибутов объекта в базе данных
        /// </summary>
        public RelayCommand SaveAttributesOfObjectCommand
        {
            get
            {
                return _saveAttributesOfObjectCommand ?? (_saveAttributesOfObjectCommand = new RelayCommand(this.SaveAttributesOfObject, this.CanSaveAttributesOfObject));
            }
        }
        void SaveAttributesOfObject(object param = null)
        {
            if (!CanSaveAttributesOfObject(param)) return;
            var sbsql = new StringBuilder();
            var sbsqlInsertValues = new StringBuilder();
            var listParam = new List<IParams>();
            for (int index = 0; index < CollectionOfAttributes.Count; index++)
            {
                var item = CollectionOfAttributes[index];
                var p = new Params { _paramName = item.NameBD };
                if (item.IsReference)
                {
                    p.typeData = NpgsqlDbType.Integer;
                }
                else
                    switch (item.Type)
                    {
                        case TypeField.Geometry:
                            p.typeData = NpgsqlDbType.Bytea;
                            break;
                        case TypeField.Date:
                            p.typeData = NpgsqlDbType.Date;
                            break;
                        case TypeField.DateTime:
                            p.typeData = NpgsqlDbType.TimestampTZ;
                            break;
                        case TypeField.Integer:
                            p.typeData = NpgsqlDbType.Integer;
                            break;
                        case TypeField.Numeric:
                            p.typeData = NpgsqlDbType.Numeric;
                            break;
                        case TypeField.Text:
                            p.typeData = NpgsqlDbType.Text;
                            break;
                        default:
                            break;
                    }
                p.value = item.Value;

                object value = (item.Type == TypeField.Geometry)
                               ? p.value
                               : p.paramName;

                if (IdObject != null)
                {
                    if (index != 0)
                        sbsql.Append(',');
                    sbsql.AppendFormat("\n\t\"{0}\" = {1}", p._paramName, value);
                }
                else
                {
                    if (index != 0)
                    {
                        sbsql.Append(',');
                        sbsqlInsertValues.Append(',');
                    }

                    sbsql.AppendFormat("\n\t \"{0}\"", p._paramName);

                    sbsqlInsertValues.Append("\n\t" + value);
                }

                listParam.Add(p);
            }
            string sql = "";
            if (IdObject != null)
            {
                // Посторение Update запроса
                sql = string.Format("UPDATE {0}.{1} SET {2} \nWHERE id = {3};",
                                TableInfo.nameSheme,
                                TableInfo.nameDB,
                                sbsql,
                                IdObject);

            }
            else
            {
                // Посторение Insert запроса
                sql = string.Format("INSERT INTO {0}.{1} \n({2}) \nVALUES \n({3})",
                                TableInfo.nameSheme,
                                TableInfo.nameDB,
                                sbsql,
                                sbsqlInsertValues);

            }

            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = sql;
                sqlCmd.ExecuteNonQuery(listParam);
            }
        }

        bool CanSaveAttributesOfObject(object param = null)
        {
            if (CollectionOfAttributes == null)
                return false;
            return !_isReadOnly;
        }

        #endregion

        #region Command: OpenTableToSelectCommand <WinAttribModel>
        public RelayCommand OpenTableToSelectCommand
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
            var attr = param as AttributesOfObjectModel;
            if (attr == null) return;

            int? val = null;


            val = FuncOpenTableToSelect.Invoke(attr);

            if (val == null) return;




            if (attr.CollectionOfVariants == null)
                attr.CollectionOfVariants = new ObservableCollection<VariantsForAttributesModel>();
            attr.CollectionOfVariants.Clear();

            var listAttributeRef = new List<AttributesOfObjectModel>
                                       {
                                           attr
                                       };
            var listSqlRefTable = new List<string>
                                      {
                                          GetSQLQuery(attr.RefTable,
                                                      string.Format("\"{0}\" = {1}", attr.refField.nameDB, val))
                                      };

            GetCollectionOfVariandsForBD(listAttributeRef, listSqlRefTable);

            attr.Value = val;
        }

        bool CanOpenTableToSelect(object param)
        {
            if (CollectionOfAttributes == null)
                return false;
            if (_isReadOnly) return false;
            var attr = param as AttributesOfObjectModel;
            return attr != null && attr.IsReference;
        }

        #endregion

        #region Command: OpenTableToViewCommand <WinAttribModel>
        public RelayCommand OpenTableToViewCommand
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

            var attr = param as AttributesOfObjectModel;
            if (attr == null) return;

            if (FuncOpenTableToView != null)
                FuncOpenTableToView.Invoke(attr);
        }

        bool CanOpenTableToView(object param = null)
        {
            if (CollectionOfAttributes == null)
                return false;
            var attr = param as AttributesOfObjectModel;
            return (attr != null);
        }
        #endregion

        #region Command: ClearValueInFieldCommand <WinAttribModel>
        public RelayCommand ClearValueInFieldCommand
        {
            get
            {
                return _clearValueInFieldCommand ?? (_clearValueInFieldCommand
                    = new RelayCommand(this.ClearValueInField, this.CanClearValueInField));
            }
        }

        void ClearValueInField(object param = null)
        {
            if (!CanClearValueInField(param)) return;
            var attr = param as AttributesOfObjectModel;
            if (attr == null) return;
            attr.Value = null;
        }

        bool CanClearValueInField(object param = null)
        {
            if (_isReadOnly == true)
                return false;
            var attr = param as AttributesOfObjectModel;
            return (attr != null);
        }
        #endregion

        #region Command: GetCollectionOfVariantsPreviewCommand <WinAttribModel>
        /// <summary> Загрузка справочников для текущих значений
        /// </summary>
        public RelayCommand GetCollectionOfVariantsPreviewCommand
        {
            get
            {
                return _clearValueInFieldCommand ?? (_clearValueInFieldCommand
                    = new RelayCommand(this.GetCollectionOfVariantsPreview, this.CanGetCollectionOfVariantsPreview));
            }
        }

        void GetCollectionOfVariantsPreview(object param = null)
        {
            if (!CanGetCollectionOfVariantsPreview(param)) return;
            var listAttributeRef = new List<AttributesOfObjectModel>();
            var listSqlRefTable = new List<string>();

            foreach (var item in CollectionOfAttributes)
            {
                if (item.IsReference || item.IsInterval)
                {
                    if (item.CollectionOfVariants == null)
                        item.CollectionOfVariants = new ObservableCollection<VariantsForAttributesModel>();
                    item.CollectionOfVariants.Clear();

                    if (item.Value == null) continue;

                    if (item.IsReference)
                    {
                        listAttributeRef.Add(item);
                        listSqlRefTable.Add(GetSQLQuery(item.RefTable, string.Format("\"{0}\" = {1}", item.refField.nameDB, item.Value), item.IsStyle));

                    }
                    if (item.IsInterval)
                    {
                        listAttributeRef.Add(item);
                        listSqlRefTable.Add(GetSQLQuery(item.RefTable, string.Format("\"{0}\" < {1} AND {1} <= \"{2}\"", item.refField.nameDB, item.Value.ToString().Replace(',', '.'), item.refFieldEnd.nameDB)));
                    }
                }
            }


            /// Получение текущих значений для ссылок и интервалов
            GetCollectionOfVariandsForBD(listAttributeRef, listSqlRefTable);
        }

        bool CanGetCollectionOfVariantsPreview(object param = null)
        {
            if (CollectionOfAttributes == null)
                return false;
            return true;
        }
        #endregion

        #region Command: GetCollectionOfVariantsCommand <WinAttribModel>
        /// <summary> Загрузка всех справочников
        /// </summary>
        public RelayCommand GetCollectionOfVariantsCommand
        {
            get
            {
                return _clearValueInFieldCommand ?? (_clearValueInFieldCommand
                    = new RelayCommand(this.GetCollectionOfVariants, this.CanGetCollectionOfVariants));
            }
        }


        void GetCollectionOfVariants(object param = null)
        {
            if (!CanGetCollectionOfVariants(param)) return;
            if (IsReadOnly == true)
            {
                GetCollectionOfVariantsPreview(param);
                return;
            }

            var listAttributeRef = new List<AttributesOfObjectModel>();
            var listSqlRefTable = new List<string>();

            foreach (var item in CollectionOfAttributes)
            {
                if (item.IsReference || item.IsInterval)
                {
                    if (item.CollectionOfVariants == null)
                        item.CollectionOfVariants = new ObservableCollection<VariantsForAttributesModel>();
                    item.CollectionOfVariants.Clear();

                    listAttributeRef.Add(item);

                    switch (item.TypeRef)
                    {
                        case TypeRef.isReferenceTable:
                            listSqlRefTable.Add(GetSQLQuery(item.RefTable,
                                string.Format("\"{0}\" = {1}", item.refField.nameDB, item.Value)));
                            break;
                        case TypeRef.isReferenceStyle:
                            listSqlRefTable.Add(GetSQLQuery(item.RefTable, isStyle: true));
                            break;
                        default:
                            listSqlRefTable.Add(GetSQLQuery(item.RefTable));
                            break;
                    }

                }
            }


            /// Получение текущих значений для ссылок и интервалов
            GetCollectionOfVariandsForBD(listAttributeRef, listSqlRefTable);
        }

        bool CanGetCollectionOfVariants(object param = null)
        {
            if (CollectionOfAttributes == null)
                return false;
            return true;
        }
        #endregion

        private int? FuncOpenTableToSelectExecution(AttributesOfObjectModel winAttribModel)
        {
            return Program.work.OpenForm.OpenTableObject(winAttribModel.RefTable, (int)winAttribModel.Value, true);
        }
        private void FuncOpenTableToViewExecution(AttributesOfObjectModel winAttribModel)
        {
            Program.work.OpenForm.OpenTableObject(winAttribModel.RefTable, null);
        }

        /// <summary>
        /// Функция генерации sql запроса
        /// </summary>
        /// <param name="tablesInfo">Таблица с полями</param>
        /// <param name="whereId">Where запрос</param>
        /// <param name="isStyle">Таблица со стилем</param>
        /// <returns>Сгенерированая строка SQL запроса</returns>
        private string GetSQLQuery(Interfaces.tablesInfo tablesInfo, string whereId = "1=1", bool? isStyle = null)
        {
            var listFields = tablesInfo.ListField;
            var listColumns = new List<string>();
            for (int i = 0; i < listFields.Count; i++)
            {
                var item = listFields[i];

                //if (item.nameDB == tablesInfo.pkField) continue;
                if (item.TypeField == TypeField.Geometry)
                    listColumns.Add(String.Format("st_astext(st_transform(\"{0}\", {1})) AS \"{0}\"", item.nameDB, Program.srid));
                else
                    listColumns.Add(String.Format("\"{0}\"", item.nameDB));
            }
            //fontname, fontcolor, fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch
            if (isStyle == true)
            {
                listColumns.AddRange(new[] {
                    "\"fontname\"", 
                    "\"fontcolor\"", 
                    "\"fontframecolor\"", 
                    "\"symbol\"", 
                    "\"pencolor\"",
                    "\"pentype\"", 
                    "\"penwidth\"",
                    "\"brushbgcolor\"",
                    "\"brushfgcolor\"", 
                    "\"brushstyle\"",
                    "\"brushhatch\"",
                    "\"fontsize\" "});

            }
            return string.Format("SELECT \n{0} \nFROM {1}.{2} \nWHERE {3}",
                                   string.Join(", \n\t", listColumns.ToArray()),
                                   tablesInfo.nameSheme,
                                   tablesInfo.nameDB,
                                   whereId);
        }

        /// <summary>
        /// Функция считывания с БД 
        /// </summary>
        /// <param name="listAttributeRef"> Список </param>
        /// <param name="listSqlRefTable"></param>
        private static void GetCollectionOfVariandsForBD(List<AttributesOfObjectModel> listAttributeRef, List<string> listSqlRefTable)
        {
            if (listSqlRefTable.Count > 0)
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = string.Join(";\n", listSqlRefTable.ToArray());
                    sqlCmd.ExecuteMultipleReader();
                    int i = 0;
                    do
                    {
                        var item = listAttributeRef[i++];

                        while (sqlCmd.CanRead())
                        {
                            var fieldName = sqlCmd.GetValue<string>(item.refFieldName.nameDB);
                            var idValue = sqlCmd.GetValue<double>(item.refField.nameDB);

                            //if (item.IsStyle)
                            //{
                            //    var style = new StylesM(sqlCmd, 0)
                            //    {
                            //        Id = (int)idValue,
                            //        Name = fieldName
                            //    };
                            //    // Конвертирует из System.Drawing в BitmapImage
                            //    using (var stream = new MemoryStream())
                            //    {
                            //        //StylesVM.GetPreviewStylePoligon(style).Save(stream, ImageFormat.Bmp);

                            //        stream.Position = 0;
                            //        var result = new BitmapImage();
                            //        result.BeginInit();
                            //        result.CacheOption = BitmapCacheOption.OnLoad;
                            //        result.StreamSource = stream;
                            //        result.EndInit();
                            //        result.Freeze();
                            //        item.CollectionOfVariants.Add(new VariantsForAttributesModel(idValue, new StyleImagePreview(result, style.Name)));
                            //    }


                            //}
                            //else
                            switch (item.TypeRef)
                            {
                                case TypeRef.isReference:
                                case TypeRef.isReferenceTable:
                                case TypeRef.isReferenceStyle:
                                    item.CollectionOfVariants.Add(new VariantsForAttributesModel(idValue, fieldName));
                                    break;

                                case TypeRef.isInterval:
                                case TypeRef.isIntervalStyle:
                                    var intervalMax = sqlCmd.GetValue<double>(item.refFieldEnd.nameDB);
                                    item.CollectionOfVariants.Add(new VariantsForAttributesModel(fieldName, idValue, intervalMax));
                                    break;
                                default:

                                    break;
                            }

                        }
                    } while (sqlCmd.CanNextResult());
                }
        }

        private VariantsForAttributesModel[] FuncGetCollectionOfVariantsExecution(AttributesOfObjectModel winAttribModel)
        {
            throw new NotImplementedException();
        }
        public static VariantsForAttributesModel GetRefValue(AttributesOfObjectModel attributes)
        {
            if (attributes == null)
                return null;
            if (!attributes.IsInterval && !attributes.IsReference)
                return null;
            if (attributes.CollectionOfVariants == null)
                return null;
            var idValue = ExtraFunctions.Converts.To<double?>(attributes.Value);
            if (idValue != null)
                foreach (var item in attributes.CollectionOfVariants)
                {
                    if (attributes.IsInterval)
                    {
                        if (item.IntervalMin < idValue && idValue <= item.IntervalMax)
                            return item;
                    }
                    else if (attributes.IsReference)
                        if (item.IdValue.CompareTo(idValue) == 0)
                            return item;
                }
            return null;
        }
    }
}
