using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using ExtraFunctions;

namespace Rekod.PluginSettings
{
    public partial class PluginSettingsForm : Form
    {
        Interfaces.Forms.IControlSettings control;
        Interfaces.IMainPlugin plugin;

        private UserSettingsControl uSetting;
        private Proxy.ProxySettingsControl uProxySetting;

        public PluginSettingsForm()
        {
            InitializeComponent();
            classesOfMetods.SetFormOwner(this);
            // Создание начальных узлов в дереве
            uSetting = new UserSettingsControl();
            uProxySetting = new Proxy.ProxySettingsControl();
            LoadTreeNodes();
            //loadPliginList();
        }
        private void LoadTreeNodes()
        {
            var tnUserSettings = treeView1.Nodes["tnUserSettings"];
            var tnProxySettings = tnUserSettings.Nodes["tnProxySettings"];
            var tnPluginsSettings = treeView1.Nodes["tnPluginsSettings"];

            tnUserSettings.Text = Rekod.Properties.Resources.PluginSettingsForm_tnUserSettings;
            tnProxySettings.Text = Rekod.Properties.Resources.PluginSettingsForm_tnProxySettings;
            tnPluginsSettings.Text = Rekod.Properties.Resources.PluginSettingsForm_tnPluginsSettings;

            tnUserSettings.Expand();
            tnPluginsSettings.Nodes.Clear();

            for (int i = 0; Plugins.ListMainPlugins.Count > i; i++)
            {
                if (Plugins.ListMainPlugins[i].SettingsForm != null)
                {
                    TreeNode item = new TreeNode(Plugins.ListMainPlugins[i].Name);
                    item.Tag = Plugins.ListMainPlugins[i];
                    tnPluginsSettings.Nodes.Add(item);
                }
            }
            tnPluginsSettings.Expand();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void ReloadSettings()
        {
            var guid = string.Empty;
            if (control != null)
            {
                if (plugin != null)
                    guid = plugin.GUID;
                else
                {
                    if (treeView1.SelectedNode.Name == "tnUserSettings")
                        guid = Rekod.Repository.SettingsFile.LocalParameters_M.Guid;
                    else if (treeView1.SelectedNode.Name == "tnProxySettings")
                        guid = Rekod.Repository.SettingsFile.ProxyParameters_M.Guid;
                }
            }
            if (guid != string.Empty)
            {
                var currentXML = Program.SettingsXML.GetPlugin(guid);
                control.LoadSettings(currentXML);
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (control != null)
                if (control.SaveSettings())
                {
                    Program.SettingsXML.ApplyLastChange();

                    this.Close();
                }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Name == "tnUserSettings")
            {
                panel2.Controls.Clear();
                control = uSetting;
                ReloadSettings();
                var setting = control as UserControl;
                if (setting == null)
                {
                    setting = control.GetUserControl();
                }
                if (setting != null)
                {
                    panel2.Controls.Add(setting);
                    setting.Dock = DockStyle.Fill;
                }
                btnSave.Enabled = true;
            }
            else if (e.Node.Name == "tnProxySettings")
            {
                panel2.Controls.Clear();
                control = uProxySetting;
                ReloadSettings();
                var setting = control as UserControl;
                if (setting == null)
                {
                    setting = control.GetUserControl();
                }
                if (setting != null)
                {
                    panel2.Controls.Add(setting);
                    setting.Dock = DockStyle.Top;
                }
                btnSave.Enabled = true;
            }
            else
            {
                if (e.Node.Tag != null)
                {
                    panel2.Controls.Clear();
                    plugin = (Interfaces.IMainPlugin)e.Node.Tag;

                    control = plugin.SettingsForm;
                    var ucControl = control as UserControl;
                    if (ucControl == null)
                    {
                        ucControl = control.GetUserControl();
                    }
                    if (ucControl != null)
                    {
                        ucControl.Dock = DockStyle.Fill;
                        ReloadSettings();
                        panel2.Controls.Add(ucControl);
                    }
                    btnSave.Enabled = true;
                }
                else
                {
                    btnSave.Enabled = false;
                    panel2.Controls.Clear();
                    ReloadSettings();
                }
            }
        }
    }
}