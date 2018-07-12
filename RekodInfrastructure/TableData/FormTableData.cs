using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Interfaces;
using Npgsql;
using axVisUtils.Styles;
using axVisUtils.TableData;
using System.Linq;
using Rekod;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using tablesInfo = Rekod.tablesInfo;
using Rekod.Properties;
using Rekod.Classes;
using Rekod.FastReportClasses;
using System.Windows.Forms.Integration;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using Rekod.Services;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.View.History;
using Interfaces.FastReport;

namespace axVisUtils
{

    public partial class FormTableData : Form
    {
        public tablesInfo tableInfo;
        public Action<DialogResult> ActionResult;
        private DataConnect data_;
        private Size startSize;
        private Panel wrkPanel;
        private bool haveStyle;
        private bool isNew1;
        private Styles.objStylesM style1;
        private string message1, pkfield; // Это когда значение не входит ни в один интервал
        private referInfo defRefValue;
        private DataColumn defRefCol;
        private int idT, idObj;
        private Panel panel3;
        private Panel panelForProperties;
        private GroupBox grBox;
        private Label countLabel;
        private string photo_path = "";
        private bool _isLayer = false;
        private PgAttributesGeomVM _geometryVM;
        //DealGeometryByHand _geomControl;
        private String _wkt;
        public float x;
        public float y;
        private bool _cancelOpenForm;

        private List<PictureBox> pboxes;
        private List<PictureBox> s_pboxes;
        private int start_select = -1;
        string col__ = "", value__ = "";

        private bool _isReadOnly = false;

        string dateFormat = string.Empty;
        string dateTimeFormat = string.Empty;

        public enTypeReport Type
        { get { return enTypeReport.Object; } }
        public int IdTable
        { get { return tableInfo.idTable; } }
        public int IdObj
        { get { return idObj; } }
        public string Where
        { get { return "1 = 1"; } }
        public bool CancelOpenForm
        {
            get { return _cancelOpenForm; }
        }
        public int max_text_width = 0;//sanek

        private int indefiniteFiles;//файлы без названия

        List<TextBoxInfo> listOftextBoxes;//для ограничения ввода в текстовые поля \\ саша

        Dictionary<string, Image> LargeImageList = new Dictionary<string, Image>();
        Dictionary<string, Image> LargeImageList2 = new Dictionary<string, Image>();


        Interfaces.UserControls.IUserControlMain UControl;

        void s_CloseForm(object sender, Interfaces.UserControls.eventCloseForm e)
        {
            if (e.IsSave)
                this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public FormTableData(Interfaces.tablesInfo table, int idObject, bool isNew, string wkt, referInfo RefValue = null, bool setOwner=true)
        {
            var idTable = table.idTable;
            _isReadOnly = table.read_only;

            dateFormat = Application.CurrentCulture.DateTimeFormat.LongDatePattern;
            dateTimeFormat = Application.CurrentCulture.DateTimeFormat.FullDateTimePattern;
            try
            {
                //cti.ThreadProgress.ShowWait();
                _wkt = wkt;
                InitializeComponent();

                if (setOwner)
                    classesOfMetods.SetFormOwner(this);

                tableInfo = classesOfMetods.getTableInfo(idTable);
                idT = (tableInfo.RefTable) ?? idTable;
                _isLayer = (tableInfo.type == 1);

                listOftextBoxes = new List<TextBoxInfo>();

                idObj = idObject;
                pkfield = tableInfo.pkField;
                tableInfo = classesOfMetods.getTableInfo(idT);
                историяToolStripMenuItem.Visible = tableInfo.isHistory;
                this.Style = tableInfo.map_style;
                foreach (var item in Plugins.ListSpoofingAttributesOfObject)
                {
                    if (item.IdTable == tableInfo.idTable)
                    {
                        int? idObjNew = (isNew) ? (int?)null : idObject;
                        UControl = item.Func.Invoke(tableInfo.idTable, idObjNew, RefValue);

                        if (UControl.CancelOpen)
                            _cancelOpenForm = true;
                        UserControl uc = UControl.GetUserControl();
                        UControl.CloseForm += new EventHandler<Interfaces.UserControls.eventCloseForm>(s_CloseForm);
                        this.Text = UControl.Title;
                        //this.Width = uc.Width;
                        //this.Height = uc.Height;

                        if (UControl.SizeWindow != null)
                            this.Size = UControl.SizeWindow;

                        this.Controls.Clear();
                        this.Controls.Add(uc);
                        uc.Dock = DockStyle.Fill;
                        return;
                    }
                }
                try
                {
                    int? id = (isNew)
                        ? null
                        : (int?)idObject;
                    data_ = new DataConnect(tableInfo, id, this);
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message, Resources.Mes_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                startSize = this.Size;
                wrkPanel = new Panel { Name = "wrkpanel", Size = new Size(468, 28), Tag = new Point(3, 3), Dock = DockStyle.Fill };

                isNew1 = isNew;
                style1 = new axVisUtils.Styles.objStylesM();
                defRefValue = RefValue;
                message1 = "";
                photo_path = (Program.app.getPhotoInfo(tableInfo.idTable) != null)
                        ? Program.app.getPhotoInfo(tableInfo.idTable).namePhotoTable
                        : "";
                this.Tag = idObject;
                if (!_isLayer)
                {
                    экспортВToolStripMenuItem.Enabled = false;
                    импортИзToolStripMenuItem.Enabled = false;
                }
                else
                {
                    if (isNew)
                    {
                        экспортВToolStripMenuItem.Visible = false;
                    }
                    else
                        экспортВToolStripMenuItem.Visible = Program.UserActionRight.FirstOrDefault(f => f.Action.Name == "Export").Allowed;
                    импортИзToolStripMenuItem.Visible = Program.UserActionRight.FirstOrDefault(f => f.Action.Name == "Import").Allowed;

                }
                if (Plugins.ListAddMenuInAttribute.Count > 0 && !isNew)
                {
                    foreach (var item in Plugins.ListAddMenuInAttribute)
                    {
                        try
                        {
                            if (item.IdTable == tableInfo.idTable)
                                menuStrip1.Items.Add(item.ToolStripMenuItem.Invoke());
                        }
                        catch
                        {
                        }
                    }
                }

                if (Program.user_info == null || !Program.user_info.admin)
                    показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem.Visible = false;
                if (isNew)
                    показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem.Enabled = false;

            }
            finally
            {
                //cti.ThreadProgress.Close();
            }
        }

        public SplitContainer splitContainer1 = new SplitContainer();
        public void EndLoadDataToForm()
        {
            this.SuspendLayout();

            panelForProperties.Dock = DockStyle.Fill;

            #region Добавление кнопок "Сохранить", "Отмена", "Стиль", "История"
            Panel panelBottomInPanelForProperties = new Panel();
            panelBottomInPanelForProperties.Dock = DockStyle.Bottom;
            //panelBottomInPanelForProperties.BorderStyle = BorderStyle.FixedSingle;
            panelBottomInPanelForProperties.Size = new System.Drawing.Size(160, 35);
            panelForProperties.Controls.Add(panelBottomInPanelForProperties);
            int dlt = 3;

            Button btnOK = new Button();
            btnOK.Name = "btnOK";
            btnOK.Text = Resources.FormTableData_Save;
            btnOK.Size = new Size(80, 23);
            btnOK.Location = new Point(5, 8);//pt.Y + 3);
            btnOK.Font = new Font(btnOK.Font, FontStyle.Bold);
            btnOK.Click += new EventHandler(this.btnOK_Click);
            if (!classesOfMetods.getWriteTable(tableInfo.idTable) || _isReadOnly)
                btnOK.Enabled = false;

            Button btnCancel = new Button();
            btnCancel.Name = "btnCancel";
            btnCancel.Text = Resources.FormTableData_Cancel;
            btnCancel.Size = new Size(90, 23);
            btnCancel.Top = btnOK.Top;
            btnCancel.Left = btnOK.Right + dlt;
            btnCancel.Font = new Font(btnCancel.Font, FontStyle.Bold);
            btnCancel.Click += new EventHandler(btnCancel_Click);

            Button btnStyle = new Button();
            btnStyle.Name = "btnStyle";
            btnStyle.Text = Resources.FormTableData_Style;
            btnStyle.Size = new Size(80, 23);
            btnStyle.Top = btnCancel.Top;
            btnStyle.Left = btnCancel.Right + dlt;
            btnStyle.Font = new Font(btnStyle.Font, FontStyle.Bold);
            btnStyle.Click += new EventHandler(btnStyle_Click);

            if (!this.Style)
            {
                btnStyle.Visible = false;
            }

            panelBottomInPanelForProperties.Controls.AddRange(new[] { btnOK, btnCancel, btnStyle });
            #endregion

            pboxes = new List<PictureBox>();//sanek, для создания мультиселекта
            s_pboxes = new List<PictureBox>();

            this.Size = new Size(700, 480);
            if (!this.isNew1)
            {
                if (classesOfMetods.getTableInfo(idT).photo)
                {
                    loadPreview(idObj);
                }


            }
            else
                if (classesOfMetods.getTableInfo(idT).photo)
                    loadPreview(-1);

            loadRefTables();//загружает все связи с этой таблицей во вкладках
            loadGeometryToEdit();
            this.ResumeLayout();


            // Для pictureBox'ов устанавливаем их теги 
            foreach (Control contrl in splitContainer1.Panel2.Controls)
            {
                if (contrl.Name.Substring(0, 3).CompareTo("pb_") == 0)
                {
                    if ((contrl as PictureBox).Cursor == Cursors.Hand)
                    {
                        var dicc = new Dictionary<Control, Point>();
                        foreach (Control con in splitContainer1.Panel2.Controls)
                        {
                            if (contrl.Top + 10 < con.Top)
                            {
                                dicc.Add(con, new Point(con.Location.X, con.Location.Y - contrl.Bottom));
                            }
                        }
                        contrl.Tag = dicc;
                        try
                        {
                            TextBox tbb = splitContainer1.Panel2.Controls["col_" + contrl.Name.Substring(3, contrl.Name.Length - 3)] as TextBox;
                            if (tbb.Text.Contains("\n"))
                            {
                                pb_Click(contrl, null);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        private void pb_Click(object sender, EventArgs e)
        {
            PictureBox pb;
            if (sender is PictureBox)
                pb = (sender as PictureBox);
            else
                return;

            var dicc = (Dictionary<Control, Point>)pb.Tag;
            TextBox tb = (TextBox)splitContainer1.Panel2.Controls["col_" + pb.Name.Substring(3)];
            if (tb == null)
                return;

            if (tb.Multiline)
            {
                tb.Height = 20;
                tb.Multiline = false;
                tb.TextChanged -= textBox_TextChanged;
                tb.ScrollBars = ScrollBars.None;
                List<Control> list_text_box_lower = new List<Control>();

                foreach (Control contrl in dicc.Keys)
                {
                    String nName = "";
                    if (contrl.Name.StartsWith("pb_"))
                    {
                        nName = contrl.Name.Substring(3);
                    }
                    else if (contrl.Name.StartsWith("col_"))
                    {
                        nName = contrl.Name.Substring(4);
                    }
                    else if (contrl.Name.StartsWith("tbut_"))
                    {
                        nName = contrl.Name.Substring(5);
                    }
                    else if (contrl.Name.StartsWith("pbox"))
                    {
                        nName = contrl.Name.Substring(4);
                    }

                    bool b = true;
                    if (contrl is TextBox)
                        if ((contrl as TextBox).Multiline == true)
                        {
                            list_text_box_lower.Add(contrl);
                            contrl.Location = new Point(dicc[contrl].X, (sender as Control).Bottom + dicc[contrl].Y);
                            b = false;
                        }
                    if (b)
                    {
                        contrl.Location = new Point(dicc[contrl].X, (sender as Control).Bottom + dicc[contrl].Y);
                    }

                    Label lb = (Label)splitContainer1.Panel1.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, (sender as Control).Bottom + dicc[contrl].Y);
                    lb = (Label)mainPanel.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, (sender as Control).Bottom + dicc[contrl].Y);
                }

                foreach (Control contrl in list_text_box_lower)
                {
                    contrl.Size = new Size(contrl.Size.Width, contrl.Size.Height - 1);
                    textBox_TextChanged(contrl, null);
                }
            }
            else
            {
                tb.Multiline = true;
                tb.TextChanged += textBox_TextChanged;
                tb.ScrollBars = ScrollBars.Vertical;
                textBox_TextChanged(tb, null);
            }
        }
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as TextBox;

            Size size2 = new Size((sender as TextBoxEx).Width, (((sender as TextBoxEx).LineCount - 1) * 13 + 20));
            List<Control> list_text_box_lower = new List<Control>();
            if (size2.Height <= 20)
            {
                if (txt.Size != new Size((sender as TextBoxEx).Width, 20))
                {
                    txt.Size = new Size((sender as TextBoxEx).Width, 20);

                    PictureBox pb = (PictureBox)splitContainer1.Panel2.Controls["pb_" + txt.Name.Substring(4)];
                    Dictionary<Control, Point> dicc = (Dictionary<Control, Point>)pb.Tag;
                    foreach (Control contrl in dicc.Keys)
                    {
                        String nName = "";
                        if (contrl.Name.StartsWith("pb_"))
                        {
                            nName = contrl.Name.Substring(3);
                        }
                        else if (contrl.Name.StartsWith("col_"))
                        {
                            nName = contrl.Name.Substring(4);
                        }
                        else if (contrl.Name.StartsWith("tbut_"))
                        {
                            nName = contrl.Name.Substring(5);
                        }
                        else if (contrl.Name.StartsWith("pbox"))
                        {
                            nName = contrl.Name.Substring(4);
                        }

                        contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                        if (contrl is TextBox && (contrl as TextBox).Multiline == true)
                        {
                            contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                            list_text_box_lower.Add(contrl);
                        }

                        Label lb = (Label)splitContainer1.Panel1.Controls["lbl1_" + nName];
                        lb.Location = new Point(lb.Location.X, txt.Bottom + dicc[contrl].Y);
                        lb = (Label)mainPanel.Controls["lbl1_" + nName];
                        lb.Location = new Point(lb.Location.X, (sender as Control).Bottom + dicc[contrl].Y);
                    }
                }
            }
            else if ((size2.Height) > 202)
            {
                txt.Height = 202;
                PictureBox pb = (PictureBox)splitContainer1.Panel2.Controls["pb_" + txt.Name.Substring(4)];
                Dictionary<Control, Point> dicc = (Dictionary<Control, Point>)pb.Tag;
                foreach (Control contrl in dicc.Keys)
                {
                    String nName = "";
                    if (contrl.Name.StartsWith("pb_"))
                    {
                        nName = contrl.Name.Substring(3);
                    }
                    else if (contrl.Name.StartsWith("col_"))
                    {
                        nName = contrl.Name.Substring(4);
                    }
                    else if (contrl.Name.StartsWith("tbut_"))
                    {
                        nName = contrl.Name.Substring(5);
                    }
                    else if (contrl.Name.StartsWith("pbox"))
                    {
                        nName = contrl.Name.Substring(4);
                    }

                    contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                    if (contrl is TextBox && (contrl as TextBox).Multiline == true)
                    {
                        contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                        list_text_box_lower.Add(contrl);
                    }

                    Label lb = (Label)splitContainer1.Panel1.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, txt.Bottom + dicc[contrl].Y);
                    lb = (Label)mainPanel.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, (sender as Control).Bottom + dicc[contrl].Y);
                }
            }
            else //if (txt.Height != size2.Height)
            {
                txt.Height = size2.Height;
                PictureBox pb = (PictureBox)splitContainer1.Panel2.Controls["pb_" + txt.Name.Substring(4)];
                Dictionary<Control, Point> dicc = (Dictionary<Control, Point>)pb.Tag;
                foreach (Control contrl in dicc.Keys)
                {
                    String nName = "";
                    if (contrl.Name.StartsWith("pb_"))
                    {
                        nName = contrl.Name.Substring(3);
                    }
                    else if (contrl.Name.StartsWith("col_"))
                    {
                        nName = contrl.Name.Substring(4);
                    }
                    else if (contrl.Name.StartsWith("tbut_"))
                    {
                        nName = contrl.Name.Substring(5);
                    }
                    else if (contrl.Name.StartsWith("pbox"))
                    {
                        nName = contrl.Name.Substring(4);
                    }

                    contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                    if (contrl is TextBox && (contrl as TextBox).Multiline == true)
                    {
                        contrl.Location = new Point(dicc[contrl].X, txt.Bottom + dicc[contrl].Y);
                        list_text_box_lower.Add(contrl);
                    }

                    Label lb = (Label)splitContainer1.Panel1.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, txt.Bottom + dicc[contrl].Y);
                    lb = (Label)mainPanel.Controls["lbl1_" + nName];
                    lb.Location = new Point(lb.Location.X, (sender as Control).Bottom + dicc[contrl].Y);
                }
            }
            foreach (Control contrl in list_text_box_lower)
            {
                //contrl.Size = new Size(contrl.Size.Width, contrl.Size.Height - 1);
                textBox_TextChanged(contrl, null);
            }
        }

        private Panel _panelRightInWrk = null;
        private Panel _panelBottomInPanelRightInWrk = null;

        private void loadPreview(int idObj)
        {
            tablesInfo ti = classesOfMetods.getTableInfo(idT);
            cti.ThreadProgress.ShowWait();
            if (_panelRightInWrk == null)
            {
                _panelRightInWrk = new Panel();
                _panelRightInWrk.Dock = DockStyle.Right;
                _panelRightInWrk.Size = new System.Drawing.Size(160, 360);
                _panelBottomInPanelRightInWrk = new Panel();
                _panelBottomInPanelRightInWrk.Dock = DockStyle.Bottom;
                _panelBottomInPanelRightInWrk.Size = new System.Drawing.Size(160, 32);
            }

            if (panel3 == null)
            {
                panel3 = new Panel();
                grBox = new GroupBox();
                wrkPanel.Controls.Add(grBox);

                bool enable = !isNew && classesOfMetods.getWriteTable(tableInfo.idTable) && !_isReadOnly;

                var btnAdd = new Button()
                {
                    BackgroundImage = Rekod.Properties.Resources.add,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(30, 30),
                    Location = new Point(3, 1),
                    Enabled = enable
                };
                btnAdd.Click += new EventHandler(clickAddFile);

                _panelBottomInPanelRightInWrk.Controls.Add(btnAdd);


                var btnSave = new Button()
                {
                    BackgroundImage = Rekod.Properties.Resources.save5,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(30, 30),
                    Location = new Point(36, 1),
                    Enabled = !isNew
                };
                btnSave.Click += new EventHandler(clickSaveFile);
                _panelBottomInPanelRightInWrk.Controls.Add(btnSave);

                var btnDelete = new Button()
                {
                    BackgroundImage = Rekod.Properties.Resources.delete,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(30, 30),
                    Location = new Point(69, 1),
                    Enabled = enable
                };

                btnDelete.Click += new EventHandler(clickDeleteFile);
                _panelBottomInPanelRightInWrk.Controls.Add(btnDelete);



                grBox.Controls.Add(panel3);

                panel3.BorderStyle = BorderStyle.Fixed3D;
                panel3.Size = new Size(150, 305);
                Point p = (Point)wrkPanel.Tag;
                panel3.BackColor = Color.White;
                panel3.Top = 15;
                panel3.Left = 5;
                panel3.AutoScroll = true;
                panel3.Dock = DockStyle.Fill;

                grBox.Text = Resources.FormTableData_Files;
                grBox.Size = new Size(160, 360);
                grBox.Top = 5;//p.Y + 10;
                grBox.Left = 412 - (150 - max_text_width);
                grBox.Dock = DockStyle.Fill;
                wrkPanel.Tag = new Point(p.X, p.Y + 195);
            }

            _panelRightInWrk.Controls.Add(grBox);
            wrkPanel.Controls.Add(_panelRightInWrk);
            _panelRightInWrk.Controls.Add(_panelBottomInPanelRightInWrk);

            panel3.Controls.Clear();
            pboxes.Clear();
            s_pboxes.Clear();
            if (!isNew)
            {
                #region Загрузка значков предпросмотра файлов
                bool have_img_preview = true;
                string sql = "SELECT ph.id, ph.img_preview, ph.file_name, (ph.img_preview is not null) as exists_prev FROM " +
                    ti.nameSheme + "." + ti.nameDB + " obj, " + ti.nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " ph " +
                    "WHERE ph." + classesOfMetods.getPhotoInfo(idT).namePhotoField + " = obj." + ti.pkField + " AND obj." + ti.pkField + " = " + idObj.ToString();

                //SqlWork sqlCmd = new SqlWork();
                //try
                //{
                //    if (Program.conn.PostgreSqlVersion.Major > 8)
                //    {

                //        sqlCmd.sql = "set bytea_output = 'hex'";
                //        sqlCmd.Execute(true);
                //        sqlCmd.Close();
                //    }
                //}
                //catch
                //{

                //}
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.sql = sql;
                try
                {
                    sqlCmd.Execute(false);
                }
                catch (NpgsqlException ex)
                {
                    if (ex.Code.CompareTo("42703") == 0)//ошибка изза отсутствия колонки
                    {
                        sqlCmd = new SqlWork();
                        sqlCmd.sql = "SELECT ph.id FROM " +
                            ti.nameSheme + "." + ti.nameDB + " obj, " + ti.nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " ph " +
                            "WHERE ph." + classesOfMetods.getPhotoInfo(idT).namePhotoField + " = obj." + classesOfMetods.getPhotoInfo(idT).nameFieldID + " AND obj." + classesOfMetods.getPhotoInfo(idT).nameFieldID + " = " + idObj.ToString();
                        sqlCmd.Execute(false);
                        have_img_preview = false;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                int i = 0;
                indefiniteFiles = 0;//файлы без названия(неопределенные файлы)
                while (sqlCmd.CanRead())
                {
                    int id_img = sqlCmd.GetInt32(0);
                    Byte[] binarybl = new Byte[0];
                    if (have_img_preview)
                    {
                        if (sqlCmd.GetValue(1) != null)
                        {
                            binarybl = sqlCmd.GetBytes(1, 0, 0, int.MaxValue);
                        }
                        if (sqlCmd.GetValue(2) != null)
                        {
                            String fileName = sqlCmd.GetString(2);
                            pboxes.Add(addPhotoToEnd(id_img, binarybl, fileName));
                        }
                        else
                        {
                            pboxes.Add(addPhotoToEnd(id_img, binarybl, null));
                        }
                        binarybl = null;
                    }
                    else
                    {
                        pboxes.Add(addPhotoToEnd(id_img, null, null));
                    }
                    i++;
                }
                if (countLabel != null)
                    countLabel.Text = changeTextOfCount(i);
                sqlCmd.Close();
                #endregion
            }
            cti.ThreadProgress.Close();
        }
        public Icon SetIcon
        {
            set
            { this.Icon = value; }
        }
        public bool Style
        {
            get { return haveStyle; }
            set { haveStyle = value; }
        }
        public bool isNew
        {
            get { return isNew1; }
            set { isNew1 = value; }
        }
        public Styles.objStylesM MyStyles
        {
            set { style1 = value; }
            get { return style1; }
        }

        private void loadRefTables()
        {
            List<ref_with_tables> ref_tables = new List<ref_with_tables>();
            SqlWork sqlCmd = new SqlWork();
            {
                sqlCmd.sql = "SELECT ref_field,id,id_table,name_db FROM " + Program.scheme + ".table_field_info WHERE ref_table=" + idT;
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    ref_with_tables ref_ = new ref_with_tables();
                    ref_.id_col = sqlCmd.GetInt32(0);
                    ref_.id_ref_col = sqlCmd.GetInt32(1);
                    ref_.id_ref_table = sqlCmd.GetInt32(2);
                    ref_.db_name_ref_col = sqlCmd.GetString(3);
                    ref_tables.Add(ref_);
                }
            }
            t_control.TabPages[0].Controls.Add(wrkPanel);
            wrkPanel.Dock = DockStyle.Fill;


            this.Size = new Size(this.Size.Width + 7, this.Size.Height + 25);
            if (classesOfMetods.getTableInfo(idT).type == 2 || classesOfMetods.getTableInfo(idT).type == 3)
                return;
            if (!isNew)
                foreach (ref_with_tables ref_ in ref_tables)
                {
                    var fi = classesOfMetods.getFieldInfo(ref_.id_ref_col);
                    if (fi.visible == true)
                    {
                        var right = classesOfMetods.getTableRight(ref_.id_ref_table);
                        if (right != null && right.read && classesOfMetods.getRefTableRight(idT, ref_.id_ref_table))
                        {
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT name_map FROM " + Program.scheme + ".table_info WHERE id=" + ref_.id_ref_table;
                            sqlCmd.Execute(false);
                            string name_table = "";
                            while (sqlCmd.CanRead())
                            {
                                name_table = sqlCmd.GetString(0);
                            }
                            sqlCmd.Close();
                            t_control.TabPages.Add(name_table);

                            string name_cur_table_col = "";
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT name_db FROM " + Program.scheme + ".table_field_info WHERE id=" + ref_.id_col;
                            sqlCmd.Execute(false);
                            while (sqlCmd.CanRead())
                            {
                                name_cur_table_col = sqlCmd.GetString(0);
                            }
                            sqlCmd.Close();
                            string cur_name_table = "";
                            string cur_name_sceme = "";
                            string name_pk_field = "";
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT name_db,scheme_name,pk_fileld FROM " + Program.scheme + ".table_info WHERE id=" + idT;
                            sqlCmd.Execute(false);
                            while (sqlCmd.CanRead())
                            {
                                cur_name_table = sqlCmd.GetString(0);
                                cur_name_sceme = sqlCmd.GetString(1);
                                name_pk_field = sqlCmd.GetString(2);
                            }
                            sqlCmd.Close();
                            int cur_value = 0;
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "SELECT " + name_cur_table_col + " FROM " + cur_name_sceme + "." + cur_name_table + " WHERE " + name_pk_field + "=" + idObj;
                            sqlCmd.Execute(false);
                            while (sqlCmd.CanRead())
                            {
                                cur_value = sqlCmd.GetInt32(0);
                            }
                            sqlCmd.Close();
                            TabPage cur_page = t_control.TabPages[t_control.TabPages.Count - 1];

                            var search = new FieldKeyValue(classesOfMetods.getFieldInfo(ref_.id_ref_col), cur_value);
                            var tableData = new UcTableObjects(search, idObj);
                            tableData.SetVisibleUseMapFilterMenu(false);
                            cur_page.Controls.Add(tableData);
                            tableData.Dock = DockStyle.Fill;
                        }
                    }
                }
        }

        // Загрузка геометрии
        private void loadGeometryToEdit()
        {
            if (_isLayer)
            {
                var connect = (NpgsqlConnectionStringBuilder)Program.connString;
                TabPage page = new TabPage(Rekod.Properties.Resources.PgGeomVRec_Header);
                int? idObjNullable = idObj;
                if (idObj == -1)
                {
                    idObjNullable = null;
                }
                int? srid = null;
                if (!String.IsNullOrEmpty(_wkt) && !isNew)
                {
                    srid = Convert.ToInt32(Program.app.mapLib.SRID);
                }
                _geometryVM = new PgAttributesGeomVM(idT, _wkt, false, idObjNullable, connect, srid);

                Rekod.DataAccess.SourcePostgres.View.PgAttributes.PgAttributesGeomV pgAttributesGeomV =
                    new Rekod.DataAccess.SourcePostgres.View.PgAttributes.PgAttributesGeomV() { DataContext = _geometryVM };

                ElementHost elementHost = new ElementHost();
                elementHost.Child = pgAttributesGeomV;
                page.Controls.Add(elementHost);
                t_control.TabPages.Add(page);
                elementHost.Dock = DockStyle.Fill;
                data_.GeometryVM = _geometryVM;
            }
        }

        void btnStyle_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Name = "styleControlContainer";
            f.Text = Resources.FormTableData_StyleForm;
            f.Icon = ((System.Drawing.Icon)(global::Rekod.Properties.Resources.Globe));
            f.StartPosition = FormStartPosition.CenterScreen;
            f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            var right = classesOfMetods.getTableRight(tableInfo.idTable);
            var sc = new Rekod.DBTablesEdit.StyleControl(true, true, true,
                right != null ? right.write && Rekod.DBTablesEdit.SyncController.HasRight(tableInfo.idTable) : false);
            f.Size = new System.Drawing.Size(sc.Width + 20, sc.Height + 25);
            f.MaximizeBox = false;
            sc.setStyles(style1);
            f.Controls.Add(sc);
            sc.Dock = DockStyle.Fill;
            if (f.ShowDialog() == DialogResult.OK)
                this.MyStyles = sc.getStyles();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        // Кнопка "Сохранить"
        void btnOK_Click(object sender, EventArgs e)
        {
            DataColumn[] col = new DataColumn[0];
            try
            {
                if (checkValues())
                {
                    int i = 0;
                    foreach (Control cc in splitContainer1.Panel2.Controls)
                    {
                        if (cc.Name.Contains("col_"))
                        {
                            DataColumn[] col1 = new DataColumn[col.Length + 1];
                            col.CopyTo(col1, 0);
                            col = new DataColumn[col1.Length];
                            col1.CopyTo(col, 0);
                            col[i] = (DataColumn)cc.Tag;
                            i++;
                        }
                    }
                    if (defRefValue != null)
                    {
                        DataColumn[] col1 = new DataColumn[col.Length + 1];
                        col.CopyTo(col1, 0);
                        col = new DataColumn[col1.Length];
                        col1.CopyTo(col, 0);
                        col[i] = defRefCol;
                    }

                    int x = idObj;
                }
                else if (message1 == "Cancel")
                {
                    return;
                }
                else if (message1 != "")
                {
                    MessageBox.Show(message1);
                    message1 = "";
                    return;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show(Resources.FormTableData_CheckFrormatData, Resources.InformationMessage_Header, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Сюда нужно вставить передачу вкт в _data
            if (data_ != null && _geometryVM != null && _isLayer)
            {
                if (_geometryVM != null)
                {
                    if (!_geometryVM.GObjectIsValid())
                    {
                        var mesg = MessageBox.Show(Resources.FormTableData_Continue, Resources.FormTableData_ErrorGeom, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (mesg != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                }

                data_.WKT = _geometryVM.WKT;

                if (!data_.Save(col))
                    return;
            }
            else if (data_ != null)
            {
                if (!data_.Save(col))
                    return;
            }

            //Обновление слоя если является слоем
            {
                string Namelayer = Program.RelationVisbleBdUser.GetNameInBd(tableInfo.idTable);
                if (!string.IsNullOrEmpty(Namelayer)
                        && Program.app.mapLib.getLayer(Namelayer) != null
                        && Program.app.mapLib.getLayer(Namelayer).External == true)
                {
                    Program.app.mapLib.getLayer(Namelayer).ExternalFullReloadVisible();//(axMapLIb1.getLayer(Namelayer).getBbox());
                    Program.app.mapLib.mapRepaint();
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private Color GetStandartControlColor(Control cntrl)
        {
            if (cntrl is Label)
            {
                if (((Label)cntrl).Font.Underline)
                {
                    return Color.FromArgb(14, 100, 220);
                }
                else
                {
                    return new Label().ForeColor;
                }
            }
            return SystemColors.ControlText;
        }
        private bool checkValues()
        {
            bool rez = true;
            Dictionary<DataColumn, List<Object>> intervalQuestions = new Dictionary<DataColumn, List<object>>();

            foreach (Control cc in splitContainer1.Panel2.Controls)
            {
                if (cc.Name.Contains("col_"))
                {
                    #region TextBox
                    if (cc is TextBox)
                    {
                        TextBox tx = (TextBox)cc;
                        DataColumn dt = (DataColumn)tx.Tag;

                        switch (dt.Type)
                        {
                            case DataColumn.enType.Numeric:
                            case DataColumn.enType.Integer:
                                {
                                    try
                                    {
                                        if (dt.BaseName != pkfield)
                                        {
                                            double? value;
                                            var txt = tx.Text;

                                            value = ConverToDouble(txt);

                                            if (value != null && dt.isInterval)
                                            {
                                                var isIntervError = true;
                                                foreach (ValueInterval ii in dt.Interval)
                                                {
                                                    if (ii.isInclude(value.Value))
                                                    {
                                                        isIntervError = false;
                                                        break;
                                                    }
                                                }
                                                if (isIntervError)
                                                {
                                                    List<Object> parameters = new List<object>() { message1, tx.Text, dt.RelTableId };
                                                    intervalQuestions.Add(dt, parameters);
                                                }
                                            }
                                            if (!tx.ReadOnly) //если связь идет с таблицей, то значение ReadOnly=true; в dt.Data уже содержиться нужная информация
                                                dt.Data = value;

                                        }
                                        else
                                        {
                                            dt.Data = null;
                                        }
                                        if (dt.IsNotNull)
                                        {
                                            if (dt.Data == null)
                                            {
                                                throw new Exception("Поле не должно быть пустым!");
                                            }
                                            else
                                            {
                                                string d = tx.Name.Replace("col_", "");
                                                splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = GetStandartControlColor(splitContainer1.Panel1.Controls["lbl1_" + d]);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        rez = false;
                                        string d = tx.Name.Replace("col_", "");
                                        splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = Color.Red;
                                    }
                                }
                                break;
                            case DataColumn.enType.Text:
                            case DataColumn.enType.Date:
                            case DataColumn.enType.DateTime:
                                {
                                    try
                                    {
                                        if (dt.BaseName != pkfield)
                                        {
                                            dt.Data = tx.Text;
                                        }
                                        else
                                        {
                                            dt.Data = "";
                                        }
                                        if (dt.IsNotNull)
                                        {
                                            if (dt.Data == null || string.IsNullOrEmpty(dt.Data.ToString()))
                                            {
                                                throw new Exception("Поле не должно быть пустым!");
                                            }
                                            else
                                            {
                                                string d = tx.Name.Replace("col_", "");
                                                splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = GetStandartControlColor(splitContainer1.Panel1.Controls["lbl1_" + d]);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        rez = false;
                                        string d = tx.Name.Replace("col_", "");
                                        splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = Color.Red;
                                    }
                                }
                                break;
                        }
                    }
                    #endregion
                    #region DateTimePicker
                    else if (cc is DateTimePicker)
                    {
                        DateTimePicker tx = (DateTimePicker)cc;

                        DataColumn dt = (DataColumn)tx.Tag;
                        if (dt.Type == DataColumn.enType.DateTime || dt.Type == DataColumn.enType.Date)
                        {
                            try
                            {
                                if (tx.Checked)
                                {
                                    dt.Data = tx.Value;
                                }
                                else
                                {
                                    dt.Data = null;
                                }
                                if (dt.IsNotNull)
                                {
                                    if (dt.Data == null)
                                    {
                                        throw new Exception("Поле не должно быть пустым!");
                                    }
                                    else
                                    {
                                        string d = tx.Name.Replace("col_", "");
                                        splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = GetStandartControlColor(splitContainer1.Panel1.Controls["lbl1_" + d]);
                                    }
                                }
                            }
                            catch
                            {
                                rez = false;
                                string d = tx.Name.Replace("col_", "");
                                splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = Color.Red;
                            }
                        }
                    }
                    #endregion
                    #region ComboBox
                    else if (cc is ComboBox)
                    {
                        ComboBox tx = (ComboBox)cc;
                        DataColumn dt = (DataColumn)tx.Tag;
                        //DicCollection dc = dt.Data;
                        try
                        {
                            if (tx.SelectedItem != null)
                            {
                                StylesM dc = (StylesM)tx.SelectedItem;
                                dt.Data = dc.Id;
                            }
                            else
                            {
                                dt.Data = null;
                            }
                            if (dt.IsNotNull)
                            {
                                if (dt.Data == null)
                                {
                                    throw new Exception("Поле не должно быть пустым!");
                                }
                                else
                                {
                                    string d = tx.Name.Replace("col_", "");
                                    splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = GetStandartControlColor(splitContainer1.Panel1.Controls["lbl1_" + d]);
                                }
                            }
                        }
                        catch
                        {
                            rez = false;
                            string d = tx.Name.Replace("col_", "");
                            splitContainer1.Panel1.Controls["lbl1_" + d].ForeColor = Color.Red;
                        }
                    }
                    #endregion
                }
            }

            if (intervalQuestions.Count != 0)
            {
                String message = "";
                foreach (var intervalQuestion in intervalQuestions)
                {
                    //message += String.Format("Значение поля \"{0}\" не попало ни в один из интервалов\n", intervalQuestion.Key.Name, intervalQuestion.Value[2]); 
                    message += String.Format(Resources.FormTableData_ValueIntervalsNotFoundBySaves + ".\n", intervalQuestion.Key.Name);
                }
                message += Resources.FormTableData_Save + "?";
                if (MessageBox.Show(message, Resources.FormTableData_SaveData, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    rez = true;
                }
                else
                {
                    message1 = "Cancel";
                    rez = false;
                }

            }
            return rez;
        }

        private static double? ConverToDouble(string txt)
        {
            double dd;
            if (!double.TryParse(txt, out dd))
                if (!double.TryParse(txt.Replace(",", "."), out dd))
                    if (!double.TryParse(txt.Replace(".", ","), out dd))
                        return null;
            return dd;
        }


        void AllowInteger(object sender, KeyPressEventArgs e)
        {
            var txt = sender as TextBox;
            if (e.KeyChar == (char)22)
            {
                e.Handled = true;
            }//Ctrl+V
            // Если это не цифра.
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))//для проверки backspace
            {
                e.Handled = true;
                ToolTip t = new ToolTip();

                int pp = txt.SelectionStart;
                t.Show(
                    Resources.FormTableData_EnterOnlyInteger, this,
                        txt.Left + 20 + pp * 6, txt.Top + 30, 2000);
            }
        }
        void AllowReal(object sender, KeyPressEventArgs e)
        {
            var txt = sender as TextBox;
            if (e.KeyChar == (char)22)
            {
                e.Handled = true;
            }//Ctrl+V
            // Если это не цифра.
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))//для проверки backspace
            {
                // Запрет на ввод более одной десятичной точки.
                if (sender is TextBox)
                    if (e.KeyChar != '.' || txt.Text.IndexOf(".") != -1)
                    {
                        e.Handled = true;
                        ToolTip t = new ToolTip();
                        int pp = txt.SelectionStart;
                        t.Show(Resources.FormTableData_EnterOnlyNumeric, this,
                                txt.Left + 20 + pp * 6, txt.Top + 30, 2000);
                    }
            }
        }

        Panel mainPanel = new Panel();
        Panel leftPanel = new Panel();
        Panel rightPanel = new Panel();

        public void AddColumn(DataColumn col, Rekod.fieldInfo fi)
        {
            Point location1;
            Label lb_1 = new Label();
            PictureBox pb = new PictureBox();
            int x, y;
            if (panelForProperties == null)
            {
                panelForProperties = new Panel();
                panelForProperties.Top = 5;
                panelForProperties.Left = 5;
                panelForProperties.AutoScroll = true;
                panelForProperties.Dock = DockStyle.Fill;
                wrkPanel.Controls.Add(panelForProperties);

                Color bcColorPanel1 = splitContainer1.Panel1.BackColor;
                mainPanel.BackColor = Color.FromArgb(bcColorPanel1.R - 30, bcColorPanel1.G - 30, bcColorPanel1.B - 30);

                splitContainer1.Panel1.BackColor = bcColorPanel1;
                splitContainer1.Panel2.BackColor = bcColorPanel1;

                splitContainer1.SplitterDistance = 270;
                splitContainer1.FixedPanel = FixedPanel.Panel1;

                splitContainer1.BringToFront();
                splitContainer1.Size = new Size(100, this.Height);
                splitContainer1.Dock = DockStyle.Fill;
                mainPanel.Dock = DockStyle.Fill;
                mainPanel.BorderStyle = BorderStyle.FixedSingle;
                mainPanel.AutoScroll = true;
                mainPanel.Controls.Add(splitContainer1);
                panelForProperties.Controls.Add(mainPanel);
            }
            if (defRefValue != null)
            {
                if (col.BaseName == defRefValue.nameField)
                {
                    defRefCol = col;
                    defRefCol.Data = defRefValue.idObj;
                    return;
                }
                else if (col.Name == defRefValue.nameField)
                {
                    defRefCol = col;
                    return;
                }
            }

            int textLeftIndent = 33;
            int textRightIndent = 16;
            int labelLeftIndent = 4;


            lb_1 = new Label();
            lb_1.AutoSize = true;
            lb_1.TextAlign = ContentAlignment.MiddleLeft;
            lb_1.Name = "lbl1_" + col.BaseName;
            lb_1.Text = fi.is_not_null ? col.Name + "*" : lb_1.Text = col.Name;

            switch (col.Type)
            {
                case DataColumn.enType.Date:
                case DataColumn.enType.DateTime:
                    {
                        var dt = new DateTimePicker();
                        dt.Format = DateTimePickerFormat.Custom;
                        if (col.Type == DataColumn.enType.Date)
                        {
                            dt.CustomFormat = dateFormat;
                            dt.Size = new Size(180, 20);
                        }
                        else
                        {
                            dt.CustomFormat = dateTimeFormat;
                            dt.Size = new Size(220, 20);
                        }
                        //dt.CustomFormat = "''";

                        dt.Name = "col_" + col.BaseName;
                        dt.Tag = col;
                        dt.Enabled = col.Edited;
                        dt.MouseUp += new MouseEventHandler(dt_MouseUp);
                        dt.ValueChanged += new EventHandler(dt_ValueChanged);
                        dt.ShowCheckBox = true;
                        //dt.KeyDown += new KeyEventHandler(dt_ValueChanged);


                        if (col.Data != null)
                            toolTip2.SetToolTip(lb_1, ((DateTime)col.Data).ToString(dt.CustomFormat));

                        pb.Image = (Image)Rekod.Properties.Resources.data_time1;
                        pb.SizeMode = PictureBoxSizeMode.StretchImage;
                        pb.Size = new Size(23, 15);
                        pb.Name = "pb_" + col.BaseName;
                        if (col.Type == DataColumn.enType.Date)
                            toolTip2.SetToolTip(pb, Resources.FormTableData_Date);
                        else
                            toolTip2.SetToolTip(pb, Resources.FormTableData_DateAndTime);

                        location1 = (Point)wrkPanel.Tag;
                        x = location1.X;
                        y = location1.Y;

                        lb_1.Location = new Point(x + labelLeftIndent, y);
                        pb.Location = new Point(x + 5, y + 3);
                        dt.Location = new Point(x + textLeftIndent, y);

                        wrkPanel.Tag = new Point(x, y + 20);
                        //dt.Width = splitContainer1.Panel2.Width - (textLeftIndent + textRightIndent);
                        dt.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                        splitContainer1.Panel1.Controls.Add(lb_1);
                        splitContainer1.Panel2.Controls.Add(pb);
                        splitContainer1.Panel2.Controls.Add(dt);

                        Label lbl1 = new Label();
                        lbl1.Location = lb_1.Location;
                        lbl1.Name = lb_1.Name;
                        mainPanel.Controls.Add(lbl1);

                    }
                    break;
                case DataColumn.enType.Integer:
                case DataColumn.enType.Numeric:
                    {

                        TextBox tx = new TextBox();
                        tx.Size = new Size(100, 20);
                        tx.Name = "col_" + col.BaseName;
                        tx.Tag = col;
                        tx.ReadOnly = !(col.Edited);
                        listOftextBoxes.Add(new TextBoxInfo(tx));
                        tx.MouseUp += new MouseEventHandler(textBoxMouseUp);
                        tx.KeyUp += new KeyEventHandler(textBoxKeyUp);

                        pb.SizeMode = PictureBoxSizeMode.StretchImage;
                        pb.Size = new Size(23, 15);
                        pb.Name = "pb_" + col.BaseName;

                        location1 = (Point)wrkPanel.Tag;
                        x = location1.X;
                        y = location1.Y;

                        lb_1.Location = new Point(x + labelLeftIndent, y);
                        pb.Location = new Point(x + 5, y + 3);
                        tx.Location = new Point(x + textLeftIndent, y);
                        wrkPanel.Tag = new Point(x, y + 20);

                        bool ref_whit_table = false;
                        if (fi.is_reference == true)
                        {
                            var t = classesOfMetods.getTableInfo(fi.ref_table.Value);
                            if (t.type == 1 || t.type == 4)
                            {
                                ref_whit_table = true;

                            }
                            else
                                ref_whit_table = false;
                        }
                        else
                            ref_whit_table = false;

                        if (ref_whit_table)
                        {
                            tx.Size = new Size(190, 20);
                            tx.Width = splitContainer1.Panel2.Width - (textLeftIndent + textRightIndent);
                            tx.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        }
                        else
                        {
                            tx.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                        }
                        //tx.Width = splitContainer1.Panel2.Width - (textLeftIndent + textRightIndent);


                        splitContainer1.Panel1.Controls.Add(lb_1);
                        splitContainer1.Panel2.Controls.Add(pb);
                        splitContainer1.Panel2.Controls.Add(tx);

                        Label lbl1 = new Label();
                        lbl1.Location = lb_1.Location;
                        lbl1.Name = lb_1.Name;
                        mainPanel.Controls.Add(lbl1);

                        if (col.Type == DataColumn.enType.Integer)
                        {
                            tx.TextChanged += new EventHandler(textBoxChangedInt);
                            toolTip2.SetToolTip(lb_1, ((int?)col.Data).ToString());
                            pb.Image = (Image)Rekod.Properties.Resources.int1;
                            toolTip2.SetToolTip(pb, Resources.FormTableData_Integer);
                        }
                        else
                        {
                            tx.TextChanged += new EventHandler(textBoxChangedReal);
                            toolTip2.SetToolTip(lb_1, ((double?)col.Data).ToString());
                            pb.Image = (Image)Rekod.Properties.Resources.real1;
                            toolTip2.SetToolTip(pb, Resources.FormTableData_Numeric);
                        }

                    }
                    break;
                case DataColumn.enType.Text:
                    {
                        TextBox tx = new TextBoxEx();//Ex для получения количества строк при расширеном редактировании
                        tx.Size = new Size(190, 20);
                        tx.Name = "col_" + col.BaseName;
                        tx.Tag = col;
                        tx.ReadOnly = !(col.Edited);


                        toolTip2.SetToolTip(lb_1, (string)col.Data);

                        pb.Image = (Image)Rekod.Properties.Resources.text1;
                        pb.SizeMode = PictureBoxSizeMode.StretchImage;
                        pb.Size = new Size(23, 15);
                        pb.Name = "pb_" + col.BaseName;

                        toolTip2.SetToolTip(pb, Resources.FormTableData_Text);
                        pb.Cursor = Cursors.Hand;
                        pb.Click += new EventHandler(pb_Click);

                        location1 = (Point)wrkPanel.Tag;
                        x = location1.X;
                        y = location1.Y;

                        lb_1.Location = new Point(x + labelLeftIndent, y);
                        pb.Location = new Point(x + 5, y + 3);
                        tx.Location = new Point(x + textLeftIndent, y);
                        wrkPanel.Tag = new Point(x, y + 20);

                        tx.Width = splitContainer1.Panel2.Width - (textLeftIndent + textRightIndent);
                        tx.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                        splitContainer1.Panel1.Controls.Add(lb_1);
                        splitContainer1.Panel2.Controls.Add(pb);
                        splitContainer1.Panel2.Controls.Add(tx);

                        Label lbl1 = new Label();
                        lbl1.Location = lb_1.Location;
                        lbl1.Name = lb_1.Name;
                        mainPanel.Controls.Add(lbl1);
                    }
                    break;
                default:
                    break;
            }
        }


        /////////////////////////////////// для ограничения ввода в текстовые поля ////////////////////////////////////
        private void textBoxChangedInt(object sender, EventArgs e)
        {
            int k;
            var txt = sender as TextBox;
            if ((!int.TryParse(txt.Text, out k) && txt.Text.CompareTo("") != 0 &&
                txt.Text.CompareTo("-") != 0) || txt.Text.Contains(" "))
            {
                String message = String.Empty;
                try
                {
                    Int32.Parse(txt.Text);
                }
                catch (OverflowException exc)
                {
                    message = exc.Message;
                }
                catch (Exception exc)
                {
                    message = Resources.FormTableData_EnterOnlyInteger;
                }

                foreach (TextBoxInfo tb in listOftextBoxes)
                {
                    if (tb.t == txt)
                    {
                        tb.undo();
                        toolTip1.Show(message, txt, 0, -txt.Height, 2000);
                    }
                }
            }
            else
            {
                foreach (TextBoxInfo tb in listOftextBoxes)
                {
                    if (tb.t == txt)
                    {
                        if (!tb.shanged)
                            tb.saveText();
                        else
                            tb.shanged = false;
                    }
                }
            }
        }
        private void textBoxChangedReal(object sender, EventArgs e)
        {
            decimal k;
            var txt = sender as TextBox;
            bool containts_dot = false;
            if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
            {
                if (txt.Text.Contains(","))
                {
                    int bb = txt.SelectionStart;
                    txt.Text = txt.Text.Replace(",", ".");
                    txt.SelectionStart = bb;
                    foreach (TextBoxInfo tb in listOftextBoxes)
                    {
                        if (tb.t == txt)
                        {
                            tb.saveText();
                        }
                    }
                    containts_dot = true;
                }
            }
            else
                if (txt.Text.Contains("."))
                {
                    int bb = txt.SelectionStart;
                    txt.Text = txt.Text.Replace(".", ",");
                    txt.SelectionStart = bb;
                    foreach (TextBoxInfo tb in listOftextBoxes)
                    {
                        if (tb.t == txt)
                        {
                            tb.saveText();
                        }
                    }
                    containts_dot = true;
                }
            if (!containts_dot
                && ((!decimal.TryParse(txt.Text, out k)
                        && !String.IsNullOrEmpty(txt.Text)
                        && txt.Text.CompareTo("-") != 0)
                    || txt.Text.Contains(" ")
                    || System.Text.RegularExpressions.Regex.IsMatch(txt.Text, ".*\\d.*-.*")))
            {
                String message = String.Empty;
                try
                {
                    decimal.Parse(txt.Text);
                }
                catch (OverflowException exc)
                {
                    message = exc.Message;
                }
                catch (Exception exc)
                {
                    message = Resources.FormTableData_EnterOnlyNumeric;
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(txt.Text, ".*\\d.*-.*"))
                    message = Resources.FormTableData_EnterOnlyNumeric;

                foreach (TextBoxInfo tb in listOftextBoxes)
                {
                    if (tb.t == txt)
                    {
                        tb.undo();
                        toolTip1.Show(message, txt, 0, -txt.Height, 2000);
                    }
                }
            }
            else
            {
                foreach (TextBoxInfo tb in listOftextBoxes)
                {
                    if (tb.t == txt)
                    {
                        if (!tb.shanged)
                            tb.saveText();
                        else
                            tb.shanged = false;
                    }
                }
            }
        }
        private void textBoxMouseUp(object sender, MouseEventArgs e)
        {
            foreach (TextBoxInfo tb in listOftextBoxes)
            {
                if (tb.t == (sender as TextBox))
                {
                    tb.saveText();
                }
            }
        }

        private void textBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Up || e.KeyData == Keys.Down)
            {
                foreach (TextBoxInfo tb in listOftextBoxes)
                {
                    if (tb.t == (sender as TextBox))
                    {
                        tb.saveText();
                    }
                }
            }
        }
        // sasha

        private void clickAddFile(Object sender, EventArgs e)
        {
            if (!classesOfMetods.getWriteTable(idT) || _isReadOnly)
            {
                MessageBox.Show(Resources.FormTableData_RightEditable);
                return;
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                cti.ThreadProgress.ShowWait();
                foreach (string str in openFileDialog1.FileNames)
                {
                    cti.ThreadProgress.Close();
                    if ((new FileInfo(str)).Length > 6291456)
                    {
                        if ((new BreakOrSkipDialog()).ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                            continue;
                        else
                            break;
                    }
                    cti.ThreadProgress.ShowWait();
                    try
                    {
                        addFile(idObj, str);
                    }
                    catch (Exception ex)
                    {
                        cti.ThreadProgress.Close();
                        workLogFile.writeLogFile(ex, true, true);
                    }
                }
                loadPreview(idObj);
                cti.ThreadProgress.Close();
            }
        }
        private void addFile(int id_obj, string patch)
        {
            FileStream fs = new FileStream(patch, FileMode.Open, FileAccess.Read);

            byte[] myData = new byte[fs.Length];
            fs.Read(myData, 0, (int)fs.Length);
            fs.Close();
            fs.Dispose();
            String fileExt = System.IO.Path.GetExtension(patch);
            MemoryStream ms2 = new MemoryStream(myData);
            MemoryStream ms = new MemoryStream();
            String fileName = System.IO.Path.GetFileName(patch);
            if (fileExt.ToUpper() == ".JPG" || fileExt.ToUpper() == ".JPEG" || fileExt.ToUpper() == ".PNG" || fileExt.ToUpper() == ".BMP")
                try
                {
                    Bitmap bt = new Bitmap(ms2);
                    switch (fileExt.ToUpper())
                    {
                        case ".JPG":
                            resizeImage(((Image)bt), new Size(125, 105)).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".JPEG":
                            resizeImage(((Image)bt), new Size(125, 105)).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".PNG":
                            resizeImage(((Image)bt), new Size(125, 105)).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case ".BMP":
                            resizeImage(((Image)bt), new Size(125, 105)).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                    }

                }
                catch { }

            var sql = string.Format("INSERT INTO {0}.{1} ({2}, {3},img_preview,file_name,is_photo) " +
                                    "values ({4}, @file_blob, @img_preview, @file_name, @is_photo)",
                classesOfMetods.getTableInfo(idT).nameSheme, classesOfMetods.getPhotoInfo(idT).namePhotoTable,
                classesOfMetods.getPhotoInfo(idT).namePhotoField, classesOfMetods.getPhotoInfo(idT).namePhotoFile,
                id_obj);


            using (SqlWork sqlCmd = new SqlWork(true))
            {
                sqlCmd.sql = sql;
                var parms = new Params[]
                                {
                                    new Params
                                        {
                                            paramName = "@file_blob",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Bytea,
                                            value = myData
                                        },

                                    new Params
                                        {
                                            paramName = "@img_preview",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Bytea,
                                            value = ms.Length > 0 ? ms.ToArray() : null
                                        },
                                    new Params
                                        {
                                            paramName = "@file_name",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                            value = fileName
                                        },
                                    new Params
                                        {
                                            paramName = "@is_photo",
                                            typeData = NpgsqlTypes.NpgsqlDbType.Boolean,
                                            value = ms.Length > 0
                                        }
                                };
                try
                {
                    sqlCmd.ExecuteNonQuery(parms);
                }
                catch (NpgsqlException ex)
                {
                    throw ex;
                }
            }
        }
        private void clickDeleteFile(Object sender, EventArgs e)
        {
            if (!classesOfMetods.getWriteTable(idT) || _isReadOnly)
            {
                MessageBox.Show(Resources.FormTableData_RightEditable);
                return;
            }
            if (s_pboxes.Count > 0)
            {
                DialogResult dr = MessageBox.Show(Resources.FormTableData_DeleteFileMes,
                    Resources.FormTableData_DeleteFile, MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    foreach (PictureBox pb in s_pboxes)
                    {
                        SqlWork sqlCmd = new SqlWork();
                        sqlCmd.sql = "DELETE FROM " + classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " WHERE id = " + pb.Tag.ToString();//((itemObj)listBox1.SelectedItem).Id_o.ToString();
                        sqlCmd.Execute(true);
                        sqlCmd.Close();
                        Rekod.Classes.workLogFile.writeLogFile("deleted photo --> idT=" + idT.ToString() + " id_obj=" + pb.Tag.ToString(), false, false);
                    }
                    loadPreview(idObj);
                }
            }
        }
        private void clickSaveFile(Object sender, EventArgs e)
        {

            if (s_pboxes.Count == 0)
                return;
            cti.ThreadProgress.ShowWait();

            Byte[] binarybl = null;
            String fileName = "";
            List<string> fileNames = new List<string>();
            List<Byte[]> files = new List<byte[]>();
            try
            {
                foreach (PictureBox pb in s_pboxes)
                {
                    SqlWork sqlCmd = new SqlWork();
                    sqlCmd.sql = "Select file_name from " +
                        classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " where id=" + pb.Tag.ToString();
                    bool have_file_name = true;
                    try
                    {
                        sqlCmd.Execute(false);
                    }
                    catch (NpgsqlException ex)
                    {
                        sqlCmd.Close();
                        if (ex.Code.CompareTo("42703") == 0)//отсутствие колонки в таблице
                        {
                            have_file_name = false;
                        }
                        else
                            throw ex;
                    }
                    if (have_file_name)
                    {
                        if (sqlCmd.CanRead())
                        {
                            if (!string.IsNullOrEmpty(sqlCmd.GetString(0)))
                                fileName = sqlCmd.GetString(0);

                        }
                    }
                    else
                    {
                        fileName = "";
                    }
                    fileNames.Add(fileName);
                    sqlCmd.Close();
                }
            }
            catch
            {
                binarybl = null;

                return;
            }
            finally
            {
                cti.ThreadProgress.Close();
            }
            //if(fileName
            saveFileDialog1.Filter = Resources.FormTableData_FilterFile;
            saveFileDialog1.FilterIndex = (s_pboxes.Count > 1) ? 2 : 1;
            saveFileDialog1.FileName = (s_pboxes.Count > 1) ? Resources.FormTableData_NameFileDefault : fileName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                try
                {
                    cti.ThreadProgress.ShowWait();
                    try
                    {
                        foreach (PictureBox pb in s_pboxes)
                        {
                            SqlWork sqlCmd = new SqlWork();
                            sqlCmd.sql = "Select " + classesOfMetods.getPhotoInfo(idT).namePhotoFile + " from " +
                                classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " where id=" + pb.Tag.ToString();
                            try
                            {
                                sqlCmd.Execute(false);
                            }
                            catch { }


                            if (sqlCmd.CanRead())
                            {
                                binarybl = sqlCmd.GetBytes(0, 0, 0, int.MaxValue);

                                files.Add(binarybl);
                            }
                            sqlCmd.Close();
                        }
                    }
                    catch
                    {
                        binarybl = null;

                        return;
                    }
                    finally
                    {
                    }

                    ////////загрузка данных


                    //frm = new cti.ThreadProgress(true);
                    FileStream FS;
                    BinaryWriter BW;
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (files.Count == 1)
                            FS = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);
                        else if (System.IO.Path.GetFileName(saveFileDialog1.FileName).CompareTo(Resources.FormTableData_NameFileDefault) != 0)
                        {
                            string dir = System.IO.Path.GetDirectoryName(saveFileDialog1.FileName);
                            string fn = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                            string ex = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                            if (ex.CompareTo("") == 0)
                                ex = System.IO.Path.GetExtension(fileNames[i]);
                            FS = new FileStream(dir + "\\" + fn + " " + i.ToString() + ex, FileMode.Create, FileAccess.Write);
                        }
                        else
                        {
                            string dir = System.IO.Path.GetDirectoryName(saveFileDialog1.FileName);
                            if (fileNames[i].CompareTo("") == 0)
                                FS = new FileStream(string.Format("{0}\\{1} {2}.jpg", dir, Resources.FormTableData_File, i), FileMode.Create, FileAccess.Write);
                            else
                                FS = new FileStream(dir + "\\" + fileNames[i], FileMode.Create, FileAccess.Write);
                        }


                        BW = new BinaryWriter(FS);

                        BW.Write(files[i]);

                        BW.Close();
                        FS.Close();
                    }

                }
                catch { }
                finally
                {
                    binarybl = null;
                    cti.ThreadProgress.Close();
                    saveFileDialog1.Filter = "";
                }
            binarybl = null;
        }
        private static Bitmap resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }
        private void ImgDoubleClick(Object sender, EventArgs e)
        {
            cti.ThreadProgress.ShowWait();
            Control currentImg = (Control)sender;
            String idPhoto = currentImg.Tag.ToString();

            Byte[] binarybl = null;
            try
            {
                if (idPhoto.Length > 0)
                {
                    SqlWork sqlCmd = new SqlWork();
                    sqlCmd.sql = "Select " + classesOfMetods.getPhotoInfo(idT).namePhotoFile + ",file_name from " +
                        classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " where id=" + idPhoto;
                    bool have_file_name = true;
                    try
                    {
                        sqlCmd.Execute(false);
                    }
                    catch (NpgsqlException ex)
                    {
                        if (ex.Code.CompareTo("42703") == 0)
                        {//Отсутствует колонка с именем фала file_name, в результате возникла ошибка((
                            //дубль 2)
                            sqlCmd = new SqlWork();
                            sqlCmd.sql = "Select " + classesOfMetods.getPhotoInfo(idT).namePhotoFile + " from " +
                                classesOfMetods.getTableInfo(idT).nameSheme + "." + classesOfMetods.getPhotoInfo(idT).namePhotoTable + " where id=" + idPhoto;
                            sqlCmd.Execute(false);
                            have_file_name = false;
                        }
                        else
                            throw ex;//А нет, не мой косяк =Ъ
                    }
                    if (sqlCmd.CanRead())
                    {
                        binarybl = sqlCmd.GetBytes(0, 0, 0, int.MaxValue);
                        String fileName;
                        if (have_file_name)
                        {
                            fileName = sqlCmd.GetString(1);
                            if (fileName == null)
                                fileName = "temp_jpg.jpg";
                        }
                        else
                            fileName = "temp_jpg.jpg";

                        FileStream FS = new FileStream(Path.GetTempPath() + fileName, FileMode.Create, FileAccess.Write);

                        BinaryWriter BW = new BinaryWriter(FS);

                        BW.Write(binarybl);
                        BW.Close();
                        FS.Close();
                        FS.Dispose();
                        System.Diagnostics.Process.Start(Path.GetTempPath() + fileName);
                    }
                    sqlCmd.Close();
                    binarybl = null;
                }
            }
            catch { }
            finally
            {
                binarybl = null;
                GC.Collect();
                cti.ThreadProgress.Close();
            }
        }
        private void ImgClick(Object sender, EventArgs e)
        {
            if (CtrlPressed())
            {
                s_pboxes.Add(sender as PictureBox);
                (sender as PictureBox).BackColor = Color.Gray;
                start_select = pboxes.IndexOf(sender as PictureBox);
            }
            else if (ShiftPressed())
            {
                foreach (PictureBox pb in s_pboxes)
                    pb.BackColor = Color.White;
                s_pboxes.Clear();
                if (start_select >= 0)
                {
                    int end_select = start_select;
                    end_select = pboxes.IndexOf(sender as PictureBox);
                    if (start_select <= end_select)
                        for (int i = start_select; i <= end_select; i++)
                        {
                            s_pboxes.Add(pboxes[i]);
                            pboxes[i].BackColor = Color.Gray;
                        }
                    else
                        for (int i = end_select; i <= start_select; i++)
                        {
                            s_pboxes.Add(pboxes[i]);
                            pboxes[i].BackColor = Color.Gray;
                        }
                }
                else
                {
                    s_pboxes.Add(sender as PictureBox);
                    (sender as PictureBox).BackColor = Color.Gray;
                    start_select = pboxes.IndexOf(sender as PictureBox);
                }
            }
            else
            {
                foreach (PictureBox pb in s_pboxes)
                    pb.BackColor = Color.White;
                s_pboxes.Clear();
                s_pboxes.Add(sender as PictureBox);
                (sender as PictureBox).BackColor = Color.Gray;
                start_select = pboxes.IndexOf(sender as PictureBox);
            }
            (sender as PictureBox).Focus();
        }
        private string changeTextOfCount(int n)
        {
            return Resources.FormTableData_Files + " " + n;
        }
        private void focusPanel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
                panel3.Focus();

        }
        private PictureBox addPhotoToEnd(int id_img, Byte[] imgPreview, String fileName)
        {
            MemoryStream fs = new MemoryStream();
            PictureBox pb = new PictureBox();

            try
            {
                pb.BorderStyle = BorderStyle.FixedSingle;
                panel3.Controls.Add(pb);
                pb.Left = 5;
                pb.Top = 110 * panel3.Controls.Count - 105;
                pb.Width = 125;
                pb.Height = 105;
                pb.Tag = id_img;
                pb.DoubleClick += new EventHandler(ImgDoubleClick);
                pb.Click += new EventHandler(ImgClick);
                //pb.Tag = pboxes.Count;

                fs = new MemoryStream(imgPreview);
                pb.SizeMode = PictureBoxSizeMode.Zoom;

                Bitmap bt = new Bitmap(fs);
                pb.Image = bt;

                bt = null;
                //bt.Dispose();
            }
            catch
            {
                SHFILEINFO shinfo = new SHFILEINFO();
                IntPtr hImgLarge;
                if (fileName != null)
                    hImgLarge = Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON | Win32.SHGFI_USEFILEATTRIBUTES);
                else
                    hImgLarge = Win32.SHGetFileInfo("sss.jpg", 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON | Win32.SHGFI_USEFILEATTRIBUTES);
                System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);

                Bitmap b = new Bitmap(125, 105);
                Graphics g = Graphics.FromImage((Image)b);
                g.DrawIcon(myIcon, new Rectangle(45, 30, myIcon.Width, myIcon.Height));
                StringFormat strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Center;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                if (fileName != null)
                    g.DrawString(fileName, new Font("Tahoma", 10), Brushes.Black, new RectangleF(0, 60, 125, 50), strFormat);
                else
                {
                    indefiniteFiles++;
                    g.DrawString(Resources.FormTableData_File + " № " + indefiniteFiles.ToString(), new Font("Tahoma", 10), Brushes.Black, new RectangleF(0, 60, 125, 50), strFormat);
                    //pb.DoubleClick += new EventHandler(clickSaveFile);
                }
                g.Dispose();

                pb.Image = (Image)b;
                pb.SizeMode = PictureBoxSizeMode.CenterImage;
                //MessageBox.Show("Ошибка при загрузке картинки из базы.");
            }
            finally
            {

                fs.Close();
                //fs = null;
                fs.Dispose();
                //imgPreview = null;
                GC.Collect();
            }
            return pb;
        }

        void dt_ValueChanged(object sender, EventArgs e)
        {
            //((DateTimePicker)sender).CustomFormat = dateFormat;
            //((DateTimePicker)sender).Format = DateTimePickerFormat.Custom;

            if (((DataColumn)((DateTimePicker)sender).Tag).Type == DataColumn.enType.Date)
            {
                if (((DateTimePicker)sender).CustomFormat == dateFormat)
                    return;
                ((DateTimePicker)sender).CustomFormat = dateFormat;
                ((DataColumn)((DateTimePicker)sender).Tag).Data = ((DateTimePicker)sender).Value;
            }
            else if (((DataColumn)((DateTimePicker)sender).Tag).Type == DataColumn.enType.DateTime)
            {
                if (((DateTimePicker)sender).CustomFormat == dateTimeFormat)
                    return;
                ((DateTimePicker)sender).CustomFormat = dateTimeFormat;
                ((DataColumn)((DateTimePicker)sender).Tag).Data = ((DateTimePicker)sender).Value;
            }
        }

        void dt_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //((DateTimePicker)sender).CustomFormat = "''";
                //((DataColumn)((DateTimePicker)sender).Tag).Data = null;
            }
        }

        private void comboBox_DrawItemGeometry(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index != -1)
            {
                StylesM style = (sender as ComboBox).Items[e.Index] as StylesM;

                Font font = (sender as ComboBox).Font;
                Brush backgroundColor;
                Brush textColor;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    backgroundColor = SystemBrushes.Highlight;
                    textColor = SystemBrushes.HighlightText;
                }
                else
                {
                    backgroundColor = SystemBrushes.Window;
                    textColor = SystemBrushes.WindowText;
                }

                int fontsize = (int)(font.Size);
                int delta = e.Bounds.Height - fontsize;
                int indent = 5;
                int left = indent;
                int imagePadding = 1;

                if (style.Preview != null)
                {
                    e.Graphics.DrawImage(style.Preview,
                                            e.Bounds.X + left,
                                            e.Bounds.Y + imagePadding,
                                            e.Bounds.Height - imagePadding * 2,
                                            e.Bounds.Height - imagePadding * 2);
                }
                left += e.Bounds.Height - imagePadding * 2 + 5;

                String str = style.Name; // +" (" + e.Node.Nodes.Count.ToString() + ")";
                e.Graphics.DrawString(str, font, Brushes.Black, e.Bounds.X + left,
                                      e.Bounds.Y + e.Bounds.Height / 2 - fontsize / 2);

                e.Graphics.Transform.Reset();

                /////
                //if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                //{
                //    this.toolTip2.Show(style.Name, (sender as ComboBox), e.Bounds.Right, e.Bounds.Bottom);
                //}
                //else
                //{
                //    //this.toolTip2.Hide(sender as ComboBox);
                //}
                /////
            }
        }
        //отрисовка по умолчанию
        void comboBox_DrawItem_standart(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0)
                return;
            string text = (sender as ComboBox).GetItemText((sender as ComboBox).Items[e.Index]);

            using (SolidBrush br = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, br, e.Bounds);
            }

            //if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            //{
            //    this.toolTip2.Show(text, (sender as ComboBox), e.Bounds.Right, e.Bounds.Bottom);
            //}
            //else
            //{
            //    //this.toolTip2.Hide((sender as ComboBox));
            //}
            e.DrawFocusRectangle();
        }

        private void hide_ToolTip2(object sender, EventArgs e)
        {
            toolTip2.Hide((sender as ComboBox));
        }
        public void AddDictionary(Rekod.fieldInfo colname, StylesM[] collection, int typeGeom)
        {
            foreach (Control cc in splitContainer1.Panel2.Controls)
            {
                if (cc.Name.CompareTo("col_" + colname.nameDB) == 0)
                {
                    if (cc is TextBox)
                    {
                        loadBrushList();
                        loadPenList();

                        (splitContainer1.Panel2.Controls["pb_" + colname.nameDB] as PictureBox).Image = (Image)Rekod.Properties.Resources.reference;
                        toolTip2.SetToolTip((splitContainer1.Panel2.Controls["pb_" + colname.nameDB] as PictureBox), Resources.FormTableData_Reference);

                        ComboBox cb = new ComboBox();

                        //if(comboBoxExtendetStyles)
                        cb.DrawMode = DrawMode.OwnerDrawVariable;
                        cb.DropDownClosed += new EventHandler(hide_ToolTip2);
                        cb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                        if (colname.is_style)
                            cb.ItemHeight = 36;

                        cb.MaxDropDownItems = 10;
                        cb.Location = cc.Location;
                        cb.Size = new Size(cc.Size.Width - 25, cc.Size.Height);
                        //cb.Size = cc.Size;
                        cb.Width = splitContainer1.Panel2.Width - (49 + 25);
                        cb.DropDownStyle = ComboBoxStyle.DropDownList;
                        cb.Tag = cc.Tag;
                        cb.Enabled = cc.Enabled && !(cc as TextBox).ReadOnly;

                        var button1 = new Button();
                        button1.BackgroundImage = global::Rekod.Properties.Resources.delete;
                        button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                        button1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                        button1.Location = new Point(cb.Right, cb.Top);
                        button1.Name = "pbox" + colname.nameDB;
                        button1.Size = new System.Drawing.Size(cc.Size.Height, cc.Size.Height);
                        button1.Tag = cb;
                        button1.Click += btnClearSelectedValueOfCatalog_Click;
                        button1.UseVisualStyleBackColor = true;
                        button1.Enabled = cc.Enabled && !(cc as TextBox).ReadOnly;
                        splitContainer1.Panel2.Controls.Add(button1);
                        splitContainer1.Panel2.Controls.SetChildIndex(button1, 0);


                        //Button but = new Button();
                        //but.Tag = cb;
                        //but.Location = new Point(cb.Right, cb.Top);
                        //but.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                        //splitContainer1.Panel2.Controls.Add(but);
                        //but.BringToFront();
                        //but.Click += btnClearSelectedValueOfCatalog_Click;
                        //but.Size = new Size(20, 20);
                        //but.BackgroundImage = global::Rekod.Properties.Resources.delete;
                        //but.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;

                        if (colname.is_style)
                        {
                            cb.DrawItem += new DrawItemEventHandler(comboBox_DrawItemGeometry);
                        }
                        else
                        {
                            cb.DrawItem += new DrawItemEventHandler(comboBox_DrawItem_standart);
                        }

                        cb.MouseDown += new MouseEventHandler(comboBox_MouseDown);
                        cb.Name = "col_" + colname.nameDB;
                        cb.TabIndex = cc.TabIndex;
                        splitContainer1.Panel2.Controls.Remove(cc);
                        cc.Dispose();
                        splitContainer1.Panel2.Controls.Add(cb);
                        cb.Items.AddRange(collection);

                        if (colname.is_style)//смещаем все компоненты, которые стоят ниже, вниз))
                        {
                            foreach (Control cccc in splitContainer1.Panel2.Controls)
                            {
                                if (cccc.Name.CompareTo("lbl1_" + colname.nameDB) == 0 || cccc.Name.CompareTo("pb_" + colname.nameDB) == 0)
                                    cccc.Top += 10;
                            }
                            Point loc = (Point)wrkPanel.Tag;
                            wrkPanel.Tag = new Point(loc.X, loc.Y + 23);
                        }
                        if (defRefValue != null)
                        {
                            if (defRefValue.nameField == colname.nameDB)
                            {
                                foreach (object oo in cb.Items)
                                {
                                    if (((StylesM)oo).Id == defRefValue.idObj)
                                    {
                                        cb.SelectedIndex = cb.Items.IndexOf(oo);
                                        break;
                                    }

                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        void btnClearSelectedValueOfCatalog_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ComboBox cbox = button.Tag as ComboBox;
            cbox.SelectedItem = null;
        }

        public void AddTable(string table, string colname, object value, object showValue, int id_table)
        {
            foreach (Control cc in splitContainer1.Panel2.Controls)
            {
                if (cc.Name.CompareTo("col_" + colname) == 0)
                {
                    if (cc is TextBox)
                    {
                        bool as_default = false;
                        if (isNew1)
                            if (value__ != "")
                                as_default = true;
                        TextBox tb = (TextBox)cc;

                        if (!as_default)
                            tb.Size = new Size(tb.Size.Width - 25, tb.Size.Height);
                        tb.BackColor = Color.White;
                        tb.TextChanged -= textBoxChangedInt;
                        tb.TextChanged -= textBoxChangedReal;
                        tb.MouseUp -= textBoxMouseUp;
                        tb.KeyUp -= textBoxKeyUp;
                        if (as_default)
                        {
                            tb.Text = getNewValueForRefTable(idT, tb, int.Parse(value__));
                        }
                        else
                            if (value != null)
                            {
                                tb.Text = "(" + value + ")- " + showValue;
                                if (showValue == null)
                                    tb.BackColor = Color.FromArgb(255, 204, 204);
                            }

                        //tb.Enabled = false;
                        //tb.AppendText(showValue);
                        //toolTip1.SetToolTip(tb, "(" + showValue + ")- " + value);
                        tb.ReadOnly = true;

                        (splitContainer1.Panel2.Controls["pb_" + colname] as PictureBox).Image = (Image)Rekod.Properties.Resources.reference;
                        //toolTip2.SetToolTip((splitContainer1.Panel2.Controls["pb_" + colname] as PictureBox), Resources.FormTableData_TableData);

                        if (!as_default)
                        {
                            var button1 = new Button();
                            button1.BackgroundImage = global::Rekod.Properties.Resources.delete;
                            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                            button1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                            button1.Location = new Point(tb.Right - 19, tb.Top + 1);
                            button1.FlatAppearance.BorderSize = 0;
                            button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                            button1.Name = "pbox" + colname;
                            button1.Size = new System.Drawing.Size(18, 18);
                            button1.UseVisualStyleBackColor = true;
                            button1.Tag = id_table;
                            button1.Click += new EventHandler(btnClearText_ref_Click);
                            splitContainer1.Panel2.Controls.Add(button1);
                            splitContainer1.Panel2.Controls.SetChildIndex(button1, 0);

                            //var pictureBox1 = new System.Windows.Forms.PictureBox();
                            //pictureBox1.BackgroundImage = global::Rekod.Properties.Resources.delete;
                            //pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                            //pictureBox1.Name = "pbox_" + colname;
                            //pictureBox1.Location = new Point(tb.Right - 18, tb.Top + 2);
                            //pictureBox1.Size = new System.Drawing.Size(16, 16);
                            //pictureBox1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                            //pictureBox1.TabStop = false;
                            //splitContainer1.Panel2.Controls.Add(pictureBox1);
                            //splitContainer1.Panel2.Controls.SetChildIndex(pictureBox1, 0);

                            Button but = new Button();
                            but.Name = "tbut_" + colname;
                            but.Tag = id_table;
                            but.Location = new Point(tb.Right, tb.Top);
                            but.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                            splitContainer1.Panel2.Controls.Add(but);
                            but.BringToFront();
                            but.Click += new EventHandler(tbut_click);
                            but.Size = new Size(cc.Size.Height, cc.Size.Height);
                            but.Text = "...";
                        }

                        break;
                    }
                }
            }
        }

        void btnClearText_ref_Click(object sender, EventArgs e)
        {
            if (!(sender is Button))
                return;
            int table = (int)(sender as Button).Tag;

            int id_obj = -1;
            string tx_name = "col_" + (sender as Button).Name.Substring(4);
            TextBox ccc = splitContainer1.Panel2.Controls[tx_name] as TextBox;
            if (ccc == null)
                return;
            ((DataColumn)ccc.Tag).Data = null;
            ccc.Text = "";
            ccc.BackColor = Color.White;
        }
        public void AddInterval(string colname, ValueInterval[] collection, int idT)
        {
            foreach (Control cc in splitContainer1.Panel2.Controls)
            {
                if (cc.Name.CompareTo("col_" + colname) == 0)
                {
                    if (cc is TextBox)
                    {
                        DataColumn dt = (DataColumn)cc.Tag;
                        dt.isInterval = true;
                        dt.Interval = collection;
                        dt.RelTableId = idT;
                        break;
                    }
                }
            }
            foreach (Control cc in splitContainer1.Panel1.Controls)
            {
                if (cc.Name.CompareTo("lbl1_" + colname) == 0)
                {
                    if (cc is Label)
                    {
                        Label ll = cc as Label;
                        ll.Tag = idT;
                        ll.Click += new EventHandler(labelIntervalClick);
                        ll.Cursor = Cursors.Hand;
                        ll.ForeColor = Color.FromArgb(14, 100, 220);
                        ll.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Underline);
                    }
                }
            }
        }
        void tbut_click(object sender, EventArgs e)
        {
            if (!(sender is Button))
                return;
            int table = (int)(sender as Button).Tag;

            int id_obj = -1;
            string tx_name = "col_" + (sender as Button).Name.Substring(5);
            Control ccc = splitContainer1.Panel2.Controls[tx_name];
            try
            {
                if (((DataColumn)ccc.Tag).Data != null)
                {
                    id_obj = ExtraFunctions.Converts.To<int>(((DataColumn)ccc.Tag).Data);
                }
                else
                {
                    id_obj = -1;
                }
            }
            catch
            {
                id_obj = -1;
            }
            Rekod.fieldInfo fi = classesOfMetods.getFieldInfo(
                classesOfMetods.getFieldInfoTable(idT).FindAll(
                                   w => w.ref_table == table).Find(
                                   w => w.nameDB == ((DataColumn)ccc.Tag).BaseName).ref_field.Value);
            //selectValueFromTable frm = new selectValueFromTable(table, id_obj);

            tablesInfo ti = classesOfMetods.getTableInfo(table);
            if (ti.pkField != fi.nameDB)
            {
                if (id_obj >= 0)
                {
                    var obj = classesOfMetods.GetPKValue(ti, fi, id_obj);
                    id_obj = obj != null ? obj.Value : -1;
                }
            }
            var frm = new itemsTableGridForm(table, goToObject: id_obj, isSelected: true, callerTableId: IdTable, callerObjectId: IdObj);
            if (frm.ShowDialog() == DialogResult.OK && frm.idObj != -1)
            {
                if (ccc is TextBox)
                {
                    (ccc as TextBox).Text = getNewValueForRefTable(idT, (ccc as TextBox), frm.idObj);
                    (ccc as TextBox).BackColor = Color.White;
                }
            }
        }
        string getNewValueForRefTable(int id_table, TextBox ccc, int new_id_ref)
        {
            string name_col = ((DataColumn)ccc.Tag).BaseName;
            int id_ref_table = 0, id_ref_field = 0, id_ref_field_name = 0;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT ref_table, ref_field, ref_field_name FROM " + Program.scheme + ".table_field_info WHERE id_table=" + idT.ToString()
                + " AND name_db='" + name_col + "'";
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                id_ref_table = sqlCmd.GetInt32("ref_table");
                id_ref_field = sqlCmd.GetInt32("ref_field");
                id_ref_field_name = sqlCmd.GetInt32("ref_field_name");
            }
            sqlCmd.Close();
            string table_name = "", scheme_name = "", pk_field_name = "";
            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT name_db, scheme_name, pk_fileld FROM " + Program.scheme + ".table_info WHERE id=" + id_ref_table.ToString();
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                table_name = sqlCmd.GetString("name_db");
                scheme_name = sqlCmd.GetString("scheme_name");
                pk_field_name = sqlCmd.GetString("pk_fileld");
            }
            sqlCmd.Close();

            string name_db = "", name_db_show = "";
            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT name_db FROM " + Program.scheme + ".table_field_info WHERE id=" + id_ref_field.ToString();
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                name_db = sqlCmd.GetString(0);
            }
            sqlCmd.Close();
            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT name_db FROM " + Program.scheme + ".table_field_info WHERE id=" + id_ref_field_name.ToString();
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                name_db_show = sqlCmd.GetString(0);
            }
            sqlCmd.Close();


            double? new_value = null;
            string new_show_value = "";
            sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT " + name_db + "," + name_db_show + " FROM " + scheme_name + "." + table_name + " WHERE " + pk_field_name + "=" + new_id_ref;
            sqlCmd.Execute(false);
            while (sqlCmd.CanRead())
            {
                new_value = sqlCmd.GetValue<double?>(name_db);
                new_show_value = sqlCmd.GetValue<string>(name_db_show);
            }
            sqlCmd.Close();
            ((DataColumn)ccc.Tag).Data = new_value;
            return "(" + new_value + ")- " + new_show_value;
        }
        void labelIntervalClick(object sender, EventArgs e)
        {
            if (sender is Label)
            {
                int idT = (int)(sender as Label).Tag;
                itemsTableGridForm frm = new itemsTableGridForm(idT);
                foreach (tablesInfo ti in Program.tables_info)
                {
                    if (ti.idTable == idT)
                    {
                        frm.Text = string.Format(Resources.FormTableData_DataTable, ti.nameMap);
                        break;
                    }
                }
                frm.Show();
                frm.Activate();
            }
        }
        public void loadDataToCol(string NameCol, object Data)
        {
            foreach (Control cc in splitContainer1.Panel2.Controls)
            {
                if (cc.Name.CompareTo("col_" + NameCol) == 0)
                {
                    if (cc is TextBox)
                    {
                        TextBox tx = (TextBox)cc;
                        if (String.IsNullOrEmpty(tx.Text))
                        {
                            tx.Text = Convert.ToString(Data);
                        }
                        ((DataColumn)tx.Tag).Data = Data;
                        break;
                    }
                    else if (cc is DateTimePicker)
                    {
                        DateTimePicker tx = (DateTimePicker)cc;
                        if (Data == null)
                        {
                            //tx.CustomFormat = "''";
                            tx.Checked = false;
                        }
                        else
                        {
                            tx.Checked = true;
                            tx.Value = (DateTime)Data;
                        }
                        ((DataColumn)tx.Tag).Data = Data;
                        break;
                    }
                    else if (cc is ComboBox)
                    {
                        ComboBox tx = (ComboBox)cc;
                        if (tx.Items.Count > 0)
                        {
                            foreach (object obj in tx.Items)
                            {
                                StylesM dic1 = (StylesM)obj;
                                int? k1 = ExtraFunctions.Converts.To<int?>(Data);
                                if (Data == null)
                                {
                                    ((DataColumn)tx.Tag).Data = null;
                                    break;
                                }
                                else
                                {
                                    if (dic1.Id == k1)
                                    {
                                        tx.SelectedIndex = tx.Items.IndexOf(dic1);
                                        ((DataColumn)tx.Tag).Data = Data;
                                        break;
                                    }
                                }

                            }
                        }
                        break;
                    }
                }
            }
        }

        private void FormTableData_Load(object sender, EventArgs e)
        {
            //cti.ThreadProgress.ShowWait();
            if (data_ != null)
            {
                try
                {
                    data_.Load();
                }
                catch (Exception x)
                {
                    //cti.ThreadProgress.Close();
                    this.Close();
                    MessageBox.Show(x.Message, Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            this.Refresh();
            this.Activate();
            this.Width += 2;


            Program.ReportModel.ListReports.CollectionChanged += (s, arg) =>
                {
                    MenuReloadReports();
                };
            MenuReloadReports();

            if (!Program.user_info.admin)
            {
                toolStripSeparator1.Visible = false;
                открытьМенеджерОтчетовToolStripMenuItem.Visible = false;
            }
            //cti.ThreadProgress.Close();
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }
        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] lpKeyState);
        private bool CtrlPressed()
        {
            byte[] keyboardState = new byte[255];
            GetKeyboardState(keyboardState);
            return keyboardState[17] == 128 || keyboardState[17] == 129;
        }
        private bool ShiftPressed()
        {
            byte[] keyboardState = new byte[255];
            GetKeyboardState(keyboardState);
            return keyboardState[16] == 128 || keyboardState[16] == 129;
        }
        private void FormTableData_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UControl != null)
            {
                UControl.CloseForm -= new EventHandler<Interfaces.UserControls.eventCloseForm>(s_CloseForm);
                this.Controls.Clear();
            }
        }
        private void loadPenList()
        {
            if (LargeImageList.Count != 0)
                return;
            LargeImageList.Add("1", (Image)global::Rekod.lineRes.LINE01);
            LargeImageList.Add("2", (Image)global::Rekod.lineRes.LINE02);
            LargeImageList.Add("3", (Image)global::Rekod.lineRes.LINE03);
            LargeImageList.Add("4", (Image)global::Rekod.lineRes.LINE04);
            LargeImageList.Add("5", (Image)global::Rekod.lineRes.LINE05);
            LargeImageList.Add("6", (Image)global::Rekod.lineRes.LINE06);
            LargeImageList.Add("7", (Image)global::Rekod.lineRes.LINE07);
            LargeImageList.Add("8", (Image)global::Rekod.lineRes.LINE08);
            LargeImageList.Add("9", (Image)global::Rekod.lineRes.LINE09);
            LargeImageList.Add("10", (Image)global::Rekod.lineRes.LINE10);
            LargeImageList.Add("11", (Image)global::Rekod.lineRes.LINE11);
            LargeImageList.Add("12", (Image)global::Rekod.lineRes.LINE12);
            LargeImageList.Add("13", (Image)global::Rekod.lineRes.LINE13);
            LargeImageList.Add("14", (Image)global::Rekod.lineRes.LINE14);
            LargeImageList.Add("15", (Image)global::Rekod.lineRes.LINE15);
            LargeImageList.Add("16", (Image)global::Rekod.lineRes.LINE16);
            LargeImageList.Add("17", (Image)global::Rekod.lineRes.LINE17);
            LargeImageList.Add("18", (Image)global::Rekod.lineRes.LINE18);
            LargeImageList.Add("19", (Image)global::Rekod.lineRes.LINE19);
            LargeImageList.Add("20", (Image)global::Rekod.lineRes.LINE20);
            LargeImageList.Add("21", (Image)global::Rekod.lineRes.LINE21);
            LargeImageList.Add("22", (Image)global::Rekod.lineRes.LINE22);
            LargeImageList.Add("23", (Image)global::Rekod.lineRes.LINE23);
            LargeImageList.Add("24", (Image)global::Rekod.lineRes.LINE24);
            LargeImageList.Add("25", (Image)global::Rekod.lineRes.LINE25);
        }
        private void loadBrushList()
        {
            if (LargeImageList2.Count != 0)
                return;
            LargeImageList2.Add("1", (Image)global::Rekod.RegionRes.POLY001);
            LargeImageList2.Add("2", (Image)global::Rekod.RegionRes.POLY002);
            LargeImageList2.Add("3", (Image)global::Rekod.RegionRes.POLY003);
            LargeImageList2.Add("4", (Image)global::Rekod.RegionRes.POLY004);
            LargeImageList2.Add("5", (Image)global::Rekod.RegionRes.POLY005);
            LargeImageList2.Add("6", (Image)global::Rekod.RegionRes.POLY006);
            LargeImageList2.Add("7", (Image)global::Rekod.RegionRes.POLY007);
            LargeImageList2.Add("8", (Image)global::Rekod.RegionRes.POLY008);
            LargeImageList2.Add("9", (Image)global::Rekod.RegionRes.POLY009);
            LargeImageList2.Add("10", (Image)global::Rekod.RegionRes.POLY010);
            LargeImageList2.Add("11", (Image)global::Rekod.RegionRes.POLY011);
            LargeImageList2.Add("12", (Image)global::Rekod.RegionRes.POLY012);
            LargeImageList2.Add("13", (Image)global::Rekod.RegionRes.POLY013);
            LargeImageList2.Add("14", (Image)global::Rekod.RegionRes.POLY014);
            LargeImageList2.Add("15", (Image)global::Rekod.RegionRes.POLY015);
            LargeImageList2.Add("16", (Image)global::Rekod.RegionRes.POLY016);
            LargeImageList2.Add("17", (Image)global::Rekod.RegionRes.POLY017);
            LargeImageList2.Add("18", (Image)global::Rekod.RegionRes.POLY018);
            LargeImageList2.Add("19", (Image)global::Rekod.RegionRes.POLY019);
            LargeImageList2.Add("20", (Image)global::Rekod.RegionRes.POLY020);
            LargeImageList2.Add("21", (Image)global::Rekod.RegionRes.POLY021);
            LargeImageList2.Add("22", (Image)global::Rekod.RegionRes.POLY022);
            LargeImageList2.Add("23", (Image)global::Rekod.RegionRes.POLY023);
            LargeImageList2.Add("24", (Image)global::Rekod.RegionRes.POLY024);
            LargeImageList2.Add("25", (Image)global::Rekod.RegionRes.POLY025);
            LargeImageList2.Add("26", (Image)global::Rekod.RegionRes.POLY026);
            LargeImageList2.Add("27", (Image)global::Rekod.RegionRes.POLY027);
            LargeImageList2.Add("28", (Image)global::Rekod.RegionRes.POLY028);
            LargeImageList2.Add("29", (Image)global::Rekod.RegionRes.POLY029);
            LargeImageList2.Add("30", (Image)global::Rekod.RegionRes.POLY030);
            LargeImageList2.Add("31", (Image)global::Rekod.RegionRes.POLY031);
            LargeImageList2.Add("32", (Image)global::Rekod.RegionRes.POLY032);
            LargeImageList2.Add("33", (Image)global::Rekod.RegionRes.POLY033);
            LargeImageList2.Add("34", (Image)global::Rekod.RegionRes.POLY034);
            LargeImageList2.Add("35", (Image)global::Rekod.RegionRes.POLY035);
            LargeImageList2.Add("36", (Image)global::Rekod.RegionRes.POLY036);
            LargeImageList2.Add("37", (Image)global::Rekod.RegionRes.POLY037);
            LargeImageList2.Add("38", (Image)global::Rekod.RegionRes.POLY038);
            LargeImageList2.Add("39", (Image)global::Rekod.RegionRes.POLY039);
            LargeImageList2.Add("40", (Image)global::Rekod.RegionRes.POLY040);
            LargeImageList2.Add("41", (Image)global::Rekod.RegionRes.POLY041);
            LargeImageList2.Add("42", (Image)global::Rekod.RegionRes.POLY042);
            LargeImageList2.Add("43", (Image)global::Rekod.RegionRes.POLY043);
            LargeImageList2.Add("44", (Image)global::Rekod.RegionRes.POLY044);
            LargeImageList2.Add("45", (Image)global::Rekod.RegionRes.POLY045);
            LargeImageList2.Add("46", (Image)global::Rekod.RegionRes.POLY046);
            LargeImageList2.Add("47", (Image)global::Rekod.RegionRes.POLY047);
            LargeImageList2.Add("48", (Image)global::Rekod.RegionRes.POLY048);
            LargeImageList2.Add("49", (Image)global::Rekod.RegionRes.POLY049);
            LargeImageList2.Add("50", (Image)global::Rekod.RegionRes.POLY050);
            LargeImageList2.Add("51", (Image)global::Rekod.RegionRes.POLY051);
            LargeImageList2.Add("52", (Image)global::Rekod.RegionRes.POLY052);
            LargeImageList2.Add("53", (Image)global::Rekod.RegionRes.POLY053);
            LargeImageList2.Add("54", (Image)global::Rekod.RegionRes.POLY054);
            LargeImageList2.Add("55", (Image)global::Rekod.RegionRes.POLY055);
            LargeImageList2.Add("56", (Image)global::Rekod.RegionRes.POLY056);
            LargeImageList2.Add("57", (Image)global::Rekod.RegionRes.POLY057);
            LargeImageList2.Add("58", (Image)global::Rekod.RegionRes.POLY058);
            LargeImageList2.Add("59", (Image)global::Rekod.RegionRes.POLY059);
            LargeImageList2.Add("60", (Image)global::Rekod.RegionRes.POLY060);
            LargeImageList2.Add("61", (Image)global::Rekod.RegionRes.POLY061);
            LargeImageList2.Add("62", (Image)global::Rekod.RegionRes.POLY062);
            LargeImageList2.Add("63", (Image)global::Rekod.RegionRes.POLY063);
            LargeImageList2.Add("64", (Image)global::Rekod.RegionRes.POLY064);
            LargeImageList2.Add("65", (Image)global::Rekod.RegionRes.POLY065);
            LargeImageList2.Add("66", (Image)global::Rekod.RegionRes.POLY066);
            LargeImageList2.Add("67", (Image)global::Rekod.RegionRes.POLY067);
            LargeImageList2.Add("68", (Image)global::Rekod.RegionRes.POLY068);
            LargeImageList2.Add("69", (Image)global::Rekod.RegionRes.POLY069);
            LargeImageList2.Add("70", (Image)global::Rekod.RegionRes.POLY070);
            LargeImageList2.Add("71", (Image)global::Rekod.RegionRes.POLY071);
            LargeImageList2.Add("72", (Image)global::Rekod.RegionRes.POLY072);
            LargeImageList2.Add("73", (Image)global::Rekod.RegionRes.POLY073);
            LargeImageList2.Add("74", (Image)global::Rekod.RegionRes.POLY074);
            LargeImageList2.Add("75", (Image)global::Rekod.RegionRes.POLY075);
            LargeImageList2.Add("76", (Image)global::Rekod.RegionRes.POLY076);
            LargeImageList2.Add("77", (Image)global::Rekod.RegionRes.POLY077);
            LargeImageList2.Add("78", (Image)global::Rekod.RegionRes.POLY078);
            LargeImageList2.Add("79", (Image)global::Rekod.RegionRes.POLY079);
            LargeImageList2.Add("80", (Image)global::Rekod.RegionRes.POLY080);
            LargeImageList2.Add("81", (Image)global::Rekod.RegionRes.POLY081);
            LargeImageList2.Add("82", (Image)global::Rekod.RegionRes.POLY082);
            LargeImageList2.Add("83", (Image)global::Rekod.RegionRes.POLY083);
            LargeImageList2.Add("84", (Image)global::Rekod.RegionRes.POLY084);
            LargeImageList2.Add("85", (Image)global::Rekod.RegionRes.POLY085);
            LargeImageList2.Add("86", (Image)global::Rekod.RegionRes.POLY086);
            LargeImageList2.Add("87", (Image)global::Rekod.RegionRes.POLY087);
            LargeImageList2.Add("88", (Image)global::Rekod.RegionRes.POLY088);
            LargeImageList2.Add("89", (Image)global::Rekod.RegionRes.POLY089);
            LargeImageList2.Add("90", (Image)global::Rekod.RegionRes.POLY090);
            LargeImageList2.Add("91", (Image)global::Rekod.RegionRes.POLY091);
            LargeImageList2.Add("92", (Image)global::Rekod.RegionRes.POLY092);
            LargeImageList2.Add("93", (Image)global::Rekod.RegionRes.POLY093);
            LargeImageList2.Add("94", (Image)global::Rekod.RegionRes.POLY094);
            LargeImageList2.Add("95", (Image)global::Rekod.RegionRes.POLY095);
            LargeImageList2.Add("96", (Image)global::Rekod.RegionRes.POLY096);
            LargeImageList2.Add("97", (Image)global::Rekod.RegionRes.POLY097);
            LargeImageList2.Add("98", (Image)global::Rekod.RegionRes.POLY098);
            LargeImageList2.Add("99", (Image)global::Rekod.RegionRes.POLY099);
            LargeImageList2.Add("100", (Image)global::Rekod.RegionRes.POLY100);
            LargeImageList2.Add("101", (Image)global::Rekod.RegionRes.POLY101);
            LargeImageList2.Add("102", (Image)global::Rekod.RegionRes.POLY102);
            LargeImageList2.Add("103", (Image)global::Rekod.RegionRes.POLY103);
            LargeImageList2.Add("104", (Image)global::Rekod.RegionRes.POLY104);
            LargeImageList2.Add("105", (Image)global::Rekod.RegionRes.POLY105);
            LargeImageList2.Add("106", (Image)global::Rekod.RegionRes.POLY106);
            LargeImageList2.Add("107", (Image)global::Rekod.RegionRes.POLY107);
            LargeImageList2.Add("108", (Image)global::Rekod.RegionRes.POLY108);
            LargeImageList2.Add("109", (Image)global::Rekod.RegionRes.POLY109);
            LargeImageList2.Add("110", (Image)global::Rekod.RegionRes.POLY110);
            LargeImageList2.Add("111", (Image)global::Rekod.RegionRes.POLY111);
            LargeImageList2.Add("112", (Image)global::Rekod.RegionRes.POLY112);
            LargeImageList2.Add("113", (Image)global::Rekod.RegionRes.POLY113);
            LargeImageList2.Add("114", (Image)global::Rekod.RegionRes.POLY114);
            LargeImageList2.Add("115", (Image)global::Rekod.RegionRes.POLY115);
            LargeImageList2.Add("116", (Image)global::Rekod.RegionRes.POLY116);
            LargeImageList2.Add("117", (Image)global::Rekod.RegionRes.POLY117);
            LargeImageList2.Add("118", (Image)global::Rekod.RegionRes.POLY118);
            LargeImageList2.Add("119", (Image)global::Rekod.RegionRes.POLY119);
            LargeImageList2.Add("120", (Image)global::Rekod.RegionRes.POLY120);
            LargeImageList2.Add("121", (Image)global::Rekod.RegionRes.POLY121);
            LargeImageList2.Add("122", (Image)global::Rekod.RegionRes.POLY122);
            LargeImageList2.Add("123", (Image)global::Rekod.RegionRes.POLY123);
            LargeImageList2.Add("124", (Image)global::Rekod.RegionRes.POLY124);
            LargeImageList2.Add("125", (Image)global::Rekod.RegionRes.POLY125);
            LargeImageList2.Add("126", (Image)global::Rekod.RegionRes.POLY126);
            LargeImageList2.Add("127", (Image)global::Rekod.RegionRes.POLY127);
            LargeImageList2.Add("128", (Image)global::Rekod.RegionRes.POLY128);
            LargeImageList2.Add("129", (Image)global::Rekod.RegionRes.POLY129);
            LargeImageList2.Add("130", (Image)global::Rekod.RegionRes.POLY130);
            LargeImageList2.Add("131", (Image)global::Rekod.RegionRes.POLY131);
            LargeImageList2.Add("132", (Image)global::Rekod.RegionRes.POLY132);
            LargeImageList2.Add("133", (Image)global::Rekod.RegionRes.POLY133);
            LargeImageList2.Add("134", (Image)global::Rekod.RegionRes.POLY134);
            LargeImageList2.Add("135", (Image)global::Rekod.RegionRes.POLY135);
            LargeImageList2.Add("136", (Image)global::Rekod.RegionRes.POLY136);
            LargeImageList2.Add("137", (Image)global::Rekod.RegionRes.POLY137);
            LargeImageList2.Add("138", (Image)global::Rekod.RegionRes.POLY138);
            LargeImageList2.Add("139", (Image)global::Rekod.RegionRes.POLY139);
            LargeImageList2.Add("140", (Image)global::Rekod.RegionRes.POLY140);
            LargeImageList2.Add("141", (Image)global::Rekod.RegionRes.POLY141);
            LargeImageList2.Add("142", (Image)global::Rekod.RegionRes.POLY142);
            LargeImageList2.Add("143", (Image)global::Rekod.RegionRes.POLY143);
            LargeImageList2.Add("144", (Image)global::Rekod.RegionRes.POLY144);
            LargeImageList2.Add("145", (Image)global::Rekod.RegionRes.POLY145);
            LargeImageList2.Add("146", (Image)global::Rekod.RegionRes.POLY146);
            LargeImageList2.Add("147", (Image)global::Rekod.RegionRes.POLY147);
            LargeImageList2.Add("148", (Image)global::Rekod.RegionRes.POLY148);
            LargeImageList2.Add("149", (Image)global::Rekod.RegionRes.POLY149);
            LargeImageList2.Add("150", (Image)global::Rekod.RegionRes.POLY150);
            LargeImageList2.Add("151", (Image)global::Rekod.RegionRes.POLY151);
            LargeImageList2.Add("152", (Image)global::Rekod.RegionRes.POLY152);
            LargeImageList2.Add("153", (Image)global::Rekod.RegionRes.POLY153);
            LargeImageList2.Add("154", (Image)global::Rekod.RegionRes.POLY154);
            LargeImageList2.Add("155", (Image)global::Rekod.RegionRes.POLY155);
            LargeImageList2.Add("156", (Image)global::Rekod.RegionRes.POLY156);
            LargeImageList2.Add("157", (Image)global::Rekod.RegionRes.POLY157);
            LargeImageList2.Add("158", (Image)global::Rekod.RegionRes.POLY158);
            LargeImageList2.Add("159", (Image)global::Rekod.RegionRes.POLY159);
            LargeImageList2.Add("160", (Image)global::Rekod.RegionRes.POLY160);
            LargeImageList2.Add("161", (Image)global::Rekod.RegionRes.POLY161);
            LargeImageList2.Add("162", (Image)global::Rekod.RegionRes.POLY162);
            LargeImageList2.Add("163", (Image)global::Rekod.RegionRes.POLY163);
            LargeImageList2.Add("164", (Image)global::Rekod.RegionRes.POLY164);
            LargeImageList2.Add("165", (Image)global::Rekod.RegionRes.POLY165);
            LargeImageList2.Add("166", (Image)global::Rekod.RegionRes.POLY166);
            LargeImageList2.Add("167", (Image)global::Rekod.RegionRes.POLY167);
            LargeImageList2.Add("168", (Image)global::Rekod.RegionRes.POLY168);
            LargeImageList2.Add("169", (Image)global::Rekod.RegionRes.POLY169);
            LargeImageList2.Add("170", (Image)global::Rekod.RegionRes.POLY170);
            LargeImageList2.Add("171", (Image)global::Rekod.RegionRes.POLY171);
            LargeImageList2.Add("172", (Image)global::Rekod.RegionRes.POLY172);
        }

        private void FormTableData_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_geometryVM != null)
            {
                _geometryVM.HidePreview();
                _geometryVM.HidePoints();
            }
            if (ActionResult != null)
                ActionResult.Invoke(DialogResult);
            if (_isLayer && isNew && DialogResult != System.Windows.Forms.DialogResult.Cancel)
            {
                classesOfMetods.reloadAllLayerData(idT);
            }
            Program.app.mapLib.Focus();
            Program.BManager.SetButtonsState();
        }
        private void comboBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ((ComboBox)sender).SelectedIndex = -1;
            }
        }

        private void экспортВToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_geometryVM != null)
            {
                _geometryVM.Export();
            }
        }
        private void импортИзToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_geometryVM != null)
            {
                _geometryVM.Import();
            }
        }
        private void историяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PgTableBaseM pgTable = Program.repository.FindTable(IdTable) as PgTableBaseM;
            PgHistoryVM pgHistVM = new PgHistoryVM(Program.repository, table: pgTable, idObject: IdObj);
            PgHistoryV pgHistV = new PgHistoryV(pgHistVM);
            pgHistV.Owner = Program.WinMain;
            pgHistV.Height = 350;
            pgHistV.Width = 900;
            pgHistV.Show();
        }
        private void показатьОбъектВОкнеДанныеТаблицыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openTable = new itemsTableGridForm(idT, idObj);
            openTable.Show();
        }
        private void MenuReloadReports()
        {
            if (isNew)
            {
                отчетыToolStripMenuItem.Visible = false;
                return;
            }
            отчетыToolStripMenuItem.DropDownItems.Remove(toolStripSeparator1);
            отчетыToolStripMenuItem.DropDownItems.Remove(открытьМенеджерОтчетовToolStripMenuItem);
            отчетыToolStripMenuItem.DropDownItems.Clear();

            var listReports = Program.ReportModel.FindReportsByIdTable(idT);
            foreach (var item in listReports)
            {
                switch (item.Type)
                {
                    case enTypeReport.Object:
                        {
                            CreateItemReport(отчетыToolStripMenuItem, item);
                        }
                        break;
                    default:
                        break;
                }
            }
            if (отчетыToolStripMenuItem.DropDownItems.Count > 0)
                отчетыToolStripMenuItem.DropDownItems.Add(toolStripSeparator1);
            отчетыToolStripMenuItem.DropDownItems.Add(открытьМенеджерОтчетовToolStripMenuItem);
        }
        private void CreateItemReport(ToolStripMenuItem tsItemMenu, IReportItem_M item)
        {
            var tsItem = new ToolStripMenuItem(item.Caption);
            tsItem.Tag = item;

            switch (item.Type)
            {
                case enTypeReport.Object:
                    {
                        tsItem.Click += tsItemOpenReportObject_Click;
                    }
                    break;
                default:
                    break;
            }
            tsItemMenu.DropDownItems.Add(tsItem);
        }
        void tsItemOpenReportObject_Click(object sender, EventArgs e)
        {
            var tsItemObject = sender as ToolStripItem;
            var report = tsItemObject.Tag as ReportItem_M;

            try
            {
                if (IdObj == -1)
                    throw new Exception("Укажите текущий объект");
                var ti = Program.app.getTableInfo((int)report.IdTable);
                String orderField = string.Format("\"{0}\" {1}", ti.pkField, "ASC");
                Program.ReportModel.OpenReport(report, new FilterTable(IdTable, IdObj, Where, orderField));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при открытии отчета", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void открытьМенеджерОтчетовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Program.ReportModel.OpenReportEditor(new FilterTable(IdTable, IdObj, Where), enTypeReport.Object);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при открытии менеджера отчетов", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
    class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
    };
    public class TextBoxInfo
    {
        string str;
        int start;
        public TextBox t;
        public bool shanged;
        public TextBoxInfo()
        {

        }
        public TextBoxInfo(TextBox tt)
        {
            t = tt;
            str = tt.Text;
            start = tt.SelectionStart;

        }
        public void undo()
        {
            shanged = true;
            t.Text = str;
            t.SelectionStart = start;
        }
        public void saveText()
        {
            str = t.Text;
            start = t.SelectionStart;
        }
        public void saveText(int k)
        {
            str = t.Text;
            start = t.SelectionStart;
            start += k;
            if (start < 0)
                start = 0;
            if (start > t.Text.Length)
                start = t.Text.Length;
        }
    }
    public class TextBoxEx : TextBox
    {
        private const int EM_GETLINECOUNT = 0xBA;
        public int LineCount
        {
            get
            {
                Message msg = Message.Create(this.Handle, EM_GETLINECOUNT, IntPtr.Zero, IntPtr.Zero);
                base.DefWndProc(ref msg);
                return msg.Result.ToInt32();
            }
        }
    }
    struct ref_with_tables
    {
        public int id_ref_table;
        public int id_ref_col;
        public int id_col;
        public string db_name_ref_col;
    }
}