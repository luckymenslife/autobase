using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Interfaces;
using Interfaces.Forms;
using NotificationPlugins.Infrastructure;
using NotificationPlugins.ViewModels;
using NotificationPlugins.Views;

namespace NotificationPlugins
{
    class PluginInitialization : IMainPlugin
    {
        public static IMainApp _app;
        public static IWorkClass _work;
        public static AppEvent _appEvent;
        private Looper _looper;


        string IMainPlugin.GUID
        {
            get { return "65E9C620-40CD-4757-A360-ECD80A2F0E66"; }
        }

        string IMainPlugin.Name
        {
            get { return "Плагин уведомлений"; }
        }

        IControlSettings IMainPlugin.SettingsForm
        {
            get { return null; }
        }

        void IMainPlugin.StartPlugin(XElement XSettings, IMainApp app, IWorkClass work)
        {
            PluginInitialization._app = app;
            PluginInitialization._work = work;
            PluginInitialization._appEvent = new AppEvent();
            _looper = new Looper(new NotificationRepository(app));
        }
    }
}
