using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Behaviors
{
    public class CommandEventParameter
    {
        public Object CommandParameter { get; set; }
        public Object EventSender { get; set; }
        public Object EventArgs { get; set; }
        public Type EventHandlerType { get; set; }
        public Object ExtraParameter { get; set; }
    }
}