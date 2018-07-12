using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using RESTLib.Model.REST;
using sscSync.Controller;

namespace sscSync.ViewModel
{
    public class AuthorizationVM : ViewModelBase
    {
        private ServerSSC _server;
        private string _serverText;
        private string _login;
        private string _loginText;
        private User _user;
        private Interfaces.IOpenForms openForms;
        private XElement XSettings;
        private ObservableCollection<ServerSSC> _servers;

        #region Свойства
        public User User
        {
            get { return _user; }
            set
            {
                if (value != null)
                {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        public ServerSSC Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged("Server");
            }
        }

        public string ServerText
        {
            get { return _serverText; }
            set
            {
                _serverText = value;
                OnPropertyChanged("ServerText");
                Server = Servers.FirstOrDefault(w => w.URL == _serverText);
            }
        }
        
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged("Login");
            }
        }

        public string LoginText
        {
            get { return _loginText; }
            set
            {
                _loginText = value; 
                OnPropertyChanged("Login");
                if (Server != null && Server.UserNames.Contains(_loginText))
                    Login = _loginText;
            }
        }

        public ObservableCollection<ServerSSC> Servers
        {
            get { return _servers; }
        }

        #endregion Свойства

        public AuthorizationVM(Interfaces.IOpenForms openForms, System.Xml.Linq.XElement XSettings)
        {
            this.openForms = openForms;
            this.XSettings = XSettings;
            ReadXML();
        }

        private void ReadXML()
        {
            _servers = GetServersList();
            var xLastCon = GetOrCreateElement("LastConnectionString", XSettings);
            ServerText = GetOrCreateElement("ConnectionString", xLastCon).Value;
            LoginText = GetOrCreateElement("Name", xLastCon).Value;
        }

        private XElement GetOrCreateElement(string name, XElement parent)
        {
            XElement element = parent.Element(name);
            if (element == null)
            {
                element = new XElement(name);
                parent.Add(element);
            }
            return element;
        }

        public XElement WriteXML()
        {
            XElement xServer = null;
            if (Server == null)
            {
                Server = new ServerSSC();
                Server.URL = ServerText;

                xServer = new XElement("Server", new XElement("Connection", ServerText));
                XSettings.Add(xServer);
            }
            else
            {
                foreach (var item in XSettings.Elements("Server"))
                {
                    var element = item.Element("Connection");
                    if (element != null && (string)element == Server.URL)
                    {
                        xServer = item;
                        break;
                    }
                }
            }

            bool isExist = Server.UserNames.Contains(LoginText);
            if (!isExist)
            {
                Server.UserNames.Add(LoginText);
                xServer.Add(new XElement("user", LoginText));
            }

            var xLastCon = GetOrCreateElement("LastConnectionString", XSettings);
            GetOrCreateElement("ConnectionString", xLastCon).Value = Server.URL;
            GetOrCreateElement("Name", xLastCon).Value = LoginText;

            return XSettings;
        }

        private ObservableCollection<ServerSSC> GetServersList()
        {
            var listServers = new ObservableCollection<ServerSSC>();
            foreach (var xBase in XSettings.Elements("Server"))
            {
                var server = new ServerSSC();
                server.URL = (string)xBase.Element("Connection");

                foreach (var xUser in xBase.Elements("user"))
                {
                    server.UserNames.Add((string)xUser);
                }

                listServers.Add(server);
            }

            return listServers;
        }

        #region Команды
        private ICommand _loginCommand;

        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(TryLogin, CanLogin));
            }
        }

        private void TryLogin(object parameter = null)
        {
            if (parameter != null && parameter is PasswordBox)
            {
                string key = openForms.ProcOpen();
                try
                {
                    Uri serverUri = new Uri(ServerText);
                    User = new User(LoginText, (parameter as PasswordBox).Password, serverUri);                    
                    openForms.ProcClose(key);
                }
                catch (System.Net.WebException e)
                {
                    openForms.ProcClose(key);
                    MessageBox.Show(e.InnerException != null ? e.InnerException.Message : e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception e)
                {
                    openForms.ProcClose(key);
                    MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanLogin(object parameter = null)
        {
            return !String.IsNullOrEmpty(LoginText)
                && !String.IsNullOrEmpty(ServerText)
                && parameter != null
                && parameter is PasswordBox
                && !String.IsNullOrEmpty((parameter as PasswordBox).Password);
        }
        #endregion Команды

        public class ServerSSC
        {
            string url;
            List<string> userNames;

            public List<string> UserNames
            {
                get { return userNames; }
                set { userNames = value; }
            }

            public string URL
            {
                get { return url; }
                set { url = value; }
            }

            public ServerSSC()
            {
                userNames = new List<string>();
            }
        }
    }
}
