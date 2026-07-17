using System.IO;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    /// <summary>스키마 파일 3단 폴백 탐색 (구체적인 이름 → 느슨한 이름 순).</summary>
    public static class SchemaLocator
    {
        public static string Resolve(string schemaDir, string excelFullPath, string sheetName)
        {
            string excelName = Path.GetFileNameWithoutExtension(excelFullPath);

            // Try [ExcelName]_[SheetName]_Schema.json
            string path1 = Path.Combine(schemaDir, $"{excelName}_{sheetName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path1)) return path1;

            // Try [ExcelName]_Schema.json
            string path2 = Path.Combine(schemaDir, $"{excelName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path2)) return path2;

            // Fallback to sheetName.json
            string path3 = Path.Combine(schemaDir, sheetName + ProjectConstants.Extensions.Json);
            if (File.Exists(path3)) return path3;

            return path1; // Default to the most specific one
        }
    }
}
