using System;
using System.Collections.Generic;
using System.Text;
using LevDan.Exif;
using System.Text.RegularExpressions;

namespace Rekod
{
    static class exif
    {
        public static string extractexif(string path1)
        {
            string wkt = "POINT(";
            string lon = "";
            string lat = "";
            string tmp1 = "";
            string pattern = "([0-9]+|[,0-9]+)";
            Regex rx = new Regex(pattern);
            MatchCollection mh;
            try
            {
                ExifTagCollection exif = new ExifTagCollection(path1);
                foreach (ExifTag tag in exif)
                {
                    if (tag.FieldName.Equals("GPSLongitude", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmp1 = tag.Value.Replace("°", "").Replace("'", "").Replace("\"", "");
                        mh = rx.Matches(tmp1);
                        lon = compare(mh);
                    }
                    if (tag.FieldName.Equals("GPSLatitude", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmp1 = tag.Value.Replace("°", "").Replace("'", "").Replace("\"", "");
                        mh = rx.Matches(tmp1);
                        lat = compare(mh);
                    }
                }
                if (lon != null || lat != null)
                {
                    if (lon != "" || lat != "")
                    {
                        wkt += lon + " " + lat + ")";
                        return wkt;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception x)
            {
                
                throw new Exception("Err1", x);
            }
        }
        private static string compare(MatchCollection col)
        {
            string tmp = "";
            switch (col.Count)
            {
                case 4:
                    tmp = (double.Parse(col[0].Value) + double.Parse(col[1].Value) / 60 + double.Parse(col[2].Value + col[3].Value.Replace(".", ",")) / 3600).ToString();
                    break;
                case 3:
                    tmp = (double.Parse(col[0].Value) + double.Parse(col[1].Value) / 60 + double.Parse(col[2].Value) / 3600).ToString();
                    break;
                case 2:
                    tmp = (double.Parse(col[0].Value) + double.Parse(col[1].Value) / 60).ToString();
                    break;
            }
            return tmp.Replace(",", ".");
        }
    
    }
}
