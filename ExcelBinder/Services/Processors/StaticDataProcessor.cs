using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services.Processors
{
    public class StaticDataProcessor : IFeatureProcessor
    {
        private readonly ExcelService _excelService = new();
        private readonly ExportService _exportService = new();
        private readonly CodeGeneratorService _codeGenService = new();

        public string CategoryName => ProjectConstants.Categories.StaticData;

        public bool IsSchemaPathVisible => true;
        public bool IsExportPathVisible => true;
        public bool IsScriptsPathVisible => true;
        public bool IsSchemaStatusVisible => true;
        public bool IsTypeMappingsVisible => true;
        public bool IsTemplatesVisible => true;
        public bool IsOutputOptionsVisible => true;

        public async System.Threading.Tasks.Task ExecuteExportAsync(ExecutionRequest request)
        {
            if (!Directory.Exists(request.Feature.ExportPath)) Directory.CreateDirectory(request.Feature.ExportPath);

            if (request.SelectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet with a valid schema.");
                return;
            }

            LogService.Instance.Clear();
            LogService.Instance.Info($"Starting Export for {request.SelectedSheets.Count} sheets...");

            foreach (var sheet in request.SelectedSheets)
            {
                string schemaFile = SchemaLocator.Resolve(request.Feature.SchemaPath, sheet.FilePath, sheet.SheetName);
                if (!File.Exists(schemaFile))
                {
                    LogService.Instance.Warning($"Schema not found for {sheet.SheetName} in {Path.GetFileName(sheet.FilePath)}");
                    continue;
                }

                try
                {
                    var schema = await System.Threading.Tasks.Task.Run(() => JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaFile)));
                    if (schema == null) continue;

                    var data = await System.Threading.Tasks.Task.Run(() => _excelService.ReadExcel(sheet.FilePath, sheet.SheetName).ToList());

                    if (request.ExportBinary && request.Feature.OutputOptions.SupportsBinary)
                    {
                        string binaryPath = Path.Combine(request.Feature.ExportPath, schema.ClassName + request.Feature.OutputOptions.Extension);
                        await System.Threading.Tasks.Task.Run(() => _exportService.ExportToBinary(schema, data, binaryPath, request.Feature));
                        LogService.Instance.Info($"Exported Binary: {schema.ClassName}");
                    }

                    if (request.ExportJson && request.Feature.OutputOptions.SupportsJson)
                    {
                        string jsonPath = Path.Combine(request.Feature.ExportPath, schema.ClassName + ProjectConstants.Extensions.Json);
                        await System.Threading.Tasks.Task.Run(() => _exportService.ExportToJson(schema, data, jsonPath, request.Feature));
                        LogService.Instance.Info($"Exported JSON: {schema.ClassName}");
                    }
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error exporting {sheet.SheetName} from {Path.GetFileName(sheet.FilePath)}: {ex.Message}");
                }
            }

            LogService.Instance.Info("Export Process Finished.");
        }

        public async System.Threading.Tasks.Task ExecuteGenerateAsync(ExecutionRequest request)
        {
            if (string.IsNullOrEmpty(request.Feature.ScriptsPath)) return;

            if (string.IsNullOrEmpty(request.Feature.Templates.DataClass) || !File.Exists(request.Feature.Templates.DataClass))
            {
                LogService.Instance.Clear();
                LogService.Instance.Warning($"Template file not found: {request.Feature.Templates.DataClass}. Code generation cancelled.");
                return;
            }

            if (!Directory.Exists(request.Feature.ScriptsPath)) Directory.CreateDirectory(request.Feature.ScriptsPath);

            LogService.Instance.Clear();
            var schemas = new List<SchemaDefinition>();

            LogService.Instance.Info($"Starting Code Generation for {request.SelectedSheets.Count} sheets...");

            if (request.SelectedSheets.Count > 0)
            {
                foreach (var sheet in request.SelectedSheets)
                {
                    string schemaFile = SchemaLocator.Resolve(request.Feature.SchemaPath, sheet.FilePath, sheet.SheetName);
                    if (!File.Exists(schemaFile))
                    {
                        LogService.Instance.Warning($"Schema not found for {sheet.SheetName} in {Path.GetFileName(sheet.FilePath)}");
                        continue;
                    }
                    await System.Threading.Tasks.Task.Run(() => ProcessSchema(schemaFile, schemas, request));
                }
            }
            else if (Directory.Exists(request.Feature.SchemaPath))
            {
                LogService.Instance.Info("No sheets selected. Generating code for all schemas in schema path...");
                foreach (var schemaFile in Directory.GetFiles(request.Feature.SchemaPath, $"*{ProjectConstants.Extensions.Json}"))
                {
                    await System.Threading.Tasks.Task.Run(() => ProcessSchema(schemaFile, schemas, request));
                }
            }

            LogService.Instance.Info("Code Generation Finished.");
        }

        private void ProcessSchema(string schemaFile, List<SchemaDefinition> schemas, ExecutionRequest request)
        {
            try
            {
                var schema = JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaFile));
                if (schema == null)
                {
                    LogService.Instance.Error($"Failed to deserialize schema: {schemaFile}");
                    return;
                }
                schemas.Add(schema);

                string? code = _codeGenService.GenerateDataCode(schema, request.Feature, request.Namespace);
                if (!string.IsNullOrEmpty(code))
                {
                    SafeFile.AtomicWriteText(Path.Combine(request.Feature.ScriptsPath, schema.ClassName + ProjectConstants.Extensions.CSharp), code);
                    LogService.Instance.Info($"Generated Code: {schema.ClassName}");
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Error processing schema {Path.GetFileName(schemaFile)}: {ex.Message}");
            }
        }

        public System.Threading.Tasks.Task CreateTemplateAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}
