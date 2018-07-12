using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rekod.DataAccess.SourceRastr.Model
{
    [XmlRootAttribute("TMSRastr")]
    public class TMSRastrM : IRastrXml
    {
        public ERastrXmlType Type { get { return ERastrXmlType.TMS; } }
        public string Url { get; set; }
        public string LayerName { get; set; }
        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }
        public string Proj { get; set; }
        public int TileSize { get; set; }
        public string CacheFolder { get; set; }
        public Extent TMSExtent { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrWhiteSpace(Url) || !string.IsNullOrWhiteSpace(Proj));
        }
    }
    public class Extent
    {
        public double a_x { get; set; }
        public double a_y { get; set; }
        public double b_x { get; set; }
        public double b_y { get; set; }
    }
}
