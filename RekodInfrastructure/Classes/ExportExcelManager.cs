using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Globalization;

namespace Rekod.Classes
{
    public static class ExportExcelManager
    {
        public static void ExportToExcel(List<object[]> data, List<int> Types, string fileName = null)
        {
            if (data == null) return;
            if (!data.Any()) return;

            int countField = data[0].Count();
            var arrObject = new object[data.Count, countField];



            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < countField; j++)
                {
                    arrObject[i, j] = data[i][j];
                }
            }
            ExportToExcel(arrObject, Types, fileName);
        }

        public static void ExportToExcel(object[,] data, List<int> Types, string fileName)
        {
            if (data == null) return;

            const int left = 1;
            const int top = 1;
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            int bottom = top + height - 1;
            int right = left + width - 1;

            if (height == 0 || width == 0)
                return;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (data[i, j] is DateTime)
                        data[i, j] = string.Format("{0:dd.MM.yyyy HH:mm:ss}", data[i, j]);
                }
            }

            var excel = new Excel.Application();
            var workbook = excel.Workbooks;
            var oWB = workbook.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            
            var worksheet = (Excel.Worksheet)oWB.ActiveSheet;

            //outputRows is a List<List<object>>


            Excel.Range rg = worksheet.Range[worksheet.Cells[top, left], worksheet.Cells[bottom, right]];

            rg.Value = data;
            rg.Borders.LineStyle = 1;
            //for (int i = 1; i <= 4; i++)
            //    rg.Borders[(Excel.XlBordersIndex)1].LineStyle = 1;

            // Set auto columns width
            rg.EntireColumn.AutoFit();

            Excel.Range rgHeader = worksheet.Range[worksheet.Cells[top, left], worksheet.Cells[top, right]];
            //rgHeader.Interior.Color = 189 * (int)Math.Pow(16, 4) + 129 * (int)Math.Pow(16, 2) + 78; // #4E81BD

            int a = 0;



            //rgHeader.Font.Bold = true;

            int savedCult = Thread.CurrentThread.CurrentCulture.LCID;
            if (Types != null)
                try
                {

                    // установим английскую "культуру"
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(0x0409, false);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(0x0409, false);
                    // здесь работаем с Excel'ем, при чем работают английские формулы, DataFormat 
                    // и колонтитулы в PageSetup

                    foreach (var item in Types)
                    {
                        Excel.Range rgColumn;
                        switch (item)
                        {
                            case 2:
                                rgColumn = worksheet.Range[worksheet.Cells[top + 1, left + a], worksheet.Cells[bottom, left + a]];
                                if ((double)rgColumn.ColumnWidth > 100)
                                    rgColumn.ColumnWidth = 100;
                                Marshal.ReleaseComObject(rgColumn);
                                break;
                            case 3:
                                rgColumn = worksheet.Range[worksheet.Cells[top + 1, left + a], worksheet.Cells[bottom, left + a]];
                                rgColumn.NumberFormat = "dd/MM/yyyy";
                                Marshal.ReleaseComObject(rgColumn);

                                break;
                            case 4:
                                rgColumn = worksheet.Range[worksheet.Cells[top + 1, left + a], worksheet.Cells[bottom, left + a]];
                                rgColumn.NumberFormat = "dd/MM/yyyy hh:mm:ss";
                                Marshal.ReleaseComObject(rgColumn);
                                break;
                            default:
                                break;
                        }
                        a++;
                    }
                }
                finally
                {
                    // восстановим пользовательскую "культуру" для отображения всех данных в
                    // привычных глазу форматах
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(savedCult, true);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(savedCult, true);
                }

            rg.EntireColumn.AutoFit();

            excel.ScreenUpdating = true;

            if (fileName == null)
                excel.Visible = true;
            else
            {
                oWB.SaveAs(fileName, ((new System.IO.FileInfo(fileName)).Extension == ".xlsx") ? Excel.XlFileFormat.xlOpenXMLWorkbook : Excel.XlFileFormat.xlExcel8,
                    Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                oWB.Close();
            }

            Marshal.ReleaseComObject(rg);
            Marshal.ReleaseComObject(rgHeader);
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excel);

            rg = null;
            rgHeader = null;
            worksheet = null;
            workbook = null;
            excel = null;
        }
    }
}
