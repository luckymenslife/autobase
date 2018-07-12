using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace Rekod.ProjectionSelection
{
    public class Projection
    {
        int srid;
        string auth_name;
        int auth_srid;
        string srtext;
        string proj4text;
        private bool is_system;
        private string _name;
        private string _location;

        public int Srid
        {
            get { return srid;}
            set { srid = value; }
        }
        public string Auth_name
        {
            get { return auth_name; }
            set { auth_name = value; }
        }
        public int Auth_srid
        {
            get { return auth_srid; }
            set { auth_srid = value; }
        }
        public string Srtext
        {
            get { return srtext; }
            set { srtext = value; }
        }
        public string Proj4text
        {
            get { return proj4text; }
            set { proj4text = value; }
        }
        public bool Sys_proj
        {
            get { return is_system; }
            set { is_system = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public string DisplayedText
        {
            get
            {
                return String.Format("{0} ({1}, {2})",
                    Srid,
                    !String.IsNullOrEmpty(Name) ? Name.Trim() : "<unknown>",
                    !String.IsNullOrEmpty(Location) ? Location.Trim() : "<unknown>");
            }
        }

        public Projection(int srid, string auth_name, int auth_srid, string srtext, string proj4text, bool is_system)
        {
            this.srid = srid;
            this.auth_name = auth_name;
            this.auth_srid = auth_srid;
            this.srtext = srtext;
            this.proj4text = proj4text;
            this.is_system = is_system;

            try
            {
                string pattern = "\"(.*?)\\/";
                string name = Regex.Match(srtext.ToString(), pattern).Value;
                if (name == null || name.Length < 2)
                {
                    pattern = "\"(.*?)\\,";
                    name = Regex.Match(srtext.ToString(), pattern).Value;
                }
                if (name.Length < 2) name = "aa";
                name = name.Remove(0, 1);
                name = name.Remove(name.Length - 1, 1);
                name = name.Replace("\"", String.Empty);

                _name = name;
            }
            catch { }

            try
            {
                string pattern = "\\/(.*?)\\,";
                string name = Regex.Match(srtext.ToString(), pattern).Value;
                name = name.Replace("3 / ", String.Empty);
                if (name == null || name.Length < 6)
                {
                    pattern = "DATUM\\[(.*?)\\,";
                    name = Regex.Match(srtext.ToString(), pattern).Value;
                    if (name.Length < 2) name = "aaaaaaa";
                    name = name.Remove(0, 4);
                }

                name = name.Remove(0, 2);
                name = name.Remove(name.Length - 1);
                name = name.Replace("\"", String.Empty);

                _location = name;
            }
            catch { }
        }

        public Projection(int srid, string auth_name, string srtext, string proj4text, bool is_system)
        {
            this.srid = srid;
            this.auth_name = auth_name;
            this.srtext = srtext;
            this.proj4text = proj4text;
            this.is_system = is_system;
        }

        public override int GetHashCode()
        {
            return this.Srid.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == DependencyProperty.UnsetValue || obj == null)
            {
                return false;
            }
            if (!(obj is Projection) || obj == null) 
                return false;

            return this.Srid.Equals((obj as Projection).Srid);
        }
    }
}
