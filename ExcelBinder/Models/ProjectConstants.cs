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
            public const string Enum = "enum";

            public static readonly string[] AllPrimitives =
            {
                Byte, Short, Int, UInt, Long, ULong, Float, Double, String, Bool
            };

            public static readonly string[] SchemaFieldTypes =
            {
                Byte, Short, Int, UInt, Long, ULong, Float, Double, String, Bool, Enum
            };
        }

        public static class Update
        {
            public const string GitHubOwner = "weperld";
            public const string GitHubRepo = "ExcelBinder";
            public const string ApiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            public const string ReleasesApiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases";
            public const string ReleasesPageUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases";
            public const string UserAgent = "ExcelBinder-UpdateChecker";
            public const int TimeoutSeconds = 10;
            public const int DownloadTimeoutMinutes = 10;

            public const string TitleUpdateCheck = "업데이트 확인";
            public const string MsgNewVersion = "새 버전 {0}이(가) 출시되었습니다!";
            public const string MsgGoToDownload = "\n\n다운로드 페이지로 이동하시겠습니까?";
            public const string MsgUpToDate = "현재 최신 버전입니다.";
            public const string MsgCheckFailed = "업데이트 확인에 실패했습니다.";
            public const string BtnDownload = "다운로드";
            public const string BtnDismiss = "닫기";

            public const string TitleDownload = "업데이트 다운로드";
            public const string ZipFilter = "Zip 파일 (*.zip)|*.zip";
            public const string MsgDownloadSuccess = "다운로드가 완료되었습니다.\n\n경로: {0}\n\n저장 폴더를 열까요?";
            public const string MsgDownloadFailed = "다운로드에 실패했습니다. 로그를 확인하세요.";
            public const string MsgDownloadError = "다운로드 중 오류가 발생했습니다: {0}";
            public const string MsgNoAsset = "이 릴리즈에 첨부된 zip 파일이 없습니다. GitHub 페이지에서 직접 받아주세요.";
            public const string DefaultDownloadFileName = "ExcelBinder_Release_{0}.zip";
            public const string BtnDownloadInApp = "다운로드";
            public const string BtnGitHubPage = "GitHub";
        }

        public static class Defaults
        {
            public const string Namespace = "GameData";
            public const string FeatureId = "new_feature";
            public const string FeatureName = "New Feature";
            public const string FeatureDefinitionsPath = "Features";
            public const string NotFound = "Not Found";
            public const string GroupsFileName = "_groups.json";
            public const string AllGroupId = "__all__";
            public const string AllGroupName = "전체";
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

            public const string TitleAddGroup = "그룹 추가";
            public const string TitleRenameGroup = "그룹 이름변경";
            public const string PromptGroupName = "그룹 이름을 입력하세요:";
            public const string MsgGroupNameRequired = "그룹 이름을 입력해야 합니다.";
            public const string MsgGroupNameDuplicate = "이미 존재하는 그룹 이름입니다.";
            public const string MsgConfirmDeleteGroup = "그룹 '{0}'을(를) 삭제하시겠습니까? (포함된 Feature는 삭제되지 않습니다)";
            public const string MenuRename = "이름변경";
            public const string MenuDelete = "삭제";
            public const string MenuAddNewGroup = "새 그룹 만들기";
            public const string MenuAddToGroup = "그룹에 추가/제거";
            public const string ButtonAddGroup = "+ 새 그룹";
            public const string ButtonManageGroupMembers = "Feature 관리";
            public const string TitleManageGroupMembers = "그룹 멤버 편집";
            public const string PromptManageGroupMembers = "'{0}' 그룹에 포함할 Feature를 선택하세요:";
            public const string MsgNoFeatures = "표시할 Feature가 없습니다.";

            public const string TitleGuide = "ExcelBinder 사용 가이드";
            public const string TooltipHelp = "사용 가이드 (F1)";
            public const string TooltipHelpView = "이 화면 가이드 보기";
            public const string ButtonClose = "닫기";
            public const string CheckDoNotShow = "다시 보지 않기";
            public const string MsgGuideMissing = "콘텐츠를 불러올 수 없습니다.";

            public const string GuideTopicGettingStarted = "getting-started";
            public const string GuideTopicFeatureBuilder = "feature-builder";
            public const string GuideTopicSchemaEditor = "schema-editor";
            public const string GuideTopicExportCodeGen = "export-codegen";
        }
    }
}
