namespace Rekod
{
    partial class ActionWithTables
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActionWithTables));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnGroups = new System.Windows.Forms.Button();
            this.btnProperties = new System.Windows.Forms.Button();
            this.btnStyle = new System.Windows.Forms.Button();
            this.btnCaptions = new System.Windows.Forms.Button();
            this.btnStruct = new System.Windows.Forms.Button();
            this.btnIndexation = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipTitle = "Операция с таблицей";
            // 
            // btnCopy
            // 
            resources.ApplyResources(this.btnCopy, "btnCopy");
            this.btnCopy.Image = global::Rekod.Properties.Resources.gnome_mime_text_x_copying;
            this.btnCopy.Name = "btnCopy";
            this.toolTip1.SetToolTip(this.btnCopy, resources.GetString("btnCopy.ToolTip"));
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnGroups
            // 
            resources.ApplyResources(this.btnGroups, "btnGroups");
            this.btnGroups.Image = global::Rekod.Properties.Resources.Group;
            this.btnGroups.Name = "btnGroups";
            this.toolTip1.SetToolTip(this.btnGroups, resources.GetString("btnGroups.ToolTip"));
            this.btnGroups.UseVisualStyleBackColor = true;
            this.btnGroups.Click += new System.EventHandler(this.btnGroups_Click);
            // 
            // btnProperties
            // 
            resources.ApplyResources(this.btnProperties, "btnProperties");
            this.btnProperties.Image = global::Rekod.Properties.Resources.edit_icon;
            this.btnProperties.Name = "btnProperties";
            this.toolTip1.SetToolTip(this.btnProperties, resources.GetString("btnProperties.ToolTip"));
            this.btnProperties.UseVisualStyleBackColor = true;
            this.btnProperties.Click += new System.EventHandler(this.btnProperties_Click);
            // 
            // btnStyle
            // 
            resources.ApplyResources(this.btnStyle, "btnStyle");
            this.btnStyle.Image = global::Rekod.Properties.Resources.instruments;
            this.btnStyle.Name = "btnStyle";
            this.toolTip1.SetToolTip(this.btnStyle, resources.GetString("btnStyle.ToolTip"));
            this.btnStyle.UseVisualStyleBackColor = true;
            this.btnStyle.Click += new System.EventHandler(this.btnStyle_Click);
            // 
            // btnCaptions
            // 
            resources.ApplyResources(this.btnCaptions, "btnCaptions");
            this.btnCaptions.Image = global::Rekod.Properties.Resources.kfontview;
            this.btnCaptions.Name = "btnCaptions";
            this.toolTip1.SetToolTip(this.btnCaptions, resources.GetString("btnCaptions.ToolTip"));
            this.btnCaptions.UseVisualStyleBackColor = true;
            this.btnCaptions.Click += new System.EventHandler(this.btnCaptions_Click);
            // 
            // btnStruct
            // 
            resources.ApplyResources(this.btnStruct, "btnStruct");
            this.btnStruct.Image = global::Rekod.Properties.Resources._struct;
            this.btnStruct.Name = "btnStruct";
            this.toolTip1.SetToolTip(this.btnStruct, resources.GetString("btnStruct.ToolTip"));
            this.btnStruct.UseVisualStyleBackColor = true;
            this.btnStruct.Click += new System.EventHandler(this.btnStruct_Click);
            // 
            // btnIndexation
            // 
            resources.ApplyResources(this.btnIndexation, "btnIndexation");
            this.btnIndexation.Image = global::Rekod.Properties.Resources.search1;
            this.btnIndexation.Name = "btnIndexation";
            this.toolTip1.SetToolTip(this.btnIndexation, resources.GetString("btnIndexation.ToolTip"));
            this.btnIndexation.UseVisualStyleBackColor = true;
            this.btnIndexation.Click += new System.EventHandler(this.btnIndexation_Click);
            // 
            // ActionWithTables
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnIndexation);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnGroups);
            this.Controls.Add(this.btnProperties);
            this.Controls.Add(this.btnStyle);
            this.Controls.Add(this.btnCaptions);
            this.Controls.Add(this.btnStruct);
            this.Name = "ActionWithTables";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCaptions;
        private System.Windows.Forms.Button btnStruct;
        private System.Windows.Forms.Button btnProperties;
        public System.Windows.Forms.Button btnStyle;
        public System.Windows.Forms.Button btnGroups;
        public System.Windows.Forms.Button btnCopy;
        public System.Windows.Forms.Button btnIndexation;

    }
}
