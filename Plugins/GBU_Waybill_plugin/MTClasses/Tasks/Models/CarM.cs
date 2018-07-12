using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class CarM : SelectableClass
    {
        #region Поля
        private int _id;
        private string _gos_nomer;
        private string _gar_nomer;
        private int _external_id;
        #endregion

        #region Конструтор
        public CarM(int id, string gos_nomer, string gar_nomer, int external_id)
        {
            _id = id;
            _gos_nomer = gos_nomer;
            _external_id = external_id;
            _gar_nomer = gar_nomer;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public int ExternalId
        {
            get { return _external_id; }
        }
        public string GosNomer
        {
            get { return _gos_nomer; }
            set
            {
                OnPropertyChanged(ref _gos_nomer, value, () => GosNomer);
            }
        }
        public string GarNomer
        {
            get { return _gar_nomer; }
            set
            {
                OnPropertyChanged(ref _gar_nomer, value, () => GarNomer);
            }
        }
        #endregion
    }
}
