using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses
{
    [DataContractAttribute]
    public class MT_CarShortReportModel
    {
        [DataMemberAttribute]
        public double length { get; set; }
        [DataMemberAttribute]
        public double fuel { get; set; }
    }

    [DataContractAttribute]
    public class MT_CarWayBill
    {
        [DataMemberAttribute]
        public long carId { get; set; }
        [DataMemberAttribute]
        public double distance { get; set; }
        [DataMemberAttribute]
        public long dateFrom { get; set; }
        [DataMemberAttribute]
        public long dateTill { get; set; }
    }

    [DataContractAttribute]
    public class MT_CarsTask
    {
        [DataMemberAttribute]
        public long id { get; set; }
        [DataMemberAttribute]
        public String description { get; set; }
        [DataMemberAttribute]
        public long from { get; set; }
        [DataMemberAttribute]
        public long till { get; set; }
        [DataMemberAttribute]
        public long? routeId { get; set; }
        [DataMemberAttribute]
        public long? zoneId { get; set; }
        [DataMemberAttribute]
        public long? typeId { get; set; }
        [DataMemberAttribute]
        public List<long> carIds { get; set; }
        [DataMemberAttribute]
        public int status { get; set; }
    }

    [DataContractAttribute]
    public class MT_GaugeType
    {
        [DataMemberAttribute]
        public long id { get; set; }
        [DataMemberAttribute]
        public String name { get; set; }
        [DataMemberAttribute]
        public bool? dinAlarm { get; set; }
        [DataMemberAttribute]
        public String unitName { get; set; }
        [DataMemberAttribute]
        public long? port { get; set; }
        [DataMemberAttribute]
        public long? systemType { get; set; }
        [DataMemberAttribute]
        public long? categoryId { get; set; }
    }

    [DataContractAttribute]
    public class MT_Gauge
    {
        [DataMemberAttribute]
        public long carId { get; set; }
        [DataMemberAttribute]
        public long? glonassId { get; set; }
        [DataMemberAttribute]
        public long? port { get; set; }
        [DataMemberAttribute]
        public string name { get; set; }
        [DataMemberAttribute]
        public long? categoryId { get; set; }
        [DataMemberAttribute]
        public long? typeId { get; set; }
        [DataMemberAttribute]
        public bool? analogToDin { get; set; }
        [DataMemberAttribute]
        public bool? din { get; set; }
        [DataMemberAttribute]
        public bool? dinInvert { get; set; }
        [DataMemberAttribute]
        public double? analog { get; set; }
        [DataMemberAttribute]
        public long? updateData { get; set; }
        [DataMemberAttribute]
        public long id { get; set; }
        [DataMemberAttribute]
        public double? analogMin { get; set; }
        [DataMemberAttribute]
        public double? analogMax { get; set; }
        [DataMemberAttribute]
        public double? analogStep { get; set; }
        [DataMemberAttribute]
        public bool? analogAlarm { get; set; }
        [DataMemberAttribute]
        public bool? dinAlarm { get; set; }
    }

    [DataContractAttribute]
    public class MT_GaugeValue
    {
        [DataMemberAttribute]
        public long date { get; set; }
        [DataMemberAttribute]
        public double value { get; set; }
    }

    [DataContractAttribute]
    public class MT_TaskType
    {
        [DataMemberAttribute]
        public long id { get; set; }
        [DataMemberAttribute]
        public String name { get; set; }
    }

    public class CarInfoM
    {
        public int id_ME;
        public int id_MT;
        public int glonass_id;
    }
    public enum ETypeTask
    {
        all = 0,
        route = 1,
        zone = 2
    }
}