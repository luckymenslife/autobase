using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mvMapLib;
using Rekod.Services;

namespace Rekod.DBTablesEdit
{
    public partial class LabelStyle : Form
    {
        private int _idT;
        private mvLayer tLayer;
        private FontDialog fd = null;
        private tablesInfo _ti;
        private bool _firstUsedLabelStyle;
        private bool _userStyle = false;
        public bool save = false;

        public LabelStyle(int idT)
		{
            _idT = idT;
            _ti = classesOfMetods.getTableInfo(idT); 
            String layerName =Program.RelationVisbleBdUser.GetNameInBd(idT);
            tLayer = Program.app.mapLib.getLayer(layerName);
            save = false;

            CenterToParent();
            InitializeComponent();

            if (!SyncController.HasRight(idT))
            {
                button_save.Enabled = false;
            }
		}
        private void button3_Click(object sender, EventArgs e)
        {
            save = false;
            Close();
        }
        private void LabelStyle_Load(object sender, EventArgs e)
        {
            text_fontName.Text = _ti.label_fontname;
            numeric_fontSize.Value = _ti.label_fontsize;
            panel_fontColor.BackColor = ConvUIntToColor(_ti.label_fontcolor);
            check_showFrame.Checked = _ti.label_showframe;
            panel_frameColor.BackColor = ConvUIntToColor(_ti.label_framecolor);
            check_alongLines.Checked = _ti.label_parallel;
            check_overLap.Checked = _ti.label_overlap;
            check_useBounds.Checked = _ti.label_usebounds;
            numeric_minScale.Value = _ti.label_minscale;
            numeric_maxScale.Value = _ti.label_maxscale;
            numeric_offSet.Value = _ti.label_offset;
            _firstUsedLabelStyle = true; 
            check_graphicUnits.Checked = _ti.label_graphicunits; 

            if (!_ti.label_usebounds)
            {
                numeric_maxScale.Enabled = false;
                numeric_minScale.Enabled = false;
            }
            else
            {
                numeric_maxScale.Enabled = true;
                numeric_minScale.Enabled = true;
            }
        }
        private void button_showFondDialog(object sender, EventArgs e)
        {
        	fd = new FontDialog();

        	FontStyle fs= FontStyle.Regular;
			if (_ti.label_fontbold) fs = fs | FontStyle.Bold;
			if (_ti.label_fontitalic) fs = fs | FontStyle.Italic;
			if (_ti.label_fontstrikeout) fs = fs | FontStyle.Strikeout;
			if (_ti.label_fontunderline) fs = fs | FontStyle.Underline;
			if (_ti.label_fontbold) fs = fs | FontStyle.Bold; 
			

        	Font font = new Font(_ti.label_fontname, _ti.label_fontsize, fs);
        	fd.Font = font;  

            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                text_fontName.Text = fd.Font.Name;
                numeric_fontSize.Value = (int)(fd.Font.Size);
                _ti.label_fontname = fd.Font.Name;
                _ti.label_fontsize = (int)(double.Parse(fd.Font.Size.ToString()));
                _ti.label_fontstrikeout = fd.Font.Strikeout;
                _ti.label_fontitalic = fd.Font.Italic;
                _ti.label_fontunderline = fd.Font.Underline;
            	_ti.label_fontbold = fd.Font.Bold; 
            }
        }

        private void panel_frameColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = panel_frameColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                panel_frameColor.BackColor = cd.Color;
                _ti.label_framecolor = ConvColorToUInt(cd.Color); 
            }																															     
        }


        private void button_save_Click(object sender, EventArgs e)
        {
            save = true;
            if (!this._userStyle)
            loadNewLabelStyleIntoBase();
            SyncController.ReloadStyle(_idT);

            if (_firstUsedLabelStyle && (tLayer != null))
            {
                classesOfMetods.reloadLayer(_idT);
            }
            else
            {
                for (int i = 0; i < Program.tables_info.Count; i++)
                {
                    if (Program.tables_info[i].idTable == _idT)
                    {
						Program.tables_info[i] = _ti;
                    	i = Program.tables_info.Count; 
                    }
                }

				if (tLayer!=null)
				{
					Program.mainFrm1.layersManager1.SetLabelStyle(_idT);
				}
            }
            Close();
        }

        public string SQL
        {
            get
            {
                if (!Program.WorkSets.CurrentWorkSet.IsDefault)
                {
                    return String.Format(
        @"UPDATE sys_scheme.table_info_sets SET label_showframe = {0}, label_framecolor = {1}, label_parallel={2},
label_overlap={3}, label_usebounds={4},label_minscale={5}, label_maxscale={6},label_offset={7},label_graphicunits={8},
label_fontname='{9}',label_fontcolor={10},label_fontsize={11}, label_fontstrikeout={12},label_fontitalic={13},label_fontunderline={14},
label_fontbold={15} WHERE id_table = {16} AND id_set = {17};",
                                _ti.label_showframe,
                                _ti.label_framecolor,
                                _ti.label_parallel,
                                _ti.label_overlap,
                                _ti.label_usebounds,
                                _ti.label_minscale,
                                _ti.label_maxscale,
                                _ti.label_offset,
                                _ti.label_graphicunits,
                                _ti.label_fontname,
                                _ti.label_fontcolor,
                                _ti.label_fontsize,
                                _ti.label_fontstrikeout,
                                _ti.label_fontitalic,
                                _ti.label_fontunderline,
                                _ti.label_fontbold,
                                _idT,
                                Program.WorkSets.CurrentWorkSet.Id);
                }
                else
                {
                    return String.Format(
        @"UPDATE sys_scheme.table_info SET label_showframe = {0}, label_framecolor = {1}, label_parallel={2},
label_overlap={3}, label_usebounds={4},label_minscale={5}, label_maxscale={6},label_offset={7},label_graphicunits={8},
label_fontname='{9}',label_fontcolor={10},label_fontsize={11}, label_fontstrikeout={12},label_fontitalic={13},label_fontunderline={14},
label_fontbold={15} WHERE id = {16};",
                                _ti.label_showframe,
                                _ti.label_framecolor,
                                _ti.label_parallel,
                                _ti.label_overlap,
                                _ti.label_usebounds,
                                _ti.label_minscale,
                                _ti.label_maxscale,
                                _ti.label_offset,
                                _ti.label_graphicunits,
                                _ti.label_fontname,
                                _ti.label_fontcolor,
                                _ti.label_fontsize,
                                _ti.label_fontstrikeout,
                                _ti.label_fontitalic,
                                _ti.label_fontunderline,
                                _ti.label_fontbold,
                                _idT);
                }
            }
        }
        public bool UserStyle
        {
            get
            {
                return _userStyle;
            }
            set
            {
                _userStyle = value;
            }
        }
        private void loadNewLabelStyleIntoBase()
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = SQL;
            sqlCmd.Execute(true);
            sqlCmd.Close();
        }

        private mvFontObject getFontStyle()
        {
            mvFontObject temp = Program.app.mapLib.createFontObject();

            if (fd != null)
            {
                temp.Color = ConvColorToUInt(panel_fontColor.BackColor);
                temp.fontname = text_fontName.Text;
                try
                {
                    temp.size = (int)(numeric_fontSize.Value); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LabelStyle_ErrorConvert+": " + ex.Message);
                }
                temp.strikeout = fd.Font.Strikeout;
                temp.italic = fd.Font.Italic;
                temp.underline = fd.Font.Underline;
            }

            temp.graphicUnits = check_graphicUnits.Checked;
            tLayer.labelParallel = check_alongLines.Checked;
            tLayer.labelOverlap = check_overLap.Checked;
            tLayer.labelOffset = (int)(numeric_offSet.Value);
            if (check_useBounds.Checked)
            {
                tLayer.usebounds = true;
                tLayer.MinScale = (uint)(numeric_minScale.Value);
                tLayer.MaxScale = (uint)(numeric_maxScale.Value);
            }
            if (check_showFrame.Checked)
            {
                temp.framecolor = ConvColorToUInt(panel_frameColor.BackColor);
            }

            temp.angle = 35;

            // temp.weight принимает 3 значения: 550, 600, 650. Диас
            temp.weight = 650;
            return temp;
        }

        public static uint ConvColorToUInt(Color clr)
        {
            uint temp = 0;
            temp = Convert.ToUInt32(clr.R + (clr.G << 8) + (clr.B << 16));
            return temp;
        }
        public static Color ConvUIntToColor(uint val)
        {
            int blue = (int)(val / 65536);
            int green = (int)((val / 256) % 256);
            int red = (int)(val%256);
            Color c = Color.FromArgb(red, green, blue);
            return c; 
        }

        private void check_useBounds_CheckedChanged(object sender, EventArgs e)
        {
            if (!check_useBounds.Checked)
            {
                numeric_maxScale.Enabled = false;
                numeric_minScale.Enabled = false;
            }
            else
            {
                numeric_maxScale.Enabled = true;
                numeric_minScale.Enabled = true;
            }
            _ti.label_usebounds = check_useBounds.Checked; 
        }
        private void panel_fontColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = panel_fontColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                panel_fontColor.BackColor = cd.Color;
                _ti.label_fontcolor = ConvColorToUInt(cd.Color); 
            }
        }
        private void check_graphicUnits_CheckedChanged(object sender, EventArgs e)
        {
            _ti.label_graphicunits = check_graphicUnits.Checked; 
        }
        private void check_showFrame_CheckedChanged(object sender, EventArgs e)
        {
            _ti.label_showframe = check_showFrame.Checked; 
        }
        private void check_alongLines_CheckedChanged(object sender, EventArgs e)
        {
            _ti.label_parallel = check_alongLines.Checked; 
        }
        private void check_overLap_CheckedChanged(object sender, EventArgs e)
        {
            _ti.label_overlap = check_overLap.Checked; 
        }
        private void numeric_minScale_ValueChanged(object sender, EventArgs e)
       {
           _ti.label_minscale = (uint)(numeric_minScale.Value); 
       }
        private void numeric_maxScale_ValueChanged(object sender, EventArgs e)
       {
           _ti.label_maxscale = (uint)(numeric_maxScale.Value); 
       }
        private void numeric_offSet_ValueChanged(object sender, EventArgs e)
       {
           _ti.label_offset = (int)(numeric_offSet.Value); 
       }
        private void numeric_fontSize_ValueChanged(object sender, EventArgs e)
       {
		  _ti.label_fontsize = (int)(numeric_fontSize.Value); 
       }
    }
}