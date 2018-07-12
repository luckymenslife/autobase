using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod.HelpFiles
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            //classesOfMetods.SetFormOwner(this);
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            String mhtPath = Application.StartupPath + "\\help_gs_mapeditor.mht";
            if (File.Exists(mhtPath))
            {
                webBrowser1.Url = new Uri(mhtPath);
            }
            else 
            {
                if (Program.user_info.admin)
                {
                    webBrowser1.Url = new Uri("http://dev.gradoservice.ru/manuals/mapeditor/2_12_0/admin");
                }
                else
                {
                    webBrowser1.Url = new Uri("http://dev.gradoservice.ru/manuals/mapeditor/2_12_0/user");
                }
            }

        }
    }
}