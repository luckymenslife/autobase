using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using Npgsql;
using Rekod.DBTablesEdit;
using ProjectionSelection;
using Rekod.ProjectionSelection;
using Rekod.Services;
using sscSync.Controller;
using Rekod.DBLayerForms;
using RESTLib.Model.REST;

namespace Rekod
{
    public partial class AddEditTable : UserControl
    {
        private bool upd;
        private int id;
        int type_frm; // Varyag
        private TextBoxInfo tb;//sasha
        private TextBoxInfo tb2;//sasha
        Dictionary<char, string> dic;
        private DBTablesGroups parent;
        private TableEdit table_edit;
        private bool have_photo_history;
        public AddEditTable(bool upd1, int id1, int type, DBTablesGroups par, TableEdit table_edit_frm)
        {
            InitializeComponent();
            type_frm = type;

            // classesOfMetods.getTableInfo(id1).type == 1 -> значит слой карты
            //if (classesOfMetods.getTableInfo(id1) != null && classesOfMetods.getTableInfo(id1).type != 1 || !upd1)
            //{
            //    button_sign.Visible = false;
            //    button_sign.Enabled = false;
            //}

            upd = upd1;
            id = id1;
            loadTipTable();
            loadTipGeom();
            loadSchems();
            if (cmbScheme.Items.Count > 0)
                cmbScheme.SelectedIndex = 0;
            else
                MessageBox.Show(Rekod.Properties.Resources.AET_NotRegScheme,
                    Rekod.Properties.Resources.DGBH_ErrorHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);

            setComboBox(cmbTypeGeom, 0);
            loadInfo();

            tb = new TextBoxInfo(this.txtNameBD);
            txtNameBD.TextChanged += new EventHandler(textBox1_TextChanged);
            txtNameBD.MouseUp += new MouseEventHandler(textBox1_MouseUp);
            txtNameBD.KeyUp += new KeyEventHandler(textBox1_KeyUp);

            tb2 = new TextBoxInfo(this.txtNameText);
            txtNameText.TextChanged += new EventHandler(textBox2_TextChanged);
            txtNameText.MouseUp += new MouseEventHandler(textBox2_MouseUp);
            txtNameText.KeyUp += new KeyEventHandler(textBox2_KeyUp);
            parent = par;
            table_edit = table_edit_frm;

            dic = initalizeDictionary();

            if (!SyncController.HasRight(id))
            {
                button2.Enabled = false;
            }

            if (Program.mapAdmin != null && !upd)
            {
                // группа указывается при создании таблицы, если есть соединение с SSC
                cbGroups.DisplayMember = "name";
                if (Program.mapAdmin.SscGroups != null)
                {
                    cbGroups.Items.AddRange(Program.mapAdmin.SscGroups.ToArray());
                    if (cbGroups.Items.Count > 0)
                        cbGroups.SelectedIndex = 0;
                }
            }
            else
            {
                // скрыть контролы группы
                int row = tableLayoutPanel1.GetRow(cbGroups);
                if (row != -1)
                {
                    tableLayoutPanel1.RowStyles[row].Height = 0;
                    tableLayoutPanel1.RowStyles[row].SizeType = SizeType.Absolute;
                    cbGroups.Hide();
                    label12.Hide();
                }
            }
        }
        private void loadInfo()
        {
            if (upd)
            {
                tablesInfo tInfo = classesOfMetods.getTableInfo(id);
                txtNameBD.Text = tInfo.nameDB;
                txtNameText.Text = tInfo.nameMap;
                setComboBox(cmbTypeTable, tInfo.type);
                setComboBox(cmbTypeGeom, classesOfMetods.GetIntGeomType(tInfo.GeomType_GC));

                if (tInfo.type == 1)
                {
                    tbProj.Text = tInfo.srid.ToString();
                }

                txtNameBD.Enabled = false;
                cmbTypeTable.Enabled = false;
                cmbTypeGeom.Enabled = false;
                cmbScheme.Enabled = false;
                cmbScheme.Text = tInfo.nameSheme;

                if (tInfo.map_style)
                {
                    cmbStyle.SelectedIndex = 1;
                }
                else
                {
                    cmbStyle.SelectedIndex = 0;
                }
                if (tInfo.photo)
                {
                    cmbPhoto.SelectedIndex = 1;
                }
                else
                {
                    cmbPhoto.SelectedIndex = 0;
                }
                chbBounds.Checked = tInfo.useBounds;
                nBoundBotton.Enabled = chbBounds.Checked;
                nBoundUp.Enabled = chbBounds.Checked;

                nBoundBotton.Value = tInfo.minScale;
                nBoundUp.Value = tInfo.maxScale;
                chbVisable.Checked = tInfo.defaultVisible;
                chbHidden.Checked = tInfo.hidden;
                ckbGraphicUnits.Checked = tInfo.graphic_units;
                chbNotDisplayWhenOpening.Checked = !tInfo.DisplayWhenOpening;
                nMinObjectSize.Value = tInfo.MinObjectSize;

                //history sanek
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT * FROM " + Program.scheme + ".table_history_info WHERE id_table=" + id.ToString();
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    //история включена, отключить на данный момент ее нельзя
                    chbHistory.Enabled = true;
                    chbHistory.Checked = true;
                }
                sqlCmd.Close();

                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT hastriggers FROM pg_tables WHERE schemaname like '" + tInfo.nameSheme + "' AND tablename like 'photo_" + tInfo.nameDB + "'";
                sqlCmd.Execute(false);
                have_photo_history = false;
                if (sqlCmd.CanRead())
                {
                    have_photo_history = sqlCmd.GetBoolean(0);
                }
                sqlCmd.Close();
                //history
            }
            else
            {
                if (type_frm == 1)
                {
                    tbProj.Text = "3395";
                }
                setComboBox(cmbTypeTable, type_frm);
                cmbPhoto.SelectedIndex = 0; // varyag
                cmbStyle.SelectedIndex = 0; // Varyag
                txtNameBD.Enabled = false;
                CheckBox edit = new CheckBox();
                edit.AutoSize = false;
                edit.Size = new Size(20, 20);
                edit.Appearance = Appearance.Button;
                edit.FlatStyle = FlatStyle.Flat;
                edit.FlatAppearance.BorderColor = Color.Silver;
                edit.Image = Rekod.Properties.Resources._1279535667_edit;
                edit.Left = txtNameBD.Right - edit.Width;
                edit.Top = txtNameBD.Top + 6;
                edit.Click += new EventHandler(edit_click);
                edit.Tag = true;
                this.Controls.Add(edit);
                edit.BringToFront();
            }
        }
        private void edit_click(object sender, EventArgs e)
        {
            CheckBox edit = (sender as CheckBox);
            if ((bool)edit.Checked)
            {
                //edit.Text = "По-умолчанию";
                //edit.Left = textBox1.Right-edit.Width;
                txtNameBD.Enabled = true;
                //edit.Tag = false;
            }
            else
            {
                //edit.Text = "Редактировать";
                //edit.Left = textBox1.Right - edit.Width;
                txtNameBD.Enabled = false;
                txtNameBD.Text = translite(txtNameText.Text.ToLower());
                //edit.Tag = true;
            }
        }
        private void loadSchems()
        {
            cmbScheme.Items.Clear();
            if (Program.schems.Count > 0)
            {
                for (int i = 0; Program.schems.Count > i; i++)
                {
                    itemObj item = new itemObj(1,
                        Program.schems[i],
                        Program.schems[i]);
                    cmbScheme.Items.Add(item);
                }
            }
        }
        private void loadTipTable()
        {
            cmbTypeTable.Items.Clear();
            if (Program.tip_table.Count > 0)
            {
                for (int i = 0; Program.tip_table.Count > i; i++)
                {
                    itemObj item = new itemObj(Program.tip_table[i].idTipTable,
                        Program.tip_table[i].nameTip,
                        Program.tip_table[i].mapLayer.ToString());
                    cmbTypeTable.Items.Add(item);
                }
            }
            //comboBox1.SelectedIndex = type_frm; // Varyag 
        }
        private void loadTipGeom()
        {
            cmbTypeGeom.Items.Clear();
            if (Program.tip_geom.Count > 0)
            {
                for (int i = 0; Program.tip_geom.Count > i; i++)
                {
                    itemObj item = new itemObj(Program.tip_geom[i].idTipGeom,
                        Program.tip_geom[i].nameGeom,
                        Program.tip_geom[i].nameDb);
                    cmbTypeGeom.Items.Add(item);
                }
            }

        }
        private void setComboBox(ComboBox cmb, int id)
        {
            for (int i = 0; cmb.Items.Count > i; i++)
            {
                if (((itemObj)cmb.Items[i]).Id_o == id)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((itemObj)cmbTypeTable.SelectedItem).Layer == "True")
            {
                if (!upd)
                {
                    cmbTypeGeom.Enabled = true;
                    cbGroups.Enabled = true;
                }

                cmbStyle.SelectedIndex = 0;
                cmbStyle.Enabled = false;
                chbBounds.Enabled = true;
                nBoundBotton.Enabled = chbBounds.Checked;
                nBoundUp.Enabled = chbBounds.Checked;
                chbVisable.Enabled = true;
                nMinObjectSize.Enabled = true;
                //ckbGraphicUnits.Enabled = true;
            }
            else
            {
                cmbTypeGeom.Enabled = false;
                cbGroups.Enabled = false;
                setComboBox(cmbTypeGeom, 0);
                if (((itemObj)cmbTypeTable.SelectedItem).Id_o == 2 || ((itemObj)cmbTypeTable.SelectedItem).Id_o == 3)
                {
                    cmbStyle.Enabled = true;
                }
                else
                    cmbStyle.Enabled = false;
                chbBounds.Enabled = false;
                nBoundBotton.Enabled = false;
                nBoundUp.Enabled = false;
                chbVisable.Enabled = false;
                nMinObjectSize.Enabled = false;
                label11.Enabled = false;
                label9.Enabled = false;
                label10.Enabled = false;
                ckbGraphicUnits.Enabled = false;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (txtNameBD.Text != "" && txtNameText.Text.Trim() != ""
                && txtNameBD.Text.IndexOf(" ") < 0 && cmbScheme.SelectedItem != null)
            {
                if (((itemObj)cmbTypeTable.SelectedItem).Layer == "True")
                {
                    if (((itemObj)cmbTypeGeom.SelectedItem).Id_o == 0)
                    {
                        MessageBox.Show(Rekod.Properties.Resources.AET_MustBeGeom, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (String.IsNullOrEmpty(tbProj.Text))
                    {
                        MessageBox.Show(Rekod.Properties.Resources.AET_MustBeProj, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    chbVisable.Checked = false;
                }

                if (chbBounds.Checked && nBoundBotton.Value >= nBoundUp.Value)
                {
                    MessageBox.Show(Rekod.Properties.Resources.AET_ErrorBoundValue, Rekod.Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!upd)
                {
                    int? id_group = null;
                    if (Program.mapAdmin != null && cbGroups.Enabled && ((itemObj)cmbScheme.SelectedItem).Name_o == "data") // нужна ли регистрация
                    {
                        if (cbGroups.SelectedIndex == -1)
                        {
                            MessageBox.Show("Не указана группа!",
                                Rekod.Properties.Resources.AET_CreatingTable,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else
                        {
                            if (Program.group_info != null)
                            {
                                var group = Program.group_info.Find(w => w.name == (cbGroups.SelectedItem as Group).name);
                                if (group.id != 0)
                                    id_group = group.id;
                            }
                        }
                    }

                    bool exist_table = false;
                    bool find_name = false;
                    string name_table = txtNameBD.Text;
                    int countt = 0;

                    if (classesOfMetods.isReservedName(name_table))
                    {
                        MessageBox.Show(Rekod.Properties.Resources.AET_NameIsReserved +
                            "\n" + Rekod.Properties.Resources.AET_ConNotSave + "\n" +
                            Rekod.Properties.Resources.AET_ChangeTable,
                            Rekod.Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    while (!find_name)
                    {
                        find_name = true;
                        using (SqlWork sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = "SELECT count(*) FROM pg_tables WHERE tablename='" + name_table + "' AND schemaname='" + cmbScheme.SelectedItem + "';";
                            sqlCmd.ExecuteReader();
                            if (sqlCmd.CanRead())
                            {
                                if (sqlCmd.GetInt32(0) > 0)
                                {
                                    exist_table = true;
                                    find_name = false;
                                    countt++;
                                }
                            }
                            if (!find_name)
                                name_table = txtNameBD.Text + "_" + countt.ToString();
                        }
                    }
                    if (exist_table)
                    {
                        DialogResult dr = MessageBox.Show(Rekod.Properties.Resources.AET_AlreadyExistsTable + "\n" +
                                        String.Format(Rekod.Properties.Resources.AET_SaveTableRename, name_table),
                                        Rekod.Properties.Resources.AET_CreatingTable, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Cancel)
                        {
                            MessageBox.Show(Rekod.Properties.Resources.AET_ConNotSave + "\n" +
                                Rekod.Properties.Resources.AET_ChangeTable, Rekod.Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    //SQLCommand sql = new SQLCommand(conn);
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "SELECT " + Program.scheme + ".create_table(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9,@val10,@val11)";
                        int table_type = ((itemObj)cmbTypeTable.SelectedItem).Id_o;

                        var parms = new IParams[]
                                        {
                                            new Params
                                                {
                                                    paramName = "@val1",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = cmbScheme.Text
                                                },
                                            new Params
                                                {
                                                    paramName = "@val2",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = name_table
                                                },
                                            new Params
                                                {
                                                    paramName = "@val3",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                    value = txtNameText.Text
                                                },
                                            new Params
                                                {
                                                    paramName = "@val4",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = ((itemObj) cmbTypeTable.SelectedItem).Id_o
                                                },
                                            new Params
                                                {
                                                    paramName = "@val5",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = ((itemObj) cmbTypeGeom.SelectedItem).Id_o
                                                },
                                            new Params
                                                {
                                                    paramName = "@val6",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = (table_type==1)?Int32.Parse(tbProj.Text):-1
                                                },
                                            new Params
                                                {
                                                    paramName = "@val7",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = Convert.ToBoolean(cmbStyle.SelectedIndex)
                                                },
                                            new Params
                                                {
                                                    paramName = "@val8",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = Convert.ToBoolean(cmbPhoto.SelectedIndex)
                                                },
                                            new Params
                                                {
                                                    paramName = "@val9",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Integer,
                                                    value = id_group
                                                },
                                            new Params
                                                {
                                                    paramName = "@val10",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = chbVisable.Checked
                                                },
                                            new Params
                                                {
                                                    paramName = "@val11",
                                                    typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                                    value = chbHidden.Checked
                                                }
                                        };
                        try
                        {
                            sqlCmd.ExecuteReader(parms);
                            if (sqlCmd.CanRead())
                                id = sqlCmd.GetInt32(0);
                            else
                                throw new Exception(Rekod.Properties.Resources.AET_ErrorCreatingTable);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            table_edit.reloadTables();
                            parent.CloseElemsAfter(this, true);
                            return;
                        }
                    }
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET " +
                           "min_object_size = @min_object_size, " +
                           "graphic_units = @graphic_units, " +
                           "display_when_opening = @display_when_opening, " +
                            "use_bounds = @use_bounds, " +
                            "min_scale = @min_scale, " +
                            "max_scale = @max_scale " +
                           " WHERE id = " + id.ToString();

                        Params[] parms = new Params[6];
                        parms[0] = new Params();
                        parms[0].paramName = "@min_object_size";
                        parms[0].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[0].value = Convert.ToInt32(nMinObjectSize.Value);

                        parms[1] = new Params();
                        parms[1].paramName = "@graphic_units";
                        parms[1].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[1].value = ckbGraphicUnits.Checked;

                        parms[2] = new Params();
                        parms[2].paramName = "@display_when_opening";
                        parms[2].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[2].value = !chbNotDisplayWhenOpening.Checked;

                        parms[3] = new Params();
                        parms[3].paramName = "@use_bounds";
                        parms[3].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[3].value = chbBounds.Checked;

                        parms[4] = new Params();
                        parms[4].paramName = "@min_scale";
                        parms[4].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[4].value = Convert.ToInt32(nBoundBotton.Value);

                        parms[5] = new Params();
                        parms[5].paramName = "@max_scale";
                        parms[5].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[5].value = Convert.ToInt32(nBoundUp.Value);

                        try
                        { sqlCmd.ExecuteNonQuery(parms); }
                        catch (Exception ex)
                        { MessageBox.Show(ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error); }

                    }
                    if (chbHistory.Checked)
                    {
                        using (var sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = "SELECT " + Program.scheme + ".create_history_table(" + id.ToString() + ")";
                            try
                            { sqlCmd.Execute(true); }
                            catch (Exception ex)
                            { MessageBox.Show(Rekod.Properties.Resources.AET_ErrorCreatingHistory + "\n" + ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error); }
                        }

                    }

                    table_edit.reloadTables();
                    if (Program.mapAdmin != null && ((itemObj)cmbTypeTable.SelectedItem).Id_o == 1)
                    {
                        if (((itemObj)cmbScheme.SelectedItem).Name_o == "data")
                            SyncController.RegisterTable(id, ((RESTLib.Model.REST.Group)cbGroups.SelectedItem).name);
                    }
                }
                else
                {
                    {
                        //SQLCommand sql = new SQLCommand(conn);
                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET " +
                            "name_map = @name_map, " +
                            "photo = @photo, " +
                            "use_bounds = @use_bounds, " +
                            "min_scale = @min_scale, " +
                            "max_scale = @max_scale," +
                            "default_visibl = @default_visibl," +
                            "hidden = @hidden," +
                            "min_object_size = @min_object_size," +
                            "graphic_units = @graphic_units, " +
                            "display_when_opening = @display_when_opening " +
                            " WHERE id = " + id.ToString();
                        //qwCmd.CommandText = "UPDATE " + Program.scheme + ".table_info SET name_map = '" + textBox2.Text + "', photo = " + Convert.ToBoolean(comboBox4.SelectedIndex).ToString() + " WHERE id = " + id.ToString();

                        Params[] parms = new Params[10];

                        parms[0] = new Params();
                        parms[0].paramName = "@name_map";
                        parms[0].typeData = NpgsqlTypes.NpgsqlDbType.Text;
                        parms[0].value = txtNameText.Text;

                        parms[1] = new Params();
                        parms[1].paramName = "@photo";
                        parms[1].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[1].value = Convert.ToBoolean(cmbPhoto.SelectedIndex);

                        parms[2] = new Params();
                        parms[2].paramName = "@use_bounds";
                        parms[2].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[2].value = chbBounds.Checked;

                        parms[3] = new Params();
                        parms[3].paramName = "@min_scale";
                        parms[3].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[3].value = Convert.ToInt32(nBoundBotton.Value);

                        parms[4] = new Params();
                        parms[4].paramName = "@max_scale";
                        parms[4].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[4].value = Convert.ToInt32(nBoundUp.Value);

                        parms[5] = new Params();
                        parms[5].paramName = "@default_visibl";
                        parms[5].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[5].value = chbVisable.Checked;

                        parms[6] = new Params();
                        parms[6].paramName = "@hidden";
                        parms[6].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[6].value = chbHidden.Checked;

                        parms[7] = new Params();
                        parms[7].paramName = "@min_object_size";
                        parms[7].typeData = NpgsqlTypes.NpgsqlDbType.Integer;
                        parms[7].value = Convert.ToInt32(nMinObjectSize.Value);

                        parms[8] = new Params();
                        parms[8].paramName = "@graphic_units";
                        parms[8].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[8].value = ckbGraphicUnits.Checked;

                        parms[9] = new Params("@display_when_opening", !chbNotDisplayWhenOpening.Checked, DbType.Boolean);
                        try
                        {
                            sqlCmd.ExecuteNonQuery(parms);
                            sqlCmd.Close();
                            classesOfMetods.getTableInfo(id).MinObjectSize = Convert.ToInt32(nMinObjectSize.Value);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            sqlCmd.Close();
                        }
                        if (classesOfMetods.getTableInfo(id).map_style)
                        {
                            if (cmbStyle.SelectedIndex == 0)
                            {
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET map_style=false WHERE id = " + id.ToString();
                                sqlCmd.Execute(true);
                                sqlCmd.Close();
                            }
                        }
                        else
                        {
                            if (cmbStyle.SelectedIndex == 1)
                            {
                                bool exists_col = true;
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET map_style=true WHERE id = " + id.ToString();
                                sqlCmd.Execute(true);
                                sqlCmd.Close();
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "SELECT EXISTS(SELECT true FROM information_schema.columns " +
                                    "WHERE table_schema like '" + classesOfMetods.getTableInfo(id).nameSheme + "' " +
                                    "AND table_name like '" + classesOfMetods.getTableInfo(id).nameDB + "' AND column_name like 'exists_style')";
                                sqlCmd.Execute(false);
                                if (sqlCmd.CanRead())
                                {
                                    exists_col = sqlCmd.GetBoolean(0);
                                }
                                sqlCmd.Close();
                                if (!exists_col)
                                {
                                    using (sqlCmd = new SqlWork())
                                    {
                                        sqlCmd.sql = "ALTER TABLE " + classesOfMetods.getTableInfo(id).nameSheme + "." + classesOfMetods.getTableInfo(id).nameDB + " " +
                                                                "ADD exists_style boolean DEFAULT true, " +
                                                                "ADD fontname character varying DEFAULT 'Map Symbols', " +
                                                                "ADD fontcolor integer DEFAULT 16711680, " +
                                                                "ADD fontframecolor integer DEFAULT 16711680, " +
                                                                "ADD fontsize integer DEFAULT 12, " +
                                                                "ADD symbol integer DEFAULT 35, " +
                                                                "ADD pencolor integer DEFAULT 16711680, " +
                                                                "ADD pentype integer DEFAULT 2, " +
                                                                "ADD penwidth integer DEFAULT 1, " +
                                                                "ADD brushbgcolor bigint DEFAULT 16711680, " +
                                                                "ADD brushfgcolor integer DEFAULT 16711680, " +
                                                                "ADD brushstyle integer DEFAULT 0, " +
                                                                "ADD brushhatch integer DEFAULT 1; ";
                                        sqlCmd.Execute(true);
                                        sqlCmd.Close();
                                    }
                                    if (classesOfMetods.getTableInfo(id).isHistory)
                                    {
                                        string history_table_name = "";
                                        using (sqlCmd = new SqlWork())
                                        {
                                            sqlCmd.sql = @"SELECT history_table_name
  FROM sys_scheme.table_history_info
  WHERE id_table=" + id.ToString() + @" AND is_work=true 
  ORDER BY id_history_table DESC
  LIMIT 1;";
                                            history_table_name = sqlCmd.ExecuteScalar<String>();
                                            sqlCmd.Close();
                                        }
                                        using (sqlCmd = new SqlWork())
                                        {
                                            sqlCmd.sql = "ALTER TABLE \"" + classesOfMetods.getTableInfo(id).nameSheme + "\".\"" + history_table_name + "\" " +
                                                                    "ADD exists_style boolean DEFAULT true, " +
                                                                    "ADD fontname character varying DEFAULT 'Map Symbols', " +
                                                                    "ADD fontcolor integer DEFAULT 16711680, " +
                                                                    "ADD fontframecolor integer DEFAULT 16711680, " +
                                                                    "ADD fontsize integer DEFAULT 12, " +
                                                                    "ADD symbol integer DEFAULT 35, " +
                                                                    "ADD pencolor integer DEFAULT 16711680, " +
                                                                    "ADD pentype integer DEFAULT 2, " +
                                                                    "ADD penwidth integer DEFAULT 1, " +
                                                                    "ADD brushbgcolor bigint DEFAULT 16711680, " +
                                                                    "ADD brushfgcolor integer DEFAULT 16711680, " +
                                                                    "ADD brushstyle integer DEFAULT 0, " +
                                                                    "ADD brushhatch integer DEFAULT 1; ";
                                            sqlCmd.Execute(true);
                                            sqlCmd.Close();
                                        }
                                    }
                                }
                            }
                        }
                        if (cmbPhoto.SelectedIndex == 1)
                        {
                            bool exist = false;
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT * FROM pg_tables WHERE schemaname like '" + cmbScheme.Text +
                                "' AND tablename like 'photo_" + txtNameBD.Text + "'";
                            sqlCmd.Execute(false);
                            if (sqlCmd.CanRead())
                            {
                                exist = true;
                            }
                            else
                            {
                                exist = false;
                            }
                            sqlCmd.Close();
                            if (exist)
                            {
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "SELECT id FROM " + Program.scheme + ".table_photo_info WHERE id_table = " + id.ToString() + ";";
                                sqlCmd.Execute(false);
                                bool est_v_tabl;
                                if (sqlCmd.CanRead())
                                {
                                    est_v_tabl = true;
                                }
                                else
                                {
                                    est_v_tabl = false;
                                }
                                sqlCmd.Close();
                                if (!est_v_tabl)
                                {
                                    sqlCmd = new SqlWork();
                                    sqlCmd.sql = "INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)" +
                                        " VALUES (" + id.ToString() + ", 'photo_" + txtNameBD.Text + "', 'id_obj', 'file', '" +
                                        classesOfMetods.getTableInfo(id).pkField + "');";
                                    sqlCmd.Execute(true);
                                    sqlCmd.Close();
                                }
                            }
                            else
                            {
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "CREATE TABLE \"" + cmbScheme.Text + "\".\"photo_" + txtNameBD.Text + "\"" +
                                                "(" +
                                                  "id serial NOT NULL," +
                                                  "id_obj integer," +
                                                  "file bytea NOT NULL," +
                                                  "img_preview bytea," +
                                                  "file_name character varying," +
                                                  "is_photo boolean," +
                                                  "dataupd timestamp without time zone DEFAULT now()," +
                                                  "master_id integer, " +
                                                  "CONSTRAINT photo_" + txtNameBD.Text + "_pkey PRIMARY KEY (id)," +
                                                  "CONSTRAINT photo_" + txtNameBD.Text + "_id_obj_fkey FOREIGN KEY (id_obj)" +
                                                  "    REFERENCES " + cmbScheme.Text + "." + txtNameBD.Text + " (" + classesOfMetods.getTableInfo(id).pkField + ") MATCH SIMPLE" +
                                                   "   ON UPDATE CASCADE ON DELETE CASCADE" +
                                                ")" +
                                                "WITH (" +
                                                  "OIDS=FALSE" +
                                                ");" +
                                                "GRANT ALL ON TABLE " + cmbScheme.Text + ".photo_" + txtNameBD.Text + " TO postgres;" +
                                                "GRANT ALL ON TABLE " + cmbScheme.Text + ".photo_" + txtNameBD.Text + " TO public;";
                                sqlCmd.Execute(true);
                                sqlCmd.Close();
                                sqlCmd = new SqlWork();
                                sqlCmd.sql = "INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)" +
                                    " VALUES (" + id.ToString() + ", 'photo_" + txtNameBD.Text + "', 'id_obj', 'file', '" + classesOfMetods.getTableInfo(id).pkField + "');";
                                sqlCmd.Execute(true);
                                sqlCmd.Close();
                            }

                        }
                        else
                        {
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "DELETE FROM " + Program.scheme + ".table_photo_info WHERE id_table = " + id.ToString();
                            sqlCmd.Execute(true);
                            sqlCmd.Close();
                        }



                        //history
                        if (chbHistory.Enabled)
                        {
                            tablesInfo ti = classesOfMetods.getTableInfo(id);
                            if (chbHistory.Checked)
                            {
                                if (chbHistory.Checked != ti.isHistory)
                                {
                                    sqlCmd = new SqlWork();
                                    sqlCmd.sql = "SELECT " + Program.scheme + ".create_history_table(" + id.ToString() + ")";
                                    try
                                    {
                                        sqlCmd.Execute(true);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    finally
                                    {
                                        sqlCmd.Close();
                                    }

                                    if (Convert.ToBoolean(cmbPhoto.SelectedIndex))
                                    {
                                        sqlCmd = new SqlWork();
                                        sqlCmd.sql = "SELECT " + Program.scheme + ".set_photo_history(" + id.ToString() + ", true)";
                                        try
                                        {
                                            sqlCmd.Execute(true);
                                        }
                                        catch (Exception ex)
                                        {//значит триггер был)))) а черт его знает как определить есть ли он
                                        }
                                        finally
                                        {
                                            sqlCmd.Close();
                                        }
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                if (chbHistory.Checked != ti.isHistory)
                                {
                                    if (MessageBox.Show(Rekod.Properties.Resources.AET_TurnHistory + " " + Rekod.Properties.Resources.AET_PerformOperation,
                                        Rekod.Properties.Resources.InformationMessage_Header,
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                    {
                                        sqlCmd = new SqlWork();
                                        sqlCmd.sql = "SELECT " + Program.scheme + ".delete_history(" + id.ToString() + ")";
                                        try
                                        {
                                            sqlCmd.Execute(true);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        finally
                                        {
                                            sqlCmd.Close();
                                        }

                                        if (Convert.ToBoolean(cmbPhoto.SelectedIndex))
                                        {
                                            sqlCmd = new SqlWork();
                                            sqlCmd.sql = "SELECT " + Program.scheme + ".set_photo_history(" + id.ToString() + ", false)";
                                            try
                                            {
                                                sqlCmd.Execute(true);
                                            }
                                            catch (Exception ex)
                                            {//значит триггер был)))) а черт его знает как определить есть ли он
                                            }
                                            finally
                                            {
                                                sqlCmd.Close();
                                            }
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                        }

                    }

                    SyncController.ReloadTable(id);
                }

                table_edit.reloadTables();
                parent.CloseElemsAfter(this, true);
                classesOfMetods.reloadUseBounds(id);
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.AET_RequiredParams, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            parent.CloseElemsAfter(this, true);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            nBoundBotton.Enabled = ((CheckBox)sender).Checked;
            nBoundUp.Enabled = ((CheckBox)sender).Checked;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        ///////////////////////////////sasha

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


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (txtNameBD == null)
                return;

            if (!string.IsNullOrEmpty(txtNameText.Text))
            {
                if (!tb2.shanged)
                    tb2.saveText();
                else
                    tb2.shanged = false;
            }
            if (txtNameBD.Enabled == false && !upd)
            {
                txtNameBD.Text = translite(txtNameText.Text.ToLower());
            }
        }

        private void textBox2_MouseUp(object sender, MouseEventArgs e)
        {
            tb2.saveText();
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Up || e.KeyData == Keys.Down)
            {
                tb2.saveText();
            }
        }

        private Dictionary<char, string> initalizeDictionary()
        {
            Dictionary<char, string> dicc = new Dictionary<char, string>();
            dicc.Add('й', "j");
            dicc.Add('ц', "c");
            dicc.Add('у', "u");
            dicc.Add('к', "k");
            dicc.Add('е', "e");
            dicc.Add('н', "n");
            dicc.Add('г', "g");
            dicc.Add('ш', "sh");
            dicc.Add('щ', "w");
            dicc.Add('з', "z");
            dicc.Add('х', "h");
            dicc.Add('ъ', "");
            dicc.Add('ф', "f");
            dicc.Add('ы', "y");
            dicc.Add('в', "v");
            dicc.Add('а', "a");
            dicc.Add('п', "p");
            dicc.Add('р', "r");
            dicc.Add('о', "o");
            dicc.Add('л', "l");
            dicc.Add('д', "d");
            dicc.Add('ж', "zh");
            dicc.Add('э', "je");
            dicc.Add('я', "ja");
            dicc.Add('ч', "ch");
            dicc.Add('с', "s");
            dicc.Add('м', "m");
            dicc.Add('и', "i");
            dicc.Add('т', "t");
            dicc.Add('ь', "");
            dicc.Add('б', "b");
            dicc.Add('ю', "ju");
            dicc.Add('ё', "jo");
            return dicc;
        }
        private string translite(string ss)
        {
            string ret = "";
            if (ss.Length > 0 && char.IsNumber(ss[0]))
            {
                ret = "_";
            }
            foreach (char c in ss)
            {
                if (dic.ContainsKey(c))
                    ret += dic[c];
                else if (char.IsLetterOrDigit(c))
                    ret += c.ToString();
                else
                    ret += "_";
            }
            return ret;
        }

        private void button_sign_Click(object sender, EventArgs e)
        {
            LabelControl sc = new LabelControl(parent, id);
            parent.CloseElemsAfter(this, false);
            parent.AddNewElemModal(sc, Rekod.Properties.Resources.AET_AddLabel);
        }

        private void cmbTypeGeom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTypeGeom.SelectedItem != null)
            {
                if (!upd && ((itemObj)cmbTypeGeom.SelectedItem).Id_o != 0)
                {
                    bnSelectProj.Enabled = true;
                    tbProj.Text = "3395";
                }
                else
                {
                    bnSelectProj.Enabled = false;
                    tbProj.Text = "";
                }
                ckbGraphicUnits.Enabled = ((itemObj)cmbTypeGeom.SelectedItem).Id_o == 1;

            }
        }

        private void bnSelectProj_Click(object sender, EventArgs e)
        {
            // Окно выбора проекции
            SelectProjectionV frmProj = new SelectProjectionV();
            SelectProjectionVM datacontext = new SelectProjectionVM(frmProj);
            frmProj.DataContext = datacontext;
            if (frmProj.ShowDialog() == true)
            {
                var proj = datacontext.SelectedProj;
                if (proj != null)
                    tbProj.Text = proj.Srid.ToString();
            }
        }

        private void cmbScheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbGroups.Enabled = (((itemObj)cmbScheme.SelectedItem).Name_o == "data");
        }
    }

}