using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.nsWaybill
{
    /// <summary>
    /// Автомобиль
    /// </summary>
    public class wb_Car
    {
        #region Поля
        private int _gid;
        private string _garno;
        private string _gosno;
        private string _type_name;
        private string _model_name;
        private string _mark_name;
        private int _model_id;
        #endregion

        #region Конструктор
        public wb_Car(int gid, string gosno, string garno, string type_name, string model_name, string mark_name, int model_id)
        {
            this._gid = gid;
            this._garno = garno;
            this._gosno = gosno;
            this._type_name = type_name;
            this._model_name = model_name;
            this._mark_name = mark_name;
            this._model_id = model_id;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Garno { get { return _garno; } }
        public string Gosno { get { return _gosno; } }
        public string Type_name { get { return _type_name; } }
        public string Model_name { get { return _model_name; } }
        public string Mark_name { get { return _mark_name; } }
        public string Mark_and_model_name { get { return string.Format("{0} {1}", _mark_name, _model_name); } }
        public int Model_id { get { return _model_id; } }
        #endregion
    }

    /// <summary>
    /// Прицеп
    /// </summary>
    public class wb_Trailer
    {
        #region Поля
        private int _gid;
        private string _garno;
        private string _gosno;
        private string _model_name;
        #endregion

        #region Конструктор
        public wb_Trailer(int gid, string garno, string gosno, string model_name)
        {
            this._gid = gid;
            this._garno = garno;
            this._gosno = gosno;
            this._model_name = model_name;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Garno { get { return _garno; } }
        public string Gosno { get { return _gosno; } }
        public string Model_name { get { return _model_name; } }
        #endregion
    }

    /// <summary>
    /// Водитель
    /// </summary>
    public class wb_Driver
    {
        #region Поля
        private int _gid;
        private string _first_name;
        private string _last_name;
        private string _middle_name;
        private string _tabno;
        #endregion

        #region Конструктор
        public wb_Driver(int gid, string first_name, string last_name, string middle_name, string tabno)
        {
            this._gid = gid;
            this._first_name = first_name ?? "-";
            this._last_name = last_name ?? "-";
            this._middle_name = middle_name ?? "-";
            this._tabno = tabno ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string First_name { get { return _first_name; } }
        public string Last_name { get { return _last_name; } }
        public string Middle_name { get { return _middle_name; } }
        public string Tabno { get { return _tabno; } }
        public string Fullname { get { return String.Concat(new string[] { _last_name, " ", _first_name, " ", _middle_name }); } }
        #endregion
    }

    /// <summary>
    /// Мастер (Ремонт? Обслуживание?)
    /// </summary>
    public class wb_Repairer
    {
        #region Поля
        private int _gid;
        private string _first_name;
        private string _last_name;
        private string _middle_name;
        #endregion

        #region Конструктор
        public wb_Repairer(int gid, string first_name, string last_name, string middle_name)
        {
            this._gid = gid;
            this._first_name = first_name ?? "-";
            this._last_name = last_name ?? "-";
            this._middle_name = middle_name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string First_name { get { return _first_name; } }
        public string Last_name { get { return _last_name; } }
        public string Middle_name { get { return _middle_name; } }
        public string Fullname { get { return String.Concat(new string[] { _last_name, " ", _first_name, " ", _middle_name }); } }
        #endregion
    }

    /// <summary>
    /// Маршрут движения
    /// </summary>
    public class wb_Route
    {
        #region Поля
        private int _gid;
        private string _name;
        #endregion

        #region Конструктор
        public wb_Route(int gid, string name)
        {
            this._gid = gid;
            this._name = name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        #endregion
    }

    /// <summary>
    /// Организация (ГБУ)
    /// </summary>
    public class wb_Organisation
    {
        #region Поля
        private int _gid;
        private string _name;
        private string _wb_sequence_name;
        private bool _wb_med_check;
        #endregion

        #region Конструктор
        public wb_Organisation(int gid, string name, string wb_sequence, bool canMedCheck = false)
        {
            if (wb_sequence == null || wb_sequence.Trim().Equals(string.Empty))
            {
                throw new ArgumentException(Messages_cls.InvalidValue);
            }
            this._gid = gid;
            this._name = name;
            this._wb_sequence_name = wb_sequence;
            this._wb_med_check = canMedCheck;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        public string Sequence_name { get { return _wb_sequence_name; } }
        public bool Med_check { get { return _wb_med_check; } }
        #endregion
    }

    /// <summary>
    /// Вид работ
    /// </summary>
    public class wb_Work_type
    {
        #region Поля
        private int _gid;
        private string _name;
        #endregion

        #region Конструктор
        public wb_Work_type(int gid, string name)
        {
            this._gid = gid;
            this._name = name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        #endregion
    }

    /// <summary>
    /// Вид груза
    /// </summary>
    public class wb_Cargo_type
    {
        #region Поля
        private int _gid;
        private string _name;
        #endregion

        #region Конструктор
        public wb_Cargo_type(int gid, string name)
        {
            this._gid = gid;
            this._name = name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        #endregion
    }

    /// <summary>
    /// Режим работы
    /// </summary>
    public class wb_Work_mode
    {
        #region Поля
        private int _gid;
        private string _name;
        private decimal _hours;
        #endregion

        #region Конструктор
        public wb_Work_mode(int gid, string name, decimal hours)
        {
            this._gid = gid;
            this._name = name ?? "-";
            this._hours = hours;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        public decimal Hours { get { return _hours; } }
        #endregion
    }

    /// <summary>
    /// Виды топлива
    /// </summary>
    public class wb_Fuel_mark
    {
        #region Поля
        private int _gid;
        private string _name;
        private bool _isNone;
        #endregion

        #region Конструктор
        public wb_Fuel_mark(int gid, string name, bool isNoFuel)
        {
            this._gid = gid;
            this._name = name ?? "-";
            this._isNone = isNoFuel;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        public bool IsNone { get { return _isNone; } }
        #endregion
    }

    /// <summary>
    /// Особые отметки
    /// </summary>
    public class wb_Special_note
    {
        #region Поля
        private int _gid;
        private string _name;
        #endregion

        #region Конструктор
        public wb_Special_note(int gid, string name)
        {
            this._gid = gid;
            this._name = name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        #endregion
    }

    /// <summary>
    /// Зоны (работы?)
    /// </summary>
    public class wb_Road_type
    {
        #region Поля
        private int _gid;
        private string _name;
        #endregion

        #region Конструктор
        public wb_Road_type(int gid, string name)
        {
            this._gid = gid;
            this._name = name ?? "-";
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        #endregion
    }

    public class wb_Fuel_card
    {
        #region Поля
        private int _gid;
        private string _name;
        private int _fuel_mark_id;
        #endregion

        #region Конструктор
        public wb_Fuel_card(int gid, string name, int fuel_mark_id)
        {
            this._gid = gid;
            this._name = name ?? "-";
            this._fuel_mark_id = fuel_mark_id;
        }
        #endregion

        #region Свойства
        public int Gid { get { return _gid; } }
        public string Name { get { return _name; } }
        public int Fuel_mark_id { get { return _fuel_mark_id; } }

        #endregion
    }
}
