using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationPlugins.Models
{
    public class NotificationTypeM:ViewModelBase
    {
        #region Поля
        private int _gid;
        private string _name;
        private bool _system_type;
        #endregion

        #region Конструктор
        #endregion

        #region Свойства
        public int Gid
        {
            get { return _gid; }
            set
            {
                OnPropertyChanged(ref _gid, value, () => Gid);
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                OnPropertyChanged(ref _name, value, () => Name);
            }
        }
        public bool SystemType
        {
            get { return _system_type; }
            set
            {
                OnPropertyChanged(ref _system_type, value, () => SystemType);
            }
        }
        #endregion
    }
}
