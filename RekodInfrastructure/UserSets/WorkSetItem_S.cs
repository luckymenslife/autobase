using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;

namespace Rekod.UserSets
{
    public class WorkSetItem_S : ViewModelBase, ICloneable
    {
        #region Поля
        private bool _isNew;
        private int _id;
        private string _name;
        private int _idUser;
        private bool _showSet;
        private WorkSets_M _model;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Новый элемент
        /// </summary>
        public bool IsNew
        { get { return _isNew; } }
        /// <summary>
        /// Id набора
        /// </summary>
        public int Id
        { get { return _id; } }
        /// <summary>
        /// Название набора
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                OnPropertyChanged(ref _name, value, () => this.Name);
                OnPropertyChanged("IsUsedName");
                OnPropertyChanged("IsEmptyName");
                OnPropertyChanged("IsValid");
                OnPropertyChanged("NameAndOwner");
            }
        }
        /// <summary>
        /// Родитель набора
        /// </summary>
        public int IdUser
        { get { return _idUser; } }
        /// <summary>
        /// Показывать набор для других пользователей
        /// </summary>
        public bool ShowSet
        {
            get { return _showSet; }
            set { OnPropertyChanged(ref _showSet, value, () => this.ShowSet); }
        }
        /// <summary>
        /// Является стандартным набором
        /// </summary>
        public bool IsDefault
        { get { return this == _model.DefaultWorkSet; } }

        /// <summary>
        /// Является стандартным набором
        /// </summary>
        public bool IsAccess
        { get { return _model.AccessChecked(this); } }

        public string NameOwner
        {
            get
            {
                var userInfo = Program.users_info.FirstOrDefault(w => w.id_user == _idUser);
                if (userInfo != null && userInfo.id_user != Program.id_user)
                {
                    return (userInfo.nameUser ?? userInfo.loginUser);
                }
                return String.Empty;
            }
        }

        public string NameAndOwner
        {
            get
            {
                string owner = NameOwner;
                if (!string.IsNullOrEmpty(owner))
                    return Name + " (" + owner + ")";
                else return Name;
            }
        }

        /// <summary>
        /// Имя уже занято 
        /// </summary>
        public bool IsUsedName
        { get { return _model.FindNameMatch(this); } }
//_name != _originalName &&

        /// <summary>
        /// Пустое имя
        /// </summary>
        public bool IsEmptyName
        { get { return String.IsNullOrWhiteSpace(_name); } }

        /// <summary>
        /// Доступное имя
        /// </summary>
        public bool IsValid
        { get { return !IsEmptyName && !IsUsedName; } }

        public bool IsCurrent
        { get { return Program.WorkSets.CurrentWorkSet.Id == Id; } }

        #endregion // Свойства

        #region Конструктор
        /// <summary>
        /// Новый рабочий набор 
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="idUser"></param>
        public WorkSetItem_S(WorkSets_M model, string name)
        {
            _isNew = true;
            _model = model;
            _name = name;
            _idUser = _model.IdUser;
        }
        /// <summary>
        /// Зарегистрированный рабочий набор
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="idUser"></param>
        public WorkSetItem_S(WorkSets_M model, int id, string name, int idUser)
        {
            _isNew = false;
            _model = model;
            _id = id;
            _name = name;
            _idUser = idUser;
        }
        #endregion // Конструктор

        #region ApplyChangeSetCommand
        private RelayCommand _applyChangeSetCommand;
        public RelayCommand ApplyChangeSetCommand
        { get { return _applyChangeSetCommand ?? (_applyChangeSetCommand = new RelayCommand(this.ApplyChangeSet, this.CanApplyChangeSet)); } }
        private void ApplyChangeSet(object obj = null)
        {
            _model.Apply(this);
        }
        private bool CanApplyChangeSet(object obj)
        {
            return (IsAccess);
        }
        #endregion ApplyChangeSetCommand


        public object Clone()
        {
            return new WorkSetItem_S(_model, this.Id, this.Name, this.IdUser)
            {
                _showSet = this._showSet,
                _isNew = this._isNew
            };
        }
    }
}
