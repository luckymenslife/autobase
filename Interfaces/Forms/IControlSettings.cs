using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Interfaces.Forms
{
    public interface IControlSettings
    {
        /// <summary> Подключение UserControl'a
        /// </summary>
        /// <returns></returns>
        UserControl GetUserControl();
        bool LoadSettings(XElement data);
        bool SaveSettings();
    }
}
