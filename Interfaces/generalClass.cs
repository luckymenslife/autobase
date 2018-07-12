using System;
using System.Collections.Generic;
using System.Text;
using AxmvMapLib;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;

namespace Interfaces
{
    public class filtrTableInfo
    {
        /// <summary>
        /// id фильтра
        /// </summary>
        public int idFilter;
        /// <summary>
        /// id Таблицы
        /// </summary>
        public int idTable;
        /// <summary>
        /// id колонки
        /// </summary>
        public int idField;
        /// <summary>
        /// id оператора
        /// </summary>
        public int idOperator;
    }
    /// <summary>
    /// Информация о правах пользователя
    /// </summary>
    public class user_right
    {
        /// <summary>
        /// ID Таблицы
        /// </summary>
        public int id_table;
        /// <summary>
        /// Разрешение на чтение
        /// </summary>
        public bool read;
        /// <summary>
        /// Разрешение на редкатирование
        /// </summary>
        public bool write;
    }
    /// <summary>
    /// Информауия о пользователе
    /// </summary>
    public class userInfo
    {
        /// <summary>
        /// id пользователя
        /// </summary>
        public int id_user;
        /// <summary>
        /// является ли пользователь администратором (устарело)
        /// </summary>
        public bool admin;
        /// <summary>
        /// Строка, которая отображается в названии главного окна
        /// </summary>
        public string windowText;
        /// <summary>
        /// Наименование пользователя
        /// </summary>
        public string nameUser;
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string loginUser;
        /// <summary>
        /// Пароль для синхронизации
        /// </summary>
        public string pwdUser;
        /// <summary>
        /// Хост сервера БД
        /// </summary>
        public string ipString;
        /// <summary>
        /// Наименование БД
        /// </summary>
        public string dbString;
        /// <summary>
        /// Порт сервера БД
        /// </summary>
        public string portString;
        /// <summary>
        /// Информауия о пользователе
        /// </summary>
        public int type_user;
    }
    public enum TypeGeometry
    {
        /// <summary>
        /// Отсутствует тип геометрии (0)
        /// </summary>
        MISSING = 0,
        /// <summary> MULTIPOINT (1)
        /// </summary>
        MULTIPOINT = 1,
        /// <summary> MULTILINESTRING (2)
        /// </summary>
        MULTILINESTRING = 2,
        /// <summary> MULTIPOLYGON (3)
        /// </summary>
        MULTIPOLYGON = 3
    }
    public enum TypeTable
    {
        /// <summary> Слой карты (1)
        /// </summary>
        LayerMap = 1,
        /// <summary> Таблица ссылки (2)
        /// </summary>
        Reference = 2,
        /// <summary> Таблица интервал (3)
        /// </summary>
        Interval = 3,
        /// <summary> Таблица данных (4)
        /// </summary>
        Tables = 4
    }
    /// <summary>
    /// Свойства таблицы
    /// </summary>
    public class tablesInfo
    {
        /// <summary>
        /// id таблицы
        /// </summary>
        public int idTable;
        /// <summary>
        /// Наименование схемы в БД
        /// </summary>
        public string nameSheme;
        /// <summary>
        /// Наименование таблицы в БД
        /// </summary>
        public string nameDB;
        /// <summary>
        /// Наименование таблицы в системе
        /// </summary>
        public string nameMap { get; set; }
        /// <summary>
        /// Строка используемая для формирование подписи на карте, если таблицы явлется слоем карты
        /// </summary>
        public string lableFieldName;

        /// <summary>
        /// Тип таблицы
        /// </summary>
        public TypeTable TypeTable { get; set; }
        /// <summary>
        /// Имеется ли возможность прикрепления файлов
        /// </summary>
        public bool photo;

        /// <summary>
        /// Тип геометрии
        /// </summary>
        public TypeGeometry TypeGeom { get; set; }

        /// <summary>
        /// Является ли таблица только для чтения
        /// </summary>
        public bool read_only;
        /// <summary>
        /// Наименование колонки в которой хранится геометрия
        /// </summary>
        public string geomFieldName;
        /// <summary>
        /// Наименование колонки первичного ключа
        /// </summary>
        public string pkField;
        /// <summary>
        /// Хранит ли таблица ситли для других таблиц
        /// </summary>
        public bool map_style;
        /// <summary>
        /// Проекция слоя
        /// </summary>
        public int? srid;
        /// <summary>
        /// Подгружается ли слой через SourceExternal движка карты
        /// </summary>
        public bool sourceLayer;
        /// <summary>
        /// Наименование колонки в которой хранится ссылка на картинку, которая подгружается в качестве стиля
        /// </summary>
        public string imageColumn;
        /// <summary>
        /// Наименование колонки в которой хранится угол поворота картинки
        /// </summary>
        public string angleColumn;
        /// <summary>
        /// Использовать ли границы видимости у слоя карты
        /// </summary>
        public bool useBounds;
        /// <summary>
        /// Минимальный масштаб при котором объекты на карте видны
        /// </summary>
        public int minScale;
        /// <summary>
        /// Максимальный масштаб при котором объекты на карте видны
        /// </summary>
        public int maxScale;
        /// <summary>
        /// Строка, которая содержит sql запрос к представлению
        /// </summary>
        public string sql_view_string;
        public List<fieldInfo> ListField { get; set; }
        /// <summary>
        /// Название представления слоя
        /// </summary>
        public string view_name;

        public photoInfo PhotoInfo { get; set; }
    }
    public enum TypeField
    {
        /// <summary> 
        /// Для всех
        /// </summary>
        Default = 0,
        /// <summary> 
        /// Числовое поле (1)
        /// </summary>
        Integer = 1,
        /// <summary> 
        /// Текстовое поле (2)
        /// </summary>
        Text = 2,
        /// <summary> 
        /// Поле с датой (3)
        /// </summary>
        Date = 3,
        /// <summary> 
        /// Поле с датой и временем (4)
        /// </summary>
        DateTime = 4,
        /// <summary> 
        /// Поле геометрии (5)
        /// </summary>
        Geometry = 5,
        /// <summary> 
        /// Числовое поле с плавоющей точкой (6)
        /// </summary>
        Numeric = 6
    }
    /// <summary>
    /// Свойства колонки
    /// </summary>
    [DebuggerDisplay("fieldInfo {nameMap}({TypeField})")]
    public class fieldInfo
    {
        /// <summary>
        /// id колонки
        /// </summary>
        public int idField;
        /// <summary>
        /// ID тбалицы
        /// </summary>
        public int idTable;
        /// <summary>
        /// Название колонки в БД
        /// </summary>
        public string nameDB;
        /// <summary>
        /// Название в системе
        /// </summary>
        public string nameMap { get; set; }
        /// <summary>
        /// Строка отображаемая в всплывающей подсказке
        /// </summary>
        public string nameLable;

        /// <summary>
        /// Тип атрибута
        /// </summary>
        public TypeField TypeField { get; set; }

        /// <summary>
        /// связана ли как справочник или как таблица с данными
        /// </summary>
        public bool is_reference { get; set; }
        /// <summary>
        /// Является ли связанном по интервалу
        /// </summary>
        public bool is_interval { get; set; }
        /// <summary>
        /// id таблицы с которая является связанной
        /// </summary>
        public int? ref_table { get; set; }
        /// <summary>
        /// id колонки по которой связываются связанные таблицы (справочники, таблицы с данными) или это левая граница, если интервал
        /// </summary>
        public int? ref_field { get; set; }
        /// <summary>
        /// id колонки в которой хранится правая граница интервала
        /// </summary>
        public int? ref_field_end { get; set; }
        /// <summary>
        /// id колонки из которой получабт значения в связанной таблице
        /// </summary>
        public int? ref_field_name { get; set; }
        /// <summary>
        /// Строка используемая для формирование подписи на карте, если таблицы явлется слоем карты
        /// </summary>
        public string name_lable { get; set; }
        public bool read_only { get; set; }
        public bool visible { get; set; }
        public bool is_style { get; set; }
        public int Order { get; set; }

    }
    public class referInfo
    {
        public string nameField;
        public int idObj;
    }
    /// <summary>
    /// Тип таблицы
    /// </summary>
    public class tipTable
    {
        /// <summary>
        /// ID типа таблицы
        /// </summary>
        public int idTipTable;
        /// <summary>
        /// Наименование типа
        /// </summary>
        public string nameTip;
        /// <summary>
        /// Является ли слоем карты
        /// </summary>
        public bool mapLayer;
    }
    /// <summary>
    /// Тип геометрии
    /// </summary>
    public class tipGeom
    {
        /// <summary>
        /// ID типа геометрии
        /// </summary>
        public int idTipGeom;
        /// <summary>
        /// Наименование типа в системе
        /// </summary>
        public string nameGeom;
        /// <summary>
        /// Наименование типа в БД
        /// </summary>
        public string nameDb;
    }
    /// <summary>
    /// Тип данных
    /// </summary>
    public class tipData
    {
        /// <summary>
        /// ID типа
        /// </summary>
        public int idTipData;
        /// <summary>
        /// Название типа в системе
        /// </summary>
        public string nameTipData;
        /// <summary>
        /// Наизвание типа в БД
        /// </summary>
        public string nameTipDataDB;
    }
    /// <summary>
    /// Тип оператора
    /// </summary>
    public class tipOperator
    {
        /// <summary>
        /// id типа оператора
        /// </summary>
        public int idTipOperator;
        /// <summary>
        /// Символ оператора
        /// </summary>
        public string nameTipOperator;
        /// <summary>
        /// Передня часть шаблона для оператора LIKE
        /// </summary>
        public string namePered;
        /// <summary>
        /// Задняя часть шаблона для оператора LIKE
        /// </summary>
        public string namePosle;
    }
    /// <summary>
    /// Информация о хранилищи файлов
    /// </summary>
    public class photoInfo
    {
        /// <summary>
        /// ID таблицы с которой она связана
        /// </summary>
        public int idTable;
        /// <summary>
        /// Наименование поля первичного ключа у таблицы с которой она связана
        /// </summary>
        public string nameFieldID;
        /// <summary>
        /// Наименование таблицы(хранилища) в которой хранятся файлы
        /// </summary>
        public string namePhotoTable;
        /// <summary>
        /// Наименование поля по которому хранилище файлов связано с таблицей
        /// </summary>
        public string namePhotoField;
        /// <summary>
        /// Наименование поля, в которой хранятся бинарные данные
        /// </summary>
        public string namePhotoFile;
    }
    /// <summary>
    /// Класс данных пользователя SSC
    /// </summary>
    public class sscUserInfo
    {
        public Uri Server { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public sscUserInfo(Uri server, string login, string password)
        {
            this.Server = server;
            this.Login = login;
            this.Password = password;
        }
    }
    /// <summary>
    /// Интерфейс работы со свойствами системы
    /// </summary>
    public interface IMainApp
    {
        /// <summary>
        /// Объект работы с картой
        /// </summary>
        AxMapLIb mapLib { get; }
        /// <summary>
        /// Путь к рабочему каталогу
        /// </summary>
        string path_string { get; }
        /// <summary>
        /// Наименование файла конфигурации
        /// </summary>
        string setting_file { get; }
        /// <summary>
        /// Наименование схемы с мета-данными системы в БД
        /// </summary>
        string scheme { get; }
        /// <summary>
        /// Проекция карты
        /// </summary>
        string srid { get; }
        /// <summary>
        /// Объект с настройками приложения
        /// </summary>
        object AppSettingsObject { get; set; }
        /// <summary>
        /// Список прав пользователей на таблицу
        /// </summary>
        List<user_right> tables_right { get; }
        /// <summary>
        /// Список таблиц
        /// </summary>
        List<tablesInfo> tables_info { get; }
        /// <summary>
        /// Список колонок в таблице
        /// </summary>
        List<fieldInfo> field_info { get; }
        /// <summary>
        /// справочник типов
        /// </summary>
        List<tipTable> tip_table { get; }
        /// <summary>
        /// Справочник геометрии
        /// </summary>
        List<tipGeom> tip_geom { get; }
        /// <summary> Тип колонки
        /// </summary>
        List<tipData> tip_data { get; }
        /// <summary> Инфо где ханится файлы 
        /// </summary>
        List<photoInfo> photo_info { get; }
        /// <summary> Схема
        /// </summary>
        List<string> schems { get; }
        /// <summary> Информация о текущем пользователе
        /// </summary>
        userInfo user_info { get; }
        /// <summary>Хост сервера Базы данных
        /// </summary>
        string ipString { get; }
        /// <summary>Наименование базы
        /// </summary>
        string dbString { get; }
        /// <summary>Порт сервера базы данных
        /// </summary>
        string portString { get; }
        /// <summary>
        /// Данные пользователя SSC (сервер, имя пользователя, пароль)
        /// </summary>
        sscUserInfo sscUser { get; set; }
        Dictionary<string, string[]> userParams { get; }


        void reloadInfo();
        /// <summary> Получить права на таблицу
        /// </summary>
        /// <param name="idTable">таблица</param>
        /// <returns>Информация о правах на таблицу</returns>
        user_right getTableRight(int idTable);
        /// <summary>Свойства таблицы
        /// </summary>
        /// <param name="idTable">id таблицы</param>
        /// <returns>Свойства таблицы у которог id = idTable</returns>
        tablesInfo getTableInfo(int idTable);
        /// <summary>Свойства таблицы
        /// </summary>
        /// <param name="nameMap">Название слоя в системе</param>
        /// <returns>Свойства таблицы</returns>
        tablesInfo getTableInfoOfNameMap(string nameMap);
        /// <summary> Свойства таблицы
        /// </summary>
        /// <param name="nameDB">Название слоя в Базе</param>
        /// <returns>Свойства таблицы</returns>
        tablesInfo getTableInfoOfNameDB(string nameDB);
        /// <summary> Свойства таблицы
        /// </summary>
        /// <param name="nameDb">Название слоя в Базе</param>
        /// <param name="schema">Название схемы в БД</param>
        /// <returns>Свойства таблицы</returns>
        tablesInfo getTableInfoOfNameDB(string nameDb, string schema);
        /// <summary> Список таблиц по типу
        /// </summary>
        /// <param name="idType">Тип таблицы</param>
        /// <returns>Список таблиц по типу</returns>
        List<tablesInfo> getTableOfType(int idType);
        /// <summary>Свойства колонки
        /// </summary>
        /// <param name="idField">ID колонки</param>
        /// <returns>Свойства колонки</returns>
        fieldInfo getFieldInfo(int idField);
        /// <summary>Список колонок в таблице
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        /// <returns>Список колонок в таблице</returns>
        List<fieldInfo> getFieldInfoTable(int idTable);
        /// <summary>
        /// Тип таблицы
        /// </summary>
        /// <param name="idTip">id типа таблиц</param>
        /// <returns>Тип таблицы</returns>
        tipTable getTipTable(int idTip);
        /// <summary>
        /// Тип геометрии
        /// </summary>
        /// <param name="idTip">ID типа геометрии</param>
        /// <returns>Тип геометрии</returns>
        tipGeom getTipGeom(int idTip);
        /// <summary>
        /// Тип атрибута (колонки)
        /// </summary>
        /// <param name="idTip">id типа колонки</param>
        /// <returns>Тип атрибута (колонки)</returns>
        tipData getTipField(int idTip);
        /// <summary>
        /// Свойства связи таблицы и хранилища файлов
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        /// <returns>Свойства связи таблицы и хранилища файлов</returns>
        photoInfo getPhotoInfo(int idTable);
        /// <summary>
        /// Информация о разрешении на чтение таблицы
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        /// <returns>Информация о разрешении на чтение таблицы</returns>
        bool getReadTable(int idTable);
        /// <summary>
        /// Информация о разрешении на редактирование таблицы
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        /// <returns>Информация о разрешении на редактирование таблицы</returns>
        bool getWriteTable(int idTable);
        /// <summary>
        /// Класс хранящий список соответствия наименование слоя в системе и наименование на карте
        /// </summary>
        IRelation relation { get; }
        /// <summary>
        /// Класс работающий с базой данных
        /// </summary>
        ISQLCommand SqlWork(bool enableException = false);
        /// <summary>
        /// Включает/выключает визуализацию слоя
        /// </summary>
        /// <param name="idTable">id таблицы</param>
        /// <param name="visble">Включение/выключение</param>
        void SetVisableLayer(int idTable, bool visble);

        string ConnectionString { get; }
    }
    /// <summary>
    /// Класс хранящий список соответствия наименование слоя в системе и наименование на карте
    /// </summary>
    public interface IRelation
    {
        /// <summary>
        /// Наименование слоя в системе
        /// </summary>
        /// <param name="NameInBd">Наименование слоя на карте</param>
        /// <returns>Строка содержащая наименование слоя в системе</returns>
        string GetNameForUser(string NameInBd);
        /// <summary>
        /// Наименование слоя на карте
        /// </summary>
        int GetIdTable(string NameInBd);
        string GetNameInBd(int idT);
        string GetNameForUser(int idT);
    }
}