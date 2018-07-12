using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using Npgsql;
using NpgsqlTypes;
using mvMapLib;
using Rekod.Services;

namespace Rekod.DBTablesEdit
{
    public partial class LabelControl : UserControl
    {
        private readonly DBTablesGroups _parent;
        private readonly int _idT;
        public LabelStyle lFont;
        private bool userStyle = false;
        private string _rezult;

        public LabelControl(DBTablesGroups par, int idTable)
        {
            _parent = par;
            _idT = idTable;
            InitializeComponent();

            if (!SyncController.HasRight(idTable))
            {
                button2.Enabled = false;
            }
        }
        public LabelControl(int idTable)
        {
            _parent = null;
            _idT = idTable;
            InitializeComponent();

            if (!SyncController.HasRight(idTable))
            {
                button2.Enabled = false;
            }
        }
        public bool UserStyle
        {
            get
            {
                return userStyle;
            }
            set
            {
                if (value)
                {
                    button2.Visible = !value;
                    button3.Visible = !value;
                    userStyle = value;
                }
            }
        }
        private void SignControl_Load(object sender, EventArgs e)
        {
            SqlWork sqlCmd = new SqlWork();
            if (!Program.WorkSets.CurrentWorkSet.IsDefault)
                sqlCmd.sql = "SELECT lablefiled FROM sys_scheme.table_info_sets WHERE id_table=" + _idT.ToString() + " AND id_set = " + Program.WorkSets.CurrentWorkSet.Id;
            else
                sqlCmd.sql = "SELECT lablefiled FROM sys_scheme.table_info WHERE id=" + _idT.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                String form = getFromBase(sqlCmd.GetValue<string>(0));
                textBox1.Text = form;
                textBox1.SelectionStart = form.Length;
                textBox1.Focus();
            }
            sqlCmd.Close();

            List<fieldInfo> afi = classesOfMetods.getFieldInfoTable(_idT);
            foreach (fieldInfo fi in afi)
            {
                comboBox1.Items.Add(fi.nameDB);
                comboBox_2.Items.Add(fi.nameDB);
            }

            if (classesOfMetods.getTableInfo(_idT).label_showlabel)
            {
                panel1.Enabled = true;
                button4.Enabled = true;
                check_showLabel.Checked = true;
            }
            else
            {
                check_showLabel.Checked = false;
                panel1.Enabled = false;
                button4.Enabled = false;
            }
        }

        private String getFromBase(String text)
        {
            if (text == null)
            {
                text = ""; 
            }
            SqlParser parser = new SqlParser();
            String result = parser.GoToStr(text);
            //String[] parts = text.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < parts.Length; i++)
            //{
            //    parts[i] = parts[i].Replace("::text", "");
            //    while (parts[i].Length > 1 && (parts[i][0] == '(' && parts[i][parts[i].Length - 1] == ')'))
            //    {
            //        parts[i] = parts[i].Substring(1, parts[i].Length - 2);
            //    }
            //    if (parts[i].Length > 1 && parts[i][0] == '\'' && parts[i][parts[i].Length - 1] == '\'')
            //    {
            //        parts[i] = parts[i].Substring(1, parts[i].Length - 2);
            //        parts[i] = "[" + parts[i] + "]";
            //    }

            //    parts[i] = parts[i].Replace("''", "'");
            //    parts[i] = "{" + parts[i] + "}";
            //}

            //foreach (String s in parts)
            //{
            //    if (result != "")
            //        result += "+" + s;
            //    else result += s;
            //}

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Test");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_parent != null)
                _parent.CloseElemsAfter(this, true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String result = parseResult(textBox1.Text);

            if (result.Replace(" ", "") == "" && check_showLabel.Checked)
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_NotEmpty, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (result.Contains("SELECT") || result.Contains("DROP") || result.Contains("INSERT") || result.Contains("UPDATE") || result.Contains("DELETE"))
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SqlWork sqlCmd = new SqlWork(true);
            if (result.Replace(" ", "") != "")
            {
                tablesInfo ti = classesOfMetods.getTableInfo(_idT);

                sqlCmd.sql = string.Format("SELECT {0} FROM \"{1}\".\"{2}\" LIMIT 1", result, ti.nameSheme, ti.nameDB);
                try
                {
                    sqlCmd.ExecuteNonQuery(null);
                }
                catch
                {
                    MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    sqlCmd.Close();
                }
            }
            _rezult = result;
            if (!userStyle)
            {
                Params par = new Params();
                par.paramName = "@labelexpr";
                par.typeData = NpgsqlDbType.Varchar;
                par.value = (!String.IsNullOrEmpty(result.Replace(" ", ""))) ? result : null;
                sqlCmd = new SqlWork();

                if (!Program.WorkSets.CurrentWorkSet.IsDefault)
                    sqlCmd.sql = String.Format("UPDATE sys_scheme.table_info_sets SET lablefiled=@labelexpr, label_showlabel={1} WHERE id_table={0} AND id_set = {2}",
                        _idT, check_showLabel.Checked, Program.WorkSets.CurrentWorkSet.Id);
                else
                    sqlCmd.sql = String.Format("UPDATE sys_scheme.table_info SET lablefiled=@labelexpr, label_showlabel={1} WHERE id={0}", _idT, check_showLabel.Checked);

                sqlCmd.ExecuteNonQuery(new IParams[] { par });
                sqlCmd.Close();

                sqlCmd = new SqlWork();
                sqlCmd.sql = String.Format("SELECT sys_scheme.create_view_for_table({0})", _idT);
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Close();

                classesOfMetods.reloadLayer(_idT);
                SyncController.ReloadStyle(_idT);
            }
            if (_parent != null)
                _parent.CloseElemsAfter(this, true);
            Program.mainFrm1.axMapLIb1.mapRepaint();
        }

        public string LableFieldRezult
        {
            get
            {
                String result = parseResult(textBox1.Text);

                if (result.Replace(" ", "") == "" && check_showLabel.Checked)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LabelControl_NotEmpty, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                if (result.Contains("SELECT") || result.Contains("DROP") || result.Contains("INSERT") || result.Contains("UPDATE") || result.Contains("DELETE"))
                {
                    MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                SqlWork sqlCmd = new SqlWork(true);
                if (result.Replace(" ", "") != "")
                {
                    tablesInfo ti = classesOfMetods.getTableInfo(_idT);

                    sqlCmd.sql = string.Format("SELECT {0} FROM \"{1}\".\"{2}\" LIMIT 1", result, ti.nameSheme, ti.nameDB);
                    try
                    {
                        sqlCmd.ExecuteNonQuery(null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    finally
                    {
                        sqlCmd.Close();
                    }
                }
                _rezult = result;
                return _rezult;
            }
        }

        private String parseResult(String text)
        {
            int state = 0;
            // state = 0 ->  ?
            // state = 1 -> '{' 
            
            SqlParser parser = new SqlParser();
            String result = parser.GoToSql(text);

            //for (int i = 0; i < text.Length; i++)
            //{
            //    if (state == 0)
            //    {
            //        if (text[i] == '{') { state = 1; result += "(("; }
            //        else if (text[i] == '+') { result += "||"; }
            //        else { result += text[i]; }
            //    }
            //    else if (state == 1)
            //    {
            //        if (text[i] == '}')
            //        {
            //            state = 0;
            //            result += ")::text)";
            //        }
            //        else if (text[i] == '[' && text[i - 1] == '{' || text[i] == ']' && text[i + 1] == '}')
            //        {
            //            result += "'";
            //        }
            //        else if (text[i] == '\'')
            //        {
            //            result += "''";
            //        }
            //        else
            //        {
            //            result += text[i];
            //        }
            //    }
            //}
            return result;
        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            int splitPlace = textBox1.SelectionStart;
            String leftPart = textBox1.Text.Substring(0, splitPlace);            
            String rightPart = textBox1.Text.Substring(splitPlace);
            if (!String.IsNullOrEmpty(leftPart) && !leftPart.Trim().EndsWith("+"))
                leftPart += "+";
            if (!String.IsNullOrEmpty(rightPart) && !rightPart.Trim().StartsWith("+"))
                rightPart = "+" + rightPart;
            if (comboBox1.SelectedIndex != -1)
            {
                textBox1.Text = leftPart + "{" + comboBox1.Text + "}" + rightPart;
            }
            else
            {
                textBox1.Text = leftPart + "{[" + comboBox1.Text + "]}" + rightPart;
            }
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            textBox1.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int splitPlace = textBox1.SelectionStart;
            String leftPart = textBox1.Text.Substring(0, splitPlace);
            String rightPart = textBox1.Text.Substring(splitPlace);


            if (textBox2.Text.Replace(" ", "") == "")
            {
                MessageBox.Show(Properties.Resources.LabelControl_NotEmpty, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (textBox2.Text.Contains("SELECT") || textBox2.Text.Contains("DROP") || textBox2.Text.Contains("INSERT") || textBox2.Text.Contains("UPDATE") || textBox2.Text.Contains("DELETE"))
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tablesInfo ti = classesOfMetods.getTableInfo(_idT);
            SqlWork sqlCmd = new SqlWork(true);
            sqlCmd.sql = string.Format("SELECT {0} FROM \"{1}\".\"{2}\" LIMIT 1", textBox2.Text, ti.nameSheme, ti.nameDB);
            try
            {
                sqlCmd.ExecuteNonQuery(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                sqlCmd.Close();
            }

            textBox1.Text = leftPart + (((leftPart.Replace(" ", "") != "") ? "+" : "") + "{" + textBox2.Text + "}") + rightPart;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            textBox1.Focus();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            textBox2.Text += "+" + comboBox_2.Text;
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            textBox2.Text += "*" + comboBox_2.Text;
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            textBox2.Text += "/" + comboBox_2.Text;
        }
        private void вычестьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox2.Text += "-" + comboBox_2.Text;
        }
        private void splitButton1_Click(object sender, EventArgs e)
        {
            textBox2.Text += comboBox_2.Text;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            lFont = new LabelStyle(_idT);
            lFont.UserStyle = this.userStyle;
            lFont.ShowDialog();
        }

        private void button_preview_Click(object sender, EventArgs e)
        {
            String result = parseResult(textBox1.Text);

            if (result.Replace(" ", "") == "")
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_NotEmpty, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (result.Contains("SELECT") || result.Contains("DROP") || result.Contains("INSERT") || result.Contains("UPDATE") || result.Contains("DELETE"))
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tablesInfo ti = classesOfMetods.getTableInfo(_idT);
            SqlWork sqlCmd = new SqlWork(true);
            sqlCmd.sql = string.Format("SELECT {0} FROM \"{1}\".\"{2}\" LIMIT 1", result, ti.nameSheme, ti.nameDB);
            try
            {
                sqlCmd.ExecuteReader(null);
                if (sqlCmd.CanRead())
                {
                    MessageBox.Show(sqlCmd.GetValue<string>(0));
                }
                else
                {
                    MessageBox.Show(Rekod.Properties.Resources.LabelControl_ErrorShowResult, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Rekod.Properties.Resources.LabelControl_InvalidExpression, Properties.Resources.AWT_EditLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                sqlCmd.Close();
            }
        }

        private void check_showLabel_CheckedChanged(object sender, EventArgs e)
        {
            if (check_showLabel.Checked)
            {
                panel1.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                panel1.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void textBox1_MouseHover(object sender, EventArgs e)
        {
            String caption;
            if (textBox1.Text.Replace(" ", "") == "")
            {
                caption = Rekod.Properties.Resources.LabelControl_ShowResult;
            }
            else
            {
                caption = Rekod.Properties.Resources.LabelControl_Result+": " + textBox1.Text;
            }
            toolTip1.SetToolTip(textBox1, caption);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            HelpLableFrm frm = new HelpLableFrm(Application.StartupPath + "\\help_lable.mht");
            frm.Text = Rekod.Properties.Resources.LabelControl_Help;
            frm.Show();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show((Control)(sender), new Point(0, 23));
        }
        private void button7_Click(object sender, EventArgs e)
        {
            textBox2.Text += comboBox_2.Text;
        }
    }
    public class SqlParser
    {
        Dictionary<String, String> _terms = new Dictionary<string, string>();
        List<String> _texts = new List<string>();

        public SqlParser()
        {
            LoadDic();
        }

        private void LoadDic()
        {
            this._terms.Add("'", "''");
            this._terms.Add("[", "'");
            this._terms.Add("]", "'");
            this._terms.Add("{", "(");
            this._terms.Add("}", ")::text");
            this._terms.Add("+", "||");
        }

        public String GoToSql(String sql)
        {
            string result = "";
            for (int i = 0; i < sql.Length; i++)
            {
                if (this._terms.ContainsKey(sql[i].ToString()))
                {

                    result += this._terms[sql[i].ToString()];
                }
                else
                {
                    result += sql[i].ToString();
                }
            }
            return result;
        }

        public String GoToStr(string sql)
        {
            this._texts.Clear();
            sql = TextParsing(sql);
            sql = sql.Replace(this._terms["}"], "}");
            sql = sql.Replace(this._terms["+"], "+");
            sql = SqlBlockParsing(sql);

            for (int i = 0; i < this._texts.Count; i++)
            {
                sql = sql.Replace("$" + i.ToString() + "$", this._texts[i]);
            }
            return sql;
        }

        private string SqlBlockParsing(string sql)
        {
            List<SymbolInfo> sInfoList = new List<SymbolInfo>();
            for (int i = 0; i < sql.Length; i++)
            {
                if (sql[i] == '(')
                {
                    sInfoList.Add(new SymbolInfo() { IsOpen = true, Position = i, Symbol = sql[i].ToString() });
                    continue;
                }
                if (sql[i] == '}' || sql[i] == ')')
                {
                    sInfoList.Add(new SymbolInfo() { IsOpen = false, Position = i, Symbol = sql[i].ToString() });
                    continue;
                }
            }

            while (sInfoList.Count > 0)
            {
                int index_last_open_item = 0;
                for (int i = 0; i < sInfoList.Count; i++)
                {
                    if (sInfoList[i].IsOpen)
                    {
                        index_last_open_item = i;
                    }
                    else
                    {
                        if (sInfoList[i].Symbol == "}")
                        {
                            sql = sql.Remove(sInfoList[i - 1].Position, 1);
                            sql = sql.Insert(sInfoList[i - 1].Position, "{");
                            sInfoList.RemoveRange(i - 1, 2);
                            break;
                        }
                        else if (sInfoList[i].Symbol == ")")
                        {
                            sql = sql.Remove(sInfoList[i - 1].Position, 1);
                            sql = sql.Insert(sInfoList[i - 1].Position, "(");
                            sInfoList.RemoveRange(i - 1, 2);
                            break;
                        }
                    }
                }
            }
            return sql;
        }
        private string TextParsing(string sql)
        {
            string result = "";
            string text_item = "";
            bool in_text = false;
            for (int i = 0; i < sql.Length; i++)
            {
                if (sql[i] == '\'')
                {
                    if (!in_text)
                    {
                        result += "$" + this._texts.Count + "$";
                        in_text = true;
                        text_item = "[";
                        continue;
                    }
                    if (i + 1 < sql.Length)
                    {
                        if (sql[i + 1] == '\'')
                        {
                            text_item += "'";
                            i++;
                        }
                        else
                        {
                            text_item += "]";
                            in_text = false;
                            this._texts.Add(text_item);
                        }
                    }
                    else
                    {
                        text_item += "]";
                        in_text = false;
                        this._texts.Add(text_item);
                    }
                }
                else
                {
                    if (!in_text)
                    {
                        result += sql[i];
                    }
                    else
                    {
                        text_item += sql[i];
                    }
                }
            }
            return result;
        }
    }
    class SymbolInfo
    {
        public bool IsOpen
        {
            get;
            set;
        }

        public String Symbol
        {
            get;
            set;
        }
        public int Position
        {
            get;
            set;
        }
    }
}
