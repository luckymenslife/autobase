namespace Rekod
{
    partial class AddEditTable
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEditTable));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.nBoundUp = new System.Windows.Forms.NumericUpDown();
            this.nBoundBotton = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.chbBounds = new System.Windows.Forms.CheckBox();
            this.cmbStyle = new System.Windows.Forms.ComboBox();
            this.cmbPhoto = new System.Windows.Forms.ComboBox();
            this.cmbTypeGeom = new System.Windows.Forms.ComboBox();
            this.cmbScheme = new System.Windows.Forms.ComboBox();
            this.cmbTypeTable = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtNameBD = new System.Windows.Forms.TextBox();
            this.txtNameText = new System.Windows.Forms.TextBox();
            this.chbVisable = new System.Windows.Forms.CheckBox();
            this.chbHistory = new System.Windows.Forms.CheckBox();
            this.chbHidden = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.nMinObjectSize = new System.Windows.Forms.NumericUpDown();
            this.ckbGraphicUnits = new System.Windows.Forms.CheckBox();
            this.chbNotDisplayWhenOpening = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbProj = new System.Windows.Forms.TextBox();
            this.bnSelectProj = new System.Windows.Forms.Button();
            this.cbGroups = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.nBoundUp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nBoundBotton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMinObjectSize)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.toolTip1.OwnerDraw = true;
            this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // nBoundUp
            // 
            resources.ApplyResources(this.nBoundUp, "nBoundUp");
            this.nBoundUp.Maximum = new decimal(new int[] {
            1783793664,
            116,
            0,
            0});
            this.nBoundUp.Name = "nBoundUp";
            // 
            // nBoundBotton
            // 
            resources.ApplyResources(this.nBoundBotton, "nBoundBotton");
            this.nBoundBotton.Maximum = new decimal(new int[] {
            1783793664,
            116,
            0,
            0});
            this.nBoundBotton.Name = "nBoundBotton";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.ForeColor = System.Drawing.Color.Maroon;
            this.label10.Name = "label10";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.ForeColor = System.Drawing.Color.Maroon;
            this.label9.Name = "label9";
            // 
            // chbBounds
            // 
            resources.ApplyResources(this.chbBounds, "chbBounds");
            this.tableLayoutPanel1.SetColumnSpan(this.chbBounds, 2);
            this.chbBounds.ForeColor = System.Drawing.Color.Maroon;
            this.chbBounds.Name = "chbBounds";
            this.chbBounds.UseVisualStyleBackColor = true;
            this.chbBounds.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // cmbStyle
            // 
            this.cmbStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStyle.FormattingEnabled = true;
            this.cmbStyle.Items.AddRange(new object[] {
            resources.GetString("cmbStyle.Items"),
            resources.GetString("cmbStyle.Items1")});
            resources.ApplyResources(this.cmbStyle, "cmbStyle");
            this.cmbStyle.Name = "cmbStyle";
            // 
            // cmbPhoto
            // 
            this.cmbPhoto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPhoto.FormattingEnabled = true;
            this.cmbPhoto.Items.AddRange(new object[] {
            resources.GetString("cmbPhoto.Items"),
            resources.GetString("cmbPhoto.Items1")});
            resources.ApplyResources(this.cmbPhoto, "cmbPhoto");
            this.cmbPhoto.Name = "cmbPhoto";
            // 
            // cmbTypeGeom
            // 
            this.cmbTypeGeom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypeGeom.FormattingEnabled = true;
            resources.ApplyResources(this.cmbTypeGeom, "cmbTypeGeom");
            this.cmbTypeGeom.Name = "cmbTypeGeom";
            this.cmbTypeGeom.SelectedIndexChanged += new System.EventHandler(this.cmbTypeGeom_SelectedIndexChanged);
            // 
            // cmbScheme
            // 
            this.cmbScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScheme.FormattingEnabled = true;
            resources.ApplyResources(this.cmbScheme, "cmbScheme");
            this.cmbScheme.Name = "cmbScheme";
            this.cmbScheme.SelectedIndexChanged += new System.EventHandler(this.cmbScheme_SelectedIndexChanged);
            // 
            // cmbTypeTable
            // 
            this.cmbTypeTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypeTable.FormattingEnabled = true;
            resources.ApplyResources(this.cmbTypeTable, "cmbTypeTable");
            this.cmbTypeTable.Name = "cmbTypeTable";
            this.cmbTypeTable.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.ForeColor = System.Drawing.Color.Maroon;
            this.label8.Name = "label8";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.ForeColor = System.Drawing.Color.Maroon;
            this.label6.Name = "label6";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.Color.Maroon;
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.ForeColor = System.Drawing.Color.Maroon;
            this.label5.Name = "label5";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.Color.Maroon;
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.Color.Maroon;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.Maroon;
            this.label1.Name = "label1";
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtNameBD
            // 
            resources.ApplyResources(this.txtNameBD, "txtNameBD");
            this.txtNameBD.Name = "txtNameBD";
            // 
            // txtNameText
            // 
            resources.ApplyResources(this.txtNameText, "txtNameText");
            this.txtNameText.Name = "txtNameText";
            // 
            // chbVisable
            // 
            resources.ApplyResources(this.chbVisable, "chbVisable");
            this.tableLayoutPanel1.SetColumnSpan(this.chbVisable, 2);
            this.chbVisable.ForeColor = System.Drawing.Color.Maroon;
            this.chbVisable.Name = "chbVisable";
            this.chbVisable.UseVisualStyleBackColor = true;
            // 
            // chbHistory
            // 
            resources.ApplyResources(this.chbHistory, "chbHistory");
            this.tableLayoutPanel1.SetColumnSpan(this.chbHistory, 2);
            this.chbHistory.ForeColor = System.Drawing.Color.Maroon;
            this.chbHistory.Name = "chbHistory";
            this.chbHistory.UseVisualStyleBackColor = true;
            // 
            // chbHidden
            // 
            resources.ApplyResources(this.chbHidden, "chbHidden");
            this.tableLayoutPanel1.SetColumnSpan(this.chbHidden, 2);
            this.chbHidden.ForeColor = System.Drawing.Color.Maroon;
            this.chbHidden.Name = "chbHidden";
            this.chbHidden.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.ForeColor = System.Drawing.Color.Maroon;
            this.label11.Name = "label11";
            // 
            // nMinObjectSize
            // 
            resources.ApplyResources(this.nMinObjectSize, "nMinObjectSize");
            this.nMinObjectSize.Name = "nMinObjectSize";
            // 
            // ckbGraphicUnits
            // 
            resources.ApplyResources(this.ckbGraphicUnits, "ckbGraphicUnits");
            this.tableLayoutPanel1.SetColumnSpan(this.ckbGraphicUnits, 2);
            this.ckbGraphicUnits.ForeColor = System.Drawing.Color.Maroon;
            this.ckbGraphicUnits.Name = "ckbGraphicUnits";
            this.ckbGraphicUnits.UseVisualStyleBackColor = true;
            // 
            // chbNotDisplayWhenOpening
            // 
            resources.ApplyResources(this.chbNotDisplayWhenOpening, "chbNotDisplayWhenOpening");
            this.tableLayoutPanel1.SetColumnSpan(this.chbNotDisplayWhenOpening, 2);
            this.chbNotDisplayWhenOpening.ForeColor = System.Drawing.Color.Maroon;
            this.chbNotDisplayWhenOpening.Name = "chbNotDisplayWhenOpening";
            this.chbNotDisplayWhenOpening.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.ForeColor = System.Drawing.Color.Maroon;
            this.label7.Name = "label7";
            // 
            // tbProj
            // 
            resources.ApplyResources(this.tbProj, "tbProj");
            this.tbProj.Name = "tbProj";
            this.tbProj.ReadOnly = true;
            // 
            // bnSelectProj
            // 
            resources.ApplyResources(this.bnSelectProj, "bnSelectProj");
            this.bnSelectProj.Name = "bnSelectProj";
            this.bnSelectProj.UseVisualStyleBackColor = true;
            this.bnSelectProj.Click += new System.EventHandler(this.bnSelectProj_Click);
            // 
            // cbGroups
            // 
            this.cbGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroups.FormattingEnabled = true;
            resources.ApplyResources(this.cbGroups, "cbGroups");
            this.cbGroups.Name = "cbGroups";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.ForeColor = System.Drawing.Color.Maroon;
            this.label12.Name = "label12";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.cbGroups, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.chbHidden, 0, 16);
            this.tableLayoutPanel1.Controls.Add(this.ckbGraphicUnits, 0, 13);
            this.tableLayoutPanel1.Controls.Add(this.chbHistory, 0, 15);
            this.tableLayoutPanel1.Controls.Add(this.cmbPhoto, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.chbVisable, 0, 14);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmbStyle, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.nBoundUp, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.cmbScheme, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.nBoundBotton, 1, 11);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chbBounds, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtNameBD, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtNameText, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmbTypeTable, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.cmbTypeGeom, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.chbNotDisplayWhenOpening, 0, 17);
            this.tableLayoutPanel1.Controls.Add(this.nMinObjectSize, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.label12, 0, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbProj);
            this.panel1.Controls.Add(this.bnSelectProj);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // AddEditTable
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "AddEditTable";
            ((System.ComponentModel.ISupportInitialize)(this.nBoundUp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nBoundBotton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMinObjectSize)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.NumericUpDown nBoundUp;
        private System.Windows.Forms.NumericUpDown nBoundBotton;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chbBounds;
        private System.Windows.Forms.ComboBox cmbStyle;
        private System.Windows.Forms.ComboBox cmbPhoto;
        private System.Windows.Forms.ComboBox cmbTypeGeom;
        private System.Windows.Forms.ComboBox cmbScheme;
        private System.Windows.Forms.ComboBox cmbTypeTable;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtNameBD;
        private System.Windows.Forms.TextBox txtNameText;
        private System.Windows.Forms.CheckBox chbVisable;
        private System.Windows.Forms.CheckBox chbHistory;
        private System.Windows.Forms.CheckBox chbHidden;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown nMinObjectSize;
        private System.Windows.Forms.CheckBox ckbGraphicUnits;
        private System.Windows.Forms.CheckBox chbNotDisplayWhenOpening;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbProj;
        private System.Windows.Forms.Button bnSelectProj;
        private System.Windows.Forms.ComboBox cbGroups;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
    }
}
