namespace Rekod
{
    partial class EditGroupsOrder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditGroupsOrder));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonEditNameDesc = new System.Windows.Forms.Button();
            this.buttonBringDown = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.buttonBringUp = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            resources.ApplyResources(this.listBox1, "listBox1");
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Name = "listBox1";
            this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDoubleClick);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.Maroon;
            this.label1.Name = "label1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelCount);
            this.groupBox1.Controls.Add(this.buttonClose);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonEditNameDesc);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.buttonBringDown);
            this.groupBox1.Controls.Add(this.addButton);
            this.groupBox1.Controls.Add(this.buttonBringUp);
            this.groupBox1.Controls.Add(this.deleteButton);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // labelCount
            // 
            resources.ApplyResources(this.labelCount, "labelCount");
            this.labelCount.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.labelCount.Name = "labelCount";
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.BackgroundImage = global::Rekod.Properties.Resources.delete;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonEditNameDesc
            // 
            resources.ApplyResources(this.buttonEditNameDesc, "buttonEditNameDesc");
            this.buttonEditNameDesc.BackgroundImage = global::Rekod.Properties.Resources.edit_icon;
            this.buttonEditNameDesc.Name = "buttonEditNameDesc";
            this.toolTip1.SetToolTip(this.buttonEditNameDesc, resources.GetString("buttonEditNameDesc.ToolTip"));
            this.buttonEditNameDesc.UseVisualStyleBackColor = true;
            this.buttonEditNameDesc.Click += new System.EventHandler(this.buttonEditNameDesc_Click);
            // 
            // buttonBringDown
            // 
            resources.ApplyResources(this.buttonBringDown, "buttonBringDown");
            this.buttonBringDown.BackgroundImage = global::Rekod.Properties.Resources._1downarrow1;
            this.buttonBringDown.Name = "buttonBringDown";
            this.toolTip1.SetToolTip(this.buttonBringDown, resources.GetString("buttonBringDown.ToolTip"));
            this.buttonBringDown.UseVisualStyleBackColor = true;
            this.buttonBringDown.Click += new System.EventHandler(this.buttonBringDown_Click);
            // 
            // addButton
            // 
            resources.ApplyResources(this.addButton, "addButton");
            this.addButton.BackgroundImage = global::Rekod.Properties.Resources.plus;
            this.addButton.Name = "addButton";
            this.toolTip1.SetToolTip(this.addButton, resources.GetString("addButton.ToolTip"));
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // buttonBringUp
            // 
            resources.ApplyResources(this.buttonBringUp, "buttonBringUp");
            this.buttonBringUp.BackgroundImage = global::Rekod.Properties.Resources._1downarrow;
            this.buttonBringUp.Name = "buttonBringUp";
            this.toolTip1.SetToolTip(this.buttonBringUp, resources.GetString("buttonBringUp.ToolTip"));
            this.buttonBringUp.UseVisualStyleBackColor = true;
            this.buttonBringUp.Click += new System.EventHandler(this.buttonBringUp_Click);
            // 
            // deleteButton
            // 
            resources.ApplyResources(this.deleteButton, "deleteButton");
            this.deleteButton.BackgroundImage = global::Rekod.Properties.Resources.minus;
            this.deleteButton.Name = "deleteButton";
            this.toolTip1.SetToolTip(this.deleteButton, resources.GetString("deleteButton.ToolTip"));
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // EditGroupsOrder
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.groupBox1);
            this.Name = "EditGroupsOrder";
            this.Load += new System.EventHandler(this.EditGroupsOrder_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button buttonBringDown;
        private System.Windows.Forms.Button buttonBringUp;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonEditNameDesc;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
