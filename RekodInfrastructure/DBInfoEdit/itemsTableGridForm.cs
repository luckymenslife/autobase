using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace Rekod
{
    public partial class itemsTableGridForm : Form
    {
        public UserControl control;
        Interfaces.UserControls.IUserControlMain UControl;
        private int _idObj = -1;
        public int idObj
        {
            get
            {
                if (_idObj != -1)
                    return _idObj;

                var t = control as UcTableObjects;
                return (t == null) ? -1 : t.IdObj;
            }
        }
        public itemsTableGridForm(int idT1, int? goToObject = null, bool isSelected = false, int? callerTableId = null, int? callerObjectId = null, bool setOwner = true)
        {
            InitializeComponent();

            //Rekod.Services.ServiceClass.ActivateOwnerOnClose(this);
            if (setOwner)
                classesOfMetods.SetFormOwner(this);

            tablesInfo table = classesOfMetods.getTableInfo(idT1);

            this.UControl = Plugins.GetControl(goToObject, isSelected, table, this, callerTableId, callerObjectId);
            if (UControl != null)
            {
                UControl.CloseForm += (o, e) =>
                {
                    if (isSelected)
                    {
                        object pk = UControl.ViewModel.GetType().GetProperty("CurrentPK").GetValue(UControl.ViewModel, null);
                        if (pk != null && int.TryParse(pk.ToString(), out _idObj))
                        {
                            DialogResult = System.Windows.Forms.DialogResult.OK;
                        }
                    }
                    s_CloseForm(o, e);
                };
                var uc = UControl.GetUserControl();
                uc.Dock = DockStyle.Fill;
                this.Text = UControl.Title;
                this.Controls.Add(uc);
            }
            else
            {
                this.control = new UcTableObjects(table, goToObject, isSelected: isSelected);
                this.Controls.Add(control);
                this.Text = string.Format(Rekod.Properties.Resources.ITGF_DataTable + " \"{0}\"", table.nameMap);
            }
        }

        void s_CloseForm(object sender, Interfaces.UserControls.eventCloseForm e)
        {
            if (UControl != null)
                UControl.CloseForm -= new EventHandler<Interfaces.UserControls.eventCloseForm>(s_CloseForm);
            this.Close();
        }

        private void itemsTableGridForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (control != null)
            {
                ((UcTableObjects)control).ApplyUserMapFilter(true);
            }
            GC.Collect();
        }

    }
}
