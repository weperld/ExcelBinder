using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Services.Processors
{
    public class ConstantsProcessor : IFeatureProcessor
    {
        public string CategoryName => ProjectConstants.Categories.Constants;

        public bool IsSchemaPathVisible => false;
        public bool IsExportPathVisible => false;
        public bool IsScriptsPathVisible => true;
        public bool IsSchemaStatusVisible => false;
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => false;
        public bool IsOutputOptionsVisible => false;

        public Task ExecuteExportAsync(IExecutionViewModel vm) => Task.CompletedTask;

        public async Task ExecuteGenerateAsync(IExecutionViewModel vm)
        {
            if (vm.SelectedFeature == null) return;

            LogService.Instance.Clear();
            LogService.Instance.Info("Starting Constants Code Generation...");

            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();

            if (selectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet.");
                vm.ShowLogs();
                return;
            }

            if (!Directory.Exists(vm.SelectedFeature.ScriptsPath))
                Directory.CreateDirectory(vm.SelectedFeature.ScriptsPath);

            var excelService = new ExcelService();

            foreach (var item in selectedSheets)
            {
                try
                {
                    await ProcessSheet(item.File.FullPath, item.Sheet.SheetName, vm, excelService);
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error processing sheet '{item.Sheet.SheetName}' in {item.File.FileName}: {ex.Message}");
                }
            }

            LogService.Instance.Info("Constants Code Generation Finished.");
            vm.ShowLogs();
        }

        private async Task ProcessSheet(string filePath, string sheetName, IExecutionViewModel vm, ExcelService excelService)
        {
            // Global Rule: Skip sheets starting with #
            if (sheetName.StartsWith(ProjectConstants.Excel.CommentPrefix))
            {
                LogService.Instance.Info($"Skipping sheet '{sheetName}' as it starts with '{ProjectConstants.Excel.CommentPrefix}'");
                return;
            }

            var rawData = excelService.ReadExcel(filePath, sheetName).ToList();
            if (rawData.Count < 1)
            {
                LogService.Instance.Warning($"Sheet '{sheetName}' in {Path.GetFileName(filePath)} is empty.");
                return;
            }

            var header = rawData[0];
            int nameIdx = Array.FindIndex(header, h => h.Trim().Equals("Name", StringComparison.OrdinalIgnoreCase));
            int typeIdx = Array.FindIndex(header, h => h.Trim().Equals("Type", StringComparison.OrdinalIgnoreCase));
            int valueIdx = Array.FindIndex(header, h => h.Trim().Equals("Value", StringComparison.OrdinalIgnoreCase));

            if (nameIdx == -1 || typeIdx == -1 || valueIdx == -1)
            {
                LogService.Instance.Error($"Invalid headers in sheet '{sheetName}' of {Path.GetFileName(filePath)}. Required: 'Name', 'Type', 'Value'");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {vm.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public static partial class {sheetName}");
            sb.AppendLine("    {");

            int generatedEntries = 0;
            // Data starts from row 2
            for (int i = 1; i < rawData.Count; i++)
            {
                var row = rawData[i];
                if (row.Length <= Math.Max(nameIdx, Math.Max(typeIdx, valueIdx))) continue;
                
                string name = row[nameIdx].Trim();
                string type = row[typeIdx].Trim();
                string value = row[valueIdx].Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value)) continue;

                // Prefix # rule
                if (name.StartsWith(ProjectConstants.Excel.CommentPrefix)) continue;

                string formattedValue = FormatValue(type, value);
                sb.AppendLine($"        public const {type} {name} = {formattedValue};");
                generatedEntries++;
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            if (generatedEntries > 0)
            {
                string outputPath = Path.Combine(vm.SelectedFeature.ScriptsPath, sheetName + ProjectConstants.Extensions.CSharp);
                await File.WriteAllTextAsync(outputPath, sb.ToString());
                LogService.Instance.Info($"Successfully generated Constants: {sheetName} ({generatedEntries} constants)");
            }
            else
            {
                LogService.Instance.Warning($"No valid entries found in sheet '{sheetName}'. Skipping file generation.");
            }
        }

        private string FormatValue(string type, string value)
        {
            if (type.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                if (!value.StartsWith("\"") || !value.EndsWith("\""))
                {
                    return $"\"{value}\"";
                }
            }
            else if (type.Equals("float", StringComparison.OrdinalIgnoreCase))
            {
                if (!value.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{value}f";
                }
            }
            return value;
        }

        public async Task CreateTemplateAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                NPOI.SS.UserModel.IWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = workbook.CreateSheet("ConstantsSample");
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Name");
                headerRow.CreateCell(1).SetCellValue("Type");
                headerRow.CreateCell(2).SetCellValue("Value");

                var dataRow = sheet.CreateRow(1);
                dataRow.CreateCell(0).SetCellValue("SAMPLE_CONST");
                dataRow.CreateCell(1).SetCellValue("int");
                dataRow.CreateCell(2).SetCellValue("100");

                workbook.Write(fs);
            });
        }
    }
}
