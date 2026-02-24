namespace ExcelBinder.Models
{
    public static class ProjectConstants
    {
        public static class Categories
        {
            public const string StaticData = "StaticData";
            public const string Logic = "Logic";
            public const string SchemaGen = "SchemaGen";
            public const string Enum = "Enum";
            public const string Constants = "Constants";
        }

        public static class Excel
        {
            public const string CommentPrefix = "#";
            public const string DefaultSheetKey = "Id";

            public const int HeaderRowIndex = 0;
            public const int DataStartRowIndex = 1;

            public const string TypeDelimiter = ":";
            public const string ReferenceMarker = "ref";
            public const string ListPrefix = "List<";
            public const string ListSuffix = ">";
            
            public static class LogicColumns
            {
                public const string Name = "Name";
                public const string ReturnType = "ReturnType";
                public const string Parameters = "Parameters";
                public const string Formula = "Formula";
            }
        }

        public static class Files
        {
            public const string AppSettings = "settings.json";
            public const string SchemaSuffix = "_Schema";
        }

        public static class CLI
        {
            public const string Feature = "--feature";
            public const string All = "--all";
            public const string Bind = "--bind";
            public const string Export = "--export";
            public const string Codegen = "--codegen";
            public const string Binary = "--binary";
            public const string Json = "--json";
            public const string Both = "--both";
        }

        public static class Extensions
        {
            public const string Json = ".json";
            public const string Liquid = ".liquid";
            public const string CSharp = ".cs";
            public const string Binary = ".bytes";
            
            public const string JsonFilter = "JSON Files (*.json)|*.json";
            public const string LiquidFilter = "Liquid Files (*.liquid)|*.liquid";
        }

        public static class Types
        {
            public const string Int = "int";
            public const string Float = "float";
            public const string String = "string";
            public const string Bool = "bool";
            public const string Long = "long";
            public const string Double = "double";
            public const string UInt = "uint";
            public const string ULong = "ulong";
            public const string Short = "short";
            public const string Byte = "byte";
            public const string Void = "void";

            public static readonly string[] AllPrimitives = 
            { 
                Byte, Short, Int, UInt, Long, ULong, Float, Double, String, Bool 
            };
        }

        public static class AI
        {
            public const string ClaudePrefix = "claude-";
            public const string OpenAIProvider = "OpenAI";
            public const string ClaudeProvider = "Claude";
            
            public const string DefaultModel = "gpt-4o";

            public const string StatusWelcome = "무엇을 도와드릴까요?";
            public const string StatusGenerating = "AI가 템플릿을 생성 중입니다...";
            public const string StatusCompleted = "완료되었습니다.";
            public const string StatusError = "오류 발생";
            
            public const string MsgSchemaNotFound = "현재 설정된 정보가 없습니다. (스키마 경로를 확인해 주세요)";
            public const string MsgTemplateCompleted = "템플릿 생성이 완료되었습니다! 오른쪽 미리보기를 확인해 보세요.";
            
            public static class Roles
            {
                public const string User = "User";
                public const string Assistant = "Assistant";
            }
        }
        
        public static class Update
        {
            public const string GitHubOwner = "weperld";
            public const string GitHubRepo = "ExcelBinder";
            public const string ApiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            public const string ReleasesPageUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases";
            public const string UserAgent = "ExcelBinder-UpdateChecker";
            public const int TimeoutSeconds = 10;

            public const string TitleUpdateCheck = "업데이트 확인";
            public const string MsgNewVersion = "새 버전 {0}이(가) 출시되었습니다!";
            public const string MsgGoToDownload = "\n\n다운로드 페이지로 이동하시겠습니까?";
            public const string MsgUpToDate = "현재 최신 버전입니다.";
            public const string MsgCheckFailed = "업데이트 확인에 실패했습니다.";
            public const string BtnDownload = "다운로드";
            public const string BtnDismiss = "닫기";
        }

        public static class Defaults
        {
            public const string Namespace = "GameData";
            public const string FeatureId = "new_feature";
            public const string FeatureName = "New Feature";
            public const string FeatureDefinitionsPath = "Features";
            public const string NotFound = "Not Found";
        }

        public static class UI
        {
            public const string TitleSelectFolder = "폴더 선택";
            public const string TitleSelectFile = "파일 선택";
            public const string TitleSaveFile = "파일 저장";
            public const string TitleError = "오류";
            public const string TitleInfo = "알림";
            public const string TitleWarning = "경고";
            
            public const string MsgRequiredIdName = "ID와 이름은 필수 입력 항목입니다.";
            public const string MsgFeatureSaved = "기능 정의가 성공적으로 저장되었습니다.";
            public const string MsgSaveError = "저장 중 오류가 발생했습니다: ";
            public const string MsgTemplateApplied = "템플릿이 성공적으로 적용되었습니다.";
        }
    }
}
