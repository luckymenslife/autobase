using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GBU_Waybill_plugin.pro.mpmo.ln;
using System.Net;
using System.Windows;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class MedServiceWSDL : IMedService
    {
        #region Поля
        private string _format;
        private string _dateSep;
        private string _timeSep;
        private string _dateFormat;
        private MedServiceService _service;
        private DateTime _last_connection;
        #endregion

        #region IMedService
        public bool Auth(string url, string login, string pwd)
        {
            try
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
                SetLastConnection();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public List<MedCheck> GetWaybills(DateTime date)
        {
            List<MedCheck> medchecks = new List<MedCheck>();
            long time = Tools.GetUnixTime(date);
            Waybill[] temp = this._service.getWaybillsByTime(time);
            if (temp != null)
            {
                foreach (var item in temp)
                {
                    medchecks.Add(Tools.ConvertToMedCheck(item));
                }
            }
            return medchecks;
        }
        public List<MedCheck> GetWaybills(DateTime date, ME_Employee employee)
        {
            List<MedCheck> medchecks = new List<MedCheck>();
            long time = Tools.GetUnixTime(date);
            Waybill[] temp = this._service.getWaybillsByTime(time);
            if (temp != null)
            {
                foreach (var item in temp)
                {
                    if (item.employeeId == employee.TabNomer)
                        medchecks.Add(Tools.ConvertToMedCheck(item));
                }
            }
            return medchecks;
        }
        public bool AddEmployee(ME_Employee employee)
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
        public bool UpdateEmployee(ME_Employee employee)
        {
            if (employee != null && !string.IsNullOrEmpty(employee.TabNomer) && ExistsEmplouyeeInMedSrvice(employee.TabNomer))
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
        public bool DeleteEmployee(string tab_no)
        {
            if (!string.IsNullOrEmpty(tab_no) && ExistsEmplouyeeInMedSrvice(tab_no))
            {
                return this._service.deleteEmployee(tab_no);
            }
            else
            {
                return false;
            }
        }
        public List<ME_Employee> GetEmployees()
        {
            object[][] data = this._service.getEmployees();
            for (int i = 0; i < data.GetUpperBound(0); i++)
            {
                System.Xml.XmlNode[] node = (System.Xml.XmlNode[])data[i][0];
                string tab_no = node[0].Value;
            }
            throw new NotImplementedException("Необходимо реализовать GetEmployees!");
        }
        public ME_Employee GetEmployee(string tab_no)
        {
            Employee temp = this._service.getEmployeeById(tab_no);
            if (temp != null)
            {
                return Tools.ConvertToME_Employee(temp);
            }
            return null;
        }
        public bool Alive
        {
            get
            {
                if (_service != null && DateTime.Now < _last_connection.AddMinutes(30))
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Закрытые Методы
        private bool ExistsEmplouyeeInMedSrvice(string tab_no)
        {
            var emp = GetEmployee(tab_no);
            if (emp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void SetLastConnection()
        {
            _last_connection = DateTime.Now;
        }
        #endregion
        public List<Org> GetOrgs()
        {
            return null;
        }
    }
}
