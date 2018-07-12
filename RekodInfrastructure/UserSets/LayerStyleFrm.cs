using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rekod.Services;

namespace Rekod.UserSets
{
    public partial class LayerStyleFrm : Form
    {
        int idT = 0;
        public Rekod.DBTablesEdit.StyleControl sc;
        public Rekod.DBTablesEdit.LabelControl ls;

        public LayerStyleFrm(int idT, bool existsStyle)
        {
            InitializeComponent();
            this.idT = idT;

            sc = new Rekod.DBTablesEdit.StyleControl(true, true, true, Rekod.DBTablesEdit.SyncController.HasRight(idT));
            //this.Size = new System.Drawing.Size(sc.Width + 23, sc.Height + 117);
            //this.MaximumSize = this.Size;
            //this.MinimumSize = this.Size;
            //this.MaximizeBox = false;
            this.tabControl1.Controls.Add(this.tabPage1);
            sc.setStyles(setStyleFromDB(idT));
            this.tabPage1.Controls.Add(sc);
            sc.panel7.Visible = false;
            sc.Dock = DockStyle.Fill;

                this.tabControl1.Controls.Add(this.tabPage2);
                ls = new DBTablesEdit.LabelControl(idT);
                ls.Dock = DockStyle.Fill;
                ls.UserStyle = true;
                this.tabPage2.Controls.Add(ls);

        }
        private axVisUtils.Styles.objStylesM setStyleFromDB(int idT)
        {
            axVisUtils.Styles.objStylesM s1 = new axVisUtils.Styles.objStylesM();

            string sql = "SELECT fontname, fontcolor, fontframecolor, fontsize,";
            sql += " symbol,";
            sql += " pencolor, pentype, penwidth,";
            sql += " brushbgcolor, brushfgcolor, brushstyle, brushhatch";
            sql += " FROM " + Program.scheme + ".table_info_sets WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table=" + idT.ToString();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = sql;
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                s1.FontStyle.FontName = sqlCmd.GetString(0);
                s1.FontStyle.Color = sqlCmd.GetValue<uint>(1);
                s1.FontStyle.FrameColor = sqlCmd.GetValue<uint>(2);
                s1.FontStyle.Size = sqlCmd.GetInt32(3);

                s1.SymbolStyle.Shape = sqlCmd.GetValue<uint>(4);

                s1.PenStyle.Color = sqlCmd.GetValue<uint>(5);
                s1.PenStyle.Type = sqlCmd.GetValue<ushort>(6);
                s1.PenStyle.Width = sqlCmd.GetValue<uint>(7);

                s1.BrushStyle.bgColor = sqlCmd.GetValue<uint>(8);
                s1.BrushStyle.fgColor = sqlCmd.GetValue<uint>(9);
                s1.BrushStyle.Style = sqlCmd.GetValue<ushort>(10);
                s1.BrushStyle.Hatch = sqlCmd.GetValue<ushort>(11);
            }
            sqlCmd.Close();
            return s1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
