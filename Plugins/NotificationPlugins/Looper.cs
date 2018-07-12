using System;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using NotificationPlugins.Enums;
using Interfaces;
using NotificationPlugins.Infrastructure;
using NotificationPlugins.ViewModels;
using NotificationPlugins.Views;
using MenuItem = System.Windows.Controls.MenuItem;

namespace NotificationPlugins
{
    public class Looper
    {
        // update, download
        private readonly BackgroundWorker _bgworker;
        private readonly int TimerIntervalSec = 5;
        private BackgroundInformation _info;
        private Timer _timer;
        // main menu
        private MenuItem _mainMenu;
        private const string MainMenuHeader = "Уведомления";
        private MenuItem _notificationMenu;
        // notifications windows
        public static NotificationsV _formNotification;
        private NotificationsVM _dataWindows;
        private NotificationRepository _repo;
        private Timer _timerMenu;
        private bool _empty;

        public Looper(NotificationRepository repo)
        {
            _repo = repo;
            _timer = new Timer();
            _timer.Interval = TimerIntervalSec * 1000;
            _timer.Tick += Timer_Tick;
            PluginInitialization._appEvent.PropertyChanged += _appEvent_PropertyChanged;
            AddMainMenu();
            _info = new BackgroundInformation();
            _info.FirstStart = true;
            _bgworker = new BackgroundWorker();
            _bgworker.DoWork += Bgworker_DoWork;
            _bgworker.RunWorkerCompleted += Bgworker_RunWorkerCompleted;
            _timer.Enabled = true;

            _timerMenu = new Timer();
            _timerMenu.Interval = 1000;
            _timerMenu.Tick += _timerMenu_Tick;
        }

        private void _appEvent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("ReadNotification"))
            {
                Timer_Tick(null, null);
            }
        }

        private void _timerMenu_Tick(object sender, EventArgs e)
        {
            _empty = !_empty;
            MenuAnimation(_empty);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _timer.Enabled = false;
                _bgworker.RunWorkerAsync(_info);
            }
            catch
            {
                // ignored
            }
        }
        private void AddMainMenu()
        {
            _mainMenu = new System.Windows.Controls.MenuItem();
            _mainMenu.Header = MainMenuHeader;
            _mainMenu.Click += NotificationMenu_Click;

            PluginInitialization._work.MainForm.Menu(_mainMenu);
            _mainMenu.IsEnabled = true;
        }
        private void ShowFormNotification()
        {
            if (_formNotification == null)
            {
                _dataWindows = new NotificationsVM(this, new NotificationRepository(PluginInitialization._app));
                _formNotification = new NotificationsV();
                _formNotification.DataContext = _dataWindows;
                _formNotification.Show();
            }
            else
            {
                if (_formNotification.IsVisible)
                    _formNotification.Activate();
                else
                    _formNotification.Show();
                _dataWindows.ReloadCmd.Execute(null);
            }

        }
        private void NotificationMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowFormNotification();
        }
        private void Bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundInformation q = e.Result as BackgroundInformation;
            if (q == null || !q.OutPassed)
            {
                _timerMenu.Enabled = false;
                MenuAnimation(true, true);
            }
            else if (q.OutPassed)
            {
                _info = q;
                if (q.OutUnreadCount == 0)
                {
                    _timerMenu.Enabled = false;
                    MenuAnimation(true);
                }
                else
                {
                    if (!_timerMenu.Enabled)
                    {
                        _timerMenu.Enabled = true;
                    }
                    if (_info.FirstStart)
                    {
                        ShowFormNotification();
                    }
                }
            }
            _info.FirstStart = false;
            _timer.Enabled = true;
        }
        private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundInformation myinfo = e.Argument as BackgroundInformation;
            if (myinfo == null) return;

            try
            {
                myinfo.OutUnreadCount = _repo.GetUnreadCount();
                myinfo.OutPassed = true;
            }
            catch
            {
                myinfo.OutPassed = false;
            }
            e.Result = myinfo;
        }
        private void MenuAnimation(bool empty, bool error = false)
        {
            if (empty)
            {
                _mainMenu.Header = MainMenuHeader+(error?" (Ошибка)":"");
                _mainMenu.FontWeight = FontWeights.Normal;
            }
            else
            {
                _mainMenu.FontWeight = FontWeights.Bold;
                _mainMenu.Header = MainMenuHeader + " +" + _info.OutUnreadCount.ToString();
            }

        }
    }
}
