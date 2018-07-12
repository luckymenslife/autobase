using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using axVisUtils.Styles;
using axVisUtils.TableData;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Rekod.DBTablesEdit
{
    public partial class StyleControl : UserControl
    {
        private TabControl tb1;
        private int penIndex = 0;
        private DBTablesGroups parent;
        private propertyTableControl property_table;

        public StyleControl(bool showSymbol_, bool showBrushStyle_, bool showPenStyle_, bool canWrite = true)
        {
            InitializeComponent();
            btnOk.Enabled = canWrite;
            tb1 = new TabControl();
            loadPenList();
            loadImgBrush();

            foreach (FontFamily ff in FontFamily.Families)
            {
                if (ff.Name.Equals("Map Symbols", StringComparison.InvariantCultureIgnoreCase))
                {
                    fontDialog1.Font = new Font("Map Symbols", 14);
                    loadSymbol();
                }
            }
            showTabPanel(showSymbol_, showBrushStyle_, showPenStyle_);
        }
        private void loadImgBrush()
        {
            ibContainer.fillImages();
            ibContainer.PropertyChanged += ibContainer_PropertyChanged;
        }

        void ibContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ibContainer.selectedBrush != null)
            {
                switch (ibContainer.selectedBrush.style)
                {
                    case 0:
                        tbOpacity.Enabled = true;
                        checkBox1.Enabled = false;
                        break;
                    case 1:
                        tbOpacity.Enabled = false;
                        checkBox1.Enabled = false;
                        break;
                    default:
                        tbOpacity.Enabled = false;
                        checkBox1.Enabled = true;
                        break;
                }
            }
        }
        public StyleControl(DBTablesGroups par, propertyTableControl p_table, GeomType type, bool canWrite = true)
        {
            InitializeComponent();
            btnOk.Enabled = canWrite;
            tb1 = new TabControl();
            loadPenList();
            loadImgBrush();   
            foreach (FontFamily ff in FontFamily.Families)
            {
                if (ff.Name.Equals("Map Symbols", StringComparison.InvariantCultureIgnoreCase))
                {
                    fontDialog1.Font = new Font("Map Symbols", 14);
                    loadSymbol();
                }
            }
            showTabPanel(true, true, true);

            parent = par;
            property_table = p_table;

            TabPage symbPage = tabControl1.TabPages[0];
            TabPage penPage = tabControl1.TabPages[1];
            TabPage brushPage = tabControl1.TabPages[2]; 

            if (type == GeomType.point)
            {
                tabControl1.TabPages.Remove(penPage);
                tabControl1.TabPages.Remove(brushPage); 
            }
            else if (type == GeomType.line)
            {
                tabControl1.TabPages.Remove(symbPage);
                tabControl1.TabPages.Remove(brushPage); 
            }
            else if (type == GeomType.polygon)
            {
                tabControl1.TabPages.Remove(symbPage); 
            }
        }
        public void showTabPanel(bool showSymbol_, bool showBrushStyle_, bool showPenStyle_)
        {
            // Символ
            if (showSymbol_ && tb1.TabPages.Contains(tab_point))
            {
                tabControl1.TabPages.Add(tab_point);
                tb1.TabPages.Remove(tab_point);
            }
            else if (showSymbol_ && !tb1.TabPages.Contains(tab_point))
            {
            }
            else if (!showSymbol_ && tb1.TabPages.Contains(tab_point))
            {
            }
            else if (!showSymbol_ && !tb1.TabPages.Contains(tab_point))
            {
                tb1.TabPages.Add(tab_point);
                tabControl1.TabPages.Remove(tab_point);
            }
            // Кисть
            if (showBrushStyle_ && tb1.TabPages.Contains(tab_region))
            {
                tabControl1.TabPages.Add(tab_region);
                tb1.TabPages.Remove(tab_region);
            }
            else if (showBrushStyle_ && !tb1.TabPages.Contains(tab_region))
            {
            }
            else if (!showBrushStyle_ && tb1.TabPages.Contains(tab_region))
            {
            }
            else if (!showBrushStyle_ && !tb1.TabPages.Contains(tab_region))
            {
                tb1.TabPages.Add(tab_region);
                tabControl1.TabPages.Remove(tab_region);
            }
            // Карандаш
            if (showPenStyle_ && tb1.TabPages.Contains(tab_line))
            {
                tabControl1.TabPages.Add(tab_line);
                tb1.TabPages.Remove(tab_line);
            }
            else if (showPenStyle_ && !tb1.TabPages.Contains(tab_line))
            {
            }
            else if (!showPenStyle_ && tb1.TabPages.Contains(tab_line))
            {
            }
            else if (!showPenStyle_ && !tb1.TabPages.Contains(tab_line))
            {
                tb1.TabPages.Add(tab_line);
                tabControl1.TabPages.Remove(tab_line);
            }
        }
        private void button1_Click(object sender, EventArgs e)//...
        {
            fontDialog1.FontMustExist = true;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = fontDialog1.Font.Name;
                textBox1.Text = Math.Round((double)fontDialog1.Font.Size, 0).ToString();
                loadSymbol();
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (parent == null)//if(parent is not DBTablesGroups)-wtf
                if ((this.Parent as Form) != null && this.Parent.Name == "styleControlContainer")
                {
                    (this.Parent as Form).DialogResult = DialogResult.Cancel;
                    (this.Parent as Form).Close();
                }
                else
                    return;
            else
            {
                parent.CloseModalElem();
            }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (parent == null)//if(parent is not DBTablesGroups)-wtf
            {
                bool tyt_bil = false;
                if ((this.Parent as Form) != null && this.Parent.Name == "styleControlContainer")
                {
                    (this.Parent as Form).DialogResult = DialogResult.OK;
                    (this.Parent as Form).Close();
                    tyt_bil = true;
                    return;
                }
                if ((this.Parent.Parent.Parent as Form) != null && this.Parent.Parent.Parent.Name == "LayerStyleFrm")
                {
                    (this.Parent.Parent.Parent as Form).DialogResult = DialogResult.OK;
                    (this.Parent.Parent.Parent as Form).Close();
                    tyt_bil = true;
                    return;
                }
                if (!tyt_bil)
                    return;
            }
            else
            {
                property_table.saveStyle(this);
                parent.CloseModalElem();
            }
        }

        public axVisUtils.Styles.objStyleSymbol createSymbol()
        {
            objStyleSymbol s1 = new objStyleSymbol();
            if (comboBox2.SelectedItem != null)
            {
                s1.Shape = Convert.ToUInt32(char.Parse(comboBox2.SelectedItem.ToString()));
            }
            return s1;
        }
        public axVisUtils.Styles.objStyleFont createFont()
        {
            objStyleFont f1 = new objStyleFont();
            f1.Color = Convert.ToUInt32(
                panel2.BackColor.R +
                (panel2.BackColor.G << 8) +
                (panel2.BackColor.B << 16));
            f1.FrameColor = Convert.ToUInt32(
                panel1.BackColor.R +
                (panel1.BackColor.G << 8) +
                (panel1.BackColor.B << 16));
            if (!textBox2.Text.Equals(""))
            {
                f1.FontName = textBox2.Text;
                f1.Size = Convert.ToInt32(textBox1.Text);
            }
            return f1;
        }
        public axVisUtils.Styles.objStylePen createPen()
        {
            objStylePen p1 = new objStylePen();
            p1.Color = Convert.ToUInt32(
                panel3.BackColor.R +
                (panel3.BackColor.G << 8) +
                (panel3.BackColor.B << 16));
            if (!textBox3.Text.Equals(""))
            {
                p1.Width = Convert.ToUInt32(textBox3.Text);
            }
            if (listView1.SelectedItems.Count >= 1)
            {
                p1.Type = ushort.Parse(listView1.SelectedItems[0].ImageKey);
            }
            else
                p1.Type = initialPen.Type;
            return p1;
        }
        public axVisUtils.Styles.objStyleBrush createBrush()
        {
            objStyleBrush b1 = new objStyleBrush();
            long opacity = Convert.ToInt64(tbOpacity.Enabled ? 255 & (int)(tbOpacity.Value * 255 / 100d) : 0);
            if (checkBox1.Enabled && checkBox1.Checked || tbOpacity.Enabled)
            {
                b1.bgColor = Convert.ToUInt32(
                    panel4.BackColor.R +
                    (panel4.BackColor.G << 8) +
                    (panel4.BackColor.B << 16) +
                    (opacity << 24));
            } 
            else
            {
                b1.bgColor = 0xffffffff;
            }

            b1.fgColor = Convert.ToUInt32(
                panel5.BackColor.R +
                (panel5.BackColor.G << 8) +
                (panel5.BackColor.B << 16));
            if (ibContainer.selectedBrush != null)
            {
                b1.Style = (ushort)ibContainer.selectedBrush.style;
                b1.Hatch = (ushort)ibContainer.selectedBrush.hatch;
            }
            return b1;
        }
            
        public bool setStyleSymbol(axVisUtils.Styles.objStyleSymbol symb1)
        {
            bool ret = false;
            try
            {
                foreach (object ss in comboBox2.Items)
                {
                    if (Convert.ToUInt32(char.Parse(((string)ss))) == symb1.Shape)
                    {
                        comboBox2.SelectedIndex = comboBox2.Items.IndexOf(ss);
                        ret = true;
                        break;
                    }
                }
            }
            catch
            {
            }
            return ret;
        }
        public bool setStyleFont(axVisUtils.Styles.objStyleFont font1)
        {
            bool ret = false;
            try
            {
                panel2.BackColor = convColor(font1.Color);
                panel1.BackColor = convColor(font1.FrameColor);
                foreach (FontFamily ff in FontFamily.Families)
                {
                    if (ff.Name == font1.FontName)
                    {
                        fontDialog1.Font = new Font(font1.FontName, (float)font1.Size);
                        textBox2.Text = font1.FontName;
                        textBox1.Text = font1.Size.ToString();
                    }
                }
                ret = true;
            }
            catch
            {
            }
            return ret;
        }
        private objStylePen initialPen;
        public bool setStylePen(axVisUtils.Styles.objStylePen pen1)
        {
            initialPen = pen1;
            bool ret = false;
            try
            {
                panel3.BackColor = convColor(pen1.Color);
                textBox3.Text = pen1.Width.ToString();
                foreach (ListViewItem ii in listView1.Items)
                {
                    if (ii.ImageKey == pen1.Type.ToString())
                    {
                        listView1.Items[listView1.Items.IndexOf(ii)].Selected = true;
                        penIndex = listView1.Items.IndexOf(ii);
                        break;
                    }
                }
                ret = true;
            }
            catch
            {
            }
            return ret;
        }
        public bool setStyleBrush(axVisUtils.Styles.objStyleBrush brush1)
        {
            try
            {
                panel4.BackColor = convColor(brush1.bgColor);
                panel5.BackColor = convColor(brush1.fgColor);
                tbOpacity.Value = Convert.ToInt32((brush1.bgColor >> 24) * 100 / 255d);
                if (brush1.Style == 0)
                {
                    brush1.Style = 5;
                    brush1.Hatch = 2;
                }
                if (brush1.Style == 1)
                {
                    brush1.Style = 5;
                    brush1.Hatch = 1;
                }
                ibContainer.selectItem(brush1.Style,brush1.Hatch);
                return true;
            }
            catch { return false; };            
        }
        public bool setStyles(axVisUtils.Styles.objStylesM style)
        {
            bool rez = true;
            if (!setStyleSymbol(style.SymbolStyle)) rez = false;

            if (!setStyleFont(style.FontStyle)) rez = false;
            if (!setStylePen(style.PenStyle)) rez = false;
            if (!setStyleBrush(style.BrushStyle)) rez = false;
            if (style.BrushStyle.bgColor == 4294967295)
            {
                checkBox1.Checked = false;
            }
            else
            {
                checkBox1.Checked = true;
            }
            return rez;
        }
        public axVisUtils.Styles.objStylesM getStyles()
        {
            objStylesM st1 = new objStylesM();
            st1.SymbolStyle = createSymbol();
            st1.FontStyle = createFont();
            st1.PenStyle = createPen();
            st1.BrushStyle = createBrush();
            return st1;
        }
        private void panel1_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel1.BackColor;
            colorDialog1.ShowDialog();
            panel1.BackColor = colorDialog1.Color;
        }
        private void panel2_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel2.BackColor;
            colorDialog1.ShowDialog();
            panel2.BackColor = colorDialog1.Color;
        }
        private void panel3_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel3.BackColor;
            colorDialog1.ShowDialog();
            panel3.BackColor = colorDialog1.Color;
        }
        private void panel4_Click(object sender, EventArgs e)
        {
            if (checkBox1.Enabled && checkBox1.Checked)
            {
                colorDialog1.Color = panel4.BackColor;
                colorDialog1.ShowDialog();
                panel4.BackColor = colorDialog1.Color;
            }
        }
        private void panel5_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel5.BackColor;
            colorDialog1.ShowDialog();
            panel5.BackColor = colorDialog1.Color;
        }
        private void loadSymbol()
        {
            List<string> strs = new List<string>();
            comboBox2.Items.Clear();
            comboBox2.Font = new Font(fontDialog1.Font.Name, 14);
            for (int i = 32; i < 256; i++)
            {
                strs.Add(((char)i).ToString());
                //хотелось бы убрать символы, которые не определены в таблице символов
                //comboBox2.Items.Add(((char)i).ToString());
            }
            comboBox2.Items.AddRange(strs.ToArray());
            comboBox2.SelectedIndex = 3;
        }
        private void loadPenList()
        {
            listView1.Items.Clear();
            listView1.LabelEdit = false;
            listView1.AllowColumnReorder = false;
            listView1.CheckBoxes = false;
            listView1.FullRowSelect = false;
            listView1.GridLines = false;
            listView1.Sorting = SortOrder.Ascending;

            ImageList LargeImageList = new ImageList();
            LargeImageList.Images.Add("1", global::Rekod.lineRes.LINE01);
            LargeImageList.Images.Add("2", global::Rekod.lineRes.LINE02);
            LargeImageList.Images.Add("3", global::Rekod.lineRes.LINE03);
            LargeImageList.Images.Add("4", global::Rekod.lineRes.LINE04);
            LargeImageList.Images.Add("5", global::Rekod.lineRes.LINE05);
            LargeImageList.Images.Add("6", global::Rekod.lineRes.LINE06);
            LargeImageList.Images.Add("7", global::Rekod.lineRes.LINE07);
            LargeImageList.Images.Add("8", global::Rekod.lineRes.LINE08);
            LargeImageList.Images.Add("9", global::Rekod.lineRes.LINE09);
            LargeImageList.Images.Add("10", global::Rekod.lineRes.LINE10);
            LargeImageList.Images.Add("11", global::Rekod.lineRes.LINE11);
            LargeImageList.Images.Add("12", global::Rekod.lineRes.LINE12);
            LargeImageList.Images.Add("13", global::Rekod.lineRes.LINE13);
            LargeImageList.Images.Add("14", global::Rekod.lineRes.LINE14);
            LargeImageList.Images.Add("15", global::Rekod.lineRes.LINE15);
            LargeImageList.Images.Add("16", global::Rekod.lineRes.LINE16);
            LargeImageList.Images.Add("17", global::Rekod.lineRes.LINE17);
            LargeImageList.Images.Add("18", global::Rekod.lineRes.LINE18);
            LargeImageList.Images.Add("19", global::Rekod.lineRes.LINE19);
            LargeImageList.Images.Add("20", global::Rekod.lineRes.LINE20);
            LargeImageList.Images.Add("21", global::Rekod.lineRes.LINE21);
            LargeImageList.Images.Add("22", global::Rekod.lineRes.LINE22);
            LargeImageList.Images.Add("23", global::Rekod.lineRes.LINE23);
            LargeImageList.Images.Add("24", global::Rekod.lineRes.LINE24);
            LargeImageList.Images.Add("25", global::Rekod.lineRes.LINE25);
            LargeImageList.ImageSize = new Size(72, 12);
            listView1.SmallImageList = LargeImageList;
            listView1.Items.AddRange(new ListViewItem[] { new ListViewItem("", "1"),
                new ListViewItem("", "2"),
                new ListViewItem("", "3"),
                new ListViewItem("", "4"),
                new ListViewItem("", "5"),
                new ListViewItem("", "6"),
                new ListViewItem("", "7"),
                new ListViewItem("", "8"),
                new ListViewItem("", "9"),
                new ListViewItem("", "10"),
                new ListViewItem("", "11"),
                new ListViewItem("", "12"),
                new ListViewItem("", "13"),
                new ListViewItem("", "14"),
                new ListViewItem("", "15"),
                new ListViewItem("", "16"),
                new ListViewItem("", "17"),
                new ListViewItem("", "18"),
                new ListViewItem("", "19"),
                new ListViewItem("", "20"),
                new ListViewItem("", "21"),
                new ListViewItem("", "22"),
                new ListViewItem("", "23"),
                new ListViewItem("", "24"),
                new ListViewItem("", "25")});
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                listView1.Focus();
                listView1.Items[penIndex].Focused = true;
                listView1.Items[penIndex].Selected = true;
            }
            if (tabControl1.SelectedIndex == 2)
            { ibContainer.Focus(); ibContainer.setScrollPosition(); }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Selected == true)
                {
                    penIndex = i;
                    listView1.Items[i].Focused = true;
                }
            }
        }
        
        public static Color convColor(uint value)
        {
            Color c1;
            uint r = value << 24;
            r = r >> 24;
            uint g = value << 16;
            g = g >> 24;
            uint b = value << 8;
            b = b >> 24;
            int r1 = Convert.ToInt32(r), g1 = Convert.ToInt32(g), b1 = Convert.ToInt32(b);
            c1 = Color.FromArgb(r1, g1, b1);
            return c1;
        }

        private void tbOpacity_Scroll(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(tbOpacity, tbOpacity.Value.ToString() + "%");
        }
    }

    public enum GeomType { line, polygon, point }; 
}