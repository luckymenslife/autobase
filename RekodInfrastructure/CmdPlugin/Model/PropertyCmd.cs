using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Rekod.CmdPlugin.Model
{
    public class PropertyCall
    {
        public string Params { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string File { get; set; }
        public override string ToString()
        {
            return Title;
        }
    }
}
