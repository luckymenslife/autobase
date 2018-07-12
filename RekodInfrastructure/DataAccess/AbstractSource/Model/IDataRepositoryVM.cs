using Rekod.DataAccess.AbstractSource.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.Windows.Input;
using System.Collections.Specialized;
using System.ComponentModel;
using Rekod.Controllers;

namespace Rekod.DataAccess.AbstractSource.Model
{
    /// <summary>
    /// Интерфейс описания источника
    /// </summary>
    public interface IDataRepositoryM : IDisposable
    {
        #region Свойства
        /// <summary>
        /// Получает идентификатор источника
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Тип источника
        /// </summary>
        ERepositoryType Type { get; }
        /// <summary>
        /// Группы содержащиеся в источнике
        /// </summary>
        IEnumerable<AbsM.IGroupM> Groups { get; }
        /// <summary>
        /// Список всех таблиц зарегистрированных в источнике
        /// </summary>
        IEnumerable<AbsM.ITableBaseM> Tables { get; }
        /// <summary>
        /// Определяет источник как хранилище объектов
        /// </summary>
        bool IsTable { get; }
        
        #endregion // Свойства

        #region События
        event NotifyCollectionChangedEventHandler GroupsCollectionChanged;
        event NotifyCollectionChangedEventHandler TablesCollectionChanged;
        event PropertyChangedEventHandler TablePropertyChanged;
        #endregion

        #region Методы
        /// <summary>
        /// Обновление всех методанных источника
        /// </summary>
        void ReloadInfo();

        /// <summary>
        /// Проверка выполнения источника
        /// </summary>
        /// <returns></returns>
        bool CheckRepository();

        /// <summary>
        /// Найти таблицу по ID
        /// </summary>
        /// <param name="id">Идентификатор таблицы</param>
        /// <returns>Таблица</returns>
        AbsM.ITableBaseM FindTable(object id);

        /// <summary>
        /// Найти таблицу по имени
        /// </summary>
        /// <param name="name">Имя таблицы</param>
        /// <returns>Таблица</returns>
        AbsM.ITableBaseM FindTableByName(string name);

        /// <summary>
        /// Поиск поля в таблце
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатор поля</param>
        /// <returns>Найденое поле</returns>
        AbsM.IFieldM FindField(AbsM.ITableBaseM table, object id);
        /// <summary>
        /// Открыть окно списка объектов
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатором объекта для перейти к нему</param>
        /// <param name="isSelected">Открыть окно для выбора объекта</param>
        /// <param name="ownerMV">Родитель ViewModel, если Null то главное окно</param>
        /// <returns>Если окно открыто для выбора, возврощает идентификатор объекта</returns>
        object OpenTable(AbsM.ITableBaseM table, object id = null, bool isSelected = false, WindowViewModelBase_VM ownerMV = null);
        /// <summary>
        /// Открыть окно список атрибутов объекта
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="ownerMV">Родитель ViewModel, если Null то главное окно</param>
        void OpenObject(AbsM.ITableBaseM table, object id, String wkt = null, WindowViewModelBase_VM ownerMV = null);

        void OpenWindow(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, WindowViewModelBase_VM ownerMV = null);
        object OpenWindowDialog(UserControl control, WindowViewModelBase_VM dataContext, double width, double height, double minwidth, double minheight, WindowViewModelBase_VM ownerMV = null);
        /// <summary>
        /// Показать окно настроек таблицы
        /// </summary>
        /// <param name="iTable"></param>
        /// <param name="positionElement"></param>
        void OpenTableSettings(AbsM.ITableBaseM iTable, System.Windows.UIElement positionElement = null);
        #endregion Методы

        #region Команды
        /// <summary>
        /// Команда для открытия таблицы
        /// </summary>
        ICommand OpenTableCommand { get; }
        #endregion Команды
    }

    public enum ERepositoryType
    {
        Postgres = 1,
        Rastr = 2,
        VMP = 3,
        Cosmetic = 4
    }
}