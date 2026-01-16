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
            set => SetProperty(ref _feature, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public ObservableCollection<TypeMappingItem> TypeMappings { get; } = new();
        public ObservableCollection<string> AvailableTypes { get; } = new()
        {
            "byte", "short", "int", "uint", "long", "ulong", "float", "double", "string", "bool"
        };

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
        public ICommand OpenAIAssistantCommand { get; }

        public event Action OnComplete;

        public FeatureBuilderViewModel(FeatureDefinition? existing = null, string? path = null)
        {
            _feature = existing ?? new FeatureDefinition { Id = "new_feature", Name = "New Feature" };
            _filePath = path ?? "";

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
            BrowseDataTemplateCommand = new RelayCommand(() => BrowseFile("Liquid Files (*.liquid)|*.liquid", p => Feature.Templates.DataClass = p));
            OpenAIAssistantCommand = new RelayCommand(ExecuteOpenAIAssistant);
        }

        private void ExecuteOpenAIAssistant()
        {
            // We need settings to get API key.
            // FeatureService can provide it.
            var service = new FeatureService();
            var settings = service.LoadSettings();

            string apiKey = settings.AiModel.StartsWith("claude-") ? settings.ClaudeApiKey : settings.OpenAiApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                string provider = settings.AiModel.StartsWith("claude-") ? "Claude" : "OpenAI";
                MessageBox.Show($"Settings에서 {provider} API Key를 먼저 설정해 주세요.", "AI Assistant", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var aiVm = new AIAssistantViewModel(Feature, settings);
            var aiWin = new Views.AIAssistantWindow { DataContext = aiVm };
            aiWin.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            
            aiVm.OnTemplateApplied += template =>
            {
                // Save template to a file
                string templatePath = Feature.Templates.DataClass;
                if (string.IsNullOrEmpty(templatePath))
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Liquid Files (*.liquid)|*.liquid",
                        FileName = $"{Feature.Id}_Template.liquid",
                        Title = "AI가 생성한 템플릿을 저장할 위치를 선택하세요"
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        templatePath = dialog.FileName;
                        Feature.Templates.DataClass = templatePath;
                    }
                    else return;
                }

                try
                {
                    File.WriteAllText(templatePath, template);
                    MessageBox.Show("템플릿이 성공적으로 적용되었습니다.", "AI Assistant");
                    aiWin.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"템플릿 저장 중 오류가 발생했습니다: {ex.Message}");
                }
            };

            aiWin.ShowDialog();
        }

        private void BrowseFolder(Action<string> setter)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Folder"
            };
            if (dialog.ShowDialog() == true)
            {
                setter(dialog.FolderName);
            }
        }

        private void BrowseFile(string filter, Action<string> setter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Title = "Select File"
            };
            if (dialog.ShowDialog() == true)
            {
                setter(dialog.FileName);
            }
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
                MessageBox.Show("ID and Name are required.");
                return;
            }

            Feature.TypeMappings = TypeMappings
                .Where(m => !string.IsNullOrWhiteSpace(m.Key))
                .ToDictionary(m => m.Key, m => m.Value);

            string json = JsonConvert.SerializeObject(Feature, Formatting.Indented);
            
            if (string.IsNullOrEmpty(FilePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    FileName = $"{Feature.Id}.json"
                };
                if (dialog.ShowDialog() == true)
                {
                    FilePath = dialog.FileName;
                }
                else return;
            }

            try
            {
                File.WriteAllText(FilePath, json);
                MessageBox.Show("Feature saved successfully.");
                OnComplete?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving feature: {ex.Message}");
            }
        }
    }

    public class TypeMappingItem : ViewModelBase
    {
        private string _key;
        private string _value;

        public string Key { get => _key; set => SetProperty(ref _key, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
    }
}
