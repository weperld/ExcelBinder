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


        protected override bool IsSheetSelectable(bool isSchemaFound) => true;

        private async void ExecuteGenerateCode() =>
            await RunProcessorAsync("코드 생성", p => p.ExecuteGenerateAsync(BuildRequest()));
    }
}
