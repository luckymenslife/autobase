using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using NotificationPlugins.Infrastructure;
using NotificationPlugins.Models;

namespace NotificationPlugins.ViewModels
{
    public class UserSettingsVM:ViewModelBase
    {
        #region Поля
        private NotificationRepository _repo;
        public List<NotificationTypeCheckableM> _types;
        #endregion

        #region Конструктор
        public UserSettingsVM(NotificationRepository repo)
        {
            _repo = repo;
            _types = new List<NotificationTypeCheckableM>();
            LoadTypes();
        }
        #endregion

        #region Свойства
        public List<NotificationTypeCheckableM> Types
        {
            get { return _types; } 
        } 
        #endregion

        #region Открытые Методы
        public void LoadTypes()
        {
            _types.Clear();
            var typesUnsubscribes = _repo.GetUserSettingsNotificationTypes(PluginInitialization._app.user_info.id_user);
            var types = _repo.GetNotificationTypes();
            foreach (var item in types)
            {
                if(item.SystemType)
                    continue;
                var type = new NotificationTypeCheckableM();
                type.Gid = item.Gid;
                type.Name = item.Name;
                type.IsChecked = typesUnsubscribes.All(w => w != type.Gid);
                _types.Add(type);
            }
        }
        #endregion

        #region Команды
        #region CloseCmd
        private ICommand _closeCmd;
        public ICommand CloseCmd
        {
            get { return _closeCmd ?? (_closeCmd = new RelayCommand(Close)); }
        }
        private void Close(object obj)
        {
            if (obj != null && obj is Window)
            {
                (obj as Window).Hide();
            }
        }
        #endregion

        #region SaveCmd
        private ICommand _saveCmd;
        public ICommand SaveCmd
        {
            get { return _saveCmd ?? (_saveCmd = new RelayCommand(Save)); }
        }
        private void Save(object obj)
        {
            Dictionary<int, bool> types = new Dictionary<int, bool>();
            foreach (var type in Types)
            {
                types.Add(type.Gid, type.IsChecked);
            }
            _repo.UpdateUnsubscribes(types);
            Close(obj);
        }
        #endregion

        #endregion
    }
}