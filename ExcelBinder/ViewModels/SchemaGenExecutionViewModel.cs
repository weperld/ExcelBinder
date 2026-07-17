using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class SchemaGenExecutionViewModel : ExecutionViewModelBase
    {
        public ICommand GenerateCodeCommand { get; }

        public SchemaGenExecutionViewModel(FeatureDefinition feature) : base(feature)
        {
            GenerateCodeCommand = new RelayCommand(ExecuteGenerateCode);
        }


        protected override bool IsSheetSelectable(bool isSchemaFound) => true;

        private async void ExecuteGenerateCode() =>
            await RunProcessorAsync("스키마 생성", _ => NavigateToSchemaEditorAsync());

        // SchemaGen은 SchemaEditor 화면 네비게이션이 필요한 GUI 전용 흐름이라 Processor가 아닌 VM에서 직접 처리한다.
        private System.Threading.Tasks.Task NavigateToSchemaEditorAsync()
        {
            var request = BuildRequest();
            if (request.SelectedSheets.Count == 0)
            {
                LogService.Instance.Warning("Please select at least one sheet for schema generation.");
                return System.Threading.Tasks.Task.CompletedTask;
            }

            if (!Directory.Exists(_feature.SchemaPath)) Directory.CreateDirectory(_feature.SchemaPath);

            var targets = request.SelectedSheets.Select(s => (s.FilePath, s.SheetName));
            var editorVm = new SchemaEditorViewModel(targets, _feature.SchemaPath);
            editorVm.OnComplete += (success) =>
            {
                if (success)
                {
                    LogService.Instance.Info("Schema Generation Session Finished.");
                    RefreshFiles();
                }
                AppServices.Navigation.NavigateToExecution();
            };
            AppServices.Navigation.NavigateToSchemaEditor(editorVm);

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
