using System;
using System.Collections.Generic;
using System.IO;
using ExcelBinder.Models;
using ExcelBinder.Services;
using Newtonsoft.Json;
using Xunit;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// List 필드의 행 길이 경계/빈 셀 처리 회귀 테스트.
    /// NPOI는 후행 빈 셀을 저장하지 않아 행 배열이 헤더보다 짧을 수 있다.
    /// </summary>
    public class ExportListGuardTests
    {
        private readonly ExportService _service = new();

        private static readonly SchemaDefinition Schema = new()
        {
            ClassName = "TestData",
            Key = "Id",
            Fields = new Dictionary<string, string>
            {
                { "Id", "int" },
                { "Item", "List<int>" },
            },
        };

        private static string TempFile(string name)
        {
            string dir = Path.Combine(Path.GetTempPath(), "ExcelBinderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, name);
        }

        [Fact]
        public void ShortRow_TrailingListCells_DoNotCrash()
        {
            // 헤더는 Item 3열인데 데이터 행 배열 길이는 2 (NPOI 후행 빈 셀 미저장 상황)
            var data = new List<string[]>
            {
                new[] { "Id", "Item", "Item", "Item" },
                new[] { "1", "5" },
            };

            string outPath = TempFile("t.json");
            _service.ExportToJson(Schema, data, outPath, new FeatureDefinition());

            var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(outPath))!;
            var items = ((Newtonsoft.Json.Linq.JArray)result[0]["Item"]).ToObject<List<int>>()!;
            Assert.Equal(new List<int> { 5 }, items);
        }

        [Fact]
        public void EmptyMiddleListCell_IsSkipped()
        {
            var data = new List<string[]>
            {
                new[] { "Id", "Item", "Item", "Item" },
                new[] { "1", "5", "", "7" },
            };

            string outPath = TempFile("t.json");
            _service.ExportToJson(Schema, data, outPath, new FeatureDefinition());

            var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(outPath))!;
            var items = ((Newtonsoft.Json.Linq.JArray)result[0]["Item"]).ToObject<List<int>>()!;
            Assert.Equal(new List<int> { 5, 7 }, items);
        }

        [Fact]
        public void Binary_ShortRow_WritesCountOfActualValues()
        {
            var data = new List<string[]>
            {
                new[] { "Id", "Item", "Item", "Item" },
                new[] { "1", "5" },
            };

            string outPath = TempFile("t.bytes");
            _service.ExportToBinary(Schema, data, outPath, new FeatureDefinition());

            using var reader = new BinaryReader(File.OpenRead(outPath));
            Assert.Equal(1, reader.ReadInt32());  // row count
            Assert.Equal(1, reader.ReadInt32());  // Id
            Assert.Equal(1, reader.ReadInt32());  // Item count = 실제 값 개수
            Assert.Equal(5, reader.ReadInt32());  // Item[0]
            Assert.Equal(reader.BaseStream.Length, reader.BaseStream.Position); // 잔여 바이트 없음
        }
    }
}
