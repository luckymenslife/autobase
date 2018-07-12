using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.DataAccess.SourcePostgres.ViewModel;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgActionM: ViewModelBase
    {
        #region Поля
        private int? _id;
        private string _name;
        private bool _table_data;
        private string _name_visible;
        private bool _operation;
        private PgDataRepositoryVM _source; 
        #endregion Поля

        #region Свойства
        public int? ID
        {
            get { return _id; }
            set { OnPropertyChanged(ref _id, value, () => this.ID); }
        }
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        public string NameVisible
        {
            get { return _name_visible; }
            set { OnPropertyChanged(ref _name_visible, value, () => this.NameVisible); }
        }
        public bool TableData
        {
            get { return _table_data; }
            set { OnPropertyChanged(ref _table_data, value, () => this.TableData); }
        }
        public bool Operation
        {
            get { return _operation; }
            set { OnPropertyChanged(ref _operation, value, () => this.Operation); }
        }

        /// <summary>
        /// Источник к которому относится пользователь
        /// </summary>
        public PgDataRepositoryVM Source
        {
            get { return _source; }
            set { OnPropertyChanged(ref _source, value, () => this.Source); }
        }

        /// <summary>
        /// Возвращает права пользователя на действия
        /// </summary>
        public PgListUserRightsVM UserRights
        {
            get
            {
                PgUserM curUser = Source.CurrentUser;
                if (curUser != null && curUser.Type == UserType.Admin)
                {
                    PgListUserRightsVM userRights = new PgListUserRightsVM(Source);
                    userRights.Action = this;
                    return userRights;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion Свойства

        #region Конструкторы
        public PgActionM(PgDataRepositoryVM source, int id, string name, string visible_name, bool table_data, bool operation)
        {
            _source = source;
            _id = id;
            _name = name;
            _name_visible = visible_name;
            _operation = operation;
            _table_data = table_data;
        }
        #endregion Конструкторы
    }
}
