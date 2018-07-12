namespace axVisUtils
{
    partial class FormTableData
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTableData));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new Rekod.FocusMenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.экспортВToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.импортИзToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.правкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.историяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.открытьМенеджерОтчетовToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.t_control = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            this.t_control.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            this.openFileDialog1.Multiselect = true;
            // 
            // saveFileDialog1
            // 
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // toolTip1
            // 
            this.toolTip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.toolTip1.OwnerDraw = true;
            this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.правкаToolStripMenuItem,
            this.отчетыToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            this.toolTip2.SetToolTip(this.menuStrip1, resources.GetString("menuStrip1.ToolTip"));
            this.toolTip1.SetToolTip(this.menuStrip1, resources.GetString("menuStrip1.ToolTip1"));
            // 
            // файлToolStripMenuItem
            // 
            resources.ApplyResources(this.файлToolStripMenuItem, "файлToolStripMenuItem");
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.экспортВToolStripMenuItem,
            this.импортИзToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            // 
            // экспортВToolStripMenuItem
            // 
            resources.ApplyResources(this.экспортВToolStripMenuItem, "экспортВToolStripMenuItem");
            this.экспортВToolStripMenuItem.Name = "экспортВToolStripMenuItem";
            this.экспортВToolStripMenuItem.Click += new System.EventHandler(this.экспортВToolStripMenuItem_Click);
            // 
            // импортИзToolStripMenuItem
            // 
            resources.ApplyResources(this.импортИзToolStripMenuItem, "импортИзToolStripMenuItem");
            this.импортИзToolStripMenuItem.Name = "импортИзToolStripMenuItem";
            this.импортИзToolStripMenuItem.Click += new System.EventHandler(this.импортИзToolStripMenuItem_Click);
            // 
            // правкаToolStripMenuItem
            // 
            resources.ApplyResources(this.правкаToolStripMenuItem, "правкаToolStripMenuItem");
            this.правкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.историяToolStripMenuItem,
            this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem});
            this.правкаToolStripMenuItem.Name = "правкаToolStripMenuItem";
            // 
            // историяToolStripMenuItem
            // 
            resources.ApplyResources(this.историяToolStripMenuItem, "историяToolStripMenuItem");
            this.историяToolStripMenuItem.Name = "историяToolStripMenuItem";
            this.историяToolStripMenuItem.Click += new System.EventHandler(this.историяToolStripMenuItem_Click);
            // 
            // показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem
            // 
            resources.ApplyResources(this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem, "показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem");
            this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem.Name = "показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem";
            this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem.Click += new System.EventHandler(this.показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem_Click);
            // 
            // отчетыToolStripMenuItem
            // 
            resources.ApplyResources(this.отчетыToolStripMenuItem, "отчетыToolStripMenuItem");
            this.отчетыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.открытьМенеджерОтчетовToolStripMenuItem});
            this.отчетыToolStripMenuItem.Name = "отчетыToolStripMenuItem";
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // открытьМенеджерОтчетовToolStripMenuItem
            // 
            resources.ApplyResources(this.открытьМенеджерОтчетовToolStripMenuItem, "открытьМенеджерОтчетовToolStripMenuItem");
            this.открытьМенеджерОтчетовToolStripMenuItem.Name = "открытьМенеджерОтчетовToolStripMenuItem";
            this.открытьМенеджерОтчетовToolStripMenuItem.Click += new System.EventHandler(this.открытьМенеджерОтчетовToolStripMenuItem_Click);
            // 
            // t_control
            // 
            resources.ApplyResources(this.t_control, "t_control");
            this.t_control.Controls.Add(this.tabPage1);
            this.t_control.Name = "t_control";
            this.t_control.SelectedIndex = 0;
            this.toolTip1.SetToolTip(this.t_control, resources.GetString("t_control.ToolTip"));
            this.toolTip2.SetToolTip(this.t_control, resources.GetString("t_control.ToolTip1"));
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.toolTip2.SetToolTip(this.tabPage1, resources.GetString("tabPage1.ToolTip"));
            this.toolTip1.SetToolTip(this.tabPage1, resources.GetString("tabPage1.ToolTip1"));
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // FormTableData
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.t_control);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormTableData";
            this.toolTip2.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip1"));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTableData_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormTableData_FormClosed);
            this.Load += new System.EventHandler(this.FormTableData_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.t_control.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem экспортВToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem импортИзToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem правкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem историяToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem;
        private Rekod.FocusMenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem отчетыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьМенеджерОтчетовToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TabControl t_control;
        private System.Windows.Forms.TabPage tabPage1;




    }
}