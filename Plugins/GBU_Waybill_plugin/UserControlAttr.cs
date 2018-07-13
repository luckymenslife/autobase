using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interfaces.UserControls;
using Interfaces;
using Interfaces.FastReport;
using GBU_Waybill_plugin.RemoteMedService;
using GBU_Waybill_plugin.MTClasses;
using GBU_Waybill_plugin.MTClasses.Tools;
using NpgsqlTypes;
using GBU_Waybill_plugin.MTClasses.Tasks;
using GBU_Waybill_plugin.MyComponents;

namespace GBU_Waybill_plugin
{

    public partial class UserControlAttr : UserControl, IUserControlMain
    {
        private ContextMenu _contextMenu = new ContextMenu();
        private ContextMenu _saveContextMenu = new ContextMenu();
        private bool _can_open;
        private int? pl_id;
        public string norma_sp = "", norma_mch = "", norma_mchobr = "", norma_t1 = "", norma_t2 = "", modcomment = "";
        public string err_body1;
        int secsave = 0;
        private string org_put_list_seq_name = "";
        private List<int> taks_to_del;
        private int MapEditorTablePutList { get; set; }
        private bool CanEditPutList_isAdmin { get; set; }
        private bool isEdited { get; set; }
        private string _decSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToString();
        private int org_id;
        private int ff;
        private bool isRES;
        private int resID = 0;
        private string issType = "";
        private string issFrom = "";
        private string issTo = "";
        private string issCust = "";
        private int? issStartNum;
        private List<int> tripToDelete;
        long? carExternalId;
        private bool clearClose = true;
        private bool isInit = true;
        private bool isMotoPlRegime = false;
        private bool isGridWithErrors = false;

        private string prevNormCons = "";
        private string prev100kmNormCons = "";

        private int carService = 0;
        private string carOwner;

        private List<myItem> driversList = new List<myItem>();
        private bool filterDriver1Combobox = true;
        private bool filterDriver2Combobox = true;

        private List<myItem> carsList = new List<myItem>();
        private bool filterCarCombobox = true;
        private bool factDateOutInitialized = false;
        private bool factDateReturnInitialized = false;

        private int prevDriver1Idx = -1;
        private int prevDriver2Idx = -1;
        private bool driverChangedAutomatically = true;
        private bool carUpdateInProcess = false;

        public UserControlAttr(int? id_object, int id_table)
        {

            InitializeComponent();

            pl_id = id_object;

            // Запоминаем таблицу на которую назначен плагин
            MapEditorTablePutList = id_table;

            // Загрузим список печатных форм
            _contextMenu.MenuItems.Clear();
            foreach (var item in MainPluginClass.Work.FastReport.FindReportsByIdTable(460).ToArray())
            {
                if (item.Type == enTypeReport.Object)
                {
                    MenuItem x = _contextMenu.MenuItems.Add(item.Caption);
                    x.Tag = item;
                    x.Click += print_MenuStrip_Click;
                }
            }
            splitButton1.SplitMenu = _contextMenu;
            splitButton1.Click += print_MenuStrip_Click;


            _saveContextMenu.MenuItems.Clear();
            MenuItem menuItem = _saveContextMenu.MenuItems.Add("Сохранить");
            menuItem.Tag = "SAVE";
            menuItem.Click += btn_save_click;
            menuItem = _saveContextMenu.MenuItems.Add("Сохранить и закрыть");
            menuItem.Tag = "SAVE&CLOSE";
            menuItem.Click += btn_save_click;
            
            splitButton_save.SplitMenu = _saveContextMenu;
            splitButton_save.Tag = "SAVE";
            splitButton_save.Click += btn_save_click;


            // ПРОЧИЕ ДЛЯ КНОПОЧЕК
            BTN_cancel.Click += btn_cancel_click;


            this.textBox_fuelBegin.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuelEnd.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuel100kmFact.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuel100kmPlan.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuelConsFact.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuelConsNorm.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuel1hPlan.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);
            this.textBox_fuelEquipPlan.LostFocus += new EventHandler(this.textBox_LostFocus_Formatter);

            //this.dataGridView_trip.SelectionChanged += new EventHandler(this.dataGridView_trip_SelectionChanged);

            isEdited = false;
        }
        
        void print_MenuStrip_Click(object sender, EventArgs e)
        {

            IReportItem_M _report_item = null;

            if (sender is MenuItem)
            {
                _report_item = (IReportItem_M)((MenuItem)sender).Tag;
            }
            else if (sender is SplitButton)
            {
                foreach (MenuItem z in splitButton1.SplitMenu.MenuItems)
                {
                    if (z.Tag != null && z.Tag is IReportItem_M)
                    {
                        if (splitButton1.Tag != null && ((IReportItem_M)z.Tag).IdReport == (int)splitButton1.Tag 
                                || splitButton1.Tag == null && "Путевой лист".Equals(((IReportItem_M)z.Tag).Caption))
                        {
                            _report_item = (IReportItem_M)z.Tag;
                            break;
                        }
                    }
                }
            }

            if (_report_item != null)
            {
                try
                {
                    if (save_PL())
                    {
                        if (this.pl_id != null)
                        {
                            MainPluginClass.Work.FastReport.OpenReport(_report_item, new FilterTable(this.pl_id.Value, MapEditorTablePutList, " gid", ""));
                        }
                        else
                        {
                            MessageBox.Show("Сохраните путевой лист!", "Ошибка сохранения!");
                        }
                    }
                }
                catch (Exception x)
                {
                    MessageBox.Show("Возможно не назначена печатная форма путевого листа!\r\n" + x.Message, "Ошибка печати!");
                }
            }
        }

        void btn_cancel_click(object sender, EventArgs e)
        {
            try
            {
                this.ParentForm.Close();
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно закрыть путевой лист: " + x.Message, "Ошибка путевого листа!"); }        
        }

        public bool CancelOpen
        {
            get { return this._can_open; }
            set { this._can_open = false; }
        }

        public event EventHandler<eventCloseForm> CloseForm;

        public UserControl GetUserControl()
        {
            return this;
        }

        public Size SizeWindow
        {
            get { return this.Size; }
        }

        public string Title
        {
            get { return ""; }
        }

        public object ViewModel
        {
            get { return null; }
        }

        /// <summary>
        /// Либо центрирование формы когда экран большой, либо сжатие формы на маленьких экранах
        /// </summary>
        private void CorrectPosition()
        {
            // Если экран маленький, то высоту окна подогнать бы...  
            if (this.ParentForm.Height > Screen.FromControl(this).WorkingArea.Height)
            {
                this.ParentForm.Height = Screen.FromControl(this).WorkingArea.Height;
            }
            // Теперь, если панель задач перекрывает окно - переместим
            if ((this.ParentForm.Location.Y + this.ParentForm.Height) > Screen.FromControl(this).WorkingArea.Height ||
                (this.ParentForm.Location.X + this.ParentForm.Width) > Screen.FromControl(this).WorkingArea.Width)
            {
                this.ParentForm.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width - this.ParentForm.Size.Width) / 2, Screen.FromControl(this).WorkingArea.Height - this.ParentForm.Height);
            }
            else
            {
                // Центрирование формы
                this.ParentForm.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width - this.ParentForm.Size.Width) / 2, (Screen.PrimaryScreen.Bounds.Size.Height - this.ParentForm.Size.Height) / 2);
            }
        }

        private void UserControlAttr_Load(object sender, EventArgs e)
        {
            this.ParentForm.PerformLayout();
            this.ParentForm.Text = "Путевой лист";
            this.ParentForm.MinimumSize = new Size(this.ParentForm.Size.Width, 0);
            taks_to_del = new List<int>();
            tripToDelete = new List<int>();
            // Поправим положение на экране
            CorrectPosition();

            this.ParentForm.FormClosing += new FormClosingEventHandler(OnFormClosing);
            this.dataGridView_trip.CellValueChanged += new DataGridViewCellEventHandler(this.dg_cell_changed);
            this.dataGridView_trip.CellValueChanged += new DataGridViewCellEventHandler(this.dataGrid_trip_Formatter);


            //comboBox_driver1.DrawMode = DrawMode.OwnerDrawFixed;
            //comboBox_driver1.DrawItem += new DrawItemEventHandler(comboBox_driver1_DrawItem);

            //comboBox_driver2.DrawMode = DrawMode.OwnerDrawFixed;
            //comboBox_driver2.DrawItem += new DrawItemEventHandler(comboBox_driver2_DrawItem);

            // Определение переменных для последующей установки элементов справчочника
            int id_driver1 = 0;
            int id_driver2 = 0;
            int id_car = 0;
            int id_issue = 0;

            int serviceId = 0;

            int pl_creator_id = 0;



            ISQLCommand sql_frompl = MainPluginClass.App.SqlWork();
            ISQLCommand sql_frompl_new = MainPluginClass.App.SqlWork();

            ///////////////////// ПОДГРУЗКА РАЙОНА РАСПОРЯЖЕНИЯ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ ///////////////////////////
            
            sql_frompl_new = MainPluginClass.App.SqlWork();

            sql_frompl_new.sql = "SELECT urk.sokrawennoe_naimenovanie " +
                "FROM autobase.umjets_rajon_zakreplenie urk " +
                    "JOIN autobase.users usr ON(usr.rajon_zakreplenija = urk.gid AND usr.user_id = autobase.get_user_id())";
            try
            {
                sql_frompl_new.ExecuteReader();
                if (sql_frompl_new.CanRead() && pl_id == null)
                {
                        textBox_userDept.Text = sql_frompl_new.GetString("sokrawennoe_naimenovanie");
                }
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

            sql_frompl_new.Close();



            ///////////////////// ЯВЛЯЕТСЯ ЛИ ТЕКУЩИЙ ПОЛЬЗОВАТЕЛЬ ПОЛЬЗОВАТЕЛЕМ РЭС + Параметры РЭС///////////////////////////

            if (pl_id != null)
            {
                sql_frompl_new = MainPluginClass.App.SqlWork();

                sql_frompl_new.sql = "SELECT usr.id as user_id " +
                                     "FROM autobase._umjets_putevye_listy_history plh " +
                                     "  JOIN sys_scheme.user_db usr ON usr.login = plh.user_name " +
                                     "WHERE type_operation = 1 AND plh.gid = " + pl_id;
                try
                {
                    sql_frompl_new.ExecuteReader();
                    if (sql_frompl_new.CanRead())
                    {
                        pl_creator_id = sql_frompl_new.GetInt32("user_id");
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

                sql_frompl_new.Close();

            }
            
            sql_frompl_new = MainPluginClass.App.SqlWork();

            sql_frompl_new.sql = "SELECT urk.gid, urk.sokrawennoe_naimenovanie, urk.otkuda_sleduet, urk.v_rasporjazhenie, urk.adres_podachi, urk.vid_raboty, " +
                    "urk.nachalnye_nomera_putevogo_lista " +
                "FROM autobase.umjets_rajon_zakreplenie urk " +
                    "JOIN autobase.users usr ON(usr.rajon_zakreplenija = urk.gid AND usr.user_id = " + (pl_creator_id > 0 ? pl_creator_id.ToString() : "autobase.get_user_id()") + " " +
                        "AND urk.sokrawennoe_naimenovanie ilike '%РЭС%')";
            try
            {
                isRES = false;
                sql_frompl_new.ExecuteReader();
                if (sql_frompl_new.CanRead())
                {
                    isRES = true;
                    comboBox_issue.Visible = false;
                    textBox_issueNum.Visible = true;

                    resID = sql_frompl_new.GetInt32("gid");
                    issType = sql_frompl_new.GetString("vid_raboty");
                    issFrom = sql_frompl_new.GetString("otkuda_sleduet");
                    issTo = sql_frompl_new.GetString("adres_podachi");
                    issCust = sql_frompl_new.GetString("v_rasporjazhenie");
                    issStartNum = sql_frompl_new.GetInt32("nachalnye_nomera_putevogo_lista");
                }
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

            sql_frompl_new.Close();





            ///////////////////// ОСНОВНАЯ ИНФОРМАЦИЯ ПО ПУТЕВЫМ ЛИСТАМ ///////////////////////////

            if (pl_id == null)
            {
                textBox_plNum.Text = issStartNum > 0 ? issStartNum.ToString() : string.Empty;
                dateTimePicker_timeOutPlan.Value = DateTime.Parse("08:00");
                dateTimePicker_timeReturnPlan.Value = DateTime.Parse("17:00");
            } else
            {
                sql_frompl_new = MainPluginClass.App.SqlWork();

                sql_frompl_new.sql =
                "SELECT upl.gid, split_part(upl.nomer_putevogo_lista, '/', 1) as nomer_putevogo_lista, " +
                  "split_part(upl.nomer_putevogo_lista, '/', 2) as nomer_putevogo_lista_suffix, upl.voditel, " +
                  "upl.voditel__vyezd, upl.udostoverenie__, upl.udostoverenie____2, " +
                  "upl.data_vyezda__plan, upl.data_vozvrawenija__plan, upl.kilometrazh__nachalo, upl.kilometrazh__projdeno, " +
                  "upl.projdeno__km, upl.toplivo_1__nachalo, upl.toplivo_1__konec, upl.rashod_po_norme, upl.rashod__fakt, upl.zajavka, " +
                  "upl.v_rasporjazhenie, upl.adres_podachi, upl.otkuda_sleduet, upl.zadanie, upl.transportnoe_sredstvo, upl.tip_ts, " +
                  "upl.gos__, upl.gar___, upl.data_vyezda__fakt, upl.data_vozvrawenija__fakt, upl.norma_rashoda_na_100_km__plan, " +
                  "upl.norma_rashoda_na_100_km__fakt, upl.kilometrazh__datch___konec, upl.toplivo_1__datch___konec, " +
                  "upl.projdeno__datch___km, upl.rashod_po_datchikam, v_rasporjazhenie__sluzhba_ " +
                "FROM autobase._umjets_putevye_listy upl WHERE upl.gid = " + pl_id;

                try
                {
                    sql_frompl_new.ExecuteReader();

                    if (sql_frompl_new.CanRead())
                    {
                        textBox_plNum.Text = sql_frompl_new.GetString("nomer_putevogo_lista");
                        textBox_userDept.Text = sql_frompl_new.GetString("nomer_putevogo_lista_suffix");
                        if (isRES)
                        {
                            textBox_issueNum.Text = sql_frompl_new.GetString("nomer_putevogo_lista");
                        }
                        else
                        {
                            id_issue = sql_frompl_new.GetInt32("zajavka");
                        }
                        textBox_rasp.Text = sql_frompl_new.GetString("v_rasporjazhenie");
                        textBox_issDest.Text = sql_frompl_new.GetString("adres_podachi");
                        textBox_issSource.Text = sql_frompl_new.GetString("otkuda_sleduet");
                        textBox_issType.Text = sql_frompl_new.GetString("zadanie");
                        id_driver1 = sql_frompl_new.GetInt32("voditel");
                        id_driver2 = sql_frompl_new.GetInt32("voditel__vyezd");
                        textBox_udos1.Text = sql_frompl_new.GetString("udostoverenie__");
                        textBox_udos2.Text = sql_frompl_new.GetString("udostoverenie____2");
                        id_car = sql_frompl_new.GetInt32("transportnoe_sredstvo");
                        textBox_gosNum.Text = sql_frompl_new.GetString("gos__");
                        textBox_garNum.Text = sql_frompl_new.GetString("gar___");
                        //textBox_regNum.Text = sql_frompl_new.GetString("gos__");

                        string sqlTimestamp = sql_frompl_new.GetString("data_vyezda__plan");
                        if (sqlTimestamp != null)
                        {
                            dateTimePicker_dateOutPlan.Value = DateTime.Parse(sqlTimestamp);
                            dateTimePicker_timeOutPlan.Value = DateTime.Parse(sqlTimestamp);
                        }
                        else
                        {
                            dateTimePicker_timeOutPlan.Value = DateTime.Parse("08:00");
                        }

                        sqlTimestamp = sql_frompl_new.GetString("data_vozvrawenija__plan");
                        if (sqlTimestamp != null)
                        {
                            dateTimePicker_dateReturnPlan.Value = DateTime.Parse(sqlTimestamp);
                            dateTimePicker_timeReturnPlan.Value = DateTime.Parse(sqlTimestamp);
                        }
                        else
                        {
                            dateTimePicker_timeReturnPlan.Value = DateTime.Parse("17:00");
                        }

                        sqlTimestamp = sql_frompl_new.GetString("data_vyezda__fakt");
                        if (sqlTimestamp != null)
                        {
                            dateTimePicker_dateOutFact.Value = DateTime.Parse(sqlTimestamp);
                            dateTimePicker_timeOutFact.Value = DateTime.Parse(sqlTimestamp);
                        }

                        sqlTimestamp = sql_frompl_new.GetString("data_vozvrawenija__fakt");
                        if (sqlTimestamp != null)
                        {
                            dateTimePicker_dateReturnFact.Value = DateTime.Parse(sqlTimestamp);
                            dateTimePicker_timeReturnFact.Value = DateTime.Parse(sqlTimestamp);
                        }

                        textBox_kmBegin.Text = sql_frompl_new.GetString("kilometrazh__nachalo");
                        textBox_kmEnd.Text = sql_frompl_new.GetString("kilometrazh__projdeno");
                        textBox_kmDiff.Text = sql_frompl_new.GetString("projdeno__km");
                        textBox_fuelBegin.Text = sql_frompl_new.GetString("toplivo_1__nachalo");
                        formatTextBox(textBox_fuelBegin);

                        textBox_fuelEnd.Text = sql_frompl_new.GetString("toplivo_1__konec");
                        formatTextBox(textBox_fuelEnd);

                        textBox_fuelConsFact.Text = sql_frompl_new.GetString("rashod__fakt");
                        formatTextBox(textBox_fuelConsFact);

                        textBox_fuelConsNorm.Text = sql_frompl_new.GetString("rashod_po_norme");
                        prevNormCons = textBox_fuelConsNorm.Text;
                        formatTextBox(textBox_fuelConsNorm);
                        if (string.IsNullOrEmpty(textBox_fuelConsFact.Text) || prevNormCons.Equals(textBox_fuelConsFact.Text))
                        {
                            textBox_fuelConsFact.Text = textBox_fuelConsNorm.Text;
                        }

                        textBox_kmGaugeEnd.Text = sql_frompl_new.GetString("kilometrazh__datch___konec");
                        textBox_fuelGaugeEnd.Text = sql_frompl_new.GetString("toplivo_1__datch___konec");
                        textBox_kmGaugeDist.Text = sql_frompl_new.GetString("projdeno__datch___km");
                        textBox_fuelGaugeCons.Text = sql_frompl_new.GetString("rashod_po_datchikam");

                        serviceId = sql_frompl_new.GetInt32("v_rasporjazhenie__sluzhba_");
                        

                        // расчет фактического расхода на 100 км
                        float diff = 0;
                        float fact = 0;
                        try
                        {
                            diff = string.IsNullOrEmpty(textBox_kmDiff.Text) ? 0 : float.Parse(textBox_kmDiff.Text);
                            fact = string.IsNullOrEmpty(textBox_fuelConsFact.Text) ? 0 : float.Parse(textBox_fuelConsFact.Text);
                        }
                        catch
                        {
                            showInvalidFormatMessage("Пройдено и/или Расход, факт");
                        }

                        textBox_fuel100kmFact.Text = diff == 0 ? "" : (fact / diff * (isMotoPlRegime ? 1 : 100)).ToString();
                        formatTextBox(textBox_fuel100kmFact);
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

                sql_frompl_new.Close();

            }
            

            ///////////////////// ПОДГРУЗКА ИНФОРМАЦИИ ПО ТС ///////////////////////////

            comboBox_car.Items.Clear();

            sql_frompl_new = MainPluginClass.App.SqlWork();

            sql_frompl_new.sql = "SELECT ua.gid, utt.naimenovanie as car_type, ua.tip_ts as car_type_id, ummt.naimenovanie as car_mark, ua.gos___, ua.gar___, ua.reg___, " +
                "coalesce((CASE WHEN extract('month' FROM now()) BETWEEN 4 AND 10 THEN letnjaja_norma_rashoda_topliva__l_1ch_ " +
                "ELSE zimnjaja_norma_rashoda_topliva__l_1ch_ END), 0) as car_norm, " +
                "coalesce((CASE WHEN extract('month' FROM now()) BETWEEN 4 AND 10 THEN letnjaja_norma_rashoda_topliva__l_100km__l_1ch_ " +
                "ELSE zimnjaja_norma_rashoda_topliva END), 0) as car_100km_norm, " +
                "coalesce(ua.rabota_s_ustanovkoj__l_1ch_, 0) as car_equip_norm, " +
                "podrazdelenie, " +
                "utt.tip_putevogo_lista in (1, 3, 4) AS is_complex, " +
                "upit.rezhim_raboty, " +
                "upit.v_rasprjazhenie_podrazdelenie, " +
                "upit.v_rasporjazhenie_fio, " +
                "wwr.hours, " +
                "ua.emkost_bakov, " +
                "CASE WHEN utt.raschet_po_motochasam = 1 THEN true ELSE false END as pl_regime " +
            "FROM autobase.umjets_avtopark ua " +
              "JOIN autobase.umjets_tip_ts utt ON ua.tip_ts = utt.gid " +
              "JOIN autobase.umjets_marki__modeli_ts ummt ON ua.marka__model_ts = ummt.gid " +
              "LEFT JOIN autobase.umjets_plan_ispolzovanija_ts upit ON upit.transportnoe_sredstvo = ua.gid " +
              "LEFT JOIN autobase.waybills_work_regimes wwr ON wwr.gid = upit.rezhim_raboty " +
              "ORDER BY ua.gos___";
            try
            {
                sql_frompl_new.ExecuteReader();
                while (sql_frompl_new.CanRead())
                {
                    float fuelTank = 0;
                    try
                    {
                        string dbFuelTank = sql_frompl_new.GetString("emkost_bakov");
                        fuelTank = !string.IsNullOrEmpty(dbFuelTank) ? float.Parse(dbFuelTank.Replace('.', ',')) : 0;
                    }
                    catch {}

                    myItem x_pl_car = new myItem(
                        sql_frompl_new.GetString("gos___") + " - " + sql_frompl_new.GetString("car_mark"),
                        sql_frompl_new.GetInt32("gid"),
                        new CarItem(
                            sql_frompl_new.GetString("car_type"),
                            sql_frompl_new.GetInt32("car_type_id"),
                            sql_frompl_new.GetString("gos___"),
                            sql_frompl_new.GetString("gar___"),
                            sql_frompl_new.GetString("reg___"),
                            sql_frompl_new.GetValue<float>("car_norm"),
                            sql_frompl_new.GetValue<float>("car_equip_norm"),
                            sql_frompl_new.GetValue<float>("car_100km_norm"),
                            sql_frompl_new.GetInt32("podrazdelenie"),
                            sql_frompl_new.GetBoolean("is_complex"),
                            sql_frompl_new.GetInt32("rezhim_raboty"),
                            sql_frompl_new.GetInt32("hours"),
                            sql_frompl_new.GetInt32("v_rasprjazhenie_podrazdelenie"),
                            sql_frompl_new.GetString("v_rasporjazhenie_fio"),
                            fuelTank,
                            sql_frompl_new.GetBoolean("pl_regime")
                            )
                        );
                    int idx = comboBox_car.Items.Add(x_pl_car);
                    carsList.Add(x_pl_car);
                    if (id_car == x_pl_car.GetId)
                    {
                        comboBox_car.SelectedIndex = idx;
                        textBox_carType.Text = ((CarItem)x_pl_car.Data).getCarType;
                        prev100kmNormCons = textBox_fuel100kmPlan.Text;
                        textBox_fuel100kmPlan.Text = ((CarItem)x_pl_car.Data).getCar100kmNorm.ToString();
                        formatTextBox(textBox_fuel100kmPlan);
                        textBox_fuel1hPlan.Text = ((CarItem)x_pl_car.Data).getCarNorm.ToString();
                        formatTextBox(textBox_fuel1hPlan);
                        textBox_fuelEquipPlan.Text = ((CarItem)x_pl_car.Data).getCarEquipNorm.ToString();
                        formatTextBox(textBox_fuelEquipPlan);

                        isMotoPlRegime = sql_frompl_new.GetBoolean("pl_regime");
                        // в случае, если значения Километража и уровня топлива при выезде пустые (вероятно, что это копия ПЛ),
                        // то обновляем значение Километража и уровня топлива при выезде в зависимости от предыдущего ПЛ по тачке
                        if (string.IsNullOrEmpty(textBox_kmBegin.Text) && string.IsNullOrEmpty(textBox_fuelBegin.Text))
                        {
                            updateStartOdoAndFuel();
                        }
                    }

                }
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

            sql_frompl_new.Close();
            



            ///////////////////// ПОДГРУЗКА ВОДИТЕЛЕЙ И ИНФОРМАЦИИ ПО НИМ ///////////////////////////

            comboBox_driver1.Items.Clear();
            comboBox_driver2.Items.Clear();

            sql_frompl_new = MainPluginClass.App.SqlWork();

            sql_frompl_new.sql = "SELECT uvs.gid, coalesce(uvs.fio, uvs.familija) as fio, uvs.voditelskoe_udostoverenie, " + 
                    "uvr_out.vremja as vremja_vyhoda, uvr_return.vremja as vremja_vozvrata " +
                "FROM autobase._umjets_voditel_sotrudnik uvs " +
                    "LEFT JOIN autobase.umjets_privjazka_voditelej upv ON uvs.gid = upv.voditel " +
                    "LEFT JOIN autobase.umjets_vremja_raboty uvr_out ON upv.vremja_vyhoda = uvr_out.gid " +
                    "LEFT JOIN autobase.umjets_vremja_raboty uvr_return ON upv.vremja_vozvrata = uvr_return.gid " +
                "ORDER BY 2";
            try
            {
                sql_frompl_new.ExecuteReader();
                while (sql_frompl_new.CanRead())
                {
                    myItem x_pl_driver = new myItem(
                        sql_frompl_new.GetString("fio"),
                        sql_frompl_new.GetInt32("gid"),
                        new DriverItem(sql_frompl_new.GetString("voditelskoe_udostoverenie"),
                            sql_frompl_new.GetString("vremja_vyhoda"),
                            sql_frompl_new.GetString("vremja_vozvrata"))
                        );

                    driversList.Add(x_pl_driver);
                }

                reorderDriverList(id_driver1, id_driver2);
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

            sql_frompl_new.Close();

            ///////////////////// ПОДГРУЗКА ЗАЯВОК ///////////////////////////

            // Если пользователь, под которым логинимся принадлежит к РЭСу (справочник "УМЭТС Подразделение" второй столбец), 
            // то дублируем сюда поле "Номер путевого листа". 
            // Иначе ничего не тянем пользователь сам выбирает из таблицы "УМЭТС Заявки на ТС".

            if (!isRES)
            {
                comboBox_issue.Items.Clear();

                sql_frompl_new = MainPluginClass.App.SqlWork();

                sql_frompl_new.sql = "SELECT uznt.gid, uznt.nomer_zajavki, uznt.v_rasporjazhenie_sotrudnika, uznt.mesto__adres__vyezda, " +
                                        "uznt.mesto__adres__naznachenija, uznt.cel_poezdki__perevozimyj_gruz__gabarit__massa_ " +
                                    "FROM autobase.umjets_zajavki_na_ts uznt ORDER BY nomer_zajavki";
                try
                {
                    sql_frompl_new.ExecuteReader();
                    while (sql_frompl_new.CanRead())
                    {
                        myItem x_issue = new myItem(
                            sql_frompl_new.GetString("gid") + " " + sql_frompl_new.GetString("nomer_zajavki") + " - " +
                            sql_frompl_new.GetString("cel_poezdki__perevozimyj_gruz__gabarit__massa_"),
                            sql_frompl_new.GetInt32("gid"),
                            new IssueItem(
                                sql_frompl_new.GetString("v_rasporjazhenie_sotrudnika"),
                                sql_frompl_new.GetString("mesto__adres__naznachenija"),
                                sql_frompl_new.GetString("mesto__adres__vyezda"),
                                sql_frompl_new.GetString("cel_poezdki__perevozimyj_gruz__gabarit__massa_")
                                )
                            );
                        int idx = comboBox_issue.Items.Add(x_issue);

                        if (id_issue == x_issue.GetId)
                        {
                            comboBox_issue.SelectedIndex = idx;
                        }
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }

                sql_frompl_new.Close();
            } else
            {
                textBox_issueNum.Text = textBox_plNum.Text;

                textBox_rasp.Text = issCust;
                textBox_issDest.Text = issTo;
                textBox_issSource.Text = issFrom;
                textBox_issType.Text = issType;

            }

            bool isComplex = false;

            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                isComplex = ((CarItem)item.Data).getIsComplex;
            }

            if (!isComplex)
            {
                // Значения колонки "Тип путевого листа"(в справочнике "УМЭТС Тип ТС") равно 2, или 5 (!isComplex)
                // Выводим выпадающий список со значениями из справочника "УМЭТС Служба" для редактирования поля.
                textBox_rasp.Visible = false;
                comboBox_service.Visible = true;

                String serviceSQL =
                        "SELECT uo.gid, naimenovanie, rajon_zakreplenie " +
                        "FROM autobase.umjets_organizacija uo";
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    comboBox_service.Items.Clear();

                    sqlCmd.sql = serviceSQL;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        int curID = sqlCmd.GetInt32("gid");

                        myItem x_service = new myItem(
                        sqlCmd.GetString("naimenovanie"),
                        curID,
                        sqlCmd.GetInt32("rajon_zakreplenie")
                        );
                        int idx = comboBox_service.Items.Add(x_service);

                        if (serviceId == curID)
                        {
                            comboBox_service.SelectedIndex = idx;
                        }
                    }
                }

            }
            else 
            {
                // Значения колонки "Тип путевого листа" (в справочнике "УМЭТС Тип ТС") равно 1,3, или 4 (isComplex)
                textBox_rasp.Visible = true;
                comboBox_service.Visible = false;
            }


            // автозаполнение нормы расхода топлива
            calcFact100kmFuelCons();

            if (string.IsNullOrEmpty(textBox_fuel100kmFact.Text) || prev100kmNormCons.Equals(textBox_fuel100kmFact.Text))
            {
                textBox_fuel100kmFact.Text = textBox_fuel100kmPlan.Text;
            }


            ///////////////////// ЗАПОЛНЕНИЕ ГРИДА ПОЕЗДОК ///////////////////////////

            if (pl_id != null)
            {
                var sql_grid = MainPluginClass.App.SqlWork();

                sql_grid.sql = "SELECT up.gid, up.data_vyezda, up.data_vozvrata, up.pokazanija_spidometra__nachalo, up.pokazanija_spidometra__vozvrawenie, " +
                                "up.dvizhenie_topliva__polucheno, up.rabota_dvigatelja_pri_stojanke__ch, up.rabota_ustanovki__ch, " +
                                "up.dvizhenie_topliva__vyezd, up.dvizhenie_topliva__vozvrawenie " +
                                "FROM autobase.umjets_poezdki up " +
                                "WHERE up.putevoj_list = " + pl_id + " " +
                                "ORDER BY up.data_vyezda";
               
                sql_grid.ExecuteReader();
                

                while (sql_grid.CanRead())
                {                    
                    string outDate = sql_grid.GetString("data_vyezda");
                    string returnDate = sql_grid.GetString("data_vozvrata");

                    DateTime outDateTime = DateTime.Now;
                    DateTime returnDateTime = DateTime.Now;

                    if (outDate != null)
                    {
                        outDateTime = DateTime.Parse(outDate);
                    }

                    if (returnDate != null)
                    {
                        returnDateTime = DateTime.Parse(returnDate);
                    }

                    String fuelGot = sql_grid.GetString("dvizhenie_topliva__polucheno");
                    String fuelOut = sql_grid.GetString("dvizhenie_topliva__vyezd");
                    String fuelReturn = sql_grid.GetString("dvizhenie_topliva__vozvrawenie");
                    try
                    {
                        fuelGot = string.Format("{0:#,###0.000}", double.Parse(fuelGot));
                        fuelOut = string.Format("{0:#,###0.000}", double.Parse(fuelOut));
                        fuelReturn = string.Format("{0:#,###0.000}", double.Parse(fuelReturn));
                    }
                    catch { }

                    int idx = dataGridView_trip.Rows.Add(
                                            sql_grid.GetInt32("gid"),
                                            outDateTime,
                                            returnDateTime,
                                            sql_grid.GetValue("pokazanija_spidometra__nachalo"),
                                            sql_grid.GetValue("pokazanija_spidometra__vozvrawenie"),
                                            fuelGot,
                                            sql_grid.GetValue("rabota_dvigatelja_pri_stojanke__ch"),
                                            sql_grid.GetValue("rabota_ustanovki__ch"),
                                            fuelOut,
                                            fuelReturn);

                    calculateRouteLenToRow(idx);
                    putCarDefaultsToRow(idx);


                }
                sql_grid.Close();
            }


            checkGridIsEditable();
            checkPLItemsIsEditable();
            updatePLandGridLabels();
            actualizeMotoGridColumnsUpdateble();
            checkIfDriverLocked();
            checkIsFuelOutOfTankCapacity();

            this.dataGridView_trip.DataError += new DataGridViewDataErrorEventHandler(this.dg_DataError);
            this.button_dgAdd.Click += new EventHandler(this.btn_dg_add_Click);
            this.dateTimePicker_dateReturnPlan.ValueChanged += new EventHandler(this.checkPlanReturnDates);
            this.dateTimePicker_timeReturnPlan.ValueChanged += new EventHandler(this.checkPlanReturnDates);
            this.dateTimePicker_dateOutPlan.ValueChanged += new EventHandler(this.checkPlanOutDates);
            this.dateTimePicker_timeOutPlan.ValueChanged += new EventHandler(this.checkPlanOutDates);
            this.comboBox_car.SelectedIndexChanged += new EventHandler(this.comboBox_car_SelectedIndexChanged);
            this.comboBox_car.TextChanged += new EventHandler(this.comboBox_car_textChanged);
            this.comboBox_driver1.SelectedIndexChanged += new EventHandler(this.comboBox_driver1_SelectedIndexChanged);
            this.comboBox_driver1.TextChanged += new EventHandler(this.comboBox_driver1_textChanged);
            this.comboBox_driver2.SelectedIndexChanged += new EventHandler(this.comboBox_driver2_SelectedIndexChanged);
            this.comboBox_driver2.TextChanged += new EventHandler(this.comboBox_driver2_textChanged);
            this.comboBox_issue.SelectedIndexChanged += new EventHandler(this.comboBox_issue_SelectedIndexChanged);
            this.comboBox_service.SelectedIndexChanged += new EventHandler(this.comboBox_service_SelectedIndexChanged);
            this.textBox_rasp.TextChanged += new EventHandler(this.textBox_rasp_TextChanged);
            this.textBox_plNum.TextChanged += new EventHandler(this.textBox_plNum_TextChanged);
            this.textBox_fuelConsFact.TextChanged += new EventHandler(this.textBox_fuelConsFact_TextChanged);
            this.textBox_fuelConsNorm.TextChanged += new EventHandler(this.textBox_fuelConsNorm_TextChanged);
            this.textBox_fuel100kmFact.TextChanged += new EventHandler(this.textBox_fuel100kmFact_TextChanged);
            this.textBox_fuel100kmPlan.TextChanged += new EventHandler(this.textBox_fuel100kmPlan_TextChanged);
            this.textBox_fuelBegin.TextChanged += new EventHandler(this.textBox_fuelBegin_TextChanged);
            this.textBox_fuelEnd.TextChanged += new EventHandler(this.textBox_fuelEnd_TextChanged);
            this.textBox_kmDiff.TextChanged += new EventHandler(this.textBox_kmDiff_TextChanged);
            this.textBox_kmEnd.TextChanged += new EventHandler(this.onOdoValueChanged);
            this.textBox_kmBegin.TextChanged += new EventHandler(this.onOdoValueChanged);
            this.button_dgDel.Click += new EventHandler(this.dg_row_deleted);
            
            this.textBox_plNum.Focus();
            isInit = false;
        }

        private void updatePLandGridLabels()
        {

            if (isMotoPlRegime)
            {
                label96.Text = "Моточасы, начало*";
                label95.Text = "Моточасы, конец";
                label97.Text = "Отработано";
                label102.Text = "Норма расхода на 1 м*час, план";
                label101.Text = "Норма расхода на 1 м*час, факт";
                dg_route_len.HeaderText = "Работа машины";
                dg_odo_begin.HeaderText = "Показания счетчика, начало";
                dg_odo_return.HeaderText = "Показания счетчика, возвращение";

                return;
            }

            label96.Text = "Километраж, начало*";
            label95.Text = "Километраж, конец";
            label97.Text = "Пройдено";
            label102.Text = "Норма расхода топлива (л/100км), план";
            label101.Text = "Норма расхода топлива (л/100км), факт";
            dg_route_len.HeaderText = "Пробег";
            dg_odo_begin.HeaderText = "Спидометр, начало";
            dg_odo_return.HeaderText = "Спидометр, возвращение";

        }

        private void actualizeMotoGridColumnsUpdateble()
        {

            if (isMotoPlRegime)
            {
                dg_motohours_stop.ReadOnly = true;
                dg_equip_motohours.ReadOnly = true;

                return;
            }
            dg_motohours_stop.ReadOnly = false;
            dg_equip_motohours.ReadOnly = false;
        }

        private void comboBox_driver1_DrawItem(object sender, DrawItemEventArgs e)
        {
            myItem item = (myItem)comboBox_driver1.Items[e.Index];
            drawItemManager(e, item);
        }

        private void comboBox_driver2_DrawItem(object sender, DrawItemEventArgs e)
        {
            myItem item = (myItem)comboBox_driver2.Items[e.Index];
            drawItemManager(e, item);
        }

        private void drawItemManager(DrawItemEventArgs e, myItem drawItem)
        {
            Font font = comboBox_driver1.Font;
            Brush brush = Brushes.Black;

            if (drawItem.GetOrder < 0)
            {
                MessageBox.Show("Выделяем в БОЛД " + drawItem.Name, "ИНФО");
                font = new Font(font, FontStyle.Bold);
            }

            e.Graphics.DrawString(drawItem.Name, font, brush, e.Bounds);
        }

        private bool save_PL()
        {
            if (!validateDataGrid() || !validatePLForm())
            {
                return false;
            }

            if (checkPLNumberExist())
            {
                return false;
            }

            List<string> command_list = new List<string>();

            var upload_pl = MainPluginClass.App.SqlWork();
            upload_pl.BeginTransaction();
            try
            {

                // что обновляем
                string temp1 = "";

                // значение
                string temp2 = "";

                #region Получение всех свойств ПЛ и формирования строк набора полей

                // номер путевого листа
                if (!string.IsNullOrEmpty(textBox_plNum.Text))
                {
                    temp1 += "nomer_putevogo_lista, ";
                    temp2 += "'" + textBox_plNum.Text + (!string.IsNullOrEmpty(textBox_userDept.Text) ? "/" + textBox_userDept.Text : "") + "', ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "nomer_putevogo_lista");
                }


                // даты выезда/возвращения по плану/факту
                temp1 += "data_vyezda__plan, data_vozvrawenija__plan, ";
                temp2 += "'" + dateTimePicker_dateOutPlan.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker_timeOutPlan.Value.ToString("HH:mm:00") + "', " +
                    "'" + dateTimePicker_dateReturnPlan.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker_timeReturnPlan.Value.ToString("HH:mm:00") + "', ";

                if (factDateOutInitialized)
                {
                    temp1 += "data_vyezda__fakt, ";
                    temp2 += "'" + dateTimePicker_dateOutFact.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker_timeOutFact.Value.ToString("HH:mm:00") + "', ";
                }

                if (factDateReturnInitialized)
                {
                    temp1 += "data_vozvrawenija__fakt, ";
                    temp2 += "'" + dateTimePicker_dateReturnFact.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker_timeReturnFact.Value.ToString("HH:mm:00") + "', ";
                }

                // водитель 1
                if (comboBox_driver1.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_driver1.SelectedItem;
                    if (item.GetId > 0)
                    {
                        temp1 += "voditel, ";
                        temp2 += item.GetId + ", ";

                        if (!string.IsNullOrEmpty(textBox_udos1.Text))
                        {
                            temp1 += "udostoverenie__, ";
                            temp2 += "'" + textBox_udos1.Text + "', ";
                        }
                    }
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "voditel");
                    nullValue(ref temp1, ref temp2, "udostoverenie__");
                }

                // водитель 2
                if (comboBox_driver2.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_driver2.SelectedItem;
                    if (item.GetId > 0)
                    {
                        temp1 += "voditel__vyezd, ";
                        temp2 += item.GetId + ", ";

                        if (!string.IsNullOrEmpty(textBox_udos2.Text))
                        {
                            temp1 += "udostoverenie____2, ";
                            temp2 += "'" + textBox_udos2.Text + "', ";
                        }
                    }
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "voditel__vyezd");
                    nullValue(ref temp1, ref temp2, "udostoverenie____2");
                }

                // километраж, начало
                if (!string.IsNullOrEmpty(textBox_kmBegin.Text))
                {
                    temp1 += "kilometrazh__nachalo, ";
                    temp2 += textBox_kmBegin.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "kilometrazh__nachalo");
                }

                // километраж, конец
                if (!string.IsNullOrEmpty(textBox_kmEnd.Text))
                {
                    temp1 += "kilometrazh__projdeno, ";
                    temp2 += textBox_kmEnd.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "kilometrazh__projdeno");
                }

                // пройдено, км
                if (!string.IsNullOrEmpty(textBox_kmDiff.Text))
                {
                    temp1 += "projdeno__km, ";
                    temp2 += textBox_kmDiff.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "projdeno__km");
                }

                // топливо, начало
                if (!string.IsNullOrEmpty(textBox_fuelBegin.Text))
                {
                    temp1 += "toplivo_1__nachalo, ";
                    temp2 += textBox_fuelBegin.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "toplivo_1__nachalo");
                }

                // топливо, конец
                if (!string.IsNullOrEmpty(textBox_fuelEnd.Text))
                {
                    temp1 += "toplivo_1__konec, ";
                    temp2 += textBox_fuelEnd.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "toplivo_1__konec");
                }

                // расход по норме
                if (!string.IsNullOrEmpty(textBox_fuelConsNorm.Text))
                {
                    temp1 += "rashod_po_norme, ";
                    temp2 += textBox_fuelConsNorm.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "rashod_po_norme");
                }

                // расход, факт
                if (!string.IsNullOrEmpty(textBox_fuelConsFact.Text))
                {
                    temp1 += "rashod__fakt, ";
                    temp2 += textBox_fuelConsFact.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "rashod__fakt");
                }

                // норма расхода на 100 км, план
                if (!string.IsNullOrEmpty(textBox_fuel100kmPlan.Text))
                {
                    temp1 += "norma_rashoda_na_100_km__plan, ";
                    temp2 += textBox_fuel100kmPlan.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "norma_rashoda_na_100_km__plan");
                }

                // норма расхода на 100 км, факт
                if (!string.IsNullOrEmpty(textBox_fuel100kmFact.Text))
                {
                    temp1 += "norma_rashoda_na_100_km__fakt, ";
                    temp2 += textBox_fuel100kmFact.Text.Replace(",", ".") + ", ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "norma_rashoda_na_100_km__fakt");
                }

                // транспортное средство: марка, тип ТС, гос. номер, гар. номер
                if (comboBox_car.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_car.SelectedItem;

                    if (item.GetId > 0)
                    {
                        temp1 += "transportnoe_sredstvo, ";
                        temp2 += item.GetId + ", ";

                        CarItem carItem = (CarItem)item.Data;

                        if (carItem.getCarTypeId > 0)
                        {
                            temp1 += "tip_ts, ";
                            temp2 += carItem.getCarTypeId + ", ";
                        }

                        if (!string.IsNullOrEmpty(carItem.getGosNum))
                        {
                            temp1 += "gos__, ";
                            temp2 += "'" + carItem.getGosNum + "', ";
                        }

                        if (!string.IsNullOrEmpty(carItem.getGarNum))
                        {
                            temp1 += "gar___, ";
                            temp2 += "'" + carItem.getGarNum + "', ";
                        }

                        if (carItem.getDeptId > 0)
                        {
                            temp1 += "organizacija1, ";
                            temp2 += carItem.getDeptId + ", ";
                        }
                        else
                        {
                            nullValue(ref temp1, ref temp2, "organizacija1");
                        }

                    }
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "transportnoe_sredstvo");
                    nullValue(ref temp1, ref temp2, "tip_ts");
                    nullValue(ref temp1, ref temp2, "gos__");
                    nullValue(ref temp1, ref temp2, "gar___");

                    // если у путевого листа не задано ТС, то использовать в качестве района закрепления район пользователя
                    if (resID > 0)
                    {
                        temp1 += "organizacija1, ";
                        temp2 += resID + ", ";
                    }
                    else
                    {
                        nullValue(ref temp1, ref temp2, "organizacija1");
                    }

                }

                // заявка
                if (!isRES && comboBox_issue.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_issue.SelectedItem;
                    if (item.GetId > 0)
                    {
                        temp1 += "zajavka, ";
                        temp2 += item.GetId + ", ";
                    }
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "zajavka");
                }

                // в распоряжение

                // если считали из плана использования ТС, то обновляем и службу, и в чье распоряжение
                if (carService > 0 && !string.IsNullOrEmpty(carOwner))
                {
                    temp1 += "v_rasporjazhenie__sluzhba_, ";
                    temp2 += carService + ", ";

                    temp1 += "v_rasporjazhenie, ";
                    temp2 += "'" + carOwner + "', ";
                }
                else
                {
                    bool isComplex = false;

                    if (comboBox_car.SelectedItem != null)
                    {
                        myItem item = (myItem)comboBox_car.SelectedItem;
                        isComplex = ((CarItem)item.Data).getIsComplex;
                    }

                    // Если пользователь принадлежит РЭС (это смотрим в таблице "Настройки|Пользователи", в колонке "Район-закрепления".
                    if (!isComplex)
                    {
                        if (comboBox_service.SelectedItem != null)
                        {
                            myItem item = (myItem)comboBox_service.SelectedItem;
                            if (item.GetId > 0)
                            {
                                temp1 += "v_rasporjazhenie__sluzhba_, ";
                                temp2 += item.GetId + ", ";
                            }
                        }
                        else
                        {
                            nullValue(ref temp1, ref temp2, "v_rasporjazhenie__sluzhba_");
                        }

                        nullValue(ref temp1, ref temp2, "v_rasporjazhenie");

                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(textBox_rasp.Text))
                        {
                            temp1 += "v_rasporjazhenie, ";
                            temp2 += "'" + textBox_rasp.Text + "', ";
                        }
                        else
                        {
                            nullValue(ref temp1, ref temp2, "v_rasporjazhenie");
                        }

                        nullValue(ref temp1, ref temp2, "v_rasporjazhenie__sluzhba_");
                    }
                }

                // куда следует
                if (!string.IsNullOrEmpty(textBox_issDest.Text))
                {
                    temp1 += "adres_podachi, ";
                    temp2 += "'" + textBox_issDest.Text + "', ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "adres_podachi");
                }

                // откуда следует
                if (!string.IsNullOrEmpty(textBox_issSource.Text))
                {
                    temp1 += "otkuda_sleduet, ";
                    temp2 += "'" + textBox_issSource.Text + "', ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "otkuda_sleduet");
                }

                // вид работы
                if (!string.IsNullOrEmpty(textBox_issType.Text))
                {
                    temp1 += "zadanie, ";
                    temp2 += "'" + textBox_issType.Text + "', ";
                }
                else
                {
                    nullValue(ref temp1, ref temp2, "zadanie");
                }

                #endregion

                // добавление метки того, что данные отправлены из специальной (этой) формы
                temp1 += "is_special_form, ";
                temp2 += "1, ";


                temp1 = temp1.Trim(new char[] { ' ', ',' });
                temp2 = temp2.Trim(new char[] { ' ', ',' });

                int plIdForTrip;

                // обновление путевого листа
                if (pl_id != null)
                {
                    command_list.Add("UPDATE autobase._umjets_putevye_listy SET (" + temp1 + ") = (" + temp2 + ") where gid = " + pl_id.ToString());

                    plIdForTrip = pl_id.Value;

                    // очистка удаленных поездок
                    foreach (int tripId in tripToDelete)
                    {
                        string deleteTripSQL;
                        deleteTripSQL = "DELETE FROM autobase.umjets_poezdki WHERE gid = " + tripId;
                        command_list.Add(deleteTripSQL);
                    }

                }
                // создание путевого листа
                else
                {
                    String createPL = "INSERT INTO autobase._umjets_putevye_listy (" + temp1 + ") VALUES (" + temp2 + ") RETURNING gid;";

                    upload_pl.sql = createPL;
                    plIdForTrip = (int)upload_pl.ExecuteScalar();

                    pl_id = plIdForTrip;

                    if (isRES && issStartNum > 0)
                    {
                        string startNumSQL = "UPDATE autobase.umjets_rajon_zakreplenie urk SET nachalnye_nomera_putevogo_lista = nachalnye_nomera_putevogo_lista + 1 " +
                            "FROM autobase.users usr WHERE usr.rajon_zakreplenija = urk.gid AND usr.user_id = autobase.get_user_id();";
                        command_list.Add(startNumSQL);
                    }

                }

                #region Создание/обновление поездок
                // создание/обновление поездок
                foreach (DataGridViewRow row in dataGridView_trip.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        String argNameListTrip = "";
                        String argValueListTrip = "";


                        Object outCell = row.Cells["dg_date_out"].Value;
                        Object returnCell = row.Cells["dg_date_return"].Value;
                        if (outCell != null && returnCell != null)
                        {
                            try
                            {
                                DateTime outDateTime = (DateTime)outCell;
                                DateTime returnDateTime = (DateTime)returnCell;

                                argNameListTrip += "data_vyezda, data_vozvrata, ";
                                argValueListTrip += "'" + outDateTime.ToString("yyyy-MM-dd") + " " + outDateTime.ToString("HH:mm:00") + "', " +
                                    "'" + returnDateTime.ToString("yyyy-MM-dd") + " " + returnDateTime.ToString("HH:mm:00") + "', ";
                            }
                            catch
                            {
                                MessageBox.Show("Неверный формат даты выезда/возвращения.", "Ошибка при сохранении");
                                return false;
                            }

                        }


                        // спидометр, начало
                        if (row.Cells["dg_odo_begin"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_odo_begin"].Value.ToString()))
                        {
                            argNameListTrip += "pokazanija_spidometra__nachalo, ";
                            argValueListTrip += row.Cells["dg_odo_begin"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "pokazanija_spidometra__nachalo");
                        }

                        // спидометр, возвращение
                        if (row.Cells["dg_odo_return"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_odo_return"].Value.ToString()))
                        {
                            argNameListTrip += "pokazanija_spidometra__vozvrawenie, ";
                            argValueListTrip += row.Cells["dg_odo_return"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "pokazanija_spidometra__vozvrawenie");
                        }

                        // движение топлива, получено
                        if (row.Cells["dg_fuel_got"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_fuel_got"].Value.ToString()))
                        {
                            argNameListTrip += "dvizhenie_topliva__polucheno, ";
                            argValueListTrip += row.Cells["dg_fuel_got"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "dvizhenie_topliva__polucheno");
                        }

                        // работа двигателя при стоянке
                        if (row.Cells["dg_motohours_stop"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_motohours_stop"].Value.ToString()))
                        {
                            argNameListTrip += "rabota_dvigatelja_pri_stojanke__ch, ";
                            argValueListTrip += row.Cells["dg_motohours_stop"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "rabota_dvigatelja_pri_stojanke__ch");
                        }

                        // работа установки
                        if (row.Cells["dg_equip_motohours"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_equip_motohours"].Value.ToString()))
                        {
                            argNameListTrip += "rabota_ustanovki__ch, ";
                            argValueListTrip += row.Cells["dg_equip_motohours"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "rabota_ustanovki__ch");
                        }

                        // топливо при выезде
                        if (row.Cells["dg_fuel_out"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_fuel_out"].Value.ToString()))
                        {
                            argNameListTrip += "dvizhenie_topliva__vyezd, ";
                            argValueListTrip += row.Cells["dg_fuel_out"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "dvizhenie_topliva__vyezd");
                        }

                        // топливо при возвращении
                        if (row.Cells["dg_fuel_return"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["dg_fuel_return"].Value.ToString()))
                        {
                            argNameListTrip += "dvizhenie_topliva__vozvrawenie, ";
                            argValueListTrip += row.Cells["dg_fuel_return"].Value.ToString().Replace(",", ".") + ", ";
                        }
                        else
                        {
                            nullValue(ref argNameListTrip, ref argValueListTrip, "dvizhenie_topliva__vozvrawenie");
                        }

                        // добавление метки того, что данные отправлены из специальной (этой) формы
                        argNameListTrip += "is_special_form, ";
                        argValueListTrip += "1, ";

                        // поездка уже создана в БД
                        if (row.Cells["dg_trip_gid"].Value != null && (int)row.Cells["dg_trip_gid"].Value > 0)
                        {
                            argNameListTrip = argNameListTrip.Trim(new char[] { ' ', ',' });
                            argValueListTrip = argValueListTrip.Trim(new char[] { ' ', ',' });
                            command_list.Add("UPDATE autobase.umjets_poezdki SET (" + argNameListTrip + ") = (" + argValueListTrip + ") where gid = " + (int)row.Cells["dg_trip_gid"].Value + ";");

                        }
                        // новая поездка
                        else
                        {
                            argNameListTrip += "putevoj_list, ";
                            argValueListTrip += plIdForTrip + ", ";

                            // из-за DO INSTEAD NOTHING правил возпользоваться RETURNING не получается.
                            // Придется gid доставать отдельно.
                            String getTripIdSQL = "SELECT nextval('autobase.umjets_poezdki_gid_seq') as nextval;";
                            int tripId = 0;
                            using (var sqlCmd = MainPluginClass.App.SqlWork())
                            {
                                sqlCmd.sql = getTripIdSQL;
                                sqlCmd.ExecuteReader();
                                if (sqlCmd.CanRead())
                                {
                                    tripId = sqlCmd.GetInt32("nextval");
                                }
                            }

                            if (tripId > 0)
                            {
                                argNameListTrip += "gid";
                                argValueListTrip += tripId;

                                String createTrip = "INSERT INTO autobase.umjets_poezdki (" + argNameListTrip + ") VALUES (" + argValueListTrip + ");";

                                // сохранение поездки сразу в БД
                                upload_pl.sql = createTrip;
                                upload_pl.ExecuteNonQuery();

                                // обновление id текущей строки, чтобы избежать дублирование поездок
                                row.Cells["dg_trip_gid"].Value = tripId;
                            }
                        }
                    }
                }
                #endregion

                foreach (string par in command_list)
                {
                    upload_pl.sql = par;
                    upload_pl.ExecuteNonQuery();
                }

                upload_pl.EndTransaction();
                clearClose = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении: " + ex.Message, "Ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {

                upload_pl.EndTransaction();
                upload_pl.Close();
            }
        }

        private void btn_save_click(object sender, EventArgs e)
        {
            if (save_PL())
            {
                // Закрывать ли окно после сохранения
                string saveType = "";
                if (sender is MenuItem)
                {
                    saveType = (string)((MenuItem)sender).Tag;
                }
                else if (sender is SplitButton && splitButton_save.Tag != null)
                {
                    saveType = (string)splitButton_save.Tag;
                }
                
                if (saveType.Equals("SAVE&CLOSE"))
                {
                    try
                    {
                        this.ParentForm.Close();
                    }
                    catch (Exception x) { MessageBox.Show("Ошибка! Невозможно закрыть путевой лист: " + x.Message, "Ошибка путевого листа!"); }
                }
            }
        }
        
        private void btn_print_click(object sender, EventArgs e)
        {
            try
            {
                if (this.pl_id != null)
                {
                    var list = MainPluginClass.Work.FastReport.FindReportsByIdTable(MapEditorTablePutList).ToArray();
                    MainPluginClass.Work.FastReport.OpenReport(list[0], new FilterTable(this.pl_id.Value, MapEditorTablePutList, " gid", ""));
                }
                else
                {
                    MessageBox.Show("Сохраните путевой лист!", "Ошибка сохранения!");
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Возможно не назначена печатная форма путевого листа!\r\n" + x.Message, "Ошибка печати!");
            }
        }

        private void comboBox_driver1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (carUpdateInProcess == false && driverChangedAutomatically == true)
            {
                driverChangedAutomatically = false;
            }

            filterDriver1Combobox = false;

            clearClose = false;
            int? driver1 = null;
            if (comboBox_driver1.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_driver1.SelectedItem;
                if (comboBox_driver2.SelectedItem != null)
                {
                    myItem item2 = (myItem)comboBox_driver2.SelectedItem;
                    if (item.GetId == item2.GetId)
                    {
                        MessageBox.Show("Выбор одного и того же водителя на разные смены недопустим", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        comboBox_driver1.SelectedIndex = prevDriver1Idx;
                        return;
                    }
                }
                textBox_udos1.Text = ((DriverItem)item.Data).getDriverCard;
                driver1 = item.GetId;
            }
            updatePLDateOut();
            updatePLDateReturn();
            checkGridIsEditable();


            this.comboBox_driver1.SelectedIndexChanged -= new EventHandler(this.comboBox_driver1_SelectedIndexChanged);
            IOrderedEnumerable<myItem> orderedList = driversList
                .OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);
            comboBox_driver1.Items.Clear();
            foreach (var listItem in orderedList)
            {
                int idx = comboBox_driver1.Items.Add(listItem);
                if (driver1 != null && driver1.Value == listItem.GetId)
                {
                    comboBox_driver1.SelectedIndex = idx;
                    prevDriver1Idx = idx;
                }
            }
            this.comboBox_driver1.SelectedIndexChanged += new EventHandler(this.comboBox_driver1_SelectedIndexChanged);

            filterDriver1Combobox = true;
        }

        private void comboBox_driver2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (carUpdateInProcess == false && driverChangedAutomatically == true)
            {
                driverChangedAutomatically = false;
            }

            filterDriver2Combobox = false;
            clearClose = false;
            int? driver2 = null;
            if (comboBox_driver2.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_driver2.SelectedItem;

                if (comboBox_driver1.SelectedItem != null)
                {
                    myItem item2 = (myItem)comboBox_driver1.SelectedItem;
                    if (item.GetId == item2.GetId)
                    {
                        MessageBox.Show("Выбор одного и того же водителя на разные смены недопустим", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        comboBox_driver2.SelectedIndex = prevDriver2Idx;
                        return;
                    }
                }

                textBox_udos2.Text = ((DriverItem)item.Data).getDriverCard;
                driver2 = item.GetId;
            }

            this.comboBox_driver2.SelectedIndexChanged -= new EventHandler(this.comboBox_driver2_SelectedIndexChanged);
            IOrderedEnumerable<myItem> orderedList = driversList
                .OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);
            comboBox_driver2.Items.Clear();
            foreach (var listItem in orderedList)
            {
                int idx = comboBox_driver2.Items.Add(listItem);
                if (driver2 != null && driver2.Value == listItem.GetId)
                {
                    comboBox_driver2.SelectedIndex = idx;
                    prevDriver2Idx = idx;
                }
            }
            this.comboBox_driver2.SelectedIndexChanged += new EventHandler(this.comboBox_driver2_SelectedIndexChanged);

            filterDriver2Combobox = true;
        }
        
        void nullValue(ref string argumentNameList, ref string argumentValueList, string argumentName)
        {
            argumentNameList += argumentName + ", ";
            argumentValueList += "null, ";
        }

        bool checkPlanDates(bool isSilent)
        {
            DateTime datePlanOut = dateTimePicker_dateOutPlan.Value.Date + dateTimePicker_timeOutPlan.Value.TimeOfDay;
            DateTime datePlanReturn = dateTimePicker_dateReturnPlan.Value.Date + dateTimePicker_timeReturnPlan.Value.TimeOfDay;


            if (DateTime.Compare(datePlanReturn, datePlanOut) < 0)
            {
                if (!isSilent)
                {
                    MessageBox.Show("Дата возвращения не может быть меньше даты выезда " + datePlanOut.ToString() + " " + datePlanReturn.ToString(), "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            return true;

        }

        void checkPlanOutDates(object sender, EventArgs e)
        {
            clearClose = false;
            checkPlanDates(false);
            updateStartOdoAndFuel();
            checkGridIsEditable();
        }

        void checkPlanReturnDates(object sender, EventArgs e)
        {
            clearClose = false;
            checkPlanDates(false);
            checkGridIsEditable();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.clearClose &&
                    MessageBox.Show("Все несохраненные данные будут потеряны. Вы уверены, что хотите закрыть окно ?",
                               "Подтверждение",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        bool checkDataGridRow(int dgIdx, bool isSilent)
        {
            if (dataGridView_trip.Rows.Count > dgIdx && dgIdx >= 0)
            {
                if (dataGridView_trip.Rows[dgIdx].Cells["dg_odo_return"].Value == null ||
                    dataGridView_trip.Rows[dgIdx].Cells["dg_date_return"].Value == null)
                {
                    if (!isSilent)
                    {
                        MessageBox.Show("В поездке поля 'Спидометр, конец' и 'Дата возврата' обязательны для заполения!", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }

                if (dataGridView_trip.Rows[dgIdx].Cells["dg_fuel_return"].Style.BackColor == Color.Red)
                {
                    if (!isSilent)
                    {
                        MessageBox.Show("Некорректное значение уровня топлива в поездке!", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }
            }
            return true;
        }

        bool checkPLNumberExist()
        {
            if (!string.IsNullOrEmpty(textBox_plNum.Text))
            {
                String plNumber = textBox_plNum.Text + (!string.IsNullOrEmpty(textBox_userDept.Text) ? "/" + textBox_userDept.Text : "");
                int plId = pl_id.HasValue ? pl_id.Value :0;
                DateTime dateOut = dateTimePicker_dateOutPlan.Value;
                String checkPLNumber =
                        "WITH date_period AS ( " +
                          "SELECT date_trunc('year', @DateOut) as year_begin, date_trunc('year', @DateOut) + '1 year'::interval as year_end " +
                        ") " +
                        "SELECT * " +
                        "FROM autobase._umjets_putevye_listy_base upl " +
                          "JOIN date_period dp ON (upl.data_vyezda__plan >= dp.year_begin AND upl.data_vyezda__plan < dp.year_end) " +
                        "WHERE upl.nomer_putevogo_lista = @PlNum AND upl.gid <> @PlId";

                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = checkPLNumber;
                    sqlCmd.AddParam("DateOut", dateOut, DbType.DateTime);
                    sqlCmd.AddParam("PlNum", plNumber, DbType.String);
                    sqlCmd.AddParam("PlId", plId, DbType.Int32);
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        MessageBox.Show("Путевой лист с таким номером уже существует.", "Ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
            }
            return false;
        }

        bool checkPreviousPLTripExists()
        {
            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;

                int carId = item.GetId;
                DateTime dateOut = dateTimePicker_dateOutPlan.Value;

                
                String checkPLTrip =    "SELECT upl.gid, upl.nomer_putevogo_lista, upl.data_vyezda__plan, count(up.gid) as trip_cnt " +
                                        "FROM autobase._umjets_putevye_listy upl " +
                                            "LEFT JOIN autobase. umjets_poezdki up ON upl.gid = up.putevoj_list " +
                                        "WHERE upl.data_vyezda__plan <= @DateOut AND upl.transportnoe_sredstvo = @CarId AND upl.gid <> @PlId " +
                                        "GROUP BY upl.gid, upl.nomer_putevogo_lista, upl.data_vyezda__plan " +
                                        "ORDER BY upl.data_vyezda__plan DESC " +
                                        "LIMIT 1";
                    
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    sqlCmd.sql = checkPLTrip;
                    sqlCmd.AddParam("DateOut", dateOut, DbType.DateTime);
                    sqlCmd.AddParam("CarId", carId, DbType.Int32);
                    sqlCmd.AddParam("PlId", pl_id != null ? pl_id.Value : 0, DbType.Int32);
                    sqlCmd.ExecuteReader();
                    if (sqlCmd.CanRead())
                    {
                        if (sqlCmd.GetInt32("trip_cnt") < 1)
                        {
                            MessageBox.Show(String.Format("По предыдущему путевому листу №{0} не указаны поездки.", sqlCmd.GetString("nomer_putevogo_lista")), 
                                "Невозможно провести путевой лист", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        void dg_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void showInvalidFormatMessage(string failedFields)
        {
            MessageBox.Show(
                "Неверный формат введенного значения. Значение должно быть числом! Поля с ошибками: " + failedFields, 
                "Ошибка при редактировании");
        }

        private void comboBox_car_SelectedIndexChanged(object sender, EventArgs e)
        {
            carUpdateInProcess = true;

            filterCarCombobox = false;
            clearClose = false;
            int? car = null;
            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                car = item.GetId;
                textBox_carType.Text = ((CarItem)item.Data).getCarType;
                textBox_gosNum.Text = ((CarItem)item.Data).getGosNum;
                textBox_garNum.Text = ((CarItem)item.Data).getGarNum;
                textBox_regNum.Text = ((CarItem)item.Data).getRegNum;

                float carNorm = ((CarItem)item.Data).getCarNorm;
                float carEquipNorm = ((CarItem)item.Data).getCarEquipNorm;
                float car100kmNorm = ((CarItem)item.Data).getCar100kmNorm;

                isMotoPlRegime = ((CarItem)item.Data).getMotoPlRegime;

                textBox_fuel100kmPlan.Text = car100kmNorm.ToString();
                formatTextBox(textBox_fuel100kmPlan);
                textBox_fuel1hPlan.Text = carNorm.ToString();
                formatTextBox(textBox_fuel1hPlan);
                textBox_fuelEquipPlan.Text = carEquipNorm.ToString();
                formatTextBox(textBox_fuelEquipPlan);

                // обновление нормы в поездках
                foreach (DataGridViewRow row in dataGridView_trip.Rows)
                {
                    row.Cells["dg_car_norm"].Value = carNorm;
                    row.Cells["dg_car_equip_norm"].Value = carEquipNorm;
                }

                updateStartOdoAndFuel();
                updatePLDateReturn();
            }
            checkIfDriverLocked();
            checkGridIsEditable();
            checkIsFuelOutOfTankCapacity();
            refresh_issue_block();
            updateCarServiceAndOwner();
            updatePLandGridLabels();
            actualizeMotoGridColumnsUpdateble();

            if (driverChangedAutomatically)
            {
                reorderDriverList(null, null);
            }

            this.comboBox_car.SelectedIndexChanged -= new EventHandler(this.comboBox_car_SelectedIndexChanged);
            IOrderedEnumerable<myItem> orderedList = carsList
                .OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);
            comboBox_car.Items.Clear();
            foreach (var listItem in orderedList)
            {
                int idx = comboBox_car.Items.Add(listItem);
                if (car != null && car.Value == listItem.GetId)
                {
                    comboBox_car.SelectedIndex = idx;
                }
            }
            this.comboBox_car.SelectedIndexChanged += new EventHandler(this.comboBox_car_SelectedIndexChanged);
            filterCarCombobox = true;
            carUpdateInProcess = false;
        }

        private void comboBox_issue_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearClose = false;
            if (comboBox_issue.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_issue.SelectedItem;

                textBox_rasp.Text = ((IssueItem)item.Data).getOwner;
                textBox_issDest.Text = ((IssueItem)item.Data).getTo;
                textBox_issSource.Text = ((IssueItem)item.Data).getFrom;
                textBox_issType.Text = ((IssueItem)item.Data).getType;
            }
            refresh_issue_block();
        }

        private void comboBox_service_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearClose = false;
            if (comboBox_service.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_service.SelectedItem;

                if (carService != 0 && item.GetId != carService)
                {
                    carService = 0;
                }
            }
        }

        private void textBox_rasp_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;
            if (!string.IsNullOrEmpty(carOwner) && string.Compare(textBox_rasp.Text, carOwner) != 0)
            {
                carOwner = string.Empty;
            }
        }

        private void updateCarServiceAndOwner()
        {
            clearClose = false;
            if (comboBox_car.SelectedItem != null)
            {
                CarItem item = (CarItem)((myItem)comboBox_car.SelectedItem).Data;

                carService = item.getService;
                if (carService > 0)
                {
                    for (int i = 0; i < comboBox_service.Items.Count; i++)
                    {
                        if (((myItem)comboBox_service.Items[i]).GetId == carService)
                        {
                            comboBox_service.SelectedIndex = i;
                            break;
                        }
                    } 
                }

                carOwner = item.getOwner;
                if (!string.IsNullOrEmpty(carOwner))
                {
                    textBox_rasp.Text = carOwner;
                }
            }
        }

        private void checkIfDriverLocked()
        {
            if (comboBox_car.SelectedItem != null)
            {
                groupBox18.Enabled = true;
            }
            else
            {
                groupBox18.Enabled = false;
            }
        }

        private void checkIsFuelOutOfTankCapacity()
        {
            float fuelReturn = string.IsNullOrEmpty(textBox_fuelEnd.Text) ? 0 : float.Parse(textBox_fuelEnd.Text);
            float fuelOut = string.IsNullOrEmpty(textBox_fuelBegin.Text) ? 0 : float.Parse(textBox_fuelBegin.Text);

            if (isOutOfTankCapacity(fuelOut))
            {
                textBox_fuelBegin.BackColor = Color.Red;
            }
            else
            {
                textBox_fuelBegin.BackColor = Color.White;
            }
            if (isOutOfTankCapacity(fuelReturn))
            {
                textBox_fuelEnd.BackColor = Color.Red;
            }
            else
            {
                textBox_fuelEnd.BackColor = Color.White;
            }

        }

        private void updateStartOdoAndFuel()
        {
            clearClose = false;
            textBox_kmBegin.Text = string.Empty;
            textBox_fuelBegin.Text = string.Empty;
            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                int carId = item.GetId;
                if (carId > 0)
                {
                    string plDate = "'" + dateTimePicker_dateOutPlan.Value.ToString("yyyy-MM-dd") + " "
                        + dateTimePicker_timeOutPlan.Value.ToString("HH:mm:00") + "'";

                    // Обновление значения начала километража и значения уровня топлива путевого листа
                    String prevPl = 
                        "SELECT kilometrazh__projdeno, toplivo_1__konec " +
                        "FROM autobase._umjets_putevye_listy upl " +
                        "WHERE upl.transportnoe_sredstvo = " + carId + " " +
                            "AND upl.data_vyezda__plan < " + plDate + "::timestamp with time zone + '1 minute'::interval " +
                        "ORDER BY upl.data_vyezda__plan DESC " +
                        "LIMIT 1";
                    using (var sqlCmd = MainPluginClass.App.SqlWork())
                    {
                        sqlCmd.sql = prevPl;
                        sqlCmd.ExecuteReader();
                        if (sqlCmd.CanRead())
                        {
                            if (sqlCmd.GetValue("kilometrazh__projdeno") != null)
                            {
                                textBox_kmBegin.Text = sqlCmd.GetValue("kilometrazh__projdeno").ToString();
                            }
                            if (sqlCmd.GetValue("toplivo_1__konec") != null)
                            {
                                textBox_fuelBegin.Text = sqlCmd.GetValue("toplivo_1__konec").ToString();
                            }
                        }
                    }
                }
            }
            formatTextBox(textBox_fuelBegin);
        }

        private void reorderDriverList(int? driver1, int? driver2)
        {
            filterDriver1Combobox = false;
            filterDriver2Combobox = false;
            clearClose = isInit;

            // сброс сортировки списка
            driversList.ForEach(delegate (myItem listItem)
            {
                listItem.SetOrder = 1;
            });


            HashSet<int> foundDrivers = new HashSet<int>();

            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                int carId = item.GetId;
                if (carId > 0)
                {
                    String sqlText =
                        "SELECT DISTINCT upv.voditel " +
                        "FROM autobase.umjets_plan_ispolzovanija_ts upit " +
                          "JOIN autobase.umjets_avtopark ua ON(upit.transportnoe_sredstvo = ua.gid and ua.gid = " + carId + ") " +
                          "JOIN autobase.umjets_privjazka_voditelej upv ON(upit.gid = upv.ts_po_planu)";

                    using (var sqlCmd = MainPluginClass.App.SqlWork())
                    {
                        sqlCmd.sql = sqlText;
                        sqlCmd.ExecuteReader();
                        while (sqlCmd.CanRead())
                        {
                            foundDrivers.Add(sqlCmd.GetInt32("voditel"));
                        }

                        for (int i = 0; i < driversList.Count; i++)
                        {
                            if (foundDrivers.Contains(driversList[i].GetId))
                            {
                                // установка у найденной строки высокого приоритета
                                driversList[i].SetOrder = 0;
                            }
                        }
                    }
                }
            }

            // сортировка по полю order и имени
            IOrderedEnumerable<myItem> orderedList = driversList.OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);

            
            comboBox_driver1.Items.Clear();
            comboBox_driver2.Items.Clear();
            if (!isInit)
            {
                textBox_udos1.Text = string.Empty;
                textBox_udos2.Text = string.Empty;
            }

            // заполнение списков водителей в порядке сортировки
            foreach (var listItem in orderedList)
            {
                int idx1 = comboBox_driver1.Items.Add(listItem);
                int idx2 = comboBox_driver2.Items.Add(listItem);

                if (driver1 != null && driver1.Value == listItem.GetId)
                {
                    comboBox_driver1.SelectedIndex = idx1;
                    prevDriver1Idx = idx1;
                }
                if (driver2 != null && driver2.Value == listItem.GetId)
                {
                    comboBox_driver2.SelectedIndex = idx2;
                    prevDriver2Idx = idx2;
                }
            }

            if (driver1 == null)
            {
                comboBox_driver1.SelectedIndex = -1;
                prevDriver1Idx = -1;
                comboBox_driver1.ResetText();
                updatePLDateOut();
            }
            if (driver2 == null)
            {
                comboBox_driver2.SelectedIndex = -1;
                prevDriver2Idx = -1;
                comboBox_driver2.ResetText();
            }

            // если найденная запись одна, то установка ее в качестве первого водителя
            if (driver1 == null && foundDrivers.Count == 1 && comboBox_driver1.Items.Count > 0)
            {
                comboBox_driver1.SelectedIndex = 0;
                prevDriver1Idx = 0;
                textBox_udos1.Text = ((DriverItem)((myItem)comboBox_driver1.Items[0]).Data).getDriverCard;
            }

            filterDriver1Combobox = true;
            filterDriver2Combobox = true;
        }

        private void comboBox_driver1_textChanged(Object sender, EventArgs e)
        {
            if (filterDriver1Combobox && comboBox_driver1.SelectedIndex == -1)
            {
                filterDriver1Combobox = false;
                string item = comboBox_driver1.Text;
                int cursorPos = comboBox_driver1.SelectionStart;
                item = item.ToLower();
                IOrderedEnumerable<myItem> orderedList = driversList
                    .Where(listItem => String.IsNullOrEmpty(listItem.Name) ? false : listItem.Name.ToLower().Contains(item))
                    .OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);

                comboBox_driver1.Items.Clear();
                comboBox_driver1.Items.AddRange(orderedList.ToArray());

                comboBox_driver1.SelectionStart = cursorPos;
                filterDriver1Combobox = true;
            }
        }

        private void comboBox_driver2_textChanged(Object sender, EventArgs e)
        {
            if (filterDriver2Combobox && comboBox_driver2.SelectedIndex == -1)
            {
                filterDriver2Combobox = false;
                string item = comboBox_driver2.Text;
                int cursorPos = comboBox_driver2.SelectionStart;
                item = item.ToLower();
                IOrderedEnumerable<myItem> orderedList = driversList
                    .Where(listItem => String.IsNullOrEmpty(listItem.Name) ? false : listItem.Name.ToLower().Contains(item))
                    .OrderBy(listItem => listItem.GetOrder).ThenBy(listItem => listItem.Name);

                comboBox_driver2.Items.Clear();
                comboBox_driver2.Items.AddRange(orderedList.ToArray());

                comboBox_driver2.SelectionStart = cursorPos;
                filterDriver2Combobox = true;
            }
        }

        private void comboBox_car_textChanged(Object sender, EventArgs e)
        {
            if (filterCarCombobox && comboBox_car.SelectedIndex == -1)
            {
                filterCarCombobox = false;
                string item = comboBox_car.Text;
                int cursorPos = comboBox_car.SelectionStart;
                item = item.ToLower();
                IEnumerable<myItem> list = carsList
                    .Where(listItem => String.IsNullOrEmpty(((CarItem)(listItem.Data)).getGosNum) ? false : ((CarItem)(listItem.Data)).getGosNum.ToLower().Contains(item));

                comboBox_car.Items.Clear();
                comboBox_car.Items.AddRange(list.ToArray());

                comboBox_car.SelectionStart = cursorPos;
                filterCarCombobox = true;
            }
        }

        private void onOdoValueChanged(object sender, EventArgs e)
        {
            clearClose = false;
            string odoOut = textBox_kmBegin.Text;
            string odoReturn = textBox_kmEnd.Text;

            float odoOutVal = 0;
            float odoReturnVal = 0;
            try
            {
                odoOutVal = !string.IsNullOrEmpty(odoOut) ? float.Parse(odoOut) : 0;
                odoReturnVal = !string.IsNullOrEmpty(odoReturn) ? float.Parse(odoReturn) : 0;

            } catch
            {
                MessageBox.Show("Неверный формат значения километража!", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            float kmDiff = odoReturnVal - odoOutVal;
            textBox_kmDiff.Text = kmDiff.ToString();
            checkGridIsEditable();
        }

        private void textBox_fuel100kmPlan_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;
            float fuel100Plan = 0;
            try
            {
                fuel100Plan = string.IsNullOrEmpty(textBox_fuel100kmPlan.Text) ? 0 : float.Parse(textBox_fuel100kmPlan.Text);
            }
            catch
            {
                showInvalidFormatMessage("Норма расхода на 100 км, план");
            }
            // обновление нормы в поездках
            foreach (DataGridViewRow row in dataGridView_trip.Rows)
            {
                row.Cells["dg_100km_plan"].Value = fuel100Plan;
            }

            if (string.IsNullOrEmpty(textBox_fuel100kmFact.Text) || prev100kmNormCons.Equals(textBox_fuel100kmFact.Text))
            {
                textBox_fuel100kmFact.Text = textBox_fuel100kmPlan.Text;
            }

            prev100kmNormCons = textBox_fuel100kmPlan.Text;
        }

        private void textBox_kmDiff_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;

            calcNormFuelCons();
            calcFact100kmFuelCons();
        }

        private void calcNormFuelCons()
        {
            float diff = 0;
            float norm = 0;
            float fuelStop = 0;
            float fuelEquip = 0;
            try
            {
                diff = string.IsNullOrEmpty(textBox_kmDiff.Text) ? 0 : float.Parse(textBox_kmDiff.Text);
                norm = string.IsNullOrEmpty(textBox_fuel100kmPlan.Text) ? 0 : float.Parse(textBox_fuel100kmPlan.Text);
            }
            catch
            {
                showInvalidFormatMessage("Пройдено");
            }

            try
            {
                foreach (DataGridViewRow row in dataGridView_trip.Rows)
                {
                    DataGridViewCell curCell = row.Cells["dg_fuel_cons_stop"];
                    fuelStop += curCell.Value == null || string.IsNullOrWhiteSpace(curCell.Value.ToString()) ? 0 : float.Parse(curCell.Value.ToString());

                    curCell = row.Cells["dg_fuel_cons_equip"];
                    fuelEquip += curCell.Value == null || string.IsNullOrWhiteSpace(curCell.Value.ToString()) ? 0 : float.Parse(curCell.Value.ToString());
                }
            }
            catch
            {
                showInvalidFormatMessage("Расход топлива, стоянка и/или Расход топлива установки");
            }

            textBox_fuelConsNorm.Text = (diff * norm / (isMotoPlRegime ? 1 : (isMotoPlRegime ? 1 : 100)) + fuelStop + fuelEquip).ToString();
            formatTextBox(textBox_fuelConsNorm);
        }

        private void calcFact100kmFuelCons()
        {
            float diff = 0;
            float fact = 0;
            try
            {
                diff = string.IsNullOrEmpty(textBox_kmDiff.Text) ? 0 : float.Parse(textBox_kmDiff.Text);
                fact = string.IsNullOrEmpty(textBox_fuelConsFact.Text) ? 0 : float.Parse(textBox_fuelConsFact.Text);
            }
            catch
            {
                showInvalidFormatMessage("Пройдено и/или Расход, факт");
            }

            textBox_fuel100kmFact.Text = diff == 0 ? "" : (fact / diff * (isMotoPlRegime ? 1 : 100)).ToString();
            formatTextBox(textBox_fuel100kmFact);
        }

        private void calcFactFuelCons()
        {
            float diff = 0;
            float fact100km = 0;
            try
            {
                diff = string.IsNullOrEmpty(textBox_kmDiff.Text) ? 0 : float.Parse(textBox_kmDiff.Text);
                fact100km = string.IsNullOrEmpty(textBox_fuel100kmFact.Text) ? 0 : float.Parse(textBox_fuel100kmFact.Text);
            }
            catch
            {
                showInvalidFormatMessage("Пройдено и/или Норма расхода на 100 км, факт");
            }

            textBox_fuelConsFact.Text = diff == 0 ? "" : (diff * fact100km / (isMotoPlRegime ? 1 : 100)).ToString();
            formatTextBox(textBox_fuelConsFact);

            updateFuelEndValue();
        }

        private void textBox_fuelConsFact_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;

            this.textBox_fuel100kmFact.TextChanged -= new EventHandler(this.textBox_fuel100kmFact_TextChanged);
            calcFact100kmFuelCons();
            this.textBox_fuel100kmFact.TextChanged += new EventHandler(this.textBox_fuel100kmFact_TextChanged);

            updateFuelEndValue();
        }

        private void updateFuelEndValue()
        {
            if (!string.IsNullOrEmpty(textBox_fuelConsFact.Text))
            {
                // если пользователь ввел значение в фактический расход, то расчет конечного уровня топлива ведем по этому значению
                updateFuelEndDueConsValue();
            }
            else
            {
                // иначе, значение конечного уровня топлива берем из грида
                float fuelEnd = 0;
                if (dataGridView_trip.RowCount > 0)
                {
                    DataGridViewCell curCell = dataGridView_trip.Rows[dataGridView_trip.RowCount - 1].Cells["dg_fuel_return"];
                    try
                    {
                        fuelEnd = curCell.Value == null || string.IsNullOrWhiteSpace(curCell.Value.ToString()) ? 0 : float.Parse(curCell.Value.ToString());
                    }
                    catch
                    {
                        showInvalidFormatMessage("Расход топлива, факт и/или Расход топлива, норма");
                    }
                }
                textBox_fuelEnd.Text = fuelEnd.ToString();
                formatTextBox(textBox_fuelEnd);
            }
        }

        private void textBox_fuelConsNorm_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;

            if (string.IsNullOrEmpty(textBox_fuelConsFact.Text) || prevNormCons.Equals(textBox_fuelConsFact.Text))
            {
                textBox_fuelConsFact.Text = textBox_fuelConsNorm.Text;
            }

            prevNormCons = textBox_fuelConsNorm.Text;

        }

        private void textBox_fuel100kmFact_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;

            updateDataGridConsFact();

            this.textBox_fuelConsFact.TextChanged -= new EventHandler(this.textBox_fuelConsFact_TextChanged);
            calcFactFuelCons();
            this.textBox_fuelConsFact.TextChanged += new EventHandler(this.textBox_fuelConsFact_TextChanged);
        }

        private void updateDataGridConsFact()
        {
            float fuel100Fact = 0;
            try
            {
                fuel100Fact = string.IsNullOrEmpty(textBox_fuel100kmFact.Text) ? 0 : float.Parse(textBox_fuel100kmFact.Text);
            }
            catch
            {
                showInvalidFormatMessage("Норма расхода на 100 км, факт");
            }
            // обновление нормы в поездках
            foreach (DataGridViewRow row in dataGridView_trip.Rows)
            {
                row.Cells["dg_100km_fact"].Value = fuel100Fact;
            }
        }

        private void textBox_plNum_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;
            if (isRES)
            {
                textBox_issueNum.Text = textBox_plNum.Text;
            }
            checkGridIsEditable();
        }

        private void textBox_fuelBegin_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;
            checkGridIsEditable();

            if (!string.IsNullOrEmpty(textBox_fuelConsFact.Text))
            {
                // если пользователь ввел значение в фактический расход, то расчет конечного уровня топлива ведем по этому значению
                updateFuelEndDueConsValue();
            }

            float fuelOut = string.IsNullOrEmpty(textBox_fuelBegin.Text) ? 0 : float.Parse(textBox_fuelBegin.Text);
            if (isOutOfTankCapacity(fuelOut))
            {
                textBox_fuelBegin.BackColor = Color.Red;
                return;
            }

            textBox_fuelBegin.BackColor = Color.White;
        }

        private void textBox_fuelEnd_TextChanged(object sender, EventArgs e)
        {
            clearClose = false;

            float fuelReturn = string.IsNullOrEmpty(textBox_fuelEnd.Text) ? 0 : float.Parse(textBox_fuelEnd.Text);
            if (isOutOfTankCapacity(fuelReturn))
            {
                textBox_fuelEnd.BackColor = Color.Red;
                return;
            }

            textBox_fuelEnd.BackColor = Color.White;
        }

        private bool isOutOfTankCapacity(float aValue)
        {
            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                float fuelTank = ((CarItem)item.Data).getFuelTankCapacity;

                if (fuelTank > 0)
                {
                    if (aValue > fuelTank)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void textBox_LostFocus_Formatter(object sender, EventArgs e)
        {
            if (sender is TextBoxNumber)
            {
                TextBoxNumber textBox = (TextBoxNumber)sender;
                formatTextBox(textBox);
            }
            
        }

        private void formatTextBox(TextBoxNumber textBox)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                try
                {
                    textBox.Text = string.Format("{0:#,###0.000}", double.Parse(textBox.Text));
                }
                catch { }
            }
        }

        private void dataGrid_trip_Formatter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView_trip.RowCount > 0 && e.RowIndex >= 0)
            {
                // Форматирование топливных полей
                if (e.ColumnIndex == 5 || e.ColumnIndex == 10 || e.ColumnIndex == 11 || e.ColumnIndex == 13
                    || e.ColumnIndex == 14 || e.ColumnIndex == 15 || e.ColumnIndex == 9 || e.ColumnIndex == 7)
                {
                    DataGridViewCell cell = dataGridView_trip.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (cell.Value != null)
                    {
                        try
                        {
                            cell.Value = string.Format("{0:#,###0.000}", double.Parse(cell.Value.ToString()));
                        }
                        catch {}
                    }
                }
            }
        }

        void btn_dg_add_Click(object sender, EventArgs e)
        {
            clearClose = false;
            if (!checkDataGridRow(dataGridView_trip.RowCount - 1, false))
            {
                return;
            }

            // Проверка, что если существует предыдущий ПЛ, то у него заведены поездки
            if (!checkPreviousPLTripExists())
            {
                return;
            }

            int k = dataGridView_trip.Rows.Add();


            putCarDefaultsToRow(k);

            DateTime returnDate = DateTime.Now;
            DateTime outDate = DateTime.Now;

            // если первая строка
            if (k == 0)
            {
                // тянем дефолты для первой поездки
                outDate = dateTimePicker_dateOutPlan.Value.Date + dateTimePicker_timeOutPlan.Value.TimeOfDay;

                dataGridView_trip.Rows[k].Cells["dg_odo_begin"].Value = textBox_kmBegin.Text;
                dataGridView_trip.Rows[k].Cells["dg_fuel_out"].Value = textBox_fuelBegin.Text;
            }
            else
            {
                // тянем дефолты для поездки из предыдущей
                Object prevReturnDate = dataGridView_trip.Rows[k - 1].Cells["dg_date_return"].Value;
                Object prevOutDate = dataGridView_trip.Rows[k - 1].Cells["dg_date_out"].Value;
                if (prevReturnDate != null && prevOutDate != null)
                {
                    try
                    {
                        DateTime myPrevReturnDate = (DateTime)prevReturnDate;
                        DateTime myPrevOutDate = (DateTime)prevOutDate;

                        switch ((int)dataGridView_trip.Rows[k].Cells["dg_car_regime"].Value)
                        {
                            case 17:
                                // Полуторасменный
                                outDate = myPrevReturnDate;
                                break;
                            case 16:
                            case 18:
                            default:
                                // Односменный
                                // Трехсменный
                                // По умолчанию
                                outDate = myPrevOutDate.AddHours(24);
                                break;
                        }

                    } catch
                    {
                        MessageBox.Show("Неверный формат даты возвращения.", "Ошибка при редактировании");
                    }
                }
                dataGridView_trip.Rows[k].Cells["dg_odo_begin"].Value = dataGridView_trip.Rows[k - 1].Cells["dg_odo_return"].Value;
                dataGridView_trip.Rows[k].Cells["dg_fuel_out"].Value = dataGridView_trip.Rows[k - 1].Cells["dg_fuel_return"].Value;

            }

            switch ((int)dataGridView_trip.Rows[k].Cells["dg_car_regime"].Value)
            {
                case 16:
                case 17:
                case 18:
                    // Односменный
                    // Полуторасменный
                    // Трехсменный
                    returnDate = outDate.AddHours((int)dataGridView_trip.Rows[k].Cells["dg_car_regime_hours"].Value);
                    break;
                default:
                    // По умолчанию
                    returnDate = outDate.AddHours(9);
                    break;
            }


            dataGridView_trip.Rows[k].Cells["dg_date_return"].Value = returnDate;
            dataGridView_trip.Rows[k].Cells["dg_date_out"].Value = outDate;

            checkPLItemsIsEditable();

        }

        void updatePLDateOut()
        {
            DateTime outDate = dateTimePicker_dateOutPlan.Value.Date + DateTime.Parse("08:00").TimeOfDay;

            if (comboBox_driver1.SelectedItem != null)
            {
                DriverItem item = (DriverItem)((myItem)comboBox_driver1.SelectedItem).Data;

                if (!string.IsNullOrEmpty(item.getOutTime))
                {
                    try
                    {
                        outDate = outDate.Date + DateTime.Parse(item.getOutTime).TimeOfDay;
                    }
                    catch
                    {
                        MessageBox.Show("Неверный формат времени выезда водителя! Установка значения по умолчанию.", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            dateTimePicker_dateOutPlan.Value = outDate;
            dateTimePicker_timeOutPlan.Value = outDate;

        }

        void updatePLDateReturn()
        {
            DateTime outDate = dateTimePicker_dateOutPlan.Value.Date + dateTimePicker_timeOutPlan.Value.TimeOfDay;
            DateTime returnDate = dateTimePicker_dateOutPlan.Value.Date + DateTime.Parse("17:00").TimeOfDay;

            if (comboBox_driver1.SelectedItem != null)
            {
                DriverItem driverItem = (DriverItem)((myItem)comboBox_driver1.SelectedItem).Data;

                if (string.IsNullOrEmpty(driverItem.getOutTime))
                {
                    if (comboBox_car.SelectedItem != null)
                    {
                        CarItem item = (CarItem)((myItem)comboBox_car.SelectedItem).Data;

                        switch (item.getRegime)
                        {
                            case 16:
                            case 17:
                            case 18:
                                // Односменный
                                // Полуторасменный
                                // Трехсменный
                                returnDate = outDate.AddHours(item.getRegimeHours);
                                break;
                            default:
                                // По умолчанию
                                break;
                        }
                    }
                }
                else
                {
                    try
                    {
                        returnDate = returnDate.Date + DateTime.Parse(driverItem.getReturnTime).TimeOfDay;
                    }
                    catch
                    {
                        MessageBox.Show("Неверный формат времени возвращения водителя! Установка значения по умолчанию.", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            dateTimePicker_dateReturnPlan.Value = returnDate;
            dateTimePicker_timeReturnPlan.Value = returnDate;

        }

        private void fillDatagridFuelCell(int rowIndex)
        {
            try
            {

                DataGridViewCell curCell = dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"];
                float fuel_return = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                curCell = dataGridView_trip.Rows[rowIndex].Cells["dg_car_fuel_tank"];
                float fuel_tank = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                if (fuel_tank != 0 && fuel_tank < fuel_return || fuel_return < 0)
                {
                    dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"].Style.BackColor = Color.Red;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"].Style.SelectionBackColor = Color.Red;
                    isGridWithErrors = true;
                }
                else
                {

                    dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"].Style.BackColor = Color.White;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"].Style.SelectionBackColor = dataGridView_trip.DefaultCellStyle.SelectionBackColor;
                    // dataGridView_trip.Rows[rowIndex].Cells["dg_fuel_return"].Style = dataGridView_trip.DefaultCellStyle;
                    isGridWithErrors = false;
                }
            }
            catch { }
        }

        private void putCarDefaultsToRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridView_trip.RowCount )
            {
                // установка значений норм расхода топлива по при стоянке ТС и при работе оборудования
                if (comboBox_car.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_car.SelectedItem;
                    float carNorm = ((CarItem)item.Data).getCarNorm;
                    float carEquipNorm = ((CarItem)item.Data).getCarEquipNorm;
                    int carRegime = ((CarItem)item.Data).getRegime;
                    int carRegimeHours = ((CarItem)item.Data).getRegimeHours;
                    float fuelTank = ((CarItem)item.Data).getFuelTankCapacity;
                    bool motoPlRegime = ((CarItem)item.Data).getMotoPlRegime;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_car_norm"].Value = carNorm;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_car_equip_norm"].Value = carEquipNorm;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_car_regime"].Value = carRegime;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_car_regime_hours"].Value = carRegimeHours;
                    dataGridView_trip.Rows[rowIndex].Cells["dg_car_fuel_tank"].Value = fuelTank;
                }

                // Установка норм топлива на 100 км
                float fuel100Plan = 0;
                float fuel100Fact = 0;
                try
                {
                    fuel100Plan = string.IsNullOrEmpty(textBox_fuel100kmPlan.Text) ? 0 : float.Parse(textBox_fuel100kmPlan.Text);
                    fuel100Fact = string.IsNullOrEmpty(textBox_fuel100kmFact.Text) ? 0 : float.Parse(textBox_fuel100kmFact.Text);
                }
                catch
                {
                    showInvalidFormatMessage("Норма расхода на 100 км, факт и/или Норма расхода на 100 км, план");
                }
                dataGridView_trip.Rows[rowIndex].Cells["dg_100km_plan"].Value = fuel100Plan;
                dataGridView_trip.Rows[rowIndex].Cells["dg_100km_fact"].Value = fuel100Fact;

                fillDatagridFuelCell(rowIndex);
            }
        }

        private double getDatePlanDaysDiff()
        {
            DateTime outDate = dateTimePicker_dateOutPlan.Value.Date + dateTimePicker_timeOutPlan.Value.TimeOfDay;
            DateTime returnDate = dateTimePicker_dateReturnPlan.Value.Date + dateTimePicker_timeReturnPlan.Value.TimeOfDay;

            return (returnDate - outDate).TotalDays;
        }

        private void update_next_dg_row(int currentRowIndex)
        {
            // если поездка не последняя в гриде
            if (dataGridView_trip.RowCount - 1 > currentRowIndex)
            {
                dataGridView_trip.Rows[currentRowIndex + 1].Cells["dg_odo_begin"].Value = dataGridView_trip.Rows[currentRowIndex].Cells["dg_odo_return"].Value;
                dataGridView_trip.Rows[currentRowIndex + 1].Cells["dg_fuel_out"].Value = dataGridView_trip.Rows[currentRowIndex].Cells["dg_fuel_return"].Value;
            }
        }

        void dg_row_deleted(object sender, EventArgs e)
        {
            clearClose = false;
            if (dataGridView_trip.SelectedRows.Count > 0)
            {


                Object idEl = dataGridView_trip.SelectedRows[0].Cells["dg_trip_gid"].Value;

                if (dataGridView_trip.SelectedRows[0].IsNewRow == false && idEl != null && (int)idEl > 0)
                {
                    tripToDelete.Add((int)idEl);
                }

                int delIndex = dataGridView_trip.SelectedRows[0].Index;
                dataGridView_trip.Rows.RemoveAt(delIndex);
                if (delIndex > 0)
                {
                    update_next_dg_row(delIndex - 1);
                }
            }
            checkPLItemsIsEditable();
        }

        void checkGridIsEditable()
        {
            if (!string.IsNullOrEmpty(textBox_plNum.Text) && comboBox_driver1.SelectedItem != null && dateTimePicker_dateOutPlan.Value != null &&
                dateTimePicker_dateReturnPlan.Value != null && !string.IsNullOrEmpty(textBox_fuelBegin.Text) && 
                !string.IsNullOrEmpty(textBox_kmBegin.Text) && comboBox_car.SelectedItem != null)
            {
                dataGridView_trip.ReadOnly = false;
                dg_fuel_out.ReadOnly = true;
                dg_fuel_return.ReadOnly = true;
                dg_fuel_cons_stop.ReadOnly = true;
                dg_fuel_cons_equip.ReadOnly = true;
                dg_fuel_cons_norm.ReadOnly = true;
                dg_fuel_cons_fact.ReadOnly = true;
                dg_fuel_diff.ReadOnly = true;
                dg_route_len.ReadOnly = true;

                button_dgDel.Enabled = true;
                button_dgAdd.Enabled = true;

                actualizeMotoGridColumnsUpdateble();
            } else
            {
                dataGridView_trip.ReadOnly = true;

                button_dgDel.Enabled = false;
                button_dgAdd.Enabled = false;
            }
        }

        void checkPLItemsIsEditable()
        {
            // если создано несколько поездок или одна, но заполненная, то тогда надо блокировать поля
            if (dataGridView_trip.RowCount > 1 || dataGridView_trip.RowCount == 1 && checkDataGridRow(0, true))
            {
                textBox_fuelBegin.ReadOnly = true;
                textBox_kmBegin.ReadOnly = true;
                
                groupBox17.Enabled = false;
                groupBox18.Enabled = false;
                groupBox19.Enabled = false;
                groupBox20.Enabled = false;
                groupBox21.Enabled = false;
            }
            else
            {
                textBox_fuelBegin.ReadOnly = false;
                textBox_kmBegin.ReadOnly = false;
                
                groupBox17.Enabled = true;
                groupBox18.Enabled = true;
                groupBox19.Enabled = true;
                groupBox20.Enabled = true;
                groupBox21.Enabled = true;
            }
            
        }


        private void updateFuelEndDueConsValue()
        {
            try
            {
                float fuelOut = string.IsNullOrEmpty(textBox_fuelBegin.Text) ? 0 : float.Parse(textBox_fuelBegin.Text);
                float fuelCons = float.Parse(textBox_fuelConsFact.Text);

                float fuelGot = 0;
                foreach (DataGridViewRow row in dataGridView_trip.Rows)
                {
                    DataGridViewCell curCell = row.Cells["dg_fuel_got"];
                    fuelGot += curCell.Value == null || string.IsNullOrWhiteSpace(curCell.Value.ToString()) ? 0 : float.Parse(curCell.Value.ToString());
                }

                textBox_fuelEnd.Text = (fuelOut - fuelCons + fuelGot).ToString();
                formatTextBox(textBox_fuelEnd);
            }
            catch
            {
                showInvalidFormatMessage("Топливо при выезде и/или Движение топлива, получено и/или Расход, факт");
            }
        }

        private void dateTimePicker_timeOutFact_ValueChanged(object sender, EventArgs e)
        {
            this.dateTimePicker_dateOutFact.Format = DateTimePickerFormat.Long;
            this.dateTimePicker_timeOutFact.CustomFormat = "HH:mm";
            factDateOutInitialized = true;
        }

        private void dateTimePicker_dateOutFact_ValueChanged(object sender, EventArgs e)
        {
            this.dateTimePicker_dateOutFact.Format = DateTimePickerFormat.Long;
            this.dateTimePicker_timeOutFact.CustomFormat = "HH:mm";
            factDateOutInitialized = true;
        }

        private void dateTimePicker_timeReturnFact_ValueChanged(object sender, EventArgs e)
        {
            this.dateTimePicker_dateReturnFact.Format = DateTimePickerFormat.Long;
            this.dateTimePicker_timeReturnFact.CustomFormat = "HH:mm";
            factDateReturnInitialized = true;
        }

        private void dateTimePicker_dateReturnFact_ValueChanged(object sender, EventArgs e)
        {
            this.dateTimePicker_dateReturnFact.Format = DateTimePickerFormat.Long;
            this.dateTimePicker_timeReturnFact.CustomFormat = "HH:mm";
            factDateReturnInitialized = true;
        }

        private void calculateRouteLenToRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridView_trip.RowCount)
            {
                try
                {

                    DataGridViewCell curCell = dataGridView_trip.Rows[rowIndex].Cells["dg_odo_begin"];
                    float odoBegin = curCell.Value != null && !string.IsNullOrWhiteSpace(curCell.Value.ToString()) ?
                         float.Parse(curCell.Value.ToString()) : 0;

                    curCell = dataGridView_trip.Rows[rowIndex].Cells["dg_odo_return"];
                    float odoEnd = curCell.Value != null && !string.IsNullOrWhiteSpace(curCell.Value.ToString()) ?
                         float.Parse(curCell.Value.ToString()) : 0;

                    dataGridView_trip.Rows[rowIndex].Cells["dg_route_len"].Value = odoEnd - odoBegin;
                }
                catch
                {
                    showInvalidFormatMessage("Спидометр, начало и/или Спидометр, возвращение");
                }
            }
        }

        private void refresh_issue_block()
        {
            bool isComplex = false;

            if (comboBox_car.SelectedItem != null)
            {
                myItem item = (myItem)comboBox_car.SelectedItem;
                isComplex = ((CarItem)item.Data).getIsComplex;
            }

            // Если пользователь принадлежит РЭС (это смотрим в таблице "Настройки|Пользователи", в колонке "Район-закрепления".
            if (isRES && !isComplex)
            {
                // Значения колонки "Тип путевого листа"(в справочнике "УМЭТС Тип ТС") равно 2, или 5 (!isComplex)
                // Выводим выпадающий список со значениями из справочника "УМЭТС Служба" для редактирования поля.
                textBox_rasp.Visible = false;
                comboBox_service.Visible = true;

                String serviceSQL =
                        "SELECT uo.gid, naimenovanie, rajon_zakreplenie " +
                        "FROM autobase.umjets_organizacija uo";
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    comboBox_service.Items.Clear();

                    sqlCmd.sql = serviceSQL;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        int curResID = sqlCmd.GetInt32("rajon_zakreplenie");

                        myItem x_service = new myItem(
                        sqlCmd.GetString("naimenovanie"),
                        sqlCmd.GetInt32("gid"),
                        curResID
                        );
                        int idx = comboBox_service.Items.Add(x_service);

                        if (resID == curResID)
                        {
                            comboBox_service.SelectedIndex = idx;
                        }
                    }
                }

            }
            else if (isRES && isComplex)
            {
                // Значения колонки "Тип путевого листа" (в справочнике "УМЭТС Тип ТС") равно 1,3, или 4 (isComplex)
                textBox_rasp.Visible = true;
                comboBox_service.Visible = false;
                // Заполняем в соответствии со значением таблицы "УМЭТС Район-закрепления" колонки "В распоряжение" на основании района закрепления пользователя (из таблицы "Настройки|Пользователи").
                textBox_rasp.Text = issCust;
            }
            // Если пользователь не принадлежит РЭС (либо пустое поле в таблице "Настройки|Пользователи" в колонке "Район-закрепления", либо значение "Автох-во ул. Киевская 14".
            else if (!isRES && !isComplex)
            {
                // Значения колонки "Тип путевого листа"(в справочнике "УМЭТС Тип ТС") равно 2, или 5 (!isComplex)
                textBox_rasp.Visible = false;
                comboBox_service.Visible = true;

                // Выводим выпадающий список со значениями из справочника "УМЭТС Служба" для редактирования поля.
                String serviceSQL =
                        "SELECT uo.gid, naimenovanie, rajon_zakreplenie " +
                        "FROM autobase.umjets_organizacija uo";
                using (var sqlCmd = MainPluginClass.App.SqlWork())
                {
                    comboBox_service.Items.Clear();

                    sqlCmd.sql = serviceSQL;
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        int curResID = sqlCmd.GetInt32("rajon_zakreplenie");

                        myItem x_service = new myItem(
                        sqlCmd.GetString("naimenovanie"),
                        sqlCmd.GetInt32("gid"),
                        curResID
                        );
                        int idx = comboBox_service.Items.Add(x_service);
                    }
                }
            }
            else
            {
                // Значения колонки "Тип путевого листа" (в справочнике "УМЭТС Тип ТС") равно 1,3, или 4 (isComplex)
                textBox_rasp.Visible = true;
                comboBox_service.Visible = false;

                // Либо тянем по соответствующей заявке из "УМЭТС Заявки на ТС", либо пользователь заполняет вручную.
                textBox_rasp.Text = "";
                if (comboBox_issue.SelectedItem != null)
                {
                    myItem item = (myItem)comboBox_issue.SelectedItem;

                    textBox_rasp.Text = ((IssueItem)item.Data).getOwner;
                }
            }
        }

        // 0 -  dg_trip_gid
        // 1 -  dg_date_out
        // 2 -  dg_date_return
        // 3 -  dg_odo_begin
        // 4 -  dg_odo_return
        // 5 -  dg_fuel_got
        // 6 -  dg_equip_motohours
        // 7 -  dg_fuel_cons_equip
        // 8 -  dg_motohours_stop
        // 9 -  dg_fuel_cons_stop
        // 10 - dg_fuel_out
        // 11 - dg_fuel_return
        // 12 - dg_route_len
        // 13 - dg_fuel_cons_norm
        // 14 - dg_fuel_cons_fact
        // 15 - dg_fuel_diff
        // 16 - dg_car_norm
        // 17 - dg_car_equip_norm
        // 18 - dg_100km_plan
        // 19 - dg_100km_fact
        // 20 - dg_car_regime
        // 21 - dg_car_regime_hours
        // 22 - dg_car_fuel_tank

        void dg_cell_changed(object sender, DataGridViewCellEventArgs e)
        {
            clearClose = isInit;

            if (dataGridView_trip.RowCount > 0 && e.RowIndex >= 0)
            {
                // Проверка того, что дата выезда больше даты возвращения
                if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
                {
                    Object outCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_date_out"].Value;
                    Object returnCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_date_return"].Value;
                    if (outCell != null && returnCell != null)
                    try
                    {
                        DateTime outDateTime = (DateTime)outCell;
                        DateTime returnDateTime = (DateTime)returnCell;
                        if (outDateTime > returnDateTime)
                        {
                            MessageBox.Show("Дата возвращения не может быть меньше даты выезда", "Ошибка при редактировании", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Неверный формат даты выезда/возвращения.", "Ошибка при редактировании");
                    }
                }

                // Раскраска поля Топливо при возвращении
                if (e.ColumnIndex == 11)
                {
                    fillDatagridFuelCell(e.RowIndex);
                }

                // Слежка за параметрами для изменения Топливо при возвращении
                if (!isInit && string.IsNullOrEmpty(textBox_fuelConsFact.Text) && (e.ColumnIndex == 10 || e.ColumnIndex == 5 || e.ColumnIndex == 13 || e.ColumnIndex == 9 || e.ColumnIndex == 7 || e.ColumnIndex == 19))
                {
                   
                    try
                    {

                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_out"];
                        float fuel_out = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_got"];
                        float fuel_got = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_norm"];
                        float fuel_norm = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_stop"];
                        float fuel_stop = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_equip"];
                        float fuel_equip = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_return"].Value = fuel_out + fuel_got - fuel_norm;

                        calcNormFuelCons();
                    }
                    catch
                    {
                        showInvalidFormatMessage("Топливо при выезде и/или Движение топлива, получено и/или Расход топлива, норма и/или Расход топлива, стоянка и/или Расход топлива, установки");
                    }
                 
                }

                // Слежка за параметрами для изменения Топливо при возвращении - в случае заполненного фактического расхода в ПЛ
                if (!isInit && !string.IsNullOrEmpty(textBox_fuelConsFact.Text) && (e.ColumnIndex == 10 || e.ColumnIndex == 5 || e.ColumnIndex == 12 || e.ColumnIndex == 19))
                {
                    try
                    {

                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_out"];
                        float fuel_out = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_got"];
                        float fuel_got = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_route_len"];
                        float route_len = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_100km_fact"];
                        float fuel_fact_100km = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_return"].Value = fuel_out + fuel_got - fuel_fact_100km * route_len / (isMotoPlRegime ? 1 : 100);
                    }
                    catch
                    {
                        showInvalidFormatMessage("Топливо при выезде и/или Движение топлива, и/или Расход, факт");
                    }

                }

                // Слежка за получением топлива для обновления фактического уровня топлива при возвращении
                if (!isInit && (e.ColumnIndex == 5 && !string.IsNullOrEmpty(textBox_fuelConsFact.Text)))
                {
                    updateFuelEndDueConsValue();
                }

                // Слежка за параметрами для изменения Расход топлива, стоянка
                if (e.ColumnIndex == 16 || e.ColumnIndex == 8)
                {
                    try
                    {
                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_car_norm"];
                        float car_norm = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_motohours_stop"];
                        float moto_stop = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_stop"].Value = car_norm * moto_stop;
                    }
                    catch
                    {
                        showInvalidFormatMessage("Работа двигателя при стоянке");
                    }
                }

                // Слежка за параметрами для изменения Расход топлива, установки
                if (e.ColumnIndex == 17 || e.ColumnIndex == 6)
                {
                    try
                    {

                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_car_equip_norm"];
                        float car_equip_norm = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_equip_motohours"];
                        float moto_equip = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_equip"].Value = car_equip_norm * moto_equip;
                    }
                    catch
                    {
                        showInvalidFormatMessage("Работа установки");
                    }
                }

                // Слежка за параметрами для изменения Расход топлива, норма
                if (e.ColumnIndex == 18 || e.ColumnIndex == 12 || e.ColumnIndex == 9 || e.ColumnIndex == 7)
                {
                    try
                    {
                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_100km_plan"];
                        float fuel_100km_plan = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_route_len"];
                        float routeLen = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_stop"];
                        float fuelStop = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_equip"];
                        float fuelEquip = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_norm"].Value = fuel_100km_plan * routeLen / (isMotoPlRegime ? 1 : 100) + 
                            (isMotoPlRegime ? 0 : 1) * (fuelStop + fuelEquip);

                        if (!isInit)
                        {
                            calcNormFuelCons();
                        }

                    }
                    catch (Exception ex)
                    {
                        showInvalidFormatMessage(ex.Message);

                        showInvalidFormatMessage("Пробег и/или Расход топлива, стоянка, и/или Расход топлива, установки");
                    }
                }

                // Слежка за параметрами для изменения Расход топлива, факт
                if (e.ColumnIndex == 19 || e.ColumnIndex == 12)
                {
                    try
                    {

                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_100km_fact"];
                        float fuel_100km_fact = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_route_len"];
                        float routeLen = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_fact"].Value = fuel_100km_fact * routeLen / (isMotoPlRegime ? 1 : 100);
                    }
                    catch(Exception ex)
                    {
                        showInvalidFormatMessage(ex.Message);
                        
                        showInvalidFormatMessage("Пробег");
                    }
                }

                // Слежка за параметрами для изменения Отклонения
                if (e.ColumnIndex == 13 || e.ColumnIndex == 14)
                {
                    try
                    {

                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_norm"];
                        float fuelNorm = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_cons_fact"];
                        float fuelFact = curCell.Value == null ? 0 : float.Parse(curCell.Value.ToString());

                        dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_diff"].Value = fuelNorm - fuelFact;
                    }
                    catch
                    {
                        showInvalidFormatMessage("Расход топлива, факт и/или Расход топлива, норма");
                    }
                }

                // Слежка за параметрами для изменения Пробега
                if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
                {
                    calculateRouteLenToRow(e.RowIndex);
                }

                // Слежка за параметрами dg_odo_return и dg_fuel_return для обновления следующих поездок
                if (!isInit && (e.ColumnIndex == 4 || e.ColumnIndex == 11))
                {
                    update_next_dg_row(e.RowIndex);
                }

                // Слежка за параметром dg_fuel_return последней строки для обновления показания уровня топлива в путевом листе
                if (!isInit && e.ColumnIndex == 11)
                {
                    if (e.RowIndex == dataGridView_trip.RowCount - 1 && string.IsNullOrEmpty(textBox_fuelConsFact.Text))
                    {
                        DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_fuel_return"];
                        if (curCell.Value != null && !string.IsNullOrWhiteSpace(curCell.Value.ToString()))
                        {
                            textBox_fuelEnd.Text = curCell.Value.ToString();
                            formatTextBox(textBox_fuelEnd);
                        }
                    }
                }

                // Слежка за параметром dg_odo_return последней строки для обновления показания конечного значения спидометра в путевом листе
                if (!isInit && (e.ColumnIndex == 4 && e.RowIndex == dataGridView_trip.RowCount - 1))
                {
                    DataGridViewCell curCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_odo_return"];
                    if (curCell.Value != null && !string.IsNullOrWhiteSpace(curCell.Value.ToString()))
                    {
                        textBox_kmEnd.Text = curCell.Value.ToString();
                    }
                }

                // Слежка за параметром dg_date_out первой поездки для обновления фактической даты выезда в ПЛ
                if (!isInit && (e.ColumnIndex == 1 && e.RowIndex == 0))
                {
                    Object outCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_date_out"].Value;
                    if (outCell != null) {
                        try
                        {
                            DateTime outDateTime = (DateTime)outCell;

                            dateTimePicker_dateOutFact.Value = outDateTime;
                            dateTimePicker_timeOutFact.Value = outDateTime;
                        }
                        catch
                        {
                            MessageBox.Show("Неверный формат даты выезда.", "Ошибка при редактировании");
                        }
                    }
                }

                // Слежка за параметром dg_date_return последней поездки для обновления фактической даты возвращения в ПЛ
                if (!isInit && (e.ColumnIndex == 2 && e.RowIndex == dataGridView_trip.RowCount - 1))
                {

                    Object returnCell = dataGridView_trip.Rows[e.RowIndex].Cells["dg_date_return"].Value;
                    if (returnCell != null)
                    {
                        try
                        {
                            DateTime returnDateTime = (DateTime)returnCell;

                            dateTimePicker_dateReturnFact.Value = returnDateTime;
                            dateTimePicker_timeReturnFact.Value = returnDateTime;
                        }
                        catch
                        {
                            MessageBox.Show("Неверный формат даты возвращения.", "Ошибка при редактировании");
                        }

                    }
                }
            }

            checkPLItemsIsEditable();
        }

        private bool validateDataGrid()
        {
            for (int i = 0; i < dataGridView_trip.RowCount; i++)
            {
                if (!checkDataGridRow(i, false))
                {
                    return false;
                }
            }
            return true;
        }

        private bool validatePLForm()
        {
            // Теперь можно сохранять ПЛ и без номера

            //if (string.IsNullOrWhiteSpace(textBox_plNum.Text))
            //{
            //    MessageBox.Show("Не указан номер путевого листа!", "Ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return false;
            //}
            return true && checkPlanDates(false);
        }
    }
}