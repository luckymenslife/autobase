using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.Classes
{
    class ExportTxtManager
    {
        public static void ExportToTxt(List<object[]> data, List<int> Types, string separator, string fileName)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false);
            for (int i = 0; data.Count > i; i++ )
            {
                string line = "";
                for (int t = 0; data[i].Length > t; t++)
                {
                    if (data[i].Length - 1 == t)
                    {
                        if (data[i][t] != null)
                        {
                            line = line + data[i][t].ToString();
                        }
                        else
                        {
                            line = line + "";
                        }
                    }
                    else
                    {
                        if (data[i][t] != null)
                        {
                            line = line + data[i][t].ToString() + separator;
                        }
                        else
                        {
                            line = line + "" + separator;
                        }
                    }
                }
                file.WriteLine(line);
            }
            file.Close();
        }
    }
}
