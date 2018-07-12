using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using FastReport;
using Rekod.Controllers;
using Rekod.Properties;
using Rekod.Services;
using Interfaces.FastReport;

namespace Rekod.FastReportClasses
{
    public class Report_M : Interfaces.FastReport.IReport_M
    {
        #region Поля
        ObservableCollection<IReportItem_M> _reports;
        private ReadOnlyObservableCollection<IReportItem_M> _rnListReports;
        #endregion // Поля

        #region Свойства
        public ObservableCollection<IReportItem_M> ListReports
        {
            get { return _reports; }
        }
        #endregion // Свойства

        #region Конструктор
        public Report_M()
        {
            _reports = new ObservableCollection<IReportItem_M>();
            _rnListReports = new ReadOnlyObservableCollection<IReportItem_M>(_reports);
        }
        #endregion // Конструктор

        #region Открытые методы
        public void Reload()
        {
            var listRepots = new List<IReportItem_M>();
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = @"
                                    SELECT 
                                        id, 
                                        type_report, 
                                        body, 
                                        id_table, 
                                        caption 
                                    FROM 
                                        sys_scheme.report_templates
                                    ORDER BY id";

                if (!sqlCmd.ExecuteReader())
                    return;
                while (sqlCmd.CanRead())
                {
                    var id = sqlCmd.GetInt32("id");

                    ReportItem_M rep = FindReportById(id) as ReportItem_M;
                    if (rep == null)
                    {
                        var type = sqlCmd.GetValue<enTypeReport>("type_report");
                        var idTable = sqlCmd.GetValue<int?>("id_table");

                        rep = new ReportItem_M(id, type, idTable);
                        rep.Caption = sqlCmd.GetString("caption");
                    }
                    else
                    {
                        string caption = sqlCmd.GetString("caption");
                        if (caption != rep.Caption)
                        {
                            rep = new ReportItem_M(rep);
                            rep.Caption = caption;
                        }
                    }
                    rep.Body = sqlCmd.GetString("body");

                    listRepots.Add(rep);
                }
                ExtraFunctions.Sorts.SortList(_reports, listRepots);

            }
        }

        /// <summary>
        /// Добавить или изменить рабочий набор
        /// </summary>
        /// <param name="workSet">рабочий набор</param>
        public ReportItem_M Apply(ReportItem_M report)
        {
            int idReport;
            using (var sqlCmd = new SqlWork())
            {
                if (report.IsNew)
                {
                    sqlCmd.sql = @"
                            INSERT INTO 
                                sys_scheme.report_templates 
                            (
                                type_report, body, caption, id_table
                            ) VALUES (
                                :type_report, :body, :caption, :id_table
                            ) RETURNING 
                                id;";
                }
                else
                {
                    sqlCmd.sql = @"
                            UPDATE 
                                sys_scheme.report_templates 
                            SET
                                type_report = :type_report,
                                caption = :caption,
                                body = :body
                            WHERE 
                                id = :id
                            RETURNING 
                                id;";
                }
                var id = (report.IdTable == -1)
                        ? (int?)null
                        : report.IdTable;
                sqlCmd.AddParam(":id", report.IdReport, DbType.Int32);
                sqlCmd.AddParam(":type_report", (int)report.Type, DbType.Int32);
                sqlCmd.AddParam(":caption", report.Caption, DbType.String);
                sqlCmd.AddParam(":body", report.Body, DbType.String);
                sqlCmd.AddParam(":id_table", id, DbType.Int32);

                idReport = sqlCmd.ExecuteScalar<int>();
                if (report.IsNew)
                    ReportItem_M.SetReportId(report, idReport);
            }
            Reload();
            return report;
        }
        public void Delete(IReportItem_M report)
        {
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = @"
                        DELETE FROM 
                            sys_scheme.report_templates 
                        WHERE 
                            id = :id;";

                sqlCmd.AddParam(":id", report.IdReport, DbType.Int32);
                sqlCmd.ExecuteNonQuery();
            }
            Reload();
        }

        public void OpenDesignObject(IReportItem_M report, IFilterTable filter)
        {
            if (report.Type != enTypeReport.Object)
                throw new Exception(Resources.Report_M_ErrorDesign);


            var reportVM = new Report_VM((report as ReportItem_M), filter);

            var ti = Program.app.getTableInfo((int)report.IdTable);
            var tm = new TableModel_M(ti, enTypeReport.Object);
            tm.Filter = filter;
            reportVM.Reports.Add(tm);

            reportVM.OpenDesign();
        }
        public void OpenDesignTable(IReportItem_M report, IFilterTable filter)
        {
            if (report.Type != enTypeReport.Table)
                throw new Exception(Resources.Report_M_ErrorDesign);

            var reportVM = new Report_VM((report as ReportItem_M), filter);

            var ti = Program.app.getTableInfo((int)report.IdTable);
            var tm = new TableModel_M(ti, enTypeReport.Table);
            tm.Filter = filter;
            reportVM.Reports.Add(tm);
            reportVM.OpenDesign();
        }

        public void OpenReport(IReportItem_M report, IFilterTable filter)
        {
            var reportVM = new Report_VM((report as ReportItem_M), filter);

            switch (report.Type)
            {
                case enTypeReport.Table:
                case enTypeReport.Object:
                    {
                        var ti = Program.app.getTableInfo((int)report.IdTable);
                        var tm = new TableModel_M(ti, report.Type);
                        tm.Filter = filter;
                        reportVM.Reports.Add(tm);
                    } break;
                case enTypeReport.All:
                    {
                        foreach (var item in Program.app.tables_info)
                        {
                            var tm = new TableModel_M(item, enTypeReport.All);
                            reportVM.Reports.Add(tm);
                        }
                    } break;
            }

            reportVM.OpenReport();
        }
        internal void OpenDesignAll(ReportItem_M report, IFilterTable filter)
        {
            if (report.Type != enTypeReport.All)
                throw new Exception(Resources.Report_M_ErrorDesign);

            var reportVM = new Report_VM((ReportItem_M)report.Clone(), filter);

            foreach (var item in Program.app.tables_info)
            {
                var tm = new TableModel_M(item, enTypeReport.All);
                reportVM.Reports.Add(tm);
            }
            reportVM.OpenDesign();
        }
        public void OpenReportEditor(IFilterTable filter, enTypeReport typeEditor)
        {
            var vm = new ReportEditor_VM(this, filter, typeEditor);
            vm.Reload();
            var v = new ReportEditor_V();
            v.Owner = Program.WinMain;
            v.DataContext = vm;
            v.Show();
        }

        public ReportItem_M NewReport(enTypeReport enTypeReport, int? table)
        {
            return new ReportItem_M(enTypeReport, table);
        }
        public IReportItem_M FindReportById(int idReport)
        {
            return _reports.FirstOrDefault(f => f.IdReport == idReport);
        }
        public IEnumerable<IReportItem_M> FindReportsByIdTable(int idTable)
        {
            return _reports.Where(f => f.IdTable.HasValue && f.IdTable == idTable);
        }
        public bool FindNameMatch(IReportItem_M report)
        {
            return _reports
                .Any(f =>
                    f.Caption == report.Caption
                    && f.IdReport != report.IdReport
                    && f.Type == report.Type
                    && (report.Type == enTypeReport.All
                        || report.IdTable == null
                        || report.IdTable == f.IdTable));
        }

        public bool AccessChecked(ReportItem_M CurrentReport)
        {
            return Program.user_info.admin;
        }
        #endregion // Открытые методы

        #region Закрытые методы
        #endregion // Закрытые методы

        ReadOnlyObservableCollection<IReportItem_M> IReport_M.ListReports
        {
            get { return _rnListReports; }
        }
    }
}
