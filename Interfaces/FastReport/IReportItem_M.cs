using System;
namespace Interfaces.FastReport
{
    public interface IReportItem_M
    {
        string Caption { get; set; }
        int IdReport { get; }
        int? IdTable { get; }
        enTypeReport Type { get; }
    }

    [Flags]
    public enum enTypeReport
    {
        All = 0,
        Table = 1,
        Object = 2
    }
}
