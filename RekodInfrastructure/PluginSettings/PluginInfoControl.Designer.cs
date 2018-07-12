namespace Rekod.PluginSettings
{
    partial class PluginInfoControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Название";
            this.label1.MouseLeave += new System.EventHandler(this.PluginInfoControl_MouseLeave);
            this.label1.Leave += new System.EventHandler(this.PluginInfoControl_Leave);
            this.label1.Enter += new System.EventHandler(this.PluginInfoControl_Enter);
            this.label1.MouseEnter += new System.EventHandler(this.PluginInfoControl_MouseEnter);
            // 
            // PluginInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "PluginInfoControl";
            this.Size = new System.Drawing.Size(246, 36);
            this.MouseLeave += new System.EventHandler(this.PluginInfoControl_MouseLeave);
            this.Leave += new System.EventHandler(this.PluginInfoControl_Leave);
            this.Enter += new System.EventHandler(this.PluginInfoControl_Enter);
            this.MouseEnter += new System.EventHandler(this.PluginInfoControl_MouseEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label label1;
    }
}
