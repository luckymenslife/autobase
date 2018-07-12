using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod.DBInfoEdit
{
    public partial class SetSeparatorFrm : Form
    {

        public SetSeparatorFrm()
        {
            InitializeComponent();
            radioButton1.Checked = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !radioButton1.Checked;
        }
        public string Separator
        {
            get
            {
                if (radioButton1.Checked)
                {
                    return "\t";
                }
                if (radioButton2.Checked)
                {
                    return textBox1.Text;
                }
                else
                {
                    return "\t";
                }
            }
        }
    }
}
