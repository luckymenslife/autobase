using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rekod.CmdPlugin.Model;

namespace Rekod.CmdPlugin.Module
{
    static class XMLModule
    {
        /// <summary> Считываем параметры с файла
        /// </summary>
        /// <param name="sSettings"></param>
        /// <returns></returns>
        public static List<PropertyCall> ReadXSettings(XElement sSettings)
        {
            var tempList = new List<PropertyCall>();
            var calls = sSettings.Element("cmds");
            if (calls == null) return tempList;
            foreach (var item in calls.Elements("cmd"))
            {
                var f = new PropertyCall()
                            {
                                File = GetValue(item, "command"),
                                Title = GetValue(item, "title"),
                                Icon = GetValue(item, "icon"),
                                Params = GetValue(item, "params")
                            };
                tempList.Add(f);
            }
            return tempList;
        }

        private static string GetValue(XElement element, string name)
        {
            XElement d = element.Element(name);
            return (d) == null
                            ? null
                            : d.Value.Trim();
        }
        /// <summary> Генерация XML для коллекции PropertyCall
        /// </summary>
        /// <param name="listPropertyCall"> Коллекция параметров "Быстрого запуска"</param>
        /// <returns>XML</returns>
        public static XElement WriteXSettings(IEnumerable<PropertyCall> listPropertyCall)
        {
            var result = new XElement("cmds");
            if (listPropertyCall != null && listPropertyCall.Count() != 0)
                foreach (var item in listPropertyCall)
                {
                    var call = new XElement("cmd");
                    call.Add(new XElement("command", item.File));
                    call.Add(new XElement("title", item.Title));
                    call.Add(new XElement("icon", item.Icon));
                    call.Add(new XElement("params", item.Params));

                    result.Add(call);
                }
            return result;
        }

    }
}
