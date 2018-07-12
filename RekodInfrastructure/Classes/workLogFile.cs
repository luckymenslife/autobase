using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Rekod.Classes
{
    class workLogFile
    {
        static public void writeLogFile(System.Exception ex, bool visibleUser, bool error)
        {
            if (ex != null)
                writeLogFile(ex.Message, visibleUser, error);
            else
            {
                writeLogFile(Rekod.Properties.Resources.workLog_null_ex, visibleUser, error);
            }
        }
        static public void writeLogFile(string ex, bool visibleUser, bool error)
        {
            string s;
            s = Program.path_string + "\\log.txt";
            var fs = new FileStream(s, FileMode.Append);
            var sw = new StreamWriter(fs);
            string messError = string.Format("{0} -- {1} ({2}) : {3}", DateTime.Now, Program.user_info.nameUser,
                                             Program.user_info.id_user, ex);
            Debug.WriteLine(messError, "writeLogFile");
            sw.WriteLine(messError);
            sw.Close();
            if (visibleUser)
            {
                if (error)
                {
                    System.Windows.Forms.MessageBox.Show(Rekod.Properties.Resources.ErrorMessage + Environment.NewLine + ex, Rekod.Properties.Resources.ErrorMessage_header,
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(Rekod.Properties.Resources.InformationMessage + Environment.NewLine + ex, Rekod.Properties.Resources.InformationMessage_Header,
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
                }
            }
        }
    }
}