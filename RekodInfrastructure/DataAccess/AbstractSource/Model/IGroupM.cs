using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.AbstractSource.Model
{
    public interface IGroupM
    {
        string Description { get; set; }
        int Id { get; }
        ObservableCollection<AbsM.ILayerM> Tables { get; }
        string Text { get; set; }
    }
}