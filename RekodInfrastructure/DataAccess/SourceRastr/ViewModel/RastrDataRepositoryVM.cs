using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Interfaces;
using Npgsql;
using Rekod.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.DataAccess.AbstractSource;
using RasM = Rekod.DataAccess.SourceRastr.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using TmM = Rekod.DataAccess.TableManager.Model;
using System.Windows;
using Rekod.Services;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using Rekod.Controllers;
using Rekod.Behaviors;

namespace Rekod.DataAccess.SourceRastr.ViewModel
{
    /// <summary>
    /// Класс работы с растровым источником
    /// </summary>
    public class RastrDataRepositoryVM : AbsVM.DataRepositoryVM
    {
        private static int rastrLayerId = 1;
        //TODO: (Сергей) нужно отделить этот класс от RastrRepositoryV, не должен данный класс наследоватся от WindowViewModelBase_VM

        #region Поля
        private List<RasM.RastTableBaseM> _defaultRastrs;
        private string _pathFile;
        private ICommand _deleteLayerCommand;
        private ICommand _addLayerCommand;
        private ICommand _saveLayersCommand;
        private ICommand _updateLayersCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Возвращает или устанавливает путь до конфигурационного xml файла
        /// </summary>
        public String PathFile
        {
            get { return _pathFile; }
            set { OnPropertyChanged(ref _pathFile, value, () => this.PathFile); }
        }

        public override string Title
        {
            get { return _title; }
            set { OnPropertyChanged(ref _title, value, () => this.Title); }
        }
        #endregion // Свойства

        #region Конструкторы
        public RastrDataRepositoryVM(TmM.ITableManagerVM source, string pathFile)
            : base(source, AbsM.ERepositoryType.Rastr, false)
        {
            _pathFile = pathFile;
            Title = Properties.Resources.LocRastrConfig;
            Text = Properties.Resources.LocSourceRastr;
            _defaultRastrs = GetDefaultLayers();

            var group = new AbsM.GroupM(this, 0) { Text = "Группа растровых слоев" };
            var group2 = new AbsM.GroupM(this, 1) { Text = "Группа статичных растровых слоев" };
            _groups.Add(group);
            _groups.Add(group2);

            ReloadInfo();
        }
        #endregion // Конструкторы

        #region Методы
        /// <summary>
        /// Добавить новый слой в коллекцию источника
        /// </summary>
        /// <param name="newrastrlayerproxy">Слой который нужно добавить</param>
        public void AddLayer(object newrastrlayerproxy = null)
        {
            if (!(newrastrlayerproxy is BindingProxy))
            {
                return;
            }

            (newrastrlayerproxy as BindingProxy).Data = null;
            (newrastrlayerproxy as BindingProxy).Data = new Model.RastTableBaseM(this, AbsM.ETableType.Rastr);
        }
        /// <summary>
        /// Создает новый слой в источнике
        /// </summary>
        /// <param name="fileName"></param>
        public RasM.RastTableBaseM AddLayer(String filePath)
        {
            RasM.RastTableBaseM rastrTable = null;
            String fileName = filePath;
            bool fileExists = false;
            fileExists = fileName != "" && System.IO.File.Exists(fileName);
            if (fileExists)
            {
                if (fileName.StartsWith(System.Windows.Forms.Application.StartupPath))
                {
                    fileName = fileName.Replace(System.Windows.Forms.Application.StartupPath, ".");
                }
                var existingTables =
                    from AbsM.ITableBaseM iTable
                        in Tables
                    where
                        (iTable as RasM.RastTableBaseM).FilePath == fileName
                    select (iTable as RasM.RastTableBaseM);

                if (existingTables.Count() > 0)
                {
                    throw new Exception(String.Format("В системе уже существует слой который указывает на {0}", fileName));
                }
                else
                {
                    var fi = new FileInfo(filePath);
                    rastrTable = new RasM.RastTableBaseM(this, rastrLayerId++, fileName, AbsM.ETableType.Rastr);
                    String rastrNameBase = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                    bool nameExists = true;
                    int i = 1;
                    String rastrName = rastrNameBase;
                    while (nameExists)
                    {
                        nameExists = (from AbsM.TableBaseM rTable in Tables where rTable.Name == rastrName select rTable).Count() > 0;
                        if (nameExists)
                        {
                            rastrName = rastrNameBase + i++;
                        }
                    }
                    rastrTable.Name = rastrTable.Text = rastrName;
                    _tables.Add(rastrTable);
                    MGroupAddTable(_groups[0], rastrTable);
                }
            }
            return rastrTable;
        }
        public void SaveLayer(Model.RastTableBaseM newRastrLayer)
        {
            if (newRastrLayer != null)
            {
                if (newRastrLayer.IsNewTable)
                {
                    RasM.RastTableBaseM.SetIsNewTable(newRastrLayer, false);
                    _tables.Add(newRastrLayer);
                    Groups.ElementAt(0).Tables.Add(newRastrLayer);
                }
            }
        }



        #region Интерфейс IDisposable
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion // Интерфейс IDisposable

        #region Методы AbsVM.DataRepositoryVM
        /// <summary>
        /// Обновить метаданные
        /// </summary>
        public override void ReloadInfo()
        {
            UpdateRastrLayers();
        }
        /// <summary>
        /// Функция изменения видимости слоя. Возвращает видимость слоя после попытки изменения
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        internal override bool MakeLayerVisible(AbsM.TableBaseM table, bool? value)
        {
            var rastrTable = table as RasM.RastTableBaseM;
            bool isChecked = (bool)(value);
            if (!(table as RasM.RastTableBaseM).IsExternal)
            {
                if (isChecked)
                {
                    try
                    {
                        cti.ThreadProgress.ShowWait();

                        //todo задача #3646
                        _mv.LoadLayer(rastrTable.FilePath, true);
                        var imLayer = _mv.getImageLayer(rastrTable.FilePath);
                        if (imLayer != null)
                        {
                            imLayer.bgcolor = (uint)Program.bgMap;
                        }
                        return (imLayer != null);
                    }
                    finally
                    {
                        cti.ThreadProgress.Close();
                    }
                }
                else
                {
                    var imLayer = _mv.getImageLayer(rastrTable.FilePath);
                    if (imLayer != null)
                    {
                        _mv.deleteImageLayer(imLayer);
                    }
                }
            }
            else
            {
                if (isChecked)
                {
                    try
                    {
                        cti.ThreadProgress.ShowWait();
                        return _mv.LoadExternalImageLayer(rastrTable.FilePath, true);
                    }
                    finally
                    {
                        cti.ThreadProgress.Close();
                    }
                }
                else
                {
                    var imextLayer = _mv.GetExternalImageLayer(rastrTable.FilePath);
                    if (imextLayer != null)
                    {
                        return _mv.DeleteExternalImageLayer(rastrTable.FilePath);
                    }
                }
            }
            return isChecked;
        }
        /// <summary>
        /// Показать окно настроек таблицы
        /// </summary>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        public override void OpenTableSettings(AbsM.ITableBaseM iTable, UIElement positionElement = null)
        {
            RasM.RastTableBaseM rastrTable = iTable as RasM.RastTableBaseM;

            View.RastrLayerV rastrLayerV = new View.RastrLayerV();
            rastrLayerV.DataContext = new BindingProxy() { Data = rastrTable };
            Window window = new Window();
            window.Title = String.Format("Редактирование свойств слоя \"{0}\"", rastrTable.Text);
            window.Content = rastrLayerV;
            window.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Pie_Chart.ico", UriKind.Absolute));
            window.Height = 340;
            window.Width = 330;
            window.MinHeight = 340;
            window.MinWidth = 330;
            window.Owner = Program.WinMain;

            if (positionElement != null)
            {
                System.Windows.Point pt = positionElement.TranslatePoint(new System.Windows.Point(0, 0), Program.WinMain);
                window.Top = pt.Y;
                window.Left = pt.X;
            }
            window.Show();
        }
        #endregion // Методы AbsVM.DataRepositoryVM

        public AbsM.ITableBaseM FindTable(string filePath)
        {
            return _tables.Cast<RasM.RastTableBaseM>().FirstOrDefault(f => f.FilePath == filePath);
        }

        private List<RasM.RastTableBaseM> GetDefaultLayers()
        {
            var defaultRastrs = new List<RasM.RastTableBaseM>();
            RasM.RastTableBaseM table;
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
//            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
//            {
//                Text = "Карта Роскосмоса",
//                Name = "roscosmos.xml",
//                GeomType = AbsM.EGeomType.None,
//                IsExternal = true,
//                BuildPyramids = false,
//                Content = roscosmos
//            };
//            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "Карта России",
                Name = "basemap.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = basemap
            };
            defaultRastrs.Add(table);

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

            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "OpenStreetMap",
                Name = "osm.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = osm
            };
            defaultRastrs.Add(table);


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

            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "OpenStreetMap Roads",
                Name = "osm_roads.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = osm_roads
            };
            defaultRastrs.Add(table);


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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "MapQuest Карта",
                Name = "mapquest-map.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = mapquestMap
            };
            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "MapQuest Спутник",
                Name = "mapquest-sat.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = mapquestSat
            };
            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "MapQuest Подписи",
                Name = "mapquest-labels.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = mapquestLabel
            };
            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "Яндекс Карта",
                Name = "yandex_map.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = yandex_map
            };
            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "Яндекс Спутник",
                Name = "yandex_sat.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = yandex_sat
            };
            defaultRastrs.Add(table);

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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "Яндекс Подписи",
                Name = "yandex_label.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = yandex_label
            };
            defaultRastrs.Add(table);


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
            table = new RasM.RastTableBaseM(this, AbsM.ETableType.Rastr)
            {
                Text = "Росреестр",
                Name = "rosreestr.xml",
                GeomType = AbsM.EGeomType.None,
                IsExternal = true,
                BuildPyramids = false,
                Content = rosreestr_map
            };
            defaultRastrs.Add(table);

            return defaultRastrs;
        }
        #endregion // Методы

        #region Команды
        #region DeleteLayerCommand
        public ICommand DeleteLayerCommand
        {
            get { return _deleteLayerCommand ?? (_deleteLayerCommand = new RelayCommand(this.DeleteLayer, this.CanDeleteLayer)); }
        }
        /// <summary>
        /// Удаляет растровый слой из источника
        /// </summary>
        /// <param name="newrastrlayerproxy">Слой который нужно удалить</param>
        public void DeleteLayer(object commandeventpar = null)
        {
            ICollection layersCollection = null;
            var commPar = commandeventpar as Rekod.Behaviors.CommandEventParameter;
            if (commPar != null)
            {
                if ((commPar.EventArgs as KeyEventArgs).Key == Key.Delete)
                {
                    layersCollection = commPar.CommandParameter as ICollection;
                }
            }
            else
                layersCollection = commandeventpar as ICollection;

            if (layersCollection != null && layersCollection.Count > 0)
            {
                if (MessageBox.Show("Удалить растровые слои?",
                                        "Удаление слоев",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var rastrLayers = new List<RasM.RastTableBaseM>();
                    foreach (var layer in layersCollection)
                    {
                        rastrLayers.Add(layer as RasM.RastTableBaseM);
                    }
                    foreach (var rastrLayer in rastrLayers)
                    {
                        if (_tables.Contains(rastrLayer))
                        {
                            rastrLayer.IsVisible = false;
                            var mvLayer = Program.mainFrm1.axMapLIb1.getImageLayer(rastrLayer.FilePath);
                            if (mvLayer != null)
                            {
                                mvLayer.Visible = false;
                                Program.mainFrm1.axMapLIb1.deleteImageLayer(mvLayer);
                            }
                            _tables.Remove(rastrLayer);
                        }
                    }
                    rastrLayers.Clear();
                    SaveRastrLayers();
                }
            }
        }
        /// <summary>
        /// Возможно ли удалить растровый слой
        /// </summary>
        /// <param name="newrastrlayerproxy">Слой для которого осуществляется проверка</param>
        /// <returns></returns>
        public bool CanDeleteLayer(object commandeventpar = null)
        {
            bool result = false;
            if (commandeventpar is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commPar =
                    commandeventpar as Rekod.Behaviors.CommandEventParameter;
                commandeventpar = commPar.CommandParameter;
            }
            if (commandeventpar is IEnumerable)
            {
                ICollection coll = commandeventpar as ICollection;
                if (coll == null || coll.Count == 0)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion DeleteLayerCommand

        #region AddLayersCommand
        public ICommand AddLayersCommand
        {
            get { return _addLayerCommand ?? (_addLayerCommand = new RelayCommand(this.AddLayers)); }
        }
        public void AddLayers(object parameter = null)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Rekod.Properties.Resources.LocAllSupportedFormats + "|*.bmp;*.tif;*.tiff;*.jpg;*.png;*.xml;*.rwms;*.rtms;*.rtwms;*.gxml|Bitmap|*.bmp|GeoTIFF|*.tif|JPEG JFIF|*.jpg|Portable Network Graphics|*.png|External|*.xml|Standard xml files|*.rwms;*.rtms;*.rtwms|Gdal xml files|*.gxml|All files|*.*";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                String errorMessage = "";
                foreach (String fileName in ofd.FileNames)
                {
                    try
                    {
                        AddLayer(fileName);
                    }
                    catch (Exception ex)
                    {
                        errorMessage += ex.Message + "\n";
                    }
                }
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage);
                }
                SaveRastrLayers();
            }
        }
        #endregion AddLayersCommand

        #region SaveLayersCommand
        public ICommand SaveLayersCommand
        {
            get { return _saveLayersCommand ?? (_saveLayersCommand = new RelayCommand(this.SaveRastrLayers)); }
        }
        /// <summary>
        /// Сохранение информации о растровых слоях в конфигурационный xml файл
        /// </summary>
        public void SaveRastrLayers(object parameter = null)
        {
            var listRastrs = new List<Rekod.Repository.SettingsFile.RastrXml_M>();
            foreach (RasM.RastTableBaseM item in Tables)
            {
                var rastr = Program.SettingsXML.FindRaster(item.FilePath);
                if (rastr == null)
                    rastr = new Repository.SettingsFile.RastrXml_M();

                rastr.Name = item.Text;
                rastr.Path = item.FilePath;
                rastr.IsExternal = item.IsExternal;
                rastr.IsHidden = item.IsHidden;
                rastr.Description = item.Description;
                rastr.UseBounds = item.UseBounds;
                rastr.MinScale = item.MinScale;
                rastr.MaxScale = item.MaxScale;
                rastr.BuildPyramids = item.BuildPyramids;
                rastr.MethodUse = (int)item.ConnectType;

                listRastrs.Add(rastr);
            }
            ExtraFunctions.Sorts.SortList(Program.SettingsXML.Rasters, listRastrs);
            Program.SettingsXML.ApplyRastrInfo();
            UpdateLayerItemsView();
            ReloadInfo();
        }
        #endregion SaveLayersCommand

        #region UpdateLayersCommand
        public ICommand UpdateLayersCommand
        {
            get { return _updateLayersCommand ?? (_updateLayersCommand = new RelayCommand(this.UpdateRastrLayers)); }
        }
        /// <summary>
        /// Перезагрузка растровых слоев. Информация о растровых слоях берется из конфигурационного xml файла
        /// </summary>
        public void UpdateRastrLayers(object parameter = null)
        {
            var tables = new List<RasM.RastTableBaseM>();
            foreach (var item in Program.SettingsXML.Rasters)
            {
                if (!string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Path))
                {
                    try
                    {
                        var table = FindTable(item.Path) as RasM.RastTableBaseM;
                        if (table == null)
                        {
                            table = new RasM.RastTableBaseM(this, rastrLayerId++, item.Path, AbsM.ETableType.Rastr);
                        }
                        table.GeomType = AbsM.EGeomType.None;
                        table.FilePath = item.Path;
                        table.IsExternal = item.IsExternal;
                        table.BuildPyramids = item.BuildPyramids;
                        table.Name = item.Name;
                        table.Text = item.Name;
                        table.IsHidden = item.IsHidden;
                        table.Description = item.Description;
                        table.UseBounds = item.UseBounds;
                        table.MinScale = item.MinScale;
                        table.MaxScale = item.MaxScale;
                        table.ConnectType = (RasM.EConnectType)item.MethodUse;

                        tables.Add(table);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message, Rekod.Properties.Resources.error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            ExtraFunctions.Sorts.SortList(_tables, tables);
            ExtraFunctions.Sorts.SortList(_groups[0].Tables, tables);
            ExtraFunctions.Sorts.SortList(_groups[1].Tables, _defaultRastrs);

            CollectionViewSource.GetDefaultView(_layers).Refresh();
            CollectionViewSource.GetDefaultView(Groups.ElementAt(0).Tables).Refresh();

        }
        #endregion UpdateLayersCommand

        #region BeginValidationCommand
        private ICommand _beginValidationCommand;
        /// <summary>
        /// Команда для начала валидации
        /// </summary>
        public ICommand BeginValidationCommand
        {
            get { return _beginValidationCommand ?? (_beginValidationCommand = new RelayCommand(this.BeginValidation, this.CanBeginValidation)); }
        }
        /// <summary>
        /// Начало валидации
        /// </summary>
        public void BeginValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                if (commEventPar.CommandParameter is BindingGroup)
                {
                    BindingGroup bindGroup = commEventPar.CommandParameter as BindingGroup;
                    bindGroup.BeginEdit();
                }
            }
        }
        /// <summary>
        /// Можно ли начать валидацию
        /// </summary>
        public bool CanBeginValidation(object parameter = null)
        {
            return true;
        }
        #endregion // BeginValidationCommand

        #region CancelValidationCommand
        private ICommand _cancelValidationCommand;
        /// <summary>
        /// Отменяет введенные значения в BindingGroup
        /// </summary>
        public ICommand CancelValidationCommand
        {
            get { return _cancelValidationCommand ?? (_cancelValidationCommand = new RelayCommand(this.CancelValidation, this.CanCancelValidation)); }
        }
        /// <summary>
        /// Отменить введенные значения
        /// </summary>
        public void CancelValidation(object parameter = null)
        {
            BindingGroup bindGroup = null;
            if (parameter is BindingGroup)
            {
                bindGroup = parameter as BindingGroup;
            }
            else if (parameter is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = parameter as CommandEventParameter;
                if (commEvtParam.CommandParameter is BindingGroup)
                {
                    bindGroup = commEvtParam.CommandParameter as BindingGroup;
                }
            }
            if (bindGroup != null)
            {
                bindGroup.CancelEdit();
                bindGroup.BeginEdit();
            }
        }
        /// <summary>
        /// Можно ли отменить введенные значения
        /// </summary>
        public bool CanCancelValidation(object parameter = null)
        {
            return true;
        }
        #endregion // CancelValidationCommand

        #region SaveValidationCommand
        private ICommand _saveValidationCommand;
        /// <summary>
        /// Применить измененные значения в BindingGroup
        /// </summary>
        public ICommand SaveValidationCommand
        {
            get { return _saveValidationCommand ?? (_saveValidationCommand = new RelayCommand(this.SaveValidation, this.CanSaveValidation)); }
        }
        /// <summary>
        /// Сохранить измененные значения в BindingGroup
        /// </summary>
        public void SaveValidation(object parameter = null)
        {
            var commEventPar = parameter as Rekod.Behaviors.CommandEventParameter;
            if (commEventPar != null)
            {
                var bindGroup = commEventPar.CommandParameter as BindingGroup;
                var bindProxy = commEventPar.ExtraParameter as BindingProxy;
                var rastrLayer = bindProxy.Data as RasM.RastTableBaseM;

                var mvLayer = Program.mainFrm1.axMapLIb1.getImageLayer(rastrLayer.FilePath);
                bool isPrevVisible = mvLayer != null ? mvLayer.Visible : false;
                if (mvLayer != null)
                {
                    mvLayer.Visible = false;
                }

                if (bindGroup.CommitEdit())
                {
                    if (bindProxy != null && rastrLayer != null)
                    {
                        if (rastrLayer.IsNewTable)
                        {
                            SaveLayer(rastrLayer);
                        }
                    }

                    if (isPrevVisible)
                    {
                        mvLayer = Program.mainFrm1.axMapLIb1.getImageLayer(rastrLayer.FilePath);
                        if (mvLayer != null)
                        {
                            mvLayer.MaxScale = (uint)rastrLayer.MaxScale;
                            mvLayer.MinScale = (uint)rastrLayer.MinScale;
                            mvLayer.usebounds = rastrLayer.UseBounds;
                            mvLayer.Visible = true;
                            Program.mainFrm1.axMapLIb1.mapRepaint();
                        }
                    }

                    bindGroup.BeginEdit();
                    SaveRastrLayers();
                    var t = Tables.FirstOrDefault(p => (p as RasM.RastTableBaseM).FilePath == rastrLayer.FilePath);
                    bindProxy.Data = t;
                }
            }
        }
        /// <summary>
        /// Можно ли сохранить измененные значения
        /// </summary>
        public bool CanSaveValidation(object parameter = null)
        {
            return true;
        }
        #endregion SaveValidationCommand

        #region ErrorValidationCommand
        private ICommand _errorValidationCommand;
        /// <summary>
        /// Команда, которая запускается, если при валидации обнаружены ошибки
        /// </summary>
        public ICommand ErrorValidationCommand
        {
            get { return _errorValidationCommand ?? (_errorValidationCommand = new RelayCommand(this.ErrorValidation, this.CanErrorValidation)); }
        }
        /// <summary>
        /// Обработать ошибки валидации
        /// </summary>
        public void ErrorValidation(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventPar =
                    parameter as Rekod.Behaviors.CommandEventParameter;

                ValidationErrorEventArgs e = commEventPar.EventArgs as ValidationErrorEventArgs;
                if (e != null && e.Action == ValidationErrorEventAction.Added)
                {
                    System.Windows.MessageBox.Show(e.Error.ErrorContent.ToString());
                }
            }
        }
        /// <summary>
        /// Можно ли обработать ошибки валидации
        /// </summary>
        public bool CanErrorValidation(object parameter = null)
        {
            return true;
        }
        #endregion // ErrorValidationCommand
        #endregion Команды
    }
}