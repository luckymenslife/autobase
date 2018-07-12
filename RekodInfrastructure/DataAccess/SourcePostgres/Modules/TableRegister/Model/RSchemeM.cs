using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourcePostgres.Modules.TableRegister.Model
{
    public class RSchemeM : ViewModelBase
    {
        #region Поля
        private String _schemeName;
        #endregion Поля

        #region Конструкторы
        public RSchemeM(String name)
        {
            _schemeName = name;
        }
        #endregion Конструкторы

        #region Свойства
        public String SchemeName
        {
            get { return _schemeName; }
            set { OnPropertyChanged(ref _schemeName, value, () => this.SchemeName); }
        }
        #endregion Свойства
    }
}