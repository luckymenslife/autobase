using GBU_Waybill_plugin.MTClasses.Tasks.ViewModels;
using GBU_Waybill_plugin.MTClasses.Tasks.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GBU_Waybill_plugin.MTClasses.Tasks
{
    public partial class TasksTableForm : Form
    {
        public TasksTableForm()
        {
            InitializeComponent();
            TasksTableVM data = new TasksTableVM(idOrg());
            tasksTableV1.DataContext = data;
        }
        private int idOrg()
        {
            int result = -1;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT autobase.get_my_org();";
                result = sqlCmd.ExecuteScalar<int>();
            }
            return result;
        }

    }
}
