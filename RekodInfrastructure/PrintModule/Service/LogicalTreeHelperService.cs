using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rekod.PrintModule.Service
{
    public static class LogicalTreeHelperService
    {
        public static FrameworkElement GetParentOfType<T>(FrameworkElement child)
        {
            FrameworkElement parent = child.Parent as FrameworkElement;
            while (!(parent is T) && parent != null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            return parent;
        }
    }
}
