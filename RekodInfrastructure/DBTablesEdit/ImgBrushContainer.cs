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
    public partial class ImgBrushContainer : UserControl, INotifyPropertyChanged
    {
        public ImgBrushContainer()
        {
            InitializeComponent();                       
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private ImgBrush _selectedBrush;
        public ImgBrush selectedBrush
        {
            get { return _selectedBrush; }
            set 
            {
                if(_selectedBrush!=null)
                    _selectedBrush.setDefaulBackground();
                if (value != null)
                {
                    if (value.style == 5 && value.hatch == 2)
                    {
                        value.style = 0;
                        value.hatch = 0;
                    }
                    if (value.style == 5 && value.hatch == 1)
                    {
                        value.style = 1;
                        value.hatch = 0;
                    }
                }
                _selectedBrush = value;
                if (_selectedBrush != null)
                    _selectedBrush.setSelectedBackground();
                OnPropertyChanged("selectedBrush");
            }
        }
        
        public void selectItem(int select_style, int select_hatch)
        {
            foreach (ImgBrush ib in Controls)
                if (ib.style == select_style && ib.hatch == select_hatch)
                    selectedBrush = ib;
        }

        private int x = 5, y = 5;
        private void positioned(ImgBrush ib)
        {
            //horizontal
            //Controls.Add(ib);
            //ib.Location = new Point(x, y);
            //y += ib.Height;
            //if(y>Height-ib.Height)
            //{ x += ib.Width; y = 2; }

            //vertical
            Controls.Add(ib);
            ib.Location = new Point(x, y);
            x += ib.Width+4;
            if (x > Width - ib.Width-10)
            { y += ib.Height+4; x = 5; }            
        }      

        public void setScrollPosition()
        {
            if (selectedBrush != null)
                AutoScrollPosition = new Point(AutoScrollPosition.X-20, selectedBrush.Location.Y);
        }        
    }
}
