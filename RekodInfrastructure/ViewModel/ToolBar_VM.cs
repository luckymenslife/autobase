using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.ViewModel
{
    public class ToolBar_VM : ViewModelBase
    {
        #region Поля
        private string _name;
        private bool _isVisable;
        private MTObservableCollection<ToolButton_VM> _listButton;
        #endregion // Поля

        #region Свойства
        public string Name
        {
            get { return _name; }
        }
        public bool IsVisable
        {
            get { return _isVisable; }
            set { OnPropertyChanged(ref _isVisable, value, () => this.IsVisable); }
        }
        public MTObservableCollection<ToolButton_VM> ListButton
        {
            get { return _listButton; }
        }
        #endregion // Свойства

        #region Коструктор
        public ToolBar_VM(string name)
        {
            _listButton = new MTObservableCollection<ToolButton_VM>();
            _name = name;
        }
        #endregion // Коструктор

    }
}
