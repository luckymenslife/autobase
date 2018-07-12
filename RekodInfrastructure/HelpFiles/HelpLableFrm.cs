using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod.DBTablesEdit
{
    public partial class HelpLableFrm : Form
    {
        string url_str;
        public HelpLableFrm(string url_str)
        {
            InitializeComponent();
            this.url_str = url_str;
        }

        private void HelpLableFrm_Load(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(url_str);
        }
    }
}
