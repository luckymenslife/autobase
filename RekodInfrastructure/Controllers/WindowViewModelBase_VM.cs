using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rekod.Controllers
{
    public abstract class WindowViewModelBase_VM : ViewModelBase
    {
        #region Поля
        protected string _title;
        private ICommand _closeCommand;
        private Window _attachedWindow;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public virtual string Title
        {
            get { return _title; }
            set { OnPropertyChanged(ref _title, value, () => this.Title); }
        }
        public Window AttachedWindow
        {
            get { return _attachedWindow; }
        } 
        #endregion // Свойства

        #region Команды
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
            set { OnPropertyChanged(ref _closeCommand, value, () => this.CloseCommand); }
        }
        #endregion // Команды

        #region Методы
        /// <summary>
        /// Метод вызывающий команду закрытия окна
        /// </summary>
        /// <param name="obj">Параметры команды</param>
        internal void CloseWindow(object obj = null)
        {
            if (CloseCommand != null)
                CloseCommand.Execute(obj);
        }
        /// <summary>
        /// Метод, для переопределения, выполняющийся проверку перед закрытием окна 
        /// </summary>
        /// <returns>True</returns>
        protected virtual bool Closing(Object obj)
        {
            var child = obj as Window;
            if (child != null)
            {
                //child.Owner = null;
            }
            return true;
        }
        #endregion // Методы

        #region Статические методы
        public static Window GetWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, Window owner, Action<object> Close = null)
        {
            var window = new Window();
            window.Width = width;
            window.Height = height;
            window.MinWidth = minwidth;
            window.MinHeight = minheight; 
            window.Content = control;
            window.DataContext = dataContext;
            window.Owner = owner;

            Binding positionBinding = new Binding("Title")
            {
                Source = dataContext
            };
            window.SetBinding(Window.TitleProperty, positionBinding);

            CancelEventHandler delClose = (a, e) => 
            {
                e.Cancel = true;
                dataContext.CloseCommand.Execute(window);
                if (owner != null)
                {
                    owner.Activate();
                }
            };
            if (dataContext != null)
            {
                window.Closing += delClose;
                dataContext._attachedWindow = window;
                dataContext.CloseCommand = new RelayCommand(delegate(object obj)
                {
                    window.Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
                    {
                        if (dataContext.Closing(obj))
                        {
                            window.Closing -= delClose;
                            if (obj == window)
                                obj = null;
                            window.Close();
                            if (Close != null)
                                Close(obj);
                        }
                    }
                    ));
                });
            }
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(window);
            return window;
        }
        public static void OpenWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, Window owner = null)
        {
            var window = GetWindow(control, dataContext, width, height, minwidth, minheight, owner);
            window.Show();
        }
        public static object OpenWindowDialog(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, Window owner = null)
        {
            object value = null;
            var window = GetWindow(control, dataContext, width, height, minwidth, minheight, owner, delegate(object obj) { value = obj; });
            window.ShowDialog();
            return value;
        }
        #endregion // Статические методы
    }
}