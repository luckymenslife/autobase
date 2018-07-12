using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Rekod.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl, IDataErrorInfo
    {
        #region Статические поля
        public static readonly DependencyProperty MinValueProperty =
    DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown), new UIPropertyMetadata(0));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown), new UIPropertyMetadata(100));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new UIPropertyMetadata(0, new PropertyChangedCallback(s)));

        // Using a DependencyProperty as the backing store for ValueCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NumericUpDown), new PropertyMetadata());

        private static void s(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numUpdown = (NumericUpDown)d;
            numUpdown.TmpValue = e.NewValue.ToString();
        }

        // Using a DependencyProperty as the backing store for tmpValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TmpValueProperty =
            DependencyProperty.Register("TmpValue", typeof(string), typeof(NumericUpDown), new PropertyMetadata());



        #endregion // Статические поля

        #region Поля
        private int? _tmpValue;

        private ICommand _editCommand;
        private ICommand _goToPageCommand;
        #endregion // Поля

        #region Свойства
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                if (Value != value)
                    TmpValue = value.ToString();
            }
        }
        public string TmpValue
        {
            get { return (string)GetValue(TmpValueProperty); }
            set { SetValue(TmpValueProperty, value); }
        }
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion // Свойства

        #region Конструктор
        public NumericUpDown()
        {
            InitializeComponent();
            txtControl.DataContext = this;
        }
        #endregion // Конструктор


        #region EditCommand
        /// <summary>
        /// Открывает и закрывает окно для редактирования
        /// </summary>
        public ICommand EditCommand
        {
            get { return _editCommand ?? (_editCommand = new RelayCommand(this.Edit)); }
        }
        /// <summary>
        /// Открывает и закрывает окно для редактирования
        /// </summary>
        private void Edit(object obj = null)
        {
            if (IsErrorTmpValue())
                return;
            if (Command != null)
                Command.Execute(_tmpValue.Value);
        }
        #endregion // EditebleCommand

        #region GoToPageCommand
        /// <summary>
        /// Указывает номер страницы
        /// </summary>
        public ICommand GoToPageCommand
        {
            get { return _goToPageCommand ?? (_goToPageCommand = new RelayCommand(this.GoToPage, this.CanGoToPage)); }
        }
        /// <summary>
        /// Coment
        /// </summary>
        private void GoToPage(object obj = null)
        {
            if (!CanGoToPage(obj))
                return;
            int newValue = _tmpValue.Value;
            if (obj is string)
            {
                string str = (string)obj;
                switch (str)
                {
                    case "next":
                        newValue++;
                        break;
                    case "previous":
                        newValue--;
                        break;
                }
            }
            else if (obj is int)
                newValue = (int)obj;

            TmpValue = newValue.ToString();
        }
        private bool CanGoToPage(object obj = null)
        {
            Debug.WriteLine(obj, "CanGoToPage");
            if (!_tmpValue.HasValue)
                return false;
            int newValue = _tmpValue.Value;
            if (obj is string)
            {
                string str = (string)obj;
                switch (str)
                {
                    case "next":
                        newValue++;
                        break;
                    case "previous":
                        newValue--;
                        break;
                }
            }
            else if (obj is int)
                newValue = (int)obj;
            else
                return false;
            return (Command != null && Command.CanExecute(newValue));
        }
        #endregion // GoToPageCommand

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                Debug.WriteLine(columnName, "CheckedError");
                if (columnName == "TmpValue")
                {
                    _tmpValue = ExtraFunctions.Converts.To<int?>(TmpValue);
                    if (_tmpValue == null && TmpValue != null)
                        return Rekod.Properties.Resources.NumberIsNotCorrest;
                    if (_tmpValue != null && Command != null && !Command.CanExecute(_tmpValue.Value))
                    {
                        return Rekod.Properties.Resources.PageIsNotExists;
                    }
                }
                return null;
            }
        }

        private bool IsErrorTmpValue()
        {
            return (this["TmpValue"] != null);
        }

        private void Button_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                GoToPageCommand.Execute("next");
            if (e.Key == Key.Down)
                GoToPageCommand.Execute("previous");
            if (e.Key == Key.Enter)
                EditCommand.Execute(null);
        }
    }
}