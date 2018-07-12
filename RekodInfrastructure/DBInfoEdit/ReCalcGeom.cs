using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod
{
    public partial class ReCalcGeom : Form
    {
        private int _oldSrid;
        private int _newSrid; 

        public ReCalcGeom(int oldSrid, int newSrid)
        {
            _oldSrid = oldSrid;
            _newSrid = newSrid; 
            InitializeComponent();
        }

        private void ReCalcGeom_Load(object sender, EventArgs e)
        {
            CenterToParent(); 
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}