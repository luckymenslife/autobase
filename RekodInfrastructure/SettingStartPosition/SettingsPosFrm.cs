using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using mvMapLib;
using Rekod.Properties;
using Rekod.Services;

namespace Rekod.SettingStartPosition
{
    public partial class SettingsPosFrm : Form
    {
        GetBboxBd _getBboxBd = new GetBboxBd();
        struct MapExtent
        {
            public int id;
            public string name;
            public mvBbox Extent;
        }
        List<MapExtent> extentList = new List<MapExtent>();
        public SettingsPosFrm()
        {
            InitializeComponent();
            loadExtentList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InsertMapExtent();
        }
        private mvBbox GetBbox()
        {
            return Program.mainFrm1.axMapLIb1.MapExtent;
        }


        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void loadExtentList()
        {
            extentList.Clear();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name, extent FROM " + Program.scheme + ".map_extents ORDER BY name;";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                MapExtent item = new MapExtent();
                item.id = sqlCmd.GetInt32(0);
                item.name = sqlCmd.GetValue<string>(1);
                item.Extent = _getBboxBd.GetBboxFromBd(sqlCmd.GetValue<string>(2), Program.srid);
                extentList.Add(item);
            }
            sqlCmd.Close();
            LoadListBox();
        }
        private void LoadListBox()
        {
            listBox1.Items.Clear();
            setEnablet(false);
            for (int i = 0; extentList.Count > i; i++)
            {
                itemObj item = new itemObj(extentList[i].id, extentList[i].name, "");
                listBox1.Items.Add(item);
            }
            label2.Text = Rekod.Properties.Resources.LocCount+": " + extentList.Count;
        }
        private void InsertMapExtent()
        {
            var frm = new PosNameFrm();
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "INSERT INTO " + Program.scheme + ".map_extents(name, extent) VALUES (@name, @extent);";
                    var param = new IParams[]
                                {
                                    new Params
                                        {
                                            paramName = "@name",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                            value = frm.textBox1.Text
                                        },

                                    new Params
                                        {
                                            paramName = "@extent",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                            value = _getBboxBd.GetBboxInBd(GetBbox())
                                        }
                                };
                    sqlCmd.ExecuteNonQuery(param);
                }
                loadExtentList();
            }
        }
        private void SetName(int id_extent, string name)
        {
            var frm = new PosNameFrm();
            frm.textBox1.Text = name;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "UPDATE " + Program.scheme + ".map_extents SET name=@name WHERE id = " + id_extent + ";";
                    var param = new IParams[]
                                    {
                                        new Params
                                            {
                                                paramName = "@name",
                                                typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                value = frm.textBox1.Text
                                            }
                                    };
                    sqlCmd.ExecuteNonQuery(param);
                }

                loadExtentList();
            }
        }
        private void SetExtent(int id_extent)
        {
            if (MessageBox.Show(Resources.SettingsPosFrm_QuestionChangeLocation, Resources.Mes_Change, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "UPDATE " + Program.scheme + ".map_extents SET extent=@extent WHERE id = " +
                                 id_extent + ";";
                    var param = new IParams[]
                                    {
                                        new Params
                                            {
                                                paramName = "@extent",
                                                typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                value = _getBboxBd.GetBboxInBd(GetBbox())
                                            }
                                    };
                    sqlCmd.ExecuteNonQuery(param);
                }
                loadExtentList();
            }
        }
        private void DeleteMapExtent(int id_extent)
        {
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "DELETE FROM " + Program.scheme + ".map_extents WHERE id = " + id_extent.ToString() + ";";
                sqlCmd.ExecuteNonQuery();
            }
            loadExtentList();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                _getBboxBd.SetToMapLib(extentList[listBox1.SelectedIndex].Extent);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                if (MessageBox.Show(Resources.SettingsPosFrm_DeleteLocation, Resources.Mes_Delete, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    DeleteMapExtent(((itemObj)listBox1.SelectedItem).Id_o);
                }
            }
            else
            {
                MessageBox.Show(Resources.SettingsPosFrm_MastSelectLocation, Resources.Mes_Delete, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                SetName(((itemObj)listBox1.SelectedItem).Id_o, ((itemObj)listBox1.SelectedItem).Name_o);
            }
            else
            {
                MessageBox.Show(Resources.SettingsPosFrm_MastSelectLocation, Resources.Mes_Change, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                SetExtent(((itemObj)listBox1.SelectedItem).Id_o);
            }
            else
            {
                MessageBox.Show(Resources.SettingsPosFrm_MastSelectLocation, Resources.Mes_Change, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void setEnablet(bool isEnablet)
        {
            button1.Enabled = isEnablet;
            button2.Enabled = isEnablet;
            button4.Enabled = isEnablet;
            button5.Enabled = isEnablet;
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                setEnablet(true);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                _getBboxBd.SetToMapLib(extentList[listBox1.SelectedIndex].Extent);
            }
        }
    }
    public class GetBboxBd
    {
        public mvBbox GetBboxFromBd(string extent, string srid)
        {
            mvBbox temp = new mvBbox();
            string[] extents = extent.Split(',');
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format("SELECT st_x(st_transform(st_geomfromtext('POINT({0} {1})', 3395), {2})) as x, " +
                                    "st_y(st_transform(st_geomfromtext('POINT({0} {1})', 3395), {2})) as y", extents[0], extents[1], Program.srid);
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                temp.a.x = sqlCmd.GetValue<float>(0);
                temp.a.y = sqlCmd.GetValue<float>(1);
            }
            sqlCmd.Close();

            sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format("SELECT st_x(st_transform(st_geomfromtext('POINT({0} {1})', 3395), {2})) as x, " +
                                    "st_y(st_transform(st_geomfromtext('POINT({0} {1})', 3395), {2})) as y", extents[2], extents[3], Program.srid);
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                temp.b.x = sqlCmd.GetValue<float>(0);
                temp.b.y = sqlCmd.GetValue<float>(1);
            }
            sqlCmd.Close();

            return temp;
        }
        public string GetBboxInBd(mvBbox bb)
        {
            string temp1 = "";
            string temp2 = "";
            temp1 = bb.a.x.ToString().Replace(",", ".") + " " + bb.a.y.ToString().Replace(",", ".");
            temp2 = bb.b.x.ToString().Replace(",", ".") + " " + bb.b.y.ToString().Replace(",", ".");
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format("SELECT st_x(st_transform(st_geomfromtext('POINT({0})', {1}), 3395)) as x, " +
                                    "st_y(st_transform(st_geomfromtext('POINT({0})', {1}), 3395)) as y", temp1, Program.srid);
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                temp1 = sqlCmd.GetValue<string>(0).Replace(",", ".") + "," + sqlCmd.GetValue<string>(1).Replace(",", ".");
            }
            sqlCmd.Close();
            sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format("SELECT st_x(st_transform(st_geomfromtext('POINT({0})', {1}), 3395)) as x, " +
                                    "st_y(st_transform(st_geomfromtext('POINT({0})', {1}), 3395)) as y", temp2, Program.srid);
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                temp2 = sqlCmd.GetValue<string>(0).Replace(",", ".") + "," + sqlCmd.GetValue<string>(1).Replace(",", ".");
            }
            sqlCmd.Close();
            return temp1 + "," + temp2;
        }
        public void SetToMapLib(mvBbox extent)
        {
            Program.mainFrm1.axMapLIb1.MapExtent = extent;
            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
    }
}
