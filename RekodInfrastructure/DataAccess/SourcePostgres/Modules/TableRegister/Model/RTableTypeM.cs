using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model
{
    public class RTableTypeM: ViewModelBase
    {
        #region Поля
        private String _typeName;
        private ObservableCollection<RTableM> _tables;
        #endregion Поля

        public ObservableCollection<RTableM> Tables
        {
            get { return _tables ?? (_tables = new ObservableCollection<RTableM>()); }
        } 

        public String TypeName
        {
            get { return _typeName; }
            set { OnPropertyChanged(ref _typeName, value, () => this.TypeName); }
        }
    }
}