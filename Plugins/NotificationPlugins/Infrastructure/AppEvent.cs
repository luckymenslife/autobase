using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationPlugins.Infrastructure
{
    public class AppEvent : ViewModelBase
    {
        #region Поля
        private List<string> _freezedEvents = new List<string>();
        #endregion

        #region Закрытые методы 
        bool IsFreezed(string event_name)
        {
            if (_freezedEvents.Contains(event_name))
                return true;
            else
                return false;
        }
        #endregion
        #region Открытые методы
        public void FreezeEvent(string event_name)
        {
            if (!_freezedEvents.Contains(event_name))
            {
                _freezedEvents.Add(event_name);
            }
        }
        public void ThawEvent(string event_name)
        {
            _freezedEvents.Remove(event_name);
        }
        public void CreateEvent(string event_name)
        {
            if (!IsFreezed(event_name))
            {
                OnPropertyChanged(event_name);
            }
        }
        #endregion
    }
}
