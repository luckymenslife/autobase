using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.ViewModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using System.Windows.Input;
using Rekod.DataAccess.AbstractSource;
using Rekod.Behaviors;
using Rekod.Controllers;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.IO;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgUserM : ViewModelBase, IComparable, ICloneable
    {
        #region Поля
        private int? _id;
        private string _pass;
        private string _passSync;
        private string _nameFull;
        private string _login;
        private string _otdel;
        private string _windowName;
        private UserType _type;
        private PgVM.PgDataRepositoryVM _source;
        private PgExtentM _extent;
        private ICommand _reloadCommand;
        private ICommand _editPasswordCommand;
        private ICommand _showSyncPasswordCommand;
        private ICommand _saveCommand;
        private string _evaluateError;
        #endregion Поля

        #region Конструкторы
        public PgUserM(PgVM.PgDataRepositoryVM source)
        {
            _source = source;
        }
        public PgUserM(PgUserM user, PgVM.PgDataRepositoryVM source)
        {
            CopyFrom(user);
            _source = source;
        }
        #endregion Конструкторы

        #region Свойства
        public int? ID
        {
            get { return _id; }
            set { OnPropertyChanged(ref _id, value, () => this.ID); OnPropertyChanged("IsNewUser"); }
        }
        public string Pass
        {
            get { return _pass; }
            set { OnPropertyChanged(ref _pass, value, () => this.Pass); }
        }
        public string PassSync
        {
            get { return _passSync; }
            set { OnPropertyChanged(ref _passSync, value, () => this.PassSync); }
        }
        public string NameFull
        {
            get { return _nameFull; }
            set { OnPropertyChanged(ref _nameFull, value, () => this.NameFull); }
        }
        public string Text
        {
            get { return _nameFull; }
            set { OnPropertyChanged(ref _nameFull, value, () => this.Text); }
        }
        public string Login
        {
            get { return _login; }
            set { OnPropertyChanged(ref _login, value, () => this.Login); }
        }
        public string Otdel
        {
            get { return _otdel; }
            set { OnPropertyChanged(ref _otdel, value, () => this.Otdel); }
        }
        public string WindowName
        {
            get { return _windowName; }
            set { OnPropertyChanged(ref _windowName, value, () => this.WindowName); }
        }
        public string EvaluateError
        {
            get { return _evaluateError; }
            set { OnPropertyChanged(ref _evaluateError, value, () => this.EvaluateError); }
        }
        public UserType Type
        {
            get { return _type; }
            set { OnPropertyChanged(ref _type, value, () => this.Type); OnPropertyChanged("IsAdmin"); }
        }
        public string NameLogin
        {
            get { return String.Format("{0} ({1})", _nameFull, _login); }
        }
        /// <summary>
        /// Возвращает права пользователя на таблицы
        /// </summary>
        public PgVM.PgListUserRightsVM UserRights
        {
            get
            {
                PgUserM curUser = Source.CurrentUser;
                if (curUser == this || curUser != null && curUser.Type == UserType.Admin)
                {
                    var userRights = new PgVM.PgListUserRightsVM(Source);
                    userRights.User = this;
                    return userRights;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Источник к которому относится пользователь
        /// </summary>
        public PgVM.PgDataRepositoryVM Source
        {
            get { return _source; }
            set { OnPropertyChanged(ref _source, value, () => this.Source); }
        }
        /// <summary>
        /// Возвращает true если пользователь является администратором
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return (_type == UserType.Admin);
            }
        }
        /// <summary>
        /// Локализация при запуске программы
        /// </summary>
        public PgExtentM Extent
        {
            get { return _extent; }
            set
            {
                OnPropertyChanged(ref _extent, value, () => this.Extent);
            }
        }
        /// <summary>
        /// Пользователь новый, если его Id == null
        /// </summary>
        public bool IsNewUser
        {
            get { return (ID == null); }
        }
        /// <summary>
        /// Свойство, которое возвращает клон объекта
        /// </summary>
        public PgUserM UserClone
        {
            get { return (PgUserM)Clone(); }
        }
        #endregion Свойства

        #region Методы
        public void CopyFrom(PgUserM user)
        {
            this.Source = user.Source;
            this.ID = user.ID;
            this.Pass = user.Pass;
            this.PassSync = user.PassSync;
            this.NameFull = user.NameFull;
            this.Login = user.Login;
            this.Otdel = user.Otdel;
            this.WindowName = user.WindowName;
            this.Type = user.Type;
            this.Extent = user.Extent;
        }
        public int CompareTo(object obj)
        {
            if (NameFull != null && obj is PgUserM)
                return NameFull.CompareTo((obj as PgUserM).NameFull);
            else return 0;
        }
        public object Clone()
        {
            return new PgUserM(this, Source);
        }
        #endregion Методы

        #region Команды
        #region SaveCommand
        /// <summary>
        /// Команда для сохранения атрибутов пользователя в базе
        /// </summary>
        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(this.Save, this.CanSave)); }
        }
        /// <summary>
        /// Сохранение атрибутов пользователя в базе
        /// </summary>
        public void Save(object parameter = null)
        {
            try
            {
                BindingProxy bindProxy = parameter as BindingProxy;
                PgUserM pgUser = bindProxy.Data as PgUserM;
                int id = Source.InsertUpdateUser(pgUser);
                if (pgUser.IsNewUser)
                {
                    pgUser = Source.Users.First(u => u.ID == id);
                    bindProxy.Data = pgUser;
                }
            }
            catch(Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
        }
        /// <summary>
        /// Можно ли сохранить пользователя
        /// </summary>
        public bool CanSave(object parameter = null)
        {
            BindingProxy bindProxy = parameter as BindingProxy;
            if (bindProxy.Data is PgUserM)
            {
                EvaluateError = null;
                PgUserM pgUser = bindProxy.Data as PgUserM;
                bool canSave = true;
                canSave &= !String.IsNullOrEmpty(pgUser.Login);
                canSave &= !String.IsNullOrEmpty(pgUser.NameFull);
                canSave &= !String.IsNullOrEmpty(pgUser.Otdel);
                canSave &= !String.IsNullOrEmpty(pgUser.WindowName);
                canSave &= (pgUser.Type != 0);
                if (!canSave)
                {
                    EvaluateError = "Введите обязательные поля";
                }

                bool ok = true;
                if (pgUser != null && pgUser.Login != null && pgUser.Login.Length > 0)
                {
                    if ((pgUser.Login[0] >= 'a' && pgUser.Login[0] <= 'z') || (pgUser.Login[0] == '_'))
                    {
                        for (int i = 1; i < pgUser.Login.Length; i++)
                        {
                            if (!((pgUser.Login[i] >= 'a' && pgUser.Login[i] <= 'z') ||
                                (pgUser.Login[i] >= '0' && pgUser.Login[i] <= '9') ||
                                pgUser.Login[i] == '_'))
                                ok = false;
                        }
                    }
                    else
                    {
                        ok = false;
                    }
                    if (!ok)
                    {
                        if (String.IsNullOrEmpty(EvaluateError))
                        {
                            EvaluateError = Rekod.Properties.Resources.LocUserLoginWarning;
                        }
                    }
                }
                else
                {
                    ok = false;
                }
                canSave &= ok;

                if (pgUser.IsNewUser && String.IsNullOrEmpty(pgUser.Pass))
                {
                    canSave &= false;
                    if (String.IsNullOrEmpty(EvaluateError))
                    {
                        EvaluateError = "Пароль не должен быть пустым"; ;
                    }
                }
                if (canSave)
                {
                    EvaluateError = null;
                }
                return canSave;
            }
            else
            {
                EvaluateError = null;
                return false;
            }
        }
        #endregion // SaveCommand

        #region ReloadCommand
        /// <summary>
        /// Команда для обновления информации о пользователе из базы
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(this.Reload, this.CanReload)); }
        }
        /// <summary>
        /// Обновление информации о пользователе из базы
        /// </summary>
        public void Reload(object parameter = null)
        {
            Source.UpdateUsers(ID);
            //if (parameter is PgUserM)
            //{
            //    PgUserM pgUser = parameter as PgUserM;
            //    PgUserM copyFromUser = Source.Users.FirstOrDefault(p => p.ID == ID);
            //    if (copyFromUser != null)
            //    {
            //        pgUser.CopyFrom(copyFromUser);
            //    }
            //}
            //OnPropertyChanged("UserClone");
        }
        /// <summary>
        /// Можно ли обновить информацию из базы
        /// </summary>
        public bool CanReload(object parameter = null)
        {
            return true;
        }
        #endregion // ReloadCommand

        #region EditPasswordCommand
        /// <summary>
        /// Команда для редактирования пароля
        /// </summary>
        public ICommand EditPasswordCommand
        {
            get { return _editPasswordCommand ?? (_editPasswordCommand = new RelayCommand(this.EditPassword, this.CanEditPassword)); }
        }
        /// <summary>
        /// Редактирование пароля
        /// </summary>
        public void EditPassword(object parameter = null)
        {
            //var form = new Rekod.DataAccess.SourcePostgres.View.ConfigView.NewPassword(ID == null ? 0 : (int)ID, Source);
            //form.ShowDialog();
            //if (form.DialogResult != System.Windows.Forms.DialogResult.OK) return;
            //Pass = form.UserPass;
            //PassSync = form.UserPassSync;

            List<object> commParams = parameter as List<object>;
            PasswordBox BoxPassword = commParams[0] as PasswordBox;
            PasswordBox BoxConfirmation = commParams[1] as PasswordBox;
            if (!String.IsNullOrEmpty(BoxPassword.Password) && BoxPassword.Password == BoxConfirmation.Password)
            {
                Pass = BoxPassword.Password;
                PassSync = Services.Encrypting.Encrypt(BoxPassword.Password, _source.Connect.Database + "_pwd");
            } 
        }
        /// <summary>
        /// Можно ли редактировать пароль
        /// </summary>
        public bool CanEditPassword(object parameter = null)
        {
            bool res = false;
            if (parameter != null)
            {
                List<object> commParams = parameter as List<object>;
                PasswordBox BoxPassword = commParams[0] as PasswordBox;
                PasswordBox BoxConfirmation = commParams[1] as PasswordBox;
                if (!String.IsNullOrEmpty(BoxPassword.Password) && BoxPassword.Password == BoxConfirmation.Password)
                {
                    res = true;
                }
            }
            return res;
        }
        #endregion // EditPasswordCommand
        #endregion Команды

        #region Действия
        public Action<object> ClearExtentAction
        {
            get
            {
                return param =>
                {
                    CommandEventParameter commEvtParam = param as CommandEventParameter;
                    ComboBox extentsBox = commEvtParam.CommandParameter as ComboBox;
                    if (extentsBox != null)
                    {
                        extentsBox.SelectedItem = null;
                    }
                };
            }
        }
        #endregion Действия
    }

    /// <summary>
    /// Тип геометрии
    /// </summary>
    [TypeResource("PgM.UserType")]
    public enum UserType
    {
        Admin = 1, User = 2
    }
}