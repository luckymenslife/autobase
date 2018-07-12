using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Interfaces.FastReport
{
    public interface IReport_M
    {
        IReportItem_M FindReportById(int idReport);
        IEnumerable<IReportItem_M> FindReportsByIdTable(int idTable);
        void OpenReportEditor(IFilterTable filter, enTypeReport typeEditor);
        ReadOnlyObservableCollection<IReportItem_M> ListReports { get; }
        void OpenReport(IReportItem_M report, IFilterTable filter);
        void Reload();
    }
}
