using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using Rekod.Services;
using Rekod.DBTablesEdit;

namespace Rekod
{
    public partial class FiledEditListControl : UserControl
    {
        public DataSet ds = new DataSet();
        //public NpgsqlDataAdapter data_adpa = new NpgsqlDataAdapter();

        private int id, idF;
        public int TableId { get { return id; } }

        private bool hasRightToChange = true;
        private DBTablesGroups parent;
        private TableEdit table_edit;
        public FiledEditListControl(int id1, DBTablesGroups par, TableEdit table_edit_frm)
        {
            InitializeComponent();
            id = id1;
            loadFields(id);
            button3.Enabled = false;
            parent = par;
            table_edit = table_edit_frm;

            hasRightToChange = DBTablesEdit.SyncController.HasRight(id);
            if (!hasRightToChange)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }


        public void save_data()
        {
            for (int k = 0; k < listBox1.Items.Count; k++)
            {
                itemObjOrdered obj = listBox1.Items[k] as itemObjOrdered;
                if (obj.order != k)
                {//сохраняем
                    SqlWork sqlCmd = new SqlWork();
                    sqlCmd.sql = "UPDATE " + Program.scheme + ".table_field_info SET num_order=" + k.ToString() + " WHERE id=" + obj.Id_o;
                    sqlCmd.Execute(true);
                    sqlCmd.Close();
                }
            }
        }
        private string select_name_db()
        {
            string table_db = "";
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT name_db FROM " + Program.scheme + ".table_info WHERE id = " + id.ToString();
            sqlCmd.Execute(false);
            if (sqlCmd.CanRead())
            {
                table_db = sqlCmd.GetValue<string>(0);
            }
            sqlCmd.Close();
            return table_db;
        }


        public void loadFields(int idT)
        {
            listBox1.Items.Clear();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id,name_db,type_field,name_map,num_order FROM " + Program.scheme + ".table_field_info WHERE id_table=" + idT.ToString() + " ORDER BY num_order";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObjOrdered item = new itemObjOrdered(sqlCmd.GetInt32(0),
                    sqlCmd.GetString(1) + " (" + classesOfMetods.getTipField(sqlCmd.GetInt32(2)).nameTipData + ") - " +
                    sqlCmd.GetString(3), sqlCmd.GetString(1), sqlCmd.GetInt32(4));
                listBox1.Items.Add(item);
            }
            sqlCmd.Close();
            label2.Text = changeTextOfCount(listBox1.Items.Count);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            parent.CloseElemsAfter(this, false);
            AddEditAtributs aea = new AddEditAtributs(false, 0, id, parent, this);
            parent.AddNewElemModal(aea, Rekod.Properties.Resources.FELC_CreatingFields);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string dependantTables = "";
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT DISTINCT ti.name_map as name FROM {0}.table_field_info tfi, {0}.table_info ti WHERE (ref_field = {1} OR ref_field_end = {1} OR ref_field_name = {1}) AND ti.id = tfi.id_table;", Program.scheme, idF);
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                        dependantTables += sqlCmd.GetString("name") + Environment.NewLine;
                }

                if (dependantTables != "")
                {
                    MessageBox.Show(String.Format(@Rekod.Properties.Resources.FELC_CannotDeleteField, dependantTables.Remove(dependantTables.Length - 2)),
                        Rekod.Properties.Resources.FELC_DeletingFields, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    var fi = classesOfMetods.getFieldInfo(idF);
                    var ti = classesOfMetods.getTableInfo(fi.idTable);
                    bool styleField = !ti.Style.DefaultStyle && ti.style_field == fi.nameDB || ti.Style.DefaultStyle && ti.Style.Range.RangeColumn == fi.nameDB;
                    bool labelField = ti.label_showlabel && ti.lableFieldName.Contains("((" + fi.nameDB + ")::text)");
                    DialogResult dr = QuestionDelete.ShowDialog(
                        Rekod.Properties.Resources.FELC_DeletingFields,
                        (styleField ? Rekod.Properties.Resources.FELC_DeletingFieldStyle + Environment.NewLine : "") +
                        (labelField ? Rekod.Properties.Resources.FELC_DeletingFieldLabel + Environment.NewLine : "") +
                        Rekod.Properties.Resources.FELC_DeletingFieldAsk,
                        Rekod.Properties.Resources.FELC_Forever);
                    if (dr != DialogResult.Cancel)
                    {
                        List<string> dep_table_list = new List<string>();
                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql= String.Format("SELECT sys_scheme.get_dependent_by_field('{0}', '{1}') as table_name;", ti.nameDB, fi.nameDB);
                        sqlCmd.ExecuteReader();
                        while (sqlCmd.CanRead())
                        {
                            String table_name = sqlCmd.GetValue<string>("table_name");
                            if (table_name != ti.view_name)
                            {
                                dep_table_list.Add(table_name);
                                if (dep_table_list.Count >= 7)
                                {
                                    dep_table_list.Add("...");
                                    break;
                                }
                            }
                        }
                        sqlCmd.Close();

                        if (dep_table_list.Count > 0)
                        {
                            MessageBox.Show("Безвозвратное удаление атрибута невозможно! Имеются следующие зависимые объекты:"+Environment.NewLine+
                                        String.Join(","+Environment.NewLine,dep_table_list.ToArray()), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }

                        sqlCmd = new SqlWork();
                        sqlCmd.sql = "SELECT " + Program.scheme + ".delete_field(" + ((itemObjOrdered)listBox1.SelectedItem).Id_o.ToString() + "," + (dr == DialogResult.Yes).ToString() + ")";
                        sqlCmd.Execute(true);
                        sqlCmd.Close();

                        classesOfMetods.reloadLayer(ti.idTable);
                        loadFields(id);

                        DBTablesEdit.SyncController.ReloadTable(ti.idTable);
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                idF = ((itemObjOrdered)listBox1.SelectedItem).Id_o;
                var fi = classesOfMetods.getFieldInfo(idF);
                var ti = classesOfMetods.getTableInfo(id);

                // кнопка связи со справочником
                if ((fi.type == 1 || fi.type == 6) && fi.nameDB != ti.pkField)
                {
                    button3.Enabled = true;
                }
                else
                {
                    button3.Enabled = false;
                }

                // кнопка удаления атрибута
                if (fi.nameDB == ti.pkField || fi.nameDB == ti.geomFieldName)
                {
                    button2.Enabled = false;
                }
                else
                {
                    button2.Enabled = hasRightToChange;
                }

                // кнопки перемещения атрибута в списке
                int i = listBox1.SelectedIndex;
                button4.Enabled = (i > 0) && hasRightToChange;
                button5.Enabled = (i != -1 && i != listBox1.Items.Count - 1)
                    && hasRightToChange;
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                //EditField frm = new EditField(conn, true, ((itemObjOrdered)listBox1.SelectedItem).Id_o, id);
                //frm.ShowDialog();
                //loadFields(id);
                parent.CloseElemsAfter(this, false);
                AddEditAtributs aea = new AddEditAtributs(true, ((itemObjOrdered)listBox1.SelectedItem).Id_o, id, parent, this);
                parent.AddNewElemModal(aea, Rekod.Properties.Resources.FELC_EditingField);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                parent.CloseElemsAfter(this, false);
                PropertyFieldControl pfc = new PropertyFieldControl(((itemObjOrdered)listBox1.SelectedItem).Id_o, parent, this);
                parent.AddNewElemModal(pfc, Rekod.Properties.Resources.FELC_UseCatalogs);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {//наверх
            if (listBox1.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                int min = listBox1.Items.Count;
                foreach (itemObjOrdered sel in listBox1.SelectedItems)
                {
                    if (listBox1.Items.IndexOf(sel) < min)
                        min = listBox1.Items.IndexOf(sel);

                    lllist.Add(sel);
                }
                if (min > 0)
                {
                    listBox1.ClearSelected();
                    foreach (itemObjOrdered sel in lllist)
                    {
                        int k = listBox1.Items.IndexOf(sel);
                        listBox1.Items.Remove(sel);
                        listBox1.Items.Insert(k - 1, sel);
                        listBox1.SelectedItems.Add(sel);
                    }
                }
            }
            //Editing_sort(1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                int max = 0;
                foreach (itemObjOrdered sel in listBox1.SelectedItems)
                {
                    if (listBox1.Items.IndexOf(sel) > max)
                        max = listBox1.Items.IndexOf(sel);

                    lllist.Add(sel);
                }
                if (max < listBox1.Items.Count - 1)
                {
                    listBox1.ClearSelected();
                    for (int i = lllist.Count - 1; i >= 0; i--)
                    {
                        itemObjOrdered sel = lllist[i];
                        int k = listBox1.Items.IndexOf(sel);
                        listBox1.Items.Remove(sel);
                        listBox1.Items.Insert(k + 1, sel);
                        listBox1.SelectedItems.Add(sel);
                    }
                }
            }
            // Editing_sort(2);
        }


        private void selectObj(int id, ListBox lb)
        {
            for (int i = 0; lb.Items.Count > i; i++)
            {
                if (((itemObjOrdered)lb.Items[i]).Id_o == id)
                {
                    lb.SelectedIndex = i;
                    break;
                }
            }
        }

        private void FiledEditListControl_Load(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        public void FiledEditListControl_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (SyncController.HasRight(id))
            {
                save_data();
            }
            classesOfMetods cl = new classesOfMetods();
            cl.reloadInfo();
        }
        private string changeTextOfCount(int l)
        {
            string sss = "";
            //if (l == 1)
            //    sss = "атрибут";
            //else if (l > 1 && l < 5)
            //    sss = "атрибута";
            //else
            //    sss = "атрибутов";
            //sss = l.ToString() + " " + sss;

            sss = Rekod.Properties.Resources.FELC_Fields+":" + l.ToString();
            return sss;
        }
    }
}
