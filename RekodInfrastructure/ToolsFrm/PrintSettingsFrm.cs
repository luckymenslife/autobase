using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using Rekod.Properties;

namespace Rekod.ToolsFrm
{
    public partial class PrintSettingsFrm : Form
    {
        private PrintSettings _printSettings;
        public PrintSettingsFrm()
        {
            InitializeComponent();


            _printSettings = new PrintSettings();
            cmbPaperFormat.Items.Clear();
            cmbPaperFormat.Items.Add(new PaperFormat("A0", 1189, 841));
            cmbPaperFormat.Items.Add(new PaperFormat("A1", 841, 594));
            cmbPaperFormat.Items.Add(new PaperFormat("A2", 594, 420));
            cmbPaperFormat.Items.Add(new PaperFormat("A3", 420, 297));
            cmbPaperFormat.Items.Add(new PaperFormat("A4", 297, 210));
            cmbPaperFormat.Items.Add(new PaperFormat("A5", 210, 148));
            cmbPaperFormat.Items.Add(new PaperFormat("A6", 148, 105));
            cmbPaperFormat.Items.Add(new PaperFormat("A7", 105, 74));
            cmbPaperFormat.Items.Add(new PaperFormat("A8", 74, 52));
            cmbPaperFormat.Items.Add(new PaperFormat("A9", 52, 37));
            cmbPaperFormat.Items.Add(new PaperFormat("A10", 37, 26));
            cmbPaperFormat.Items.Add(new PaperFormat(Resources.PrintSettingsFrm_PaperFormat_Random, -1, -1));

            cmbOrientation.Items.Clear();
            cmbOrientation.Items.Add(Resources.PrintSettingsFrm_Portrait);
            cmbOrientation.Items.Add(Resources.PrintSettingsFrm_Landscape);

            cmbQuality.Items.Clear();
            cmbQuality.Items.Add(new ImageQuality(Resources.PrintSettingsFrm_Bad, 75));
            cmbQuality.Items.Add(new ImageQuality(Resources.PrintSettingsFrm_Good, 150));
            cmbQuality.Items.Add(new ImageQuality(Resources.PrintSettingsFrm_Excellent, 300));
            cmbQuality.Items.Add(new ImageQuality(Resources.PrintSettingsFrm_Random, -1));


            cmbPaperFormat.SelectedIndex = 0;
            cmbOrientation.SelectedIndex = 0;
            cmbQuality.SelectedIndex = 1;

            ShowImageSize();

        }
        public PrintSettings MyPrintSetting
        {
            get { return _printSettings; }
        }

        #region Методы
        private void ShowImageSize()
        {
            Size s = _printSettings.GetSizeImage();
            tlsRezultLabel.Text = s.Width.ToString() + "x" + s.Height.ToString();
        }
        private void cmbPaperFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPaperFormat.SelectedItem != null)
            {
                if (cmbPaperFormat.SelectedIndex == 11)
                {
                    nudHeight.Enabled = true;
                    nudWidth.Enabled = true;
                    cmbOrientation.Enabled = false;
                    cmbOrientation.SelectedIndex = 0;

                    _printSettings.PaperHeight = Convert.ToDouble(nudHeight.Value);
                    _printSettings.PaperWidth = Convert.ToDouble(nudWidth.Value);
                }
                else
                {
                    nudHeight.Enabled = false;
                    nudWidth.Enabled = false;
                    cmbOrientation.Enabled = true;

                    _printSettings.PaperHeight = ((PaperFormat)cmbPaperFormat.SelectedItem).Height;
                    nudHeight.Value = Convert.ToDecimal(_printSettings.PaperHeight);
                    _printSettings.PaperWidth = ((PaperFormat)cmbPaperFormat.SelectedItem).Width;
                    nudWidth.Value = Convert.ToDecimal(_printSettings.PaperWidth);
                }
                ShowImageSize();
            }
        }
        private void cmbOrientation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOrientation.SelectedItem != null)
            {
                if (cmbOrientation.SelectedIndex == 0)
                    _printSettings.isLandscape = false;
                else
                    _printSettings.isLandscape = true;

                ShowImageSize();
            }
        }
        private void cmbQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbQuality.SelectedItem != null)
            {
                if (cmbQuality.SelectedIndex == 3)
                {
                    nudQuality.Enabled = true;

                    _printSettings.dpiCount = Convert.ToInt32(nudQuality.Value);
                }
                else
                {
                    nudQuality.Enabled = false;

                    _printSettings.dpiCount = ((ImageQuality)cmbQuality.SelectedItem).dpi;
                    nudQuality.Value = Convert.ToDecimal(_printSettings.dpiCount);
                }
                ShowImageSize();
            }
        }
        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            _printSettings.PaperWidth = Convert.ToDouble(nudWidth.Value);
            ShowImageSize();
        }
        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {
            _printSettings.PaperHeight = Convert.ToDouble(nudHeight.Value);
            ShowImageSize();
        }
        private void nudQuality_ValueChanged(object sender, EventArgs e)
        {
            _printSettings.dpiCount = Convert.ToInt32(nudQuality.Value);
            ShowImageSize();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
        #endregion

        #region Классы
        public class PrintSettings
        {
            #region Property
            public double PaperHeight
            {
                get;
                set;
            }
            public double PaperWidth
            {
                get;
                set;
            }
            public bool isLandscape
            {
                get;
                set;
            }
            public int dpiCount
            {
                get;
                set;
            }
            #endregion
            #region Methods
            public Size GetSizeImage()
            {
                int _h = 0, _w = 0;
                _h = Convert.ToInt32((PaperHeight / 25.4) * dpiCount);
                _w = Convert.ToInt32((PaperWidth / 25.4) * dpiCount);
                if (isLandscape)
                    return new Size(_h, _w);
                else
                    return new Size(_w, _h);
            }
            public PageSettings GetPageSettings()
            {
                PageSettings settings = new PageSettings();
                Size s = GetSizeImage();
                settings.PaperSize = new PaperSize("my settings", s.Width, s.Height);
                settings.PrinterResolution = new PrinterResolution();
                settings.PrinterResolution.Kind = PrinterResolutionKind.Custom;
                settings.PrinterResolution.X = dpiCount;
                settings.PrinterResolution.Y = dpiCount;
                settings.Landscape = isLandscape;
                settings.Color = true;
                settings.PrinterSettings.PrintToFile = true;
                return settings;
            }
            #endregion
        }
        public class PaperFormat
        {
            private double _height;
            private double _width;
            private string _name;


            public PaperFormat(string Name, double Height, double Width)
            {
                this._height = Height;
                this._name = Name;
                this._width = Width;
            }
            public override string ToString()
            {
                return _name;
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
            public double Height
            {
                get { return _height; }
                set { _height = value; }
            }
            public double Width
            {
                get { return _width; }
                set { _width = value; }
            }
        }
        public class ImageQuality
        {
            private int _dpi;
            private string _name;


            public ImageQuality(string Name, int dpi)
            {
                this._dpi = dpi;
                this._name = Name;
            }
            public override string ToString()
            {
                return _name;
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
            public int dpi
            {
                get { return _dpi; }
                set { _dpi = value; }
            }
        }
        #endregion

    }
}
