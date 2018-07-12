namespace GBU_Waybill_plugin
{
    partial class Form_mod_grounds
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.BTN_cancel = new System.Windows.Forms.Button();
            this.BTN_save = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.grounds = new System.Windows.Forms.TextBox();
            this.err_body = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BTN_cancel);
            this.panel1.Controls.Add(this.BTN_save);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(301, 50);
            this.panel1.TabIndex = 5;
            // 
            // BTN_cancel
            // 
            this.BTN_cancel.Location = new System.Drawing.Point(93, 13);
            this.BTN_cancel.Name = "BTN_cancel";
            this.BTN_cancel.Size = new System.Drawing.Size(75, 25);
            this.BTN_cancel.TabIndex = 6;
            this.BTN_cancel.Text = "Отменить";
            this.BTN_cancel.UseVisualStyleBackColor = true;
            // 
            // BTN_save
            // 
            this.BTN_save.Location = new System.Drawing.Point(12, 13);
            this.BTN_save.Name = "BTN_save";
            this.BTN_save.Size = new System.Drawing.Size(75, 25);
            this.BTN_save.TabIndex = 5;
            this.BTN_save.Text = "Сохранить";
            this.BTN_save.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.grounds);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 65);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(301, 66);
            this.panel2.TabIndex = 6;
            // 
            // grounds
            // 
            this.grounds.Location = new System.Drawing.Point(12, 7);
            this.grounds.Multiline = true;
            this.grounds.Name = "grounds";
            this.grounds.Size = new System.Drawing.Size(280, 53);
            this.grounds.TabIndex = 3;
            this.grounds.TabStop = false;
            // 
            // err_body
            // 
            this.err_body.AutoSize = true;
            this.err_body.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.err_body.Location = new System.Drawing.Point(54, 44);
            this.err_body.MaximumSize = new System.Drawing.Size(300, 300);
            this.err_body.MinimumSize = new System.Drawing.Size(210, 10);
            this.err_body.Name = "err_body";
            this.err_body.Size = new System.Drawing.Size(210, 13);
            this.err_body.TabIndex = 1;
            this.err_body.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.MaximumSize = new System.Drawing.Size(300, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(283, 26);
            this.label1.TabIndex = 2;
            this.label1.Text = "Значения следующих полей были изменены, укажите основание для изменений";
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.err_body);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(301, 65);
            this.panel3.TabIndex = 7;
            // 
            // Form_mod_grounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(301, 181);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form_mod_grounds";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Основание изменений";
            this.Load += new System.EventHandler(this.Form_mod_grounds_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BTN_cancel;
        private System.Windows.Forms.Button BTN_save;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox grounds;
        private System.Windows.Forms.Label err_body;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;

    }
}