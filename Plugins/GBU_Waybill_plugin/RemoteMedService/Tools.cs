using GBU_Waybill_plugin.pro.mpmo.ln;
using GBU_Waybill_plugin.RemoteMedService.ModelsREST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class Tools
    {
        public static long GetUnixTime(DateTime winTime)
        {
            return (long)(winTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }
        public static DateTime GetWinTime(long unixTime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }
        public static MedCheck ConvertToMedCheck(Waybill wb)
        {
            MedCheck result = new MedCheck();
            result.CheckDate = GetWinTime(wb.time);
            result.CheckType = GetCheckTypeWSDL(wb.beforeAfter);
            result.CheckStatus = GetCheckStatusWSDL(wb.access);
            result.EmployeeId = wb.employeeId;
            result.DoctorName = wb.medicFullname;
            result.ECP = wb.EDS;
            return result;
        }
        public static MedCheck ConvertToMedCheck(MedCheckJsonM medcheckjson, string tab_no)
        {
            MedCheck medcheck = new MedCheck();
            medcheck.CheckType = GetCheckTypeREST(medcheckjson.type);
            medcheck.CheckDate = Convert.ToDateTime(medcheckjson.date);
            medcheck.CheckStatus = GetCheckStatusREST(medcheckjson.medResult);
            medcheck.DoctorName = medcheckjson.medUser;
            medcheck.DoctorPost = medcheckjson.comment;
            medcheck.ECP = medcheckjson.eds;
            medcheck.EmployeeId = tab_no;
            return medcheck;
        }
        public static ME_Employee ConvertToME_Employee(Employee employee)
        {
            ME_Employee result = new ME_Employee();
            result.FullName = employee.fullName;
            result.Birthday = employee.birthday.HasValue? employee.birthday.Value: DateTime.MinValue;
            result.FirstName = employee.firstName;
            result.LastName = employee.lastName;
            result.MiddleName = employee.middleName;
            result.TabNomer = employee.employeeId;
            return result;
        }
        public static ME_Employee ConvertToME_Employee(DriverJsonM item)
        {
            ME_Employee result = new ME_Employee();
            result.FullName = String.Format("{0} {1} {2}", item.surname, item.name, item.patronymic);
            result.Birthday = DateTime.MinValue;
            result.FirstName = item.name;
            result.LastName = item.surname;
            result.MiddleName = item.patronymic;
            result.TabNomer = item.number;
            return result;
        }
        internal static ME_Employee ConvertToME_Employee(DriverDetailJsonM item)
        {
            ME_Employee result = new ME_Employee();
            result.FullName = String.Format("{0} {1} {2}", item.surname, item.name, item.patronymic);
            result.Birthday = DateTime.MinValue;
            result.FirstName = item.name;
            result.LastName = item.surname;
            result.MiddleName = item.patronymic;
            result.TabNomer = item.number;
            return result;
        }
        public static string Win1251ToUTF8(string source)
        {

            Encoding utf8 = Encoding.GetEncoding("utf-8");
            Encoding win1251 = Encoding.GetEncoding("windows-1251");

            byte[] win1251Bytes = win1251.GetBytes(source);
            byte[] utf8Bytes = Encoding.Convert(win1251, utf8, win1251Bytes);
            source = win1251.GetString(utf8Bytes);
            return source;
        }
        public static string UTF8ToWin1251(string source)
        {

            Encoding utf8 = Encoding.GetEncoding("utf-8");
            Encoding win1251 = Encoding.GetEncoding("windows-1251");

            byte[] utf8Bytes = utf8.GetBytes(source);
            byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
            source = utf8.GetString(win1251Bytes);
            return source;
        }
        public static int GetCheckTypeWSDL(string type)
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
        public static int GetCheckStatusWSDL(string status)
        {
            switch (status)
            {
                case "прошел":
                    return 1;
                default:
                    return 2;
            }
        }

        public static int GetCheckTypeREST(string type)
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
        public static int GetCheckStatusREST(string status)
        {
            switch (status)
            {
                case "True":
                    return 1;
                default:
                    return 2;
            }
        }
    }
}
