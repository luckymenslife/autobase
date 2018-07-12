using Rekod.Controllers;
using Rekod.DataAccess.SourceCosmetic.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public class CosTableViewFiltersM : ViewModelBase, ITableViewFiltersM
    {
        #region Поля
        private CosmeticTableViewFilterVM _source;
        private ITableViewFiltersM _parent;
        private bool _hasError;
        private System.Collections.ObjectModel.ObservableCollection<ITableViewFilterM> _container; 
        #endregion

        #region Конструкторы
        public CosTableViewFiltersM(CosmeticTableViewFilterVM source, ITableViewFiltersM parent = null, bool setDefault = true)
        {
            _source = source;
            _parent = parent;
            if (setDefault)
            {
                Container.Add(new CosTableViewFilterM(source, this));
            }
        }
        #endregion

        #region Свойства
        public CosmeticTableViewFilterVM Source
        {
            get { return _source; }
        }
        public ObservableCollection<ITableViewFilterM> Container
        {
            get { return _container ?? (_container = new ObservableCollection<ITableViewFilterM>()); }
        }
        public TableViewFilterType Type
        {
            get { return TableViewFilterType.Container; }
        }
        public ITableViewFiltersM Parent
        {
            get { return _parent; }
        }
        public int Depth
        {
            get
            {
                int depth = (Parent == null) ? 0 : ((Parent as CosTableViewFiltersM).Depth + 1);
                return depth % 5 + 1;
            }
        }
        /// <summary>
        /// Имеет ли фильтр ошибки
        /// </summary>
        public bool HasError
        {
            get
            {
                if (Container.Count == 0)
                {
                    return true;
                }
                else
                {
                    bool hasError = false;
                    foreach (ITableViewFilterM iFilter in Container)
                    {
                        hasError |= iFilter.HasError;
                    }
                    return hasError;
                }
            }
        }
        #endregion
    }
}