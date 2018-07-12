using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Reflection;
using System.Collections.Generic;

namespace Rekod.Controllers
{
    public static class FilePreviewController
    {
        /// <summary> Structure that encapsulates basic information of icon embedded in a file.
        /// </summary>
        struct EmbeddedIconInfo
        {
            public string FileName;
            public int IconIndex;
        }

        #region APIs

        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        #endregion

        #region CORE METHODS

        /// <summary> Извлекает иконку из файла.
        /// </summary>
        /// <param name="fileAndParam"> 
        /// Строка параметров, 
        ///  например: "C:\\Program Files\\NetMeeting\\conf.exe,1".</param>
        /// <param name="isLarge">
        /// Возвращается икона большая (может быть 32x32 
        ///  или маленькая иконка (16х16 пикселей).</param>
        public static Bitmap GetIcon(string fileAndParam, bool isLarge)
        {
            EmbeddedIconInfo embeddedIcon = GetEmbeddedIconInfo(fileAndParam);
            return GetIcon(embeddedIcon.FileName, embeddedIcon.IconIndex, isLarge);
        }

        /// <summary> Извлекает иконку из файла.
        /// </summary>
        /// <param name="fileName">Полный путь к файлу</param>
        /// <param name="iconIndex">Индекс иконки.
        /// По стандарту 0</param>
        /// <param name="isLarge">
        /// Возвращается икона большая (может быть 32x32 
        ///  или маленькая иконка (16х16 пикселей).</param>
        public static Bitmap GetIcon(string fileName, int iconIndex, bool isLarge)
        {
            var hDummy = new IntPtr[1] { IntPtr.Zero };
            var hIconEx = new IntPtr[1] { IntPtr.Zero };

            try
            {
                uint readIconCount = 0;

                readIconCount   = isLarge 
                                ? ExtractIconEx(fileName, iconIndex, hIconEx, hDummy, 1) 
                                : ExtractIconEx(fileName, iconIndex, hDummy, hIconEx, 1);

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero)
                {
                    // Get first icon.
                    var extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();
                    return extractedIcon.ToBitmap();
                }
                else // No icon read
                {
                    string pathRegistrFile = GetPathIcon(Path.GetExtension(fileName));
                    if (!string.IsNullOrEmpty(pathRegistrFile))
                        return GetIcon(pathRegistrFile, isLarge);
                    else
                        return null;
                }
            }
            catch (Exception exc)
            {
                // Extract icon error.
                throw new ApplicationException(Rekod.Properties.Resources.CouldNotExtractIcon, exc);
            }
            finally
            {
                // Release resources.
                foreach (IntPtr ptr in hIconEx)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);

                foreach (IntPtr ptr in hDummy)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);
            }
        }
        public static byte[] GetIcon(string extension)
        {
            string pathIcon = GetPathIcon(extension);
            if (pathIcon == null)
                return null;
            return GetIcon(pathIcon, true).ToByteArray();
        }
        //public static Bitmap GetIcon(string extension)
        //{
        //    string pathIcon = GetPathIcon(extension);
        //    if (pathIcon == null)
        //        return null;
        //    return GetIcon(pathIcon, true);
        //}

        /// <summary> Извлекает картинку из файла
        /// </summary>
        /// <param name="filePath">Полный путь к файлу</param>
        /// <returns>Bitmap картинки</returns>
        public static Bitmap GetPreviewPhoto(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var myData = new byte[fs.Length];
            fs.Read(myData, 0, (int)fs.Length);
            fs.Close();
            fs.Dispose();
            return GetPreviewPhoto(myData, System.IO.Path.GetExtension(filePath));
        }

        /// <summary> Извлекает картинку из файла
        /// </summary>
        /// <param name="myData">Двоичный данные файла</param>
        /// <param name="extension">Расширение файла</param>
        /// <returns>Bitmap картинки</returns>
        public static Bitmap GetPreviewPhoto(byte[] myData, string extension = ".JPG")
        {

            var msImage = new MemoryStream(myData);
            var msPreview = new MemoryStream();
            extension = string.IsNullOrEmpty(extension)
                        ? ".JPG"
                        : extension.ToLower();

            try
            {
                var bt = new Bitmap(msImage);
                var preview = ResizeImage(((Image)bt), new Size(125, 105));
                switch (extension)
                {
                    case ".jpg":
                        preview.Save(msPreview, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return new Bitmap(msPreview);
                    case ".jpeg":
                        preview.Save(msPreview, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return new Bitmap(msPreview);
                    case ".png":
                        preview.Save(msPreview, System.Drawing.Imaging.ImageFormat.Png);
                        return new Bitmap(msPreview);
                    case ".bmp":
                        preview.Save(msPreview, System.Drawing.Imaging.ImageFormat.Bmp);
                        return new Bitmap(msPreview);
                    default:
                        return null;
                }

            }
            catch { return null; }
        }

        public static BitmapImage GetBitmapImage(byte[] rawImageBytes)
        {
            if (rawImageBytes == null)
                return null;
            BitmapImage imageSource = null;
            try
            {
                using (var stream = new MemoryStream(rawImageBytes))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var b = new BitmapImage
                                {
                                    StreamSource = stream
                                };
                    imageSource = b;
                }
            }
            catch
            {
            }
            return imageSource;
        }

        public static BitmapImage GetBitmapImage(Bitmap rawImageBytes)
        {
            if (rawImageBytes == null)
                return null;
            BitmapImage imageSource = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    rawImageBytes.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    stream.Seek(0, SeekOrigin.Begin);
                    var b = new BitmapImage
                                {
                                    StreamSource = stream
                                };
                    imageSource = b;
                }
            }
            catch
            {
            }

            return imageSource;
        }
        #endregion

        #region UTILITY METHODS

        /// <summary> Parses the parameters string to the structure of EmbeddedIconInfo.
        /// </summary>
        /// <param name="fileAndParam">The params string, 
        /// such as ex: "C:\\Program Files\\NetMeeting\\conf.exe,1".</param>
        /// <returns></returns>
        private static EmbeddedIconInfo GetEmbeddedIconInfo(string fileAndParam)
        {
            var embeddedIcon = new EmbeddedIconInfo();

            if (String.IsNullOrEmpty(fileAndParam))
                return embeddedIcon;

            //Use to store the file contains icon.
            string fileName;

            //The index of the icon in the file.
            int iconIndex = 0;
            string iconIndexString = String.Empty;

            int commaIndex = fileAndParam.IndexOf(",");
            //if fileAndParam is some thing likes that: "C:\\Program Files\\NetMeeting\\conf.exe,1".
            if (commaIndex > 0)
            {
                fileName = fileAndParam.Substring(0, commaIndex);
                iconIndexString = fileAndParam.Substring(commaIndex + 1);
            }
            else
                fileName = fileAndParam;

            if (!String.IsNullOrEmpty(iconIndexString))
            {
                //Get the index of icon.
                iconIndex = int.Parse(iconIndexString);
                if (iconIndex < 0)
                    iconIndex = 0;  //To avoid the invalid index.
            }

            embeddedIcon.FileName = fileName;
            embeddedIcon.IconIndex = iconIndex;

            return embeddedIcon;
        }

        /// <summary> Получает путь до иконки из реестра
        /// </summary>
        /// <param name="extension">Расширение файла</param>
        /// <returns>Путь к иконке</returns>
        public static string GetPathIcon(string extension)
        {
            RegistryKey rkRoot = Registry.ClassesRoot;

            if (String.IsNullOrEmpty(extension))
                return null;

            if (!extension.StartsWith("."))
                return null;

            using (RegistryKey rkFileType = rkRoot.OpenSubKey(extension))
            {
                if (rkFileType == null)
                    return null;

                //Gets the default value of this key that contains the information of file type.
                object defaultValue = rkFileType.GetValue("");
                if (defaultValue == null)
                    return null;

                //Go to the key that specifies the default icon associates with this file type.
                string defaultIcon = defaultValue.ToString() + "\\DefaultIcon";
                using (RegistryKey rkFileIcon = rkRoot.OpenSubKey(defaultIcon))
                {
                    if (rkFileIcon != null)
                    {
                        //Get the file contains the icon and the index of the icon in that file.
                        object value = rkFileIcon.GetValue("");
                        if (value != null)
                        {
                            //Clear all unecessary " sign in the string to avoid error.
                            return value.ToString().Replace("\"", "");

                        }
                        rkFileIcon.Close();
                    }
                }
                rkFileType.Close();
            }
            return null;
        }

        private static Bitmap ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;


            float nPercentW = ((float)size.Width / (float)sourceWidth);
            float nPercentH = ((float)size.Height / (float)sourceHeight);
            float nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            var b = new Bitmap(destWidth, destHeight);
            var g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }


        public static byte[] ToByteArray(this Bitmap bmp)
        {
            var ms = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }

        #endregion
    }
}
