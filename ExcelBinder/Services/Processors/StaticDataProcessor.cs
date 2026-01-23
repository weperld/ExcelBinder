using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;
using Newtonsoft.Json;

namespace ExcelBinder.Services.Processors
{
    public class StaticDataProcessor : IFeatureProcessor
    {
        public string CategoryName => ProjectConstants.Categories.StaticData;

        public bool IsSchemaPathVisible => true;
        public bool IsExportPathVisible => true;
        public bool IsScriptsPathVisible => true;
        public bool IsTypeMappingsVisible => true;
        public bool IsTemplatesVisible => true;
        public bool IsOutputOptionsVisible => true;

        public async System.Threading.Tasks.Task ExecuteExportAsync(MainViewModel vm)
        {
            if (vm.SelectedFeature == null) return;
            if (!Directory.Exists(vm.SelectedFeature.ExportPath)) Directory.CreateDirectory(vm.SelectedFeature.ExportPath);

            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();

            if (selectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet with a valid schema.");
                vm.ShowLogs();
                return;
            }

            LogService.Instance.Clear();
            LogService.Instance.Info($"Starting Export for {selectedSheets.Count} sheets...");

            var excelService = new ExcelService();
            var exportService = new ExportService();

            foreach (var item in selectedSheets)
            {
                string schemaFile = vm.GetSchemaPath(item.File.FullPath, item.Sheet.SheetName);
                if (!File.Exists(schemaFile))
                {
                    LogService.Instance.Warning($"Schema not found for {item.Sheet.SheetName} in {item.File.FileName}");
                    continue;
                }

                try 
                {
                    var schema = await System.Threading.Tasks.Task.Run(() => JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaFile)));
                    if (schema == null) continue;

                    var data = await System.Threading.Tasks.Task.Run(() => excelService.ReadExcel(item.File.FullPath, item.Sheet.SheetName).ToList());

                    if (vm.IsBinaryChecked && vm.SelectedFeature.OutputOptions.SupportsBinary)
                    {
                        string binaryPath = Path.Combine(vm.SelectedFeature.ExportPath, schema.ClassName + vm.SelectedFeature.OutputOptions.Extension);
                        await System.Threading.Tasks.Task.Run(() => exportService.ExportToBinary(schema, data, binaryPath, vm.SelectedFeature));
                        LogService.Instance.Info($"Exported Binary: {schema.ClassName}");
                    }

                    if (vm.IsJsonChecked && vm.SelectedFeature.OutputOptions.SupportsJson)
                    {
                        string jsonPath = Path.Combine(vm.SelectedFeature.ExportPath, schema.ClassName + ProjectConstants.Extensions.Json);
                        await System.Threading.Tasks.Task.Run(() => exportService.ExportToJson(schema, data, jsonPath, vm.SelectedFeature));
                        LogService.Instance.Info($"Exported JSON: {schema.ClassName}");
                    }
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error exporting {item.Sheet.SheetName} from {item.File.FileName}: {ex.Message}");
                }
            }
            
            LogService.Instance.Info("Export Process Finished.");
            vm.ShowLogs();
        }

        public async System.Threading.Tasks.Task ExecuteGenerateAsync(MainViewModel vm)
        {
            if (vm.SelectedFeature == null || string.IsNullOrEmpty(vm.SelectedFeature.ScriptsPath)) return;

            if (string.IsNullOrEmpty(vm.SelectedFeature.Templates.DataClass) || !File.Exists(vm.SelectedFeature.Templates.DataClass))
            {
                LogService.Instance.Clear();
                LogService.Instance.Warning($"Template file not found: {vm.SelectedFeature.Templates.DataClass}. Code generation cancelled.");
                vm.ShowLogs();
                return;
            }

            if (!Directory.Exists(vm.SelectedFeature.ScriptsPath)) Directory.CreateDirectory(vm.SelectedFeature.ScriptsPath);

            LogService.Instance.Clear();
            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();
            var schemas = new List<SchemaDefinition>();

            LogService.Instance.Info($"Starting Code Generation for {selectedSheets.Count} sheets...");

            if (selectedSheets.Count > 0)
            {
                foreach (var item in selectedSheets)
                {
                    string schemaFile = vm.GetSchemaPath(item.File.FullPath, item.Sheet.SheetName);
                    if (!File.Exists(schemaFile))
                    {
                        LogService.Instance.Warning($"Schema not found for {item.Sheet.SheetName} in {item.File.FileName}");
                        continue;
                    }
                    await System.Threading.Tasks.Task.Run(() => vm.ProcessSchema(schemaFile, schemas));
                }
            }
            else if (Directory.Exists(vm.SelectedFeature.SchemaPath))
            {
                LogService.Instance.Info("No sheets selected. Generating code for all schemas in schema path...");
                foreach (var schemaFile in Directory.GetFiles(vm.SelectedFeature.SchemaPath, $"*{ProjectConstants.Extensions.Json}"))
                {
                    await System.Threading.Tasks.Task.Run(() => vm.ProcessSchema(schemaFile, schemas));
                }
            }

            LogService.Instance.Info("Code Generation Finished.");
            vm.ShowLogs();
        }
    }
}
