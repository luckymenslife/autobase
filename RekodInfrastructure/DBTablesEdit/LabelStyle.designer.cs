namespace Rekod.DBTablesEdit
{
    partial class LabelStyle
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabelStyle));
            this.gb_ViewModes = new System.Windows.Forms.GroupBox();
            this.numeric_offSet = new System.Windows.Forms.NumericUpDown();
            this.numeric_maxScale = new System.Windows.Forms.NumericUpDown();
            this.numeric_minScale = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.check_useBounds = new System.Windows.Forms.CheckBox();
            this.check_overLap = new System.Windows.Forms.CheckBox();
            this.check_alongLines = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel_frameColor = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.check_showFrame = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numeric_fontSize = new System.Windows.Forms.NumericUpDown();
            this.button_showFontDialog = new System.Windows.Forms.Button();
            this.check_graphicUnits = new System.Windows.Forms.CheckBox();
            this.panel_fontColor = new System.Windows.Forms.Panel();
            this.text_fontName = new System.Windows.Forms.TextBox();
            this.button_save = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.panel_main = new System.Windows.Forms.Panel();
            this.gb_ViewModes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_offSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_maxScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_minScale)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_fontSize)).BeginInit();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // gb_ViewModes
            // 
            this.gb_ViewModes.Controls.Add(this.numeric_offSet);
            this.gb_ViewModes.Controls.Add(this.numeric_maxScale);
            this.gb_ViewModes.Controls.Add(this.numeric_minScale);
            this.gb_ViewModes.Controls.Add(this.label3);
            this.gb_ViewModes.Controls.Add(this.label2);
            this.gb_ViewModes.Controls.Add(this.label1);
            this.gb_ViewModes.Controls.Add(this.check_useBounds);
            this.gb_ViewModes.Controls.Add(this.check_overLap);
            this.gb_ViewModes.Controls.Add(this.check_alongLines);
            resources.ApplyResources(this.gb_ViewModes, "gb_ViewModes");
            this.gb_ViewModes.Name = "gb_ViewModes";
            this.gb_ViewModes.TabStop = false;
            // 
            // numeric_offSet
            // 
            resources.ApplyResources(this.numeric_offSet, "numeric_offSet");
            this.numeric_offSet.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric_offSet.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.numeric_offSet.Name = "numeric_offSet";
            this.numeric_offSet.ValueChanged += new System.EventHandler(this.numeric_offSet_ValueChanged);
            // 
            // numeric_maxScale
            // 
            resources.ApplyResources(this.numeric_maxScale, "numeric_maxScale");
            this.numeric_maxScale.Maximum = new decimal(new int[] {
            -64771072,
            6,
            0,
            0});
            this.numeric_maxScale.Name = "numeric_maxScale";
            this.numeric_maxScale.ValueChanged += new System.EventHandler(this.numeric_maxScale_ValueChanged);
            // 
            // numeric_minScale
            // 
            resources.ApplyResources(this.numeric_minScale, "numeric_minScale");
            this.numeric_minScale.Maximum = new decimal(new int[] {
            -64771072,
            6,
            0,
            0});
            this.numeric_minScale.Name = "numeric_minScale";
            this.numeric_minScale.ValueChanged += new System.EventHandler(this.numeric_minScale_ValueChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // check_useBounds
            // 
            resources.ApplyResources(this.check_useBounds, "check_useBounds");
            this.check_useBounds.Name = "check_useBounds";
            this.check_useBounds.UseVisualStyleBackColor = true;
            this.check_useBounds.CheckedChanged += new System.EventHandler(this.check_useBounds_CheckedChanged);
            // 
            // check_overLap
            // 
            resources.ApplyResources(this.check_overLap, "check_overLap");
            this.check_overLap.Name = "check_overLap";
            this.check_overLap.UseVisualStyleBackColor = true;
            this.check_overLap.CheckedChanged += new System.EventHandler(this.check_overLap_CheckedChanged);
            // 
            // check_alongLines
            // 
            resources.ApplyResources(this.check_alongLines, "check_alongLines");
            this.check_alongLines.Name = "check_alongLines";
            this.check_alongLines.UseVisualStyleBackColor = true;
            this.check_alongLines.CheckedChanged += new System.EventHandler(this.check_alongLines_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel_frameColor);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.check_showFrame);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // panel_frameColor
            // 
            this.panel_frameColor.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.panel_frameColor, "panel_frameColor");
            this.panel_frameColor.Name = "panel_frameColor";
            this.panel_frameColor.Click += new System.EventHandler(this.panel_frameColor_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // check_showFrame
            // 
            resources.ApplyResources(this.check_showFrame, "check_showFrame");
            this.check_showFrame.Name = "check_showFrame";
            this.check_showFrame.UseVisualStyleBackColor = true;
            this.check_showFrame.CheckedChanged += new System.EventHandler(this.check_showFrame_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numeric_fontSize);
            this.groupBox2.Controls.Add(this.button_showFontDialog);
            this.groupBox2.Controls.Add(this.check_graphicUnits);
            this.groupBox2.Controls.Add(this.panel_fontColor);
            this.groupBox2.Controls.Add(this.text_fontName);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // numeric_fontSize
            // 
            resources.ApplyResources(this.numeric_fontSize, "numeric_fontSize");
            this.numeric_fontSize.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numeric_fontSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_fontSize.Name = "numeric_fontSize";
            this.numeric_fontSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_fontSize.ValueChanged += new System.EventHandler(this.numeric_fontSize_ValueChanged);
            // 
            // button_showFontDialog
            // 
            resources.ApplyResources(this.button_showFontDialog, "button_showFontDialog");
            this.button_showFontDialog.Name = "button_showFontDialog";
            this.button_showFontDialog.UseVisualStyleBackColor = true;
            this.button_showFontDialog.Click += new System.EventHandler(this.button_showFondDialog);
            // 
            // check_graphicUnits
            // 
            resources.ApplyResources(this.check_graphicUnits, "check_graphicUnits");
            this.check_graphicUnits.Name = "check_graphicUnits";
            this.check_graphicUnits.UseVisualStyleBackColor = true;
            this.check_graphicUnits.CheckedChanged += new System.EventHandler(this.check_graphicUnits_CheckedChanged);
            // 
            // panel_fontColor
            // 
            this.panel_fontColor.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.panel_fontColor, "panel_fontColor");
            this.panel_fontColor.Name = "panel_fontColor";
            this.panel_fontColor.Click += new System.EventHandler(this.panel_fontColor_Click);
            // 
            // text_fontName
            // 
            resources.ApplyResources(this.text_fontName, "text_fontName");
            this.text_fontName.Name = "text_fontName";
            // 
            // button_save
            // 
            resources.ApplyResources(this.button_save, "button_save");
            this.button_save.Name = "button_save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // panel_main
            // 
            this.panel_main.Controls.Add(this.groupBox2);
            this.panel_main.Controls.Add(this.gb_ViewModes);
            this.panel_main.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.panel_main, "panel_main");
            this.panel_main.Name = "panel_main";
            // 
            // LabelStyle
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button_save);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "LabelStyle";
            this.Load += new System.EventHandler(this.LabelStyle_Load);
            this.gb_ViewModes.ResumeLayout(false);
            this.gb_ViewModes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_offSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_maxScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_minScale)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_fontSize)).EndInit();
            this.panel_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gb_ViewModes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox check_useBounds;
        private System.Windows.Forms.CheckBox check_overLap;
        private System.Windows.Forms.CheckBox check_alongLines;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel_frameColor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox check_showFrame;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_showFontDialog;
        private System.Windows.Forms.CheckBox check_graphicUnits;
        private System.Windows.Forms.Panel panel_fontColor;
        private System.Windows.Forms.TextBox text_fontName;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown numeric_offSet;
        private System.Windows.Forms.NumericUpDown numeric_maxScale;
        private System.Windows.Forms.NumericUpDown numeric_minScale;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.NumericUpDown numeric_fontSize;
    }
}