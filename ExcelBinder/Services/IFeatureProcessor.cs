using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public interface IFeatureProcessor
    {
        string CategoryName { get; }
        
        // Visibility Flags
        bool IsSchemaPathVisible { get; }
        bool IsExportPathVisible { get; }
        bool IsScriptsPathVisible { get; }
        bool IsTypeMappingsVisible { get; }
        bool IsTemplatesVisible { get; }
        bool IsOutputOptionsVisible { get; }

        // Actions
        void ExecuteExport(ViewModels.MainViewModel vm);
        void ExecuteGenerate(ViewModels.MainViewModel vm);
    }
}
