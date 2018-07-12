using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Controllers;
using System.Windows.Input;

namespace Rekod.DataAccess.SourcePostgres.Model.PgHistory
{
    public class PgHistoryDate : ViewModelBase
    {
        #region Поля
        private ObservableCollection<PgHistoryEvent> _events;
        private int _eventsLoaded;
        private int _eventsCount;
        private int _loadByCount = 100;
        private bool _loadMore;
        private bool _eventsAreLoaded = false;
        #endregion Поля

        #region Конструкторы
        public PgHistoryDate(DateTime historyDate, PgVM.PgHistoryVM parentVM)
        {
            HistoryDate = historyDate;
            ParentPgHistoryVM = parentVM; 
        }
        #endregion Конструкторы

        #region Свойства
        public DateTime HistoryDate { get; private set; }
        public PgVM.PgHistoryVM ParentPgHistoryVM { get; private set; }
        public int EventsCount
        {
            get { return _eventsCount; }
            set
            {
                OnPropertyChanged(ref _eventsCount, value, () => this.EventsCount);
                OnPropertyChanged("LoadMoreString");
            }
        }
        public int EventsLoaded
        {
            get { return _eventsLoaded; }
            set 
            {
                OnPropertyChanged(ref _eventsLoaded, value, () => this.EventsLoaded);
                OnPropertyChanged("LoadMoreString");
            }
        }
        public int LoadByCount
        {
            get { return _loadByCount; }
        }
        public bool LoadMore
        {
            get 
            {
                return _loadMore; 
            }
            set
            {
                OnPropertyChanged(ref _loadMore, value, () => this.LoadMore);
            }
        }
        public String LoadMoreString
        {
            get
            {
                return String.Format("Загрузить еще. Загружено {0} из {1} ({2}%)",
                    EventsLoaded,
                    EventsCount,
                    Convert.ToInt32(EventsLoaded / (double)EventsCount * 100)); 
            }
        }
        public bool EventsAreLoaded
        {
            get { return _eventsAreLoaded; }
            set { OnPropertyChanged(ref _eventsAreLoaded, value, () => this.EventsAreLoaded); }
        }
        #endregion Свойства

        #region Коллекции
        public ObservableCollection<PgHistoryEvent> Events
        {
            get { return _events ?? (_events = new ObservableCollection<PgHistoryEvent>()); }
        }
        #endregion Коллекции

        #region Методы
        public override string ToString()
        {
            return HistoryDate.ToLongDateString(); 
        }
        #endregion Методы

        #region Команды
        #region LoadMoreEventsCommand
        private ICommand _loadMoreEventsCommand;
        /// <summary>
        /// Команда для подгрузки новых событий для даты
        /// </summary>
        public ICommand LoadMoreEventsCommand
        {
            get { return _loadMoreEventsCommand ?? (_loadMoreEventsCommand = new RelayCommand(this.LoadMoreEvents, this.CanLoadMoreEvents)); }
        }
        /// <summary>
        /// Подгрузка новых событий
        /// </summary>
        public void LoadMoreEvents(object parameter = null)
        {
            ParentPgHistoryVM.LoadDate(this, true);
        }
        /// <summary>
        /// Можно ли подгрузить новые события
        /// </summary>
        public bool CanLoadMoreEvents(object parameter = null)
        {
            return true;
        }
        #endregion LoadMoreEventsCommand 
        #endregion Команды
    }
}