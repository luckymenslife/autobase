using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.ViewModel
{
    public class ToolButton_VM : ViewModelBase
    {
        #region Поля
        private ToolBar_VM _source;
        private string _name;
        private string _text;
        private bool _isVisable;
        private bool _isChecked;
        private Image _image;


        private Action<ToolButton_VM, object> _clickCommandAction;

        private RelayCommand _clickCommand;
        #endregion // Поля

        #region Свойста
        public ToolBar_VM Source
        { get { return _source; } }
        public string Name
        {
            get { return _name; }
        }
        public string Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { OnPropertyChanged(ref _isChecked, value, () => this.IsChecked); }
        }
        public bool IsVisable
        {
            get { return _isVisable; }
            set { OnPropertyChanged(ref _isVisable, value, () => this.IsVisable); }
        }
        public Image Image
        {
            get { return _image; }
            set { OnPropertyChanged(ref _image, value, () => this.Image); }
        }
        #endregion // Свойста

        #region Конструктор
        public ToolButton_VM(ToolBar_VM source, string name, Action<ToolButton_VM, object> clickCommandAction)
        {
            _source = source;
            _name = name;
            _clickCommandAction = clickCommandAction;
        }
        #endregion // Конструктор

        #region Команды
        public RelayCommand ClickCommand
        {
            get { return _clickCommand ?? (_clickCommand = new RelayCommand(this.Click)); }
        }

        private void Click(object obj)
        {
            if (_clickCommandAction != null)
                _clickCommandAction(this, obj);
        }
        #endregion
    }
}
