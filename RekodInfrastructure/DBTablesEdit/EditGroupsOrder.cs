using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod
{
    public partial class EditGroupsOrder : UserControl
    {
        private DBTablesGroups _parentForm;
        private PanelGroup _panelGroup;
        private GroupFunct _groupFunct; 
        public EditGroupsOrder(DBTablesGroups parent, PanelGroup panelgroup)
        {
            _parentForm = parent;
            _panelGroup = panelgroup;
            _groupFunct = new GroupFunct(); 
            InitializeComponent();
            listBox1.SelectedIndexChanged += (o, e) =>
            {
                int i = listBox1.SelectedIndex;
                buttonBringUp.Enabled = (i > 0);
                buttonBringDown.Enabled = (i != -1 && i != listBox1.Items.Count - 1);
                buttonEditNameDesc.Enabled = deleteButton.Enabled = (i != -1);
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _parentForm.CloseModalElem(); 
        }

        private void EditGroupsOrder_Load(object sender, EventArgs e)
        {
            reloadGroups(); 
        }

        private void reloadGroups()
        {
            listBox1.Items.Clear();
            List<itemObjOrdered> listGroups = _groupFunct.LoadGroups();
            itemObjOrdered[] arr_group = new itemObjOrdered[listGroups.Count];
            listGroups.CopyTo(arr_group);
            listBox1.Items.AddRange(arr_group);
            labelCount.Text = listBox1.Items.Count.ToString() + " " + Rekod.Properties.Resources.EGO_groups; 
        }


        private void addButton_Click(object sender, EventArgs e)
        {
            AddEditGroup aeg = new AddEditGroup(new itemObjOrdered(-1, "", "", -1), this);
            aeg.Location = new Point((this.Width - aeg.Width) / 2, (this.Height - aeg.Height) / 2); 
            groupBox1.Enabled = false; 
            this.Controls.Add(aeg);
            aeg.BringToFront(); 
        }
        public void CloseAddEditGroup(AddEditGroup aeg)
        {
            if (aeg.SaveOk)
            {
                if (aeg.GroupId < 0)
                {
                    _groupFunct.AddNewGroup(aeg.GroupName, aeg.GroupDescription);
                    reloadGroups(); 
                }
                else 
                {
                    _groupFunct.SaveGroup(aeg.GroupId, aeg.GroupName, aeg.GroupDescription);
                    reloadGroups(); 
                }
            }

            this.Controls.Remove(aeg);
            groupBox1.Enabled = true;
            aeg.Dispose();
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                DialogResult dr = MessageBox.Show(String.Format(Rekod.Properties.Resources.EGO_DeleteGroup, (listBox1.SelectedItem as itemObj).Name_o),
                    Rekod.Properties.Resources.EGO_DeletingGroup, MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    _groupFunct.DeleteGroup((listBox1.SelectedItem as itemObjOrdered).Id_o);
                    reloadGroups();
                }
            }
        }
        private void buttonEditNameDesc_Click(object sender, EventArgs e)
        {
            editNameDescription();
        }
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            editNameDescription();
        }
        private void editNameDescription()
        {
            itemObjOrdered itemObjOrd = listBox1.SelectedItem as itemObjOrdered;
            if (itemObjOrd != null)
            {
                AddEditGroup aeg = new AddEditGroup(itemObjOrd, this);
                aeg.Location = new Point((this.Width - aeg.Width) / 2, (this.Height - aeg.Height) / 2);
                groupBox1.Enabled = false;
                this.Controls.Add(aeg);
                aeg.BringToFront();
            }
        }
        private void buttonBringUp_Click(object sender, EventArgs e)
        {
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
        }
        
        private void buttonBringDown_Click(object sender, EventArgs e)
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
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {   
            List<itemObjOrdered> newGroupsOrder = new List<itemObjOrdered>();
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                newGroupsOrder.Add(listBox1.Items[i] as itemObjOrdered); 
            }

            _groupFunct.SetGroupsOrder(newGroupsOrder);
            _panelGroup.reloadGroups(); 
            _parentForm.CloseModalElem();
            Program.mainFrm1.layerItemsView1.RefreshLayers();
        }        
    }
}