using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WrapperMaplib.Wrapper.Geometry;

namespace GBU_Waybill_plugin.MTClasses.Tools
{
    public class RouteWebAPI
    {
        public static string api_url = "https://router.geo4.me/";
        public static Distances GetDistances(List<vPointd> points)
        {
            string temp = ConvertPointsToStr(points);
            temp = Get(api_url + "table?" + temp+"z=20");
            Distances ds = JsonHelper.JsonDeserialize<Distances>(temp);
            return ds;
        }
        public static Route GetRoute(List<vPointd> points)
        {
            //GET/viaroute?loc=55.7887400,49.1221400&loc=55.8437600,48.5178400
            string temp = ConvertPointsToStr(points);
            temp = Get(api_url + "viaroute?" + temp.TrimEnd('&'));
            Route ds = JsonHelper.JsonDeserialize<Route>(temp);
            return ds;
        }
        public static string GetGpxRoute(List<vPointd> points)
        {
            //GET/viaroute?loc=55.7887400,49.1221400&loc=55.8437600,48.5178400
            string temp = ConvertPointsToStr(points);
            temp = Get(api_url + "viaroute?" + temp+"output=gpx");
            return temp;
        }
        public static string ConvertPointsToStr(List<vPointd> points)
        {
            ///https://router.geo4.me/table?loc=52.554070,13.160621&loc=52.431272,13.720654&loc=52.554070,13.720654&loc=52.554070,13.160621&z=14
            StringBuilder strs = new StringBuilder();
            foreach (var item in points)
            {
                string x = item.x.ToString().Replace(",", ".");
                string y = item.y.ToString().Replace(",", ".");
                string point = string.Format("loc={0},{1}", y, x);
                strs.Append(point + "&");
            }
            return strs.ToString();
        }
        public static string Get(string uri)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers["Accept"] = "application/json";
            wc.Headers["Content-Type"] = "application/json";
            string str = wc.DownloadString(uri);
            return str;
        }
        private static string Post(string uri, string body)
        {
            var wc = new WebClient();
            ServicePointManager.Expect100Continue = false;
            wc.Encoding = Encoding.UTF8;
            wc.Headers["Accept"] = "application/json";
            wc.Headers["Content-Type"] = "application/json";
            var uplode = wc.UploadString(uri, body);
            return uplode;
        }
    }
    public class Distances
    {
        public List<double[]> distance_table { get; set; }
    }
    public class Route
    {
        public string route_geometry { get; set; }
    }
}
