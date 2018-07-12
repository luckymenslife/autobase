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
    public partial class AddEditGroup : UserControl
    {
        private EditGroupsOrder _parent;
        public int GroupId
        {
            get { return _id; }
        }
        public String GroupName
        {
            get { return textBox1.Text; }
        }
        public String GroupDescription
        {
            get { return textBox2.Text; }
        }
        private int _id;
        private GroupFunct gf;
        public bool SaveOk = false; 
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id_group">ID группы, если же создаеться новая то должно быть отрицательным</param>
        /// <param name="par">Форма на которой создаеться этот контрол</param>
        /// <param name="p_group">Контрол из которого создаеться</param>
        public AddEditGroup(itemObjOrdered itemObjOrd, EditGroupsOrder parent)
        {
            InitializeComponent();
            _parent = parent;
            _id = itemObjOrd.Id_o; 
            gf = new GroupFunct();

            textBox1.Text = itemObjOrd.Name_o;
            textBox2.Text = itemObjOrd.Layer; 
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveOk = false; 
            _parent.CloseAddEditGroup(this); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != string.Empty)
            {
                SaveOk = true;
                _parent.CloseAddEditGroup(this);
            }
            else
            {
                toolTip1.Show("Имя не должно быть пустым или содержать только пробелы", this,
                    textBox1.Left, textBox1.Bottom + 10, 2000);
            }
        }
    }
}
