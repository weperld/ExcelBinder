using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            using var stream = OpenStream(filePath);
            IWorkbook workbook = OpenWorkbook(filePath, stream);
            try
            {
                var names = new List<string>();
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    names.Add(workbook.GetSheetName(i));
                }
                return names;
            }
            finally
            {
                (workbook as IDisposable)?.Dispose();
            }
        }

        public List<string[]> ReadExcel(string filePath, string sheetName = "")
        {
            using var stream = OpenStream(filePath);
            IWorkbook workbook = OpenWorkbook(filePath, stream);
            try
            {
                ISheet sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);
                if (sheet == null) return new List<string[]>();

                return ReadSheetRows(sheet);
            }
            finally
            {
                (workbook as IDisposable)?.Dispose();
            }
        }

        public Dictionary<string, List<string[]>> ReadMultipleSheets(string filePath, IEnumerable<string> sheetNames)
        {
            using var stream = OpenStream(filePath);
            IWorkbook workbook = OpenWorkbook(filePath, stream);
            try
            {
                var result = new Dictionary<string, List<string[]>>(StringComparer.OrdinalIgnoreCase);
                foreach (var name in sheetNames)
                {
                    ISheet sheet = workbook.GetSheet(name);
                    if (sheet == null) continue;

                    result[name] = ReadSheetRows(sheet);
                }
                return result;
            }
            finally
            {
                (workbook as IDisposable)?.Dispose();
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

        // 엑셀에서 파일을 열어둔 상태에서도 읽을 수 있도록 ReadWrite 공유로 연다.
        private static FileStream OpenStream(string filePath) =>
            new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        private static IWorkbook OpenWorkbook(string filePath, Stream stream)
        {
            return Path.GetExtension(filePath).ToLower() switch
            {
                ".xlsx" => new XSSFWorkbook(stream),
                ".xls" => new HSSFWorkbook(stream),
                _ => throw new NotSupportedException("Unsupported excel format")
            };
        }

        private static List<string[]> ReadSheetRows(ISheet sheet)
        {
            var result = new List<string[]>();
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                // LastCellNum은 셀이 하나도 없는 행에서 -1을 반환한다.
                string[] cellValues = new string[Math.Max(0, (int)row.LastCellNum)];
                for (int j = 0; j < cellValues.Length; j++)
                {
                    cellValues[j] = GetCellString(row.GetCell(j));
                }
                result.Add(cellValues);
            }
            return result;
        }

        /// <summary>
        /// 셀 값을 타입 기반으로 읽어 invariant 문자열로 반환합니다.
        /// cell.ToString()은 수식 셀에서 수식 텍스트를, 숫자 셀에서 현재 로케일 형식
        /// 문자열을 반환하므로 사용하지 않습니다 (수식/날짜/culture 왜곡의 근원).
        /// </summary>
        internal static string GetCellString(ICell? cell)
        {
            if (cell == null) return string.Empty;
            return GetCellString(cell, cell.CellType);
        }

        private static string GetCellString(ICell cell, CellType type)
        {
            switch (type)
            {
                case CellType.Blank:
                    return string.Empty;
                case CellType.String:
                    return cell.StringCellValue ?? string.Empty;
                case CellType.Boolean:
                    return cell.BooleanCellValue ? "true" : "false";
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        var date = cell.DateCellValue;
                        return date?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
                    }
                    return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                case CellType.Formula:
                    // 재계산 대신 엑셀이 저장해 둔 캐시 결과를 사용한다 (항상 존재, 외부 참조에도 안전).
                    return GetCellString(cell, cell.CachedFormulaResultType);
                case CellType.Error:
                    LogService.Instance.Warning($"Error cell at '{cell.Sheet.SheetName}' {cell.Address}: treated as empty.");
                    return string.Empty;
                default:
                    return cell.ToString() ?? string.Empty;
            }
        }
    }
}
