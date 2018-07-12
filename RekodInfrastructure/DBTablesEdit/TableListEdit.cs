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
    public partial class TableEdit : UserControl
    {
        public NpgsqlConnection conn;
        public NpgsqlDataReader zxReader;
        public int idT;
        private int type; //varyag
        public bool prev_selected = false;
        DBTablesGroups parent;
        private ActionWithTables act_tables;

        //private ActionWithTables awt;

        public TableEdit(NpgsqlConnection conn1, DBTablesGroups par)
        {
            InitializeComponent();
            conn = conn1;

            loadTableTypes();
            parent = par;
            if (cbTableType.Items.Count > 0)
            {
                cbTableType.SelectedIndex = 0;
            }
        }
        private void loadTableTypes()
        {
            cbTableType.Items.Clear();
            if (Program.tip_table.Count > 0)
            {
                for (int i = 0; Program.tip_table.Count > i; i++)
                {
                    itemObj item = new itemObj(Program.tip_table[i].idTipTable,
                        Program.tip_table[i].nameTip,
                        Program.tip_table[i].mapLayer.ToString());
                    cbTableType.Items.Add(item);
                }
            }
        }

        public void loadTables()
        {
            listBox1.Items.Clear();
            if (cbTableType.SelectedItem != null)
            {
                int tableTypeId = ((itemObj)cbTableType.SelectedItem).Id_o;
                List<tablesInfo> tInfo = classesOfMetods.getTableOfType(tableTypeId);
                tInfo.Sort(new DinoComparer());
                if (tInfo.Count > 0)
                {
                    for (int i = 0; tInfo.Count > i; i++)
                    {
                        if (tInfo[i].nameMap.ToLower().IndexOf(textBox1.Text.ToLower()) > -1)
                        {
                            itemObj item = new itemObj(tInfo[i].idTable,
                                tInfo[i].nameMap,
                                tInfo[i].nameDB);
                            listBox1.Items.Add(item);
                        }
                    }
                }
                listBox1_SelectedIndexChanged(listBox1, null);
            }
            label1.Text = changeTextOfCount(listBox1.Items.Count);
        }
        public void reloadTables()
        {
            classesOfMetods cls = new classesOfMetods();
            cls.reloadInfo();
            loadTables();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (cbTableType.SelectedItem != null)
            {
                type = ((itemObj)cbTableType.SelectedItem).Id_o;
            }
            else
            {
                type = 1;
            }
            /*EditTable frm = new EditTable(conn, false, 0, type); // Varyag
            frm.Text = "Создание таблицы";
            frm.ShowDialog();*/
            prev_selected = false;
            parent.CloseElemsAfter(this, false);
            AddEditTable aet = new AddEditTable(false, 0, type, parent, this);
            parent.AddNewElem(aet, Rekod.Properties.Resources.AET_CreatingTable);
        }
        public void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                /*fieldEditFrm frm = new fieldEditFrm(conn, idT);
                frm.ShowDialog();*/
                if (act_tables != null)
                    parent.CloseElemsAfter(act_tables, false);
                else parent.CloseElemsAfter(this, false);
                FiledEditListControl felc = new FiledEditListControl(idT, parent, this);
                parent.AddNewElem(felc, Rekod.Properties.Resources.TLE_TableStructure);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {

                int idTable = -1;
                int typetable = -1;
                foreach (tablesInfo ti in Program.tables_info)
                {
                    if (ti.idTable == ((itemObj)listBox1.SelectedItem).Id_o)
                    {
                        idTable = ti.idTable;
                        typetable = ti.type;
                        break;
                    }
                }
                List<itemObj> list_tables = new List<itemObj>();
                if (typetable == 3) // если интервал
                {
                    SqlWork sqlCmd = new SqlWork();
                    sqlCmd.sql = "SELECT id_table,name_db FROM " + Program.scheme + ".table_field_info WHERE ref_table=" + idTable;
                    sqlCmd.Execute(false);
                    while (sqlCmd.CanRead())
                    {
                        list_tables.Add(new itemObj(sqlCmd.GetInt32(0), sqlCmd.GetString(1), sqlCmd.GetString(1)));
                    }
                    sqlCmd.Close();
                }

                DialogResult dr = DialogResult.Cancel;
                switch (typetable)
                {
                    case 1:
                        dr = QuestionDelete.ShowDialog(Rekod.Properties.Resources.TLE_DeletingLayer,
                            Rekod.Properties.Resources.TLE_DeletingLayerAsk, 
                            Rekod.Properties.Resources.FELC_Forever);
                        break;
                    case 2:
                        dr = QuestionDelete.ShowDialog(Rekod.Properties.Resources.TLE_DeletingCatalog,
                            Rekod.Properties.Resources.TLE_Attention+"\n"+Rekod.Properties.Resources.TLE_DeletingCatalogAsk,
                            Rekod.Properties.Resources.FELC_Forever);
                        break;
                    case 3:
                        dr = QuestionDelete.ShowDialog(Rekod.Properties.Resources.TLE_DeletingInterval,
                            Rekod.Properties.Resources.TLE_Attention + "\n" + Rekod.Properties.Resources.TLE_DeletingIntervalAsk,
                            Rekod.Properties.Resources.FELC_Forever, SystemIcons.Warning);
                        break;
                    case 4:
                        dr = QuestionDelete.ShowDialog(Rekod.Properties.Resources.TLE_DeletingTable,
                            Rekod.Properties.Resources.TLE_Attention + "\n"+Rekod.Properties.Resources.TLE_DeletingTableAsk,
                            Rekod.Properties.Resources.FELC_Forever);
                        break;
                }
                if (dr != DialogResult.Cancel)
                {
                    SqlWork sqlCmd = new SqlWork();
                    foreach (itemObj io in list_tables)
                    {
                        sqlCmd = new SqlWork();
                        sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET default_style=true,style_field='style' WHERE id=" + io.Id_o + " AND style_field='" + io.Name_o + "'";
                        sqlCmd.Execute(true);
                        sqlCmd.Close();
                    }
                    // удалить все ссылки на связанные таблицы
                    sqlCmd = new SqlWork();
                    sqlCmd.sql = String.Format("UPDATE {0}.table_field_info SET ref_table=null, ref_field=null, ref_field_name=null, is_reference=false, is_interval = FALSE WHERE ref_table={1};", Program.scheme, idTable);
                    sqlCmd.Execute(true);
                    sqlCmd.Close();
                    sqlCmd = new SqlWork();
                    sqlCmd.sql = "SELECT " + Program.scheme + ".delete_table(" + ((itemObj)listBox1.SelectedItem).Id_o + "," + (dr == DialogResult.Yes).ToString() + ")";
                    sqlCmd.Execute(true);
                    sqlCmd.Close();
                    classesOfMetods.DeleteLayerInMap(((itemObj)listBox1.SelectedItem).Id_o);
                    try
                    {
                        SyncController.DeleteTable(((itemObj)listBox1.SelectedItem).Id_o, false);
                    }
                    catch(Exception ex)
                    {
                        Classes.workLogFile.writeLogFile(ex, false, true);
                    }
                    classesOfMetods cls = new classesOfMetods();
                    
                    cls.reloadInfo();
                    loadTables();

                    MessageBox.Show(((dr == DialogResult.Yes) ? Rekod.Properties.Resources.TLE_DeleteForever : Rekod.Properties.Resources.TLE_DeleteNoForever));
                    Control[] cntrs = parent.Controls.Find("CloseButton", true);
                    if (cntrs.Length == 1)
                    {
                        ((CloseButton)cntrs[0]).CloseBox(e);
                    }
                }
            }
        }
        public void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                if (act_tables != null)
                    parent.CloseElemsAfter(act_tables, false);
                else parent.CloseElemsAfter(this, false);
                propertyTableControl ptc = new propertyTableControl(((itemObj)listBox1.SelectedItem).Id_o, parent, this);
                parent.AddNewElem(ptc, String.Format(Rekod.Properties.Resources.TLE_SettingTable, ((itemObj)listBox1.SelectedItem).Name_o));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                idT = ((itemObj)listBox1.SelectedItem).Id_o;
                if (!prev_selected)
                {
                    prev_selected = true;
                    parent.CloseElemsAfter(this, false);
                    ActionWithTables awt = new ActionWithTables(parent, this);
                    parent.AddNewElemAfter(awt, this, Rekod.Properties.Resources.TLE_ActionToTable);
                    act_tables = awt;
                }
                act_tables.isLayerMap(cbTableType.SelectedIndex == 0);
                parent.CloseElemsAfter(act_tables, false);
            }
            else
            {
                prev_selected = false;
                parent.CloseElemsAfter(this, false);
            }
        }

        public void showGroups()
        {
            if (listBox1.SelectedItem != null)
            {
                //propertyTable frm = new propertyTable(conn, ((itemObj)listBox1.SelectedItem).Id_o);
                //frm.ShowDialog();
                if (act_tables != null)
                    parent.CloseElemsAfter(act_tables, false);
                else parent.CloseElemsAfter(this, false);
                TableGroup tg = new TableGroup(((itemObj)listBox1.SelectedItem).Id_o, parent, this);
                parent.AddNewElem(tg, string.Format(Rekod.Properties.Resources.TLE_EntryTableInGroup, ((itemObj)listBox1.SelectedItem).Name_o));
            }
        }

        public void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            /*type = cbTableType.SelectedIndex;
            EditTable frm = new EditTable(conn, true, idT, type); // Varyag
            frm.ShowDialog();
            loadTables();*/
            if (listBox1.SelectedItem != null)
            {
                if (act_tables != null)
                    parent.CloseElemsAfter(act_tables, false);
                else parent.CloseElemsAfter(this, false);
                AddEditTable aet = new AddEditTable(true, idT, type, parent, this);
                parent.AddNewElem(aet, Rekod.Properties.Resources.TLE_EditingTable+":\"" + listBox1.SelectedItem.ToString() + "\"");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTableType.SelectedItem != null)
            {
                //button4.Visible = ((itemObj)cbTableType.SelectedItem).Layer == "True";
                this.loadTables();
            }
        }

        private string changeTextOfCount(int l)
        {
            string sss = "";
            //if (l == 1)
            //    sss = "таблица";
            //else if (l > 1 && l < 5)
            //    sss = "таблицы";
            //else
            //    sss = "таблиц";
            //sss = l.ToString() + " " + sss;
            sss =Rekod.Properties.Resources.PanelGroup_tables + ":" + l.ToString();
            return sss;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            loadTables();
        }

        private void importBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Rekod.Properties.Resources.LocAllSupportedFormats + "|*.shp;*.tab;*.mif;*.xls;*.xlsx;*.geojson;*.sqlite;*.dbf|" +
                 @"ESRI Shape-files|*.shp|
MapInfo-files|*.tab|
MapInfo MIF/MID|*.mif|
MS Excel|*.xls;*.xlsx|
GeoJSON|*.geojson|
SQLite|*.sqlite|
File dBase|*.dbf";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Rekod.DataAccess.SourcePostgres.View.ImportTableV importTableFrm = new DataAccess.SourcePostgres.View.ImportTableV();
                var importTableVM = new DataAccess.SourcePostgres.ViewModel.PgImportTableVM(new System.IO.FileInfo(ofd.FileName), importTableFrm);
                if (importTableVM.DataTable != null)
                {
                    importTableFrm.DataContext = importTableVM;
                    importTableFrm.ShowDialog();
                    reloadTables();
                }
            }
        }
    }
}