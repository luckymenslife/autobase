using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rekod.Repository.SettingsFile
{
    public enum eProxyStatus { Non = 0, Customer }
    [XmlRootAttribute("Proxy")]
    public class ProxyParameters_M
    {
        public eProxyStatus Status { get; set; }
        public string IP { get; set; }
        public int? Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public static string Guid = "E3585449-C474-4476-A2AC-99D0039127C3";
    }
}
