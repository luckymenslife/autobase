using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Interfaces;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.Model
{
    public class AttributesOfObjectModel : ViewModelBase
    {
        public Interfaces.tablesInfo RefTable { get; private set; }
        readonly Interfaces.fieldInfo _field;
        private VariantsForAttributesModel _refValue;

        public AttributesOfObjectModel(Interfaces.fieldInfo field, Interfaces.tablesInfo refTable = null)
        {
            RefTable = refTable;
            _field = field;

        }

        object _value;

        public int IdField
        {
            get { return _field.idField; }
            set { _field.idField = value; OnPropertyChanged(() => this.IdField); }
        }
        public string Text
        {
            get { return _field.nameMap; }
            set { _field.nameMap = value; OnPropertyChanged(() => this.Text); }
        }
        public string NameBD
        {
            get { return _field.nameDB; }
            set { _field.nameDB = value; OnPropertyChanged(() => this.NameBD); }
        }

        public string Lable
        {
            get { return _field.nameLable; }
            set { _field.nameLable = value; OnPropertyChanged(() => this.Lable); }
        }

        public int Table
        {
            get { return _field.idTable; }
            set { _field.idTable = value; OnPropertyChanged(() => this.Table); }
        }
        public TypeField Type
        {
            get { return _field.TypeField; }
            set { _field.TypeField = value; OnPropertyChanged(() => this.Type); }
        }

        public bool IsReadOnly
        {
            get { return _field.read_only; }
        }
        public bool IsVisible
        {
            get { return _field.visible; }
            set { _field.visible = value; OnPropertyChanged(() => this.IsVisible); }
        }

        public bool IsStyle
        {
            get { return _field.is_style; }
        }
        public bool IsReference
        {
            get { return _field.is_reference; }
        }
        public bool IsInterval
        {
            get { return _field.is_interval; }
            set { _field.is_interval = value; }
        }

        public int? ref_table
        { get { return _field.ref_table; } }
        public int? ref_field
        { get { return _field.ref_field; } }
        public int? ref_field_end
        { get { return _field.ref_field_end; } }
        public int? ref_field_name
        { get { return _field.ref_field_name; } }

        public Interfaces.fieldInfo refField
        {
            get
            {
                return (RefTable == null) ? null :
                    RefTable.ListField.FirstOrDefault(f => f.idField == ref_field);
            }
        }
        public Interfaces.fieldInfo refFieldEnd
        {
            get
            {
                return (RefTable == null) ? null :
                    RefTable.ListField.FirstOrDefault(f => f.idField == ref_field_end);
            }
        }
        public Interfaces.fieldInfo refFieldName
        {
            get
            {
                return (RefTable == null) ? null :
                    RefTable.ListField.FirstOrDefault(f => f.idField == ref_field_name);
            }
        }



        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(() => this.Value);
            }
        }


        public VariantsForAttributesModel RefValue
        {
            get { return _refValue; }
            private set
            {
                _refValue = value;
                OnPropertyChanged(() => this.RefValue);
            }
        }

        public TypeRef TypeRef
        {
            get
            {
                if (RefTable != null)
                {
                    if (_field.is_reference)
                        if (RefTable.TypeTable == TypeTable.LayerMap)
                            return TypeRef.isReferenceTable;
                        else if (_field.is_style)
                            return TypeRef.isReferenceStyle;
                        else
                            return TypeRef.isReference;
                    if (_field.is_interval)
                        if (_field.is_style)
                            return TypeRef.isIntervalStyle;
                        else
                            return TypeRef.isInterval;
                }
                return TypeRef.not;
            }
        }
        public ObservableCollection<VariantsForAttributesModel> CollectionOfVariants { get; set; }

#if DEBUG
        public override string ToString()
        {
            string result =
                string.Format("{0} value = ({1}) {3}{2}{3}", NameBD, Type, Value, (Value is string) ? "\"" : "");

            return result;
        }
#endif

    }
    public enum TypeRef
    {
        not,
        isReference,
        isReferenceTable,
        isReferenceStyle,
        isInterval,
        isIntervalStyle
    }
    public class VariantsForAttributesModel
    {
        public double IdValue { get { return IntervalMin; } }
        public object NewValue { get; private set; }

        public double IntervalMin { get; private set; }
        public double? IntervalMax { get; private set; }

        public VariantsForAttributesModel()
        {
        }
        public VariantsForAttributesModel(double idValue, object newValue)
        {
            this.IntervalMin = idValue;
            this.NewValue = newValue;

        }
        public VariantsForAttributesModel(object newValue, double intervalMin, double intervalMax)
        {
            this.NewValue = newValue;

            IntervalMin = intervalMin;
            IntervalMax = intervalMax;
        }

        public override string ToString()
        {
            return (NewValue ?? "").ToString();
        }
        public override int GetHashCode()
        {
            return IdValue.GetHashCode();
        }

        
    }
    public class StyleImagePreview
    {
        private BitmapImage _img;
        private string _name;

        public StyleImagePreview(BitmapImage img, string name)
        {
            _img = img;
            _name = name;
        }

        public string Name
        { get { return _name; } }

        public BitmapImage Img
        { get { return _img; } }
    }


}