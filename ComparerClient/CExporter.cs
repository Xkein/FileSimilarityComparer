using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DataTable = System.Data.DataTable;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ComparerClient
{
    class CExporter
    {
        public enum ExportType
        {
            xls
        }

        public static bool ExportAs<T>(IEnumerable<T> enumerable, ExportType exportType = ExportType.xls)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            dataTable.TableName = type.Name;

            DataColumn column;
            DataRow row;

            foreach (var property in properties)
            {
                column = new DataColumn();
                column.DataType = property.PropertyType;
                column.ColumnName = property.Name;
                column.AutoIncrement = false;
                column.Caption = property.Name;
                column.ReadOnly = false;
                column.Unique = false;

                dataTable.Columns.Add(column);
            }

            foreach (var item in enumerable)
            {
                row = dataTable.NewRow();

                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(item);
                }

                dataTable.Rows.Add(row);
            }

            if (dataTable.Rows.Count > 0)
            {
                switch (exportType)
                {
                    case ExportType.xls:
                        return ExportAsXls(dataTable);
                }
            }
            return false;
        }

        static bool ExportAsXls(DataTable dataTable)
        {
            string fileName = dataTable.TableName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx";
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);

            ExcelPackage package = new ExcelPackage(fileStream);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dataTable.TableName);

            int rowIndex = 1;
            int colIndex = 1;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                worksheet.Cells[rowIndex, colIndex + i].Value = dataTable.Columns[i].ColumnName;
            }

            rowIndex++;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    worksheet.Cells[rowIndex + i, colIndex + j].Value = dataTable.Rows[i][j].ToString();
                }

                worksheet.Row(rowIndex + i).CustomHeight = true;
            }

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                worksheet.Column(colIndex + i).AutoFit();
                worksheet.Column(colIndex + i).Width += 2;
            }

            worksheet.Cells.Style.Font.Name = "Arial";
            //worksheet.Cells.Style.Font.Bold = true;
            worksheet.Cells.Style.Font.Size = 12;
            worksheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);
            //worksheet.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //worksheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DimGray);
            worksheet.Cells.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#0097DD"));
            //worksheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //worksheet.Cells.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.WrapText = false;
            worksheet.Cells.Style.Numberformat.Format = "@";
            //worksheet.Cells.Style.ShrinkToFit = true;

            package.Save();

            fileStream.Close();
            fileStream.Dispose();

            worksheet.Dispose();
            package.Dispose();

            return true;
        }
        /*
        static bool ExportAsXls_(DataTable dataTable)
        {
            var excel = new Excel.Application();
            excel.Visible = false;

            var workBook = excel.Workbooks.Add();
            var workSheet = workBook.Worksheets[1] as Excel.Worksheet;

            workSheet.Name = dataTable.TableName;

            int rowIndex = 1;
            int colIndex = 1;

            Excel.Range range;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                range = workSheet.Cells[rowIndex, colIndex + i];
                range.Value = dataTable.Columns[i].ColumnName;
                range.Font.Bold = true;
            }

            rowIndex++;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    workSheet.Cells[rowIndex + i, colIndex + j] = dataTable.Rows[i][j].ToString();
                }
            }

            workSheet.Cells.Columns.AutoFit();

            excel.DisplayAlerts = false;
            workBook.Saved = true;

            workBook.SaveCopyAs(dataTable.TableName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx");

            return true;
        }
        */
    }
}
