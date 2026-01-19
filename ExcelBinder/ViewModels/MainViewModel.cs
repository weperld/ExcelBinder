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
        private bool _isBinaryChecked = true;
        private bool _isJsonChecked = false;
        private string _namespace = ProjectConstants.Defaults.Namespace;

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
                        Namespace = value.DefaultNamespace;
                        Settings.LastFeatureId = value.Id;
                        _featureService.SaveSettings(Settings);
                        RefreshFiles();
                    }
                    UpdateVisibilities();
                }
            }
        }

        private bool _isSchemaPathVisible;
        public bool IsSchemaPathVisible { get => _isSchemaPathVisible; set => SetProperty(ref _isSchemaPathVisible, value); }

        private bool _isExportPathVisible;
        public bool IsExportPathVisible { get => _isExportPathVisible; set => SetProperty(ref _isExportPathVisible, value); }

        private bool _isScriptsPathVisible;
        public bool IsScriptsPathVisible { get => _isScriptsPathVisible; set => SetProperty(ref _isScriptsPathVisible, value); }

        private void UpdateVisibilities()
        {
            if (SelectedFeature == null)
            {
                IsSchemaPathVisible = false;
                IsExportPathVisible = false;
                IsScriptsPathVisible = false;
                return;
            }

            var processor = FeatureProcessorFactory.GetProcessor(SelectedFeature.Category);
            IsSchemaPathVisible = processor.IsSchemaPathVisible;
            IsExportPathVisible = processor.IsExportPathVisible;
            IsScriptsPathVisible = processor.IsScriptsPathVisible;
        }

        public string Namespace
        {
            get => _namespace;
            set => SetProperty(ref _namespace, value);
        }

        public bool IsBinaryChecked
        {
            get => _isBinaryChecked;
            set 
            {
                if (SetProperty(ref _isBinaryChecked, value))
                {
                    Settings.IsBinaryChecked = value;
                    _featureService.SaveSettings(Settings);
                }
            }
        }

        public bool IsJsonChecked
        {
            get => _isJsonChecked;
            set 
            {
                if (SetProperty(ref _isJsonChecked, value))
                {
                    Settings.IsJsonChecked = value;
                    _featureService.SaveSettings(Settings);
                }
            }
        }

        public ObservableCollection<FeatureDefinition> Features { get; } = new();
        public ObservableCollection<FileItemViewModel> ExcelFiles { get; } = new();

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

        public ICommand RefreshFilesCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand GenerateCodeCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

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

            RefreshFilesCommand = new RelayCommand(RefreshFiles);
            ExportCommand = new RelayCommand(ExecuteExport);
            GenerateCodeCommand = new RelayCommand(ExecuteGenerateCode);
            SelectAllCommand = new RelayCommand(() => 
            { 
                foreach (var f in ExcelFiles) 
                {
                    f.IsSelected = true;
                    foreach (var s in f.Sheets) s.IsSelected = true;
                }
            });
            DeselectAllCommand = new RelayCommand(() => 
            { 
                foreach (var f in ExcelFiles) 
                {
                    f.IsSelected = false;
                    foreach (var s in f.Sheets) s.IsSelected = false;
                }
            });

            _settings = _featureService.LoadSettings();
            _isBinaryChecked = _settings.IsBinaryChecked;
            _isJsonChecked = _settings.IsJsonChecked;
            
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

        public void RefreshFiles()
        {
            try
            {
                if (SelectedFeature == null || !Directory.Exists(SelectedFeature.ExcelPath)) return;

                ExcelFiles.Clear();
                var files = Directory.GetFiles(SelectedFeature.ExcelPath, "*.xlsx")
                                     .Where(f => !Path.GetFileName(f).StartsWith("~$"));
                foreach (var file in files)
                {
                    var fileItem = new FileItemViewModel { FileName = Path.GetFileName(file), FullPath = file };
                    
                    // Load sheets and check schemas
                    try
                    {
                        var sheetNames = _excelService.GetSheetNames(file);
                        foreach (var sheetName in sheetNames)
                        {
                            string schemaFile = GetSchemaPath(file, sheetName);
                            bool found = File.Exists(schemaFile);
                            bool canSelect = found || SelectedFeature.Category != ProjectConstants.Categories.StaticData;

                            fileItem.Sheets.Add(new SheetItemViewModel 
                            { 
                                SheetName = sheetName, 
                                IsSchemaFound = found,
                                SchemaPath = found ? schemaFile : ProjectConstants.Defaults.NotFound,
                                CanBeSelected = canSelect,
                                IsSelected = false // Default deselected as requested
                            });
                        }
                    }
                    catch { /* Skip error files */ }

                    ExcelFiles.Add(fileItem);
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Clear();
                LogService.Instance.Error($"Error loading excel files from {SelectedFeature?.ExcelPath}: {ex.Message}");
                ShowLogs();
            }
        }

        internal void ShowLogs()
        {
            if (Application.Current.MainWindow != null)
            {
                var logWin = new Views.LogWindow { Owner = Application.Current.MainWindow };
                logWin.ShowDialog();
            }
        }

        public void ExecuteExport()
        {
            if (SelectedFeature == null) return;
            var processor = FeatureProcessorFactory.GetProcessor(SelectedFeature.Category);
            processor.ExecuteExport(this);
        }

        public void ExecuteGenerateCode()
        {
            if (SelectedFeature == null) return;
            var processor = FeatureProcessorFactory.GetProcessor(SelectedFeature.Category);
            processor.ExecuteGenerate(this);
        }

        internal string GetSchemaPath(string excelFullPath, string sheetName)
        {
            if (SelectedFeature == null) return string.Empty;
            string excelName = Path.GetFileNameWithoutExtension(excelFullPath);
            
            // Try [ExcelName]_[SheetName]_Schema.json
            string path1 = Path.Combine(SelectedFeature.SchemaPath, $"{excelName}_{sheetName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path1)) return path1;
            
            // Try [ExcelName]_Schema.json
            string path2 = Path.Combine(SelectedFeature.SchemaPath, $"{excelName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path2)) return path2;

            // Fallback to sheetName.json
            string path3 = Path.Combine(SelectedFeature.SchemaPath, sheetName + ProjectConstants.Extensions.Json);
            if (File.Exists(path3)) return path3;

            return path1; // Default to the most specific one
        }


        internal void ProcessSchema(string schemaFile, List<SchemaDefinition> schemas)
        {
            if (SelectedFeature == null) return;
            try
            {
                var schema = JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaFile));
                if (schema == null)
                {
                    LogService.Instance.Error($"Failed to deserialize schema: {schemaFile}");
                    return;
                }
                schemas.Add(schema);

                string? code = _codeGenService.GenerateDataCode(schema, SelectedFeature, Namespace);
                if (!string.IsNullOrEmpty(code))
                {
                    File.WriteAllText(Path.Combine(SelectedFeature.ScriptsPath, schema.ClassName + ProjectConstants.Extensions.CSharp), code);
                    LogService.Instance.Info($"Generated Code: {schema.ClassName}");
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Error processing schema {Path.GetFileName(schemaFile)}: {ex.Message}");
            }
        }
    }

    public class SheetItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _canBeSelected = true;

        public string SheetName { get; set; } = string.Empty;
        public bool IsSchemaFound { get; set; }
        public string SchemaPath { get; set; } = string.Empty;

        public bool CanBeSelected
        {
            get => _canBeSelected;
            set => SetProperty(ref _canBeSelected, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set 
            {
                if (value && !CanBeSelected) return;
                SetProperty(ref _isSelected, value);
            }
        }
    }

    public class FileItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public ObservableCollection<SheetItemViewModel> Sheets { get; } = new();

        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public FileItemViewModel()
        {
            SelectAllCommand = new RelayCommand(() => 
            { 
                foreach (var sheet in Sheets) sheet.IsSelected = true;
                _isSelected = true;
                OnPropertyChanged(nameof(IsSelected));
            });
            DeselectAllCommand = new RelayCommand(() => 
            { 
                foreach (var sheet in Sheets) sheet.IsSelected = false;
                _isSelected = false;
                OnPropertyChanged(nameof(IsSelected));
            });
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    foreach (var sheet in Sheets)
                    {
                        sheet.IsSelected = value;
                    }
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute((T)parameter!);
        public event EventHandler? CanExecuteChanged;
    }
}
