using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rekod.DataAccess.SourceRastr.Model
{
    [XmlRootAttribute("WMSRastr")]
    public class WMSRastrM : IRastrXml
    {
        public ERastrXmlType Type { get { return ERastrXmlType.WMS; } }
        public string ServerUrl { get; set; }
        public ObservableCollection<WMSLayerM> Layers { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrWhiteSpace(ServerUrl) || Layers != null || Layers.Count > 0);
        }
    }
    public class WMSLayerM
    {
        public string LayerName { get; set; }
        public string StyleName { get; set; }
    }
}
