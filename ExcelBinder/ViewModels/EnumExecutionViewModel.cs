using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class EnumExecutionViewModel : ExecutionViewModelBase
    {
        public ICommand GenerateCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand ShowLogsCommand { get; }

        public EnumExecutionViewModel(FeatureDefinition feature) : base(feature)
        {
            GenerateCommand = new RelayCommand(ExecuteGenerate);
            CreateTemplateCommand = new RelayCommand(ExecuteCreateTemplate);
            ShowLogsCommand = new RelayCommand(ShowLogs);
        }

        protected override bool IsSheetSelectable(bool isSchemaFound) => true;

        private async void ExecuteGenerate()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var processor = FeatureProcessorFactory.GetProcessor(_feature.Category);
                await processor.ExecuteGenerateAsync(this);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void ExecuteCreateTemplate()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Save Enum Excel Template",
                FileName = "NewEnumTemplate.xlsx",
                InitialDirectory = _feature.ExcelPath
            };

            if (dialog.ShowDialog() == true)
            {
                if (IsBusy) return;
                try
                {
                    IsBusy = true;
                    var processor = FeatureProcessorFactory.GetProcessor(_feature.Category);
                    await processor.CreateTemplateAsync(dialog.FileName);
                    LogService.Instance.Info($"Template created successfully: {dialog.FileName}");
                }
                catch (System.Exception ex)
                {
                    LogService.Instance.Error($"Error creating template: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                    ShowLogs();
                }
            }
        }
    }
}
