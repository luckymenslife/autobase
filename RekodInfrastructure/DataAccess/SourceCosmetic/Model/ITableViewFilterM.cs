using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public enum TableViewFilterType { Filter, Container }
    public interface ITableViewFilterM
    {
        TableViewFilterType Type { get; }
        bool HasError { get; }
        ITableViewFiltersM Parent { get; }
    }
    public interface ITableViewFiltersM : ITableViewFilterM
    {
        ObservableCollection<ITableViewFilterM> Container { get; }
    }
}