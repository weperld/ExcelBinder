using System;
using System.IO;
using System.Linq;
using ExcelBinder.Models;

namespace ExcelBinder.Services.Processors
{
    public class LogicProcessor : IFeatureProcessor
    {
        private readonly ExcelService _excelService = new();
        private readonly LogicParserService _logicParserService = new();
        private readonly CodeGeneratorService _codeGenService = new();

        public string CategoryName => ProjectConstants.Categories.Logic;

        public bool IsSchemaPathVisible => false;
        public bool IsExportPathVisible => false;
        public bool IsScriptsPathVisible => true;
        public bool IsSchemaStatusVisible => false;
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => true;
        public bool IsOutputOptionsVisible => false;

        public System.Threading.Tasks.Task ExecuteExportAsync(ExecutionRequest request)
        {
            // Logic doesn't support export
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async System.Threading.Tasks.Task ExecuteGenerateAsync(ExecutionRequest request)
        {
            if (string.IsNullOrEmpty(request.Feature.Templates.DataClass) || !File.Exists(request.Feature.Templates.DataClass))
            {
                LogService.Instance.Clear();
                LogService.Instance.Warning($"Template file not found: {request.Feature.Templates.DataClass}. Code generation cancelled.");
                return;
            }

            if (!Directory.Exists(request.Feature.ScriptsPath)) Directory.CreateDirectory(request.Feature.ScriptsPath);

            if (request.SelectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet for logic generation.");
                return;
            }

            LogService.Instance.Clear();
            LogService.Instance.Info($"Starting Logic Generation for {request.SelectedSheets.Count} sheets...");

            foreach (var sheet in request.SelectedSheets)
            {
                try
                {
                    var data = await System.Threading.Tasks.Task.Run(() => _excelService.ReadExcel(sheet.FilePath, sheet.SheetName).ToList());
                    if (data.Count < 1) continue; // 최소 1행 필요 (첫 행 헤더)

                    string className = sheet.SheetName;
                    var context = await System.Threading.Tasks.Task.Run(() => _logicParserService.PrepareLogicContext(data, request.Feature, request.Namespace, className));
                    string? code = await System.Threading.Tasks.Task.Run(() => _codeGenService.GenerateLogicCode(context, request.Feature));
                    if (!string.IsNullOrEmpty(code))
                    {
                        await System.Threading.Tasks.Task.Run(() => SafeFile.AtomicWriteText(Path.Combine(request.Feature.ScriptsPath, className + ProjectConstants.Extensions.CSharp), code));
                        LogService.Instance.Info($"Generated Logic: {className}");
                    }
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error generating logic for {sheet.SheetName}: {ex.Message}");
                }
            }

            LogService.Instance.Info("Logic Generation Finished.");
        }

        public System.Threading.Tasks.Task CreateTemplateAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}
