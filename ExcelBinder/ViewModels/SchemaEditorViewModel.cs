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

        public event Action<bool>? OnComplete;

        public SchemaEditorViewModel(IEnumerable<(string excelPath, string sheetName)> targets, string outputPath)
        {
            foreach (var target in targets)
            {
                var item = new SchemaEditorItemViewModel(target.excelPath, target.sheetName, outputPath);
                Items.Add(item);
            }

            SelectedItem = Items.FirstOrDefault();

            CompleteCommand = new RelayCommand(ExecuteComplete);
            CancelCommand = new RelayCommand(() => OnComplete?.Invoke(false));
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
                OnComplete?.Invoke(true);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Error saving schemas: {ex.Message}");
                // We still want to invoke OnComplete to show logs if some were saved, 
                // but usually errors here are critical. 
                // The MainViewModel's OnComplete handler will call ShowLogs.
                OnComplete?.Invoke(true);
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
        public string ExcelPath => _excelPath;
        public string ExcelFileName => Path.GetFileName(_excelPath);
        public string ExcelDirectory => Path.GetDirectoryName(_excelPath) ?? string.Empty;

        public string SelectedKey
        {
            get => _selectedKey;
            set => SetProperty(ref _selectedKey, value);
        }

        public ObservableCollection<string> Headers { get; } = new();
        public ObservableCollection<SchemaFieldViewModel> Fields { get; } = new();
        public ObservableCollection<string> AvailableTypes { get; } = new(ProjectConstants.Types.AllPrimitives);

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
            var allData = excelService.ReadExcel(excelPath, sheetName).ToList();
            
            if (allData.Count >= 2)
            {
                var data = allData[1]; // 전역 규칙: 두 번째 행을 헤더로 사용
                var groupedHeaders = data
                    .Where(h => !string.IsNullOrWhiteSpace(h) && !h.TrimStart().StartsWith(ProjectConstants.Excel.CommentPrefix))
                    .GroupBy(h => h)
                    .Select(g => new { Name = g.Key, Count = g.Count() });

                foreach (var group in groupedHeaders)
                {
                    Headers.Add(group.Name);

                    string selectedType = ProjectConstants.Types.Int;
                    string referenceType = "";

                    if (existingSchema != null && existingSchema.Fields.TryGetValue(group.Name, out var typeStr))
                    {
                        var info = TypeParser.ParseType(typeStr, group.Name);
                        selectedType = info.BaseType;
                        referenceType = info.RefType ?? "";
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
                    _selectedKey = Headers.FirstOrDefault() ?? ProjectConstants.Excel.DefaultSheetKey;
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

        private string _selectedType = ProjectConstants.Types.Int;
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
