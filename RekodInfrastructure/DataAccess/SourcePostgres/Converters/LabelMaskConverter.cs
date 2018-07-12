using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Rekod.DataAccess.SourcePostgres.Converters
{
    [ValueConversion(typeof(String), typeof(String))]
    class LabelMaskConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null; 
            }
            String text = value.ToString(); 
            if (text == null)
            {
                text = "";
            }
            String result = "";
            String[] parts = text.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Replace("::text", "");
                while (parts[i].Length > 1 && (parts[i][0] == '(' && parts[i][parts[i].Length - 1] == ')'))
                {
                    parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                }
                if (parts[i].Length > 1 && parts[i][0] == '\'' && parts[i][parts[i].Length - 1] == '\'')
                {
                    parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                    parts[i] = "[" + parts[i] + "]";
                }

                parts[i] = parts[i].Replace("''", "'");
                parts[i] = "{" + parts[i] + "}";
            }

            foreach (String s in parts)
            {
                if (result != "")
                    result += "+" + s;
                else result += s;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            String text = value.ToString(); 

            int state = 0;
            // state = 0 ->  ?
            // state = 1 -> '{' 
            String result = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (state == 0)
                {
                    if (text[i] == '{') { state = 1; result += "(("; }
                    else if (text[i] == '+') { result += "||"; }
                    else { result += text[i]; }
                }
                else if (state == 1)
                {
                    if (text[i] == '}')
                    {
                        state = 0;
                        result += ")::text)";
                    }
                    else if (text[i] == '[' && text[i - 1] == '{' || text[i] == ']' && text[i + 1] == '}')
                    {
                        result += "'";
                    }
                    else if (text[i] == '\'')
                    {
                        result += "''";
                    }
                    else
                    {
                        result += text[i];
                    }
                }
            }
            return result;
        }
    }
}