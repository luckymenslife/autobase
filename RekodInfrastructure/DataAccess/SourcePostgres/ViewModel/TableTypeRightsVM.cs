using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    /// <summary>
    /// Класс управляет правами к различным типам таблиц
    /// </summary>
    public class TableTypeRightsVM : ViewModelBase
    {
        #region Поля
        private ObservableCollection<PgUserRightsM> _rightsList;
        private string _name;
        #endregion Поля

        #region Конструктор
        public TableTypeRightsVM(string name, IEnumerable<PgUserRightsM> list)
        {
            _rightsList = new ObservableCollection<PgUserRightsM>();
            _rightsList.CollectionChanged += new NotifyCollectionChangedEventHandler(RightsList_CollectionChanged);
            if (list != null)
            {
                // _rightsList = new ObservableCollection<PgUserRightsM>(list);
                foreach (var item in list)
                    _rightsList.Add(item);
            }

            _name = name;
        }
        #endregion Конструктор

        #region Обработчики
        void RightsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("CanRead");
            OnPropertyChanged("CanWrite");

            if (e.NewItems != null)
            {
                foreach (PgUserRightsM userRight in e.NewItems)
                {
                    userRight.PropertyChanged += new PropertyChangedEventHandler(userRight_PropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (PgUserRightsM userRight in e.OldItems)
                {
                    userRight.PropertyChanged -= new PropertyChangedEventHandler(userRight_PropertyChanged);
                }
            }
        }

        void userRight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CanRead");
            OnPropertyChanged("CanWrite");
        }
        #endregion Обработчики

        #region Свойства
        public string TableText { get { return _name; } }
        public ObservableCollection<PgUserRightsM> RightsList { get { return _rightsList; } }

        public bool? CanRead
        {
            get
            {
                int canReadCount = 0;
                foreach (PgUserRightsM right in RightsList)
                {
                    if (right.CanRead)
                    {
                        canReadCount++;
                    }
                }
                if (canReadCount == 0)
                {
                    return false;
                }
                else if (canReadCount < RightsList.Count)
                {
                    return null;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                foreach (var right in RightsList)
                    right.CanRead = (bool)value;
                OnPropertyChanged("CanRead");
            }
        }
        public bool? CanWrite
        {
            get
            {
                int canWriteCount = 0;
                foreach (PgUserRightsM right in RightsList)
                {
                    if (right.CanWrite)
                    {
                        canWriteCount++;
                    }
                }
                if (canWriteCount == 0)
                {
                    return false;
                }
                else if (canWriteCount < RightsList.Count)
                {
                    return null;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                foreach (var right in RightsList)
                    right.CanWrite = (bool)value;
                OnPropertyChanged("CanWrite");
            }
        }
        #endregion Свойства
    }
}
