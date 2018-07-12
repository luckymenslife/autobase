using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService
{
    public class ME_Employee
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime Birthday { get; set; }
        public String OrgName { get; set; }
        public string TabNomer { get; set; }
        public int OrgId { get; set; }
        public string DriverLicense { get; set; }
    }
    public class HistoryInfo
    {
        public int IdHistory { get; set; }
        public int TypeOpertaion { get; set; }
        public int IdObject { get; set; }
    }
    //[ { "id": "4", "name": "ГБУ Жилищник района Мещанский", "main": "True", "link": "" } ]
    public class Org
    {
        public string id { get; set; }
        public string name { get; set; }
        public string main { get; set; }
        public string link { get; set; }
    }
}
