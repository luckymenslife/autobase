using System;
using System.Drawing;
using System.IO;

namespace Rekod.CmdPlugin.Module
{
    static class IconModule
    {
        /// <summary> Получаем иконку с файла
        /// </summary>
        /// <param name="pathIcon">Путь до файла</param>
        /// <param name="defaultIcon">Стандартная иконка, если небыла найдена</param>
        /// <returns>Иконка</returns>
        internal static Icon GetIcon(string pathIcon, Icon defaultIcon = null)
        {
            if (defaultIcon == null)
                defaultIcon = SystemIcons.WinLogo;
            if (pathIcon == null)
                pathIcon = "";
            pathIcon = pathIcon.Trim(' ', '"');
            if (pathIcon == null || pathIcon == "")
                return defaultIcon;
            string path = Path.GetFullPath(pathIcon);
            var f = new FileInfo(path);
            if (f.Exists == true)
                return Icon.ExtractAssociatedIcon(path);
            else
                return defaultIcon;
        }

        /// <summary> Получаем иконку с файла
        /// </summary>
        /// <param name="pathIcon">Путь до файла</param>
        /// <param name="defaultPathFile">Иконка, если небыла найдена иконка</param>
        /// <returns>Иконка</returns>
        internal static Icon GetIcon(string pathIcon, string defaultPathFile)
        {
            return GetIcon(pathIcon, GetIcon(defaultPathFile));
        }
    }
}
