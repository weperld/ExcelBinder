using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    public class SchemaGeneratorService
    {
        public void GenerateSchemaFromExcel(string excelPath, string sheetName, string schemaOutputPath)
        {
            var excelService = new ExcelService();
            var data = excelService.ReadExcel(excelPath, sheetName).ToList();
            // 전역 규칙: 첫 번째 행을 헤더로 사용. 최소 1행 필요.
            if (data.Count < ProjectConstants.Excel.DataStartRowIndex) return;

            var header = data[ProjectConstants.Excel.HeaderRowIndex]; // 첫 번째 행을 헤더로 사용
            var firstValidColumn = header.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c) && !c.TrimStart().StartsWith(ProjectConstants.Excel.CommentPrefix));

            var schema = new SchemaDefinition
            {
                ClassName = sheetName + "Data",
                Key = firstValidColumn ?? ProjectConstants.Excel.DefaultSheetKey,
                Fields = new Dictionary<string, string>()
            };

            foreach (var colName in header)
            {
                if (string.IsNullOrWhiteSpace(colName)) continue;
                if (colName.TrimStart().StartsWith(ProjectConstants.Excel.CommentPrefix)) continue;

                if (!schema.Fields.ContainsKey(colName))
                {
                    schema.Fields[colName] = ProjectConstants.Types.Int; // Default to int, user can change later
                }
            }

            File.WriteAllText(schemaOutputPath, JsonConvert.SerializeObject(schema, Formatting.Indented));
        }
    }
}
