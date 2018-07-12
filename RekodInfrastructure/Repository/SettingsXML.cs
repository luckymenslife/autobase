using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Rekod.Repository.SettingsFile;
using ExtraFunctions;
using System.Globalization;

namespace Rekod.Repository
{
    public sealed class SettingsXML : Rekod.Classes.Singleton<SettingsXML>
    {
        #region Поля
        string pathXML;
        XElement _parentXML;

        private LocalParameters_M _localParameters;
        private List<DataBase_M> _dataBases;
        private List<RastrXml_M> _rasters;
        #endregion // Поля

        #region Свойства
        public LocalParameters_M LocalParameters
        { get { return _localParameters; } }
        public List<DataBase_M> DataBases
        { get { return _dataBases; } }
        public List<RastrXml_M> Rasters
        { get { return _rasters; } }
        #endregion // Свойства

        #region Конструктор
        private SettingsXML()
        {
            pathXML = Program.path_string + "\\" + Program.setting_file;
            try
            {
                _parentXML = XElement.Load(pathXML);
                _localParameters = new LocalParameters_M();
                ReloadLocalParameters();
                _rasters = new List<RastrXml_M>();
                ReloadRastrInfo();
                _dataBases = GetUsersList();
            }
            catch{ }
        }
        #endregion // Конструктор

        #region Открытые методы
        /// <summary>
        /// Получение XML плагина
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public XElement GetPlugin(string guid)
        {
            var pluginsInfo = _parentXML.GetOrCreateElement("PluginsInfo");
            return pluginsInfo.GetOrCreateElement("guid-" + guid);
        }

        /// <summary>
        /// Применение изменений плагина
        /// </summary>
        /// <param name="guid">Гуид плагина</param>
        /// <param name="settings">XML с новыми параметрами</param>
        public void ApplyPlugin(string guid, XElement settings)
        {
            var elemMain = GetPlugin(guid);
            elemMain.ReplaceWith(settings);

            _parentXML.Save(pathXML);
        }

        /// <summary>
        /// Обнавление пользовательских настроек
        /// </summary>
        public void ReloadLocalParameters()
        {
            var xml = GetPlugin(LocalParameters_M.Guid);
            _localParameters.EnterTheScreen = Converts.To<bool?>((string)xml.GetOrCreateElement("EnterTheScreen")) ?? false;
            _localParameters.ShowCoordinatesPanel = Converts.To<bool?>((string)xml.GetOrCreateElement("ShowCoordinatesPanel")) ?? true;
            _localParameters.TurnOffVMPWhenRastr = Converts.To<bool?>((string)xml.GetOrCreateElement("TurnOffVMPWhenRastr")) ?? false;
            _localParameters.OpenAttrsAfterCreate = Converts.To<bool?>((string)xml.GetOrCreateElement("OpenAttrsAfterCreate")) ?? true;

            var cultureName = (string)xml.GetOrCreateElement("LocaleProgram");
            var culture = Rekod.Culture_M.ConvertCulture(cultureName);
            if (culture != null)
            {
                _localParameters.LocaleProgram = culture;

                // Нужно чтобы WPF контролы использовали текущую локаль
                System.Windows.FrameworkElement.LanguageProperty.OverrideMetadata(
                       typeof(System.Windows.FrameworkElement),
                       new System.Windows.FrameworkPropertyMetadata(
                       System.Windows.Markup.XmlLanguage.GetLanguage(_localParameters.LocaleProgram.IetfLanguageTag)));
            }
            else
            {
#if LNG_MN
                _localParameters.LocaleProgram = new CultureInfo("mn-MN");
#else
                _localParameters.LocaleProgram = CultureInfo.InvariantCulture;
#endif
            }

            var xmlProxy = GetPlugin(ProxyParameters_M.Guid);

            var xProxy = xmlProxy.GetOrCreateElement("Proxy");
            _localParameters.Proxy = XmlWork.DeserializeXML<ProxyParameters_M>(xProxy);
        }
        /// <summary>
        /// Сохранение пользовательских настроек
        /// </summary>
        public void ApplyLocalParameters()
        {
            var xml = GetPlugin(LocalParameters_M.Guid);
            xml.GetOrCreateElement("EnterTheScreen").SetValue(_localParameters.EnterTheScreen);
            xml.GetOrCreateElement("ShowCoordinatesPanel").SetValue(_localParameters.ShowCoordinatesPanel);
            xml.GetOrCreateElement("TurnOffVMPWhenRastr").SetValue(_localParameters.TurnOffVMPWhenRastr);
            xml.GetOrCreateElement("LocaleProgram").SetValue(_localParameters.LocaleProgram.Name);
            xml.GetOrCreateElement("OpenAttrsAfterCreate").SetValue(_localParameters.OpenAttrsAfterCreate);
            
            ApplyPlugin(LocalParameters_M.Guid, xml);

            var xmlProxy = GetPlugin(ProxyParameters_M.Guid);
            var xProxy = xmlProxy.GetOrCreateElement("Proxy");
            var xmlProxyNew = _localParameters.Proxy.SerializeXML();

            xProxy.ReplaceWith(xmlProxyNew);
            ApplyPlugin(ProxyParameters_M.Guid, xmlProxy);
        }

        public void ReloadRastrInfo()
        {
            var list = new List<RastrXml_M>();
            var rl = _parentXML.GetOrCreateElement("RastrLayers");
            var rlList = rl.Elements("Layer");
            foreach (var item in rlList)
            {
                var rastr = new RastrXml_M();

                rastr.Name = (string)item.GetOrCreateElement("Name");
                rastr.Path = (string)item.GetOrCreateElement("Path");
                rastr.IsExternal = Converts.To<bool?>((string)item.GetOrCreateElement("IsExternal")) ?? false;
                rastr.Description = (string)item.GetOrCreateElement("Description");
                rastr.IsHidden = Converts.To<bool?>((string)item.GetOrCreateElement("IsHidden")) ?? false;
                rastr.UseBounds = Converts.To<bool?>((string)item.GetOrCreateElement("UseBounds")) ?? false;
                rastr.MinScale = Converts.To<int>((string)item.GetOrCreateElement("MinScale"));
                rastr.MaxScale = Converts.To<int>((string)item.GetOrCreateElement("MaxScale"));
                rastr.MethodUse = Converts.To<int>((string)item.GetOrCreateElement("MethodUse"));
                rastr.BuildPyramids = Converts.To<bool>((string)item.GetOrCreateElement("BuildPyramids"));

                list.Add(rastr);
            }
            Sorts.SortList(Rasters, list);
        }
        public void ApplyRastrInfo()
        {
            var list = new List<RastrXml_M>();
            var rl = _parentXML.GetOrCreateElement("RastrLayers");
            rl.RemoveNodes();
            var rlList = rl.Elements("Layer");
            foreach (var item in Rasters)
            {
                var xRastr = item.SerializeXML();
                rl.Add(xRastr);
            }
            ApplyLastChange();
        }

        /// <summary>
        /// Сохраненние последних изменений сделанных не через функции
        /// </summary>
        public void ApplyLastChange()
        {
            var rl = _parentXML.GetOrCreateElement("RastrLayers");
            if (rl.IsEmpty)
                rl.Remove();

            _parentXML.Save(pathXML);
        }

        public void GetLastDataBase(out string dBase, out string user)
        {
            var xLastCon = _parentXML.GetOrCreateElement("LastConnectionString");
            dBase = (string)xLastCon.GetOrCreateElement("ConnectionString");
            user = (string)xLastCon.GetOrCreateElement("Name");
        }

        public void SetLastDataBase(string dBase, string user)
        {
            XElement xdb = null;
            var db = FindDataBase(dBase);
            if (db == null)
            {
                db = new DataBase_M();
                db.DataBase = dBase;

                xdb = new XElement("DateBase", new XElement("Connection", dBase));
                _parentXML.Add(xdb);
            }
            else
                xdb = _parentXML.Elements("DateBase").FindElement("Connection", dBase);

            bool isExist = db.Logins.Contains(user);
            if (!isExist)
            {
                db.Logins.Add(user);
                xdb.Add(new XElement("user", user));
            }

            var xLastCon = _parentXML.GetOrCreateElement("LastConnectionString");
            xLastCon.GetOrCreateElement("ConnectionString").Value = dBase;
            xLastCon.GetOrCreateElement("Name").Value = user;

            ApplyLastChange();
        }

        public DataBase_M FindDataBase(string dBase)
        {
            DataBase_M db = null;
            for (int i = 0; i < DataBases.Count; i++)
            {
                var item = DataBases[i];
                if (item.DataBase == dBase)
                {
                    db = item;
                    break;
                }
            }
            return db;
        }
        #endregion // Открытые методы

        #region Закрытые методы
        private List<DataBase_M> GetUsersList()
        {
            var listUsers = new List<DataBase_M>();
            foreach (var xBase in _parentXML.Elements("DateBase"))
            {
                var dBase = new DataBase_M();
                dBase.DataBase = (string)xBase.Element("Connection");

                foreach (var xUser in xBase.Elements("user"))
                {
                    dBase.Logins.Add((string)xUser);
                }

                listUsers.Add(dBase);
            }

            return listUsers;
        }
        public RastrXml_M FindRaster(string path)
        {
            return Rasters.FirstOrDefault(f => f.Path == path);
        }
        #endregion // Закрытые методы
    }
}