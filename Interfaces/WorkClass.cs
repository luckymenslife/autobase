using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Interfaces
{
    public class AssemblyClass
    {

        /// <summary> Функция получающая Guid плагина из данных о сборки
        /// </summary>
        /// <param name="assembly">Данных о сборки</param>
        /// <returns>Guid плагина</returns>
        public static string GetGuid(Assembly assembly)
        {
            try
            {
                if (assembly == null)
                    throw new System.ArgumentNullException("assembly");
                if (assembly.GetCustomAttributes(typeof(GuidAttribute), true).Length > 0)
                {
                    return (assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0] as GuidAttribute).Value;
                }
                else
                {
                    return null;
                }
            }
            catch
            { return null; }
        }
        /// <summary> Функция получающая Название плагина из данных о сборки
        /// </summary>
        /// <param name="assembly">Данных о сборки</param>
        /// <returns>Название плагина</returns>
        public static string GetName(Assembly assembly)
        {
            try
            {
                if (assembly == null)
                    throw new System.ArgumentNullException("assembly");
                if (assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true).Length > 0)
                {
                    return (assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0] as AssemblyTitleAttribute).Title;
                }
                else
                {
                    return null;
                }
            }
            catch
            { return null; }
        }

    }
}
