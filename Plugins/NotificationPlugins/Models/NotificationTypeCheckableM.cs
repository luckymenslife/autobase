using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationPlugins.Models
{
    public class NotificationTypeCheckableM:NotificationTypeM
    {
        #region Поля
        private bool _isChecked;
        #endregion

        #region Свойства
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                OnPropertyChanged(ref _isChecked, value, () => IsChecked);
            }
        }
        #endregion
    }
}
