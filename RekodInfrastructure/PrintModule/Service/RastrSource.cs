using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxmvMapLib;
using System.Windows.Media.Imaging;
using mvMapLib;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;

namespace Rekod.PrintModule.Service
{
    public class RastrSource
    {
        #region Поля
        private AxMapLIb _axMapLib1;
        private double _dpi; 
        private int _imageWidth;
        private int _imageHeight;
        private ImageSource _imageSource;
        private mvBbox _imageExtent;
        private IntPtr _bmpHBitmapPointer = IntPtr.Zero;
        private IntPtr _bmpByteArrayPointer = IntPtr.Zero;
        private int _srcWidth;
        private int _srcHeight; 
        private double dpiFactor = 1;       

        mvBbox _exBbox; 
        int _exHeight; 
        int _exWidth;
        #endregion Поля

        #region Конструкторы
        public RastrSource(AxMapLIb axMapLib, double dpi)
        {
            _axMapLib1 = axMapLib;
            _dpi = dpi;
            using (System.Drawing.Image image = _axMapLib1.Image)
            {
                LoadBitmapByExtent(_axMapLib1.MapExtent, image.Width, image.Height);
            }
        }
        #endregion Конструкторы

        #region Свойства
        public bool LoadSuccess
        {
            get;
            private set; 
        }
        public AxMapLIb AxMapLib1
        {
            get { return _axMapLib1; }
        }
        public double Dpi
        {
            get { return _dpi; }
            set { _dpi = value; }
        }
        public int ImageWidth
        {
            get { return _imageWidth; }
            set { _imageWidth = value; }
        }
        public int ImageHeight
        {
            get { return _imageHeight; }
            set { _imageHeight = value; }
        }
        public ImageSource ImageSource
        {
            get { return _imageSource; }
        }
        public mvBbox ImageExtent
        {
            get { return _imageExtent; }
            set 
            {
                _imageExtent = value;
            }
        }
        public Rect RastrRectG
        {
            get;
            private set; 
        }

        public int SrcWidth
        {
            get { return _srcWidth; }
        }
        public int SrcHeight
        {
            get { return _srcHeight; }
        }
        public IntPtr BmpHBitmapPointer
        {
            get { return _bmpHBitmapPointer; }
        }
        public IntPtr BmpByteArrayPointer
        {
            get { return _bmpByteArrayPointer; }
        }
        #endregion Свойства

        #region Методы
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("maplib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void maplib_deleteArray(IntPtr res);
        [DllImport("maplib.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern unsafe byte* maplib_givePreviewByteArray(
                        double leftTopXRel,
                        double leftTopYRel,
                        double prevBoxWidthRel,
                        double prevBoxHeightRel,
                        int resWidth,
                        int resHeight,
                        int resBpp, 
                        int srcWidth,
                        int srcHeight, 
                        uint* bmpArrayPtr, 
                        String fileName);
        [DllImport("maplib.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool maplib_saveByteArray(
                        int leftTopX,
                        int leftTopY,
                        int rightBottomX,
                        int rightBottomY,
                        int srcWidth,
                        int srcHeight,
                        IntPtr bmpArrayPtr,
                        String fileToSave); 

        public void LoadBitmapExParams()
        {
            if (_exWidth > 0 && _exHeight > 0)
            {
                LoadBitmapByExtent(_exBbox, _exWidth, _exHeight);
            }
        }
        public void LoadBitmapByExtent(mvBbox bbox, int width, int height)
        {
            RastrRectG = new Rect(new Point(bbox.a.x, bbox.a.y), new Point(bbox.b.x, bbox.b.y));

            _exBbox = bbox;
            _exWidth = width;
            _exHeight = height;

            double imageZoom = 0.0;
            int imageWidth = (int)(width * dpiFactor);
            int imageHeight = (int)(height * dpiFactor);
            LoadSuccess = false;

            int byteIntPointer = 0; 

            if(_bmpHBitmapPointer != IntPtr.Zero)
            {
                DeleteObject(_bmpHBitmapPointer);
            }
            _bmpHBitmapPointer = IntPtr.Zero; 
            try
            {
                if (_bmpHBitmapPointer == IntPtr.Zero)
                {
                    _axMapLib1.SetSettingsParameter("TilesLoadingSynchronous", "1");
                    _bmpHBitmapPointer = new IntPtr(_axMapLib1.GetHBitmapByExtent(bbox, ref imageZoom, ref imageHeight, ref imageWidth, ref byteIntPointer, false));
                    _axMapLib1.SetSettingsParameter("TilesLoadingSynchronous", "0");
                    _bmpByteArrayPointer = new IntPtr(byteIntPointer);
                    _srcWidth = imageWidth;
                    _srcHeight = imageHeight;
                }

                _imageWidth = width;
                _imageHeight = height;
                _imageExtent = bbox;
                if (_bmpHBitmapPointer != IntPtr.Zero)
                {
                    LoadSuccess = true;
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void UpdatePreview(double leftTopXRel, double leftTopYRel, double prevBoxWidthRel, double prevBoxHeightRel, int resWidth, int resHeight)
        {
            unsafe
            {
                if (LoadSuccess)
                {
                    IntPtr res = IntPtr.Zero;
                    try
                    {
                        int bytesPerPixel = 3;
                        res = (IntPtr)maplib_givePreviewByteArray
                                                (
                                                    leftTopXRel,
                                                    leftTopYRel,
                                                    prevBoxWidthRel,
                                                    prevBoxHeightRel,
                                                    resWidth,
                                                    resHeight,
                                                    bytesPerPixel,
                                                    _srcWidth,
                                                    _srcHeight,
                                                    (uint*)_bmpByteArrayPointer,
                                                    ""
                                                );

                        int arrayLength = resWidth * resHeight * bytesPerPixel;
                        byte[] ar = new byte[arrayLength];

                        WriteableBitmap writeableBitmap =
                            new System.Windows.Media.Imaging.WriteableBitmap(
                                resWidth, resHeight, _dpi, _dpi, PixelFormats.Bgr24, null);
                        int stride = resWidth * bytesPerPixel;
                        System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, resWidth, resHeight);

                        Marshal.Copy(res, ar, 0, resWidth * resHeight * bytesPerPixel);
                        writeableBitmap.WritePixels(rect, ar, stride, 0);
                        _imageSource = writeableBitmap;
                        ar = null;
                        GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        _imageSource = null;
                    }
                    finally
                    {
                        if (res != IntPtr.Zero)
                        {
                            maplib_deleteArray(res);
                        }
                    }
                }
                else
                {
                    _imageSource = null; 
                }
            }
        }
        public void LoadExtent(Point leftBottomGlobal, Point rightTopGlobal, Rect winRect)
        {
            mvBbox bbox = new mvBbox()
                {
                    a = new mvPointWorld() { x = leftBottomGlobal.X, y = leftBottomGlobal.Y },
                    b = new mvPointWorld() { x = rightTopGlobal.X, y = rightTopGlobal.Y }
                };
            LoadBitmapByExtent(bbox, (int)winRect.Width, (int)winRect.Height);
        }
        #endregion Методы
    }
}