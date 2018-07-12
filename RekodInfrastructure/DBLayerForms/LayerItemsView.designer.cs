namespace Rekod.DBLayerForms
{
    partial class LayerItemsView
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerItemsView));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAddPointOnMap = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAddLineOnMap = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAddPolygonOnMap = new System.Windows.Forms.ToolStripMenuItem();
            this.addCoordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemSetExtent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsShowSettings = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.AllowDrop = true;
            this.treeView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView1.CheckBoxes = true;
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.treeView1.ForeColor = System.Drawing.Color.Black;
            this.treeView1.FullRowSelect = true;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.ItemHeight = 27;
            this.treeView1.LineColor = System.Drawing.Color.DarkGray;
            this.treeView1.Name = "treeView1";
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.ShowRootLines = false;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeCollapse);
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapseExpand);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapseExpand);
            this.treeView1.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView1_DrawNode);
            this.treeView1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeSelect);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            this.treeView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyUp);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "new lokalnii.png");
            this.imageList1.Images.SetKeyName(1, "rastrovii izobrageni9ya.png");
            this.imageList1.Images.SetKeyName(2, "new udalennii.png");
            this.imageList1.Images.SetKeyName(3, "точка1 серая.png");
            this.imageList1.Images.SetKeyName(4, "Площадь1 серая.png");
            this.imageList1.Images.SetKeyName(5, "КРИВАЯ1 серая.png");
            this.imageList1.Images.SetKeyName(6, "new udalennii.png");
            this.imageList1.Images.SetKeyName(7, "LayersManagerPoint.png");
            this.imageList1.Images.SetKeyName(8, "LayersManagerPolygon.png");
            this.imageList1.Images.SetKeyName(9, "LayersManagerPolygon(2).png");
            this.imageList1.Images.SetKeyName(10, "LayersManagerPolygon(3).png");
            this.imageList1.Images.SetKeyName(11, "LayersManagerPolygon(4).png");
            this.imageList1.Images.SetKeyName(12, "LayersManagerLine.png");
            this.imageList1.Images.SetKeyName(13, "NewLayersPodlojka.png");
            this.imageList1.Images.SetKeyName(14, "LayersManagerRastr.png");
            this.imageList1.Images.SetKeyName(15, "polylocked.png");
            this.imageList1.Images.SetKeyName(16, "polyunlocked.png");
            this.imageList1.Images.SetKeyName(17, "linelocked.png");
            this.imageList1.Images.SetKeyName(18, "lineunlocked.png");
            this.imageList1.Images.SetKeyName(19, "pointlocked.png");
            this.imageList1.Images.SetKeyName(20, "pointunlocked.png");
            this.imageList1.Images.SetKeyName(21, "LayersManagerBottomLayer.png");
            this.imageList1.Images.SetKeyName(22, "LayersManagerRastr.png");
            this.imageList1.Images.SetKeyName(23, "lineunlockedWithOutLocke.png");
            this.imageList1.Images.SetKeyName(24, "polyunlockedWithOutLocke.png");
            this.imageList1.Images.SetKeyName(25, "pointunlockedWithOutLocke.png");
            this.imageList1.Images.SetKeyName(26, "LineNewLocke.png");
            this.imageList1.Images.SetKeyName(27, "PointNewLocke.png");
            this.imageList1.Images.SetKeyName(28, "PolygonNewLocke.png");
            this.imageList1.Images.SetKeyName(29, "LineNewUnLocke.png");
            this.imageList1.Images.SetKeyName(30, "PointNewUnLocke.png");
            this.imageList1.Images.SetKeyName(31, "PolygonNewUnLocke.png");
            this.imageList1.Images.SetKeyName(32, "LayersManagerBottomLayerNew.png");
            this.imageList1.Images.SetKeyName(33, "LayersManagerRastrNew.png");
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.panel1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.panel1.Name = "panel1";
            this.panel1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDoubleClick);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.BackgroundImage = global::Rekod.Properties.Resources.обновить;
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            this.pictureBox2.MouseEnter += new System.EventHandler(this.pictureBox2_MouseEnter);
            this.pictureBox2.MouseLeave += new System.EventHandler(this.pictureBox2_MouseLeave);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Cursor = System.Windows.Forms.Cursors.SizeNESW;
            this.panel2.Name = "panel2";
            this.panel2.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDoubleClick);
            this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
            this.panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseUp);
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.treeView1);
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Name = "panel4";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.pictureBox1);
            this.panel3.Controls.Add(this.pictureBox2);
            this.panel3.Controls.Add(this.textBox2);
            this.panel3.Cursor = System.Windows.Forms.Cursors.Default;
            this.panel3.Name = "panel3";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::Rekod.Properties.Resources.cancel;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            // 
            // textBox2
            // 
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox2.ForeColor = System.Drawing.Color.Silver;
            this.textBox2.Name = "textBox2";
            this.textBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBox2_MouseClick);
            this.textBox2.Enter += new System.EventHandler(this.textBox2_Enter);
            this.textBox2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyUp);
            this.textBox2.Leave += new System.EventHandler(this.textBox2_Leave);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.UseWaitCursor = true;
            // 
            // tabPage2
            // 
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.UseWaitCursor = true;
            // 
            // tabPage3
            // 
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.tabPage3.UseWaitCursor = true;
            // 
            // tabPage4
            // 
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openTableToolStripMenuItem,
            this.tsAddPointOnMap,
            this.tsAddLineOnMap,
            this.tsAddPolygonOnMap,
            this.addCoordToolStripMenuItem,
            this.ToolStripMenuItemSetExtent,
            this.tsShowSettings});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            // 
            // openTableToolStripMenuItem
            // 
            resources.ApplyResources(this.openTableToolStripMenuItem, "openTableToolStripMenuItem");
            this.openTableToolStripMenuItem.Image = global::Rekod.Properties.Resources.object_List;
            this.openTableToolStripMenuItem.Name = "openTableToolStripMenuItem";
            this.openTableToolStripMenuItem.Click += new System.EventHandler(this.openTableToolStripMenuItem_Click);
            // 
            // tsAddPointOnMap
            // 
            resources.ApplyResources(this.tsAddPointOnMap, "tsAddPointOnMap");
            this.tsAddPointOnMap.Image = global::Rekod.Properties.Resources.AddPointOnMap;
            this.tsAddPointOnMap.Name = "tsAddPointOnMap";
            this.tsAddPointOnMap.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // tsAddLineOnMap
            // 
            resources.ApplyResources(this.tsAddLineOnMap, "tsAddLineOnMap");
            this.tsAddLineOnMap.Image = global::Rekod.Properties.Resources.AddLineOnMap;
            this.tsAddLineOnMap.Name = "tsAddLineOnMap";
            this.tsAddLineOnMap.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // tsAddPolygonOnMap
            // 
            resources.ApplyResources(this.tsAddPolygonOnMap, "tsAddPolygonOnMap");
            this.tsAddPolygonOnMap.Image = global::Rekod.Properties.Resources.AddPolygonOnMap;
            this.tsAddPolygonOnMap.Name = "tsAddPolygonOnMap";
            this.tsAddPolygonOnMap.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // addCoordToolStripMenuItem
            // 
            resources.ApplyResources(this.addCoordToolStripMenuItem, "addCoordToolStripMenuItem");
            this.addCoordToolStripMenuItem.Image = global::Rekod.Properties.Resources.addNotEnabledXY;
            this.addCoordToolStripMenuItem.Name = "addCoordToolStripMenuItem";
            this.addCoordToolStripMenuItem.Click += new System.EventHandler(this.addCoordToolStripMenuItem_Click);
            // 
            // ToolStripMenuItemSetExtent
            // 
            resources.ApplyResources(this.ToolStripMenuItemSetExtent, "ToolStripMenuItemSetExtent");
            this.ToolStripMenuItemSetExtent.Image = global::Rekod.Properties.Resources.ZoomToLayer;
            this.ToolStripMenuItemSetExtent.Name = "ToolStripMenuItemSetExtent";
            this.ToolStripMenuItemSetExtent.Click += new System.EventHandler(this.ToolStripMenuItemSetExtent_Click);
            // 
            // tsShowSettings
            // 
            resources.ApplyResources(this.tsShowSettings, "tsShowSettings");
            this.tsShowSettings.Name = "tsShowSettings";
            this.tsShowSettings.Click += new System.EventHandler(this.tsShowSettings_Click);
            // 
            // LayerItemsView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "LayerItemsView";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TabPage tabPage4;
        public System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        public System.Windows.Forms.ToolStripMenuItem openTableToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem addCoordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSetExtent;
        public System.Windows.Forms.ToolStripMenuItem tsAddPolygonOnMap;
        public System.Windows.Forms.ToolStripMenuItem tsAddPointOnMap;
        public System.Windows.Forms.ToolStripMenuItem tsAddLineOnMap;
        private System.Windows.Forms.ToolStripMenuItem tsShowSettings;
    }
}
