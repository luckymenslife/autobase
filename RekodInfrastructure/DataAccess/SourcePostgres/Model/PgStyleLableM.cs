using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.Behaviors;
using System.Windows;
using Rekod.DBTablesEdit;
using System.Windows.Controls;
using System.Windows.Data;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Стили подписей PostgreSql
    /// </summary>
    public class PgStyleLableM : ViewModelBase
    {
        #region Поля
        private bool _isNewTable = false;
        private int _id;
        private PgTableBaseM _table;
        private PgStyleSetsM _set;
        private string _lableMask;
        private bool _labelShowLabel;
        private bool _labelUseLabelStyle;
        private bool _labelShowFrame;
        private uint _labelFrameColor;
        private bool _labelParallel;
        private bool _labelOverlap;
        private bool _labelUseBounds;
        private uint _labelMinScale;
        private uint _labelMaxScale;
        private int _labelOffset;
        private bool _labelUseGraphicUnits;
        private string _labelFontName;
        private uint _labelFontColor;
        private int _labelFontSize;
        private bool _labelFontStrikeout;
        private bool _labelFontItalic;
        private bool _labelFontUnderline;
        private bool _labelFondBold;
        #endregion // Поля

        #region Конструтор
        public PgStyleLableM()
        {
            LabelFontName = SystemFonts.CaptionFontFamily.Source;
            LabelFontSize = (int)SystemFonts.CaptionFontSize;
        }
        public PgStyleLableM(PgTableBaseM table, PgStyleSetsM set)
        {
            _isNewTable = true;
            _table = table;
            _set = set;
        }
        public PgStyleLableM(int id, PgTableBaseM table, PgStyleSetsM set)
        {
            _id = id;
            _table = table;
            _set = set;
        }
        #endregion // Конструктор

        #region Свойства
        /// <summary>
        /// Признак нового элемента
        /// </summary>
        public bool IsNewTable
        {
            get { return _isNewTable; }
        }
        /// <summary>
        /// Идентификатор подписи
        /// </summary>
        public int Id
        { get { return _id; } }
        /// <summary>
        /// Ссылка на таблицу
        /// </summary>
        public PgTableBaseM Table
        { get { return _table; } }
        /// <summary>
        /// Ссылка на набор
        /// </summary>
        public PgStyleSetsM Set
        { get { return _set; } }
        /// <summary>
        /// Маска вывода подписи
        /// </summary>
        public string LableMask
        {
            get { return _lableMask; }
            set { OnPropertyChanged(ref _lableMask, value, () => this.LableMask); }
        }
        /// <summary>
        /// Отображать рамку для подписи объектов слоя
        /// </summary>
        public Boolean LabelShowFrame
        {
            get { return _labelShowFrame; }
            set { OnPropertyChanged(ref _labelShowFrame, value, () => this.LabelShowFrame); }
        }
        /// <summary>
        /// Цвет рамки для подписи объектов
        /// </summary>
        public UInt32 LabelFrameColor
        {
            get { return _labelFrameColor; }
            set { OnPropertyChanged(ref _labelFrameColor, value, () => this.LabelFrameColor); }
        }
        /// <summary>
        /// Отображать подпись параллельно объекту (если тип геомтерии Line)
        /// </summary>
        public Boolean LabelParallel
        {
            get { return _labelParallel; }
            set { OnPropertyChanged(ref _labelParallel, value, () => this.LabelParallel); }
        }
        /// <summary>
        /// Могут ли подписи перекрываться
        /// </summary>
        public Boolean LabelOverlap
        {
            get { return _labelOverlap; }
            set { OnPropertyChanged(ref _labelOverlap, value, () => this.LabelOverlap); }
        }
        /// <summary>
        /// Использовать границы видимости для подписи объектов
        /// </summary>
        public Boolean LabelUseBounds
        {
            get { return _labelUseBounds; }
            set { OnPropertyChanged(ref _labelUseBounds, value, () => this.LabelUseBounds); }
        }
        /// <summary>
        /// Минимальный масштаб при котором отображается подпись
        /// </summary>
        public UInt32 LabelMinScale
        {
            get { return _labelMinScale; }
            set { OnPropertyChanged(ref _labelMinScale, value, () => this.LabelMinScale); }
        }
        /// <summary>
        /// Максимальный масштаб при котором отображается подпись
        /// </summary>
        public UInt32 LabelMaxScale
        {
            get { return _labelMaxScale; }
            set { OnPropertyChanged(ref _labelMaxScale, value, () => this.LabelMaxScale); }
        }
        /// <summary>
        /// Сдвиг подписи
        /// </summary>
        public Int32 LabelOffset
        {
            get { return _labelOffset; }
            set { OnPropertyChanged(ref _labelOffset, value, () => this.LabelOffset); }
        }
        /// <summary>
        /// Отображать подпись в соответствие с масштабом карты
        /// </summary>
        public Boolean LabelUseGraphicUnits
        {
            get { return _labelUseGraphicUnits; }
            set { OnPropertyChanged(ref _labelUseGraphicUnits, value, () => this.LabelUseGraphicUnits); }
        }
        /// <summary>
        /// Название шрифта для подписи
        /// </summary>
        public String LabelFontName
        {
            get { return _labelFontName; }
            set { OnPropertyChanged(ref _labelFontName, value, () => this.LabelFontName); }
        }
        /// <summary>
        /// Цвет шрифта подписи
        /// </summary>
        public UInt32 LabelFontColor
        {
            get { return _labelFontColor; }
            set { OnPropertyChanged(ref _labelFontColor, value, () => this.LabelFontColor); }
        }
        /// <summary>
        /// Размер шрифта подписи
        /// </summary>
        public Int32 LabelFontSize
        {
            get { return _labelFontSize; }
            set { OnPropertyChanged(ref _labelFontSize, value, () => this.LabelFontSize); }
        }
        /// <summary>
        /// Зачеркнута ли подпись
        /// </summary>
        public Boolean LabelFontStrikeout
        {
            get { return _labelFontStrikeout; }
            set { OnPropertyChanged(ref _labelFontStrikeout, value, () => this.LabelFontStrikeout); }
        }
        /// <summary>
        /// Отображается ли подпись курсивом
        /// </summary>
        public Boolean LabelFontItalic
        {
            get { return _labelFontItalic; }
            set { OnPropertyChanged(ref _labelFontItalic, value, () => this.LabelFontItalic); }
        }
        /// <summary>
        /// Подчеркивается ли подпись
        /// </summary>
        public Boolean LabelFontUnderline
        {
            get { return _labelFontUnderline; }
            set { OnPropertyChanged(ref _labelFontUnderline, value, () => this.LabelFontUnderline); }
        }
        /// <summary>
        /// Выводится ли подпись жирным шрифтом
        /// </summary>
        public Boolean LabelFontBold
        {
            get { return _labelFondBold; }
            set { OnPropertyChanged(ref _labelFondBold, value, () => this.LabelFontBold); }
        }
        /// <summary>
        /// Отображать подпись для объектов слоя
        /// </summary>
        public Boolean LabelShowLabel
        {
            get { return _labelShowLabel; }
            set { OnPropertyChanged(ref _labelShowLabel, value, () => this.LabelShowLabel); }
        }
        #endregion // Свойства

        #region Методы
        private String parseResult(String text)
        {
            int state = 0;
            // state = 0 ->  ?
            // state = 1 -> '{' 
            String result = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (state == 0)
                {
                    if (text[i] == '{') { state = 1; result += "(("; }
                    else if (text[i] == '+') { result += "||"; }
                    else { result += text[i]; }
                }
                else if (state == 1)
                {
                    if (text[i] == '}')
                    {
                        state = 0;
                        result += ")::text)";
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

        private bool IsCorrectExpression(String labelmask, ref String message)
        {
            if (labelmask.Contains("SELECT") || labelmask.Contains("DROP") || labelmask.Contains("INSERT") || labelmask.Contains("UPDATE") || labelmask.Contains("DELETE"))
            {
                message = "Вы используете недопустимое выражение";
                return false;
            }

            if (Table != null)
            {
                SqlWork sqlCmd = new Rekod.Services.SqlWork((Table.Source as PgVM.PgDataRepositoryVM).Connect, true);
                sqlCmd.sql = string.Format("SELECT {0} FROM {1}.{2} LIMIT 1", labelmask, Table.SchemeName, Table.Name);
                try
                {
                    sqlCmd.ExecuteReader(null);
                    if (sqlCmd.CanRead())
                    {
                        message = sqlCmd.GetValue(0).ToString();
                    }
                    else
                    {
                        message = "Предпросмотр невозможен. Похоже в таблице этого слоя нет ни одной записи";
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

                        String result = parseResult(labelExpression);

                        if (result.Replace(" ", "") == "")
                        {
                            MessageBox.Show(@"Выражение не должно быть пустым");
                            return;
                        }
                        String message = "";

                        IsCorrectExpression(result, ref message);
                        if (!String.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    };
            }
        }
        public Action<object> ShowHelpAction
        {
            get
            {
                return param =>
                    {
                        HelpLableFrm frm = new HelpLableFrm(System.Windows.Forms.Application.StartupPath + "\\help_lable.mht");
                        frm.Text = Rekod.Properties.Resources.LabelControl_Help;
                        frm.Show();
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
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;
                        
                        TextBox TextBoxLabelExpression = commParams[0] as TextBox;
                        ComboBox ComboBoxAttachWith = commParams[1] as ComboBox;

                        int splitPlace = TextBoxLabelExpression.SelectionStart;
                        String leftPart = TextBoxLabelExpression.Text.Substring(0, splitPlace);
                        String rightPart = TextBoxLabelExpression.Text.Substring(splitPlace);
                        if (ComboBoxAttachWith.SelectedIndex != -1)
                        {
                            TextBoxLabelExpression.Text = leftPart + (((leftPart != "") ? "+" : "") + "{" + ComboBoxAttachWith.Text + "}") + rightPart;
                        }
                        else
                        {
                            TextBoxLabelExpression.Text = leftPart + (((leftPart != "") ? "+" : "") + "{[" + ComboBoxAttachWith.Text + "]}") + rightPart;
                        }
                    };
            }
        }
        public Action<object> AttachWithArithExprAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;

                        TextBox TextBoxLabelExpression = commParams[0] as TextBox;
                        TextBox TextBoxArithmeticExpression = commParams[1] as TextBox;

                        int splitPlace = TextBoxLabelExpression.SelectionStart;
                        String leftPart = TextBoxLabelExpression.Text.Substring(0, splitPlace);
                        String rightPart = TextBoxLabelExpression.Text.Substring(splitPlace);

                        if (TextBoxArithmeticExpression.Text.Replace(" ", "") == "")
                        {
                            MessageBox.Show(@"Выражение не должно быть пустым");
                            return;
                        }
                        if (TextBoxArithmeticExpression.Text.Contains("SELECT") || TextBoxArithmeticExpression.Text.Contains("DROP")
                            || TextBoxArithmeticExpression.Text.Contains("INSERT") || TextBoxArithmeticExpression.Text.Contains("UPDATE")
                            || TextBoxArithmeticExpression.Text.Contains("DELETE"))
                        {
                            MessageBox.Show(@"Вы используете недопустимое выражение");
                            return;
                        }

                        if (Table != null)
                        {
                            SqlWork sqlCmd = new SqlWork((Table.Source as PgVM.PgDataRepositoryVM).Connect, true);
                            sqlCmd.sql = string.Format("SELECT {0} FROM {1}.{2} LIMIT 1", TextBoxArithmeticExpression.Text, Table.SchemeName, Table.Name);
                            try
                            {
                                sqlCmd.ExecuteNonQuery(null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(@"Вы ввели некорректное выражение");
                                return;
                            }
                            finally
                            {
                                sqlCmd.Close();
                            }

                            TextBoxLabelExpression.Text = leftPart + (((leftPart.Replace(" ", "") != "") ? "+" : "") + "{" + TextBoxArithmeticExpression.Text + "}") + rightPart;
                        }
                    };
            }
        }
        public Action<object> AttachWithOperandAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;
                        TextBox TextBoxArithmeticExpression = commParams[0] as TextBox;
                        ComboBox ComboBoxOperand = commParams[1] as ComboBox;
                        TextBoxArithmeticExpression.Text += ComboBoxOperand.Text;
                    };
            }
        }
        public Action<object> OperandOperationAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;
                        Button OperandOperationButton = commParams[0] as Button;
                        OperandOperationButton.ContextMenu.IsOpen = true; 
                    };
            }
        }
        public Action<object> MenuItemOperationAction
        {
            get
            {
                return param =>
                    {
                        CommandEventParameter commEvtParam = param as CommandEventParameter;
                        List<object> commParams = commEvtParam.CommandParameter as List<object>;
                        System.Windows.Controls.MenuItem MenuItemAdd = commParams[0] as System.Windows.Controls.MenuItem;
                        System.Windows.Controls.MenuItem MenuItemSubstract = commParams[1] as System.Windows.Controls.MenuItem;
                        System.Windows.Controls.MenuItem MenuItemMultiply = commParams[2] as System.Windows.Controls.MenuItem;
                        System.Windows.Controls.MenuItem MenuItemDivide = commParams[3] as System.Windows.Controls.MenuItem;

                        TextBox TextBoxArithmeticExpression = commParams[4] as TextBox;
                        ComboBox ComboBoxOperand = commParams[5] as ComboBox;

                        if (commEvtParam.EventSender == MenuItemAdd)
                        {
                            TextBoxArithmeticExpression.Text += "+" + ComboBoxOperand.Text;
                        }
                        else if (commEvtParam.EventSender == MenuItemSubstract)
                        {
                            TextBoxArithmeticExpression.Text += "-" + ComboBoxOperand.Text;
                        }
                        else if (commEvtParam.EventSender == MenuItemMultiply)
                        {
                            TextBoxArithmeticExpression.Text += "*" + ComboBoxOperand.Text;
                        }
                        else if (commEvtParam.EventSender == MenuItemDivide)
                        {
                            TextBoxArithmeticExpression.Text += "/" + ComboBoxOperand.Text;
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
        public Action<object> FontDialogAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    List<object> commParams = commEvtParam.CommandParameter as List<object>;
                    TextBox TextBoxFontName = commParams[0] as TextBox;
                    TextBox TextBoxFontSize = commParams[1] as TextBox;
                    
                    using (System.Windows.Forms.FontDialog fontDialog = new System.Windows.Forms.FontDialog())
                    {
                        System.Drawing.FontStyle fs = System.Drawing.FontStyle.Regular;

                        if (LabelFontBold) fs = fs | System.Drawing.FontStyle.Bold;
                        if (LabelFontItalic) fs = fs | System.Drawing.FontStyle.Italic;
                        if (LabelFontStrikeout) fs = fs | System.Drawing.FontStyle.Strikeout;
                        if (LabelFontUnderline) fs = fs | System.Drawing.FontStyle.Underline;

                        if (!String.IsNullOrWhiteSpace(TextBoxFontName.Text))
                        {
                            System.Drawing.Font font = new System.Drawing.Font(TextBoxFontName.Text, Convert.ToInt32(TextBoxFontSize.Text), fs);
                            fontDialog.Font = font;
                        }

                        if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            TextBoxFontName.Text = fontDialog.Font.Name;
                            TextBoxFontSize.Text = ((int)(fontDialog.Font.Size)).ToString();
                            LabelFontStrikeout = fontDialog.Font.Strikeout;
                            LabelFontItalic = fontDialog.Font.Italic;
                            LabelFontUnderline = fontDialog.Font.Underline;
                            LabelFontBold = fontDialog.Font.Bold;
                        }
                    }
                };
            }
        }

        public Action<object> BindingGroupLoadedAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupErrorAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    ValidationErrorEventArgs errorArgs = commEvtParam.EventArgs as ValidationErrorEventArgs;
                    if (errorArgs.Action == ValidationErrorEventAction.Added)
                    {
                        MessageBox.Show(errorArgs.Error.ErrorContent.ToString());
                    }
                };
            }
        }
        public Action<object> BindingGroupCancelAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    BindingGroup bindGroup = commEvtParam.CommandParameter as BindingGroup;
                    bindGroup.CancelEdit();
                    bindGroup.BeginEdit();
                };
            }
        }
        public Action<object> BindingGroupSaveAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    List<object> commParams = commEvtParam.CommandParameter as List<object>;
                    BindingGroup bindGroup = commParams[0] as BindingGroup;
                    TextBox TextBoxLabelExpression = commParams[1] as TextBox;

                    String result = parseResult(TextBoxLabelExpression.Text);
                    String message = "";
                    if (!String.IsNullOrEmpty(result) && !IsCorrectExpression(result, ref message))
                    {
                        if (!String.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    }
                    else if (bindGroup.CommitEdit())
                    {
                        if (Table != null && Table.LabelStyle != null)
                        {
                            (Table.Source as PgVM.PgDataRepositoryVM).DBSaveLabelStyle(Table.LabelStyle);
                            if (Table.IsVisible)
                            {
                                Table.IsVisible = false;
                                Table.IsVisible = true;
                            }
                        }
                        bindGroup.BeginEdit();
                    }
                };
            }
        }
        #endregion Действия
    }
}