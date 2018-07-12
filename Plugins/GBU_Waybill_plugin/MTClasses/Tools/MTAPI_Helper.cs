using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tools
{
    public class MTAPI_Helper
    {
        public static string mt_url = "http://gbu.asuds77.ru:8000";
        #region Public
        public static MT_CarShortReportModel GetCarShortReport(int glonass_id, DateTime date_begin, DateTime date_end, string token)
        {
            String data = MTAPI_Helper.Get(String.Format(MTAPI_Helper.mt_url + "/reports/short/{0}/{1}/{2}?token={3}", glonass_id,
               MTAPI_Helper.GetUnixTime(date_begin), MTAPI_Helper.GetUnixTime(date_end), token));
            return JsonHelper.JsonDeserialize<MT_CarShortReportModel>(data);
        }
        public static bool PostWayBills(List<MT_CarWayBill> way_bills, string token)
        {
            MTAPI_Helper.Post(String.Format(MTAPI_Helper.mt_url + "/wayBill?token={0}", token), JsonHelper.JsonSerializer<List<MT_CarWayBill>>(way_bills));
            return true;
        }
        public static MT_CarsTask PostCarsTask(MT_CarsTask cars_task, string token)
        {
            string rezult = MTAPI_Helper.Post(String.Format(MTAPI_Helper.mt_url + "/modules/carroutes/tasks?token={0}", token), JsonHelper.JsonSerializer<MT_CarsTask>(cars_task));
            MT_CarsTask new_task = JsonHelper.JsonDeserialize<MT_CarsTask>(rezult);
            return new_task;
        }
        public static List<MT_CarsTask> GetMtTasks(string token)
        {
            string rezult = MTAPI_Helper.Get(String.Format(MTAPI_Helper.mt_url + "/modules/carroutes/tasks?token={0}", token));
            List<MT_CarsTask> tasks = JsonHelper.JsonDeserialize<List<MT_CarsTask>>(rezult);
            return tasks;
        }
        public static bool DeleteTask(int id_mt_task, string reason, string token)
        {
            try
            {
                MTAPI_Helper.Put(String.Format(MTAPI_Helper.mt_url + "/modules/carroutes/tasks/{0}/cancel?token={1}", id_mt_task, token),
                                 "{"+String.Format("\"reason\":\"{0}\"", reason)+"}");
                return true;
            }
            catch
            {
                return false;
            }
        }
        static public long GetUnixTime(DateTime winTime)
        {
            return (long)(winTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }
        static public DateTime GetWinTime(long unixTime)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(unixTime).ToLocalTime();
        }
        #endregion Public
        #region Private
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
        public static string Put(string uri, string body)
        {
            WebClient wc = new WebClient();
            ServicePointManager.Expect100Continue = false;
            wc.Encoding = Encoding.UTF8;
            wc.Headers["Content-Type"] = "application/json";
            wc.Headers["Rest-Method"] = "PUT";
            string str = wc.UploadString(uri, body);
            return str;
        }  
        #endregion Private
    }
}
