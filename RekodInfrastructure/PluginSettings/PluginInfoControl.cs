using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Rekod.PluginSettings
{
    public partial class PluginInfoControl : UserControl
    {
        public Interfaces.IMainPlugin plugin;

        public PluginInfoControl(string name, Interfaces.IMainPlugin plugin)
        {
            InitializeComponent();
            label1.Text = name;
            this.plugin = plugin;
        }
        private void PluginInfoControl_Enter(object sender, EventArgs e)
        {
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
        }
        private void PluginInfoControl_Leave(object sender, EventArgs e)
        {
            this.BackColor = System.Drawing.SystemColors.Control;
        }
        private void PluginInfoControl_MouseLeave(object sender, EventArgs e)
        {
            if (!this.Focused)
                this.BackColor = System.Drawing.SystemColors.Control;
        }
        private void PluginInfoControl_MouseEnter(object sender, EventArgs e)
        {
            if(!this.Focused)
                this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
        }

    }
}
