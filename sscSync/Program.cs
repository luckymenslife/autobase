using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Interfaces;
using RESTLib.Model.REST;

namespace sscSync
{
    public class Program
    {
        public string GUID
        {
            get { return AssemblyClass.GetGuid(Assembly.GetExecutingAssembly()); }
        }

        public string Name
        {
            get { return AssemblyClass.GetName(Assembly.GetExecutingAssembly()); }
        }

        public Interfaces.Forms.IControlSettings SettingsForm
        {
            get { return null; }
        }

        public void StartPlugin(System.Xml.Linq.XElement XSettings, IMainApp app, IWorkClass work)
        {
            if (app.sscUser != null)
            {
                MenuItem miSSC = new MenuItem()
                    {
                        Header = "Регистрация слоев в MapAdmin"
                    };
                miSSC.Click += (o, e) =>
                    {
                        OpenRegisterWindow(XSettings, app, work, true);
                    };
                MenuItem miSystem = new MenuItem()
                    {
                        Header = "Регистрация слоев в системе"
                    };
                miSystem.Click += (o, e) =>
                    {
                        OpenRegisterWindow(XSettings, app, work, false);
                    };

                MenuItem plug = new MenuItem();
                plug.Header = "MapAdmin";
                plug.Items.Add(miSSC);
                plug.Items.Add(miSystem);

                work.MainForm.Menu(plug);
            }
        }

        private User Authorize(System.Xml.Linq.XElement XSettings, IMainApp app, IWorkClass work)
        {
            View.AuthorizationView authWindow = new View.AuthorizationView();
            var dataContext = new ViewModel.AuthorizationVM(work.OpenForm, XSettings);
            authWindow.DataContext = dataContext;
            dataContext.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "User")
                    {
                        authWindow.DialogResult = true;
                        authWindow.Close();
                    }
                };
            if (authWindow.ShowDialog() == true)
            {
                //work.SaveSettings(GUID, dataContext.WriteXML());
                app.sscUser = new sscUserInfo(dataContext.User.Uri, dataContext.User.Username, dataContext.User.Password);
                return dataContext.User;
            }
            return null;
        }

        private void OpenRegisterWindow(System.Xml.Linq.XElement XSettings, IMainApp app, IWorkClass work, bool isMapAdminRegister)
        {
            User user = null;
            if (app.sscUser != null)
            {
                string key = work.OpenForm.ProcOpen();
                try
                {
                    user = new User(app.sscUser.Login, app.sscUser.Password, app.sscUser.Server);
                    work.OpenForm.ProcClose(key);
                }
                catch (Exception e)
                {
                    work.OpenForm.ProcClose(key);
                    MessageBox.Show("При подключении к серверу произошла ошибка:" + Environment.NewLine + 
                        e.Message + Environment.NewLine + 
                        "Проверьте подключение к сети.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                user = Authorize(XSettings, app, work);

            if (user != null)
            {
                View.MainWindow window = new View.MainWindow();
                window.DataContext = new ViewModel.MainViewModel(app, work, user, isMapAdminRegister);
                if (isMapAdminRegister)
                    window.Title = "Регистрация слоев в MapAdmin";
                else
                    window.Title = "Регистрация слоев в системе";
                window.Closed += (ooo, eee) =>
                {
                    app.reloadInfo();
                };
                window.Show();
            }
            else
                app.sscUser = null;
        }
    }
}
