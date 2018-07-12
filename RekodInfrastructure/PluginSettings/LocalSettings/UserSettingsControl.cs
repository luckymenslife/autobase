using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Rekod.Controllers;
using System.IO;
using System.Globalization;

namespace Rekod.PluginSettings
{
    public delegate void ButtonDelegate(XElement settings);
    public partial class UserSettingsControl : UserControl, Interfaces.Forms.IControlSettings
    {
        public XElement Settings { get; private set; }
        public UserSettingsControl()
        {
            InitializeComponent();
        }
        public UserControl GetUserControl()
        {
            return this;
        }
        public bool LoadSettings(XElement data)
        {
            Settings = data;
            chbEnterTheScreen.Checked = Program.SettingsXML.LocalParameters.EnterTheScreen;
            chbTurnOffVMPWhenRastr.Checked = Program.SettingsXML.LocalParameters.TurnOffVMPWhenRastr;
            chbOpenAttributesAfterObjectCreate.Checked = Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate;
            cbLanguage.DataSource = Rekod.Culture_M.GetCulturesProgram();
            cbLanguage.DisplayMember = "Name";
            cbLanguage.ValueMember = "Culture";
            cbLanguage.SelectedValue = Program.SettingsXML.LocalParameters.LocaleProgram;
            return true;
        }
        public bool SaveSettings()
        {
            Program.SettingsXML.LocalParameters.EnterTheScreen = chbEnterTheScreen.Checked;
            Program.SettingsXML.LocalParameters.TurnOffVMPWhenRastr = chbTurnOffVMPWhenRastr.Checked;
            Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate = chbOpenAttributesAfterObjectCreate.Checked;
            Program.SettingsXML.LocalParameters.LocaleProgram = (CultureInfo)cbLanguage.SelectedValue;
            Program.SettingsXML.ApplyLocalParameters();
            return true;
        }

    }
}