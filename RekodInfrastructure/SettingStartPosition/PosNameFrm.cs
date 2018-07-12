using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using Rekod.Services;

namespace Rekod.SettingStartPosition
{
    public partial class PosNameFrm : Form
    {

        public PosNameFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT name FROM " + Program.scheme + ".map_extents WHERE name=@name;";
                var param = new IParams[]
                                    {
                                        new Params
                                            {
                                                paramName = "@name",
                                                typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                value = textBox1.Text.Trim()
                                            }
                                    };
                sqlCmd.Execute(false, param);
                if (sqlCmd.CanRead())
                {
                    MessageBox.Show("Локация с таким именем уже существует");
                    return;
                }
            }
            if (textBox1.Text.Trim() != "")
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.PosNameFrm_NullName, Rekod.Properties.Resources.Mes_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
