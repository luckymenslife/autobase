using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using Rekod.Services;
using System.Text.RegularExpressions;

namespace Rekod.ImportExport.Importers
{
    class ExcelImporter : Importer
    {
        private FileInfo inputFile;
        private tablesInfo tableInfo;
        private ExcelUC excelUC;
        override public UserControl SettingsPanel { get { return excelUC; } }
        private readonly int rowsInSect = ImporterHelper.RowsToLoadInPortion;
        private int rowsCount;
        override public int RowsCount { get { return rowsCount; } }
        public ExcelImporter() { excelUC = new ExcelUC(); }
        #region какие-то служебные методы
        public String GetUsedNotEmptyRange(Excel.Worksheet workSheet)
        {
            Excel.Range workRange = workSheet.UsedRange;
            Excel.Application excelApp = workSheet.Application;
            int lastRow = workRange.Row + workRange.Rows.Count - 1;
            int firstRow = workRange.Row;
            int lastColumn = workRange.Column + workRange.Columns.Count - 1;
            int firstColumn = workRange.Column;

            int newFirstRow = firstRow;
            int newLastRow = lastRow;
            int newFirstColumn = firstColumn;
            int newLastColumn = lastColumn;

            String mes = "";

            if (excelApp.WorksheetFunction.CountA(workRange) == 0)
                return null;
            for (int i = firstRow; i <= lastRow; i++)
            {
                Excel.Range subRange = (Excel.Range)(workRange.Rows[i - firstRow + 1]);
                if (excelApp.WorksheetFunction.CountA(subRange) == 0)
                {
                    newFirstRow++;
                    //((Excel.Range)(workRange.Rows[i - firstRow + 1])).Delete();
                    mes += String.Format(Rekod.Properties.Resources.ExcelImporter_RowNEmpty, i) + "\n";
                }
                else
                    break;
            }
            for (int i = lastRow; i >= firstRow; i--)
            {
                Excel.Range subRange = (Excel.Range)(workRange.Rows[i - firstRow + 1]);
                if (excelApp.WorksheetFunction.CountA(subRange) == 0)
                {
                    newLastRow--;
                    //((Excel.Range)(workRange.Rows[i - firstRow + 1])).Delete();
                    mes += String.Format(Rekod.Properties.Resources.ExcelImporter_RowNEmpty, i) + "\n";
                }
                else
                    break;
            }
            for (int i = lastColumn; i >= firstColumn; i--)
            {
                Excel.Range subRange = (Excel.Range)(workRange.Columns[i - firstColumn + 1]);
                if (excelApp.WorksheetFunction.CountA(subRange) == 0)
                {
                    newLastColumn--;
                    //((Excel.Range)(workRange.Columns[i - firstColumn + 1])).Delete();
                    mes += String.Format(Rekod.Properties.Resources.ExcelImporter_ColumnNEmpty, i) + "\n";
                }
                else
                    break;
            }
            for (int i = firstColumn; i <= lastColumn; i++)
            {
                Excel.Range subRange = (Excel.Range)(workRange.Columns[i - firstColumn + 1]);
                if (excelApp.WorksheetFunction.CountA(subRange) == 0)
                {
                    newFirstColumn++;
                    //((Excel.Range)(workRange.Columns[i - firstColumn + 1])).Delete();
                    mes += String.Format(Rekod.Properties.Resources.ExcelImporter_ColumnNEmpty, i) + "\n";
                }
                else
                    break;
            }

            //MessageBox.Show(mes);
            String from = letterColumn(newFirstColumn) + newFirstRow.ToString();
            String to = letterColumn(newLastColumn) + newLastRow.ToString();
            //MessageBox.Show(from + ":" + to);
            return from + ":" + to;
        }

        private String letterColumn(int col)
        {
            col = col - 1;
            int first = 0;
            int second = 0;
            first = (col / 26);
            second = (col % 26 + 1);

            char fChar = (char)((int)('A') + first - 1);
            char sChar = (char)((int)('A') + second - 1);

            String res = ((first == 0) ? "" : fChar.ToString()) + ((second == 0) ? "" : sChar.ToString());
            return res;
        }
        #endregion

        override public void Init(FileInfo inputFile, tablesInfo tableInfo)
        {
            this.inputFile = inputFile;
            this.tableInfo = tableInfo;
            var excelAppl = new Excel.Application() { Visible = false };
            var workbook = excelAppl.Workbooks.Open(inputFile.FullName);
            excelUC.listCB.Items.Clear();
            Excel.Worksheet sheet = null;
            foreach (Excel.Worksheet worksheet in workbook.Sheets)
            {
                excelUC.listCB.Items.Add(worksheet.Name);
                if (sheet == null)
                    sheet = worksheet;
            }
            GC.Collect(2, GCCollectionMode.Forced); //не знаю зачем это
            excelUC.listCB.SelectedIndex = 0;
            var workrange = sheet.UsedRange;
            String rng = GetUsedNotEmptyRange(sheet);
            if (rng != null)
            {
                String upLeft = rng.Split(':')[0];
                String bottomRight = rng.Split(':')[1];
                excelUC.diapBeg.Text = upLeft;
                excelUC.diapEnd.Text = bottomRight;
                String sheetString = sheet.Name;
                String rangeString = rng;
                Excel.Worksheet workSheet = (Excel.Worksheet)(workbook.Sheets.Item[sheetString]);
                Excel.Range workRange = workSheet.get_Range(rangeString);
                // Где на странице Excel находятся данные
                int lastRow = workRange.Row + workRange.Rows.Count - 1;
                int firstRow = workRange.Row;
                int lastColumn = workRange.Column + workRange.Columns.Count - 1;
                int firstColumn = workRange.Column;
                // Получаются данные из excel в виде массива объектов
                var excValues = (object[,])(workRange.Value);
                // Сколько строк и сколько столбцов
                rowsCount = excValues.GetLength(0) - 1;//первая строка заголовки
            }
            else
            {
                excelUC.diapBeg.Text = excelUC.diapEnd.Text = "Нет данных";
                rowsCount = 0;
            }
            GC.Collect(2, GCCollectionMode.Forced);//не знаю зачем это 
            workbook.Close();
            excelAppl.Quit();
            WorkerReportsProgress = true;
        }
        override public void SetTableInfo(tablesInfo tableInfo)
        {
            this.tableInfo = tableInfo;
        }
        override public DataTable GetPreviewTable(int previewRowsCount = 100)
        {
            if (excelUC.listCB.SelectedItem == null)
                throw new Exception(Rekod.Properties.Resources.ExcelImporter_NotSelectedList);
            string rngString = excelUC.listCB.SelectedItem.ToString() + "$" + excelUC.diapBeg.Text + ":" + excelUC.diapEnd.Text;
            String sheetString = rngString.Split('$')[0];
            String rangeString = rngString.Split('$')[1];

            var excelAppl = new Excel.Application() { Visible = false };
            var workbook = excelAppl.Workbooks.Open(inputFile.FullName);
            Excel.Worksheet workSheet = (Excel.Worksheet)(workbook.Sheets.Item[sheetString]);
            Excel.Range workRange = workSheet.get_Range(rangeString);
            // Где на странице Excel находятся данные
            int lastRow = workRange.Row + workRange.Rows.Count - 1;
            int firstRow = workRange.Row;
            int lastColumn = workRange.Column + workRange.Columns.Count - 1;
            int firstColumn = workRange.Column;
            // Получаются данные из excel в виде массива объектов
            var excValues = (object[,])(workRange.Value);
            workbook.Close();

            // Сколько строк и сколько столбцов
            rowsCount = excValues.GetLength(0) - 1;//первая строка заголовки
            var colsCount = excValues.GetLength(1);
            excelAppl.Quit();

            DataTable result = new DataTable();
            for (int j = 1; j <= colsCount; j++)
                result.Columns.Add(excValues[1, j].ToString());
            for (int i = 2; i <= previewRowsCount + 1 && i <= rowsCount + 1; i++)
                result.Rows.Add(getRow(excValues, i, colsCount));
            return result;
        }

        private object[] getRow(object[,] values, int row, int colsCount)
        {
            var result = new object[colsCount];
            for (int j = 0; j < colsCount; j++)
                result[j] = values[row, j + 1];
            return result;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                object[] pms = (object[])e.Argument;
                load((List<FieldMatch>)pms[0], (string)pms[1]);
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        override public void Load(List<FieldMatch> fields)
        {
            if (excelUC.listCB.SelectedItem == null)
                throw new Exception(Rekod.Properties.Resources.ExcelImporter_NotSelectedList);
            if (!Regex.IsMatch(excelUC.diapBeg.Text, "[a-zA-z]{1,}\\d{1,}") || !Regex.IsMatch(excelUC.diapEnd.Text, "[a-zA-z]{1,}\\d{1,}"))
                throw new Exception(Rekod.Properties.Resources.ExcelImporter_NotSpecifiedRange);
            string rngString = excelUC.listCB.SelectedItem.ToString() + "$" + excelUC.diapBeg.Text + ":" + excelUC.diapEnd.Text;
            RunWorkerAsync(new object[] { fields, rngString });
        }

        private void load(List<FieldMatch> fields, string rngString)
        {
            if (fields.Count == 0)
                return;
            var parameters = new List<Interfaces.Params>();
            var insertSql = "insert into {0}.{1} (";
            string valuesStr = ") values (";
            foreach (var f in fields)
            {
                insertSql += "\"" + f.Dest.nameDB + "\",";
                valuesStr += ":" + f.Dest.nameDB + ",";
                var p = new Interfaces.Params() { paramName = f.Dest.nameDB };
                ImporterHelper.SetParamType(f.Dest, p);
                parameters.Add(p);
            }
            insertSql = insertSql.TrimEnd(',');
            valuesStr = valuesStr.TrimEnd(',');
            insertSql += valuesStr + ")";
            //читаем excel
            String sheetString = rngString.Split('$')[0];
            String rangeString = rngString.Split('$')[1]; 
            var excelAppl = new Excel.Application() { Visible = false };
            var workbook = excelAppl.Workbooks.Open(inputFile.FullName);
            Excel.Worksheet workSheet = (Excel.Worksheet)(workbook.Sheets.Item[sheetString]);
            Excel.Range workRange = workSheet.get_Range(rangeString);
            // Где на странице Excel находятся данные
            int lastRow = workRange.Row + workRange.Rows.Count - 1;
            int firstRow = workRange.Row;
            int lastColumn = workRange.Column + workRange.Columns.Count - 1;
            int firstColumn = workRange.Column;
            // Получаются данные из excel в виде массива объектов
            var excValues = (object[,])(workRange.Value);
            workbook.Close();

            // Сколько строк и сколько столбцов
            rowsCount = excValues.GetLength(0) - 1;//первая строка заголовки
            var colsCount = excValues.GetLength(1);
            excelAppl.Quit();

            DataTable datas = new DataTable();
            for (int j = 1; j <= colsCount; j++)
                datas.Columns.Add((String)excValues[1, j]);
            var sw = new SqlWork(true);
            sw.BeginTransaction();
            sw.sql = string.Format(insertSql, tableInfo.nameSheme, tableInfo.nameDB);
            for (int i = 2, report = 0; i <= rowsCount + 1; i++)
            {
                //добавляем в таблицу
                datas.Rows.Add(getRow(excValues, i, colsCount));
                report++;
                if (report >= rowsInSect)
                {
                    //убираем ненужные колонки
                    List<object> cols2delete = new List<object>();
                    for (int j = 0; j < datas.Columns.Count; j++)
                        if (fields.Find(w => w.Src == datas.Columns[j].Caption) == null)
                            cols2delete.Add(datas.Columns[j]);
                    foreach (var c in cols2delete)
                        datas.Columns.Remove((DataColumn)c);
                    //кладем в базу
                    for (int j = 0; j < datas.Rows.Count; j++)
                    {
                        for (int k = 0; k < datas.Columns.Count; k++)
                            ImporterHelper.SetParamValue(parameters[k], datas.Rows[j][k]);
                        sw.ExecuteNonQuery(parameters.ToArray());
                    }
                    report = 0;
                    datas = new DataTable();
                    for (int j = 1; j <= colsCount; j++)
                        datas.Columns.Add(excValues[1, j].ToString());
                }
                ReportProgress(0, i - 2);
            }
            sw.EndTransaction();
        }
    }
}

#region вариант с оле db не пашет
/*String connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + inputFile.FullName + ";Extended Properties=Excel 8.0;";
			DataTable dt = new DataTable("Harakuri");
			using (OleDbConnection conn = new OleDbConnection(connectionString))
			{
				conn.Open();
				DataSet DtSet = new System.Data.DataSet();
				OleDbDataAdapter MyAdapter = new System.Data.OleDb.OleDbDataAdapter(String.Format("select * from [{0}]", rngString), conn);
				DtSet.Tables.Add(dt);
				DataTableMapping tableMap = MyAdapter.TableMappings.Add("Table", "Harakuri");
				conn.Close();
			}
			return dt;
		}
	}
}*/
#endregion