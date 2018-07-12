using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace Rekod.FastReportClasses
{
    public class TableForReport : INotifyPropertyChanged
    {
        private int _idTable;
        private string _scheme;
        private string _nameDb;
        private string _nameMap;
        public bool _isSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        public TableForReport()
        {

        }
        public int IdTable
        {
            get { return _idTable; }
            set { _idTable = value; OnPropertyChanged("IdTable"); }
        }
        public String Scheme
        {
            get { return _scheme; }
            set { _scheme = value; OnPropertyChanged("Scheme"); }
        }
        public String NameDb
        {
            get { return _nameDb; }
            set { _nameDb = value; OnPropertyChanged("NameDb"); }
        }
        public String NameMap
        {
            get { return _nameMap; }
            set { _nameMap = value; OnPropertyChanged("NameMap"); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }
        public override string ToString()
        {
            return "#" + this.IdTable + " " + this.NameMap;
        }
        public string UserName
        {
            get { return this.ToString(); }
        }

        private void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}