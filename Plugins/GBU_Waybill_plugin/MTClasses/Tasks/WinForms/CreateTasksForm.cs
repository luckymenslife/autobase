using GBU_Waybill_plugin.MTClasses.Tasks.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GBU_Waybill_plugin.MTClasses.Tasks.WinForms
{
    public partial class CreateTasksForm : Form
    {
        TasksVM data;
        public CreateTasksForm(int id_org)
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.None;
            data = new TasksVM(id_org);
            this.createTasksV1.DataContext = data;
            data.PropertyChanged += data_PropertyChanged;
            MainPluginClass.AppEvents.PropertyChanged += AppEvents_PropertyChanged;
        }

        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CloseCmd")
            {
                Close();
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CreateTasksForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            data.PropertyChanged -= data_PropertyChanged;
            data.RemoveHandler();
            data.RemoveZoneHandler();
        }
        private void AppEvents_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CreateLayerOdh")
            {
                data.LayerOdh = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).LayerOdh;
                data.Map = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).Map;
                data.MapControl = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).MapControl;
            }
            if (e.PropertyName == "CreateLayerZones")
            {
                data.LayerZone = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).LayerZones;
                data.Map = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).Map;
                data.MapControl = ((MapUc)this.createTasksV1.windowsFormsHost1.Child).MapControl;
            }
        }
    }
}
