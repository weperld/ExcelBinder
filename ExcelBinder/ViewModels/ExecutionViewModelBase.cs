using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public abstract class ExecutionViewModelBase : ViewModelBase
    {
        protected readonly ExcelService _excelService = new();
        protected readonly FeatureDefinition _feature;
        private readonly IFeatureProcessor _processor;
        private string _namespace;

        public FeatureDefinition SelectedFeature => _feature;

        public ObservableCollection<FileItemViewModel> ExcelFiles { get; } = new();

        public string Namespace
        {
            get => _namespace;
            set => SetProperty(ref _namespace, value);
        }

        public virtual bool IsBinaryChecked
        {
            get => false;
            set { }
        }

        public virtual bool IsJsonChecked
        {
            get => false;
            set { }
        }

        public ICommand RefreshFilesCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }

        public bool IsSchemaPathVisible => _processor.IsSchemaPathVisible;
        public bool IsExportPathVisible => _processor.IsExportPathVisible;
        public bool IsScriptsPathVisible => _processor.IsScriptsPathVisible;
        public bool IsSchemaStatusVisible => _processor.IsSchemaStatusVisible;

        protected ExecutionViewModelBase(FeatureDefinition feature)
        {
            _feature = feature;
            _namespace = feature.DefaultNamespace;
            _processor = FeatureProcessorFactory.GetProcessor(feature.Category);

            RefreshFilesCommand = new RelayCommand(RefreshFiles);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
            NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);

            RefreshFiles();
        }

        public virtual void RefreshFiles()
        {
            try
            {
                if (!Directory.Exists(_feature.ExcelPath)) return;

                ExcelFiles.Clear();
                var files = Directory.GetFiles(_feature.ExcelPath, "*.xlsx")
                                     .Where(f => !Path.GetFileName(f).StartsWith("~$"));
                foreach (var file in files)
                {
                    var fileItem = new FileItemViewModel { FileName = Path.GetFileName(file), FullPath = file };

                    try
                    {
                        var sheetNames = _excelService.GetSheetNames(file);
                        foreach (var sheetName in sheetNames)
                        {
                            // Global Rule: Skip sheets starting with #
                            if (sheetName.StartsWith(ProjectConstants.Excel.CommentPrefix)) continue;

                            string schemaFile = GetSchemaPath(file, sheetName);
                            bool found = File.Exists(schemaFile);
                            bool canSelect = IsSheetSelectable(found);

                            fileItem.Sheets.Add(new SheetItemViewModel
                            {
                                SheetName = sheetName,
                                IsSchemaFound = found,
                                SchemaPath = found ? schemaFile : ProjectConstants.Defaults.NotFound,
                                CanBeSelected = canSelect,
                                IsSelected = false
                            });
                        }
                    }
                    catch (Exception ex) { LogService.Instance.Warning($"Error reading sheets from {file}: {ex.Message}"); }

                    ExcelFiles.Add(fileItem);
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Error loading excel files: {ex.Message}");
            }
        }

        protected abstract bool IsSheetSelectable(bool isSchemaFound);

        private void ExecuteSelectAll()
        {
            foreach (var f in ExcelFiles)
            {
                f.IsSelected = true;
                foreach (var s in f.Sheets) s.IsSelected = true;
            }
        }

        private void ExecuteDeselectAll()
        {
            foreach (var f in ExcelFiles)
            {
                f.IsSelected = false;
                foreach (var s in f.Sheets) s.IsSelected = false;
            }
        }

        private void ExecuteNavigateToDashboard()
        {
            AppServices.Navigation.NavigateToDashboard();
        }

        public string GetSchemaPath(string excelFullPath, string sheetName)
            => SchemaLocator.Resolve(_feature.SchemaPath, excelFullPath, sheetName);

        /// <summary>현재 선택 상태를 Processor에 전달할 불변 DTO로 변환합니다.</summary>
        public ExecutionRequest BuildRequest()
        {
            var selectedSheets = ExcelFiles
                .SelectMany(f => f.Sheets.Where(s => s.IsSelected).Select(s => new SheetTarget(f.FullPath, s.SheetName)))
                .ToList();
            var selectedFiles = ExcelFiles.Where(f => f.IsSelected).Select(f => f.FullPath).ToList();

            return new ExecutionRequest
            {
                Feature = _feature,
                SelectedSheets = selectedSheets,
                SelectedFiles = selectedFiles,
                Namespace = Namespace,
                ExportBinary = IsBinaryChecked,
                ExportJson = IsJsonChecked
            };
        }

        /// <summary>
        /// 공통 실행 골격: IsBusy 가드 → 실행 → 완료 시 로그 창, 치명 실패 시 오류 대화상자.
        /// </summary>
        protected async System.Threading.Tasks.Task RunProcessorAsync(
            string operationName, Func<IFeatureProcessor, System.Threading.Tasks.Task> action)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await action(_processor);
                ShowLogs();
            }
            catch (Exception ex)
            {
                ReportFatal(operationName, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>치명적 실패를 에러 로그 + 오류 대화상자 + 로그 창으로 사용자에게 알립니다.</summary>
        protected void ReportFatal(string operation, Exception ex)
        {
            LogService.Instance.Error($"{operation} 실패: {ex.Message}");
            if (!App.IsCliMode)
            {
                AppServices.Dialog.ShowMessage(
                    $"{operation} 중 오류가 발생했습니다.\n\n{ex.Message}\n\n자세한 내용은 로그 창을 확인하세요.",
                    "오류", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            ShowLogs();
        }

        public void ShowLogs()
        {
            if (App.IsCliMode) return; // CLI: Dialog 서비스 미초기화 상태 — 콘솔 로그로 대체

            AppServices.Dialog.ShowLogWindow();
        }
    }
}
