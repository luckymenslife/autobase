using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WrapperMaplib.Wrapper.Geometry;
using WrapperMaplib.Wrapper.Map;
using WrapperMaplib.Wrapper.Style;

namespace GBU_Waybill_plugin.MTClasses.Tasks
{
    public partial class CreateTaskWayBillForm : Form
    {
        string _projMap;
        TasksWayBillVM _data;
        VectorLayer _layer_odh;
        VectorLayer _layer_zone;
        int _id_wb;
        int _id_org;

        public CreateTaskWayBillForm(int id_wb, int id_org)
        {
            InitializeComponent();
            _id_wb = id_wb;
            _id_org = id_org;
        }

        private void CreateTaskForm_Load(object sender, EventArgs e)
        {
            string text = "";
            try
            {
                //maplibControl1.SetApplicationParameter("BoundaryZoom", "1");
                text = MainPluginClass.Work.OpenForm.ProcOpen("TaskCreate");
                _projMap = "+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs";
                maplibControl1.CreateMap(_projMap, "id", 50000, 0, 0);
                AddExternalRastrLayer();
                GoToMoscow();
                AddLayerOdh();
                AddLayerZone();
                maplibControl1.Maplib.Fastmode = true;
                _data = new TasksWayBillVM(_id_wb, _id_org, MainPluginClass._type_task);
                _data.LayerOdh = _layer_odh;
                _data.LayerZone = _layer_zone;
                _data.Map = maplibControl1.Maplib;
                _data.MapControl = maplibControl1;
                _data.SetSelected();
                _data.PropertyChanged += _data_PropertyChanged;
                this.taskAttrV1.DataContext = _data;
                this.Text = _data.Title;
            }
            finally
            {
                MainPluginClass.Work.OpenForm.ProcClose(text);
            }
        }

        private void _data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "close")
            {
                Close();
            }
        }

        private void CreateTaskForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _data.RemoveHandler();
            _data.RemoveTypeSelectedHandler();
            _data.RemoveZoneSelectedHandler();
            maplibControl1.Maplib.CloseMap();
        }
        private void AddExternalRastrLayer()
        {
            string projMercator = "+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs";
            //string projMap = "+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs ";
            vBboxd box = new vBboxd(new double[]
                        {
                            -2.003750834E7,
                            -2.003750834E7,
                            2.003750834E7,
                            2.003750834E7
                        });
            string name = "cosmosshort";
            var imageLoaded = maplibControl1.Maplib.Map.AddImageLayerTMS("http://vec02.maps.yandex.net/tiles?l=map&v=4.3.3&x=$(x)&y=$(y)&z=$(z)&lang=ru_RU", 
                name, name, 0, 20, ref box, projMercator, 256, "cache", _projMap);
            //bool res = maplibControl1.Maplib.BindScale("cosmosshort");
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
        private void AddLayerZone()
        {
            _layer_zone = maplibControl1.Maplib.Map.CreateLayer("zones", new string[] { "id", "name" });
            _layer_zone.Encoding = "UTF-8";
            _layer_zone.Editable = true;
            _layer_zone.Uniform = true;
            var sld_layer = SldLayer.create(maplibControl1.Maplib, "zones_sld", _layer_zone, "sld_zone.xml");
            maplibControl1.Maplib.Map.Layers.Add(sld_layer);

            while (maplibControl1.Maplib.Map.Layers.MoveUp(sld_layer)) ;
            _layer_zone.Enabled = false;
        }
        private void GoToMoscow()
        {
            vBboxd bMoscow = new vBboxd(4169026.83, 7453277.59, 4206486.94, 7495036.48);
            maplibControl1.Maplib.Extent = bMoscow;
        }
    }
}
