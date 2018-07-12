using System;
using System.Collections.Generic;
using System.Windows;
using AxmvMapLib;
using mvMapLib;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Interfaces
{
    /// <summary> Интерфейс идентификации плагина
    /// </summary>
    public interface IMainPlugin
    {
        /// <summary> Название плагина
        /// </summary>
        string Name { get; }
        /// <summary> Guid плагина
        /// </summary>
        string GUID { get; }

        /// <summary> Основная функция инициализации плагина
        /// </summary>
        /// <param name="XSettings">Параметры в settings файле</param>
        /// <param name="app">Интерфейс работы с базой</param>
        /// <param name="work">Интерфейс работы с окнами</param>
        void StartPlugin(XElement XSettings, IMainApp app, IWorkClass work);

        /// <summary> Получения окна настройки плагина
        /// </summary>
        Forms.IControlSettings SettingsForm { get; }
    }

    /// <summary> Интерфейс работы с окнами
    /// </summary>
    public interface IWorkClass
    {
        /// <summary> Интерефейс методов открытия окон
        /// </summary>
        IOpenForms OpenForm { get; }

        /// <summary> Интерефейс работы с отчетами
        /// </summary>
        FastReport.IReport_M FastReport { get; }

        /// <summary> Подменяет форму списка объектов для определенной таблицы
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        /// <param name="func">Функция в которой передается id таблицы для получения нового UserControl</param>
        /// <returns>Подтверждение добавление подмены</returns>
        bool AddSpoofingTableOfObjects(int idTable, Func<int, bool, Form, int?, int?, UserControls.IUserControlMain> func);

        /// <summary> Отписывает UserControl'a от определенной таблицы окна списка объектов 
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        void RemoveSpoofingTableOfObjects(int idTable);

        /// <summary> Удаляет подмену форму списка объектов для определенной таблицы
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        /// <param name="func">Функция в которой передается id таблицы и id объекта для получения нового UserControl</param>
        /// <returns>Подтверждение добавление подмены</returns>
        bool AddSpoofingAttributesOfObject(int idTable, Func<int, int?, referInfo, UserControls.IUserControlMain> func);

        /// <summary> Удаляет подмену форму атрибутики объекта для определенной таблицы
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        void RemoveSpoofingAttributesOfObject(int idTable);

        /// <summary> Подписывает обработчик добавления меню в окна списка объектов
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        /// <param name="menuItem">Метод генерирующий ToolStripMenuItem</param>
        void AddMenuInTable(int idTable, Func<ToolStripMenuItem> menuItem);

        /// <summary> Отписывает обработчик добавления меню в окна списка объектов
        /// </summary>
        void RemoveMenuInTable(Func<ToolStripMenuItem> menuItem);

        /// <summary> Подписывает обработчик добавления меню в окна списка объектов или в аттрибутику объекта
        /// </summary>
        /// <param name="idTable">id_table</param>
        /// <param name="menuItem">Метод генерирующий ToolStripMenuItem</param>
        /// <param name="inTable"> </param>
        /// <param name="inAttribute"> </param>
        void AddMenuForObject(int idTable, Func<ToolStripMenuItem> menuItem, bool inTable = true, bool inAttribute = true);

        /// <summary> Отписывает обработчик добавления меню в окна списка объектов или в аттрибутику объекта
        /// </summary>
        void RemoveMenuForObject(Func<ToolStripMenuItem> menuItem);
        
        /// <summary> Методы подписываемые в главном окне
        /// </summary>
        Forms.ImainFrm MainForm { get; }

        /// <summary> Создание Excel документа
        /// </summary>
        /// <param name="data">Загаловки c данные</param>
        /// <param name="Types">Список типов полей как в БД </param>
        void ExportToExcel(List<object[]> data, List<int> Types);

        void SaveSettings();
        void SaveSettings(string guid, XElement settings);

        /// <summary>
        /// Установить UserControl для просмотра списка процессов и заявок
        /// </summary>
        void SetProcessesControl(Interfaces.UserControls.IUserControlMain userControl);
    }

    /// <summary> Интерефейс методов открытия окон
    /// </summary>
    public interface IOpenForms
    {
        /// <summary>Открытие окна списка объектов через Show()
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        void OpenTableObject(Interfaces.tablesInfo table, Form owner);
        void OpenTableObject(Interfaces.tablesInfo table, Form owner, Window wpfowner);
        /// <summary>Открытие окна списка объектов в текущем потоке с выводом результата
        /// </summary>
        /// <param name="Table">Ссылка на таблицу</param>
        /// <param name="idSelected">id объекта для выделения</param>
        int? OpenTableObject(Interfaces.tablesInfo Table, int? idSelected, bool isSelected);

        /// <summary>Открытие окно атрибутики объекта через ShowDialog()
        /// (Устаревший метод, альтернатива: ShowAttributeObject или ShowDialogAttributeObject)
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="idObject">id объекта</param>
        /// <param name="isNew">Признак нового объекта</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        void OpenAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Interfaces.referInfo refValue = null, Form owner = null);
        /// <summary> Открытие окно атрибутики объекта через Show()
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="idObject">id объекта</param>
        /// <param name="isNew">Признак нового объекта</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        /// <param name="ActionDResult">Метод вызываемый при закрытии окна</param>
        void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner, Action<DialogResult> ActionDResult = null);

        void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Window wpfWindow);
        /// <summary> Открытие окно атрибутики объекта через Show()
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="idObject">id объекта</param>
        /// <param name="isNew">Признак нового объекта</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        /// <param name="ActionDResult">Метод вызываемый при закрытии окна</param>
        /// <param name="wkt">Строка с геометрией в формате WKT</param>
        /// <param name="SearchField">Наименование колонки по которой будет производиться фильтрация</param>
        /// <param name="SearchId">Значение по которому будет производиться фильтрация</param>
        void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner, String wkt, Action<DialogResult> ActionDResult = null,
             String SearchField = "", int SearchId = -1);
        /// <summary>Открытие окно атрибутики объекта в текущем потоке с выводом результата
        /// </summary>
        /// <param name="table">Ссылка на таблицу</param>
        /// <param name="idObject">Id объекта</param>
        /// <param name="isNew">Признак нового объекта</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        DialogResult ShowDialogAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner);

        /// <summary> Открытие окна истории объекта
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        /// <param name="idObj">Id объекта</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        void HistoryFrm(int idTable, int idObj, Form owner);
        /// <summary> Открытие окна истории таблицы
        /// </summary>
        /// <param name="idTable">Id таблицы</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        void HistoryFrm(int idTable, Form owner);
        /// <summary> Открытие окна истории пользователя
        /// </summary>
        /// <param name="userName">Логин пользователя</param>
        /// <param name="owner">Ссылка на родительское окно</param>
        void HistoryFrm(string userName, Form owner);

        /// <summary> Привязать данное окно к главной форме
        /// </summary>
        /// <param name="frm">Данная форма</param>
        void SetFormOwner(Form frm);

        /// <summary> Создание идентификатора процесса для окна загрузки
        /// </summary>
        /// <param name="prefix">Префикс для определения типа процесса в логе</param>
        /// <returns>Ключ для закрытия процесса</returns>
        string ProcOpen(string prefix = "");
        /// <summary> Закрытие идентификатора процесса для окна загрузки
        /// </summary>
        /// <param name="key">Ключ для закрытия процесса</param>
        void ProcClose(string key);

        void SetText(string txt);
    }

    public interface IGetUserControl<T>
    {
        /// <summary> Название схемы
        /// </summary>
        string NameSchema { get; }
        /// <summary> Название таблицы
        /// </summary>
        string NameTable { get; }
        /// <summary> Guid плагина
        /// </summary>
        string GUID { get; }
        /// <summary> Интерфейс выводимого UserControl'a
        /// </summary>
        T Control { get; }
    }
}