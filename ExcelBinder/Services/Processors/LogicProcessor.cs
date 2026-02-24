using System;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;

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

        public System.Threading.Tasks.Task ExecuteExportAsync(IExecutionViewModel vm)
        {
            // Logic doesn't support export
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async System.Threading.Tasks.Task ExecuteGenerateAsync(IExecutionViewModel vm)
        {
            if (vm.SelectedFeature == null) return;
            
            if (string.IsNullOrEmpty(vm.SelectedFeature.Templates.DataClass) || !File.Exists(vm.SelectedFeature.Templates.DataClass))
            {
                LogService.Instance.Clear();
                LogService.Instance.Warning($"Template file not found: {vm.SelectedFeature.Templates.DataClass}. Code generation cancelled.");
                vm.ShowLogs();
                return;
            }

            if (!Directory.Exists(vm.SelectedFeature.ScriptsPath)) Directory.CreateDirectory(vm.SelectedFeature.ScriptsPath);

            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();
            if (selectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet for logic generation.");
                vm.ShowLogs();
                return;
            }

            LogService.Instance.Clear();
            LogService.Instance.Info($"Starting Logic Generation for {selectedSheets.Count} sheets...");

            foreach (var item in selectedSheets)
            {
                try
                {
                    var data = await System.Threading.Tasks.Task.Run(() => _excelService.ReadExcel(item.File.FullPath, item.Sheet.SheetName).ToList());
                    if (data.Count < 1) continue; // 최소 1행 필요 (첫 행 헤더)

                    string className = item.Sheet.SheetName;
                    var context = await System.Threading.Tasks.Task.Run(() => _logicParserService.PrepareLogicContext(data, vm.SelectedFeature, vm.Namespace, className));
                    string? code = await System.Threading.Tasks.Task.Run(() => _codeGenService.GenerateLogicCode(context, vm.SelectedFeature));
                    if (!string.IsNullOrEmpty(code))
                    {
                        await System.Threading.Tasks.Task.Run(() => File.WriteAllText(Path.Combine(vm.SelectedFeature.ScriptsPath, className + ProjectConstants.Extensions.CSharp), code));
                        LogService.Instance.Info($"Generated Logic: {className}");
                    }
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error($"Error generating logic for {item.Sheet.SheetName}: {ex.Message}");
                }
            }
            
            LogService.Instance.Info("Logic Generation Finished.");
            vm.ShowLogs();
        }

        public System.Threading.Tasks.Task CreateTemplateAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}
