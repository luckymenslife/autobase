using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Resources;
using Npgsql;
using Interfaces;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Rekod.Repository.SettingsFile;
using Rekod.Services;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Rekod
{
    public partial class Login : Form
    {
        private NpgsqlConnectionStringBuilder _connectBuilder;
        private List<DataBase_M> _listDB;
        private int? _idUser;

        public Login()
        {
            InitializeComponent();
        }

        public void SetErorr(string text)
        {

            pOutput.Visible = true;
            pOutput.Enabled = true;
            pLogin.Visible = false;
            pLogin.Enabled = false;

            ShowDetail(false);

            txtOutput.Text = text;

        }
        private void ClearErorr()
        {
            pOutput.Visible = false;
            pOutput.Enabled = false;
            pLogin.Visible = true;
            pLogin.Enabled = true;

            ShowDetail(false);

        }
        public void ShowDetail(bool IsOpen)
        {
            if (IsOpen)
            {
                this.Size = new System.Drawing.Size(514, 233);
                txtOutput.Size = new System.Drawing.Size(502, 65);
                txtOutput.Visible = true;
            }
            else
            {
                this.Size = new System.Drawing.Size(496, 161);
                txtOutput.Size = new System.Drawing.Size(502, 65);
                txtOutput.Visible = false;
                txtPass.Focus();
            }
        }

        public void SetListDB(List<DataBase_M> listDB)
        {
            _listDB = listDB;
        }

        public void SetConnectionParams(NpgsqlConnectionStringBuilder conn)
        {
            _connectBuilder = conn;
            var line = ConcatenateServer(conn);

            FillcbServer();
            cbServer.Text = line;
            FillcbLogin(line);

            if (!string.IsNullOrWhiteSpace(line))
                cbLogin.Text = conn.UserName.ToLower();
            else
                cbLogin.Text = string.Empty;

        }
        public void GetConnectionParams(out NpgsqlConnectionStringBuilder conn, out int? idUser)
        {
            //ApplyConnectParams();
            conn = _connectBuilder;
            idUser = _idUser;
        }
        private void ApplyConnectParams()
        {
            ParseServerParams(cbServer.Text.Trim(), _connectBuilder);

            _connectBuilder.UserName = cbLogin.Text.Trim().ToLower();
            _connectBuilder.Password = txtPass.Text;
        }


        private void FillcbServer()
        {
            foreach (var item in _listDB)
            {
                cbServer.Items.Add(item.DataBase);
            }
        }
        private void FillcbLogin(string dBase)
        {
            cbLogin.Items.Clear();
            var db = FindDataBase(dBase);
            if (db != null)
            {
                foreach (var user in db.Logins)
                {
                    cbLogin.Items.Add(user);
                }
                if (cbLogin.Items.Count > 0)
                {
                    cbLogin.SelectedIndex = 0;
                }
            }

        }

        private void Login_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;
            this.Focus();
            this.Activate();


            this.pictureBox1.Image = global::Rekod.Properties.Resources.ГС_new_logo; //.a_c48d37de1 //.для_арсенала
            this.Text = "ООО \"Градосервис\" MapEditor GS"; // ГИС Арсенал "ООО \"Градосервис\" GS MapEditor"
#if DEBUG
            txtPass.PasswordChar = '\0';
#endif

            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += " " + Rekod.Properties.Resources.Version + " " + classesOfMetods.GetVersionString;
        }
        private void Login_Shown(object sender, EventArgs e)
        {
            txtPass.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ClearErorr();
            try
            {
                if (string.IsNullOrWhiteSpace(cbServer.Text))
                    throw new Exception(Rekod.Properties.Resources.login_erCnArg);
                if (string.IsNullOrWhiteSpace(cbLogin.Text))
                    throw new Exception(Rekod.Properties.Resources.login_errNoUName);
            }
            catch (Exception ex)
            {
                SetErorr(ex.Message);
                return;
            }

            try
            {

                ApplyConnectParams();
                SqlWork.CorrectConnectBuilder(_connectBuilder);
                string pwd = _connectBuilder["Password"].ToString();
                try
                {
                    _connectBuilder.Password = Encrypting.CreateMD5(pwd).ToLower();
                    _idUser = SqlWork.CheckedUser(_connectBuilder);
                }
                catch
                {
                    _connectBuilder.Password = pwd;
                    _idUser = SqlWork.CheckedUser(_connectBuilder);
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                if (ex is NpgsqlException)
                {
                    if ((ex as NpgsqlException).Code == "28P01")
                        SetErorr(Rekod.Properties.Resources.login_errWPass + _connectBuilder.UserName + "'");
                }
                else
                    SetErorr(ex.Message);
               
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnDetail_Click(object sender, EventArgs e)
        {
            ShowDetail(true);
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            ClearErorr();
        }

        private void cbServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillcbLogin(cbServer.Text.Trim());
            txtPass.Focus();
        }
        private void cbServer_TextChanged(object sender, EventArgs e)
        {
            cbLogin.Items.Clear();
        }


        private DataBase_M FindDataBase(string dBase)
        {
            DataBase_M db = null;
            for (int i = 0; i < _listDB.Count; i++)
            {
                var item = _listDB[i];
                if (item.DataBase == dBase)
                {
                    db = item;
                    break;
                }
            }
            return db;
        }

        #region Статические методы
        public static void ParseServerParams(string line, NpgsqlConnectionStringBuilder conn)
        {
            conn.Database = ParseServerString(line, @"\s*([^@: ]*)\s*@");
            if (String.IsNullOrEmpty(conn.Database))
                throw new Exception(Rekod.Properties.Resources.login_errAdStr);

            conn.Host = ParseServerString(line, @"@\s*([^@: ]*)\s*:?");
            if (String.IsNullOrEmpty(conn.Host))
                throw new Exception(Rekod.Properties.Resources.login_errSerName);

            string port = ParseServerString(line, @":\s*(\d+)\s*");
            if (String.IsNullOrEmpty(port))
                conn.Port = 5432;
            else
                conn.Port = Int32.Parse(port);
        }
        public static string ParseServerString(string line, string pattern)
        {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            if (match == null || match.Groups.Count < 2 || String.IsNullOrEmpty(match.Groups[1].Value))
                return null;
            else
                return match.Groups[1].Value;
        }

        public static string ConcatenateServer(DbConnectionStringBuilder conn)
        {
            if (string.IsNullOrWhiteSpace((string)conn["Database"]) || string.IsNullOrWhiteSpace((string)conn["Host"]))
                return string.Empty;
            if ((conn["Port"].ToString()) == "5432")
                return conn["Database"] + "@" + (string)conn["Host"];
            else
                return conn["Database"] + "@" + (string)conn["Host"] + ":" + conn["Port"].ToString() ;
        }
        public static string ConcatenateServerWithoutPort(DbConnectionStringBuilder conn)
        {
            if (string.IsNullOrWhiteSpace((string)conn["Database"]) || string.IsNullOrWhiteSpace((string)conn["Host"]))
                return string.Empty;
            return conn["Database"] + "@" + (string)conn["Host"];
        }
        #endregion // Статические методы


    }
}
