using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class ConstantsExecutionViewModel : ExecutionViewModelBase
    {
        public ICommand GenerateCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand ShowLogsCommand { get; }

        public ConstantsExecutionViewModel(FeatureDefinition feature) : base(feature)
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
            catch (Exception ex)
            {
                LogService.Instance.Error($"Code generation error: {ex.Message}");
                ShowLogs();
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
                Title = "Save Constants Excel Template",
                FileName = "NewConstantsTemplate.xlsx",
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
