using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.AbstractSource.Model
{
    public class FieldM: IFieldM
    {
        #region Поля
        private object _idTable;
        private int _id;
        private ITableBaseM _table;
        private string _name;
        private string _text;
        private EFieldType _type;
        private bool _isReadOnly;
        #endregion Поля

        #region Конструкторы
        public FieldM(int id, String name)
        {
            _id = id; _name = name;
        }
        #endregion Конструкторы

        #region Свойства
        public int Id
        {
            get { return _id; }
        }

        public object IdTable
        {
            get { return _idTable; }
        }

        public ITableBaseM Table
        {
            get { return _table; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public EFieldType Type
        {
            get { return _type; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        #endregion Свойства
    }
}