using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Rekod.HelpFiles
{
    public partial class LicenseForm : Form
    {
        public LicenseForm()
        {
            InitializeComponent();
            webBrowser1.Url = new Uri(Application.StartupPath + "\\licensing.htm");
        }
    }
}
