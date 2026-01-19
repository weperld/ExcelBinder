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

        public void ExecuteExport(MainViewModel vm)
        {
            // SchemaGen doesn't support export
        }

        public void ExecuteGenerate(MainViewModel vm)
        {
            if (vm.SelectedFeature == null) return;
            var selectedSheets = vm.ExcelFiles.SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new { File = f, Sheet = s })).ToList();
            if (selectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet for schema generation.");
                vm.ShowLogs();
                return;
            }

            if (!Directory.Exists(vm.SelectedFeature.SchemaPath)) Directory.CreateDirectory(vm.SelectedFeature.SchemaPath);

            var targets = selectedSheets.Select(item => ((string)item.File.FullPath, (string)item.Sheet.SheetName));
            var editorVm = new SchemaEditorViewModel(targets, vm.SelectedFeature?.SchemaPath ?? "");
            editorVm.OnComplete += () =>
            {
                LogService.Instance.Info("Schema Generation Session Finished.");
                vm.RefreshFiles();
                (Application.Current.MainWindow as MainWindow)?.NavigateToExecution();
                vm.ShowLogs();
            };
            (Application.Current.MainWindow as MainWindow)?.NavigateToSchemaEditor(editorVm);
        }
    }
}
