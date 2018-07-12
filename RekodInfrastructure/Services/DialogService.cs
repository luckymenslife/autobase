using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DialogResult = System.Windows.Forms.DialogResult;

namespace Rekod.Services
{
	/// <summary>
    /// Класс ответственный за абстрагирование моделей представления от представлений
	/// </summary>
	public class DialogService : IDialogService
	{
        #region Статические члены
        public static readonly DialogService DialogServiceObject;
        static DialogService()
        {
            DialogServiceObject = new DialogService();
        } 
        #endregion

        #region Поля
        private readonly HashSet<FrameworkElement> views;
        private readonly IWindowViewModelMappings windowViewModelMappings;
        private Rekod.WinMain _mainWindow; 
        #endregion Поля

        #region Конструкторы
        /// <summary>
        /// Инициализирует новый экземляр класса DialogService
        /// </summary>
        /// <param name="windowViewModelMappings">
        /// Отображение Окно - Модель представления. По умолчанию - null
        /// </param>
        public DialogService()
        {
            this.windowViewModelMappings = new WindowViewModelMappings(); 
            views = new HashSet<FrameworkElement>();
        }
        #endregion Конструкторы

        #region Члены IDialogService
        /// <summary>
		/// Получает зарегистрированные представления
		/// </summary>
		public ReadOnlyCollection<FrameworkElement> Views
		{
			get { return new ReadOnlyCollection<FrameworkElement>(views.ToList()); }
		}

		/// <summary>
		/// Регистрирует представление
		/// </summary>
		/// <param name="view">Зарегистрированное представление</param>
		public void Register(FrameworkElement view)
		{
            if (view.GetType() == typeof(Rekod.WinMain))
            {
                _mainWindow = (Rekod.WinMain)view; 
            }
            // Получает окно - владельца
			Window owner = GetOwner(view);
			if (owner == null)
			{
				// Выполняет отложенную регистрацию если представление еще не было загружено
				// Такое может случиться, если представление содержится например во Frame
				view.Loaded += LateRegister;
				return;
			}

			// Подписываемся на событие закрытия окна, так как мы должны затем снять с регистрации представление
            // для предотвращения утечки памяти
			owner.Closed += OwnerClosed;

			views.Add(view);
		}

		/// <summary>
		/// Снять представление с регистрации
		/// </summary>
		/// <param name="view">Представление которое нужно снять с регистрации</param>
		public void Unregister(FrameworkElement view)
		{
			views.Remove(view);
		}

		/// <summary>
		/// Показывает диалог
		/// </summary>
		/// <remarks>
        /// Диалог, используемый для представления ViewModel извлекается из списка зарегистрированных представлений
		/// </remarks>
		/// <param name="ownerViewModel">
        /// Модель представления которая представляет окно-владельца диалога
		/// </param>
		/// <param name="viewModel">Модель представления диалога</param>
		/// <returns>
		/// Nullable значение типа bool который означает как окно было закрыто пользователем
		/// </returns>
		public bool? ShowDialog(object ownerViewModel, object viewModel)
		{
			Type dialogType = windowViewModelMappings.GetWindowTypeFromViewModelType(viewModel.GetType());
			return ShowDialog(ownerViewModel, viewModel, dialogType);
		}

		/// <summary>
		/// Показывает диалог
		/// </summary>
		/// <param name="ownerViewModel">
        /// Модель представления которая представляет окно-владельца диалога
		/// </param>
		/// <param name="viewModel">The ViewModel of the new dialog.</param>
		/// <typeparam name="T">Тип диалога, который нужно отобразить пользователю</typeparam>
		/// <returns>
        /// Nullable значение типа bool который означает как окно было закрыто пользователем
		/// </returns>
		public bool? ShowDialog<T>(object ownerViewModel, object viewModel) where T : Window
		{
			return ShowDialog(ownerViewModel, viewModel, typeof(T));
		}

		/// <summary>
		/// Показывает MessageBox
		/// </summary>
		/// <param name="ownerViewModel">
        /// Модель представления которая представляет окно-владельца диалога
		/// </param>
		/// <param name="messageBoxText">Строка определяющая тект который нужно отобразить</param>
		/// <param name="caption">Строка определяющая текст в заголовке</param>
		/// <param name="button">
		/// Значение MessageBoxButton которое определяют какую кнопку или кнопки отобразить
		/// </param>
		/// <param name="icon">Значение MessageBoxImage которое определяет какую иконку отобразить</param>
		/// <returns>
        /// Значение MessageBoxResult которое определяет какая кнопка была нажата пользователем
		/// </returns>
		public MessageBoxResult ShowMessageBox(
			object ownerViewModel,
			string messageBoxText,
			string caption,
			MessageBoxButton button,
			MessageBoxImage icon)
		{
			return MessageBox.Show(FindOwnerWindow(ownerViewModel), messageBoxText, caption, button, icon);
		}

		#endregion

		#region Прикрепленные свойства
		/// <summary>
        /// Прикрепленное свойство зависимости описывающая, ведет ли себя FrameworkElement как представление в MVVM
		/// </summary>
		public static readonly DependencyProperty IsRegisteredViewProperty =
			DependencyProperty.RegisterAttached(
			"IsRegisteredView",
			typeof(bool),
			typeof(DialogService),
			new UIPropertyMetadata(IsRegisteredViewPropertyChanged));

		/// <summary>
        /// Берет значение описывающее, ведет ли себя FrameworkElement как представление в MVVM
		/// </summary>
		public static bool GetIsRegisteredView(FrameworkElement target)
		{
			return (bool)target.GetValue(IsRegisteredViewProperty);
		}

		/// <summary>
        /// Получает значение описывающее, ведет ли себя FrameworkElement как представление в MVVM
		/// </summary>
		public static void SetIsRegisteredView(FrameworkElement target, bool value)
		{
			target.SetValue(IsRegisteredViewProperty, value);
		}

		/// <summary>
        /// Ответственно за обработки изменение свойства IsRegisteredViewProperty, 
        /// т.е. ведет ли себя FrameworkElement как представление в MVVM или нет
		/// </summary>
		private static void IsRegisteredViewPropertyChanged(DependencyObject target,
			DependencyPropertyChangedEventArgs e)
		{
			// The Visual Studio Designer or Blend will run this code when setting the attached
			// property, however at that point there is no IDialogService registered
			// in the ServiceLocator which will cause the Resolve method to throw a ArgumentException.
			if (DesignerProperties.GetIsInDesignMode(target)) return;

			FrameworkElement view = target as FrameworkElement;
			if (view != null)
			{
				// Cast values
				bool newValue = (bool)e.NewValue;
				bool oldValue = (bool)e.OldValue;

				if (newValue)
				{
					DialogServiceObject.Register(view);
				}
				else
				{
                    DialogServiceObject.Unregister(view);
				}
			}
		}
		#endregion

        #region Методы
        /// <summary>
        /// Показывает диалог
        /// </summary>
        /// <param name="ownerViewModel">
        /// Модель представления которая представляет окно-владельца диалога
        /// </param>
        /// <param name="viewModel">Модель представления нового диалога</param>
        /// <param name="dialogType">Тип диалога</param>
        /// <returns>
        /// Nullable bool, который определяет как окно было закрыто пользователем
        /// </returns>
        private bool? ShowDialog(object ownerViewModel, object viewModel, Type dialogType)
        {
            // Создать диалог и установить свойтсва
            Window dialog = (Window)Activator.CreateInstance(dialogType);
            dialog.Owner = FindOwnerWindow(ownerViewModel);
            dialog.DataContext = viewModel;

            // Показать диалог
            return dialog.ShowDialog();
        }
        /// <summary>
        /// Показывает диалог
        /// </summary>
        /// <param name="viewModel">Модель представления нового диалога</param>
        /// <param name="windowType">Тип диалога</param>
        /// <returns>
        /// Nullable bool, который определяет как окно было закрыто пользователем
        /// </returns>
        public void ShowWindow(object viewModel, Type windowType)
        {
            // Создать диалог и установить свойства
            Window dialog = (Window)Activator.CreateInstance(windowType);
            dialog.DataContext = viewModel;
            dialog.Owner = _mainWindow;
            // Показать диалог
            dialog.Show();
        }
        /// <summary>
        /// Показывает диалог
        /// </summary>
        /// <param name="viewModel">Модель представления нового диалога</param>
        /// <param name="windowType">Тип диалога</param>
        public void ShowWindow(Type windowType)
        {
            // Создать диалог и установить свойства
            Window dialog = (Window)Activator.CreateInstance(windowType);
            dialog.Owner = _mainWindow;
            // Показать диалог
            dialog.Show();
        }
        /// <summary>
        /// Находит окно соответствующее указанному ViewModel
        /// </summary>
        private Window FindOwnerWindow(object viewModel)
        {
            FrameworkElement view = views.SingleOrDefault(v => ReferenceEquals(v.DataContext, viewModel));
            if (view == null)
            {
                throw new ArgumentException("Viewmodel is not referenced by any registered View.");
            }

            // Получить окно владельца
            Window owner = view as Window;
            if (owner == null)
            {
                owner = Window.GetWindow(view);
            }

            // Удостоверимся, что окно-владелец было найдено
            if (owner == null)
            {
                throw new InvalidOperationException("View is not contained within a Window.");
            }

            return owner;
        }
        /// <summary>
        /// Обратный вызов для поздней регистрации представления. 
        /// Не было возможности сделать немедленную регистрацию, так как
        /// представление не было в этот момент частью логического или визуального дерева
        /// </summary>
        private void LateRegister(object sender, RoutedEventArgs e)
        {
            FrameworkElement view = sender as FrameworkElement;
            if (view != null)
            {
                // Отписаться от события загрузки представления
                view.Loaded -= LateRegister;

                // Зарегистрировать представление
                Register(view);
            }
        }
        /// <summary>
        /// Обрабатывает закрытие окна владельца, ViewService должен снять с регистрации
        /// все представления работающие внутри закрываемого окна
        /// </summary>
        private void OwnerClosed(object sender, EventArgs e)
        {
            Window owner = sender as Window;
            if (owner != null)
            {
                // Найти представления работающие внутри закрываемого окна
                IEnumerable<FrameworkElement> windowViews =
                    from view in views
                    where Window.GetWindow(view) == owner
                    select view;

                // Снять с регистрации представления в окна
                foreach (FrameworkElement view in windowViews.ToArray())
                {
                    Unregister(view);
                }
            }
        }
        /// <summary>
        /// Получает окна-владельца представления
        /// </summary>
        /// <param name="view">Представление</param>
        /// <returns>Окно-владелец если найдено, иначе null</returns>
        private Window GetOwner(FrameworkElement view)
        {
            return view as Window ?? Window.GetWindow(view);
        }
        #endregion Методы
    }
}