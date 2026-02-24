using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;
using Newtonsoft.Json;

namespace ExcelBinder.ViewModels
{
    public class FeatureBuilderViewModel : ViewModelBase
    {
        private FeatureDefinition _feature;
        private string _filePath;

        public FeatureDefinition Feature
        {
            get => _feature;
            set
            {
                if (SetProperty(ref _feature, value))
                {
                    OnPropertyChanged(nameof(Category));
                    UpdateVisibilities();
                }
            }
        }

        public string Category
        {
            get => _feature.Category;
            set
            {
                if (_feature.Category != value)
                {
                    _feature.Category = value;
                    OnPropertyChanged();
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

        private bool _isTypeMappingsVisible;
        public bool IsTypeMappingsVisible { get => _isTypeMappingsVisible; set => SetProperty(ref _isTypeMappingsVisible, value); }

        private bool _isTemplatesVisible;
        public bool IsTemplatesVisible { get => _isTemplatesVisible; set => SetProperty(ref _isTemplatesVisible, value); }

        private bool _isOutputOptionsVisible;
        public bool IsOutputOptionsVisible { get => _isOutputOptionsVisible; set => SetProperty(ref _isOutputOptionsVisible, value); }

        private void UpdateVisibilities()
        {
            var processor = FeatureProcessorFactory.GetProcessor(Category);
            IsSchemaPathVisible = processor.IsSchemaPathVisible;
            IsExportPathVisible = processor.IsExportPathVisible;
            IsScriptsPathVisible = processor.IsScriptsPathVisible;
            IsTypeMappingsVisible = processor.IsTypeMappingsVisible;
            IsTemplatesVisible = processor.IsTemplatesVisible;
            IsOutputOptionsVisible = processor.IsOutputOptionsVisible;
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public ObservableCollection<TypeMappingItem> TypeMappings { get; } = new();
        public ObservableCollection<string> AvailableTypes { get; } = new(ProjectConstants.Types.AllPrimitives);

        public ICommand SaveCommand { get; }
        public ICommand AddMappingCommand { get; }
        public ICommand AddDefaultMappingsCommand { get; }
        public ICommand RemoveMappingCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseExcelPathCommand { get; }
        public ICommand BrowseSchemaPathCommand { get; }
        public ICommand BrowseExportPathCommand { get; }
        public ICommand BrowseScriptsPathCommand { get; }
        public ICommand BrowseDataTemplateCommand { get; }
        public event Action? OnComplete;

        public FeatureBuilderViewModel(FeatureDefinition? existing = null, string? path = null)
        {
            _feature = existing ?? new FeatureDefinition { Id = ProjectConstants.Defaults.FeatureId, Name = ProjectConstants.Defaults.FeatureName };
            _filePath = path ?? "";

            UpdateVisibilities();

            foreach (var kvp in _feature.TypeMappings)
            {
                TypeMappings.Add(new TypeMappingItem { Key = kvp.Key, Value = kvp.Value });
            }

            SaveCommand = new RelayCommand(ExecuteSave);
            AddMappingCommand = new RelayCommand(() => TypeMappings.Add(new TypeMappingItem()));
            AddDefaultMappingsCommand = new RelayCommand(ExecuteAddDefaultMappings);
            RemoveMappingCommand = new RelayCommand<TypeMappingItem>(item => TypeMappings.Remove(item));
            CancelCommand = new RelayCommand(() => OnComplete?.Invoke());

            BrowseExcelPathCommand = new RelayCommand(() => BrowseFolder(p => Feature.ExcelPath = p));
            BrowseSchemaPathCommand = new RelayCommand(() => BrowseFolder(p => Feature.SchemaPath = p));
            BrowseExportPathCommand = new RelayCommand(() => BrowseFolder(p => Feature.ExportPath = p));
            BrowseScriptsPathCommand = new RelayCommand(() => BrowseFolder(p => Feature.ScriptsPath = p));
            BrowseDataTemplateCommand = new RelayCommand(() => BrowseFile(ProjectConstants.Extensions.LiquidFilter, p => Feature.Templates.DataClass = p));
        }

        private void BrowseFolder(Action<string> setter)
        {
            string? path = AppServices.Dialog.BrowseFolder(ProjectConstants.UI.TitleSelectFolder);
            if (path != null) setter(path);
        }

        private void BrowseFile(string filter, Action<string> setter)
        {
            string? path = AppServices.Dialog.BrowseOpenFile(filter, ProjectConstants.UI.TitleSelectFile);
            if (path != null) setter(path);
        }

        private void ExecuteAddDefaultMappings()
        {
            foreach (var type in AvailableTypes)
            {
                if (TypeMappings.All(m => m.Key != type))
                {
                    TypeMappings.Add(new TypeMappingItem { Key = type, Value = type });
                }
            }
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(Feature.Id) || string.IsNullOrWhiteSpace(Feature.Name))
            {
                AppServices.Dialog.ShowMessage(ProjectConstants.UI.MsgRequiredIdName, ProjectConstants.UI.TitleWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Feature.TypeMappings = TypeMappings
                .Where(m => !string.IsNullOrWhiteSpace(m.Key))
                .ToDictionary(m => m.Key, m => m.Value);

            string json = JsonConvert.SerializeObject(Feature, Formatting.Indented);

            if (string.IsNullOrEmpty(FilePath))
            {
                string? path = AppServices.Dialog.BrowseSaveFile(
                    ProjectConstants.Extensions.JsonFilter,
                    $"{Feature.Id}{ProjectConstants.Extensions.Json}");
                if (path == null) return;
                FilePath = path;
            }

            try
            {
                File.WriteAllText(FilePath, json);
                AppServices.Dialog.ShowMessage(ProjectConstants.UI.MsgFeatureSaved, ProjectConstants.UI.TitleInfo);
                OnComplete?.Invoke();
            }
            catch (Exception ex)
            {
                AppServices.Dialog.ShowMessage($"{ProjectConstants.UI.MsgSaveError}{ex.Message}", ProjectConstants.UI.TitleError);
            }
        }
    }

    public class TypeMappingItem : ViewModelBase
    {
        private string _key = string.Empty;
        private string _value = string.Empty;

        public string Key { get => _key; set => SetProperty(ref _key, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
    }
}
