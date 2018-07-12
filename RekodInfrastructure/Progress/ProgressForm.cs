using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using Rekod;

namespace cti
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            //var culture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentUICulture = Program.SettingsXML.LocalParameters.LocaleProgram;

            InitializeComponent();
        }
        public void InThread(MethodInvoker mth)//для того чтобы работать в том потоке в котором находиться форма
        {
            try
            {
                if (IsDisposed)
                    return;
                if (InvokeRequired)
                    Invoke(mth);
                else
                    mth();
            }
            catch { }
        }
    }
}
