using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Rekod.Controllers;

namespace Rekod
{
    public class CultureItem : ViewModelBase
    {
        string _name;
        CultureInfo _culture;

        public string Name
        { get { return _name; } }
        public CultureInfo Culture
        { get { return _culture; } }

        public CultureItem(string name, CultureInfo culture)
        {
            _name = name;
            _culture = culture;
        }
    }
    public static class Culture_M
    {
        public static List<CultureItem> GetCulturesProgram()
        {
            var kultRu = ConvertCulture("ru");
            var cultures = new List<CultureItem>();
            cultures.Add(new CultureItem(Rekod.Properties.Resources.Culture_M_CultureDefault, CultureInfo.InvariantCulture));
            cultures.Add(new CultureItem(kultRu.DisplayName, kultRu));
            var appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            var mainDir = new DirectoryInfo(Application.StartupPath);
            foreach (var item in mainDir.GetDirectories())
            {
                var files = item.GetFiles(appName + ".resources.dll");
                foreach (var file in files.OrderBy(f => f.Name))
                {
                    var culture = ConvertCulture(item.Name);
                    if (culture != null)
                    {
                        var cultureItem = new CultureItem(culture.DisplayName, culture);
                        cultures.Add(cultureItem);
                    }
                }
            }
            return cultures;
        }
        public static void UpdateCulture()
        {
            CultureInfo culture = Program.SettingsXML.LocalParameters.LocaleProgram;
            if (culture == CultureInfo.InvariantCulture)
                culture = CultureInfo.InstalledUICulture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        public static CultureInfo ConvertCulture(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                try
                {
                    return CultureInfo.GetCultureInfo(name);
                }
                catch (Exception)
                { }
            return null;
        }
    }
}
