using System;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class LogicExecutionViewModel : ExecutionViewModelBase
    {
        public ICommand GenerateCodeCommand { get; }

        public LogicExecutionViewModel(FeatureDefinition feature) : base(feature)
        {
            GenerateCodeCommand = new RelayCommand(ExecuteGenerateCode);
        }

        public override bool IsSchemaPathVisible => false;
        public override bool IsExportPathVisible => false;

        protected override bool IsSheetSelectable(bool isSchemaFound) => true;

        private async void ExecuteGenerateCode()
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
    }
}
