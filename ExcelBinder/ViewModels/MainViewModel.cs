using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ExcelBinder.Services;
using ExcelBinder.Models;
using Newtonsoft.Json;
using System.Windows;
using Microsoft.Win32;

namespace ExcelBinder.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private AppSettings _settings = new();
        private FeatureDefinition? _selectedFeature;
        private ExecutionViewModelBase? _currentExecutionViewModel;

        public ExecutionViewModelBase? CurrentExecutionViewModel
        {
            get => _currentExecutionViewModel;
            set => SetProperty(ref _currentExecutionViewModel, value);
        }

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

        private readonly FeatureService _featureService = new();
        private readonly ExcelService _excelService = new();
        private readonly ExportService _exportService = new();
        private readonly CodeGeneratorService _codeGenService = new();

        public MainViewModel()
        {
            RefreshFeaturesCommand = new RelayCommand(RefreshFeatures);
            SelectFeatureCommand = new RelayCommand<FeatureDefinition>(ExecuteSelectFeature);
            NavigateToDashboardCommand = new RelayCommand(() => (Application.Current.MainWindow as MainWindow)?.NavigateToDashboard());
            NavigateToSettingsCommand = new RelayCommand(ExecuteNavigateToSettings);
            SaveSettingsCommand = new RelayCommand<Window>(ExecuteSaveSettings);
            BrowseFeatureDefinitionsPathCommand = new RelayCommand(ExecuteBrowseFeatureDefinitionsPath);
            BindFeatureCommand = new RelayCommand(ExecuteBindFeature);
            BindFeatureFolderCommand = new RelayCommand(ExecuteBindFeatureFolder);
            RemoveBoundFeatureCommand = new RelayCommand<string>(ExecuteRemoveBoundFeature);
            CreateFeatureCommand = new RelayCommand(ExecuteCreateFeature);
            EditFeatureCommand = new RelayCommand<FeatureDefinition>(ExecuteEditFeature);

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
                    _ => null
                };

                (Application.Current.MainWindow as MainWindow)?.NavigateToExecution();
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
            var settingsWin = new Views.SettingsWindow { DataContext = this };
            settingsWin.Owner = Application.Current.MainWindow;
            settingsWin.ApiKeyBox.Password = Settings.OpenAiApiKey;
            settingsWin.ClaudeApiKeyBox.Password = Settings.ClaudeApiKey;
            settingsWin.ShowDialog();
        }

        private void ExecuteSaveSettings(Window window)
        {
            if (window is Views.SettingsWindow settingsWin)
            {
                Settings.OpenAiApiKey = settingsWin.ApiKeyBox.Password;
                Settings.ClaudeApiKey = settingsWin.ClaudeApiKeyBox.Password;
            }
            _featureService.SaveSettings(Settings);
            RefreshFeatures();
            window?.Close();
        }

        private void ExecuteBrowseFeatureDefinitionsPath()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = ProjectConstants.UI.TitleSelectFolder
            };

            if (dialog.ShowDialog() == true)
            {
                Settings.FeatureDefinitionsPath = dialog.FolderName;
            }
        }

        private void ExecuteBindFeature()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = ProjectConstants.Extensions.JsonFilter,
                Title = ProjectConstants.UI.TitleSelectFile
            };

            if (dialog.ShowDialog() == true)
            {
                if (!Settings.BoundFeatures.Contains(dialog.FileName))
                {
                    Settings.BoundFeatures.Add(dialog.FileName);
                }
            }
        }

        private void ExecuteBindFeatureFolder()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = ProjectConstants.UI.TitleSelectFolder
            };

            if (dialog.ShowDialog() == true)
            {
                if (!Settings.BoundFeatures.Contains(dialog.FolderName))
                {
                    Settings.BoundFeatures.Add(dialog.FolderName);
                }
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
                (Application.Current.MainWindow as MainWindow)?.NavigateToDashboard();
            };
            (Application.Current.MainWindow as MainWindow)?.NavigateToFeatureBuilder(builderVm);
        }

        private void ExecuteEditFeature(FeatureDefinition feature)
        {
            string? path = _featureService.GetFeaturePath(feature.Id, Settings);
            var builderVm = new FeatureBuilderViewModel(feature, path);
            builderVm.OnComplete += () =>
            {
                RefreshFeatures();
                (Application.Current.MainWindow as MainWindow)?.NavigateToDashboard();
            };
            (Application.Current.MainWindow as MainWindow)?.NavigateToFeatureBuilder(builderVm);
        }

        public void ShowLogs()
        {
            if (Application.Current.MainWindow != null)
            {
                var logWin = new Views.LogWindow { Owner = Application.Current.MainWindow };
                logWin.ShowDialog();
            }
        }
    }
}
