namespace Rekod
{
    partial class mainFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainFrm));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.axMapLIb1 = new AxmvMapLib.AxMapLIb();
            this.layerItemsView1 = new Rekod.DBLayerForms.LayerItemsView();
            ((System.ComponentModel.ISupportInitialize)(this.axMapLIb1)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButton3, "toolStripButton3");
            this.toolStripButton3.Name = "toolStripButton3";
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            resources.ApplyResources(this.BottomToolStripPanel, "BottomToolStripPanel");
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.TopToolStripPanel.BackgroundImage = global::Rekod.Properties.Resources.bg_gorizontal;
            resources.ApplyResources(this.TopToolStripPanel, "TopToolStripPanel");
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            resources.ApplyResources(this.RightToolStripPanel, "RightToolStripPanel");
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            resources.ApplyResources(this.LeftToolStripPanel, "LeftToolStripPanel");
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // ContentPanel
            // 
            resources.ApplyResources(this.ContentPanel, "ContentPanel");
            // 
            // axMapLIb1
            // 
            resources.ApplyResources(this.axMapLIb1, "axMapLIb1");
            this.axMapLIb1.Name = "axMapLIb1";
            this.axMapLIb1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMapLIb1.OcxState")));
            this.axMapLIb1.OnDblClick += new System.EventHandler(this.axMapLIb1_OnDblClick);
            this.axMapLIb1.OnKeyPress += new AxmvMapLib.IMapLIbEvents_OnKeyPressEventHandler(this.axMapLIb1_OnKeyPress);
            this.axMapLIb1.ObjectSelected += new AxmvMapLib.IMapLIbEvents_ObjectSelectedEventHandler(this.axMapLIb1_ObjectSelected);
            this.axMapLIb1.ObjectUnselected += new AxmvMapLib.IMapLIbEvents_ObjectUnselectedEventHandler(this.axMapLIb1_ObjectUnselected);
            this.axMapLIb1.ObjectEdited += new AxmvMapLib.IMapLIbEvents_ObjectEditedEventHandler(this.axMapLIb1_ObjectEdited);
            this.axMapLIb1.MouseDownEvent += new AxmvMapLib.IMapLIbEvents_MouseDownEventHandler(this.axMapLIb1_MouseDownEvent_1);
            this.axMapLIb1.MouseMoveEvent += new AxmvMapLib.IMapLIbEvents_MouseMoveEventHandler(this.axMapLIb1_MouseMoveEvent);
            this.axMapLIb1.ObjectAfterCreate += new AxmvMapLib.IMapLIbEvents_ObjectAfterCreateEventHandler(this.axMapLIb1_ObjectAfterCreate);
            this.axMapLIb1.OnExtentChanged += new AxmvMapLib.IMapLIbEvents_OnExtentChangedEventHandler(this.axMapLIb1_OnExtentChanged);
            this.axMapLIb1.OnStatusInfoChanged += new AxmvMapLib.IMapLIbEvents_OnStatusInfoChangedEventHandler(this.axMapLIb1_OnStatusInfoChanged);
            // 
            // layerItemsView1
            // 
            this.layerItemsView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.layerItemsView1, "layerItemsView1");
            this.layerItemsView1.Name = "layerItemsView1";
            // 
            // mainFrm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axMapLIb1);
            this.Controls.Add(this.layerItemsView1);
            this.Name = "mainFrm";
            this.Tag = "0";
            ((System.ComponentModel.ISupportInitialize)(this.axMapLIb1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;


        //****************************************************
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        public AxmvMapLib.AxMapLIb axMapLIb1;
        public DBLayerForms.LayerItemsView layerItemsView1;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
    }
}

