using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using System.IO;
using mvMapLib;
using Rekod.Services;

namespace Rekod
{
    public partial class GeometryCharacteristics : Form
    {
        public class Attribute
        {
            public static Point Location = new Point(9, 20);
            private static int _textBoxIndent = 105;
            private static int _nextLabelIndent = 25;
            public static Control ParentControl; 

            private Label _attrLabel; 
            private TextBox _attrTextBox;
            private Button _attrCopyButt; 
            public Attribute(String attrName, String attrValue)
            {
                _attrLabel = new Label();
                _attrLabel.Text = attrName;
                _attrLabel.Location = new Point(Location.X, Location.Y);
                _attrLabel.AutoSize = true;

                _attrCopyButt = new Button();
                _attrCopyButt.Click += new EventHandler(_attrCopyButt_Click);
                _attrCopyButt.Size = new Size(78, 21);
                _attrCopyButt.Text = Rekod.Properties.Resources.GC_Copy;
                _attrCopyButt.Location = new Point(ParentControl.ClientRectangle.Width - _attrCopyButt.Width - Location.X, Location.Y - 3);
                _attrCopyButt.Anchor = AnchorStyles.Top | AnchorStyles.Right; 

                _attrTextBox = new TextBox(); 
                _attrTextBox.Text = attrValue;
                _attrTextBox.Location = new Point(Location.X + _textBoxIndent, Location.Y - 3);
                _attrTextBox.Width = ParentControl.ClientRectangle.Width - _attrCopyButt.ClientRectangle.Width - _textBoxIndent - Location.X * 3; 
                _attrTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _attrTextBox.ReadOnly = true;
                _attrTextBox.ForeColor = Color.Black;

                ParentControl.SuspendLayout(); 
                ParentControl.Controls.Add(_attrLabel);
                ParentControl.Controls.Add(_attrCopyButt);
                ParentControl.Controls.Add(_attrTextBox);
                ParentControl.ResumeLayout();

                int newHeight = Location.Y + _attrLabel.Height + Location.X + (ParentControl.Height - ParentControl.ClientRectangle.Height);
                ParentControl.MinimumSize = new Size(250, newHeight);
                ParentControl.MaximumSize = new Size(1200, newHeight);
                ParentControl.Height = newHeight;

                Location.Y += _nextLabelIndent; 
            }

            void _attrCopyButt_Click(object sender, EventArgs e)
            {
                Clipboard.SetData(DataFormats.StringFormat, _attrTextBox.Text); 
            }
        }

        private String _wkt;
        private int _initSrid; 

        public GeometryCharacteristics(String wkt, int srid)
        {
            _initSrid = srid; 
            _wkt = wkt; 
            InitializeComponent();
            Text = Rekod.Properties.Resources.GC_CharacteristicsGeometry;
            CenterToParent(); 
        }

        private void GeometryCharacteristics_Load(object sender, EventArgs e)
        {
            bool geography_exists = false;
            CenterToParent(); 
            Attribute.ParentControl = this;
            Attribute.Location = new Point(9, 12);

            String geom = "", geom_old = "";
            using (SqlWork sqlCmd = new SqlWork())
            {
                String[] vers = sqlCmd.ServerVersion.Split(new[] { '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                int major = int.Parse(vers[0]);
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT exists(SELECT true FROM pg_catalog.pg_type WHERE typname like 'geography')";
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    geography_exists = sqlCmd.GetBoolean(0);
                }
                sqlCmd.Close();
            }

            if (geography_exists)
            {
                geom = String.Format("geography(st_transform(st_geomfromtext('{0}', {1}), {2}))", _wkt, _initSrid, 4326);
                geom_old = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", _wkt, _initSrid, Program.srid);
            }
            else
            {
                geom = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", _wkt, _initSrid, Program.srid);
                geom_old = String.Format("st_transform(st_geomfromtext('{0}', {1}), {2})", _wkt, _initSrid, Program.srid);
            }
            String geomType = "";
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT st_geometrytype({0})", geom_old);
                geomType = sqlCmd.ExecuteScalar().ToString();
                sqlCmd.Close();
            }
            new Attribute(Rekod.Properties.Resources.GC_Type, geomType);

            if (geomType.ToUpper().Contains("POLYGON"))
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT st_area({0})", geom);
                    string temp = sqlCmd.ExecuteScalar().ToString();
                    try
                    {
                        new Attribute(Rekod.Properties.Resources.GC_Area, temp);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    sqlCmd.Close();
                }

                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT ST_perimeter({0})", geom_old);
                    new Attribute(Rekod.Properties.Resources.GC_Perimetr, sqlCmd.ExecuteScalar().ToString());
                    sqlCmd.Close();
                }
            }
            else if (geomType.ToUpper().Contains("LINE"))
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = String.Format("SELECT st_length({0})", geom);
                    new Attribute(Rekod.Properties.Resources.GC_Length, sqlCmd.ExecuteScalar().ToString());
                    sqlCmd.Close();
                }
            }
            String centroidCoords = "X: ";
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT st_transform(st_geomfromtext('{0}', {1}), {2})", _wkt, _initSrid, Program.srid);
                geom = (string)sqlCmd.ExecuteScalar();
                sqlCmd.Close();
            }

            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT st_x(st_centroid('{0}'))", geom);
                centroidCoords += sqlCmd.ExecuteScalar().ToString();
                sqlCmd.Close();
            }

            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT st_y(st_centroid('{0}'))", geom);
                centroidCoords += ", Y: " + sqlCmd.ExecuteScalar().ToString();
                sqlCmd.Close();
            }

            new Attribute(String.Format(Rekod.Properties.Resources.GC_Centroid +" ({0})", Program.srid), centroidCoords); 
        }
    }
}
