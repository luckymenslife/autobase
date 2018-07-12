namespace Rekod.HelpFiles
{
    partial class AboutBoxGS
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBoxGS));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.labelMaplibVersion = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.OkBtn = new System.Windows.Forms.Button();
            this.labelCopyright = new System.Windows.Forms.TextBox();
            this.labelProductName = new System.Windows.Forms.TextBox();
            this.lineSeparetor = new System.Windows.Forms.Label();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.labelDescription = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.labelMaplibVersion, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.labelCopyright, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.labelProductName, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.lineSeparetor, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.labelDescription, 0, 5);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // labelMaplibVersion
            // 
            this.labelMaplibVersion.BackColor = this.BackColor;
            this.labelMaplibVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelMaplibVersion, "labelMaplibVersion");
            this.labelMaplibVersion.Name = "labelMaplibVersion";
            this.labelMaplibVersion.ReadOnly = true;
            this.labelMaplibVersion.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.OkBtn, 2, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // OkBtn
            // 
            resources.ApplyResources(this.OkBtn, "OkBtn");
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // labelCopyright
            // 
            this.labelCopyright.BackColor = this.BackColor;
            this.labelCopyright.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelCopyright, "labelCopyright");
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.ReadOnly = true;
            this.labelCopyright.TabStop = false;
            // 
            // labelProductName
            // 
            this.labelProductName.BackColor = this.BackColor;
            this.labelProductName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.labelProductName, "labelProductName");
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.ReadOnly = true;
            this.labelProductName.TabStop = false;
            // 
            // lineSeparetor
            // 
            resources.ApplyResources(this.lineSeparetor, "lineSeparetor");
            this.lineSeparetor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lineSeparetor.Name = "lineSeparetor";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Image = global::Rekod.Properties.Resources.ГС_new_logo;
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // labelDescription
            // 
            resources.ApplyResources(this.labelDescription, "labelDescription");
            this.labelDescription.BackColor = this.BackColor;
            this.labelDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.ReadOnly = true;
            this.labelDescription.TabStop = false;
            // 
            // AboutBoxGS
            // 
            this.AcceptButton = this.OkBtn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBoxGS";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TextBox labelMaplibVersion;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.TextBox labelCopyright;
        private System.Windows.Forms.TextBox labelProductName;
        private System.Windows.Forms.Label lineSeparetor;
        private System.Windows.Forms.TextBox labelDescription;

    }
}
