using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService.ModelsREST
{
    public class DriverJsonM
    {
        public string id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string patronymic { get; set; }
        public string org_name { get; set; }
        public string org { get; set; }
        public string year { get; set; }
        public string license { get; set; }
    }
    public class DriverDetailJsonM
    {
        public string id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string patronymic { get; set; }
        public string org { get; set; }
        public string org_id { get; set; }
    }
}
