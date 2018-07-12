using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Interfaces.UserControls;
using Rekod.Model.PluginModel;
using Rekod.PluginSettings;
using System.Runtime.InteropServices;

namespace Rekod
{
    //[Guid("8F7DF4E5-2CD8-4760-91DC-E3DE57F70103")] - использовать для класса
    public class Plugins
    {
        //Основной список плагинов
        List<Assembly> ListAssembly = new List<Assembly>();
        public static readonly List<Interfaces.IMainPlugin> ListMainPlugins = new List<Interfaces.IMainPlugin>();

        //static public List<Interfaces.IPluginInfo> ListPlugins = new List<Interfaces.IPluginInfo>();
        public static readonly List<PluginFunc<Func<int, int?, Interfaces.referInfo, IUserControlMain>>> ListSpoofingAttributesOfObject =
                        new List<PluginFunc<Func<int, int?, Interfaces.referInfo, IUserControlMain>>>();

        public static readonly List<PluginFunc<Func<int, bool, Form, int?, int?, IUserControlMain>>> ListSpoofingTableOfObjects =
                new List<PluginFunc<Func<int, bool, Form, int?, int?, IUserControlMain>>>();

        public static IUserControlMain ProcessesControl;

        public static readonly List<MenuItem> ListAddMenuInTable = new List<MenuItem>();
        public static readonly List<MenuItem> ListAddMenuInAttribute = new List<MenuItem>();
        public static readonly List<String> BlackList = new List<string>();

        public void FindPlugins(AxmvMapLib.AxMapLIb axMapLIb1, mainFrm mainForm)
        {
            Program.app = new Rekod.Classes.MainApp(axMapLIb1);
            Interfaces.IWorkClass work = new Rekod.Classes.WorkClass();
            Program.work = work;
            // папка с плагинами
            string folder = Program.path_string + "\\Plugins";

            // dll-файлы в этой папке
            ListMainPlugins.Clear();
            string[] files;

            BlackList.Add("28e89a9f-e67d-3028-aa1b-e5ebcde6f3c8");
            BlackList.Add("f790d274-ab2d-44dd-9b18-eded248d90ed");

            try
            {
                files = Directory.GetFiles(folder, "*.dll");
            }
            catch
            {
                return;
            }

            foreach (string file in files)
            {
                Assembly assembly = Assembly.LoadFile(file);
                var gd = Interfaces.AssemblyClass.GetGuid(assembly);
                Assembly ТакойжеПлагин = ListAssembly.FirstOrDefault(f => Interfaces.AssemblyClass.GetGuid(f) == Interfaces.AssemblyClass.GetGuid(assembly));
                if (ТакойжеПлагин != null)
                {
                    if (ТакойжеПлагин.GetName().Version >= assembly.GetName().Version)
                        continue;
                    else
                    {
                        ListAssembly.Remove(ТакойжеПлагин);
                    }
                }
                ListAssembly.Add(assembly);
            }
            foreach (Assembly assembly in ListAssembly)
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        Type iface = type.GetInterface("Interfaces.IMainPlugin");
                        if (iface != null)
                        {
                            Interfaces.IMainPlugin MainPlugin = Activator.CreateInstance(type) as Interfaces.IMainPlugin;
                            if (MainPlugin != null)
                            {
                                //MessageBox.Show(BlackList.Find(f => f == MainPlugin.GUID));
                                if (BlackList.Find(f => f == MainPlugin.GUID) == null)
                                {
                                    XElement Settings = Program.SettingsXML.GetPlugin(MainPlugin.GUID);
                                    //if ((bool)Settings.Attribute("isEnable") == false) continue;
                                    MainPlugin.StartPlugin(Settings, Program.app, work);
                                    ListMainPlugins.Add(MainPlugin);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, string.Format(Rekod.Properties.Resources.Plugins_Exception, System.IO.Path.GetFileName(assembly.Location)));
                }
            }
            Program.SettingsXML.ApplyLastChange();
        }

        public static Interfaces.UserControls.IUserControlMain GetControl(
            int? goToObject, bool isSelected, tablesInfo table, Form form, int? callerTableId = null, int? callerObjectId = null)
        {
            foreach (var item in Plugins.ListSpoofingTableOfObjects)
            {
                if (item.IdTable == table.idTable)
                {
                    return item.Func.Invoke(item.IdTable, isSelected, form, callerTableId, callerObjectId);
                }
            }
            return null;
        }
    }
}