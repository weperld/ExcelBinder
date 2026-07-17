using System;
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

        private async void ExecuteGenerateCode()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var processor = FeatureProcessorFactory.GetProcessor(_feature.Category);
                await processor.ExecuteGenerateAsync(this);
                ShowLogs();
            }
            catch (Exception ex)
            {
                ReportFatal("스키마 생성", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
