namespace Rekod
{
    partial class FindBox2
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
            this.cbListColumn = new System.Windows.Forms.ComboBox();
            this.btnAddRemoveFilter = new System.Windows.Forms.Button();
            this.cbListFilter = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnSearch = new System.Windows.Forms.Button();
            this.pValue = new System.Windows.Forms.Panel();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.pValue.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbListColumn
            // 
            this.cbListColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbListColumn.FormattingEnabled = true;
            this.cbListColumn.Location = new System.Drawing.Point(33, 2);
            this.cbListColumn.Name = "cbListColumn";
            this.cbListColumn.Size = new System.Drawing.Size(145, 21);
            this.cbListColumn.TabIndex = 8;
            // 
            // btnAddRemoveFilter
            // 
            this.btnAddRemoveFilter.BackgroundImage = global::Rekod.Properties.Resources.add;
            this.btnAddRemoveFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddRemoveFilter.Location = new System.Drawing.Point(4, 1);
            this.btnAddRemoveFilter.Name = "btnAddRemoveFilter";
            this.btnAddRemoveFilter.Size = new System.Drawing.Size(24, 24);
            this.btnAddRemoveFilter.TabIndex = 10;
            this.btnAddRemoveFilter.UseVisualStyleBackColor = true;
            this.btnAddRemoveFilter.Click += new System.EventHandler(this.btnAddRemoveFilter_Click);
            // 
            // cbListFilter
            // 
            this.cbListFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbListFilter.FormattingEnabled = true;
            this.cbListFilter.Items.AddRange(new object[] {
            "Входит",
            "Входит сначала",
            "Не входит"});
            this.cbListFilter.Location = new System.Drawing.Point(184, 2);
            this.cbListFilter.Name = "cbListFilter";
            this.cbListFilter.Size = new System.Drawing.Size(102, 21);
            this.cbListFilter.TabIndex = 11;
            // 
            // btnSearch
            // 
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSearch.Image = global::Rekod.Properties.Resources.search;
            this.btnSearch.Location = new System.Drawing.Point(516, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(33, 20);
            this.btnSearch.TabIndex = 9;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // pValue
            // 
            this.pValue.Controls.Add(this.dateTimePicker1);
            this.pValue.Controls.Add(this.txtValue);
            this.pValue.Location = new System.Drawing.Point(293, 3);
            this.pValue.Name = "pValue";
            this.pValue.Size = new System.Drawing.Size(217, 20);
            this.pValue.TabIndex = 12;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker1.Location = new System.Drawing.Point(0, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(217, 20);
            this.dateTimePicker1.TabIndex = 1;
            this.dateTimePicker1.Visible = false;
            this.dateTimePicker1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Value_KeyUp);
            // 
            // txtValue
            // 
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.Location = new System.Drawing.Point(0, 0);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(217, 20);
            this.txtValue.TabIndex = 0;
            this.txtValue.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Value_KeyUp);
            // 
            // FindBox2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pValue);
            this.Controls.Add(this.cbListFilter);
            this.Controls.Add(this.btnAddRemoveFilter);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.cbListColumn);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FindBox2";
            this.Size = new System.Drawing.Size(552, 25);
            this.Load += new System.EventHandler(this.FindBox2_Load);
            this.pValue.ResumeLayout(false);
            this.pValue.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbListColumn;
        private System.Windows.Forms.Button btnAddRemoveFilter;
        private System.Windows.Forms.ComboBox cbListFilter;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Panel pValue;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
    }
}
