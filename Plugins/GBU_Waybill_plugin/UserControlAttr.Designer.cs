using GBU_Waybill_plugin.MyComponents;
using System.Windows.Forms;

namespace GBU_Waybill_plugin
{
    partial class UserControlAttr
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            this.SC_1 = new System.Windows.Forms.SplitContainer();
            this.groupBox24 = new System.Windows.Forms.GroupBox();
            this.textBox_fuelEquipPlan = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_fuel1hPlan = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_fuelConsFact = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label103 = new System.Windows.Forms.Label();
            this.textBox_fuel100kmFact = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.textBox_fuel100kmPlan = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label101 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.textBox_fuelConsNorm = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label98 = new System.Windows.Forms.Label();
            this.textBox_fuelEnd = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.textBox_fuelBegin = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label99 = new System.Windows.Forms.Label();
            this.label100 = new System.Windows.Forms.Label();
            this.textBox_kmDiff = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label97 = new System.Windows.Forms.Label();
            this.textBox_kmEnd = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.textBox_kmBegin = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label95 = new System.Windows.Forms.Label();
            this.label96 = new System.Windows.Forms.Label();
            this.groupBox23 = new System.Windows.Forms.GroupBox();
            this.textBox_fuelGaugeCons = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.textBox_kmGaugeDist = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label106 = new System.Windows.Forms.Label();
            this.label107 = new System.Windows.Forms.Label();
            this.textBox_fuelGaugeEnd = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.textBox_kmGaugeEnd = new GBU_Waybill_plugin.MyComponents.TextBoxNumber();
            this.label104 = new System.Windows.Forms.Label();
            this.label105 = new System.Windows.Forms.Label();
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.button_dgDel = new System.Windows.Forms.Button();
            this.dataGridView_trip = new System.Windows.Forms.DataGridView();
            this.dg_trip_gid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_date_out = new GBU_Waybill_plugin.MyComponents.CalendarColumn();
            this.dg_date_return = new GBU_Waybill_plugin.MyComponents.CalendarColumn();
            this.dg_odo_begin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_odo_return = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_got = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_equip_motohours = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_cons_equip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_motohours_stop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_cons_stop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_out = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_return = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_route_len = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_cons_norm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_cons_fact = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_fuel_diff = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_car_norm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_car_equip_norm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_100km_plan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_100km_fact = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_car_regime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_car_regime_hours = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dg_car_fuel_tank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_dgAdd = new System.Windows.Forms.Button();
            this.groupBox21 = new System.Windows.Forms.GroupBox();
            this.dateTimePicker_timeReturnFact = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_dateReturnFact = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_timeOutFact = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_dateOutFact = new System.Windows.Forms.DateTimePicker();
            this.label92 = new System.Windows.Forms.Label();
            this.label94 = new System.Windows.Forms.Label();
            this.label91 = new System.Windows.Forms.Label();
            this.label93 = new System.Windows.Forms.Label();
            this.groupBox20 = new System.Windows.Forms.GroupBox();
            this.dateTimePicker_timeReturnPlan = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_timeOutPlan = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_dateReturnPlan = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_dateOutPlan = new System.Windows.Forms.DateTimePicker();
            this.label89 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.label87 = new System.Windows.Forms.Label();
            this.label88 = new System.Windows.Forms.Label();
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.comboBox_car = new System.Windows.Forms.ComboBox();
            this.textBox_regNum = new System.Windows.Forms.TextBox();
            this.label86 = new System.Windows.Forms.Label();
            this.textBox_garNum = new System.Windows.Forms.TextBox();
            this.label84 = new System.Windows.Forms.Label();
            this.textBox_gosNum = new System.Windows.Forms.TextBox();
            this.label85 = new System.Windows.Forms.Label();
            this.textBox_carType = new System.Windows.Forms.TextBox();
            this.label83 = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.groupBox18 = new System.Windows.Forms.GroupBox();
            this.comboBox_driver2 = new System.Windows.Forms.ComboBox();
            this.comboBox_driver1 = new System.Windows.Forms.ComboBox();
            this.textBox_udos2 = new System.Windows.Forms.TextBox();
            this.label81 = new System.Windows.Forms.Label();
            this.textBox_udos1 = new System.Windows.Forms.TextBox();
            this.label80 = new System.Windows.Forms.Label();
            this.label79 = new System.Windows.Forms.Label();
            this.label78 = new System.Windows.Forms.Label();
            this.groupBox17 = new System.Windows.Forms.GroupBox();
            this.comboBox_service = new System.Windows.Forms.ComboBox();
            this.comboBox_issue = new System.Windows.Forms.ComboBox();
            this.textBox_issType = new System.Windows.Forms.TextBox();
            this.label77 = new System.Windows.Forms.Label();
            this.textBox_issSource = new System.Windows.Forms.TextBox();
            this.label76 = new System.Windows.Forms.Label();
            this.textBox_issDest = new System.Windows.Forms.TextBox();
            this.label75 = new System.Windows.Forms.Label();
            this.textBox_rasp = new System.Windows.Forms.TextBox();
            this.label74 = new System.Windows.Forms.Label();
            this.textBox_issueNum = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.textBox_userDept = new System.Windows.Forms.TextBox();
            this.label72 = new System.Windows.Forms.Label();
            this.textBox_plNum = new System.Windows.Forms.TextBox();
            this.label71 = new System.Windows.Forms.Label();
            this.splitButton_save = new GBU_Waybill_plugin.SplitButton();
            this.splitButton1 = new GBU_Waybill_plugin.SplitButton();
            this.BTN_cancel = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.calendarColumn1 = new GBU_Waybill_plugin.MyComponents.CalendarColumn();
            this.calendarColumn2 = new GBU_Waybill_plugin.MyComponents.CalendarColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.SC_1)).BeginInit();
            this.SC_1.Panel1.SuspendLayout();
            this.SC_1.Panel2.SuspendLayout();
            this.SC_1.SuspendLayout();
            this.groupBox24.SuspendLayout();
            this.groupBox23.SuspendLayout();
            this.groupBox22.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_trip)).BeginInit();
            this.groupBox21.SuspendLayout();
            this.groupBox20.SuspendLayout();
            this.groupBox19.SuspendLayout();
            this.groupBox18.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.SuspendLayout();
            // 
            // SC_1
            // 
            this.SC_1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SC_1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SC_1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.SC_1.IsSplitterFixed = true;
            this.SC_1.Location = new System.Drawing.Point(0, 0);
            this.SC_1.Name = "SC_1";
            this.SC_1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SC_1.Panel1
            // 
            this.SC_1.Panel1.AutoScroll = true;
            this.SC_1.Panel1.Controls.Add(this.groupBox24);
            this.SC_1.Panel1.Controls.Add(this.groupBox23);
            this.SC_1.Panel1.Controls.Add(this.groupBox22);
            this.SC_1.Panel1.Controls.Add(this.groupBox21);
            this.SC_1.Panel1.Controls.Add(this.groupBox20);
            this.SC_1.Panel1.Controls.Add(this.groupBox19);
            this.SC_1.Panel1.Controls.Add(this.groupBox18);
            this.SC_1.Panel1.Controls.Add(this.groupBox17);
            this.SC_1.Panel1.Controls.Add(this.groupBox16);
            // 
            // SC_1.Panel2
            // 
            this.SC_1.Panel2.Controls.Add(this.splitButton_save);
            this.SC_1.Panel2.Controls.Add(this.splitButton1);
            this.SC_1.Panel2.Controls.Add(this.BTN_cancel);
            this.SC_1.Panel2MinSize = 33;
            this.SC_1.Size = new System.Drawing.Size(1174, 820);
            this.SC_1.SplitterDistance = 785;
            this.SC_1.SplitterWidth = 2;
            this.SC_1.TabIndex = 76;
            // 
            // groupBox24
            // 
            this.groupBox24.Controls.Add(this.textBox_fuelEquipPlan);
            this.groupBox24.Controls.Add(this.label2);
            this.groupBox24.Controls.Add(this.textBox_fuel1hPlan);
            this.groupBox24.Controls.Add(this.label1);
            this.groupBox24.Controls.Add(this.textBox_fuelConsFact);
            this.groupBox24.Controls.Add(this.label103);
            this.groupBox24.Controls.Add(this.textBox_fuel100kmFact);
            this.groupBox24.Controls.Add(this.textBox_fuel100kmPlan);
            this.groupBox24.Controls.Add(this.label101);
            this.groupBox24.Controls.Add(this.label102);
            this.groupBox24.Controls.Add(this.textBox_fuelConsNorm);
            this.groupBox24.Controls.Add(this.label98);
            this.groupBox24.Controls.Add(this.textBox_fuelEnd);
            this.groupBox24.Controls.Add(this.textBox_fuelBegin);
            this.groupBox24.Controls.Add(this.label99);
            this.groupBox24.Controls.Add(this.label100);
            this.groupBox24.Controls.Add(this.textBox_kmDiff);
            this.groupBox24.Controls.Add(this.label97);
            this.groupBox24.Controls.Add(this.textBox_kmEnd);
            this.groupBox24.Controls.Add(this.textBox_kmBegin);
            this.groupBox24.Controls.Add(this.label95);
            this.groupBox24.Controls.Add(this.label96);
            this.groupBox24.Location = new System.Drawing.Point(3, 336);
            this.groupBox24.Name = "groupBox24";
            this.groupBox24.Size = new System.Drawing.Size(1156, 117);
            this.groupBox24.TabIndex = 96;
            this.groupBox24.TabStop = false;
            this.groupBox24.Text = "Показание спидометра/топливо";
            // 
            // textBox_fuelEquipPlan
            // 
            this.textBox_fuelEquipPlan.Location = new System.Drawing.Point(767, 88);
            this.textBox_fuelEquipPlan.Name = "textBox_fuelEquipPlan";
            this.textBox_fuelEquipPlan.ReadOnly = true;
            this.textBox_fuelEquipPlan.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelEquipPlan.TabIndex = 51;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(548, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 15);
            this.label2.TabIndex = 50;
            this.label2.Text = "Работа с установкой (л/ч)";
            // 
            // textBox_fuel1hPlan
            // 
            this.textBox_fuel1hPlan.Location = new System.Drawing.Point(767, 65);
            this.textBox_fuel1hPlan.Name = "textBox_fuel1hPlan";
            this.textBox_fuel1hPlan.ReadOnly = true;
            this.textBox_fuel1hPlan.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuel1hPlan.TabIndex = 49;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(548, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(218, 15);
            this.label1.TabIndex = 48;
            this.label1.Text = "Норма расхода топлива (л/ч)";
            // 
            // textBox_fuelConsFact
            // 
            this.textBox_fuelConsFact.Location = new System.Drawing.Point(433, 88);
            this.textBox_fuelConsFact.Name = "textBox_fuelConsFact";
            this.textBox_fuelConsFact.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelConsFact.TabIndex = 47;
            // 
            // label103
            // 
            this.label103.Location = new System.Drawing.Point(291, 91);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(120, 15);
            this.label103.TabIndex = 46;
            this.label103.Text = "Расход, факт";
            // 
            // textBox_fuel100kmFact
            // 
            this.textBox_fuel100kmFact.Location = new System.Drawing.Point(767, 42);
            this.textBox_fuel100kmFact.Name = "textBox_fuel100kmFact";
            this.textBox_fuel100kmFact.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuel100kmFact.TabIndex = 45;
            // 
            // textBox_fuel100kmPlan
            // 
            this.textBox_fuel100kmPlan.Location = new System.Drawing.Point(767, 19);
            this.textBox_fuel100kmPlan.Name = "textBox_fuel100kmPlan";
            this.textBox_fuel100kmPlan.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuel100kmPlan.TabIndex = 43;
            // 
            // label101
            // 
            this.label101.Location = new System.Drawing.Point(548, 45);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(218, 15);
            this.label101.TabIndex = 44;
            this.label101.Text = "Норма расхода топлива (л/100км), факт";
            // 
            // label102
            // 
            this.label102.Location = new System.Drawing.Point(548, 22);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(218, 15);
            this.label102.TabIndex = 42;
            this.label102.Text = "Норма расхода топлива (л/100км), план";
            // 
            // textBox_fuelConsNorm
            // 
            this.textBox_fuelConsNorm.Location = new System.Drawing.Point(433, 65);
            this.textBox_fuelConsNorm.Name = "textBox_fuelConsNorm";
            this.textBox_fuelConsNorm.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelConsNorm.TabIndex = 41;
            // 
            // label98
            // 
            this.label98.Location = new System.Drawing.Point(291, 68);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(120, 15);
            this.label98.TabIndex = 40;
            this.label98.Text = "Расход по норме";
            // 
            // textBox_fuelEnd
            // 
            this.textBox_fuelEnd.Location = new System.Drawing.Point(433, 42);
            this.textBox_fuelEnd.Name = "textBox_fuelEnd";
            this.textBox_fuelEnd.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelEnd.TabIndex = 39;
            // 
            // textBox_fuelBegin
            // 
            this.textBox_fuelBegin.Location = new System.Drawing.Point(433, 19);
            this.textBox_fuelBegin.Name = "textBox_fuelBegin";
            this.textBox_fuelBegin.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelBegin.TabIndex = 37;
            // 
            // label99
            // 
            this.label99.Location = new System.Drawing.Point(291, 45);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(113, 15);
            this.label99.TabIndex = 38;
            this.label99.Text = "Топливо, конец";
            // 
            // label100
            // 
            this.label100.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label100.Location = new System.Drawing.Point(291, 22);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(141, 15);
            this.label100.TabIndex = 36;
            this.label100.Text = "Топливо, начало*";
            // 
            // textBox_kmDiff
            // 
            this.textBox_kmDiff.Location = new System.Drawing.Point(171, 65);
            this.textBox_kmDiff.Name = "textBox_kmDiff";
            this.textBox_kmDiff.ReadOnly = true;
            this.textBox_kmDiff.Size = new System.Drawing.Size(103, 20);
            this.textBox_kmDiff.TabIndex = 35;
            // 
            // label97
            // 
            this.label97.Location = new System.Drawing.Point(6, 68);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(131, 15);
            this.label97.TabIndex = 34;
            this.label97.Text = "Пройдено";
            // 
            // textBox_kmEnd
            // 
            this.textBox_kmEnd.Location = new System.Drawing.Point(171, 42);
            this.textBox_kmEnd.Name = "textBox_kmEnd";
            this.textBox_kmEnd.Size = new System.Drawing.Size(103, 20);
            this.textBox_kmEnd.TabIndex = 33;
            // 
            // textBox_kmBegin
            // 
            this.textBox_kmBegin.Location = new System.Drawing.Point(171, 19);
            this.textBox_kmBegin.Name = "textBox_kmBegin";
            this.textBox_kmBegin.Size = new System.Drawing.Size(103, 20);
            this.textBox_kmBegin.TabIndex = 31;
            // 
            // label95
            // 
            this.label95.Location = new System.Drawing.Point(6, 45);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(161, 15);
            this.label95.TabIndex = 32;
            this.label95.Text = "Километраж, конец";
            // 
            // label96
            // 
            this.label96.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label96.Location = new System.Drawing.Point(6, 22);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(161, 15);
            this.label96.TabIndex = 30;
            this.label96.Text = "Километраж, начало*";
            // 
            // groupBox23
            // 
            this.groupBox23.Controls.Add(this.textBox_fuelGaugeCons);
            this.groupBox23.Controls.Add(this.textBox_kmGaugeDist);
            this.groupBox23.Controls.Add(this.label106);
            this.groupBox23.Controls.Add(this.label107);
            this.groupBox23.Controls.Add(this.textBox_fuelGaugeEnd);
            this.groupBox23.Controls.Add(this.textBox_kmGaugeEnd);
            this.groupBox23.Controls.Add(this.label104);
            this.groupBox23.Controls.Add(this.label105);
            this.groupBox23.Location = new System.Drawing.Point(3, 704);
            this.groupBox23.Name = "groupBox23";
            this.groupBox23.Size = new System.Drawing.Size(1156, 74);
            this.groupBox23.TabIndex = 95;
            this.groupBox23.TabStop = false;
            this.groupBox23.Text = "Показание спидометра/топливо, датчики";
            // 
            // textBox_fuelGaugeCons
            // 
            this.textBox_fuelGaugeCons.Location = new System.Drawing.Point(624, 43);
            this.textBox_fuelGaugeCons.Name = "textBox_fuelGaugeCons";
            this.textBox_fuelGaugeCons.ReadOnly = true;
            this.textBox_fuelGaugeCons.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelGaugeCons.TabIndex = 35;
            // 
            // textBox_kmGaugeDist
            // 
            this.textBox_kmGaugeDist.Location = new System.Drawing.Point(624, 17);
            this.textBox_kmGaugeDist.Name = "textBox_kmGaugeDist";
            this.textBox_kmGaugeDist.ReadOnly = true;
            this.textBox_kmGaugeDist.Size = new System.Drawing.Size(94, 20);
            this.textBox_kmGaugeDist.TabIndex = 33;
            // 
            // label106
            // 
            this.label106.Location = new System.Drawing.Point(455, 45);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(183, 15);
            this.label106.TabIndex = 34;
            this.label106.Text = "Расход топлива по датчикам, л";
            // 
            // label107
            // 
            this.label107.Location = new System.Drawing.Point(455, 19);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(154, 15);
            this.label107.TabIndex = 32;
            this.label107.Text = "Пройдено по датчиками, км";
            // 
            // textBox_fuelGaugeEnd
            // 
            this.textBox_fuelGaugeEnd.Location = new System.Drawing.Point(195, 45);
            this.textBox_fuelGaugeEnd.Name = "textBox_fuelGaugeEnd";
            this.textBox_fuelGaugeEnd.ReadOnly = true;
            this.textBox_fuelGaugeEnd.Size = new System.Drawing.Size(94, 20);
            this.textBox_fuelGaugeEnd.TabIndex = 31;
            // 
            // textBox_kmGaugeEnd
            // 
            this.textBox_kmGaugeEnd.Location = new System.Drawing.Point(195, 19);
            this.textBox_kmGaugeEnd.Name = "textBox_kmGaugeEnd";
            this.textBox_kmGaugeEnd.ReadOnly = true;
            this.textBox_kmGaugeEnd.Size = new System.Drawing.Size(94, 20);
            this.textBox_kmGaugeEnd.TabIndex = 29;
            // 
            // label104
            // 
            this.label104.Location = new System.Drawing.Point(6, 48);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(183, 15);
            this.label104.TabIndex = 30;
            this.label104.Text = "Топливо датч, конец";
            // 
            // label105
            // 
            this.label105.Location = new System.Drawing.Point(6, 22);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(183, 15);
            this.label105.TabIndex = 28;
            this.label105.Text = "Километраж датч, конец";
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.button_dgDel);
            this.groupBox22.Controls.Add(this.dataGridView_trip);
            this.groupBox22.Controls.Add(this.button_dgAdd);
            this.groupBox22.Location = new System.Drawing.Point(2, 459);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(1157, 239);
            this.groupBox22.TabIndex = 95;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "Поездки";
            // 
            // button_dgDel
            // 
            this.button_dgDel.Enabled = false;
            this.button_dgDel.Location = new System.Drawing.Point(87, 209);
            this.button_dgDel.Name = "button_dgDel";
            this.button_dgDel.Size = new System.Drawing.Size(75, 23);
            this.button_dgDel.TabIndex = 94;
            this.button_dgDel.Text = "Удалить";
            this.button_dgDel.UseVisualStyleBackColor = true;
            // 
            // dataGridView_trip
            // 
            this.dataGridView_trip.AllowUserToAddRows = false;
            this.dataGridView_trip.AllowUserToDeleteRows = false;
            this.dataGridView_trip.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_trip.ColumnHeadersHeight = 50;
            this.dataGridView_trip.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView_trip.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dg_trip_gid,
            this.dg_date_out,
            this.dg_date_return,
            this.dg_odo_begin,
            this.dg_odo_return,
            this.dg_fuel_got,
            this.dg_equip_motohours,
            this.dg_fuel_cons_equip,
            this.dg_motohours_stop,
            this.dg_fuel_cons_stop,
            this.dg_fuel_out,
            this.dg_fuel_return,
            this.dg_route_len,
            this.dg_fuel_cons_norm,
            this.dg_fuel_cons_fact,
            this.dg_fuel_diff,
            this.dg_car_norm,
            this.dg_car_equip_norm,
            this.dg_100km_plan,
            this.dg_100km_fact,
            this.dg_car_regime,
            this.dg_car_regime_hours,
            this.dg_car_fuel_tank});
            this.dataGridView_trip.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView_trip.Location = new System.Drawing.Point(6, 15);
            this.dataGridView_trip.MultiSelect = false;
            this.dataGridView_trip.Name = "dataGridView_trip";
            this.dataGridView_trip.ReadOnly = true;
            this.dataGridView_trip.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView_trip.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_trip.Size = new System.Drawing.Size(1145, 188);
            this.dataGridView_trip.TabIndex = 93;
            this.dataGridView_trip.TabStop = false;
            // 
            // dg_trip_gid
            // 
            this.dg_trip_gid.HeaderText = "dg_trip_gid";
            this.dg_trip_gid.Name = "dg_trip_gid";
            this.dg_trip_gid.ReadOnly = true;
            this.dg_trip_gid.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_trip_gid.Visible = false;
            this.dg_trip_gid.Width = 65;
            // 
            // dg_date_out
            // 
            this.dg_date_out.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.Format = "f";
            dataGridViewCellStyle1.NullValue = null;
            this.dg_date_out.DefaultCellStyle = dataGridViewCellStyle1;
            this.dg_date_out.HeaderText = "Дата выезда";
            this.dg_date_out.Name = "dg_date_out";
            this.dg_date_out.ReadOnly = true;
            this.dg_date_out.Width = 160;
            // 
            // dg_date_return
            // 
            this.dg_date_return.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Format = "f";
            this.dg_date_return.DefaultCellStyle = dataGridViewCellStyle2;
            this.dg_date_return.HeaderText = "Дата возвращения";
            this.dg_date_return.Name = "dg_date_return";
            this.dg_date_return.ReadOnly = true;
            this.dg_date_return.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dg_date_return.Width = 160;
            // 
            // dg_odo_begin
            // 
            this.dg_odo_begin.HeaderText = "Спидометр, начало";
            this.dg_odo_begin.Name = "dg_odo_begin";
            this.dg_odo_begin.ReadOnly = true;
            this.dg_odo_begin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_odo_begin.Width = 99;
            // 
            // dg_odo_return
            // 
            this.dg_odo_return.HeaderText = "Спидометр, возвращени";
            this.dg_odo_return.Name = "dg_odo_return";
            this.dg_odo_return.ReadOnly = true;
            this.dg_odo_return.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_odo_return.Width = 124;
            // 
            // dg_fuel_got
            // 
            this.dg_fuel_got.HeaderText = "Движение топлива, получено";
            this.dg_fuel_got.Name = "dg_fuel_got";
            this.dg_fuel_got.ReadOnly = true;
            this.dg_fuel_got.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_got.Width = 104;
            // 
            // dg_equip_motohours
            // 
            this.dg_equip_motohours.HeaderText = "Работа установки";
            this.dg_equip_motohours.Name = "dg_equip_motohours";
            this.dg_equip_motohours.ReadOnly = true;
            this.dg_equip_motohours.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_equip_motohours.Width = 94;
            // 
            // dg_fuel_cons_equip
            // 
            dataGridViewCellStyle3.Format = "N3";
            this.dg_fuel_cons_equip.DefaultCellStyle = dataGridViewCellStyle3;
            this.dg_fuel_cons_equip.HeaderText = "Расход топлива установки";
            this.dg_fuel_cons_equip.Name = "dg_fuel_cons_equip";
            this.dg_fuel_cons_equip.ReadOnly = true;
            this.dg_fuel_cons_equip.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_cons_equip.Width = 86;
            // 
            // dg_motohours_stop
            // 
            this.dg_motohours_stop.HeaderText = "Работа двигателя при стоянке";
            this.dg_motohours_stop.Name = "dg_motohours_stop";
            this.dg_motohours_stop.ReadOnly = true;
            this.dg_motohours_stop.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_motohours_stop.Width = 80;
            // 
            // dg_fuel_cons_stop
            // 
            dataGridViewCellStyle4.Format = "N3";
            this.dg_fuel_cons_stop.DefaultCellStyle = dataGridViewCellStyle4;
            this.dg_fuel_cons_stop.HeaderText = "Расход топлива, стоянка";
            this.dg_fuel_cons_stop.Name = "dg_fuel_cons_stop";
            this.dg_fuel_cons_stop.ReadOnly = true;
            this.dg_fuel_cons_stop.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_cons_stop.Width = 89;
            // 
            // dg_fuel_out
            // 
            dataGridViewCellStyle5.Format = "N3";
            dataGridViewCellStyle5.NullValue = null;
            this.dg_fuel_out.DefaultCellStyle = dataGridViewCellStyle5;
            this.dg_fuel_out.HeaderText = "Топливо при выезде";
            this.dg_fuel_out.Name = "dg_fuel_out";
            this.dg_fuel_out.ReadOnly = true;
            this.dg_fuel_out.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_out.Width = 65;
            // 
            // dg_fuel_return
            // 
            dataGridViewCellStyle6.Format = "N3";
            this.dg_fuel_return.DefaultCellStyle = dataGridViewCellStyle6;
            this.dg_fuel_return.HeaderText = "Топливо при возвращении";
            this.dg_fuel_return.Name = "dg_fuel_return";
            this.dg_fuel_return.ReadOnly = true;
            this.dg_fuel_return.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_return.Width = 134;
            // 
            // dg_route_len
            // 
            this.dg_route_len.HeaderText = "Пробег";
            this.dg_route_len.Name = "dg_route_len";
            this.dg_route_len.ReadOnly = true;
            this.dg_route_len.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_route_len.Width = 50;
            // 
            // dg_fuel_cons_norm
            // 
            dataGridViewCellStyle7.Format = "N3";
            this.dg_fuel_cons_norm.DefaultCellStyle = dataGridViewCellStyle7;
            this.dg_fuel_cons_norm.HeaderText = "Расход топлива, норма";
            this.dg_fuel_cons_norm.Name = "dg_fuel_cons_norm";
            this.dg_fuel_cons_norm.ReadOnly = true;
            this.dg_fuel_cons_norm.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_cons_norm.Width = 89;
            // 
            // dg_fuel_cons_fact
            // 
            dataGridViewCellStyle8.Format = "N3";
            this.dg_fuel_cons_fact.DefaultCellStyle = dataGridViewCellStyle8;
            this.dg_fuel_cons_fact.HeaderText = "Расход топлива, факт";
            this.dg_fuel_cons_fact.Name = "dg_fuel_cons_fact";
            this.dg_fuel_cons_fact.ReadOnly = true;
            this.dg_fuel_cons_fact.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_cons_fact.Width = 77;
            // 
            // dg_fuel_diff
            // 
            dataGridViewCellStyle9.Format = "N3";
            this.dg_fuel_diff.DefaultCellStyle = dataGridViewCellStyle9;
            this.dg_fuel_diff.HeaderText = "Отклонение";
            this.dg_fuel_diff.Name = "dg_fuel_diff";
            this.dg_fuel_diff.ReadOnly = true;
            this.dg_fuel_diff.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dg_fuel_diff.Width = 74;
            // 
            // dg_car_norm
            // 
            this.dg_car_norm.HeaderText = "dg_car_norm";
            this.dg_car_norm.Name = "dg_car_norm";
            this.dg_car_norm.ReadOnly = true;
            this.dg_car_norm.Visible = false;
            this.dg_car_norm.Width = 94;
            // 
            // dg_car_equip_norm
            // 
            this.dg_car_equip_norm.HeaderText = "dg_car_equip_norm";
            this.dg_car_equip_norm.Name = "dg_car_equip_norm";
            this.dg_car_equip_norm.ReadOnly = true;
            this.dg_car_equip_norm.Visible = false;
            this.dg_car_equip_norm.Width = 126;
            // 
            // dg_100km_plan
            // 
            this.dg_100km_plan.HeaderText = "dg_100km_plan";
            this.dg_100km_plan.Name = "dg_100km_plan";
            this.dg_100km_plan.ReadOnly = true;
            this.dg_100km_plan.Visible = false;
            this.dg_100km_plan.Width = 108;
            // 
            // dg_100km_fact
            // 
            this.dg_100km_fact.HeaderText = "dg_100km_fact";
            this.dg_100km_fact.Name = "dg_100km_fact";
            this.dg_100km_fact.ReadOnly = true;
            this.dg_100km_fact.Visible = false;
            this.dg_100km_fact.Width = 106;
            // 
            // dg_car_regime
            // 
            this.dg_car_regime.HeaderText = "dg_car_regime";
            this.dg_car_regime.Name = "dg_car_regime";
            this.dg_car_regime.ReadOnly = true;
            this.dg_car_regime.Visible = false;
            this.dg_car_regime.Width = 102;
            // 
            // dg_car_regime_hours
            // 
            this.dg_car_regime_hours.HeaderText = "dg_car_regime_hours";
            this.dg_car_regime_hours.Name = "dg_car_regime_hours";
            this.dg_car_regime_hours.ReadOnly = true;
            this.dg_car_regime_hours.Visible = false;
            this.dg_car_regime_hours.Width = 134;
            // 
            // dg_car_fuel_tank
            // 
            this.dg_car_fuel_tank.HeaderText = "dg_car_fuel_tank";
            this.dg_car_fuel_tank.Name = "dg_car_fuel_tank";
            this.dg_car_fuel_tank.ReadOnly = true;
            this.dg_car_fuel_tank.Visible = false;
            this.dg_car_fuel_tank.Width = 115;
            // 
            // button_dgAdd
            // 
            this.button_dgAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_dgAdd.Enabled = false;
            this.button_dgAdd.Location = new System.Drawing.Point(6, 209);
            this.button_dgAdd.Name = "button_dgAdd";
            this.button_dgAdd.Size = new System.Drawing.Size(75, 23);
            this.button_dgAdd.TabIndex = 93;
            this.button_dgAdd.Text = "Добавить";
            this.button_dgAdd.UseVisualStyleBackColor = true;
            // 
            // groupBox21
            // 
            this.groupBox21.Controls.Add(this.dateTimePicker_timeReturnFact);
            this.groupBox21.Controls.Add(this.dateTimePicker_dateReturnFact);
            this.groupBox21.Controls.Add(this.dateTimePicker_timeOutFact);
            this.groupBox21.Controls.Add(this.dateTimePicker_dateOutFact);
            this.groupBox21.Controls.Add(this.label92);
            this.groupBox21.Controls.Add(this.label94);
            this.groupBox21.Controls.Add(this.label91);
            this.groupBox21.Controls.Add(this.label93);
            this.groupBox21.Location = new System.Drawing.Point(587, 255);
            this.groupBox21.Name = "groupBox21";
            this.groupBox21.Size = new System.Drawing.Size(572, 75);
            this.groupBox21.TabIndex = 95;
            this.groupBox21.TabStop = false;
            this.groupBox21.Text = "Дата/время выезда, факт";
            // 
            // dateTimePicker_timeReturnFact
            // 
            this.dateTimePicker_timeReturnFact.CustomFormat = " ";
            this.dateTimePicker_timeReturnFact.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_timeReturnFact.ImeMode = System.Windows.Forms.ImeMode.On;
            this.dateTimePicker_timeReturnFact.Location = new System.Drawing.Point(494, 49);
            this.dateTimePicker_timeReturnFact.Name = "dateTimePicker_timeReturnFact";
            this.dateTimePicker_timeReturnFact.ShowUpDown = true;
            this.dateTimePicker_timeReturnFact.Size = new System.Drawing.Size(72, 20);
            this.dateTimePicker_timeReturnFact.TabIndex = 33;
            this.dateTimePicker_timeReturnFact.ValueChanged += new System.EventHandler(this.dateTimePicker_timeReturnFact_ValueChanged);
            // 
            // dateTimePicker_dateReturnFact
            // 
            this.dateTimePicker_dateReturnFact.CustomFormat = " ";
            this.dateTimePicker_dateReturnFact.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_dateReturnFact.Location = new System.Drawing.Point(160, 49);
            this.dateTimePicker_dateReturnFact.Name = "dateTimePicker_dateReturnFact";
            this.dateTimePicker_dateReturnFact.Size = new System.Drawing.Size(162, 20);
            this.dateTimePicker_dateReturnFact.TabIndex = 33;
            this.dateTimePicker_dateReturnFact.ValueChanged += new System.EventHandler(this.dateTimePicker_dateReturnFact_ValueChanged);
            // 
            // dateTimePicker_timeOutFact
            // 
            this.dateTimePicker_timeOutFact.CustomFormat = " ";
            this.dateTimePicker_timeOutFact.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_timeOutFact.Location = new System.Drawing.Point(494, 23);
            this.dateTimePicker_timeOutFact.Name = "dateTimePicker_timeOutFact";
            this.dateTimePicker_timeOutFact.ShowUpDown = true;
            this.dateTimePicker_timeOutFact.Size = new System.Drawing.Size(72, 20);
            this.dateTimePicker_timeOutFact.TabIndex = 32;
            this.dateTimePicker_timeOutFact.ValueChanged += new System.EventHandler(this.dateTimePicker_timeOutFact_ValueChanged);
            // 
            // dateTimePicker_dateOutFact
            // 
            this.dateTimePicker_dateOutFact.CustomFormat = " ";
            this.dateTimePicker_dateOutFact.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_dateOutFact.Location = new System.Drawing.Point(161, 23);
            this.dateTimePicker_dateOutFact.Name = "dateTimePicker_dateOutFact";
            this.dateTimePicker_dateOutFact.Size = new System.Drawing.Size(161, 20);
            this.dateTimePicker_dateOutFact.TabIndex = 32;
            this.dateTimePicker_dateOutFact.ValueChanged += new System.EventHandler(this.dateTimePicker_dateOutFact_ValueChanged);
            // 
            // label92
            // 
            this.label92.Location = new System.Drawing.Point(356, 26);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(132, 15);
            this.label92.TabIndex = 32;
            this.label92.Text = "Время выезда";
            // 
            // label94
            // 
            this.label94.Location = new System.Drawing.Point(6, 26);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(136, 15);
            this.label94.TabIndex = 28;
            this.label94.Text = "Дата выезда";
            // 
            // label91
            // 
            this.label91.Location = new System.Drawing.Point(356, 52);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(147, 15);
            this.label91.TabIndex = 34;
            this.label91.Text = "Время возвращения";
            // 
            // label93
            // 
            this.label93.Location = new System.Drawing.Point(6, 52);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(151, 15);
            this.label93.TabIndex = 30;
            this.label93.Text = "Дата возвращения";
            // 
            // groupBox20
            // 
            this.groupBox20.Controls.Add(this.dateTimePicker_timeReturnPlan);
            this.groupBox20.Controls.Add(this.dateTimePicker_timeOutPlan);
            this.groupBox20.Controls.Add(this.dateTimePicker_dateReturnPlan);
            this.groupBox20.Controls.Add(this.dateTimePicker_dateOutPlan);
            this.groupBox20.Controls.Add(this.label89);
            this.groupBox20.Controls.Add(this.label90);
            this.groupBox20.Controls.Add(this.label87);
            this.groupBox20.Controls.Add(this.label88);
            this.groupBox20.Location = new System.Drawing.Point(2, 255);
            this.groupBox20.Name = "groupBox20";
            this.groupBox20.Size = new System.Drawing.Size(579, 75);
            this.groupBox20.TabIndex = 94;
            this.groupBox20.TabStop = false;
            this.groupBox20.Text = "Дата/время выезда";
            // 
            // dateTimePicker_timeReturnPlan
            // 
            this.dateTimePicker_timeReturnPlan.CustomFormat = "HH:mm";
            this.dateTimePicker_timeReturnPlan.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_timeReturnPlan.ImeMode = System.Windows.Forms.ImeMode.On;
            this.dateTimePicker_timeReturnPlan.Location = new System.Drawing.Point(493, 49);
            this.dateTimePicker_timeReturnPlan.Name = "dateTimePicker_timeReturnPlan";
            this.dateTimePicker_timeReturnPlan.ShowUpDown = true;
            this.dateTimePicker_timeReturnPlan.Size = new System.Drawing.Size(80, 20);
            this.dateTimePicker_timeReturnPlan.TabIndex = 31;
            // 
            // dateTimePicker_timeOutPlan
            // 
            this.dateTimePicker_timeOutPlan.CustomFormat = "HH:mm";
            this.dateTimePicker_timeOutPlan.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_timeOutPlan.Location = new System.Drawing.Point(493, 22);
            this.dateTimePicker_timeOutPlan.Name = "dateTimePicker_timeOutPlan";
            this.dateTimePicker_timeOutPlan.ShowUpDown = true;
            this.dateTimePicker_timeOutPlan.Size = new System.Drawing.Size(80, 20);
            this.dateTimePicker_timeOutPlan.TabIndex = 30;
            // 
            // dateTimePicker_dateReturnPlan
            // 
            this.dateTimePicker_dateReturnPlan.Location = new System.Drawing.Point(165, 49);
            this.dateTimePicker_dateReturnPlan.Name = "dateTimePicker_dateReturnPlan";
            this.dateTimePicker_dateReturnPlan.Size = new System.Drawing.Size(166, 20);
            this.dateTimePicker_dateReturnPlan.TabIndex = 29;
            // 
            // dateTimePicker_dateOutPlan
            // 
            this.dateTimePicker_dateOutPlan.Location = new System.Drawing.Point(166, 23);
            this.dateTimePicker_dateOutPlan.Name = "dateTimePicker_dateOutPlan";
            this.dateTimePicker_dateOutPlan.Size = new System.Drawing.Size(165, 20);
            this.dateTimePicker_dateOutPlan.TabIndex = 28;
            // 
            // label89
            // 
            this.label89.Location = new System.Drawing.Point(376, 53);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(116, 15);
            this.label89.TabIndex = 26;
            this.label89.Text = "Время возвращения";
            // 
            // label90
            // 
            this.label90.Location = new System.Drawing.Point(376, 26);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(101, 15);
            this.label90.TabIndex = 24;
            this.label90.Text = "Время выезда";
            // 
            // label87
            // 
            this.label87.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label87.Location = new System.Drawing.Point(6, 53);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(162, 15);
            this.label87.TabIndex = 22;
            this.label87.Text = "Дата возвращения*";
            // 
            // label88
            // 
            this.label88.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label88.Location = new System.Drawing.Point(6, 26);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(154, 15);
            this.label88.TabIndex = 20;
            this.label88.Text = "Дата выезда*";
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.comboBox_car);
            this.groupBox19.Controls.Add(this.textBox_regNum);
            this.groupBox19.Controls.Add(this.label86);
            this.groupBox19.Controls.Add(this.textBox_garNum);
            this.groupBox19.Controls.Add(this.label84);
            this.groupBox19.Controls.Add(this.textBox_gosNum);
            this.groupBox19.Controls.Add(this.label85);
            this.groupBox19.Controls.Add(this.textBox_carType);
            this.groupBox19.Controls.Add(this.label83);
            this.groupBox19.Controls.Add(this.label82);
            this.groupBox19.Location = new System.Drawing.Point(587, 155);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(572, 94);
            this.groupBox19.TabIndex = 94;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "Транспортное средство";
            // 
            // comboBox_car
            // 
            this.comboBox_car.DropDownHeight = 156;
            this.comboBox_car.DropDownWidth = 307;
            this.comboBox_car.FormattingEnabled = true;
            this.comboBox_car.IntegralHeight = false;
            this.comboBox_car.Location = new System.Drawing.Point(69, 18);
            this.comboBox_car.Name = "comboBox_car";
            this.comboBox_car.Size = new System.Drawing.Size(286, 21);
            this.comboBox_car.TabIndex = 30;
            // 
            // textBox_regNum
            // 
            this.textBox_regNum.Location = new System.Drawing.Point(421, 65);
            this.textBox_regNum.Name = "textBox_regNum";
            this.textBox_regNum.ReadOnly = true;
            this.textBox_regNum.Size = new System.Drawing.Size(145, 20);
            this.textBox_regNum.TabIndex = 29;
            // 
            // label86
            // 
            this.label86.Location = new System.Drawing.Point(366, 68);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(49, 15);
            this.label86.TabIndex = 28;
            this.label86.Text = "Рег. №";
            // 
            // textBox_garNum
            // 
            this.textBox_garNum.Location = new System.Drawing.Point(421, 42);
            this.textBox_garNum.Name = "textBox_garNum";
            this.textBox_garNum.ReadOnly = true;
            this.textBox_garNum.Size = new System.Drawing.Size(145, 20);
            this.textBox_garNum.TabIndex = 27;
            // 
            // label84
            // 
            this.label84.Location = new System.Drawing.Point(366, 45);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(49, 15);
            this.label84.TabIndex = 26;
            this.label84.Text = "Гар. №";
            // 
            // textBox_gosNum
            // 
            this.textBox_gosNum.Location = new System.Drawing.Point(421, 19);
            this.textBox_gosNum.Name = "textBox_gosNum";
            this.textBox_gosNum.ReadOnly = true;
            this.textBox_gosNum.Size = new System.Drawing.Size(145, 20);
            this.textBox_gosNum.TabIndex = 25;
            // 
            // label85
            // 
            this.label85.Location = new System.Drawing.Point(366, 22);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(49, 15);
            this.label85.TabIndex = 24;
            this.label85.Text = "Гос. №";
            // 
            // textBox_carType
            // 
            this.textBox_carType.Location = new System.Drawing.Point(69, 42);
            this.textBox_carType.Name = "textBox_carType";
            this.textBox_carType.ReadOnly = true;
            this.textBox_carType.Size = new System.Drawing.Size(286, 20);
            this.textBox_carType.TabIndex = 23;
            // 
            // label83
            // 
            this.label83.Location = new System.Drawing.Point(6, 47);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(57, 15);
            this.label83.TabIndex = 22;
            this.label83.Text = "Тип ТС";
            // 
            // label82
            // 
            this.label82.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label82.Location = new System.Drawing.Point(6, 24);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(57, 15);
            this.label82.TabIndex = 20;
            this.label82.Text = "Марка*";
            // 
            // groupBox18
            // 
            this.groupBox18.Controls.Add(this.comboBox_driver2);
            this.groupBox18.Controls.Add(this.comboBox_driver1);
            this.groupBox18.Controls.Add(this.textBox_udos2);
            this.groupBox18.Controls.Add(this.label81);
            this.groupBox18.Controls.Add(this.textBox_udos1);
            this.groupBox18.Controls.Add(this.label80);
            this.groupBox18.Controls.Add(this.label79);
            this.groupBox18.Controls.Add(this.label78);
            this.groupBox18.Location = new System.Drawing.Point(2, 155);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Size = new System.Drawing.Size(579, 94);
            this.groupBox18.TabIndex = 93;
            this.groupBox18.TabStop = false;
            this.groupBox18.Text = "Водители";
            // 
            // comboBox_driver2
            // 
            this.comboBox_driver2.FormattingEnabled = true;
            this.comboBox_driver2.Location = new System.Drawing.Point(59, 47);
            this.comboBox_driver2.Name = "comboBox_driver2";
            this.comboBox_driver2.Size = new System.Drawing.Size(252, 21);
            this.comboBox_driver2.TabIndex = 21;
            // 
            // comboBox_driver1
            // 
            this.comboBox_driver1.FormattingEnabled = true;
            this.comboBox_driver1.Location = new System.Drawing.Point(59, 21);
            this.comboBox_driver1.Name = "comboBox_driver1";
            this.comboBox_driver1.Size = new System.Drawing.Size(252, 21);
            this.comboBox_driver1.TabIndex = 20;
            // 
            // textBox_udos2
            // 
            this.textBox_udos2.Location = new System.Drawing.Point(427, 47);
            this.textBox_udos2.Name = "textBox_udos2";
            this.textBox_udos2.Size = new System.Drawing.Size(146, 20);
            this.textBox_udos2.TabIndex = 19;
            // 
            // label81
            // 
            this.label81.Location = new System.Drawing.Point(317, 50);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(100, 15);
            this.label81.TabIndex = 18;
            this.label81.Text = "Удостоверение №";
            // 
            // textBox_udos1
            // 
            this.textBox_udos1.Location = new System.Drawing.Point(427, 21);
            this.textBox_udos1.Name = "textBox_udos1";
            this.textBox_udos1.Size = new System.Drawing.Size(146, 20);
            this.textBox_udos1.TabIndex = 17;
            // 
            // label80
            // 
            this.label80.Location = new System.Drawing.Point(317, 24);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(104, 15);
            this.label80.TabIndex = 16;
            this.label80.Text = "Удостоверение №";
            // 
            // label79
            // 
            this.label79.Location = new System.Drawing.Point(6, 50);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(52, 15);
            this.label79.TabIndex = 14;
            this.label79.Text = "ФИО";
            // 
            // label78
            // 
            this.label78.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label78.Location = new System.Drawing.Point(6, 24);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(52, 15);
            this.label78.TabIndex = 12;
            this.label78.Text = "ФИО*";
            // 
            // groupBox17
            // 
            this.groupBox17.Controls.Add(this.comboBox_service);
            this.groupBox17.Controls.Add(this.comboBox_issue);
            this.groupBox17.Controls.Add(this.textBox_issType);
            this.groupBox17.Controls.Add(this.label77);
            this.groupBox17.Controls.Add(this.textBox_issSource);
            this.groupBox17.Controls.Add(this.label76);
            this.groupBox17.Controls.Add(this.textBox_issDest);
            this.groupBox17.Controls.Add(this.label75);
            this.groupBox17.Controls.Add(this.textBox_rasp);
            this.groupBox17.Controls.Add(this.label74);
            this.groupBox17.Controls.Add(this.textBox_issueNum);
            this.groupBox17.Controls.Add(this.label73);
            this.groupBox17.Location = new System.Drawing.Point(587, 3);
            this.groupBox17.Name = "groupBox17";
            this.groupBox17.Size = new System.Drawing.Size(572, 146);
            this.groupBox17.TabIndex = 93;
            this.groupBox17.TabStop = false;
            this.groupBox17.Text = "Задание на выезд";
            // 
            // comboBox_service
            // 
            this.comboBox_service.FormattingEnabled = true;
            this.comboBox_service.Location = new System.Drawing.Point(132, 47);
            this.comboBox_service.Name = "comboBox_service";
            this.comboBox_service.Size = new System.Drawing.Size(434, 21);
            this.comboBox_service.TabIndex = 23;
            // 
            // comboBox_issue
            // 
            this.comboBox_issue.FormattingEnabled = true;
            this.comboBox_issue.Location = new System.Drawing.Point(132, 23);
            this.comboBox_issue.Name = "comboBox_issue";
            this.comboBox_issue.Size = new System.Drawing.Size(434, 21);
            this.comboBox_issue.TabIndex = 22;
            // 
            // textBox_issType
            // 
            this.textBox_issType.Location = new System.Drawing.Point(132, 119);
            this.textBox_issType.Name = "textBox_issType";
            this.textBox_issType.Size = new System.Drawing.Size(434, 20);
            this.textBox_issType.TabIndex = 11;
            // 
            // label77
            // 
            this.label77.Location = new System.Drawing.Point(6, 122);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(129, 15);
            this.label77.TabIndex = 10;
            this.label77.Text = "Вид работы";
            // 
            // textBox_issSource
            // 
            this.textBox_issSource.Location = new System.Drawing.Point(132, 95);
            this.textBox_issSource.Name = "textBox_issSource";
            this.textBox_issSource.Size = new System.Drawing.Size(434, 20);
            this.textBox_issSource.TabIndex = 9;
            // 
            // label76
            // 
            this.label76.Location = new System.Drawing.Point(6, 98);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(129, 15);
            this.label76.TabIndex = 8;
            this.label76.Text = "Откуда следует";
            // 
            // textBox_issDest
            // 
            this.textBox_issDest.Location = new System.Drawing.Point(132, 71);
            this.textBox_issDest.Name = "textBox_issDest";
            this.textBox_issDest.Size = new System.Drawing.Size(434, 20);
            this.textBox_issDest.TabIndex = 7;
            // 
            // label75
            // 
            this.label75.Location = new System.Drawing.Point(6, 74);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(129, 15);
            this.label75.TabIndex = 6;
            this.label75.Text = "Куда следует";
            // 
            // textBox_rasp
            // 
            this.textBox_rasp.Location = new System.Drawing.Point(132, 47);
            this.textBox_rasp.Name = "textBox_rasp";
            this.textBox_rasp.Size = new System.Drawing.Size(434, 20);
            this.textBox_rasp.TabIndex = 5;
            // 
            // label74
            // 
            this.label74.Location = new System.Drawing.Point(6, 50);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(129, 15);
            this.label74.TabIndex = 4;
            this.label74.Text = "В распоряжение";
            // 
            // textBox_issueNum
            // 
            this.textBox_issueNum.Location = new System.Drawing.Point(132, 23);
            this.textBox_issueNum.Name = "textBox_issueNum";
            this.textBox_issueNum.ReadOnly = true;
            this.textBox_issueNum.Size = new System.Drawing.Size(434, 20);
            this.textBox_issueNum.TabIndex = 3;
            this.textBox_issueNum.Visible = false;
            // 
            // label73
            // 
            this.label73.Location = new System.Drawing.Point(6, 26);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(129, 15);
            this.label73.TabIndex = 2;
            this.label73.Text = "№ заявки";
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.textBox_userDept);
            this.groupBox16.Controls.Add(this.label72);
            this.groupBox16.Controls.Add(this.textBox_plNum);
            this.groupBox16.Controls.Add(this.label71);
            this.groupBox16.Location = new System.Drawing.Point(2, 3);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(579, 146);
            this.groupBox16.TabIndex = 92;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "Путевой лист";
            // 
            // textBox_userDept
            // 
            this.textBox_userDept.Location = new System.Drawing.Point(325, 23);
            this.textBox_userDept.Name = "textBox_userDept";
            this.textBox_userDept.ReadOnly = true;
            this.textBox_userDept.Size = new System.Drawing.Size(81, 20);
            this.textBox_userDept.TabIndex = 3;
            // 
            // label72
            // 
            this.label72.Location = new System.Drawing.Point(312, 26);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(10, 15);
            this.label72.TabIndex = 2;
            this.label72.Text = "/";
            // 
            // textBox_plNum
            // 
            this.textBox_plNum.Location = new System.Drawing.Point(174, 23);
            this.textBox_plNum.Name = "textBox_plNum";
            this.textBox_plNum.Size = new System.Drawing.Size(137, 20);
            this.textBox_plNum.TabIndex = 1;
            // 
            // label71
            // 
            this.label71.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label71.Location = new System.Drawing.Point(6, 26);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(168, 15);
            this.label71.TabIndex = 0;
            this.label71.Text = "Номер путевого листа*";
            // 
            // splitButton_save
            // 
            this.splitButton_save.AutoSize = true;
            this.splitButton_save.Location = new System.Drawing.Point(6, 3);
            this.splitButton_save.Name = "splitButton_save";
            this.splitButton_save.Size = new System.Drawing.Size(75, 25);
            this.splitButton_save.TabIndex = 60;
            this.splitButton_save.Text = "Сохранить";
            this.splitButton_save.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.splitButton_save.UseVisualStyleBackColor = true;
            // 
            // splitButton1
            // 
            this.splitButton1.AutoSize = true;
            this.splitButton1.Location = new System.Drawing.Point(181, 3);
            this.splitButton1.MaximumSize = new System.Drawing.Size(200, 0);
            this.splitButton1.MinimumSize = new System.Drawing.Size(0, 25);
            this.splitButton1.Name = "splitButton1";
            this.splitButton1.Size = new System.Drawing.Size(132, 25);
            this.splitButton1.TabIndex = 59;
            this.splitButton1.Text = "Сохранение и печать";
            this.splitButton1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.splitButton1.UseVisualStyleBackColor = true;
            // 
            // BTN_cancel
            // 
            this.BTN_cancel.Location = new System.Drawing.Point(100, 3);
            this.BTN_cancel.Name = "BTN_cancel";
            this.BTN_cancel.Size = new System.Drawing.Size(75, 25);
            this.BTN_cancel.TabIndex = 54;
            this.BTN_cancel.Text = "Отмена";
            this.BTN_cancel.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "dg_trip_gid";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Visible = false;
            this.dataGridViewTextBoxColumn1.Width = 65;
            // 
            // calendarColumn1
            // 
            dataGridViewCellStyle10.Format = "f";
            dataGridViewCellStyle10.NullValue = null;
            this.calendarColumn1.DefaultCellStyle = dataGridViewCellStyle10;
            this.calendarColumn1.HeaderText = "Дата выезда";
            this.calendarColumn1.Name = "calendarColumn1";
            this.calendarColumn1.Width = 72;
            // 
            // calendarColumn2
            // 
            dataGridViewCellStyle11.Format = "f";
            this.calendarColumn2.DefaultCellStyle = dataGridViewCellStyle11;
            this.calendarColumn2.HeaderText = "Дата возвращения";
            this.calendarColumn2.Name = "calendarColumn2";
            this.calendarColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Спидометр, начало";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.Width = 99;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "Спидометр, возвращени";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 124;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewCellStyle12.NullValue = "0";
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewTextBoxColumn4.HeaderText = "Движение топлива, получено";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn4.Width = 104;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Работа двигателя при стоянке";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn5.Width = 80;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Работа установки";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn6.Width = 94;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewCellStyle13.Format = "N3";
            dataGridViewCellStyle13.NullValue = null;
            this.dataGridViewTextBoxColumn7.DefaultCellStyle = dataGridViewCellStyle13;
            this.dataGridViewTextBoxColumn7.HeaderText = "Топливо при выезде";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn7.Width = 65;
            // 
            // dataGridViewTextBoxColumn8
            // 
            dataGridViewCellStyle14.Format = "N3";
            this.dataGridViewTextBoxColumn8.DefaultCellStyle = dataGridViewCellStyle14;
            this.dataGridViewTextBoxColumn8.HeaderText = "Топливо при возвращении";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn8.Width = 134;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.HeaderText = "Расход топлива, стоянка";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn9.Width = 89;
            // 
            // dataGridViewTextBoxColumn10
            // 
            dataGridViewCellStyle15.Format = "N3";
            this.dataGridViewTextBoxColumn10.DefaultCellStyle = dataGridViewCellStyle15;
            this.dataGridViewTextBoxColumn10.HeaderText = "Расход топлива установки";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn10.Width = 86;
            // 
            // dataGridViewTextBoxColumn11
            // 
            dataGridViewCellStyle16.Format = "N3";
            this.dataGridViewTextBoxColumn11.DefaultCellStyle = dataGridViewCellStyle16;
            this.dataGridViewTextBoxColumn11.HeaderText = "Расход топлива, норма";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn11.Width = 89;
            // 
            // dataGridViewTextBoxColumn12
            // 
            dataGridViewCellStyle17.Format = "N3";
            this.dataGridViewTextBoxColumn12.DefaultCellStyle = dataGridViewCellStyle17;
            this.dataGridViewTextBoxColumn12.HeaderText = "Расход топлива, факт";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            this.dataGridViewTextBoxColumn12.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn12.Width = 77;
            // 
            // dataGridViewTextBoxColumn13
            // 
            dataGridViewCellStyle18.Format = "N3";
            this.dataGridViewTextBoxColumn13.DefaultCellStyle = dataGridViewCellStyle18;
            this.dataGridViewTextBoxColumn13.HeaderText = "Отклонение";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn13.Width = 74;
            // 
            // dataGridViewTextBoxColumn14
            // 
            dataGridViewCellStyle19.Format = "N3";
            this.dataGridViewTextBoxColumn14.DefaultCellStyle = dataGridViewCellStyle19;
            this.dataGridViewTextBoxColumn14.HeaderText = "Пробег";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            this.dataGridViewTextBoxColumn14.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn14.Width = 50;
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.HeaderText = "dg_car_norm";
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            this.dataGridViewTextBoxColumn15.Visible = false;
            this.dataGridViewTextBoxColumn15.Width = 94;
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.HeaderText = "dg_car_equip_norm";
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            this.dataGridViewTextBoxColumn16.Visible = false;
            this.dataGridViewTextBoxColumn16.Width = 126;
            // 
            // dataGridViewTextBoxColumn17
            // 
            this.dataGridViewTextBoxColumn17.HeaderText = "dg_100km_plan";
            this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
            this.dataGridViewTextBoxColumn17.Visible = false;
            this.dataGridViewTextBoxColumn17.Width = 108;
            // 
            // dataGridViewTextBoxColumn18
            // 
            this.dataGridViewTextBoxColumn18.HeaderText = "dg_100km_fact";
            this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
            this.dataGridViewTextBoxColumn18.Visible = false;
            this.dataGridViewTextBoxColumn18.Width = 106;
            // 
            // dataGridViewTextBoxColumn19
            // 
            this.dataGridViewTextBoxColumn19.HeaderText = "dg_car_regime";
            this.dataGridViewTextBoxColumn19.Name = "dataGridViewTextBoxColumn19";
            this.dataGridViewTextBoxColumn19.Visible = false;
            this.dataGridViewTextBoxColumn19.Width = 102;
            // 
            // dataGridViewTextBoxColumn20
            // 
            this.dataGridViewTextBoxColumn20.HeaderText = "dg_car_regime_hours";
            this.dataGridViewTextBoxColumn20.Name = "dataGridViewTextBoxColumn20";
            this.dataGridViewTextBoxColumn20.Visible = false;
            this.dataGridViewTextBoxColumn20.Width = 134;
            // 
            // dataGridViewTextBoxColumn21
            // 
            this.dataGridViewTextBoxColumn21.HeaderText = "dg_car_fuel_tank";
            this.dataGridViewTextBoxColumn21.Name = "dataGridViewTextBoxColumn21";
            this.dataGridViewTextBoxColumn21.Visible = false;
            this.dataGridViewTextBoxColumn21.Width = 115;
            // 
            // UserControlAttr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.SC_1);
            this.MaximumSize = new System.Drawing.Size(1176, 820);
            this.Name = "UserControlAttr";
            this.Size = new System.Drawing.Size(1174, 820);
            this.Load += new System.EventHandler(this.UserControlAttr_Load);
            this.SC_1.Panel1.ResumeLayout(false);
            this.SC_1.Panel2.ResumeLayout(false);
            this.SC_1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SC_1)).EndInit();
            this.SC_1.ResumeLayout(false);
            this.groupBox24.ResumeLayout(false);
            this.groupBox24.PerformLayout();
            this.groupBox23.ResumeLayout(false);
            this.groupBox23.PerformLayout();
            this.groupBox22.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_trip)).EndInit();
            this.groupBox21.ResumeLayout(false);
            this.groupBox20.ResumeLayout(false);
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            this.groupBox18.ResumeLayout(false);
            this.groupBox18.PerformLayout();
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.groupBox16.ResumeLayout(false);
            this.groupBox16.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer SC_1;
        internal System.Windows.Forms.Button BTN_cancel;
        private SplitButton splitButton1;
        private System.Windows.Forms.GroupBox groupBox23;
        private System.Windows.Forms.GroupBox groupBox22;
        private System.Windows.Forms.Button button_dgDel;
        internal System.Windows.Forms.DataGridView dataGridView_trip;
        private System.Windows.Forms.Button button_dgAdd;
        private System.Windows.Forms.GroupBox groupBox21;
        private System.Windows.Forms.GroupBox groupBox20;
        private System.Windows.Forms.GroupBox groupBox19;
        private System.Windows.Forms.GroupBox groupBox18;
        private System.Windows.Forms.GroupBox groupBox17;
        private System.Windows.Forms.GroupBox groupBox16;
        private System.Windows.Forms.Label label71;
        private System.Windows.Forms.Label label72;
        private System.Windows.Forms.TextBox textBox_plNum;
        private System.Windows.Forms.TextBox textBox_issueNum;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.TextBox textBox_issType;
        private System.Windows.Forms.Label label77;
        private System.Windows.Forms.TextBox textBox_issSource;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.TextBox textBox_issDest;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.TextBox textBox_rasp;
        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.TextBox textBox_udos1;
        private System.Windows.Forms.Label label80;
        private System.Windows.Forms.Label label79;
        private System.Windows.Forms.Label label78;
        private System.Windows.Forms.TextBox textBox_garNum;
        private System.Windows.Forms.Label label84;
        private System.Windows.Forms.TextBox textBox_gosNum;
        private System.Windows.Forms.Label label85;
        private System.Windows.Forms.TextBox textBox_carType;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.TextBox textBox_udos2;
        private System.Windows.Forms.Label label81;
        private System.Windows.Forms.Label label87;
        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.TextBox textBox_regNum;
        private System.Windows.Forms.Label label86;
        private System.Windows.Forms.GroupBox groupBox24;
        private System.Windows.Forms.Label label95;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.Label label94;
        private System.Windows.Forms.Label label91;
        private System.Windows.Forms.Label label93;
        private System.Windows.Forms.Label label89;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.Label label101;
        private System.Windows.Forms.Label label102;
        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.Label label99;
        private System.Windows.Forms.Label label100;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.Label label106;
        private System.Windows.Forms.Label label107;
        private System.Windows.Forms.Label label104;
        private System.Windows.Forms.Label label105;
        private System.Windows.Forms.DateTimePicker dateTimePicker_timeReturnFact;
        private System.Windows.Forms.DateTimePicker dateTimePicker_dateReturnFact;
        private System.Windows.Forms.DateTimePicker dateTimePicker_timeOutFact;
        private System.Windows.Forms.DateTimePicker dateTimePicker_dateOutFact;
        private System.Windows.Forms.DateTimePicker dateTimePicker_timeReturnPlan;
        private System.Windows.Forms.DateTimePicker dateTimePicker_timeOutPlan;
        private System.Windows.Forms.DateTimePicker dateTimePicker_dateReturnPlan;
        private System.Windows.Forms.DateTimePicker dateTimePicker_dateOutPlan;
        private System.Windows.Forms.ComboBox comboBox_driver2;
        private System.Windows.Forms.ComboBox comboBox_driver1;
        private System.Windows.Forms.TextBox textBox_userDept;
        private System.Windows.Forms.ComboBox comboBox_car;
        private System.Windows.Forms.ComboBox comboBox_issue;
        private MyComponents.TextBoxNumber textBox_kmEnd;
        private MyComponents.TextBoxNumber textBox_kmBegin;
        private MyComponents.TextBoxNumber textBox_fuelConsFact;
        private MyComponents.TextBoxNumber textBox_fuel100kmFact;
        private MyComponents.TextBoxNumber textBox_fuel100kmPlan;
        private MyComponents.TextBoxNumber textBox_fuelConsNorm;
        private MyComponents.TextBoxNumber textBox_fuelEnd;
        private MyComponents.TextBoxNumber textBox_fuelBegin;
        private MyComponents.TextBoxNumber textBox_kmDiff;
        private MyComponents.TextBoxNumber textBox_fuelGaugeCons;
        private MyComponents.TextBoxNumber textBox_kmGaugeDist;
        private MyComponents.TextBoxNumber textBox_fuelGaugeEnd;
        private MyComponents.TextBoxNumber textBox_kmGaugeEnd;
        private ComboBox comboBox_service;
        private SplitButton splitButton_save;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private CalendarColumn calendarColumn1;
        private CalendarColumn calendarColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn20;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn21;
        private DataGridViewTextBoxColumn dg_trip_gid;
        private CalendarColumn dg_date_out;
        private CalendarColumn dg_date_return;
        private DataGridViewTextBoxColumn dg_odo_begin;
        private DataGridViewTextBoxColumn dg_odo_return;
        private DataGridViewTextBoxColumn dg_fuel_got;
        private DataGridViewTextBoxColumn dg_equip_motohours;
        private DataGridViewTextBoxColumn dg_fuel_cons_equip;
        private DataGridViewTextBoxColumn dg_motohours_stop;
        private DataGridViewTextBoxColumn dg_fuel_cons_stop;
        private DataGridViewTextBoxColumn dg_fuel_out;
        private DataGridViewTextBoxColumn dg_fuel_return;
        private DataGridViewTextBoxColumn dg_route_len;
        private DataGridViewTextBoxColumn dg_fuel_cons_norm;
        private DataGridViewTextBoxColumn dg_fuel_cons_fact;
        private DataGridViewTextBoxColumn dg_fuel_diff;
        private DataGridViewTextBoxColumn dg_car_norm;
        private DataGridViewTextBoxColumn dg_car_equip_norm;
        private DataGridViewTextBoxColumn dg_100km_plan;
        private DataGridViewTextBoxColumn dg_100km_fact;
        private DataGridViewTextBoxColumn dg_car_regime;
        private DataGridViewTextBoxColumn dg_car_regime_hours;
        private DataGridViewTextBoxColumn dg_car_fuel_tank;
        private TextBoxNumber textBox_fuel1hPlan;
        private Label label1;
        private TextBoxNumber textBox_fuelEquipPlan;
        private Label label2;
    }
}
