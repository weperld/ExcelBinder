using System;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Services.Processors
{
    public class SchemaGenProcessor : IFeatureProcessor
    {
        public string CategoryName => ProjectConstants.Categories.SchemaGen;

        public bool IsSchemaPathVisible => true;
        public bool IsExportPathVisible => false;
        public bool IsScriptsPathVisible => false;
        public bool IsSchemaStatusVisible => true;
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => false;
        public bool IsOutputOptionsVisible => false;

        public System.Threading.Tasks.Task ExecuteExportAsync(IExecutionViewModel vm)
        {
            // SchemaGen doesn't support export
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task ExecuteGenerateAsync(IExecutionViewModel vm)
        {
            if (vm.SelectedFeature == null) return System.Threading.Tasks.Task.CompletedTask;
            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();
            if (selectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet for schema generation.");
                vm.ShowLogs();
                return System.Threading.Tasks.Task.CompletedTask;
            }

            if (!Directory.Exists(vm.SelectedFeature.SchemaPath)) Directory.CreateDirectory(vm.SelectedFeature.SchemaPath);

            var targets = selectedSheets.Select(item => ((string)item.File.FullPath, (string)item.Sheet.SheetName));
            var editorVm = new SchemaEditorViewModel(targets, vm.SelectedFeature?.SchemaPath ?? "");
            editorVm.OnComplete += (success) =>
            {
                if (success)
                {
                    LogService.Instance.Info("Schema Generation Session Finished.");
                    vm.RefreshFiles();
                    vm.ShowLogs();
                }
                AppServices.Navigation.NavigateToExecution();
            };
            AppServices.Navigation.NavigateToSchemaEditor(editorVm);
            
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task CreateTemplateAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}
