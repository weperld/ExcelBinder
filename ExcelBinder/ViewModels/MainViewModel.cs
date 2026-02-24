using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExcelBinder.Services;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private AppSettings _settings = new();
        private FeatureDefinition? _selectedFeature;
        private ExecutionViewModelBase? _currentExecutionViewModel;
        private VersionInfo? _latestVersionInfo;
        private bool _isUpdateBannerVisible;

        public ExecutionViewModelBase? CurrentExecutionViewModel
        {
            get => _currentExecutionViewModel;
            set => SetProperty(ref _currentExecutionViewModel, value);
        }

        /// <summary>
        /// 최신 버전 정보입니다. null이면 업데이트 없음 또는 확인 전 상태입니다.
        /// </summary>
        public VersionInfo? LatestVersionInfo
        {
            get => _latestVersionInfo;
            set
            {
                if (SetProperty(ref _latestVersionInfo, value))
                {
                    IsUpdateBannerVisible = value != null;
                    OnPropertyChanged(nameof(UpdateMessage));
                    OnPropertyChanged(nameof(IsUpdateIndicatorVisible));
                }
            }
        }

        /// <summary>
        /// 업데이트 알림 배너의 표시 여부입니다.
        /// </summary>
        public bool IsUpdateBannerVisible
        {
            get => _isUpdateBannerVisible;
            set
            {
                if (SetProperty(ref _isUpdateBannerVisible, value))
                {
                    OnPropertyChanged(nameof(IsUpdateIndicatorVisible));
                }
            }
        }

        /// <summary>
        /// 업데이트가 존재하지만 토스트가 닫혀 있을 때 인디케이터 버튼을 표시합니다.
        /// </summary>
        public bool IsUpdateIndicatorVisible => LatestVersionInfo != null && !IsUpdateBannerVisible;

        /// <summary>
        /// 업데이트 알림 배너에 표시할 메시지입니다.
        /// </summary>
        public string UpdateMessage => LatestVersionInfo != null
            ? string.Format(ProjectConstants.Update.MsgNewVersion, LatestVersionInfo.VersionString)
            : string.Empty;

        public AppSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public FeatureDefinition? SelectedFeature
        {
            get => _selectedFeature;
            set
            {
                if (SetProperty(ref _selectedFeature, value))
                {
                    if (value != null)
                    {
                        Settings.LastFeatureId = value.Id;
                        _featureService.SaveSettings(Settings);
                    }
                }
            }
        }

        public ObservableCollection<FeatureDefinition> Features { get; } = new();

        public ICommand RefreshFeaturesCommand { get; }
        public ICommand SelectFeatureCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand BrowseFeatureDefinitionsPathCommand { get; }
        public ICommand BindFeatureCommand { get; }
        public ICommand BindFeatureFolderCommand { get; }
        public ICommand RemoveBoundFeatureCommand { get; }
        public ICommand CreateFeatureCommand { get; }
        public ICommand EditFeatureCommand { get; }
        public ICommand OpenUpdatePageCommand { get; }
        public ICommand CheckForUpdateCommand { get; }
        public ICommand DismissUpdateBannerCommand { get; }
        public ICommand ShowUpdateBannerCommand { get; }

        private readonly FeatureService _featureService = new();
        private readonly UpdateCheckService _updateCheckService = new();

        public MainViewModel()
        {
            RefreshFeaturesCommand = new RelayCommand(RefreshFeatures);
            SelectFeatureCommand = new RelayCommand<FeatureDefinition>(ExecuteSelectFeature);
            NavigateToDashboardCommand = new RelayCommand(() => AppServices.Navigation.NavigateToDashboard());
            NavigateToSettingsCommand = new RelayCommand(ExecuteNavigateToSettings);
            SaveSettingsCommand = new RelayCommand<Window>(ExecuteSaveSettings);
            BrowseFeatureDefinitionsPathCommand = new RelayCommand(ExecuteBrowseFeatureDefinitionsPath);
            BindFeatureCommand = new RelayCommand(ExecuteBindFeature);
            BindFeatureFolderCommand = new RelayCommand(ExecuteBindFeatureFolder);
            RemoveBoundFeatureCommand = new RelayCommand<string>(ExecuteRemoveBoundFeature);
            CreateFeatureCommand = new RelayCommand(ExecuteCreateFeature);
            EditFeatureCommand = new RelayCommand<FeatureDefinition>(ExecuteEditFeature);
            OpenUpdatePageCommand = new RelayCommand(ExecuteOpenUpdatePage);
            CheckForUpdateCommand = new RelayCommand(ExecuteCheckForUpdate);
            DismissUpdateBannerCommand = new RelayCommand(() => IsUpdateBannerVisible = false);
            ShowUpdateBannerCommand = new RelayCommand(() => IsUpdateBannerVisible = true);

            _settings = _featureService.LoadSettings();

            // Subscribe to settings changes for live preview
            _settings.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(AppSettings.FeatureDefinitionsPath)) RefreshFeatures(); };
            _settings.BoundFeatures.CollectionChanged += (s, e) => RefreshFeatures();

            RefreshFeatures();

            if (!string.IsNullOrEmpty(Settings.LastFeatureId))
            {
                var lastFeature = Features.FirstOrDefault(f => f.Id == Settings.LastFeatureId);
                if (lastFeature != null)
                {
                    SelectedFeature = lastFeature;
                }
            }

            if (Settings.CheckForUpdatesOnStartup)
            {
                StartUpdateCheckOnStartup();
            }
        }

        private void RefreshFeatures()
        {
            Features.Clear();

            // Load from directory
            if (Directory.Exists(Settings.FeatureDefinitionsPath))
            {
                var featuresFromDir = _featureService.LoadFeatures(Settings.FeatureDefinitionsPath);
                foreach (var f in featuresFromDir) Features.Add(f);
            }

            // Load from bound paths
            foreach (var path in Settings.BoundFeatures)
            {
                if (Directory.Exists(path))
                {
                    var featuresFromDir = _featureService.LoadFeatures(path);
                    foreach (var f in featuresFromDir)
                    {
                        if (Features.All(x => x.Id != f.Id))
                            Features.Add(f);
                    }
                }
                else if (File.Exists(path))
                {
                    var f = _featureService.LoadFeatureFromFile(path);
                    if (f != null && Features.All(x => x.Id != f.Id))
                    {
                        Features.Add(f);
                    }
                }
            }
        }

        private void ExecuteSelectFeature(FeatureDefinition feature)
        {
            try
            {
                SelectedFeature = feature;

                // Create appropriate Execution ViewModel
                CurrentExecutionViewModel = feature.Category switch
                {
                    ProjectConstants.Categories.StaticData => new StaticDataExecutionViewModel(feature, Settings),
                    ProjectConstants.Categories.Logic => new LogicExecutionViewModel(feature),
                    ProjectConstants.Categories.SchemaGen => new SchemaGenExecutionViewModel(feature),
                    ProjectConstants.Categories.Enum => new EnumExecutionViewModel(feature),
                    ProjectConstants.Categories.Constants => new ConstantsExecutionViewModel(feature),
                    _ => null
                };

                AppServices.Navigation.NavigateToExecution();
            }
            catch (Exception ex)
            {
                LogService.Instance.Clear();
                LogService.Instance.Error($"Error selecting feature: {ex.Message}\n{ex.StackTrace}");
                ShowLogs();
            }
        }

        private void ExecuteNavigateToSettings()
        {
            AppServices.Dialog.ShowSettingsDialog(this, Settings.OpenAiApiKey, Settings.ClaudeApiKey);
        }

        private void ExecuteSaveSettings(Window window)
        {
            if (window is IPasswordProvider passwordProvider)
            {
                Settings.OpenAiApiKey = passwordProvider.OpenAiApiKey;
                Settings.ClaudeApiKey = passwordProvider.ClaudeApiKey;
            }
            _featureService.SaveSettings(Settings);
            RefreshFeatures();
            window?.Close();
        }

        private void ExecuteBrowseFeatureDefinitionsPath()
        {
            string? path = AppServices.Dialog.BrowseFolder(ProjectConstants.UI.TitleSelectFolder);
            if (path != null)
            {
                Settings.FeatureDefinitionsPath = path;
            }
        }

        private void ExecuteBindFeature()
        {
            string? path = AppServices.Dialog.BrowseOpenFile(ProjectConstants.Extensions.JsonFilter, ProjectConstants.UI.TitleSelectFile);
            if (path != null && !Settings.BoundFeatures.Contains(path))
            {
                Settings.BoundFeatures.Add(path);
            }
        }

        private void ExecuteBindFeatureFolder()
        {
            string? path = AppServices.Dialog.BrowseFolder(ProjectConstants.UI.TitleSelectFolder);
            if (path != null && !Settings.BoundFeatures.Contains(path))
            {
                Settings.BoundFeatures.Add(path);
            }
        }

        private void ExecuteRemoveBoundFeature(string path)
        {
            Settings.BoundFeatures.Remove(path);
        }

        private void ExecuteCreateFeature()
        {
            var builderVm = new FeatureBuilderViewModel();
            builderVm.OnComplete += () =>
            {
                RefreshFeatures();
                AppServices.Navigation.NavigateToDashboard();
            };
            AppServices.Navigation.NavigateToFeatureBuilder(builderVm);
        }

        private void ExecuteEditFeature(FeatureDefinition feature)
        {
            string? path = _featureService.GetFeaturePath(feature.Id, Settings);
            var builderVm = new FeatureBuilderViewModel(feature, path);
            builderVm.OnComplete += () =>
            {
                RefreshFeatures();
                AppServices.Navigation.NavigateToDashboard();
            };
            AppServices.Navigation.NavigateToFeatureBuilder(builderVm);
        }

        public void ShowLogs()
        {
            AppServices.Dialog.ShowLogWindow();
        }

        /// <summary>
        /// 앱 시작 시 fire-and-forget으로 업데이트를 확인합니다.
        /// async void는 생성자에서의 비동기 호출을 위한 의도적 사용입니다.
        /// </summary>
        private async void StartUpdateCheckOnStartup()
        {
            await CheckForUpdateAsync();
        }

        private async Task CheckForUpdateAsync()
        {
            try
            {
                var versionInfo = await _updateCheckService.CheckForUpdateAsync();
                LatestVersionInfo = versionInfo;
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"업데이트 확인 중 오류: {ex.Message}");
            }
        }

        private void ExecuteOpenUpdatePage()
        {
            var url = LatestVersionInfo?.HtmlUrl ?? ProjectConstants.Update.ReleasesPageUrl;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"브라우저 열기 실패: {ex.Message}");
            }
        }

        private async void ExecuteCheckForUpdate()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var versionInfo = await _updateCheckService.CheckForUpdateAsync();
                LatestVersionInfo = versionInfo;
                if (versionInfo == null)
                {
                    LogService.Instance.Info(ProjectConstants.Update.MsgUpToDate);
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"{ProjectConstants.Update.MsgCheckFailed}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
