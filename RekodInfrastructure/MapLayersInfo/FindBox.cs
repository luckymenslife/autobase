using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Rekod
{
    public partial class FindBox : UserControl
    {
        public object par;//parent
        private bool isSearching = false;
        private bool onMouseUpSelectText = false;
        private bool colChanged = false;

        public int changePosition=0;
        private int speedChangePos = 2;
        public Timer timerr;
        string[] find_list_text=new string[]{"Included","Includes first","Not included"};
        string[] find_list = new string[] { "=", ">", "<","<>",">=","<=" };

        private bool removeFunc = false;//это для того чтобы после добавления нового поиска значек и функция менялись на удаление

        public FindBox(object parentt)
        {
            InitializeComponent();

            comboBox2.SelectedIndex = 0;
            par = parentt;
            timerr = new Timer();
            timerr.Tick += new EventHandler(timer_tick_change_pos);
            timerr.Interval = 15;
            if (par is layerInfo)
            {
                comboBox1.Items.AddRange((par as layerInfo).listItem);
                //comboBox2.Visible = false;
                //textBox1.Left -= comboBox2.Width + 5;
                //button1.Left -= comboBox2.Width + 5;
            }
            //if(par is itemsTableGridControl)
            //    comboBox1.Items.AddRange((par as itemsTableGridControl).listItem);
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += selectBox_SelectedIndexChanged;
        }
        public FindRequest getFindRequest()//возвращает настройки поиска
        {
            return new FindRequest(comboBox1.SelectedIndex - 1,
                (isSearching) ? textBox1.Text : "",
                (comboBox2.Items.Count==6)? comboBox2.SelectedIndex : comboBox2.SelectedIndex+6);
        }

        private void findBox_Enter(object sender, EventArgs e)
        {
            if (!isSearching)
            {
                (sender as TextBox).ForeColor = Color.Black;
                (sender as TextBox).Text = "";
                isSearching = true;
            }
            else
            {
                (sender as TextBox).Select(0, (sender as TextBox).Text.Length);
                onMouseUpSelectText = true;
            }
        }

        private void findBox_Leave(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.CompareTo("") == 0)
            {
                isSearching = false;
                (sender as TextBox).ForeColor = Color.DarkGray;
                (sender as TextBox).Text = Rekod.Properties.Resources.FindBox_Find;
            }
        }

        private void findBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (onMouseUpSelectText)
            {
                (sender as TextBox).Select(0, (sender as TextBox).Text.Length);
                onMouseUpSelectText = false;
            }
        }

        private void findBox_TextChanged(object sender, EventArgs e)
        {
            if (!isSearching)
                return;
            if(par is layerInfo)
                (par as layerInfo).findBoxChange();
            //if (par is itemsTableGridControl)
            //    (par as itemsTableGridControl).findBoxChange();
        }
        private void find_Click(object sender, EventArgs e)
        {
            if (colChanged)
            {
                if (!isSearching)
                    return;
                if (par is layerInfo)
                    (par as layerInfo).findBoxChange();
                //if (par is itemsTableGridControl)
                //    (par as itemsTableGridControl).findBoxChange();
            }
        }

        private void selectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            colChanged = true;
            //if (par is itemsTableGridControl)
            //{
            //    int n = (comboBox2.Items.Count == 6) ? 1 : 2;
            //    int nn = ((sender as ComboBox).SelectedIndex == 0) ? 2 : 1;//во всем
            //    //if ((sender as ComboBox).SelectedIndex != 0)
            //    //    if ((par as itemsTableGridControl).typesCol[(sender as ComboBox).SelectedIndex - 1] == 2)
            //    //        nn = 2;//текст
            //    if (n != nn)
            //    {
            //        comboBox2.Items.Clear();
            //        if (nn == 1)
            //            comboBox2.Items.AddRange(find_list);
            //        if (nn == 2)
            //            comboBox2.Items.AddRange(find_list_text);
            //        comboBox2.SelectedIndex = 0;
            //    }
            //}
            if (!isSearching || textBox1.Text.CompareTo("") == 0) return;
            if (par is layerInfo)
            {
                (par as layerInfo).findBoxChange();
            }
            //if (par is itemsTableGridControl)
            //{
            //    (par as itemsTableGridControl).findBoxChange();
            //}
        }

        private void findBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)//если нажали на Enter
            {
                find_Click(this, null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!removeFunc)
            {//add
                remove_func = true;
                if(par is layerInfo)
                    (par as layerInfo).addNewFindBox();
                //if (par is itemsTableGridControl)
                //    (par as itemsTableGridControl).addNewFindBox();
            }
            else
            {//remove
                if (par is layerInfo)
                    (par as layerInfo).delFindBox(this);
                //if (par is itemsTableGridControl)
                //    (par as itemsTableGridControl).delFindBox(this);
            }
        }
        private void timer_tick_change_pos(object sender, EventArgs eArgs)
        {
            if(changePosition==0)
                (sender as Timer).Stop();
            else{
                if (speedChangePos <= changePosition)
                {
                    this.Top -= speedChangePos;
                    changePosition -= speedChangePos;
                }
                else
                {
                    this.Top -= changePosition;
                    changePosition = 0;
                }
            }
        }
        

        public bool remove_func
        {
            get
            {
                return removeFunc;
            }
            set
            {
                removeFunc = value;
                if (value)
                    button2.BackgroundImage = Rekod.Properties.Resources.remove;
                else
                    button2.BackgroundImage = Rekod.Properties.Resources.add;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colChanged)
            {
                if (!isSearching)
                    return;
                if (par is layerInfo)
                    (par as layerInfo).findBoxChange();
                //if (par is itemsTableGridControl)
                //    (par as itemsTableGridControl).findBoxChange();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as ComboBox).Items.Count == 6)
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 0:
                        toolTip1.SetToolTip(sender as Control, "=");
                        break;
                    case 1:
                        toolTip1.SetToolTip(sender as Control, ">");
                        break;
                    case 2:
                        toolTip1.SetToolTip(sender as Control, "<");
                        break;
                    case 3:
                        toolTip1.SetToolTip(sender as Control, "<>");
                        break;
                    case 4:
                        toolTip1.SetToolTip(sender as Control, ">=");
                        break;
                    case 5:
                        toolTip1.SetToolTip(sender as Control, "<=");
                        break;
                }
            else toolTip1.SetToolTip(sender as Control, (sender as ComboBox).SelectedItem.ToString());
            if (!isSearching || textBox1.Text.CompareTo("") == 0) return;
            if (par is layerInfo)
                (par as layerInfo).findBoxChange();
            //if (par is itemsTableGridControl)
            //    (par as itemsTableGridControl).findBoxChange();
        }
    }
    public class FindRequest
    {
        public int col;
        public string findText;
        public int type_fr =0;
        public FindRequest(int coll, string findd)
        {
            col = coll;
            findText = findd;
        }
        public FindRequest(int coll, string findd,int type)
        {
            col = coll;
            findText = findd;
            type_fr = type;
        }
    }
}
