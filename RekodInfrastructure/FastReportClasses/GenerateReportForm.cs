using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastReport;

namespace Rekod.FastReportClasses
{
    public partial class GenerateReportForm : Form
    {
        private Report_VM _reportVM;

        public GenerateReportForm(Report_VM reportVM)
        {
            InitializeComponent();

            _reportVM = reportVM;
            designerControl1.Report = reportVM.Report;
            designerControl1.Tag = _reportVM;
            designerControl1.cmdSaveAs.CustomAction += cmdSaveAs_CustomAction;

            SetTitleForm(_reportVM);

        }

        void cmdSaveAs_CustomAction(object sender, EventArgs e)
        {
            _reportVM.SaveReport(true);
        }


        void GenerateReportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_reportVM.ReportItemM.Body == _reportVM.Report.ReportResourceString)
            {
                return;
            }
            var res = _reportVM.SaveReportDialog();
            if (res == null)
                e.Cancel = true;
            else if (res == true)
            {
                _reportVM.SaveReport();
                SetTitleForm(_reportVM);
            }
        }
        void environmentSettings1_CustomSaveDialog(object sender, FastReport.Design.OpenSaveDialogEventArgs e)
        {
            //Нужен для блокирования стандартного окна сохранения отчета
        }
        void environmentSettings1_CustomSaveReport(object sender, FastReport.Design.OpenSaveReportEventArgs e)
        {
            _reportVM.SaveReport();
            SetTitleForm(_reportVM);
        }

        private void environmentSettings1_CustomOpenReport(object sender, FastReport.Design.OpenSaveReportEventArgs e)
        {
            designerControl1.Report.ShowPrepared();
        }

        private void SetTitleForm(Report_VM report)
        {
            if (report.ReportItemM.IsNew)
                this.Text = "Новый отчет";
            else
                this.Text = "Отчет: " + report.ReportItemM.Caption;
        }
    }
}
