using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public class CosmeticObjectM
    {
        private int _id;
        private string _wkt;
        private Dictionary<string, object> _attributes;
        private PgStyleObjectM _style;

        public PgStyleObjectM Style
        {
            get { return _style; }
            set { _style = value; }
        }

        public string WKT
        {
            get { return _wkt; }
            set { _wkt = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        
        public CosmeticObjectM(int id, string wkt)
        {
            _id = id;
            _wkt = wkt;
            _attributes = new Dictionary<string, object>();
        }

        public void AddAttribute(string fieldName, object value)
        {
            if (!_attributes.ContainsKey(fieldName))
            {
                _attributes.Add(fieldName, value);
            }
        }

        public object GetAttributeValue(string fieldName)
        {
            if (_attributes.ContainsKey(fieldName))
            {
                return _attributes[fieldName];
            }
            return null;
        }

        public void SetAttribute(string fieldName, object value)
        {
            if (_attributes.ContainsKey(fieldName))
            {
                _attributes[fieldName] = value;
            }
            else
            {
                _attributes.Add(fieldName, value);
            }
        }
    }
}
