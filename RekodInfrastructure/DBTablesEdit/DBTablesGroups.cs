using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Npgsql;
//using Rekod.DBTablesEdit;

namespace Rekod
{
    public partial class DBTablesGroups : Form
    {
        private List<UserControl> list_of_controls = new List<UserControl>();
        private List<GroupBox> list_of_grbox = new List<GroupBox>();
        private List<GroupBox> list_of_grbox_d = new List<GroupBox>();
        private List<GroupBox> list_of_grbox_dock = new List<GroupBox>();
        private int width_controls = 0;
        private int max_height_control = 0;
        private bool showed_modal = false;
        private UserControl modal_comtrol;
        private int standartWidthUderModalControl=0;

        private int change_width = 0;
        private int change_height=0;
        private int speed = 4;
        private int delta_speed = 25;//чем больше тем меньше скорость
        private System.Windows.Forms.Timer timerr;

        private void changeSize(object sender, EventArgs eArgs)
        {
            Size ss = new Size(0, 0);
            if (change_width != 0 || change_height!=0)
            {
                if (change_width > speed)
                {
                    change_width -= speed;
                    ss.Width += speed;
                }
                else if (-change_width > speed)
                {
                    change_width += speed;
                    ss.Width -= speed;
                }
                else
                {
                    ss.Width += change_width;
                    change_width = 0;
                }

                if (change_height > speed)
                {
                    change_height -= speed;
                    ss.Height += speed;
                }
                else if (-change_height > speed)
                {
                    change_height += speed;
                    ss.Height -= speed;
                }
                else
                {
                    ss.Height += change_height;
                    change_height = 0;
                }
                this.Size += ss;
            }
            else
            {
                timerr.Enabled = false;
                endChangeSize();
                
            }
        }
        private void endChangeSize()
        {
            foreach (GroupBox gr in list_of_grbox_d)
            {
                mainPanel.Controls.Remove(gr);
            }
            list_of_grbox_d.Clear();
            foreach (GroupBox gr in list_of_grbox_dock)
            {
                gr.Dock = DockStyle.None;
            }
            list_of_grbox_dock.Clear();
        }
        public DBTablesGroups(NpgsqlConnection con1)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            this.max_height_control = this.Height;
            timerr = new System.Windows.Forms.Timer();
            timerr.Interval = 15;
            timerr.Tick += new EventHandler(changeSize);
            button3_Click(button3, null);
            if (Program.app.sscUser != null)
                button2.Visible = false;
            //panelForControlModal.Dock = DockStyle.Top;
            //panelForControlModal.Height = 200;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (button2.BackgroundImage == null)
            {
                CloseElemsAfter(null, false);
                PanelGroup pg = new PanelGroup(this);
                AddNewElem(pg, Rekod.Properties.Resources.DBTG_ManagingGroups);
                button2.BackgroundImage = Rekod.Properties.Resources.bg_selecteg_button;
                button3.BackgroundImage = null;
                button4.BackgroundImage = null;
                button5.BackgroundImage = null;
                button6.BackgroundImage = null;
            }
            //else
            //{
            //    CloseElemsAfter(null, false);
            //    checkBox2.BackgroundImage = null;
            //}
            panel1.Focus();
        }
        public void AddNewElem(UserControl pl,string name)
        {
            if (showed_modal)
                CloseModalElem();
            list_of_controls.Insert(list_of_controls.Count, pl);//помещаем контрол в конец
            GroupBox gr=new GroupBox();
            list_of_grbox.Insert(list_of_grbox.Count, gr);

            gr.Size=new Size(pl.Width+2,pl.Height);
            int prev_max_height_control = this.Height-45;
            if (pl.Height > max_height_control)
            {
                //max_height_control = pl.Height;
                foreach (GroupBox ggr in list_of_grbox)
                {
                    //ggr.Height = pl.Height+10;
                }
            }
            else
            {
                gr.Height = max_height_control+20;
            }
            gr.Text = name;
            gr.Left = width_controls + panel1.Width;
            //gr.BackColor = Color.Aquamarine;
            pl.Top = 15;
            pl.Left = 0;

            if (!(pl is PanelGroup) && !(pl is TableEdit))
            {
                CloseButton but = new CloseButton(pl, this);
                but.Location = new Point(gr.Width - 20, 2);
                gr.Controls.Add(but);
            }

            mainPanel.Controls.Add(gr);
            gr.Dock = DockStyle.Right;

            gr.SendToBack();
            foreach (GroupBox uc in list_of_grbox_d)
            {
                uc.SendToBack();
            }
            list_of_grbox_dock.Add(gr);
            pl.Dock = DockStyle.Fill;
            gr.Controls.Add(pl);
            width_controls += gr.Width;

            change_width = panel1.Width + width_controls + 6 - this.Width;
            change_height = max_height_control - prev_max_height_control;

            speed = (Math.Abs(change_height) > Math.Abs(change_width)) ? Math.Abs(change_height) / delta_speed : Math.Abs(change_width) / delta_speed;
            if (speed < 3) speed = 3;
            timerr.Enabled = true;

            //this.Size = new Size(panel1.Width + width_controls ,max_height_control+45);
        }
        public void AddNewElemAfter(UserControl pl, UserControl after, string name)
        {
            if (showed_modal)
                CloseModalElem();
            int k = list_of_controls.IndexOf(after)+1;
            list_of_controls.Insert(k, pl);//помещаем контрол в конец
            GroupBox gr = new GroupBox();
            list_of_grbox.Insert(k, gr);

            gr.Size = new Size(pl.Width + 6, pl.Height + 10);

            int prev_max_height_control = this.Height - 45;
            if (pl.Height > max_height_control)
            {
                max_height_control = pl.Height;
                foreach (GroupBox ggr in list_of_grbox)
                {
                    ggr.Height = pl.Height + 10;
                }
            }
            else
            {
                gr.Height = max_height_control+19;
            }
            
            gr.Text = name;
            gr.Left = width_controls+panel1.Width;
            //gr.BackColor = Color.Aquamarine;
            pl.Top = 15;
            pl.Left = 0;

            CloseButton but = new CloseButton(pl, this);
            but.Location = new Point(gr.Width - 20, 2);
            but.Name = "CloseButton";
            gr.Controls.Add(but);
            gr.Controls.Add(pl);

            for (int i = list_of_grbox.IndexOf(gr) + 1; i < list_of_grbox.Count; i++)
            {
                mainPanel.Controls.Remove(list_of_grbox[i]);
            }
            mainPanel.Controls.Add(gr);
            gr.Dock = DockStyle.Right;
            gr.SendToBack();
            list_of_grbox_dock.Add(gr);
            foreach (GroupBox uc in list_of_grbox_d)
            {
                uc.SendToBack();
            }
            pl.Dock = DockStyle.Fill;
            for (int i = list_of_grbox.IndexOf(gr) + 1; i < list_of_grbox.Count; i++)
            {
                mainPanel.Controls.Add(list_of_grbox[i]);
            }

            width_controls += gr.Width;
            change_width = panel1.Width + width_controls + 6 - this.Width;
            change_height = max_height_control - prev_max_height_control;

            speed = (Math.Abs(change_height) > Math.Abs(change_width)) ? Math.Abs(change_height) / delta_speed : Math.Abs(change_width) / delta_speed;
            if (speed < 3) speed = 3;
            timerr.Enabled = true;
            
            //this.Size = new Size(panel1.Width + width_controls, max_height_control + 45);
        }
        public void AddNewElemModal(UserControl pl, string name)
        {
            if (showed_modal)
                CloseModalElem();

            if (list_of_controls[list_of_controls.Count - 1].Width < pl.Width)
            {
                standartWidthUderModalControl = list_of_controls[list_of_controls.Count - 1].Width;
                list_of_grbox[list_of_grbox.Count - 1].Width = pl.Width + 32;
                list_of_controls[list_of_controls.Count - 1].Width = pl.Width + 30;
                width_controls += -standartWidthUderModalControl + list_of_controls[list_of_controls.Count - 1].Width;

                change_width = panel1.Width + width_controls + 6 - this.Width;

                speed = (Math.Abs(change_height) > Math.Abs(change_width)) ? Math.Abs(change_height) / delta_speed : Math.Abs(change_width) / delta_speed;
                if (speed < 3) speed = 3;
                timerr.Enabled = true;
                //this.Size = new Size(panel1.Width + width_controls, max_height_control + 45);
            }

            list_of_controls[list_of_controls.Count - 1].Enabled = false;
            list_of_grbox[list_of_grbox.Count - 1].Controls.Add(pl);
            pl.BringToFront();
            pl.Location = new Point((list_of_grbox[list_of_grbox.Count - 1].Width - pl.Width) / 2, (list_of_grbox[list_of_grbox.Count - 1].Height - pl.Height) / 2);
            modal_comtrol = pl;
            showed_modal = true;
        }
        public void CloseElemsAfter(UserControl pl,bool delete_self)
        {
            if (list_of_controls.Count == 0) return;

            if (showed_modal)
                CloseModalElem();

            int j = list_of_controls.IndexOf(pl);
            if (!delete_self) j++;
            if (j == -1) return;
            for (; j < list_of_controls.Count; )
            {
                width_controls -= list_of_grbox[j].Width;
                //this.Width = panel1.Width + width_controls + 2;
                list_of_grbox[j].Dock = DockStyle.None;
                if (list_of_controls[j] is FiledEditListControl)
                {
                    (list_of_controls[j] as FiledEditListControl).FiledEditListControl_ControlRemoved(this, null);
                }
                list_of_grbox_d.Add(list_of_grbox[j]);
                //mainPanel.Controls.Remove(list_of_grbox[j]);
                list_of_grbox.RemoveAt(j);
                list_of_controls.RemoveAt(j);
            }
            int prev_max_height_control = this.Height - 45;
            //max_height_control = 100;
            foreach (UserControl uc in list_of_controls)
                if (uc.Height > max_height_control) max_height_control = uc.Height;

            change_width = panel1.Width + width_controls + 6 - this.Width;
            change_height = max_height_control - prev_max_height_control;

            speed = (Math.Abs(change_height) > Math.Abs(change_width)) ? Math.Abs(change_height) / delta_speed : Math.Abs(change_width) / delta_speed;
            if (speed < 3) speed = 3;
            timerr.Enabled = true;
            //this.Size = new Size(panel1.Width + width_controls, max_height_control + 45);
        }
        public void CloseModalElem()
        {
            if (showed_modal)
            {
                if(modal_comtrol!=null)
                    list_of_grbox[list_of_grbox.Count - 1].Controls.Remove(modal_comtrol);
                list_of_controls[list_of_controls.Count - 1].Enabled = true;
                if (standartWidthUderModalControl > 0)
                {
                    width_controls += standartWidthUderModalControl - list_of_controls[list_of_controls.Count - 1].Width;

                    change_width = panel1.Width + width_controls + 6 - this.Width;

                    speed = (Math.Abs(change_height) > Math.Abs(change_width)) ? Math.Abs(change_height) / delta_speed : Math.Abs(change_width) / delta_speed;
                    if (speed < 3) speed = 3;
                    timerr.Enabled = true;
                    //this.Size = new Size(panel1.Width + width_controls, max_height_control + 45);
                    list_of_controls[list_of_controls.Count - 1].Width = standartWidthUderModalControl;
                    list_of_grbox[list_of_grbox.Count - 1].Width = standartWidthUderModalControl+6;
                    
                }
                modal_comtrol = null;
                showed_modal = false;
                standartWidthUderModalControl = -1;
            }
        }

        private void DBTablesGroups_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseElemsAfter(null, false);
            Program.repository.ReloadPartInfo();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (list_of_controls.Count > 0)
            {
                if (!(list_of_controls[0] is TableEdit))
                {
                    CloseElemsAfter(null, false);
                    TableEdit te = new TableEdit(null, this);
                    AddNewElem(te, Rekod.Properties.Resources.DBTG_TablesList);
                    button2.BackgroundImage = null;
                }
            }
            else
            {
                CloseElemsAfter(null, false);
                TableEdit te = new TableEdit(null, this);
                AddNewElem(te, Rekod.Properties.Resources.DBTG_TablesList);
            }

            button3.BackgroundImage = null;
            button4.BackgroundImage = null;
            button5.BackgroundImage = null;
            button6.BackgroundImage = null;
            if(sender==button3){
                button3.BackgroundImage = Rekod.Properties.Resources.bg_selecteg_button;
                button2.BackgroundImage = null;
                (list_of_controls[0] as TableEdit).cbTableType.SelectedIndex = 0;
            }
            else if (sender == button4)
            {
                button4.BackgroundImage = Rekod.Properties.Resources.bg_selecteg_button;
                button2.BackgroundImage = null;
                (list_of_controls[0] as TableEdit).cbTableType.SelectedIndex = 1;
            }
            else if (sender == button5)
            {
                button5.BackgroundImage = Rekod.Properties.Resources.bg_selecteg_button;
                button2.BackgroundImage = null;
                (list_of_controls[0] as TableEdit).cbTableType.SelectedIndex = 2;
            }
            else if (sender == button6)
            {
                button6.BackgroundImage = Rekod.Properties.Resources.bg_selecteg_button;
                button2.BackgroundImage = null;
                (list_of_controls[0] as TableEdit).cbTableType.SelectedIndex = 3;
            }
            //else
            //{
            //    CloseElemsAfter(null, false);
            //    checkBox1.BackgroundImage = null;
            //}
            panel1.Focus();
        }

        private void DBTablesGroups_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.mainFrm1.axMapLIb1.mapRepaint();
            Program.mainFrm1.layerItemsView1.RefreshLayers();
            Program.mainFrm1.bManager.SetButtonsState();
        }
    }
}