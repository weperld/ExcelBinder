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
    public class EnumProcessor : IFeatureProcessor
    {
        public string CategoryName => ProjectConstants.Categories.Enum;

        public bool IsSchemaPathVisible => false;
        public bool IsExportPathVisible => false;
        public bool IsScriptsPathVisible => true;
        public bool IsSchemaStatusVisible => false;
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => false;
        public bool IsOutputOptionsVisible => false;

        public Task ExecuteExportAsync(IExecutionViewModel vm) => Task.CompletedTask;

        public async Task CreateTemplateAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                NPOI.SS.UserModel.IWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = workbook.CreateSheet("Definition");
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Name");
                headerRow.CreateCell(1).SetCellValue("Type");
                headerRow.CreateCell(2).SetCellValue("IsFlag");

                // Add sample data or just leave it blank with headers
                workbook.Write(fs);
            });
        }

        public async Task ExecuteGenerateAsync(IExecutionViewModel vm)
        {
            if (vm.SelectedFeature == null) return;

            LogService.Instance.Clear();
            LogService.Instance.Info("Starting Enum Code Generation...");

            var selectedFiles = vm.ExcelFiles.Where(f => f.IsSelected).ToList();

            if (selectedFiles.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one excel file.");
                vm.ShowLogs();
                return;
            }

            if (!Directory.Exists(vm.SelectedFeature.ScriptsPath))
                Directory.CreateDirectory(vm.SelectedFeature.ScriptsPath);

            foreach (var file in selectedFiles)
            {
                try
                {
                    await ProcessFile(file.FullPath, vm);
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error processing file {file.FileName}: {ex.Message}");
                }
            }

            LogService.Instance.Info("Enum Code Generation Finished.");
            vm.ShowLogs();
        }

        private async Task ProcessFile(string filePath, IExecutionViewModel vm)
        {
            var excelService = new ExcelService();
            var sheetNames = excelService.GetSheetNames(filePath);

            string definitionSheetName = sheetNames.FirstOrDefault(s => s.Equals("Definition", StringComparison.OrdinalIgnoreCase));
            if (definitionSheetName == null)
            {
                LogService.Instance.Error($"'Definition' sheet not found in {Path.GetFileName(filePath)}. Generation cancelled for this file.");
                return;
            }

            // Global Rule: Skip sheets starting with #
            if (definitionSheetName.StartsWith(ProjectConstants.Excel.CommentPrefix))
            {
                LogService.Instance.Info($"Skipping 'Definition' sheet in {Path.GetFileName(filePath)} as it starts with '{ProjectConstants.Excel.CommentPrefix}'");
                return;
            }

            var rawData = excelService.ReadExcel(filePath, definitionSheetName).ToList();
            if (rawData.Count < 1)
            {
                LogService.Instance.Warning($"'Definition' sheet in {Path.GetFileName(filePath)} is empty.");
                return;
            }

            var header = rawData[0];
            int nameIdx = Array.FindIndex(header, h => h.Trim().Equals("Name", StringComparison.OrdinalIgnoreCase));
            int typeIdx = Array.FindIndex(header, h => h.Trim().Equals("Type", StringComparison.OrdinalIgnoreCase));
            int flagIdx = Array.FindIndex(header, h => h.Trim().Equals("IsFlag", StringComparison.OrdinalIgnoreCase));

            if (nameIdx == -1 || flagIdx == -1)
            {
                LogService.Instance.Error($"Invalid headers in 'Definition' sheet of {Path.GetFileName(filePath)}. Required: 'Name', 'IsFlag'");
                return;
            }

            // Data starts from the second row
            for (int i = 1; i < rawData.Count; i++)
            {
                var row = rawData[i];
                if (row.Length <= nameIdx || string.IsNullOrWhiteSpace(row[nameIdx])) continue;
                
                // Prefix # rule
                if (row[nameIdx].TrimStart().StartsWith(ProjectConstants.Excel.CommentPrefix)) continue;

                string enumName = row[nameIdx].Trim();
                string underlyingType = typeIdx != -1 && row.Length > typeIdx ? row[typeIdx].Trim() : "";
                bool isFlag = flagIdx != -1 && row.Length > flagIdx && row[flagIdx].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);

                await GenerateEnum(filePath, enumName, underlyingType, isFlag, vm, sheetNames);
            }
        }

        private async Task GenerateEnum(string filePath, string enumName, string underlyingType, bool isFlag, IExecutionViewModel vm, List<string> sheetNames)
        {
            string targetSheet = sheetNames.FirstOrDefault(s => s.Equals(enumName, StringComparison.OrdinalIgnoreCase));
            if (targetSheet == null)
            {
                LogService.Instance.Warning($"Sheet '{enumName}' not found in {Path.GetFileName(filePath)}. Skipping Enum generation.");
                return;
            }

            if (targetSheet.StartsWith(ProjectConstants.Excel.CommentPrefix))
            {
                LogService.Instance.Info($"Skipping Enum '{enumName}' as its sheet starts with '{ProjectConstants.Excel.CommentPrefix}'");
                return;
            }

            var excelService = new ExcelService();
            var rawData = excelService.ReadExcel(filePath, targetSheet).ToList();
            if (rawData.Count < 1)
            {
                LogService.Instance.Warning($"Sheet '{targetSheet}' is empty. Skipping Enum generation.");
                return;
            }

            var header = rawData[0];
            int nameIdx = Array.FindIndex(header, h => h.Trim().Equals("Name", StringComparison.OrdinalIgnoreCase));
            int valueIdx = Array.FindIndex(header, h => h.Trim().Equals("Value", StringComparison.OrdinalIgnoreCase));

            if (nameIdx == -1 || valueIdx == -1)
            {
                LogService.Instance.Error($"Invalid headers in sheet '{targetSheet}'. Required: 'Name', 'Value'. Generation failed for '{enumName}'.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {vm.Namespace}");
            sb.AppendLine("{");

            if (isFlag)
            {
                sb.AppendLine("    [Flags]");
            }

            string typeSuffix = string.IsNullOrWhiteSpace(underlyingType) ? "" : $" : {underlyingType}";
            sb.AppendLine($"    public enum {enumName}{typeSuffix}");
            sb.AppendLine("    {");

            int generatedEntries = 0;
            for (int i = 1; i < rawData.Count; i++)
            {
                var row = rawData[i];
                if (row.Length <= nameIdx || string.IsNullOrWhiteSpace(row[nameIdx])) continue;
                
                // Prefix # rule
                if (row[nameIdx].TrimStart().StartsWith(ProjectConstants.Excel.CommentPrefix)) continue;

                string entryName = row[nameIdx].Trim();
                string entryValue = row.Length > valueIdx ? row[valueIdx].Trim() : "";

                if (string.IsNullOrEmpty(entryValue))
                {
                    sb.AppendLine($"        {entryName},");
                }
                else
                {
                    sb.AppendLine($"        {entryName} = {entryValue},");
                }
                generatedEntries++;
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            if (generatedEntries > 0)
            {
                string outputPath = Path.Combine(vm.SelectedFeature.ScriptsPath, enumName + ProjectConstants.Extensions.CSharp);
                await File.WriteAllTextAsync(outputPath, sb.ToString());
                LogService.Instance.Info($"Successfully generated Enum: {enumName} ({generatedEntries} members)");
            }
            else
            {
                LogService.Instance.Warning($"No valid entries found for Enum '{enumName}'. Skipping file generation.");
            }
        }
    }
}
