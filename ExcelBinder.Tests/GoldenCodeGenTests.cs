using System.IO;
using ExcelBinder.Models;
using ExcelBinder.Services;
using Newtonsoft.Json;
using Xunit;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// CodeGeneratorService 출력을 ExternalTestData/StaticData/Scripts의 골든 코드와 비교.
    /// Scriban 업그레이드 회귀의 직접 감지기.
    /// </summary>
    public class GoldenCodeGenTests
    {
        private readonly CodeGeneratorService _codeGen = new();

        public static TheoryData<string, string> Cases => new()
        {
            { "Character_Character_Schema.json", "CharacterData.cs" },
            { "Character_Stat_Schema.json", "StatData.cs" },
            { "MultiSheet_Item_Schema.json", "ItemData.cs" },
            { "MultiSheet_Skill_Schema.json", "SkillData.cs" },
        };

        [Theory]
        [MemberData(nameof(Cases))]
        public void GeneratedCode_MatchesGolden(string schemaFile, string goldenScript)
        {
            string schemaPath = Path.Combine(TestPaths.StaticData, "Schemas", schemaFile);
            string goldenPath = Path.Combine(TestPaths.StaticData, "Scripts", goldenScript);
            Assert.True(File.Exists(goldenPath), $"골든 파일 없음: {goldenPath}");

            var schema = JsonConvert.DeserializeObject<SchemaDefinition>(File.ReadAllText(schemaPath));
            Assert.NotNull(schema);

            var feature = new FeatureDefinition
            {
                Name = "External Unity Static Data",
                DefaultNamespace = "ExternalGameData",
                Templates = new FeatureTemplates
                {
                    DataClass = Path.Combine(TestPaths.StaticData, "Templates", "UnityData.liquid"),
                },
            };

            string? generated = _codeGen.GenerateDataCode(schema!, feature, feature.DefaultNamespace);
            Assert.NotNull(generated);

            Assert.Equal(Normalize(File.ReadAllText(goldenPath)), Normalize(generated!));
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
