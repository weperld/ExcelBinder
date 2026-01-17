using System;
using System.Collections.Generic;
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
    public class SchemaEditorViewModel : ViewModelBase
    {
        private SchemaEditorItemViewModel? _selectedItem;

        public ObservableCollection<SchemaEditorItemViewModel> Items { get; } = new();

        public SchemaEditorItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand CompleteCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action OnComplete;

        public SchemaEditorViewModel(IEnumerable<(string excelPath, string sheetName)> targets, string outputPath)
        {
            foreach (var target in targets)
            {
                var item = new SchemaEditorItemViewModel(target.excelPath, target.sheetName, outputPath);
                Items.Add(item);
            }

            SelectedItem = Items.FirstOrDefault();

            CompleteCommand = new RelayCommand(ExecuteComplete);
            CancelCommand = new RelayCommand(() => OnComplete?.Invoke());
        }

        private void ExecuteComplete()
        {
            LogService.Instance.Clear();
            LogService.Instance.Info($"Starting to save {Items.Count} schemas...");

            try
            {
                foreach (var item in Items)
                {
                    item.Save();
                    LogService.Instance.Info($"Saved schema: {item.DisplayName}");
                }
                LogService.Instance.Info("All schemas saved successfully.");
                OnComplete?.Invoke();
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Error saving schemas: {ex.Message}");
                // We still want to invoke OnComplete to show logs if some were saved, 
                // but usually errors here are critical. 
                // The MainViewModel's OnComplete handler will call ShowLogs.
                OnComplete?.Invoke();
            }
        }
    }

    public class SchemaEditorItemViewModel : ViewModelBase
    {
        private string _fileName;
        private string _sheetName;
        private string _excelPath;
        private string _outputPath;
        private string _selectedKey;

        public string DisplayName => $"{_fileName} ({_sheetName})";
        public string SheetName => _sheetName;

        public string SelectedKey
        {
            get => _selectedKey;
            set => SetProperty(ref _selectedKey, value);
        }

        public ObservableCollection<string> Headers { get; } = new();
        public ObservableCollection<SchemaFieldViewModel> Fields { get; } = new();
        public ObservableCollection<string> AvailableTypes { get; } = new()
        {
            "byte", "short", "int", "uint", "long", "ulong", "float", "double", "string", "bool"
        };

        public SchemaEditorItemViewModel(string excelPath, string sheetName, string outputPath)
        {
            _excelPath = excelPath;
            _sheetName = sheetName;
            _fileName = Path.GetFileNameWithoutExtension(excelPath);
            _outputPath = outputPath;

            // Load existing schema if available
            SchemaDefinition? existingSchema = null;
            string finalFileName = $"{_fileName}_{_sheetName}_Schema.json";
            string savePath = Path.Combine(_outputPath, finalFileName);
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    existingSchema = JsonConvert.DeserializeObject<SchemaDefinition>(json);
                }
                catch
                {
                    // Ignore errors, fallback to default
                }
            }

            var excelService = new ExcelService();
            var data = excelService.ReadExcel(excelPath, sheetName).FirstOrDefault();
            
            if (data != null)
            {
                var groupedHeaders = data
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .GroupBy(h => h)
                    .Select(g => new { Name = g.Key, Count = g.Count() });

                foreach (var group in groupedHeaders)
                {
                    Headers.Add(group.Name);

                    string selectedType = "int";
                    string referenceType = "";

                    if (existingSchema != null && existingSchema.Fields.TryGetValue(group.Name, out var typeStr))
                    {
                        // Parse type string: List<type:ref:target> or type:ref:target
                        if (typeStr.StartsWith("List<") && typeStr.EndsWith(">"))
                        {
                            typeStr = typeStr.Substring(5, typeStr.Length - 6);
                        }

                        var parts = typeStr.Split(':');
                        selectedType = parts[0];
                        if (parts.Length >= 3 && parts[1] == "ref")
                        {
                            referenceType = parts[2];
                        }
                    }

                    Fields.Add(new SchemaFieldViewModel 
                    { 
                        HeaderName = group.Name, 
                        Count = group.Count,
                        SelectedType = selectedType,
                        ReferenceType = referenceType 
                    });
                }

                if (existingSchema != null && Headers.Contains(existingSchema.Key))
                {
                    _selectedKey = existingSchema.Key;
                }
                else
                {
                    _selectedKey = Headers.FirstOrDefault() ?? "Id";
                }
            }
        }

        public void Save()
        {
            var schema = new SchemaDefinition
            {
                ClassName = _sheetName + "Data",
                Key = SelectedKey,
                Fields = new Dictionary<string, string>()
            };

            foreach (var field in Fields)
            {
                string typeStr = field.SelectedType;
                if (!string.IsNullOrWhiteSpace(field.ReferenceType))
                {
                    typeStr = $"{typeStr}:ref:{field.ReferenceType}";
                }

                if (field.Count > 1)
                {
                    typeStr = $"List<{typeStr}>";
                }

                schema.Fields[field.HeaderName] = typeStr;
            }

            if (!Directory.Exists(_outputPath)) Directory.CreateDirectory(_outputPath);
            
            string finalFileName = $"{_fileName}_{_sheetName}_Schema.json";

            string savePath = Path.Combine(_outputPath, finalFileName);
            File.WriteAllText(savePath, JsonConvert.SerializeObject(schema, Formatting.Indented));
        }
    }

    public class SchemaFieldViewModel : ViewModelBase
    {
        public string HeaderName { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
        public string DisplayName => Count > 1 ? $"{HeaderName}({Count})" : HeaderName;

        private string _selectedType = "int";
        private string _referenceType = string.Empty;

        public string SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        public string ReferenceType
        {
            get => _referenceType;
            set => SetProperty(ref _referenceType, value);
        }
    }
}
