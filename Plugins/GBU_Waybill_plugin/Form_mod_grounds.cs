using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GBU_Waybill_plugin
{
    public partial class Form_mod_grounds : Form
    {
        public Form_mod_grounds(string data)
        {
            InitializeComponent();
            this.data = data;

        }

        string data;

        private void Form_mod_grounds_Load(object sender, EventArgs e)
        {
            //this.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width - this.Size.Width) / 2, (Screen.PrimaryScreen.Bounds.Size.Height - this.Size.Height) / 2);
            err_body.Text = data;
            this.panel3.Height = label1.Height + err_body.Height;
            BTN_save.DialogResult = DialogResult.OK;
            BTN_cancel.DialogResult = DialogResult.Cancel;
            this.AcceptButton = BTN_save;
            this.CancelButton = BTN_cancel;

        }
        public string M_grounds
        {
            get
            {
                return grounds.Text;
            }
        }
    }
}
