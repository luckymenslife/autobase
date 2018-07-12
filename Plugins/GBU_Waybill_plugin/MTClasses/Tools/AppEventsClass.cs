using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tools
{
    public class AppEventsClass:ViewModelBase
    {
        public void CreateEvent(string event_text)
        {
            OnPropertyChanged(event_text);
        }
    }
}
