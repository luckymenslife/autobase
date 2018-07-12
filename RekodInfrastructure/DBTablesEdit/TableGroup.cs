using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace Rekod
{
    public partial class TableGroup : UserControl
    {
        private int id;
        private GroupFunct gf;
        private DBTablesGroups parent;
        List<itemObj> listt;
        private TableEdit table_edit;
        public TableGroup(int id_table,DBTablesGroups par, TableEdit t_edit)
        {
            InitializeComponent();
            id = id_table;
            parent = par;
            table_edit = t_edit;
            gf = new GroupFunct();
            listBox1.SelectedIndexChanged += (s, e) =>
            {
                button1.Enabled = (listBox1.SelectedIndex != -1);
            };
            listBox2.SelectedIndexChanged += (s, e) =>
            {
                button2.Enabled = (listBox2.SelectedIndex != -1);
            };
            reload();
        }
        private void reload()
        {
            listBox1.Items.Clear();
            listt = gf.GetGroupsWithTable(id,true);//((comboBox1.SelectedItem as itemObj).Id_o, true);
            itemObj[] it = new itemObj[listt.Count];
            listt.CopyTo(it);
            listBox1.Items.AddRange(it);

            listBox2.Items.Clear();
            List<itemObj> listt2 = gf.GetGroupsWithTable(id, false);
            itemObj[] it2 = new itemObj[listt2.Count];
            listt2.CopyTo(it2);
            listBox2.Items.AddRange(it2);

            label1.Text = changeTextOfCount(listBox1.Items.Count);
            label2.Text = changeTextOfCount(listBox2.Items.Count);
        }

        private string changeTextOfCount(int l)
        {
            string sss = "";
            //if (l == 1) 
            //    sss = "группа";
            //else if(l>1 && l<5)
            //    sss = "группы";
            //else 
            //    sss = "групп";
            //sss = l.ToString()+" "+sss;
            sss = Rekod.Properties.Resources.EGO_groups + ":" + l.ToString();
            return sss;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                listBox2.SelectedIndices.Clear();
                List<itemObj> lllist = new List<itemObj>();
                foreach (itemObj sel in listBox1.SelectedItems)
                {
                    //itemObj sel = listBox2.SelectedItem as itemObj;
                    lllist.Add(sel);
                }
                foreach (itemObj sel in lllist)
                {
                    listBox1.Items.Remove(sel);
                    listBox2.Items.Add(sel);
                    listBox2.SelectedIndices.Add(listBox2.Items.IndexOf(sel));
                }
                label1.Text = changeTextOfCount(listBox1.Items.Count);
                label2.Text = changeTextOfCount(listBox2.Items.Count);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (listBox2.SelectedItems.Count > 0)
            {
                listBox1.SelectedIndices.Clear();
                List<itemObj> lllist=new List<itemObj>();
                foreach (itemObj sel in listBox2.SelectedItems)
                {
                    //itemObj sel = listBox2.SelectedItem as itemObj;
                    lllist.Add(sel);
                }
                foreach (itemObj sel in lllist)
                {
                    listBox2.Items.Remove(sel);
                    listBox1.Items.Add(sel);
                    listBox1.SelectedIndices.Add(listBox1.Items.IndexOf(sel));
                }
                label1.Text = changeTextOfCount(listBox1.Items.Count);
                label2.Text = changeTextOfCount(listBox2.Items.Count);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<itemObj> listt2 = new List<itemObj>(listt);
            foreach (itemObj it in listt)
            {
                bool del = true;
                foreach (itemObj it2 in listBox1.Items)
                    if (it.Id_o == it2.Id_o)
                    {
                        del = false;
                        break;
                    }
                if (del)
                {
                    gf.DeleteTableFromGroup(it.Id_o,id);
                }
            }

            foreach (itemObj it in listBox1.Items)
            {
                bool add = true;
                foreach (itemObj it2 in listt)
                    if (it.Id_o == it2.Id_o)
                    {
                        add = false;
                        break;
                    }
                if (add)
                {
                    gf.MoveTableToGroup(it.Id_o, id);
                }
            }
            if (parent != null)
                parent.CloseElemsAfter(this, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            reload();
        }
        
    }
}
