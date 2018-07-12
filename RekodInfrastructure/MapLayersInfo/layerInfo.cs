using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using mvMapLib;
using AxmvMapLib;
using System.Threading;

namespace Rekod
{
    public partial class layerInfo : Form
    {
        private AxMapLIb axMapLIb1;
        private DataGridViewRow[] dgvrc;
        private string layerName;
        private Thread lastThread;
        private bool runFirstTime = true;
        private int objCountt;

        private int changeSize = 0;
        private int speedResize = 2;

        private List<FindBox> findBoxList;
        private List<FindBox> findBoxListForDelete;
        public string[] listItem;
        private List<FindRequest> listFindRequest;
        private System.Windows.Forms.Timer timerr;

        public layerInfo(AxMapLIb a, string layer_name)
        {
            InitializeComponent();
            classesOfMetods.SetFormOwner(this);
            axMapLIb1 = a;
            layerName = layer_name;
            this.Text = string.Format(Rekod.Properties.Resources.layerInfo_LayerMap, layer_name);
        }
        private void layerInfo_Load(object sender, EventArgs e)
        {
            loadFileds(dataGridView1);
            dgvrc = loadRowsInTable();

            findBoxList = new List<FindBox>();
            findBoxListForDelete = new List<FindBox>();
            listFindRequest = new List<FindRequest>();

            FindBox newFB = new FindBox(this);
            newFB.Top = 0;
            panelForFindBoxes.Controls.Add(newFB);
            findBoxList.Add(newFB);
            panelForFindBoxes.Height = newFB.Height;
            //dataGridView1.Top = newFB.Height;
            //dataGridView1.Height = 507 - newFB.Height;

            lastThread = new Thread(delegate() { searchAll(dgvrc, listFindRequest); });
            lastThread.Start();

            timerr = new System.Windows.Forms.Timer();
            timerr.Interval = 15;
            timerr.Tick += new EventHandler(timer_tick_resize);
        }
        private void loadFileds(DataGridView dgv)
        {
            mvLayer layer = axMapLIb1.getLayer(layerName);
            int count = layer.getFields().count;
            DataGridViewColumn[] list = new DataGridViewColumn[count];
            listItem = new string[count + 1];//для FindBox, он при создании обращается к этой переменной для Items в combobox
            listItem[0] = Rekod.Properties.Resources.layerInfo_OverAll;
            //создаем колонки, далее заносим названия колонок в них
            for (int i = 0; i < count; i++)
            {
                list[i] = new DataGridViewTextBoxColumn();
                list[i].Name = ExtraFunctions.Converts.To<string>(layer.FieldName(i));
                list[i].HeaderText = list[i].Name;
                listItem[i + 1] = list[i].Name;
            }
            //кидаем все в дата вьюв
            dgv.Columns.AddRange(list);
        }
        private DataGridViewRow[] loadRowsInTable()
        {
            mvLayer layer = axMapLIb1.getLayer(layerName);
            var rowCount = (layer.ObjectsCount > 50000)
                    ? 50000
                    : layer.ObjectsCount;
            DataGridViewRow[] k = new DataGridViewRow[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                k[i] = new DataGridViewRow();
                for (int j = 0; j < layer.FieldsCount; j++)
                {
                    DataGridViewCell cell = new DataGridViewTextBoxCell();
                    bool del;
                    cell.Value = layer.FieldValueByNum(i + 1, j, out del);

                    k[i].Cells.Add(cell);
                }
                k[i].Tag = i + 1;
            }
            objCountt = layer.ObjectsCount;
            return k;
        }
        private void InThread(MethodInvoker mth)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
                Invoke(mth);
            else
                mth();
        }
        private void searchAll(DataGridViewRow[] rows, List<FindRequest> lfr)
        {
            bool emptySearch = true;
            foreach (FindRequest fr in lfr)
            {
                if (fr.findText.CompareTo("") != 0)
                {
                    emptySearch = false;
                    break;
                }
            }
            if (emptySearch)
            {
                InThread(() =>
                {
                    this.dataGridView1.Rows.Clear();
                    dataGridView1.Rows.AddRange(rows);
                    tstlObjectLoaded.Text = dataGridView1.Rows.Count.ToString();
                    tstlObjectAll.Text = objCountt.ToString();
                    toolStripProgressBar1.Visible = false;
                });
            }
            else
            {
                InThread(() =>
                {
                    this.dataGridView1.Rows.Clear();
                    tstlObjectLoaded.Text = "0";
                    tstlObjectAll.Text = objCountt.ToString();
                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Value = 0;
                });
                bool ad = true;
                bool searchedInRow = true;
                int curIndex = 0;
                int countLoaded = 0;
                DataGridViewRow[] rows2 = new DataGridViewRow[0];
                foreach (DataGridViewRow dr in rows)
                {
                    if (ad)
                    {
                        ad = false;
                        rows2 = new DataGridViewRow[1000];
                        curIndex = 0;
                    }

                    searchedInRow = true;//при первом несоответствии будет переведено в false
                    foreach (FindRequest fr in lfr)
                    {
                        if (fr.col >= 0)//поиск по 1 колонке
                        {
                            if (dr.Cells[fr.col].Value != null)
                            {
                                if (!dr.Cells[fr.col].Value.ToString().ToLower().Contains(fr.findText.ToLower()))
                                    searchedInRow = false;
                            }
                        }//поиск по всем колонкам
                        else
                        {
                            bool added = false;
                            for (int i = 0; i < dr.Cells.Count && !added; i++)//проходим по всем колонкам
                            {
                                if (dr.Cells[i].Value != null)
                                {
                                    if (dr.Cells[i].Value.ToString().ToLower().Contains(fr.findText.ToLower()))
                                        added = true;
                                }
                            }
                            if (!added)
                                searchedInRow = false;
                        }
                    }
                    if (searchedInRow)
                    {
                        rows2[curIndex] = dr;
                        curIndex++;
                    }
                    if (curIndex >= 1000)
                    {
                        InThread(() => dataGridView1.Rows.AddRange(rows2));
                        curIndex = 0;
                        ad = true;
                        countLoaded += 1000;
                        InThread(() => toolStripProgressBar1.Value = 100 * countLoaded / objCountt);
                    }
                }
                DataGridViewRow[] tempRows = new DataGridViewRow[curIndex];
                for (int j = 0; j < curIndex; j++)
                    tempRows[j] = rows2[j];


                InThread(() =>
                {
                    dataGridView1.Rows.AddRange(tempRows);
                    toolStripProgressBar1.Visible = false;
                });

            }
            if (runFirstTime)
            {
                InThread(() =>
                {
                    this.Show();
                    this.Focus();
                });
            }
        }
        private void layerInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (lastThread != null)
                lastThread.Abort();
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                int numOfObject = int.Parse(dataGridView1.Rows[e.RowIndex].Tag.ToString());
                mvLayer layer = axMapLIb1.getLayer(layerName);
                if (layer != null)
                {
                    layer.DeselectAll();
                    mvVectorObject vo = layer.getObjectByNum(numOfObject);
                    if (vo != null)
                    {
                        axMapLIb1.MoveTo(layer.getObjectByNum(numOfObject).Centroid);
                        vo.Selected = true;
                    }
                }
            }
        }
        public void findBoxChange()
        {
            if (lastThread != null)
            {
                lastThread.Abort();
            }
            listFindRequest.Clear();
            foreach (FindBox fb in findBoxList)
            {
                listFindRequest.Add(fb.getFindRequest());
            }
            lastThread = new Thread(delegate() { searchAll(dgvrc, listFindRequest); });
            lastThread.Start();
        }
        public void addNewFindBox()
        {
            FindBox newFB = new FindBox(this);
            newFB.Top = findBoxList.Count * newFB.Height;
            panelForFindBoxes.Controls.Add(newFB);
            findBoxList.Add(newFB);
            if (findBoxList.Count > 4)
            {
                newFB.remove_func = true;
            }
            changeSize += newFB.Height;//задаем размер на который надо изменить панельку
            timerr.Start();
        }
        public void delFindBox(FindBox deletingFB)
        {
            changeSize -= deletingFB.Height;//задаем размер на который надо изменить панельку
            FindBox lastFB = null;
            foreach (FindBox fb in findBoxList)
            {
                if (findBoxList.Count > 4)
                {
                    if (fb != deletingFB)
                        if (lastFB == null) lastFB = fb;
                        else
                            if (lastFB.Top < fb.Top)
                                lastFB = fb;
                }
                if (fb.Top > deletingFB.Top)
                {
                    fb.changePosition += fb.Height;
                    fb.BringToFront();
                    fb.timerr.Start();
                }
            }
            if (lastFB != null)
            {
                lastFB.remove_func = false;
            }
            findBoxList.Remove(deletingFB);
            findBoxListForDelete.Add(deletingFB);
            timerr.Start();
            if (deletingFB.getFindRequest().findText.CompareTo("") != 0)
                findBoxChange();
        }
        private void deleteFindBoxForDelete()
        {
            foreach (FindBox fb in findBoxListForDelete)
            {
                panelForFindBoxes.Controls.Remove(fb);
            }
            findBoxListForDelete.Clear();
        }
        private void timer_tick_resize(object sender, EventArgs eArgs)
        {
            if (changeSize == 0)
                (sender as System.Windows.Forms.Timer).Stop();
            else
            {
                if (changeSize < 0)
                {
                    if (-speedResize >= changeSize)
                    {
                        panelForFindBoxes.Height -= speedResize;
                        //dataGridView1.Height += speedResize;
                        //dataGridView1.Top -= speedResize;
                        changeSize += speedResize;
                        if (changeSize == 0)
                            deleteFindBoxForDelete();
                    }
                    else
                    {
                        panelForFindBoxes.Height += changeSize;
                        //dataGridView1.Height -= changeSize;
                        //dataGridView1.Top += changeSize;
                        changeSize = 0;
                        deleteFindBoxForDelete();
                    }
                }
                else
                {
                    if (speedResize <= changeSize)
                    {
                        //dataGridView1.Height -= speedResize;
                        //dataGridView1.Top += speedResize;
                        panelForFindBoxes.Height += speedResize;
                        changeSize -= speedResize;
                        if (changeSize == 0)
                            deleteFindBoxForDelete();//на всяк пожарный
                    }
                    else
                    {
                        //dataGridView1.Height -= changeSize;
                        //dataGridView1.Top += changeSize;
                        panelForFindBoxes.Height += changeSize;
                        changeSize = 0;
                        deleteFindBoxForDelete();//на всяк пожарный
                    }
                }
            }
        }
        private void layerInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
        }
    };
    public class index_showed
    {
        public int num;
        public bool showed;
        public index_showed()
        {
        }
        public index_showed(int n, bool s)
        {
            num = n;
            showed = s;
        }
    }
}