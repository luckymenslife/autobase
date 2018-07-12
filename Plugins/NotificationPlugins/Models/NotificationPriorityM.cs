using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationPlugins.Models
{
    public class NotificationPriorityM:ViewModelBase
    {
        #region Поля
        private int _gid;
        private int _weight;
        private string _name;
        private System.Drawing.KnownColor _knowncolor; 
        #endregion

        #region Конструктор
        public NotificationPriorityM(int gid, int weight, string name, System.Drawing.KnownColor windowsKnownColor)
        {
            _gid = gid;
            _weight = weight;
            _name = name;
            _knowncolor = windowsKnownColor;
        } 
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
        public int Weight
        {
            get { return _weight; }
            set
            {
                OnPropertyChanged(ref _weight, value, () => Weight);
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
        public System.Drawing.KnownColor WindowsKnownColor
        {
            get { return _knowncolor; }
            set
            {
                OnPropertyChanged(ref _knowncolor, value, () => WindowsKnownColor);
            }
        } 
        #endregion
    }
}
