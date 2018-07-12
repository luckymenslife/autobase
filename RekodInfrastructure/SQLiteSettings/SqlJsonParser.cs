using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;
using System.Web.Script.Serialization;

namespace Rekod.SQLiteSettings
{
    class SqlJsonParser
    {

        public static string ToJson(object t)
        {   
            return new JavaScriptSerializer().Serialize(t);
        }
        public static T FromJson<T>(string json)
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }
    }
}
