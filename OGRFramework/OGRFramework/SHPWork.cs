using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace OGRFramework
{
    public partial class SHPWork
    {
        private FileInfo inputFile;
        public SHPWork(FileInfo inputFile)
        {
            this.inputFile = inputFile;
        }
        private string DriverName
        {
            get
            {
                if (inputFile.Extension.ToLower() == ".shp")
                    return "ESRI Shapefile";
                if (inputFile.Extension.ToLower() == ".tab" || inputFile.Extension.ToLower() == ".mif")
                    return "MapInfo File";
                if (inputFile.Extension.ToLower() == ".bna")
                    return "BNA";
                if (inputFile.Extension.ToLower() == ".csv")
                    return "CSV";
                if (inputFile.Extension.ToLower() == ".geojson")
                    return "GeoJSON";
                if (inputFile.Extension.ToLower() == ".gml")
                    return "GML";
                if (inputFile.Extension.ToLower() == ".gmt")
                    return "GMT";
                if (inputFile.Extension.ToLower() == ".itf")
                    return "Interlis 1";
                if (inputFile.Extension.ToLower() == ".kml")
                    return "KML";
                if (inputFile.Extension.ToLower() == ".sqlite")
                    return "SQLite";
                if (inputFile.Extension.ToLower() == ".gxt")
                    return "Geoconcept";
                if (inputFile.Extension.ToLower() == ".xml")
                    return "GeoRSS";
                if (inputFile.Extension.ToLower() == ".dxf")
                    return "DXF";
                return "";
            }
        }
        private string LayerName 
        { get { return inputFile.Name.Substring(0, inputFile.Name.LastIndexOf(inputFile.Extension)); } }
    }
}