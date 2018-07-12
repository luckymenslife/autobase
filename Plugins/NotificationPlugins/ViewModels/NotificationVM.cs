using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using NotificationPlugins.Infrastructure;
using NotificationPlugins.Models;

namespace NotificationPlugins.ViewModels
{
    public class NotificationVM : ViewModelBase
    {
        #region Поля
        private NotificationM _notification;
        private NotificationRepository _repo;
        #endregion

        #region Конструктор

        public NotificationVM(NotificationM notification, NotificationRepository repo)
        {
            _notification = notification;
            _repo = repo;
        }
        #endregion

        #region Свойства
        public NotificationM Notification
        {
            get
            {
                return _notification;
            } 
        }
        public NotificationTypeM TypeRef
        {
            get { return NotificationsVM.Types.FirstOrDefault(w => w.Gid == this._notification.Type); }
        }
        public NotificationPriorityM PriorityRef
        {
            get { return NotificationsVM.Priorityes.FirstOrDefault(w => w.Gid == this._notification.Priority); }
        }
        public bool Read
        {
            get { return Notification.SeenDatetime.HasValue; }
        }
        public bool Favorite
        {
            get { return _notification.Favorite; }
            set
            {
                if (_notification.Favorite != value)
                {
                    SetFavorite(value);
                }
            }
        }
        public bool IsExistsRefObj
        {
            get { return Notification.RefObj != null && Notification.RefObj.IdTable.HasValue; }
        }
        #endregion

        #region Команды
        #region OpenRefLinkCmd
        private ICommand _openRefLinkCmd;
        public ICommand OpenRefLinkCmd
        {
            get { return _openRefLinkCmd ?? (_openRefLinkCmd = new RelayCommand(OpenRefLink, CanOpenRefLink)); }
        }
        private bool CanOpenRefLink(object obj)
        {
            if (IsExistsRefObj)
            {
                var ti = PluginInitialization._app.getTableInfo(Notification.RefObj.IdTable.Value);
                if (ti != null)
                    return true;

            }            
            return false;
        }
        private void OpenRefLink(object obj)
        {
            if(!CanOpenRefLink(obj))
                return;
            ReadNotification();
            if (Notification.RefObj.IdObj.HasValue)
                PluginInitialization._work.OpenForm.ShowAttributeObject(
                    PluginInitialization._app.getTableInfo(Notification.RefObj.IdTable.Value),
                    Notification.RefObj.IdObj.Value,
                    false,
                    wpfWindow: Looper._formNotification);
            else
            {
                PluginInitialization._work.OpenForm.OpenTableObject(
                    PluginInitialization._app.getTableInfo(Notification.RefObj.IdTable.Value), owner:null, wpfowner: Looper._formNotification);
            }
        }
        #endregion
        #region ReadNotificationCmd
        private ICommand _readNotificationCmd;
        public ICommand ReadNotificationCmd
        {
            get { return _readNotificationCmd ?? (_readNotificationCmd = new RelayCommand(ReadNotification)); }
        }
        private void ReadNotification(object obj)
        {
            ReadNotification();
        }

        #endregion
        #endregion

        #region Открытые Методы
        public void ReadNotification()
        {
            _repo.SetReadedNotification(this.Notification);
            var item = _repo.GetNotification(this.Notification.Gid);
            this.Notification.SeenDatetime = item.SeenDatetime;
            PluginInitialization._appEvent.CreateEvent("ReadNotification");
            this.OnPropertyChanged("Read");
        }
        public void SetFavorite(bool value)
        {
            if (!Read)
            {
                _repo.SetReadedNotification(this.Notification);
                PluginInitialization._appEvent.CreateEvent("ReadNotification");
            }
            _repo.SetFavorite(this.Notification, value);
            var item = _repo.GetNotification(this.Notification.Gid);
            this.Notification.Favorite = item.Favorite;
            this.Notification.SeenDatetime = item.SeenDatetime;
            this.OnPropertyChanged("Favorite");
            this.OnPropertyChanged("Read");
        }
        #endregion

        #region Закрытые Методы
        #endregion
    }
}
