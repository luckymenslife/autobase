using axVisUtils;
using Npgsql;
using Rekod.Behaviors;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.Model.PgFullTextSearch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Services;
using System.Windows.Controls;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    class PgFullTextSearchVM : WindowViewModelBase_VM
    {
        #region Поля
        private PgDataRepositoryVM _source;
        private ObservableCollection<PgSearchTableM> _results;
        private ObservableCollection<PgSearchTableM> _tables;
        private Int32 _searchLimit = 100;
        private Boolean _movingToObject = true;
        private String _searchText;
        #endregion Поля

        #region Конструкторы
        public PgFullTextSearchVM(PgDataRepositoryVM source)
        {
            Title = Properties.Resources.PgFullSearch_Title;
            _source = source;
            FillTables();
        }
        #endregion Конструкторы

        #region Коллекции
        /// <summary>
        /// Результаты поиска
        /// </summary>
        public ObservableCollection<PgSearchTableM> Results
        {
            get { return _results ?? (_results = new ObservableCollection<PgSearchTableM>()); }
        }
        /// <summary>
        /// Таблицы по которым будет осуществлен поиск
        /// </summary>
        public ObservableCollection<PgSearchTableM> Tables
        {
            get
            { return _tables ?? (_tables = new ObservableCollection<PgSearchTableM>()); }
        }
        #endregion Коллекции

        #region Свойства
        public PgDataRepositoryVM Source
        {
            get { return _source; }
        }
        public Boolean MovingToObject
        {
            get
            {
                return _movingToObject;
            }
            set
            {
                OnPropertyChanged(ref _movingToObject, value, () => this.MovingToObject);
            }
        }
        public String SearchText
        {
            get { return _searchText; }
            private set
            {
                OnPropertyChanged(ref _searchText, value, () => this.SearchText);
            }
        }
        #endregion Свойства

        #region Команды
        #region FullTextSearchCommand
        private ICommand _fullTextSearchCommand;
        /// <summary>
        /// Команда для полнотекстового поиска
        /// </summary>
        public ICommand FullTextSearchCommand
        {
            get { return _fullTextSearchCommand ?? (_fullTextSearchCommand = new RelayCommand(this.FullTextSearch, this.CanFullTextSearch)); }
        }
        /// <summary>
        /// Полнотекстовый поиск
        /// </summary>
        public void FullTextSearch(object parameter = null)
        {
            SearchText = parameter.ToString();
            _results.Clear();

            var searchTables = from PgSearchTableM ftsTable in Tables where ftsTable.SearchWithin select ftsTable.Table;

            if (searchTables.Count() > 0)
            {
                Dictionary<int, PgSearchTableM> tableIdSearchTable = new Dictionary<int, PgSearchTableM>();
                using (var sqlWork = new SqlWork(_source.Connect))
                {
                    sqlWork.sql = String.Format(@"SELECT id_table, pkid, label_text
                                               FROM sys_scheme.fts_index
                                               WHERE fts @@ plainto_tsquery('{0}', '{1}') AND id_table IN ({3})
                                               LIMIT {2}",
                                                       "russian",
                                                       SearchText,
                                                       _searchLimit,
                                                       String.Join(",", (from PgTableBaseM table in searchTables select table.Id).ToArray()));
                    sqlWork.ExecuteReader();
                    while (sqlWork.CanRead())
                    {
                        int idTable = sqlWork.GetValue<Int32>("id_table");
                        int primKey = sqlWork.GetValue<Int32>("pkid");
                        String labelText = sqlWork.GetValue<String>("label_text");
                        PgSearchTableM pgSearchTable = null;
                        if (tableIdSearchTable.ContainsKey(idTable))
                        {
                            pgSearchTable = tableIdSearchTable[idTable];
                        }
                        else
                        {
                            pgSearchTable = new PgSearchTableM(_source, idTable);
                            tableIdSearchTable.Add(idTable, pgSearchTable);
                            _results.Add(pgSearchTable);
                        }
                        PgSearchObjectM pgSearchObject = new PgSearchObjectM(pgSearchTable, primKey, labelText);
                        pgSearchTable.SearchObjects.Add(pgSearchObject);
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли произвести полнотекстовый поиск
        /// </summary>
        public bool CanFullTextSearch(object parameter = null)
        {
            return Tables.Count > 0 && parameter != null && !String.IsNullOrEmpty(parameter.ToString());
        }
        #endregion // FullTextSearchCommand

        #region OpenAttributesWindowCommand
        private ICommand _openAttributesWindowCommand;
        /// <summary>
        /// Команда для открытия окна атрибутики
        /// </summary>
        public ICommand OpenAttributesWindowCommand
        {
            get { return _openAttributesWindowCommand ?? (_openAttributesWindowCommand = new RelayCommand(this.OpenAttributesWindow, this.CanOpenAttributesWindow)); }
        }
        /// <summary>
        /// Открытие окна атрибутики
        /// </summary>
        public void OpenAttributesWindow(object parameter = null)
        {
            PgSearchObjectM searchObject = null;
            if (parameter is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = parameter as CommandEventParameter;
                MouseButtonEventArgs eventArgs = commEvtParam.EventArgs as MouseButtonEventArgs;
                System.Windows.Controls.TreeView treeView = commEvtParam.EventSender as System.Windows.Controls.TreeView;
                if (treeView.SelectedItem != null)
                {
                    searchObject = treeView.SelectedItem as PgSearchObjectM;
                }
            }
            else if (parameter is PgSearchObjectM)
            {
                searchObject = parameter as PgSearchObjectM;
            }
            if (searchObject != null)
            {
                var table = Program.app.getTableInfo(searchObject.SearchTable.Table.Id);
                var idObject = searchObject.IdObject;
                Program.work.OpenForm.ShowAttributeObject(table, idObject, (idObject <= 0), null);
            }
        }
        /// <summary>
        /// Можно ли открыть окно атрибутики
        /// </summary>
        public bool CanOpenAttributesWindow(object parameter = null)
        {
            bool result = false;
            if (parameter is CommandEventParameter || parameter is PgSearchObjectM)
            {
                result = true;
            }
            return result;
        }
        #endregion OpenAttributesWindowCommand

        #region MoveToObjectCommand
        private ICommand _moveToObjectCommand;
        /// <summary>
        /// Команда для перелета к объекту
        /// </summary>
        public ICommand MoveToObjectCommand
        {
            get { return _moveToObjectCommand ?? (_moveToObjectCommand = new RelayCommand(this.MoveToObject, this.CanMoveToObject)); }
        }
        /// <summary>
        /// Перелет к объекту
        /// </summary>
        public void MoveToObject(object parameter = null)
        {
            if (_movingToObject)
            {
                CommandEventParameter commEvtParam = parameter as CommandEventParameter;
                RoutedPropertyChangedEventArgs<object> eventArgs = commEvtParam.EventArgs as RoutedPropertyChangedEventArgs<object>;
                if (eventArgs.NewValue is PgSearchObjectM)
                {
                    PgSearchObjectM searchObject = eventArgs.NewValue as PgSearchObjectM;
                    if (searchObject.SearchTable.Table.IsLayer)
                    {
                        int idObject = searchObject.IdObject;
                        int idTable = searchObject.SearchTable.Table.Id;

                        var layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(idTable));
                        if (layer == null || layer.Visible != true)
                        {
                            // пытаемся включить слой
                            Program.mainFrm1.layerItemsView1.SetLayerVisible(idTable);
                            layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(idTable));
                            if (layer == null || layer.Visible != true)
                                return;
                        }
                        try
                        {
                            if (idObject != -1)
                            {
                                mvMapLib.mvVectorObject mvObj = layer.getObject(idObject);
                                layer.DeselectAll();
                                if (Program.SettingsXML.LocalParameters.EnterTheScreen)
                                {
                                    if (mvObj.VectorType != mvMapLib.mvVecTypes.mvVecPoint)
                                    {
                                        Program.mainFrm1.axMapLIb1.SetExtent(mvObj.bbox);
                                    }
                                    Program.mainFrm1.axMapLIb1.setScrCenter((mvObj.bbox.b.x + mvObj.bbox.a.x) / 2, (mvObj.bbox.b.y + mvObj.bbox.a.y) / 2);
                                    mvObj.Selected = true;
                                }
                                else
                                {
                                    layer.MoveTo(idObject, true);
                                }
                            }
                            Program.mainFrm1.axMapLIb1.mapRepaint();
                        }
                        catch (Exception ex)
                        {
                            Classes.workLogFile.writeLogFile(ex, true, true);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли перелететь к объекту
        /// </summary>
        public bool CanMoveToObject(object parameter = null)
        {
            return _movingToObject;
        }
        #endregion // MoveToObjectCommand

        #region HideWindowCommand
        private ICommand _hideWindowCommand;
        /// <summary>
        /// Команда для скрытия окна поиска
        /// </summary>
        public ICommand HideWindowCommand
        {
            get { return _hideWindowCommand ?? (_hideWindowCommand = new RelayCommand(this.HideWindow, this.CanHideWindow)); }
        }
        /// <summary>
        /// Скрытие окна поиска
        /// </summary>
        public void HideWindow(object parameter = null)
        {
            AttachedWindow.Hide();
        }
        /// <summary>
        /// Можно ли скрыть окно поиска
        /// </summary>
        public bool CanHideWindow(object parameter = null)
        {
            return true;
        }
        #endregion // HideWindowCommand
        #endregion Команды

        #region Методы
        private void FillTables()
        {
            Tables.Clear();
            using (SqlWork sqlWork = new SqlWork(_source.Connect))
            {
                var hasRightsList = from PgUserRightsM userRight in _source.CurrentUser.UserRights.Rights where userRight.CanRead == true select userRight.TableId;
                sqlWork.sql =
                    String.Format(@"SELECT DISTINCT ti.id, ti.name_map
                                    FROM sys_scheme.table_info ti
                                    INNER JOIN sys_scheme.fts_tables ft ON ft.id_table = ti.id
                                    INNER JOIN sys_scheme.table_right tr ON ti.id = tr.id_table
                                    WHERE tr.read_data = TRUE AND tr.id_user = {0}", _source.CurrentUser.ID);
                sqlWork.ExecuteReader();
                while (sqlWork.CanRead())
                {
                    int idTable = sqlWork.GetValue<int>(0);
                    PgTableBaseM pgTable = _source.FindTable(idTable) as PgTableBaseM;
                    if (pgTable != null)
                    {
                        Tables.Add(new PgSearchTableM(pgTable));
                    }
                }
            }
        }
        protected override bool Closing(object obj)
        {
            Window window = obj as Window;
            window.Hide();
            return false;
        }
        #endregion Методы
    }
}