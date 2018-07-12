using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces;
using GBU_Waybill_plugin;
using System.Collections.ObjectModel;

namespace GBU_Waybill_plugin.nsWaybill
{
    /// <summary>
    /// Осуществляет взаимодействие с БД для путевых листов
    /// </summary>
    public static class PgDatabase
    {
        /// <summary>
        /// Загрузка справочника автомобилей
        /// </summary>
        /// <param name="org_id">Организация</param>
        /// <param name="gid">Автомобиль</param>
        /// <returns></returns>
        public static ObservableCollection<wb_Car> Load_Cars(int org_id, int? gid)
        {
            ObservableCollection<wb_Car> x = new ObservableCollection<wb_Car>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT c.gid, c.gos_no, c.gar_no, ct.name type_name, cm.name mark_name, cmd.name model_name, c.model_id" +
                                " FROM autobase.cars c" +
                                " LEFT JOIN autobase.cars_types ct ON c.type_id = ct.gid" +
                                " LEFT JOIN autobase.cars_marks cm ON c.mark_id = cm.gid" +
                                " LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid" +
                                " WHERE c.org_id = {0} and c.type_id not in (2,39)", org_id);
                    if (gid != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT c.gid, c.gos_no, c.gar_no, ct.name type_name, cm.name mark_name, cmd.name model_name, c.model_id" +
                                " FROM autobase.cars c" +
                                " LEFT JOIN autobase.cars_types ct ON c.type_id = ct.gid" +
                                " LEFT JOIN autobase.cars_marks cm ON c.mark_id = cm.gid" +
                                " LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid" +
                                " WHERE c.gid = {0} ORDER BY gos_no", gid);
                    }
                    else
                    {
                        sqlqry.sql += " ORDER BY gos_no";
                    }
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Car(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("gos_no"),
                            sqlqry.GetValue<string>("gar_no"),
                            sqlqry.GetValue<string>("type_name"),
                            sqlqry.GetValue<string>("model_name"),
                            sqlqry.GetValue<string>("mark_name"),
                            sqlqry.GetValue<int>("model_id")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника прицепов
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Trailer> Load_Trailer(int org_id, int? gid)
        {
            ObservableCollection<wb_Trailer> x = new ObservableCollection<wb_Trailer>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT c.gid, c.gos_no, c.gar_no, cmd.name model_name" +
                                " FROM autobase.cars c" +
                                " LEFT JOIN autobase.cars_types ct ON c.type_id = ct.gid" +
                                " LEFT JOIN autobase.cars_marks cm ON c.mark_id = cm.gid" +
                                " LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid" +
                                " WHERE c.org_id = {0} and c.type_id in (2,39)", org_id);
                    if (gid != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT c.gid, c.gos_no, c.gar_no, cmd.name model_name" +
                                " FROM autobase.cars c" +
                                " LEFT JOIN autobase.cars_types ct ON c.type_id = ct.gid" +
                                " LEFT JOIN autobase.cars_marks cm ON c.mark_id = cm.gid" +
                                " LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid" +
                                " WHERE c.gid = {0} ORDER BY gos_no", gid);
                    }
                    else
                    {
                        sqlqry.sql += " ORDER BY gos_no";
                    }
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Trailer(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("gos_no"),
                            sqlqry.GetValue<string>("gar_no"),
                            sqlqry.GetValue<string>("model_name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника водителей
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Driver> Load_Driver(int org_id, int? gid)
        {
            ObservableCollection<wb_Driver> x = new ObservableCollection<wb_Driver>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT e.gid, e.firstname, e.lastname, e.middlename, e.tab_no" +
                                " FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg" +
                                " WHERE e.position_id = p.gid and p.group_id = pg.gid and pg.gid = 3 and e.org_id={0}", org_id);
                    if (gid != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT e.gid, e.firstname, e.lastname, e.middlename, e.tab_no" +
                                " FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg" +
                                " WHERE e.position_id = p.gid and p.group_id = pg.gid and e.gid={0}", gid);
                    }
                    else
                    {
                        sqlqry.sql += " ORDER BY lastname";
                    }
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Driver(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("firstname"),
                            sqlqry.GetValue<string>("lastname"),
                            sqlqry.GetValue<string>("middlename"),
                            sqlqry.GetValue<string>("tab_no")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника мастеров
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Repairer> Load_Repairer(int org_id, int? gid)
        {
            ObservableCollection<wb_Repairer> x = new ObservableCollection<wb_Repairer>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT e.gid, e.firstname, e.lastname, e.middlename" +
                                " FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg" +
                                " WHERE e.position_id = p.gid and p.group_id = pg.gid and pg.gid = 1 and e.org_id={0}", org_id);
                    if (gid != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT e.gid, e.firstname, e.lastname, e.middlename" +
                                " FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg" +
                                " WHERE e.position_id = p.gid and p.group_id = pg.gid and e.gid={0}", gid);
                    }
                    else
                    {
                        sqlqry.sql += " ORDER BY lastname";
                    }
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Repairer(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("firstname"),
                            sqlqry.GetValue<string>("lastname"),
                            sqlqry.GetValue<string>("middlename")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника маршрутов
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Route> Load_Route(int org_id)
        {
            ObservableCollection<wb_Route> x = new ObservableCollection<wb_Route>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT gid, name FROM autobase.waybills_routes WHERE org_id={0} ORDER BY name", org_id);
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Route(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника организаций
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Organisation> Load_Organisations()
        {
            ObservableCollection<wb_Organisation> x = new ObservableCollection<wb_Organisation>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = @"SELECT gid, name, waybill_name_seq, waybill_med_checks FROM autobase.orgs WHERE gid in ( SELECT autobase.get_access_orgs() ) and waybill_name_seq is not null;";
                    sqlqry.ExecuteReader();
                    if (sqlqry.CanRead())
                    {
                        x.Add(new wb_Organisation(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name"),
                            sqlqry.GetValue<string>("waybill_name_seq"),
                            sqlqry.GetValue<bool>("waybill_med_checks")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника видов работ
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Work_type> Load_Work_types()
        {
            ObservableCollection<wb_Work_type> x = new ObservableCollection<wb_Work_type>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name FROM autobase.waybills_work_types";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Work_type(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника видов груза
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Cargo_type> Load_Cargo_types()
        {
            ObservableCollection<wb_Cargo_type> x = new ObservableCollection<wb_Cargo_type>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name FROM autobase.waybills_cargo_types";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Cargo_type(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника режимов работы
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Work_mode> Load_Work_modes()
        {
            ObservableCollection<wb_Work_mode> x = new ObservableCollection<wb_Work_mode>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name, hours FROM autobase.waybills_work_regimes";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Work_mode(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name"),
                            sqlqry.GetValue<decimal>("hours")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника видов топлива
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Fuel_mark> Load_Fuel_marks()
        {
            ObservableCollection<wb_Fuel_mark> x = new ObservableCollection<wb_Fuel_mark>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name FROM autobase.waybills_fuel_marks";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Fuel_mark(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name"),
                            sqlqry.GetValue<string>("name").ToUpper().Trim().Equals("НЕТ") ? true : false
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника особых отметок
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Special_note> Load_Special_notes()
        {
            ObservableCollection<wb_Special_note> x = new ObservableCollection<wb_Special_note>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name FROM autobase.waybills_special_notes";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Special_note(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника зоны (работ)
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Road_type> Load_Road_types()
        {
            ObservableCollection<wb_Road_type> x = new ObservableCollection<wb_Road_type>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT gid, name FROM autobase.waybills_road_types";
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Road_type(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("name")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Загрузка справочника топливных карт
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<wb_Fuel_card> Load_Fuel_cards(int org_id, int? gid_one = null, int? gid_two = null)
        {
            ObservableCollection<wb_Fuel_card> x = new ObservableCollection<wb_Fuel_card>();
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT gid, card_no, fuel_mark_id FROM autobase.waybills_fuel_cards WHERE org_id = {0} ", org_id);
                    if (gid_one != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT gid, card_no, fuel_mark_id FROM autobase.waybills_fuel_cards where gid = {0}", gid_one);
                    }
                    if (gid_two != null)
                    {
                        sqlqry.sql += String.Format(" UNION SELECT gid, card_no, fuel_mark_id FROM autobase.waybills_fuel_cards where gid = {0}", gid_two);
                    }
                    sqlqry.ExecuteReader();
                    while (sqlqry.CanRead())
                    {
                        x.Add(new wb_Fuel_card(
                            sqlqry.GetValue<int>("gid"),
                            sqlqry.GetValue<string>("card_no"),
                            sqlqry.GetValue<int>("fuel_mark_id")
                            ));
                    }
                    sqlqry.Close();
                }
                catch (Exception z)
                {
                    sqlqry.Close();
                    throw z;
                }
            }
            return x;
        }

        /// <summary>
        /// Может ли текущий пользователь изменять значения путевого листа, после его выписки
        /// </summary>
        /// <returns></returns>
        public static bool isAdminWaybill()
        {
            bool rez = false;
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = "SELECT autobase.is_admin_waybills() AS adminQ";
                    if (sqlqry.ExecuteReader() && sqlqry.CanRead())
                    {
                        rez = sqlqry.GetValue<bool>("adminQ");
                    }
                    sqlqry.Close();
                }
                catch (Exception x)
                {
                    sqlqry.Close();
                    throw x;
                }
            }
            return rez;
        }

        public static void Load_Waybill_Item(ref Waybill_M modelka)
        {
            //Waybill_M modelka = new Waybill_M();
            if (modelka.Gid == null) return;
            int id_object = (int)modelka.Gid;
            using (ISQLCommand sqlqry = MainPluginClass.App.SqlWork())
            {
                try
                {
                    sqlqry.sql = String.Format("SELECT * FROM autobase.waybills WHERE gid = {0}", id_object);
                    if (sqlqry.ExecuteReader() && sqlqry.CanRead())
                    {

                        modelka.Gid = sqlqry.GetValue<int>("gid");
                        modelka.Doc_date = sqlqry.GetValue("doc_date") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("doc_date");
                        modelka.Doc_no = sqlqry.GetValue("doc_no") == null ? "" : sqlqry.GetValue<String>("doc_no");
                        modelka.Car_id = sqlqry.GetValue<int>("car_id");
                        modelka.Trailer_id = sqlqry.GetValue<int?>("trailer_id");
                        modelka.Automaster_id = sqlqry.GetValue<int?>("automaster_id");
                        modelka.Date_out_plan = sqlqry.GetValue("date_out_plan") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("date_out_plan");
                        modelka.Date_out_fact = sqlqry.GetValue("date_out_fact") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("date_out_fact");
                        modelka.Date_in_plan = sqlqry.GetValue("date_in_plan") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("date_in_plan");
                        modelka.Date_in_fact = sqlqry.GetValue("date_in_fact") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("date_in_fact");
                        modelka.Driver_id = sqlqry.GetValue<int?>("driver_id");
                        modelka.Work_regime_id = sqlqry.GetValue<int?>("work_regime_id");
                        modelka.Motorcade = sqlqry.GetValue<int?>("motorcade");
                        modelka.Brigade = sqlqry.GetValue<int?>("brigade");
                        modelka.Route_id = sqlqry.GetValue<int?>("route_id");
                        modelka.Work_type_id = sqlqry.GetValue<int?>("work_type_id");
                        modelka.Cargo_type_id = sqlqry.GetValue<int?>("cargo_type_id");
                        modelka.Ttd_count = sqlqry.GetValue<int?>("ttd_count");
                        modelka.Trip_count = sqlqry.GetValue<int?>("trip_count");
                        modelka.Road_type_id = sqlqry.GetValue<int?>("road_type_id");
                        modelka.Km_begin = sqlqry.GetValue<decimal?>("km_begin");
                        modelka.Km_end = sqlqry.GetValue<decimal?>("km_end");
                        modelka.Km_run = sqlqry.GetValue<decimal?>("km_run");
                        modelka.Km_run_glonass = sqlqry.GetValue<decimal?>("km_run_glonass");
                        modelka.Mh_begin = sqlqry.GetValue<decimal?>("mh_begin");
                        modelka.Mh_end = sqlqry.GetValue<decimal?>("mh_end");
                        modelka.Mh_run = sqlqry.GetValue<decimal?>("mh_run");
                        modelka.Mh_run_glonass = sqlqry.GetValue<decimal?>("mh_run_glonass");
                        modelka.Mh_ob_begin = sqlqry.GetValue<decimal?>("mh_ob_begin");
                        modelka.Mh_ob_end = sqlqry.GetValue<decimal?>("mh_ob_end");
                        modelka.Mh_ob_run = sqlqry.GetValue<decimal?>("mh_ob_run");
                        modelka.Mh_ob_run_glonass = sqlqry.GetValue<decimal?>("mh_ob_run_glonass");
                        modelka.Fuel_mark_id = sqlqry.GetValue<int?>("fuel_mark_id");
                        modelka.Fuel_plan = sqlqry.GetValue<decimal?>("fuel_plan");
                        modelka.Fuel_fact = sqlqry.GetValue<decimal?>("fuel_fact");
                        modelka.Fuel_fact_glonass = sqlqry.GetValue<decimal?>("fuel_fact_glonass");
                        modelka.Fuel_begin = sqlqry.GetValue<decimal?>("fuel_begin");
                        modelka.Fuel_end = sqlqry.GetValue<decimal?>("fuel_end");
                        modelka.Fuel_begin_glonass = sqlqry.GetValue<decimal?>("fuel_begin_glonass");
                        modelka.Fuel_end_glonass = sqlqry.GetValue<decimal?>("fuel_end_glonass");
                        modelka.Fuel_mark2_id = sqlqry.GetValue<int?>("fuel_mark2_id");
                        modelka.Fuel_plan2 = sqlqry.GetValue<decimal?>("fuel_plan2");
                        modelka.Fuel_fact2 = sqlqry.GetValue<decimal?>("fuel_fact2");
                        modelka.Fuel_fact2_glonass = sqlqry.GetValue<decimal?>("fuel_fact2_glonass");
                        modelka.Fuel_begin2 = sqlqry.GetValue<decimal?>("fuel_begin2");
                        modelka.Fuel_end2 = sqlqry.GetValue<decimal?>("fuel_end2");
                        modelka.Fuel_begin2_glonass = sqlqry.GetValue<decimal?>("fuel_begin2_glonass");
                        modelka.Fuel_end2_glonass = sqlqry.GetValue<decimal?>("fuel_end2_glonass");
                        modelka.Fuel_card_id = sqlqry.GetValue<int?>("fuel_card_id");
                        modelka.Fuel_card2_id = sqlqry.GetValue<int?>("fuel_card2_id");
                        modelka.Calc_fuel_norm = sqlqry.GetValue<decimal?>("calc_fuel_norm");
                        modelka.Calc_fuel_fact = sqlqry.GetValue<decimal?>("calc_fuel_fact");
                        modelka.Calc_fuel_delta = sqlqry.GetValue<decimal?>("calc_fuel_delta");
                        modelka.Calc_fuel_drain = sqlqry.GetValue<int?>("calc_fuel_drain");
                        modelka.Calc_km_run_delta = sqlqry.GetValue<decimal?>("calc_km_run_delta");
                        modelka.Calc_mh_run_delta = sqlqry.GetValue<decimal?>("calc_mh_run_delta");
                        modelka.Pay_work_h = sqlqry.GetValue<int?>("pay_work_h");
                        modelka.Pay_lunch_h = sqlqry.GetValue<int?>("pay_lunch_h");
                        modelka.Pay_duty_h = sqlqry.GetValue<int?>("pay_duty_h");
                        modelka.Pay_repair_h = sqlqry.GetValue<int?>("pay_repair_h");
                        modelka.Pay_day_h = sqlqry.GetValue<int?>("pay_day_h");
                        modelka.Pay_night_h = sqlqry.GetValue<int?>("pay_night_h");
                        modelka.Pay_rate_rh = sqlqry.GetValue<decimal?>("pay_rate_rh");
                        modelka.Pay_total_r = sqlqry.GetValue<decimal?>("pay_total_r");
                        modelka.Support_persons = sqlqry.GetValue("support_persons") == null ? "" : sqlqry.GetValue<String>("support_persons");
                        modelka.Special_note_id = sqlqry.GetValue<int?>("special_note_id");
                        modelka.Notes = sqlqry.GetValue("notes") == null ? "" : sqlqry.GetValue<String>("notes");
                        modelka.Createdate = sqlqry.GetValue("createdate") == null ? DateTime.MinValue : sqlqry.GetValue<DateTime>("createdate");
                        modelka.Secondsave = sqlqry.GetValue<int?>("secondsave");
                        modelka.User_id = sqlqry.GetValue<int?>("user_id");
                        modelka.Org_id = sqlqry.GetValue<int?>("org_id");

                    }
                    sqlqry.Close();
                }
                catch (Exception x)
                {
                    sqlqry.Close();
                    throw x;
                }
            }
        }
    }
}
