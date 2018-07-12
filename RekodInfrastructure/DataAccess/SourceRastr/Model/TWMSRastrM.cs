using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rekod.DataAccess.SourceRastr.Model
{
    [XmlRootAttribute("TWMSRastr")]
    public class TWMSRastrM : IRastrXml
    {
        public ERastrXmlType Type { get { return ERastrXmlType.TWMS; } }
        public string Url { get; set; }
        public List<TWMSLayerM> Layers { get; set; }

        public int TileSize { get; set; }
        public int ZoomCount { get; set; }
        public string CacheFolder { get; set; }
        public Extent TWMSExtent { get; set; }


        public bool IsValid()
        {
            return (!string.IsNullOrWhiteSpace(Url));
        }
    }
    public class TWMSLayerM
    {
        public string LayerName { get; set; }
        public string StyleName { get; set; }
    }

}
