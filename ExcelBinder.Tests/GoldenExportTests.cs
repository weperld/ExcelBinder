using System;
using System.IO;
using ExcelBinder.Models;
using ExcelBinder.Services;
using Newtonsoft.Json;
using Xunit;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// ExternalTestData의 커밋된 골든 산출물과 현재 ExportService 출력을 비교하는 회귀 테스트.
    /// Scriban 업그레이드·셀 읽기 리팩터 등 출력에 영향 주는 변경의 감지기 역할.
    /// </summary>
    public class GoldenExportTests
    {
        private readonly ExcelService _excelService = new();
        private readonly ExportService _exportService = new();

        // 의도된 출력 변경 시 EXCELBINDER_REGEN_GOLDEN=1로 실행하면 골든을 현재 출력으로 갱신
        private static bool RegenGolden =>
            Environment.GetEnvironmentVariable("EXCELBINDER_REGEN_GOLDEN") == "1";

        // (엑셀 파일, 시트명, 스키마 파일) → Output/{className}.bytes|.json
        public static TheoryData<string, string, string> Cases => new()
        {
            { "Character.xlsx", "Character", "Character_Character_Schema.json" },
            { "Character.xlsx", "Stat", "Character_Stat_Schema.json" },
            { "MultiSheet.xlsx", "Item", "MultiSheet_Item_Schema.json" },
            { "MultiSheet.xlsx", "Skill", "MultiSheet_Skill_Schema.json" },
        };

        [Theory]
        [MemberData(nameof(Cases))]
        public void Binary_MatchesGolden(string excelFile, string sheetName, string schemaFile)
        {
            var (schema, data) = Load(excelFile, sheetName, schemaFile);
            string goldenPath = Path.Combine(TestPaths.StaticData, "Output", schema.ClassName + ".bytes");
            Assert.True(File.Exists(goldenPath), $"골든 파일 없음: {goldenPath}");

            string outPath = TempFile(schema.ClassName + ".bytes");
            _exportService.ExportToBinary(schema, data, outPath, new FeatureDefinition());

            if (RegenGolden) { File.Copy(outPath, goldenPath, overwrite: true); return; }
            Assert.Equal(File.ReadAllBytes(goldenPath), File.ReadAllBytes(outPath));
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Json_MatchesGolden(string excelFile, string sheetName, string schemaFile)
        {
            var (schema, data) = Load(excelFile, sheetName, schemaFile);
            string goldenPath = Path.Combine(TestPaths.StaticData, "Output", schema.ClassName + ".json");
            Assert.True(File.Exists(goldenPath), $"골든 파일 없음: {goldenPath}");

            string outPath = TempFile(schema.ClassName + ".json");
            _exportService.ExportToJson(schema, data, outPath, new FeatureDefinition());

            if (RegenGolden) { File.Copy(outPath, goldenPath, overwrite: true); return; }
            Assert.Equal(Normalize(File.ReadAllText(goldenPath)), Normalize(File.ReadAllText(outPath)));
        }

        private (SchemaDefinition schema, System.Collections.Generic.List<string[]> data) Load(
            string excelFile, string sheetName, string schemaFile)
        {
            string excelPath = Path.Combine(TestPaths.StaticData, "Excel", excelFile);
            string schemaPath = Path.Combine(TestPaths.StaticData, "Schemas", schemaFile);
            var schema = JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaPath));
            Assert.NotNull(schema);
            var data = _excelService.ReadExcel(excelPath, sheetName);
            Assert.NotEmpty(data);
            return (schema!, data);
        }

        private static string TempFile(string name)
        {
            string dir = Path.Combine(Path.GetTempPath(), "ExcelBinderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, name);
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
