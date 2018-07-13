using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin
{
    public class DriverItem
    {
        private string _driverCard;
        private string _outTime;
        private string _returnTime;

        public DriverItem(string driverCard, string outTime, string returnTime)
        {
            _driverCard = driverCard;
            _outTime = outTime;
            _returnTime = returnTime;
        }

        public String getDriverCard
        {
            get { return this._driverCard; }
        }

        public String getOutTime
        {
            get { return this._outTime; }
        }

        public String getReturnTime
        {
            get { return this._returnTime; }
        }
    }
}
