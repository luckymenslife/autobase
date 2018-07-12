using System.Collections.Generic;
using System.Windows.Forms;
using System;
namespace Rekod
{
    partial class UcTableObjects
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UcTableObjects));
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.pButtons = new System.Windows.Forms.Panel();
            this.flpSelecter = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.flpEditorTable = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDel = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.BtnEdit = new System.Windows.Forms.Button();
            this.tsКолони = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsОбъектов = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsОбъектовВсего = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssSelected = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsВыбОбъекты = new System.Windows.Forms.ToolStripStatusLabel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.pList = new System.Windows.Forms.Panel();
            this.cbMapMoveTo = new System.Windows.Forms.CheckBox();
            this.cbViewAll = new System.Windows.Forms.CheckBox();
            this.nNum = new System.Windows.Forms.NumericUpDown();
            this.btnNum = new System.Windows.Forms.Button();
            this.btnNextNext = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnPrevPrev = new System.Windows.Forms.Button();
            this.pMess = new System.Windows.Forms.Panel();
            this.lblMess = new System.Windows.Forms.Label();
            this.contextMenuForDataGridHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.настроитьToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip2 = new Rekod.FocusMenuStrip();
            this.инструментыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsExport = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьВMSExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.видыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.колонкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохраненныеВидыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьТекущийВидToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалениеВидовToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.видПоУмолчаниюToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настроитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.правкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьКопиюВыделеннойСтрокиToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьВсеЗаписиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.фильтрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filtersToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалениеФильтровToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.применитьНаКартеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетыСТекущимОбъектомToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетыСТаблицейToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.открытьМенеджерОтчетовToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pButtons.SuspendLayout();
            this.flpSelecter.SuspendLayout();
            this.flpEditorTable.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.pList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nNum)).BeginInit();
            this.pMess.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.MarqueeAnimationSpeed = 20;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            resources.ApplyResources(this.toolStripProgressBar1, "toolStripProgressBar1");
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.flpSelecter);
            this.pButtons.Controls.Add(this.flpEditorTable);
            resources.ApplyResources(this.pButtons, "pButtons");
            this.pButtons.Name = "pButtons";
            // 
            // flpSelecter
            // 
            this.flpSelecter.Controls.Add(this.btnCancel);
            this.flpSelecter.Controls.Add(this.btnOK);
            resources.ApplyResources(this.flpSelecter, "flpSelecter");
            this.flpSelecter.Name = "flpSelecter";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // flpEditorTable
            // 
            this.flpEditorTable.Controls.Add(this.btnDel);
            this.flpEditorTable.Controls.Add(this.btnAdd);
            this.flpEditorTable.Controls.Add(this.BtnEdit);
            resources.ApplyResources(this.flpEditorTable, "flpEditorTable");
            this.flpEditorTable.Name = "flpEditorTable";
            // 
            // btnDel
            // 
            resources.ApplyResources(this.btnDel, "btnDel");
            this.btnDel.Name = "btnDel";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // btnAdd
            // 
            resources.ApplyResources(this.btnAdd, "btnAdd");
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // BtnEdit
            // 
            resources.ApplyResources(this.BtnEdit, "BtnEdit");
            this.BtnEdit.Name = "BtnEdit";
            this.BtnEdit.UseVisualStyleBackColor = true;
            this.BtnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // tsКолони
            // 
            resources.ApplyResources(this.tsКолони, "tsКолони");
            this.tsКолони.Name = "tsКолони";
            // 
            // tsОбъектов
            // 
            resources.ApplyResources(this.tsОбъектов, "tsОбъектов");
            this.tsОбъектов.Name = "tsОбъектов";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsОбъектовВсего,
            this.toolStripStatusLabel2,
            this.tsОбъектов,
            this.toolStripStatusLabel3,
            this.tsКолони,
            this.tssSelected,
            this.tsВыбОбъекты,
            this.toolStripProgressBar1});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // tsОбъектовВсего
            // 
            resources.ApplyResources(this.tsОбъектовВсего, "tsОбъектовВсего");
            this.tsОбъектовВсего.Name = "tsОбъектовВсего";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            resources.ApplyResources(this.toolStripStatusLabel3, "toolStripStatusLabel3");
            // 
            // tssSelected
            // 
            this.tssSelected.Name = "tssSelected";
            resources.ApplyResources(this.tssSelected, "tssSelected");
            // 
            // tsВыбОбъекты
            // 
            resources.ApplyResources(this.tsВыбОбъекты, "tsВыбОбъекты");
            this.tsВыбОбъекты.Name = "tsВыбОбъекты";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(250)))));
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.ShowRowErrors = false;
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            this.dataGridView1.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dataGridView1_SortCompare);
            this.dataGridView1.Sorted += new System.EventHandler(this.dataGridView1_Sorted);
            this.dataGridView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyUp);
            // 
            // pFilter
            // 
            resources.ApplyResources(this.pFilter, "pFilter");
            this.pFilter.Name = "pFilter";
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this.dataGridView1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.pButtons, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.pFilter, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.pList, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.pMess, 0, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // pList
            // 
            this.pList.Controls.Add(this.cbMapMoveTo);
            this.pList.Controls.Add(this.cbViewAll);
            this.pList.Controls.Add(this.nNum);
            this.pList.Controls.Add(this.btnNum);
            this.pList.Controls.Add(this.btnNextNext);
            this.pList.Controls.Add(this.btnNext);
            this.pList.Controls.Add(this.btnPrev);
            this.pList.Controls.Add(this.btnPrevPrev);
            resources.ApplyResources(this.pList, "pList");
            this.pList.Name = "pList";
            // 
            // cbMapMoveTo
            // 
            resources.ApplyResources(this.cbMapMoveTo, "cbMapMoveTo");
            this.cbMapMoveTo.Name = "cbMapMoveTo";
            this.cbMapMoveTo.UseVisualStyleBackColor = true;
            this.cbMapMoveTo.CheckedChanged += new System.EventHandler(this.cbMapMoveTo_CheckedChanged);
            // 
            // cbViewAll
            // 
            resources.ApplyResources(this.cbViewAll, "cbViewAll");
            this.cbViewAll.Name = "cbViewAll";
            this.cbViewAll.UseVisualStyleBackColor = true;
            this.cbViewAll.CheckedChanged += new System.EventHandler(this.cbViewAll_CheckedChanged);
            // 
            // nNum
            // 
            resources.ApplyResources(this.nNum, "nNum");
            this.nNum.Name = "nNum";
            this.nNum.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nNum_KeyUp);
            this.nNum.Validated += new System.EventHandler(this.nNum_Validated);
            // 
            // btnNum
            // 
            resources.ApplyResources(this.btnNum, "btnNum");
            this.btnNum.Name = "btnNum";
            this.btnNum.UseVisualStyleBackColor = true;
            this.btnNum.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnNextNext
            // 
            resources.ApplyResources(this.btnNextNext, "btnNextNext");
            this.btnNextNext.Name = "btnNextNext";
            this.btnNextNext.UseVisualStyleBackColor = true;
            this.btnNextNext.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnNext
            // 
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnPrev
            // 
            resources.ApplyResources(this.btnPrev, "btnPrev");
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnPrevPrev
            // 
            resources.ApplyResources(this.btnPrevPrev, "btnPrevPrev");
            this.btnPrevPrev.Name = "btnPrevPrev";
            this.btnPrevPrev.UseVisualStyleBackColor = true;
            this.btnPrevPrev.Click += new System.EventHandler(this.btnList_Click);
            // 
            // pMess
            // 
            resources.ApplyResources(this.pMess, "pMess");
            this.pMess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(215)))), ((int)(((byte)(227)))));
            this.pMess.Controls.Add(this.lblMess);
            this.pMess.Name = "pMess";
            // 
            // lblMess
            // 
            resources.ApplyResources(this.lblMess, "lblMess");
            this.lblMess.Name = "lblMess";
            // 
            // contextMenuForDataGridHeader
            // 
            this.contextMenuForDataGridHeader.Name = "contextMenuForDataGridHeader";
            resources.ApplyResources(this.contextMenuForDataGridHeader, "contextMenuForDataGridHeader");
            // 
            // настроитьToolStripMenuItem1
            // 
            this.настроитьToolStripMenuItem1.Name = "настроитьToolStripMenuItem1";
            resources.ApplyResources(this.настроитьToolStripMenuItem1, "настроитьToolStripMenuItem1");
            // 
            // menuStrip2
            // 
            resources.ApplyResources(this.menuStrip2, "menuStrip2");
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.инструментыToolStripMenuItem,
            this.видыToolStripMenuItem,
            this.правкаToolStripMenuItem,
            this.фильтрыToolStripMenuItem,
            this.отчетыToolStripMenuItem});
            this.menuStrip2.Name = "menuStrip2";
            // 
            // инструментыToolStripMenuItem
            // 
            this.инструментыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsExport,
            this.importFromFile,
            this.открытьВMSExcelToolStripMenuItem});
            this.инструментыToolStripMenuItem.Name = "инструментыToolStripMenuItem";
            resources.ApplyResources(this.инструментыToolStripMenuItem, "инструментыToolStripMenuItem");
            // 
            // tsExport
            // 
            this.tsExport.Name = "tsExport";
            resources.ApplyResources(this.tsExport, "tsExport");
            this.tsExport.Click += new System.EventHandler(this.tsExport_Click);
            // 
            // importFromFile
            // 
            this.importFromFile.Name = "importFromFile";
            resources.ApplyResources(this.importFromFile, "importFromFile");
            this.importFromFile.Click += new System.EventHandler(this.importFromFile_Click);
            // 
            // открытьВMSExcelToolStripMenuItem
            // 
            this.открытьВMSExcelToolStripMenuItem.Name = "открытьВMSExcelToolStripMenuItem";
            resources.ApplyResources(this.открытьВMSExcelToolStripMenuItem, "открытьВMSExcelToolStripMenuItem");
            this.открытьВMSExcelToolStripMenuItem.Click += new System.EventHandler(this.открытьВMSExcelToolStripMenuItem_Click);
            // 
            // видыToolStripMenuItem
            // 
            this.видыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.колонкиToolStripMenuItem});
            this.видыToolStripMenuItem.Name = "видыToolStripMenuItem";
            resources.ApplyResources(this.видыToolStripMenuItem, "видыToolStripMenuItem");
            // 
            // колонкиToolStripMenuItem
            // 
            this.колонкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сохраненныеВидыToolStripMenuItem,
            this.сохранитьТекущийВидToolStripMenuItem,
            this.удалениеВидовToolStripMenuItem,
            this.видПоУмолчаниюToolStripMenuItem,
            this.настроитьToolStripMenuItem});
            this.колонкиToolStripMenuItem.Name = "колонкиToolStripMenuItem";
            resources.ApplyResources(this.колонкиToolStripMenuItem, "колонкиToolStripMenuItem");
            // 
            // сохраненныеВидыToolStripMenuItem
            // 
            this.сохраненныеВидыToolStripMenuItem.Name = "сохраненныеВидыToolStripMenuItem";
            resources.ApplyResources(this.сохраненныеВидыToolStripMenuItem, "сохраненныеВидыToolStripMenuItem");
            // 
            // сохранитьТекущийВидToolStripMenuItem
            // 
            this.сохранитьТекущийВидToolStripMenuItem.Name = "сохранитьТекущийВидToolStripMenuItem";
            resources.ApplyResources(this.сохранитьТекущийВидToolStripMenuItem, "сохранитьТекущийВидToolStripMenuItem");
            this.сохранитьТекущийВидToolStripMenuItem.Click += new System.EventHandler(this.сохранитьТекущийВидToolStripMenuItem_Click);
            // 
            // удалениеВидовToolStripMenuItem
            // 
            this.удалениеВидовToolStripMenuItem.Name = "удалениеВидовToolStripMenuItem";
            resources.ApplyResources(this.удалениеВидовToolStripMenuItem, "удалениеВидовToolStripMenuItem");
            this.удалениеВидовToolStripMenuItem.Click += new System.EventHandler(this.удалениеВидовToolStripMenuItem_Click);
            // 
            // видПоУмолчаниюToolStripMenuItem
            // 
            this.видПоУмолчаниюToolStripMenuItem.Name = "видПоУмолчаниюToolStripMenuItem";
            resources.ApplyResources(this.видПоУмолчаниюToolStripMenuItem, "видПоУмолчаниюToolStripMenuItem");
            this.видПоУмолчаниюToolStripMenuItem.Click += new System.EventHandler(this.видПоУмолчаниюToolStripMenuItem_Click);
            // 
            // настроитьToolStripMenuItem
            // 
            this.настроитьToolStripMenuItem.Name = "настроитьToolStripMenuItem";
            resources.ApplyResources(this.настроитьToolStripMenuItem, "настроитьToolStripMenuItem");
            this.настроитьToolStripMenuItem.Click += new System.EventHandler(this.настроитьToolStripMenuItem_Click);
            // 
            // правкаToolStripMenuItem
            // 
            this.правкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.создатьКопиюВыделеннойСтрокиToolStripMenuItem1,
            this.удалитьВсеЗаписиToolStripMenuItem,
            this.tsHistory,
            this.tsRefresh});
            this.правкаToolStripMenuItem.Name = "правкаToolStripMenuItem";
            resources.ApplyResources(this.правкаToolStripMenuItem, "правкаToolStripMenuItem");
            // 
            // создатьКопиюВыделеннойСтрокиToolStripMenuItem1
            // 
            this.создатьКопиюВыделеннойСтрокиToolStripMenuItem1.Name = "создатьКопиюВыделеннойСтрокиToolStripMenuItem1";
            resources.ApplyResources(this.создатьКопиюВыделеннойСтрокиToolStripMenuItem1, "создатьКопиюВыделеннойСтрокиToolStripMenuItem1");
            this.создатьКопиюВыделеннойСтрокиToolStripMenuItem1.Click += new System.EventHandler(this.создатьКопиюВыделеннойСтрокиToolStripMenuItem_Click);
            // 
            // удалитьВсеЗаписиToolStripMenuItem
            // 
            this.удалитьВсеЗаписиToolStripMenuItem.Name = "удалитьВсеЗаписиToolStripMenuItem";
            resources.ApplyResources(this.удалитьВсеЗаписиToolStripMenuItem, "удалитьВсеЗаписиToolStripMenuItem");
            this.удалитьВсеЗаписиToolStripMenuItem.Click += new System.EventHandler(this.удалитьВсеЗаписиToolStripMenuItem_Click);
            // 
            // tsHistory
            // 
            this.tsHistory.Name = "tsHistory";
            resources.ApplyResources(this.tsHistory, "tsHistory");
            this.tsHistory.Click += new System.EventHandler(this.tsHistory_Click);
            // 
            // tsRefresh
            // 
            this.tsRefresh.Name = "tsRefresh";
            resources.ApplyResources(this.tsRefresh, "tsRefresh");
            this.tsRefresh.Click += new System.EventHandler(this.tsRefresh_Click);
            // 
            // фильтрыToolStripMenuItem
            // 
            this.фильтрыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filtersToolStripMenuItem1,
            this.сохранитьToolStripMenuItem,
            this.удалениеФильтровToolStripMenuItem,
            this.применитьНаКартеToolStripMenuItem});
            this.фильтрыToolStripMenuItem.Name = "фильтрыToolStripMenuItem";
            resources.ApplyResources(this.фильтрыToolStripMenuItem, "фильтрыToolStripMenuItem");
            // 
            // filtersToolStripMenuItem1
            // 
            this.filtersToolStripMenuItem1.Name = "filtersToolStripMenuItem1";
            resources.ApplyResources(this.filtersToolStripMenuItem1, "filtersToolStripMenuItem1");
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            resources.ApplyResources(this.сохранитьToolStripMenuItem, "сохранитьToolStripMenuItem");
            this.сохранитьToolStripMenuItem.Click += new System.EventHandler(this.сохранитьToolStripMenuItem_Click);
            // 
            // удалениеФильтровToolStripMenuItem
            // 
            this.удалениеФильтровToolStripMenuItem.Name = "удалениеФильтровToolStripMenuItem";
            resources.ApplyResources(this.удалениеФильтровToolStripMenuItem, "удалениеФильтровToolStripMenuItem");
            this.удалениеФильтровToolStripMenuItem.Click += new System.EventHandler(this.удалениеФильтровToolStripMenuItem_Click);
            // 
            // применитьНаКартеToolStripMenuItem
            // 
            this.применитьНаКартеToolStripMenuItem.Name = "применитьНаКартеToolStripMenuItem";
            resources.ApplyResources(this.применитьНаКартеToolStripMenuItem, "применитьНаКартеToolStripMenuItem");
            this.применитьНаКартеToolStripMenuItem.Click += new System.EventHandler(this.применитьНаКартеToolStripMenuItem_Click);
            // 
            // отчетыToolStripMenuItem
            // 
            this.отчетыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.отчетыСТекущимОбъектомToolStripMenuItem,
            this.отчетыСТаблицейToolStripMenuItem,
            this.toolStripSeparator1,
            this.открытьМенеджерОтчетовToolStripMenuItem});
            this.отчетыToolStripMenuItem.Name = "отчетыToolStripMenuItem";
            resources.ApplyResources(this.отчетыToolStripMenuItem, "отчетыToolStripMenuItem");
            // 
            // отчетыСТекущимОбъектомToolStripMenuItem
            // 
            this.отчетыСТекущимОбъектомToolStripMenuItem.Name = "отчетыСТекущимОбъектомToolStripMenuItem";
            resources.ApplyResources(this.отчетыСТекущимОбъектомToolStripMenuItem, "отчетыСТекущимОбъектомToolStripMenuItem");
            // 
            // отчетыСТаблицейToolStripMenuItem
            // 
            this.отчетыСТаблицейToolStripMenuItem.Name = "отчетыСТаблицейToolStripMenuItem";
            resources.ApplyResources(this.отчетыСТаблицейToolStripMenuItem, "отчетыСТаблицейToolStripMenuItem");
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // открытьМенеджерОтчетовToolStripMenuItem
            // 
            this.открытьМенеджерОтчетовToolStripMenuItem.Name = "открытьМенеджерОтчетовToolStripMenuItem";
            resources.ApplyResources(this.открытьМенеджерОтчетовToolStripMenuItem, "открытьМенеджерОтчетовToolStripMenuItem");
            this.открытьМенеджерОтчетовToolStripMenuItem.Click += new System.EventHandler(this.открытьМенеджерОтчетовToolStripMenuItem_Click);
            // 
            // UcTableObjects
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip2);
            this.MinimumSize = new System.Drawing.Size(577, 386);
            this.Name = "UcTableObjects";
            this.Load += new System.EventHandler(this.UcTableObjects_Load);
            this.pButtons.ResumeLayout(false);
            this.flpSelecter.ResumeLayout(false);
            this.flpEditorTable.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.pList.ResumeLayout(false);
            this.pList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nNum)).EndInit();
            this.pMess.ResumeLayout(false);
            this.pMess.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Panel pButtons;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Button BtnEdit;
        private System.Windows.Forms.ToolStripStatusLabel tsКолони;
        private System.Windows.Forms.ToolStripStatusLabel tsОбъектов;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem инструментыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsExport;
        private System.Windows.Forms.ToolStripMenuItem tsHistory;
        private System.Windows.Forms.ToolStripMenuItem tsRefresh;
        public System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.FlowLayoutPanel pFilter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;

        
        private Panel pList;
        private ToolStripStatusLabel tsОбъектовВсего;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private Button btnNum;
        private Button btnNextNext;
        private Button btnNext;
        private Button btnPrev;
        private Button btnPrevPrev;
        private NumericUpDown nNum;
        private FlowLayoutPanel flpSelecter;
        private Button btnCancel;
        private Button btnOK;
        private ToolStripMenuItem importFromFile;
        private FlowLayoutPanel flpEditorTable;
        private Button btnAdd;
        private CheckBox cbViewAll;
        private ToolStripMenuItem правкаToolStripMenuItem;
        private ToolStripMenuItem создатьКопиюВыделеннойСтрокиToolStripMenuItem1;
        private CheckBox cbMapMoveTo;
        private ToolStripMenuItem удалитьВсеЗаписиToolStripMenuItem;
        private ToolStripMenuItem открытьВMSExcelToolStripMenuItem;
        private ToolStripMenuItem фильтрыToolStripMenuItem;
        private ToolStripMenuItem сохранитьToolStripMenuItem;
        private FocusMenuStrip menuStrip2;
        private ToolStripMenuItem filtersToolStripMenuItem1;
        private ToolStripMenuItem удалениеФильтровToolStripMenuItem;
        private ToolStripMenuItem применитьНаКартеToolStripMenuItem;
        private Panel pMess;
        private Label lblMess;
        private ToolStripStatusLabel tssSelected;
        private ToolStripStatusLabel tsВыбОбъекты;
        private ToolStripMenuItem отчетыToolStripMenuItem;
        private ToolStripMenuItem отчетыСТекущимОбъектомToolStripMenuItem;
        private ToolStripMenuItem открытьМенеджерОтчетовToolStripMenuItem;
        private ToolStripMenuItem отчетыСТаблицейToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ContextMenuStrip contextMenuForDataGridHeader;
        private ToolStripMenuItem видыToolStripMenuItem;
        private ToolStripMenuItem сохраненныеВидыToolStripMenuItem;
        private ToolStripMenuItem сохранитьТекущийВидToolStripMenuItem;
        private ToolStripMenuItem удалениеВидовToolStripMenuItem;
        private ToolStripMenuItem видПоУмолчаниюToolStripMenuItem;
        private ToolStripMenuItem настроитьToolStripMenuItem;
        private ToolStripMenuItem колонкиToolStripMenuItem;
        private ToolStripMenuItem настроитьToolStripMenuItem1;
    }
}
