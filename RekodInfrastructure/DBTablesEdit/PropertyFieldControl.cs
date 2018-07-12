using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using Rekod.Services;

namespace Rekod
{
    public partial class PropertyFieldControl : UserControl
    {
        private DBTablesGroups parent;
        private FiledEditListControl field_edit;

        private int id;
        public PropertyFieldControl(int id1, DBTablesGroups par, FiledEditListControl f_edit)
        {
            InitializeComponent();
            panel3.Location = panel4.Location;
            panel5.Location = panel4.Location;
            
            id = id1;
            loadTables();
            loadInfo();
            parent = par;
            field_edit = f_edit;

            if (field_edit != null
                && !Rekod.DBTablesEdit.SyncController.HasRight(field_edit.TableId))
            {
                button1.Enabled = false;
            }
        }
        private void setComboBox(ComboBox cmb, int idF)
        {
            for (int i = 0; cmb.Items.Count > i; i++)
            {
                if (((itemObj)cmb.Items[i]).Id_o == idF)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }
        private void loadInfo()
        {
            int q1 = 0, q2 = 0, q3 = 0, q4 = 0;
            bool refer = false, refer2 = false;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT is_reference, is_interval, ref_table, ref_field, ref_field_end, ref_field_name " +
                        "FROM "+Program.scheme+".table_field_info WHERE id = " + id.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {

                if (sqlCmd.GetBoolean(0))
                {
                    q1 = sqlCmd.GetInt32(2);
                    q2 = sqlCmd.GetInt32(3);
                    q4 = sqlCmd.GetInt32(5);
                    refer = true;
                }
                if (sqlCmd.GetBoolean(1))
                {
                    q1 = sqlCmd.GetInt32(2);
                    q2 = sqlCmd.GetInt32(3);
                    q3 = sqlCmd.GetInt32(4);
                    q4 = sqlCmd.GetInt32(5);
                    refer2 = true;
                }
            }
            sqlCmd.Close();
            if (refer)
            {
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT type FROM " + Program.scheme + ".table_info WHERE id=" + q1;
                sqlCmd.Execute(false);
                int type_table = 0;
                while (sqlCmd.CanRead())
                {
                    type_table = sqlCmd.GetInt32(0);
                }
                if (type_table == 1 || type_table == 4)
                {
                    comboBox8.SelectedIndex = 3;
                    setComboBox(comboBox9, q1);
                    setComboBox(comboBox10, q2);
                    setComboBox(comboBox11, q4);
                }
                else
                {
                    comboBox8.SelectedIndex = 1;
                    setComboBox(comboBox1, q1);
                    setComboBox(comboBox2, q2);
                    setComboBox(comboBox3, q4);
                }
            }
            else
                if (refer2)
                {
                    comboBox8.SelectedIndex = 2;
                    setComboBox(comboBox4, q1);
                    setComboBox(comboBox5, q2);
                    setComboBox(comboBox6, q3);
                    setComboBox(comboBox7, q4);
                }
                else
                {
                    comboBox8.SelectedIndex = 0;
                }
        }
        private void loadTables()
        {
            comboBox1.Items.Clear();
            comboBox4.Items.Clear();
            comboBox9.Items.Clear();

            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_info WHERE type = 2";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(2) + " (" +
                            sqlCmd.GetValue<string>(1) + ")", sqlCmd.GetValue<string>(1));
                comboBox1.Items.Add(item);
            }
            sqlCmd.Close();

            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_info WHERE type = 3";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(2) + " (" +
                            sqlCmd.GetValue<string>(1) + ")", sqlCmd.GetValue<string>(1));
                comboBox4.Items.Add(item);
            }
            sqlCmd.Close();

            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_info WHERE type = 1 OR type=4";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(2) + " (" +
                            sqlCmd.GetValue<string>(1) + ")", sqlCmd.GetValue<string>(1));
                comboBox9.Items.Add(item);
            }
            sqlCmd.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var fi = classesOfMetods.getFieldInfo(id);
            var ti = classesOfMetods.getTableInfo(fi.idTable);

            if (comboBox8.SelectedIndex == 1)
            {
                if (comboBox1.SelectedItem != null &&
                    comboBox2.SelectedItem != null &&
                    comboBox3.SelectedItem != null)
                {
                    string sql = "";
                    if (ti.style_field == fi.nameDB && (((itemObj)comboBox1.SelectedItem).Id_o != fi.ref_table
                        || ((itemObj)comboBox2.SelectedItem).Id_o != fi.ref_field
                        || ((itemObj)comboBox3.SelectedItem).Id_o != fi.ref_field_name))
                    {
                        if (MessageBox.Show(Rekod.Properties.Resources.FELC_DeletingFieldStyle + Environment.NewLine +
                                    Rekod.Properties.Resources.PFC_EditingFieldsAsk,
                                    Rekod.Properties.Resources.PFC_EditingFields,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                            return;
                        else sql = "UPDATE " + Program.scheme + ".table_info SET default_style=true, style_field='style' WHERE id=" + ti.idTable + ";";
                    }

                    SqlWork sqlCmd = new SqlWork();
                    sqlCmd.sql = sql + "UPDATE " + Program.scheme + ".table_field_info SET is_reference = true, is_interval=false, " +
                        "ref_table = " + ((itemObj)comboBox1.SelectedItem).Id_o + ", " +
                        "ref_field = " + ((itemObj)comboBox2.SelectedItem).Id_o + ", " +
                        "ref_field_name = " + ((itemObj)comboBox3.SelectedItem).Id_o + " " +
                        " WHERE id =" + id;
                    sqlCmd.Execute(true);
                    sqlCmd.Close();
                    parent.CloseModalElem();
                    classesOfMetods cls = new classesOfMetods();
                    cls.reloadInfo();
                    DBTablesEdit.SyncController.ReloadTable(ti.idTable);
                    return;
                }
                else
                {
                    MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                    return;
                }
            }
            else
                if (comboBox8.SelectedIndex == 3)
                {
                    if (comboBox9.SelectedItem != null &&
                        comboBox10.SelectedItem != null &&
                        comboBox11.SelectedItem != null)
                    {
                        string sql = "";
                        if (ti.style_field == fi.nameDB && (((itemObj)comboBox9.SelectedItem).Id_o != fi.ref_table
                            || ((itemObj)comboBox10.SelectedItem).Id_o != fi.ref_field
                            || ((itemObj)comboBox11.SelectedItem).Id_o != fi.ref_field_name))
                        {
                            if (MessageBox.Show(Rekod.Properties.Resources.FELC_DeletingFieldStyle + Environment.NewLine +
                                    Rekod.Properties.Resources.PFC_EditingFieldsAsk,
                                    Rekod.Properties.Resources.PFC_EditingFields,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                                return;
                            else sql = "UPDATE " + Program.scheme + ".table_info SET default_style=true, style_field='style' WHERE id=" + ti.idTable + ";";
                        }

                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql = sql + "UPDATE " + Program.scheme + ".table_field_info SET is_reference = true, is_interval=false, " +
                            "ref_table = " + ((itemObj)comboBox9.SelectedItem).Id_o + ", " +
                            "ref_field = " + ((itemObj)comboBox10.SelectedItem).Id_o + ", " +
                            "ref_field_name = " + ((itemObj)comboBox11.SelectedItem).Id_o + " " +
                            " WHERE id =" + id;
                        sqlCmd.Execute(true);
                        sqlCmd.Close();
                        parent.CloseModalElem();
                        classesOfMetods cls = new classesOfMetods();
                        cls.reloadInfo();
                        DBTablesEdit.SyncController.ReloadTable(ti.idTable);
                        return;
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                        return;
                    }
                }
                else
                {
                    if (comboBox8.SelectedIndex == 2)
                    {
                        if (comboBox4.SelectedItem != null &&
                            comboBox5.SelectedItem != null &&
                            comboBox6.SelectedItem != null &&
                            comboBox7.SelectedItem != null)
                        {
                            string sql = "";
                            if (ti.style_field == fi.nameDB && (((itemObj)comboBox4.SelectedItem).Id_o != fi.ref_table
                                || ((itemObj)comboBox5.SelectedItem).Id_o != fi.ref_field
                                || ((itemObj)comboBox6.SelectedItem).Id_o != fi.ref_field_end
                                || ((itemObj)comboBox7.SelectedItem).Id_o != fi.ref_field_name))
                            {
                                if (MessageBox.Show(Rekod.Properties.Resources.FELC_DeletingFieldStyle + Environment.NewLine +
                                    Rekod.Properties.Resources.PFC_EditingFieldsAsk,
                                    Rekod.Properties.Resources.PFC_EditingFields,
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                                    return;
                                else sql = "UPDATE " + Program.scheme + ".table_info SET default_style=true, style_field='style' WHERE id=" + ti.idTable + ";";
                            }
                            SqlWork sqlCmd = new SqlWork();
                            sqlCmd.sql = sql + "UPDATE " + Program.scheme + ".table_field_info SET is_reference = false, is_interval=true, " +
                                    "ref_table = " + ((itemObj)comboBox4.SelectedItem).Id_o + ", " +
                                    "ref_field = " + ((itemObj)comboBox5.SelectedItem).Id_o + ", " +
                                    "ref_field_end = " + ((itemObj)comboBox6.SelectedItem).Id_o + ", " +
                                    "ref_field_name = " + ((itemObj)comboBox7.SelectedItem).Id_o + " " +
                                    " WHERE id =" + id;
                            sqlCmd.Execute(true);
                            sqlCmd.Close();
                            //MessageBox.Show("Настройки зафиксированы!");
                            parent.CloseModalElem();
                            classesOfMetods cls = new classesOfMetods();
                            cls.reloadInfo();
                            DBTablesEdit.SyncController.ReloadTable(ti.idTable);
                            return;
                        }
                        else
                        {
                            MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                            return;
                        }
                    }
                    else
                    {
                        string sql = "";

                        if (ti.Style.DefaultStyle != true && ti.style_field == fi.nameDB && fi.ref_table != null)
                        {
                            if (MessageBox.Show(Rekod.Properties.Resources.FELC_DeletingFieldStyle + Environment.NewLine +
                                    Rekod.Properties.Resources.PFC_EditingFieldsAsk,
                                    Rekod.Properties.Resources.PFC_EditingFields,
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                sql = "UPDATE " + Program.scheme + ".table_info SET default_style=true, style_field='style' WHERE id=" + ti.idTable + ";";
                            }
                            else
                            {
                                return;
                            }
                        }
                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql = sql + "UPDATE " + Program.scheme + ".table_field_info SET is_reference = false, is_interval = false, is_style = false, ref_table = NULL, ref_field = NULL, ref_field_end = NULL, ref_field_name = NULL " +
                " WHERE id =" + id.ToString();
                        sqlCmd.Execute(true);
                        sqlCmd.Close();
                        parent.CloseModalElem();
                        classesOfMetods cls = new classesOfMetods();
                        cls.reloadInfo();
                        DBTablesEdit.SyncController.ReloadTable(ti.idTable);
                        return;
                    }
                }            
        }
        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                comboBox2.Items.Clear();
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)comboBox1.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox2.Items.Add(item);
                }
                sqlCmd.Close();

                comboBox3.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE type_field = 2 AND id_table = " + ((itemObj)comboBox1.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox3.Items.Add(item);
                }
                sqlCmd.Close();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedItem != null)
            {
                comboBox5.Items.Clear();
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)comboBox4.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox5.Items.Add(item);
                }
                sqlCmd.Close();

                comboBox6.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)comboBox4.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox6.Items.Add(item);
                }
                sqlCmd.Close();

                comboBox7.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE type_field = 2 AND id_table = " + ((itemObj)comboBox4.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox7.Items.Add(item);
                }
                sqlCmd.Close();
            }
        }
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox9.SelectedItem != null)
            {
                comboBox10.Items.Clear();
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)comboBox9.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox10.Items.Add(item);
                }
                sqlCmd.Close();

                comboBox11.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE type_field = 2 AND id_table = " + ((itemObj)comboBox9.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    comboBox11.Items.Add(item);
                }
                sqlCmd.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            parent.CloseModalElem();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel3.Visible = comboBox8.SelectedIndex == 1;
            panel4.Visible = comboBox8.SelectedIndex == 2;
            panel5.Visible = comboBox8.SelectedIndex == 3;
        }

    }
}
