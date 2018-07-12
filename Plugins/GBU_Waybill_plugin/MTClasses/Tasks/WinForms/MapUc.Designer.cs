namespace GBU_Waybill_plugin.MTClasses.Tasks.WinForms
{
    partial class MapUc
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
            this.maplibControl1 = new WrapperMaplib.MaplibControl();
            this.SuspendLayout();
            // 
            // maplibControl1
            // 
            this.maplibControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maplibControl1.EnableKeyDown = false;
            this.maplibControl1.EnableKeyPress = false;
            this.maplibControl1.EnableKeyUp = false;
            this.maplibControl1.EnableMouseDoubleClick = false;
            this.maplibControl1.EnableMouseDown = false;
            this.maplibControl1.EnableMouseMove = false;
            this.maplibControl1.EnableMouseUp = false;
            this.maplibControl1.EnableMouseWheel = false;
            this.maplibControl1.EnablePaint = false;
            this.maplibControl1.EnableResize = false;
            this.maplibControl1.Location = new System.Drawing.Point(0, 0);
            this.maplibControl1.Name = "maplibControl1";
            this.maplibControl1.Size = new System.Drawing.Size(514, 380);
            this.maplibControl1.TabIndex = 0;
            // 
            // MapUc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.maplibControl1);
            this.Name = "MapUc";
            this.Size = new System.Drawing.Size(514, 380);
            this.Load += new System.EventHandler(this.MapUc_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private WrapperMaplib.MaplibControl maplibControl1;
    }
}
