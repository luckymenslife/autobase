using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class TaskGroupM
    {
        private int _gid;
        private int _routeId;
        private string _groupName;
        private DateTime _from;
        private DateTime _till;

        public TaskGroupM(int gid, int routeId, string groupName, DateTime from, DateTime till)
        {
            this._gid = gid;
            this._routeId = routeId;
            this._groupName = groupName;
            this._from = from;
            this._till = till;
        }

        public int Id
        {
            get
            {
                return _gid;
            }
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }
        }

        public DateTime FromTime
        {
            get
            {
                return _from;
            }
        }

        public DateTime TillTime
        {
            get
            {
                return _till;
            }
        }
    }
}
