using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin
{
    public class IssueItem
    {
        private string _owner;
        private string _to;
        private string _from;
        private string _type;

        public IssueItem(string owner, string to, string from, string type)
        {
            _owner = owner;
            _to = to;
            _from = from;
            _type = type;
        }

        public String getOwner
        {
            get { return this._owner; }
        }

        public String getTo
        {
            get { return this._to; }
        }

        public String getFrom
        {
            get { return this._from; }
        }

        public String getType
        {
            get { return this._type; }
        }
    }
}
