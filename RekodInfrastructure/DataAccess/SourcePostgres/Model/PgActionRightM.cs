using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.DataAccess.SourcePostgres.ViewModel;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Класс, описывающий право пользователя на действие
    /// </summary>
    public class PgActionRightM: ViewModelBase
    {
        #region Поля
        private int? _id;
        private PgUserM _user;
        private PgActionM _action;
        private bool _allowed;
        private bool _isChanged;
        private PgDataRepositoryVM _source; 
        #endregion Поля

        #region Конструктор
        public PgActionRightM(PgUserM user, PgDataRepositoryVM source)
        {
            User = user;
            _source = source;
        }
        #endregion Конструктор

        #region Свойства
        public int? ID
        {
            get { return _id; }
            set { OnPropertyChanged(ref _id, value, () => this.ID); }
        }
        public bool Allowed
        {
            get { return _allowed; }
            set { OnPropertyChanged(ref _allowed, value, () => this.Allowed); }
        }
        public PgActionM Action
        {
            get { return _action; }
            set { OnPropertyChanged(ref _action, value, () => this.Action); }
        }
        public PgUserM User
        {
            get { return _user; }
            set { OnPropertyChanged(ref _user, value, () => this.User); }
        }
        public String UserFullName
        {
            get { return User != null ? User.NameFull : null; }
        }
        public String UserLogin
        {
            get { return User != null ? User.Login : null; }
        }

        /// <summary>
        /// Источник к которому относится пользователь
        /// </summary>
        public PgDataRepositoryVM Source
        {
            get { return _source; }
            set { OnPropertyChanged(ref _source, value, () => this.Source); }
        } 

        public bool IsChanged { get { return _isChanged; } }

        #endregion Свойства

        #region Конструкторы
        public PgActionRightM()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PgUserActionRightsM_PropertyChanged);
        }
        #endregion Конструкторы

        #region Обработчики
        void PgUserActionRightsM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _isChanged = true;
        }
        #endregion Обработчики

        #region Методы
        public void Reset()
        {
            _isChanged = false;
        }
        #endregion Методы
    }
}
