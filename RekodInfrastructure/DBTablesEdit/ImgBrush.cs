using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rekod
{
    public partial class ImgBrush : UserControl
    {
        public int style,hatch;
        public Image img;
        private Color selCol = Color.Blue;
        private Color defCol = Color.White;
        public ImgBrush(int style, int hatch, Image img)
        {
            this.style = style;
            this.hatch = hatch;
            this.img = img;
            InitializeComponent();
            pb.Image = img;
        }

        private void pb_MouseEnter(object sender, EventArgs e)
        {
            if (BackColor != selCol)
                BackColor = Color.Gray;
        }

        private void pb_MouseLeave(object sender, EventArgs e)
        {
            if (BackColor != selCol)
                BackColor = defCol;
        }

        private void pb_Click(object sender, EventArgs e)
        {
            if (Parent is ImgBrushContainer)
                (Parent as ImgBrushContainer).selectedBrush = this;
        }

        internal void setDefaulBackground()
        {
            BackColor = defCol;
        }

        internal void setSelectedBackground()
        {
            BackColor = selCol;
        }

        private void pb_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                MessageBox.Show("s="+style+"_h="+hatch);
        }
    }
}
