using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Interfaces;

namespace Rekod
{
    public partial class FindBox2 : UserControl
    {
        UcTableObjects _form;//parent

        readonly string[] _findList = new string[] { Rekod.Properties.Resources.FindBox2_Empty, Rekod.Properties.Resources.FindBox2_IsNotEmpty };
        readonly string[] _findListText = new string[] { Rekod.Properties.Resources.FindBox2_Contains, 
            Rekod.Properties.Resources.FindBox2_StartsWith, Rekod.Properties.Resources.FindBox2_DoesNotContain, "="};
        readonly string[] _findListNum = new string[] { "=", ">", "<", "<>", ">=", "<=" };

        private bool _removeFunc = false;        //это для того чтобы после добавления нового поиска значек и функция менялись на удаление
        private UcTableObjects.FieldInfoFull _currentFilter;
        List<UcTableObjects.FieldInfoFull> _listIS;
        public List<UcTableObjects.FieldInfoFull> ListIS
        {
            get
            {
                return _listIS ?? null;
            }
            set
            {
                InThread(() =>
                {
                    cbListColumn.Items.Clear();
                    cbListColumn.Items.AddRange(value.ToArray());
                    foreach (var t in value)
                    {
                        if (_currentFilter == null)
                        { cbListColumn.SelectedItem = _currentFilter = t; break; }
                        else
                            if (_currentFilter.idField == t.idField)
                            {
                                cbListColumn.SelectedItem = _currentFilter = t;
                                this.CbListColumnSelectedIndexChanged(cbListColumn, null);
                                break;
                            }
                    }
                });

                _listIS = value;

            }
        }

        private bool _isText = true;

        private bool isText
        {
            get { return _isText; }
            set
            {
                if (_isText != value)
                {
                    _isText = value;
                    txtValue.Visible = _isText;
                    dateTimePicker1.Visible = !_isText;
                }
            }
        }

        private void InThread(MethodInvoker mth)
        {
            try
            {
                if (IsDisposed)
                    return;
                if (InvokeRequired)
                    Invoke(mth);
                else
                    mth();
            }
            catch
            { }
        }
        public FindBox2()
        {
            InitializeComponent();

            cbListFilter.Items.Clear();
            cbListFilter.Items.AddRange(_findListText);
            cbListFilter.Items.AddRange(_findList);
            cbListFilter.SelectedIndex = 0;

        }
        public FindBox2(UcTableObjects.FieldInfoFull key, object value)
            : this()
        {
            if (key == null) return;
            _currentFilter = key;
            cbListFilter.Items.Clear();
            cbListFilter.Items.AddRange(_findListNum);
            cbListFilter.Items.AddRange(_findList);
            cbListFilter.SelectedIndex = 0;
            switch (key.TypeField)
            {
                case TypeField.Default:
                    txtValue.Text = value.ToString();
                    break;
                case TypeField.Integer:
                    txtValue.Text = value.ToString();
                    break;
                case TypeField.Text:
                    txtValue.Text = value.ToString();
                    break;
                case TypeField.Date:
                    dateTimePicker1.Value = Convert.ToDateTime(value);
                    break;
                case TypeField.DateTime:
                    dateTimePicker1.Value = Convert.ToDateTime(value);
                    break;
                case TypeField.Numeric:
                    txtValue.Text = value.ToString();
                    break;
                default:
                    txtValue.Text = value.ToString();
                    break;
            }

        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            var control = this.Parent as Control;
            while (control != null)
            {
                if (control is UcTableObjects)
                {
                    this._form = control as UcTableObjects;
                    break;
                }
                else
                    control = control.Parent;
            }
            if (control == null)
            {
                this._form = null;
            }
        }

        private void btnAddRemoveFilter_Click(object sender, EventArgs e)
        {
            if (_form != null)
                if (!_removeFunc)
                {//add
                    RemoveFunc = true;
                    btnSearch.Visible = false;
                    var nextFindBox = new FindBox2();
                    _form.AddFilter(nextFindBox);

                }
                else
                {//remove
                    _form.RemoveFilter(this);
                }
        }
        public bool VisibolFindButton
        {
            get { return btnSearch.Visible; }
            set { btnSearch.Visible = value; }
        }
        public bool RemoveFunc
        {
            get
            { return _removeFunc; }
            set
            {
                _removeFunc = value;
                btnAddRemoveFilter.BackgroundImage = value ? Properties.Resources.remove : Properties.Resources.add;
            }
        }
        private void CbListColumnSelectedIndexChanged(object sender, EventArgs e)
        {
            _currentFilter = cbListColumn.SelectedItem as UcTableObjects.FieldInfoFull;
            setValid();
            var currentText = cbListFilter.Text;
            cbListFilter.Items.Clear();
            if (_currentFilter == null) return;
            //DateTime? date;
            if (_currentFilter.is_reference)
            {
                SetObject(true, GetValue());
                cbListFilter.Items.AddRange(_findListText);
            }
            else
                switch (_currentFilter.TypeField)
                {
                    case TypeField.Integer:
                        SetObject(true, GetValue());
                        cbListFilter.Items.AddRange(_findListNum);
                        break;
                    case TypeField.Numeric:
                        SetObject(true, GetValue());
                        cbListFilter.Items.AddRange(_findListNum);
                        break;
                    case TypeField.Date:
                        SetObject(false, GetValue());
                        cbListFilter.Items.AddRange(_findListNum);
                        break;
                    case TypeField.DateTime:
                        SetObject(false, GetValue());
                        cbListFilter.Items.AddRange(_findListNum);
                        break;
                    default:
                        SetObject(true, GetValue());
                        cbListFilter.Items.AddRange(_findListText);
                        break;
                }
            cbListFilter.Items.AddRange(_findList);
            cbListFilter.Text = currentText;
            if (cbListFilter.SelectedIndex == -1)
                cbListFilter.SelectedIndex = 0;

        }

        void SetObject(bool isTextBox, object value)
        {
            var textbox = pValue.Controls[1] as TextBox;
            var timePicker = pValue.Controls[0] as DateTimePicker;
            isText = isTextBox;
        }
        object GetValue()
        {
            object result = null;
            if (isText)
            {
                var textBox = pValue.Controls[1] as TextBox;
                if (textBox != null) result = textBox.Text;
            }
            else
            {
                var dateTimePicker = pValue.Controls[0] as DateTimePicker;
                if (dateTimePicker != null) result = dateTimePicker.Value;
            }


            return result;
        }

        private void cbListFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            pValue.Enabled = !_findList.Contains(cbListFilter.Text);
        }
        public FindRequest2 GetFilter()
        {
            object value = null;
            var isFindList = false;
            InThread(() => isFindList = _findList.Contains(cbListFilter.Text));
            if (!isFindList)
            {
                try
                {
                    if (_currentFilter.is_reference)
                        value = GetValue();
                    else
                        switch (_currentFilter.TypeField)
                        {
                            case TypeField.Date:
                                value = GetValue(); break;
                            case TypeField.DateTime:
                                value = GetValue(); break;
                            case TypeField.Integer:
                                value = Convert.ToInt32(GetValue()); break;
                            case TypeField.Numeric:
                                value = Convert.ToDouble(GetValue()); break;
                            case TypeField.Text:
                                value = GetValue();
                                break;
                            case TypeField.Default:
                                value = GetValue();
                                break;
                        }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); return null; }

                switch (_currentFilter.TypeField)
                {
                    case TypeField.Date:
                        if (!CheckValue(value))
                            return null;
                        break;
                    case TypeField.DateTime:
                        if (!CheckValue(value))
                            return null;
                        break;
                    case TypeField.Integer:
                        if (!CheckValue(value))
                            return null;
                        break;
                    case TypeField.Numeric:
                        if (!CheckValue(value))
                            return null;
                        break;
                    case TypeField.Text:
                        if (!CheckValueForText(value))
                            return null;
                        break;
                    case TypeField.Default:
                        if (!CheckValueForText(value))
                            return null;
                        break;
                }
            }
            var filterText = "";
            InThread(() => filterText = cbListFilter.Text);

            var result = new FindRequest2(_currentFilter, value, filterText);
            Debug.WriteLine(result.ToString());
            return result;
        }
        private bool CheckValue(object value)
        {
            if (value == null)
                return false;
            if (value is string && value.ToString() == "")
            {
                return false;
            }
            return true;
        }
        private bool CheckValueForText(object value)
        {
            if (value == null)
                return false;
            return true;
        }
        private void txtInt_KeyPress(object sender, KeyPressEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt != null)
            {
                if (e.KeyChar == '-')
                {
                    txt.Text = txt.Text.StartsWith("-") ? txt.Text.Remove(0, 1) : txt.Text.Insert(0, "-");
                    e.Handled = true;
                    return;
                }
            }
            if (char.IsControl(e.KeyChar))
                return;
            if (!char.IsNumber(e.KeyChar))
                e.Handled = true;
        }
        private void txtDouble_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                if (sender != null)
                {
                    var txt = sender as TextBox;
                    if (txt != null)
                    {
                        if (txt.Text.LastIndexOf(',') > -1)
                            return;
                    }
                }
            }
            else
                txtInt_KeyPress(sender, e);
        }

        private void Value_KeyUp(object sender, KeyEventArgs e)
        {
            setValid();
            if (_form != null && e.KeyData == Keys.Enter)
            {
               // _form.ApplyFilter();
                btnSearch.Focus();
                btnSearch_Click(btnSearch, null);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            setValid();
            if (_form != null) _form.ApplyFilter();
        }
        private void setValid()
        {
            switch (_currentFilter.TypeField)
            {
                case TypeField.Integer:
                    try
                    {
                        Convert.ToInt32(txtValue.Text);
                        txtValue.BackColor = Color.White;
                    }
                    catch
                    {
                        if (!_findList.Contains(cbListFilter.SelectedItem))
                            txtValue.BackColor = Color.LightPink;
                        else
                            txtValue.BackColor = Color.White;
                    }
                    break;
                case TypeField.Numeric:
                    try
                    {
                        Convert.ToDouble(txtValue.Text);
                        txtValue.BackColor = Color.White;
                    }
                    catch
                    {
                        if (!_findList.Contains(cbListFilter.SelectedItem))
                            txtValue.BackColor = Color.LightPink;
                        else
                            txtValue.BackColor = Color.White;
                    }
                    break;
                default:
                    txtValue.BackColor = Color.White;
                    break;
            }
        }
        private void FindBox2_Load(object sender, EventArgs e)
        {
            this.cbListColumn.SelectedIndexChanged -= new System.EventHandler(this.CbListColumnSelectedIndexChanged);
            this.cbListFilter.SelectedIndexChanged -= new System.EventHandler(this.cbListFilter_SelectedIndexChanged);

            this.cbListColumn.SelectedIndexChanged += new System.EventHandler(this.CbListColumnSelectedIndexChanged);
            this.cbListFilter.SelectedIndexChanged += new System.EventHandler(this.cbListFilter_SelectedIndexChanged);
        }
        public void SetFilter(SQLiteSettings.FilterElementModel filter, List<UcTableObjects.FieldInfoFull> fList)
        {
            fieldInfo fi = classesOfMetods.getFieldInfo(Convert.ToInt32(filter.Column));
            switch (fi.type)
            {
                case 1:
                    txtValue.Text = filter.Value.ToString();
                    break;
                case 2:
                    txtValue.Text = filter.Value.ToString();
                    break;
                case 3:
                    dateTimePicker1.Value = Convert.ToDateTime(filter.Value);
                    break;
                case 4:
                    dateTimePicker1.Value = Convert.ToDateTime(filter.Value);
                    break;
                case 6:
                    txtValue.Text = filter.Value.ToString();
                    break;
                default:
                    txtValue.Text = filter.Value.ToString();
                    break;
            }

            _currentFilter = fList.Find(w => w.idField == fi.idField);
            this.ListIS = fList;
            SetFilterType(filter);
        }
        private void SetFilterType(SQLiteSettings.FilterElementModel filter)
        {
            switch (filter.Type)
            {
                case Rekod.SQLiteSettings.TypeOperation.More:
                    cbListFilter.SelectedIndex = FindIndex(">");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.Less:
                    cbListFilter.SelectedIndex = FindIndex("<");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.Equal:
                    cbListFilter.SelectedIndex = FindIndex("=");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.NotEqual:
                    cbListFilter.SelectedIndex = FindIndex("<>");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.MoreOrEqual:
                    cbListFilter.SelectedIndex = FindIndex(">=");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.LessOrEqual:
                    cbListFilter.SelectedIndex = FindIndex("<=");
                    break;
                case Rekod.SQLiteSettings.TypeOperation.Empty:
                    cbListFilter.SelectedIndex = FindIndex(Rekod.Properties.Resources.FindBox2_Empty);
                    break;
                case Rekod.SQLiteSettings.TypeOperation.NotEmpty:
                    cbListFilter.SelectedIndex = FindIndex(Rekod.Properties.Resources.FindBox2_IsNotEmpty);
                    break;
                case Rekod.SQLiteSettings.TypeOperation.Included:
                    cbListFilter.SelectedIndex = FindIndex(Rekod.Properties.Resources.FindBox2_Contains);
                    break;
                case Rekod.SQLiteSettings.TypeOperation.IncludesFirst:
                    cbListFilter.SelectedIndex = FindIndex(Rekod.Properties.Resources.FindBox2_StartsWith);
                    break;
                case Rekod.SQLiteSettings.TypeOperation.NotIncluded:
                    cbListFilter.SelectedIndex = FindIndex(Rekod.Properties.Resources.FindBox2_DoesNotContain);
                    break;
                case Rekod.SQLiteSettings.TypeOperation.Init:
                    cbListFilter.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }
        private int FindIndex(string val)
        {
            for (int i = 0; i < cbListFilter.Items.Count; i++)
            {
                if (cbListFilter.Items[i].ToString() == val)
                {
                    return i;
                }
            }
            return -1;
        }
        private SQLiteSettings.TypeOperation GetTypeOperation(string val)
        {
            switch (val)
            {
                case ">":
                    return SQLiteSettings.TypeOperation.More;
                case "<":
                    return SQLiteSettings.TypeOperation.Less;
                case "=":
                    return SQLiteSettings.TypeOperation.Equal;
                case ">=":
                    return SQLiteSettings.TypeOperation.MoreOrEqual;
                case "<=":
                    return SQLiteSettings.TypeOperation.LessOrEqual;
                case "<>":
                    return SQLiteSettings.TypeOperation.NotEqual;
                default:
                    if (val == Rekod.Properties.Resources.FindBox2_Empty)
                    {
                        return SQLiteSettings.TypeOperation.Empty;
                    }
                    else if (val == Rekod.Properties.Resources.FindBox2_IsNotEmpty)
                    {
                        return SQLiteSettings.TypeOperation.NotEmpty;
                    }
                    else if (val == Rekod.Properties.Resources.FindBox2_Contains)
                    {
                        return SQLiteSettings.TypeOperation.Included;
                    }
                    else if (val == Rekod.Properties.Resources.FindBox2_StartsWith)
                    {
                        return SQLiteSettings.TypeOperation.IncludesFirst;
                    }
                    else if (val == Rekod.Properties.Resources.FindBox2_DoesNotContain)
                    {
                        return SQLiteSettings.TypeOperation.NotIncluded;
                    }
                    return SQLiteSettings.TypeOperation.Init;
            }
        }
        public SQLiteSettings.FilterElementModel GetFilterElementModel()
        {
            if (_currentFilter.TypeField != TypeField.Default)
            {
                SQLiteSettings.FilterElementModel temp = new SQLiteSettings.FilterElementModel(-1, _currentFilter.idField.ToString(),
                    GetTypeOperation(cbListFilter.Text), GetValue().ToString(), false);
                return temp;
            }
            else
            {
                SQLiteSettings.FilterElementModel temp = new SQLiteSettings.FilterElementModel(-1, _currentFilter.idField.ToString(),
    GetTypeOperation(cbListFilter.Text), GetValue().ToString(), false);
                return temp;
            }
        }
    }

    public class FindRequest2
    {
        public readonly UcTableObjects.FieldInfoFull Col;
        public readonly object FindValue;
        public readonly string TypeFr;

        public FindRequest2(UcTableObjects.FieldInfoFull coll, object findd, string type)
        {
            Col = coll;
            FindValue = findd;
            TypeFr = type;
        }
#if DEBUG
        public override string ToString()
        {
            return string.Format(Rekod.Properties.Resources.FindBox2_Colmn +
                ": \t{0}\n" + Rekod.Properties.Resources.FindBox2_Text + ": \t{1}\n" + Rekod.Properties.Resources.FindBox2_Type +
                ": \t{2}\n\n", Col.nameDB, FindValue, TypeFr);
        }
#endif

    }
}
