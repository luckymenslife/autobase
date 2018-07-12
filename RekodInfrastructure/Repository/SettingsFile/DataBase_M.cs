using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Repository.SettingsFile
{
    public class DataBase_M
    {
        private List<string> _logins;

        public string DataBase { get; set; }
        public List<string> Logins
        { get { return _logins ?? (_logins = new List<string>()); } }

    }
}
