using NotificationPlugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NotificationPlugins.Enums;

namespace NotificationPlugins
{
    
    class BackgroundInformation
    {
        private int _unread;
        private bool _passed;
        private bool _firstStart;
        

        public BackgroundInformation()
        {
            _unread = 0;
            _passed = false;
        }

        public int OutUnreadCount
        {
            get { return _unread; }
            set { _unread = value; }
        }

        public bool OutPassed
        {
            get { return _passed; }
            set { _passed = value; }
        }

        public bool FirstStart
        {
            get { return _firstStart; }
            set { _firstStart = value; }
        }
    }
}
