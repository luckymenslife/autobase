using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using Rekod.Services;

namespace Rekod
{
    
    /// <summary>
    /// Этот контрол предназначен для управления слоями
    /// </summary>
    //[Rekod]
    public partial class ManagementTables : UserControl
    {
        private DBTablesGroups parent;
        private List<itemColor> itemColors = new List<itemColor>();
        public ManagementTables(DBTablesGroups par)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            parent = par;

            reloadTables();
        }

        public void reloadTables()
        {

            listBox1.Items.Clear();
            listBox2.Items.Clear();

            List<itemObjOrdered> list_of_tables = new List<itemObjOrdered>();
            List<itemObjOrdered> list_of_tables_visible = new List<itemObjOrdered>();

            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id,name_map,order_num,default_visibl FROM " + Program.scheme + ".table_info WHERE type=1 ORDER BY order_num";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                itemObjOrdered item = new itemObjOrdered(sqlCmd.GetInt32(0), sqlCmd.GetString(1), sqlCmd.GetBoolean(3).ToString(), sqlCmd.GetInt32(2));
                list_of_tables.Add(item);
                if (item.Layer.Contains("True"))
                    list_of_tables_visible.Add(item);
            }
            sqlCmd.Close();

            listBox1.Items.AddRange(list_of_tables_visible.ToArray());
            listBox2.Items.AddRange(list_of_tables.ToArray());

            label6.Text = changeTextOfCount(listBox1.Items.Count);
            label7.Text = changeTextOfCount(listBox2.Items.Count);
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                itemObjOrdered item = (listBox2.Items[i] as itemObjOrdered);
                if (item.Layer.Contains("True") != listBox1.Items.Contains(item) || i!=item.order)
                {
                    saveTable(item.Id_o, listBox1.Items.Contains(item), i);
                }
            }
            reloadTables();
        }
        private void saveTable(int id, bool default_visibl, int order_num)
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET default_visibl=" + default_visibl.ToString()
                + ", order_num="+order_num.ToString()+" WHERE id="+id.ToString();
            sqlCmd.Execute(true);
            sqlCmd.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> list_items = new List<itemObjOrdered>();
                foreach (itemObjOrdered sel in listBox1.SelectedItems)
                {
                    list_items.Add(sel);
                }
                foreach (itemObjOrdered sel in list_items)
                    listBox1.Items.Remove(sel);

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
                    if (!listBox1.Items.Contains(sel))
                    {
                        listBox1.Items.Add(sel);
                        listBox1.SelectedIndices.Add(listBox1.Items.IndexOf(sel));
                    }else
                    {
                        bool b=false;
                        int cur_index=listBox1.Items.IndexOf(sel);
                        foreach (itemColor ic in itemColors)
                        {
                            if(ic.index==cur_index)
                            {
                                b=true;
                                ic.color=150;
                                break;
                            }
                        }
                        if(!b)
                            itemColors.Add(new itemColor(150,cur_index));

                    }
                }
                label6.Text = changeTextOfCount(listBox1.Items.Count);
                label7.Text = changeTextOfCount(listBox2.Items.Count);
                if (itemColors.Count > 0) timer1.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            reloadTables();
        }
        private string changeTextOfCount(int l)
        {
            string sss = "";
            if (l == 1)
                sss = "таблица";
            else if (l > 1 && l < 5)
                sss = "таблицы";
            else
                sss = "таблиц";
            sss = l.ToString() + " " + sss;
            return sss;
        }
        private string changeTextOfCount2(int l)
        {
            string sss = "";
            if (l == 1)
                sss = "группа";
            else if (l > 1 && l < 5)
                sss = "группы";
            else
                sss = "групп";
            sss = l.ToString() + " " + sss;
            return sss;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                int min=listBox2.Items.Count;
                foreach (itemObjOrdered sel in listBox2.SelectedItems)
                {
                    if (listBox2.Items.IndexOf(sel) < min) 
                        min = listBox2.Items.IndexOf(sel);

                    lllist.Add(sel);
                }
                if (min > 0)
                {
                    listBox2.ClearSelected();
                    foreach (itemObjOrdered sel in lllist)
                    {
                        int k = listBox2.Items.IndexOf(sel);
                        listBox2.Items.Remove(sel);
                        listBox2.Items.Insert(k - 1, sel);
                        listBox2.SelectedItems.Add(sel);
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count > 0)
            {
                List<itemObjOrdered> lllist = new List<itemObjOrdered>();
                int max = 0;
                foreach (itemObjOrdered sel in listBox2.SelectedItems)
                {
                    if (listBox2.Items.IndexOf(sel) > max)
                        max = listBox2.Items.IndexOf(sel);

                    lllist.Add(sel);
                }
                if (max < listBox2.Items.Count - 1)
                {
                    listBox2.ClearSelected();
                    for (int i = lllist.Count-1; i >= 0; i--)
                    {
                        itemObjOrdered sel = lllist[i];
                        int k = listBox2.Items.IndexOf(sel);
                        listBox2.Items.Remove(sel);
                        listBox2.Items.Insert(k + 1, sel);
                        listBox2.SelectedItems.Add(sel);
                    }
                }
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            itemColor cur = new itemColor();
            foreach (itemColor ic in itemColors)
            {
                if (e.Index == ic.index)
                {
                    cur = ic;
                    break;
                }
            }

            
            Graphics g = e.Graphics;
            if (cur.index!=-1)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255 - cur.color, 255, 255 - cur.color)), e.Bounds);
                g.DrawString((sender as ListBox).Items[e.Index].ToString(), (sender as ListBox).Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }else
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(51, 153, 255)), e.Bounds);
                g.DrawString((sender as ListBox).Items[e.Index].ToString(), (sender as ListBox).Font, Brushes.White, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }
            else
            {
                e.DrawBackground();
                g.DrawString((sender as ListBox).Items[e.Index].ToString(), (sender as ListBox).Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }

            // Print text

            e.DrawFocusRectangle();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(itemColors.Count==0)
            {
                timer1.Enabled=false;
                return;
            }
            for (int i = 0; i < itemColors.Count; i++)
            {
                itemColors[i].color -= 15;
                if (itemColors[i].color <= 0)
                {
                    itemColors.RemoveAt(i);
                    i--;
                }
            }
            listBox1.Refresh();
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(51, 153, 255)), e.Bounds);
                g.DrawString(e.Index.ToString() + ") " + (sender as ListBox).Items[e.Index].ToString(), (sender as ListBox).Font, Brushes.White, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }
            else
            {
                g.DrawString(e.Index.ToString() + ") " + (sender as ListBox).Items[e.Index].ToString(), (sender as ListBox).Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }

            // Print text

            e.DrawFocusRectangle();
        }

    }
    public class itemColor
    {
        public int color;
        public int index;
        public itemColor()
        {
            color = -1;
            index = -1;
        }
        public itemColor(int colorr, int indexx)
        {
            color = colorr;
            index = indexx;
        }
    }
}
