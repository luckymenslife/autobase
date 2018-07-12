using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class InputFormM : ViewModelBase
    {
        #region Поля
        private string _title;
        private string _label_text;
        private string _text;

        #endregion

        #region Конструктор
        public InputFormM(string title, string label_text, string text)
        {
            _title = title;
            _label_text = label_text;
            _text = text;
        }
        #endregion

        #region Свойства
        public string Title
        {
            get { return _title; }
            set
            {
                OnPropertyChanged(ref _title, value, () => this.Title);
            }
        }
        public string LabelText
        {
            get { return _label_text; }
            set
            {
                OnPropertyChanged(ref _label_text, value, () => this.LabelText);
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                OnPropertyChanged(ref _text, value, () => this.Text);
            }
        }
        #endregion

        #region OkCmd
        private ICommand _ok;
        public ICommand OkCmd
        {
            get { return _ok ?? (_ok = new RelayCommand(Ok, CanOk)); }
        }
        private bool CanOk(object obj)
        {
            return !string.IsNullOrEmpty(_text);
        }
        private void Ok(object obj)
        {
            if (obj is Window)
            {
                var win = obj as Window;
                win.DialogResult = true;
                win.Close();
            }
        }
        #endregion

        #region CancelCmd
        private ICommand _cancel;
        public ICommand CancelCmd
        {
            get { return _cancel ?? (_cancel = new RelayCommand(Cancel, CanCancel)); }
        }
        private bool CanCancel(object obj)
        {
            return true;
        }
        private void Cancel(object obj)
        {
            if (obj is Window)
            {
                var win = obj as Window;
                win.DialogResult = false;
                win.Close();
            }
        }
        #endregion
    }
}
