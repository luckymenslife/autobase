using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class EmployeesSync
    {
        public static int IdOrg;
        public static string Url;
        public static string Login;
        public static string Pwd;
        public static string UrlRest;
        public static int? MedServiceOrgId;
        public static IMedService _med_service;
        #region EmployeeSync
        public void Main()
        {
            if (_med_service == null || !_med_service.Alive)
            {
                if (string.IsNullOrEmpty(UrlRest))
                {
                    _med_service = new MedServiceWSDL();
                    _med_service.Auth(Url, Login, Pwd);
                }
                else
                {
                    if (!MedServiceOrgId.HasValue)
                    {
                        MessageBox.Show("Не определен идентификатор организации в медсервисе!", "Прекращение операции", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    _med_service = new MedServiceREST();
                    _med_service.Auth(UrlRest, Login, Pwd);
                }
            }
            //_med_service.DeleteAllEmployees();
            Console.WriteLine("Connected to MedService: Ok");
            List<HistoryInfo> changes = LoadEmployeesChangesFromDB();
            int ok_ids=0;
            int bad_ids=0;
            foreach (var item in changes)
            {
                switch (item.TypeOpertaion)
                {
                    case 1:
                        Console.WriteLine("Insert : " + item.IdObject.ToString());
                        //med.DeleteEmplouyee(item.IdObject);
                        if (_med_service.AddEmployee(GetME_Employee(item.IdObject)))
                        {
                            ok_ids++;
                        }
                        else
                        {
                            bad_ids++;
                        }
                        break;
                    case 2:
                        Console.WriteLine("Update : " + item.IdObject.ToString());
                        if (!_med_service.UpdateEmployee(GetME_Employee(item.IdObject)))
                        {
                            bad_ids++;
                        }
                        break;
                    case 3:
                        Console.WriteLine("Delete : " + item.IdObject.ToString());
                        if (_med_service.DeleteEmployee(GetDeletedME_Employee(item).TabNomer))
                        {
                            bad_ids++;
                        }
                        break;
                    default:
                        break;
                }
            }
            MessageBox.Show("Синхронизация закончена!"+Environment.NewLine+
                "Успешно добавлено:" + ok_ids.ToString()+Environment.NewLine+
                "Ошибок синхронизации:" + bad_ids.ToString(), "Синхронизация", MessageBoxButton.OK, MessageBoxImage.Information);
            Console.WriteLine("End");
        }
        private static ME_Employee GetDeletedME_Employee(HistoryInfo obj)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = @"
SELECT e.gid, e.lastname, e.firstname, e.middlename, e.birthday, e.tab_no, co.name, e.driver_license
FROM autobase.employees_history e, autobase.orgs co 
WHERE co.gid = e.org_id AND e.id_history = " + obj.IdHistory.ToString() + ";";
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    return new ME_Employee()
                    {
                        ID = sqlCmd.GetInt32("gid"),
                        LastName = sqlCmd.GetString("lastname"),
                        FirstName = sqlCmd.GetString("firstname"),
                        MiddleName = sqlCmd.GetString("middlename"),
                        Birthday = sqlCmd.GetValue<DateTime>("birthday"),
                        TabNomer = sqlCmd.GetString("tab_no"),
                        OrgName = sqlCmd.GetString("name"),
                        OrgId = MedServiceOrgId.Value,
                        DriverLicense = sqlCmd.GetString("driver_license")
                    };
                }
                else
                {
                    return null;
                }
            }
        }
        private static ME_Employee GetME_Employee(int idObject)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = @"
SELECT e.gid, e.lastname, e.firstname, e.middlename, e.birthday, e.tab_no, co.name, e.driver_license
FROM autobase.employees e, autobase.orgs co 
WHERE co.gid = e.org_id AND e.gid = " + idObject.ToString() + ";";
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    return new ME_Employee()
                    {
                        ID = sqlCmd.GetInt32("gid"),
                        LastName = sqlCmd.GetString("lastname"),
                        FirstName = sqlCmd.GetString("firstname"),
                        MiddleName = sqlCmd.GetString("middlename"),
                        Birthday = sqlCmd.GetValue<DateTime>("birthday"),
                        TabNomer = sqlCmd.GetString("tab_no"),
                        OrgName = sqlCmd.GetString("name"),
                        OrgId = MedServiceOrgId.Value,
                        DriverLicense = sqlCmd.GetString("driver_license")
                    };
                }
                else
                {
                    return null;
                }
            }
        }
        private static List<HistoryInfo> LoadEmployeesChangesFromDB()
        {
            List<HistoryInfo> changes = new List<HistoryInfo>();
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = @"SELECT h.id_history, h.type_operation, h.id_object,
(SELECT ep.group_id=3 FROM autobase.employees_positions ep WHERE ep.gid = e.position_id) as driver,
(e.org_id = " + IdOrg.ToString() + @") as org_true
FROM sys_scheme.get_history_list_by_object(203) h
INNER JOIN autobase.waybills_med_checks_org_params lu ON h.data_changes>lu.lastupdate AND lu.org_id = " + IdOrg.ToString() + @"
LEFT JOIN autobase.employees_history e ON e.id_history =h.id_history
ORDER BY id_history;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    if (sqlCmd.GetValue<bool>("driver") && sqlCmd.GetValue<bool>("org_true"))
                    {
                        changes.Add(new HistoryInfo()
                        {
                            IdHistory = sqlCmd.GetInt32("id_history"),
                            TypeOpertaion = sqlCmd.GetInt32("type_operation"),
                            IdObject = sqlCmd.GetInt32("id_object")
                        });
                    }
                }
            }
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "UPDATE autobase.waybills_med_checks_org_params SET lastupdate = now() WHERE org_id = " + IdOrg.ToString() + ";";
                sqlCmd.ExecuteNonQuery();
            }
            return changes;
        }
        #endregion EmployeeSync
        #region WayBillSync
        public void WayBillSyncMain(string tab_no)
        {
            if (_med_service == null || !_med_service.Alive)
            {
                if (string.IsNullOrEmpty(UrlRest))
                {
                    _med_service = new MedServiceWSDL();
                    _med_service.Auth(Url, Login, Pwd);
                }
                else
                {
                    if (!MedServiceOrgId.HasValue)
                    {
                        MessageBox.Show("У определен идентификатор организации в медсервисе!", "Прекращение операции", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    _med_service = new MedServiceREST();
                    _med_service.Auth(UrlRest, Login, Pwd);
                }
            }
            var list = _med_service.GetWaybills(GetLastUpdateMedCheks(), new ME_Employee() { TabNomer = tab_no });
            //list = list.Where(w => w.EmployeeId.ToUpper() == tab_no.ToUpper()).ToList();
            InsertToBD(list);
            Console.WriteLine(list.Count.ToString());
        }

        private void InsertToBD(List<MedCheck> listToMe)
        {
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                foreach (var item in listToMe)
                {
                    List<Params> param = new List<Params>();
                    param.Add(new Params("@tab_no", item.EmployeeId, NpgsqlTypes.NpgsqlDbType.Text));
                    param.Add(new Params("@check_date", item.CheckDate, NpgsqlTypes.NpgsqlDbType.Timestamp));
                    param.Add(new Params("@check_status", item.CheckStatus, NpgsqlTypes.NpgsqlDbType.Integer));
                    param.Add(new Params("@doctor_name", item.DoctorName, NpgsqlTypes.NpgsqlDbType.Text));
                    param.Add(new Params("@check_type", item.CheckType, NpgsqlTypes.NpgsqlDbType.Integer));
                    param.Add(new Params("@ecp", item.ECP, NpgsqlTypes.NpgsqlDbType.Text));
                    param.Add(new Params("@org_id", EmployeesSync.IdOrg, NpgsqlTypes.NpgsqlDbType.Integer));

                    sqlCmd.sql = @"SELECT autobase.insert_waybill_med_check(@tab_no, @check_date, @check_status, @doctor_name, @check_type, @ecp, @org_id);";
                    sqlCmd.ExecuteNonQuery(param);
                }
            }
        }
        private DateTime GetLastUpdateMedCheks()
        {
            if (_med_service is MedServiceWSDL)
                return new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day);
            else
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }
        #endregion
    }
}
