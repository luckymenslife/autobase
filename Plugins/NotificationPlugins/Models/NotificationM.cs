using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationPlugins.Models
{
    public class NotificationM : ViewModelBase
    {
        #region Поля

        private int _gid;
        private int _type;
        private int _priority;
        private int _weight;
        private string _subject;
        private string _message;
        private DateTime? _created;
        private DateTime? _seenDatetime;
        private int? _tableGid;
        private int? _rowGid;
        private int _userGid;
        private bool _favorite;
        private RefObjM _refObj;
        #endregion

        #region Конструктор
        public NotificationM() { }
        public NotificationM(int gid, int type, int priority, int weight, string subject, string message, DateTime? created, DateTime? seen_datetime, int user_gid, bool favorite, RefObjM refObj)
        {
            _gid = gid;
            _type = type;
            _priority = priority;
            _weight = weight;
            _subject = subject;
            _message = message;
            _created = created;
            _seenDatetime = seen_datetime;
            _userGid = user_gid;
            _favorite = favorite;
            _refObj = refObj;
        }
        #endregion

        #region Свойства
        public int Gid
        {
            get { return _gid; }
            set { OnPropertyChanged(ref _gid, value, () => Gid); }
        }
        public int Type
        {
            get { return _type; }
            set { OnPropertyChanged(ref _type, value, () => Type); }
        }
        public int Priority
        {
            get { return _priority; }
            set { OnPropertyChanged(ref _priority, value, () => Priority); }
        }
        public int Weight
        {
            get { return _weight; }
            set { OnPropertyChanged(ref _weight, value, () => Weight); }
        }
        public String Subject
        {
            get { return _subject; }
            set { OnPropertyChanged(ref _subject, value, () => Subject); }
        }
        public string Message
        {
            get { return _message; }
            set { OnPropertyChanged(ref _message, value, () => Message); }
        }
        public DateTime? Created
        {
            get { return _created; }
            set { OnPropertyChanged(ref _created, value, () => Created); }
        }
        public DateTime? SeenDatetime
        {
            get { return _seenDatetime; }
            set { OnPropertyChanged(ref _seenDatetime, value, () => SeenDatetime); }
        }
        public int UserGid
        {
            get { return _userGid; }
            set { OnPropertyChanged(ref _userGid, value, () => UserGid); }
        }
        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                OnPropertyChanged(ref _favorite, value, () => Favorite);
            }
        }
        public RefObjM RefObj
        {
            get { return _refObj; }
            set
            {
                OnPropertyChanged(ref _refObj, value, () => RefObj);
            }
        }
        #endregion
    }
}
