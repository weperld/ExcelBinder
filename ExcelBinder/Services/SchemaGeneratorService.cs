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
            if (data.Count < 1) return;

            var header = data[0];
            var schema = new SchemaDefinition
            {
                ClassName = sheetName + "Data",
                Key = header.Length > 0 ? header[0] : "Id",
                Fields = new Dictionary<string, string>()
            };

            foreach (var colName in header)
            {
                if (string.IsNullOrWhiteSpace(colName)) continue;
                if (!schema.Fields.ContainsKey(colName))
                {
                    schema.Fields[colName] = "int"; // Default to int, user can change later
                }
            }

            File.WriteAllText(schemaOutputPath, JsonConvert.SerializeObject(schema, Formatting.Indented));
        }
    }
}
