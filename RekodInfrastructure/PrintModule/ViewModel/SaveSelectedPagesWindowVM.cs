using Rekod.Controllers;
using Rekod.PrintModule.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Rekod.PrintModule.ViewModel
{
    class SaveSelectedPagesWindowVM: ViewModelBase
    {
        #region Поля
        private SaveSelectedPagesWindow _ownerWindow;
        #endregion Поля

        #region Конструкторы
        public SaveSelectedPagesWindowVM(SaveSelectedPagesWindow ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }
        #endregion Конструкторы

        #region Свойства
        public bool DialogResult
        {
            get;
            private set;
        }
        #endregion Свойства

        #region Команды
        #region SaveButtonCommand
        private ICommand _saveButtonCommand;
        /// <summary>
        /// Команда обработки нажатия кнопки сохранения
        /// </summary>
        public ICommand SaveButtonCommand
        {
            get { return _saveButtonCommand ?? (_saveButtonCommand = new RelayCommand(this.SaveButton, this.CanSaveButton)); }
        }
        /// <summary>
        /// Обработка нажатия кнопки сохранения
        /// </summary>
        public void SaveButton(object parameter = null)
        {
            DialogResult = true;
            _ownerWindow.Close();
        }
        /// <summary>
        /// Комментарий_к_методу_CanActionMethod
        /// </summary>
        public bool CanSaveButton(object parameter = null)
        {
            bool result = true;
            String prefixText = _ownerWindow.PrefixBox.Text; 
            result &= Directory.Exists(_ownerWindow.FolderPathBox.Text);
            result &= !String.IsNullOrEmpty(_ownerWindow.PrefixBox.Text.Trim());

            if (prefixText.Contains('\\') ||
                prefixText.Contains('/') ||
                prefixText.Contains(':') ||
                prefixText.Contains('*') ||
                prefixText.Contains('?') ||
                prefixText.Contains('"') ||
                prefixText.Contains('<') ||
                prefixText.Contains('>') ||
                prefixText.Contains('|'))
            {
                result = false;
                _ownerWindow.PrefixBox.BorderBrush = Brushes.Red;
                _ownerWindow.PrefixBox.BorderThickness = new Thickness(1.01);
            }
            else
            {
                _ownerWindow.PrefixBox.BorderBrush = Brushes.LightGray;
            }
            return result;
        }
        #endregion SaveButtonCommand 

        #region CancelCommand
        private ICommand _cancelCommand;
        /// <summary>
        /// Команда для закрытия окна
        /// </summary>
        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(this.Cancel, this.CanCancel)); }
        }
        /// <summary>
        /// Закрытие окна
        /// </summary>
        public void Cancel(object parameter = null)
        {
            _ownerWindow.Close();
        }
        /// <summary>
        /// Можно ли закрыть окно
        /// </summary>
        public bool CanCancel(object parameter = null)
        {
            return true;
        }
        #endregion // CancelCommand

        #region ChooseFolderCommand
        private ICommand _chooseFolderCommand;
        /// <summary>
        /// Команда для выбора папки сохранения
        /// </summary>
        public ICommand ChooseFolderCommand
        {
            get { return _chooseFolderCommand ?? (_chooseFolderCommand = new RelayCommand(this.ChooseFolder, this.CanChooseFolder)); }
        }
        /// <summary>
        /// Выбор папки для сохранения
        /// </summary>
        public void ChooseFolder(object parameter = null)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               _ownerWindow.FolderPathBox.Text = fbd.SelectedPath;
            }
        }
        /// <summary>
        /// Можно ли выбрать папку для сохранения
        /// </summary>
        public bool CanChooseFolder(object parameter = null)
        {
            return true;
        }
        #endregion ChooseFolderCommand
        #endregion Команды
    }
}