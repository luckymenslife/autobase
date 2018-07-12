namespace GBU_Waybill_plugin.MTClasses.Tasks.WinForms
{
    partial class RoutesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoutesForm));
            this.maplibControl1 = new WrapperMaplib.MaplibControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.routesV1 = new GBU_Waybill_plugin.MTClasses.Tasks.Views.RoutesV();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // maplibControl1
            // 
            this.maplibControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.maplibControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maplibControl1.EnableKeyDown = true;
            this.maplibControl1.EnableKeyPress = true;
            this.maplibControl1.EnableKeyUp = true;
            this.maplibControl1.EnableMouseDoubleClick = true;
            this.maplibControl1.EnableMouseDown = true;
            this.maplibControl1.EnableMouseMove = true;
            this.maplibControl1.EnableMouseUp = true;
            this.maplibControl1.EnableMouseWheel = true;
            this.maplibControl1.EnablePaint = true;
            this.maplibControl1.EnableResize = true;
            this.maplibControl1.Location = new System.Drawing.Point(440, 0);
            this.maplibControl1.Name = "maplibControl1";
            this.maplibControl1.Size = new System.Drawing.Size(687, 611);
            this.maplibControl1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.elementHost1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(440, 611);
            this.panel1.TabIndex = 3;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(440, 611);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.routesV1;
            // 
            // RoutesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1127, 611);
            this.Controls.Add(this.maplibControl1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RoutesForm";
            this.Text = "Управление маршрутами";
            this.Load += new System.EventHandler(this.RoutesForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private WrapperMaplib.MaplibControl maplibControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private Views.RoutesV routesV1;
    }
}