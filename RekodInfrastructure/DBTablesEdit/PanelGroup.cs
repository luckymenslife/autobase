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
    
    /// <summary>
    /// Этот контрол предназначен для работы с группами
    /// </summary>
    //[Rekod]
    public partial class PanelGroup : UserControl
    {
        private GroupFunct gf;
        private DBTablesGroups parent;
        List<itemObjOrdered> listt;
        public PanelGroup(DBTablesGroups par)
        {
            InitializeComponent();
            gf = new GroupFunct();
            parent = par;
            listBox1.SelectedIndexChanged += (o, e) =>
            {
                int i = listBox1.SelectedIndex;
                button1.Enabled = (i != -1);
                button5.Enabled = (i != -1 && i != listBox1.Items.Count - 1);
                button6.Enabled = (i > 0);
            };
            listBox2.SelectedIndexChanged += (o, e) =>
            {
                button2.Enabled = (listBox2.SelectedIndex != -1);
            };
            reloadGroups();
        }

        private void PanelGroup_Load(object sender, EventArgs e)
        {
            reloadGroups();
        }
        public void AddNewGroup(string name_gr,string desc_gr)
        {
            gf.AddNewGroup(name_gr, desc_gr);
            reloadGroups();
        }

        public void reloadGroups()
        {
            itemObjOrdered selectedItem = comboBox1.SelectedItem as itemObjOrdered; 
            
            comboBox1.Items.Clear();
            List<itemObjOrdered> list_groups = gf.LoadGroups();
            itemObjOrdered[] arr_group = new itemObjOrdered[list_groups.Count];
            list_groups.CopyTo(arr_group);
            comboBox1.Items.AddRange(arr_group);
            label8.Text = changeTextOfCount2(comboBox1.Items.Count);

            if (selectedItem != null)
            {
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    if ((comboBox1.Items[i] as itemObjOrdered).Id_o == selectedItem.Id_o)
                    {
                        comboBox1.SelectedItem = (comboBox1.Items[i] as itemObjOrdered); 
                        break; 
                    }
                }
            }
            if (comboBox1.SelectedItem == null)
            {
                panel1.Visible = false; 
            }
        }
        public void DeleteGroup(int id)
        {
            gf.DeleteGroup(id);
            reloadGroups();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //listOfGroups.Items.Clear();
            //List<itemObjOrdered> list_groups = gf.GetListTablesInOrOutGroup(0, false);
            //itemObjOrdered[] arr_group = new itemObjOrdered[list_groups.Count];
            //list_groups.CopyTo(arr_group);
            //listOfGroups.Items.AddRange(arr_group);
            ////gf.MoveTableToGroup(1, 45);
            //gf.DeleteTableFromGroup(10, 10);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                DialogResult dr = MessageBox.Show(String.Format(Rekod.Properties.Resources.EGO_DeleteGroup, (comboBox1.SelectedItem as itemObj).Name_o),
                    Rekod.Properties.Resources.EGO_DeletingGroup, MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    gf.DeleteGroup((comboBox1.SelectedItem as itemObj).Id_o);
                    reloadGroups();
                }
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            //parent.CloseElemsAfter(this, false);
            //AddEditGroup aeg = new AddEditGroup(-1, parent, this);
            //parent.AddNewElemModal(aeg, "");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                panel1.Visible = true;
                listBox1.Items.Clear();
                listt = gf.GetListTablesInOrOutGroup((comboBox1.SelectedItem as itemObj).Id_o, true);
                itemObjOrdered[] it = new itemObjOrdered[listt.Count];
                listt.CopyTo(it);
                listBox1.Items.AddRange(it);

                listBox2.Items.Clear();
                List<itemObjOrdered> listt2 = gf.GetListTablesInOrOutGroup((comboBox1.SelectedItem as itemObj).Id_o, false);
                itemObjOrdered[] it2 = new itemObjOrdered[listt2.Count];
                listt2.CopyTo(it2);
                listBox2.Items.AddRange(it2);

                itemObj gr = gf.GetGroup((comboBox1.SelectedItem as itemObj).Id_o);
                textBox2.Text = gr.Layer;

                label6.Text = changeTextOfCount(listBox1.Items.Count);
                label7.Text = changeTextOfCount(listBox2.Items.Count);
            }
            else
            {
                panel1.Visible = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<itemObjOrdered> listt2 = new List<itemObjOrdered>(listt);
            gf.SaveGroup((comboBox1.SelectedItem as itemObj).Id_o, (comboBox1.SelectedItem as itemObj).Name_o, textBox2.Text);
            foreach (itemObjOrdered it in listt)
            {
                bool del = true;
                foreach (itemObjOrdered it2 in listBox1.Items)
                    if (it.Id_o == it2.Id_o)
                    {
                        del = false;
                        break;
                    }
                if (del)
                {
                    gf.DeleteTableFromGroup((comboBox1.SelectedItem as itemObj).Id_o, it.Id_o);
                }
            }

            foreach (itemObjOrdered it in listBox1.Items)
            {
                bool add = true;
                foreach (itemObjOrdered it2 in listt)
                    if (it.Id_o == it2.Id_o)
                    {
                        add = false;
                        break;
                    }
                if (add)
                {
                    gf.MoveTableToGroup((comboBox1.SelectedItem as itemObj).Id_o, it.Id_o);
                }
            }
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if ((listBox1.Items[i] as itemObjOrdered).order != i)
                {
                    (listBox1.Items[i] as itemObjOrdered).order = i;
                    gf.SetOrderTableInGroup((comboBox1.SelectedItem as itemObj).Id_o, (listBox1.Items[i] as itemObjOrdered).Id_o, (listBox1.Items[i] as itemObjOrdered).order);

                }
            }
            comboBox1_SelectedIndexChanged(this, null);

            var cls = new classesOfMetods();
            Program.tablegroups_info = cls.loadTablesGroupsInfo();
            Program.mainFrm1.layerItemsView1.RefreshLayers();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                listBox2.SelectedIndices.Clear();
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                foreach (itemObjOrdered sel in listBox1.SelectedItems)
                {
                    lllist.Add(sel);
                }
                foreach (itemObjOrdered sel in lllist)
                {
                    listBox1.Items.Remove(sel);
                    listBox2.Items.Add(sel);
                    listBox2.SelectedIndices.Add(listBox2.Items.IndexOf(sel));
                }
                label6.Text = changeTextOfCount(listBox1.Items.Count);
                label7.Text = changeTextOfCount(listBox2.Items.Count);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count > 0)
            {
                listBox1.SelectedIndices.Clear();
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                foreach (itemObjOrdered sel in listBox2.SelectedItems)
                {
                    lllist.Add(sel);
                }
                foreach (itemObjOrdered sel in lllist)
                {
                    listBox2.Items.Remove(sel);
                    listBox1.Items.Add(sel);
                    listBox1.SelectedIndices.Add(listBox1.Items.IndexOf(sel));
                }
                label6.Text = changeTextOfCount(listBox1.Items.Count);
                label7.Text = changeTextOfCount(listBox2.Items.Count);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(this, null);
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
            sss = Rekod.Properties.Resources.PanelGroup_tables+":" + l.ToString();
            return sss;
        }
        private string changeTextOfCount2(int l)
        {
            string sss = "";
            //if (l == 1)
            //    sss = "группа";
            //else if (l > 1 && l < 5)
            //    sss = "группы";
            //else
            //    sss = "групп";
            //sss = l.ToString() + " " + sss;
            sss = Rekod.Properties.Resources.EGO_groups + ":" + l.ToString();
            return sss;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                int min=listBox1.Items.Count;
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
                if (max < listBox1.Items.Count-1)
                {
                    listBox1.ClearSelected();
                    for (int i = lllist.Count-1; i >= 0; i--)
                    {
                        itemObjOrdered sel = lllist[i];
                        int k = listBox1.Items.IndexOf(sel);
                        listBox1.Items.Remove(sel);
                        listBox1.Items.Insert(k + 1, sel);
                        listBox1.SelectedItems.Add(sel);
                    }
                }
            }
        }

        object _prevSelected = null; 
        private void listBox2_MouseMove(object sender, MouseEventArgs e)
        {
            int topIndex = listBox2.TopIndex;
            int itemHeight = listBox2.ItemHeight;
            int addItem = e.Y / itemHeight;
            if (topIndex + addItem < listBox2.Items.Count)
            {
                object item = listBox2.Items[topIndex + addItem];
                if (item == _prevSelected) return;
                else _prevSelected = item; 

                Graphics g = Graphics.FromHwnd(this.Handle);
                int strWidth = (int)(g.MeasureString(item.ToString(), listBox2.Font).Width);
                if (strWidth > listBox2.Width)
                {
                    int x = 0;
                    int y = addItem * itemHeight;
                    try
                    {
                        toolTip1.Show(item.ToString(), listBox2, x, y, 1100);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    toolTip1.Hide(listBox2); 
                }
            }
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int topIndex = listBox1.TopIndex;
            int itemHeight = listBox1.ItemHeight;
            int addItem = e.Y / itemHeight;
            if (topIndex + addItem < listBox1.Items.Count)
            {
                int ind = topIndex + addItem;
                if (ind < 0) return;
                object item = listBox1.Items[ind];
                if (item == _prevSelected) return;
                else _prevSelected = item; 

                Graphics g = Graphics.FromHwnd(this.Handle);
                int strWidth = (int)(g.MeasureString(item.ToString(), listBox1.Font).Width);
                if (strWidth > listBox1.Width)
                {
                    int x = 0; 
                    int y = addItem * itemHeight;
                    try
                    {
                        toolTip1.Show(item.ToString(), listBox1, x, y, 1100);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    toolTip1.Hide(listBox1); 
                }
            }
        }



        private void groupsOrderButton_Click(object sender, EventArgs e)
        {
            parent.CloseElemsAfter(this, false);
            //AddEditGroup aeg = new AddEditGroup(-1, parent, this);
            EditGroupsOrder ego = new EditGroupsOrder(parent, this);
            parent.AddNewElemModal(ego, "");
        }
    }
}