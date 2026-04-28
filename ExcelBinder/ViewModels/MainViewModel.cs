using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExcelBinder.Services;
using ExcelBinder.Models;
using ExcelBinder.Views;
using Newtonsoft.Json;

namespace ExcelBinder.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private AppSettings _settings = new();
        private FeatureDefinition? _selectedFeature;
        private FeatureGroup? _selectedGroup;
        private ExecutionViewModelBase? _currentExecutionViewModel;
        private VersionInfo? _latestVersionInfo;
        private bool _isUpdateBannerVisible;
        private string _releaseNotesText = string.Empty;
        private List<ReleaseNoteEntry> _releaseNoteEntries = new();

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

        /// <summary>
        /// 현재 버전 이후의 릴리즈 노트 텍스트입니다.
        /// </summary>
        public string ReleaseNotesText
        {
            get => _releaseNotesText;
            set => SetProperty(ref _releaseNotesText, value);
        }

        /// <summary>
        /// 릴리즈 노트를 버전 태그와 본문으로 분리한 엔트리 목록입니다.
        /// </summary>
        public List<ReleaseNoteEntry> ReleaseNoteEntries
        {
            get => _releaseNoteEntries;
            set => SetProperty(ref _releaseNoteEntries, value);
        }

        public AppSettings Settings
        {
            get => _settings;
            set
            {
                var old = _settings;
                if (SetProperty(ref _settings, value))
                {
                    if (old != null)
                    {
                        old.PropertyChanged -= OnSettingsPropertyChanged;
                        old.BoundFeatures.CollectionChanged -= OnBoundFeaturesCollectionChanged;
                    }
                    if (value != null)
                    {
                        value.PropertyChanged += OnSettingsPropertyChanged;
                        value.BoundFeatures.CollectionChanged += OnBoundFeaturesCollectionChanged;
                    }
                }
            }
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
        public ObservableCollection<FeatureGroup> Groups { get; } = new();
        public ObservableCollection<FeatureGroup> CustomGroups { get; } = new();

        public FeatureGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    OnPropertyChanged(nameof(FilteredFeatures));
                    OnPropertyChanged(nameof(IsCustomGroupSelected));
                    OnPropertyChanged(nameof(SelectedGroupName));
                    if (value != null && Settings.LastSelectedGroupId != value.Id)
                    {
                        Settings.LastSelectedGroupId = value.Id;
                        _featureService.SaveSettings(Settings);
                    }
                }
            }
        }

        public string SelectedGroupName => _selectedGroup?.Name ?? string.Empty;

        public IEnumerable<FeatureDefinition> FilteredFeatures
        {
            get
            {
                if (_selectedGroup == null || _selectedGroup.IsAllGroup)
                    return Features;
                var ids = new HashSet<string>(_selectedGroup.FeatureIds);
                return Features.Where(f => ids.Contains(f.Id));
            }
        }

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
        public ICommand AddGroupCommand { get; }
        public ICommand RenameGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand ToggleFeatureGroupMembershipCommand { get; }
        public ICommand ManageGroupMembersCommand { get; }
        public ICommand OpenGuideCommand { get; }

        public bool IsCustomGroupSelected => _selectedGroup != null && !_selectedGroup.IsAllGroup;

        private readonly FeatureService _featureService = new();
        private readonly UpdateCheckService _updateCheckService = new();
        private readonly FeatureGroupService _groupService;

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
            AddGroupCommand = new RelayCommand(ExecuteAddGroup);
            RenameGroupCommand = new RelayCommand<FeatureGroup>(ExecuteRenameGroup);
            DeleteGroupCommand = new RelayCommand<FeatureGroup>(ExecuteDeleteGroup);
            ToggleFeatureGroupMembershipCommand = new RelayCommand<FeatureGroupToggleArgs>(ExecuteToggleFeatureGroupMembership);
            ManageGroupMembersCommand = new RelayCommand(ExecuteManageGroupMembers);
            OpenGuideCommand = new RelayCommand(() => OpenGuide());

            Settings = _featureService.LoadSettings();
            _groupService = new FeatureGroupService(Settings.FeatureDefinitionsPath);

            RefreshFeatures();
            RefreshGroups();

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

        private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.FeatureDefinitionsPath))
            {
                _groupService?.UpdateFeaturesDirectory(Settings.FeatureDefinitionsPath);
                RefreshFeatures();
                RefreshGroups();
            }
        }

        private void OnBoundFeaturesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshFeatures();
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

            if (_groupService != null)
            {
                var customGroups = Groups.Where(g => !g.IsAllGroup).ToList();
                if (customGroups.Count > 0)
                {
                    _groupService.CleanupDanglingIds(Features.Select(f => f.Id), customGroups);
                }
                OnPropertyChanged(nameof(FilteredFeatures));
            }
        }

        private void RefreshGroups()
        {
            Groups.Clear();
            CustomGroups.Clear();

            var allGroup = new FeatureGroup
            {
                Id = ProjectConstants.Defaults.AllGroupId,
                Name = ProjectConstants.Defaults.AllGroupName
            };
            Groups.Add(allGroup);

            var loaded = _groupService.LoadGroups();
            _groupService.CleanupDanglingIds(Features.Select(f => f.Id), loaded);
            foreach (var g in loaded)
            {
                Groups.Add(g);
                CustomGroups.Add(g);
            }

            var lastId = Settings.LastSelectedGroupId;
            SelectedGroup = Groups.FirstOrDefault(g => g.Id == lastId) ?? allGroup;
        }

        private void ExecuteAddGroup()
        {
            string? input = AppServices.Dialog.ShowInput(
                ProjectConstants.UI.TitleAddGroup,
                ProjectConstants.UI.PromptGroupName,
                defaultValue: null,
                validator: name => ValidateGroupName(name, currentId: null));

            if (input == null) return;

            var existing = Groups.ToList();
            var result = _groupService.AddGroup(input, existing);
            if (!result.Success)
            {
                AppServices.Dialog.ShowMessage(result.ErrorMessage ?? "그룹 추가 실패",
                    ProjectConstants.UI.TitleAddGroup, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (result.Group != null)
            {
                Groups.Add(result.Group);
                CustomGroups.Add(result.Group);
                SelectedGroup = result.Group;
            }
        }

        private void ExecuteRenameGroup(FeatureGroup? group)
        {
            if (group == null || group.IsAllGroup) return;

            string? input = AppServices.Dialog.ShowInput(
                ProjectConstants.UI.TitleRenameGroup,
                ProjectConstants.UI.PromptGroupName,
                defaultValue: group.Name,
                validator: name => ValidateGroupName(name, currentId: group.Id));

            if (input == null) return;

            var existing = Groups.ToList();
            var result = _groupService.RenameGroup(group.Id, input, existing);
            if (!result.Success)
            {
                AppServices.Dialog.ShowMessage(result.ErrorMessage ?? "그룹 이름변경 실패",
                    ProjectConstants.UI.TitleRenameGroup, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            group.Name = input.Trim();
            int idx = Groups.IndexOf(group);
            if (idx >= 0)
            {
                Groups.RemoveAt(idx);
                Groups.Insert(idx, group);
            }
            int customIdx = CustomGroups.IndexOf(group);
            if (customIdx >= 0)
            {
                CustomGroups.RemoveAt(customIdx);
                CustomGroups.Insert(customIdx, group);
            }
            if (SelectedGroup == group) OnPropertyChanged(nameof(SelectedGroup));
        }

        private void ExecuteDeleteGroup(FeatureGroup? group)
        {
            if (group == null || group.IsAllGroup) return;

            var confirm = AppServices.Dialog.ShowMessage(
                string.Format(ProjectConstants.UI.MsgConfirmDeleteGroup, group.Name),
                ProjectConstants.UI.TitleWarning,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var existing = Groups.ToList();
            if (!_groupService.DeleteGroup(group.Id, existing)) return;

            bool wasSelected = SelectedGroup == group;
            Groups.Remove(group);
            CustomGroups.Remove(group);
            if (wasSelected) SelectedGroup = Groups.FirstOrDefault();
        }

        private void ExecuteToggleFeatureGroupMembership(FeatureGroupToggleArgs? args)
        {
            if (args?.Group == null || args.Feature == null) return;
            if (args.Group.IsAllGroup) return;

            var existing = Groups.ToList();
            if (!_groupService.ToggleFeatureInGroup(args.Group.Id, args.Feature.Id, existing)) return;

            if (SelectedGroup == args.Group)
            {
                OnPropertyChanged(nameof(FilteredFeatures));
            }
        }

        private void ExecuteManageGroupMembers()
        {
            if (_selectedGroup == null || _selectedGroup.IsAllGroup) return;

            var selectedIds = AppServices.Dialog.ShowFeatureSelectionDialog(
                ProjectConstants.UI.TitleManageGroupMembers,
                string.Format(ProjectConstants.UI.PromptManageGroupMembers, _selectedGroup.Name),
                Features,
                _selectedGroup.FeatureIds);

            if (selectedIds == null) return;

            _selectedGroup.FeatureIds.Clear();
            _selectedGroup.FeatureIds.AddRange(selectedIds);
            _groupService.SaveGroups(Groups);
            OnPropertyChanged(nameof(FilteredFeatures));
        }

        private string? ValidateGroupName(string name, string? currentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ProjectConstants.UI.MsgGroupNameRequired;

            string trimmed = name.Trim();
            if (string.Equals(trimmed, ProjectConstants.Defaults.AllGroupName, StringComparison.OrdinalIgnoreCase))
                return ProjectConstants.UI.MsgGroupNameDuplicate;

            bool duplicate = Groups.Any(g =>
                !g.IsAllGroup &&
                g.Id != currentId &&
                string.Equals(g.Name, trimmed, StringComparison.OrdinalIgnoreCase));

            return duplicate ? ProjectConstants.UI.MsgGroupNameDuplicate : null;
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
            AppServices.Dialog.ShowSettingsDialog(this);
        }

        private void ExecuteSaveSettings(Window window)
        {
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
            if (path == null) return;
            path = Path.GetFullPath(path);
            if (!Settings.BoundFeatures.Contains(path))
            {
                Settings.BoundFeatures.Add(path);
            }
        }

        private void ExecuteBindFeatureFolder()
        {
            string? path = AppServices.Dialog.BrowseFolder(ProjectConstants.UI.TitleSelectFolder);
            if (path == null) return;
            path = Path.GetFullPath(path);
            if (!Settings.BoundFeatures.Contains(path))
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
        /// 가이드 윈도우를 모달로 표시합니다. 메인 ?/F1 진입 및 첫 실행 자동 표시 시 사용됩니다.
        /// </summary>
        public void OpenGuide(string? topicId = null, bool isFirstRunMode = false)
        {
            var vm = new GuideViewModel(topicId, isFirstRunMode);
            var win = new GuideWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.RequestClose += () =>
            {
                if (vm.IsFirstRunMode && vm.DoNotShowAgain)
                {
                    Settings.HasSeenGuide = true;
                    _featureService.SaveSettings(Settings);
                }
                win.Close();
            };
            win.ShowDialog();
        }

        /// <summary>
        /// 첫 실행이라면(HasSeenGuide=false) "시작하기" 토픽으로 가이드 자동 표시.
        /// MainWindow.Loaded에서 호출됩니다.
        /// </summary>
        public void TriggerFirstRunGuideIfNeeded()
        {
            if (!Settings.HasSeenGuide)
            {
                OpenGuide(ProjectConstants.UI.GuideTopicGettingStarted, isFirstRunMode: true);
            }
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

                if (versionInfo != null)
                {
                    var releases = await _updateCheckService.GetReleaseNotesSinceCurrentAsync();
                    ReleaseNotesText = UpdateCheckService.FormatReleaseNotes(releases);
                    ReleaseNoteEntries = UpdateCheckService.ToReleaseNoteEntries(releases);
                }
                else
                {
                    ReleaseNotesText = string.Empty;
                    ReleaseNoteEntries = new();
                }
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

        public class FeatureGroupToggleArgs
        {
            public FeatureGroup? Group { get; set; }
            public FeatureDefinition? Feature { get; set; }
        }

        private async void ExecuteCheckForUpdate()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var versionInfo = await _updateCheckService.CheckForUpdateAsync();
                LatestVersionInfo = versionInfo;
                if (versionInfo != null)
                {
                    var releases = await _updateCheckService.GetReleaseNotesSinceCurrentAsync();
                    ReleaseNotesText = UpdateCheckService.FormatReleaseNotes(releases);
                    ReleaseNoteEntries = UpdateCheckService.ToReleaseNoteEntries(releases);

                    var navigate = AppServices.Dialog.ShowUpdateInfoDialog(
                        string.Format(ProjectConstants.Update.MsgNewVersion, versionInfo.VersionString),
                        ReleaseNoteEntries,
                        ProjectConstants.Update.TitleUpdateCheck);
                    if (navigate)
                    {
                        ExecuteOpenUpdatePage();
                    }
                }
                else
                {
                    AppServices.Dialog.ShowMessage(
                        ProjectConstants.Update.MsgUpToDate,
                        ProjectConstants.Update.TitleUpdateCheck, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                AppServices.Dialog.ShowMessage(
                    $"{ProjectConstants.Update.MsgCheckFailed}: {ex.Message}",
                    ProjectConstants.Update.TitleUpdateCheck, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
