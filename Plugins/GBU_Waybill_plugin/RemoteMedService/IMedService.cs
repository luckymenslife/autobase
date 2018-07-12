using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public interface IMedService
    {
        bool Auth(string url, string login, string pwd);
        List<MedCheck> GetWaybills(DateTime date);
        List<MedCheck> GetWaybills(DateTime date, ME_Employee employee);
        bool AddEmployee(ME_Employee employee);
        bool UpdateEmployee(ME_Employee employee);
        bool DeleteEmployee(string tab_no);
        List<ME_Employee> GetEmployees();
        ME_Employee GetEmployee(string tab_no);
        bool Alive { get; }
        List<Org> GetOrgs();
    }
}
