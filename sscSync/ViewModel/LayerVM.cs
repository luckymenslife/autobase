using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RESTLib.Model.REST;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Model.WFS;
using sscSync.Controller;
using Interfaces;
using System.Collections.ObjectModel;
using RESTLib.Enums;
using System.Windows;

namespace sscSync.ViewModel
{
    /// <summary>
    /// ViewModel добавления слоя в MapAdmin
    /// </summary>
    public class LayerVM : sscSync.ViewModel.ViewModelBase
    {
        #region Поля
        private SSCData _sscData;
        private string _nameRus;
        private string _nameEng;
        private Group _group;
        private LStyle _style;
        private User _user;
        private ObservableCollection<LAttribute> _attributes;
        private int _geomType;
        private bool _isFinished;
        #endregion Поля

        #region Свойства

        /// <summary>
        /// Является ли пользователь администратором ведомства
        /// </summary>
        public bool IsAdmin
        {
            get { return _sscData.IsAdmin; }
        }

        /// <summary>
        /// Русское название слоя
        /// </summary>
        public string NameRus
        {
            get { return _nameRus; }
            set { _nameRus = value; OnPropertyChanged("NameRus"); }
        }

        /// <summary>
        /// Английское название слоя
        /// </summary>
        public string NameEng
        {
            get { return _nameEng; }
            set { _nameEng = value; OnPropertyChanged("NameEng"); }
        }

        /// <summary>
        /// Атрибуты
        /// </summary>
        public ObservableCollection<LAttribute> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; OnPropertyChanged("Attributes"); }
        }

        /// <summary>
        /// Группа
        /// </summary>
        public Group Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        /// <summary>
        /// Стиль
        /// </summary>
        public LStyle Style
        {
            get { return _style; }
            set { _style = value; OnPropertyChanged("Style"); }
        }

        /// <summary>
        /// Список доступных групп
        /// </summary>
        public List<Group> Groups
        {
            get { return _sscData.GetGroups(); }
        }

        /// <summary>
        /// Список доступных типов геометрии
        /// </summary>
        public List<string> Types
        {
            get {
                return new List<string>()
                {
                    "Точки", "Линии", "Полигоны"
                };
            }
        }

        /// <summary>
        /// Тип геометрии слоя
        /// </summary>
        public int GeomType
        {
            get { return _geomType; }
            set
            {
                _geomType = value;
                Style.Type = (RESTStyles)(_geomType + 2);
                OnPropertyChanged("GeomType");
                OnPropertyChanged("Style");
            }
        }

        /// <summary>
        /// Можно ли закрыть окно
        /// </summary>
        public bool IsFinished
        {
            get { return _isFinished; }
            set { _isFinished = value; OnPropertyChanged("IsFinished"); }
        }

        #endregion Свойства

        public LayerVM(sscUserInfo userInfo)
        {
            Attributes = new ObservableCollection<LAttribute>();

            _user = new User(userInfo.Login, userInfo.Password, userInfo.Server);
            _sscData = new SSCData(_user);

            if (_sscData == null)
                throw new Exception("Нет подключения к серверу MapAdmin");

            Style = _sscData.getDefaultStyle();
            GeomType = 0;
            Group = Groups.FirstOrDefault();
        }

        #region Команды

        #region AddAttributeCommand
        private ICommand _addAttributeCommand = null;
        public ICommand AddAttributeCommand
        {
            get
            {
                return _addAttributeCommand ?? (_addAttributeCommand = new RelayCommand((o) =>                    
                    {
                        Attributes.Add(new LAttribute(Attributes));
                    }
                    ));
            }
        }
        #endregion AddAttributeCommand

        #region CreateCommand
        private ICommand _createCommand = null;

        public ICommand CreateCommand
        {
            get
            {
                return _createCommand ?? (_createCommand = new RelayCommand(Create, CanCreate));
            }
        }

        private bool CanCreate(object obj)
        {
            return (!IsAdmin || !String.IsNullOrEmpty(NameEng))
                && !String.IsNullOrEmpty(NameRus)
                && Group != null
                && Style.IsValid;
        }

        private void Create(object obj)
        {
            try
            {
                _sscData.CreateLayer(
                    NameEng, NameRus, 
                    (GeomType)(GeomType + 1),
                    Attributes
                        .Where(w => !String.IsNullOrEmpty(w.Name) && !String.IsNullOrEmpty(w.Name_ru))
                        .Cast<LayerAttribute>()
                        .ToList(), 
                    Group, 
                    Style.RESTLayerStyle);
                IsFinished = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка при создании слоя", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion CreateCommand

        #endregion Команды
    }

    /// <summary>
    /// Класс атрибута слоя
    /// </summary>
    public class LAttribute : LayerAttribute
    {
        private ObservableCollection<LAttribute> attributes;
        private int _aType;
        private bool _isTitle;

        public bool IsTitle
        {
            get { return _isTitle; }
            set
            {
                _isTitle = value;
                Title = IsTitle ? 't' : 'f';
            }
        }

        /// <summary>
        /// Тип атрибута
        /// </summary>
        public int AType
        {
            get { return _aType; }
            set
            {
                _aType = value;
                this.Type = ((AttributeType)AType).ToString().ToLower();
            }
        }

        public LAttribute(ObservableCollection<LAttribute> attributes)
        {
            this.attributes = attributes;
            this.AType = 0;
            this.IsTitle = false;
        }

        private ICommand _deleteCommand = null;
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new RelayCommand(
                    (o) => { attributes.Remove(this); }));
            }
        }

        public List<String> AttributeTypes
        {
            get
            {
                return new List<string>()
                    {
                        "Текст", "Целое", "Вещественное"
                    };
            }
        }
    }
}
