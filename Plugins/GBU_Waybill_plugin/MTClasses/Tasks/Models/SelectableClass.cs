using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public abstract class SelectableClass:ViewModelBase
    {
        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                OnPropertyChanged(ref _selected, value, () => Selected);
                MainPluginClass.AppEvents.CreateEvent("SelectedItem");
            }
        }
    }
}
