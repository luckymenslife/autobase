using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Model.PluginModel
{
    public class PluginFunc<TFunc>
    {
        public int IdTable { get; private set; }
        public TFunc Func { get; private set; }

        public PluginFunc(int idTable, TFunc func)
        {
            IdTable = idTable;
            Func = func;
        }
    }

}
