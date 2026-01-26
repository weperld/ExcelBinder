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
        bool IsSchemaStatusVisible { get; }
        bool IsTypeMappingsVisible { get; }
        bool IsTemplatesVisible { get; }
        bool IsOutputOptionsVisible { get; }

        // Actions
        System.Threading.Tasks.Task ExecuteExportAsync(ViewModels.IExecutionViewModel vm);
        System.Threading.Tasks.Task ExecuteGenerateAsync(ViewModels.IExecutionViewModel vm);
        System.Threading.Tasks.Task CreateTemplateAsync(string filePath);
    }
}
