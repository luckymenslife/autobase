using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Rekod.SQLiteSettings
{
    public enum TypeOperation
    {
        More, Less, Equal, NotEqual, MoreOrEqual, LessOrEqual, Empty, NotEmpty, Included, IncludesFirst, NotIncluded, Init, Contains,
        InEnd,
        InBegin,
        NotContains
    }
    public enum TypeRelation { AND, OR, ELEMENT, UNKNOWN}
    /// <summary>Модель элемента фильтра
    /// </summary>
    public class FilterElementModel
    {
        private int _id;
        private string _column;
        private TypeOperation _type;
        private string _value;
        private bool _isReference;
        //private string _elementCode;

        public FilterElementModel(int id,string column, TypeOperation type, string value, bool isRef)
        {
            this._id = id;
            this._column = column;
            this._type = type;
            this._value = value;
            this._isReference = isRef;
        }
        public FilterElementModel()
        {
            this._type = TypeOperation.Init;
        }

        //public string ElementCode
        //{
        //    get { return _elementCode; }
        //    set{_elementCode = value;}
        //}

        [ScriptIgnoreAttribute]
        public object GetValue
        {
            get
            {
                switch (_type)
                {
                    case TypeOperation.Empty:
                        return "IS NULL";
                    case TypeOperation.NotEmpty:
                        return "IS NOT NULL";
                    case TypeOperation.Init:
                        return "";
                    default:
                        GetValueInNeedType();
                        break;
                }
                return null;
            }
        }
        [ScriptIgnoreAttribute]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public TypeOperation Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool IsReference
        {
            get { return _isReference; }
            set { _isReference = value; }
        }

        private object GetValueInNeedType()
        {
            return null;
        }
    }
    /// <summary>Модель фильтра
    /// </summary>
    public class FilterRelationModel
    {
        private TypeRelation _type;
        private FilterElementModel _element;
        private ObservableCollection<FilterRelationModel> _arguments;

        public FilterRelationModel(TypeRelation type, FilterElementModel element, ObservableCollection<FilterRelationModel> arguments)
        {
            this._type = type;
            this._element = element;
            this._arguments = arguments;
        }
        public FilterRelationModel()
        {
            this._type = TypeRelation.UNKNOWN;
        }
        public TypeRelation Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public FilterElementModel Element
        {
            get { return _element; }
            set { _element = value; }
        }
        public ObservableCollection<FilterRelationModel> Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }
    }
}
