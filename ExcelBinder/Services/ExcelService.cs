using System;
using System.Collections.Generic;
using System.IO;
using ExcelBinder.Models;
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

        public List<(string[] Data, int OriginalIndex)> GetFilteredData(IEnumerable<string[]> rawData)
        {
            var dataList = rawData.ToList();
            // 전역 규칙: 첫 번째 행을 헤더로 사용하며 두 번째 행부터 데이터를 처리함
            if (dataList.Count < ProjectConstants.Excel.DataStartRowIndex + 1) return new List<(string[], int)>();

            var header = dataList[ProjectConstants.Excel.HeaderRowIndex]; // 첫 번째 행을 헤더로 사용
            var rows = dataList.Skip(ProjectConstants.Excel.DataStartRowIndex); // 두 번째 행부터 실제 데이터

            // Identify the first valid column (not starting with comment prefix)
            int firstValidColIdx = -1;
            for (int i = 0; i < header.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(header[i]) && !header[i].TrimStart().StartsWith(Models.ProjectConstants.Excel.CommentPrefix))
                {
                    firstValidColIdx = i;
                    break;
                }
            }

            // Filter rows where the first valid column's value starts with comment prefix
            return rows.Select((row, idx) => (row, originalRowIndex: idx + ProjectConstants.Excel.DataStartRowIndex + 1)) // Skip 1 row, so data starts at row 2
                .Where(item =>
                {
                    if (firstValidColIdx == -1 || firstValidColIdx >= item.row.Length) return true;
                    var cellValue = item.row[firstValidColIdx];
                    return string.IsNullOrEmpty(cellValue) || !cellValue.TrimStart().StartsWith(Models.ProjectConstants.Excel.CommentPrefix);
                }).ToList();
        }
    }
}
