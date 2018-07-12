using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.DataAccess.AbstractSource.Model;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Это класс, который содержит в себе пользователя и список его прав
    /// </summary>
    public class PgUserRightsM : ViewModelBase, IComparable
    {
        #region Поля
        private bool isChanged;
        private int? id;
        private bool _canRead;
        private bool _canWrite;
        private PgUserM _user;
        private PgTableBaseM _table;
        #endregion Поля

        #region Конструкторы
        public PgUserRightsM()
        {
            PropertyChanged += PgUserRightsM_PropertyChanged;
            UpdateRef = true;
        }
        #endregion Конструкторы

        #region Свойства
        public int? ID
        {
            get { return id; }
            set { OnPropertyChanged(ref id, value, () => this.ID); }
        }
        public bool CanRead
        {
            get { return _canRead; }
            set
            {
                if (_canRead == value)
                {
                    return;
                }
                OnPropertyChanged(ref _canRead, value, () => this.CanRead);
                   if (value == false)
                       OnPropertyChanged(ref _canWrite, value, () => this.CanWrite);
                   if (RefTables != null && UpdateRef && value)
                   {
                       foreach (var table in RefTables)
                       {
                           if (table.CanRead != value)
                               table.CanRead = value;
                       }
                   }
            }
        }
        public bool CanWrite
        {
            get { return _canWrite; }
            set
            {
                if (_canWrite == value)
                {
                    return;
                }
                OnPropertyChanged(ref _canWrite, value, () => this.CanWrite);
                if (value == true)
                    OnPropertyChanged(ref _canRead, value, () => this.CanRead);
                if (RefTables != null && UpdateRef && value)
                {
                    foreach (var table in RefTables)
                    {
                        if (table.CanWrite != value)
                            table.CanWrite = value;
                    }
                }
            }
        }
        public PgUserM User
        {
            get { return _user; }
            set { OnPropertyChanged(ref _user, value, () => this.User); }
        }
        public PgTableBaseM Table
        {
            get { return _table; }
            set 
            {
                if (value != null)
                {
                    TableId = value.Id;
                    TableName = value.Name;
                    TableScheme = value.SchemeName;
                    TableText = value.Text;
                }
                OnPropertyChanged(ref _table, value, () => this.Table); 
            }
        }
        public int TableId { get; set; }
        public String TableName { get; set; }
        public String TableScheme { get; set; }
        public String TableText { get; set; }
        public ETableType TableType { get; set; }
        public List<PgUserRightsM> RefTables { get; set; }
        public bool UpdateRef { get; set; }
        public String UserFullName
        {
            get { return User != null ? User.NameFull : null; }
        }
        public String UserLogin
        {
            get { return User != null ? User.Login : null; }
        }
        public bool IsChanged { get { return isChanged; } }
        #endregion Свойства

        #region Обработчики
        void PgUserRightsM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            isChanged = true;
        }
        #endregion Обработчики

        #region Методы
        public void Reset()
        {
            isChanged = false;
        }
        public int CompareTo(object obj)
        {
            if (obj is PgUserRightsM && Table != null && Table.Name != null)
                return Table.Name.CompareTo(((PgUserRightsM)obj).Table.Name);
            else return 0;
        }
        #endregion Методы
    }
}