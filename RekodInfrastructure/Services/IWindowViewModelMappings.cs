using System;

namespace Rekod.Services
{
	/// <summary>
    /// Интерфейс описывающий отображение Window-ViewModel который используется в DialogService
	/// </summary>
	public interface IWindowViewModelMappings
	{
		/// <summary>
		/// Получает тип окна на основе зарегистрированного типа ViewModel
		/// </summary>
		/// <param name="viewModelType">Тип ViewModel</param>
		/// <returns>Тип окна на основе зарегистрированного типа ViewModel</returns>
		Type GetWindowTypeFromViewModelType(Type viewModelType);
	}
}