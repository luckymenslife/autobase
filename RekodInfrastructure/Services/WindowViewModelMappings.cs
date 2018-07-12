using System;
using System.Collections.Generic;

namespace Rekod.Services
{
	/// <summary>
    /// Класс описывающий отображение Window-ViewModel который используется в DialogService
	/// </summary>
	public class WindowViewModelMappings : IWindowViewModelMappings
	{
		private IDictionary<Type, Type> mappings;

		/// <summary>
        /// Инициализирует новый экземляр класса WindowViewModelMappings
		/// </summary>
		public WindowViewModelMappings()
		{
			mappings = new Dictionary<Type, Type>
			{
                // { typeof(SomeViewModel), typeof(SomeDialog) }
                { typeof(Rekod.DataAccess.TableManager.ViewModel.TableManagerVM), typeof(Rekod.WinMain) }
			};
		}
        
		/// <summary>
        /// Получает тип окна на основе зарегистрированного типа ViewModel
		/// </summary>
        /// <param name="viewModelType">Тип ViewModel</param>
        /// <returns>Тип окна на основе зарегистрированного типа ViewModel</returns>
		public Type GetWindowTypeFromViewModelType(Type viewModelType)
		{
			return mappings[viewModelType];
		}
	}
}