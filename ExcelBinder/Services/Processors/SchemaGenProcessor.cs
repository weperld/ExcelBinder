using ExcelBinder.Models;

namespace ExcelBinder.Services.Processors
{
    public class SchemaGenProcessor : IFeatureProcessor
    {
        public string CategoryName => ProjectConstants.Categories.SchemaGen;

        public bool IsSchemaPathVisible => true;
        public bool IsExportPathVisible => false;
        public bool IsScriptsPathVisible => false;
        public bool IsSchemaStatusVisible => true;
        public bool IsTypeMappingsVisible => false;
        public bool IsTemplatesVisible => false;
        public bool IsOutputOptionsVisible => false;

        public System.Threading.Tasks.Task ExecuteExportAsync(ExecutionRequest request)
        {
            // SchemaGen doesn't support export
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task ExecuteGenerateAsync(ExecutionRequest request)
        {
            // SchemaGen은 SchemaEditor 화면 네비게이션이 필요한 GUI 전용 흐름이다.
            // 실제 진입점은 SchemaGenExecutionViewModel의 커맨드 핸들러이며, CLI 등 헤드리스 호출은 여기서 안내만 하고 종료한다.
            LogService.Instance.Warning("SchemaGen은 GUI 전용입니다. GUI에서 스키마 생성 화면을 이용해주세요.");
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task CreateTemplateAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}
