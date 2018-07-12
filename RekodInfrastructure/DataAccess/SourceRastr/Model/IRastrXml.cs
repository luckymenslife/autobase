using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourceRastr.Model
{
    public interface IRastrXml
    {
        ERastrXmlType Type { get; }
        bool IsValid();
    }


    public enum ERastrXmlType
    {
        None = 0,
        WMS,
        TMS,
        TWMS
    }
}
