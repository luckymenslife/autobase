using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.UserSets
{
    public class MessageInfo_VM : ViewModelBase
    {

        #region Поля
        private string _text;
        private enMessageStatus _status;
        private ObservableCollection<string> _listButtons;

        private Action<int> _eventResult;
        private RelayCommand _buttonClickConnamd;
        #endregion // Поля

        #region Свойства
        public string Text
        { get { return _text; } }

        public enMessageStatus Status
        { get { return _status; } }

        /// <summary>
        /// Коллекция кнопок 
        /// </summary>
        public ObservableCollection<string> ListButtons
        {
            get { return _listButtons; }
        }
        #endregion

        #region Конструктор
        public MessageInfo_VM()
        {
            _listButtons = new ObservableCollection<string>();
        }
        public void ClearStatus()
        {
            OnPropertyChanged(ref _text, null, () => this.Text);
            OnPropertyChanged(ref _status, enMessageStatus.None, () => this.Status);
            _listButtons.Clear();
            _eventResult = null;
        }
        public void SetStatus(string text, enMessageStatus status)
        {
            ClearStatus();
            OnPropertyChanged(ref _text, text, () => this.Text);
            OnPropertyChanged(ref _status, status, () => this.Status);
        }
        /// <summary>
        /// установить статус вопроса
        /// </summary>
        /// <param name="text">Текс вопроса</param>
        /// <param name="eventResult">Событие отрабатываемое нажатия кнопок (возвращает номер кнопки в массиве)</param>
        /// <param name="textButton">Коллекция названий кнопок</param>
        public void SetQuestion(string text,Action<int> eventResult, string[] textButton)
        {
            ClearStatus();
            OnPropertyChanged(ref _text, text, () => this.Text);
            ExtraFunctions.Sorts.SortList(_listButtons, textButton);
            _eventResult = eventResult;
            OnPropertyChanged(ref _status, enMessageStatus.Question, () => this.Status);
        }
        #endregion // Конструктор

        #region Команды
        public RelayCommand ButtonClickConnamd
        {
            get { return _buttonClickConnamd ?? (_buttonClickConnamd = new RelayCommand(this.ButtonClick)); }
        }

        private void ButtonClick(object obj)
        {
            string res = obj as string;

            if (_eventResult == null)
                return;
            var index = Array.IndexOf(_listButtons.ToArray(), res);
            _eventResult(index);
            ClearStatus();
            
        }
        #endregion // Команды
    }

    public enum enMessageStatus
    {
        None = 0,
        Error,
        Stop,
        Question,
        Exclamation,
        Warning,
        Information
    }
}
