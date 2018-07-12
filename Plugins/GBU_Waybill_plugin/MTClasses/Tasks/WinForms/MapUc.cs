using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WrapperMaplib.Wrapper.Map;
using WrapperMaplib.Wrapper.Geometry;
using WrapperMaplib.Wrapper;
using WrapperMaplib;

namespace GBU_Waybill_plugin.MTClasses.Tasks.WinForms
{
    public partial class MapUc : UserControl
    {
        private VectorLayer _layer_odh;
        private VectorLayer _layer_zones;
        string _projMap;


        public MapUc()
        {
            InitializeComponent();
        }

        public VectorLayer LayerOdh
        {
            get { return _layer_odh; }
        }
        public VectorLayer LayerZones
        {
            get { return _layer_zones; }
        }
        public trWin Map
        {
            get { return this.maplibControl1.Maplib; }
        }

        public MaplibControl MapControl
        {
            get { return maplibControl1; }
        }

        private void MapUc_Load(object sender, EventArgs e)
        {
            try
            {
                //maplibControl1.SetApplicationParameter("BoundaryZoom", "1");
                _projMap = "+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs";
                maplibControl1.CreateMap(_projMap, "id", 50000, 0, 0);
                AddExternalRastrLayer();
                AddLayerOdh();
                AddLayerZones();
                GoToMoscow();
                maplibControl1.Maplib.Fastmode = true;
                MainPluginClass.AppEvents.OnPropertyChanged("CreateLayerOdh");
                MainPluginClass.AppEvents.OnPropertyChanged("CreateLayerZones");
            }
            finally
            {
            }
        }
        private void GoToMoscow()
        {
            vBboxd bMoscow = new vBboxd(4169026.83, 7453277.59, 4206486.94, 7495036.48);
            maplibControl1.Maplib.Extent = bMoscow;
        }
        private void AddExternalRastrLayer()
        {
            vBboxd box = new vBboxd(new double[]
                        {
                            -2.003750834E7,
                            -2.003750834E7,
                            2.003750834E7,
                            2.003750834E7
                        });
            string name = "cosmosshort";
            var imageLoaded = maplibControl1.Maplib.Map.AddImageLayerTMS("http://vec02.maps.yandex.net/tiles?l=map&v=4.3.3&x=$(x)&y=$(y)&z=$(z)&lang=ru_RU",
                name, name, 0, 18, ref box, _projMap, 256, "cache", _projMap);
            //bool res = Map.BindScale("cosmosshort");
        }
        private void AddLayerOdh()
        {
            _layer_odh = maplibControl1.Maplib.Map.CreateLayer("odhs", new string[] { "id", "name" });
            _layer_odh.Encoding = "UTF-8";
            _layer_odh.Editable = true;
            _layer_odh.Uniform = true;
            var sld_layer = SldLayer.create(maplibControl1.Maplib, "odhs_sld", _layer_odh, "sld.xml");
            maplibControl1.Maplib.Map.Layers.Add(sld_layer);

            while (maplibControl1.Maplib.Map.Layers.MoveUp(sld_layer)) ;
            _layer_odh.Enabled = false;
        }
        private void AddLayerZones()
        {
            _layer_zones = maplibControl1.Maplib.Map.CreateLayer("zones", new string[] { "id", "name" });
            _layer_zones.Encoding = "UTF-8";
            _layer_zones.Editable = true;
            _layer_zones.Uniform = true;
            var sld_layer = SldLayer.create(maplibControl1.Maplib, "zones_sld", _layer_zones, "sld_zone.xml");
            maplibControl1.Maplib.Map.Layers.Add(sld_layer);

            while (maplibControl1.Maplib.Map.Layers.MoveUp(sld_layer)) ;
            _layer_zones.Enabled = false;
        }
    }
}
