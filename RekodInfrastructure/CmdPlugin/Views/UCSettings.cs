using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Rekod.CmdPlugin.Module;
using Rekod.CmdPlugin.Model;
using Rekod.PluginSettings;

namespace Rekod.CmdPlugin.Views
{
    public delegate void ButtonDelegate(XElement settings);

    public partial class UCSettings : Form
    {

        public event ButtonDelegate Save;
        private XElement _settings;

        public UCSettings(XElement settings)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            //listBox1.DisplayMember = "Name";
            _settings = settings;
            LoadSettings();
            UpdateCount();
        }

        #region Управление PropertyCall

        private void PropertyCallAdd(PropertyCall value)
        {
            if (value == null)
            {
                value = new PropertyCall();
                if (!PropertyCallOpen(value, true)) return;
            }
            listBox1.Items.Add(value);
            SaveSettings();
            UpdateCount();
        }

        private void PropertyCallDelelte(PropertyCall value)
        {
            if (value == null) return;
            var dialogResult = MessageBox.Show(this.ParentForm, Rekod.Properties.Resources.CMD_DeleteConfirm,
                                                Rekod.Properties.Resources.CMD_Confirmation,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                listBox1.Items.Remove(value);
                SaveSettings();
            }
            UpdateCount();
        }

        private void PropertyCallUp(PropertyCall value)
        {
            if (value == null) return;
            int index = listBox1.Items.IndexOf(value);
            if (index == 0) return;
            listBox1.Items.Remove(value);
            listBox1.Items.Insert(index - 1, value);
            listBox1.SetSelected(index - 1, true);
            SaveSettings();
        }

        private void PropertyCallDown(PropertyCall value)
        {
            if (value == null) return;
            int index = listBox1.Items.IndexOf(value);
            if (index == listBox1.Items.Count - 1) return;
            listBox1.Items.Remove(value);
            listBox1.Items.Insert(index + 1, value);
            listBox1.SetSelected(index + 1, true);
            SaveSettings();
        }

        private void PropertyCallUpdate(PropertyCall value)
        {
            if (value == null) return;
            int index = listBox1.Items.IndexOf(value);
            listBox1.Items.Remove(value);
            listBox1.Items.Insert(index, value);
            listBox1.SetSelected(index, true);
            SaveSettings();
        }

        private bool PropertyCallOpen(PropertyCall value, bool isNew)
        {
            if (value == null) return false;
            var property = new PropertyCmdView(value, isNew);
            if (property.ShowDialog() == DialogResult.OK)
                return true;

            return false;
        }

        string textNum(uint value, string val1, string val234, string val)
        {
            uint endValue = value % 100;

            if (endValue > 10 && endValue < 20)
                return val;

            uint firstNum = value % 10;

            switch (firstNum)
            {
                case 1:
                    return val1;
                case 2:
                case 3:
                case 4:
                    return val234;
                default:
                    return val;
            }
        }

        void UpdateCount()
        {
            var count = (uint)listBox1.Items.Count;
            labelCount.Text = Rekod.Properties.Resources.LocCount + ": " + count;
            //labelCount.Text = count + " " + textNum(count, "программа", "программы", "программ");
        }
        #endregion



        #region IControlSettings
        public bool LoadSettings()
        {
            listBox1.Items.Clear();

            var listPropertyCall = XMLModule.ReadXSettings(_settings);
            foreach (PropertyCall t in listPropertyCall)
            {
                PropertyCallAdd(t);
            }
            return true;
        }

        public bool SaveSettings()
        {
            _settings.ReplaceNodes(XMLModule.WriteXSettings(listBox1.Items.OfType<PropertyCall>()));
            if (Save != null)
                Save(_settings);
            Program.SettingsXML.ApplyPlugin(MainCallPlugin.GUID, _settings);
            return true;
        }
        #endregion


        #region События формы
        private void btnAdd_Click(object sender, EventArgs e)
        {
            PropertyCallAdd(null);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            PropertyCallDelelte(listBox1.SelectedItem as PropertyCall);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            PropertyCallUp(listBox1.SelectedItem as PropertyCall);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            PropertyCallDown(listBox1.SelectedItem as PropertyCall);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            var rect = listBox1.GetItemRectangle(listBox1.SelectedIndex);
            if (!rect.Contains(e.Location)) return;

            PropertyCallOpen(listBox1.SelectedItem as PropertyCall, false);

            PropertyCallUpdate(listBox1.SelectedItem as PropertyCall);
        }
        #endregion



    }
}
