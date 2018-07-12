using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtraFunctions
{
    public static class Converts
    {
        public static T To<T>(object value)
        {
            Type t = typeof(T);

            if (value is string && string.IsNullOrEmpty((string)value) && t != typeof(string))
                value = null;

            if (value == null || value == DBNull.Value)
                return default(T);
            else if (value is T)
                return (T)value;
            else
                try
                {
                    if (!t.IsGenericType && t.IsEnum)
                        return (T)Enum.ToObject(t, value);

                    if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        t = Nullable.GetUnderlyingType(t);
                    return (T)Convert.ChangeType(value, t);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Ошибка конвертации в методе To", "Converts");
                    return default(T);
                }
        }
        public static bool TryTo<T>(object value, out T result)
        {
            Type t = typeof(T);
            result = default(T);

            if (value != null && value != DBNull.Value)
            {
                if (value is T)
                    result = (T)value;
                else
                    try
                    {
                        if (!t.IsGenericType && t.IsEnum)
                            result = (T)Enum.ToObject(t, value);
                        else
                        {
                            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                                t = Nullable.GetUnderlyingType(t);
                            result = (T)Convert.ChangeType(value, t);
                        }
                    }
                    catch (InvalidCastException)
                    {
                        return false;
                    }
            }
            return true;
        }

        public static object To(object value, Type type)
        {
            Type t = typeof(Converts);
            var to = t.GetMethod("To", new Type[] { typeof(object) });
            return to.MakeGenericMethod(type).Invoke(null, new object[] { value });
        }

        public static object GetDefault(this Type type)
        {
            return typeof(Converts).GetMethod("GetDefault").MakeGenericMethod(type).Invoke(null, new Type[0]);
        }
        private static T GetDefault<T>()
        {
            return default(T);
        }
    }
}