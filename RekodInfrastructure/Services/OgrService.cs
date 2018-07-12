using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;

namespace Rekod.Services
{
    public class OgrService : Rekod.Classes.Singleton<OgrService>
    {
        #region Поля
        private Dictionary<int, SpatialReference> _refSpatialReference;
        #endregion Поля

        public Func<int, string> funcGetRefSysName;


        private OgrService()
        {
            _refSpatialReference = new Dictionary<int, SpatialReference>();
        }

        #region Методы
        public SpatialReference GetSpatialReference(int srid)
        {
            if (_refSpatialReference.ContainsKey(srid))
            {
                return _refSpatialReference[srid];
            }
            if (!_refSpatialReference.ContainsKey(srid) && funcGetRefSysName != null)
            {
                var refSysName = funcGetRefSysName(srid);
                if (!string.IsNullOrEmpty(refSysName))
                {
                    var reference = new SpatialReference(refSysName);
                    _refSpatialReference.Add(srid, reference);
                    return reference;
                }
            }

            throw new Exception("Ненайдена данная проекция");
        }
        public int GetSrid(SpatialReference spat)
        {
            if (_refSpatialReference.ContainsValue(spat))
            {
                return _refSpatialReference.FirstOrDefault(f => f.Value == spat).Key;
            }
            else
                throw new Exception("Незарегистрировано такой проекции");
        }
        #endregion // Методы
    }
}
