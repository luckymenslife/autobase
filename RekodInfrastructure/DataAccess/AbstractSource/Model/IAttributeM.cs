using System;
using System.Collections.Generic;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.AbstractSource.Model
{
    public interface IAttributeM
    {
        AbsM.IFieldM Field { get; }
        object Value { get; set; }
        bool IsReadOnly { get; }
    }
}