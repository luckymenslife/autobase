using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.ComponentModel;
using System.Windows.Data;
using Rekod.Controllers;

namespace Rekod.DataAccess.AbstractSource.Model
{
    public class GroupM : ViewModelBase, AbsM.IGroupM
    {
        #region Поля
        int _id;
        String _description;
        String _text;
        bool? _isVisible = false;
        protected bool _isNewGroup = false;
        protected AbsM.IDataRepositoryM _source;
        private ObservableCollection<AbsM.ILayerM> _tables = null;
        #endregion // Поля

        #region Конструкторы
        public GroupM(AbsM.IDataRepositoryM source, int id)
        {
            _tables = new ObservableCollection<AbsM.ILayerM>();
            this.Id = id;
            this.Source = source; 
        }
        public GroupM(AbsM.IDataRepositoryM source)
        {
            _tables = new ObservableCollection<AbsM.ILayerM>();
            this.Source = source;
            _isNewGroup = true; 
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        /// <summary>
        /// Признак что группа новая. Используется при первом сохранении
        /// </summary>
        public bool IsNewGroup
        {
            get { return _isNewGroup; }
        }
        /// <summary>
        /// Источник
        /// </summary>
        public AbsM.IDataRepositoryM Source
        {
            get { return _source; }
            private set { _source = value; }
        }
        /// <summary>
        /// Описание группы
        /// </summary>
        public String Description
        {
            get { return _description; }
            set { OnPropertyChanged(ref _description, value, () => this.Description); }
        }
        /// <summary>
        /// Текстовое название группы в пользовательском интерфейсе
        /// </summary>
        public String Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref  _text, value, () => this.Text); }
        }
        /// <summary>
        /// Коллекция таблиц принадлежащих группе
        /// </summary>
        /// <remarks>
        /// Для представления по умолчанию создается фильтр который скрывает слои со свойством IsHidden равное true
        /// </remarks>
        public ObservableCollection<AbsM.ILayerM> Tables
        {
            get 
            {
                if (_tables == null)
                {
                    ICollectionView defView = CollectionViewSource.GetDefaultView(_tables);
                    defView.Filter = delegate(object o)
                    {
                        return !(o as AbsM.TableBaseM).IsHidden;
                    };
                }
                return _tables; 
            }
        }
        /// <summary>
        /// Признак видимости слоев в группе
        /// </summary>
        public Boolean? IsVisible
        {
            get
            { return _isVisible; }
            set
            {
                if (_isVisible == value)
                {
                    return; 
                }
                // todo: Рассмотреть случай когда у группы нет источника
                ((AbsVM.DataRepositoryVM)Source).SetGroupVisible(this, value); 
            }
        }
        #endregion // Свойства

        #region Переопределения стандартных методов
        public override string ToString()
        {
            return string.Format("g: {0} {1}", _id, _text);
        }
        #endregion // Переопределения стандартных методов

        #region Статические члены
        internal static void UpdateIsVisible(GroupM group, bool? value)
        {
            group.OnPropertyChanged(ref group._isVisible, value, () => group.IsVisible);
        }
        internal static ObservableCollection<AbsM.ILayerM> GetTables(GroupM group)
        {
            return group._tables;
        }
        #endregion
    }
}