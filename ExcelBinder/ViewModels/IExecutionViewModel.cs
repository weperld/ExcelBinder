using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExcelBinder.Models;

namespace ExcelBinder.ViewModels
{
    public interface IExecutionViewModel
    {
        FeatureDefinition? SelectedFeature { get; }
        ObservableCollection<FileItemViewModel> ExcelFiles { get; }
        bool IsBinaryChecked { get; set; }
        bool IsJsonChecked { get; set; }
        bool IsSchemaStatusVisible { get; }
        string Namespace { get; }
        string GetSchemaPath(string excelFullPath, string sheetName);
        void ProcessSchema(string schemaFile, List<SchemaDefinition> schemas);
        void RefreshFiles();
        void ShowLogs();
    }
}
