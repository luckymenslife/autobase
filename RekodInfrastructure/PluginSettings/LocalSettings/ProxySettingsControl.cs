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
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using ExtraFunctions;
using Rekod.Repository.SettingsFile;

namespace Rekod.PluginSettings.Proxy
{
    public delegate void ButtonDelegate(XElement settings);
    public partial class ProxySettingsControl : UserControl, Interfaces.Forms.IControlSettings
    {
        XElement _settings;
        eProxyStatus _proxyStatus;

        public ProxySettingsControl()
        {
            InitializeComponent();
        }
        public UserControl GetUserControl()
        {
            return this;
        }
        public bool LoadSettings(XElement data)
        {
            _settings = data;
            if (_settings != null)
            {
                SetFormData(Program.SettingsXML.LocalParameters.Proxy);
            }
            return true;
        }
        public bool SaveSettings()
        {
            if (_settings != null)
            {
                if (CheckFormData())
                {
                    GetFormData(Program.SettingsXML.LocalParameters.Proxy);

                    Program.SettingsXML.ApplyLocalParameters();
                    Rekod.MapFunctions.ProxyParameters.ProxyApplyChanges();

                }
                else
                    return false;
            }
            return true;
        }


        private void rbProxy_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == rbNonProxy && rbNonProxy.Checked == true)
            {
                SetStatusProxy(eProxyStatus.Non);
            }
            else if (sender == rbProxyCustomer && rbProxyCustomer.Checked == true)
            {
                SetStatusProxy(eProxyStatus.Customer);
            }
        }

        void SetStatusProxy(eProxyStatus status)
        {
            bool enable = false;
            _proxyStatus = status;

            switch (status)
            {
                case eProxyStatus.Non:
                    rbNonProxy.Checked = true;
                    enable = false;
                    break;
                case eProxyStatus.Customer:
                    rbProxyCustomer.Checked = true;
                    enable = true;
                    break;
                default:
                    break;
            }

            txtIP.Enabled = enable;
            txtLogin.Enabled = enable;
            txtPassword.Enabled = enable;
            txtPort.Enabled = enable;
        }

        /// <summary>
        /// Заполняет форму данными из прокси
        /// </summary>
        /// <param name="proxy"></param>
        void SetFormData(ProxyParameters_M proxy)
        {
            SetStatusProxy(proxy.Status);
            txtIP.Text = proxy.IP;
            txtPort.Text = Convert.ToString(proxy.Port);
            txtLogin.Text = proxy.Login;
            txtPassword.Text = Services.Encrypting.TryDecrypt(proxy.Password, "proxy_pwd");
        }

        /// <summary>
        /// Считывает данные с формы и заполняет прокси
        /// </summary>
        /// <param name="proxy"></param>
        void GetFormData(ProxyParameters_M proxy)
        {
            if (!CheckFormData())
                return;

            proxy.Status = _proxyStatus;
            proxy.IP = txtIP.Text;

            int port = 0;
            int.TryParse(txtPort.Text.Trim(), out port);
            proxy.Port = (port != 0)
                ? port
                : (int?)null;
            proxy.Login = txtLogin.Text;
            proxy.Password = Services.Encrypting.Encrypt(txtPassword.Text, "proxy_pwd");
        }
        /// <summary>
        /// Выполняет проверку на форме всех элементов
        /// </summary>
        /// <returns></returns>
        private bool CheckFormData()
        {
            int port = 0;

            bool ipError = false;
            bool portError = false;
            bool loginError = false;
            bool passwordError = false;

            if (_proxyStatus == eProxyStatus.Customer)
            {
                ipError = string.IsNullOrEmpty(txtIP.Text.Trim());
                portError = !int.TryParse(txtPort.Text.Trim(), out port);
                loginError = string.IsNullOrEmpty(txtLogin.Text);
                passwordError = string.IsNullOrEmpty(txtPassword.Text);
            }

            ControlMarks(txtIP, ipError);
            ControlMarks(txtPort, portError);
            ControlMarks(txtLogin, loginError);
            ControlMarks(txtPassword, passwordError);

            return !ipError && !portError && !loginError && !passwordError;
        }
        /// <summary>
        /// Почечае контрол с ошибками
        /// </summary>
        /// <param name="control"></param>
        /// <param name="isError"></param>
        private void ControlMarks(Control control, bool isError)
        {
            if (isError)
                control.BackColor = Color.IndianRed;
            else
                control.BackColor = SystemColors.Window;
        }

    }
}
