using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PgTV_VM = Rekod.DataAccess.SourcePostgres.ViewModel.PgTableView;

namespace Rekod.DataAccess.SourcePostgres.Model.PgTableView
{
    public enum PgTableViewFilterType { Filter, Container }
    public interface IPgTableViewFilterM
    {
        PgTableViewFilterType Type { get; }
        PgTableViewFiltersM Parent { get; }
        PgTV_VM.PgTableViewFilterVM Source { get; }
        bool HasError { get; }
    }
}
