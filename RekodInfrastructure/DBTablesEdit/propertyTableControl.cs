using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using axVisUtils;
using axVisUtils.Styles;
using Rekod.Services;

namespace Rekod
{
    public partial class propertyTableControl : UserControl
    {
        /*Информация про comboBox16 - по какому параметру измененять цвет
         * Заливки 0
         * Фона 1
         * Границ или линий 2
         * Каймы 3
        */
        private List<itemObj> typeColors;
        private int id;
        private bool geom;
        private string tableName;
        private DBTablesGroups parent;
        private TableEdit table_edit;

        public propertyTableControl(int id1, DBTablesGroups par, TableEdit table_edit_frm)
        {
            InitializeComponent();
            id = id1;
            geom = getGeomTable(id);
            parent = par;
            table_edit = table_edit_frm;
            if (!geom)
            {
                MessageBox.Show(Rekod.Properties.Resources.PTC_TableIsNotLayer);
                parent.CloseElemsAfter(this, true);
            }

            cbRefType.SelectedIndex = 0;

            loadTableGeomField();
            loadTableStyleField();
            loadTables();
            loadInfo();
            loadDaipazonInfo(id);
                        
            SetControlVisibility();

            if (!Rekod.DBTablesEdit.SyncController.HasRight(id))
            {
                button2.Enabled = false;
            }
        }

        private void SetControlVisibility()
        {
            panelRef.Visible = cbStyleType.SelectedIndex == 1;
            panelInterval.Visible = cbStyleType.SelectedIndex == 2;
            panelRange.Visible = cbStyleType.SelectedIndex == 3;
            panelPic.Visible = cbStyleType.SelectedIndex == 1 && cbRefType.SelectedIndex == 1;
            button3.Visible = cbStyleType.SelectedIndex == 0 || cbStyleType.SelectedIndex == 3;
        }

        private void loadInfo()
        {
            bool default_style = false;
            bool range_colors = false;
            string style_field = String.Empty;
            string imageColumn = String.Empty;
            string angleColumn = String.Empty;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT geom_field, default_style, style_field, range_colors, image_column, angle_column FROM " 
                + Program.scheme + ".table_info WHERE id = " + id.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                cbGeomField.Text = sqlCmd.GetValue<string>(0);

                if (sqlCmd.GetBoolean(1))
                {
                    default_style = sqlCmd.GetBoolean(1);
                    range_colors = sqlCmd.GetBoolean(3);
                    if (!sqlCmd.GetBoolean(3))
                    {
                        cbStyleType.SelectedIndex = 0;
                    }
                    else
                    {
                        cbStyleType.SelectedIndex = 3;
                        button3.Visible = true;
                    }
                }
                else
                {
                    style_field = sqlCmd.GetValue<string>(2);
                    imageColumn = sqlCmd.GetString("image_column");
                    angleColumn = sqlCmd.GetString("angle_column");
                }
            }
            sqlCmd.Close();
            if (!default_style)
            {
                int ref_table = 0, ref_field = 0, ref_field_end = 0, ref_field_name = 0;
                bool refer = false;
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT is_reference, is_interval, ref_table, ref_field, ref_field_end, ref_field_name " +
                            "FROM " + Program.scheme + ".table_field_info WHERE is_style = true AND id_table = " + id;
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    if (sqlCmd.GetBoolean("is_reference"))
                    {
                        cbStyleType.SelectedIndex = 1;
                        cbRefType.SelectedIndex = String.IsNullOrEmpty(imageColumn) ? 0 : 1;

                        ref_table = sqlCmd.GetInt32("ref_table");
                        ref_field = sqlCmd.GetInt32("ref_field");
                        ref_field_name = sqlCmd.GetInt32("ref_field_name");
                        refer = true;
                    }
                    if (sqlCmd.GetBoolean("is_interval"))
                    {
                        cbStyleType.SelectedIndex = 2;
                        ref_table = sqlCmd.GetInt32("ref_table");
                        ref_field = sqlCmd.GetInt32("ref_field");
                        ref_field_end = sqlCmd.GetInt32("ref_field_end");
                        ref_field_name = sqlCmd.GetInt32("ref_field_name");
                    }
                }
                sqlCmd.Close();
                if (refer)
                {
                    cbRefStyleField.Text = style_field;
                    setComboBox(cbRefTable, ref_table);
                    setComboBox(cbRefLink, ref_field);
                    setComboBox(cbRefFieldName, ref_field_name);
                    setComboBox(cbFileNameColumn, imageColumn);
                    setComboBox(cbAngleColumn, angleColumn);
                }
                else
                {
                    cbIntervalStyleField.Text = style_field;
                    setComboBox(cbIntervalTable, ref_table);
                    setComboBox(cbIntervalStart, ref_field);
                    setComboBox(cbIntervalEnd, ref_field_end);
                    setComboBox(cbIntervalName, ref_field_name);
                }
            }
            else
            {
                if (range_colors)
                {

                }
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
        private void setComboBox(ComboBox cmb, string fieldName)
        {
            for (int i = 0; cmb.Items.Count > i; i++)
            {
                if (((itemObj)cmb.Items[i]).Name_o == fieldName)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }
        private bool getGeomTable(int idT)
        {
            bool map = false;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT tt.map_layer, ti.name_db FROM " + Program.scheme + ".table_info ti, " + Program.scheme + ".table_type_table tt" +
                    " WHERE tt.id=ti.type AND ti.id= " + idT;
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                map = sqlCmd.GetBoolean(0);
                tableName = sqlCmd.GetValue<string>(1);
            }
            sqlCmd.Close();
            return map;
        }
        private void loadTables()
        {
            cbRefTable.Items.Clear();
            cbIntervalTable.Items.Clear();

            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map, type FROM " + Program.scheme + ".table_info ti WHERE map_style = true ORDER BY name_db";
            sqlCmd.Execute(false);

            int max_width_1 = 0, max_width_2 = 0;

            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0),
                    String.Format("{0} ({1})", sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2)),
                    sqlCmd.GetValue<string>(1));

                Label tempLabel = new Label();

                switch (sqlCmd.GetInt32(3))
                {
                    case 2:
                        cbRefTable.Items.Add(item);

                        tempLabel.Text = item.Name_o;
                        if (tempLabel.PreferredWidth > max_width_1)
                            max_width_1 = tempLabel.PreferredWidth;
                        break;
                    case 3:
                        cbIntervalTable.Items.Add(item);

                        tempLabel.Text = item.Name_o;
                        if (tempLabel.PreferredWidth > max_width_2)
                            max_width_2 = tempLabel.PreferredWidth;
                        break;
                    default:
                        break;
                }

            }

            if (max_width_1 > cbRefTable.DropDownWidth)
                cbRefTable.DropDownWidth = max_width_1;
            if (max_width_2 > cbIntervalTable.DropDownWidth)
                cbIntervalTable.DropDownWidth = max_width_2;

            sqlCmd.Close();
        }
        private void loadTableGeomField()
        {
            cbGeomField.Items.Clear();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE type_field = 5 AND id_table = " + id.ToString();
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                cbGeomField.Items.Add(item);
            }
            sqlCmd.Close();
        }
        private void loadTableStyleField()
        {
            cbRefStyleField.Items.Clear();
            cbIntervalStyleField.Items.Clear();
            cbAngleColumn.Items.Clear();
            cbAngleColumn.Items.Add(new itemObj(-1, "<Не указано>", null));
            cbAngleColumn.SelectedIndex = 0;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format(@"SELECT 
                                            tfi.id, tfi.name_db, tfi.name_map 
                                            FROM sys_scheme.table_field_info tfi
                                            INNER JOIN sys_scheme.table_info ti
                                            ON ti.id = tfi.id_table
                                            WHERE (tfi.type_field = 1 OR tfi.type_field = 6) 
                                                AND tfi.id_table = {0} AND tfi.name_db <> ti.pk_fileld", id);
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                cbRefStyleField.Items.Add(item);
                cbIntervalStyleField.Items.Add(item);
                cbAngleColumn.Items.Add(item);
            }
            sqlCmd.Close();
        }
        private void propertyTableControl_Load(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (cbGeomField.SelectedItem != null)
            {
                //if (radioButton1.Checked)
                if (cbStyleType.SelectedIndex == 0)
                { // одинаково для всех
                    String sql = String.Format(@"
UPDATE {0}.table_field_info SET is_style = false WHERE id_table = {1};
UPDATE {0}.table_info SET geom_field = '{2}', default_style = true, range_colors=false WHERE id = {1};",
Program.scheme, id, cbGeomField.Text);

                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = sql;
                        sqlCmd.Execute(true);
                    }

                    setStyleSetting(1);
                    classesOfMetods.reloadLayer(id);
                    parent.CloseElemsAfter(this, true);
                }
                //if (radioButton2.Checked)
                if (cbStyleType.SelectedIndex == 1)
                { // по справочнику
                    if (cbRefStyleField.SelectedItem != null &&
                        cbRefTable.SelectedItem != null &&
                        cbRefLink.SelectedItem != null &&
                        cbRefFieldName.SelectedItem != null)
                    {
                        String sql = String.Format(@"
UPDATE {0}.table_field_info SET is_style = false WHERE id_table = {1};
UPDATE {0}.table_info SET geom_field = '{2}', default_style = false, style_field = '{3}' WHERE id = {1};",
    Program.scheme, id, cbGeomField.Text, cbRefStyleField.Text);

                        if (cbStyleType.SelectedIndex == 1
                            && cbRefType.SelectedIndex == 1)
                        {
                            if (cbFileNameColumn.SelectedItem != null)
                            {
                                sql += String.Format(@"UPDATE {0}.table_info SET image_column='{2}' , angle_column='{3}' WHERE id = {1};",
                                    Program.scheme, id, cbFileNameColumn.Text, cbAngleColumn.SelectedIndex == 0 ? "" : cbAngleColumn.Text);
                            }
                            else
                            {
                                MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                                return;
                            }
                        }

                        using (SqlWork sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = sql;
                            sqlCmd.Execute(true);
                        }

                        setStyleSetting(2);
                        classesOfMetods.reloadLayer(id);
                        parent.CloseElemsAfter(this, true);
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                        return;
                    }
                }
                if (cbStyleType.SelectedIndex == 2)
                //if (radioButton3.Checked)
                { // по интервалу
                    if (cbIntervalStyleField.SelectedItem != null &&
                        cbIntervalTable.SelectedItem != null &&
                        cbIntervalStart.SelectedItem != null &&
                        cbIntervalEnd.SelectedItem != null &&
                        cbIntervalName.SelectedItem != null)
                    {
                        String sql = String.Format(@"
UPDATE {0}.table_field_info SET is_style = false WHERE id_table = {1};
UPDATE {0}.table_info SET geom_field = '{2}', default_style = false, style_field = '{3}' WHERE id = {1};",
    Program.scheme, id, cbGeomField.Text, cbIntervalStyleField.Text);

                        using (SqlWork sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = sql;
                            sqlCmd.Execute(true);
                        }

                        setStyleSetting(3);
                        classesOfMetods.reloadLayer(id);
                        parent.CloseElemsAfter(this, true);
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                        return;
                    }
                }
                if (cbStyleType.SelectedIndex == 3)
                { // по диапазону
                    if (cbRangeValueField.SelectedItem != null &&
                        cbRangeColorChange.SelectedItem != null)
                    {
                        if (numericUpDownRangeMinValue.Value > numericUpDownRangeMaxValue.Value && checkBoxRangeMin.Checked && checkBoxRangeMax.Checked)
                        {
                            MessageBox.Show(Rekod.Properties.Resources.PTC_ErrorMaxMin,
                                Rekod.Properties.Resources.DGBH_ErrorHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        String sql = String.Format(@"
UPDATE {0}.table_field_info SET is_style = false WHERE id_table = {1};
UPDATE {0}.table_info SET geom_field = '{2}', default_style = true, range_colors=true WHERE id = {1};",
    Program.scheme, id, cbGeomField.Text);

                        using (SqlWork sqlCmd = new SqlWork())
                        {
                            sqlCmd.sql = sql;
                            sqlCmd.Execute(true);
                        }

                        setStyleSetting(4);
                        classesOfMetods.reloadLayer(id);
                        //MessageBox.Show("Операция прошла успешно!");
                        parent.CloseElemsAfter(this, true);
                        //Close();
                    }
                    else
                    {
                        MessageBox.Show(Rekod.Properties.Resources.PFC_AllFieldsFill);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.PTC_ErrorNotSelectGeom);
                return;
            }
            
            DBTablesEdit.SyncController.ReloadStyle(id);

            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        private void setStyleSetting(int nRadio)
        {
            switch (nRadio)
            {
                case 1:
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = string.Format(@"
                                UPDATE 
                                    {0}.table_field_info 
                                SET 
                                    is_reference = false
                                    , is_interval = false
                                    , is_style = false 
                                WHERE 
                                    is_style = true 
                                    AND id_table = {1}"
                            ,
                            Program.scheme,
                            id);
                        sqlCmd.ExecuteNonQuery();
                    }
                    break;
                case 2:
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = string.Format(@"
                                    UPDATE 
                                        {0}.table_field_info 
                                    SET 
                                        is_reference = true
                                        , is_interval = false
                                        , is_style = true
                                        , ref_table = {1}
                                        , ref_field = {2}
                                        , ref_field_name = {3}
                                    WHERE 
                                        id = {4}"
                                ,
                                Program.scheme,
                                ((itemObj)cbRefTable.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbRefLink.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbRefFieldName.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbRefStyleField.SelectedItem).Id_o.ToString());
                        sqlCmd.ExecuteNonQuery();
                    }
                    break;
                case 3:
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = string.Format(@"
                                    UPDATE 
                                        {0}.table_field_info 
                                    SET 
                                        is_reference = false
                                        , is_interval = true
                                        , is_style = true
                                        , ref_table = {1}
                                        , ref_field = {2}
                                        , ref_field_end = {3}
                                        , ref_field_name = {4}
                                    WHERE 
                                        id = {5}"
                                ,
                                Program.scheme,
                                ((itemObj)cbIntervalTable.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbIntervalStart.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbIntervalEnd.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbIntervalName.SelectedItem).Id_o.ToString(),
                                ((itemObj)cbIntervalStyleField.SelectedItem).Id_o.ToString());
                        sqlCmd.Execute(true);
                    }
                    break;
                case 4:
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = string.Format(@"
                                    UPDATE 
                                        {0}.table_field_info 
                                    SET 
                                        is_reference = false
                                        , is_interval = false
                                        , is_style = false 
                                    WHERE 
                                        is_style = true 
                                        AND id_table = {1}"
                                ,
                                Program.scheme,
                                id);
                        sqlCmd.ExecuteNonQuery();
                    }
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = string.Format(@"
                                    UPDATE 
                                        {0}.table_info 
                                    SET 
                                        range_column = '{1}'
                                        , precision_point = {2}
                                        , type_color = {3}
                                        , min_color = {4}
                                        , min_val = {5}
                                        , max_color = {6}
                                        , max_val = {7}
                                        , use_min_val = {8}
                                        , use_max_val = {9}
                                        , null_color = 0
                                        , use_null_color = false 
                                    WHERE 
                                        id = {10}"
                                    ,
                                Program.scheme,
                                ((itemObj)cbRangeValueField.SelectedItem).Layer,
                                numericUpDownRangePrec.Value,
                                (cbRangeColorChange.SelectedItem as itemObj).Id_o,
                                convToUInt(panelRangeMinColor.BackColor),
                                numericUpDownRangeMinValue.Value,
                                convToUInt(panelRangeMaxColor.BackColor),
                                numericUpDownRangeMaxValue.Value,
                                checkBoxRangeMin.Checked,
                                checkBoxRangeMax.Checked,
                                id);
                        sqlCmd.Execute(true);
                    }
                    break;
                default:
                    break;
            }
            if (nRadio == 1)
            {

            }
            else
            {
                if (nRadio == 2)
                {

                }
                else
                {
                    if (nRadio == 3)
                    {

                    }
                    else
                    {
                        if (nRadio == 4)
                        {

                        }
                        else
                        {
                            MessageBox.Show(Rekod.Properties.Resources.PTC_NotRight);
                        }
                    }
                }
            }
        }
        private bool getstyleExists()
        {
            bool exists_style = false;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT * FROM " + Program.scheme + "." + tableName + " LIMIT 1";
            sqlCmd.Execute(false);
            for (int i = 0; sqlCmd.GetFiealdCount() > i; i++)
            {
                if (sqlCmd.GetFieldName(i) == "exists_style")
                {
                    exists_style = true;
                    break;
                }
            }
            sqlCmd.Close();
            return exists_style;
        }
        private void cbRefTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbRefTable.SelectedItem != null)
            {
                cbRefLink.Items.Clear();
                cbRefFieldName.Items.Clear();
                cbFileNameColumn.Items.Clear();

                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = "SELECT id, name_db, name_map, type_field FROM " + Program.scheme + ".table_field_info WHERE id_table = " + ((itemObj)cbRefTable.SelectedItem).Id_o.ToString();
                    sqlCmd.Execute(false);

                    while (sqlCmd.CanRead())
                    {
                        int type_field = sqlCmd.GetInt32("type_field");
                        itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));

                        if (type_field == 1 || type_field == 6)
                        {
                            cbRefLink.Items.Add(item);
                        }
                        else if (type_field == 2)
                        {
                            cbRefFieldName.Items.Add(item);
                            cbFileNameColumn.Items.Add(item);
                        }
                    }
                }
            }
        }
        private void cbIntervalTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbIntervalTable.SelectedItem != null)
            {
                cbIntervalStart.Items.Clear();
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)cbIntervalTable.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    cbIntervalStart.Items.Add(item);
                }
                sqlCmd.Close();

                cbIntervalEnd.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + ((itemObj)cbIntervalTable.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    cbIntervalEnd.Items.Add(item);
                }
                sqlCmd.Close();

                cbIntervalName.Items.Clear();
                sqlCmd = new SqlWork();
                sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE type_field = 2 AND id_table = " + ((itemObj)cbIntervalTable.SelectedItem).Id_o.ToString();
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(1), sqlCmd.GetValue<string>(2));
                    cbIntervalName.Items.Add(item);
                }
                sqlCmd.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            parent.CloseElemsAfter(this, true);
            //Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            parent.CloseElemsAfter(this, false);
            tablesInfo tInfo = classesOfMetods.getTableInfo(id);
            Rekod.DBTablesEdit.GeomType gType = DBTablesEdit.GeomType.point;
            if (tInfo.GeomType_GC.ToUpper().Contains("LINESTRING")) gType = DBTablesEdit.GeomType.line;
            else if (tInfo.GeomType_GC.ToUpper().Contains("POLYGON")) gType = DBTablesEdit.GeomType.polygon;

            Rekod.DBTablesEdit.StyleControl cs = new Rekod.DBTablesEdit.StyleControl(parent, this, gType, DBTablesEdit.SyncController.HasRight(id));
            cs.setStyles(setStyleFromDB(id));
            parent.AddNewElemModal(cs, Rekod.Properties.Resources.PTC_Styles);
        }
        public void saveStyle(Rekod.DBTablesEdit.StyleControl sc)
        {
            objStyleBrush brush1 = sc.createBrush();
            objStyleFont font1 = sc.createFont();
            objStylePen pen1 = sc.createPen();
            objStyleSymbol symb1 = sc.createSymbol();
            string bgColor = "";
            if (sc.checkBox1.Enabled && !sc.checkBox1.Checked)
            {
                bgColor = "4294967295";
            }
            else
            {
                bgColor = brush1.bgColor.ToString();
            }
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET fontname = '" + font1.FontName + "', " +
                "fontcolor = " + font1.Color.ToString() + ", " +
                "fontframecolor = " + font1.FrameColor.ToString() + ", " +
                "fontsize = " + font1.Size.ToString() + ", " +
                "symbol = " + symb1.Shape.ToString() + ", " +
                "pencolor = " + pen1.Color.ToString() + ", " +
                "pentype = " + pen1.Type.ToString() + ", " +
                "penwidth = " + pen1.Width.ToString() + ", " +
                "brushbgcolor = " + bgColor + ", " +
                "brushfgcolor = " + brush1.fgColor.ToString() + ", " +
                "brushstyle = " + brush1.Style.ToString() + ", " +
                "brushhatch = " + brush1.Hatch.ToString() + " WHERE id = " + id.ToString();
            sqlCmd.Execute(true);
            sqlCmd.Close();
            sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET default_style = true WHERE id = " + id.ToString();
            sqlCmd.Execute(true);
            sqlCmd.Close();

            DBTablesEdit.SyncController.ReloadStyle(id);
        }
        private axVisUtils.Styles.objStylesM setStyleFromDB(int idT)
        {
            axVisUtils.Styles.objStylesM s1 = new axVisUtils.Styles.objStylesM();

            string sql = "SELECT fontname, fontcolor, fontframecolor, fontsize,";
            sql += " symbol,";
            sql += " pencolor, pentype, penwidth,";
            sql += " brushbgcolor, brushfgcolor, brushstyle, brushhatch";
            sql += " FROM " + Program.scheme + ".table_info WHERE id=" + id.ToString();
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
        private void cbStyleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetControlVisibility();
        }
        private void loadColumsForDiapazon(int id_table)
        {
            cbRangeValueField.Items.Clear();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id, name_db, name_map FROM " + Program.scheme + ".table_field_info WHERE (type_field = 1 OR type_field = 6) AND id_table = " + id_table.ToString();
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObj item = new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetValue<string>(2), sqlCmd.GetValue<string>(1));
                cbRangeValueField.Items.Add(item);
            }
            sqlCmd.Close();
        }
        private void loadDaipazonInfo(int id_table)
        {
            loadColumsForDiapazon(id_table);
            SqlWork sqlCmd = new SqlWork();
            typeColors = new List<itemObj>();
            typeColors.Add(new itemObj(0, Rekod.Properties.Resources.PTC_BasicElement, ""));
            typeColors.Add(new itemObj(1, Rekod.Properties.Resources.PTC_Background, ""));
            typeColors.Add(new itemObj(2, Rekod.Properties.Resources.PTC_BordersAndLines, ""));
            typeColors.Add(new itemObj(3, Rekod.Properties.Resources.PTC_BorderSymbol, ""));
            sqlCmd.sql = "SELECT range_colors, range_column, precision_point, type_color, min_color, " +
                                "min_val, max_color, max_val, use_min_val, use_max_val, null_color, " +
                                "use_null_color, geom_type FROM " + Program.scheme + ".table_info WHERE id = " + id_table;
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                switch (sqlCmd.GetInt32(12))
                {
                    case 0://Без геометрии
                        Classes.workLogFile.writeLogFile(new Exception("Загружена таблица без геометрии"), true, false);
                        break;
                    case 1://Точки
                        typeColors.RemoveAt(2);
                        typeColors.RemoveAt(1);
                        break;
                    case 2://Линии
                        typeColors.RemoveAt(3);
                        typeColors.RemoveAt(1);
                        typeColors.RemoveAt(0);
                        break;
                    case 3://площадные
                        typeColors.RemoveAt(3);
                        break;
                }
                cbRangeColorChange.DataSource = typeColors;
                setComboBox(cbRangeValueField, sqlCmd.GetValue<string>(1));
                setComboBox(cbRangeColorChange, sqlCmd.GetInt32(3).ToString(), sqlCmd.GetInt32(3));

                numericUpDownRangePrec.Value = sqlCmd.GetInt32(2);
                panelRangeMinColor.BackColor = convColor(sqlCmd.GetValue<uint>(4));
                numericUpDownRangeMinValue.Value = sqlCmd.GetInt32(5);
                panelRangeMaxColor.BackColor = convColor(sqlCmd.GetValue<uint>(6));
                numericUpDownRangeMaxValue.Value = sqlCmd.GetInt32(7);
                checkBoxRangeMin.Checked = sqlCmd.GetBoolean(8);
                checkBoxRangeMax.Checked = sqlCmd.GetBoolean(9);
            }
            sqlCmd.Close();
            numericUpDownRangeMinValue.Enabled = checkBoxRangeMin.Checked;
            numericUpDownRangeMaxValue.Enabled = checkBoxRangeMax.Checked;
        }
        private void setComboBox(ComboBox cb, string name, int id = -1)
        {
            for (int i = 0; cb.Items.Count > i; i++)
            {
                if (id != -1)
                {
                    if (((itemObj)cb.Items[i]).Id_o == id)
                    {
                        cb.SelectedIndex = i;
                        break;
                    }
                }
                else if (((itemObj)cb.Items[i]).Layer == name)
                {
                    cb.SelectedIndex = i;
                    break;
                }
            }
        }
        private void panel4_MouseClick(object sender, MouseEventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ((Panel)sender).BackColor = colorDialog1.Color;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownRangeMinValue.Enabled = checkBoxRangeMin.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownRangeMaxValue.Enabled = checkBoxRangeMax.Checked;
        }

        public static Color convColor(uint value)
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb(r1, g1, b1);
            return c1;
        }
        public static uint convToUInt(Color clr)
        {
            uint temp = 0;
            temp = Convert.ToUInt32(clr.R + (clr.G << 8) + (clr.B << 16));
            return temp;
        }

        private void cbRangeValueField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbRangeValueField.SelectedItem != null)
            {
                if (classesOfMetods.getFieldInfo(((itemObj)cbRangeValueField.SelectedItem).Id_o).type == 1)
                {
                    numericUpDownRangePrec.Value = 0;
                    numericUpDownRangePrec.Enabled = false;
                }
                else
                {
                    numericUpDownRangePrec.Enabled = true;
                }
            }
        }

        private void cbRefType_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelPic.Visible = cbRefType.SelectedIndex == 1;
        }
    }
}
