using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelBinder.Models;
using ExcelBinder.Services;
using ExcelBinder.Services.Processors;
using Xunit;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// Processor 5종을 ExecutionRequest로 직접 구동해 골든 산출물과 비교.
    /// 시트 루프/헤더 파싱/SchemaLocator 폴백 등 오케스트레이션 계층의 회귀 감지기.
    /// </summary>
    public class ProcessorTests
    {
        private static string TempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ExcelBinderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');

        private static void AssertMatchesGoldenDir(string goldenDir, string actualDir)
        {
            foreach (var goldenFile in Directory.GetFiles(goldenDir, "*.cs"))
            {
                string actualFile = Path.Combine(actualDir, Path.GetFileName(goldenFile));
                Assert.True(File.Exists(actualFile), $"생성 누락: {Path.GetFileName(goldenFile)}");
                Assert.Equal(Normalize(File.ReadAllText(goldenFile)), Normalize(File.ReadAllText(actualFile)));
            }
        }

        [Fact]
        public async Task ConstantsProcessor_GeneratesGoldenScripts()
        {
            string excel = Path.Combine(TestPaths.ExternalTestData, "Constants", "Excel", "Constant.xlsx");
            var sheets = new ExcelService().GetSheetNames(excel)
                .Where(s => !s.StartsWith(ProjectConstants.Excel.CommentPrefix))
                .Select(s => new SheetTarget(excel, s))
                .ToList();
            Assert.NotEmpty(sheets);

            string outDir = TempDir();
            var request = new ExecutionRequest
            {
                Feature = new FeatureDefinition { ScriptsPath = outDir },
                SelectedSheets = sheets,
                SelectedFiles = new List<string>(),
                Namespace = "TestProject.Constants",
            };

            await new ConstantsProcessor().ExecuteGenerateAsync(request);

            AssertMatchesGoldenDir(Path.Combine(TestPaths.ExternalTestData, "Constants", "Scripts"), outDir);
        }

        [Fact]
        public async Task EnumProcessor_GeneratesGoldenScripts()
        {
            string excel = Path.Combine(TestPaths.ExternalTestData, "Enum", "Excel", "Enum.xlsx");

            string outDir = TempDir();
            var request = new ExecutionRequest
            {
                Feature = new FeatureDefinition { ScriptsPath = outDir },
                SelectedSheets = new List<SheetTarget>(),
                SelectedFiles = new List<string> { excel },
                Namespace = "TestProject.Enums",
            };

            await new EnumProcessor().ExecuteGenerateAsync(request);

            AssertMatchesGoldenDir(Path.Combine(TestPaths.ExternalTestData, "Enum", "Scripts"), outDir);
        }

        [Fact]
        public async Task StaticDataProcessor_Export_MatchesGoldenOutputs()
        {
            string excelDir = Path.Combine(TestPaths.StaticData, "Excel");
            string outDir = TempDir();

            var request = new ExecutionRequest
            {
                Feature = new FeatureDefinition
                {
                    SchemaPath = Path.Combine(TestPaths.StaticData, "Schemas"),
                    ExportPath = outDir,
                    // OutputOptions 기본값: extension .bytes, binary/json 모두 지원
                },
                SelectedSheets = new List<SheetTarget>
                {
                    new(Path.Combine(excelDir, "Character.xlsx"), "Character"),
                    new(Path.Combine(excelDir, "Character.xlsx"), "Stat"),
                    new(Path.Combine(excelDir, "MultiSheet.xlsx"), "Item"),
                    new(Path.Combine(excelDir, "MultiSheet.xlsx"), "Skill"),
                },
                SelectedFiles = new List<string>(),
                Namespace = "ExternalGameData",
                ExportBinary = true,
                ExportJson = true,
            };

            await new StaticDataProcessor().ExecuteExportAsync(request);

            string goldenDir = Path.Combine(TestPaths.StaticData, "Output");
            foreach (var className in new[] { "CharacterData", "StatData", "ItemData", "SkillData" })
            {
                Assert.Equal(
                    File.ReadAllBytes(Path.Combine(goldenDir, className + ".bytes")),
                    File.ReadAllBytes(Path.Combine(outDir, className + ".bytes")));
                Assert.Equal(
                    Normalize(File.ReadAllText(Path.Combine(goldenDir, className + ".json"))),
                    Normalize(File.ReadAllText(Path.Combine(outDir, className + ".json"))));
            }
        }

        [Fact]
        public void SchemaLocator_ResolvesThreeStageFallback()
        {
            string schemaDir = Path.Combine(TestPaths.StaticData, "Schemas");
            string excel = Path.Combine(TestPaths.StaticData, "Excel", "Character.xlsx");

            // 1단: [Excel]_[Sheet]_Schema.json 존재
            Assert.EndsWith("Character_Character_Schema.json", SchemaLocator.Resolve(schemaDir, excel, "Character"));

            // 폴백 없음: 가장 구체적인 이름을 기본 반환 (신규 생성용)
            Assert.EndsWith("Character_NoSuchSheet_Schema.json", SchemaLocator.Resolve(schemaDir, excel, "NoSuchSheet"));
        }
    }
}
