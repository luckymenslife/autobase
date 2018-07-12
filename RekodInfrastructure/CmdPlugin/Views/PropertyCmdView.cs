using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rekod.CmdPlugin;
using Rekod.CmdPlugin.Model;
using Rekod.CmdPlugin.Module;

namespace Rekod.CmdPlugin.Views
{
    public partial class PropertyCmdView : Form
    {
        private PropertyCall _propertyCall;

        public PropertyCmdView(PropertyCall propertyCall, bool isNew)
        {
            InitializeComponent();

            if (isNew)
                this.Text = Rekod.Properties.Resources.CMD_NewProgram;
            else
                this.Text = Rekod.Properties.Resources.CMD_SettingsProgram;

            if (propertyCall == null) return;
            _propertyCall = propertyCall;
            this.txtIcon.Text = _propertyCall.Icon;
            this.txtFile.Text = _propertyCall.File;
            this.txtTitle.Text = _propertyCall.Title;
            this.txtParams.Text = _propertyCall.Params;

            this.pIcon.BackgroundImage = IconModule.GetIcon(txtIcon.Text, txtFile.Text).ToBitmap();
        }


        private bool CanPropertyCallUpdate()
        {
            if (string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                MessageBox.Show(Rekod.Properties.Resources.CMD_EnterNameOfProgram);
                return false;
            }
            if (string.IsNullOrEmpty(txtFile.Text.Trim()))
            {
                MessageBox.Show(Rekod.Properties.Resources.CMD_EnterFile);
                return false;
            }

            return true;
        }

        private void PropertyCallUpdate()
        {
            _propertyCall.Title = txtTitle.Text.Trim();
            _propertyCall.Icon = txtIcon.Text.Trim();
            _propertyCall.File = txtFile.Text.Trim();
            _propertyCall.Params = txtParams.Text.Trim();
        }


        private void btnIcon_Click(object sender, EventArgs e)
        {
            var dial = new OpenFileDialog();
            if (dial.ShowDialog() != DialogResult.OK)
                return;

            txtIcon.Text = GetRelativePath(dial.FileName);
            this.pIcon.BackgroundImage = IconModule.GetIcon(txtIcon.Text, txtFile.Text).ToBitmap();
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            var dial = new OpenFileDialog();
            if (dial.ShowDialog() != DialogResult.OK)
                return;

            txtFile.Text = GetRelativePath(dial.FileName);
            this.pIcon.BackgroundImage = IconModule.GetIcon(txtIcon.Text, txtFile.Text).ToBitmap();
        }

        private string GetRelativePath(string path)
        {
            if (path.ToLower().StartsWith(Application.StartupPath.ToLower()))
                path = path.Remove(0, Application.StartupPath.Count());
            return path.TrimStart('\\');
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!CanPropertyCallUpdate()) return;
            PropertyCallUpdate();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
