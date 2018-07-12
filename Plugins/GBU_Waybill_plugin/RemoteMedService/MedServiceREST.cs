using GBU_Waybill_plugin.MTClasses.Tools;
using GBU_Waybill_plugin.RemoteMedService.ModelsREST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class MedServiceREST : IMedService
    {
        #region Поля
        private DateTime _last_connection;
        private string _url;
        #endregion

        #region IMedService
        public bool Auth(string url, string login, string pwd)
        {
            _url = url;
            return true;
        }
        public List<MedCheck> GetWaybills(DateTime date)
        {
            return null;
        }
        public List<MedCheck> GetWaybills(DateTime date, ME_Employee employee)
        {
            using (WebClient wc = new WebClient())
            {
                string qwery_url = string.Format("driver/med_result/{0}/{1}/{2}", Tools.GetUnixTime(date.AddHours(4)), employee.TabNomer, EmployeesSync.MedServiceOrgId);
                string json = wc.DownloadString(GetUrl(qwery_url));
                var result = JsonHelper.JsonDeserialize<MedChecksJsonM>(Tools.UTF8ToWin1251(json));
                if (result != null)
                {
                    List<MedCheck> temp = new List<MedCheck>();
                    foreach (var item in result.medchecks)
                    {
                        temp.Add(Tools.ConvertToMedCheck(item, employee.TabNomer));
                    }
                    return temp;
                }
            }
            return null;
        }
        public bool AddEmployee(ME_Employee employee)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    DriverJsonM driver = ConvertToDriverJsonM(employee);
                    string post = JsonHelper.JsonSerializer<DriverJsonM>(driver);
                    string json = wc.UploadString(GetUrl("api/driver/add"), "PUT", Tools.Win1251ToUTF8(post));
                    return true;
                }
                return false;
            }
            catch
            { return false; }
        }
        public bool UpdateEmployee(ME_Employee employee)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    DriverJsonM driver = ConvertToDriverJsonM(employee);
                    string post = JsonHelper.JsonSerializer<DriverJsonM>(driver);
                    string json = wc.UploadString(GetUrl("api/driver/update"), "PUT", Tools.Win1251ToUTF8(post));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool DeleteEmployee(string tab_no)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string post = "{\"number\": \"" + tab_no + "\", \"org\":\"1\"}";
                    string json = wc.UploadString(GetUrl("api/driver/delete/"), "PUT", Tools.Win1251ToUTF8(post));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }

        }
        public List<ME_Employee> GetEmployees()
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(GetUrl("driver"));
                var result = JsonHelper.JsonDeserialize<List<DriverJsonM>>(Tools.UTF8ToWin1251(json));
                if (result != null)
                {
                    List<ME_Employee> temp = new List<ME_Employee>();
                    foreach (var item in result)
                    {
                        temp.Add(Tools.ConvertToME_Employee(item));
                    }
                    return temp;
                }
                return null;
            }
        }
        public ME_Employee GetEmployee(string tab_no)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string json = wc.DownloadString(GetUrl("/driver_detail/" + tab_no));
                    var result = JsonHelper.JsonDeserialize<DriverDetailJsonM>(Tools.UTF8ToWin1251(json));
                    if (result != null)
                    {
                        return Tools.ConvertToME_Employee(result);
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public bool Alive
        {
            get { return true; }
        }
        #endregion

        #region Закрытые Методы
        private string GetUrl(string qwery_url)
        {
            if (!qwery_url.StartsWith("/"))
                qwery_url = "/" + qwery_url;
            return _url + qwery_url;
        }
        private DateTime GetDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
        }
        private DriverJsonM ConvertToDriverJsonM(ME_Employee employee)
        {
            DriverJsonM result = new DriverJsonM();
            result.number = employee.TabNomer;
            result.name = employee.FirstName;
            result.surname = employee.LastName;
            result.patronymic = employee.MiddleName;
            result.org = employee.OrgId.ToString();
            result.year = employee.Birthday.ToString("dd-MM-yyyy");
            result.license = employee.DriverLicense;
            return result;
        }
        #endregion
        public List<Org> GetOrgs()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string json = wc.DownloadString(GetUrl("/driver/org"));
                    var result = JsonHelper.JsonDeserialize<List<Org>>(Tools.UTF8ToWin1251(json));
                    if (result != null)
                    {
                        return result;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
