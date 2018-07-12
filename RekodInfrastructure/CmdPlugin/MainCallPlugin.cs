using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Rekod.CmdPlugin.Model;
using Rekod.CmdPlugin.Module;
using Rekod.CmdPlugin.Views;
using Interfaces;
using Rekod.ViewModel;

namespace Rekod.CmdPlugin
{
    public class MainCallPlugin
    {
        private string _guid;

        public static string GUID = "04da2745-676d-4b32-899f-e54c31821d4d";
        private XElement _setting;
        public void StartPlugin(XElement xSettings, IMainApp app, IWorkClass work)
        {
            _setting = xSettings;
            _listToolStripItem = new Dictionary<ToolButton_VM, PropertyCall>();
            Program.app = app;
            Program.work = work;
            AddConrolPanelTools(XMLModule.ReadXSettings(xSettings));
        }

        public Views.UCSettings SettingsForm
        {
            get
            {
                var ucSettings = new UCSettings(_setting);
                ucSettings.Save += new ButtonDelegate(ucSettings_Save);
                return ucSettings;
            }
        }

        void ucSettings_Save(XElement settings)
        {
            AddConrolPanelTools(XMLModule.ReadXSettings(settings));
        }


        private Dictionary<ToolButton_VM, PropertyCall> _listToolStripItem;

        private void AddConrolPanelTools(ICollection<PropertyCall> listPropertyCall)
        {
            if (listPropertyCall == null)
                return;
            var toolBar = Program.BManager.FindToolBar("TbFastStart");
            toolBar.ListButton.Clear();
            _listToolStripItem.Clear();
            foreach (var item in listPropertyCall.Reverse())
            {
                Icon icon = IconModule.GetIcon(item.Icon, item.File);
                ToolButton_VM button = new ToolButton_VM(toolBar, item.Title, ToolButton_Click);
                button.Image = (icon != null)
                            ? icon.ToBitmap()
                            : null;
                toolBar.ListButton.Add(button);
                _listToolStripItem.Add(button, item);
            }
        }

        void ToolButton_Click(ToolButton_VM tb, object sender)
        {
            if (!_listToolStripItem.ContainsKey(tb))
                return;
            var propertyCall = _listToolStripItem[tb];
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                var procStartInfo = new ProcessStartInfo(propertyCall.File, propertyCall.Params);
                var proc = new Process
                               {
                                   StartInfo = procStartInfo
                               };
                proc.Start();
            }
            catch (Exception ex)
            {
                // Ошибка при запуске приложения. Возможный выход - попросить пользователя самому запустить 
                MessageBox.Show("Ошибка при запуске! Ошибка: " + ex.Message);
            }
        }

        private void toolStripButtonDel_MouseEnter(object sender, EventArgs e)
        {
            //if (_isActionDel == false)
            var toolStripItem = sender as ToolStripItem;
            if (toolStripItem != null)
                toolStripItem.BackgroundImage = global::Rekod.Properties.Resources.pri_vibore_instrumenta2;
        }

        private void toolStripButtonDel_MouseLeave(object sender, EventArgs e)
        {
            //if (_isActionDel == false)
            var toolStripItem = sender as ToolStripItem;
            if (toolStripItem != null) toolStripItem.BackgroundImage = null;
        }
    }



}
