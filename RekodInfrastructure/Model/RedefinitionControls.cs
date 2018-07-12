using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Rekod.Model
{
    public class ToolStripNewButton : ToolStripLabel
    {
        bool isChecked = false;
        /// <summary>
        /// Показвает что объект выбираемый
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        bool isCheck = false;
        /// <summary>
        /// Показывает текущее выделение объекта
        /// </summary>
        public bool IsCheck
        {
            get { return (isChecked) ? isCheck : false; }
            set
            {
                ChangedIsCheckToolStripNewButtonEventArgs EventArgs = new ChangedIsCheckToolStripNewButtonEventArgs(isCheck, value);
                if (CheckChanged != null)
                    CheckChanged(this, EventArgs);
                if (EventArgs.Cancel == true) return;

                if (isCheck ^ value)
                {
                    isCheck = value;
                    SetBackgroundImage();
                    if (imageInvert != null)
                        if (value == true)
                            this.Image = imageInvert;
                        else
                            this.Image = imageCurrent;

                }
            }
        }

        public event EventHandler<ChangedIsCheckToolStripNewButtonEventArgs> CheckChanged;
        Image imageInvert = null;
        Image imageCurrent;
        public Image ImageInvert
        {
            get { return imageInvert; }
            set { imageInvert = value; }
        }

        bool isMouseEnter = false;
        bool IsMouseEnter
        {
            get { return isMouseEnter; }
            set
            {
                isMouseEnter = value;
                SetBackgroundImage();
            }
        }

        void SetBackgroundImage()
        {
            if (isCheck == true)
                this.BackgroundImage = Rekod.Properties.Resources.pri_navedenii2;
            else if (isMouseEnter == false)
                this.BackgroundImage = null;
            else
                this.BackgroundImage = Rekod.Properties.Resources.pri_vibore_instrumenta2;
        }

        protected override void OnClick(EventArgs e)
        {
            if (isChecked == true)
            {
                IsCheck = !IsCheck;
            }

            base.OnClick(e);
        }
        public override Image Image
        {
            get
            { return base.Image; }
            set
            { base.Image = value; if (imageCurrent == null) imageCurrent = value; }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            IsMouseEnter = false;
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            IsMouseEnter = true;
        }
    }

    public class ChangedIsCheckToolStripNewButtonEventArgs : EventArgs
    {
        public bool OldCheck { get; set; }
        public bool NewCheck { get; set; }
        public bool Cancel { get; set; }

        public ChangedIsCheckToolStripNewButtonEventArgs(bool oldCheck, bool newCheck)
        {
            Cancel = false;
            OldCheck = oldCheck;
            NewCheck = newCheck;
        }
    }
}
