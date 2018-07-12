using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    /// <summary>
    /// Набор стилей
    /// </summary>
    public class PgStyleSetsM : ViewModelBase
    {
        #region Поля
        private string _name;
        private Dictionary<PgTableBaseM, PgStyleLableM> _lable;
        private Dictionary<PgTableBaseM, PgStyleLayerM> _layers;
        #endregion // Поля

        #region Конструктор
        public PgStyleSetsM(int id, int ownerUser, bool isDefault, bool isPublic)
        {
            Id = id;
            OwnerUser = ownerUser;
            IsDefault = isDefault;
            IsPublic = isPublic;
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Идентификатор подписи
        /// </summary>
        public int Id { get; private set; }

        public int OwnerUser { get; private set; }
        /// <summary>
        /// Этот стиль по умолчанию
        /// </summary>
        public bool IsDefault { get; private set; }
        /// <summary>
        /// Этот стиль публичный
        /// </summary>
        public bool IsPublic { get; private set; }
        /// <summary>
        /// Название набора
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        /// <summary>
        /// Стили подписей
        /// </summary>
        public Dictionary<PgTableBaseM, PgStyleLableM> Labels
        {
            get { return _lable ?? (_lable = new Dictionary<PgTableBaseM,PgStyleLableM>()); }
        }

        public Dictionary<PgTableBaseM, PgStyleLayerM> Layers
        {
            get { return _layers ?? (_layers = new Dictionary<PgTableBaseM,PgStyleLayerM>()); }
        }


        #endregion // Свойства
    }
}