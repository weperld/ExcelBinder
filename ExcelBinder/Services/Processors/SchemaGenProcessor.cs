using System;
using System.IO;
using System.Linq;
using System.Windows;
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
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => false;
        public bool IsOutputOptionsVisible => false;

        public System.Threading.Tasks.Task ExecuteExportAsync(MainViewModel vm)
        {
            // SchemaGen doesn't support export
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task ExecuteGenerateAsync(MainViewModel vm)
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
                (Application.Current.MainWindow as MainWindow)?.NavigateToExecution();
            };
            (Application.Current.MainWindow as MainWindow)?.NavigateToSchemaEditor(editorVm);
            
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
