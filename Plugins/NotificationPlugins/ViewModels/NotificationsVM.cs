using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NotificationPlugins.Enums;
using NotificationPlugins.Infrastructure;
using NotificationPlugins.Models;
using NotificationPlugins.Views;
using MessageBox = System.Windows.MessageBox;

namespace NotificationPlugins.ViewModels
{
    public class NotificationsVM : ViewModelBase
    {
        #region Статические поля

        private static int COUNT_IN_PAGE = 100;
        private static List<NotificationTypeM> _types;
        private static List<NotificationPriorityM> _priorityes;
        #endregion

        #region Поля
        private ObservableCollection<NotificationVM> _notifications;
        private int _countUnread;
        private int _countFavorite;
        private Looper _looper;
        private NotificationRepository _repo;
        private List<NotificationTypeM> _filterTypes;
        private List<NotificationPriorityM> _filterPriorityes;
        private string _filterText;
        private NotificationPriorityM _selectedFilterPriority;
        private NotificationTypeM _selectedFilterType;
        private ENotificationStatus _selectedFilterNotificationStatus;
        private int _countAll;
        private int _pageNum;
        private NotificationVM _selectedNotification;
        private Timer _timerReload;
        #endregion

        #region Конструктор
        public NotificationsVM(Looper looper, NotificationRepository repo)
        {
            _looper = looper;
            _repo = repo;
            _filterTypes = new List<NotificationTypeM>();
            _filterPriorityes = new List<NotificationPriorityM>();

            LoadTypes();
            LoadPriorityes();
            Notifications = new ObservableCollection<NotificationVM>();
            ClearFilter(null);

            _timerReload = new Timer();
            _timerReload.Interval = 800;
            _timerReload.Tick += _timerReload_Tick;
            _timerReload.Enabled = false;
        }
        #endregion

        #region Статические свойства
        public static List<NotificationTypeM> Types
        {
            get { return NotificationsVM._types; }
        }
        public static List<NotificationPriorityM> Priorityes
        {
            get { return NotificationsVM._priorityes; }
        }
        #endregion

        #region Свойства
        public ObservableCollection<NotificationVM> Notifications
        {
            get { return _notifications; }
            set
            {
                OnPropertyChanged(ref _notifications, value, () => Notifications);
            }
        }
        public int CountUnread
        {
            get { return _countUnread; }
            set { OnPropertyChanged(ref _countUnread, value, () => CountUnread); }
        }
        public int CountFavorite
        {
            get { return _countFavorite; }
            set { OnPropertyChanged(ref _countFavorite, value, () => CountFavorite); }
        }
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                OnPropertyChanged(ref _filterText, value, () => FilterText);
                TimerStart();
            }
        }

        private void TimerStart()
        {
            if (_timerReload != null)
            {
                _timerReload.Stop();
                _timerReload.Start();
            }
        }

        public List<NotificationPriorityM> FilterPriorityes
        {
            get { return _filterPriorityes; }
        }
        public List<NotificationTypeM> FilterTypes
        {
            get { return _filterTypes; }
        }
        public NotificationPriorityM SelectedFilterPriority
        {
            get { return _selectedFilterPriority; }
            set
            {
                OnPropertyChanged(ref _selectedFilterPriority, value, () => SelectedFilterPriority);
                TimerStart();
            }
        }
        public NotificationTypeM SelectedFilterType
        {
            get { return _selectedFilterType; }
            set
            {
                OnPropertyChanged(ref _selectedFilterType, value, () => SelectedFilterType);
                TimerStart();
            }
        }
        public ENotificationStatus SelectedFilterNotificationStatus
        {
            get { return _selectedFilterNotificationStatus; }
            set
            {
                OnPropertyChanged(ref _selectedFilterNotificationStatus, value, () => SelectedFilterNotificationStatus);
                TimerStart();
            }
        }
        public NotificationVM SelectedNotification
        {
            get { return _selectedNotification; }
            set
            {
                OnPropertyChanged(ref _selectedNotification, value, () => SelectedNotification);
                if (_selectedNotification != null && !_selectedNotification.Read)
                {
                    _selectedNotification.ReadNotification();
                }
            }
        }
        public int PageNum
        {
            get { return _pageNum == 0 ? 1 : _pageNum; }
            private set
            {
                OnPropertyChanged(ref _pageNum, value, () => PageNum);
            }
        }
        public int CountAll
        {
            get { return _countAll; }
            private set
            {
                OnPropertyChanged(ref _countAll, value, () => CountAll);
            }
        }
        #endregion

        #region Методы
        private void LoadTypes()
        {
            if (_types == null)
                _types = new List<NotificationTypeM>();
            else
                _types.Clear();
            _filterTypes.Clear();
            _filterTypes.Add(new NotificationTypeM() { Gid = -1, Name = "Все" });
            var loadTypes = _repo.GetNotificationTypes();
            foreach (var item in loadTypes)
            {
                _types.Add(item);
                _filterTypes.Add(item);
            }
        }
        private void LoadPriorityes()
        {
            if (_priorityes == null)
                _priorityes = new List<NotificationPriorityM>();
            else
                _priorityes.Clear();
            _filterPriorityes.Clear();
            _filterPriorityes.Add(new NotificationPriorityM(-1, 0, "Все", KnownColor.White));
            var loadPriorityes = _repo.GetPriorityTypes();
            foreach (var item in loadPriorityes)
            {
                _priorityes.Add(item);
                _filterPriorityes.Add(item);
            }
        }
        private void LoadNotifications()
        {
            string proc = PluginInitialization._work.OpenForm.ProcOpen("LoadNotifications");
            try
            {
                CountAll = _repo.GetCountNotifications(
                SelectedFilterNotificationStatus,
                SelectedFilterPriority,
                SelectedFilterType,
                FilterText,
                COUNT_IN_PAGE,
                PageNum);

                if (Notifications == null)
                    Notifications = new ObservableCollection<NotificationVM>();
                else
                    Notifications.Clear();

                var data = _repo.GetNotifications(
                    SelectedFilterNotificationStatus,
                    SelectedFilterPriority,
                    SelectedFilterType,
                    FilterText,
                    COUNT_IN_PAGE,
                    PageNum);
                foreach (var item in data)
                {
                    Notifications.Add(new NotificationVM(item, _repo));
                }
            }
            catch (Exception ex)
            {
                PluginInitialization._work.OpenForm.ProcClose(proc);
                MessageBox.Show("Ошибка при получении уведомлений! Описание: " + ex.Message, "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                PluginInitialization._work.OpenForm.ProcClose(proc);
            }
        }
        private void _timerReload_Tick(object sender, EventArgs e)
        {
            _timerReload.Enabled = false;
            Reload(null);
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

        #region ReloadCmd
        private ICommand _reloadCmd;
        public ICommand ReloadCmd
        {
            get { return _reloadCmd ?? (_reloadCmd = new RelayCommand(Reload)); }
        }
        private void Reload(object obj)
        {
            PageNum = 1;
            LoadNotifications();
        }
        #endregion

        #region ClearFilterCmd
        private ICommand _clearFilterCmd;
        public ICommand ClearFilterCmd
        {
            get { return _clearFilterCmd ?? (_clearFilterCmd = new RelayCommand(ClearFilter)); }
        }
        private void ClearFilter(object obj)
        {
            FilterText = "";
            SelectedFilterPriority = FilterPriorityes.First();
            SelectedFilterType = FilterTypes.First();
            SelectedFilterNotificationStatus = ENotificationStatus.All;
            PageNum = 1;
            LoadNotifications();
        }
        #endregion

        #region SetAllAsReadedCmd
        private ICommand _setAllAsReadedCmd;
        public ICommand SetAllAsReadedCmd
        {
            get { return _setAllAsReadedCmd ?? (_setAllAsReadedCmd = new RelayCommand(SetAllAsReaded)); }
        }
        private void SetAllAsReaded(object obj)
        {
            string proc = PluginInitialization._work.OpenForm.ProcOpen("SetAllAsReaded");
            int? id = null;
            try
            {
                if (SelectedNotification != null)
                    id = SelectedNotification.Notification.Gid;
                _repo.SetReadedAllNotification(
                    SelectedFilterNotificationStatus,
                    SelectedFilterPriority,
                    SelectedFilterType,
                    FilterText);
                PluginInitialization._appEvent.CreateEvent("ReadNotification");
            }
            catch (Exception ex)
            {
                PluginInitialization._work.OpenForm.ProcClose(proc);
                MessageBox.Show("Ошибка при изменеии уведомлений! Описание: " + ex.Message, "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                PluginInitialization._work.OpenForm.ProcClose(proc);
            }
            Reload(null);
            if (id.HasValue)
                SelectedNotification = Notifications.FirstOrDefault(w => w.Notification.Gid == id.Value);

        }
        #endregion

        #region ChangePageCmd
        private ICommand _changePageCmd;
        public ICommand ChangePageCmd
        {
            get { return _changePageCmd ?? (_changePageCmd = new RelayCommand(ChangePage, CanChangePage)); }
        }
        private bool CanChangePage(object obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
                return false;

            if (CountAll != 0)
            {
                double countPage = CountAll / COUNT_IN_PAGE;
                if (CountAll % COUNT_IN_PAGE > 0)
                    countPage++;
                if (obj.ToString().ToUpper() == "ToLeft".ToUpper())
                {
                    if (PageNum == 1) return false;
                    return true;
                }
                else if (obj.ToString().ToUpper() == "ToRight".ToUpper())
                {
                    if (countPage == PageNum) return false;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        private void ChangePage(object obj)
        {
            if (!CanChangePage(obj))
                return;

            if (obj.ToString().ToUpper() == "ToLeft".ToUpper())
            {
                PageNum--;
            }
            else if (obj.ToString().ToUpper() == "ToRight".ToUpper())
            {
                PageNum++;
            }
            LoadNotifications();
        }
        #endregion

        #region OpenUserSettingsCmd
        private ICommand _openUserSettingsCmd;
        public ICommand OpenUserSettingsCmd
        {
            get { return _openUserSettingsCmd ?? (_openUserSettingsCmd = new RelayCommand(OpenUserSettings)); }
        }
        private void OpenUserSettings(object obj)
        {
            var data = new UserSettingsVM(_repo);
            var frm = new UserSettingsV();
            frm.DataContext = data;
            frm.ShowDialog();
        }
        #endregion
        #endregion
    }
}
