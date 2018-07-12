using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using ConfigView = Rekod.DataAccess.SourcePostgres.View.ConfigView;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Behaviors;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using Rekod.Services;
using System.Threading;

namespace Rekod.DataAccess.SourcePostgres.ViewModel
{
    public class PgIndexVM : WindowViewModelBase_VM
    {
        #region Поля

        private PgM.PgTableBaseM _table;
        private PgVM.PgDataRepositoryVM _source;
        private bool _enabled;
        private ObservableCollection<AbsM.IFieldM> _indexedFields;
        private ObservableCollection<AbsM.IFieldM> _notIndexedFields;
        private String _displayText;
        private bool _isNew, _isNotNew;
        private AbsM.IFieldM _indexedSelectedItem;
        private AbsM.IFieldM _notIndexedSelectedItem;

        #endregion Поля

        #region Свойства
        public PgM.PgTableBaseM Table
        {
            get { return _table; }
        }

        public PgVM.PgDataRepositoryVM Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Используется ли индекс
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set { OnPropertyChanged(ref _enabled, value, () => this.Enabled); }
        }

        /// <summary>
        /// Индекс к таблице не создавался
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                OnPropertyChanged(ref _isNew, value, () => this.IsNew);
                OnPropertyChanged(ref _isNotNew, !value, () => this.IsNotNew);
            }
        }

        /// <summary>
        /// Таблица проиндексирована
        /// </summary>
        public bool IsNotNew { get { return _isNotNew; } }

        public String DisplayText
        {
            get { return _displayText; }
            set { OnPropertyChanged(ref _displayText, value, () => this.DisplayText); }
        }

        public AbsM.IFieldM IndexedSelectedItem
        {
            get { return _indexedSelectedItem; }
            set { OnPropertyChanged(ref _indexedSelectedItem, value, () => this.IndexedSelectedItem); }
        }

        public AbsM.IFieldM NotIndexedSelectedItem
        {
            get { return _notIndexedSelectedItem; }
            set { OnPropertyChanged(ref _notIndexedSelectedItem, value, () => this.NotIndexedSelectedItem); }
        }

        #endregion

        #region Коллекции
        /// <summary>
        /// Индексируемые поля
        /// </summary>
        public ObservableCollection<AbsM.IFieldM> IndexedFields
        {
            get { return _indexedFields; }
        }

        /// <summary>
        /// Неиндексируемые поля
        /// </summary>
        public ObservableCollection<AbsM.IFieldM> NotIndexedFields
        {
            get { return _notIndexedFields; }
        }

        /// <summary>
        /// Поля для подписи
        /// </summary>
        public List<AbsM.IFieldM> LabelFields
        {
            get { return _table.Fields.ToList().FindAll(w => w.Name != _table.GeomField); }
        }
        
        #endregion
        
        #region Конструктор

        public PgIndexVM(PgM.PgTableBaseM table)
        {
            _table = table;
            _source = table.Source as PgVM.PgDataRepositoryVM;
            _indexedFields = new ObservableCollection<AbsM.IFieldM>();
            _notIndexedFields = new ObservableCollection<AbsM.IFieldM>();

            LoadIndexInfo();
        }

        #endregion Конструктор

        #region Методы
        /// <summary>
        /// Загружает из БД данные о наличии индекса, используемых полях и подписи
        /// </summary>
        private void LoadIndexInfo()
        {
            if (Table == null)
                return;

            _indexedFields.Clear();
            _notIndexedFields.Clear();
            foreach (var item in _table.Fields)
            {
                var field = new PgM.PgFieldM(_table, item.Id)
                {
                    Name = item.Name,
                    Text = item.Text
                };
                if (field.Name == _table.GeomField || field.Name == _table.PrimaryKey)
                    continue;
                _notIndexedFields.Add((AbsM.IFieldM)field);
            }

            using (var sqlCmd = new SqlWork(Source.Connect))
            {
                sqlCmd.sql = "SELECT display_text, enabled FROM " + Program.scheme + 
                    ".fts_tables WHERE id_table = " + Table.Id;
                bool success = sqlCmd.ExecuteReader();

                if (sqlCmd.CanRead())
                {
                    IsNew = false;
                    Enabled = sqlCmd.GetBoolean("enabled");
                    DisplayText = ParseResultBackDB(sqlCmd.GetString("display_text"));

                    using (var sqlMore = new SqlWork(Source.Connect))
                    {
                        sqlMore.sql = "SELECT id_field FROM " + Program.scheme +
                            ".fts_fields WHERE id_table = " + Table.Id + " ORDER BY order_num";
                        sqlMore.ExecuteReader();

                        while (sqlMore.CanRead())
                        {
                            int id_field = sqlMore.GetInt32("id_field");
                            var indexedField = _notIndexedFields.FirstOrDefault(w => w.Id == id_field);
                            if (indexedField != null)
                            {
                                _notIndexedFields.Remove(indexedField);
                                _indexedFields.Add(indexedField);
                            }
                        }
                    }
                }
                else
                {
                    IsNew = true;
                    Enabled = false;
                    DisplayText = String.Empty;
                }
            }
        }

        String ParseResultDB(String text)
        {
            int state = 0;
            // state = 0 ->  ?
            // state = 1 -> '{' 
            String result = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (state == 0)
                {
                    if (text[i] == '{') { state = 1; result += "(COALESCE("; }
                    else if (text[i] == '+') { result += "||"; }
                    else { result += text[i]; }
                }
                else if (state == 1)
                {
                    if (text[i] == '}')
                    {
                        state = 0;
                        result += "::text, quote_ident(''))::text)";
                    }
                    else if (text[i] == '[' && text[i - 1] == '{' || text[i] == ']' && text[i + 1] == '}')
                    {
                        result += "'";
                    }
                    else if (text[i] == '\'')
                    {
                        result += "''";
                    }
                    else
                    {
                        result += text[i];
                    }
                }
            }
            return result;
        }

        String ParseResultBackDB(String text)
        {
            if (text == null)
            {
                text = "";
            }
            String result = "";
            text = text.Replace("(COALESCE(", "((");
            String[] parts = text.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Replace("::text, quote_ident('')", "");
                parts[i] = parts[i].Replace("::text", "");
                while (parts[i].Length > 1 && (parts[i][0] == '(' && parts[i][parts[i].Length - 1] == ')'))
                {
                    parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                }
                if (parts[i].Length > 1 && parts[i][0] == '\'' && parts[i][parts[i].Length - 1] == '\'')
                {
                    parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                    parts[i] = "[" + parts[i] + "]";
                }

                parts[i] = parts[i].Replace("''", "'");
                parts[i] = "{" + parts[i] + "}";
            }

            foreach (String s in parts)
            {
                if (result != "")
                    result += "+" + s;
                else result += s;
            }

            return result;
        }

        /// <summary>
        /// Проверка подписи
        /// </summary>
        private bool IsCorrectExpression(String labelmask, ref String message)
        {
            if (labelmask.Contains("SELECT") || labelmask.Contains("DROP") || labelmask.Contains("INSERT") || labelmask.Contains("UPDATE") || labelmask.Contains("DELETE"))
            {
                message = "Вы используете недопустимое выражение";
                return false;
            }

            if (Table != null)
            {
                SqlWork sqlCmd = new SqlWork((Table.Source as PgVM.PgDataRepositoryVM).Connect, true);
                sqlCmd.sql = string.Format("SELECT str FROM (SELECT {0} AS str FROM {1}.{2}) AS query WHERE str IS NOT NULL LIMIT 1", labelmask, Table.SchemeName, Table.Name);
                try
                {
                    sqlCmd.ExecuteReader(null);
                    if (sqlCmd.CanRead())
                    {
                        message = sqlCmd.GetValue(0).ToString();
                    }
                    else
                    {
                        message = "Предпросмотр невозможен. Похоже в таблице этого слоя нет ни одной непустой записи";
                    }
                }
                catch (Exception ex)
                {
                    message = "Вы ввели некорректное выражение";
                    return false;
                }
                finally
                {
                    sqlCmd.Close();
                }
                return true;
            }
            else
            {
                message = "Нет доступа к источнику";
                return false;
            }
        }

        /// <summary>
        /// Можно ли переместить поле
        /// </summary>
        private bool CanMove(object parameter = null)
        {
            return parameter is IList && ((IList)parameter).Count > 0;
        }
        #endregion Методы

        #region Действия
        public Action<object> PreviewAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    String labelExpression = commEvtParam.CommandParameter.ToString();

                    String result = ParseResultDB(labelExpression);

                    if (result.Replace(" ", "") == "")
                    {
                        MessageBox.Show(@"Выражение не должно быть пустым", "Ошибка предпросмотра", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    String message = "";

                    IsCorrectExpression(result, ref message);
                    if (!String.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(message, "Предпросмотр", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                };
            }
        }

        public Action<object> ClearAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    List<object> commParams = commEvtParam.CommandParameter as List<object>;
                    TextBox TextBoxLabelExpression = commParams[0] as TextBox;
                    TextBoxLabelExpression.Text = "";
                };
            }
        }

        public Action<object> AttachAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    if (commEvtParam.EventArgs != null && commEvtParam.EventArgs is KeyEventArgs)
                    {
                        if (((KeyEventArgs)commEvtParam.EventArgs).Key != Key.Enter)
                            return;
                        ((KeyEventArgs)commEvtParam.EventArgs).Handled = true;
                    }

                    List<object> commParams = commEvtParam.CommandParameter as List<object>;

                    TextBox TextBoxLabelExpression = commParams[0] as TextBox;
                    ComboBox ComboBoxAttachWith = commParams[1] as ComboBox;

                    int splitPlace = TextBoxLabelExpression.SelectionStart;
                    String leftPart = TextBoxLabelExpression.Text.Substring(0, splitPlace);
                    String rightPart = TextBoxLabelExpression.Text.Substring(splitPlace);
                    if (ComboBoxAttachWith.SelectedIndex != -1 
                        && ((PgM.PgFieldM)ComboBoxAttachWith.SelectedItem).Text == ComboBoxAttachWith.Text)
                    {
                        TextBoxLabelExpression.Text = leftPart + (((leftPart != "") ? "+" : "") + "{" + ((PgM.PgFieldM)ComboBoxAttachWith.SelectedItem).Name + "}") + ((rightPart != "") ? "+" : "") + rightPart;
                    }
                    else
                    {
                        TextBoxLabelExpression.Text = leftPart + (((leftPart != "") ? "+" : "") + "{[" + ComboBoxAttachWith.Text + "]}") + ((rightPart != "") ? "+" : "") + rightPart;
                    }
                };
            }
        }
        #endregion Действия

        #region Команды
        #region IncludeCommand
        private ICommand _includeCommand;
        /// <summary>
        /// Команда включения поля в используемые для индекса
        /// </summary>
        public ICommand IncludeCommand
        {
            get { return _includeCommand ?? (_includeCommand = new RelayCommand(Include, CanMove)); }
        }
        /// <summary>
        /// Включение поля в используемые для индекса
        /// </summary>
        public void Include(object parameter = null)
        {
            var fields = parameter as IList;
            while (fields.Count > 0)
            {
                AbsM.IFieldM field = (AbsM.IFieldM)fields[0];
                NotIndexedFields.Remove(field);
                IndexedFields.Add(field);
                IndexedSelectedItem = field;
            }
        }
        #endregion IncludeCommand

        #region ExcludeCommand
        private ICommand _excludeCommand;
        /// <summary>
        /// Команда исключения поля из используемых для индекса
        /// </summary>
        public ICommand ExcludeCommand
        {
            get { return _excludeCommand ?? (_excludeCommand = new RelayCommand(Exclude, CanMove)); }
        }
        /// <summary>
        /// Исключение поля из используемых для индекса
        /// </summary>
        public void Exclude(object parameter = null)
        {
            var fields = parameter as IList;
            while (fields.Count > 0)
            {
                AbsM.IFieldM field = (AbsM.IFieldM)fields[0];
                NotIndexedFields.Add(field);
                IndexedFields.Remove(field);
                NotIndexedSelectedItem = field;
            }
        }
        #endregion ExcludeCommand

        #region MoveUpCommand
        private ICommand _moveUpCommand;
        /// <summary>
        /// Команда включения поля в используемые для индекса
        /// </summary>
        public ICommand MoveUpCommand
        {
            get { return _moveUpCommand ?? (_moveUpCommand = new RelayCommand(MoveUp)); }
        }
        /// <summary>
        /// Увеличение приоритета поля
        /// </summary>
        public void MoveUp(object parameter = null)
        {
            var field = parameter as AbsM.IFieldM;
            int index = _indexedFields.IndexOf(field);
            _indexedFields.RemoveAt(index);
            _indexedFields.Insert(index - 1, field);
            IndexedSelectedItem = field;
        }
        #endregion MoveUpCommand

        #region MoveDownCommand
        private ICommand _moveDownCommand;
        /// <summary>
        /// Команда включения поля в используемые для индекса
        /// </summary>
        public ICommand MoveDownCommand
        {
            get { return _moveDownCommand ?? (_moveDownCommand = new RelayCommand(MoveDown)); }
        }
        /// <summary>
        /// Уменьшение приоритета поля
        /// </summary>
        public void MoveDown(object parameter = null)
        {
            var field = parameter as AbsM.IFieldM;
            int index = _indexedFields.IndexOf(field);
            _indexedFields.RemoveAt(index);
            _indexedFields.Insert(index + 1, field);
            IndexedSelectedItem = field;
        }
        #endregion MoveDownCommand

        #region CreateCommand
        private ICommand _createCommand;
        
        /// <summary>
        /// Команда сохранения индекса
        /// </summary>
        public ICommand CreateCommand
        {
            get { return _createCommand ?? (_createCommand = new RelayCommand(Create)); }
        }

        /// <summary>
        /// Сохранение индекса
        /// </summary>
        public void Create(object parameter = null)
        {
            if (!Validate()) return;

            var displayText = ParseResultDB(_displayText);
            cti.ThreadProgress.ShowWait("createindex");

            using (var sqlCmd = new SqlWork(Source.Connect, true))
            {
                sqlCmd.sql = String.Format("SELECT count(*) from \"{0}\".\"{1}\";", _table.SchemeName, _table.Name);
                int count = sqlCmd.ExecuteScalar<Int32>();
                if (count > 10000)
                    cti.ThreadProgress.SetText(Rekod.Properties.Resources.IV_PleaseWait);
            }

            try
            {
                using (var sqlCmd = new SqlWork(Source.Connect, true))
                {
                    displayText = "E'" + displayText.Replace("'", "\\'") + "'";

                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(
                        "INSERT INTO " + Program.scheme +
                        ".fts_tables (id_table, display_text, enabled) VALUES (" +
                        _table.Id + ", " + displayText + ", " + _enabled + ");");

                    int order_num = 1;
                    foreach (var field in _indexedFields)
                    {
                        sql.AppendLine(
                            "INSERT INTO " + Program.scheme +
                            ".fts_fields (id_table, id_field, order_num) VALUES (" +
                            _table.Id + ", " + field.Id + ", " + order_num++ + ");");
                    }

                    sql.AppendLine(
                        "SELECT * FROM " + Program.scheme +
                        ".fts_prepareindex(" + _table.Id + ");");
                    
                    sqlCmd.sql = sql.ToString();
                    sqlCmd.ExecuteNonQuery();

                    IsNew = false;
                    cti.ThreadProgress.Close("createindex");
                    Thread.Sleep(500);
                    MessageBox.Show("Индекс успешно создан!", "Сохранение индекса", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception e)
            {
                cti.ThreadProgress.Close("createindex");
                Thread.Sleep(500);
                MessageBox.Show(e.Message, "Ошибка при сохранении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool Validate()
        {
            //if (_indexedFields.Count <= 0)
            //{
            //    MessageBox.Show("Вы должны выбрать по крайней мере одно индексируемое поле!", "Ошибка при сохранении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}

            if (String.IsNullOrEmpty(_displayText) && _indexedFields.Count > 0)
            {
                MessageBox.Show("Подпись не может быть пустой!", "Ошибка при сохранении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            string message = "";
            var displayText = ParseResultDB(_displayText);
            if (!IsCorrectExpression(displayText, ref message) && _indexedFields.Count > 0)
            {
                MessageBox.Show(message, "Ошибка при сохранении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        #endregion CreateCommand

        #region DeleteCommand
        private ICommand _deleteCommand;

        /// <summary>
        /// Команда удаления индекса
        /// </summary>
        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); }
        }

        /// <summary>
        /// Удаление индекса
        /// </summary>
        public void Delete(object parameter = null)
        {
            if (MessageBox.Show("Вы действительно хотите удалить индекс?",
                "Удаление индекса", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;
            try
            {
                DisplayText = null;
                Enabled = false;
                cti.ThreadProgress.ShowWait("deleteindex");
                using (var sqlCmd = new SqlWork(Source.Connect, true))
                {
                    sqlCmd.sql =
                        "SELECT * FROM " + Program.scheme +
                        ".fts_deleteindex(" + _table.Id + ");";

                    sqlCmd.ExecuteNonQuery();

                    IsNew = true;
                    cti.ThreadProgress.Close("deleteindex");
                    MessageBox.Show("Индекс успешно удален!", "Удаление индекса", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception e)
            {
                cti.ThreadProgress.Close("deleteindex");
                MessageBox.Show(e.Message, "Ошибка при удалении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cti.ThreadProgress.Close("deleteindex");
                LoadIndexInfo();
            }
        }
        #endregion Command

        #region SaveCommand
        private ICommand _saveCommand;

        /// <summary>
        /// Команда сохранения настроек индекса
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); }
        }

        /// <summary>
        /// Сохранение настроек индекса
        /// </summary>
        public void Save(object parameter = null)
        {
            if (!Validate())
                return;

            if (_indexedFields.Count > 0)
            {
                if (!IsNotNew)
                {
                    Create();
                }
                else
                {
                    try
                    {
                        using (var sqlCmd = new SqlWork(Source.Connect, true))
                        {
                            sqlCmd.sql =
                                "SELECT * FROM " + Program.scheme +
                                ".fts_deleteindex(" + _table.Id + ");";
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка при удалении индекса", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Create();
                }
            }
            else 
            {
                if (IsNotNew)
                {
                    Delete();
                }
                else 
                {
                    Enabled = false;
                    DisplayText = "";
                }
            }
        }
        #endregion SaveCommand
        #endregion
    }
}