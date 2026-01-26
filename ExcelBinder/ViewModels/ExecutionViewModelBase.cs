using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public abstract class ExecutionViewModelBase : ViewModelBase, IExecutionViewModel
    {
        protected readonly ExcelService _excelService = new();
        protected readonly CodeGeneratorService _codeGenService = new();
        protected readonly FeatureDefinition _feature;
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

        public bool IsSchemaPathVisible => FeatureProcessorFactory.GetProcessor(_feature.Category).IsSchemaPathVisible;
        public bool IsExportPathVisible => FeatureProcessorFactory.GetProcessor(_feature.Category).IsExportPathVisible;
        public bool IsScriptsPathVisible => FeatureProcessorFactory.GetProcessor(_feature.Category).IsScriptsPathVisible;
        public bool IsSchemaStatusVisible => FeatureProcessorFactory.GetProcessor(_feature.Category).IsSchemaStatusVisible;

        protected ExecutionViewModelBase(FeatureDefinition feature)
        {
            _feature = feature;
            _namespace = feature.DefaultNamespace;

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
                    catch { /* Skip error files */ }

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
            if (System.Windows.Application.Current.MainWindow is ExcelBinder.MainWindow mainWindow)
            {
                mainWindow.NavigateToDashboard();
            }
        }

        public string GetSchemaPath(string excelFullPath, string sheetName)
        {
            string excelName = Path.GetFileNameWithoutExtension(excelFullPath);
            
            // Try [ExcelName]_[SheetName]_Schema.json
            string path1 = Path.Combine(_feature.SchemaPath, $"{excelName}_{sheetName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path1)) return path1;
            
            // Try [ExcelName]_Schema.json
            string path2 = Path.Combine(_feature.SchemaPath, $"{excelName}_Schema{ProjectConstants.Extensions.Json}");
            if (File.Exists(path2)) return path2;

            // Fallback to sheetName.json
            string path3 = Path.Combine(_feature.SchemaPath, sheetName + ProjectConstants.Extensions.Json);
            if (File.Exists(path3)) return path3;

            return path1; // Default to the most specific one
        }

        public void ProcessSchema(string schemaFile, List<SchemaDefinition> schemas)
        {
            try
            {
                var schema = Newtonsoft.Json.JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaFile));
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

        public void ShowLogs()
        {
            if (System.Windows.Application.Current.MainWindow is ExcelBinder.MainWindow mainWindow)
            {
                var logWin = new Views.LogWindow { Owner = mainWindow };
                logWin.ShowDialog();
            }
        }
    }
}
