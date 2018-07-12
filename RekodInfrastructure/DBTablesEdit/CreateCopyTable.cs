using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rekod.Services;

namespace Rekod.DBTablesEdit
{
    public partial class CreateCopyTable : Form
    {
        int idT;
        string pk_field;
        string geom_field;
        private TextBoxInfo tb;
        public CreateCopyTable(int idT)
        {
            InitializeComponent();
            var ti = classesOfMetods.getTableInfo(idT);
            if (Program.sscUser == null || classesOfMetods.getTableInfo(idT).type != 1)
            {
                this.panelForGroup.Visible = false;
            }
            else
            {
                loadSSCGroup();
            }
            tb = new TextBoxInfo(this.textBox1);
            this.idT = idT;
            loadField(idT);
            textBox1.Text = classesOfMetods.getTableInfo(idT).nameDB + "_copy";
            textBox2.Text = classesOfMetods.getTableInfo(idT).nameMap + " " + Rekod.Properties.Resources.CCT_Postfix;
        }

        private void loadSSCGroup()
        {
            this.sscGroupCmb.Items.Clear();
            this.sscGroupCmb.DisplayMember = "name";
            foreach (var item in Program.mapAdmin.SscGroups)
            {
                this.sscGroupCmb.Items.Add(item);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
        private void loadField(int idT)
        {
            pk_field = classesOfMetods.getTableInfo(idT).pkField;
            geom_field = classesOfMetods.getTableInfo(idT).geomFieldName;
            List<fieldInfo> fInfo = classesOfMetods.getFieldInfoTable(idT);
            for (int i = 0; fInfo.Count > i; i++)
            {
                itemObj item = new itemObj(fInfo[i].idField, fInfo[i].nameMap, fInfo[i].nameDB);
                if (pk_field != fInfo[i].nameDB && geom_field != fInfo[i].nameDB)
                    checkedListBox1.Items.Add(item, true);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                if (Program.sscUser!=null && this.sscGroupCmb.SelectedItem == null)
                {
                    MessageBox.Show("Не указана группа!", Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string name_scheme = classesOfMetods.getTableInfo(idT).nameSheme;
                string name_table = FindAcceptableTableName(textBox1.Text, name_scheme);
                if (String.IsNullOrEmpty(name_table))
                    return;

                string nameField = "";
                int id_filed = 0, id_table = 0;
                string type_field = "", field_list = "", insert_sql = "", type_geom = "";
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT nextval('sys_scheme.table_info_id_seq')::INTEGER";
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    id_table = sqlCmd.GetInt32(0);
                }
                sqlCmd.Close();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT type FROM geometry_columns WHERE f_table_schema like '" + classesOfMetods.getTableInfo(idT).nameSheme
                    + "' AND f_table_name like '" + classesOfMetods.getTableInfo(idT).nameDB +
                    "' AND f_geometry_column like '" + classesOfMetods.getTableInfo(idT).geomFieldName + "';";
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    type_geom = sqlCmd.GetValue<string>(0);
                }
                sqlCmd.Close();

                string sql = "CREATE TABLE " + name_scheme +
                    "." + name_table + " ( " +
                    classesOfMetods.getTableInfo(idT).pkField + " serial NOT NULL, ";
                for (int i = 0; checkedListBox1.CheckedItems.Count > i; i++)
                {
                    id_filed = ((itemObj)checkedListBox1.CheckedItems[i]).Id_o;
                    field_list = field_list + id_filed + ",";
                    nameField = classesOfMetods.getFieldInfo(id_filed).nameDB;
                    switch (classesOfMetods.getFieldInfo(id_filed).type)
                    {
                        case 1:
                            type_field = " integer DEFAULT 0, ";
                            break;
                        case 2:
                            type_field = " character varying, ";
                            break;
                        case 3:
                            type_field = " date, ";
                            break;
                        case 4:
                            type_field = " timestamp with time zone, ";
                            break;
                        case 5:
                            type_field = " geometry, ";
                            break;
                        case 6:
                            type_field = " numeric DEFAULT 0, ";
                            break;
                    }
                    sql = sql + nameField + type_field;
                    insert_sql = insert_sql + nameField + ",";
                }
                List<fieldInfo> fInfo = classesOfMetods.getFieldInfoTable(idT);
                for (int i = 0; fInfo.Count > i; i++)
                {
                    itemObj item = new itemObj(fInfo[i].idField, fInfo[i].nameMap, fInfo[i].nameDB);
                    if (pk_field == fInfo[i].nameDB || geom_field == fInfo[i].nameDB)
                    {
                        field_list = field_list + fInfo[i].idField + ",";
                        insert_sql = insert_sql + fInfo[i].nameDB + ",";
                    }
                }

                field_list = field_list.Substring(0, field_list.Length - 1);
                insert_sql = insert_sql.Substring(0, insert_sql.Length - 1);
                sql = sql + "CONSTRAINT " + name_table + "_pkey PRIMARY KEY (" + pk_field + ") );" +
                    "ALTER TABLE " + classesOfMetods.getTableInfo(idT).nameSheme + "." + name_table +
                    " OWNER TO admin_" + Program.connString["DataBase"] + ";";
                if (classesOfMetods.getTableInfo(idT).type == 1)
                {
                    sql = sql + " SELECT AddGeometryColumn('" + classesOfMetods.getTableInfo(idT).nameSheme +
                        "', '" + name_table + "','" + geom_field + "'," +
                        classesOfMetods.getTableInfo(idT).srid.ToString() + ",'" + type_geom + "',2); ";
                }
                sql = sql + "INSERT INTO sys_scheme.table_info( " +
                            "id, scheme_name, name_db, name_map, lablefiled, map_style, geom_field, " +
                            "style_field, geom_type, type, default_style, fontname, fontcolor, " +
                            "fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, " +
                            "brushbgcolor, brushfgcolor, brushstyle, brushhatch, read_only, " +
                            "id_style, pk_fileld, is_style, source_layer, image_column, " +
                            "angle_column, use_bounds, min_scale, max_scale, default_visibl, " +
                            "range_colors, range_column, " +
                            "precision_point, type_color, min_color, min_val, max_color, max_val, " +
                            "use_min_val, null_color, use_null_color, hidden, use_max_val) " +
                       "SELECT " + id_table.ToString() + ", scheme_name, '" + name_table + "', '" +
                            textBox2.Text + "', lablefiled, map_style, geom_field, " +
                            "style_field, geom_type, type, default_style, fontname, fontcolor, " +
                            "fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, " +
                            "brushbgcolor, brushfgcolor, brushstyle, brushhatch, read_only, " +
                            "id_style, pk_fileld, is_style, source_layer, image_column, " +
                            "angle_column, use_bounds, min_scale, max_scale, default_visibl, " +
                            "range_colors, range_column, " +
                            "precision_point, type_color, min_color, min_val, max_color, max_val, " +
                            "use_min_val, null_color, use_null_color, hidden, use_max_val " +
                        "FROM sys_scheme.table_info " +
                        "WHERE id = " + idT.ToString() + ";";
                sql = sql + "INSERT INTO sys_scheme.table_field_info( " +
                            "id_table, name_db, name_map, type_field, visible, name_lable, " +
                            "is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, " +
                            "ref_field_name, read_only) " +
                "SELECT " + id_table.ToString() + ", name_db, name_map, type_field, visible, name_lable, " +
                            "is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, " +
                            "ref_field_name, read_only " +
                "FROM sys_scheme.table_field_info " +
                "WHERE id in (" + field_list + ") ORDER BY num_order;";
                sql = sql + " INSERT INTO " + classesOfMetods.getTableInfo(idT).nameSheme +
                    "." + name_table + " (" + insert_sql + ") SELECT " + insert_sql + " FROM " +
                        classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getTableInfo(idT).nameDB + ";";
                sql += string.Format(
                    "INSERT INTO {0}.table_right (id_table, id_user, read_data, write_data) SELECT {1}, id, true, true FROM {0}.user_db WHERE admin=true;",
                    Program.scheme,
                    id_table);
                if (!Program.user_info.admin)
                {
                    sql += string.Format(
                    "INSERT INTO {0}.table_right (id_table, id_user, read_data, write_data) VALUES({1},{2},true,true);",
                    Program.scheme,
                    id_table,
                    Program.user_info.id_user);
                }
                sqlCmd = new SqlWork();
                sqlCmd.sql = sql;
                if (sqlCmd.Execute(true))
                {
                    //Установить значение сиквенса
                    string sequenceSetError = null;
                    try
                    {
                        sqlCmd.sql =
                            String.Format("SELECT \"{0}\" FROM \"{1}\".\"{2}\" ORDER BY \"{0}\" DESC LIMIT 1",
                                pk_field,
                                name_scheme,
                                name_table);
                        int? lastGid = sqlCmd.ExecuteScalar<int?>();
                        sqlCmd.CloseReader();
                        if (lastGid != null)
                        {
                            sqlCmd.sql =
                                String.Format("SELECT pg_get_serial_sequence('{0}.{1}', '{2}')",
                                    name_scheme,
                                    name_table,
                                    pk_field);
                            sqlCmd.Connection.Open();
                            String sequenceName = sqlCmd.ExecuteScalar<String>();
                            sqlCmd.CloseReader();

                            sqlCmd.sql = String.Format("SELECT setval('{0}', {1})", sequenceName, lastGid);
                            sqlCmd.Connection.Open();
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        sequenceSetError = ex.Message;
                    }
                    String messageBoxText = Rekod.Properties.Resources.DGBH_OperSuccess;
                    if (sequenceSetError != null)
                    {
                        messageBoxText = messageBoxText + "\nError while setting sequence: " + sequenceSetError;
                    }

                    MessageBox.Show(messageBoxText, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    classesOfMetods cls = new classesOfMetods();
                    cls.reloadInfo();
                    sqlCmd.Close();
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
                if (Program.sscUser != null && classesOfMetods.getTableInfo(id_table).type == 1)
                {
                    SyncController.RegisterTable(id_table, ((RESTLib.Model.REST.Group)this.sscGroupCmb.SelectedItem).name);
                }
            }
        }

        /// <summary>
        /// Поиск незанятого имени таблицы
        /// </summary>
        private string FindAcceptableTableName(string originalName, string name_scheme)
        {
            string name_table = originalName;
            if (classesOfMetods.isReservedName(name_table))
            {
                MessageBox.Show(Rekod.Properties.Resources.AET_NameIsReserved +
                    Environment.NewLine + Rekod.Properties.Resources.AET_ConNotSave + Environment.NewLine +
                    Rekod.Properties.Resources.AET_ChangeTable,
                    Rekod.Properties.Resources.AET_CreatingTable,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return null;
            }

            bool nameChanged = false;
            int count = 0;
            while (true)
            {
                using (SqlWork sqlQuery = new SqlWork())
                {
                    sqlQuery.sql = String.Format("SELECT count(*) FROM pg_tables WHERE tablename='{0}' AND schemaname='{1}';", name_table, name_scheme);
                    sqlQuery.ExecuteReader();
                    if (sqlQuery.CanRead())
                    {
                        if (sqlQuery.GetInt32(0) > 0)
                        {
                            nameChanged = true;
                            count++;
                        }
                        else
                            break;
                    }
                    name_table = originalName + count.ToString();
                }
            }
            if (nameChanged)
            {
                DialogResult dr = MessageBox.Show(Rekod.Properties.Resources.AET_AlreadyExistsTable + Environment.NewLine +
                    string.Format(Rekod.Properties.Resources.AET_SaveTableRename, name_table),
                    Rekod.Properties.Resources.AET_CreatingTable,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                {
                    MessageBox.Show(Rekod.Properties.Resources.AET_ConNotSave + Environment.NewLine + Rekod.Properties.Resources.AET_ChangeTable,
                                    Rekod.Properties.Resources.AET_CreatingTable,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return null;
                }
            }
            return name_table;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool ok = true;

            int start = (sender as TextBox).SelectionStart;
            int len = (sender as TextBox).SelectionLength;
            (sender as TextBox).Text = (sender as TextBox).Text.ToLower();
            (sender as TextBox).SelectionStart = start;
            (sender as TextBox).SelectionLength = len;

            if ((sender as TextBox).Text.Length > 0)
            {
                if (((sender as TextBox).Text[0] >= 'a' && (sender as TextBox).Text[0] <= 'z') || ((sender as TextBox).Text[0] == '_'))
                {
                    for (int i = 1; i < (sender as TextBox).Text.Length; i++)
                    {
                        if (!(((sender as TextBox).Text[i] >= 'a' && (sender as TextBox).Text[i] <= 'z') ||
                            //((sender as TextBox).Text[i] >= 'A' && (sender as TextBox).Text[i] <= 'Z') ||
                            ((sender as TextBox).Text[i] >= '0' && (sender as TextBox).Text[i] <= '9') ||
                            (sender as TextBox).Text[i] == '_'))
                            ok = false;
                    }
                }
                else
                    ok = false;
            }
            if (!ok)
            {
                tb.undo();
                int pp = (sender as TextBox).Text.Length;
                toolTip1.Show(Rekod.Properties.Resources.AET_TableNameFormat, this,
                        (sender as TextBox).Left + 20 + pp * 6, (sender as TextBox).Top + 30, 2000);
            }
            else
            {
                if (!tb.shanged)
                    tb.saveText();
                else
                    tb.shanged = false;
            }
            //(sender as TextBox).SelectionStart = start;
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            tb.saveText();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Up || e.KeyData == Keys.Down)
            {
                tb.saveText();
            }
        }

    }
}
