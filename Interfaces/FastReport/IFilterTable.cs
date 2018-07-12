using System;
using Interfaces.FastReport;

namespace Interfaces.FastReport
{
    public interface IFilterTable
    {
        int IdObj { get; }
        int IdTable { get; }
        string Where { get; }
        string Order { get; }
    }
}
