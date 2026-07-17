using System;
using System.IO;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class StaticDataExecutionViewModel : ExecutionViewModelBase
    {
        private bool _isBinaryChecked;
        private bool _isJsonChecked;

        public override bool IsBinaryChecked
        {
            get => _isBinaryChecked;
            set => SetProperty(ref _isBinaryChecked, value);
        }

        public override bool IsJsonChecked
        {
            get => _isJsonChecked;
            set => SetProperty(ref _isJsonChecked, value);
        }

        public ICommand ExportCommand { get; }
        public ICommand GenerateCodeCommand { get; }

        public StaticDataExecutionViewModel(FeatureDefinition feature, AppSettings settings) : base(feature)
        {
            _isBinaryChecked = settings.IsBinaryChecked;
            _isJsonChecked = settings.IsJsonChecked;

            ExportCommand = new RelayCommand(ExecuteExport);
            GenerateCodeCommand = new RelayCommand(ExecuteGenerateCode);
        }

        protected override bool IsSheetSelectable(bool isSchemaFound) => isSchemaFound;

        private async void ExecuteExport() =>
            await RunProcessorAsync("Export", p => p.ExecuteExportAsync(BuildRequest()));

        private async void ExecuteGenerateCode() =>
            await RunProcessorAsync("코드 생성", p => p.ExecuteGenerateAsync(BuildRequest()));
    }
}
