using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.nsWaybill
{
    public class Waybill_M : ViewModelBase
    {
        #region Fields

        private int? _gid;
        private DateTime _doc_date; // .Net Default => DateTime.MinValue
        private string _doc_no;
        private int? _car_id;
        private int? _trailer_id;
        private int? _automaster_id;
        private DateTime _date_out_plan; // .Net Default => DateTime.MinValue
        private DateTime _date_out_fact; // .Net Default => DateTime.MinValue
        private DateTime _date_in_plan; // .Net Default => DateTime.MinValue
        private DateTime _date_in_fact; // .Net Default => DateTime.MinValue
        private int? _driver_id;
        private int? _work_regime_id;
        private int? _motorcade;
        private int? _brigade;
        private int? _route_id;
        private int? _work_type_id;
        private int? _cargo_type_id;
        private int? _ttd_count;
        private int? _trip_count;
        private int? _road_type_id;
        private decimal? _km_begin;
        private decimal? _km_end;
        private decimal? _km_run;
        private decimal? _km_run_glonass;
        private decimal? _mh_begin;
        private decimal? _mh_end;
        private decimal? _mh_run;
        private decimal? _mh_run_glonass;
        private decimal? _mh_ob_begin;
        private decimal? _mh_ob_end;
        private decimal? _mh_ob_run;
        private decimal? _mh_ob_run_glonass;
        private int? _fuel_mark_id;
        private decimal? _fuel_plan;
        private decimal? _fuel_fact;
        private decimal? _fuel_fact_glonass;
        private decimal? _fuel_begin;
        private decimal? _fuel_end;
        private decimal? _fuel_begin_glonass;
        private decimal? _fuel_end_glonass;
        private int? _fuel_mark2_id;
        private decimal? _fuel_plan2;
        private decimal? _fuel_fact2;
        private decimal? _fuel_fact2_glonass;
        private decimal? _fuel_begin2;
        private decimal? _fuel_end2;
        private decimal? _fuel_begin2_glonass;
        private decimal? _fuel_end2_glonass;
        private int? _fuel_card_id;
        private int? _fuel_card2_id;
        private decimal? _calc_fuel_norm;
        private decimal? _calc_fuel_fact;
        private decimal? _calc_fuel_delta;
        private int? _calc_fuel_drain;
        private decimal? _calc_km_run_delta;
        private decimal? _calc_mh_run_delta;
        private int? _pay_work_h;
        private int? _pay_lunch_h;
        private int? _pay_duty_h;
        private int? _pay_repair_h;
        private int? _pay_day_h;
        private int? _pay_night_h;
        private decimal? _pay_rate_rh;
        private decimal? _pay_total_r;
        private string _support_persons;
        private int? _special_note_id;
        private string _notes;
        private DateTime _createdate; // .Net Default => DateTime.MinValue
        private int? _secondsave;
        private int? _user_id;
        private int? _org_id;

        #endregion Fields

        #region Contructor

        public Waybill_M(int? id_object)
        {
            Gid = id_object;
        }
        public Waybill_M()
        {
        }

        #endregion

        #region ReadOnly Properties

        /// <summary>
        /// ReadOnly. Gid в таблице
        /// </summary>
        public int? Gid
        {
            get { return _gid; }
            set
            {
                if (_gid == null) { _gid = value; OnPropertyChanged("Gid"); }
            }
        }
        /// <summary>
        /// ReadOnly. Дата создания путевого листа
        /// </summary>
        public DateTime Doc_date { get { return _doc_date; } set { if (_doc_date == DateTime.MinValue) { _doc_date = value; OnPropertyChanged("Doc_date"); } } }
        /// <summary>
        /// ReadOnly. Прядковый номер путевого листа в организации
        /// </summary>
        public string Doc_no { get { return _doc_no; } set { _doc_no = value; OnPropertyChanged("Doc_no"); } }
        /// <summary>
        /// ReadOnly. Дата создания путевого листа
        /// </summary>
        public DateTime Createdate { get { return _createdate; } set { if (_createdate == DateTime.MinValue) { _createdate = value; OnPropertyChanged("Createdate"); } } }
        /// <summary>
        /// Идентификатор из MapEditor автора путевого листа
        /// </summary>
        public int? User_id { get { return _user_id; } set { if (_user_id == null) { _user_id = value; OnPropertyChanged("User_id"); } } }

        #endregion //ReadOnly Properties

        #region ReadOnly Glonass Properties

        /// <summary>
        /// ReadOnly. Пробег авто по данным ГЛОНАСС
        /// </summary>
        public decimal? Km_run_glonass { get { return _km_run_glonass; } set { if (_km_run_glonass == null) { _km_run_glonass = value; OnPropertyChanged("Km_run_glonass"); } } }
        /// <summary>
        /// ReadOnly. Моточасы по данным ГЛОНАСС
        /// </summary>
        public decimal? Mh_run_glonass { get { return _mh_run_glonass; } set { if (_mh_run_glonass == null) { _mh_run_glonass = value; OnPropertyChanged("Mh_run_glonass"); } } }
        /// <summary>
        /// ReadOnly. Моточасы оборудования по даным ГЛОНАСС
        /// </summary>
        public decimal? Mh_ob_run_glonass { get { return _mh_ob_run_glonass; } set { if (_mh_ob_run_glonass == null) { _mh_ob_run_glonass = value; OnPropertyChanged("Mh_ob_run_glonass"); } } }
        /// <summary>
        /// ReadOnly. Расход поплива (осн) по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_fact_glonass { get { return _fuel_fact_glonass; } set { if (_fuel_fact_glonass == null) { _fuel_fact_glonass = value; OnPropertyChanged("Fuel_fact_glonass"); } } }
        /// <summary>
        /// ReadOnly. Количество топлива (осн) на момент выезда по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_begin_glonass { get { return _fuel_begin_glonass; } set { if (_fuel_begin_glonass == null) { _fuel_begin_glonass = value; OnPropertyChanged("Fuel_begin_glonass"); } } }
        /// <summary>
        /// ReadOnly. Количество топлива (осн) на момент возвращения по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_end_glonass { get { return _fuel_end_glonass; } set { if (_fuel_end_glonass == null) { _fuel_end_glonass = value; OnPropertyChanged("Fuel_end_glonass"); } } }
        /// <summary>
        /// ReadOnly. Расход топлива (втор) по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_fact2_glonass { get { return _fuel_fact2_glonass; } set { if (_fuel_fact2_glonass == null) { _fuel_fact2_glonass = value; OnPropertyChanged("Fuel_fact2_glonass"); } } }
        /// <summary>
        /// ReadOnly. Количество топлива (втор) на момент выезда по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_begin2_glonass { get { return _fuel_begin2_glonass; } set { if (_fuel_begin2_glonass == null) { _fuel_begin2_glonass = value; OnPropertyChanged("Fuel_begin2_glonass"); } } }
        /// <summary>
        /// ReadOnly. Количество топлива (втор) на момент возвращения по даным ГЛОНАСС
        /// </summary>
        public decimal? Fuel_end2_glonass { get { return _fuel_end2_glonass; } set { if (_fuel_end2_glonass == null) { _fuel_end2_glonass = value; OnPropertyChanged("Fuel_end2_glonass"); } } }

        #endregion //ReadOnly Glonass Properties

        #region Properties

        /// <summary>
        /// Автомобиль
        /// </summary>
        public int? Car_id { get { return _car_id; } set { _car_id = value; OnPropertyChanged("Car_id"); } }
        /// <summary>
        /// Прицеп
        /// </summary>
        public int? Trailer_id { get { return _trailer_id; } set { _trailer_id = value; OnPropertyChanged("Trailer_id"); } }
        /// <summary>
        /// Мастер (ремонт, обслуживание)
        /// </summary>
        public int? Automaster_id { get { return _automaster_id; } set { _automaster_id = value; OnPropertyChanged("Automaster_id"); } }
        /// <summary>
        /// Планируемое время выезда
        /// </summary>
        public DateTime Date_out_plan { get { return _date_out_plan; } set { _date_out_plan = value; OnPropertyChanged("Date_out_plan"); } }
        /// <summary>
        /// Фактическое время выезда
        /// </summary>
        public DateTime Date_out_fact { get { return _date_out_fact; } set { _date_out_fact = value; OnPropertyChanged("Date_out_fact"); } }
        /// <summary>
        /// Планируемое время возвращения
        /// </summary>
        public DateTime Date_in_plan { get { return _date_in_plan; } set { _date_in_plan = value; OnPropertyChanged("Date_in_plan"); } }
        /// <summary>
        /// Фактическое время возвращения
        /// </summary>
        public DateTime Date_in_fact { get { return _date_in_fact; } set { _date_in_fact = value; OnPropertyChanged("Date_in_fact"); } }
        /// <summary>
        /// Водитель
        /// </summary>
        public int? Driver_id { get { return _driver_id; } set { _driver_id = value; OnPropertyChanged("Driver_id"); } }
        /// <summary>
        /// Режим работы
        /// </summary>
        public int? Work_regime_id { get { return _work_regime_id; } set { _work_regime_id = value; OnPropertyChanged("Work_regime_id"); } }
        /// <summary>
        /// Колонна
        /// </summary>
        public int? Motorcade { get { return _motorcade; } set { _motorcade = value; OnPropertyChanged("Motorcade"); } }
        /// <summary>
        /// Бригада
        /// </summary>
        public int? Brigade { get { return _brigade; } set { _brigade = value; OnPropertyChanged("Brigade"); } }
        /// <summary>
        /// Маршрут
        /// </summary>
        public int? Route_id { get { return _route_id; } set { _route_id = value; OnPropertyChanged("Route_id"); } }
        /// <summary>
        /// Тип выполняемых работ
        /// </summary>
        public int? Work_type_id { get { return _work_type_id; } set { _work_type_id = value; OnPropertyChanged("Work_type_id"); } }
        /// <summary>
        /// Тип груза
        /// </summary>
        public int? Cargo_type_id { get { return _cargo_type_id; } set { _cargo_type_id = value; OnPropertyChanged("Cargo_type_id"); } }
        /// <summary>
        /// Количество ТТД
        /// </summary>
        public int? Ttd_count { get { return _ttd_count; } set { _ttd_count = value; OnPropertyChanged("Ttd_count"); } }
        /// <summary>
        /// Количество рейсов
        /// </summary>
        public int? Trip_count { get { return _trip_count; } set { _trip_count = value; OnPropertyChanged("Trip_count"); } }
        /// <summary>
        /// Зона выполнения работ
        /// </summary>
        public int? Road_type_id { get { return _road_type_id; } set { _road_type_id = value; OnPropertyChanged("Road_type_id"); } }
        /// <summary>
        /// Пробег перед выездом
        /// </summary>
        public decimal? Km_begin { get { return _km_begin; } set { _km_begin = value; OnPropertyChanged("Km_begin"); } }
        /// <summary>
        /// Пробег по возвращению
        /// </summary>
        public decimal? Km_end { get { return _km_end; } set { _km_end = value; OnPropertyChanged("Km_end"); } }
        /// <summary>
        /// Пробег по данному путевому листу (разница начала и конца)
        /// </summary>
        public decimal? Km_run { get { return _km_run; } set { _km_run = value; OnPropertyChanged("Km_run"); } }
        /// <summary>
        /// Моточасы перед выездом
        /// </summary>
        public decimal? Mh_begin { get { return _mh_begin; } set { _mh_begin = value; OnPropertyChanged("Mh_begin"); } }
        /// <summary>
        /// Моточасы по возвращению
        /// </summary>
        public decimal? Mh_end { get { return _mh_end; } set { _mh_end = value; OnPropertyChanged("Mh_end"); } }
        /// <summary>
        /// Моточасы разница между возвращением и выездом
        /// </summary>
        public decimal? Mh_run { get { return _mh_run; } set { _mh_run = value; OnPropertyChanged("Mh_run"); } }
        /// <summary>
        /// Моточасы оборудования перед выездом
        /// </summary>
        public decimal? Mh_ob_begin { get { return _mh_ob_begin; } set { _mh_ob_begin = value; OnPropertyChanged("Mh_ob_begin"); } }
        /// <summary>
        /// Моточасы оборудования по возвращению
        /// </summary>
        public decimal? Mh_ob_end { get { return _mh_ob_end; } set { _mh_ob_end = value; OnPropertyChanged("Mh_ob_end"); } }
        /// <summary>
        /// Моточасы оборудования разница между возвращение и выездом
        /// </summary>
        public decimal? Mh_ob_run { get { return _mh_ob_run; } set { _mh_ob_run = value; OnPropertyChanged("Mh_ob_run"); } }
        /// <summary>
        /// Вид основного топлива
        /// </summary>
        public int? Fuel_mark_id { get { return _fuel_mark_id; } set { _fuel_mark_id = value; OnPropertyChanged("Fuel_mark_id"); } }
        /// <summary>
        /// Сколько выдать основного топлива
        /// </summary>
        public decimal? Fuel_plan { get { return _fuel_plan; } set { _fuel_plan = value; OnPropertyChanged("Fuel_plan"); } }
        /// <summary>
        /// Сколько фактически выдано основого топлива
        /// </summary>
        public decimal? Fuel_fact { get { return _fuel_fact; } set { _fuel_fact = value; OnPropertyChanged("Fuel_fact"); } }
        /// <summary>
        /// Сколько топлива было перед выездом
        /// </summary>
        public decimal? Fuel_begin { get { return _fuel_begin; } set { _fuel_begin = value; OnPropertyChanged("Fuel_begin"); } }
        /// <summary>
        /// Сколько топлива было по возвращению
        /// </summary>
        public decimal? Fuel_end { get { return _fuel_end; } set { _fuel_end = value; OnPropertyChanged("Fuel_end"); } }
        /// <summary>
        /// Вид второго топлива
        /// </summary>
        public int? Fuel_mark2_id { get { return _fuel_mark2_id; } set { _fuel_mark2_id = value; OnPropertyChanged("Fuel_mark2_id"); } }
        /// <summary>
        /// Сколько выдать второго топлива
        /// </summary>
        public decimal? Fuel_plan2 { get { return _fuel_plan2; } set { _fuel_plan2 = value; OnPropertyChanged("Fuel_plan2"); } }
        /// <summary>
        /// Сколько фактически выдано второго топлива
        /// </summary>
        public decimal? Fuel_fact2 { get { return _fuel_fact2; } set { _fuel_fact2 = value; OnPropertyChanged("Fuel_fact2"); } }
        /// <summary>
        /// Сколько второго топлива было перед выездом
        /// </summary>
        public decimal? Fuel_begin2 { get { return _fuel_begin2; } set { _fuel_begin2 = value; OnPropertyChanged("Fuel_begin2"); } }
        /// <summary>
        /// Сколько второго топлива было по возвращению
        /// </summary>
        public decimal? Fuel_end2 { get { return _fuel_end2; } set { _fuel_end2 = value; OnPropertyChanged("Fuel_end2"); } }
        /// <summary>
        /// Топливная карта основного топлива
        /// </summary>
        public int? Fuel_card_id { get { return _fuel_card_id; } set { _fuel_card_id = value; OnPropertyChanged("Fuel_card_id"); } }
        /// <summary>
        /// Топливная карта второго топлива
        /// </summary>
        public int? Fuel_card2_id { get { return _fuel_card2_id; } set { _fuel_card2_id = value; OnPropertyChanged("Fuel_card2_id"); } }
        /// <summary>
        /// Сводный расчет. Расход топлива по проделанным операциям
        /// </summary>
        public decimal? Calc_fuel_norm { get { return _calc_fuel_norm; } set { _calc_fuel_norm = value; OnPropertyChanged("Calc_fuel_norm"); } }
        /// <summary>
        /// Сводный расчет. Расход топлива по выданным данным
        /// </summary>
        public decimal? Calc_fuel_fact { get { return _calc_fuel_fact; } set { _calc_fuel_fact = value; OnPropertyChanged("Calc_fuel_fact"); } }
        /// <summary>
        /// Разница расхода топлива по операциям и выданного
        /// </summary>
        public decimal? Calc_fuel_delta { get { return _calc_fuel_delta; } set { _calc_fuel_delta = value; OnPropertyChanged("Calc_fuel_delta"); } }
        /// <summary>
        /// Сводный расчет. Слив топлива
        /// </summary>
        public int? Calc_fuel_drain { get { return _calc_fuel_drain; } set { _calc_fuel_drain = value; OnPropertyChanged("Calc_fuel_drain"); } }
        /// <summary>
        /// Сводный расчет. Разница в км
        /// </summary>
        public decimal? Calc_km_run_delta { get { return _calc_km_run_delta; } set { _calc_km_run_delta = value; OnPropertyChanged("Calc_km_run_delta"); } }
        /// <summary>
        /// Сводный рачет. Разница в моточасах
        /// </summary>
        public decimal? Calc_mh_run_delta { get { return _calc_mh_run_delta; } set { _calc_mh_run_delta = value; OnPropertyChanged("Calc_mh_run_delta"); } }
        /// <summary>
        /// Работа, ч
        /// </summary>
        public int? Pay_work_h { get { return _pay_work_h; } set { _pay_work_h = value; OnPropertyChanged("Pay_work_h"); } }
        /// <summary>
        /// Обед, ч
        /// </summary>
        public int? Pay_lunch_h { get { return _pay_lunch_h; } set { _pay_lunch_h = value; OnPropertyChanged("Pay_lunch_h"); } }
        /// <summary>
        /// Дежурство,ч
        /// </summary>
        public int? Pay_duty_h { get { return _pay_duty_h; } set { _pay_duty_h = value; OnPropertyChanged("Pay_duty_h"); } }
        /// <summary>
        /// Ремонт, ч
        /// </summary>
        public int? Pay_repair_h { get { return _pay_repair_h; } set { _pay_repair_h = value; OnPropertyChanged("Pay_repair_h"); } }
        /// <summary>
        /// День, ч
        /// </summary>
        public int? Pay_day_h { get { return _pay_day_h; } set { _pay_day_h = value; OnPropertyChanged("Pay_day_h"); } }
        /// <summary>
        /// Ночь, ч
        /// </summary>
        public int? Pay_night_h { get { return _pay_night_h; } set { _pay_night_h = value; OnPropertyChanged("Pay_night_h"); } }
        /// <summary>
        /// Ставка, руч/Ч
        /// </summary>
        public decimal? Pay_rate_rh { get { return _pay_rate_rh; } set { _pay_rate_rh = value; OnPropertyChanged("Pay_rate_rh"); } }
        /// <summary>
        /// Итого, руб
        /// </summary>
        public decimal? Pay_total_r { get { return _pay_total_r; } set { _pay_total_r = value; OnPropertyChanged("Pay_total_r"); } }
        /// <summary>
        /// Сопровождающие лица
        /// </summary>
        public string Support_persons { get { return _support_persons; } set { _support_persons = value; OnPropertyChanged("Support_persons"); } }
        /// <summary>
        /// Особые отметки
        /// </summary>
        public int? Special_note_id { get { return _special_note_id; } set { _special_note_id = value; OnPropertyChanged("Special_note_id"); } }
        /// <summary>
        /// Комментарий к путевому листу
        /// </summary>
        public string Notes { get { return _notes; } set { _notes = value; OnPropertyChanged("Notes"); } }
        /// <summary>
        /// Этап правки путевого листа. null - создается, 1 - открыт, 2 - закрыт
        /// </summary>
        public int? Secondsave { get { return _secondsave; } set { _secondsave = value; OnPropertyChanged("Secondsave"); } }
        /// <summary>
        /// Идентификатор организации, кому принадлежит путевой лист
        /// </summary>
        public int? Org_id { get { return _org_id; } set { _org_id = value; OnPropertyChanged("Org_id"); } }

        #endregion Properties
    }
}
