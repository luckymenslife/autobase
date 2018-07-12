using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows;
using OSGeo.GDAL;
using DeepZoom.TestOverview.View;
using System.Threading.Tasks;
using DeepZoom;

namespace Rekod.RasterGeoreferenceModule.Model
{
    public class GDALOverviewsTileSource : MultiScaleTileSource
    {
        private int _tileSize;
        private GDALOverviewsM _gdalOverviewsM;

        public GDALOverviewsM GDALOverviewsM
        {
            get { return _gdalOverviewsM; }
        }

        public GDALOverviewsTileSource(GDALOverviewsM gdalOverviewsM, int tileSize)
            : base(gdalOverviewsM.ImageWidth, gdalOverviewsM.ImageHeight, tileSize, 0)
        {
            _tileSize = tileSize;
            _gdalOverviewsM = gdalOverviewsM;
        }

        protected override object GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY)
        {
            if (tileLevel >= _gdalOverviewsM.MinTileLevel && tileLevel <= _gdalOverviewsM.MaxTileLevel && _gdalOverviewsM.DataSet != null)
            {
                Dataset ds = _gdalOverviewsM.DataSet;
                Band redBand = _gdalOverviewsM.BandsCollection[0, tileLevel - _gdalOverviewsM.MinTileLevel]; 
                Band greenBand = _gdalOverviewsM.BandsCollection[1, tileLevel - _gdalOverviewsM.MinTileLevel];
                Band blueBand = _gdalOverviewsM.BandsCollection[2, tileLevel - _gdalOverviewsM.MinTileLevel]; 

                int resWidth = redBand.XSize;
                int resHeight = redBand.YSize;
                
                int offsetX = tilePositionX * _tileSize;
                int offsetY = tilePositionY * _tileSize;
                int currentTileWidth = (_tileSize < resWidth - offsetX) ? _tileSize : resWidth - offsetX;
                int currentTileHeight = (_tileSize < resHeight - offsetY) ? _tileSize : resHeight - offsetY;

                byte[] r = new byte[currentTileWidth * currentTileHeight];
                byte[] g = new byte[currentTileWidth * currentTileHeight];
                byte[] b = new byte[currentTileWidth * currentTileHeight];
                byte[] a = new byte[currentTileWidth * currentTileHeight];

                //Parallel.For(0, 3, new Action<int>(i =>
                //    {
                //        byte[] array = null;
                //        Band band = null;
                //        switch (i)
                //        {
                //            case 0:
                //                array = r;
                //                band = redBand;
                //                break;
                //            case 1:
                //                array = g;
                //                band = greenBand;
                //                break;
                //            case 2:
                //                array = b;
                //                band = blueBand;
                //                break; 
                //        }
                //        band.ReadRaster(offsetX, offsetY, currentTileWidth, currentTileHeight, array, currentTileWidth, currentTileHeight, 0, 0);
                //    }));
                
                redBand.ReadRaster(offsetX, offsetY, currentTileWidth, currentTileHeight, r, currentTileWidth, currentTileHeight, 0, 0);
                greenBand.ReadRaster(offsetX, offsetY, currentTileWidth, currentTileHeight, g, currentTileWidth, currentTileHeight, 0, 0);
                blueBand.ReadRaster(offsetX, offsetY, currentTileWidth, currentTileHeight, b, currentTileWidth, currentTileHeight, 0, 0);

                byte[] bytes = new byte[_tileSize * _tileSize * 4];
                Parallel.For(0, currentTileWidth, new Action<int>(i =>
                {
                    Parallel.For(0, currentTileHeight, new Action<int>(j =>
                    {
                        int pixelOffset = (i + j * _tileSize) * 4;
                        bytes[pixelOffset + 0] = b[i + j * currentTileWidth];
                        bytes[pixelOffset + 1] = g[i + j * currentTileWidth];
                        bytes[pixelOffset + 2] = r[i + j * currentTileWidth];
                        bytes[pixelOffset + 3] = 255;
                    }));
                }));

                var src = BitmapSource.Create(
                    _tileSize,
                    _tileSize,
                    96,
                    96, System.Windows.Media.PixelFormats.Bgra32, null, bytes, _tileSize * 4);
                return src;
            }
            else
            {
                return null;
            }
        }
    }
}