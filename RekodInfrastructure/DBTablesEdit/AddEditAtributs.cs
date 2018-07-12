using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using Npgsql;
using Rekod.Services;
using Rekod.DBTablesEdit;

namespace Rekod
{
    public partial class AddEditAtributs : UserControl
    {
        public NpgsqlDataReader zxReader;
        private bool upd;
        private int id, idT;
        private TextBoxInfo tb, tb2;//sasha
        private DBTablesGroups parent;
        private FiledEditListControl field_edit;
        Dictionary<char, string> dic;
        public AddEditAtributs(bool upd1, int id1, int idT1, DBTablesGroups par, FiledEditListControl f_edit)
        {
            InitializeComponent();
            upd = upd1;
            id = id1;
            idT = idT1;
            loadTipField();
            loadInfo();
            parent = par;
            field_edit = f_edit;
            if (!upd)
            {
                tb = new TextBoxInfo(textBox1);
                textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
                textBox1.MouseUp += new MouseEventHandler(textBox1_MouseUp);
                textBox1.KeyUp += new KeyEventHandler(textBox1_KeyUp);
            }
            tb2 = new TextBoxInfo(this.textBox2);
            textBox2.TextChanged += new EventHandler(textBox2_TextChanged);
            textBox2.MouseUp += new MouseEventHandler(textBox2_MouseUp);
            textBox2.KeyUp += new KeyEventHandler(textBox2_KeyUp);
            var fi = classesOfMetods.getFieldInfo(id);
            var ti = classesOfMetods.getTableInfo(idT);
            if (!SyncController.HasRight(idT))
            {
                button2.Enabled = false;
            }

            if (fi.type == 5)
            {
                button2.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                checkBox1.Enabled = false;
                checkBoxHidden.Enabled = false;
            }
            if (ti.pkField == fi.nameDB)
            {
                checkBox1.Enabled = false;
                checkBoxHidden.Enabled = false;
            }
            dic = initalizeDictionary();
        }
        private void loadTipField()
        {
            comboBox1.Items.Clear();
            if (Program.tip_data.Count > 0)
            {
                for (int i = 0; Program.tip_data.Count > i; i++)
                {
                    if (Program.tip_data[i].idTipData == 5 && !upd) 
                        continue; // пользовательский атрибут типа геометрия создать невозможно
                    itemObj item = new itemObj(Program.tip_data[i].idTipData,
                        Program.tip_data[i].nameTipData,
                        Program.tip_data[i].nameTipDataDB);
                    comboBox1.Items.Add(item);
                }
            }
        }
        private void loadInfo()
        {
            if (upd)
            {
                fieldInfo fInfo = classesOfMetods.getFieldInfo(id);
                textBox1.Text = fInfo.nameDB;
                textBox2.Text = fInfo.nameMap;
                textBox3.Text = fInfo.nameLable;
                setComboBox(comboBox1, fInfo.type);
                textBox1.Enabled = false;
                comboBox1.Enabled = false;
                checkBox1.Checked = fInfo.read_only;
                checkBoxHidden.Checked = !fInfo.visible;
            }
            else
            {
                textBox1.Enabled = false;
                CheckBox edit = new CheckBox();
                edit.AutoSize = false;
                edit.Size = new Size(20, 20);
                edit.Appearance = Appearance.Button;
                edit.FlatStyle = FlatStyle.Flat;
                edit.FlatAppearance.BorderColor = Color.Silver;
                edit.Image = Rekod.Properties.Resources._1279535667_edit;
                edit.Left = textBox1.Right - edit.Width;
                edit.Top = textBox1.Top;
                edit.Click += new EventHandler(edit_click);
                edit.Tag = textBox1;
                this.Controls.Add(edit);
                edit.BringToFront();

                textBox3.Enabled = false;
                CheckBox edit2 = new CheckBox();
                edit2.AutoSize = false;
                edit2.Size = new Size(20, 20);
                edit2.Appearance = Appearance.Button;
                edit2.FlatStyle = FlatStyle.Flat;
                edit2.FlatAppearance.BorderColor = Color.Silver;
                edit2.Image = Rekod.Properties.Resources._1279535667_edit;
                edit2.Left = textBox3.Right - edit2.Width;
                edit2.Top = textBox3.Top;
                edit2.Click += new EventHandler(edit_click);
                edit2.Tag = textBox3;
                this.Controls.Add(edit2);
                edit2.BringToFront();
            }

        }
        private void edit_click(object sender, EventArgs e)
        {
            CheckBox edit = (sender as CheckBox);
            if ((bool)edit.Checked)
            {
                (edit.Tag as TextBox).Enabled = true;
            }
            else
            {
                (edit.Tag as TextBox).Enabled = false;
                (edit.Tag as TextBox).Text = translite(textBox2.Text.ToLower());
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
        private void button1_Click(object sender, EventArgs e)
        {
            parent.CloseModalElem();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text.Trim() != "" && textBox1.Text.IndexOf(" ") < 0 && textBox3.Text.Trim() != "")
            {
                if (comboBox1.SelectedItem != null)
                {
                    var ti = classesOfMetods.getTableInfo(this.idT);
                    if (!upd)
                    {
                        try
                        {

                            bool existField = false;
                            string nameField = textBox1.Text;
                            int countt = 0;
                            var fieldNames = new List<string>();

                            if (classesOfMetods.isReservedName(nameField))
                            {
                                MessageBox.Show(Rekod.Properties.Resources.AEAtr_ReservedName+
                                    "\n"+Rekod.Properties.Resources.AEAtr_NotSaveField+
                                    "\n" + Rekod.Properties.Resources.AEAtr_RenameInDB, 
                                    Rekod.Properties.Resources.AEAtr_CreatingAttribute, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            using (SqlWork sqlCmd = new SqlWork())
                            {
                                sqlCmd.sql = String.Format("SELECT column_name FROM information_schema.columns WHERE table_schema = '{0}' AND table_name = '{1}';",
                                    ti.nameSheme, ti.nameDB);
                                sqlCmd.ExecuteReader();
                                while (sqlCmd.CanRead())
                                {
                                    fieldNames.Add(sqlCmd.GetString(0));
                                }
                                sqlCmd.Close();//считали все поля таблицы с базы и занеесли в лист
                                while (fieldNames.Contains(nameField))
                                {
                                    existField = true;
                                    countt++;
                                    nameField = textBox1.Text + countt.ToString();//если такое имя уже существует пробуем другое имя
                                }
                                if (existField)
                                {
                                    DialogResult dr = MessageBox.Show(String.Format(Rekod.Properties.Resources.AEAtr_SaveRenameField, nameField),
                                        Rekod.Properties.Resources.AEAtr_CreatingAttribute, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                    if (dr == DialogResult.Cancel)
                                    {
                                        MessageBox.Show(Rekod.Properties.Resources.AEAtr_NotSaveField+
                                            "\n"+ Rekod.Properties.Resources.AEAtr_RenameInDB,
                                            Rekod.Properties.Resources.AEAtr_CreatingAttribute, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }
                                }
                            }

                            int? id_f = null;
                            using (SqlWork sqlCmd = new SqlWork())
                            {
                                sqlCmd.sql = "SELECT " + Program.scheme + ".create_field(" + idT.ToString() + ",@field_name_db,@field_name_map," + ((itemObj)comboBox1.SelectedItem).Id_o.ToString() + ",@field_name_lable)";
                                var parms = new Params[]
                                            {
                                                new Params
                                                    {
                                                        paramName = "@field_name_db",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = nameField
                                                    },
                                                new Params
                                                    {
                                                        paramName = "@field_name_map",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = textBox2.Text
                                                    },
                                                new Params
                                                    {
                                                        paramName = "@field_name_lable",
                                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                                        value = textBox3.Text
                                                    }
                                            };

                                sqlCmd.ExecuteReader(parms);
                                if (sqlCmd.CanRead())
                                {
                                    id_f = sqlCmd.GetInt32(0);
                                }
                            }
                            if (id_f != null)
                            {
                                using (SqlWork sqlCmd = new SqlWork())
                                {
                                    sqlCmd.sql = "UPDATE " + Program.scheme + ".table_field_info " +
                                                 "SET read_only = " + checkBox1.Checked.ToString() + ", visible = " +
                                                 !checkBoxHidden.Checked +
                                                 " WHERE id = " + id_f.ToString();
                                    sqlCmd.Execute(false);
                                }
                            }

                            var cls = new classesOfMetods();
                            cls.reloadInfo();
                            field_edit.loadFields(idT);
                            parent.CloseModalElem();
                        }
                        catch (Exception ex)
                        {
                            Rekod.Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.AEAtr_ErrorCreateAtrField + " "+
                                Rekod.Properties.Resources.DGBH_ErrorHeader+": " + ex.Message, true, true);
                        }
                    }
                    else
                    {
                        if (checkBoxHidden.Checked)
                        {
                            SqlWork sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT COUNT(*) FROM " + Program.scheme + ".table_field_info WHERE id_table=" + idT +
                                " AND visible=true";
                            if (sqlCmd.Execute(false))
                                if (sqlCmd.CanRead())
                                    if (sqlCmd.GetInt64(0) == 1)
                                    {
                                        MessageBox.Show(Rekod.Properties.Resources.AEAtr_ErrorAllFieldHiden, 
                                            Rekod.Properties.Resources.AEAtr_TableAttributesHeader, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                                        checkBoxHidden.Checked = false;
                                    }
                            sqlCmd.Close();
                        }
                        SqlWork sqlCmd2 = new SqlWork();
                        sqlCmd2.sql = "UPDATE " + Program.scheme + ".table_field_info " +
                            "SET name_map =@field_name_map, name_lable =@field_name_lable, read_only = @read_only, visible = @visible WHERE id = " + id.ToString();

                        Params[] parms = new Params[4];
                        parms[0] = new Params();
                        parms[0].paramName = "@field_name_map";
                        parms[0].typeData = NpgsqlTypes.NpgsqlDbType.Text;
                        parms[0].value = textBox2.Text;

                        parms[1] = new Params();
                        parms[1].paramName = "@field_name_lable";
                        parms[1].typeData = NpgsqlTypes.NpgsqlDbType.Text;
                        parms[1].value = textBox3.Text;

                        parms[2] = new Params();
                        parms[2].paramName = "@read_only";
                        parms[2].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[2].value = checkBox1.Checked;

                        parms[3] = new Params();
                        parms[3].paramName = "@visible";
                        parms[3].typeData = NpgsqlTypes.NpgsqlDbType.Boolean;
                        parms[3].value = !checkBoxHidden.Checked;

                        sqlCmd2.ExecuteNonQuery(parms);
                        sqlCmd2.Close();

                        classesOfMetods cls = new classesOfMetods();
                        cls.reloadInfo();
                        field_edit.loadFields(idT);
                        parent.CloseModalElem();
                    }

                    DBTablesEdit.SyncController.ReloadTable(idT);
                }
                else
                {
                    MessageBox.Show(Rekod.Properties.Resources.AEAtr_NotSelectType, Properties.Resources.AEAtr_CreatingAttribute, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.AEAtr_NotPutName, Rekod.Properties.Resources.Mes_Error, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                else ok = false;
            }
            if (!ok)
            {
                tb.undo();
                int pp = (sender as TextBox).Text.Length;
                toolTip1.Show(Rekod.Properties.Resources.AEAtr_NameFormat, this,
                        (sender as TextBox).Left + 20 + pp * 6, (sender as TextBox).Top + 30, 2000);
            }
            else
            {
                if (!tb.shanged)
                    tb.saveText();
                else tb.shanged = false;
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

            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                if (!tb2.shanged)
                    tb2.saveText();
                else
                    tb2.shanged = false;
            }
            if (textBox1.Enabled == false && !upd)
            {
                textBox1.Text = translite(textBox2.Text.ToLower());
            }
            if (textBox3.Enabled == false && !upd)
            {
                textBox3.Text = textBox2.Text;
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
                else ret += "_";
            }
            return ret;
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
    }
}