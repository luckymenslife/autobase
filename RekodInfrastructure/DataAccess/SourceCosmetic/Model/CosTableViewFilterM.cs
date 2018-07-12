using Rekod.Classes;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourceCosmetic.ViewModel;
using Rekod.DataAccess.SourcePostgres.Model.PgTableView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public class CosTableViewFilterM : ViewModelBase, ITableViewFilterM
    {
        #region Поля
        private CosmeticTableViewFilterVM _source;
        private ITableViewFiltersM _parent;
        private bool _hasError;
        private IFieldM _field;
        private EFieldType _fieldType;
        private IEnumerable<NameValue> _collOperation; 
        #endregion

        #region Конструкторы
        public CosTableViewFilterM(CosmeticTableViewFilterVM source, ITableViewFiltersM parent)
        {
            _source = source;
            _parent = parent;
            Field = _source.Fields[0];
        }
        #endregion

        #region Свойства
        public EFieldType FieldType
        {
            get { return _fieldType; }
            private set { OnPropertyChanged(ref _fieldType, value, () => this.FieldType); }
        }
        public TableViewFilterType Type
        {
            get { return TableViewFilterType.Filter; }
        }
        public bool HasError
        {
            get { return _hasError; }
            private set { OnPropertyChanged(ref _hasError, value, () => this.HasError); }
        }
        public ITableViewFiltersM Parent
        {
            get { return _parent; }
        }
        public CosmeticTableViewFilterVM Source
        {
            get { return _source; }
        }
        public IFieldM Field
        {
            get { return _field; }
            set
            {
                OnPropertyChanged(ref _field, value, () => this.Field);
                FieldType = _field.Type;
                if (FieldType == EFieldType.Text)
                {
                    CollOperation = Source.CollOperationText;
                }
                else
                {
                    CollOperation = Source.CollOperationValue;
                }
                OnPropertyChanged(() => this.HasError);
            }
        }
        public IEnumerable<NameValue> CollOperation
        {
            get { return _collOperation; }
            private set
            { OnPropertyChanged(ref _collOperation, value, () => this.CollOperation); }
        }
        #endregion        
    }
}