using System;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace ExcelBinder.Services
{
    public class ExcelService
    {
        public List<string> GetSheetNames(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            IWorkbook workbook = Path.GetExtension(filePath).ToLower() switch
            {
                ".xlsx" => new XSSFWorkbook(stream),
                ".xls" => new HSSFWorkbook(stream),
                _ => throw new NotSupportedException("Unsupported excel format")
            };
            var names = new List<string>();
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                names.Add(workbook.GetSheetName(i));
            }
            return names;
        }

        public IEnumerable<string[]> ReadExcel(string filePath, string sheetName = "")
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            IWorkbook workbook = Path.GetExtension(filePath).ToLower() switch
            {
                ".xlsx" => new XSSFWorkbook(stream),
                ".xls" => new HSSFWorkbook(stream),
                _ => throw new NotSupportedException("Unsupported excel format")
            };

            ISheet sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);
            if (sheet == null) yield break;

            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                string[] cellValues = new string[row.LastCellNum];
                for (int j = 0; j < row.LastCellNum; j++)
                {
                    ICell cell = row.GetCell(j);
                    cellValues[j] = cell?.ToString() ?? string.Empty;
                }
                yield return cellValues;
            }
        }
    }
}
