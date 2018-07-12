
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rekod.DBTablesEdit;

namespace Rekod.HelpFiles
{
    partial class AboutBoxGS : Form
    {
        public AboutBoxGS()
        {
            classesOfMetods.SetFormOwner(this);
            InitializeComponent();
            this.Text = String.Format(Rekod.Properties.Resources.About + " {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct + " " + AssemblyVersion;
            this.labelMaplibVersion.Text = MaplibVersion;
            this.labelCopyright.Text = AssemblyCopyright;
            this.OkBtn.Select();
        }

        #region Методы доступа к атрибутам сборки

        public string AssemblyTitle
        {
            get
            {

                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return String.Format("{0}{1}", classesOfMetods.GetVersionString, classesOfMetods.GetBuildNumber);
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                try
                {
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if (attributes.Length != 0)
                    {
                        string copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
                        if (copyright.Contains("©"))
                        {
                            string[] parts = copyright.Split(new char[] { '©' });
                            if (parts.Length == 2)
                            {
                                for (int i = 0; i < 2; i++)
                                    if (Regex.IsMatch(parts[i], @"\d+-\d+"))
                                    {
                                        return String.Format("© {0}   {1}", parts[1 - i].Trim(), parts[i].Trim());
                                    }
                            }
                        }
                    }
                }
                catch { }
                return String.Empty;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        public string MaplibVersion
        {
            get
            {
                string version = Program.mainFrm1.axMapLIb1.AboutBuild();
                if (!String.IsNullOrEmpty(version))
                {
                    try
                    {
                        var versions = Regex.Matches(version, @"\d.\d.\d");
                        var buids = Regex.Matches(version, @"build \d+");
                        if (versions != null
                            && versions.Count == 2
                            && buids != null
                            && buids.Count == 2)
                        {
                            version = String.Format("{0} {1} ({2}, wrapper {3})",
                                version.Substring(0, version.IndexOf(":")),
                                versions[0].Value,
                                buids[0],
                                buids[1]);
                        }
                    }
                    catch { }
                }
                return version;
            }
        }
        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            HelpLableFrm frm = new HelpLableFrm(Application.StartupPath + "\\version_info.mht");
            frm.Text = Rekod.Properties.Resources.Versions;
            frm.Show();
        }
    }
}