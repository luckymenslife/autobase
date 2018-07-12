using GBU_Waybill_plugin.MTClasses.Tasks.Models;
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
    public partial class InputForm : Form
    {
        InputFormM _data;

        public InputForm(InputFormM data)
        {
            InitializeComponent();
            this._data = data;
            this.Text = _data.Title;
            this.label.Text = _data.LabelText;
            this.text.Text = _data.Text;
        }
        public InputFormM Data
        {
            get { return _data; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this._data.Text = this.text.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
