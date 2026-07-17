using System.Collections.Generic;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public sealed record SheetTarget(string FilePath, string SheetName);

    public sealed record ExecutionRequest
    {
        public required FeatureDefinition Feature { get; init; }
        public required IReadOnlyList<SheetTarget> SelectedSheets { get; init; } // 시트 단위 선택
        public required IReadOnlyList<string> SelectedFiles { get; init; }      // Enum용 파일 단위 선택
        public required string Namespace { get; init; }
        public bool ExportBinary { get; init; }
        public bool ExportJson { get; init; }
    }
}
