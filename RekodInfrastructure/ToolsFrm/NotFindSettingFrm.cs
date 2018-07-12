using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod.ToolsFrm
{
    public partial class NotFindSettingFrm : Form
    {
        public NotFindSettingFrm()
        {
            InitializeComponent();

            this.pictureBox1.Image = global::Rekod.Properties.Resources.ГС_new_logo; //.a_c48d37de1 //.для_арсенала
            this.Text = "ООО \"Градосервис\" MapEditor GS"; // ГИС Арсенал "ООО \"Градосервис\" GS MapEditor"

            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += " " + Rekod.Properties.Resources.Version + " " + classesOfMetods.GetVersionString;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = Rekod.Properties.Resources.login_msgConfF;
            openFile.FileName = "settings";

            openFile.Filter = "Файл конфигурации|*.mews; *.aews|Все файлы *.*| *.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                this.FileName = openFile.FileName;
                this.SafeFileName = openFile.SafeFileName;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
        public string FileName
        {
            get;
            set;
        }
        public string SafeFileName
        {
            get;
            set;
        }
    }
}
