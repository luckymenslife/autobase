using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;

namespace Rekod.Services
{
	/// <summary>
    /// Интерфейс ответственный за абстрагирование моделей представления от представлений
	/// </summary>
	public interface IDialogService
	{
		/// <summary>
        /// Получает зарегистрированные представления
		/// </summary>
		ReadOnlyCollection<FrameworkElement> Views { get; }

		/// <summary>
        /// Регистрирует представление
		/// </summary>
        /// <param name="view">Зарегистрированное представление</param>
		void Register(FrameworkElement view);

		/// <summary>
		/// Снимает представление с регистрации
		/// </summary>
		/// <param name="view">Представление для снятия с регистрации</param>
		void Unregister(FrameworkElement view);

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
		bool? ShowDialog(object ownerViewModel, object viewModel);


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
		bool? ShowDialog<T>(object ownerViewModel, object viewModel) where T : Window;


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
		MessageBoxResult ShowMessageBox(
			object ownerViewModel,
			string messageBoxText,
			string caption,
			MessageBoxButton button,
			MessageBoxImage icon);
	}
}