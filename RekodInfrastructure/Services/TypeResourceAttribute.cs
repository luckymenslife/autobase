using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Services
{
    public sealed class TypeResourceAttribute: Attribute 
    {
        public String ResourceKey
        {
            get;
            set; 
        }
        public TypeResourceAttribute(string resourceKey)
        {
            ResourceKey = resourceKey; 
        }
        public TypeResourceAttribute() { }
    }
}