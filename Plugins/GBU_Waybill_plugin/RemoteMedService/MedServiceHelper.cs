using GBU_Waybill_plugin.pro.mpmo.ln;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class MedServiceHelper
    {
        #region Поля
        private string _format;
        private string _dateSep;
        private string _timeSep;
        private string _dateFormat;
        private MedServiceService _service;
        #endregion
        #region Конструктор
        public MedServiceHelper(string url, String login, String pwd)
        {
            //http://ln.mpmo.pro:85/PMOMedicalBase/AvtoDorogi/Drivers.nsf/MedService?OpenWebService
            _service = new MedServiceService(url);
            _service.PreAuthenticate = true;
            _service.Credentials = new NetworkCredential(login, pwd);

            //_service.CookieContainer = GetCookieContainer("http://ln.mpmo.pro:85/", login, pwd);
            //получить формат даты MDY или DMY
            this._format = _service.dateFormat();
            //получить разделитель
            this._dateSep = _service.getDateSep();
            //получить разделитель времени
            this._timeSep = _service.getTimeSep();
            this._dateFormat = (this._format.Equals("DMY") ? "dd" + this._dateSep + "MM" : "MM" + this._dateSep + "dd") + this._dateSep + "yyyy";
        }
        #endregion Конструктор
        #region Закрытые методы
        private int GetCheckType(string type)
        {
            switch (type)
            {
                case "before":
                    return 1;
                case "after":
                    return 2;
                default:
                    return 0;
            }
        }
        private int GetCheckStatus(string status)
        {
            switch (status)
            {
                case "прошел":
                    return 1;
                default:
                    return 2;
            }
        }
        private long GetUnixTime(DateTime winTime)
        {
            return (long)(winTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }
        private CookieContainer GetCookieContainer(string server, string login, string password)
        {
            CookieContainer cookies = new CookieContainer();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(server + "/names.nsf");
            request.Method = "POST";
            request.AllowAutoRedirect = false;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;


            string post = "Username=" + login + "&Password=" + password;
            byte[] bytes = Encoding.ASCII.GetBytes(post);


            request.ContentLength = bytes.Length;
            Stream streamOut = request.GetRequestStream();
            streamOut.Write(bytes, 0, bytes.Length);
            streamOut.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ((response.StatusCode == HttpStatusCode.Found) ||
                (response.StatusCode == HttpStatusCode.Redirect) ||
                (response.StatusCode == HttpStatusCode.Moved) ||
                (response.StatusCode == HttpStatusCode.MovedPermanently))
            {
                return cookies;
            }

            return null;
        }
        #endregion Закрытые методы
        #region Открытые методы
        static public DateTime GetWinTime(long unixTime)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(unixTime).ToLocalTime();
        }
        public bool AddEmplouyee(ME_Employee employee)
        {
            if (employee != null && !string.IsNullOrEmpty(employee.TabNomer))
            {
                return this._service.addEmployee(employee.TabNomer,
                    employee.LastName,
                    employee.FirstName,
                    employee.MiddleName,
                    employee.Birthday,
                    employee.OrgName);
            }
            else
            {
                return false;
            }
        }
        public bool DeleteEmplouyee(ME_Employee employee)
        {
            if (employee != null && !string.IsNullOrEmpty(employee.TabNomer) && ExistsEmplouyeeInMedSrvice(employee))
            {
                return this._service.deleteEmployee(employee.TabNomer);
            }
            else
            {
                return false;
            }
        }
        public List<Waybill> GetWaybillList(DateTime lastUpdate)
        {
            List<Waybill> waybills = new List<Waybill>();
            long time = this.GetUnixTime(lastUpdate);
            Waybill[] temp = this._service.getWaybillsByTime(time);
            if (temp != null)
            {
                waybills.AddRange(temp);
            }
            return waybills;
        }
        public List<MedCheck> ConvertToMedCheck(List<Waybill> list)
        {
            List<MedCheck> result = new List<MedCheck>();
            foreach (var item in list)
            {
                result.Add(new MedCheck()
                {
                    EmployeeId = item.employeeId,
                    DoctorName = item.medicFullname,
                    CheckDate = GetWinTime(item.time),
                    ECP = item.EDS,
                    CheckStatus = GetCheckStatus(item.access),
                    CheckType = GetCheckType(item.beforeAfter)
                });
            }
            return result;
        }
        public bool UpdateEmplouyee(ME_Employee employee)
        {
            if (employee != null && !string.IsNullOrEmpty(employee.TabNomer) && ExistsEmplouyeeInMedSrvice(employee))
            {
                string dateFormat = (this._format.Equals("DMY") ? "dd" + this._dateSep + "MM" : "MM" + this._dateSep + "dd") + this._dateSep + "yyyy";
                return this._service.updateEmployee(employee.TabNomer, new string[] { "Birthday", "LastName", "FirstName", "MiddleName", "Organization" },
                      new string[] {
                          employee.Birthday.ToString(dateFormat),
                          employee.LastName,
                          employee.FirstName,
                          employee.MiddleName,
                          employee.OrgName
                      });
            }
            else
            {
                return false;
            }
        }
        public bool ExistsEmplouyeeInMedSrvice(ME_Employee employee)
        {
            var emp = this._service.getEmployeeById(employee.TabNomer);
            if (emp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void DeleteAllEmployees()
        {
            int count = this._service.getEmloyeesCount();
            object[][] data = this._service.getEmployees();
            for (int i = 0; i < count; i++)
            {
                System.Xml.XmlNode[] node = (System.Xml.XmlNode[])data[i][0];
                string tab_no = node[0].Value;
                //this._service.deleteEmployee(tab_no);
            }

        }
        #endregion Открытые методы

    }

}
