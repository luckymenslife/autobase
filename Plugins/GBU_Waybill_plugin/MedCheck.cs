using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin
{
    public class MedCheck
    {
        public string EmployeeId { get; set; }
        public DateTime CheckDate { get; set; }
        public int CheckStatus { get; set; }
        public String DoctorName { get; set; }
        public String DoctorPost { get; set; }
        public String ECP { get; set; }
        public int CheckType { get; set; }
    }
}
