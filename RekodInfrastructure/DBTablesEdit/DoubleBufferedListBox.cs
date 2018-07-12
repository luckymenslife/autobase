using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Rekod
{
    public sealed class DoubleBufferedListBox : ListBox
    {
        /*protected override CreateParams CreateParams
        {
            get
            {
                CreateParams result = base.CreateParams;
                result.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return result;
            }
        }*/
        //public List<itemColor> itemColors;
        public DoubleBufferedListBox()
        {
            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            UpdateStyles();
        }
        /*
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            itemColor cur = new itemColor();
            if(itemColors!=null)
            foreach (itemColor ic in itemColors)
            {
                if (e.Index == ic.index)
                {
                    cur = ic;
                    break;
                }
            }

            e.DrawBackground();
            Graphics g = e.Graphics;
            if (cur.index != -1)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255 - cur.color, 255 - cur.color)), e.Bounds);
                g.DrawString(this.Items[e.Index].ToString(), this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }
            else
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(51, 153, 255)), e.Bounds);
                    g.DrawString(this.Items[e.Index].ToString(), this.Font, Brushes.White, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                }
                else
                {
                    g.DrawString(this.Items[e.Index].ToString(), this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                }

            // Print text
            e.DrawFocusRectangle();

            base.OnDrawItem(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Region iRegion = new Region(e.ClipRectangle);
            e.Graphics.FillRegion(new SolidBrush(this.BackColor), iRegion);
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.Items.Count; ++i)
                {
                    System.Drawing.Rectangle irect = this.GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        if ((this.SelectionMode == SelectionMode.One && this.SelectedIndex == i)
                        || (this.SelectionMode == SelectionMode.MultiSimple && this.SelectedIndices.Contains(i))
                        || (this.SelectionMode == SelectionMode.MultiExtended && this.SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Selected, this.ForeColor,
                                this.BackColor));
                        }
                        else
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Default, this.ForeColor,
                                this.BackColor));
                        }
                        iRegion.Complement(irect);
                    }
                }
            }
            base.OnPaint(e);
        }
        /*#region Method Overrides
        /// <summary>
        /// Override OnTemplateListDrawItem to supply an off-screen buffer to event
        /// handlers.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;

            Rectangle newBounds = new Rectangle(0, 0, e.Bounds.Width, e.Bounds.Height);
            using (BufferedGraphics bufferedGraphics = currentContext.Allocate(e.Graphics, newBounds))
            {
                DrawItemEventArgs newArgs = new DrawItemEventArgs(
                    bufferedGraphics.Graphics, e.Font, newBounds, e.Index, e.State, e.ForeColor, e.BackColor);

                // Supply the real OnTemplateListDrawItem with the off-screen graphics context
                base.OnDrawItem(newArgs);

                // Wrapper around BitBlt
                //GDI.CopyGraphics(e.Graphics, e.Bounds, bufferedGraphics.Graphics, new Point(0, 0));
            }
        }
        #endregion*/
    }
}
