using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace Rekod
{
    public partial class layersManagerFRMRastr
    {
        //private layersManagerFRM basefrm1;

        private List<layerRastrM> list1;

        public layersManagerFRMRastr()
        {

            list1 = new List<layerRastrM>();

            loadRastrReestr();

        }
        public Rekod.DBLayerForms.RastrInfo[] GetRastrLayers()
        {
            var nodeCollection = new List<Rekod.DBLayerForms.RastrInfo>();
//            String roscosmos =
//                    @"<?xml version=""1.0"" encoding=""utf-8""?>
//                        <TMSRastr>
//                        <Url>http://geoportal.ntsomz.ru/get_tile_external.php?x=$(x)&amp;y=$(y)&amp;scale=$(z)</Url>
//                        <LayerName>roscosmos</LayerName>
//                        <MinZoom>0</MinZoom>
//                        <MaxZoom>16</MaxZoom>
//                        <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
//                        <TileSize>256</TileSize>
//                        <CacheFolder>cache/roscosmos_cache</CacheFolder>
//                        <TMSExtent>
//                          <a_x>-2.003750834E7</a_x>
//                          <a_y>-2.003750834E7</a_y>
//                          <b_x>2.003750834E7</b_x>
//                          <b_y>2.003750834E7</b_y>
//                        </TMSExtent>
//                        </TMSRastr>";
//            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Карта Роскосмоса", "roscosmos.xml", true, false, 0) { Content = roscosmos });
            String basemap =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <TMSRastr>
	                        <Url>http://basemap.rekod.ru/worldmap/$(z)/$(x)/$(y).png</Url>
	                        <LayerName>worldmap</LayerName>
	                        <MinZoom>0</MinZoom>
	                        <MaxZoom>18</MaxZoom>
	                        <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
	                        <TileSize>256</TileSize>
	                        <CacheFolder>cache/basemap_cache</CacheFolder>
	                        <TMSExtent>
	                          <a_x>-2.003750834E7</a_x>
	                          <a_y>-2.003750834E7</a_y>
	                          <b_x>2.003750834E7</b_x>
	                          <b_y>2.003750834E7</b_y>
	                        </TMSExtent>
                        </TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Карта России", "basemap.xml", true, false, 0) { Content = basemap });

            String osm =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <TMSRastr>
                          <Url>http://a.tile.openstreetmap.org/$(z)/$(x)/$(y).png</Url>
                          <LayerName>osm</LayerName>
                          <MinZoom>0</MinZoom>
                          <MaxZoom>18</MaxZoom>
                          <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
                          <TileSize>256</TileSize>
                          <CacheFolder>cache/osm_cache</CacheFolder>
                          <TMSExtent>
                            <a_x>-2.003750834E7</a_x>
                            <a_y>-2.003750834E7</a_y>
                            <b_x>2.003750834E7</b_x>
                            <b_y>2.003750834E7</b_y>
                          </TMSExtent>
                        </TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("OpenStreetMap", "osm.xml", true, false, 0) { Content = osm });

            String osm_roads =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://openmapsurfer.uni-hd.de/tiles/roads/x=$(x)&amp;y=$(y)&amp;z=$(z)</Url>
  <LayerName>osm_roads</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/osm_roads</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";

            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("OpenStreetMap Roads", "osm_roads.xml", true, false, 0) { Content = osm_roads });
            

            String mapquestMap =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://ttiles01.mqcdn.com/tiles/1.0.0/vy/map/$(z)/$(x)/$(y).png</Url>
  <LayerName>mapquest-map</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/mapquest_map_cache</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("MapQuest Карта", "mapquest-map.xml", true, false, 0) { Content = mapquestMap });


            String mapquestSat =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://ttiles01.mqcdn.com/tiles/1.0.0/vy/sat/$(z)/$(x)/$(y).png</Url>
  <LayerName>mapquest-sat</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>13</MaxZoom>
  <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/mapquest_sat_cache</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("MapQuest Спутник", "mapquest-sat.xml", true, false, 0) { Content = mapquestSat });

            String mapquestLabel =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://ttiles01.mqcdn.com/tiles/1.0.0/vy/hyb/$(z)/$(x)/$(y).png</Url>
  <LayerName>mapquest-labels</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +units=m +k=1.0 +nadgrids=@null +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/mapquest_hyb_cache</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("MapQuest Подписи", "mapquest-labels.xml", true, false, 0) { Content = mapquestLabel });

            String yandex_map =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://vec02.maps.yandex.net/tiles?l=map&amp;v=4.3.3&amp;x=$(x)&amp;y=$(y)&amp;z=$(z)&amp;lang=ru_RU</Url>
  <LayerName>yandex map</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/yandex_map</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Яндекс Карта", "yandex_map.xml", true, false, 0) { Content = yandex_map });

            String yandex_sat =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://sat02.maps.yandex.net/tiles?l=sat&amp;v=3.132.0&amp;x=$(x)&amp;y=$(y)&amp;z=$(z)&amp;lang=ru_RU</Url>
  <LayerName>yandex-sat</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/yandex_sat</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Яндекс Спутник", "yandex_sat.xml", true, false, 0) { Content = yandex_sat });

            String yandex_label =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<TMSRastr>
  <Url>http://vec02.maps.yandex.net/tiles?l=skl&amp;v=4.4.9&amp;x=$(x)&amp;y=$(y)&amp;z=$(z)&amp;lang=ru_RU</Url>
  <LayerName>yandex-label</LayerName>
  <MinZoom>0</MinZoom>
  <MaxZoom>18</MaxZoom>
  <Proj>+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs</Proj>
  <TileSize>256</TileSize>
  <CacheFolder>cache/yandex_label</CacheFolder>
  <TMSExtent>
    <a_x>-2.003750834E7</a_x>
    <a_y>-2.003750834E7</a_y>
    <b_x>2.003750834E7</b_x>
    <b_y>2.003750834E7</b_y>
  </TMSExtent>
</TMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Яндекс Подписи", "yandex_label.xml", true, false, 0) { Content = yandex_label });



            String rosreestr_map =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<TWMSRastr>
  <Url>http://maps.rosreestr.ru/ArcGIS/rest/services/CadastreNew/Cadastre/MapServer/export?dpi=96&amp;transparent=true&amp;format=png32&amp;bbox=$(box)&amp;bboxSR=$(boxsrid)&amp;imageSR=$(imagesrid)&amp;size=$(width),$(height)&amp;f=image</Url>
  <Layers>
    <TWMSLayerM>
      <LayerName>RosReester</LayerName>
      <StyleName/>
    </TWMSLayerM>
  </Layers>
  <SRID>3395</SRID>
  <TileSize>256</TileSize>
  <ZoomCount>20</ZoomCount>
  <CacheFolder>cache/RosReester</CacheFolder>
  <TWMSExtent>
    <a_x>-20037508.3428</a_x>
    <a_y>-15496570.7397</a_y>
    <b_x>20037508.3428</b_x>
    <b_y>18764656.2314</b_y>
  </TWMSExtent>
</TWMSRastr>";
            nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo("Росреестр", "rosreestr.xml", true, false, 0) { Content = rosreestr_map });



            foreach (layerRastrM ll in list1)
            {
                nodeCollection.Add(new Rekod.DBLayerForms.RastrInfo(ll.Name, ll.Path, ll.IsExternal, ll.BuildPyramids, ll.MethodUse, ll.Usebounds, ll.Minscale, ll.Maxscale));
                //layersManager1.addLayer(ll.Name, -1, false, false, false, true, true, ll.Path);
                //layersManager1.addLayer(name, id, edit, visible, loaded, BaseLayer, vectorlayer, rastrPath);
            }
            return nodeCollection.ToArray();
        }

        private void loadRastrReestr()
        {
            list1.Clear();
            foreach (var item in Program.SettingsXML.Rasters)
            {
                if (!string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Path))
                {
                    list1.Add(new layerRastrM(item.Name, item.Path, item.IsExternal) { Usebounds = item.UseBounds, 
                        Maxscale = item.MaxScale, 
                        Minscale = item.MinScale, 
                        MethodUse = item.MethodUse,
                        BuildPyramids = item.BuildPyramids
                    });
                }
            }
        }

    }
}