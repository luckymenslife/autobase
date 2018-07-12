using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.TableData
{
    public class DataColumn
    {
        private bool enabled_;
        private string textname_;
        private string pgname_;
        private DataColumn.enType type_;
        private object data1;
        private bool interval1;
        private int? _relTableId;
        private ValueInterval[] intvalue1;
        private bool is_not_null;

        public DataColumn(string Name, string BaseName, DataColumn.enType Type, bool Edited)
        {
            enabled_ = Edited;
            textname_ = Name;
            pgname_ = BaseName;
            type_ = Type;
            interval1 = false;
        }
        public int? RelTableId
        {
            get { return _relTableId; }
            set { _relTableId = value; }
        }
        public bool isInterval
        {
            get { return interval1; }
            set { interval1 = value; }
        }
        public bool Edited
        {
            get { return enabled_; }
            set { enabled_ = value; }
        }
        public bool IsNotNull
        {
            get { return is_not_null; }
            set { is_not_null = value; }
        }
        public ValueInterval[] Interval
        {
            get { return intvalue1; }
            set { intvalue1 = value; }
        }

        public bool isEdited
        {
            get { return enabled_; }
            set { enabled_ = value; }
        }

        public string Name
        {
            get { return textname_; }
            set { textname_ = value; }
        }

        public string BaseName
        {
            get { return pgname_; }
            set { pgname_ = value; }
        }

        public DataColumn.enType Type
        {
            get { return type_; }
            set { type_ = value; }
        }

        public object Data
        {
            get { return data1; }
            set { data1 = value; }
        }

        public override string ToString()
        {
            return textname_;
        }

        public enum enType { Integer, Text, Date, DateTime, Geometry, Numeric };

    }
}
