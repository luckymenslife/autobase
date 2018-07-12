using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Rekod.Controllers;

namespace Rekod.Repository.SettingsFile
{

    [XmlRootAttribute("Layer")]
    public class RastrXml_M
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsExternal { get; set; }
        public string Description { get; set; }
        public bool IsHidden { get; set; }
        public bool UseBounds { get; set; }
        public int MinScale { get; set; }
        public int MaxScale { get; set; }
        public bool BuildPyramids { get; set; }
        public int MethodUse { get; set; }
    }
}
