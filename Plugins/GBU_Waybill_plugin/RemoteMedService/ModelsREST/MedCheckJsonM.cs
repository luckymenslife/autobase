using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.RemoteMedService.ModelsREST
{
    public class MedCheckJsonM
    {
        public string id { get; set; }
        public string date { get; set; }
        public string user { get; set; }
        public string medUser { get; set; }
        public string type { get; set; }
        public string medResult { get; set; }
        public string eds { get; set; }
        public string comment { get; set; }
    }
}
