using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    /// <summary>
    /// 템플릿 엑셀 생성 + 코드 생성만 수행하는 카테고리(Enum/Constants)의 공통 실행 VM.
    /// View 매핑(App.xaml의 타입별 DataTemplate)을 유지하기 위해 카테고리별 서브클래스는 남긴다.
    /// </summary>
    public abstract class TemplateCodeGenExecutionViewModel : ExecutionViewModelBase
    {
        private readonly string _templateDialogTitle;
        private readonly string _defaultTemplateFileName;

        public ICommand GenerateCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand ShowLogsCommand { get; }

        protected TemplateCodeGenExecutionViewModel(
            FeatureDefinition feature, string templateDialogTitle, string defaultTemplateFileName)
            : base(feature)
        {
            _templateDialogTitle = templateDialogTitle;
            _defaultTemplateFileName = defaultTemplateFileName;

            GenerateCommand = new RelayCommand(ExecuteGenerate);
            CreateTemplateCommand = new RelayCommand(ExecuteCreateTemplate);
            ShowLogsCommand = new RelayCommand(ShowLogs);
        }

        protected override bool IsSheetSelectable(bool isSchemaFound) => true;

        private async void ExecuteGenerate() =>
            await RunProcessorAsync("코드 생성", p => p.ExecuteGenerateAsync(BuildRequest()));

        private async void ExecuteCreateTemplate()
        {
            string? path = AppServices.Dialog.BrowseSaveFile(
                "Excel Files (*.xlsx)|*.xlsx", _defaultTemplateFileName, _templateDialogTitle, _feature.ExcelPath);
            if (path == null) return;

            await RunProcessorAsync("템플릿 생성", async p =>
            {
                await p.CreateTemplateAsync(path);
                LogService.Instance.Info($"Template created successfully: {path}");
            });
        }
    }
}
