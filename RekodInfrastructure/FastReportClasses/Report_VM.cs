using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastReport;
using FastReport.Data;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Properties;
using Interfaces.FastReport;
using Rekod.Services;

namespace Rekod.FastReportClasses
{
    public class Report_VM : ViewModelBase
    {
        #region Поля
        protected ObservableCollection<TableModel_M> _reports;
        private ReportItem_M _reportItemM;

        private Report _report;

        static int num = 0;
        string keyProcess;
        private IFilterTable _filter;
        private bool _isDesign;

        Dictionary<TableDataSource, string> dicStringSQL = new Dictionary<TableDataSource, string>();
        #endregion // Поля

        #region Свойства
        public ReportItem_M ReportItemM
        {
            get { return _reportItemM; }
        }
        public Report Report
        { get { return _report; } }
        public ObservableCollection<TableModel_M> Reports
        { get { return _reports; } }
        #endregion // Свойства

        #region Конструктор
        public Report_VM(ReportItem_M reportItemM, IFilterTable filter)
        {
            _reportItemM = reportItemM;
            _reports = new ObservableCollection<TableModel_M>();

            _filter = filter;
        }
        #endregion // Конструктор

        #region Открытые методы

        public void OpenReport()
        {
            _isDesign = false;
            _report = CreateReport();
            _report.StartReport += Report_StartReport;
            _report.FinishReport += Report_FinishReport;
            _report.Show();
        }
        public void OpenDesign()
        {
            _isDesign = true;
            _report = CreateReport();
            _report.StartReport += Report_StartReport;
            _report.FinishReport += Report_FinishReport;
            var frm = new GenerateReportForm(this);

            frm.ShowDialog();

        }
        void Report_StartReport(object sender, EventArgs e)
        {
            if (_reportItemM.IdTable == null)
                return;
            var tableinfo = Program.app.getTableInfo((int)_reportItemM.IdTable);
            if (tableinfo == null)
                return;

            var postgresConn = FindConnection(_report, "Current data") as PostgresDataConnection;
            string where = string.Empty;
            switch (_reportItemM.Type)
            {
                case enTypeReport.Table:
                    where = _filter.Where;
                    break;
                case enTypeReport.Object:
                    where = "\"" + tableinfo.pkField + "\" = " + _filter.IdObj;
                    break;
            }
            foreach (TableDataSource item in postgresConn.Tables)
            {
                dicStringSQL.Add(item, item.SelectCommand);
                item.SelectCommand = item.SelectCommand.Replace("@WhereQuestionSQL", where);
                item.SelectCommand = item.SelectCommand.Replace("@OrderQuestionSQL", _filter.Order);
            }
        }
        void Report_FinishReport(object sender, EventArgs e)
        {
            if (_reportItemM.IdTable == null)
                return;
            var tableinfo = Program.app.getTableInfo((int)_reportItemM.IdTable);
            if (tableinfo == null)
                return;

            var postgresConn = FindConnection(_report, "Current data") as PostgresDataConnection;

            foreach (TableDataSource item in postgresConn.Tables)
            {
                item.SelectCommand = dicStringSQL[item];
            }
            dicStringSQL.Clear();
        }


        public bool? SaveReportDialog()
        {
            var dr = MessageBox.Show(Resources.Report_VM_QuestionSaveReport, Resources.Report_VM_QuestionSave, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dr == DialogResult.Cancel)
            {
                return null;
            }
            else if (dr == DialogResult.Yes)
            {
                return SaveReport();
            }
            return false;
        }
        public bool? SaveReport(bool isNew = false)
        {
            if (_reportItemM.IsNew || isNew)
            {
                var frm = new SaveReportFrm();
                frm.DataContext = _reportItemM;
                var result = frm.ShowDialog();
                if (result == true)
                {
                    if (isNew)
                        _reportItemM.SetNew();
                }
                else
                {
                    return null;
                }
            }
            string connString = string.Empty;
            var postgresConn = FindConnection(_report, "Current data") as PostgresDataConnection;
            if (postgresConn != null)
            {
                connString = postgresConn.ConnectionString;
                postgresConn.ConnectionString = string.Empty;
            }
            _reportItemM.Body = _report.ReportResourceString;
            if (postgresConn != null)
            {
                postgresConn.ConnectionString = connString;
            }
            Program.ReportModel.Apply(_reportItemM);

            return true;
        }
        #endregion // Открытые методы

        #region Закрытые методы
        private FastReport.Report CreateReport()
        {
            var report = new Report();
         
            if (!_reportItemM.IsNew)
            {
                report.LoadFromString(_reportItemM.Body);
            }
            var postgresConn = FindConnection(report, "Current data") as PostgresDataConnection;
            if (postgresConn == null)
            {
                postgresConn = new PostgresDataConnection()
                {
                    Name = "Current data",
                    Enabled = true
                };
                report.Dictionary.Connections.Add(postgresConn);
            }
            postgresConn.ConnectionString = Program.connString.ToString();

            Interfaces.tablesInfo tableInfo = null;
            if (_reportItemM.Type == enTypeReport.Table || _reportItemM.Type == enTypeReport.Object)
                tableInfo = Program.app.getTableInfo(_filter.IdTable);

            if (_isDesign)
            {
                cti.ThreadProgress.ShowWait("CreateReport");
                foreach (var item in _reports)
                {
                    try
                    {
                        var table = FindTableDataSource(postgresConn, item);
                        if (table == null)
                        {
                            table = CreateTableDataSource(item);
                            table.Enabled = (_reportItemM.Type == enTypeReport.Object || _reportItemM.Type == enTypeReport.Table);
                            postgresConn.Tables.Add(table);
                        }

                        if (item.Table.photo)
                        {
                            var tmPhoto = new TableModel_M(item.Table, item.Type, true);
                            var tablePhoto = FindTablePhotoDataSource(postgresConn, tmPhoto);
                            if (tablePhoto == null)
                            {
                                tablePhoto = CreateTablePhotoDataSource(tmPhoto);
                                tablePhoto.Enabled = false;
                                postgresConn.Tables.Add(tablePhoto);
                            }


                            //var pkColumn = table.Columns.FindByName(item.Table.pkField);
                            //var pkColumnPhoto = tablePhoto.Columns.FindByName("id_obj");
                            //var relName = table.TableName + "_" + item.Table.nameMap + "_photo";


                            //RelationCollection
                            //var ds = new DataSet();
                            //ds.Tables.Add(table);
                            //ds.Tables.Add(tablePhoto);
                            //var d = new DataRelation(relName, pkColumn, pkColumnPhoto);

                            //report.RegisterData(d, relName);

                            // Declare all the tables that will have relations
                            var nameRelation = table.TableName + "_" + item.Table.nameMap + "_photo";
                            Relation rel = report.Dictionary.Relations.FindByName(nameRelation);
                            if (rel == null)
                            {
                                var ParentTable = postgresConn.FindObject(item.Table.nameDB) as TableDataSource;
                                var PhotoTable = postgresConn.FindObject(item.Table.nameDB + "_photo") as TableDataSource;


                                // Declare the relations
                                rel = new Relation();
                                rel.Enabled = false;
                                rel.Name = nameRelation;
                                rel.ParentDataSource = ParentTable;
                                rel.ChildDataSource = PhotoTable;
                                rel.ParentColumns = new string[] { item.Table.pkField };
                                rel.ChildColumns = new string[] { Resources.Report_VM_photoIdObj };
                                report.Dictionary.Relations.Add(rel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "NpgsqlException");
                        continue;
                    }
                }
                cti.ThreadProgress.Close("CreateReport");
            }
            //if (tableInfo != null)
            //report.SetParameterValue("idCurrentObject", "\"" + tableInfo.pkField + "\" = " + _filter.IdObj);
            return report;
        }

        private static DataConnectionBase FindConnection(FastReport.Report report, string name)
        {
            foreach (DataConnectionBase item in report.Dictionary.Connections)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }
        private static TableDataSource FindTableDataSource(PostgresDataConnection pgConnFR, TableModel_M tm)
        {
            foreach (TableDataSource item in pgConnFR.Tables)
            {
                if (item.Name == tm.Table.nameDB)
                    return item;
            }
            return null;
        }

        private static TableDataSource FindTablePhotoDataSource(PostgresDataConnection pgConnFR, TableModel_M tm)
        {
            foreach (TableDataSource item in pgConnFR.Tables)
            {
                if (item.Name == tm.Table.nameDB + "_photo")
                    return item;
            }
            return null;
        }
        private static TableDataSource CreateTableDataSource(TableModel_M tm)
        {
            var reportTable = new TableDataSource()
            {
                Name = tm.Table.nameDB,
                TableName = tm.Table.nameDB,
                Alias = tm.Table.nameMap.Replace(".", "_")
            };

            string sqlStr;
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format("SELECT sys_scheme.get_sql_for_table_with_geom({0}, {1});", tm.Table.idTable, Program.srid);
                sqlStr = sqlCmd.ExecuteScalar<string>();
            }

            string tableSqlStr = string.Format("SELECT \n\t*\n FROM\n\t ({0}) as ss\nWHERE {2}\n{1}", sqlStr, "{0}", "{1}");

            string where = GetWhere(tm);
            string orderby = GetOrderby(tm);
            reportTable.SelectCommand = string.Format(tableSqlStr, orderby, where);
            var paramWhere = new CommandParameter()
            {
                Name = "WhereQuestionSQL",
                DataType = 2,
                Value = true
            };
            var paramOrder = new CommandParameter()
            {
                Name = "OrderQuestionSQL",
                DataType = 19,
                Value = tm.Table.pkField
            };


            reportTable.Parameters.Add(paramWhere);
            reportTable.Parameters.Add(paramOrder);



            using (SqlWork sqlCmd = new SqlWork(true))
            {
                DataTable table;
                sqlCmd.sql = string.Format(tableSqlStr, "", "1 = 1") + " LIMIT 0";
                try
                {
                    table = sqlCmd.ExecuteGetTable();
                    foreach (DataColumn field in table.Columns)
                    {
                        var ReportColumn = new Column();
                        ReportColumn.Name = field.Caption;
                        ReportColumn.DataType = field.DataType;
                        reportTable.Columns.Add(ReportColumn);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            // Заполняем названия колонок 
            foreach (Column item in reportTable.Columns)
            {
                var field = tm.Table.ListField.FirstOrDefault(f => f.nameDB == item.Name);
                if (field != null)
                {
                    item.Alias = field.nameMap;
                    continue;
                }
                else if (item.Name.StartsWith("id!"))
                {
                    var fieldKey = tm.Table.ListField.FirstOrDefault(f => ("id!" + f.nameDB) == item.Name);
                    item.Alias = string.Format(Resources.Report_VM_KeyAliasFormat, fieldKey.nameMap);
                }
                else if (item.Name.StartsWith("geom!"))
                {
                    switch (item.Name)
                    {
                        case "geom!type": item.Alias = Resources.Report_VM_geomType; break;
                        case "geom!area": item.Alias = Resources.Report_VM_geomArea; break;
                        case "geom!perimeter": item.Alias = Resources.Report_VM_geomPerimeter; break;
                        case "geom!length": item.Alias = Resources.Report_VM_geomLength; break;
                        case "geom!center_x": item.Alias = Resources.Report_VM_geomCenter_x; break;
                        case "geom!center_y": item.Alias = Resources.Report_VM_geomCenter_y; break;
                    }
                }
            }

            //foreach (var field in )
            //{
            //    var column = table.Columns[field.nameDB];
            //    if (column != null)
            //        column.Caption = field.nameMap;

            //    column = table.Columns["id!" + field.nameDB];
            //    if (column != null)
            //        column.Caption = field.nameMap + "(id)";
            //}

            return reportTable;
        }

        private static string GetOrderby(TableModel_M tm)
        {
            string orderby = string.Empty;
            if (!tm.IsFileTable)
            {
                switch (tm.Type)
                {
                    case enTypeReport.Table:
                    case enTypeReport.Object:
                        {
                            orderby = "ORDER BY @OrderQuestionSQL";
                        } break;
                }
            }
            return orderby;
        }

        private static string GetWhere(TableModel_M tm)
        {
            string where;
            //if (!tm.IsFileTable)
            //{
            switch (tm.Type)
            {
                case enTypeReport.Table:
                case enTypeReport.Object:
                    {
                        where = "@WhereQuestionSQL";
                    } break;
                default:
                    {
                        where = "1 = 1";
                    } break;
            }
            if (tm.Table.TypeTable == Interfaces.TypeTable.LayerMap)
            {
                //where += string.Format(" AND ST_IsValid(\"{0}\")", tm.Table.geomFieldName);
            }
            //}
            //else
            //{
            //    where = "1 = 1";
            //}
            return where;
        }

        private static TableDataSource CreateTablePhotoDataSource(TableModel_M tmPhoto)
        {
            var reportTable = new TableDataSource()
            {
                Name = tmPhoto.Table.nameDB + "_photo",
                TableName = tmPhoto.Table.nameDB + "_photo",
                Alias = tmPhoto.Table.nameMap + "_photo"
            };
            var fileInfo = tmPhoto.Table.PhotoInfo;

            string sqlStr;
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format("SELECT sys_scheme.get_sql_for_table({0});", tmPhoto.Table.idTable);
                sqlStr = sqlCmd.ExecuteScalar<string>();
            }
            string fileSqlString = string.Format(@"
SELECT
    ""t!file"".id, 
    ""t!file"".""{0}"" AS id_obj,
    ""t!file"".dataupd AS date_update,
    ""t!file"".img_preview AS preview,
    ""t!file"".""{1}""  AS file_body,
    ""t!file"".file_name AS file_name
FROM 
    (   
  SELECT *
  FROM
  (   
{2}
  ) AS ""t!gen_sql""
        WHERE {3}
    ) AS ""t!main""
    RIGHT JOIN ""{4}"".""{5}"" AS ""t!file""
            ON (""t!main"".{6} = ""t!file"".""{0}"")
WHERE ""t!file"".is_photo = true
",
                            fileInfo.namePhotoField,
                            fileInfo.namePhotoFile,
                            sqlStr,
                            "{0}",
                            tmPhoto.Table.nameSheme,
                            fileInfo.namePhotoTable,
                            tmPhoto.Table.pkField);

            string where = GetWhere(tmPhoto);
            reportTable.SelectCommand = string.Format(fileSqlString, where);

            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format(fileSqlString, "1 = 1") + " LIMIT 0";
                DataTable fileTable = sqlCmd.ExecuteGetTable();

                foreach (DataColumn field in fileTable.Columns)
                {
                    var ReportColumn = new Column();
                    ReportColumn.Name = field.Caption;
                    ReportColumn.DataType = field.DataType;
                    reportTable.Columns.Add(ReportColumn);
                }
            }

            // Заполняем названия колонок 
            foreach (Column item in reportTable.Columns)
            {
                switch (item.Name)
                {
                    case "id_obj": item.Alias = Resources.Report_VM_photoIdObj; break;
                    case "date_update": item.Alias = Resources.Report_VM_photoDateUpdate; break;
                    case "preview": item.Alias = Resources.Report_VM_photoPreview; break;
                    case "file_body": item.Alias = Resources.Report_VM_photoFileBody; break;
                    case "file_name": item.Alias = Resources.Report_VM_photoFileName; break;
                }
            }

#if DEBUG
            //if (tmPhoto.Type != enTypeReport.All)
            //UpdateDateTable(fileTable);
#endif
            var paramWhere = new CommandParameter() { Name = "WhereQuestionSQL", DataType = 2, Value = true };
            reportTable.Parameters.Add(paramWhere);
            return reportTable;
        }



        #endregion // Закрытые методы


    }
}
