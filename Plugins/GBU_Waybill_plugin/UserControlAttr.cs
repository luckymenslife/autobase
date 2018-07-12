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



namespace GBU_Waybill_plugin
{

    public partial class UserControlAttr : UserControl, IUserControlMain
    {
        private ContextMenu _contextMenu = new ContextMenu();
        private bool _can_open;
        private int? _id;
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
        private int base_car_id;
        long? carExternalId;
        public UserControlAttr(int? id_object, int id_table)
        {

            InitializeComponent();

            _id = id_object;

            // Запоминаем таблицу на которую назначен плагин
            MapEditorTablePutList = id_table;

            // Загрузка нормы если выбрали операцию, либо подсчет суммы расхода топлива
            DataGridView1.CellValueChanged += DataGridView1_CellValueChanged;

            // Загрузим список печатных форм
            _contextMenu.MenuItems.Clear();
            foreach (var item in MainPluginClass.Work.FastReport.FindReportsByIdTable(260).ToArray())
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

            // Единый сводный расчет и местами раскарска если внесли данные
            TB_tax_f.TextChanged += T1_T2_TextChanged;
            TB_tax_drain.TextChanged += T1_T2_TextChanged;
            TB_tax_n.TextChanged += T1_T2_TextChanged;
            TB_t1_k.TextChanged += T1_T2_TextChanged;
            TB_t1_n.TextChanged += T1_T2_TextChanged;
            TB_t1_f.TextChanged += T1_T2_TextChanged;
            TB_t1_pl.TextChanged += T1_T2_TextChanged;
            TB_t2_n.TextChanged += T1_T2_TextChanged;
            TB_t2_k.TextChanged += T1_T2_TextChanged;
            TB_t2_pl.TextChanged += T1_T2_TextChanged;
            TB_t2_f.TextChanged += T1_T2_TextChanged;
            TB_spk.TextChanged += T1_T2_TextChanged;
            TB_sp_n.TextChanged += T1_T2_TextChanged;
            TB_mch_k.TextChanged += T1_T2_TextChanged;
            TB_mch.TextChanged += T1_T2_TextChanged;
            TB_mch_obr_k.TextChanged += T1_T2_TextChanged;
            TB_mch_obr.TextChanged += T1_T2_TextChanged;


            // ПРОЧИЕ ДЛЯ КНОПОЧЕК
            BTN_save.Click += btn_save_click;
            //BTN_print.Click += btn_print_click;
            BTN_cancel.Click += btn_cancel_click;
            btn_edit.Click += btn_edit_Click;

            // ПРОЧИЕ ДЛЯ КОМБОБОКСА
            CB_gar_no.SelectedIndexChanged += CB_gar_no_SelectedIndexChanged;
            CB_gos_no.SelectedIndexChanged += CB_gos_no_SelectedIndexChanged_1;
            CB_p_gar_no.SelectedIndexChanged += CB_p_gar_no_SelectedIndexChanged;
            CB_p_gos_no.SelectedIndexChanged += CB_p_gos_no_SelectedIndexChanged;
            CB_rrab.SelectedIndexChanged += CB_rrab_SelectedIndexChanged;
            CB_top2.SelectedIndexChanged += CB_top2_SelectedIndexChanged;
            CB_gruztype.SelectedIndexChanged += CB_gruztype_SelectedIndexChanged;
            CB_wrktype.SelectedIndexChanged += CB_wrktype_SelectedIndexChanged;
            CB_route.SelectedIndexChanged += CB_route_SelectedIndexChanged;
            CB_pl_zone.SelectedIndexChanged += CB_pl_zone_SelectedIndexChanged;
            CB_pl_driverel.SelectedIndexChanged += CB_pl_driverel_SelectedIndexChanged;
            CB_pl_drivertn.SelectedIndexChanged += CB_pl_drivertn_SelectedIndexChanged;
            CB_escort_driverel.SelectedIndexChanged += CB_escort_driverel_SelectedIndexChanged;
            CB_escort_drivertn.SelectedIndexChanged += CB_escort_drivertn_SelectedIndexChanged;
            CB_org.SelectedIndexChanged += CB_org_SelectedIndexChanged;

            CB_top1.SelectedIndexChanged += CB_top1_SelectedIndexChanged;
            TB_modelid.TextChanged += TB_modelid_TextChanged;

            // Какие-то настройки компонентов
            btn_tax_add.Click += btn_tax_add_Click;
            btn_tax_del.Click += btn_tax_del_Click;
            DataGridView1.DataError += DataGridView1_DataError;
            CHB_calc_norm_type.CheckedChanged += CHB_calc_norm_type_CheckedChanged;

            create_task_btn.Visible = (_id != null) && MainPluginClass.OrgsIds.Contains(org_id.ToString());
            isEdited = false;
        }

        private void CB_escort_drivertn_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_escort_drivertn.SelectedItem).GetId;
                Set_dict_values(r, CB_escort_driverel);
            }
            catch { }
        }

        private void CB_escort_driverel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_escort_driverel.SelectedItem).GetId;
                Set_dict_values(r, CB_escort_drivertn);
            }
            catch { }
        }

        private void CHB_calc_norm_type_CheckedChanged(object sender, EventArgs e)
        {
            TB_t1_k.Enabled = !CHB_calc_norm_type.Checked;
            if (CB_top2.SelectedItem != null && !CB_top2.SelectedItem.ToString().ToUpper().Trim().Equals("НЕТ"))
            {
                TB_t2_k.Enabled = !CHB_calc_norm_type.Checked;
            }
            T1_T2_TextChanged(null, new EventArgs());
        }

        void print_MenuStrip_Click(object sender, EventArgs e)
        {

            IReportItem_M _report_item = null;

            if (sender is MenuItem)
            {
                _report_item = (IReportItem_M)((MenuItem)sender).Tag;
            }
            else if (sender is SplitButton && splitButton1.Tag != null)
            {
                foreach (MenuItem z in splitButton1.SplitMenu.MenuItems)
                {
                    if (z.Tag != null && z.Tag is IReportItem_M && ((IReportItem_M)z.Tag).IdReport == (int)splitButton1.Tag)
                    {
                        _report_item = (IReportItem_M)z.Tag;
                    }
                }

            }

            if (_report_item != null)
            {
                //Проверим, нужно ли делать мед. проверку сотрудника:
                bool can_med = false;
                ISQLCommand cmd = MainPluginClass.App.SqlWork();
                try
                {
                    cmd.sql = "select waybill_med_checks from autobase.orgs where gid = " + org_id.ToString();
                    cmd.ExecuteReader();
                    if (cmd.CanRead())
                    {
                        can_med = cmd.GetBoolean(0);
                    }
                }
                catch
                {
                    can_med = false;
                }
                cmd.Close();
                if (can_med)
                {
                    try
                    {
                        if (EmployeesSync.Url != null)
                        {
                            EmployeesSync temp = new EmployeesSync();
                            temp.WayBillSyncMain(((myItem)CB_pl_drivertn.SelectedItem).Name);
                        }
                    }
                    catch { }
                }
                try
                {
                    if (this._id != null)
                    {
                        MainPluginClass.Work.FastReport.OpenReport(_report_item, new FilterTable(this._id.Value, MapEditorTablePutList, " gid", ""));
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
        }

        void CB_org_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_org.SelectedItem != null)
            {
                org_id = ((myItem)CB_org.SelectedItem).GetId;

                ISQLCommand sql_frompl = MainPluginClass.App.SqlWork();

                // ЗАГРУЗКА НАЗВАНИЯ сиквенса для путевого листа
                sql_frompl.sql = @"SELECT waybill_name_seq FROM autobase.orgs WHERE gid = " + org_id.ToString();
                org_put_list_seq_name = (string)sql_frompl.ExecuteScalar();
                sql_frompl.Close();
                //sql_frompl = MainPluginClass.App.SqlWork();
            }
            if (_id <= 0 || _id == null)
            {
                // Если мы только создаем путевой лист, то перазагружаем некоторые справочники зависимые от выбора организации
                load_dicts_after_orgs_without_id_waybill();
            }
        }

        void load_dicts_after_orgs_without_id_waybill()
        {
            if (org_id <= 0) return;
            ISQLCommand sql_frompl = MainPluginClass.App.SqlWork();
            ////////////////           ГОСНОМЕРА ГАРАЖНЫЕ НОМЕРА              ////////////////
            CB_gos_no.Items.Clear();
            CB_gar_no.Items.Clear();
            // Отвязываем автомобиль от организации. Бахтин 05,04,2016
            //sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id not in (2,39) AND org_id = " + org_id.ToString() + " AND (length(trim(gos_no))>0 OR length(trim(gar_no))>0) ORDER BY gos_no";
            sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id not in (2,39) AND gid in (select * from  autobase.get_access_cars()) ORDER BY gos_no";
            sql_frompl.ExecuteReader();
            while (sql_frompl.CanRead())
            {
                myItem x = new myItem(sql_frompl.GetString(0) == null ? "-" : sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                myItem y = new myItem(sql_frompl.GetString(1) == null ? "-" : sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                CB_gos_no.Items.Add(x);
                CB_gar_no.Items.Add(y);
            }
            sql_frompl.Close();
            sql_frompl = MainPluginClass.App.SqlWork();

            ////////////////           ГОСНОМЕРА ГАРАЖНЫЕ НОМЕРА  ПРИЦЕПА            ////////////////
            CB_p_gos_no.Items.Clear();
            CB_p_gar_no.Items.Clear();
            // Добавим пустую строку, чтобы можно было "удалить" запись
            CB_p_gos_no.Items.Add(new myItem(" ", -99));
            CB_p_gar_no.Items.Add(new myItem(" ", -99));
            // Отвязываем автомобиль от организации. Бахтин 05,04,2016
            //sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id in (2,39) AND org_id = " + org_id.ToString() + " AND (length(trim(gos_no))>0 OR length(trim(gar_no))>0) ORDER BY gos_no";
            sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id in (2,39) AND gid in (select * from  autobase.get_access_cars()) ORDER BY gos_no";
            sql_frompl.ExecuteReader();
            while (sql_frompl.CanRead())
            {
                myItem x = new myItem(sql_frompl.GetString(0) == null ? "-" : sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                myItem y = new myItem(sql_frompl.GetString(1) == null ? "-" : sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                CB_p_gos_no.Items.Add(x);
                CB_p_gar_no.Items.Add(y);
            }
            sql_frompl.Close();
            sql_frompl = MainPluginClass.App.SqlWork();

            ////////////////           ВОДИТЕЛЬ, + сопровождение               ////////////////
            CB_pl_driverel.Items.Clear();
            CB_pl_drivertn.Items.Clear();
            CB_escort_driverel.Items.Clear();
            CB_escort_drivertn.Items.Clear();
            CB_escort_driverel.Items.Add(new myItem(" ", -99));
            CB_escort_drivertn.Items.Add(new myItem(" ", -99));
            sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), coalesce(e.tab_no,'-') tab_no, e.gid " +
                            "FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg " +
                            "WHERE e.position_id = p.gid and p.group_id = pg.gid and pg.gid = 3 and status_id <> 2 and e.org_id=" + org_id.ToString() + " order by e.lastname";
            sql_frompl.ExecuteReader();
            while (sql_frompl.CanRead())
            {
                myItem x_pl_driver = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                CB_pl_driverel.Items.Add(x_pl_driver);
                CB_escort_driverel.Items.Add(x_pl_driver);
                myItem x_pl_driver2 = new myItem(sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                CB_pl_drivertn.Items.Add(x_pl_driver2);
                CB_escort_drivertn.Items.Add(x_pl_driver2);
            }
            sql_frompl.Close();
            sql_frompl = MainPluginClass.App.SqlWork();

            ////////////////           МАСТЕР               ////////////////
            CB_pl_master.Items.Clear();
            sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), e.gid " +
                            "FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg " +
                            "WHERE e.position_id = p.gid and p.group_id = pg.gid and pg.gid = 1 and status_id <> 2 and e.org_id=" + org_id.ToString() + " order by e.lastname";
            sql_frompl.ExecuteReader();
            while (sql_frompl.CanRead())
            {
                myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                CB_pl_master.Items.Add(x);
            }
            sql_frompl.Close();
            sql_frompl = MainPluginClass.App.SqlWork();

            ////////////////           МАРШРУТ               ////////////////
            CB_route.Items.Clear();
            sql_frompl.sql = "select coalesce(name,'-'), gid from autobase.waybills_routes where org_id = " + org_id.ToString() + " AND length(trim(name)) > 0 order by name";
            sql_frompl.ExecuteReader();
            while (sql_frompl.CanRead())
            {
                myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                CB_route.Items.Add(x);
            }
            sql_frompl.Close();
            sql_frompl = MainPluginClass.App.SqlWork();
        }

        void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        void btn_edit_Click(object sender, EventArgs e)
        {
            isEdited = true;
            UserControlAttr_Load(this, new EventArgs());
            isEdited = true;
            this.PerformLayout();
            enable_all_Controls(this);
            this.ResumeLayout();
        }

        private void enable_all_Controls(Control pControl)
        {
            foreach (Control item in pControl.Controls)
            {
                if (item.Enabled == false && !item.Equals(CB_org))
                    item.Enabled = true;
                if (item.Controls.Count > 0)
                    enable_all_Controls(item);
            }

        }

        void CB_pl_drivertn_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_pl_drivertn.SelectedItem).GetId;
                Set_dict_values(r, CB_pl_driverel);
                CB_pl_drivertn.BackColor = SystemColors.Window;
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора водителя!"); }

        }

        void btn_tax_del_Click(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count > 0 && DataGridView1.SelectedRows[0].IsNewRow == false)
            {
                if (DataGridView1.SelectedRows[0].Cells["oper_gid"].Value != null && (int)DataGridView1.SelectedRows[0].Cells["oper_gid"].Value > 0)
                {
                    taks_to_del.Add((int)DataGridView1.SelectedRows[0].Cells["oper_gid"].Value);
                }
                DataGridView1.Rows.RemoveAt(DataGridView1.SelectedRows[0].Index);
            }
        }

        void btn_tax_add_Click(object sender, EventArgs e)
        {
            if (operacija.Items.Count > 0)
            {
                int k = DataGridView1.Rows.Add();
                {
                    try
                    {
                        int g = ((myItem)operacija.Items[0]).GetId;
                        DataGridView1.Rows[k].Cells["operacija"].Value = g;
                    }
                    catch { }
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить операции для данного ТС!");
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

        /// <summary>
        /// В зависимости от ситуации, разрешаем или запрещаем редактировать те или иные контролы
        /// </summary>
        private void Decoration()
        {
            if (CanEditPutList_isAdmin && secsave >= 1)
            {
                btn_edit.Visible = true;
            }
            else
            {
                btn_edit.Visible = false;
            }
            if (secsave >= 2)
            {
                CHB_calc_norm_type.Enabled = false;
                CB_pl_master.Enabled = false;
                DateTimePicker4.Enabled = false;
                dateTimePicker7.Enabled = false;
                DateTimePicker2.Enabled = false;
                dateTimePicker6.Enabled = false;
                TB_ttd.Enabled = false;
                TB_Ezdok.Enabled = false;
                TB_spk.Enabled = false;
                TB_mch_k.Enabled = false;
                TB_mch_obr_k.Enabled = false;
                TB_t1_k.Enabled = false; // Настраивается checkbox'ом "Остаток по норме"
                TB_t2_k.Enabled = false;
                TB_t1_f.Enabled = false;
                TB_t2_f.Enabled = false;
                btn_tax_add.Enabled = false;
                btn_tax_del.Enabled = false;
                BTN_save.Enabled = false;
                DataGridView1.Enabled = false;
                TB_rab.Enabled = false;
                TB_obed.Enabled = false;
                TB_dejrst.Enabled = false;
                TB_remont.Enabled = false;
                TB_den.Enabled = false;
                TB_noch.Enabled = false;
                TB_stavka.Enabled = false;
                TB_itogo.Enabled = false;
                RTB_comm.Enabled = false;
                CB_route.Enabled = false;
                CB_wrktype.Enabled = false;
                CB_gruztype.Enabled = false;
                CB_pl_zone.Enabled = false;
                TB_sp_n.Enabled = false;
                TB_mch.Enabled = false;
                TB_t1_pl.Enabled = false;
                TB_t1_n.Enabled = false;
                TB_t2_pl.Enabled = false;
                TB_t2_n.Enabled = false;
                CB_fuel_card.Enabled = false;
                CB_fuel_card2.Enabled = false;
                TB_mch_obr.Enabled = false;
                TB_tax_drain.Enabled = false;
            }
            else if (secsave == 1)
            {
                CB_fuel_card.Enabled = true;
                CHB_calc_norm_type.Enabled = true;
                TB_sp_n.BackColor = Color.FromArgb(255, 224, 192);
                TB_mch.BackColor = Color.FromArgb(255, 224, 192);
                TB_mch_obr.BackColor = Color.FromArgb(255, 224, 192);
                CB_pl_master.BackColor = Color.FromArgb(255, 224, 192);
                DateTimePicker4.BackColor = Color.FromArgb(255, 224, 192);
                dateTimePicker7.BackColor = Color.FromArgb(255, 224, 192);
                DateTimePicker2.BackColor = Color.FromArgb(255, 224, 192);
                dateTimePicker6.BackColor = Color.FromArgb(255, 224, 192);
                CB_route.BackColor = Color.FromArgb(255, 224, 192);
                CB_wrktype.BackColor = Color.FromArgb(255, 224, 192);
                CB_gruztype.BackColor = Color.FromArgb(255, 224, 192);
                CB_pl_zone.BackColor = Color.FromArgb(255, 224, 192);
                TB_Ezdok.BackColor = Color.FromArgb(255, 224, 192);
                if (TB_sp_n.Text != null && !TB_sp_n.Text.Equals("0") && !TB_sp_n.Text.Equals(""))
                {
                    TB_spk.BackColor = Color.FromArgb(255, 224, 192);
                }
                if (TB_mch.Text != null && !TB_mch.Text.Equals("0") && !TB_mch.Text.Equals(""))
                {
                    TB_mch_k.BackColor = Color.FromArgb(255, 224, 192);
                }
                if (TB_mch_obr.Text != null && !TB_mch_obr.Text.Equals("0") && !TB_mch_obr.Text.Equals(""))
                {
                    TB_mch_obr_k.BackColor = Color.FromArgb(255, 224, 192);
                }
                TB_t1_k.BackColor = Color.FromArgb(255, 224, 192);
                TB_t2_k.BackColor = Color.FromArgb(255, 224, 192);
                TB_t1_f.BackColor = Color.FromArgb(255, 224, 192);
                TB_t2_f.BackColor = Color.FromArgb(255, 224, 192);
                TB_t1_pl.BackColor = Color.FromArgb(255, 224, 192);
                TB_t1_n.BackColor = Color.FromArgb(255, 224, 192);
                TB_t2_pl.BackColor = Color.FromArgb(255, 224, 192);
                TB_t2_n.BackColor = Color.FromArgb(255, 224, 192);
                CB_pl_master.Enabled = true;
                DateTimePicker4.Enabled = true;
                dateTimePicker7.Enabled = true;
                DateTimePicker2.Enabled = true;
                dateTimePicker6.Enabled = true;
                TB_ttd.Enabled = true;
                TB_Ezdok.Enabled = true;
                TB_spk.Enabled = true;

                TB_mch_k.Enabled = true;
                TB_mch_k.BackColor = Color.FromArgb(255, 224, 192);
                TB_mch_obr_k.Enabled = true;
                TB_mch_obr_k.BackColor = Color.FromArgb(255, 224, 192);
                TB_t1_k.Enabled = !CHB_calc_norm_type.Checked;
                if (CB_top2.SelectedItem != null && !CB_top2.SelectedItem.ToString().ToUpper().Trim().Equals("НЕТ"))
                {
                    TB_t2_k.Enabled = true;
                    TB_t2_f.Enabled = true;
                }
                TB_t1_f.Enabled = true;
                btn_tax_add.Enabled = true;
                btn_tax_del.Enabled = true;
                TB_tax_drain.Enabled = true;
            }
            if (_id != null)
            {
                CB_status.Enabled = false;
                CB_pl_driverel.BackColor = SystemColors.Window;
                CB_pl_drivertn.BackColor = SystemColors.Window;
                CB_gos_no.BackColor = SystemColors.Window;
                CB_gar_no.BackColor = SystemColors.Window;
                CB_rrab.BackColor = SystemColors.Window;
                CB_route.BackColor = SystemColors.Window;
                CB_wrktype.BackColor = SystemColors.Window;
                CB_gruztype.BackColor = SystemColors.Window;
                CB_pl_zone.BackColor = SystemColors.Window;
                TB_sp_n.BackColor = SystemColors.Window;
                TB_mch.BackColor = SystemColors.Window;
                TB_mch_obr.BackColor = SystemColors.Window;
                CB_top1.BackColor = SystemColors.Window;
                TB_t1_pl.BackColor = SystemColors.Window;
                TB_t1_n.BackColor = SystemColors.Window;
                CB_top2.BackColor = SystemColors.Window;
                TB_t2_pl.BackColor = SystemColors.Window;
                TB_t2_n.BackColor = SystemColors.Window;
                DateTimePicker4.Visible = true;
                dateTimePicker7.Visible = true;
                DateTimePicker2.Visible = true;
                dateTimePicker6.Visible = true;
                Label6.Visible = true;
                Label4.Visible = true;
                CB_gar_no.Enabled = false;
                CB_gos_no.Enabled = false;
                CB_p_gar_no.Enabled = false;
                CB_p_gos_no.Enabled = false;
                CB_escort_driverel.Enabled = false;
                CB_escort_drivertn.Enabled = false;
                CB_pl_driverel.Enabled = false;
                CB_pl_drivertn.Enabled = false;
                CB_rrab.Enabled = false;
                CB_top1.Enabled = false;
                CB_top2.Enabled = false;
                CB_notes.Enabled = false;
                TB_brig.Enabled = false;
                TB_col.Enabled = false;
                splitButton1.Enabled = true;
                DateTimePicker1.Enabled = false;
                dateTimePicker5.Enabled = false;
                DateTimePicker3.Enabled = false;
                dateTimePicker8.Enabled = false;
            }
            else
            {
                CHB_calc_norm_type.Enabled = false;
                CB_fuel_card.Enabled = false;
                CB_fuel_card2.Enabled = false;
                btn_tax_add.Enabled = false;
                btn_tax_del.Enabled = false;
                TB_spk.Enabled = false;
                TB_mch_k.Enabled = false;
                TB_mch_obr_k.Enabled = false;
                TB_t1_k.Enabled = false; // Настраивается checkbox'ом "Остаток по норме"
                TB_t2_k.Enabled = false;
                TB_t1_f.Enabled = false;
                TB_t2_f.Enabled = false;
                splitButton1.Enabled = false;
                btn_tax_del.Enabled = false;
                DateTimePicker1.Value = DateTime.Today;
                DateTimePicker3.Value = DateTime.Today.AddDays(+1);
                DateTimePicker4.Value = DateTimePicker3.Value;
                DateTimePicker2.Value = DateTimePicker1.Value;
                dateTimePicker5.Value = DateTimePicker1.Value.AddHours(8);
                dateTimePicker6.Value = dateTimePicker5.Value;
                dateTimePicker8.Value = DateTimePicker3.Value.AddHours(8);
                dateTimePicker7.Value = dateTimePicker8.Value;
                TB_tax_drain.Enabled = false;
            }
            CB_status.Enabled = false;
            // Разрешение сохранять путевой лист
            if (!MainPluginClass.App.getWriteTable(260)) // В данном случае вьюшка v_waybills
            {
                BTN_save.Enabled = false;
            }
            // Разрешение сохранять таксировку
            if (!MainPluginClass.App.getWriteTable(174)) // таблица waybills_taks
            {
                btn_tax_add.Enabled = false;
                btn_tax_del.Enabled = false;
            }
        }

        private void UserControlAttr_Load(object sender, EventArgs e)
        {
            this.ParentForm.PerformLayout();
            this.ParentForm.Text = "Путевой лист";
            this.ParentForm.MinimumSize = new Size(this.ParentForm.Size.Width, 0);
            taks_to_del = new List<int>();
            // Поправим положение на экране
            CorrectPosition();

            // Определение переменных для последующей установки элементов справчочника
            int id_avt = 0;
            int id_pl_driver = 0;
            int id_escort_driver = 0;
            int id_topl = 0;
            int id_marsh = 0;
            int id_work = 0;
            int id_gruz = 0;
            int id_mark = 0;
            int id_pl_zone = 0;
            int id_pl_master = 0;
            int id_top2 = 0;
            int id_rrab = 0;
            int id_trailer = 0;
            int id_org = 0;


            ISQLCommand sql_frompl = MainPluginClass.App.SqlWork();


            if (_id != null)
            {
                // alter table autobase.waybills add column escort_driver_id integer;
                sql_frompl.sql = "select doc_no, date(date_out_plan), date(date_out_fact), date(date_in_plan), date(date_in_plan), " +
                "motorcade, brigade, ttd_count, trip_count, km_begin, km_end, mh_begin," +
                "mh_end, mh_ob_begin, mh_ob_end , fuel_plan, fuel_fact, " +
                "fuel_begin, fuel_end, pay_work_h,pay_lunch_h, pay_duty_h, " +
                "pay_repair_h, pay_day_h, pay_night_h, pay_rate_rh, pay_total_r, notes, fuel_plan2, " +
                "fuel_fact2, fuel_begin2, fuel_end2, secondsave, date_out_plan, date_out_fact, date_in_plan, date_in_fact, support_persons, fuel_card_id, " +
                "car_id, driver_id, fuel_mark_id, route_id, work_type_id, cargo_type_id, " +
                "special_note_id, road_type_id, automaster_id, fuel_mark2_id, work_regime_id, trailer_id, km_run_glonass,mh_run_glonass,mh_ob_run_glonass, " +
                "fuel_fact_glonass,fuel_begin_glonass,fuel_end_glonass,fuel_fact2_glonass,fuel_begin2_glonass,fuel_end2_glonass,fuel_card2_id,org_id,calc_fuel_drain, calc_norm_type, escort_driver_id " +
                "FROM autobase.waybills WHERE gid = " + _id;
                try
                {
                    sql_frompl.ExecuteReader();
                    if (sql_frompl.CanRead())
                    {
                        TextBox1.Text = sql_frompl.GetString(0);
                        DateTimePicker1.Value = DateTime.Parse(sql_frompl.GetString(1));
                        DateTimePicker2.Value = DateTime.Parse(sql_frompl.GetString(2));
                        DateTimePicker3.Value = DateTime.Parse(sql_frompl.GetString(3));
                        DateTimePicker4.Value = DateTime.Parse(sql_frompl.GetString(4));
                        dateTimePicker5.Value = DateTime.Parse(sql_frompl.GetString(33));
                        dateTimePicker6.Value = DateTime.Parse(sql_frompl.GetString(34));
                        dateTimePicker7.Value = DateTime.Parse(sql_frompl.GetString(36));
                        dateTimePicker8.Value = DateTime.Parse(sql_frompl.GetString(35));
                        TB_col.Text = sql_frompl.GetString(5);
                        TB_brig.Text = sql_frompl.GetString(6);
                        TB_ttd.Text = sql_frompl.GetString(7);
                        TB_Ezdok.Text = sql_frompl.GetString(8);
                        if (sql_frompl.GetString(9) != null) { TB_sp_n.Text = (Convert.ToDecimal(sql_frompl.GetString(9).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(10) != null) { TB_spk.Text = (Convert.ToDecimal(sql_frompl.GetString(10).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(11) != null) { TB_mch.Text = (Convert.ToDecimal(sql_frompl.GetString(11).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(12) != null) { TB_mch_k.Text = (Convert.ToDecimal(sql_frompl.GetString(12).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(13) != null) { TB_mch_obr.Text = (Convert.ToDecimal(sql_frompl.GetString(13).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(14) != null) { TB_mch_obr_k.Text = (Convert.ToDecimal(sql_frompl.GetString(14).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(15) != null) { TB_t1_pl.Text = (Convert.ToDecimal(sql_frompl.GetString(15).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(16) != null) { TB_t1_f.Text = (Convert.ToDecimal(sql_frompl.GetString(16).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(17) != null) { TB_t1_n.Text = (Convert.ToDecimal(sql_frompl.GetString(17).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(18) != null) { TB_t1_k.Text = (Convert.ToDecimal(sql_frompl.GetString(18).Replace(".", _decSeparator))).ToString(); };

                        if (sql_frompl.GetString(19) != null) { TB_rab.Text = (Convert.ToDecimal(sql_frompl.GetString(19).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(20) != null) { TB_obed.Text = (Convert.ToDecimal(sql_frompl.GetString(20).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(21) != null) { TB_dejrst.Text = (Convert.ToDecimal(sql_frompl.GetString(21).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(22) != null) { TB_remont.Text = (Convert.ToDecimal(sql_frompl.GetString(22).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(23) != null) { TB_den.Text = (Convert.ToDecimal(sql_frompl.GetString(23).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(24) != null) { TB_noch.Text = (Convert.ToDecimal(sql_frompl.GetString(24).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(25) != null) { TB_stavka.Text = (Convert.ToDecimal(sql_frompl.GetString(25).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(26) != null) { TB_itogo.Text = (Convert.ToDecimal(sql_frompl.GetString(26).Replace(".", _decSeparator))).ToString(); };

                        RTB_comm.Text = sql_frompl.GetString(27);
                        if (sql_frompl.GetString(28) != null) { TB_t2_pl.Text = (Convert.ToDecimal(sql_frompl.GetString(28).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(29) != null) { TB_t2_f.Text = (Convert.ToDecimal(sql_frompl.GetString(29).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(30) != null) { TB_t2_n.Text = (Convert.ToDecimal(sql_frompl.GetString(30).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetString(31) != null) { TB_t2_k.Text = (Convert.ToDecimal(sql_frompl.GetString(31).Replace(".", _decSeparator))).ToString(); };
                        if (sql_frompl.GetValue("calc_norm_type") != null) { CHB_calc_norm_type.Checked = sql_frompl.GetInt32("calc_norm_type") > 0 ? true : false; }

                        TB_sp_glprob.Text = sql_frompl.GetString(51);
                        //TextBox23.Text = sql_frompl.GetString(52);
                        //TextBox27.Text = sql_frompl.GetString(53);
                        TextBox35.Text = sql_frompl.GetString(54);
                        TextBox36.Text = sql_frompl.GetString(55);
                        TextBox37.Text = sql_frompl.GetString(56);
                        TextBox40.Text = sql_frompl.GetString(57);
                        TextBox39.Text = sql_frompl.GetString(58);
                        TextBox38.Text = sql_frompl.GetString(59);
                        TB_tax_drain.Text = sql_frompl.GetString("calc_fuel_drain");

                        CB_fuel_card.Tag = sql_frompl.GetInt32(38);
                        CB_fuel_card2.Tag = sql_frompl.GetInt32(60);
                        secsave = sql_frompl.GetInt32(32);
                        id_avt = sql_frompl.GetInt32(39);
                        base_car_id = id_avt;
                        id_pl_driver = sql_frompl.GetInt32(40);
                        id_escort_driver = sql_frompl.GetInt32(64);
                        id_topl = sql_frompl.GetInt32(41);
                        id_marsh = sql_frompl.GetInt32(42);
                        id_work = sql_frompl.GetInt32(43);
                        id_gruz = sql_frompl.GetInt32(44);
                        id_mark = sql_frompl.GetInt32(45);
                        id_pl_zone = sql_frompl.GetInt32(46);
                        id_pl_master = sql_frompl.GetInt32(47);
                        id_top2 = sql_frompl.GetInt32(48);
                        id_rrab = sql_frompl.GetInt32(49);
                        id_trailer = sql_frompl.GetInt32(50);
                        id_org = sql_frompl.GetInt32(61);
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();
                if (secsave == 1) //На этапе закрытия путевого листа, время возвращения факт ставим плановому. Тоже для выезда.
                {
                    DateTimePicker4.Value = DateTimePicker3.Value;
                    dateTimePicker7.Value = dateTimePicker8.Value;
                    DateTimePicker2.Value = DateTimePicker1.Value;
                    dateTimePicker6.Value = dateTimePicker5.Value;
                }

            }
            // Загрузка сопутствующих справочных элементов
            try
            {
                // Попытка установить право редактирование путевого листа
                if (_id != null)
                {
                    sql_frompl.sql = @"SELECT * FROM autobase.is_admin_waybills()";
                    if (sql_frompl.ExecuteReader() && sql_frompl.CanRead())
                    {
                        CanEditPutList_isAdmin = sql_frompl.GetInt32(0) == 1;
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();
                }
                else
                    CanEditPutList_isAdmin = false;

                CB_org.Items.Clear();
                sql_frompl.sql = @"SELECT gid, coalesce(name,'-') FROM autobase.orgs WHERE gid in (SELECT autobase.get_access_orgs())" + (id_org > 0 ? " or gid =" + id_org.ToString() : "");
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(1), sql_frompl.GetInt32(0));
                    CB_org.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();
                // Если организация одна, то сразу устанавливаем ее
                if (CB_org.Items.Count == 1)
                {
                    CB_org.SelectedIndex = 0;
                    CB_org.Enabled = false;
                }
                // Или если мы загружаем пут. лист.
                if (id_org > 0)
                {
                    foreach (myItem item in CB_org.Items)
                    {
                        if (item.GetId == id_org)
                        {
                            CB_org.SelectedItem = item;
                            CB_org.Enabled = false;
                            break;
                        }
                    }
                }
                // Если мы загружаем пут. луст, то после установки организации, справочники не подгрузятся (_id > 0)
                // Для того, чтобы загружая справочники, учесть тот факт, что некоторые присоединенные к организации "элементы" типа водителей или машин
                // на момент загрузки пут. листа, могут быть уже откреплены, но их нам все же надо загрузить.
                //
                // UPD. Загружаем только установленные эелементы, кроме тех, которые выбираются на этапе открытого путевого листа, т.е. для его закрытия
                if (_id != null && id_org > 0)
                {

                    ////////////////           ГОСНОМЕРА ГАРАЖНЫЕ НОМЕРА              ////////////////
                    CB_gos_no.Items.Clear();
                    CB_gar_no.Items.Clear();
                    if (isEdited || secsave < 1)
                    {
                        // Отвязываем от организации. Бахтин 05,04,2016
                        //sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id not in (2,39) AND org_id = " + id_org.ToString() + " AND (length(trim(gos_no))>0 OR length(trim(gar_no))>0) " + (id_avt > 0 ? "or gid = " + id_avt.ToString() : "") + " ORDER BY gos_no";
                        sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id not in (2,39) and gid in (select * from autobase.get_access_cars()) " + (id_avt > 0 ? "or gid = " + id_avt.ToString() : "") + " ORDER BY gos_no";
                    }
                    else
                    {
                        sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where gid = " + id_avt.ToString();
                    }
                    sql_frompl.ExecuteReader();
                    while (sql_frompl.CanRead())
                    {
                        myItem x = new myItem(sql_frompl.GetString(0) == null ? "-" : sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                        myItem y = new myItem(sql_frompl.GetString(1) == null ? "-" : sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                        CB_gos_no.Items.Add(x);
                        CB_gar_no.Items.Add(y);
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();

                    ////////////////           ГОСНОМЕРА ГАРАЖНЫЕ НОМЕРА  ПРИЦЕПА            ////////////////
                    CB_p_gos_no.Items.Clear();
                    CB_p_gar_no.Items.Clear();
                    CB_p_gos_no.Items.Add(new myItem(" ", -99));
                    CB_p_gar_no.Items.Add(new myItem(" ", -99));
                    if (isEdited || secsave < 1)
                    {
                        // Отвязываем от организации. Бахтин 05,04,2016
                        //sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id in (2,39) AND org_id = " + id_org.ToString() + " AND (length(trim(gos_no))>0 OR length(trim(gar_no))>0) " + (id_trailer > 0 ? "or gid = " + id_trailer.ToString() : "") + " ORDER BY gos_no";
                        sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where type_id in (2,39) and gid in (select * from autobase.get_access_cars())" + (id_trailer > 0 ? "or gid = " + id_trailer.ToString() : "") + " ORDER BY gos_no";
                    }
                    else
                    {
                        sql_frompl.sql = "select coalesce(gos_no,'-'), coalesce(gar_no,'-'), gid from autobase.cars where gid = " + id_trailer.ToString();
                    }
                    sql_frompl.ExecuteReader();
                    while (sql_frompl.CanRead())
                    {
                        myItem x = new myItem(sql_frompl.GetString(0) == null ? "-" : sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                        myItem y = new myItem(sql_frompl.GetString(1) == null ? "-" : sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                        CB_p_gos_no.Items.Add(x);
                        CB_p_gar_no.Items.Add(y);
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();

                    ////////////////           ВОДИТЕЛЬ               ////////////////
                    CB_pl_driverel.Items.Clear();
                    CB_pl_drivertn.Items.Clear();
                    CB_escort_driverel.Items.Clear();
                    CB_escort_drivertn.Items.Clear();
                    CB_escort_driverel.Items.Add(new myItem(" ", -99));
                    CB_escort_drivertn.Items.Add(new myItem(" ", -99));
                    if (isEdited || secsave < 1)
                    {
                        sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), coalesce(e.tab_no,'-') tab_no, e.gid " +
                                "FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg " +
                                "WHERE e.position_id = p.gid and p.group_id = pg.gid and ((pg.gid = 3 and status_id <> 2 and e.org_id=" + org_id.ToString() + (id_pl_driver > 0 ? ") or e.gid =" + id_pl_driver.ToString() + ") order by e.lastname" : ")) order by e.lastname");
                    }
                    else
                    {
                        sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), coalesce(e.tab_no,'-') tab_no, e.gid " +
                            "FROM autobase.employees e WHERE e.gid in (" + id_pl_driver.ToString() + ", " + id_escort_driver.ToString() + ")";
                    }
                    sql_frompl.ExecuteReader();
                    while (sql_frompl.CanRead())
                    {
                        myItem x_pl_driver = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(2));
                        CB_pl_driverel.Items.Add(x_pl_driver);
                        CB_escort_driverel.Items.Add(x_pl_driver);
                        myItem x_pl_driver2 = new myItem(sql_frompl.GetString(1), sql_frompl.GetInt32(2));
                        CB_pl_drivertn.Items.Add(x_pl_driver2);
                        CB_escort_drivertn.Items.Add(x_pl_driver2);
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();

                    ////////////////           МАСТЕР               ////////////////
                    CB_pl_master.Items.Clear();
                    if (isEdited || secsave <= 1)
                    {
                        sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), e.gid " +
                                "FROM autobase.employees e, autobase.employees_positions p, autobase.employees_positions_groups pg " +
                                "WHERE e.position_id = p.gid and p.group_id = pg.gid and ((pg.gid = 1 and status_id <> 2 and e.org_id=" + org_id.ToString() + (id_pl_master > 0 ? ") or e.gid =" + id_pl_master.ToString() + ") order by e.lastname" : ")) order by e.lastname");
                    }
                    else {
                        sql_frompl.sql = "SELECT coalesce(e.lastname,'') ||' '|| coalesce(e.firstname,'') ||' '|| coalesce(e.middlename,''), e.gid " +
                                "FROM autobase.employees e WHERE e.gid =" + id_pl_master.ToString();
                    }
                    sql_frompl.ExecuteReader();
                    while (sql_frompl.CanRead())
                    {
                        myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                        CB_pl_master.Items.Add(x);
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();

                    ////////////////           МАРШРУТ               ////////////////
                    CB_route.Items.Clear();
                    if (isEdited || secsave <= 1)
                    {
                        sql_frompl.sql = "select coalesce(name,'-'), gid from autobase.waybills_routes where org_id = " + id_org.ToString() + " AND length(trim(name)) > 0 " + (id_marsh > 0 ? " or gid = " + id_marsh.ToString() : "") + " order by name";
                    }
                    else {
                        sql_frompl.sql = "select coalesce(name,'-'), gid from autobase.waybills_routes where gid = " + id_marsh.ToString();
                    }
                    sql_frompl.ExecuteReader();
                    while (sql_frompl.CanRead())
                    {
                        myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                        CB_route.Items.Add(x);
                    }
                    sql_frompl.Close();
                    sql_frompl = MainPluginClass.App.SqlWork();
                }
                ////////////////           СТАТУС П.ЛИСТА            ////////////////
                CB_status.Items.Clear();
                sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_statuses order by name";
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                    CB_status.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////           ВИДЫ РАБОТ               ////////////////
                CB_wrktype.Items.Clear();
                sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_work_types order by name";
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                    CB_wrktype.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////           ВИДЫ ГРУЗА               ////////////////
                CB_gruztype.Items.Clear();
                if (isEdited || secsave <= 1)
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_cargo_types where length(trim(name)) > 0 " + (id_gruz > 0 ? " or gid =" + id_gruz.ToString() : "");
                }
                else {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_cargo_types where gid =" + id_gruz.ToString();
                }
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                    CB_gruztype.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////           РЕЖИМ РАБОТЫ               ////////////////
                CB_rrab.Items.Clear();
                if (isEdited || secsave < 1)
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid, hours from  autobase.waybills_work_regimes where length(trim(name)) > 0 " + (id_rrab > 0 ? " or gid =" + id_rrab.ToString() : "");
                }
                else {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid, hours from  autobase.waybills_work_regimes where gid =" + id_rrab.ToString();
                }
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1), sql_frompl.GetInt32(2));
                    CB_rrab.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////           ВИДЫ ТОПЛИВА (1,2)       ////////////////
                CB_top1.Items.Clear();
                CB_top2.Items.Clear();
                if (isEdited || secsave < 1)
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_fuel_marks where length(trim(name)) > 0 " + (id_topl > 0 ? " or gid =" + id_topl.ToString() : "") + (id_top2 > 0 ? " or gid =" + id_top2.ToString() : "");
                }
                else
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_fuel_marks where gid in (" + id_topl.ToString() + "," + id_top2.ToString() + ")";
                }
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1)); // не загружать вид топливва "нет"
                    if (sql_frompl.GetString(0) != null && !sql_frompl.GetString(0).ToLower().Equals("нет"))
                    {
                        CB_top1.Items.Add(x);
                    }
                    CB_top2.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////           ОСОБЫЕ ОТМЕТКИ               ////////////////
                CB_notes.Items.Clear();
                if (isEdited || secsave < 1)
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_special_notes where length(trim(name)) > 0 " + (id_mark > 0 ? " or gid =" + id_mark.ToString() : "");
                }
                else {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_special_notes where gid =" + id_mark.ToString();
                }
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                    CB_notes.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                ////////////////          ЗОНЫ               ////////////////
                CB_pl_zone.Items.Clear();
                if (isEdited || secsave <= 1)
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_road_types where length(trim(name)) > 0 " + (id_pl_zone > 0 ? " or gid =" + id_pl_zone.ToString() : "");
                }
                else
                {
                    sql_frompl.sql = "SELECT coalesce(name,'-'), gid from autobase.waybills_road_types where gid =" + id_pl_zone.ToString();
                }
                sql_frompl.ExecuteReader();
                while (sql_frompl.CanRead())
                {
                    myItem x = new myItem(sql_frompl.GetString(0), sql_frompl.GetInt32(1));
                    CB_pl_zone.Items.Add(x);
                }
                sql_frompl.Close();
                sql_frompl = MainPluginClass.App.SqlWork();

                norma__l__spravochnik_.ReadOnly = true;
                itogo__l__schitaetsja_avtomatom_.ReadOnly = true;
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить данные " + x.Message, "Ошибка загрузки!"); }
            sql_frompl.Close();

            Set_dict_values(id_pl_driver, CB_pl_driverel);
            Set_dict_values(id_escort_driver, CB_escort_driverel);
            Set_dict_values(id_rrab, CB_rrab);
            Set_dict_values(id_trailer, CB_p_gos_no);
            Set_dict_values(id_pl_master, CB_pl_master);
            Set_dict_values(id_marsh, CB_route);
            Set_dict_values(id_work, CB_wrktype);
            Set_dict_values(id_gruz, CB_gruztype);
            Set_dict_values(id_mark, CB_notes);
            Set_dict_values(id_pl_zone, CB_pl_zone);
            Set_dict_values(id_avt, CB_gos_no);
            if (CB_gos_no.SelectedItem != null && operacija.Items.Count <= 0) TB_modelid_TextChanged(null, new EventArgs());
            Set_dict_values(id_topl, CB_top1);
            Set_dict_values(id_top2, CB_top2);
            Set_dict_values(secsave, CB_status);

            isEdited = false;

            Decoration();
            this.ParentForm.ResumeLayout();
            if (CB_gos_no.SelectedItem != null)
            {
                carExternalId = GetExternalId(((myItem)CB_gos_no.SelectedItem).GetId, "cars");
                create_task_btn.Visible = (carExternalId != null && carExternalId.Value > 0) && MainPluginClass.OrgsIds.Contains(org_id.ToString()); //&& MainPluginClass.OrgsIds.Contains(org_id.ToString()
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == 1 && CB_org.SelectedItem != null)
            {
                // Загрузка нормы для выбранной операции
                DataGridViewCell norma = DataGridView1.Rows[e.RowIndex].Cells["norma__l__spravochnik_"];
                string oper = DataGridView1.Rows[e.RowIndex].Cells["operacija"].Value.ToString();

                // ИД автомобиля
                int r = ((myItem)CB_gos_no.SelectedItem).GetId;

                //Узнаем время года
                bool sezon_leto = false;
                var sql_sezon = MainPluginClass.App.SqlWork();
                sql_sezon.sql = "select season_id from autobase.waybills_seasons_regulations where (current_date >= begin_date) and (current_date < end_date) and org_id = " + ((myItem)CB_org.SelectedItem).GetId.ToString();
                try
                {
                    sql_sezon.ExecuteReader();
                    if (sql_sezon.CanRead())
                    {
                        if (sql_sezon.GetInt32(0) == 1)
                        {
                            sezon_leto = true;
                        }
                        else
                        {
                            sezon_leto = false;
                        }
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить время года. Возможно есть пересечения!" + x.Message, "Ошибка таксировки!"); return; }
                sql_sezon.Close();

                //
                var sql_norma = MainPluginClass.App.SqlWork();
                sql_norma.sql = "select value_summer, value_winter";
                sql_norma.sql += " from  autobase.waybills_fuel_expenses where";
                sql_norma.sql += " org_id = " + ((myItem)CB_org.SelectedItem).GetId.ToString();
                sql_norma.sql += " and car_id = " + r.ToString();
                sql_norma.sql += " and operation_id = " + oper;
                sql_norma.sql += " union select value_summer, value_winter";
                sql_norma.sql += " from autobase.waybills_fuel_expenses where";
                sql_norma.sql += " org_id = " + ((myItem)CB_org.SelectedItem).GetId.ToString();
                sql_norma.sql += " and model_id = " + TB_modelid.Text;
                sql_norma.sql += " and operation_id = " + oper;
                sql_norma.sql += " and operation_id not in (select operation_id from autobase.waybills_fuel_expenses where org_id = " +
                    ((myItem)CB_org.SelectedItem).GetId.ToString() + " and car_id = " + r.ToString() + " and operation_id = " + oper + ")";
                try
                {
                    sql_norma.ExecuteReader();
                    while (sql_norma.CanRead())
                    {
                        norma.Value = sezon_leto ? sql_norma.GetString(0) : sql_norma.GetString(1); // Для зимы и лета - по разному однако
                        break;
                    }
                    if (norma.Value == null) { norma.Value = 0; };
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно загрузить норму" + x.Message, "Ошибка таксировки!"); }
                sql_norma.Close();
            };
            try
            {

                if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
                {
                    DataGridViewCell itogo = DataGridView1.Rows[e.RowIndex].Cells["itogo__l__schitaetsja_avtomatom_"];
                    DataGridViewCell norma = DataGridView1.Rows[e.RowIndex].Cells["norma__l__spravochnik_"];
                    DataGridViewCell znach = DataGridView1.Rows[e.RowIndex].Cells["znachenie"];
                    if (znach.Value != null && norma.Value != null)
                    {
                        itogo.Value = Convert.ToDecimal(norma.Value.ToString().Replace(".", _decSeparator)) * Convert.ToDecimal(znach.Value.ToString().Replace(".", _decSeparator));
                    }
                    else
                    {
                        itogo.Value = 0;
                    }
                }
                decimal sum = 0;
                foreach (DataGridViewRow row in DataGridView1.Rows)
                {
                    if (row.Cells["znachenie"].Value != null && row.Cells["norma__l__spravochnik_"].Value != null)
                    {
                        sum += Convert.ToDecimal(row.Cells["itogo__l__schitaetsja_avtomatom_"].Value.ToString().Replace(".", _decSeparator));
                    }
                }
                TB_tax_n.Text = sum.ToString();
            }
            catch
            {
                DataGridView1.Rows[e.RowIndex].Cells["znachenie"].Value = "0";
                TB_tax_n.Text = "0";
            }
        }

        private void Set_dict_values(int selected_prm, ComboBox set_value)
        {
            foreach (object x in set_value.Items)
            {
                if (((myItem)x).GetId == selected_prm)
                {
                    set_value.SelectedItem = x;
                    break;
                }
            }
        }

        private void CB_pl_driverel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_pl_driverel.SelectedItem).GetId;
                Set_dict_values(r, CB_pl_drivertn);
                CB_pl_driverel.BackColor = SystemColors.Window;
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора водителя!"); }
        }

        private void CB_gos_no_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (_id != null && CB_gos_no.SelectedItem != null && base_car_id > 0 && ((myItem)CB_gos_no.SelectedItem).GetId != base_car_id)
            {
                DialogResult dr = MessageBox.Show("Заменив ТС Вы потеряете все данные!\r\nПродолжить?", "Внимание", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                {
                    CB_gos_no.SelectedIndexChanged -= CB_gos_no_SelectedIndexChanged_1;
                    CB_gar_no.SelectedIndexChanged -= CB_gar_no_SelectedIndexChanged;
                    Set_dict_values(base_car_id, CB_gos_no);
                    Set_dict_values(base_car_id, CB_gar_no);
                    CB_gos_no.SelectedIndexChanged += CB_gos_no_SelectedIndexChanged_1;
                    CB_gar_no.SelectedIndexChanged += CB_gar_no_SelectedIndexChanged;
                    return;
                }
                base_car_id = -1;

                norma_sp = "";
                norma_mch = "";
                norma_mchobr = "";
                norma_t1 = "";
                norma_t2 = "";
                TB_sp_n.Text = "";
                TB_mch.Text = "";
                TB_mch_obr.Text = "";
                TB_t1_n.Text = "";
                TB_t2_n.Text = "";
                CB_top1.SelectedIndexChanged -= CB_top1_SelectedIndexChanged;
                CB_top1.SelectedIndex = -1;
                CB_top1.SelectedItem = null;
                CB_top1.SelectedIndexChanged += CB_top1_SelectedIndexChanged;
                CB_top2.SelectedIndexChanged -= CB_top2_SelectedIndexChanged;
                CB_top2.SelectedItem = null;
                CB_top2.SelectedIndex = -1;
                CB_top2.SelectedIndexChanged += CB_top2_SelectedIndexChanged;

                CB_pl_driverel.SelectedIndexChanged -= CB_pl_driverel_SelectedIndexChanged;
                CB_pl_driverel.SelectedItem = null;
                CB_pl_driverel.SelectedIndex = -1;
                CB_pl_driverel.SelectedIndexChanged += CB_pl_driverel_SelectedIndexChanged;
                CB_pl_drivertn.SelectedIndexChanged -= CB_pl_drivertn_SelectedIndexChanged;
                CB_pl_drivertn.SelectedItem = null;
                CB_pl_drivertn.SelectedIndex = -1;
                CB_pl_drivertn.SelectedIndexChanged += CB_pl_drivertn_SelectedIndexChanged;

                CB_p_gos_no.SelectedIndexChanged -= CB_p_gos_no_SelectedIndexChanged;
                CB_p_gos_no.SelectedItem = null;
                CB_p_gos_no.SelectedIndex = -1;
                CB_p_gos_no.SelectedIndexChanged += CB_p_gos_no_SelectedIndexChanged;
                CB_p_gar_no.SelectedIndexChanged -= CB_p_gar_no_SelectedIndexChanged;
                CB_p_gar_no.SelectedItem = null;
                CB_p_gar_no.SelectedIndex = -1;
                CB_p_gar_no.SelectedIndexChanged += CB_p_gar_no_SelectedIndexChanged;

                CB_fuel_card.SelectedItem = null;
                CB_fuel_card.SelectedIndex = -1;
                CB_fuel_card2.SelectedItem = null;
                CB_fuel_card2.SelectedIndex = -1;

                TB_spk.Text = "";
                TB_sp_glprob.Text = "";
                TB_sp_prob.Text = "";
                TB_mch_k.Text = "";
                TB_mch_pr.Text = "";
                TB_mch_obr_k.Text = "";
                TB_mch_obr_pr.Text = "";
                TB_t1_pl.Text = "";
                TB_t1_f.Text = "";
                TextBox35.Text = "";
                TB_t1_k.Text = "";
                TB_t2_pl.Text = "";
                TB_t2_f.Text = "";
                TextBox40.Text = "";
                TB_t2_k.Text = "";
                TextBox38.Text = "";
                TextBox39.Text = "";
                TB_tax_drain.Text = "";
                TB_tax_km_delta.Text = "";
                TB_tax_mh_delta.Text = "";
                TB_tax_n.Text = "";
                TB_tax_r.Text = "";
                TB_tax_f.Text = "";

                foreach (DataGridViewRow item in DataGridView1.Rows)
                {
                    taks_to_del.Add((int)item.Cells["oper_gid"].Value);
                    DataGridView1.Rows.RemoveAt(item.Index);
                }
            }
            var sql_ts_details = MainPluginClass.App.SqlWork();
            var sql_sp = MainPluginClass.App.SqlWork();
            var sql_grid = MainPluginClass.App.SqlWork();
            bool is_last_wb_closed = true;
            int? car_type_id = null;
            try
            {
                int r = ((myItem)CB_gos_no.SelectedItem).GetId;

                sql_ts_details.sql = "SELECT ct.name type_name, cm.name mark_name, cmd.name model_name, c.model_id, ct.gid car_type_id FROM autobase.cars c " +
                                    "LEFT JOIN autobase.cars_types ct ON c.type_id = ct.gid " +
                                    "LEFT JOIN autobase.cars_marks cm ON c.mark_id = cm.gid " +
                                    "LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid " +
                                    "WHERE c.gid=" + r.ToString();
                sql_ts_details.ExecuteReader();
                while (sql_ts_details.CanRead())
                {
                    TB_tstype.Text = sql_ts_details.GetString(0);
                    TB_tstype.Enabled = true;
                    TB_tsmodel.Text = sql_ts_details.GetString(1) + "  " + sql_ts_details.GetString(2);
                    TB_tsmodel.Enabled = true;
                    Set_dict_values(r, CB_gar_no);
                    TB_modelid.Text = sql_ts_details.GetString(3);
                    car_type_id = sql_ts_details.GetValue<int?>("car_type_id");
                }
                sql_ts_details.Close();
                CB_gos_no.BackColor = SystemColors.Window;

                // От типа выбранного авто, ищем какой отчет по умолчанию назначить
                if (car_type_id != null)
                {
                    int? report_id = null;
                    using (ISQLCommand cmd = MainPluginClass.App.SqlWork())
                    {
                        try
                        {
                            cmd.sql = "select t1.report_id, t2.caption from autobase.waybills_reports_default t1, sys_scheme.report_templates t2 where t1.report_id=t2.id and car_type_id = " + car_type_id;
                            cmd.ExecuteReader();
                            if (cmd.CanRead())
                            {
                                report_id = cmd.GetValue<int?>("report_id");
                            }
                        }
                        catch { }
                        cmd.Close();
                    }
                    if (report_id != null)
                    {
                        splitButton1.Tag = report_id;
                    }
                    else
                    {
                        splitButton1.Tag = null;
                    }
                }
                ////////////////           СПИДОМЕТР, МОТОЧАСЫ, ОБОРУДОВАНИЕ              ////////////////
                if (_id == null || (isEdited && CB_gos_no.SelectedItem!=null && ((myItem)CB_gos_no.SelectedItem).GetId != base_car_id))
                {
                    sql_sp.sql = "SELECT km_end, mh_end, mh_ob_end, fuel_end, fuel_end2 ,fuel_mark_id, fuel_mark2_id, driver_id, fuel_card_id, autobase.get_last_waybill_is_closed(" + r + ") as is_closed, calc_fuel_drain, trailer_id FROM  autobase.waybills WHERE gid = (SELECT autobase.get_last_waybill(" + r + "));";
                    sql_sp.ExecuteReader();
                    if (sql_sp.CanRead())
                    {
                        if (sql_sp.GetValue("is_closed") != null)
                        {
                            is_last_wb_closed = (int)sql_sp.GetValue("is_closed") == 1 || _id != null ? true : false;
                        }
                        if (sql_sp.GetValue(0) != null)
                        { TB_sp_n.Text = (Convert.ToDecimal(sql_sp.GetString(0).Replace(".", _decSeparator))).ToString(); norma_sp = sql_sp.GetString(0) == null ? "" : sql_sp.GetString(0); }
                        else { TB_sp_n.Text = ""; norma_sp = ""; }
                        if (sql_sp.GetValue(1) != null)
                        { TB_mch.Text = (Convert.ToDecimal(sql_sp.GetString(1).Replace(".", _decSeparator))).ToString(); norma_mch = sql_sp.GetString(1) == null ? "" : sql_sp.GetString(1); }
                        else { TB_mch.Text = ""; norma_mch = ""; }
                        if (sql_sp.GetValue(2) != null)
                        { TB_mch_obr.Text = (Convert.ToDecimal(sql_sp.GetString(2).Replace(".", _decSeparator))).ToString(); norma_mchobr = sql_sp.GetString(2) == null ? "" : sql_sp.GetString(2); }
                        else { TB_mch_obr.Text = ""; norma_mchobr = ""; }
                        if (sql_sp.GetValue(3) != null)
                        {
                            decimal s1 = Convert.ToDecimal(sql_sp.GetString(3).Replace(".", _decSeparator));
                            decimal s2 = Convert.ToDecimal(sql_sp.GetValue("calc_fuel_drain") != null ? sql_sp.GetString("calc_fuel_drain").Replace(".", _decSeparator) : "0");
                            decimal s3 = s1 - s2;
                            TB_t1_n.Text = s3.ToString();
                            norma_t1 = TB_t1_n.Text;
                        }
                        else { TB_t1_n.Text = ""; norma_t1 = ""; }
                        if (sql_sp.GetValue(4) != null)
                        { TB_t2_n.Text = (Convert.ToDecimal(sql_sp.GetString(4).Replace(".", _decSeparator))).ToString(); norma_t2 = sql_sp.GetString(4) == null ? "" : sql_sp.GetString(4); }
                        else { TB_t2_n.Text = ""; norma_t2 = ""; }
                        if (sql_sp.GetValue(5) != null)
                        { Set_dict_values(sql_sp.GetInt32(5), CB_top1); }
                        else
                        {
                            //CB_top1.SelectedIndexChanged -= CB_top1_SelectedIndexChanged;
                            CB_top1.SelectedIndex = -1;
                            CB_top1.SelectedItem = null;
                            CB_fuel_card.Items.Clear();
                            CB_fuel_card.SelectedItem = null;
                            CB_fuel_card.Items.Add(new myItem(" ", -99));
                            //CB_top1.SelectedIndexChanged += CB_top1_SelectedIndexChanged;
                        }
                        if (sql_sp.GetValue(6) != null)
                        { Set_dict_values(sql_sp.GetInt32(6), CB_top2); }
                        else
                        {
                            //CB_top2.SelectedIndexChanged -= CB_top2_SelectedIndexChanged;
                            CB_top2.SelectedItem = null;
                            CB_top2.SelectedIndex = -1;
                            CB_fuel_card2.Items.Clear();
                            CB_fuel_card2.SelectedItem = null;
                            CB_fuel_card2.Items.Add(new myItem(" ", -99));
                            //CB_top2.SelectedIndexChanged += CB_top2_SelectedIndexChanged;
                        }
                        if (sql_sp.GetValue(7) != null && CB_pl_driverel.SelectedItem == null)
                        { Set_dict_values(sql_sp.GetInt32(7), CB_pl_driverel); }
                        else
                        {
                            CB_pl_driverel.SelectedIndexChanged -= CB_pl_driverel_SelectedIndexChanged;
                            CB_pl_driverel.SelectedItem = null;
                            CB_pl_driverel.SelectedIndex = -1;
                            CB_pl_driverel.SelectedIndexChanged += CB_pl_driverel_SelectedIndexChanged;
                            CB_pl_drivertn.SelectedIndexChanged -= CB_pl_drivertn_SelectedIndexChanged;
                            CB_pl_drivertn.SelectedItem = null;
                            CB_pl_drivertn.SelectedIndex = -1;
                            CB_pl_drivertn.SelectedIndexChanged += CB_pl_drivertn_SelectedIndexChanged;

                        }
                        if (sql_sp.GetValue("trailer_id") != null && CB_p_gos_no.SelectedItem == null)
                        { Set_dict_values(sql_sp.GetInt32("trailer_id"), CB_p_gos_no); }
                        else
                        {
                            CB_p_gos_no.SelectedIndexChanged -= CB_p_gos_no_SelectedIndexChanged;
                            CB_p_gos_no.SelectedItem = null;
                            CB_p_gos_no.SelectedIndex = -1;
                            CB_p_gos_no.SelectedIndexChanged += CB_p_gos_no_SelectedIndexChanged;
                            CB_p_gar_no.SelectedIndexChanged -= CB_p_gar_no_SelectedIndexChanged;
                            CB_p_gar_no.SelectedItem = null;
                            CB_p_gar_no.SelectedIndex = -1;
                            CB_p_gar_no.SelectedIndexChanged += CB_p_gar_no_SelectedIndexChanged;

                        }
                        /* 2014-10-01 forge http://forge.gradoservice.ru/issues/15975
                        if (sql_sp.GetValue(8) != null)
                        {
                            Set_dict_values(sql_sp.GetInt32(8), CB_fuel_card);
                            CB_fuel_card.Tag = sql_sp.GetInt32(8);
                        }
                        */
                    }
                    else
                    {
                        norma_sp = "";
                        norma_mch = "";
                        norma_mchobr = "";
                        norma_t1 = "";
                        norma_t2 = "";
                        TB_sp_n.Text = "";
                        TB_mch.Text = "";
                        TB_mch_obr.Text = "";
                        TB_t1_n.Text = "";
                        TB_t2_n.Text = "";
                        CB_top1.SelectedIndexChanged -= CB_top1_SelectedIndexChanged;
                        CB_top1.SelectedIndex = -1;
                        CB_top1.SelectedItem = null;
                        CB_top1.SelectedIndexChanged += CB_top1_SelectedIndexChanged;
                        CB_top2.SelectedIndexChanged -= CB_top2_SelectedIndexChanged;
                        CB_top2.SelectedItem = null;
                        CB_top2.SelectedIndex = -1;
                        CB_top2.SelectedIndexChanged += CB_top2_SelectedIndexChanged;

                        CB_pl_driverel.SelectedIndexChanged -= CB_pl_driverel_SelectedIndexChanged;
                        CB_pl_driverel.SelectedItem = null;
                        CB_pl_driverel.SelectedIndex = -1;
                        CB_pl_driverel.SelectedIndexChanged += CB_pl_driverel_SelectedIndexChanged;
                        CB_pl_drivertn.SelectedIndexChanged -= CB_pl_drivertn_SelectedIndexChanged;
                        CB_pl_drivertn.SelectedItem = null;
                        CB_pl_drivertn.SelectedIndex = -1;
                        CB_pl_drivertn.SelectedIndexChanged += CB_pl_drivertn_SelectedIndexChanged;

                        CB_p_gos_no.SelectedIndexChanged -= CB_p_gos_no_SelectedIndexChanged;
                        CB_p_gos_no.SelectedItem = null;
                        CB_p_gos_no.SelectedIndex = -1;
                        CB_p_gos_no.SelectedIndexChanged += CB_p_gos_no_SelectedIndexChanged;
                        CB_p_gar_no.SelectedIndexChanged -= CB_p_gar_no_SelectedIndexChanged;
                        CB_p_gar_no.SelectedItem = null;
                        CB_p_gar_no.SelectedIndex = -1;
                        CB_p_gar_no.SelectedIndexChanged += CB_p_gar_no_SelectedIndexChanged;

                        CB_fuel_card.SelectedItem = null;
                        CB_fuel_card.SelectedIndex = -1;
                        CB_fuel_card2.SelectedItem = null;
                        CB_fuel_card2.SelectedIndex = -1;
                    }
                }

                if (_id != null)
                {
                    //////////////////           ЗАПОЛНЕНИЕ ОПЕРАЦИИ ПТО в GRID            ////////////////
                    DataGridView1.Rows.Clear();

                    if (_id != null && _id > 0)
                    {
                        sql_grid.sql = "SELECT gid, operation_id, value_input, value_norm, value_total from autobase.waybills_taks WHERE waybill_id = " + _id.ToString();
                        sql_grid.ExecuteReader();
                        decimal sum = 0;
                        myItem item_id = new myItem("", 0);
                        while (sql_grid.CanRead())
                        {
                            foreach (myItem z in operacija.Items)
                            {
                                if (z.GetId == sql_grid.GetInt32(1)) { item_id = z; break; }
                            }
                            int t = sql_grid.GetInt32(0);
                            int t2 = sql_grid.GetInt32(1);
                            //DataGridView1.Rows.Add(t, t2, sql_grid.GetValue(2), sql_grid.GetValue(3), sql_grid.GetValue(4));
                            if (!taks_to_del.Contains(t))
                            {
                                int t3 = DataGridView1.Rows.Add(t, t2, sql_grid.GetValue(2), sql_grid.GetValue(3), sql_grid.GetValue(4));
                                sum += Convert.ToDecimal(sql_grid.GetValue(4).ToString().Replace(".", _decSeparator));
                                TB_tax_n.Text = sum.ToString();
                            }
                            //DataGridView1.Rows[t3].Cells[2].Value = sql_grid.GetValue(2);
                            //DataGridView1.Rows[t3].Cells[3].Value = sql_grid.GetValue(3);
                            //DataGridView1.Rows[t3].Cells[4].Value = sql_grid.GetValue(4);

                        }
                        sql_grid.Close();
                    }
                    if (TB_t1_f.Text != "" && TB_t1_n.Text != "" && TB_t1_k.Text != "")
                    {
                        TB_tax_f.Text = (Convert.ToDecimal(TB_t1_f.Text.Replace(".", _decSeparator)) + Convert.ToDecimal(TB_t1_n.Text.Replace(".", _decSeparator)) - Convert.ToDecimal(TB_t1_k.Text.Replace(".", _decSeparator))).ToString();
                    }
                    if (TB_tax_f.Text.Trim().Length != 0 && TB_tax_n.Text.Trim().Length != 0)
                    {
                        TB_tax_r.Text = (Convert.ToDecimal(TB_tax_n.Text.Replace(".", _decSeparator)) - Convert.ToDecimal(TB_tax_f.Text.Replace(".", _decSeparator))).ToString();
                    }
                }
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора автомобиля!"); }
            sql_ts_details.Close();
            sql_sp.Close();
            sql_grid.Close();
            if (!is_last_wb_closed && (_id == null || isEdited))
            {
                MessageBox.Show("Предыдущий путевой лист на ТС не закрыт!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        /// <summary>
        /// Проверка ввода обязательных значений
        /// </summary>
        private bool allow_required_fields
        {
            get
            {
                string caption = "Ошибка сохранения!";
                string err_head = "Укажите следующие поля:";
                string err_body = "";
                bool rez = false;
                if (secsave < 1)
                {
                    err_body += CB_pl_driverel.SelectedIndex < 0 ? "   Водитель [ФИО]\r\n" : "";
                    err_body += CB_gos_no.SelectedIndex < 0 ? "   Транспортное средство [Гос. номер]\r\n" : "";
                    err_body += CB_rrab.SelectedIndex < 0 ? "   Режим работы [ТС]\r\n" : "";
                    err_body += CB_top1.SelectedIndex < 0 ? "   Топливо1 [вид]\r\n" : "";
                    err_body += CB_top2.SelectedIndex < 0 ? "   Топливо2 [вид]\r\n" : "";
                }
                else if (secsave == 1 && isEdited == false)
                {
                    err_body += CB_route.SelectedIndex < 0 ? "   Маршрут [Задание]\r\n" : "";
                    //err_body += CB_wrktype.SelectedIndex < 0 ? "   Вид работ [Задание]\r\n" : "";
                    //err_body += CB_gruztype.SelectedIndex < 0 ? "   Груз [Задание]\r\n" : "";
                    //err_body += CB_pl_zone.SelectedIndex < 0 ? "   Зона [Задание]\r\n" : "";
                    //err_body += CB_pl_master.SelectedIndex < 0 ? "   Мастер [ФИО]\r\n" : "";
                    //err_body += checkParamIsNull(TB_Ezdok.Text) ? "   Количество ездок [задание]\r\n" : "";
                    if (checkParamIsNull(TB_sp_n.Text) && checkParamIsNull(TB_mch.Text))
                    {
                        err_body += "   Одно из значений: Спидометр [начало, км], Моточасы [начало, м/ч]\r\n";
                    }
                    err_body += (checkParamIsNull(TB_t1_pl.Text)) ? "   Топливо1 [выдать, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_n.Text)) ? "   Топливо1 [начало, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_f.Text)) ? "   Топливо1 [выдано, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_k.Text)) ? "   Топливо1 [конец, л]\r\n" : "";
                    if (CB_top2.SelectedIndex > 0 && !CB_top2.SelectedItem.ToString().ToUpper().Trim().Equals("НЕТ"))
                    {
                        err_body += (checkParamIsNull(TB_t2_pl.Text)) ? "   Топливо2 [выдать, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_n.Text)) ? "   Топливо2 [начало, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_f.Text)) ? "   Топливо2 [выдано, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_k.Text)) ? "   Топливо2 [конец, л]\r\n" : "";
                    }
                    if (checkParamOverZero(TB_sp_n.Text) && !checkParamOverZero(TB_spk.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Спидометр [конец, км]\r\n";
                    }
                    if (checkParamOverZero(TB_mch.Text) && !checkParamOverZero(TB_mch_k.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Моточасы [конец, м/ч]\r\n";
                    }
                    if (checkParamOverZero(TB_mch_obr.Text) && !checkParamOverZero(TB_mch_obr_k.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Моточасы оборудования [конец, м/ч]\r\n";
                    }
                    if (DataGridView1.RowCount < 1)
                    {
                        err_body += "   Таксировка\r\n";
                    }
                    // Логические проверки:
                    if (parseTextToDecimal(TB_tax_drain.Text) > parseTextToDecimal(TB_t1_k.Text))
                    {
                        err_body += "!!   Слив [л] превышает Топливо 1 [конец, л]\r\n";
                    }
                    if (parseTextToDecimal(TB_spk.Text) < parseTextToDecimal(TB_sp_n.Text))
                    {
                        err_body += "!!   Спидометр [конец, км] меньше Спидометр [начало, км]\r\n";
                    }
                    if (parseTextToDecimal(TB_mch_k.Text) < parseTextToDecimal(TB_mch.Text))
                    {
                        err_body += "!!   Счетчик моточасов [конец, м/ч] меньше Счетчик моточасов [начало, м/ч]\r\n";
                    }
                    if (parseTextToDecimal(TB_mch_obr_k.Text) < parseTextToDecimal(TB_mch_obr.Text))
                    {
                        err_body += "!!   Счетчик моточасов оборудования [конец, м/ч] меньше Счетчик моточасов оборудования [начало, м/ч]\r\n";
                    }
                    if (parseTextToDecimal(TB_t1_k.Text) > parseTextToDecimal(TB_t1_n.Text) + parseTextToDecimal(TB_t1_f.Text))
                    {
                        err_body += "!!   Топливо 1 [конец, л] больше чем [выдано + начало]\r\n";
                    }
                }
                // Полный дубль проверок, только на шаг назад. На случай, когда редактируем путевой лист.
                if (isEdited && secsave == 1)
                {
                    err_body += CB_pl_driverel.SelectedIndex < 0 ? "   Водитель [ФИО]\r\n" : "";
                    err_body += CB_gos_no.SelectedIndex < 0 ? "   Транспортное средство [Гос. номер]\r\n" : "";
                    err_body += CB_rrab.SelectedIndex < 0 ? "   Режим работы [ТС]\r\n" : "";
                    err_body += CB_top1.SelectedIndex < 0 ? "   Топливо1 [вид]\r\n" : "";
                    err_body += CB_top2.SelectedIndex < 0 ? "   Топливо2 [вид]\r\n" : "";
                }
                else if (isEdited && secsave == 2)
                {
                    // проверки 1го этапа
                    err_body += CB_pl_driverel.SelectedIndex < 0 ? "   Водитель [ФИО]\r\n" : "";
                    err_body += CB_gos_no.SelectedIndex < 0 ? "   Транспортное средство [Гос. номер]\r\n" : "";
                    err_body += CB_rrab.SelectedIndex < 0 ? "   Режим работы [ТС]\r\n" : "";
                    err_body += CB_top1.SelectedIndex < 0 ? "   Топливо1 [вид]\r\n" : "";
                    err_body += CB_top2.SelectedIndex < 0 ? "   Топливо2 [вид]\r\n" : "";
                    // проверки 2го этапа
                    err_body += CB_route.SelectedIndex < 0 ? "   Маршрут [Задание]\r\n" : "";
                    //err_body += CB_wrktype.SelectedIndex < 0 ? "   Вид работ [Задание]\r\n" : "";
                    //err_body += CB_gruztype.SelectedIndex < 0 ? "   Груз [Задание]\r\n" : "";
                    //err_body += CB_pl_zone.SelectedIndex < 0 ? "   Зона [Задание]\r\n" : "";
                    //err_body += CB_pl_master.SelectedIndex < 0 ? "   Мастер [ФИО]\r\n" : "";
                    //err_body += checkParamIsNull(TB_Ezdok.Text) ? "   Количество ездок [задание]\r\n" : "";
                    if (checkParamIsNull(TB_sp_n.Text) && checkParamIsNull(TB_mch.Text))
                    {
                        err_body += "   Одно из значений: Спидометр [начало, км], Моточасы [начало, м/ч]\r\n";
                    }
                    err_body += (checkParamIsNull(TB_t1_pl.Text)) ? "   Топливо1 [выдать, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_n.Text)) ? "   Топливо1 [начало, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_f.Text)) ? "   Топливо1 [выдано, л]\r\n" : "";
                    err_body += (checkParamIsNull(TB_t1_k.Text)) ? "   Топливо1 [конец, л]\r\n" : "";
                    if (CB_top2.SelectedIndex > 0 && !CB_top2.SelectedItem.ToString().ToUpper().Trim().Equals("НЕТ"))
                    {
                        err_body += (checkParamIsNull(TB_t2_pl.Text)) ? "   Топливо2 [выдать, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_n.Text)) ? "   Топливо2 [начало, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_f.Text)) ? "   Топливо2 [выдано, л]\r\n" : "";
                        err_body += (checkParamIsNull(TB_t2_k.Text)) ? "   Топливо2 [конец, л]\r\n" : "";
                    }
                    if (checkParamOverZero(TB_sp_n.Text) && !checkParamOverZero(TB_spk.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Спидометр [конец, км]\r\n";
                    }
                    if (checkParamOverZero(TB_mch.Text) && !checkParamOverZero(TB_mch_k.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Моточасы [конец, м/ч]\r\n";
                    }
                    if (checkParamOverZero(TB_mch_obr.Text) && !checkParamOverZero(TB_mch_obr_k.Text))//Если было начало - должен быть и конец
                    {
                        err_body += "   Моточасы оборудования [конец, м/ч]\r\n";
                    }
                    if (DataGridView1.RowCount < 1)
                    {
                        err_body += "   Таксировка\r\n";
                    }
                    // Логические проверки:
                    if (parseTextToDecimal(TB_tax_drain.Text) > parseTextToDecimal(TB_t1_k.Text))
                    {
                        err_body += "!!   Слив [л] превышает Топливо 1 [конец, л]\r\n";
                    }
                    if (parseTextToDecimal(TB_spk.Text) < parseTextToDecimal(TB_sp_n.Text))
                    {
                        err_body += "!!   Спидометр [конец, км] меньше Спидометр [начало, км]\r\n";
                    }
                    if (parseTextToDecimal(TB_mch_k.Text) < parseTextToDecimal(TB_mch.Text))
                    {
                        err_body += "!!   Счетчик моточасов [конец, м/ч] меньше Счетчик моточасов [начало, м/ч]\r\n";
                    }
                    if (parseTextToDecimal(TB_mch_obr_k.Text) < parseTextToDecimal(TB_mch_obr.Text))
                    {
                        err_body += "!!   Счетчик моточасов оборудования [конец, м/ч] меньше Счетчик моточасов оборудования [начало, м/ч]\r\n";
                    }
                    if (parseTextToDecimal(TB_t1_k.Text) > parseTextToDecimal(TB_t1_n.Text) + parseTextToDecimal(TB_t1_f.Text))
                    {
                        err_body += "!!   Топливо 1 [конец, л] больше чем [выдано + начало]\r\n";
                    }
                }
                if (err_body.Length > 0)
                {
                    MessageBox.Show(err_head + "\r\n" + err_body, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    rez = true;
                }
                return rez;
            }

        }

        /// <summary>
        /// Если текст больше нуля, то возращает True. Если текст не удалось преобразовать к числу или меньше нуля - возвращает False
        /// </summary>
        /// <param name="param">Текстовое значение числа, иначе False</param>
        /// <returns></returns>
        private bool checkParamOverZero(string param)
        {
            decimal zero = 0;
            if (decimal.TryParse(param, out zero) == false) return false; // Указано не число
            if (zero <= 0) return false;
            return true;
        }

        /// <summary>
        /// Проверяет, указано ли значение. Даже 0 сойдет
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private bool checkParamIsNull(string param)
        {
            decimal zero = 0;
            if (decimal.TryParse(param.Trim(), out zero) == false) return true; // Указано не число
            return false;
        }
        /// <summary>
        /// Просто, получаем число. Если не удалось - возвращает НОЛЬ
        /// </summary>
        /// <param name="param">Текст, который необходимо преобразовать</param>
        /// <returns></returns>
        private decimal parseTextToDecimal(string param)
        {
            decimal zero = 0;
            if (decimal.TryParse(param, out zero) == false) return 0; // Указано не число
            return zero;
        }
        /// <summary>
        /// Проверяет, чтобы некоторые значения были не больше других.
        /// Логическая проверка указанных данных.
        /// </summary>
        private bool Check_mod_grounds
        {
            get
            {
                err_body1 = "";
                bool rez = false;
                err_body1 += parseTextToDecimal(norma_sp) != parseTextToDecimal(TB_sp_n.Text) ? "   Спидометр [начало, км]\r\n" : "";
                err_body1 += parseTextToDecimal(norma_mch) != parseTextToDecimal(TB_mch.Text) ? "   Моточасы [начало, м/ч]\r\n" : "";
                err_body1 += parseTextToDecimal(norma_mchobr) != parseTextToDecimal(TB_mch_obr.Text) ? "   Моточасы оборудования [начало, м/ч]\r\n" : "";
                err_body1 += parseTextToDecimal(norma_t1) != parseTextToDecimal(TB_t1_n.Text) ? "   Топливо 1 [начало, л]\r\n" : "";
                err_body1 += parseTextToDecimal(norma_t2) != parseTextToDecimal(TB_t2_n.Text) ? "   Топливо 2 [начало, л]\r\n" : "";
                if (err_body1.Length > 0)
                {
                    Form_mod_grounds f = new Form_mod_grounds(err_body1);
                    f.ShowDialog();
                    if (f.DialogResult == DialogResult.OK)
                    {
                        if (f.M_grounds != "")
                        {
                            modcomment = f.M_grounds;
                            rez = true;
                            f.Dispose();
                        }
                        else
                        {
                            MessageBox.Show("Вы не указали основания изменений, путевой лист не сохранен!", "Ошибка при сохранении!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        f.Dispose();
                    }
                }
                else
                {
                    rez = true;
                }
                return rez;
            }
        }



        /// <summary>
        /// Проверка на повторяющиеся операции, а также на имеющиеся данные (ИД операции, значение)
        /// </summary>
        /// <returns></returns>
        private bool CheckValidTaksirovkaList()
        {
            bool rez = true;
            for (int i = 0; i < DataGridView1.RowCount; i++)
            {
                int id_operc = Convert.ToInt32(DataGridView1.Rows[i].Cells["operacija"].Value);
                for (int j = 0; j < DataGridView1.RowCount; j++)
                {
                    if (i != j)
                    {
                        if (Convert.ToInt32(DataGridView1.Rows[j].Cells["operacija"].Value) == id_operc)
                        {
                            rez = false;
                        }
                    }
                }
                if (DataGridView1.Rows[i].Cells["operacija"].Value == null || DataGridView1.Rows[i].Cells["znachenie"].Value == null)
                {
                    rez = false;
                }
            }
            return rez;
        }

        private bool checkDateTime(ref string Message)
        {
            bool rez = true;
            int z = 0;
            if (CB_rrab.SelectedItem != null)
            {
                // Режим работы, выраженный в часах
                z = Convert.ToInt32(((myItem)CB_rrab.SelectedItem).Data);
            }

            if (DateTimePicker1.Value.Date.AddHours(dateTimePicker5.Value.Hour + z).AddMinutes(dateTimePicker5.Value.Minute)
                > DateTimePicker3.Value.Date.AddHours(dateTimePicker8.Value.Hour).AddMinutes(dateTimePicker8.Value.Minute)
            )
            {
                Message += "\r\nРазница времени выезда и возвращения не может быть меньше режима работы! (Планируемое)\r\n";
                rez = false;
            }

            if (DateTimePicker2.Value.Date.AddHours(dateTimePicker6.Value.Hour).AddMinutes(dateTimePicker6.Value.Minute)
                >= DateTimePicker4.Value.Date.AddHours(dateTimePicker7.Value.Hour).AddMinutes(dateTimePicker7.Value.Minute)
                )
            {
                Message += "\r\nВремя выезда не может быть позже времени возвращения! (Фактическое)\r\n";
                rez = false;
            }

            return rez;
        }

        private void btn_save_click(object sender, EventArgs e)
        {
            string msgDate = "";
            if (!checkDateTime(ref msgDate))
            {
                MessageBox.Show(msgDate, "Ошибка при сохранении!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!CheckValidTaksirovkaList())
            {
                MessageBox.Show("Имеются повторяющиеся операции в таксировке, либо не указано значение!", "Ошибка при сохранении!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (allow_required_fields)
            {
                if (!(_id == null && !Check_mod_grounds))
                {
                    List<string> command_list = new List<string>();

                    var upload_pl = MainPluginClass.App.SqlWork();
                    upload_pl.BeginTransaction();
                    try
                    {
                        #region UPDATE
                        if (_id != null)
                        {
                            string temp1 = "";// что обновляем
                            string temp2 = "";// значение
                            temp1 += "calc_norm_type,";
                            temp2 += CHB_calc_norm_type.Checked ? "1, " : "0, ";
                            temp1 += "date_out_plan, date_out_fact, date_in_plan, date_in_fact, ";
                            temp2 += "'" + DateTimePicker1.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker5.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker2.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker6.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker3.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker8.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker4.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker7.Value.ToString("HH:mm:00") + "', ";
                            if (!TB_sp_n.Text.Equals(""))
                            {
                                temp1 += "km_begin, ";
                                temp2 += TB_sp_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_spk.Text.Equals(""))
                            {
                                temp1 += "km_end, ";
                                temp2 += TB_spk.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_sp_prob.Text.Equals(""))
                            {
                                temp1 += "km_run, ";
                                temp2 += TB_sp_prob.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_top1.SelectedItem != null)
                            {
                                temp1 += "fuel_mark_id, ";
                                temp2 += ((myItem)CB_top1.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_t1_pl.Text.Equals(""))
                            {
                                temp1 += "fuel_plan, ";
                                temp2 += TB_t1_pl.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_n.Text.Equals(""))
                            {
                                temp1 += "fuel_begin, ";
                                temp2 += TB_t1_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_k.Text.Equals(""))
                            {
                                temp1 += "fuel_end, ";
                                temp2 += TB_t1_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_f.Text.Equals(""))
                            {
                                temp1 += "fuel_fact, ";
                                temp2 += TB_t1_f.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_top2.SelectedItem != null)
                            {
                                temp1 += "fuel_mark2_id, ";
                                temp2 += ((myItem)CB_top2.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_t2_pl.Text.Equals(""))
                            {
                                temp1 += "fuel_plan2, ";
                                temp2 += TB_t2_pl.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_n.Text.Equals(""))
                            {
                                temp1 += "fuel_begin2, ";
                                temp2 += TB_t2_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_k.Text.Equals(""))
                            {
                                temp1 += "fuel_end2, ";
                                temp2 += TB_t2_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_f.Text.Equals(""))
                            {
                                temp1 += "fuel_fact2, ";
                                temp2 += TB_t2_f.Text.Replace(",", ".") + ", ";
                            }

                            if (!TB_ttd.Text.Equals(""))
                            {
                                temp1 += "ttd_count, ";
                                temp2 += TB_ttd.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_Ezdok.Text.Equals(""))
                            {
                                temp1 += "trip_count, ";
                                temp2 += TB_Ezdok.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_rab.Text.Equals(""))
                            {
                                temp1 += "pay_work_h, ";
                                temp2 += TB_rab.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_obed.Text.Equals(""))
                            {
                                temp1 += "pay_lunch_h, ";
                                temp2 += TB_obed.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_dejrst.Text.Equals(""))
                            {
                                temp1 += "pay_duty_h, ";
                                temp2 += TB_dejrst.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_remont.Text.Equals(""))
                            {
                                temp1 += "pay_repair_h, ";
                                temp2 += TB_remont.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_den.Text.Equals(""))
                            {
                                temp1 += "pay_day_h, ";
                                temp2 += TB_den.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_noch.Text.Equals(""))
                            {
                                temp1 += "pay_night_h, ";
                                temp2 += TB_noch.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_stavka.Text.Equals(""))
                            {
                                temp1 += "pay_rate_rh, ";
                                temp2 += TB_stavka.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_itogo.Text.Equals(""))
                            {
                                temp1 += "pay_total_r, ";
                                temp2 += TB_itogo.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch.Text.Equals(""))
                            {
                                temp1 += "mh_begin, ";
                                temp2 += TB_mch.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_k.Text.Equals(""))
                            {
                                temp1 += "mh_end, ";
                                temp2 += TB_mch_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_pr.Text.Equals(""))
                            {
                                temp1 += "mh_run, ";
                                temp2 += TB_mch_pr.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr.Text.Equals(""))
                            {
                                temp1 += "mh_ob_begin, ";
                                temp2 += TB_mch_obr.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr_k.Text.Equals(""))
                            {
                                temp1 += "mh_ob_end, ";
                                temp2 += TB_mch_obr_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr_pr.Text.Equals(""))
                            {
                                temp1 += "mh_ob_run, ";
                                temp2 += TB_mch_obr_pr.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_gos_no.SelectedItem != null)
                            {
                                temp1 += "car_id, ";
                                temp2 += ((myItem)CB_gos_no.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_driverel.SelectedItem != null)
                            {
                                temp1 += "driver_id, ";
                                temp2 += ((myItem)CB_pl_driverel.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_route.SelectedItem != null)
                            {
                                temp1 += "route_id, ";
                                temp2 += ((myItem)CB_route.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_col.Text.Equals(""))
                            {
                                temp1 += "motorcade, ";
                                temp2 += TB_col.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_brig.Text.Equals(""))
                            {
                                temp1 += "brigade, ";
                                temp2 += TB_brig.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_wrktype.SelectedItem != null)
                            {
                                temp1 += "work_type_id, ";
                                temp2 += ((myItem)CB_wrktype.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_gruztype.SelectedItem != null)
                            {
                                temp1 += "cargo_type_id, ";
                                temp2 += ((myItem)CB_gruztype.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_notes.SelectedItem != null)
                            {
                                temp1 += "special_note_id, ";
                                temp2 += ((myItem)CB_notes.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_master.SelectedItem != null)
                            {
                                temp1 += "automaster_id, ";
                                temp2 += ((myItem)CB_pl_master.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_zone.SelectedItem != null)
                            {
                                temp1 += "road_type_id, ";
                                temp2 += ((myItem)CB_pl_zone.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!RTB_comm.Text.Equals(""))
                            {
                                temp1 += "notes, ";
                                temp2 += "'" + RTB_comm.Text.Replace("'", "") + "', ";
                            }
                            if (CB_fuel_card.SelectedItem != null)
                            {
                                temp1 += "fuel_card_id, ";
                                //temp2 += ((myItem)CB_fuel_card.SelectedItem).GetId.ToString() + ", ";
                                if (((myItem)CB_fuel_card.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_fuel_card.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_fuel_card2.SelectedItem != null)
                            {
                                temp1 += "fuel_card2_id, ";
                                //temp2 += ((myItem)CB_fuel_card2.SelectedItem).GetId.ToString() + ", ";
                                if (((myItem)CB_fuel_card2.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_fuel_card2.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_rrab.SelectedItem != null)
                            {
                                temp1 += "work_regime_id, ";
                                temp2 += ((myItem)CB_rrab.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (secsave == 1 && !isEdited)
                            {
                                temp1 += "secondsave, ";
                                temp2 += "2, ";
                            }
                            else
                            {
                                temp1 += "secondsave, ";
                                temp2 += ((myItem)CB_status.SelectedItem).GetId.ToString() + ", ";
                            }

                            if (!TB_tax_n.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_norm, ";
                                temp2 += TB_tax_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_f.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_fact, ";
                                temp2 += TB_tax_f.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_r.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_delta, ";
                                temp2 += TB_tax_r.Text.Replace(",", ".") + ", ";
                            }

                            if (!TB_tax_drain.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_drain, ";
                                temp2 += TB_tax_drain.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_km_delta.Text.Equals(""))
                            {
                                temp1 += "calc_km_run_delta, ";
                                temp2 += TB_tax_km_delta.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_mh_delta.Text.Equals(""))
                            {
                                temp1 += "calc_mh_run_delta, ";
                                temp2 += TB_tax_mh_delta.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_p_gos_no.SelectedItem != null)
                            {
                                temp1 += "trailer_id, ";
                                if (((myItem)CB_p_gos_no.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_p_gos_no.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_escort_driverel.SelectedItem != null)
                            {
                                temp1 += "escort_driver_id, ";
                                if (((myItem)CB_escort_driverel.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_escort_driverel.SelectedItem).GetId.ToString() + ", ";
                                }
                            }

                            temp1 = temp1.Trim(new char[] { ' ', ',' });
                            temp2 = temp2.Trim(new char[] { ' ', ',' });
                            command_list.Add("UPDATE autobase.waybills SET (" + temp1 + ") = (" + temp2 + ") where gid = " + _id.ToString());

                            // Обновление таксировки
                            if (_id != null && (secsave == 1 || CanEditPutList_isAdmin)) // Можно только есть пут. лист уже создан. И у нас второй этап. Т.е возвращение в гараж.
                            {
                                //Для того, что уже есть в таблице таксировки
                                foreach (DataGridViewRow row in DataGridView1.Rows)
                                {
                                    bool is_new_oper = true; // Новое ли значение таксировки
                                    if (!row.IsNewRow)
                                    {
                                        // Если эта операция ранее сохраненная, у нее есть ID
                                        if (row.Cells["oper_gid"].Value != null && int.Parse(row.Cells["oper_gid"].Value.ToString()) > 0)
                                        {
                                            is_new_oper = false;
                                        }
                                        if (!is_new_oper) // Если таксировку нужно обновить
                                        {
                                            if (row.Cells["znachenie"].Value != null && row.Cells["oper_gid"].Value != null)
                                            {
                                                string sql1;
                                                sql1 = "UPDATE autobase.waybills_taks SET " +
                                                    " operation_id = " + row.Cells["operacija"].Value.ToString().Replace(",", ".") +
                                                    " ,value_input = " + row.Cells["znachenie"].Value.ToString().Replace(",", ".") +
                                                    " ,value_norm = " + row.Cells["norma__l__spravochnik_"].Value.ToString().Replace(",", ".") +
                                                    " ,value_total = " + row.Cells["itogo__l__schitaetsja_avtomatom_"].Value.ToString().Replace(",", ".") +
                                                    " WHERE gid = " + row.Cells["oper_gid"].Value.ToString();
                                                command_list.Add(sql1);
                                            }
                                        }
                                        else
                                        {
                                            string sql1;
                                            sql1 = "INSERT INTO autobase.waybills_taks(waybill_id,operation_id,value_input,value_norm,value_total) VALUES ( " +
                                                _id.ToString() + ", " + row.Cells["operacija"].Value.ToString() + ", " +
                                                row.Cells["znachenie"].Value.ToString().Replace(",", ".") + ", '" +
                                                row.Cells["norma__l__spravochnik_"].Value.ToString().Replace(",", ".") + "', '" +
                                                row.Cells["itogo__l__schitaetsja_avtomatom_"].Value.ToString().Replace(",", ".") + "' );";
                                            command_list.Add(sql1);
                                        }
                                    }
                                }
                                // Если мы удалили какую-то таксировочку
                                foreach (int taks_del_item in taks_to_del)
                                {
                                    string sql1;
                                    sql1 = "DELETE FROM autobase.waybills_taks WHERE gid = " + taks_del_item.ToString();
                                    command_list.Add(sql1);
                                }
                            }
                        }
                        #endregion
                        #region INSERT
                        else
                        {
                            string temp1 = "";// что обновляем
                            string temp2 = "";// значение
                            temp1 += "doc_no, ";
                            temp2 += "nextval('" + org_put_list_seq_name + "'), ";
                            temp1 += "org_id, ";
                            temp2 += org_id.ToString() + ", ";
                            temp1 += "calc_norm_type,";
                            temp2 += CHB_calc_norm_type.Checked ? "1, " : "0, ";
                            temp1 += "date_out_plan, date_out_fact, date_in_plan, date_in_fact, ";
                            temp2 += "'" + DateTimePicker1.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker5.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker2.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker6.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker3.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker8.Value.ToString("HH:mm:00") + "', " +
                                "'" + DateTimePicker4.Value.ToString("yyyy-MM-dd") + " " + dateTimePicker7.Value.ToString("HH:mm:00") + "', ";
                            if (!TB_sp_n.Text.Equals(""))
                            {
                                temp1 += "km_begin, ";
                                temp2 += TB_sp_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_spk.Text.Equals(""))
                            {
                                temp1 += "km_end, ";
                                temp2 += TB_spk.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_sp_prob.Text.Equals(""))
                            {
                                temp1 += "km_run, ";
                                temp2 += TB_sp_prob.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_top1.SelectedItem != null)
                            {
                                temp1 += "fuel_mark_id, ";
                                temp2 += ((myItem)CB_top1.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_t1_pl.Text.Equals(""))
                            {
                                temp1 += "fuel_plan, ";
                                temp2 += TB_t1_pl.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_n.Text.Equals(""))
                            {
                                temp1 += "fuel_begin, ";
                                temp2 += TB_t1_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_k.Text.Equals(""))
                            {
                                temp1 += "fuel_end, ";
                                temp2 += TB_t1_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t1_f.Text.Equals(""))
                            {
                                temp1 += "fuel_fact, ";
                                temp2 += TB_t1_f.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_top2.SelectedItem != null)
                            {
                                temp1 += "fuel_mark2_id, ";
                                temp2 += ((myItem)CB_top2.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_t2_pl.Text.Equals(""))
                            {
                                temp1 += "fuel_plan2, ";
                                temp2 += TB_t2_pl.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_n.Text.Equals(""))
                            {
                                temp1 += "fuel_begin2, ";
                                temp2 += TB_t2_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_k.Text.Equals(""))
                            {
                                temp1 += "fuel_end2, ";
                                temp2 += TB_t2_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_t2_f.Text.Equals(""))
                            {
                                temp1 += "fuel_fact2, ";
                                temp2 += TB_t2_f.Text.Replace(",", ".") + ", ";
                            }

                            if (!TB_ttd.Text.Equals(""))
                            {
                                temp1 += "ttd_count, ";
                                temp2 += TB_ttd.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_Ezdok.Text.Equals(""))
                            {
                                temp1 += "trip_count, ";
                                temp2 += TB_Ezdok.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_rab.Text.Equals(""))
                            {
                                temp1 += "pay_work_h, ";
                                temp2 += TB_rab.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_obed.Text.Equals(""))
                            {
                                temp1 += "pay_lunch_h, ";
                                temp2 += TB_obed.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_dejrst.Text.Equals(""))
                            {
                                temp1 += "pay_duty_h, ";
                                temp2 += TB_dejrst.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_remont.Text.Equals(""))
                            {
                                temp1 += "pay_repair_h, ";
                                temp2 += TB_remont.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_den.Text.Equals(""))
                            {
                                temp1 += "pay_day_h, ";
                                temp2 += TB_den.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_noch.Text.Equals(""))
                            {
                                temp1 += "pay_night_h, ";
                                temp2 += TB_noch.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_stavka.Text.Equals(""))
                            {
                                temp1 += "pay_rate_rh, ";
                                temp2 += TB_stavka.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_itogo.Text.Equals(""))
                            {
                                temp1 += "pay_total_r, ";
                                temp2 += TB_itogo.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch.Text.Equals(""))
                            {
                                temp1 += "mh_begin, ";
                                temp2 += TB_mch.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_k.Text.Equals(""))
                            {
                                temp1 += "mh_end, ";
                                temp2 += TB_mch_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_pr.Text.Equals(""))
                            {
                                temp1 += "mh_run, ";
                                temp2 += TB_mch_pr.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr.Text.Equals(""))
                            {
                                temp1 += "mh_ob_begin, ";
                                temp2 += TB_mch_obr.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr_k.Text.Equals(""))
                            {
                                temp1 += "mh_ob_end, ";
                                temp2 += TB_mch_obr_k.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_mch_obr_pr.Text.Equals(""))
                            {
                                temp1 += "mh_ob_run, ";
                                temp2 += TB_mch_obr_pr.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_gos_no.SelectedItem != null)
                            {
                                temp1 += "car_id, ";
                                temp2 += ((myItem)CB_gos_no.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_driverel.SelectedItem != null)
                            {
                                temp1 += "driver_id, ";
                                temp2 += ((myItem)CB_pl_driverel.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_route.SelectedItem != null)
                            {
                                temp1 += "route_id, ";
                                temp2 += ((myItem)CB_route.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!TB_col.Text.Equals(""))
                            {
                                temp1 += "motorcade, ";
                                temp2 += TB_col.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_brig.Text.Equals(""))
                            {
                                temp1 += "brigade, ";
                                temp2 += TB_brig.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_wrktype.SelectedItem != null)
                            {
                                temp1 += "work_type_id, ";
                                temp2 += ((myItem)CB_wrktype.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_gruztype.SelectedItem != null)
                            {
                                temp1 += "cargo_type_id, ";
                                temp2 += ((myItem)CB_gruztype.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_notes.SelectedItem != null)
                            {
                                temp1 += "special_note_id, ";
                                temp2 += ((myItem)CB_notes.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_master.SelectedItem != null)
                            {
                                temp1 += "automaster_id, ";
                                temp2 += ((myItem)CB_pl_master.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (CB_pl_zone.SelectedItem != null)
                            {
                                temp1 += "road_type_id, ";
                                temp2 += ((myItem)CB_pl_zone.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!RTB_comm.Text.Equals(""))
                            {
                                temp1 += "notes, ";
                                temp2 += "'" + RTB_comm.Text.Replace("'", "") + "', ";
                            }
                            if (CB_fuel_card.SelectedItem != null)
                            {
                                temp1 += "fuel_card_id, ";
                                //temp2 += ((myItem)CB_fuel_card.SelectedItem).GetId.ToString() + ", ";
                                if (((myItem)CB_fuel_card.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_fuel_card.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_fuel_card2.SelectedItem != null)
                            {
                                temp1 += "fuel_card2_id, ";
                                //temp2 += ((myItem)CB_fuel_card2.SelectedItem).GetId.ToString() + ", ";
                                if (((myItem)CB_fuel_card2.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_fuel_card2.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_rrab.SelectedItem != null)
                            {
                                temp1 += "work_regime_id, ";
                                temp2 += ((myItem)CB_rrab.SelectedItem).GetId.ToString() + ", ";
                            }
                            if (!isEdited)
                            {
                                temp1 += "secondsave, ";
                                temp2 += "1, ";
                            }
                            if (!TB_tax_n.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_norm, ";
                                temp2 += TB_tax_n.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_f.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_fact, ";
                                temp2 += TB_tax_f.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_r.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_delta, ";
                                temp2 += TB_tax_r.Text.Replace(",", ".") + ", ";
                            }

                            if (!TB_tax_drain.Text.Equals(""))
                            {
                                temp1 += "calc_fuel_drain, ";
                                temp2 += TB_tax_drain.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_km_delta.Text.Equals(""))
                            {
                                temp1 += "calc_km_run_delta, ";
                                temp2 += TB_tax_km_delta.Text.Replace(",", ".") + ", ";
                            }
                            if (!TB_tax_mh_delta.Text.Equals(""))
                            {
                                temp1 += "calc_mh_run_delta, ";
                                temp2 += TB_tax_mh_delta.Text.Replace(",", ".") + ", ";
                            }
                            if (CB_p_gos_no.SelectedItem != null)
                            {
                                temp1 += "trailer_id, ";
                                if (((myItem)CB_p_gos_no.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_p_gos_no.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            if (CB_escort_driverel.SelectedItem != null)
                            {
                                temp1 += "escort_driver_id, ";
                                if (((myItem)CB_escort_driverel.SelectedItem).GetId == -99)
                                {
                                    temp2 += "null, ";
                                }
                                else
                                {
                                    temp2 += ((myItem)CB_escort_driverel.SelectedItem).GetId.ToString() + ", ";
                                }
                            }
                            //if (!tb_escort.text.equals(""))
                            //{
                            //    temp1 += "support_persons, ";
                            //    temp2 += "'" + tb_escort.text.replace("'", "") + "', ";
                            //}
                            temp1 = temp1.Trim(new char[] { ' ', ',' });
                            temp2 = temp2.Trim(new char[] { ' ', ',' });
                            command_list.Add("INSERT INTO autobase.waybills (" + temp1 + ") VALUES (" + temp2 + ");");
                            if (norma_sp != TB_sp_n.Text)
                            {
                                command_list.Add("INSERT INTO autobase.waybills_changes(waybill_id, number_old, number_new, field_name, basis) VALUES (" +
                                    "currval('autobase.waybills_gid_seq'), " + (norma_sp == "" ? "null" : norma_sp.Replace(",", ".")) + ", " + (TB_sp_n.Text == "" ? "null" : TB_sp_n.Text.Replace(",", ".")) + ", 'km_begin', " + "'" + modcomment + "'" + "); ");
                            };
                            if (norma_mch != TB_mch.Text)
                            {
                                command_list.Add("INSERT INTO autobase.waybills_changes(waybill_id, number_old, number_new, field_name, basis) VALUES (" +
                                    "currval('autobase.waybills_gid_seq'), " + (norma_mch == "" ? "null" : norma_mch.Replace(",", ".")) + ", " + (TB_mch.Text == "" ? "null" : TB_mch.Text.Replace(",", ".")) + ", 'mh_begin', " + "'" + modcomment + "'" + "); ");
                            };
                            if (norma_mchobr != TB_mch_obr.Text)
                            {
                                command_list.Add("INSERT INTO autobase.waybills_changes(waybill_id, number_old, number_new, field_name, basis) VALUES (" +
                                    "currval('autobase.waybills_gid_seq'), " + (norma_mchobr == "" ? "null" : norma_mchobr.Replace(",", ".")) + ", " + (TB_mch_obr.Text == "" ? "null" : TB_mch_obr.Text.Replace(",", ".")) + ", 'mh_ob_begin', " + "'" + modcomment + "'" + "); ");
                            };
                            if (norma_t1 != TB_t1_n.Text)
                            {
                                command_list.Add("INSERT INTO autobase.waybills_changes(waybill_id, number_old, number_new, field_name, basis) VALUES (" +
                                    "currval('autobase.waybills_gid_seq'), " + (norma_t1 == "" ? "null" : norma_t1.Replace(",", ".")) + ", " + (TB_t1_n.Text == "" ? "null" : TB_t1_n.Text.Replace(",", ".")) + ", 'fuel_begin', " + "'" + modcomment + "'" + "); ");
                            };
                            if (norma_t2 != TB_t2_n.Text)
                            {
                                command_list.Add("INSERT INTO autobase.waybills_changes(waybill_id, number_old, number_new, field_name, basis) VALUES (" +
                                    "currval('autobase.waybills_gid_seq'), " + (norma_t2 == "" ? "null" : norma_t2.Replace(",", ".")) + ", " + (TB_t2_n.Text == "" ? "null" : TB_t2_n.Text.Replace(",", ".")) + ", 'fuel_begin2', " + "'" + modcomment + "'" + "); ");
                            };
                        }
                        #endregion
                        foreach (string par in command_list)
                        {
                            upload_pl.sql = par;
                            upload_pl.ExecuteNonQuery();
                        }
                        #region AutoMap Sync
                        if (!isEdited && secsave == 1 && MainPluginClass.CanSyncAutoMapWaybills)//&& MainPluginClass.App.user_info.id_user == 353
                        {
                            try
                            {
                                this.SendDataToMT(((myItem)CB_gos_no.SelectedItem).GetId,
                                    new DateTime(DateTimePicker2.Value.Year, DateTimePicker2.Value.Month, DateTimePicker2.Value.Day, dateTimePicker6.Value.Hour, dateTimePicker6.Value.Minute, 0),
                                    new DateTime(DateTimePicker4.Value.Year, DateTimePicker4.Value.Month, DateTimePicker4.Value.Day, dateTimePicker7.Value.Hour, dateTimePicker7.Value.Minute, 0),
                                    (TB_sp_prob.Text.Equals("") ? 0 : double.Parse(TB_sp_prob.Text)), upload_pl);
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    using (var sqlCmd = MainPluginClass.App.SqlWork())
                                    {
                                        sqlCmd.sql = @"INSERT INTO esmc_sync.errors(exception, data, resolved, operation)
    VALUES (@exception, @data, @resolved, @operation);";
                                        sqlCmd.AddParam(new Params("@exception", ex.Message, NpgsqlDbType.Text));
                                        sqlCmd.AddParam(new Params("@data", ex.Message, NpgsqlDbType.Text));
                                        sqlCmd.AddParam(new Params("@resolved", false, NpgsqlDbType.Boolean));
                                        sqlCmd.AddParam(new Params("@operation", "SendDataToMT", NpgsqlDbType.Text));
                                        sqlCmd.ExecuteNonQuery();
                                    }
                                }
                                catch
                                {
                                    //MessageBox.Show("Ошибка в методе SendDataToMT! Описание:" + Environment.NewLine +
                                    //ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            #region Получаем параметры датчиков
                            try
                            {
                                // Запись в путевой лист информации из автомониторинга о затратах топлива
                                long from = MTAPI_Helper.GetUnixTime(DateTimePicker2.Value);
                                long till = MTAPI_Helper.GetUnixTime(DateTimePicker4.Value);
                                carExternalId = GetExternalId(((myItem)CB_gos_no.SelectedItem).GetId, "cars");
                                if (carExternalId != null)
                                {
                                    // Получить список типов датчиков
                                    String gaugeTypesJSon = MTAPI_Helper.Get(String.Format(MTAPI_Helper.mt_url + "/gauges/types?token={0}", Token));
                                    List<MT_GaugeType> gaugeTypes = JsonHelper.JsonDeserialize<List<MT_GaugeType>>(gaugeTypesJSon);
                                    if (gaugeTypes != null && gaugeTypes.Count > 0)
                                    {
                                        // Выбрать из списка датчик "Топливо" и узнать его typeId, port
                                        MT_GaugeType fuelGaugeType = (from MT_GaugeType gt in gaugeTypes where gt.name == "Топливо" select gt).FirstOrDefault();
                                        if (fuelGaugeType != null)
                                        {
                                            // Для текущего автомобиля получить список датчиков
                                            String carGaugesJSon = MTAPI_Helper.Get(
                                                String.Format(MTAPI_Helper.mt_url + "/cars/{0}/gauges?token={1}",
                                                        carExternalId,
                                                        Token));
                                            List<MT_Gauge> carGauges = JsonHelper.JsonDeserialize<List<MT_Gauge>>(carGaugesJSon);
                                            if (carGauges != null && carGauges.Count > 0)
                                            {
                                                // Выбрать из списка датчики, чьи typeId соответствуют датчику топлива
                                                IEnumerable<MT_Gauge> fuelGauges = from MT_Gauge g in carGauges where g.typeId == fuelGaugeType.id select g;
                                                if (fuelGauges.Count() > 0)
                                                {
                                                    MT_Gauge firstGauge = fuelGauges.ElementAt(0);
                                                    // Получить показания первого датчика за выбранный период
                                                    String gaugeValuesJSon = MTAPI_Helper.Get(
                                                        String.Format(MTAPI_Helper.mt_url + "/reports/gaugehistory/{0}/{1}/{2}/{3}/{4}?token={5}",
                                                                carExternalId,
                                                                firstGauge.port,
                                                                fuelGaugeType.id,
                                                                from,
                                                                till,
                                                                Token));
                                                    List<MT_GaugeValue> gaugeValues = JsonHelper.JsonDeserialize<List<MT_GaugeValue>>(gaugeValuesJSon);
                                                    if (gaugeValues != null && gaugeValues.Count > 1)
                                                    {
                                                        MT_GaugeValue firstValue = gaugeValues[0];
                                                        MT_GaugeValue lastValue = gaugeValues[gaugeValues.Count - 1];
                                                        // Записать показания первого датчика за выбранный период как значения для первого бака
                                                        upload_pl.sql =
                                                            @"UPDATE autobase.waybills SET 
                                                                    fuel_begin_glonass=:pl_glfuel_begin, 
                                                                    fuel_end_glonass=:pl_glfuel_end WHERE gid = " + _id.Value;
                                                        List<Params> _params = new List<Params>();
                                                        _params.Add(new Params(":pl_glfuel_begin", firstValue.value, NpgsqlDbType.Numeric));
                                                        _params.Add(new Params(":pl_glfuel_end", lastValue.value, NpgsqlDbType.Numeric));
                                                        upload_pl.ExecuteNonQuery(_params);
                                                    }
                                                }

                                                // Если есть второй датчик, записать его показания как значения для второго бака
                                                if (fuelGauges.Count() > 1)
                                                {
                                                    MT_Gauge secondGauge = fuelGauges.ElementAt(1);
                                                    // Получить показания первого датчика за выбранный период
                                                    String secondGaugeValuesJSon = MTAPI_Helper.Get(
                                                        String.Format(MTAPI_Helper.mt_url + "/reports/gaugehistory/{0}/{1}/{2}/{3}/{4}?token={5}",
                                                                carExternalId,
                                                                secondGauge.port,
                                                                fuelGaugeType.id,
                                                                from,
                                                                till,
                                                                Token));
                                                    List<MT_GaugeValue> secondGaugeValues = JsonHelper.JsonDeserialize<List<MT_GaugeValue>>(secondGaugeValuesJSon);
                                                    if (secondGaugeValues != null && secondGaugeValues.Count > 1)
                                                    {
                                                        MT_GaugeValue firstValue = secondGaugeValues[0];
                                                        MT_GaugeValue lastValue = secondGaugeValues[secondGaugeValues.Count - 1];
                                                        // Записать показания первого датчика за выбранный период как значения для первого бака
                                                        upload_pl.sql =
                                                            @"UPDATE autobase.waybills SET 
                                                                    fuel_begin2_glonass=:pl_glfuel_begin2, 
                                                                    fuel_end2_glonass=:pl_glfuel_end2 WHERE gid = " + _id.Value;
                                                        List<Params> _params = new List<Params>();
                                                        _params.Add(new Params(":pl_glfuel_begin2", firstValue.value, NpgsqlDbType.Numeric));
                                                        _params.Add(new Params(":pl_glfuel_end2", lastValue.value, NpgsqlDbType.Numeric));
                                                        upload_pl.ExecuteNonQuery(_params);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show("Ошибка при получении параметров датчиков! Описание:" + Environment.NewLine +
                                //    ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            #endregion
                        }
                        #endregion
                        if (_id == null)
                        {
                            //try
                            //{
                            //    DateTime from = new DateTime(DateTimePicker1.Value.Year, DateTimePicker1.Value.Month, DateTimePicker1.Value.Day, dateTimePicker5.Value.Hour, dateTimePicker5.Value.Minute, 0);
                            //    DateTime till = new DateTime(DateTimePicker3.Value.Year, DateTimePicker3.Value.Month, DateTimePicker3.Value.Day, dateTimePicker8.Value.Hour, dateTimePicker8.Value.Minute, 0);
                            //    String description = "Сгенерированная задача";

                            //    TimeSpan duration = till - from;
                            //    if (duration > new TimeSpan(0, 1, 0, 0) &&
                            //        duration <= new TimeSpan(1, 0, 0, 0))
                            //    {
                            //        long carGid = ((myItem)CB_gos_no.SelectedItem).GetId;
                            //        long routeGid = ((myItem)CB_route.SelectedItem).GetId;
                            //        long typeGid = ((myItem)CB_wrktype.SelectedItem).GetId;

                            //        long? carExternalId = GetExternalId(carGid, "cars");
                            //        long? routeExternalId = GetExternalId(routeGid, "waybills_routes");
                            //        long? typeExternalId = GetExternalId(typeGid, "waybills_work_types");

                            //        if (carExternalId != null && routeExternalId != null && typeExternalId != null)
                            //        {
                            //            MT_CarsTask carsTask = new MT_CarsTask();
                            //            carsTask.carIds = new List<long>() { carExternalId.Value };
                            //            carsTask.description = description;
                            //            carsTask.from = MTAPI_Helper.GetUnixTime(from);
                            //            carsTask.routeId = routeExternalId.Value;
                            //            carsTask.till = MTAPI_Helper.GetUnixTime(till);
                            //            carsTask.typeId = typeExternalId.Value;
                            //            MTAPI_Helper.PostCarsTask(carsTask, Token);
                            //        }
                            //    }
                            //}
                            //catch
                            //{

                            //}

                            upload_pl.sql = "SELECT gid from autobase.waybills WHERE doc_no = currval('" + org_put_list_seq_name + "')::character varying and car_id=" + ((myItem)CB_gos_no.SelectedItem).GetId.ToString();
                            _id = (int)upload_pl.ExecuteScalar();
                            upload_pl.sql = "SELECT doc_no from autobase.waybills WHERE gid = " + _id.ToString();
                            TextBox1.Text = upload_pl.ExecuteScalar().ToString();
                        }
                        upload_pl.EndTransaction();
                        this.UserControlAttr_Load(this, new EventArgs());
                    }
                    catch
                    {
                        upload_pl.EndTransaction();
                        upload_pl.Close();
                        return;
                    }

                    upload_pl.Close();
                }
            }
        }

        private void CB_gar_no_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_gar_no.SelectedItem).GetId;
                Set_dict_values(r, CB_gos_no);
                CB_gar_no.BackColor = SystemColors.Window;
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора автомобиля!"); }
        }

        void CB_p_gos_no_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sql_pr_details = MainPluginClass.App.SqlWork();
            try
            {
                int r = ((myItem)CB_p_gos_no.SelectedItem).GetId;
                sql_pr_details.sql = "SELECT cmd.name FROM autobase.cars c LEFT JOIN autobase.cars_models cmd ON c.model_id = cmd.gid WHERE c.gid=" + r.ToString();
                sql_pr_details.ExecuteReader();
                if (sql_pr_details.CanRead())
                {
                    TB_prmodel.Text = sql_pr_details.GetString(0);
                    TB_prmodel.Enabled = true;
                }
                else
                {
                    TB_prmodel.Text = "";
                }
                CB_p_gos_no.BackColor = SystemColors.Window;
                Set_dict_values(r, CB_p_gar_no);
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора автомобиля!"); }
            sql_pr_details.Close();
        }

        private void CB_p_gar_no_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int r = ((myItem)CB_p_gar_no.SelectedItem).GetId;
                Set_dict_values(r, CB_p_gos_no);
            }
            catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения! " + x.Message, "Ошибка выбора автомобиля!"); }
        }

        private void btn_print_click(object sender, EventArgs e)
        {
            //Проверим, нужно ли делать мед. проверку сотрудника:
            bool can_med = false;
            ISQLCommand cmd = MainPluginClass.App.SqlWork();
            try
            {
                cmd.sql = "select waybill_med_checks from autobase.orgs where gid = " + org_id.ToString();
                cmd.ExecuteReader();
                if (cmd.CanRead())
                {
                    can_med = cmd.GetBoolean(0);
                }
            }
            catch
            {
                can_med = false;
            }
            cmd.Close();
            if (can_med)
            {
                try
                {
                    if (EmployeesSync.Url != null)
                    {
                        EmployeesSync temp = new EmployeesSync();
                        temp.WayBillSyncMain(((myItem)CB_pl_drivertn.SelectedItem).Name);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Не удалось запросить последние данные по мед. проверки сотрудника!", "Ошибка загрузки!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            try
            {
                if (this._id != null)
                {
                    var list = MainPluginClass.Work.FastReport.FindReportsByIdTable(MapEditorTablePutList).ToArray();
                    MainPluginClass.Work.FastReport.OpenReport(list[0], new FilterTable(this._id.Value, MapEditorTablePutList, " gid", ""));
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

        private void TB_modelid_TextChanged(object sender, EventArgs e)
        {
            //////////////////           ОПЕРАЦИИ ПТО в GRID            ////////////////

            operacija.Items.Clear();
            //DataGridView1.Rows.Clear();
            if (_id == null) { return; } // Если мы только созлает путевой лист - операции нам не нужны,
            // Заодно избавимся от ошибки. когда гос. номер не загружен
            if (TB_modelid.Text != null)
            {
                var sql_pto = MainPluginClass.App.SqlWork();
                try
                {
                    sql_pto.sql = "select DISTINCT (o.name) operation_name, o.gid operation_id from  autobase.waybills_fuel_expenses fe, autobase.waybills_operations o where fe.operation_id=o.gid and (fe.model_id = " + TB_modelid.Text.ToString() + " or fe.car_id = " + (((myItem)CB_gos_no.SelectedItem).GetId).ToString() + ") and fe.org_id = " + org_id.ToString() +
                        " union select name, gid from autobase.waybills_operations where gid in (select distinct operation_id from autobase.waybills_taks where waybill_id = " + _id.ToString() + ")";
                    sql_pto.ExecuteReader();
                    while (sql_pto.CanRead())
                    {
                        myItem oper = new myItem(sql_pto.GetString(0), sql_pto.GetInt32(1));
                        operacija.Items.Add(oper);
                        operacija.DisplayMember = "Name";
                        operacija.ValueMember = "GetId";
                    }
                }
                catch (Exception x) { MessageBox.Show("Ошибка! Невозможно установить значения операции! " + x.Message, "Ошибка выбора автомобиля!"); }
                sql_pto.Close();
            }
        }

        private void CB_rrab_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_rrab.BackColor = SystemColors.Window;
            if (CB_rrab.SelectedItem != null && secsave < 2)
            {
                this.dateTimePicker8.Value = this.DateTimePicker1.Value.AddHours(this.dateTimePicker5.Value.Hour + Convert.ToInt32(((myItem)CB_rrab.SelectedItem).Data)).AddMinutes(this.dateTimePicker5.Value.Minute);
                this.DateTimePicker3.Value = this.dateTimePicker8.Value;
            }
        }

        private void CB_route_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_route.BackColor = SystemColors.Window;
        }

        private void CB_wrktype_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_wrktype.BackColor = SystemColors.Window;
        }

        private void CB_gruztype_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_gruztype.BackColor = SystemColors.Window;
        }

        private void CB_pl_zone_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_pl_zone.BackColor = SystemColors.Window;
        }

        private void CB_top1_SelectedIndexChanged(object sender, EventArgs e)
        {

            //////////////////           ТОПЛИВО 1              ////////////////
            if (CB_gos_no.SelectedItem != null && CB_top1.SelectedItem != null)
            {
                CB_top1.BackColor = SystemColors.Window;
                // Подгрузка топливной карты
                ISQLCommand my_sql = MainPluginClass.App.SqlWork();
                int r = ((myItem)CB_gos_no.SelectedItem).GetId;
                CB_fuel_card.Items.Clear();
                CB_fuel_card.Items.Add(new myItem(" ", -99));
                try
                {
                    my_sql.sql = @"SELECT card_no, gid FROM autobase.waybills_fuel_cards WHERE (fuel_mark_id = " + ((myItem)CB_top1.SelectedItem).GetId.ToString() + @" and org_id = " + org_id.ToString() + ") " + (CB_fuel_card.Tag != null ? " or gid =" + ((int)CB_fuel_card.Tag).ToString() : "");
                    my_sql.ExecuteReader();
                    while (my_sql.CanRead())
                    {
                        myItem y = new myItem(my_sql.GetString(0) == null ? "-" : my_sql.GetString(0), my_sql.GetInt32(1));
                        CB_fuel_card.Items.Add(y);
                    }
                    my_sql.Close();
                }
                catch { }

                if (_id != null)
                {
                    if (CB_fuel_card.Tag == null)
                    {
                        /* 2014-10-01 forge http://forge.gradoservice.ru/issues/15975
                        var sql_sp = MainPluginClass.App.SqlWork();
                        sql_sp.sql = "SELECT \"_RealValue_fuel_card_id\" FROM esmc.put_lists_vw WHERE gid = (SELECT esmc.get_last_second_put_list(" + r + "));";
                        sql_sp.ExecuteReader();
                        if (sql_sp.CanRead())
                        {
                            if (sql_sp.GetValue(0) != null)
                            {
                                Set_dict_values(sql_sp.GetInt32(0), CB_fuel_card);
                                CB_fuel_card.Tag = sql_sp.GetInt32(0);
                            }
                            Set_dict_values((int)CB_fuel_card.Tag, CB_fuel_card);
                        }
                        sql_sp.Close();
                        */
                        CB_fuel_card.SelectedIndex = -1;
                        CB_fuel_card.SelectedItem = null;
                    }
                    else
                    {
                        Set_dict_values((int)CB_fuel_card.Tag, CB_fuel_card);
                    }
                }
            }
            else
            {
                CB_top1.SelectedIndexChanged -= CB_top1_SelectedIndexChanged;
                CB_top1.SelectedItem = null;
                CB_top1.SelectedIndexChanged += CB_top1_SelectedIndexChanged;
                MessageBox.Show("Сначала выберите автомобиль!", "Ошибка загрузки!");
            }
        }

        private void CB_top2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_gos_no.SelectedItem != null && CB_top2.SelectedItem != null)
            {
                int r = ((myItem)CB_gos_no.SelectedItem).GetId;
                CB_top2.BackColor = SystemColors.Window;
                CB_fuel_card2.SelectedIndex = -1;
                CB_fuel_card2.SelectedItem = null;
                CB_fuel_card2.Items.Clear();
                CB_fuel_card2.Items.Add(new myItem(" ", -99));
                ////////////////           ТОПЛИВО 2              ////////////////
                var sql_t2 = MainPluginClass.App.SqlWork();
                if (CB_top2.SelectedItem.ToString().ToUpper().Trim().Equals("НЕТ"))
                {
                    TB_t2_f.BackColor = SystemColors.Window;
                    TB_t2_f.Enabled = false;
                    TB_t2_f.Text = "";
                    TB_t2_pl.BackColor = SystemColors.Window;
                    TB_t2_pl.Enabled = false;
                    TB_t2_pl.Text = "";
                    TB_t2_n.BackColor = SystemColors.Window;
                    TB_t2_n.Enabled = false;
                    TB_t2_n.Text = "";
                    TB_t2_k.BackColor = SystemColors.Window;
                    TB_t2_k.Enabled = false;
                    TB_t2_k.Text = "";
                    CB_fuel_card2.Enabled = false;
                }
                else
                {
                    ISQLCommand my_sql = MainPluginClass.App.SqlWork();
                    try
                    {
                        my_sql.sql = @"SELECT card_no, gid FROM autobase.waybills_fuel_cards WHERE (fuel_mark_id = " + ((myItem)CB_top2.SelectedItem).GetId.ToString() + @" and org_id = " + org_id.ToString() + ") " + (CB_fuel_card2.Tag != null ? " or gid =" + ((int)CB_fuel_card2.Tag).ToString() : "");
                        my_sql.ExecuteReader();
                        while (my_sql.CanRead())
                        {
                            myItem y = new myItem(my_sql.GetString(0) == null ? "-" : my_sql.GetString(0), my_sql.GetInt32(1));
                            CB_fuel_card2.Items.Add(y);
                        }
                        my_sql.Close();
                    }
                    catch { }
                    if (_id != null)
                    {
                        if (CB_fuel_card2.Tag != null)
                        {
                            Set_dict_values((int)CB_fuel_card2.Tag, CB_fuel_card2);
                        }
                    }
                    if (secsave < 1)
                    {
                        TB_t2_pl.Enabled = true;
                        if (TB_t2_pl.Text == "") { TB_t2_pl.BackColor = System.Drawing.Color.FromArgb(255, 224, 192); };
                        TB_t2_n.Enabled = true;
                        if (TB_t2_n.Text == "") { TB_t2_n.BackColor = System.Drawing.Color.FromArgb(255, 224, 192); };
                    };
                    if (secsave == 1)
                    {
                        TB_t2_f.Enabled = true;
                        if (TB_t2_f.Text == "") { TB_t2_f.BackColor = System.Drawing.Color.FromArgb(255, 224, 192); };
                        TB_t2_k.Enabled = true;
                        if (TB_t2_k.Text == "") { TB_t2_k.BackColor = System.Drawing.Color.FromArgb(255, 224, 192); }
                        CB_fuel_card2.Enabled = true;
                    };
                }
            }
            else
            {
                CB_top2.SelectedIndexChanged -= CB_top2_SelectedIndexChanged;
                CB_top2.SelectedItem = null;
                CB_top2.SelectedIndexChanged += CB_top2_SelectedIndexChanged;
                MessageBox.Show("Сначала выберите автомобиль!", "Ошибка загрузки!");
            }
        }


        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            List<int> _idObjects = new List<int>();
            _idObjects.Clear();
            foreach (DataGridViewRow row in DataGridView1.SelectedRows)
            {
                _idObjects.Add(Convert.ToInt32(row.Cells[0].Value));
                if (Convert.ToInt32(row.Cells[0].Value) == 0)
                {
                    btn_tax_del.Enabled = true;
                }
                else
                {
                    btn_tax_del.Enabled = false;
                }

            }
            if (_idObjects.Count == 0)
            {
                btn_tax_del.Enabled = false;
            }

        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker2.MinDate = DateTimePicker1.Value;
            DateTimePicker3.MinDate = DateTimePicker1.Value;
            CB_rrab_SelectedIndexChanged(null, null);
        }

        private void DateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker4.MinDate = DateTimePicker3.Value;
        }

        private void CB_pl_master_SelectedIndexChanged(object sender, EventArgs e)
        {
            CB_pl_master.BackColor = SystemColors.Window;
        }

        private void T1_T2_TextChanged(object sender, EventArgs e)
        {
            // Раскраска
            if (sender as MyComponents.TextBoxNumber == TB_t2_f) { if (secsave == 1) { TB_t2_f.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_t2_pl) { TB_t2_pl.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_t2_k) { if (secsave == 1) { TB_t2_k.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_t2_n) { TB_t2_n.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_t1_pl) { TB_t1_pl.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_t1_f) { if (secsave == 1) { TB_t1_f.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_t1_n) { TB_t1_n.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_t1_k) { if (secsave == 1) { TB_t1_k.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_spk) { if (secsave == 1) { TB_spk.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_sp_n) { if (secsave == 1) { TB_sp_n.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_Ezdok) { TB_Ezdok.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_mch) { TB_mch.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_mch_obr) { TB_mch_obr.BackColor = SystemColors.Window; }
            else if (sender as MyComponents.TextBoxNumber == TB_sp_n) { if (secsave == 1) { TB_mch_obr_k.BackColor = SystemColors.Window; } }
            else if (sender as MyComponents.TextBoxNumber == TB_mch_k) { if (secsave == 1) { TB_mch_k.BackColor = SystemColors.Window; } }

            // Расчет, если путевой лист не закрыт. Или он редактируемый
            if (secsave <= 1 || isEdited)
            {
                decimal TB_t1_n_dec = 0; decimal.TryParse(TB_t1_n.Text, out TB_t1_n_dec);
                decimal TB_t1_f_dec = 0; decimal.TryParse(TB_t1_f.Text, out TB_t1_f_dec);
                decimal TB_tax_n_dec = 0; decimal.TryParse(TB_tax_n.Text, out TB_tax_n_dec);
                decimal TB_spk_dec = 0; decimal.TryParse(TB_spk.Text, out TB_spk_dec);
                decimal TB_sp_n_dec = 0; decimal.TryParse(TB_sp_n.Text, out TB_sp_n_dec);
                decimal TB_mch_dec = 0; decimal.TryParse(TB_mch.Text, out TB_mch_dec);
                decimal TB_mch_k_dec = 0; decimal.TryParse(TB_mch_k.Text, out TB_mch_k_dec);
                decimal TB_mch_obr_k_dec = 0; decimal.TryParse(TB_mch_obr_k.Text, out TB_mch_obr_k_dec);
                decimal TB_mch_obr_dec = 0; decimal.TryParse(TB_mch_obr.Text, out TB_mch_obr_dec);
                if (CHB_calc_norm_type.Checked && secsave == 1) // Если расчет "Конец, л" делать по какой-то там норме
                {
                    TB_t1_k.TextChanged -= T1_T2_TextChanged;
                    TB_t1_k.Text = (TB_t1_n_dec + TB_t1_f_dec - TB_tax_n_dec).ToString();
                    TB_t1_k.TextChanged += T1_T2_TextChanged;
                }
                decimal TB_t1_k_dec = 0; decimal.TryParse(TB_t1_k.Text, out TB_t1_k_dec);
                if (TB_t1_k_dec <= TB_t1_n_dec + TB_t1_f_dec)
                {
                    TB_tax_f.TextChanged -= T1_T2_TextChanged;
                    TB_tax_f.Text = (TB_t1_f_dec + TB_t1_n_dec - TB_t1_k_dec).ToString();
                    TB_tax_f.TextChanged += T1_T2_TextChanged;
                }
                else
                {
                    TB_tax_f.TextChanged -= T1_T2_TextChanged;
                    TB_tax_f.Text = "0";
                    TB_tax_f.TextChanged += T1_T2_TextChanged;
                };
                decimal TB_tax_f_dec = 0; decimal.TryParse(TB_tax_f.Text, out TB_tax_f_dec);
                if (TB_mch_obr_k_dec >= TB_mch_obr_dec)
                {
                    TB_mch_obr_pr.Text = (TB_mch_obr_k_dec - TB_mch_obr_dec).ToString();
                }
                else
                {
                    TB_mch_obr_pr.Text = "0";
                };
                if (TB_mch_k_dec >= TB_mch_dec)
                {
                    TB_mch_pr.Text = (TB_mch_k_dec - TB_mch_dec).ToString();
                }
                else
                {
                    TB_mch_pr.Text = "0";
                };
                if (TB_spk_dec >= TB_sp_n_dec)
                {
                    TB_sp_prob.Text = (TB_spk_dec - TB_sp_n_dec).ToString();
                }
                else
                {
                    TB_sp_prob.Text = "0";
                };

                if (TB_tax_n_dec >= 0 && TB_tax_f_dec >= 0)
                {
                    TB_tax_r.Text = (TB_tax_n_dec - TB_tax_f_dec).ToString();
                }
            }
        }

        private CarInfoM GetCarInfo(int id_car)
        {
            CarInfoM car = new CarInfoM();
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = "SELECT glonass_id, external_id FROM autobase.cars WHERE gid = " + id_car.ToString();
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    car.id_ME = id_car;
                    car.glonass_id = sqlCmd.GetValue<int>("glonass_id");
                    car.id_MT = sqlCmd.GetValue<int>("external_id");
                }
            }
            return car;
        }

        private long? GetExternalId(long gid, String tableName)
        {
            long? result = null;
            using (var sqlCmd = MainPluginClass.App.SqlWork())
            {
                sqlCmd.sql = String.Format("SELECT external_id FROM autobase.{0} WHERE gid = {1}", tableName, gid);
                result = sqlCmd.ExecuteScalar<Int32>();
            }
            return result;
        }

        private void SendDataToMT(int id_car, DateTime data_begin, DateTime data_end, double distance, ISQLCommand sqlCmd)
        {
            CarInfoM car = GetCarInfo(id_car);
            if (car.glonass_id != 0)
            {
                MT_CarShortReportModel car_short_report = null;
                try
                {
                    car_short_report = MTAPI_Helper.GetCarShortReport(car.glonass_id, data_begin, data_end, Token);
                }
                catch (Exception)
                {
                    car_short_report = new MT_CarShortReportModel();
                    car_short_report.length = 0;
                    car_short_report.fuel = 0;
                }

                //Василий просил закомментировать 
                //sqlCmd.sql = "UPDATE esmc.put_lists SET pl_glrun_km=:pl_glrun_km, pl_glfuel_fact=:pl_glfuel_fact WHERE gid = " + _id.Value.ToString();
                //List<Params> _params = new List<Params>();
                //_params.Add(new Params(":pl_glrun_km", car_short_report.length, NpgsqlDbType.Numeric));
                //_params.Add(new Params(":pl_glfuel_fact", car_short_report.fuel, NpgsqlDbType.Numeric));

                sqlCmd.sql = "UPDATE autobase.waybills SET km_run_glonass=:pl_glrun_km WHERE gid = " + _id.Value.ToString();
                List<Params> _params = new List<Params>();
                _params.Add(new Params(":pl_glrun_km", car_short_report.length, NpgsqlDbType.Numeric));
                sqlCmd.ExecuteNonQuery(_params);
            }
            if (car.id_MT != 0)
            {
                List<MT_CarWayBill> way_bills = new List<MT_CarWayBill>();
                way_bills.Add(new MT_CarWayBill()
                {
                    carId = car.id_MT,
                    distance = distance,
                    dateFrom = MTAPI_Helper.GetUnixTime(data_begin),
                    dateTill = MTAPI_Helper.GetUnixTime(data_end)
                });
                MTAPI_Helper.PostWayBills(way_bills, Token);
            }
        }

        private static DateTime _token_time;

        private static string _token;

        public static string Token
        {
            get
            {
                if (_token_time.AddMinutes(29) < DateTime.Now || String.IsNullOrEmpty(_token))
                {
                    using (var sqlCmd = MainPluginClass.App.SqlWork())
                    {
                        sqlCmd.sql = "SELECT autobase.get_automap_token();";
                        _token = sqlCmd.ExecuteScalar().ToString();
                        _token_time = DateTime.Now;
                    }
                }
                return _token;
            }
        }

        private void create_task_btn_Click(object sender, EventArgs e)
        {

            CreateTaskWayBillForm frm = new CreateTaskWayBillForm(_id.Value, org_id);
            frm.ShowDialog();
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    int currentCarExternalId = 218;
        //    long from = 1408277053000;
        //    long till = 1410424268127;
        //    // Получить список типов датчиков
        //    String gaugeTypesJSon = MTAPI_Helper.Get(String.Format("http://gbu.asuds77.ru:8000/gauges/types?token={0}", Token));
        //    List<MT_GaugeType> gaugeTypes = JsonHelper.JsonDeserialize<List<MT_GaugeType>>(gaugeTypesJSon);
        //    if (gaugeTypes != null && gaugeTypes.Count > 0)
        //    {
        //        // Выбрать из списка датчик "Топливо" и узнать его typeId, port
        //        MT_GaugeType fuelGaugeType = (from MT_GaugeType gt in gaugeTypes where gt.name == "Топливо" select gt).FirstOrDefault();
        //        if (fuelGaugeType != null)
        //        {
        //            // Для текущего автомобиля получить список датчиков
        //            String carGaugesJSon = MTAPI_Helper.Get(String.Format("http://gbu.asuds77.ru:8000/cars/{0}/gauges?token={1}", currentCarExternalId, Token));
        //            List<MT_Gauge> carGauges = JsonHelper.JsonDeserialize<List<MT_Gauge>>(carGaugesJSon);
        //            if(carGauges != null && carGauges.Count > 0)
        //            {
        //                // Выбрать из списка датчики, чьи typeId соответствуют датчику топлива
        //                IEnumerable<MT_Gauge> fuelGauges = from MT_Gauge g in carGauges where g.typeId == fuelGaugeType.id select g;
        //                if (fuelGauges.Count() > 0)
        //                {
        //                    MT_Gauge firstGauge = fuelGauges.ElementAt(0);
        //                    // Получить показания первого датчика за выбранный период
        //                    String gaugeValuesJSon = MTAPI_Helper.Get(
        //                        String.Format("http://gbu.asuds77.ru:8000/reports/gaugehistory/{0}/{1}/{2}/{3}/{4}?token={5}",
        //                                currentCarExternalId, 
        //                                firstGauge.port, 
        //                                fuelGaugeType.id,
        //                                from, 
        //                                till,
        //                                Token));
        //                    List<MT_GaugeValue> gaugeValues = JsonHelper.JsonDeserialize<List<MT_GaugeValue>>(gaugeValuesJSon);
        //                    if (gaugeValues != null && gaugeValues.Count > 1)
        //                    {
        //                        MT_GaugeValue firstValue = gaugeValues[0];
        //                        MT_GaugeValue lastValue = gaugeValues[gaugeValues.Count - 1];
        //                        // Записать показания первого датчика за выбранный период как значения для первого бака
        //                    }
        //                    // Если есть второй датчик, записать его показания как значения для второго бака
        //                }
        //            }
        //        }
        //    }
        //}

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    var sw = System.IO.File.AppendText("E:\\gauges.txt");
        //    for(int i = 81; i<=4143; i++)
        //    {
        //        String carGauges = "[]";
        //        try
        //        {
        //            carGauges = MTAPI_Helper.Get(String.Format("http://gbu.asuds77.ru:8000/cars/{0}/gauges?token={1}", i, Token));
        //        }
        //        catch (Exception)
        //        { }
        //        if(carGauges != "[]")
        //        {
        //            sw.WriteLine("Car id: {0}", i);
        //            sw.WriteLine("Car gauges:");
        //            sw.WriteLine(carGauges);
        //            sw.WriteLine("==============================================================================================================");
        //            sw.WriteLine();                  
        //        }
        //    }
        //    sw.Close();
        //}
    }
}