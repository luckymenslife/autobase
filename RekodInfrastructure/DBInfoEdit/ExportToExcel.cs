using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod.DBInfoEdit
{
    public partial class ExportToExcel : Form
    {
        private tablesInfo _tableInfo;
        public string _sqlCmd = "";
        public string _select = "";
        private List<fieldInfo> _fieldInfoList = new List<fieldInfo>();
        List<string> _listParams;

        public ExportToExcel(tablesInfo ti, List<string> listParams)
        {
            InitializeComponent();
            _tableInfo = ti;
            _listParams = listParams;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton3.Checked = false;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = false;
            radioButton2.Checked = false;
        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
        }

        private void ExportToExcel_Load(object sender, EventArgs e)
        {
            _fieldInfoList = Program.field_info.FindAll(w => (w.idTable == _tableInfo.idTable) && (w.type != 5) &&  (w.visible == true));

            foreach (var fi in _fieldInfoList)
            {
                if (fi.is_reference || fi.is_interval)
                    checkedListBox1.Items.Add(fi, true);
            }

            if (checkedListBox1.Items.Count > 0)
                panel2.Height = (int)(checkedListBox1.GetItemHeight(0) * (checkedListBox1.Items.Count + 1));

            checkedListBox1.DisplayMember = "nameMap";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sqlCmd = "SELECT ";
            _select = " ";
	        string select_id_string = ", ";
            string from_string = String.Format(" FROM {0}.{1} ", _tableInfo.nameSheme, _tableInfo.nameDB);
            int index_tab_alias = 1;
            foreach (var fi in _fieldInfoList)
            {
                bool forFilter = _listParams.Contains(fi.nameDB);
                bool original = radioButton3.Checked && !checkedListBox1.CheckedItems.Contains(fi)
                    || radioButton1.Checked && !fi.is_interval && !fi.is_reference
                    || radioButton2.Checked;
                if ( original && !forFilter)
                {
                    // взять оригинальные поля из БД
                    if (fi.type == 4)
                        _sqlCmd += String.Format("{0}.{1}.{2}::timestamp without time zone as {2}, ", _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB);
                    else
                        _sqlCmd += String.Format("{0}.{1}.{2} as \"{2}\", ", _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB);
                    _select += "\"" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";
                }
                else
                {
                    if (fi.is_interval || fi.is_reference)
                    {
                        // поля выгружаются из справочника или интервала
                        string ref_field_name_name = Program.field_info.Find(w => w.idField == fi.ref_field_name).nameDB;
                        string ref_field_name = Program.field_info.Find(w => w.idField == fi.ref_field).nameDB;
                        string ref_field_end_name = "";
                        if (fi.is_interval)
                            ref_field_end_name = Program.field_info.Find(w => w.idField == fi.ref_field_end).nameDB;
                        string ref_scheme_name = Program.tables_info.Find(w => w.idTable == fi.ref_table).nameSheme;
                        string ref_table_name = Program.tables_info.Find(w => w.idTable == fi.ref_table).nameDB;
                        string ref_table_pkfield = Program.tables_info.Find(w => w.idTable == fi.ref_table).pkField;

                        if (fi.is_interval)
                        {
                            select_id_string += "\"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\".\"" + fi.nameDB + "\" as \"id!" + fi.nameDB + "\", ";
                            if (original)
                                _select += "\"id!" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";
                            else
                                _select += "\"" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";

                            string alias_string = "___v$" + index_tab_alias++;
                            string alias_string2 = "___v$" + index_tab_alias++;

                            _sqlCmd += String.Format("COALESCE({0}.{1}||'('||{2}.{3}.{4}||')', {2}.{3}.{4}::text) as {4}, ",
                                alias_string, ref_field_name_name, _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB);

                            from_string += String.Format("LEFT JOIN {0}.{1} {2}" +
                                " ON {2}.{7} = (SELECT {3}.{7} FROM {0}.{1} {3} " +
                                "WHERE {4}.{5}.{6} > {3}.{9} AND {4}.{5}.{6}<={3}.{8} LIMIT 1) ",
                                ref_scheme_name, ref_table_name, alias_string, alias_string2, _tableInfo.nameSheme,
                                _tableInfo.nameDB, fi.nameDB, ref_table_pkfield, ref_field_end_name, ref_field_name);
                        }

                        else if (fi.is_reference)
                        {
                            select_id_string += "\"" + _tableInfo.nameSheme + "\".\"" + _tableInfo.nameDB + "\".\"" + fi.nameDB + "\" as \"id!" + fi.nameDB + "\", ";
                            if (original)
                                _select += "\"id!" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";
                            else
                                _select += "\"" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";

                            string alias_string = "___v$" + index_tab_alias++;
                            _sqlCmd += String.Format("{0}.{1} as {2}, ", alias_string, ref_field_name_name, fi.nameDB);
                            from_string += String.Format("LEFT JOIN {0}.{1} {2}" +
                                " ON {3}.{4}.{5}={2}.{6} ",
                                ref_scheme_name, ref_table_name, alias_string, _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB, ref_field_name);
                        }
                    }
                    else
                    {
                        // взять оригинальные поля из БД
                        if (fi.type == 4)
                            _sqlCmd += String.Format("{0}.{1}.{2}::timestamp without time zone as {2}, ", _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB);
                        else
                            _sqlCmd += String.Format("{0}.{1}.{2} as \"{2}\", ", _tableInfo.nameSheme, _tableInfo.nameDB, fi.nameDB);
                        _select += "\"" + fi.nameDB + "\" as \"" + fi.nameDB + "\", ";
                    }
                }
            }
            if (select_id_string.Length >= 2)
                _sqlCmd = _sqlCmd.Substring(0, _sqlCmd.Length - 2);
            select_id_string = select_id_string.Substring(0, select_id_string.Length - 2);
            _sqlCmd += select_id_string + " " + from_string;
            if (_select.Length >= 2)
                _select = _select.Substring(0, _select.Length - 2);

            this.Close();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Visible = radioButton3.Checked;
        }

        private void panel2_VisibleChanged(object sender, EventArgs e)
        {
            if (panel2.Visible)
            {
                button1.Location = new Point(button1.Location.X, button1.Location.Y + 3 + panel2.Height);
                button2.Location = new Point(button2.Location.X, button2.Location.Y + 3 + panel2.Height);
            }
            else
            {
                button1.Location = new Point(button1.Location.X, button1.Location.Y - 3 - panel2.Height);
                button2.Location = new Point(button2.Location.X, button2.Location.Y - 3 - panel2.Height);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
